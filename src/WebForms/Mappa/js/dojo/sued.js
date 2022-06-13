define(["esri/graphicsUtils", "esri/graphic", "esri/symbols/SimpleFillSymbol", "dojo/on"],
	function (graphicsUtils, Graphic, SimpleFillSymbol, on) {
		"use strict";

		var _groupLayerId = -1;
		var _tocLayers = [];
		var _visibleLayers = [];

		var $btnUpdateMap = $('<button type="button" class="btn btn-success btn-sm">Aggiorna Mappa</button>');
		var $info, $listItem;
		var $tocContainer = null;
		var $sidebar = null;


		return {
			inizializza: function (groupLayerId, layerInfos) {

				createSidebar();

				setTemplates();
				
				setButtonInfoHandle();
			
				createTOC(groupLayerId, layerInfos);
				
			},
			apriToc: function () {
				$sidebar.show();
			}
		};

		function createSidebar() {
			$tocContainer = $('<div>').sidebar();
			app.mapContainer.append($tocContainer);
			$sidebar = $tocContainer.data('bs.sidebar');

			// Aggiunge un bottone nella toolbar secondaria per aprire il pannello con l'elenco dei vincoli
			var $btnOpenSidebar = $($('#tmplPanelButton').render({ Id: 'SUED', Title: 'SUED', Icon: 'glyphicon glyphicon-briefcase', Label: 'SUED' }));
			$btnOpenSidebar.removeClass('toggle-panel').removeAttr('data-target').click(function () { $sidebar.show(); });
			$(app.mainToolbar).append($btnOpenSidebar);


		}

		function setTemplates() {
			$info = $.templates(
				'<table class="table-info">' +
				'	<tbody>' +
				'	<tr><td class="field-label">SIGLATIPODOC</td><td>{{:SIGLATIPODOC}}</td></tr>' +
				'	<tr><td class="field-label">numero_protocollo</td><td>{{:numero_protocollo}}</td></tr>' +
				'	<tr><td class="field-label">data_protocollo</td><td>{{:data_protocollo}}</td></tr>' +
				'	<tr><td class="field-label">num_pratica</td><td>{{:num_pratica}}</td></tr>' +
				'	<tr><td class="field-label">statopratica</td><td>{{:statopratica}}</td></tr>' +
				'	<tr><td class="field-label">tipologia</td><td>{{:tipologia}}</td></tr>' +
				'	<tr><td class="field-label">richiedente</td><td>{{:richiedente}}</td></tr>' +
				'	<tr><td class="field-label">altri_richiedenti</td><td>{{:altri_richiedenti}}</td></tr>' +
				'	<tr><td class="field-label">responsabile_proced</td><td>{{:responsabile_proced}}</td></tr>' +
				'	<tr><td class="field-label">oggetto</td><td>{{:oggetto}}</td></tr>' +
				'	<tr><td class="field-label">tipologia_procedime</td><td>{{:tipologia_procedime}}</td></tr>' +
				'	<tr><td class="field-label">localita</td><td>{{:localita}}</td></tr>' +
				'	<tr><td class="field-label">impresa</td><td>{{:impresa}}</td></tr>' +
				'	<tr><td class="field-label">progettista</td><td>{{:progettista}}</td></tr>' +
				'	<tr><td class="field-label">diretore_lavori</td><td>{{:diretore_lavori}}</td></tr>' +
				'	<tr><td class="field-label">resp_sicurezza</td><td>{{:resp_sicurezza}}</td></tr>' +
				'	<tr><td class="field-label">collaudatore</td><td>{{:collaudatore}}</td></tr>' +
				'	<tr><td class="field-label">foglio</td><td>{{:foglio}}</td></tr>' +
				'	<tr><td class="field-label">particella</td><td>{{:particella}}</td></tr>' +
				'	<tr><td class="field-label">sub</td><td>{{:sub}}</td></tr>' +
				'	<tr><td class="field-label">sup_asservita</td><td>{{:sup_asservita}}</td></tr>' +
				'	<tr><td class="field-label">sup_catastale</td><td>{{:sup_catastale}}</td></tr>' +
				'	<tr><td class="field-label">classe</td><td>{{:classe}}</td></tr>' +
				'	<tr><td class="field-label">rendita</td><td>{{:rendita}}</td></tr>' +
				'	<tr><td class="field-label">tarsu</td><td>{{:tarsu}}</td></tr>' +
				'	<tr><td class="field-label">definitivo</td><td>{{:definitivo}}</td></tr>' +
				'	<tr><td class="field-label">note</td><td>{{:note}}</td></tr>' +
				'	</tbody>' +
				'</table><hr/>');

			$listItem = $.templates('<tr>' +
				'<td  style="padding-bottom: 2px; padding-right:1rem;">' +
				'	<button type="button" class="btn btn-sm btn-primary zoom-particella" data-foglio="{{:foglio}}" data-particella="{{:particella}}"><span class="glyphicon glyphicon-search"></span></button>' +
				'</td>' +
				'<td style="padding-right:1rem;">Fg. {{:foglio}} n. {{:particella}}</td>' +
				'<td><small>{{:valore}}</small></td>' +
				'</tr>');
		}

		function setButtonInfoHandle() {
			$(document).on('click', 'button.btn-info-sued', function () {
				var terna = $(this).data('terna');
				var livello = $(this).data('livello');
				var catastali = terna.split('|');

				callWebMethod(page_Url + '/SUED_EstraiInfo', {
					Comune: catastali[0],
					Foglio: catastali[1],
					Numero: catastali[2]
				}, function (data) {

					var dbr = JSON.parse(data.d);

					if (dbr.ErrorMessage != '') {
						BootstrapDialog.alert(dbr.ErrorMessage);
					}
					else {

						var $div = $('<div style="max-height:350px;overflow:auto;">');

						$.each(dbr.DataTable, function () {
							$div.append($info.render(this));
						});

						BootstrapDialog.show({
							title: 'NR PRATICHE ' + dbr.DataTable.length,
							message: $div,
							buttons: [
								{
									label: 'Chiudi',
									action: function (dialog) {
										dialog.close();
									}
								}]
						});

					}




				});

			});
		}

		function createTOC(groupLayerId, layerInfos) {

			for (var i = 0; i < layerInfos.length; i++) {
				var info = layerInfos[i];



				if (info.parentLayerId == groupLayerId) {
					_groupLayerId = info.parentLayerId;
					_tocLayers.push({ layer: info, title: info.name }); // Per la TOC solo i livelli nel GroupLayer Tematismi
				}

			}
			_visibleLayers = app.visibleLayers.slice();//app.operationalMap.visibleLayers.slice();// conservo valore dei livelli visibile allo startup

			_tocLayers.sort(function (a, b) {
				var n1 = a.layer.name.toUpperCase(), n2 = b.layer.name.toUpperCase();
				return n1 > n2 ? 1 : n2 > n1 ? -1 : 0; // Attenzione MAIUSCOLE/minuscole
			});


			var $div = $('<div class="form-group">');
			$div.append('<label class="small">SELEZIONA LIVELLO DA VISUALIZZARE</label>');
			var $selectLayer = $('<select class="form-control input-sm select-layer">');

			$selectLayer.append('<option value="">NESSUNO</option>').change(function(){
				refreshMap();
				loadData();
			});

			$.each(_tocLayers, function () {
				$selectLayer.append('<option value="' + this.layer.id + '">' + this.layer.name + '</option>');
			});

			$div.append($selectLayer);

			$.fn.delayBind = function (type, data, func, timeout) {
				if ($.isFunction(data)) {
					timeout = func;
					func = data;
					data = undefined;
				}

				var self = this,
					wait = null,
					handler = function (e) {
						clearTimeout(wait);
						wait = setTimeout(function () {
							func.apply(self, [$.extend({}, e)]);
						}, timeout);
					};

				return this.bind(type, data, handler);
			};

			var $searchBox = $('<input type="text" class="form-control input-sm searchBox" placeholder="Ricerca libera..." title="Filtra" style="margin:0.5em 0" />');
			$searchBox.delayBind("keyup", function (e) {
				var words = $(this).val().toLowerCase().split(" ");
				$('tbody tr', '.searchable').each(function () {
					var s = $(this).html().toLowerCase().replace(/<.+?>/g, "").replace(/\s+/g, " "),
						state = 0;
					$.each(words, function () {
						if (s.indexOf(this) < 0) {
							state = 1;
							return false; // break $.each()
						}
					});

					if (state) {
						$(this).hide();
					} else {
						$(this).show();
					}
				});
			}, 300);

			$div.append($searchBox);

			

			var $lst = $('<div style="position:fixed; bottom: 0;overflow:auto; top: 190px; margin-bottom: 50px; width:275px; background-color: #fff; border: 1px solid #eee;">');
			$div.append($lst);

			$sidebar.setContent('<p>SUED<br/><small>Aggiornato il ' + _sued_last_update + '</small></p>', $div);

			$div.on('click', '.zoom-particella', function () {
				var where = " foglio = '" + $(this).data('foglio').trim() + "' AND numero = '" + $(this).data('particella').trim() + "'";
				app.queryFeatures(app.enumLayers.PARTICELLE.id, where, function (features) {
					
						app.zoomToFeatures('', features);

						app.highlightFeatures(features);
					
				});
			});

			function refreshMap() {
				var visibleLayersId = _visibleLayers.slice();

				var layerId = $selectLayer.val();
				if (layerId != '') {
					//visibleLayersId.push(_groupLayerId);
					visibleLayersId.push(parseInt(layerId));
				}
				
				//visible.sort();
				app.operationalMap.setVisibleLayers(visibleLayersId);
				app.legend.refresh();
			}

			function loadData() {
				$lst.empty();
				var livello_pe = $selectLayer.find('option:selected').text();
				if (livello_pe == 'NESSUNO') {
					
					return;
				}
				callWebMethod(page_Url + '/SUED_EstraiParticelle', {
					LIVELLO_PE: livello_pe
				}, function (data) {

					var dbr = JSON.parse(data.d);

					if (dbr.ErrorMessage != '') {
						$lst.append('<p class="text-danger">' + dbr.ErrorMessage + '</p>');
					}
					else {
						var $tbl = $('<table class="table table-condensed table-bordered searchable">');
						var $tbody = $('<tbody>');
						$tbl.append($tbody);
						$.each(dbr.DataTable, function () {
							$tbody.append($listItem.render(this));
						});
						$lst.append($tbl);
						
					}




				});
			}
		}

		
	});


