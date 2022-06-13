Imports System.Security.Cryptography

Public Class IdentityHelper
	'Utilizzati per XSRF durante il collegamento degli account di accesso esterni
	Public Const XsrfKey As String = "xsrfKey"

	Public Const ProviderNameKey As String = "providerName"
	Public Shared Function GetProviderNameFromRequest(request As HttpRequest) As String
		Return request.QueryString(ProviderNameKey)
	End Function

	Public Const CodeKey As String = "code"
	Public Shared Function GetCodeFromRequest(request As HttpRequest) As String
		Return request.QueryString(CodeKey)
	End Function

	Public Const UserIdKey As String = "userId"

	Public Shared Function LoginUser(userName As String, password As String) As LoginResult
		Dim result = New LoginResult With {.ErrorMessage = String.Empty}
		If Membership.ValidateUser(userName, password) Then
			result.User = Membership.GetUser(userName)


			result.Profile = New MemberProfile(result.User)

			'// VERIFICA SCADENZA CONTRATTO
			'If Date.Compare(up.ExpirationDate, Now) < 0 Then
			'    mu.IsApproved = False
			'    Membership.UpdateUser(mu)
			'    Response.Redirect("~/Account/Lockout")
			'Else

			If (result.User.IsApproved And result.Profile.EmailConfirmed) Or result.User.UserName.ToLower = "admin" Then
				FormsAuthentication.SetAuthCookie(userName, False)
				AuthTrack.LoggedIn(userName)

				If result.Profile.PasswordRequest Then
					result.RedirectPage = "~/Account/ChangePassword"

				Else
					'Dim ReturnUrl As String = HttpContext.Current.Request.QueryString("ReturnUrl")
					'If Not String.IsNullOrEmpty(ReturnUrl) Then
					'    IdentityHelper.RedirectToReturnUrl(ReturnUrl, HttpContext.Current.Response)
					'Else
					'    HttpContext.Current.Response.Redirect("~/WebForms/Index")
					'End If

					result.RedirectPage = GetRedirectPage(userName)



				End If
			Else
				If Not result.Profile.EmailConfirmed Then
					result.ErrorMessage = "È stata richiesta la convalida del tuo indirizzo email.!"
				End If
				If Not result.User.IsApproved Then
					result.ErrorMessage = "Il tuo account è stato bloccato.!"
				End If
			End If




			'End If

		Else

			result.ErrorMessage = "Le credenziali fornite non sono valide!"

		End If
		Return result
	End Function

	Public Shared Function RegisterUser(userName As String, tipoUtente As String,
										  sogAnagrafica As AnagraficaSoggetto,
										  sogRecapito As AddressJson, sogNascita As AddressJson) As RegistrationResult

		Dim rr As New RegistrationResult With {.ErrorMessage = ""}

		Dim rnd As New Random

		Dim userPwd As String = rnd.Next(10000001, 99999999) 'RandomPassword.Generate
		Dim userEmail As String = sogAnagrafica.EMAIL

		Dim mu As MembershipUser = Membership.GetUser(userName)

		Dim ssh As New SSHelper()
		If mu Is Nothing Then

			Try


				mu = Membership.CreateUser(userName, userPwd, userEmail)

				Dim cmd As New SqlClient.SqlCommand With {.CommandType = CommandType.StoredProcedure, .CommandText = "sp_UTENTE_Reg"}
				With cmd.Parameters
					.AddWithValue("@UserId", mu.ProviderUserKey)

					.AddWithValue("@TIPO_UTENTE", tipoUtente)


					.AddWithValue("@SOGGETTO_ID", sogAnagrafica.SOGGETTO_ID)
					.AddWithValue("@SOG_COGNOME", sogAnagrafica.COGNOME)
					.AddWithValue("@SOG_NOME", sogAnagrafica.NOME)
					.AddWithValue("@SOG_CF", sogAnagrafica.CF.ToString)

					.AddWithValue("@SOG_NASCITA_DATA", Utility.ParseDate(sogNascita.DATA, False))
					.AddWithValue("@SOG_NASCITA_NAZIONE", sogNascita.NAZIONE)
					.AddWithValue("@SOG_NASCITA_SIGLA_PROVINCIA", sogNascita.SIGLA_PROVINCIA)
					.AddWithValue("@SOG_NASCITA_PROVINCIA", sogNascita.PROVINCIA)
					.AddWithValue("@SOG_NASCITA_COMUNE", sogNascita.COMUNE)

					.AddWithValue("@SOG_RESIDENZA_NAZIONE", sogRecapito.NAZIONE)
					.AddWithValue("@SOG_RESIDENZA_SIGLA_PROVINCIA", sogRecapito.SIGLA_PROVINCIA)
					.AddWithValue("@SOG_RESIDENZA_PROVINCIA", sogRecapito.PROVINCIA)
					.AddWithValue("@SOG_RESIDENZA_COMUNE", sogRecapito.COMUNE)
					.AddWithValue("@SOG_RESIDENZA_CAP", sogRecapito.CAP)
					.AddWithValue("@SOG_RESIDENZA_INDIRIZZO", sogRecapito.INDIRIZZO)
					.AddWithValue("@SOG_RESIDENZA_CIVICO", sogRecapito.CIVICO)


				End With


				Dim dbResult As DatabaseOperationResult = ssh.ExecuteStored(cmd)
				If Not String.IsNullOrEmpty(dbResult.ErrorMessage) Then
					Throw New Exception(dbResult.ErrorMessage)
				End If

				Dim eMailObj As eMailCls = Nothing
				eMailObj = MailUtils.PrepareEmail(mu.UserName, MailUtils.EmailTypeEnum.Validation)

				If String.IsNullOrEmpty(eMailObj.p_Error) Then
					rr.ErrorMessage = MailUtils.SendMail(eMailObj)
					If Not String.IsNullOrEmpty(rr.ErrorMessage) Then
						Throw New Exception(rr.ErrorMessage)
					End If

					rr.User = mu
				Else
					Throw New Exception(eMailObj.p_Error)
				End If



			Catch ex As Exception
				rr.ErrorMessage = ex.Message
				'-- Roolback della registrazione
				Dim userId = mu.ProviderUserKey
				Try
					Membership.DeleteUser(userName, True)
					Dim cmd As New SqlClient.SqlCommand With {.CommandType = CommandType.StoredProcedure, .CommandText = "sp_UTENTE_Del"}
					With cmd.Parameters
						.AddWithValue("@UserId", userId)
					End With


					Dim dbResult As DatabaseOperationResult = ssh.ExecuteStored(cmd)
				Catch ex1 As Exception

				End Try


			End Try
		Else
			rr.ErrorMessage = "Nel database esiste già un utente con questo UserName"
		End If

		Return rr
	End Function

	Public Shared Function GetRedirectPage(UserName As String) As String

		Dim pageUrl As String = "~"
		Select Case UserName.ToLower
			Case "admin"
				pageUrl = "~/WebForms/Gestione/Progetti"
			Case Else
				pageUrl = "~/WebForms/Gestione/Progetti"
		End Select
		Return pageUrl
	End Function
	Public Shared Function GetUserIdFromRequest(request As HttpRequest) As String
		Return HttpUtility.UrlDecode(request.QueryString(UserIdKey))
	End Function

	Public Shared Function GetResetPasswordRedirectUrl(code As String, request As HttpRequest) As String
		Dim absoluteUri = "/Account/ResetPassword?" + CodeKey + "=" + HttpUtility.UrlEncode(code)
		Return New Uri(request.Url, absoluteUri).AbsoluteUri.ToString()
	End Function

	Public Shared Function GetUserConfirmationRedirectUrl(code As String, userId As String, request As HttpRequest) As String
		Dim absoluteUri = "/Account/Confirm?" + CodeKey + "=" + HttpUtility.UrlEncode(code) + "&" + UserIdKey + "=" + HttpUtility.UrlEncode(userId)
		Return New Uri(request.Url, absoluteUri).AbsoluteUri.ToString()
	End Function

	Private Shared Function IsLocalUrl(url As String) As Boolean
		Return Not String.IsNullOrEmpty(url) AndAlso ((url(0) = "/"c AndAlso (url.Length = 1 OrElse (url(1) <> "/"c AndAlso url(1) <> "\"c))) OrElse (url.Length > 1 AndAlso url(0) = "~"c AndAlso url(1) = "/"c))
	End Function

	Public Shared Sub RedirectToReturnUrl(returnUrl As String, response As HttpResponse)
		If Not [String].IsNullOrEmpty(returnUrl) AndAlso IsLocalUrl(returnUrl) Then
			response.Redirect(returnUrl)
		Else
			response.Redirect("~/")
		End If
	End Sub

	Public Shared Function ResetUserPassword(appUser As MembershipUser) As Object
		'Dim pwd As String = String.Empty
		'Dim msg As String = String.Empty
		'Try
		'	pwd = appUser.ResetPassword()
		'Catch ex As Exception
		'	msg = ex.Message
		'End Try

		Dim rnd As New Random
		Dim onlyNumbers = rnd.Next(100001, 999999)
		Dim msg As String = String.Empty
		Try


			Dim pwd As String = appUser.ResetPassword()

			If Not appUser.ChangePassword(pwd, onlyNumbers) Then
				Throw New Exception("Impossibile resettare la password!<br/>Contattare gli amministratori del sito.")
			End If

		Catch ex As Exception
			msg = ex.Message
		End Try


		Return New With {.PWD = onlyNumbers, .MSG = msg}

	End Function



	Public Shared Function IsAdmin() As Boolean
		Return HttpContext.Current.User.Identity.Name.ToLower = "admin"
	End Function

	Public Shared Function GetAuthToken(token As String) As SSOParams
		Try
			Dim deserializer As New Script.Serialization.JavaScriptSerializer
			Dim wrapper As New Simple3Des(ConfigurationManager.AppSettings("PASSPHRASE"))
			Dim decrypted As String = wrapper.DecryptData(token)
			Return deserializer.Deserialize(decrypted, GetType(SSOParams))
		Catch ex As Exception
			Return New SSOParams With {.ErrorMessage = ex.Message}
		End Try


	End Function

End Class

Public Class LoginResult
	Public ErrorMessage As String
	Public RedirectPage As String
	Public User As MembershipUser
	Public Profile As MemberProfile
End Class

Public Class RegistrationResult
	Public ErrorMessage As String
	Public User As MembershipUser
End Class

<Serializable>
Public Class SSOParams
	Public CodiceComune As String
	Public DatiCatastali As List(Of IdCatastali)
	Public Ambiente As String
	Public Funzione As String
	Public UserName As String
	Public UserPwd As String
	Public ErrorMessage As String

	Sub New()
		DatiCatastali = New List(Of IdCatastali)
		CodiceComune = String.Empty
		Ambiente = String.Empty
		Funzione = String.Empty
		UserName = String.Empty
		UserPwd = String.Empty
		ErrorMessage = String.Empty
	End Sub
End Class

<Serializable>
Public Class IdCatastali
	Public CodiceComune As String
	Public Foglio As String
	Public Particella As String
	Public Subalterno As String
End Class

Public Class Simple3Des
	'Private TripleDes As New TripleDESCryptoServiceProvider
	'Private Function TruncateHash(
	'ByVal key As String,
	'ByVal length As Integer) As Byte()

	'	Dim sha1 As New SHA1CryptoServiceProvider

	'	' Hash the key.
	'	Dim keyBytes() As Byte =
	'		System.Text.Encoding.Unicode.GetBytes(key)
	'	Dim hash() As Byte = sha1.ComputeHash(keyBytes)

	'	' Truncate or pad the hash.
	'	ReDim Preserve hash(length - 1)
	'	Return hash
	'End Function
	'Sub New(ByVal key As String)
	'	' Initialize the crypto provider.
	'	TripleDes.Key = TruncateHash(key, TripleDes.KeySize \ 8)
	'	TripleDes.IV = TruncateHash("", TripleDes.BlockSize \ 8)
	'End Sub

	Private TripleDes As TripleDESCryptoServiceProvider
	Sub New(key As String)

		Dim _md5 As MD5 = New MD5CryptoServiceProvider

		Dim md5Bytes() As Byte = _md5.ComputeHash(Encoding.Unicode.GetBytes(key))
		Dim ivBytes As Byte() = New Byte(7) {}

		TripleDes = New TripleDESCryptoServiceProvider
		With TripleDes
			.KeySize = 128
			.Mode = CipherMode.CBC
			.Padding = PaddingMode.PKCS7
			.Key = md5Bytes
			.IV = ivBytes
		End With
	End Sub
	Public Function EncryptData(
	ByVal plaintext As String) As String

		' Convert the plaintext string to a byte array.
		Dim plaintextBytes() As Byte =
			System.Text.Encoding.Unicode.GetBytes(plaintext)

		' Create the stream.
		Dim ms As New System.IO.MemoryStream
		' Create the encoder to write to the stream.
		Dim encStream As New CryptoStream(ms,
			TripleDes.CreateEncryptor(),
			System.Security.Cryptography.CryptoStreamMode.Write)

		' Use the crypto stream to write the byte array to the stream.
		encStream.Write(plaintextBytes, 0, plaintextBytes.Length)
		encStream.FlushFinalBlock()

		' Convert the encrypted stream to a printable string.
		Return Convert.ToBase64String(ms.ToArray)



	End Function

	Public Function DecryptData(
	ByVal encryptedtext As String) As String

		' Convert the encrypted text string to a byte array.
		Dim encryptedBytes() As Byte = Convert.FromBase64String(encryptedtext)

		' Create the stream.
		Dim ms As New System.IO.MemoryStream
		' Create the decoder to write to the stream.
		Dim decStream As New CryptoStream(ms,
			TripleDes.CreateDecryptor(),
			System.Security.Cryptography.CryptoStreamMode.Write)

		' Use the crypto stream to write the byte array to the stream.
		decStream.Write(encryptedBytes, 0, encryptedBytes.Length)
		decStream.FlushFinalBlock()

		' Convert the plaintext stream to a string.
		Return System.Text.Encoding.Unicode.GetString(ms.ToArray)
	End Function
End Class