Public Class Address
    Inherits System.Web.UI.UserControl
    Private _Caption As String
    Public Property Caption() As String
        Get
            Return _Caption
        End Get
        Set(ByVal value As String)
            _Caption = value
        End Set
    End Property



    Private _IsBirthdayInfo As Boolean
    Public Property IsBirthdayInfo() As Boolean
        Get
            Return _IsBirthdayInfo
        End Get
        Set(ByVal value As Boolean)
            _IsBirthdayInfo = value
        End Set
    End Property

    Private _IsRequired As Boolean
    Public Property IsRequired() As Boolean
        Get
            Return _IsRequired
        End Get
        Set(ByVal value As Boolean)
            _IsRequired = value
        End Set
    End Property

    Protected Function GetRequiredAttribute() As String
        If Me.IsRequired Then
            Return " required"
        Else
            Return String.Empty
        End If

    End Function

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        ' Define the name, type and url of the client script on the page.
        Dim csname As String = "AddressScript"
        Dim csurl As String = "~/UserControls/Address.js"
        Dim cstype As Type = Me.GetType()

        ' Get a ClientScriptManager reference from the Page class.
        Dim cs As ClientScriptManager = Page.ClientScript

        ' Check to see if the include script is already registered.
        If (Not cs.IsClientScriptIncludeRegistered(cstype, csname)) Then

            cs.RegisterClientScriptInclude(cstype, csname, ResolveClientUrl(csurl))

        End If

        If IsRequired Then
            Me.Provincia.Attributes.Add("required", "required")
            Me.Comune.Attributes.Add("required", "required")
            If Not IsBirthdayInfo Then
                Me.Indirizzo.Attributes.Add("required", "required")
            Else
                Me.Data.Attributes.Add("required", "required")
            End If


        End If


		Dim dbResult As DatabaseOperationResult = ws_Util.fn_ElencaNazioni

		With Me.Nazione
			.DataSource = dbResult.GetTable(0)
			.DataTextField = "DESCRIZIONE"
			.DataValueField = "CODICE_NAZIONE"
			.DataBind()
		End With
		If Not Page.ClientScript.IsStartupScriptRegistered(GetType(String), "_nazioni") Then
			Page.ClientScript.RegisterStartupScript(GetType(String), "_nazioni", "var _nazioni = " & dbResult.SerializeTable(0), True)
		End If


	End Sub

    Private Sub Nazione_DataBound(sender As Object, e As EventArgs) Handles Nazione.DataBound

    End Sub

    Public Property PROP_NAZIONE() As String
        Get
            Return Me.Nazione.SelectedValue
        End Get
        Set(ByVal value As String)
            Me.Nazione.SelectedValue = value
        End Set
    End Property

    Public Property PROP_SIGLA_PROVINCIA() As String
        Get
            Return Me.Sigla.Value
        End Get
        Set(ByVal value As String)
            Me.Sigla.Value = value
        End Set
    End Property
    Public Property PROP_PROVINCIA() As String
        Get
            Return Me.Provincia.Text
        End Get
        Set(ByVal value As String)
            Me.Provincia.Text = value
        End Set
    End Property


    Public Property PROP_COMUNE() As String
        Get
            Return Me.Comune.Text
        End Get
        Set(ByVal value As String)
            Me.Comune.Text = value
        End Set
    End Property
    Public Property PROP_CAP() As String
        Get
            Return Me.Cap.Text
        End Get
        Set(ByVal value As String)
            Me.Cap.Text = value
        End Set
    End Property

    Public Property PROP_INDIRIZZO() As String
        Get
            Return Me.Indirizzo.Text
        End Get
        Set(ByVal value As String)
            Me.Indirizzo.Text = value
        End Set
    End Property

    Public Property PROP_CIVICO() As String
        Get
            Return Me.Civico.Text
        End Get
        Set(ByVal value As String)
            Me.Civico.Text = value
        End Set
    End Property

    Public Property PROP_DATA() As String
        Get
            Return Me.Data.Text
        End Get
        Set(ByVal value As String)
            Me.Data.Text = value
        End Set
    End Property
End Class