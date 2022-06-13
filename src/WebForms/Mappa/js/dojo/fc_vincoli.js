define(["esri/request",
	"esri/graphicsUtils",
	"esri/graphic",
	"esri/symbols/SimpleFillSymbol",
	"esri/tasks/PrintTask", "esri/tasks/PrintParameters", "esri/tasks/PrintTemplate", "esri/tasks/Geoprocessor",
	"dojo/_base/array", "dojo/on"],
	function (esriRequest, graphicsUtils, Graphic, SimpleFillSymbol, PrintTask, PrintParameters, PrintTemplate, Geoprocessor, arrayUtils, on) {

		"use strict";

		var _tocLayers = [];
		var _hideLayers = [];
		var _visibleLayers = [];
		var _vincoli_intercettati = [];
		var _errori_esportazione = 0;
		
		
		var _identifyTask = null;
		var _identifyParams = null;

		// Elenco Layouts per esportazione stralci in pdf
		var _layoutOptions = null;

		
		var $sidebar = null;
		var $tocContainer = null;
		var _highlightSymbol = new SimpleFillSymbol({
			"style": "none",
			"outline": {
				"width": 2,
				"style": "solid",
				"color": { "r": 0, "g": 255, "b": 255, "a": 1 }

			},
			"color": { "r": 0, "g": 0, "b": 0, "a": 0 }
		});

		//var $dropdown = $('<div class="btn-group" style="margin-bottom: 24px;">' +
		//	'<button type="button" class="btn btn-default dropdown-toggle" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false">' +
		//	'Stampa stralcio <span class="caret"></span>' +
		//	'</button>' +
		//	'<ul class="dropdown-menu"></ul>' +
		//	'</div>');

		//var $drowdownmenu = null;

		var _currentStatus;

		var exportStatus = {
			Start: 0,
			Process: 1,
			Error: 2,
			Success: 3
		};

		var waitDialog = null;

		// Utilizzo del Geoprocessing per l'esportazione
		var _layoutIndex = 0;
		var _exportError = 0;
		var useGeoProcessor = true;
		var exportMapGPServerURL = '';
		var printTask, gp;
		var layoutTemplate, templateNames, mapOnlyIndex, templates, printParams;
		//---

		return {
			inizializza: function (groupLayerId, layerInfos) {

				

				if (useGeoProcessor) {
					initGPServer();
				}
				else {
					_layoutOptions = JSON.parse(_layouts);
				}

				setIdentifyTask();
				createSidebar();
				createTOC(groupLayerId, layerInfos);
				
				$(document).on('click', 'button.btn-analizza-vincoli', function () {
					var oid = $(this).data('oid');
					var livello = $(this).data('livello');
					analizzaVincoli(livello, oid);
				});

				$(document).on('click', 'a.export-map', function () {
					esportaStralcio($(this).data('label'), $(this).data('layout'));
				});
			},
			analizza: function (livello, oid) {

				var layerId = livello == app.enumLayers.FABBRICATI.name ? app.enumLayers.FABBRICATI.id : app.enumLayers.PARTICELLE.id;

				app.queryIds(layerId, [oid], function (features) {
					app.map.graphics.add(new Graphic(features[0].geometry, _highlightSymbol));
					var def = app.map.setExtent(graphicsUtils.graphicsExtent(features).expand(2));
					def.then(function () {
						analizzaVincoli(livello, oid);
					});
				});

				//app.dijitSearch.search('1/108').then(function (response) {
				//	alert(response);
				//});
			}
		};

		// Utilizzo Geoprocessing
		function initGPServer() {
			exportMapGPServerURL = app.ags_services_url + '/' + _servizio + '/GPServer/Export%20Web%20Map';

			var printInfo = esriRequest({
				'url': exportMapGPServerURL,
				'content': { 'f': 'json' }
			});
			printInfo.then(handlePrintInfo, function (err) {
				console.log("Something broke: ", err);
			});

			function handlePrintInfo(resp) {


				//var printTask = new PrintTask(exportMapGPServerURL); // Per default
				printTask = new PrintTask(exportMapGPServerURL, { async: true });

				gp = new Geoprocessor(exportMapGPServerURL);

				

				layoutTemplate = arrayUtils.filter(resp.parameters, function (param, idx) {
					return param.name === "Layout_Template";
				});

				if (layoutTemplate.length === 0) {
					console.log("print service parameters name for templates must be \"Layout_Template\"");
					return;
				}
				templateNames = layoutTemplate[0].choiceList;

				// remove the MAP_ONLY template then add it to the end of the list of templates 
				mapOnlyIndex = arrayUtils.indexOf(templateNames, "MAP_ONLY");
				if (mapOnlyIndex > -1) {
					var mapOnly = templateNames.splice(mapOnlyIndex, mapOnlyIndex + 1)[0];
					templateNames.push(mapOnly);
				}

				// create a print template for each choice
				templates = arrayUtils.map(templateNames, function (ch) {
					var plate = new PrintTemplate();
					plate.layout = plate.label = ch;
					plate.format = "PDF";
					//plate.layoutOptions = {
					//	"authorText": "Made by:  Terrextra",
					//	"copyrightText": "Terrextra \u00A9",
					//	"legendLayers": [],
					//	"titleText": "GeoSafetyV2",
					//	"scalebarUnit": "Meters"
					//};

					plate.layoutOptions = {
					};

					//plate.preserveScale = false;
					return plate;
				});

				printParams = arrayUtils.map(templates, function (t) {
					var params = new PrintParameters();
					params.map = app.map;
					params.template = t;
					return params;
				});


				console.log('Geoprocessor ok');
				


				//$('#printBtn').click(function () {
				//	var params = $("#printOptionsLayoutTemplate option:selected").data('Params');
				//	var title = $('#printOptionsTitle').val() == '' ? prjITEMDESC : $('#printOptionsTitle').val();
				//	var dpi = $('#printOptionsDpi').val();
				//	exportLayout(params, title, dpi);
				//});

			}

			
		}
		
		function exportWebMap(gpParams, progressCallback) {
			

			gp.submitJob(gpParams, function (jobinfo) {
				//gpJobComplete
				gp.getResultData(jobinfo.jobId, "Output_File", function (result) {

					progressCallback({
						jobId: jobinfo.jobId,
						status: 1,
						message: result.value.url,
						percentage: 100
					});


				}, function (error) {
						
						progressCallback({
							jobId: jobinfo.jobId,
							status: -1,
							message: error,
							percentage: 100
						});
				});

			}, function (jobinfo) {
				//gpJobStatus
				switch (jobinfo.jobStatus) {
					case 'esriJobSubmitted':
						
						progressCallback({
							jobId: jobinfo.jobId,
							status: 0,
							message: 'Avvio processo di esportazione...',
							percentage: 25
						});
						break;
					case 'esriJobExecuting':
						
						progressCallback({
							jobId: jobinfo.jobId,
							status: 0,
							message: 'Esportazione...',
							percentage: 50
						});
						break;
					case 'esriJobSucceeded':
						
						progressCallback({
							jobId: jobinfo.jobId,
							status: 0,
							message: 'Esportazione completata con successo',
							percentage: 100
						});
						break;
					case 'esriJobFailed':
						
						progressCallback({
							jobId: jobinfo.jobId,
							status: 0,
							message: 'Esportazione fallita',
							percentage: 100
						});
						break;
				}
			}, function (error) {
				//gpJobFailed
				
					progressCallback({
						jobId: -1,
						status: -1,
						message: error,
						percentage: 100
					});
			});


			
		}
		// ------------------------

		function setIdentifyTask() {
			// Inizio preparazione variabili per estrazione vincoli
			_identifyTask = new esri.tasks.IdentifyTask(app.operationalMap.url);
			_identifyParams = new esri.tasks.IdentifyParameters();
			_identifyParams.tolerance = 0;
			_identifyParams.returnGeometry = true;
			_identifyParams.layerOption = esri.tasks.IdentifyParameters.LAYER_OPTION_ALL;
			_identifyParams.width = app.map.width;
			_identifyParams.height = app.map.height;
			_identifyParams.layerIds = [];
		}

		function createSidebar() {
			$tocContainer = $('<div>').sidebar();
			app.mapContainer.append($tocContainer);
			$sidebar = $tocContainer.data('bs.sidebar');

			// Aggiunge un bottone nella toolbar secondaria per aprire il pannello con l'elenco dei vincoli
			var $btnOpenSidebar = $($('#tmplPanelButton').render({ Id: 'VINCOLI', Title: 'VINCOLI', Icon: 'glyphicon glyphicon-leaf', Label: 'VINCOLI' }));
			$btnOpenSidebar.removeClass('toggle-panel').removeAttr('data-target').click(function () { $sidebar.show(); });
			$(app.mainToolbar).append($btnOpenSidebar);


		}

		function createTOC(groupLayerId, layerInfos) {
			var $tmplLayer = $.templates(
				'<tr>' +
				'<td style="vertical-align:top; padding-left:5px;"><input type="checkbox" class="check_vincolo" data-id="{{:layer.id}}" /></td>' +
				'<td style="vertical-align:top; padding-left:5px;">{{:layer.name}}</td>' +
				'</tr>');
			for (var i = 0; i < layerInfos.length; i++) {
				var info = layerInfos[i];


				if (info.parentLayerId == groupLayerId) {
					_identifyParams.layerIds.push(info.id);
					_tocLayers.push({ layer: info, title: info.name }); // Per la TOC solo i livelli nel GroupLayer Tematismi
				}

			}
			_visibleLayers = app.visibleLayers.slice();// conservo valore dei livelli visibile allo startup

			_tocLayers.sort(function (a, b) {
				var n1 = a.layer.name.toUpperCase(), n2 = b.layer.name.toUpperCase();
				return n1 > n2 ? 1 : n2 > n1 ? -1 : 0; // Attenzione MAIUSCOLE/minuscole
			});
			var $table = $('<table>');
			var $tbody = $('<tbody>');
			$table.append($tbody);
			$.each(_tocLayers, function () {
				//$tbody.append(
				//	$('<tr>')
				//		.html('<td><input type="checkbox" class="check_vincolo" data-id="' + this.layer.id + '" id="check_vincolo_' + this.layer.id + '" /></td><td><span style="padding-left:5px;">' + this.layer.name + '</span></td>')

				//);
				$tbody.append($tmplLayer.render(this));
			});
			
			var $lst = $('<div style="position:fixed; bottom: 0;overflow:auto; top: 110px; margin-bottom: 50px; width:275px; background-color: #fff; border: 1px solid #eee;">');
			$table.appendTo($lst);

			// Bottone aggiornamento mappa
			var $btnUpdateMap = $('<button type="button" class="btn btn-success btn-sm" style="margin-right:5px;">Aggiorna Mappa</button>');
			$btnUpdateMap.click(function () {
				event.preventDefault();
				var visibleLayersId = _visibleLayers.slice(); //_visibleLayers.slice();

				app.visibleLayersVincoli = [];
				$lst.find('.check_vincolo:checked').each(function () {

					visibleLayersId.push($(this).data('id'));
					app.visibleLayersVincoli.push($(this).data('id'));
				});
				$.each(app.visibleLayersBase, function (i, val) {
					visibleLayersId.push(val);
				});
				//visible.sort();
				app.operationalMap.setVisibleLayers(visibleLayersId);
				app.legend.refresh();
			});

			var $btnUncheckAll = $('<button type="button" class="btn btn-danger btn-sm" style="margin-right:5px;">Spegni tutto</button>');
			$btnUncheckAll.click(function () {
				event.preventDefault();
				

				$lst.find('.check_vincolo').each(function () {
					$(this).prop('checked', false);
				});
				var visibleLayersId = app.operationalMap.visibleLayers.slice(); //_visibleLayers.slice();
				app.operationalMap.setVisibleLayers(visibleLayersId);
				app.legend.refresh();
			});
			
			var $container = $('<div>');
			$container.append($btnUpdateMap).append($btnUncheckAll).append($lst);
			
			$sidebar.setContent('<p>VINCOLI<br/><small>', $container);

			$tocContainer.on('show.bs.sidebar', function () {
			//	updateTOC();
			});

			function updateTOC() {
				$.each(app.operationalMap.visibleLayers, function () {
					
				});
			}
		}


		//function initTOC($ul) {
		//	$ul.find('.list-group-item').each(function () {

		//		// Settings
		//		var $widget = $(this),
		//			$checkbox = $('<input type="checkbox" class="hidden" />'),
		//			color = ($widget.data('color') ? $widget.data('color') : "primary"),
		//			style = ($widget.data('style') == "button" ? "btn-" : "list-group-item-"),
		//			settings = {
		//				on: {
		//					icon: 'glyphicon glyphicon-check'
		//				},
		//				off: {
		//					icon: 'glyphicon glyphicon-unchecked'
		//				}
		//			};

		//		$widget.css('cursor', 'pointer');
		//		$widget.append($checkbox);

		//		// Event Handlers
		//		$widget.on('click', function () {
		//			$checkbox.prop('checked', !$checkbox.is(':checked'));
		//			$checkbox.triggerHandler('change');
		//			updateDisplay();
		//		});
		//		$checkbox.on('change', function () {
		//			updateDisplay();
		//		});


		//		// Actions
		//		function updateDisplay() {
		//			var isChecked = $checkbox.is(':checked');

		//			// Set the button's state
		//			$widget.data('state', (isChecked) ? "on" : "off");

		//			// Set the button's icon
		//			$widget.find('.state-icon')
		//				.removeClass()
		//				.addClass('state-icon ' + settings[$widget.data('state')].icon);

		//			// Update the button's color
		//			if (isChecked) {
		//				$widget.addClass(style + color + ' active');
		//			} else {
		//				$widget.removeClass(style + color + ' active');
		//			}
		//		}

		//		// Initialization
		//		function init() {

		//			if ($widget.data('checked') == true) {
		//				$checkbox.prop('checked', !$checkbox.is(':checked'));
		//			}

		//			updateDisplay();

		//			// Inject the icon if applicable
		//			if ($widget.find('.state-icon').length == 0) {
		//				$widget.prepend('<span class="state-icon ' + settings[$widget.data('state')].icon + '"></span>');
		//			}
		//		}
		//		init();
		//	});

			
		//}

		function analizzaVincoli(livello, oid) {
			// 
			var query = new esri.tasks.Query();
			query.objectIds = [oid];
			query.outFields = ["FOGLIO, NUMERO, RICERCA"];
			query.returnGeometry = true;

			var layerId = livello == app.enumLayers.FABBRICATI.name ? app.enumLayers.FABBRICATI.id : app.enumLayers.PARTICELLE.id;
			var queryTask = new esri.tasks.QueryTask(app.operationalMap.url + '/' + layerId);

			queryTask.execute(query, function (result) {
				if (result.features.length > 0) {
					doIdentify(livello, result.features[0]);
				}
			});
		}

		function doIdentify(livello, feature) {
			_vincoli_intercettati = [];
			

			BootstrapDialog.show({
				title: 'ANALISI IN CORSO...',
				message: $('<div></div>').load(app_url + 'WebForms/Mappa/html/Vincoli.html?' + Math.random()),
				closable: true,
				closeByBackdrop: false,
				closeByKeyboard: false,
				onshown: function (dialog) {
					

					var $wait = dialog.getModalContent().find('div.spinner');
					var $tbody = dialog.getModalContent().find('tbody');
					var $exportOptions = dialog.getModalContent().find('div.export-options');

					var $btnExport = dialog.getModalContent().find('button.button-export');
					var $btnClose = dialog.getModalContent().find('button.button-close');
					var $ddlExtent = dialog.getModalContent().find('select.select-extent');
					var $ddlLayoutList = dialog.getModalContent().find('select.select-layout');
					var $txtScale = dialog.getModalContent().find('input.current-scale');


					$txtScale.val(roundMapScale()).blur(function () {
						if (parseFloat(this.value) > 0) {
							if (this.value != roundMapScale()) {
								app.zoomToScale(this.value);
							}
							
						}
						else {
							this.value = roundMapScale();
							
						}
					});

					function roundMapScale() {
						var scale = app.map.getScale();
						return parseInt(scale);
					}

					if (useGeoProcessor) {
						arrayUtils.forEach(printParams, function (params, index) {

							$ddlLayoutList.append(
								$('<option>').data('Params', params).val(params.template.label).text(params.template.label)
							);


						});
					}
					else {
						$.each(_layoutOptions, function (i, o) {
							$ddlLayoutList.append($('<option>').val(o.value).text(o.label));
						});
					}
					

					// -- FINE

					$btnExport.click(function () {
						dialog.close();
						_errori_esportazione = 0;

						// Utilizzo Geoprocessing --------------------------------------------
						

						if (useGeoProcessor) {
							if (_vincoli_intercettati.length > 0) {
								var params = $ddlLayoutList.find("option:selected").data('Params');
								var title = 'LIVELLO: ' + livello + ' FOGLIO: ' + feature.attributes.FOGLIO + ' NUMERO: ' + feature.attributes.NUMERO;
								var dpi = 96;// $('#printOptionsDpi').val();

								updateExportStatus(exportStatus.Start, 'Preparazione mappa...');
								_layoutIndex = 0;
								_exportError = 0;
								exportLayout($ddlExtent.val(), $txtScale.val(), livello, feature.attributes.FOGLIO, feature.attributes.NUMERO, params, title, dpi);



							}
						}
						else {
							var desiredExtent = $ddlExtent.val();
							var DPI = 150;
							var values = $ddlLayoutList.val().split('|');
							var fileName = $('option:selected', $ddlLayoutList).text();
							var pageSize = app.getPageSize(values, fileName);

							if (_vincoli_intercettati.length > 0) {

								updateExportStatus(exportStatus.Start, 'Preparazione mappa...');

								// Riporta la mappa alla visualizzazione iniziale, spegnendo eventuali livelli accesi manualmente
								var evt = on(app.operationalMap, 'update-end', function (err) {
									evt.remove();
									esportaVincoli(0, desiredExtent, DPI, pageSize, livello, feature.attributes.FOGLIO, feature.attributes.NUMERO);
								});
								app.operationalMap.setVisibleLayers(_visibleLayers);
							}
						}
						
						

						// -- FINE
							
						
				

					});

					$btnClose.click(function () {
						dialog.close();
					});

					_identifyParams.geometry = feature.geometry;
					_identifyParams.mapExtent = app.map.extent;
					_identifyTask.execute(_identifyParams, function (idResults) {
						var distinct = [];

						

						for (var i = 0, il = idResults.length; i < il; i++) {

							if ($.inArray(idResults[i].layerName, distinct) === -1) {
								_vincoli_intercettati.push(
									{
										layerId: idResults[i].layerId,
										layerName: idResults[i].layerName,
										legend: app.getLayerLegend(idResults[i].layerId),
										exportResult: null
									});
								distinct.push(idResults[i].layerName);

								$tbody.append($('<tr>').append($('<td>').html(idResults[i].layerName)));
							}
						}


						
						dialog.setTitle('<p><strong>' + livello + ': ' + feature.attributes.RICERCA + ' ELENCO VINCOLI: ' + _vincoli_intercettati.length + '</strong> RISULTATI</p>');
						$exportOptions.show();
						$wait.hide();
					});

					
					
					
				}
			});

			
		}


		// Utilizzo Geoprocessing --------------------------------------------
		function exportLayout(extentType, currentScale, livello, foglio, numero, params, title, dpi) {

			
			if (_layoutIndex < _vincoli_intercettati.length) {

				updateExportStatus(exportStatus.Process, _vincoli_intercettati[_layoutIndex].layerName + '...');

				var visibleLayers = _visibleLayers.slice(); // elenco dei layer visibili allo startup
				var currentLayerId = _vincoli_intercettati[_layoutIndex].layerId;//id livello del vincolo da accendere
				
				visibleLayers.push(parseInt(currentLayerId)); // aggiunge id livello del vincolo da accendere

				var jsonRequest = getPrintTaskRequest(params, title, dpi);//ottiene il template della richiesta
				
				jsonRequest.operationalLayers[0].visibleLayers = visibleLayers.sort().slice(); // Layers visibili
				jsonRequest.layoutOptions.legendOptions.operationalLayers.push({
					"id": jsonRequest.operationalLayers[0].id,
					"sublayerIds": [currentLayerId]
				});

				if (extentType == 'CURRENT_SCALE') {
					jsonRequest.mapOptions.scale = parseFloat(currentScale);
					//var extent = app.getExtentForScale(currentScale);
					//jsonRequest.mapOptions.extent = app.getExtentForScale(currentScale);
				}
				
				var gpParams = {
					Web_Map_as_JSON: JSON.stringify(jsonRequest),
					Format: 'PDF',
					Layout_Template: params.template.layout
				};

				exportWebMap(gpParams, function (result) {
					switch (result.status) {
						case 0:
							break;
						case 1:

							_vincoli_intercettati[_layoutIndex].exportResult = result;
							_vincoli_intercettati[_layoutIndex].errorMessage = '';
					

							break;
						case -1:
							_exportError += 1;
							_vincoli_intercettati[_layoutIndex].errorMessage = _vincoli_intercettati[_layoutIndex].layerName + ': ' + result.message;

							break;
					}

					if (result.status != 0) {
						_layoutIndex += 1;
						exportLayout(extentType, currentScale, livello, foglio, numero, params, title, dpi);
					}
				});
			}
			else {

				if (_exportError == 0) {

					updateExportStatus(exportStatus.Process, 'Generazione report...');


					var vincoli = [];
					$.each(_vincoli_intercettati, function (i, v) {
						if (v.errorMessage == '') {
							vincoli.push(v.exportResult);
						}
						
					});

					var postData = {
						Servizio: _servizio,
						CodiceComune: _codice,
						Livello: livello,
						Foglio: foglio,
						Numero: numero,
						Jobs: vincoli.slice()
					};
					callWebMethod(page_Url + '/CombinePDFFile', postData, function (data) {

						//updateExportStatus(exportStatus.Success, 'Processo completato');

						updateExportStatus(exportStatus.Success, data.d);
						setTimeout(function () { location.href = data.d; }, 250);

					}, function (thrownError) {

						updateExportStatus(exportStatus.Error, thrownError);

					}, function () {

					});

				}
				else {
					updateExportStatus(exportStatus.Error, 'Impossibile completare');

				}

			}

		}


		/*  www.spatialtimes.com/2015/04/arcgis-javascript-api-printing-group-layers/
			*  "You’re not hallucinating – sometimes the map you see in your ArcGIS JavaScript application doesn’t 
			*  look like the one you printed via the Print Service. 
			*  There are many reasons why this might happen, and one is related to Group Layers."
			*/
		function getPrintTaskRequest(params, title, dpi) {

		
			var jsonRequest = printTask._getPrintDefinition(app.map, params);


			jsonRequest.layoutOptions = {
				"titleText": title,
				"authorText": "GeoSafety",
				"copyrightText": "\u00A9 " + new Date().getFullYear() + " GeoSafety s.r.l.",
				"scaleBarOptions":
				{
					"metricUnit": "meters",
					"metricLabel": "m"
				},
				"legendOptions": {
					"operationalLayers": []
				}
			};
			jsonRequest.exportOptions = {
				"dpi": dpi,
				"outputSize": [500, 500]// valido solo per MAP_ONLY
			};
			
			
			return jsonRequest;
		}

		function findLayerInfo(mapService, subLayerId) {
			var infos = mapService.layerInfos;
			for (var i = 0, il = infos.length; i < il; i++) {
				if (infos[i].id == subLayerId) {
					return infos[i];
				}
			}
		}


		function esportaVincoli(index, desiredExtent, DPI, pageSize, livello, foglio, numero) {
			var _errore_esportazione = '';
			if (index < _vincoli_intercettati.length) {

				updateExportStatus(exportStatus.Process, _vincoli_intercettati[index].layerName + '...');


				// Rendo visibile il vincolo corrente
				var visibleLayersId = _visibleLayers.slice();
				visibleLayersId.push(_vincoli_intercettati[index].layerId);

				var evt = on(app.operationalMap, 'update-end', function (err) {
					evt.remove();

					app.exportLayoutClient(desiredExtent, pageSize, DPI,
						function (p_ExportResult) {
							if (p_ExportResult.ERROR_MESSAGE == '') {

								_vincoli_intercettati[index].exportResult = p_ExportResult;
								esportaVincoli(index + 1, desiredExtent, DPI, pageSize, livello, foglio, numero);
							}
							else {
								_errore_esportazione = p_ExportResult.ERROR_MESSAGE;
								esportaVincoli(_vincoli_intercettati.length, desiredExtent, DPI, pageSize, livello, foglio, numero);
							}
						});
				});
				app.operationalMap.setVisibleLayers(visibleLayersId);
			}
			else {

				if (_errore_esportazione == '') {

					updateExportStatus(exportStatus.Process, 'Generazione report...');


					var vincoli = [];
					$.each(_vincoli_intercettati, function (i, v) {
						vincoli.push({ Titolo: v.layerName, Immagine: v.exportResult.EXPORTED_IMAGE, Legenda: v.legend });
					});

					var postData = {
						CodiceComune: _codice,
						Livello: livello,
						Foglio: foglio,
						Numero: numero,
						Servizio: _servizio,
						PaginaTipo: pageSize.Id,
						PaginaOrientamento: pageSize.Orientation,
						PaginaLarghezza: pageSize.Xmm,
						PaginaAltezza: pageSize.Ymm,
						Template: pageSize.FileName,
						ImmaginiEsportate: vincoli.slice()
					};
					callWebMethod(page_Url + '/CreaReportVincoli', postData, function (data) {

						//updateExportStatus(exportStatus.Success, 'Processo completato');
						
						updateExportStatus(exportStatus.Success, data.d);
						setTimeout(function () { location.href = data.d; }, 250);

					}, function (thrownError) {

						updateExportStatus(exportStatus.Error, thrownError);

					}, function () {

					});

				}
				else {
					updateExportStatus(exportStatus.Error, _errore_esportazione);

				}
				// Riporta la mappa alla situazione iniziale
				basemap.setVisibleLayers(_visibleLayers);
			}

		}
		// ---
		
		function updateExportStatus(status, msg) {
			if (_currentStatus != status) {
				switch (status) {
					case exportStatus.Start:

						waitDialog = BootstrapDialog.show({
							title: 'Attendere il completamento...',
							message: $('<div></div>').load(app_url + 'WebForms/Mappa/html/MultipleExportDialog.html'),
							closable: false,
							closeByBackdrop: false,
							closeByKeyboard: false,
							buttons: [
								{
									id: 'closeWaitDialog',
									label: 'Chiudi',
									action: function (dialogRef) {
										dialogRef.close();
									}
								}
							],
							onshown: function (dialog) {
								$('#MultipleExportStatusMessage').removeClass().addClass('text-info').empty();
								$('#MultipleExportLoader').show();

								dialog.getButton('closeWaitDialog').disable();
							}
						});

						


						
						
						break;
					case exportStatus.Process:

						break;
					case exportStatus.Error:
						
						$('#MultipleExportStatusMessage').removeClass().addClass('text-danger').empty();
						$('#MultipleExportLoader').hide();
						waitDialog.setTitle('Operazione completata');
						waitDialog.getButton('closeWaitDialog').enable();
						break;
					case exportStatus.Success:
						$('#MultipleExportStatusMessage').removeClass().addClass('text-success').empty();
						$('#MultipleExportLoader').hide();
						waitDialog.setTitle('Operazione completata');
						waitDialog.getButton('closeWaitDialog').enable();
						break;
				}
			}

			if (status == exportStatus.Success) {
				$('#MultipleExportStatusMessage').html('<a href="' + msg + '" target="_blank" style="font-size:10pt;"><span class="glyphicon glyphicon-download-alt"></span>Scarica il file PDF</a>');

			}
			else {
				$('#MultipleExportStatusMessage').html(msg);
			}
		}




		//--
		function esportaStralcio(label, layout) {
			updateExportStatus(exportStatus.Start, 'Preparazione mappa...');

			var values = layout.split('|');
			var fileName = label;
			var pageSize = app.getPageSize(values, fileName);

			app.exportLayoutClient(app.exportExtent.CURRENT_SCALE, pageSize, 150,
				function (p_ExportResult) {
					updateExportStatus(exportStatus.Process, 'Generazione report...');

					// Loop visible layers
					var l = { layers: [] };

					for (var i = 0; i < app.operationalMap.visibleLayers.length; i++) {
						var id = app.operationalMap.visibleLayers[i];
						$.each(app.operationalMapLegend.layers, function (i, esriLayer) {
							if (esriLayer.layerId == id) {
								l.layers.push(esriLayer);
								return false;
							}
						});
					}
					var postData = {
						CodiceComune: _codice,
						Servizio: _servizio,
						PaginaTipo: pageSize.Id,
						PaginaOrientamento: pageSize.Orientation,
						PaginaLarghezza: pageSize.Xmm,
						PaginaAltezza: pageSize.Ymm,
						Template: pageSize.FileName,
						Immagine: p_ExportResult.EXPORTED_IMAGE,
						Livelli: l
					};
					callWebMethod(page_Url + '/EsportaStralcio', postData, function (data) {

						updateExportStatus(exportStatus.Success, data.d);
						setTimeout(function () { location.href = data.d; }, 250);


					}, function (thrownError) {

						updateExportStatus(exportStatus.Error, thrownError);

					}, function () {

					});
				});
		}
		//--

		function truncateString(str, num) {
			if (str.length <= num) {
				return str
			}
			return str.slice(0, num) + '...'
		}
	});