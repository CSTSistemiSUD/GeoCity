Imports System.Web.Services

Public Class Progetti
	Inherits System.Web.UI.Page

	Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
		If Not Page.IsPostBack Then
			Dim query As String = "SELECT * FROM [dbo].[PRJ]"
			If Not IdentityHelper.IsAdmin Then
				Dim mp As New MemberProfile(HttpContext.Current.User.Identity.Name)

				query = String.Format("SELECT [dbo].[PRJ].* 
                    FROM dbo.TD_UTENTI 
                    INNER JOIN dbo.PRJ_USERS ON dbo.TD_UTENTI.UserId = dbo.PRJ_USERS.USERID AND dbo.TD_UTENTI.UserId = '{0}'
                    INNER JOIN dbo.PRJ ON dbo.PRJ_USERS.CODICE = dbo.PRJ.CODICE", mp.UserId)
			End If
			Dim ssh As New SSHelper
			Dim dbResult As DatabaseOperationResult = ssh.GetData(query)
			With Me.Repeater1
				.DataSource = dbResult.GetTable(0)
				.DataBind()
			End With
		End If
	End Sub



	<WebMethod>
	Public Shared Function UpdatePRJ(prjObject As PRJ) As String
		Dim update As String = "UPDATE [dbo].[PRJ]
            SET [PRJDESC] = @PRJDESC
                ,[PRJDATABASE] = @PRJDATABASE
                ,[OUTSIDE] = @OUTSIDE
                ,[INSIDE] = @INSIDE
                ,[INSIDE_PLAN] = @INSIDE_PLAN
            WHERE PRJGUID = @PRJGUID"

		Dim cmd As New SqlClient.SqlCommand With {.CommandType = CommandType.Text, .CommandText = update}
		With cmd.Parameters
			.AddWithValue("@PRJGUID", prjObject.PRJGUID)
			.AddWithValue("@PRJDESC", prjObject.PRJDESC)
			.AddWithValue("@PRJDATABASE", prjObject.PRJDATABASE)
			.AddWithValue("@OUTSIDE", String.Empty)
			.AddWithValue("@INSIDE", prjObject.INSIDE)
			.AddWithValue("@INSIDE_PLAN", prjObject.INSIDE_PLAN)





		End With

		Dim ssh As New SSHelper
		Dim dbResult As DatabaseOperationResult = ssh.ExecuteStored(cmd)
		Return dbResult.ErrorMessage
	End Function
End Class