Public Class RegisterUser
    Inherits System.Web.UI.UserControl

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        ' Define the name, type and url of the client script on the page.
        Dim csname As String = "RegisterUserScript"
        Dim csurl As String = "~/UserControls/RegisterUser.js"
        Dim cstype As Type = Me.GetType()

        ' Get a ClientScriptManager reference from the Page class.
        Dim cs As ClientScriptManager = Page.ClientScript

        ' Check to see if the include script is already registered.
        If (Not cs.IsClientScriptIncludeRegistered(cstype, csname)) Then

            cs.RegisterClientScriptInclude(cstype, csname, ResolveClientUrl(csurl))

        End If


        Me.COGNOME.Attributes.Add("required", "required")
        Me.NOME.Attributes.Add("required", "required")
        'Me.CODICE_FISCALE.Attributes.Add("required", "required")
        Me.EMAIL.Attributes.Add("required", "required")
        Me.RegisterUserDialog.Style.Add("display", "none")
    End Sub

End Class