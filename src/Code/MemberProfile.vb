Public Class MemberProfile
    Public Sub New(UserName As String)
        Dim mu As MembershipUser = Membership.GetUser(UserName)
        Load(mu)
    End Sub

    Public Sub New(mu As MembershipUser)
        Load(mu)
    End Sub

    Private Sub Load(mu As MembershipUser)
        Me.UserId = mu.ProviderUserKey
        Me.Email = mu.Email

        Try
            Dim ssh As New SSHelper()

            Dim query As String = "SELECT * FROM VW_UTENTI WHERE UserId = @UserId;"


            Dim cmd As New SqlClient.SqlCommand With {.CommandText = query, .CommandType = CommandType.Text}
            cmd.Parameters.AddWithValue("@UserId", Me.UserId)
            Dim dbResult As DatabaseOperationResult = ssh.GetData(cmd)


            Dim dr As DataRow = dbResult.GetRow(0, 0)


            Me.PasswordRequest = dr("PasswordRequest")
            Me.EmailConfirmed = dr("EmailConfirmed")
            Me.RegistrationDate = dr("RegistrationDate")
            Me.ExpirationDate = dr("ExpirationDate")


            Me.TIPO_UTENTE = dr("TIPO_UTENTE")
            Me.ANAGRAFICA = New AnagraficaSoggetto With {
                    .COGNOME = dr("COGNOME").ToString,
                    .NOME = dr("NOME").ToString,
                    .EMAIL = dr("Email").ToString,
                    .SOGGETTO_ID = dr("SOGGETTO_ID"),
                    .CF = dr("CODICE_FISCALE").ToString
                }
            Me.RESIDENZA = New AddressJson With {
                    .NAZIONE = dr("RESIDENZA_NAZIONE").ToString,
                    .SIGLA_PROVINCIA = dr("RESIDENZA_SIGLA_PROVINCIA").ToString,
                    .PROVINCIA = dr("RESIDENZA_PROVINCIA").ToString,
                    .COMUNE = dr("RESIDENZA_COMUNE").ToString,
                    .CAP = dr("RESIDENZA_CAP").ToString,
                    .INDIRIZZO = dr("RESIDENZA_INDIRIZZO").ToString,
                    .CIVICO = dr("RESIDENZA_CIVICO").ToString
                }
            Me.NASCITA = New AddressJson With {
                    .NAZIONE = dr("NASCITA_NAZIONE").ToString,
                    .SIGLA_PROVINCIA = dr("NASCITA_SIGLA_PROVINCIA").ToString,
                    .PROVINCIA = dr("NASCITA_PROVINCIA").ToString,
                    .COMUNE = dr("NASCITA_COMUNE").ToString,
                    .DATA = dr("NASCITA_DATA").ToString
                }
        Catch ex As Exception

            If mu.UserName.ToLower = "admin" Then
                Me.PasswordRequest = False
                Me.EmailConfirmed = True
            End If

            Me.RegistrationDate = Nothing
            Me.ExpirationDate = Nothing


            Me.TIPO_UTENTE = ""
            Me.ANAGRAFICA = New AnagraficaSoggetto
            Me.RESIDENZA = New AddressJson
            Me.NASCITA = New AddressJson
        End Try




    End Sub

    Public UserId As Guid


    Public Email As String = String.Empty
    Public PasswordRequest As Boolean = False
    Public EmailConfirmed As Boolean = False
    Public RegistrationDate As Date
    Public ExpirationDate As Date


    Public TIPO_UTENTE As String
    Public ANAGRAFICA As AnagraficaSoggetto
    Public RESIDENZA As AddressJson
    Public NASCITA As AddressJson

    Public Function Save() As String
        Dim ssh As New SSHelper()
        Dim sqlCmd As New SqlClient.SqlCommand With {.CommandText = "sp_UTENTE_InsUpd", .CommandType = CommandType.StoredProcedure}
        With sqlCmd.Parameters
            .AddWithValue("@SOGGETTO_ID", Me.ANAGRAFICA.SOGGETTO_ID)

            .AddWithValue("@TIPO_UTENTE", Me.TIPO_UTENTE)
            .AddWithValue("@UserId", Me.UserId)
            .AddWithValue("@EmailConfirmed", Me.EmailConfirmed)
            .AddWithValue("@PasswordRequest", Me.PasswordRequest)
            .AddWithValue("@RegistrationDate", Me.RegistrationDate)
            .AddWithValue("@ExpirationDate", Me.ExpirationDate)

            .AddWithValue("@COGNOME", Me.ANAGRAFICA.COGNOME)
            .AddWithValue("@NOME", Me.ANAGRAFICA.NOME)
            .AddWithValue("@CODICE_FISCALE", Me.ANAGRAFICA.CF)
            .AddWithValue("@RESIDENZA_NAZIONE", Me.RESIDENZA.NAZIONE)
            .AddWithValue("@RESIDENZA_SIGLA_PROVINCIA", Me.RESIDENZA.SIGLA_PROVINCIA)
            .AddWithValue("@RESIDENZA_PROVINCIA", Me.RESIDENZA.PROVINCIA)
            .AddWithValue("@RESIDENZA_COMUNE", Me.RESIDENZA.COMUNE)
            .AddWithValue("@RESIDENZA_CAP", Me.RESIDENZA.CAP)
            .AddWithValue("@RESIDENZA_INDIRIZZO", Me.RESIDENZA.INDIRIZZO)
            .AddWithValue("@RESIDENZA_CIVICO", Me.RESIDENZA.CIVICO)
            .AddWithValue("@NASCITA_DATA", Utility.ParseDate(Me.NASCITA.DATA, False))
            .AddWithValue("@NASCITA_NAZIONE", Me.NASCITA.NAZIONE)
            .AddWithValue("@NASCITA_SIGLA_PROVINCIA", Me.NASCITA.SIGLA_PROVINCIA)
            .AddWithValue("@NASCITA_PROVINCIA", Me.NASCITA.PROVINCIA)
            .AddWithValue("@NASCITA_COMUNE", Me.NASCITA.COMUNE)



        End With


        Dim dbResult As DatabaseOperationResult = ssh.ExecuteStored(sqlCmd)
        Return dbResult.ErrorMessage
    End Function



End Class
