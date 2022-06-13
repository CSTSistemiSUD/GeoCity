define([],
    function (Locator, on, domConstruct) {
        "use strict";

        
        //this.locator = null;
        //this.clickedMapPoint = null;
        //var self = this;

		return {
			inizializza: function () {

				// Aggiunge un bottone nella toolbar secondaria per aprire il pannello con l'elenco dei vincoli
				var $btn = $('<button type="button" class="btn btn-sm btn-default"><span class="glyphicon glyphicon-folder-close"></span> Catasto</button>');
				$btn.click(function () {
					ricercaTitolari();
				});
				$(app.mainToolbar).prepend($btn);

				$(document).on('click', 'button.btn-info-catasto, button.btn-zoom-catasto', function () {
					var terna = $(this).data('terna');
					var livello = $(this).data('livello');
					var catastali = terna.split('|');
					
					if ($(this).hasClass('btn-info-catasto')) {
						infoImmobile(catastali, livello);
					}
					else if ($(this).hasClass('btn-zoom-catasto')) {
						var fg = catastali[1].replace(/^0+/, '');
						var num = catastali[2].replace(/^0+/, '');

						var where = " foglio = '" + fg.trim() + "' AND numero = '" + num.trim() + "'";
						var layerId = livello == app.enumLayers.FABBRICATI.name ? app.enumLayers.FABBRICATI.id : app.enumLayers.PARTICELLE.id;
						

						app.queryFeatures(layerId, where, function (features) {
							app.zoomToFeatures('', features);

							app.highlightFeatures(features);
						});
					}
				});
			}
		};

		function caricaImmobili(codice_comune, foglio, numero, tipo_immobile, id_soggetto, onSuccess, onError) {
			
			callWebMethod(ws_Catasto + '/CaricaImmobili', {
				p_PRJFOLDER: codice_comune,
				IDSogg: id_soggetto,
				Foglio: foglio,
				Numero: numero,
				TipoImmobili: tipo_immobile
			}, function (data) {

				var dbr = JSON.parse(data.d);

				if (dbr.ErrorMessage == '') {
					if ($.isFunction(onSuccess)) {
						onSuccess(dbr);
					}
				}
				else {
					if ($.isFunction(onError)) {
						onError(dbr.ErrorMessage);
					}
				}

				
				

			});
		}

		function caricaDettaglioImmobile(codice_comune, id_immobile, tipo_immobile, indice_scheda, onSuccess) {
			callWebMethod(ws_Catasto + '/DettaglioImmobile', {
				p_PRJFOLDER: codice_comune,
				IdImmobile: id_immobile,
				TipoImmobile: tipo_immobile,
				IndiceScheda: indice_scheda
			}, function (data) {

				
				if ($.isFunction(onSuccess)) {
					onSuccess(data.d);
				}

			});
		}

		function infoImmobile(catastali, livello) {
			var codice_comune = catastali[0];
			var foglio = catastali[1];
			var numero = catastali[2];
			BootstrapDialog.show({
				title: 'Attendere prego...',
				message: $('<div></div>').load(app_url + 'WebForms/Mappa/html/Catasto_Info.html?' + Math.random()),
				closable: true,
				closeByBackdrop: false,
				closeByKeyboard: false,
				buttons: [
					{
						label: 'Chiudi',
						action: function (dialogRef) {
							dialogRef.close();
						}
					}
				],
				onshown: function (dialog) {

					var $wait = dialog.getModalContent().find('div.spinner');
					var $selIdentificativo = $('#selIdentificativo');
					var tabIndex = 1;
					$selIdentificativo.change(function () {
						caricaDettaglioImmobile(codice_comune, $selIdentificativo.val(), livello, tabIndex, mostraInfo);
					});

					$('#infoCatastoTabs a').click(function () {
						tabIndex = $(this).data('index');
						caricaDettaglioImmobile(codice_comune, $selIdentificativo.val(), livello, tabIndex, mostraInfo);
					});

					caricaImmobili(
						codice_comune,
						foglio,
						numero,
						livello,
						'',
						function (dbr) {
							var id_immobile = -1;
							dbr.ReturnDataset.PARTICELLE = dbr.ReturnDataset.PARTICELLE || [];
							dbr.ReturnDataset.FABBRICATI = dbr.ReturnDataset.FABBRICATI || [];
							if (dbr.ReturnDataset.PARTICELLE.length == 0 && dbr.ReturnDataset.FABBRICATI.length == 0) {
								$wait.hide();
								dialog.setTitle('Nessun risultato');
								dialog.setMessage('Nessun risultato');
							}
							else {
								switch (livello) {
									case app.enumLayers.PARTICELLE.name:
										id_immobile = dbr.ReturnDataset.PARTICELLE[0].idParticella;

										$.each(dbr.ReturnDataset.PARTICELLE, function () {
											$selIdentificativo.append('<option value="' + this.idParticella + '">' + 'FG. ' + this.foglio + ' NUM. ' + this.numero + '</option>');
										});

										$('#infoCatastoTab2').attr('aria-controls', 'Porzioni').html('Porzioni');

										break;
									case app.enumLayers.FABBRICATI.name:
										id_immobile = dbr.ReturnDataset.FABBRICATI[0].idImmobile;
										

										$.each(dbr.ReturnDataset.FABBRICATI, function () {
											//var lbl = 'SUB ' + this.subalterno;
											//if (this.categoria != '') lbl += ' - CATEGORIA ' + this.categoria;
											//if (this.classe != '') lbl += ' - CLASSE ' + this.classe;
											

											$selIdentificativo.append('<option value="' + this.idImmobile + '">' + 'FG. ' + this.foglio + ' NUM. ' + this.numero + ' SUB ' + this.subalterno + '</option>');
										});

										$('#infoCatastoTab2').attr('aria-controls', 'Identificativi').html('Identificativi');

										break;
								}
								dialog.setTitle(livello);
								

								caricaDettaglioImmobile(codice_comune, id_immobile, livello, tabIndex, mostraInfo);
							}
						},
						function (errMessage) {
							dialog.setTitle('Errore');
							dialog.setMessage('<div class="alert alert-danger" role="alert">' + errMessage + '</div>');
						});

					function mostraInfo(content) {
						$('#infoCatastoContent').html(content);
						$wait.hide();
						$('#divInfoCatasto').show();
					}


				}
			});
		}

		function ricercaTitolari() {
			var $btnShowInfo, $dialogInfo;
			var $dialogContent;

			callWebMethod(ws_Catasto + '/InfoAggiornamento', {
				p_PRJFOLDER: _codice,
				testo: ''
			}, function (data) {

				var dbr = JSON.parse(data.d);
					var infos = [];
				if (dbr.ErrorMessage == '') {
					// carica tabella

					$.each(dbr.DataTable, function () {
						var $p = $('<p>');
						$p.append(
							'Esportazione: <strong>' + this.descrizione + '</strong>'+
							' (num. record: ' + this.numRecord +
							', data selezione: ' + this.dataSelezione +
							', data elaborazione: ' + this.dataElaborazione +
							', periodo: ' + this.periodo + ')'
						);
						infos.push($p);
					});

				}
				else {
					
				}

					BootstrapDialog.show({
						title: 'Ricerca titolarità',
						message: $('<div></div>').load(app_url + 'WebForms/Mappa/html/Catasto_Ricerca.html?' + Math.random()),
						size: BootstrapDialog.SIZE_WIDE,
						closable: true,
						closeByBackdrop: false,
						closeByKeyboard: false,
						buttons: [

							{
								label: 'Riduci',
								cssClass: 'btn-warning',
								action: function (dialogRef) {
									if ($(this).html() == 'Riduci') {
										$(this).html('Espandi');
									}
									else {
										$(this).html('Riduci');
									}
									$dialogContent.toggle();
								}
							},
							{
								label: 'Chiudi',
								action: function (dialogRef) {
									dialogRef.close();
								}
							}
						],
						onshown: function (dialog) {
							$btnShowInfo = dialog.getModalContent().find('[data-id="btnShowInfo"]');
							$dialogInfo = dialog.getModalContent().find('[data-id="dialogInfo"]');
							$dialogContent = dialog.getModalContent().find('[data-id="dialogContent"]');

							$btnShowInfo.click(function () {
								$dialogInfo.toggle();
							})

							$.each(infos, function () {
								$dialogInfo.append(this);
							});

							$('#btnRicercaCatasto').click(function () {

								if ($('#txtRicercaCatasto').val() == '') return;
								$('#tblRicercaCatasto').empty();
								callWebMethod(ws_Catasto + '/RicercaTitolari', {
									p_PRJFOLDER: _codice,
									testo: $('#txtRicercaCatasto').val()
								}, function (data) {

									var dbr = JSON.parse(data.d);

									if (dbr.ErrorMessage == '') {
										// carica tabella
										$('#tblRicercaCatasto').append($('#tmplRigaRicercaCatasto').render(dbr.DataTable));
									}
									else {
										alertDanger(dbr.ErrorMessage);
									}



								});
							});
							//
							dialog.getModalContent().on('click', 'a.toggle-next-row', function () {
								var $btn = $(this);
								var IdSoggetto = $btn.attr('data-id');
								var $row = $btn.closest('tr');
								var $nextRow = $row.next('tr');
								var $div = $nextRow.children('td').find('.risultati');
								if ($nextRow.is(':visible')) {
									$nextRow.hide();
									$btn.html('<span class="glyphicon glyphicon-triangle-right"></span>');
								}
								else {
									$nextRow.show();
									$btn.html('<span class="glyphicon glyphicon-triangle-bottom"></span>');
									// Carica immobili

									caricaImmobili(_codice, '', '', '', IdSoggetto, function (dbr) {
										if (dbr.ErrorMessage == '') {
											// carica tabella
											$div.empty().append($('#tmplTabelleImmobili').render(dbr.ReturnDataset));
										}
										else {
											alertDanger(dbr.ErrorMessage);
										}
									});
								}
							});
						}
					});


			});

			
		}
	});

