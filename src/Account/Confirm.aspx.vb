Public Class Confirm
    Inherits System.Web.UI.Page

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Dim code As String = IdentityHelper.GetCodeFromRequest(Request)
        Dim userId As String = IdentityHelper.GetUserIdFromRequest(Request)

        Try
            If code IsNot Nothing AndAlso userId IsNot Nothing Then
                Dim mu As MembershipUser = Membership.GetUser(New Guid(userId), False)
                If Not mu Is Nothing Then
                    Dim up As New MemberProfile(mu)




                    If New Guid(code) = up.UserId Then

                        up.EmailConfirmed = True
                        up.PasswordRequest = True
                        Dim result As String = up.Save()
                        If String.IsNullOrEmpty(result) Then
                            Dim eMailObj As eMailCls = MailUtils.PrepareEmail(mu.UserName, MailUtils.EmailTypeEnum.PasswordRequest)
                            Dim eMailSendResult As String = String.Empty
                            If String.IsNullOrEmpty(eMailObj.p_Error) Then
                                eMailSendResult = MailUtils.SendMail(eMailObj)
                                If String.IsNullOrEmpty(eMailSendResult) Then

                                    SuccessPanel.Visible = True
                                Else
                                    Throw New Exception(eMailSendResult)
                                End If
                            Else
                                Throw New Exception(eMailObj.p_Error)
                            End If
                        Else
                            Throw New Exception(result)
                        End If




                    Else
                        Throw New Exception("Il codice di verifica non corrisponde!")
                    End If
                Else
                    Throw New Exception("Codice utente non valido!")
                End If
            Else
                Throw New Exception("Codice di verifica non valido!")
            End If

        Catch ex As Exception
            SuccessPanel.Visible = False
            ErrorPanel.Visible = True
            ErrorLabel.Text = ex.Message
        End Try
    End Sub

End Class