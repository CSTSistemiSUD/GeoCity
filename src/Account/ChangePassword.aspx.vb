Public Class ChangePassword
    Inherits System.Web.UI.Page

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        ErrorPanel.Visible = False
        SuccessPanel.Visible = False
    End Sub

    Private Sub UpdatePassword_Click(sender As Object, e As EventArgs) Handles UpdatePassword.Click
        Dim mu As MembershipUser = Membership.GetUser(Context.User.Identity.Name)
        Dim alert As String = String.Empty

        If Not mu.ChangePassword(Me.OldPassword.Text, Me.NewPassword.Text) Then
            ErrorLabel.Text = "Processo di modifica della password non riuscito!"
            ErrorPanel.Visible = True
        Else
            Dim up As New MemberProfile(mu)
            up.PasswordRequest = False
            Dim result As String = up.Save()
            If String.IsNullOrEmpty(result) Then
                Dim eMailObj As eMailCls = Nothing
                eMailObj = MailUtils.PrepareEmail(mu.UserName, MailUtils.EmailTypeEnum.PasswordChanged)

                Dim sendMailResult As String = String.Empty

                If String.IsNullOrEmpty(eMailObj.p_Error) Then
                    sendMailResult = MailUtils.SendMail(eMailObj)
                    If Not String.IsNullOrEmpty(sendMailResult) Then
                        'Mostra errore email
                    Else

                    End If

                Else
                    'Mostra errore email
                End If


                CheckRedirect.Checked = True
                UrlRedirect.Value = Page.ResolveUrl("~/Account/Logout")
                SuccessPanel.Visible = True
            Else
                ErrorLabel.Text = result
                ErrorPanel.Visible = True
            End If


        End If



    End Sub
End Class