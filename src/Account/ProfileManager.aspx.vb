Public Class ProfileManager
    Inherits System.Web.UI.Page

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Me.ErrorPanel.Visible = False
        Me.SuccessPanel.Visible = False
        If Not Page.IsPostBack Then
            Dim mu As MembershipUser = Membership.GetUser(Context.User.Identity.Name)
            Dim up As New MemberProfile(mu)

            Me.txtAnagraficaCodFis.Text = up.ANAGRAFICA.CF
            Me.txtAnagraficaEmail.Text = mu.Email

            Me.lblRegistrationDate.Text = String.Format("{0:dd/MM/yyyy}", up.RegistrationDate)
            Me.lblExpirationDate.Text = String.Format("{0:dd/MM/yyyy}", up.ExpirationDate)

            Me.txtAnagraficaCognome.Text = up.ANAGRAFICA.COGNOME
            Me.txtAnagraficaNome.Text = up.ANAGRAFICA.NOME

            Me.UserAddress.PROP_NAZIONE = up.RESIDENZA.NAZIONE
            Me.UserAddress.PROP_SIGLA_PROVINCIA = up.RESIDENZA.SIGLA_PROVINCIA
            Me.UserAddress.PROP_PROVINCIA = up.RESIDENZA.PROVINCIA
            Me.UserAddress.PROP_COMUNE = up.RESIDENZA.COMUNE
            Me.UserAddress.PROP_CAP = up.RESIDENZA.CAP
            Me.UserAddress.PROP_INDIRIZZO = up.RESIDENZA.INDIRIZZO
            Me.UserAddress.PROP_CIVICO = up.RESIDENZA.CIVICO


            Me.UserBirthday.PROP_NAZIONE = up.NASCITA.NAZIONE
            Me.UserBirthday.PROP_SIGLA_PROVINCIA = up.NASCITA.SIGLA_PROVINCIA
            Me.UserBirthday.PROP_PROVINCIA = up.NASCITA.PROVINCIA
            Me.UserBirthday.PROP_COMUNE = up.NASCITA.COMUNE
            Me.UserBirthday.PROP_DATA = up.NASCITA.DATA
        End If
    End Sub

    Private Sub UpdateUser_Click(sender As Object, e As EventArgs) Handles UpdateUser.Click
        Dim mu As MembershipUser = Membership.GetUser(Context.User.Identity.Name)
        Dim up As New MemberProfile(mu)
        up.ANAGRAFICA.COGNOME = Me.txtAnagraficaCognome.Text
        up.ANAGRAFICA.NOME = Me.txtAnagraficaNome.Text
        up.ANAGRAFICA.CF = Me.txtAnagraficaCodFis.Text

        up.RESIDENZA.NAZIONE = Me.UserAddress.PROP_NAZIONE
        up.RESIDENZA.SIGLA_PROVINCIA = Me.UserAddress.PROP_SIGLA_PROVINCIA
        up.RESIDENZA.PROVINCIA = Me.UserAddress.PROP_PROVINCIA
        up.RESIDENZA.COMUNE = Me.UserAddress.PROP_COMUNE
        up.RESIDENZA.CAP = Me.UserAddress.PROP_CAP
        up.RESIDENZA.INDIRIZZO = Me.UserAddress.PROP_INDIRIZZO
        up.RESIDENZA.CIVICO = Me.UserAddress.PROP_CIVICO


        up.NASCITA.NAZIONE = Me.UserBirthday.PROP_NAZIONE
        up.NASCITA.SIGLA_PROVINCIA = Me.UserBirthday.PROP_SIGLA_PROVINCIA
        up.NASCITA.PROVINCIA = Me.UserBirthday.PROP_PROVINCIA
        up.NASCITA.COMUNE = Me.UserBirthday.PROP_COMUNE
        up.NASCITA.DATA = Me.UserBirthday.PROP_DATA


        Dim result As String = up.Save()
        If String.IsNullOrEmpty(result) Then
            Me.SuccessPanel.Visible = True
        Else
            Me.ErrorLabel.Text = result
            Me.ErrorPanel.Visible = True
        End If

    End Sub
End Class