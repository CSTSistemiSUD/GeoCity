Imports Microsoft.VisualBasic
'Imports Microsoft.AspNet.Identity
'Imports Microsoft.AspNet.Identity.EntityFramework
'Imports Microsoft.AspNet.Identity.Owin
'Imports Owin
Imports System.Net.Mail
Public Class MailUtils

    Enum EmailTypeEnum
        Validation
        Activation
        Confirmation
        PasswordRequest
        PasswordChanged
    End Enum
    Public Shared Function GetApplicationLink() As String
        Dim applicationLink As String
        If Not HttpContext.Current.Request.Url.IsDefaultPort Then
            applicationLink = String.Format("http://{0}:{1}/{2}",
            HttpContext.Current.Request.Url.Host, HttpContext.Current.Request.Url.Port, HttpContext.Current.Request.ApplicationPath.Substring(1))
        Else
            applicationLink = String.Format("http://{0}/{1}",
            HttpContext.Current.Request.Url.Host, HttpContext.Current.Request.ApplicationPath.Substring(1))
        End If


        Return applicationLink

    End Function
    Public Shared Function PrepareEmail(ByVal pUserName As String,
                                        ByVal pEmailType As EmailTypeEnum) As eMailCls

        Dim eMailObj As New eMailCls
        Try



            Dim appUser As MembershipUser = Membership.GetUser(pUserName)
            Dim up As New MemberProfile(appUser)

            Dim result As String = String.Empty

            Dim settings As NameValueCollection =
             System.Web.Configuration.WebConfigurationManager.AppSettings

            Dim App_Title As String = settings("App_Title")



            Dim REG_UserName As String = settings("REG_UserName")
            Dim REG_UserDisplay As String = String.Format("{0} '{1}'", settings("REG_UserDisplay"), App_Title)
            Dim REG_Password As String = settings("REG_Password")
            Dim REG_CCEMail As String = settings("REG_CCEMail")

            'Istanzia un nuovo indirizzo di posta, dato l'indirizzo e.mail e il nome completo dell'autore
            'Dichiara un oggetto destinato a contenere l'indirizzo di posta del destinatario

            Dim destAddress As MailAddress = Nothing
            Dim subjectString As String = String.Empty
            Dim actionLink As String = String.Empty

            '--'
            Dim req As HttpRequest = HttpContext.Current.Request
            Dim appPath As String = IIf(req.ApplicationPath = "/", "", req.ApplicationPath)
            Dim confirmPage As String = String.Format("{0}/Account/Confirm?code={1}&userId={2}", appPath, up.UserId.ToString, appUser.ProviderUserKey.ToString)
            Dim loginPage As String = String.Format("{0}/Account/Login", appPath)
            Dim contactPage As String = String.Format("{0}/Contact", appPath)

            ' Dim absoluteUri = "/Account/Confirm?" + CodeKey + "=" + HttpUtility.UrlEncode(code) + "&" + UserIdKey + "=" + HttpUtility.UrlEncode(userId)

            Dim ConfirmUrl = GetAbsoluteUri(confirmPage, req) 'IdentityHelper.GetUserConfirmationRedirectUrl(up.UserId.ToString, appUser.ProviderUserKey.ToString, HttpContext.Current.Request)
            Debug.WriteLine(ConfirmUrl)

            Dim LoginUrl = GetAbsoluteUri(loginPage, req)
            Debug.WriteLine(LoginUrl)
            Dim ContactUrl = GetAbsoluteUri(contactPage, req)
            Debug.WriteLine(ContactUrl)
            '---

            Dim params As New Hashtable
            params.Add("{FirstName}", up.ANAGRAFICA.NOME)
            params.Add("{LastName}", up.ANAGRAFICA.COGNOME)
            params.Add("{ApplicationName}", App_Title)
            params.Add("{ConfirmUrl}", ConfirmUrl)
            params.Add("{UserName}", appUser.UserName)
            params.Add("{UserEmail}", appUser.Email)
            params.Add("{UserId}", up.UserId.ToString)
            params.Add("{RegistrationDate}", String.Format("{0:dd/MM/yyyy}", up.RegistrationDate))
            params.Add("{ExpirationDate}", String.Format("{0:dd/MM/yyyy}", up.ExpirationDate))



            params.Add("{ContactUrl}", ContactUrl)
            params.Add("{LoginUrl}", LoginUrl)
            params.Add("{InfoMail}", REG_CCEMail)

            Dim pMessageTemplatePath As String = String.Empty

            destAddress = New MailAddress(appUser.Email, appUser.UserName)

            If pEmailType = EmailTypeEnum.Validation Then
                '// Invia la email con il link per la VALIDAZIONE dell'indirizzo
                '// all'utente che si è registrato
                pMessageTemplatePath = HttpContext.Current.Server.MapPath("~/Account/EmailTemplate/MailValidation.htm")


                subjectString = "Richiesta di registrazione in " & App_Title

            ElseIf pEmailType = EmailTypeEnum.Activation Then



                '    '// Invia la email con il link per la ATTIVAZIONE del'utente
                '    '// all'Amministratore
                '    pMessageTemplatePath = HttpContext.Current.Server.MapPath("~/Account/EmailTemplate/MailActivation.htm")

                '    destAddress = New MailAddress(REG_CCEMail, REG_CCEMail)
                '    subjectString = "Attivazione dell'account utente in " & App_Title




            ElseIf pEmailType = EmailTypeEnum.Confirmation Then
                '// Invia la email per CONFERMARE l'attivazione
                pMessageTemplatePath = HttpContext.Current.Server.MapPath("~/Account/EmailTemplate/MailConfirmation.htm")



                subjectString = "Conferma attivazione dell'account utente in " & App_Title


            ElseIf pEmailType = EmailTypeEnum.PasswordRequest Then

                '// Invia la email per richiesta PASSWORD
                pMessageTemplatePath = HttpContext.Current.Server.MapPath("~/Account/EmailTemplate/MailPasswordRequest.htm")

                Dim res = IdentityHelper.ResetUserPassword(appUser)
                If String.IsNullOrEmpty(res.MSG) Then
                    params.Add("{TemporaryPassword}", res.PWD)
                Else
                    Throw New Exception(res.MSG)
                End If




                subjectString = "Richiesta password account utente in " & App_Title
                'actionLink = String.Concat(pApplicationUrl, "Account/Login")

            ElseIf pEmailType = EmailTypeEnum.PasswordChanged Then
                '// Invia la email per richiesta PASSWORD
                pMessageTemplatePath = HttpContext.Current.Server.MapPath("~/Account/EmailTemplate/MailPasswordChanged.htm")


                destAddress = New MailAddress(appUser.Email, appUser.UserName)

                subjectString = "Modifica password account utente in " & App_Title
                'actionLink = String.Concat(pApplicationUrl, "Account/Login")
            End If

            Dim message As New StringBuilder
            If Not String.IsNullOrEmpty(pMessageTemplatePath) Then

                message.Append("<!DOCTYPE html>
                            <html>
                                <head>
                                    <meta charset=""utf-8"" />
                                    <title></title>
                                </head>
                                <body>")

                Dim body As String = String.Empty
                Dim template As String = String.Empty
                Dim footer As String = String.Empty

                Using reader As IO.StreamReader = IO.File.OpenText(pMessageTemplatePath)


                    template = reader.ReadToEnd
                End Using


                body = InterpolateString(template, params)

                message.Append(body)

                Using reader = IO.File.OpenText(HttpContext.Current.Server.MapPath("~/Account/EmailTemplate/MessageFooter.htm"))
                    footer = reader.ReadToEnd
                End Using

                message.Append(footer)

                message.Append("
                                </body>
                            </html>")
            End If

            With eMailObj
                .p_Subject = subjectString
                .p_Body = message.ToString
                .p_Recipient.Add(destAddress)
            End With
        Catch ex As Exception
            eMailObj.p_Error = "Errore nella creazione della email: <br/>" & ex.Message
        End Try


        Return eMailObj


    End Function

    Public Shared Function SendMail(eMailObj As eMailCls) As String
        'Istanzia un nuovo oggetto MailMessage, che contiene tutti gli elementi necessari per
        'inviare il messaggio di posta
        Dim result As String = String.Empty
        Dim message As New MailMessage

        Dim settings As NameValueCollection =
         System.Web.Configuration.WebConfigurationManager.AppSettings

        Dim App_Title As String = settings("App_Title")

        Dim SMTP_Provider As String = settings("SMTP_Provider")
        Dim SMTP_Port As String = settings("SMTP_Port")

		Dim REG_UserEmail As String = settings("REG_UserEmail")
		Dim REG_UserName As String = settings("REG_UserName")
        Dim REG_UserDisplay As String = String.Format("{0} '{1}'", settings("REG_UserDisplay"), App_Title)
        Dim REG_Password As String = settings("REG_Password")
        Dim REG_CCEMail As String = settings("REG_CCEMail")





        Try
			'Istanzia l'autore del messaggio di posta
			Dim SMTP_User As New MailAddress(REG_UserEmail, REG_UserDisplay)

			With message


                'Assegna agli elementi del messaggio ciò che è stato specificato nel form
                'oltre all'autore del messaggio, istanziato precedentemente
                .Subject = eMailObj.p_Subject

                .Sender = SMTP_User
                .Body = eMailObj.p_Body
                .IsBodyHtml = True

                .From = SMTP_User
                '// MODIFICA OGGETTO eMailCls 31/08/2015
                '.To.Add(eMailObj.p_Recipient)
                If eMailObj.p_Recipient.Count > 0 Then
                    For Each rcp As MailAddress In eMailObj.p_Recipient
                        .To.Add(rcp)
                    Next
                End If

                If eMailObj.p_CC.Count > 0 Then
                    For Each cc As MailAddress In eMailObj.p_CC
                        .CC.Add(cc)
                    Next
                End If

                'Specifica quale messaggio di notifica deve essere inviato al mittente
                'In questo caso solo se l'invio del messaggio fallisce.
                .DeliveryNotificationOptions = DeliveryNotificationOptions.OnFailure

            End With

            Dim Client As New SmtpClient(SMTP_Provider)
            'abilita l'utilizzo della crittografia nell'invio dei messaggi
            'si tenga conto che non tutti i destinatari possono supportare questo metodo.
            'Client.EnableSsl = True

            With Client
                .Port = SMTP_Port
                .UseDefaultCredentials = False
                .DeliveryMethod = SmtpDeliveryMethod.Network
                .Credentials = New System.Net.NetworkCredential(REG_UserName, REG_Password)
                .EnableSsl = Boolean.Parse(settings("SMTP_EnableSSL"))
                .Send(message)
            End With





        Catch ex As InvalidOperationException
            result = ("Non è stato specificato il nome Host del server")
        Catch ex As SmtpFailedRecipientException
            result = ("Tentativo di invio al server locale, ma non è presente una mailbox")
        Catch ex As SmtpException
            result = ("Utente non valido/Host non trovato/Altro errore in fase di invio")
        Catch ex As Exception
            result = String.Concat("Errore nell'invio delle email:<br/>", ex.ToString)
        End Try
        Return result
    End Function

    Public Shared Function InterpolateString(template As String, params As Hashtable) As String
        For Each de As DictionaryEntry In params
            template = template.Replace(de.Key, de.Value)
        Next
        Return template
    End Function

    Public Shared Function GetAbsoluteUri(absoluteUri As String, request As HttpRequest) As String
        Return New Uri(request.Url, absoluteUri).AbsoluteUri.ToString()
    End Function
End Class

Public Class eMailCls
    Public p_Subject As String
    Public p_Body As String
    Public p_Recipient As New List(Of MailAddress) 'As MailAddress
    Public p_Error As String
    Public p_CC As New List(Of MailAddress)
End Class