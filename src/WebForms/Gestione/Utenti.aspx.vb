Imports System.Web.Services

Public Class Utenti
    Inherits System.Web.UI.Page

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load

    End Sub



    <WebMethod>
    Public Shared Function LoadUsers() As String
        Dim query As String = "SELECT TOP (1000) [UserId]
      ,[UserName]
      ,[Email]
      ,[EmailConfirmed]
      ,[RegistrationDate]
      ,[SOGGETTO_ID]
      ,[COGNOME]
      ,[NOME]
  FROM [PITER_Config].[dbo].[VW_UTENTI]"

        Dim ssh As New SSHelper()
        Dim dbResult As DatabaseOperationResult = ssh.GetData(query)
        Return dbResult.SerializeTable(0)
    End Function

    <WebMethod>
    Public Shared Function DeleteUser(UserId As String) As String
        Dim message As String = String.Empty
        Try
            Dim mu As MembershipUser = Membership.GetUser(Guid.Parse(UserId))
            If mu Is Nothing Then
                Throw New Exception("Utente non trovato!")

            End If
            Membership.DeleteUser(mu.UserName, True)
            Dim cmd As New SqlClient.SqlCommand With {.CommandType = CommandType.StoredProcedure, .CommandText = "sp_UTENTE_Del"}
            With cmd.Parameters
                .AddWithValue("@UserId", UserId)
            End With


            Dim ssh As New SSHelper()
            Dim dbResult As DatabaseOperationResult = ssh.ExecuteStored(cmd)
        Catch ex As Exception
            message = ex.Message
        End Try
        Return message

    End Function

    <WebMethod>
    Public Shared Function LoadPrjForUser(UserId As String) As String
		Dim query As String = "SELECT        
        dbo.PRJ.CODICE, dbo.PRJ.DESCRIZIONE, dbo.TD_UTENTI.UserId
        FROM dbo.TD_UTENTI 
        INNER JOIN dbo.PRJ_USERS ON dbo.TD_UTENTI.UserId = dbo.PRJ_USERS.USERID AND dbo.TD_UTENTI.UserId = @UserId 
        RIGHT OUTER JOIN dbo.PRJ ON dbo.PRJ_USERS.CODICE= dbo.PRJ.CODICE"

		Dim cmd As New SqlClient.SqlCommand With {.CommandText = query, .CommandType = CommandType.Text}
        With cmd.Parameters
            .AddWithValue("@UserId", UserId)
        End With
        Dim ssh As New SSHelper()
        Dim dbResult As DatabaseOperationResult = ssh.GetData(cmd)
        Return dbResult.SerializeTable(0)
    End Function

	<WebMethod>
	Public Shared Function AssignPrjToUser(CODICE As String, UserId As String, Assign As Boolean) As String
		Dim query As String
		If Assign Then
			query = "INSERT INTO [dbo].[PRJ_USERS] ([CODICE], [USERID]) VALUES (@CODICE, @USERID);"
		Else
			query = "DELETE [dbo].[PRJ_USERS] WHERE [CODICE] = @CODICE AND [USERID] = @USERID;"
		End If


		Dim cmd As New SqlClient.SqlCommand With {.CommandText = query, .CommandType = CommandType.Text}
		With cmd.Parameters
			.AddWithValue("@CODICE", CODICE)
			.AddWithValue("@USERID", UserId)
		End With
		Dim ssh As New SSHelper()
		Dim dbResult As DatabaseOperationResult = ssh.ExecuteStored(cmd)
		Return dbResult.ErrorMessage
	End Function

	'<WebMethod>
	'   Public Shared Function RegisterUser(userName As String, tipoUtente As String,
	'                                        sogAnagrafica As AnagraficaSoggetto,
	'                                        sogRecapito As AddressJson, sogNascita As AddressJson) As String



	'       Dim message As String = IdentityHelper.RegisterUser(userName, tipoUtente,
	'                                         sogAnagrafica,
	'                                         sogRecapito, sogNascita)











	'       Return message
	'   End Function

	<WebMethod>
    Public Shared Function RegisterFakeUser(userName As String, userPwd As String, userEmail As String) As String



        Dim message As String = String.Empty





        Dim mu As MembershipUser = Membership.GetUser(userName)

        Dim ssh As New SSHelper()
        If mu Is Nothing Then

            Try


                mu = Membership.CreateUser(userName, userPwd, userEmail)
                If Not mu Is Nothing Then



                    Dim cmd As New SqlClient.SqlCommand With {.CommandType = CommandType.StoredProcedure, .CommandText = "sp_UTENTE_Reg"}
                    With cmd.Parameters
                        .AddWithValue("@UserId", mu.ProviderUserKey)

                        .AddWithValue("@TIPO_UTENTE", "FAKE")


                        .AddWithValue("@SOGGETTO_ID", -1)
                        .AddWithValue("@SOG_COGNOME", "FAKE")
                        .AddWithValue("@SOG_NOME", "USER")
                        .AddWithValue("@SOG_CF", "")

                        .AddWithValue("@SOG_NASCITA_DATA", Now)
                        .AddWithValue("@SOG_NASCITA_NAZIONE", "IT")
                        .AddWithValue("@SOG_NASCITA_SIGLA_PROVINCIA", "")
                        .AddWithValue("@SOG_NASCITA_PROVINCIA", "")
                        .AddWithValue("@SOG_NASCITA_COMUNE", "")

                        .AddWithValue("@SOG_RESIDENZA_NAZIONE", "IT")
                        .AddWithValue("@SOG_RESIDENZA_SIGLA_PROVINCIA", "")
                        .AddWithValue("@SOG_RESIDENZA_PROVINCIA", "")
                        .AddWithValue("@SOG_RESIDENZA_COMUNE", "")
                        .AddWithValue("@SOG_RESIDENZA_CAP", "")
                        .AddWithValue("@SOG_RESIDENZA_INDIRIZZO", "")
                        .AddWithValue("@SOG_RESIDENZA_CIVICO", "")


                    End With


                    Dim dbResult As DatabaseOperationResult = ssh.ExecuteStored(cmd)
                    If Not String.IsNullOrEmpty(dbResult.ErrorMessage) Then
                        Throw New Exception(dbResult.ErrorMessage)
                    Else
                        Dim up As New MemberProfile(mu)


                        up.EmailConfirmed = True
                        up.PasswordRequest = False
                        Dim result As String = up.Save()
                        If Not String.IsNullOrEmpty(result) Then
                            Throw New Exception(result)
                        End If
                    End If

                End If





            Catch ex As Exception
                message = ex.Message
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
            message = "Nel database esiste già un utente con questo UserName"
        End If

        Return message
    End Function

End Class