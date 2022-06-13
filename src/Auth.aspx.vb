Public Class Auth
	Inherits System.Web.UI.Page

	Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load

		'PRJGUID: 
		'PRJITEMGUID: 
		'Livello(uno dei seguenti valori) : P01, P02, P03
		'TOKEN:

		Dim PRJGUID As String = Request.QueryString("PRJGUID")
		Dim PRJITEMGUID As String = Request.QueryString("PRJITEMGUID")
		Dim LIVELLO As String = Request.QueryString("LIVELLO")
		Dim HOSTID As String = Request.QueryString("HOSTID")
		Dim SENSOR As String = Request.QueryString("SENSOR")
		Dim TOKEN As String = Request.QueryString("TOKEN")

		'If Context.IsDebuggingEnabled Then
		'	PRJGUID = "5d69ae8b-0a44-4a21-987b-1ec83a4c7f9e"
		'	PRJITEMGUID = "cc295a96-c2b8-4cd5-8de4-1ede081bd9af"
		'	LIVELLO = "P01"
		'	TOKEN = "de94bba1-06a1-4g40-a16a"
		'End If

		If String.IsNullOrEmpty(TOKEN) Then
			Me.lblResult.Text = "Parametri insufficienti all'autenticazione"
		Else

			If String.IsNullOrEmpty(PRJGUID) And String.IsNullOrEmpty(HOSTID) Then
				'Almeno il valore per PRJGUID per arrivare all'outside
				'oppure HOSTID per arrivare INSIDE
				Me.lblResult.Text = "Parametri insufficienti alla navigazione"
			Else
				Dim authResult = Nothing 'ws_Main.fn_CheckToken(TOKEN)
				If Not IsNothing(authResult) AndAlso Not IsNothing(authResult.data) Then

					Me.lblResult.Text = String.Format("{0}: User {1} Permission {2}", authResult.status_message, authResult.data.name, authResult.data.permission)

					Dim res As LoginResult = IdentityHelper.LoginUser("demo", "demo$01")
					If String.IsNullOrEmpty(res.ErrorMessage) Then
						SetRedirect(res, PRJGUID, PRJITEMGUID, LIVELLO, HOSTID, SENSOR)
					End If

				Else
					If IsNothing(authResult) Then
						Me.lblResult.Text = "Errore nell'autenticazione remota"
					Else
						Me.lblResult.Text = String.Format("Status {0} Message {1}", authResult.status, authResult.status_message)
					End If

				End If
			End If




		End If




	End Sub


	Private Sub SetRedirect(CurrentLoginResult As LoginResult, PRJGUID As String, PRJITEMGUID As String, LIVELLO As String, HOSTID As String, SENSOR As String)

		Dim prj As DataRow = GetPRJ(CurrentLoginResult, PRJGUID, HOSTID, SENSOR) '-- Estrae la riga di progetto
		If Not IsNothing(prj) Then
			CheckIsValid.Checked = True
			Dim viewer As String = "outside"

			If Not String.IsNullOrEmpty(PRJITEMGUID) OrElse Not String.IsNullOrEmpty(HOSTID) Then
				' Se è stato passato PRJITEMGUID opppure HOSTID posso andare inside
				viewer = "inside"
			End If

			Dim PRJDATABASE As String = prj("PRJDATABASE").ToString
			Dim INSIDE As String = prj("INSIDE").ToString
			Dim INSIDE_PLAN As String = prj("INSIDE_PLAN").ToString

			If Not String.IsNullOrEmpty(HOSTID) Then
				'Il livello viene preso dalla riga di progetto se è stato passato HOSTID 
				HOSTID = prj("OBJECTID")
				PRJGUID = prj("PRJGUID").ToString
				PRJITEMGUID = prj("PRJITEMGUID").ToString
				LIVELLO = prj("Livello")
			End If

			Dim jsParams As New Hashtable


			jsParams.Add("_PRJGUID", PRJGUID)
			jsParams.Add("_PRJITEMGUID", PRJITEMGUID)
			jsParams.Add("_PRJDATABASE", PRJDATABASE)
			jsParams.Add("_INSIDE", INSIDE)
			jsParams.Add("_INSIDE_PLAN", INSIDE_PLAN)
			jsParams.Add("_LIVELLO", LIVELLO)
			jsParams.Add("_HOSTID", HOSTID)
			jsParams.Add("_VIEWER", viewer)


			Dim jsCode As New Text.StringBuilder
			For Each de As DictionaryEntry In jsParams
				jsCode.AppendFormat("var {0} = '{1}';", de.Key, de.Value)
				jsCode.Append(Environment.NewLine)
			Next
			If Not Page.ClientScript.IsStartupScriptRegistered(GetType(String), "ViewerParams") Then
				Page.ClientScript.RegisterStartupScript(GetType(String), "ViewerParams", jsCode.ToString, True)
			End If
		Else
			Me.lblResult.Text = "I parametri passati non sono validi"

		End If



	End Sub


	Private Function GetPRJ(CurrentLoginResult As LoginResult, PRJGUID As String, HOSTID As String, SENSOR As String) As DataRow
		Dim query As String
		If Not String.IsNullOrEmpty(PRJGUID) And String.IsNullOrEmpty(HOSTID) Then
			'-- Se non è stato Passato HOSTID
			query = String.Format("SELECT prj.*, '' as Livello FROM 
			PITER_Config.dbo.PRJ prj
			INNER JOIN PITER_Config.dbo.PRJ_USERS users ON prj.PRJGUID = users.PRJGUID AND users.UserId = '{0}'
            WHERE prj.PRJGUID = '{1}'", CurrentLoginResult.Profile.UserId, PRJGUID)
		Else
			'-- Se è stato Passato HOSTID estraggo anche il LIVELLO
			query = String.Format("SELECT prj.*, sensori.OBJECTID, sensori.PRJITEMGUID, sensori.Livello FROM 
			Geosafety_UTM33N.dbo.FC_SENSORI as sensori 
			INNER JOIN Geosafety_UTM33N.dbo.FC_STRUTTURA strutture ON sensori.PRJITEMGUID = strutture.PRJITEMGUID
			INNER JOIN PITER_Config.dbo.PRJ prj ON strutture.PRJGUID = prj.PRJGUID	
			INNER JOIN PITER_Config.dbo.PRJ_USERS users ON prj.PRJGUID = users.PRJGUID AND users.USERID = '{0}'
			WHERE sensori.HostId = '{1}'", CurrentLoginResult.Profile.UserId, HOSTID)

			If Not String.IsNullOrEmpty(SENSOR) Then
				query = String.Concat(query, String.Format("AND sensori.SENSOR = '{0}'", SENSOR))
			End If

		End If


		Dim ssh As New SSHelper
		Dim dbResult As DatabaseOperationResult = ssh.GetData(query)


		Dim prj As DataRow = Nothing
		If dbResult.GetTable(0).Rows.Count > 0 Then
			prj = dbResult.GetRow(0, 0)
		End If
		Return prj
	End Function
End Class