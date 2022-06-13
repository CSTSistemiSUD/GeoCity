Public Class Page
    Inherits System.Web.UI.MasterPage

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load

		Dim codComune As String = ""
		If Session("CodComune") Is Nothing Then
			codComune = Utility.VerificaAppPath()
			If String.IsNullOrEmpty(codComune) Then
				Response.Redirect("~/Account/NotFound")
			Else
				Session("CodComune") = codComune

			End If
		Else
			codComune = Session("CodComune")
		End If
		Me.LogoImg.Src = Page.ResolveUrl(String.Concat("~/WebHandlers/Logo.ashx?CodComune=", codComune))

		If Not Page.ClientScript.IsStartupScriptRegistered(GetType(String), "ws_url") Then
			Page.ClientScript.RegisterStartupScript(GetType(String), "ws_url", Utility.GetJsUrl, True)
		End If

		If Not Page.ClientScript.IsStartupScriptRegistered(GetType(String), "NumberFormat") Then
			Page.ClientScript.RegisterStartupScript(GetType(String),
			"NumberFormat", Utility.GetJsNumberFormat, True)
		End If

		If Page.User.Identity.IsAuthenticated Then
			Dim mu As MembershipUser = Membership.GetUser(Me.Page.User.Identity.Name)
			Me.lblUser.Text = mu.UserName
			Me.lblUserEmail.Text = mu.Email
		End If
	End Sub

End Class