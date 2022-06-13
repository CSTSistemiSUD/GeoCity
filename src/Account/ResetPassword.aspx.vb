Public Class ResetPassword
    Inherits System.Web.UI.Page

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        ErrorPanel.Visible = False
        SuccessPanel.Visible = False
    End Sub
    Protected Sub ResetPasswordButton_Click(sender As Object, e As EventArgs)
        ErrorLabel.Text = String.Empty
        ErrorPanel.Visible = False
        Dim eMailObj As eMailCls = Nothing

        Dim email As String = EmailCtrl.Text
        Dim userName As String = Membership.GetUserNameByEmail(email)
        If String.IsNullOrEmpty(userName) Then
            ErrorLabel.Text = "Utente non trovato!"
        Else
            eMailObj = MailUtils.PrepareEmail(userName, MailUtils.EmailTypeEnum.PasswordRequest)

            If String.IsNullOrEmpty(eMailObj.p_Error) Then
                ErrorLabel.Text = MailUtils.SendMail(eMailObj)

            Else
                ErrorLabel.Text = eMailObj.p_Error
            End If
        End If

		If String.IsNullOrEmpty(ErrorLabel.Text) Then
			ResetPasswordForm.Visible = False
			SuccessPanel.Visible = True
			Me.lblUserEMail.Text = EmailCtrl.Text
			Dim up As New MemberProfile(userName)
			up.PasswordRequest = True
			up.Save()

		Else
			ErrorPanel.Visible = True
        End If
    End Sub
End Class