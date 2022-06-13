define(["esri/request",
	"esri/graphicsUtils",
	"esri/graphic",
	"esri/symbols/SimpleFillSymbol",
	"dojo/_base/array", "dojo/on"],
	function (esriRequest, graphicsUtils, Graphic, SimpleFillSymbol, arrayUtils, on) {

		"use strict";

		var _tocLayers = [];
		var _visibleLayers = [];
		var _identifyParams = null;

		
		var $sidebar = null;
		var $tocContainer = null;

		return {
			inizializza: function (groupLayerId, layerInfos) {
				//setIdentifyTask();
				createSidebar();
				createTOC(groupLayerId, layerInfos);
				
			},
		};
		// ------------------------

		//function setIdentifyTask() {
		//	// Inizio preparazione variabili per estrazione vincoli
		//	_identifyTask = new esri.tasks.IdentifyTask(app.operationalMap.url);
		//	_identifyParams = new esri.tasks.IdentifyParameters();
		//	_identifyParams.tolerance = 0;
		//	_identifyParams.returnGeometry = true;
		//	_identifyParams.layerOption = esri.tasks.IdentifyParameters.LAYER_OPTION_ALL;
		//	_identifyParams.width = app.map.width;
		//	_identifyParams.height = app.map.height;
		//	_identifyParams.layerIds = [];
		//}

		function createSidebar() {
			$tocContainer = $('<div>').sidebar();
			app.mapContainer.append($tocContainer);
			$sidebar = $tocContainer.data('bs.sidebar');

			// Aggiunge un bottone nella toolbar secondaria per aprire il pannello con l'elenco dei layers nel gruppo base
			var $btnOpenSidebar = $($('#tmplPanelButton').render({ Id: 'BASE', Title: 'BASE', Icon: 'glyphicon glyphicon-leaf', Label: 'BASE' }));
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
					//_identifyParams.layerIds.push(info.id);
					_tocLayers.push({ layer: info, title: info.name }); // Per la TOC solo i livelli nel GroupLayer Tematismi
				}

			}
			//info.id = 100
			//info.name = "PRG Raster"
			//info.defaultVisibility = false;
			//_tocLayers.push({ layer: info, title: 'PRG Raster' });

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
				var visibleLayersId = _visibleLayers.slice();

				app.visibleLayersBase = [];
				$lst.find('.check_vincolo:checked').each(function () {

					visibleLayersId.push($(this).data('id'));
					app.visibleLayersBase.push($(this).data('id'));
				});
				$.each(app.visibleLayersVincoli, function (i, val) {
					visibleLayersId.push(val);
				});
				//visible.sort();
				app.operationalMap.setVisibleLayers(visibleLayersId);
				//app.legend.refresh();
			});

			var $btnUncheckAll = $('<button type="button" class="btn btn-danger btn-sm" style="margin-right:5px;">Spegni tutto</button>');
			$btnUncheckAll.click(function () {
				event.preventDefault();
				

				$lst.find('.check_vincolo').each(function () {
					$(this).prop('checked', false);
				});
				var visibleLayersId = _visibleLayers.slice(); //_visibleLayers.slice();
				app.operationalMap.setVisibleLayers(visibleLayersId);
				app.legend.refresh();
			});
			
			var $container = $('<div>');
			$container.append($btnUpdateMap).append($btnUncheckAll).append($lst);
			
			$sidebar.setContent('<p>BASE<br/><small>', $container);

			$tocContainer.on('show.bs.sidebar', function () {
				updateTOC();
			});

			function updateTOC() {
				$.each(app.operationalMap.visibleLayers, function () {
					
				});
			}
		}


		function findLayerInfo(mapService, subLayerId) {
			var infos = mapService.layerInfos;
			for (var i = 0, il = infos.length; i < il; i++) {
				if (infos[i].id == subLayerId) {
					return infos[i];
				}
			}
		}

		function truncateString(str, num) {
			if (str.length <= num) {
				return str
			}
			return str.slice(0, num) + '...'
		}
	});