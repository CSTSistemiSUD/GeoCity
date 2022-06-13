define([
	"esri/request",
	"esri/tasks/IdentifyTask", "esri/tasks/IdentifyParameters", "esri/symbols/PictureMarkerSymbol", "esri/graphic",
	"dojo/on", "dojo/dom-construct", "dojo/_base/array",
	"dojo/domReady!"],
	function (esriRequest, IdentifyTask, IdentifyParameters, PictureMarkerSymbol, Graphic, on, domConstruct, arrayUtils) {
		"use strict";



		var identifyTask;
		var identifyParams;
		var identifyResults;
		var identifyLayers = [];
		var mapServiceUrl;

		var iconPath = location.href.replace(/\/[^/]+$/, '/');
		var symbol = new PictureMarkerSymbol(iconPath + "img/Location_marker_pin_map_gps.png", 24, 24);
		symbol.setOffset(-2, 8);
		var pushPinGraphic = new Graphic(null, symbol);

		var showClickeddPoint = false;

		return {

			executeIdentifyTask: function (evt, dynamicServiceLayer, identifyLayerIds, fn_IdentifyComplete) {
				// Inizializzazione
				if (showClickeddPoint) {
					pushPinGraphic.setGeometry(app.map.extent.getCenter());
					pushPinGraphic.hide();
					app.map.graphics.add(pushPinGraphic);
				}



				mapServiceUrl = dynamicServiceLayer.url;
				identifyTask = new IdentifyTask(mapServiceUrl);
				identifyParams = new IdentifyParameters();
				identifyParams.returnGeometry = true;
				identifyParams.layerOption = IdentifyParameters.LAYER_OPTION_ALL;

				identifyParams.tolerance = 5;
				identifyParams.width = app.map.width;
				identifyParams.height = app.map.height;
				identifyParams.layerIds = identifyLayerIds; //app.map.getLayer(operationalLayerId).visibleLayers; //

				identifyParams.layerDefinitions = dynamicServiceLayer.layerDefinitions.slice();

				identifyParams.geometry = evt.mapPoint;
				identifyParams.mapExtent = app.map.extent;



				identifyTask.execute(identifyParams, function (response) {
					//               identifyResults = [];


					//               // Riordina array dei risultati
					//response.sort(function (a, b) {
					//                   if (a.layerId > b.layerId)
					//                       return 1;
					//                   else
					//                       return -1;

					//               });


					//for (var i = 0; i < response.length; i++) {


					//	if (isEmpty(identifyResults[response[i].layerName])) {
					//		identifyResults[response[i].layerName] = [];
					//                   }
					//	identifyResults[response[i].layerName].push({
					//		layerId: response[i].layerId,
					//		feature: response[i].feature
					//	});


					//               }

					identifyResults = [];
					identifyResults = arrayUtils.map(response, function (result) {
						var feature = result.feature;
						var layerName = result.layerName;
						feature.attributes.layerName = layerName;

						if (app.visibleLayersVincoli.indexOf(result.layerId) > -1 ||
							app.visibleLayersBase.indexOf(result.layerId) > -1 ||
							app.visibleLayers.indexOf(result.layerId) > -1 ||
							layerName === 'Fabbricati' ||
							layerName === 'Particelle') {
							if (result.layerId > 10 && layerName !== 'Fabbricati' && layerName !== 'Particelle') {
								feature.setInfoTemplate(app.getInfoTemplate('TEMATISMI', layerName));
							} else {
								feature.setInfoTemplate(app.getInfoTemplate(layerName, layerName));
							}

							return feature;
						}
					});
					identifyResults = identifyResults.filter(function (element) {
						return element !== undefined;
					});

					if (response.length > 0) { // Aggiunto per evitare lo spostamento della mappa
						var def = app.map.centerAt(app.map.toMap(evt.screenPoint));
						if (showClickeddPoint) {
							pushPinGraphic.setGeometry(app.map.toMap(evt.screenPoint));
							pushPinGraphic.show();
						}
						def.then(function (value) {

							fn_IdentifyComplete(identifyResults, evt);



						}, function (err) {
							// Do something when the process errors out
						}, function (update) {
							// Do something when the process provides progress information
						});
					}


				}); // Fine identifyTask.execute
			}


		}; // return


		function writeIdentifyResult() {
			var infoContent = '';
			for (var layer in identifyResults) {
				infoContent += '<div class="table-responsive">';
				infoContent += '<table class="table table-condensed">';
				infoContent += '<caption>LIVELLO: <strong>' + layer.toUpperCase() + '</strong></caption>';
				infoContent += '<thead><tr><th></th>';

				for (var field in identifyResults[layer][0].feature.attributes) {
					infoContent += '<th>' + field + '</th>';
				}


				infoContent += '</tr></thead>';
				infoContent += '<tbody>';
				$.each(identifyResults[layer], function (i, result) {
					infoContent += '<tr><td>' +
						'<button type="button" ' +
						'   class="btn btn-default btn-xs zoom-to-feature" ' +
						'   data-layer="' + layer + '" data-idx="' + i + '"><span class="glyphicon glyphicon-search"></span></button></td>';
					for (var field in result.feature.attributes) {
						infoContent += '<td>' + notEmpty(result.feature.attributes[field]) + '</td>';
					}
					infoContent += '</tr>';
				});
				infoContent += '</tbody>';
				infoContent += '</table ></div>';
			}

			$infoPanel.empty().append(infoContent);
			$infoPanelContainer.show();


		}

		function showInfoWindow(layerId, feature) {


			app.highlightFeatures([feature]);

			return;
//			dojo.empty(_contentContainerId);

			//var infoFields;
			//if (isEmpty(identifyLayers[layerId])) {
			//    var requestHandle = esriRequest({
			//        "url": mapServiceUrl + '/' + layerId + '?f=pjson'
			//    });
			//    requestHandle.then(
			//        function (response, io) {
			//            //dom.byId("status").innerHTML = "";
			//            //dojoJson.toJsonIndentStr = "  ";
			//            //dom.byId("content").value = dojoJson.toJson(response, true);
			//            identifyLayers[layerId] = response;
			//            infoFields = decodeFields(feature, response);
			//            showFeatureDetail(infoFields);
			//        },
			//        function (error, io) {
			//            //domClass.add(dom.byId("content"), "failure");
			//            //dom.byId("status").innerHTML = "";

			//            //dojoJson.toJsonIndentStr = " ";
			//            //dom.byId("content").value = dojoJson.toJson(error, true);
			//        }
			//    );
			//}
			//else {
			//    infoFields = decodeFields(feature, identifyLayers[layerId]);
			//    showFeatureDetail(infoFields);
			//}
			//showFeatureDetail(feature);

		}

		function decodeFields(feature, featureLayer) {

			var infoFields = [];
			var decodedField = [];
			if (!isEmpty(featureLayer.typeIdField)) {// Stabilisce se ci sta un sottotipo
				var subTypeId, subTypeValue, subTypeDomains;
				subTypeId = feature.attributes[featureLayer.typeIdField]; // Recupera valore subtype

				// decodifica valore subtype e trova gli eventuali campi della feature che hanno il dominio
				// dipendente dal sottotipo
				dojo.every(featureLayer.types, function (subType) {
					if (subType.id == subTypeId) {
						subTypeValue = subType.name;
						subTypeDomains = subType.domains;
						return false;
					}
					return true;
				});

				infoFields.push({
					fieldName: featureLayer.typeIdField,
					fieldLabel: featureLayer.typeIdField,
					fieldValue: subTypeValue
				});

				decodedField.push(featureLayer.typeIdField);

				for (d in subTypeDomains) {// d vale il nome del campo


					dojo.every(subTypeDomains[d].codedValues, function (cv) {
						if (cv.code == feature.attributes[d]) { // decodifica il valore di dominio
							infoFields.push({ fieldName: d, fieldLabel: d, fieldValue: notEmpty(cv.name, '') });
							decodedField.push(d);
							return false;
						}
						return true;
					});


				}

			}

			// per tutti campi
			dojo.forEach(featureLayer.fields, function (field) {
				var fieldValue = feature.attributes[field.name];
				if (field.type == 'esriFieldTypeDate') {
					var milliseconds = new Date(feature.attributes[field.name]);
					//fieldValue = milliseconds.customFormat("#DD#/#MM#/#YYYY#");
				}
				else if (field.domain) {
					if (field.domain.codedValues) {
						dojo.every(field.domain.codedValues, function (cv) {
							if (cv.code == fieldValue) { // decodifica il valore di dominio
								fieldValue = cv.name;
								return false;
							}
							return true;
						});
					}
				}
				if (fieldValue == null) fieldValue = "";
				infoFields.push({ fieldName: field.name, fieldLabel: field.alias, fieldValue: notEmpty(fieldValue, '') });
			});

			return infoFields;
		}

		function showFeatureDetail(feature) {
			var content = domConstruct.create('div');

			var tr = '<tr><td class="field-label">${0}:</td><td class="field-value">${1}</td></tr>';

			var infoContent = '';
			infoContent += '<table class="table-info"><tbody>';


			for (var field in feature.attributes) {
				infoContent += dojo.string.substitute(tr, [field, notEmpty(feature.attributes[field])]);
			}


			infoContent += '</tbody></table>';

			var contentContainer = dojo.byId(_contentContainerId);
			domConstruct.place(dojo.toDom(infoContent), content);
			domConstruct.place(content, contentContainer);
		}

		function highlightFeature(feature, one_selected) {
			var g = new esri.Graphic(feature.geometry);
			if (feature.geometry.type == "point") {
				var sym = new SimpleMarkerSymbol(app.map.infoWindow.markerSymbol.toJson());
				sym.setSize(48);
				g.setSymbol(sym);
			}
			else {
				g.setSymbol(app.map.infoWindow.fillSymbol);
			}
			if (one_selected) {
				app.map.graphics.clear();
			}

			app.map.graphics.add(g);
		}

		function notEmpty(currentValue, defaultIfEmpty) {
			defaultIfEmpty = isEmpty(defaultIfEmpty) ? ' ' : defaultIfEmpty;
			if (isEmpty(currentValue)) {

				return defaultIfEmpty;
			}
			else {
				return currentValue;
			}

		}
	});; // define function