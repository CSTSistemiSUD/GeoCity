Public Class SiteMaster
    Inherits MasterPage
    Protected Sub Page_Load(ByVal sender As Object, ByVal e As EventArgs) Handles Me.Load
        Debug.Write(MailUtils.GetApplicationLink)
        Debug.WriteLine(HttpContext.Current.Request.RawUrl)
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
    End Sub
End Class