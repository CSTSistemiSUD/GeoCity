Imports System.Web.Services
Imports iTextSharp.text
Imports iTextSharp.text.pdf

Public Class Viewer
	Inherits System.Web.UI.Page


	Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
		Dim mu As MembershipUser
		Dim userType As Integer
		'SELECT *, shape.STCentroid().STX, shape.STCentroid().STY From [dbo].[PARTICELLE]
		Try
			Dim params As SSOParams = Nothing
			Dim codice As String = String.Empty
			Dim ambiente As String = ""
			Dim funzione As String = ""
			If Page.User.Identity.IsAuthenticated Then
				codice = Request.Form("CODICE")

				mu = Membership.GetUser(Page.User.Identity.Name)

			Else

				Dim token As String = Request.Form("token")
				params = IdentityHelper.GetAuthToken(token)
				If Not String.IsNullOrEmpty(params.ErrorMessage) Then
					Throw New Exception(params.ErrorMessage)
				Else
					If Membership.ValidateUser(params.UserName, params.UserPwd) Then
						codice = params.CodiceComune
						ambiente = params.Ambiente
						funzione = params.Funzione

						mu = Membership.GetUser(params.UserName)

					Else
						Throw New Exception("Utenza non valida!")
					End If
				End If


			End If
			'-- Estrae tipo utenza
			If Not mu Is Nothing Then
				userType = GetUserType(mu, codice)
			End If

			Dim prj As DataRow = Nothing
			If String.IsNullOrEmpty(codice) Then
				Throw New Exception("Codice Comune non specificato!")
			Else
				Dim query As String = String.Format("SELECT * FROM [dbo].[PRJ] WHERE [CODICE] = '{0}';", codice)
				Dim ssh As New SSHelper
				Dim dbResult As DatabaseOperationResult = ssh.GetData(query)
				prj = dbResult.GetRow(0, 0)

			End If

			Dim SUED_LastUpdate As String = ""
			Dim particella_oid As Integer = -1
			If prj Is Nothing Then
				Throw New Exception("Il Codice Comune non ha un progetto attivo!")

			Else

				'-- Aggiornamento SUED
				Dim SUED_NeedUpdate As Boolean = False

				If Not IsDBNull(prj("SUED_LAST_UPDATE")) Then
					If DateDiff(DateInterval.Hour, prj("SUED_LAST_UPDATE"), Now) > 24 Then
						SUED_NeedUpdate = True
					Else
						SUED_LastUpdate = String.Format("{0:dd/MM/yyyy HH:mm}", prj("SUED_LAST_UPDATE"))
					End If
				End If
				If SUED_NeedUpdate Then
					SUED_LastUpdate = SUED_Update(prj("PRJ_ID"), prj("GEODATABASE"))
				End If


				If Not IsNothing(params) AndAlso params.DatiCatastali.Count > 0 Then
					'-- Per il momento analizzo solo il primo
					Dim queryParticella As String = String.Format("SELECT OBJECTID 
					FROM  [dbo].[PARTICELLE] 
					WHERE COMUNE = '{0}' AND FOGLIO =  '{1}' AND NUMERO = '{2}';", params.DatiCatastali(0).CodiceComune, params.DatiCatastali(0).Foglio, params.DatiCatastali(0).Particella)

					Dim ssh As New SSHelper(prj("GEODATABASE"))
					Dim dbResult As DatabaseOperationResult = ssh.GetData(queryParticella)
					Dim particella As DataRow = dbResult.GetRow(0, 0)
					If Not particella Is Nothing Then
						particella_oid = particella("OBJECTID")
					End If
				End If
			End If



			Dim jsParams As New Hashtable
			Dim jsCode As New StringBuilder
			jsParams.Add("_userType", userType)
			jsParams.Add("_codice", prj("CODICE"))
			jsParams.Add("_servizio", prj("SERVIZIO"))
			jsParams.Add("_descrizione", prj("DESCRIZIONE"))
			jsParams.Add("_ambiente", ambiente)
			jsParams.Add("_funzione", funzione)
			jsParams.Add("_particella_oid", particella_oid)
			jsParams.Add("_exception", "")

			'--
			jsParams.Add("_layouts", Utility.JsonSerialize(EstraiLayouts(codice)))

			jsParams.Add("_sued_last_update", SUED_LastUpdate)

			For Each de As DictionaryEntry In jsParams
				jsCode.AppendFormat("var {0} = '{1}';", de.Key, de.Value)
			Next

			If Not Page.ClientScript.IsStartupScriptRegistered(GetType(String), "globalVars") Then
				Page.ClientScript.RegisterStartupScript(GetType(String), "globalVars", jsCode.ToString, True)
			End If
		Catch ex As Exception
			If Not Page.ClientScript.IsStartupScriptRegistered(GetType(String), "hasError") Then
				Page.ClientScript.RegisterStartupScript(GetType(String), "hasError", String.Format("var _exception = '{0}';", HttpUtility.JavaScriptStringEncode(ex.Message)), True)
			End If
		End Try





	End Sub

	Private Function GetUserType(mu As MembershipUser, codice As String)
		Dim ssh As New SSHelper
		Dim cmd As New SqlClient.SqlCommand("SELECT USERTYPE FROM [PRJ_USERS] WHERE [CODICE] = @CODICE AND [USERID] = @USERID")
		cmd.Parameters.AddWithValue("@CODICE", codice)
		cmd.Parameters.AddWithValue("@USERID", mu.ProviderUserKey)
		Dim dbr As DatabaseOperationResult = ssh.GetData(cmd)
		Dim row As DataRow = dbr.GetRow(0, 0)


		Return If(IsDBNull(row(0)), 2, row(0))
	End Function

	Private Function SUED_Update(PrjID As Integer, GeoDatabase As String) As String
		Dim lastUpdate As String = ""
		Dim ssh As New SSHelper("OWKSUED")
		Dim cmdList As New List(Of SqlClient.SqlCommand)

		Dim updateStatoTemplate = "UPDATE Particelle SET PE_STATO = '{0}' WHERE {1}"
		Dim updateTipologiaTemplate = "UPDATE Particelle SET PE_TIPOLOGIA = '{0}' WHERE {1}"
		Dim updateAsservimentoTemplate = "UPDATE Particelle SET PE_ASSERVIMENTO = '{0}' WHERE {1}"
		Try
			Dim dbr As DatabaseOperationResult = ssh.GetData("SELECT * FROM [dbo].[PSYS_INTEGRAZIONE_SIT]")
			If Not String.IsNullOrEmpty(dbr.ErrorMessage) Then
				Throw New Exception(dbr.ErrorMessage)
			End If
			Dim viewData As DataTable = dbr.GetTable(0)
			'-- tutte le particelle
			Dim catastali As DataTable = viewData.DefaultView.ToTable(True, New String() {"foglio", "particella"})
			' -- gli stati, le tipologie doc e gli asservimenti per particella
			Dim stati As DataTable = viewData.DefaultView.ToTable(True, New String() {"statopratica", "foglio", "particella"})
			Dim tipologie As DataTable = viewData.DefaultView.ToTable(True, New String() {"SIGLATIPODOC", "foglio", "particella"})
			Dim asservimenti As DataTable = viewData.DefaultView.ToTable(True, New String() {"tipo_asservimento", "foglio", "particella"})

			For Each dr As DataRow In catastali.Rows
				Dim fg As String = dr("foglio").ToString.Trim
				Dim num As String = dr("particella").ToString.Trim
				Dim selectFilter As String = String.Format("foglio = '{0}' and particella = '{1}'", fg, num)
				Dim updateFilter As String = String.Format("FOGLIO = '{0}' and NUMERO = '{1}'", fg, num)
				'-- Aggiornamento PE_STATO
				Dim updateStatoCmd As New SqlClient.SqlCommand
				Dim stati_particelle() As DataRow = stati.Select(selectFilter)
				If stati_particelle.Length = 1 Then
					updateStatoCmd.CommandText = String.Format(updateStatoTemplate, stati_particelle(0)("statopratica").ToString.ToUpper, updateFilter)
				Else
					updateStatoCmd.CommandText = String.Format(updateStatoTemplate, "MISTO", updateFilter)
				End If
				cmdList.Add(updateStatoCmd)
				'-- Aggiornamento PE_TIPOLOGIA
				Dim updateTipologiaCmd As New SqlClient.SqlCommand
				Dim tipologie_particelle() As DataRow = tipologie.Select(selectFilter)
				If tipologie_particelle.Length = 1 Then
					updateTipologiaCmd.CommandText = String.Format(updateTipologiaTemplate, tipologie_particelle(0)("SIGLATIPODOC").ToString.ToUpper, updateFilter)
				Else
					updateTipologiaCmd.CommandText = String.Format(updateTipologiaTemplate, "MISTO", updateFilter)
				End If
				cmdList.Add(updateTipologiaCmd)
				'-- Aggiornamento PE_ASSERVIMENTO
				Dim updateAsservimentoCmd As New SqlClient.SqlCommand
				Dim asservimenti_particelle() As DataRow = asservimenti.Select(selectFilter)
				If asservimenti_particelle.Length = 1 Then
					updateAsservimentoCmd.CommandText = String.Format(updateAsservimentoTemplate, asservimenti_particelle(0)("tipo_asservimento").ToString.ToUpper, updateFilter)
				Else
					updateAsservimentoCmd.CommandText = String.Format(updateAsservimentoTemplate, "MISTO", updateFilter)
				End If
				cmdList.Add(updateAsservimentoCmd)
			Next

			If cmdList.Count > 0 Then
				Dim cmd As New SqlClient.SqlCommand("UPDATE [PITER_Config].[dbo].[PRJ] SET [SUED_LAST_UPDATE] = @NOW WHERE PRJ_ID = @PRJ_ID")
				cmd.Parameters.AddWithValue("@PRJ_ID", PrjID)
				cmd.Parameters.AddWithValue("@NOW", Now)
				cmdList.Add(cmd)
			End If
			ssh = New SSHelper(GeoDatabase)
			Dim spr = ssh.ExecuteStored(cmdList)

			If String.IsNullOrEmpty(spr.ErrorMessage) Then
				lastUpdate = String.Format("{0:dd/MM/yyyy HH:mm}", Now)
			End If
			Debug.WriteLine(spr.ErrorMessage)
		Catch ex As Exception
			Debug.WriteLine(ex.Message)
		End Try

		Return lastUpdate

	End Function


	Private Function EstraiLayouts(CodiceComune As String)
		Dim PointToMillimeter As Double = 0.352777778
		Dim options As New List(Of Object)
		Dim jsonResult As String = String.Empty
		Try
			Dim layoutsFolderPath As String = String.Format(ConfigurationManager.AppSettings("LayoutsBasePath"), CodiceComune)
			Dim layoutsFolder As System.IO.DirectoryInfo = New System.IO.DirectoryInfo(layoutsFolderPath)


			Dim optionValueTemplate = "{0}|{1}|{2}|{3}"

			For Each fi As System.IO.FileInfo In layoutsFolder.GetFiles("*.pdf", System.IO.SearchOption.TopDirectoryOnly)

				Dim reader As iTextSharp.text.pdf.PdfReader = New iTextSharp.text.pdf.PdfReader(fi.FullName)
				Dim rect As iTextSharp.text.Rectangle = reader.GetPageSize(1)
				Dim orientation As String = IIf(rect.Height > rect.Width, "V", "O")

				Using ms1 As New System.IO.MemoryStream
					Using stamper As iTextSharp.text.pdf.PdfStamper = New iTextSharp.text.pdf.PdfStamper(reader, ms1)
						Dim form As iTextSharp.text.pdf.AcroFields = stamper.AcroFields
						Dim mapContainer As List(Of iTextSharp.text.pdf.AcroFields.FieldPosition) = form.GetFieldPositions("MapContainer")
						If Not mapContainer Is Nothing AndAlso mapContainer.Count > 0 Then
							Dim mapContainerRect As iTextSharp.text.Rectangle = mapContainer(0).position
							Dim fileName As String = System.IO.Path.GetFileNameWithoutExtension(fi.Name)
							Dim Wmm As String = (mapContainerRect.Width * PointToMillimeter).ToString.Replace(",", ".")
							Dim Hmm As String = (mapContainerRect.Height * PointToMillimeter).ToString.Replace(",", ".")
							'mapContainerRect.Width, mapContainerRect.Height
							options.Add(New With {.value = String.Format(optionValueTemplate, fileName, Wmm, Hmm, orientation), .label = System.IO.Path.GetFileNameWithoutExtension(fi.Name)})
						End If
					End Using
				End Using

			Next


		Catch ex As Exception

		End Try
		options.Sort(Function(x, y) y.label.compareto(x.label))
		Return options
	End Function

	'Private Sub CreaPannello(PanelId As String, PanelTitle As String, PanelIcon As String)
	'	Dim tmplPanelButton As String = "<a href=""#"" class=""btn btn-default"" data-target=""#panel{0}"" aria-haspopup=""true"" title=""{1}""><span class=""{2}""></span></a>"

	'	Dim tmplPanel As String =
	'		"<div id=""panel{0}"" class=""panel collapse"">
	'			<div id=""heading{0}"" class=""panel-heading"" role=""tab"">
	'				<div class=""panel-title"">
	'					<a class=""panel-toggle collapsed"" role=""button"" data-toggle=""collapse"" href=""#collapse{0}"" aria-expanded=""false"" aria-controls=""collapse{0}"">
	'						<i class=""{2}"" aria-hidden=""true""></i><span class=""panel-label"">{1}</span>
	'					</a> 
	'					<a class=""panel-close"" role=""button"" data-toggle=""collapse"" tabindex=""0"" href=""#panel{0}"">
	'						<span class=""esri-icon esri-icon-close"" aria-hidden=""true""></span>
	'					</a> 
	'				</div>
	'			</div>
	'			<div id=""collapse{{ID}}"" class=""panel-collapse collapse"" role=""tabpanel"" aria-labelledby=""heading{0}"">
	'				<div class=""panel-body"">
	'					<div id=""panelContent{0}"">
	'					</div>
	'				</div>
	'			</div>
	'		</div>"

	'	'Me.panelButtonContainer.Controls.Add(New LiteralControl(String.Format(tmplPanelButton, PanelId, PanelTitle, PanelIcon)))
	'	'Me.panelContainer.Controls.Add(New LiteralControl(String.Format(tmplPanel, PanelId, PanelTitle, PanelIcon)))
	'End Sub

	<WebMethod>
	Public Shared Function GetUserProfile(viewerType As String) As String

		Dim userName As String = HttpContext.Current.User.Identity.Name
		Dim mu As MembershipUser = Membership.GetUser(userName)
		Dim up As New MemberProfile(mu)
		Dim userProfile = New With {
			.Name = userName,
			.Type = up.TIPO_UTENTE
		}


		Return Newtonsoft.Json.JsonConvert.SerializeObject(userProfile, Newtonsoft.Json.Formatting.Indented)
	End Function

	<WebMethod>
	Public Shared Function Log_Spostamenti(PRJDATABASE As String, PRJGUID As String, PRJITEMGUID As String,
	RifTabella As String, RifOID As String, RifGUID As String, RifALIAS As String,
	AMBIENTE_GUID_OLD As String, AMBIENTE_GUID_NEW As String)


		Dim insert As String = "INSERT INTO [dbo].[TD_LOG_SPOSTAMENTI]
           ([ModUser]
           ,[ModDateTime]
           ,[REFTABLE]
           ,[REFOBJECTID]
           ,[REFGLOBALID]
           ,[REFALIAS]
           ,[PRJGUID]
           ,[PRJITEMGUID]
           ,[AMBIENTE_GUID_OLD]
           ,[AMBIENTE_GUID_NEW])
     VALUES
           (@ModUser
           ,@ModDateTime
           ,@REFTABLE
           ,@REFOBJECTID
           ,@REFGLOBALID
           ,@REFALIAS
           ,@PRJGUID
           ,@PRJITEMGUID
           ,@AMBIENTE_GUID_OLD
           ,@AMBIENTE_GUID_NEW)"

		Dim cmdLog As New SqlClient.SqlCommand With {.CommandType = CommandType.Text, .CommandText = insert}
		With cmdLog.Parameters
			.AddWithValue("@ModUser", HttpContext.Current.User.Identity.Name)
			.AddWithValue("@ModDateTime", Now)

			.AddWithValue("@REFTABLE", RifTabella)
			.AddWithValue("@REFOBJECTID", RifOID)
			.AddWithValue("@REFGLOBALID", RifGUID)
			.AddWithValue("@REFALIAS", RifALIAS)

			.AddWithValue("@PRJGUID", PRJGUID)
			.AddWithValue("@PRJITEMGUID", PRJITEMGUID)


			.AddWithValue("@AMBIENTE_GUID_OLD", AMBIENTE_GUID_OLD)
			.AddWithValue("@AMBIENTE_GUID_NEW", New Guid(AMBIENTE_GUID_NEW))

		End With








		Dim ssh As New SSHelper(PRJDATABASE)
		Dim dbResult As DatabaseOperationResult = ssh.ExecuteStored(cmdLog)
		Return dbResult.ErrorMessage
	End Function

	<WebMethod()>
	Public Shared Function CreaReportVincoli(CodiceComune As String, Livello As String, Foglio As String, Numero As String,
									  Servizio As String,
									  PaginaTipo As String,
									  PaginaOrientamento As String,
									  PaginaLarghezza As Double,
									  PaginaAltezza As Double,
									  Template As String,
									  ImmaginiEsportate As List(Of RisultatoEsportazione)) As String

		Dim AGS_OutputFolder As String = String.Format(ConfigurationManager.AppSettings("AGS_OutputFolder"), Servizio)
		Dim layoutsFolderPath As String = String.Format(ConfigurationManager.AppSettings("LayoutsBasePath"), CodiceComune)
		Dim downloadFolderPath As String = ConfigurationManager.AppSettings("DownloadFilesTempBasePath")
		Dim tmplFileName As String = System.IO.Path.Combine(layoutsFolderPath, String.Concat(Template, ".pdf"))

		If Not System.IO.File.Exists(tmplFileName) Then
			Throw New Exception(String.Format("Impossibile trovare il file {0}.pdf", Template))

		End If

		If Not System.IO.Directory.Exists(downloadFolderPath) Then
			Try
				System.IO.Directory.CreateDirectory(downloadFolderPath)
			Catch ex As Exception
				Debug.WriteLine(ex.Message)
			End Try

		End If

		Dim fileName As String = String.Format("{0}_{1:yyyyMMddHHmmss}.pdf", "Esportazione", Now)
		Dim filePath As String = System.IO.Path.Combine(AGS_OutputFolder, fileName)
		Dim destPath As String = System.IO.Path.Combine(downloadFolderPath, fileName)

		Dim bf As Font = New iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 8, iTextSharp.text.Font.NORMAL)
		Dim bf_b As Font = New iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 8, iTextSharp.text.Font.BOLD)

		Using fs As System.IO.FileStream = New System.IO.FileStream(filePath, System.IO.FileMode.Create)
			Dim pdfDoc As Document = Nothing
			Dim writer As PdfWriter = Nothing
			pdfDoc = New Document()

			writer = PdfWriter.GetInstance(pdfDoc, fs)
			writer.SetFullCompression()
			writer.ViewerPreferences = PdfWriter.PageModeUseOutlines
			pdfDoc.Open()

			Dim pagina_stralcio = New iTextSharp.text.Rectangle(PageSize.A4.Width, PageSize.A4.Height)
			If PaginaTipo.ToUpper.StartsWith("A4") And PaginaOrientamento.ToUpper = "O" Then
				pagina_stralcio = New iTextSharp.text.Rectangle(PageSize.A4.Height, PageSize.A4.Width)
			End If
			If PaginaTipo.ToUpper.StartsWith("A3") And PaginaOrientamento.ToUpper = "V" Then
				pagina_stralcio = New iTextSharp.text.Rectangle(PageSize.A3.Width, PageSize.A3.Height)
			End If
			If PaginaTipo.ToUpper.StartsWith("A3") And PaginaOrientamento.ToUpper = "O" Then
				pagina_stralcio = New iTextSharp.text.Rectangle(PageSize.A3.Height, PageSize.A3.Width)
			End If
			pdfDoc.SetPageSize(pagina_stralcio)

			Dim mapMarkerPath As String = HttpContext.Current.Server.MapPath("~/images/Map-Marker.png")
			Dim mapMarker As iTextSharp.text.Image = iTextSharp.text.Image.GetInstance(mapMarkerPath)
			mapMarker.ScaleToFit(24, 24)
			For Each img As RisultatoEsportazione In ImmaginiEsportate


				Dim tmplReader As PdfReader = New PdfReader(tmplFileName)

				Dim exportedImagePath As String = System.IO.Path.Combine(AGS_OutputFolder, img.Immagine)
				If System.IO.File.Exists(exportedImagePath) Then
					Using ms1 As New System.IO.MemoryStream
						Using stamper As PdfStamper = New PdfStamper(tmplReader, ms1)
							Dim form As AcroFields = stamper.AcroFields
							Dim overContent As PdfContentByte = stamper.GetOverContent(1)

							form.SetField("LIVELLO", Livello)
							form.SetField("FOGLIO", Foglio)
							form.SetField("NUMERO", Numero)

							Dim mapContainer As List(Of iTextSharp.text.pdf.AcroFields.FieldPosition) = form.GetFieldPositions("MapContainer")
							If Not mapContainer Is Nothing AndAlso mapContainer.Count > 0 Then
								Dim mapContainerRect As iTextSharp.text.Rectangle = mapContainer(0).position


								Dim exportedMapImage As iTextSharp.text.Image = iTextSharp.text.Image.GetInstance(exportedImagePath)
								'exportedMapImage.ScaleToFit(mapContainerRect.Width, mapContainerRect.Height)
								'exportedMapImage.SetAbsolutePosition(mapContainerRect.Right - exportedMapImage.ScaledWidth + (mapContainerRect.Width - exportedMapImage.ScaledWidth) / 2, mapContainerRect.Bottom + (mapContainerRect.Height - exportedMapImage.ScaledHeight) / 2)
								exportedMapImage.ScaleToFit(mapContainerRect.Width - 2, mapContainerRect.Height - 2)
								exportedMapImage.SetAbsolutePosition(mapContainerRect.Left + 1, mapContainerRect.Bottom + 1)
								overContent.AddImage(exportedMapImage)


								'Map Marker'


								Dim absX As Single = mapContainerRect.Left + (mapContainerRect.Width - mapMarker.ScaledWidth) / 2
								Dim absY As Single = mapContainerRect.Bottom + (mapContainerRect.Height - mapMarker.ScaledHeight) / 2 + (mapMarker.ScaledHeight / 2)

								mapMarker.SetAbsolutePosition(absX, absY)
								overContent.AddImage(mapMarker)

							End If

							'// LEGENDA
							Dim legendContainer As List(Of iTextSharp.text.pdf.AcroFields.FieldPosition) = form.GetFieldPositions("LegendContainer")
							If Not legendContainer Is Nothing AndAlso legendContainer.Count > 0 Then
								Dim legendContainerRect As iTextSharp.text.Rectangle = legendContainer(0).position
								Dim legend As New PdfPTable(2)
								With legend
									.TotalWidth = legendContainerRect.Width
									.SetWidths(New Integer() {1, 9})
									.DefaultCell.Border = PdfPCell.NO_BORDER

								End With
								legend.AddCell(New PdfPCell(New Phrase(img.Titolo, bf_b)) With {.Colspan = 2, .Padding = 4})

								For Each item In img.Legenda

									Dim bytes() As Byte = Convert.FromBase64String(item.imageData)
									Dim png As iTextSharp.text.Image = iTextSharp.text.Image.GetInstance(bytes)
									With png
										.BorderColor = iTextSharp.text.BaseColor.BLACK
										.ScaleAbsolute(24, 24)

									End With
									legend.AddCell(New PdfPCell(png, False) With {.Border = PdfPCell.NO_BORDER, .HorizontalAlignment = PdfPCell.ALIGN_CENTER})
									legend.AddCell(New Phrase(item.label, bf))
								Next
								Try
									legend.WriteSelectedRows(0, -1, legendContainerRect.Left, legendContainerRect.Top, overContent)
								Catch ex As Exception
									Debug.WriteLine(ex.Message)
								End Try
							End If

							stamper.FormFlattening = True
						End Using

						tmplReader = New PdfReader(ms1.ToArray)


						Dim rect As iTextSharp.text.Rectangle = tmplReader.GetPageSizeWithRotation(1)
						Dim importedPage As PdfImportedPage = writer.GetImportedPage(tmplReader, 1)

						pdfDoc.NewPage()

						Dim pageOrientation As Integer = tmplReader.GetPageRotation(1)
						If pageOrientation = 90 Or pageOrientation = 270 Then
							writer.DirectContent.AddTemplate(importedPage, 0, -1, 1, 0, 0, rect.Height)
						Else
							writer.DirectContent.AddTemplate(importedPage, 0, 0)
						End If

					End Using
					Try '// ELIMINA IMMAGINE
						System.IO.File.Delete(exportedImagePath)
					Catch ex As Exception
						System.Diagnostics.Debug.WriteLine(ex.Message)
					End Try
				End If

			Next


			pdfDoc.Close()
		End Using

		Try
			System.IO.File.Move(filePath, destPath)
			Dim downloadPath As String = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(destPath))
			Dim downloadUrl As String = String.Concat(VirtualPathUtility.ToAbsolute("~/WebHandlers/SimpleDownload.ashx?Path="), downloadPath)
			Return downloadUrl
		Catch ex As Exception
			Throw New Exception("La cartella Download del Server non esiste")
		End Try


	End Function

	<WebMethod()>
	Public Shared Function EsportaStralcio(CodiceComune As String, Servizio As String,
										   PaginaTipo As String,
									  PaginaOrientamento As String,
									  PaginaLarghezza As Double,
									  PaginaAltezza As Double,
									  Template As String,
									  Immagine As String,
									  Livelli As LayerList) As String

		Dim AGS_OutputFolder As String = String.Format(ConfigurationManager.AppSettings("AGS_OutputFolder"), Servizio)
		Dim layoutsFolderPath As String = String.Format(ConfigurationManager.AppSettings("LayoutsBasePath"), CodiceComune)
		Dim downloadFolderPath As String = ConfigurationManager.AppSettings("DownloadFilesTempBasePath")
		Dim tmplFileName As String = System.IO.Path.Combine(layoutsFolderPath, String.Concat(Template, ".pdf"))

		If Not System.IO.File.Exists(tmplFileName) Then
			Throw New Exception(String.Format("Impossibile trovare il file {0}.pdf", Template))

		End If

		If Not System.IO.Directory.Exists(downloadFolderPath) Then
			Try
				System.IO.Directory.CreateDirectory(downloadFolderPath)
			Catch ex As Exception
				Debug.WriteLine(ex.Message)
			End Try

		End If

		Dim fileName As String = String.Format("{0}_{1:yyyyMMddHHmmss}.pdf", "Esportazione", Now)
		Dim filePath As String = String.Concat(AGS_OutputFolder, "\", fileName)
		Dim destPath As String = System.IO.Path.Combine(downloadFolderPath, fileName)

		Dim bf As Font = New iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 8, iTextSharp.text.Font.NORMAL)

		Using fs As System.IO.FileStream = New System.IO.FileStream(filePath, System.IO.FileMode.Create)
			Dim pdfDoc As Document = Nothing
			Dim writer As PdfWriter = Nothing
			pdfDoc = New Document()

			writer = PdfWriter.GetInstance(pdfDoc, fs)
			writer.SetFullCompression()
			writer.ViewerPreferences = PdfWriter.PageModeUseOutlines
			pdfDoc.Open()

			Dim pagina_stralcio = New iTextSharp.text.Rectangle(PageSize.A4.Width, PageSize.A4.Height)
			If PaginaTipo.ToUpper.StartsWith("A4") And PaginaOrientamento.ToUpper = "O" Then
				pagina_stralcio = New iTextSharp.text.Rectangle(PageSize.A4.Height, PageSize.A4.Width)
			End If
			If PaginaTipo.ToUpper.StartsWith("A3") And PaginaOrientamento.ToUpper = "V" Then
				pagina_stralcio = New iTextSharp.text.Rectangle(PageSize.A3.Width, PageSize.A3.Height)
			End If
			If PaginaTipo.ToUpper.StartsWith("A3") And PaginaOrientamento.ToUpper = "O" Then
				pagina_stralcio = New iTextSharp.text.Rectangle(PageSize.A3.Height, PageSize.A3.Width)
			End If
			pdfDoc.SetPageSize(pagina_stralcio)


			Dim tmplReader As PdfReader = New PdfReader(tmplFileName)

			Dim exportedImagePath As String = String.Concat(AGS_OutputFolder, "\", Immagine)
			If System.IO.File.Exists(exportedImagePath) Then
				Using ms1 As New System.IO.MemoryStream
					Using stamper As PdfStamper = New PdfStamper(tmplReader, ms1)
						Dim form As AcroFields = stamper.AcroFields
						Dim overContent As PdfContentByte = stamper.GetOverContent(1)



						Dim mapContainer As List(Of iTextSharp.text.pdf.AcroFields.FieldPosition) = form.GetFieldPositions("MapContainer")
						If Not mapContainer Is Nothing AndAlso mapContainer.Count > 0 Then
							Dim mapContainerRect As iTextSharp.text.Rectangle = mapContainer(0).position


							Dim exportedMapImage As iTextSharp.text.Image = iTextSharp.text.Image.GetInstance(exportedImagePath)
							'exportedMapImage.ScaleToFit(mapContainerRect.Width, mapContainerRect.Height)
							'exportedMapImage.SetAbsolutePosition(mapContainerRect.Right - exportedMapImage.ScaledWidth + (mapContainerRect.Width - exportedMapImage.ScaledWidth) / 2, mapContainerRect.Bottom + (mapContainerRect.Height - exportedMapImage.ScaledHeight) / 2)
							exportedMapImage.ScaleToFit(mapContainerRect.Width - 2, mapContainerRect.Height - 2)
							exportedMapImage.SetAbsolutePosition(mapContainerRect.Left + 1, mapContainerRect.Bottom + 1)
							overContent.AddImage(exportedMapImage)




						End If

						'// LEGENDA
						Dim legendContainer As List(Of iTextSharp.text.pdf.AcroFields.FieldPosition) = form.GetFieldPositions("LegendContainer")
						If Not legendContainer Is Nothing AndAlso legendContainer.Count > 0 Then
							Dim legendContainerRect As iTextSharp.text.Rectangle = legendContainer(0).position
							Dim legend As New PdfPTable(2)
							With legend
								.TotalWidth = legendContainerRect.Width
								.SetWidths(New Integer() {1, 9})
								.DefaultCell.Border = PdfPCell.NO_BORDER

							End With

							'For Each livello In Livelli.layers
							'	legend.AddCell(New PdfPCell(New Phrase(livello.layerName, bf)) With {.Colspan = 2})
							'	For Each item In livello.legend

							'		Dim bytes() As Byte = Convert.FromBase64String(item.imageData)
							'		Dim png As iTextSharp.text.Image = iTextSharp.text.Image.GetInstance(bytes)
							'		With png
							'			.BorderColor = iTextSharp.text.BaseColor.BLACK
							'			.ScaleAbsolute(24, 24)

							'		End With
							'		legend.AddCell(New PdfPCell(png, False) With {.Border = PdfPCell.NO_BORDER, .HorizontalAlignment = PdfPCell.ALIGN_CENTER})
							'		legend.AddCell(New Phrase(item.label, bf))
							'	Next

							'Next
							'legend.AddCell(New PdfPCell(New Phrase("Legenda non disponibile", bf)) With {.Colspan = 2})

							For Each livello In Livelli.layers
								legend.AddCell(New PdfPCell(New Phrase(livello.layerName, bf)) With {.Colspan = 2, .Border = PdfPCell.NO_BORDER})
							Next

							Try
								legend.WriteSelectedRows(0, -1, legendContainerRect.Left, legendContainerRect.Top, overContent)
							Catch ex As Exception
								Debug.WriteLine(ex.Message)
							End Try
						End If

						stamper.FormFlattening = True
					End Using

					tmplReader = New PdfReader(ms1.ToArray)


					Dim rect As iTextSharp.text.Rectangle = tmplReader.GetPageSizeWithRotation(1)
					Dim importedPage As PdfImportedPage = writer.GetImportedPage(tmplReader, 1)

					pdfDoc.NewPage()

					Dim pageOrientation As Integer = tmplReader.GetPageRotation(1)
					If pageOrientation = 90 Or pageOrientation = 270 Then
						writer.DirectContent.AddTemplate(importedPage, 0, -1, 1, 0, 0, rect.Height)
					Else
						writer.DirectContent.AddTemplate(importedPage, 0, 0)
					End If

				End Using
				Try '// ELIMINA IMMAGINE
					System.IO.File.Delete(exportedImagePath)
				Catch ex As Exception
					System.Diagnostics.Debug.WriteLine(ex.Message)
				End Try
			End If


			pdfDoc.Close()
		End Using

		Try
			System.IO.File.Move(filePath, destPath)
			Dim downloadPath As String = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(destPath))
			Dim downloadUrl As String = String.Concat(VirtualPathUtility.ToAbsolute("~/WebHandlers/SimpleDownload.ashx?Path="), downloadPath)
			Return downloadUrl
		Catch ex As Exception
			Throw New Exception("La cartella Download del Server non esiste")
		End Try


	End Function


	<WebMethod()>
	Public Shared Function CombinePDFFile(Servizio As String,
										  CodiceComune As String,
										  Livello As String,
										  Foglio As String,
										  Numero As String,
										  Jobs As List(Of ExportLayoutJob)) As String

		Dim arcgisjobs = String.Format("C:\arcgisserver\directories\arcgisjobs\{0}_gpserver", Servizio.ToLower.Replace("/", "\"))

		Dim downloadFolderPath As String = ConfigurationManager.AppSettings("DownloadFilesTempBasePath")
		Dim fileName As String = String.Format("{0}_{1}_{2}_{3}.pdf", CodiceComune, Livello, Foglio, Numero)

		Dim destPath As String = System.IO.Path.Combine(downloadFolderPath, fileName)

		Dim readerList As New List(Of PdfReader)
		Using fs As System.IO.FileStream = New System.IO.FileStream(destPath, System.IO.FileMode.Create)
			Dim pdfDoc As Document = Nothing
			Dim writer As PdfWriter = Nothing
			pdfDoc = New Document()

			writer = PdfWriter.GetInstance(pdfDoc, fs)
			writer.SetFullCompression()
			writer.ViewerPreferences = PdfWriter.PageModeUseOutlines
			pdfDoc.Open()


			For Each j As ExportLayoutJob In Jobs

				Dim jobDir As String = System.IO.Path.Combine(arcgisjobs, j.jobId)
				Dim jobResult As String = System.IO.Path.GetFileName(j.message)
				Dim tmplFileName As String = System.IO.Path.Combine(jobDir, "scratch", jobResult)

				If System.IO.File.Exists(tmplFileName) Then
					Dim tmplReader As PdfReader = New PdfReader(tmplFileName)
					For p As Integer = 1 To tmplReader.NumberOfPages
						'// COMMENTATO PERCHÈ rect.Rotation SEMBRA NON FUNZIONARE

						'Dim importedPage As PdfImportedPage = writer.GetImportedPage(tmplReader, p)

						'pdfDoc.NewPage()

						'Dim rect As iTextSharp.text.Rectangle = tmplReader.GetPageSizeWithRotation(p)
						'If rect.Rotation = 90 Or rect.Rotation = 270 Then
						'	writer.DirectContent.AddTemplate(importedPage, 0, -1, 1, 0, 0, rect.Height)
						'Else
						'	writer.DirectContent.AddTemplate(importedPage, 0, 0)
						'End If

						Dim rect As iTextSharp.text.Rectangle = tmplReader.GetPageSizeWithRotation(p)
						pdfDoc.SetPageSize(rect)
						pdfDoc.NewPage()
						Dim importedPage As PdfImportedPage = writer.GetImportedPage(tmplReader, p)
						Dim pageOrientation As Integer = tmplReader.GetPageRotation(p)
						If pageOrientation = 90 Or pageOrientation = 270 Then
							writer.DirectContent.AddTemplate(importedPage, 0, -1, 1, 0, 0, rect.Height)
						Else
							writer.DirectContent.AddTemplate(importedPage, 0, 0)
						End If


					Next
					readerList.Add(tmplReader)


				End If




			Next


			pdfDoc.Close()
		End Using

		For Each r As PdfReader In readerList
			Try
				r.Close()
			Catch ex As Exception

			End Try

		Next

		For Each j As ExportLayoutJob In Jobs
			Dim jobDir As String = System.IO.Path.Combine(arcgisjobs, j.jobId)
			Try
				System.IO.Directory.Delete(jobDir, True)
			Catch ex As Exception
				Debug.WriteLine("Impossibile eliminare la directory {0}", jobDir)
			End Try
		Next

		Try

			Dim downloadPath As String = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(destPath))
			Dim downloadUrl As String = String.Concat(VirtualPathUtility.ToAbsolute("~/WebHandlers/SimpleDownload.ashx?Path="), downloadPath)
			Return downloadUrl
		Catch ex As Exception
			Throw New Exception("La cartella Download del Server non esiste")
		End Try


	End Function

	<WebMethod>
	Public Shared Function SUED_EstraiInfo(Comune As String, Foglio As String, Numero As String)
		Dim ssh As New SSHelper("OWKSUED")
		Dim selectFilter As String = String.Format(" SELECT * FROM [dbo].[PSYS_INTEGRAZIONE_SIT] WHERE foglio = '{0}' and particella = '{1}'", Foglio, Numero)
		Dim dbr As DatabaseOperationResult = ssh.GetData(selectFilter)

		Return dbr.SerializeTable(0)
	End Function

	<WebMethod>
	Public Shared Function SUED_EstraiParticelle(LIVELLO_PE As String)
		Dim selectFilter As String = ""

		Select Case LIVELLO_PE.ToUpper
			Case "ASSERVIMENTO"
				selectFilter = " SELECT foglio, particella, tipo_asservimento as valore FROM [dbo].[PSYS_INTEGRAZIONE_SIT] ORDER BY foglio, particella, tipo_asservimento"
			Case "STATO"
				selectFilter = " SELECT foglio, particella, statopratica as valore FROM [dbo].[PSYS_INTEGRAZIONE_SIT] ORDER BY foglio, particella, statopratica"
			Case "TIPOLOGIA"
				selectFilter = " SELECT foglio, particella, SIGLATIPODOC as valore FROM [dbo].[PSYS_INTEGRAZIONE_SIT] ORDER BY foglio, particella, SIGLATIPODOC"
		End Select
		Dim ssh As New SSHelper("OWKSUED")

		Dim dbr As DatabaseOperationResult = ssh.GetData(selectFilter)

		Return dbr.SerializeTable(0)
	End Function
End Class
