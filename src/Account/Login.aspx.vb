Public Class Login
    Inherits System.Web.UI.Page

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load

    End Sub

    Private Sub LoginButton_Click(sender As Object, e As EventArgs) Handles LoginButton.Click
        Dim res As LoginResult = IdentityHelper.LoginUser(UserName.Text, Password.Text)
        If Not String.IsNullOrEmpty(res.ErrorMessage) Then
            Me.ErrorLabel.Text = res.ErrorMessage
            Me.ErrorPanel.Visible = True
        Else
            Response.Redirect(res.RedirectPage)
        End If
        'If Membership.ValidateUser(UserName.Text, Password.Text) Then
        '    Dim mu As MembershipUser = Membership.GetUser(UserName.Text)


        '    Dim up As New MemberProfile(mu)

        '    '// VERIFICA SCADENZA CONTRATTO
        '    If Date.Compare(up.ExpirationDate, Now) < 0 Then
        '        mu.IsApproved = False
        '        Membership.UpdateUser(mu)
        '        Response.Redirect("~/Account/Lockout")
        '    Else
        '        FormsAuthentication.SetAuthCookie(UserName.Text, False)


        '        If up.PasswordRequest Then
        '            Response.Redirect("~/Account/ChangePassword")
        '        Else
        '            Dim ReturnUrl As String = Request.QueryString("ReturnUrl")
        '            If Not String.IsNullOrEmpty(ReturnUrl) Then
        '                IdentityHelper.RedirectToReturnUrl(ReturnUrl, Response)
        '            Else
        '                Response.Redirect("~/WebForms/Index")
        '            End If

        '        End If


        '    End If

        'Else

        '    Me.ErrorLabel.Text = "Le credenziali fornite non sono valide!"
        '    Me.ErrorPanel.Visible = True

        'End If
    End Sub
End Class