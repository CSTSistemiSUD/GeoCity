define([],
	function () {
		"use strict";

		app.exportExtent = {
			CURRENT_EXTENT: 'CURRENT_EXTENT',
			CURRENT_SCALE: 'CURRENT_SCALE'
		};
		
		app.isBaseMapPlanLoaded = function () {
			return false; //Fittizio perchè in questo progetto non è previsto un serivizio base
		};

		app.getPageSize = function (values, fileName) {
			var pageId = '';
			var pageOrientation = '';
			var xmm = 0;
			var ymm = 0;

			pageId = values[0];
			xmm = values[1];
			ymm = values[2];
			pageOrientation = values[3];



			return { "Id": pageId, "Xmm": xmm, "Ymm": ymm, "Orientation": pageOrientation, "FileName": fileName };
		};

		app.exportLayoutClient = function (desiredExtent, pageSize, DPI, onExportLayoutCompleted) {
			// Esportazione


			var wmm = pageSize.Xmm;
			var hmm = pageSize.Ymm;


			var xpe = wmm / 25.4 * DPI;
			var ype = hmm / 25.4 * DPI;
			var xpixel = Math.round(xpe);
			var ypixel = Math.round(ype);



			var params = new esri.layers.ImageParameters();

			params.width = xpixel;
			params.height = ypixel;


			if (desiredExtent == app.exportExtent.CURRENT_EXTENT) {
				params.bbox = map.extent;
			}
			else {
				params.bbox = preserveScale(params.width, params.height, DPI);

			}

			params.format = "png8";
			params.transparent = true;
			params.dpi = DPI;

			params.layerDefinitions = app.operationalMap.layerDefinitions == null ? [] : app.operationalMap.layerDefinitions.slice();
			params.layerIds = app.operationalMap.visibleLayers.slice();
			params.layerOption = esri.layers.ImageParameters.LAYER_OPTION_SHOW;


			var urlParams = '';

			app.operationalMap.exportMapImage(params, function (mapImage) {
				var result = processExportLayoutResult(mapImage, pageSize.Id, pageSize.Orientation);

				if (result.ERROR_MESSAGE == '') {
					if (app.isBaseMapPlanLoaded()) {

						// ATTENZIONE MAI TESTATO (PROCEDURA ORIGINALE IN GEOSAFETY)

						var params1 = new esri.layers.ImageParameters();

						params1.width = params.width;
						params1.height = params.height;
						params1.bbox = params.bbox;
						params1.format = params.format;
						//params1.transparent = true;
						params1.dpi = params.dpi;
						basemapPlan.exportMapImage(params1, function (mapImage1) {
							var result1 = processExportLayoutResult(mapImage1, pageSize.Id, pageSize.Orientation);
							if (result1.ERROR_MESSAGE == '') {
								// Merge image
								dojo.xhrPost({
									url: _dbServiceUrl + "/CombineMapImage",
									handleAs: 'text',
									headers: { "Content-Type": "application/json", "Accept": "application/json" },
									timeout: 100000,
									postData: dojo.toJson({ tiledMapImageName: result1.EXPORTED_IMAGE, baseMapImageName: result.EXPORTED_IMAGE }),
									load: function (options, args) {

										result.EXPORTED_IMAGE = dojo.fromJson(options).d;
										result.DOWNLOAD_LINK = getDownloadLink(result.EXPORTED_IMAGE, result.MAP_SCALE, pageSize.Id, pageSize.Orientation);

									},
									error: function (error, args) {
										result.ERROR_MESSAGE == error;
									},
									handle: function () {
										onExportLayoutCompleted(result);
									}
								});
							}
							else {
								//onExportLayoutCompleted(result1); //modifica 23/06/2015: non capisco perchè chiamo onExportLayoutCompleted con result1 se result1 è in errore 
								onExportLayoutCompleted(result);
							}
						});
					}
				}

				if (!app.isBaseMapPlanLoaded() || result.ERROR_MESSAGE != '') {
					onExportLayoutCompleted(result);
				}


			});
		};

		function processExportLayoutResult(mapImage, layoutId, layoutOrientation) {
			var result = {
				ERROR_MESSAGE: mapImage.href, // In caso di errore rimane il messaggio
				EXPORTED_IMAGE: '',
				MAP_SCALE: 0
			};

			if (mapImage.href.indexOf('Error') < 0) {
				result.ERROR_MESSAGE = '';
				if (mapImage.href.lastIndexOf('/') != -1) {
					var firstpos = mapImage.href.lastIndexOf('/') + 1;
					var lastpos = mapImage.href.length;
					result.EXPORTED_IMAGE = mapImage.href.substring(firstpos, lastpos);
					result.MAP_SCALE = Math.ceil(mapImage.scale);
				}


			}
			return result;
		}

		function preserveScale(imageWidth, imageHeight, DPI) {

			var dots_per_m = DPI / 2.54 * 100;
			var width_size_in_m = (imageWidth / 2) / dots_per_m;
			var height_size_in_m = (imageHeight / 2) / dots_per_m;
			var maxX = app.map.extent.xmax;
			var minX = app.map.extent.xmin;
			var maxY = app.map.extent.ymax;
			var minY = app.map.extent.ymin;
			var centerX = maxX - (maxX - minX) / 2.;
			var centerY = maxY - (maxY - minY) / 2.;
			var scale = esri.geometry.getScale(app.map);
			maxX = scale * width_size_in_m + centerX;
			maxY = scale * height_size_in_m + centerY;
			minX = centerX - (maxX - centerX);
			minY = centerY - (maxY - centerY);
			var multi = new esri.geometry.Multipoint();
			var point = new esri.geometry.Point(minX, minY, new esri.SpatialReference({ wkid: app.map.spatialReference.wkid }));
			multi.addPoint(point);
			point = new esri.geometry.Point(maxX, maxY, new esri.SpatialReference({ wkid: app.map.spatialReference.wkid }));
			multi.addPoint(point);
			var scaleExtent = multi.getExtent();
			scaleExtent.setSpatialReference(app.map.spatialReference);
			return scaleExtent;
		}
	});