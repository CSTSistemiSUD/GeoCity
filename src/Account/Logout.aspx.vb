Public Class Logout
    Inherits System.Web.UI.Page

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load

        If HttpContext.Current.Request.IsAuthenticated Then
            AuthTrack.LoggedOut(HttpContext.Current.User.Identity.Name)
        End If

        FormsAuthentication.SignOut()

        Response.Redirect("~/Default")
    End Sub

End Class