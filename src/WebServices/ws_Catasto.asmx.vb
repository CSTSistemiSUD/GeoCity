Imports System.Web
Imports System.Web.Services
Imports System.Web.Services.Protocols
Imports System.Data
Imports System.Web.Script.Services
' Per consentire la chiamata di questo servizio Web dallo script utilizzando ASP.NET AJAX, rimuovere il commento dalla riga seguente.
' <System.Web.Script.Services.ScriptService()> _
<System.Web.Services.WebService(Namespace:="http://tempuri.org/")>
<System.Web.Services.WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)>
<ScriptService()>
Public Class ws_Catasto
	Inherits System.Web.Services.WebService

	Const FABBRICATI As String = "FABBRICATI"
	Const PARTICELLE As String = "PARTICELLE"

	Private section_title = "<p class=""section-title"">{0}</p>"
	Private form_group = "<div class=""col-sm-{0}""><p><span class=""info-label"">{1}</span><span class=""info-value"">{2}</span></p></div>"

	Private panel_heading = "<div class=""panel-heading""><a class=""panel-heading collapsed"" data-toggle=""collapse"" data-target=""#collapse_{0}"" href=""#collapse_{0}"" style=""margin-left:12px;""> {1}</a></div>"
	Private panel_collapse = "<div id=""collapse_{0}"" class=""panel-collapse collapse""><div class=""panel-body"">{1}</div></div>"

	<WebMethod>
	Public Function InfoAggiornamento(p_PRJFOLDER As String, testo As String)
		Dim sqlh As New SQLiteHelper(BuildDBPath(p_PRJFOLDER))
		Dim dbr As DatabaseOperationResult = sqlh.GetData("select
[descrizione],
[numRecord],
strftime('%d/%m/%Y',[dataRegInizio]) dataRegInizio,
strftime('%d/%m/%Y',[dataSelezione]) dataSelezione,
strftime('%d/%m/%Y',[dataElaborazione]) dataElaborazione,
[periodo]
FROM  [FILE]")

		Return dbr.SerializeTable(0)

	End Function

	<WebMethod>
	Public Function RicercaTitolari(p_PRJFOLDER As String, testo As String)
		Dim sb As New StringBuilder()
		sb.AppendLine("		select p.codComune as codcomune ,s.sezione as sezione, p.idSoggetto as IdSoggetto, s.tipoPersona as tipopersona, p.cognome || ' ' || p.nome as denominazione,")
		sb.AppendLine("		p.sesso as sesso, p.dataNascita as dataNascita, c.decodifica as luogoNascita, '' as sede, p.codFiscale as cf_iva, p.indicazioniSup as indicazioniSup")
		sb.AppendLine("		from persona_fisica p left join cod_comune c on p.luogoNascita=c.codice left join soggetti s on p.idSoggetto=s.idsoggetto")
		sb.AppendFormat("	where denominazione like '{0}%'", testo.Replace("'", "`"))
		sb.AppendLine("     union")
		sb.AppendLine("		select g.codComune as codcomune,s.sezione as sezione, g.idSoggetto as IdSoggetto, s.tipoPersona as tipopersona, g.denominazione as denominazione,")
		sb.AppendLine("		'' as sesso, null as dataNascita, '' as luogoNascita, c.decodifica as sede, g.codFiscale as cf_iva, '' as indicazioniSup")
		sb.AppendLine("		from persona_giuridica g left join cod_comune c on g.sede=c.codice left join soggetti s on g.idSoggetto=s.idsoggetto")
		sb.AppendFormat("	where denominazione like '{0}%'", testo.Replace("'", "`"))
		sb.AppendLine(" 	ORDER BY denominazione")

		Dim sqlh As New SQLiteHelper(BuildDBPath(p_PRJFOLDER))
		Dim dbr As DatabaseOperationResult = sqlh.GetData(sb.ToString)

		Return dbr.SerializeTable(0)
	End Function

	<WebMethod()>
	Public Function CaricaImmobili(p_PRJFOLDER As String, IDSogg As String, Foglio As String, Numero As String, TipoImmobili As String) As String
		Dim fabb As New StringBuilder()
		Dim terr As New StringBuilder()

		Dim foglio_formattato As String = AddLeadingZeros(Foglio, 4)
		Dim numero_formattato As String = AddLeadingZeros(Numero, 5)

		If Not String.IsNullOrEmpty(IDSogg) Then

			fabb.AppendLine("SELECT [TITOLARITA].[idImmobile] as idImmobile,")
			fabb.AppendLine("  [IDENTIFICATIVI_IMMOBILIARI].[codComune] || '' as codComune,")
			fabb.AppendLine("  [IDENTIFICATIVI_IMMOBILIARI].[sezione] as Sezione,")
			fabb.AppendLine("  [IDENTIFICATIVI_IMMOBILIARI].[sezioneUrbana] as sezurbana,")
			fabb.AppendLine("  [IDENTIFICATIVI_IMMOBILIARI].[foglio] as foglio,")
			fabb.AppendLine("  [IDENTIFICATIVI_IMMOBILIARI].[numero] as numero,")
			fabb.AppendLine("  [IDENTIFICATIVI_IMMOBILIARI].[denominatore] as denominatore,")
			fabb.AppendLine("  [IDENTIFICATIVI_IMMOBILIARI].[subalterno] as subalterno,")
			fabb.AppendLine("  [UNITA_IMMOBILIARI].[categoria] as categoria,")
			fabb.AppendLine("  [UNITA_IMMOBILIARI].[classe] as classe,")
			fabb.AppendLine("  [UNITA_IMMOBILIARI].[renditaEuro] as renditaEuro,")
			fabb.AppendLine("  [INDIRIZZI].[indirizzo] as indirizzo,")
			fabb.AppendLine("  [TITOLARITA].[idSoggetto] as idSoggetto")
			fabb.AppendLine("FROM [TITOLARITA]")
			fabb.AppendLine("  LEFT OUTER JOIN [IDENTIFICATIVI_IMMOBILIARI]")
			fabb.AppendLine("    ON [TITOLARITA].[idImmobile] =")
			fabb.AppendLine("    [IDENTIFICATIVI_IMMOBILIARI].[idImmobile]")
			fabb.AppendLine("  LEFT OUTER JOIN [UNITA_IMMOBILIARI]")
			fabb.AppendLine("    ON [IDENTIFICATIVI_IMMOBILIARI].[idImmobile] =")
			fabb.AppendLine("    [UNITA_IMMOBILIARI].[idImmobile]")
			fabb.AppendLine("  LEFT OUTER JOIN [INDIRIZZI] ON [UNITA_IMMOBILIARI].[idImmobile] =")
			fabb.AppendLine("    [INDIRIZZI].[idImmobile]")
			fabb.AppendFormat(" WHERE [TITOLARITA].[idSoggetto] = {0} AND NOT [TITOLARITA].[idImmobile] IS NULL", IDSogg)
		ElseIf Not String.IsNullOrEmpty(Foglio) AndAlso Not String.IsNullOrEmpty(Numero) Then

			fabb.AppendLine("SELECT [UNITA_IMMOBILIARI].IdImmobile, ")
			fabb.AppendLine("	[IDENTIFICATIVI_IMMOBILIARI].[sezione] as Sezione, ")
			fabb.AppendLine("  [IDENTIFICATIVI_IMMOBILIARI].[sezioneUrbana] as sezurbana,")
			fabb.AppendLine("  [IDENTIFICATIVI_IMMOBILIARI].[foglio] as foglio,")
			fabb.AppendLine("  [IDENTIFICATIVI_IMMOBILIARI].[numero] as numero,")
			fabb.AppendLine("  [IDENTIFICATIVI_IMMOBILIARI].[denominatore] as denominatore,")
			fabb.AppendLine("  [IDENTIFICATIVI_IMMOBILIARI].[subalterno] as subalterno,")
			fabb.AppendLine("  [UNITA_IMMOBILIARI].[categoria] as categoria,")
			fabb.AppendLine("  [UNITA_IMMOBILIARI].[classe] as classe,")
			fabb.AppendLine("  [UNITA_IMMOBILIARI].[renditaEuro] as renditaEuro,")
			fabb.AppendLine("  [INDIRIZZI].[indirizzo] as indirizzo")

			fabb.AppendLine("FROM [IDENTIFICATIVI_IMMOBILIARI]")
			fabb.AppendLine("  LEFT OUTER JOIN [UNITA_IMMOBILIARI]")
			fabb.AppendLine("    ON [IDENTIFICATIVI_IMMOBILIARI].[idImmobile] =")
			fabb.AppendLine("    [UNITA_IMMOBILIARI].[idImmobile]")
			fabb.AppendLine("  LEFT OUTER JOIN [INDIRIZZI] ON [UNITA_IMMOBILIARI].[idImmobile] =")
			fabb.AppendLine("    [INDIRIZZI].[idImmobile]")
			fabb.AppendFormat(" WHERE [IDENTIFICATIVI_IMMOBILIARI].[foglio] = '{0}' AND [IDENTIFICATIVI_IMMOBILIARI].[numero] = '{1}'", foglio_formattato, numero_formattato)
		End If

		fabb.AppendLine("    order by foglio, numero, denominatore, subalterno")



		If Not String.IsNullOrEmpty(IDSogg) Then
			terr.AppendLine("SELECT [TITOLARITA].[idSoggetto] as idSoggetto,")
			terr.AppendLine("  [PARTICELLA].[codComune] as codComune,")
			terr.AppendLine("  [PARTICELLA].[idParticella] as idParticella,")
			terr.AppendLine("  [PARTICELLA].[foglio] as foglio,")
			terr.AppendLine("  [PARTICELLA].[numero] as numero,")
			terr.AppendLine("  [PARTICELLA].[denominatore] as denominatore,")
			terr.AppendLine("  [PARTICELLA].[subalterno] as subalterno,")
			terr.AppendLine("  [CARATTERISTICHE_PARTICELLA].[codQualita] as codQualita,")
			terr.AppendLine("  REPLACE([CARATTERISTICHE_PARTICELLA].[redditoDomEuro], ',','.') as redditoDomEuro,")
			terr.AppendLine("  REPLACE([CARATTERISTICHE_PARTICELLA].[redditoAgrEuro], ',','.') as redditoAgrEuro,")
			terr.AppendLine("  [COD_QUALITA].[decodifica] as qualita")
			terr.AppendLine("FROM [TITOLARITA]")
			terr.AppendLine("  LEFT OUTER JOIN [PARTICELLA] ON [TITOLARITA].[idParticella] =")
			terr.AppendLine("    [PARTICELLA].[idParticella]")
			terr.AppendLine("  LEFT OUTER JOIN [CARATTERISTICHE_PARTICELLA]")
			terr.AppendLine("    ON [PARTICELLA].[idParticella] =")
			terr.AppendLine("    [CARATTERISTICHE_PARTICELLA].[idParticella]")
			terr.AppendLine("  LEFT OUTER JOIN [COD_QUALITA]")
			terr.AppendLine("    ON [CARATTERISTICHE_PARTICELLA].[codQualita] =")
			terr.AppendLine("    [COD_QUALITA].[codice]")
			terr.AppendFormat(" WHERE [TITOLARITA].[idSoggetto] = {0} AND NOT [PARTICELLA].[idParticella] IS NULL", IDSogg)
		ElseIf Not String.IsNullOrEmpty(Foglio) AndAlso Not String.IsNullOrEmpty(Numero) Then
			terr.AppendLine("SELECT [PARTICELLA].[idParticella] as idParticella,")
			terr.AppendLine("  [PARTICELLA].[foglio] as foglio,")
			terr.AppendLine("  [PARTICELLA].[numero] as numero,")
			terr.AppendLine("  [PARTICELLA].[denominatore] as denominatore,")
			terr.AppendLine("  [PARTICELLA].[subalterno] as subalterno,")
			terr.AppendLine("  [CARATTERISTICHE_PARTICELLA].[codQualita] as codQualita,")
			terr.AppendLine("  REPLACE([CARATTERISTICHE_PARTICELLA].[redditoDomEuro], ',','.') as redditoDomEuro,")
			terr.AppendLine("  REPLACE([CARATTERISTICHE_PARTICELLA].[redditoAgrEuro], ',','.') as redditoAgrEuro,")
			terr.AppendLine("  [COD_QUALITA].[decodifica] as qualita")
			terr.AppendLine("FROM [PARTICELLA]")
			terr.AppendLine("  LEFT OUTER JOIN [CARATTERISTICHE_PARTICELLA]")
			terr.AppendLine("    ON [PARTICELLA].[idParticella] =")
			terr.AppendLine("    [CARATTERISTICHE_PARTICELLA].[idParticella]")
			terr.AppendLine("  LEFT OUTER JOIN [COD_QUALITA]")
			terr.AppendLine("    ON [CARATTERISTICHE_PARTICELLA].[codQualita] =")
			terr.AppendLine("    [COD_QUALITA].[codice]")
			terr.AppendFormat(" WHERE foglio = '{0}' AND numero = '{1}'", foglio_formattato, numero_formattato)
		End If

		terr.AppendLine("    order by foglio, numero, denominatore, subalterno")

		Dim ht As New Hashtable
		If String.IsNullOrEmpty(TipoImmobili) OrElse TipoImmobili.ToUpper = "TUTTI" Then
			ht.Add(FABBRICATI, fabb.ToString)
			ht.Add(PARTICELLE, terr.ToString)
		ElseIf TipoImmobili.ToUpper = FABBRICATI Then
			ht.Add(FABBRICATI, fabb.ToString)
		ElseIf TipoImmobili.ToUpper = PARTICELLE Then
			ht.Add(PARTICELLE, terr.ToString)
		End If
		Dim sqlh As New SQLiteHelper(BuildDBPath(p_PRJFOLDER))
		Dim dbr As DatabaseOperationResult = sqlh.GetData(ht)

		Return dbr.Serialize


	End Function

	<WebMethod>
	Public Function DettaglioImmobile(p_PRJFOLDER As String, IdImmobile As Integer, TipoImmobile As String, IndiceScheda As String) As String


		'// FABBRICATI
		If TipoImmobile = FABBRICATI Then
			Select Case IndiceScheda
				Case 1
					Return fabb_Scheda_1(p_PRJFOLDER, IdImmobile)
				Case 2
					Return fabb_Scheda_2(p_PRJFOLDER, IdImmobile)
				Case 3
					Return imm_Scheda_3(p_PRJFOLDER, IdImmobile, TipoImmobile)
				Case 4
					Return imm_Scheda_4(p_PRJFOLDER, IdImmobile, TipoImmobile)
				Case Else
					Return String.Empty
			End Select

		ElseIf TipoImmobile = PARTICELLE Then
			'// PARTICELLE
			Select Case IndiceScheda
				Case 1
					Return terr_Scheda_1(p_PRJFOLDER, IdImmobile)
				Case 2
					Return terr_Scheda_2(p_PRJFOLDER, IdImmobile)
				Case 3
					Return imm_Scheda_3(p_PRJFOLDER, IdImmobile, TipoImmobile)
				Case 4
					Return imm_Scheda_4(p_PRJFOLDER, IdImmobile, TipoImmobile)
				Case Else
					Return String.Empty
			End Select
		Else
			Return String.Empty
		End If

	End Function


	Public Function fabb_Scheda_1(p_PRJFOLDER As String, IdImmobile As Integer) As String
		Dim sql1 As String = String.Format("SELECT UI.*, cat.* FROM UNITA_IMMOBILIARI AS UI left outer join cod_categorie as cat on ui.categoria = cat.codice WHERE IdImmobile = {0}", IdImmobile)
		Dim sql2 As String = String.Format("SELECT I.*,c.decodifica as topo_desc FROM INDIRIZZI AS I left outer join cod_toponimo as c on i.toponimo=c.codice WHERE i.IdImmobile = {0}", IdImmobile)

		Dim sqlh As New SQLiteHelper(BuildDBPath(p_PRJFOLDER))
		Dim dbr As DatabaseOperationResult = sqlh.GetData(New String() {sql1, sql2})
		Dim ds As DataSet = dbr.ReturnDataset

		Dim fabb As DataRow = ds.Tables(0).Rows(0)


		Dim sb As New StringBuilder()

		sb.AppendFormat(section_title, "Dati relativi al classamento dell'unità immobiliare")

		With sb
			.AppendLine("<div class=""row"">")
			.AppendFormat(form_group, "3", "Zona:", fabb("zona"))
			.AppendFormat(form_group, "3", "Classe:", fabb("classe"))
			.AppendFormat(form_group, "3", "Consistenza:", fabb("consistenza"))
			.AppendFormat(form_group, "3", "Superficie:", fabb("superficie"))

			.AppendLine("</div>")

		End With
		With sb
			.AppendLine("<div class=""row"">")
			.AppendFormat(form_group, "6", "Rendita (Euro):", fabb("renditaEuro"))
			.AppendFormat(form_group, "6", "Partita:", fabb("partita"))

			.AppendLine("</div>")

		End With
		With sb
			.AppendLine("<div class=""row"">")
			.AppendFormat(form_group, "6", "Categoria:", String.Concat(fabb("categoria").ToString, " - ", fabb("decodifica").ToString))
			.AppendFormat(form_group, "6", "Annotazione:", fabb("annotazione").ToString)


			.AppendLine("</div>")

		End With


		With sb
			Dim classamento As String = String.Concat(fabb("flagClassamento"), " - ")

			Select Case fabb("flagClassamento").ToString
				Case "1" : classamento += "classamento proposto dalla parte"
				Case "2" : classamento += "classamento proposto dalla parte e validato dall’ufficio"
				Case "3" : classamento += "classamento automatico (attribuito in sostituzione del classamento proposto)"
				Case "4" : classamento += "classamento rettificato (in sostituzione del classamento proposto)"
				Case "5" : classamento += "classamento proposto divenuto definitivo per decorrenza termini."
			End Select
			.AppendLine("<div class=""row"">")
			.AppendFormat(form_group, "12", "Classamento:", classamento)

			.AppendLine("</div>")
		End With



		With sb
			.AppendLine("<div class=""row"">")
			.AppendFormat(form_group, "6", "Protocollo Notifica:", fabb("protocollonotifica"))
			.AppendFormat(form_group, "6", "Data Notifica:", fabb("datanotifica"))


			.AppendLine("</div>")


		End With

		sb.AppendFormat(section_title, "Dati relativi all'ubicazione dell'immobile nel fabbricato")

		With sb

			.AppendLine("<div class=""row"">")
			.AppendFormat(form_group, "3", "Lotto:", fabb("lotto"))
			.AppendFormat(form_group, "3", "Edificio:", fabb("edificio"))
			.AppendFormat(form_group, "2", "Scala:", fabb("scala"))
			.AppendFormat(form_group, "2", "Interno:", fabb("interno1"))
			.AppendFormat(form_group, "2", "Piano:", fabb("piano1"))


			.AppendLine("</div>")


		End With

		sb.AppendFormat(section_title, "Indirizzi")

		sb.AppendLine("<div class=""table-responsive"">")

		sb.AppendLine("	<table class=""table table-condensed table-bordered table-striped"">")
		sb.AppendLine("		<thead>")
		sb.AppendLine("			<tr>")
		sb.AppendLine("				<th class=""col-xs-3"">Indirizzo</th>")
		sb.AppendLine("				<th class=""col-xs-1"">Civico</th>")
		sb.AppendLine("				<th class=""col-xs-2"">Fonte</th>")
		sb.AppendLine("				<th class=""col-xs-2"">Delibera</th>")
		sb.AppendLine("				<th class=""col-xs-2"">Località</th>")
		sb.AppendLine("				<th class=""col-xs-1"">Km</th>")
		sb.AppendLine("				<th class=""col-xs-1"">CAP</th>")
		sb.AppendLine("			</tr>")
		sb.AppendLine("		</thead>")
		sb.AppendLine("		<tbody>")

		For Each i As DataRow In ds.Tables(1).Rows
			With sb
				.AppendFormat("	<tr>")

				.AppendFormat("		<td>{0} {1}</td>", i("topo_desc"), i("indirizzo"))
				.AppendFormat("		<td>{0}</td>", i("civico1"))
				.AppendFormat("		<td>{0}</td>", i("fonte"))
				.AppendFormat("		<td>{0}</td>", i("delibera"))
				.AppendFormat("		<td>{0}</td>", i("localita"))
				.AppendFormat("		<td>{0}</td>", i("km"))
				.AppendFormat("		<td>{0}</td>", i("cap"))
				.AppendFormat("	</tr>")
			End With

		Next

		sb.AppendLine("		</tbody>")
		sb.AppendLine("	</table>")
		sb.AppendLine("</div>")

		Return sb.ToString
	End Function

	Public Function fabb_Scheda_2(p_PRJFOLDER As String, IdImmobile As Integer) As String
		Dim sql1 As String = String.Format("SELECT II.* FROM IDENTIFICATIVI_IMMOBILIARI AS II WHERE IdImmobile = {0}", IdImmobile)
		Dim sqlh As New SQLiteHelper(BuildDBPath(p_PRJFOLDER))
		Dim dbr As DatabaseOperationResult = sqlh.GetData(New String() {sql1})
		Dim ds As DataSet = dbr.ReturnDataset


		Dim sb As New StringBuilder()
		sb.AppendFormat(section_title, "Identificativi")
		sb.AppendLine("<div class=""table-responsive"">")

		sb.AppendLine("	<table class=""table table-condensed table-bordered table-striped"">")
		sb.AppendLine("		<thead>")
		sb.AppendLine("			<tr>")
		sb.AppendLine("				<th>Sezione Urbana</th>")
		sb.AppendLine("				<th>Foglio</th>")
		sb.AppendLine("				<th>Numero</th>")
		sb.AppendLine("				<th>Denominatore</th>")
		sb.AppendLine("				<th>Subalterno</th>")

		sb.AppendLine("			</tr>")
		sb.AppendLine("		</thead>")
		sb.AppendLine("		<tbody>")

		For Each i As DataRow In ds.Tables(0).Rows
			With sb
				.AppendFormat("	<tr>")
				.AppendFormat("		<td>{0}</td>", i("sezioneurbana"))
				.AppendFormat("		<td>{0}</td>", i("foglio"))
				.AppendFormat("		<td>{0}</td>", i("numero"))
				.AppendFormat("		<td>{0}</td>", i("denominatore"))
				.AppendFormat("		<td>{0}</td>", i("subalterno"))
				.AppendFormat("	</tr>")
			End With

		Next

		sb.AppendLine("		</tbody>")
		sb.AppendLine("	</table>")
		sb.AppendLine("</div>")

		Return sb.ToString
	End Function

	Public Function imm_Scheda_3(p_PRJFOLDER As String, IdImmobile As Integer, TipoImmobile As String) As String
		Dim sql2 As String = String.Format("select " &
		 " ui.idmutazioneiniziale," &
		 " mut1.dataEfficacia as Ini_DataEfficacia, " &
		 " mut1.dataRegistrazione as Ini_DataRegistrazione, " &
		 " mut1.tipoNota as Ini_TipoNota," &
		 " mut1.numeronota as Ini_NumeroNota," &
		 " mut1.progrnota as Ini_ProgrNota," &
		 " mut1.annonota as Ini_AnnoNota," &
		 " mut1.codcausale as Ini_CodCausale," &
		 " mut1.desccausale as Ini_DescCausale," &
		 " ui.idmutazionefinale," &
		 " mut2.dataEfficacia as Fin_DataEfficacia, " &
		 " mut2.dataRegistrazione as Fin_DataRegistrazione, " &
		 " mut2.tipoNota as Fin_TipoNota," &
		 " mut2.numeronota as Fin_NumeroNota," &
		 " mut2.progrnota as Fin_ProgrNota," &
		 " mut2.annonota as Fin_AnnoNota," &
		 " mut2.codcausale as Fin_CodCausale," &
		 " mut2.desccausale as Fin_DescCausale" &
		 " from unita_immobiliari as ui left join dati_atto as mut1 on ui.idmutazioneiniziale=mut1.id left JOIN dati_atto as mut2 on ui.idmutazionefinale=mut2.id where idimmobile={0}", IdImmobile)

		Dim sql1 As New StringBuilder()
		With sql1
			If TipoImmobile = FABBRICATI Then

				.AppendLine("select ")
				.AppendLine("  ui.idmutazioneiniziale, ")
				.AppendLine("  mut1.dataefficacia as Ini_DataEfficacia, ")
				.AppendLine("  mut1.dataregistrazione as Ini_DataRegistrazione, ")
				.AppendLine("  mut1.tiponota as Ini_TipoNota, ")
				.AppendLine("  nota1.decodifica as Ini_DescNota,")
				.AppendLine("  mut1.numeronota as Ini_NumeroNota, ")
				.AppendLine("  mut1.progrnota as Ini_ProgrNota, ")
				.AppendLine("  mut1.annonota as Ini_AnnoNota, ")
				.AppendLine("  mut1.codcausale as Ini_CodCausale, ")
				.AppendLine("  mut1.desccausale as Ini_DescCausale, ")
				.AppendLine("  ui.idmutazionefinale, ")
				.AppendLine("  mut2.dataefficacia as Fin_DataEfficacia, ")
				.AppendLine("  mut2.dataregistrazione as Fin_DataRegistrazione, ")
				.AppendLine("  mut2.tiponota as Fin_TipoNota, ")
				.AppendLine("  nota2.decodifica as Fin_DescNota,")
				.AppendLine("  mut2.numeronota as Fin_NumeroNota, ")
				.AppendLine("  mut2.progrnota as Fin_ProgrNota, ")
				.AppendLine("  mut2.annonota as Fin_AnnoNota, ")
				.AppendLine("  mut2.codcausale as Fin_CodCausale, ")
				.AppendLine("  mut2.desccausale as Fin_DescCausale ")
				.AppendLine("from unita_immobiliari as ui ")
				.AppendLine("  left join dati_atto as mut1 on ")
				.AppendLine("    ui.idmutazioneiniziale=mut1.id ")
				.AppendLine("  left join dati_atto as mut2 on ")
				.AppendLine("    ui.idmutazionefinale=mut2.id ")
				.AppendLine("  left join cod_nota as nota1 on")
				.AppendLine("       Ini_TipoNota = nota1.codice and nota1.tipocatasto = 'F' ")
				.AppendLine("  left join cod_nota as nota2 on")
				.AppendLine("       Fin_TipoNota = nota2.codice and nota2.tipocatasto = 'F' ")
				.AppendLine("where")
				.AppendFormat("  ui.idimmobile={0} ", IdImmobile)

			ElseIf TipoImmobile = PARTICELLE Then
				.AppendLine("select ")
				.AppendLine("  part.idmutazioneiniziale, ")
				.AppendLine("  mut1.dataefficacia as Ini_DataEfficacia, ")
				.AppendLine("  mut1.dataregistrazione as Ini_DataRegistrazione, ")
				.AppendLine("  mut1.tiponota as Ini_TipoNota,")
				.AppendLine("  nota1.decodifica as Ini_DescNota, ")
				.AppendLine("  mut1.numeronota as Ini_NumeroNota, ")
				.AppendLine("  mut1.progrnota as Ini_ProgrNota, ")
				.AppendLine("  mut1.annonota as Ini_AnnoNota, ")
				.AppendLine("  mut1.codcausale as Ini_CodCausale, ")
				.AppendLine("  mut1.desccausale as Ini_DescCausale, ")
				.AppendLine("  part.idmutazionefinale, ")
				.AppendLine("  mut2.dataefficacia as Fin_DataEfficacia, ")
				.AppendLine("  mut2.dataregistrazione as Fin_DataRegistrazione, ")
				.AppendLine("  mut2.tiponota as Fin_TipoNota, ")
				.AppendLine("  nota2.decodifica as Fin_DescNota, ")
				.AppendLine("  mut2.numeronota as Fin_NumeroNota, ")
				.AppendLine("  mut2.progrnota as Fin_ProgrNota, ")
				.AppendLine("  mut2.annonota as Fin_AnnoNota, ")
				.AppendLine("  mut2.codcausale as Fin_CodCausale, ")
				.AppendLine("  mut2.desccausale as Fin_DescCausale ")
				.AppendLine("from caratteristiche_particella as part ")
				.AppendLine("  left join dati_atto as mut1 on ")
				.AppendLine("    part.idmutazioneiniziale=mut1.id ")
				.AppendLine("  left join dati_atto as mut2 on ")
				.AppendLine("    part.idmutazionefinale=mut2.id ")
				.AppendLine("  left join cod_nota as nota1 on")
				.AppendLine("       Ini_TipoNota = nota1.codice and nota1.tipocatasto = 'T' ")
				.AppendLine("  left join cod_nota as nota2 on")
				.AppendLine("       Fin_TipoNota = nota2.codice and nota2.tipocatasto = 'T' ")
				.AppendLine("where")
				.AppendFormat("  part.idparticella={0}", IdImmobile)
			End If

		End With


		Dim sqlh As New SQLiteHelper(BuildDBPath(p_PRJFOLDER))
		Dim dbr As DatabaseOperationResult = sqlh.GetData(New String() {sql1.ToString})
		Dim ds As DataSet = dbr.ReturnDataset


		Dim atto As DataRow = ds.Tables(0).Rows(0)

		Dim sb As New StringBuilder()

		sb.AppendFormat(section_title, "Dati relativi all'atto che ha generato la situazione oggettiva dell'unità")

		With sb
			.AppendLine("<div class=""row"">")
			.AppendFormat(form_group, "4", "Data di efficacia:", FormattaData(atto("Ini_DataEfficacia")))
			.AppendFormat(form_group, "4", "Data Reg. in atti:", FormattaData(atto("Ini_DataRegistrazione")))
			.AppendFormat(form_group, "4", "Tipo nota:", String.Concat(atto("Ini_TipoNota"), " - ", atto("Ini_DescNota")))

			.AppendLine("</div>")

		End With

		With sb
			.AppendLine("<div class=""row"">")
			.AppendFormat(form_group, "4", "Numero nota:", atto("Ini_NumeroNota"))
			.AppendFormat(form_group, "4", "Progressivo Nota:", atto("Ini_ProgrNota"))
			.AppendFormat(form_group, "4", "Anno nota:", atto("Ini_AnnoNota"))

			.AppendLine("</div>")

		End With

		With sb
			.AppendLine("<div class=""row"">")
			.AppendFormat(form_group, "4", "Id. mutazione iniziale:", atto("idmutazioneiniziale"))
			.AppendFormat(form_group, "4", "Codice causale atto generante:", atto("Ini_CodCausale"))


			.AppendLine("</div>")

		End With

		With sb
			.AppendLine("<div class=""row"">")
			.AppendFormat(form_group, "12", "Descrizione atto generante:", atto("Ini_DescCausale"))

			.AppendLine("</div>")

		End With

		sb.AppendFormat(section_title, "Dati relativi all'atto che ha concluso la situazione oggettiva dell'unità")

		With sb
			.AppendLine("<div class=""row"">")
			.AppendFormat(form_group, "4", "Data di efficacia:", FormattaData(atto("Fin_DataEfficacia")))
			.AppendFormat(form_group, "4", "Data Reg. in atti:", FormattaData(atto("Fin_DataRegistrazione")))
			.AppendFormat(form_group, "4", "Tipo nota:", String.Concat(atto("Fin_TipoNota"), " - ", atto("Fin_DescNota")))

			.AppendLine("</div>")

		End With

		With sb
			.AppendLine("<div class=""row"">")
			.AppendFormat(form_group, "4", "Numero nota:", atto("Fin_NumeroNota"))
			.AppendFormat(form_group, "4", "Progressivo Nota:", atto("Fin_ProgrNota"))
			.AppendFormat(form_group, "4", "Anno nota:", atto("Fin_AnnoNota"))

			.AppendLine("</div>")

		End With

		With sb
			.AppendLine("<div class=""row"">")
			.AppendFormat(form_group, "4", "Id. mutazione finale:", atto("idmutazionefinale"))
			.AppendFormat(form_group, "4", "Codice causale atto conclusivo:", atto("Fin_CodCausale"))


			.AppendLine("</div>")

		End With

		With sb
			.AppendLine("<div class=""row"">")
			.AppendFormat(form_group, "12", "Descrizione atto conclusivo:", atto("Fin_DescCausale"))

			.AppendLine("</div>")

		End With


		Return sb.ToString
	End Function

	Public Function imm_Scheda_4(p_PRJFOLDER As String, IdImmobile As Integer, TipoImmobile As String) As String
		Dim sql1 As New StringBuilder()
		With sql1
			.AppendLine(" select tit.idSoggetto, tit.tipoSoggetto,")
			.AppendLine(" pf.cognome || ' ' || pf.nome as denominazione, pf.datanascita as datanascita, pf.codfiscale as codfiscale, cod_comune.decodifica as comuneNascita,")
			.AppendLine(" tit.coddiritto, cod_diritto.decodifica as diritto, tit.titolononcod, tit.quotanum, tit.quotaden,")
			.AppendLine(" tit.identifMutazIni, atto_ini.dataEfficacia as dataEfficaciaIni, atto_ini.dataRegistrazione as dataRegistrazioneIni, atto_ini.numeroNota as numeroNotaIni, atto_ini.tipoNota as tipoNotaIni, atto_ini.codCausale as codCausaleIni, atto_ini.descCausale as descCausaleIni,")
			.AppendLine(" tit.identifMutazFin, atto_fin.dataEfficacia as dataEfficaciaFin, atto_Fin.dataRegistrazione as dataRegistrazioneFin, atto_Fin.numeroNota as numeroNotaFin, atto_Fin.tipoNota as tipoNotaFin, atto_Fin.codCausale as codCausaleFin, atto_Fin.descCausale as descCausaleFin")
			.AppendLine(" from titolarita as tit ")
			.AppendLine(" left join cod_diritto on tit.coddiritto = cod_diritto.codice")
			.AppendLine(" left join dati_atto as atto_ini on tit.identifMutazIni=atto_ini.id")
			.AppendLine(" left join dati_atto as atto_fin on tit.identifMutazFin=atto_fin.id")
			.AppendLine(" left join persona_fisica as pf on tit.idsoggetto=pf.idsoggetto ")
			.AppendLine(" left join cod_comune on pf.luogoNascita = cod_comune.codice")
			.AppendLine(" where tit.tiposoggetto='P'")

			If TipoImmobile = FABBRICATI Then
				.AppendFormat("  and tit.idImmobile={0}", IdImmobile)
			ElseIf TipoImmobile = PARTICELLE Then
				.AppendFormat("  and tit.idParticella={0}", IdImmobile)
			End If


			.AppendLine(" union")

			.AppendLine(" select tit.idSoggetto, tit.tipoSoggetto,")
			.AppendLine(" pg.denominazione, null as datanascita, pg.codfiscale as codfiscale, cod_comune.decodifica as sede,")
			.AppendLine(" tit.coddiritto, cod_diritto.decodifica as diritto, tit.titolononcod, tit.quotanum, tit.quotaden,")
			.AppendLine(" tit.identifMutazIni, atto_ini.dataEfficacia as dataEfficaciaIni, atto_ini.dataRegistrazione as dataRegistrazioneIni, atto_ini.numeroNota as numeroNotaIni, atto_ini.tipoNota as tipoNotaIni, atto_ini.codCausale as codCausaleIni, atto_ini.descCausale as descCausaleIni,")
			.AppendLine(" tit.identifMutazFin, atto_fin.dataEfficacia as dataEfficaciaFin, atto_Fin.dataRegistrazione as dataRegistrazioneFin, atto_Fin.numeroNota as numeroNotaFin, atto_Fin.tipoNota as tipoNotaFin, atto_Fin.codCausale as codCausaleFin, atto_Fin.descCausale as descCausaleFin")
			.AppendLine(" from titolarita as tit ")
			.AppendLine(" left join cod_diritto on tit.coddiritto = cod_diritto.codice")
			.AppendLine(" left join dati_atto as atto_ini on tit.identifMutazIni=atto_ini.id")
			.AppendLine(" left join dati_atto as atto_fin on tit.identifMutazFin=atto_fin.id")
			.AppendLine(" left join persona_giuridica as pg on tit.idsoggetto=pg.idsoggetto ")
			.AppendLine(" left join cod_comune on pg.sede = cod_comune.codice")
			.AppendLine(" where tit.tiposoggetto='G'")

			If TipoImmobile = FABBRICATI Then
				.AppendFormat("  and tit.idImmobile={0}", IdImmobile)
			ElseIf TipoImmobile = PARTICELLE Then
				.AppendFormat("  and tit.idParticella={0}", IdImmobile)
			End If
		End With
		Dim sqlh As New SQLiteHelper(BuildDBPath(p_PRJFOLDER))
		Dim dbr As DatabaseOperationResult = sqlh.GetData(New String() {sql1.ToString})

		If Not String.IsNullOrEmpty(dbr.ErrorMessage) Then
			Return dbr.ErrorMessage
		End If

		Dim ds As DataSet = dbr.ReturnDataset

		Dim sb As New StringBuilder()
		sb.AppendFormat(section_title, "Intestatari")

		sb.AppendLine("<div class=""panel-group"">")

		For i As Integer = 0 To ds.Tables(0).Rows.Count - 1
			Dim dr As DataRow = ds.Tables(0).Rows(i)

			sb.AppendFormat("<div class=""panel panel-default"" id=""panel_{0}"" >", i)

			Dim titolo As String = String.Concat("<strong>", dr("codFiscale"), " - ", dr("denominazione"), "</strong>")

			sb.AppendFormat(panel_heading, i, titolo)

			Dim content As New StringBuilder

			content.AppendFormat(section_title, "Dettagli titolarità")
			With content
				.AppendLine("<div class=""row"">")
				If String.IsNullOrEmpty(dr("titolononcod")) Then

					.AppendFormat(form_group, "12", "Titolarità:", String.Concat(dr("diritto"), " ", dr("quotanum"), "/", dr("quotaden")))
				End If



				.AppendLine("</div>")

				.AppendLine("<div class=""row"">")
				.AppendFormat(form_group, "4", "Codice Fiscale:", dr("codFiscale"))
				.AppendFormat(form_group, "8", "Denominazione:", dr("denominazione"))




				.AppendLine("</div>")

				.AppendLine("<div class=""row"">")
				.AppendFormat(form_group, "4", "Data di nascita:", FormattaData(dr("datanascita")))
				.AppendFormat(form_group, "8", "Luogo di nascita:", dr("comunenascita"))




				.AppendLine("</div>")

			End With

			content.AppendFormat(section_title, "Inizio titolarità")
			With content
				.AppendLine("<div class=""row"">")
				.AppendFormat(form_group, "4", "Data di validità:", FormattaData(dr("dataEfficaciaIni")))
				.AppendFormat(form_group, "4", "Data di registrazione:", FormattaData(dr("dataRegistrazioneIni")))
				.AppendFormat(form_group, "4", "Numero e tipo nota:", String.Concat(dr("numeroNotaIni"), " - ", dr("tipoNotaIni")))

				.AppendLine("</div>")

			End With

			With content
				.AppendLine("<div class=""row"">")
				.AppendFormat(form_group, "4", "Id mutazione:", dr("identifMutazIni"))
				.AppendFormat(form_group, "8", "Causale:", String.Concat(dr("codCausaleIni"), " - ", dr("descCausaleIni")))


				.AppendLine("</div>")

			End With

			content.AppendFormat(section_title, "Fine titolarità")
			With content
				.AppendLine("<div class=""row"">")
				.AppendFormat(form_group, "4", "Data di validità:", FormattaData(dr("dataEfficaciaFin")))
				.AppendFormat(form_group, "4", "Data di registrazione:", FormattaData(dr("dataRegistrazioneFin")))
				.AppendFormat(form_group, "4", "Numero e tipo nota:", String.Concat(dr("numeroNotaFin"), " - ", dr("tipoNotaFin")))

				.AppendLine("</div>")

			End With

			With content
				.AppendLine("<div class=""row"">")
				.AppendFormat(form_group, "4", "Id mutazione:", dr("identifMutazFin"))
				.AppendFormat(form_group, "8", "Causale:", String.Concat(dr("codCausaleFin"), " - ", dr("descCausaleFin")))


				.AppendLine("</div>")

			End With

			sb.AppendFormat(panel_collapse, i, content.ToString)
			sb.AppendLine("</div>")
		Next

		sb.AppendLine("</div>")



		Return sb.ToString
	End Function


	Public Function terr_Scheda_1(p_PRJFOLDER As String, IdImmobile As Integer) As String
		Dim sql1 As New StringBuilder()
		sql1.AppendLine("select part.foglio, part.numero, part.denominatore, part.subalterno,")
		sql1.AppendLine("car.codqualita, cod_qualita.decodifica as qualita, car.classe, car.ettari, car.are, car.centiare, replace(car.redditodomeuro, ',','.') as redditodomeuro, replace(car.redditoAgrEuro,',','.') as redditoAgrEuro,")
		sql1.AppendLine("car.partita,car.annotazione")
		sql1.AppendLine("from particella as part ")
		sql1.AppendLine("left join CARATTERISTICHE_PARTICELLA as car on part.idparticella=car.idparticella")
		sql1.AppendLine("left join cod_qualita on car.codqualita=cod_qualita.codice")
		sql1.AppendFormat("where part.idparticella={0}", IdImmobile)

		Dim sqlh As New SQLiteHelper(BuildDBPath(p_PRJFOLDER))
		Dim dbr As DatabaseOperationResult = sqlh.GetData(New String() {sql1.ToString})
		Dim ds As DataSet = dbr.ReturnDataset


		Dim part As DataRow = ds.Tables(0).Rows(0)


		Dim sb As New StringBuilder()
		sb.AppendFormat(section_title, "Dati relativi al classamento")

		With sb
			.AppendLine("<div class=""row"">")
			.AppendFormat(form_group, "3", "Classe:", part("classe"))
			.AppendFormat(form_group, "3", "Ettari:", part("ettari"))
			.AppendFormat(form_group, "3", "Are:", part("are"))
			.AppendFormat(form_group, "3", "Centiare:", part("centiare"))
			.AppendLine("</div>")

		End With


		With sb
			.AppendLine("<div class=""row"">")
			.AppendFormat(form_group, "6", "Qualità:", part("codqualita"))
			.AppendFormat(form_group, "6", "Descrizione qualità:", part("qualita"))

			.AppendLine("</div>")



		End With


		With sb

			.AppendLine("<div class=""row"">")
			.AppendFormat(form_group, "6", "Reddito domenicale (Euro):", part("redditodomeuro"))
			.AppendFormat(form_group, "6", "Reddito agrario (Euro):", part("redditoAgrEuro"))

			.AppendLine("</div>")



		End With


		With sb

			.AppendLine("<div class=""row"">")
			.AppendFormat(form_group, "6", "Partita:", part("partita"))
			.AppendFormat(form_group, "6", "Annotazione:", part("annotazione"))
			.AppendLine("</div>")


		End With




		sb.AppendFormat(section_title, "Dati identificativi del terreno")


		With sb
			With sb
				.AppendLine("<div class=""row"">")
				.AppendFormat(form_group, "3", "Foglio:", part("foglio"))
				.AppendFormat(form_group, "3", "Numero:", part("numero"))
				.AppendFormat(form_group, "3", "Denominatore:", part("denominatore"))
				.AppendFormat(form_group, "3", "Subalterno:", part("subalterno"))
				.AppendLine("</div>")

			End With

		End With



		Return sb.ToString
	End Function

	Public Function terr_Scheda_2(p_PRJFOLDER As String, IdImmobile As Integer) As String
		Dim sql1 As New StringBuilder()
		With sql1
			.AppendLine("select ")
			.AppendLine("  porz.*, ")
			.AppendLine("  cod_qualita.decodifica as desc_qualita ")
			.AppendLine("from porzioni_particella as porz ")
			.AppendLine("  left join cod_qualita on ")
			.AppendLine("    porz.qualita= cod_qualita.codice ")
			.AppendLine("where")
			.AppendFormat("  idparticella={0} ", IdImmobile)
		End With

		Dim sqlh As New SQLiteHelper(BuildDBPath(p_PRJFOLDER))
		Dim dbr As DatabaseOperationResult = sqlh.GetData(New String() {sql1.ToString})
		Dim ds As DataSet = dbr.ReturnDataset


		Dim sb As New StringBuilder()

		sb.AppendFormat(section_title, "Porzioni")




		sb.AppendLine("<div class=""table-responsive"">")

		sb.AppendLine("	<table class=""table table-condensed table-bordered table-striped"">")
		sb.AppendLine("		<thead>")
		sb.AppendLine("			<tr>")
		sb.AppendLine("				<th>Porzione</th>")
		sb.AppendLine("				<th>Qualità</th>")
		sb.AppendLine("				<th>Classe</th>")
		sb.AppendLine("				<th>Ettari</th>")
		sb.AppendLine("				<th>Are</th>")
		sb.AppendLine("				<th>Centiare</th>")

		sb.AppendLine("			</tr>")
		sb.AppendLine("		</thead>")
		sb.AppendLine("		<tbody>")

		For Each i As DataRow In ds.Tables(0).Rows
			With sb
				.AppendFormat("	<tr>")
				.AppendFormat("		<td>{0}</td>", i("IdPorzione"))
				.AppendFormat("		<td>{0} - {1}</td>", i("qualita"), i("desc_qualita"))
				.AppendFormat("		<td>{0}</td>", i("classe"))
				.AppendFormat("		<td>{0}</td>", i("ettari"))
				.AppendFormat("		<td>{0}</td>", i("are"))
				.AppendFormat("		<td>{0}</td>", i("centiare"))
				.AppendFormat("	</tr>")
			End With

		Next

		sb.AppendLine("		</tbody>")
		sb.AppendLine("	</table>")
		sb.AppendLine("</div>")

		Return sb.ToString
	End Function



#Region "Utilità"
	Public Function BuildDBPath(CodiceCatasto As String) As String
		'D:\PROGETTI\PITER\PRJ\{0}\Catasto
		Return IO.Path.Combine(String.Format(ConfigurationManager.AppSettings("EDCBasePath"), CodiceCatasto), "edc.db")
	End Function

	Public Function AddLeadingZeros(num As Object, zeros As Integer) As String
		Dim n As Integer
		If Integer.TryParse(num, n) Then
			Return num.ToString.PadLeft(zeros, "0")
		Else
			Return num
		End If
	End Function

	Public Function FormattaData(valore As Object) As String
		Try
			If Not IsDBNull(valore) Then
				If valore.ToString.Length = 8 Then
					Return String.Concat(valore.Substring(0, 2), "/", valore.Substring(2, 2), "/", valore.Substring(4, 4))
				Else
					If DateTime.TryParse(valore, New DateTime) Then
						Return String.Format("{0:dd/MM/yyyy}", valore)
					Else
						If valore.ToString.Length > 8 Then
							Dim pezzi() As String = valore.ToString.Split("-")
							If pezzi.Length > 0 Then
								Return String.Concat(pezzi(2), "/", pezzi(1), "/", pezzi(0))
							Else
								Return valore.ToString
							End If
						Else
							Return valore.ToString
						End If
					End If


				End If
			Else
				Return valore.ToString
			End If

		Catch ex As Exception
			Return valore.ToString
		End Try


	End Function
#End Region
End Class