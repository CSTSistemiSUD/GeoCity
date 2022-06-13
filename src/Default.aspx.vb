Public Class _Default
	Inherits System.Web.UI.Page

	Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
		Dim NavbarCollapse As PlaceHolder = CType(Master.FindControl("NavbarCollapse"), PlaceHolder)
		If Not NavbarCollapse Is Nothing Then
			Dim ul As New HtmlGenericControl("ul")
			'ul.Attributes("class") = "nav navbar-nav navbar-right text-uppercase"

			'ul.Controls.Add(New LiteralControl("<li><a href = ""#home""> Home</a></li>"))
			'ul.Controls.Add(New LiteralControl("<li><a href = ""#feature""> Caratteristiche</a></li>"))
			'ul.Controls.Add(New LiteralControl("<li><a href = ""#pricing""> Prezzi</a></li>"))

			'ul.Controls.Add(New LiteralControl("<li><a href = ""#contact""> Contatti</a></li>"))

			ul.Attributes("class") = "nav navbar-nav"
			ul.Controls.Add(New LiteralControl("<li class=""page-scroll active""><a href = ""#intro""> Home</a></li>"))
			ul.Controls.Add(New LiteralControl("<li class=""page-scroll""><a href = ""#about""> Info</a></li>"))
			ul.Controls.Add(New LiteralControl("<li class=""page-scroll""><a href = ""#features""> Caratteristiche</a></li>"))
			ul.Controls.Add(New LiteralControl("<li class=""page-scroll""><a href = ""#contact""> Contatti</a></li>"))
			NavbarCollapse.Controls.Add(ul)
		End If


		pnlLogin.Visible = Not Page.User.Identity.IsAuthenticated
		pnlLogout.Visible = Page.User.Identity.IsAuthenticated
		'LoginLink.Visible = Not Page.User.Identity.IsAuthenticated
		'EnterLink.Visible = Page.User.Identity.IsAuthenticated
		'LogoutLink.Visible = Page.User.Identity.IsAuthenticated
	End Sub
	Private Sub LoginButton_Click(sender As Object, e As EventArgs) Handles LoginButton.Click
		Dim res As LoginResult = IdentityHelper.LoginUser(UserName.Text, Password.Text)
		If Not String.IsNullOrEmpty(res.ErrorMessage) Then
			Me.ErrorLabel.Text = res.ErrorMessage
			Me.ErrorPanel.Visible = True
		Else
			Response.Redirect(res.RedirectPage)
		End If
	End Sub

	Private Sub LogoutButton_Click(sender As Object, e As EventArgs) Handles LogoutButton.Click
		FormsAuthentication.SignOut()
		Roles.DeleteCookie()
		Session.Clear()
		Response.Redirect("~")
	End Sub

	Private Sub MainButton_Click(sender As Object, e As EventArgs) Handles MainButton.Click
		Dim res As String = IdentityHelper.GetRedirectPage(Page.User.Identity.Name)
		Response.Redirect(res)
	End Sub

	Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
		Dim params As New SSOParams
		With params
			.CodiceComune = "B644"
			.Ambiente = "SUED"
			.Funzione = "ANALIZZA_VINCOLI"
			.DatiCatastali.Add(New IdCatastali With {.CodiceComune = "B644", .Foglio = "1", .Particella = "108"})
			.UserName = "Admin"
			.UserPwd = "Adm@Piter$01"
		End With
		Dim serializer As New Script.Serialization.JavaScriptSerializer
		Dim plainText As String = serializer.Serialize(params)
		Dim wrapper As New Simple3Des(ConfigurationManager.AppSettings("PASSPHRASE"))
		Dim cipherText = wrapper.EncryptData(plainText)
		Me.hfRedirectUrl.Value = ResolveUrl("~/WebForms/Mappa/Viewer")
		Me.hfToken.Value = cipherText
	End Sub
End Class