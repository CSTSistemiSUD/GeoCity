define([
	"pkg/calcitemaps.min",
	"pkg/identify.min",
	"pkg/legend.min",
	"pkg/print.min",
	"pkg/export.min",
	"pkg/fc_vincoli.min",
	"pkg/fc_catasto.min",
	"pkg/fc_base.min",
	"pkg/sued.min",
	"esri/request",
	"esri/graphicsUtils",
	"esri/urlUtils",
	"esri/map",
	"esri/toolbars/edit",
	"esri/geometry/Extent",
	"esri/geometry/scaleUtils",
	"esri/tasks/locator",
	"esri/tasks/QueryTask",
	"esri/tasks/query",
	"esri/layers/ArcGISDynamicMapServiceLayer",
	"esri/layers/FeatureLayer",
	"esri/layers/ImageParameters",
	"esri/InfoTemplate",
	"esri/dijit/Search", "esri/dijit/Legend",
	"esri/dijit/BasemapGallery", "esri/dijit/Basemap", "esri/dijit/BasemapLayer",
	"dojo/query",
	"dojo/dom", "dojo/on", "dojo/dom-construct",
	"dojo/i18n!esri/nls/jsapi",
	"dojo/domReady!"
], function (
	CalciteMaps,
	Identify,
	Legend,
	Print,
	Export,
	fc_vincoli,
	fc_catasto,
	fc_base,
	sued,
	esriRequest,
	graphicsUtils,
	urlUtils,
	Map,
	Edit,
	Extent,
	scaleUtils,
	Locator,
	QueryTask,
	Query,
	ArcGISDynamicMapServiceLayer,
	FeatureLayer,
	ImageParameters,
	InfoTemplate,
	Search,
		Legend,
		BasemapGallery, Basemap, BasemapLayer,
	query,
	dom, on, domConstruct,
	esriBundle) {

	// **** Inizializzazione
	// ---- Valorizza variabili globali




	var ags_proxy = app_url + '/proxy/proxy.ashx';
	var ags_server = "http://localhost";

	return {
		startup: function () {


			if (_exception != '') {
				$('#mapViewDiv').append('<div class="alert alert-danger text-center" role="alert" style="margin:120px;"><h4>ERRORE DI INIZIALIZZAZIONE</h4><p>' + _exception + '</p></div>');

				$('#loadingImg').hide();
				return false;
			}

			urlUtils.addProxyRule({
				urlPrefix: ags_server,
				proxyUrl: ags_proxy
			});

			app.ags_services_url = ags_server + "/arcgis/rest/services/";
			app.mapContainer = $('#mapContainer');
			app.map = new Map("mapViewDiv", {
				logo: false, slider: false, showLabels: true,
				scrollWheelZoom: true, maxScale: 200,
				minScale: 100000
			});

			app.mapServerUrl = '';
			app.operationalMap = null;
			app.operationalMapLegend = null;
			app.editToolbar = null;
			//app.rasterMap = null;

			app.mainToolbar = '#mainToolbar'; // Barra degli strumenti
			app.sidebarToolbar = '#sidebarToolbar';
			app.panelContainer = '#panelContainer';

			app.legend = null;

			app.dijitSearch = null;
			app.searchDiv = 'searchNavDiv';
			app.loading = dom.byId("loadingImg");
			app.initialExtent = null;
			app.editTypeEnum = {
				Add: 1, Edit: 2, Delete: 3
			};

			app.identifyConfig = {
				Layer: null,
				LayerIds: []
			};

			app.tools = {
				IDENTIFY: 'identify',
				SELECT: 'select',
				ADD_POINT: 'add_point'
			};
			app.activeTool = app.tools.IDENTIFY;

			app.currentEditingLayer = null;
			app.enumLayers = {
				PARTICELLE: {
					name: 'PARTICELLE', id: -1
				},
				FABBRICATI: {
					name: 'FABBRICATI', id: -1
				},
				EDIFICI: {
					name: 'EDIFICI', id: -1
				},
				VINCOLI: {
					name: 'TEMATISMI', id: -1
				},
				SUED: {
					name: 'SUED', id: -1
				},
				GRAFO: {
					name: 'STRADE', id: -1
				},
				CIVICI: {
					name: 'NUMERAZIONE CIVICA', id: -1
				},
				BASE: {
					name: 'BASE', id: -1
				}
			};


			// Variabile che viene valorizzata con i visible layers della basemap a cui però vengono tolti
			// i grouplayer SUED e TEMATISMI, che portano problemi con l'accensione/spegnimento dei sotto livelli
			app.visibleLayers = [];
			app.visibleLayersBase = [];
			app.visibleLayersVincoli = [];


			/*
			 * Methods
			 */

			app.showLoading = function () {
				esri.show(app.loading);
				app.map.disableMapNavigation();
				app.map.hideZoomSlider();
			};

			app.hideLoading = function (error) {
				esri.hide(app.loading);
				app.map.enableMapNavigation();
				app.map.showZoomSlider();
			};

			app.log = function (msg) {
				$('#logDiv').html(msg);
			};

			app.createDynamicLayer = function (mapServerUlr, operationalLayerId, onLoadCallback) {
				var imageParameters = new ImageParameters();
				imageParameters.format = 'png32 ';
				imageParameters.transparent = true;
				var agsDynamic = new ArcGISDynamicMapServiceLayer(mapServerUlr, {
					'id': operationalLayerId,
					'imageParameters': imageParameters
				});
				agsDynamic.on("load", onLoadCallback);
				agsDynamic.on("error", function (err) {
					//BootstrapDialog.alert('Impossibile caricare il servizio.\nErrore: ' + err.error + '\nService Name: ' + serviceUrl);
					if (err.error.message !== 'Request canceled') {
						alert('Impossibile caricare il servizio:\n' + mapServerUlr + '\n' + err.error);//+ '\nService Name: ' + serviceName);
						console.log(mapServerUlr);
					}

				});

			};

			app.calculateDefaultFullExtent = function (zoomLayer, mapReady) {
				var queryTask = new esri.tasks.QueryTask(zoomLayer.url);
				var query = new esri.tasks.Query();
				query.returnGeometry = true;
				query.outFields = ["*"];
				query.where = zoomLayer.defQuery;
				queryTask.execute(query, function (results) {
					if (results.features.length > 0) {

						dojo.forEach(results.features, function (feat) {


							if (app.initialExtent === null) {
								app.initialExtent = feat.geometry.getExtent();
							}
							else {
								app.initialExtent = app.initialExtent.union(feat.geometry.getExtent());
							}



						}); // end dojo.forEach


						app.initialExtent = app.initialExtent.expand(2);

						var def = app.map.setExtent(app.initialExtent);


						def.then(function (value) {
							mapReady();
						}, function (err) {
							// Do something when the process errors out
						}, function (update) {
							// Do something when the process provides progress information
						});
					}
					else {
						mapReady();
					}
				});
			};

			app.getExtentForScale = function (scale) {
				return scaleUtils.getExtentForScale(app.map, scale);
			}

			app.zoomToScale = function (scale) {
				app.map.setExtent(scaleUtils.getExtentForScale(app.map, scale));
			}

			app.updateFeatureLayer = function (layerName, feature, editType, onEditComplete) {
				var featureLayer = app.map.getLayer(layerName);
				var deferred;
				switch (editType) {
					case app.editTypeEnum.Add:
						deferred = featureLayer.applyEdits([feature], null, null);
						break;
					case app.editTypeEnum.Edit:
						deferred = featureLayer.applyEdits(null, [feature], null);
						break;
					case app.editTypeEnum.Delete:
						deferred = featureLayer.applyEdits(null, null, [feature]);
						break;
				}

				deferred.then(function (adds, updates, deletes) {
					var result, msg = '';
					switch (editType) {
						case app.editTypeEnum.Add:
							result = adds.slice();
							break;
						case app.editTypeEnum.Edit:
							result = updates.slice();
							break;
						case app.editTypeEnum.Delete:
							result = deletes.slice();
							break;
					}
					for (var i = 0; i < result.length; i++) {
						var r = result[i];
						if (r.success === false) {
							msg += r.error.message + '\n';
						}
					}

					if (msg !== '') {
						BootstrapDialog.alert({ title: 'ERRORE', message: msg, type: BootstrapDialog.TYPE_DANGER });
					}
					else {
						//
						if ($.isFunction(onEditComplete)) {
							onEditComplete(feature);
						}
					}


				}, function (error) {
					BootstrapDialog.alert({ title: 'ERRORE', message: error.message + '\nError Code: ' + error.code, type: BootstrapDialog.TYPE_DANGER });
				});
			};

			app.getLayerAttributes = function (layerId) {
				var featureLayer = app.map.getLayer(layerId);
				if (isEmpty(featureLayer)) {
					alertDanger('Livello ' + layerId + ' non valido.\nSe il problema persiste, ricaricare la pagina (CTRL+F5).');
					return;
				}
				var attributes = {};
				dojo.forEach(featureLayer.fields, function (field) {
					attributes[field.name] = null;
				});
				return attributes;
			};

			app.queryFeatures = function (layerId, where, onQueryComplete) {
				app.showLoading();
				var queryTask = new esri.tasks.QueryTask(app.operationalMap.url + '/' + layerId);
				var query = new Query();
				query.where = where;
				//query.outFields = ['*'];
				query.returnGeometry = true;

				queryTask.execute(query,
					function (result) {
						app.hideLoading();
						if (result.features.length == 0) {
							alertWarning('<p style="font-size:10pt;">La ricerca per <strong>"' + where + '"</strong>, non ha prodotto risultati!</p>');
						}
						else {
							onQueryComplete(result.features);
						}

					},
					function (err) {
						app.hideLoading();
						console.log(err);
					});
			};

			app.queryIds = function (layerId, oids, onQueryComplete) {
				var queryTask = new esri.tasks.QueryTask(app.operationalMap.url + '/' + layerId);
				var query = new Query();
				query.objectIds = oids.slice();
				query.outFields = ['*'];
				query.returnGeometry = true;

				queryTask.execute(query, function (result) {
					onQueryComplete(result.features);
				});
			};

			app.selectFeatures = function (layerId, query, selectionMode, onSelectComplete) {
				var featureLayer = app.map.getLayer(layerId);


				featureLayer.selectFeatures(query, selectionMode, function (featureSet) {
					onSelectComplete(featureSet);

				});
			};

			app.zoomToSelectedFeatures = function (layerId) {
				var featureLayer = app.map.getLayer(layerId);
				//var features = featureLayer.getSelectedFeatures();
				//if (features.length > 0) {

				//	if (featureLayer.geometryType == 'esriGeometryPoint' && features.length == 1) {
				//		app.map.setExtent(app.createExtentAroundAPoint(features[0].geometry, 5, 5));

				//	}
				//	else {
				//		app.map.setExtent(graphicsUtils.graphicsExtent(features).expand(2));
				//	}

				//}

				app.zoomToFeatures(featureLayer.geometryType, featureLayer.getSelectedFeatures());
			};

			app.zoomToFeatures = function (geometryType, features) {
				if (features.length > 0) {

					if (geometryType == 'esriGeometryPoint' && features.length == 1) {
						app.map.setExtent(app.createExtentAroundAPoint(features[0].geometry, 5, 5));

					}
					else {
						app.map.setExtent(graphicsUtils.graphicsExtent(features).expand(2));
					}

				}
			};

			app.createExtentAroundAPoint = function (point, width, height) {

				return new Extent(point.x - width, point.y - height, point.x + width, point.y + height, point.spatialReference);
			};

			app.activateToolbar = function (layerId, objectId) {
				if (app.editToolbar === null) {
					app.editToolbar = new Edit(app.map);
					// -- Fired when the mouse button is released, usually after moving the graphic. 
					// -- Applicable only when the MOVE tool is active. (Added at v3.6)
					app.editToolbar.on('graphic-move-stop', function (feature, transform) {
						app.currentEditingLayer.clearSelection();
						app.editToolbar.deactivate();
					});

					app.editToolbar.on("deactivate", function (evt) {
						if (evt.info.isModified) {
							app.map.infoWindow.hide();

							if ($.isFunction(app.checkPosition)) {
								app.checkPosition(evt.graphic, false);
							}

						}
					});
				}

				app.map.infoWindow.hide();
				app.currentEditingLayer = app.map.getLayer(layerId);
				app.currentEditingLayer.clearSelection();

				app.queryIds(layerId, objectId, FeatureLayer.SELECTION_NEW, true, function (features) {
					if (features.length > 0) {
						var feature = features[0];
						var tool = 0;
						tool = tool | Edit.MOVE;
						var options = {};

						app.editToolbar.activate(tool, feature, options);
					}
				});

			};

			app.highlightGraphic = function (oid, graphics) {

				var g = null;
				app.map.infoWindow.hide();
				for (var i = 0; i < graphics.length; i++) {
					g = graphics[i];

					if (oid === g.attributes.OBJECTID) {
						i = graphics.length;
						//g.getShape().moveToFront();
						var wait = app.map.centerAt(g.geometry);
						wait.then(function (value) {
							// Do something when the process completes
							app.map.infoWindow.clearFeatures();
							app.map.infoWindow.setFeatures([g]);
							app.map.infoWindow.show(g.geometry);
						}, function (err) {
							// Do something when the process errors out
						}, function (update) {
							// Do something when the process provides progress information
						});

					}
				}
			};

			app.highlightFeatures = function (features) {
				app.map.graphics.clear();

				for (var i = 0; i < features.length; i++) {
					var geom = features[0].geometry;
					var graph = new esri.Graphic(geom);
					if (geom.type == "point") {
						var sym = new SimpleMarkerSymbol(app.map.infoWindow.markerSymbol.toJson());
						sym.setSize(48);
						graph.setSymbol(sym);
					}
					else {
						graph.setSymbol(app.map.infoWindow.fillSymbol);
					}

					app.map.graphics.add(graph);
				}

			}

			app.closeAllSidebar = function () {
				$('div.sidebar').each(function () {
					if ($(this).data('bs.sidebar')) {
						$(this).data('bs.sidebar').hide();
					}
				});
			};

			app.showInfoWithToolbar = function (layerId, feature, infoTitle, infoContent, editDetailCallback, hasEditPosition, hasDeleteFeature, evt) {
				var btnGroup = domConstruct.create("div", { className: "btn-group pull-right", role: "group" });

				var content = domConstruct.create("div");
				var subcontent = domConstruct.toDom('<div style="max-height:200px; overflow:auto;"></div>');
				var toolbar = domConstruct.toDom('<div></div>');

				if ($.isFunction(editDetailCallback)) {
					var editDetail = domConstruct.create("a", { href: "#", title: "Modifica", className: 'btn btn-default btn-sm', innerHTML: '<span class="glyphicon glyphicon-pencil"></span>' });
					on(editDetail, "click", function () {

						editDetailCallback(feature);
					});
					domConstruct.place(editDetail, btnGroup);
				}

				if (hasEditPosition) {
					var editPosition = domConstruct.create("a", { href: "#", title: "Sposta", className: 'btn btn-default btn-sm', innerHTML: '<span class="glyphicon glyphicon-move"></span>' });
					on(editPosition, "click", function () {

						app.activateToolbar(layerId, feature.attributes.OBJECTID);
					});
					domConstruct.place(editPosition, btnGroup);
				}

				if (hasDeleteFeature) {
					var deleteFeature = domConstruct.create("a", { href: "#", title: "Elimina", className: 'btn btn-default btn-sm', innerHTML: '<span class="glyphicon glyphicon-trash"></span>' });
					on(deleteFeature, "click", function () {

						BootstrapDialog.confirm({
							title: 'CONFERMA',
							message: 'Eliminare questo elemento?',
							type: BootstrapDialog.TYPE_WARNING, // <-- Default value is BootstrapDialog.TYPE_PRIMARY
							btnCancelLabel: 'Annulla', // <-- Default value is 'Cancel',
							btnOKLabel: 'Ok!', // <-- Default value is 'OK',
							btnOKClass: 'btn-warning', // <-- If you didn't specify it, dialog type will be used,
							callback: function (result) {
								// result will be true if button was click, while it will be false if users close the dialog directly.
								if (result) {
									app.map.infoWindow.hide();
									app.updateFeatureLayer(layerId, feature, app.editTypeEnum.Delete, function (deleted) {
										app.operationalMap.refresh();
										// requestItems();

									});
								}
							}
						});




					});
					domConstruct.place(deleteFeature, btnGroup);
				}


				domConstruct.place(btnGroup, toolbar);

				domConstruct.place(subcontent, content);
				domConstruct.place(dojo.toDom(infoContent), subcontent);
				domConstruct.place(domConstruct.create("hr"), content);
				domConstruct.place(toolbar, content);

				app.map.infoWindow.setTitle(infoTitle);
				app.map.infoWindow.setContent(content);
				app.map.infoWindow.show(evt.mapPoint);
			};

			app.initSearch = function (searchSources, allPlaceholder) {
				// search capabilities based on locator service
				app.dijitSearch = new Search({
					map: app.map,
					enableHighlight: false,
					showInfoWindowOnSelect: true,
					sources: searchSources.slice(),
					allPlaceholder: allPlaceholder
				}, app.searchDiv);
				app.dijitSearch.startup();

				app.dijitSearch.on('select-result', function (e) {
					//console.log(e);

					app.zoomToFeatures(e.result.feature._layer.geometryType, [e.result.feature]);
				});
			};

			app.loadLegend = function (url) {
				$.getJSON(url, {},
					function (data, textStatus, jqXHR) {
						app.operationalMapLegend = data;
					});
			};

			app.getLayerLegend = function (layerId) {
				var layerLegend;
				$.each(app.operationalMapLegend.layers, function (i, esriLayer) {
					if (esriLayer.layerId == layerId) {
						layerLegend = esriLayer.legend.slice();
						return false;
					}
				});
				return layerLegend;
			};

			app.getInfoTemplate = function (layerName, layerNameDescr) {
				switch (layerName.toUpperCase()) {
					case app.enumLayers.GRAFO.name:
						return new InfoTemplate(
							layerName.toUpperCase(),
							'Toponimo: ${LABEL}'
						);
					case app.enumLayers.CIVICI.name:
						return new InfoTemplate(
							layerName.toUpperCase(),
							'INDIRIZZO: ${INDIRIZZO}<br/>' +
							'OCCUPANTE: ${OCCUPANTE_}<br/>' +
							'PROPRIETARIO: ${PROPRIET}'
						);
					case app.enumLayers.PARTICELLE.name:
					case app.enumLayers.FABBRICATI.name:
						if (_userType != 2) {
							return new InfoTemplate(
								layerName.toUpperCase(),
								'COD. ${COMUNE} FG. ${FOGLIO} NUM. ${NUMERO}<br/>' +
								'<button type="button" class="btn btn-primary btn-xs btn-analizza-vincoli" data-oid="${OBJECTID}" data-livello="' + layerName.toUpperCase() + '" style="margin-top: 24px;">Analizza Vincoli</button>&nbsp;' +
								'<button type="button" class="btn btn-primary btn-xs btn-info-catasto" data-terna="${COMUNE}|${FOGLIO}|${NUMERO}" data-livello="' + layerName.toUpperCase() + '" style="margin-top: 24px;">Info Catastali</button>&nbsp;' +
								'<button type="button" class="btn btn-primary btn-xs btn-info-sued" data-terna="${COMUNE}|${FOGLIO}|${NUMERO}" data-livello="' + layerName.toUpperCase() + '" style="margin-top: 24px;">SUED</button>'
							);
						}
						else {
							return new InfoTemplate(
								layerName.toUpperCase(),
								'COD. ${COMUNE} FG. ${FOGLIO} NUM. ${NUMERO}<br/>' +
								'<button type="button" class="btn btn-primary btn-xs btn-analizza-vincoli" data-oid="${OBJECTID}" data-livello="' + layerName.toUpperCase() + '" style="margin-top: 24px;">Analizza Vincoli</button>&nbsp;'
							);
						}
						break;
					case app.enumLayers.VINCOLI.name:
						return new InfoTemplate(
							"VINCOLI",
							'Tipo vincolo: ' + layerNameDescr + '<br />' +
							'Rif. normativo: ${RIF_NORMATIVO}<br/>'
						);
				}
			};

			app.init = function () {

				$('#TitleBar').html(_descrizione);

				//$('#panelButtonContainer').append($('#tmplPanelButton').render({ Id: 'Basemaps', Title: 'Tematismi', Icon: 'glyphicon glyphicon-th-large' }));
				//$('#panelContainer').append($('#tmplPanel').render({ Id: 'Basemaps', Title: 'Tematismi', Icon: 'glyphicon glyphicon-th-large' }));
				var allPlaceholder = '';

				//app.createDynamicLayer(app.ags_services_url + 'PITER/PRGRaster/MapServer', 'PRGRasterLayer', function (response) {
				//	app.rasterMap = response.layer;

				app.mapServerUrl = app.ags_services_url + '/' + _servizio + '/MapServer';
				app.createDynamicLayer(app.mapServerUrl, 'MainLayer', function (response) {
					attachDomEvents();
					app.operationalMap = response.layer;
					app.identifyConfig.Layer = response.layer;

					//var prgInfos = app.rasterMap.layerInfos[0]
					//prgInfos.id = 56;
					//app.operationalMap.layerInfos.push(prgInfos);
					// Introdotto il 17/07/2020
					app.visibleLayers = app.operationalMap.visibleLayers.slice();

					var searchSources = [];
					var infos = app.operationalMap.layerInfos;
					for (var i = 0; i < infos.length; i++) {
						info = infos[i];

						switch (info.name.toUpperCase()) {

							case 'CATASTO':
								if (_userType != 2) {
									fc_catasto.inizializza();
								}

								//var $btn = $('<button type="button" class="btn btn-sm btn-default"><span id="catastoIcon" class="glyphicon glyphicon-unchecked"></span> Catasto</button>');
								//$btn.click(function () {
								//	catastoAccendiSpegni();
								//});
								//$(app.mainToolbar).prepend($btn);

								break;

							case app.enumLayers.PARTICELLE.name:
							case app.enumLayers.FABBRICATI.name:
								app.identifyConfig.LayerIds.push(info.id);

								allPlaceholder = 'CERCA FG/NUM';
								if (info.name.toUpperCase() == app.enumLayers.PARTICELLE.name) {
									// Rimuovere If per aggiungere anche FABBRICATI ALLA CASELLA DI RICERCA CON AUTOCOMPLETAMENTO
									searchSources.push({
										featureLayer: new FeatureLayer(app.mapServerUrl + '/' + info.id, {
											outFields: ['*'],
											infoTemplate: app.getInfoTemplate(info.name.toUpperCase())
										}),
										outFields: ['OBJECTID', 'COMUNE', 'FOGLIO', 'NUMERO', 'RICERCA'],
										searchFields: ['RICERCA'],
										maxSuggestions: 6,
										displayField: 'RICERCA',
										suggestionTemplate: 'FG. ${FOGLIO} NUM. ${NUMERO}',
										name: info.name.toUpperCase(),
										placeholder: 'CERCA FG/NUM ' + info.name.toUpperCase(),
										enableSuggestions: true,
										autoNavigate: false

									});

								}

								if (info.name.toUpperCase() == app.enumLayers.PARTICELLE.name) {
									app.enumLayers.PARTICELLE.id = info.id;
								}
								if (info.name.toUpperCase() == app.enumLayers.FABBRICATI.name) {
									app.enumLayers.FABBRICATI.id = info.id;
								}


								break;
							case app.enumLayers.VINCOLI.name:
								app.identifyConfig.LayerIds.push(info.id);

								app.visibleLayers.splice(app.visibleLayers.indexOf(info.id), 1);
								app.enumLayers.VINCOLI.id = info.id;
								break;
							case app.enumLayers.BASE.name:

								app.visibleLayers.splice(app.visibleLayers.indexOf(info.id), 1);
								app.enumLayers.BASE.id = info.id;


								break;
							case app.enumLayers.SUED.name:
								if (_userType != 2) {
									app.visibleLayers.splice(app.visibleLayers.indexOf(info.id), 1);
									app.enumLayers.SUED.id = info.id;

								}
								break;
							case app.enumLayers.GRAFO.name:
								app.identifyConfig.LayerIds.push(info.id);

								if (info.name.toUpperCase() == app.enumLayers.GRAFO.name) {
									app.enumLayers.GRAFO.id = info.id;
								}

								allPlaceholder = 'CERCA STRADA';
								if (info.name.toUpperCase() == app.enumLayers.GRAFO.name) {
									// Rimuovere If per aggiungere anche FABBRICATI ALLA CASELLA DI RICERCA CON AUTOCOMPLETAMENTO
									searchSources.push({
										featureLayer: new FeatureLayer(app.mapServerUrl + '/' + info.id, {
											outFields: ['*'],
											infoTemplate: app.getInfoTemplate(info.name.toUpperCase())
										}),
										outFields: ['OBJECTID', 'LABEL'],
										searchFields: ['LABEL'],
										maxSuggestions: 6,
										displayField: 'LABEL',
										suggestionTemplate: '${LABEL}',
										name: info.name.toUpperCase(),
										placeholder: 'CERCA STRADA ' + info.name.toUpperCase(),
										enableSuggestions: true,
										autoNavigate: false
									});
								}
								break;
							case app.enumLayers.CIVICI.name:
								app.identifyConfig.LayerIds.push(info.id);


								if (info.name.toUpperCase() == app.enumLayers.CIVICI.name) {
									app.enumLayers.CIVICI.id = info.id;
								}

								if (info.name.toUpperCase() == app.enumLayers.CIVICI.name) {
									allPlaceholder = 'CERCA CIVICI';
									searchSources.push({
										featureLayer: new FeatureLayer(app.mapServerUrl + '/' + info.id, {
											outFields: ['*'],
											infoTemplate: app.getInfoTemplate(info.name.toUpperCase())
										}),
										outFields: ['OBJECTID', 'INDIRIZZO', 'PROPRIET', 'OCCUPANTE_'],
										searchFields: ['INDIRIZZO'],
										maxSuggestions: 6,
										displayField: 'INDIRIZZO',
										suggestionTemplate: '${INDIRIZZO}',
										name: "CIVICI",
										placeholder: 'CERCA CIVICO ' + info.name.toUpperCase(),
										enableSuggestions: true,
										autoNavigate: false
									});

									allPlaceholder = 'CERCA UTENTI';
									searchSources.push({
										featureLayer: new FeatureLayer(app.mapServerUrl + '/' + info.id, {
											outFields: ['*'],
											infoTemplate: app.getInfoTemplate(info.name.toUpperCase())
										}),
										outFields: ['OBJECTID', 'INDIRIZZO', 'PROPRIET', 'OCCUPANTE_'],
										searchFields: ['PROPRIET', 'OCCUPANTE_'],
										maxSuggestions: 6,
										displayField: 'INDIRIZZO',
										suggestionTemplate: 'PR. ${PROPRIET} OC. ${OCCUPANTE_}',
										name: "CIVICI",
										placeholder: 'CERCA UTENTE ' + info.name.toUpperCase(),
										enableSuggestions: true,
										autoNavigate: false
									});

								}
						}
					} // End For

					if (searchSources.length > 0) {
						app.initSearch(searchSources, allPlaceholder);
					}

					// inizializza funzioni speciali
					if (app.enumLayers.VINCOLI.id != -1) {
						fc_vincoli.inizializza(app.enumLayers.VINCOLI.id, infos);
					}

					if (app.enumLayers.BASE.id != -1) {
						fc_base.inizializza(app.enumLayers.BASE.id, infos);
					}

					if (app.enumLayers.SUED.id != -1) {
						sued.inizializza(app.enumLayers.SUED.id, infos);
					}
					app.map.addLayers([response.layer]);
					//initBaseMapGallery();
				});
				//})
			}

			app.map.on('click', function (evt) {

				switch (app.activeTool) {
					case app.tools.IDENTIFY:
						Identify.executeIdentifyTask(evt, app.identifyConfig.Layer, app.identifyConfig.LayerIds, function (identifyResults) {

							app.map.infoWindow.setFeatures(identifyResults);
							app.map.infoWindow.show(evt.mapPoint);
						});
						break;
				}


			});
			app.map.on('update-start', app.showLoading);
			app.map.on('update-end', app.hideLoading);
			app.map.on('layers-add-result', function (result) {

				app.initialExtent = app.map.extent;
				//Legend.build(DynamicLayer.layer, '#legendDiv');

				$(app.sidebarToolbar).append($('#tmplPanelButton').render({ Id: 'Legenda', Title: 'Legenda', Icon: 'glyphicon glyphicon-th-list' }));
				//$(app.sidebarToolbar).append($('#tmplPanelButton').render({ Id: 'basemapWidget', Title: 'Mappe di base', Icon: 'esri-icon-basemap' }));
				$(app.panelContainer).append($('#tmplPanel').render({ Id: 'Legenda', Title: 'Legenda', Icon: 'glyphicon glyphicon-th-list' }));
				//$(app.panelContainer).append($('#tmplPanel').render({ Id: 'basemapWidget', Title: 'Mappe di base', Icon: 'esri-icon-basemap' }));
				//dojo.forEach(result.layers, function (l) {
				//	if (l.layer.id === 'MainLayer') {
				//		Legend.build(l.layer, '#panelContentLegenda');
				//	}
				//});


				app.legend = new Legend({
					map: app.map
				}, "panelContentLegenda");
				app.legend.startup();


				app.loadLegend(ags_proxy + "?" + app.mapServerUrl + "/legend?f=json");

				// Se viene passata una particella in input, ne analizza i vincoli
				if (_particella_oid > 0) {
					fc_vincoli.analizza(app.enumLayers.PARTICELLE, _particella_oid);
				}

				//initBaseMapGallery();
			});

			//        callWebMethod(page_Url + '/GetUserProfile', {
			//            viewerType: _viewerType
			//        }, function (response) {
			//            userProfile = $.parseJSON(response.d);
			//            if ($.isFunction(app.init)) {
			//            }

			//});

			app.init();
		}
	};
	//var exportMapGPServerURL = ags_services_url + "/ExportWebMap/GPServer/Export%20Web%20Map";

	// Init App e Map
	function initBaseMapGallery() {
		//--- manually create basemaps to add to basemap gallery
		var thumb_path = app_url + '/WebForms/css/images/';
		var basemaps = [];

		basemaps.push(
			new Basemap({
				id: "World_Street_Map",
				layers: [new BasemapLayer({
					url: "https://services.arcgisonline.com/ArcGIS/rest/services/World_Street_Map/MapServer"
				})],
				title: "Vie",
				thumbnailUrl: thumb_path + "basemap_World_Street_Map.png"
			})
		);

		basemaps.push(
			new Basemap({
				id: "World_Topo_Map",
				layers: [new BasemapLayer({
					url: "https://services.arcgisonline.com/ArcGIS/rest/services/World_Topo_Map/MapServer"
				})],
				title: "Topografico",
				thumbnailUrl: thumb_path + "basemap_World_Topo_Map.png"
			})
		);


		basemaps.push(
			new Basemap({
				id: "World_Imagery",
				layers: [new BasemapLayer({
					url: "https://services.arcgisonline.com/ArcGIS/rest/services/World_Imagery/MapServer"
				})],
				title: "Immagini",
				thumbnailUrl: thumb_path + "basemap_World_Imagery.png"
			})
		);



		basemapGallery = new esri.dijit.BasemapGallery({
			showArcGISBasemaps: false,
			basemaps: basemaps,
			map: app.map
		}, "panelContentbasemapWidget");
		//----
		basemapGallery.startup();

		basemapGallery.on("error", function (evt) {
			console.log("basemap gallery error:  ", evt.message);
		});
		basemapGallery.on("selection-change", function () {
			var currentBasemap = basemapGallery.getSelected();
			//app.map.setBasemap(currentBasemap);
		});
		}

	function catastoAccendiSpegni() {
		//app.visibleLayers = app.operationalMap.visibleLayers.slice();
		var visibleLayersId = app.visibleLayers.slice();

		//var _visibleLayers = app.operationalMap.visibleLayers;
		if (visibleLayersId.filter(x => x === app.enumLayers.PARTICELLE.id).length > 0) {
			$("#catastoIcon").removeClass("glyphicon-check").addClass("glyphicon-unchecked");
			//	_visibleLayers.splice($.inArray(app.enumLayers.PARTICELLE.id, _visibleLayers), 1);
			//	_visibleLayers.splice($.inArray(app.enumLayers.FABBRICATI.id, _visibleLayers), 1);
		} else {
			$("#catastoIcon").removeClass("glyphicon-unchecked").addClass("glyphicon-check");
			visibleLayersId.push(app.enumLayers.PARTICELLE.id);
			visibleLayersId.push(app.enumLayers.FABBRICATI.id);
		}

		//app.operationalMap.setVisibleLayers(_visibleLayers);
		//app.legend.refresh();

		//app.operationalMap.visibleLayers = _visibleLayers;

		app.operationalMap.setVisibleLayers(visibleLayersId);
		app.legend.refresh();

	}

	function attachDomEvents() {


		// Inizializzazione controlli
		$('#zoomFull').click(function () {
			if (app.initialExtent) {
				app.map.setExtent(app.initialExtent);
			}

		});
		$('#zoomIn').click(function () {
			zoom(0.5);
		});
		$('#zoomOut').click(function () {
			zoom(1.5);
		});

		function zoom(factor) {
			app.map.setExtent(app.map.extent.expand(factor));
		}

		$(document).on('change', '.select-all-feature', function () {
			$('.select-feature').prop('checked', $(this).prop('checked'));
		});

		$(document).on('click', '.select-feature, .zoom-feature', function () {


			var layerId = $(this).attr('data-layer');
			var objectId = $(this).attr('data-objectid');

			var selectionMode = FeatureLayer.SELECTION_NEW; // valido per zoom-feature

			if ($(this).hasClass('select-feature')) {
				selectionMode = $(this).prop('checked') ? FeatureLayer.SELECTION_ADD : FeatureLayer.SELECTION_SUBTRACT;
			}

			app.queryIds(layerId, objectId, selectionMode, false, function (featureSet) {
				app.zoomToSelectedFeatures(layerId);

			});
		});

		$(document).on('click', '.btn-edit-position', function () {
			var layerId = $(this).attr('data-layer');
			var objectId = $(this).attr('data-objectid');
			app.activateToolbar(layerId, objectId);
		});




		$(document).on('click', 'button.zoom-coords', function () {
			var lng = $(this).data('lng');
			var lat = $(this).data('lat');
			var lod = $(this).data('lod');
			var pt = webMercatorUtils.geographicToWebMercator(new Point(lng, lat));
			app.map.centerAt(pt);//, lod);
		});


	}

	function getParameterByName(name, url) {
		if (!url) url = window.location.href;
		name = name.replace(/[\[\]]/g, "\\$&");
		var regex = new RegExp("[?&]" + name + "(=([^&#]*)|&|#|$)"),
			results = regex.exec(url);
		if (!results) return null;
		if (!results[2]) return '';
		return decodeURIComponent(results[2].replace(/\+/g, " "));
	}


});