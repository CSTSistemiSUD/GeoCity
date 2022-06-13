Public Class Register
	Inherits System.Web.UI.Page

	Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load

		If Not Page.IsPostBack Then
			Dim codice As String = ""
			Dim appPath As String = HttpContext.Current.Request.ApplicationPath.Substring(1)

			Dim ssh As New SSHelper
			Dim dbResult As DatabaseOperationResult = ssh.GetData(String.Format("SELECT * FROM PRJ WHERE SERVIZIO = '{0}' IS_PUBLIC = 1 ORDER BY DESCRIZIONE", appPath))


			'With Me.ddlPRJ
			'	.DataSource = dbResult.GetTable(0)
			'	.DataTextField = "DESCRIZIONE"
			'	.DataValueField = "CODICE"
			'	.DataBind()
			'End With
		End If
	End Sub

	Private Sub ConfirmButton_Click(sender As Object, e As EventArgs) Handles ConfirmButton.Click

		Me.RegistrationForm.Visible = False

		Dim CodComune As String = Session("CodComune")

		If Session("CodComune") Is Nothing OrElse String.IsNullOrEmpty(Session("CodComune")) Then

			Me.ErrorLabel.Text = "La registrazione di nuovi utenti è disabilitata"
			Me.RegistrationError.Visible = True
			Me.RegistrationComplete.Visible = False

		Else

			Dim rr As RegistrationResult = IdentityHelper.RegisterUser(Me.EMailCtrl.Text, "User",
										  New AnagraficaSoggetto With {.SOGGETTO_ID = -1, .NOME = Me.NomeCtrl.Text, .COGNOME = Me.CognomeCtrl.Text, .EMAIL = Me.EMailCtrl.Text, .CF = ""},
											New AddressJson, New AddressJson)

			If String.IsNullOrEmpty(rr.ErrorMessage) Then

				Dim query As String = "INSERT INTO [dbo].[PRJ_USERS] ([CODICE], [USERID], [USERTYPE]) VALUES (@CODICE, @USERID, 2);"

				Dim cmd As New SqlClient.SqlCommand With {.CommandText = query, .CommandType = CommandType.Text}
				With cmd.Parameters
					'.AddWithValue("@CODICE", Me.ddlPRJ.SelectedValue)
					.AddWithValue("@CODICE", Session("CodComune"))
					.AddWithValue("@USERID", rr.User.ProviderUserKey)
				End With
				Dim ssh As New SSHelper()
				Dim dbr As DatabaseOperationResult = ssh.ExecuteStored(cmd)



				Me.lblUserEmail.Text = Me.EMailCtrl.Text
				Me.RegistrationError.Visible = False
				Me.RegistrationComplete.Visible = True
			Else
				Me.ErrorLabel.Text = rr.ErrorMessage
				Me.RegistrationError.Visible = True
				Me.RegistrationComplete.Visible = False
			End If

		End If


	End Sub
End Class