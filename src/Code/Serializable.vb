<Serializable>
Public Class AnagraficaAzienda
    Public AZIENDA_ID As Integer
    Public DENOMINAZIONE As String
    Public TIPO As String
    Public PARTITA_IVA As String
    Public CODICE_FISCALE As String

    Public INDIRIZZO As String
    Public CIVICO As String
    Public CAP As String
    Public COMUNE As String
    Public PROVINCIA As String

    Public COD_IPA As String

End Class



<Serializable>
Public Class AnagraficaSoggetto
    Public SOGGETTO_ID As String
    Public COGNOME As String
    Public NOME As String
    Public CF As String
    Public EMAIL As String
End Class

<Serializable>
Public Class AddressJson
    Public NAZIONE As String
    Public SIGLA_PROVINCIA As String
    Public PROVINCIA As String
    Public COMUNE As String
    Public CAP As String
    Public INDIRIZZO As String
    Public CIVICO As String
	Public DATA As String

	Sub New()
		NAZIONE = ""
		SIGLA_PROVINCIA = ""
		PROVINCIA = ""
		COMUNE = ""
		CAP = ""
		INDIRIZZO = ""
		CIVICO = ""
		DATA = ""

	End Sub

End Class


<Serializable>
Public Class Livello
    Public NOME As String
    Public AMBIENTI As Integer
    Public AREA As String

End Class


<Serializable>
Public Class PRJ
	Public PRJDESC As String
	Public PRJGUID As String
	Public PRJDATABASE As String
	Public OUTSIDE As String
	Public INSIDE As String
	Public INSIDE_PLAN As String
	Public ID_1LIV As String
End Class


<Serializable>
Public Class RisultatoEsportazione
	Public Titolo As String
	Public Immagine As String
	Public Legenda As List(Of LegendItem)
End Class

<Serializable>
Public Class ExportLayoutJob
	Public jobId As String
	Public status As Integer
	Public message As String
	Public percentage As Integer
End Class

<Serializable>
Public Class LegendItem
	Public label As String
	Public url As String
	Public imageData As String
	Public contentType As String
End Class

<Serializable>
Public Class LayerItem
	Public layerID As Integer
	Public layerName As String
	Public layerType As String
	Public minScale As Integer
	Public maxScale As Integer
	Public legend As List(Of LegendItem)
End Class

<Serializable>
Public Class LayerList
	Public layers As List(Of LayerItem)
End Class


