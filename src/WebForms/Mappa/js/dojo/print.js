define([
    "esri/request", "esri/tasks/PrintTask", "esri/tasks/PrintParameters", "esri/tasks/PrintTemplate", "esri/tasks/Geoprocessor", "esri/tasks/LegendLayer", "dojo/_base/array",
    "dojo/domReady!"],
    function (esriRequest, PrintTask, PrintParameters, PrintTemplate, Geoprocessor, LegendLayer, arrayUtils) {
        this.exportMapGPServerURL = '';

        this.printOptionsTitle = null;
        this.printOptionsDpi = null;
        this.printOptionsLayoutTemplate = null;
        this.printBtn = null;
        this.printProcess = null;

        this.waitMessage = null;
        this.waitProgress = null;
        this.legendLayerIds = [];

        this.options = {
            authorText: '',
            copyrightText: ''
        }
            
        var self = this;

        return {
            startup: function (containerDiv, legendLayerIds, exportMapGPServerURL, options) {
                self.legendLayerIds = legendLayerIds;
                self.options = options;
                self.exportMapGPServerURL = exportMapGPServerURL;

                createPrintForm(containerDiv);

                var printInfo = esriRequest({
                    "url": exportMapGPServerURL,
                    "content": { "f": "pjson" }
                });
                printInfo.then(handlePrintInfo, handleError);
            }
        }

        function createPrintForm(containerDiv) {
            var printForm = '';

            printForm += '<table class="fields">';
            printForm += '     <tbody>';
            printForm += '         <tr>';
            printForm += '             <td class="field-label">Titolo</td>';
            printForm += '             <td><input type="text" class="printOptionsTitle form-control input-sm" placeholder="Titolo"></td>';
            printForm += '         </tr>';
            printForm += '         <tr>';
            printForm += '             <td class="field-label">Risoluzione</td>';
            printForm += '             <td>';
            printForm += '                 <select class="printOptionsDpi form-control input-sm">';
            printForm += '                     <option value="96" selected>96 DPI</option>';
            printForm += '                     <option value="150">150 DPI</option>';
            printForm += '                     <option value="300">300 DPI</option>';
            printForm += '                 </select>';
            printForm += '             </td>';
            printForm += '         </tr>';
            printForm += '         <tr>';
            printForm += '              <td class="field-label">Template</td>';
            printForm += '              <td><select class="printOptionsLayoutTemplate form-control input-sm"></select></td>';
            printForm += '         </tr>';
            printForm += '     </tbody>';
            printForm += ' <table>';
            printForm += ' <hr />';
            printForm += ' <div class="text-center">';
            printForm += '     <a href="#"  class="printBtn btn btn-primary btn-sm"><span class="glyphicon glyphicon-print"></span> Esporta</a>';
            printForm += ' </div>';
            printForm += ' <br />';
            printForm += ' <div class="printProcess"></div>';

            $(containerDiv).append(printForm);
            self.printOptionsTitle = $(containerDiv).find('.printOptionsTitle').eq(0);
            self.printOptionsDpi = $(containerDiv).find('.printOptionsDpi').eq(0);
            self.printOptionsLayoutTemplate = $(containerDiv).find('.printOptionsLayoutTemplate').eq(0);
            self.printBtn = $(containerDiv).find('.printBtn').eq(0);
            self.printProcess = $(containerDiv).find('.printProcess').eq(0);
        }

        function handlePrintInfo(resp) {
            
            var printTask = new PrintTask(self.exportMapGPServerURL, { async: true });

            var gp = new Geoprocessor(self.exportMapGPServerURL);

            var layoutTemplate, templateNames, mapOnlyIndex;

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

            

            arrayUtils.forEach(templateNames, function (templateLabel, index) {

                self.printOptionsLayoutTemplate.append(
                    $('<option>').val(templateLabel).text(templateLabel)
                );


            });

            self.printOptionsTitle.val('');

            self.printBtn.click(function () {

                // Controllare self.legendLayerIds

                var templateName = self.printOptionsLayoutTemplate.find("option:selected").val();
                var template = new PrintTemplate();
                template.layout = template.label = templateName;
                template.format = "PDF";

                var title = self.printOptionsTitle.val();
                var dpi = self.printOptionsDpi.val();
                exportLayout(template, title, dpi);
            });

            function exportLayout(template, title, dpi) {
                
                disableButtons();

                self.printProcess.empty().append('<p class="WaitMessage" style="text-align:center;"></p>' +
                    '<div class="progress">' +
                    '<div class="WaitProgress progress-bar progress-bar-info active" role="progressbar" ' +
                    'aria-valuenow="0" aria-valuemin="0" aria-valuemax="100" style="width: 0%"></div>' +
                    '</div>');

                self.waitProgress = self.printProcess.find('.WaitProgress').eq(0);
                self.waitMessage = self.printProcess.find('.WaitMessage').eq(0);

                var jsonRequest = adjustRequest(template, title, dpi);
                // Avvia processo
                var gpParams = {
                    Web_Map_as_JSON: jsonRequest,
                    Format: template.format,
                    Layout_Template: template.layout
                };
                gp.submitJob(gpParams, function (jobinfo) {
                    //gpJobComplete
                    gp.getResultData(jobinfo.jobId, "Output_File", function (result) {
                        

                        //result.value.url punta a localhost;
                        var path = result.value.url.toLowerCase().indexOf('/arcgis');
                        var newurl = result.value.url.substring(path);
                        var downloadUrl = '';
                        if (location.hostname == 'localhost') {
                            downloadUrl = location.protocol + "//localhost" + newurl;
                        }
                        else {
                            downloadUrl = location.protocol + "//" + location.host + newurl;
                        }

                        var downloadLink = '<a href="' + downloadUrl + '" class="btn btn-success btn-sm" target="_blank">' +
                            '<span class="glyphicon glyphicon-download-alt"></span> Download</a>';
                        

                        msgUpdate(downloadLink, 100, false);

                        enableButtons();

                    }, function (error) {
                        msgUpdate('<div class="alert alert-danger" role="alert">' + error + '</div>', 100, true);
                        enableButtons();
                    });

                }, function (jobinfo) {
                    //gpJobStatus
                    switch (jobinfo.jobStatus) {
                        case 'esriJobSubmitted':
                            msgUpdate('<div class="alert alert-info" role="alert">Avvio processo di esportazione...</div>', 30, false);
                            break;
                        case 'esriJobExecuting':
                            msgUpdate('<div class="alert alert-info" role="alert">Esportazione...</div>', 60, false);
                            break;
                        case 'esriJobSucceeded':
                            msgUpdate('<div class="alert alert-success" role="alert">Esportazione completata con successo...</div>', 100, false);
                            break;
                        case 'esriJobFailed':
                            msgUpdate('<div class="alert alert-danger" role="alert">Esportazione fallita...</div>', 100, true);
                            break;
                    }
                }, function (error) {
                    //gpJobFailed
                    msgUpdate('<div class="alert alert-danger" role="alert">' + error + '</div>', 100, true);
                    enableButtons();
                });

                function msgUpdate(msg, progress, hasError) {
                    self.waitProgress.animate({
                        width: progress + "%"
                    }, 500, function () {
                        self.waitMessage.empty().append(msg);
                    });
                    if (progress == 100) {
                        if (hasError) {
                            self.waitProgress.removeClass('progress-bar-info').addClass('progress-bar-danger');
                        } else {
                            self.waitProgress.removeClass('progress-bar-info').addClass('progress-bar-success');
                        }
                    }
                }

                function enableButtons() {
                    self.printBtn.prop('disabled', false);
                }
                function disableButtons() {
                    self.printBtn.prop('disabled', true);
                }
                /*  www.spatialtimes.com/2015/04/arcgis-javascript-api-printing-group-layers/
                *  "You’re not hallucinating – sometimes the map you see in your ArcGIS JavaScript application doesn’t 
                *  look like the one you printed via the Print Service. 
                *  There are many reasons why this might happen, and one is related to Group Layers."
                */
                function adjustRequest(template, title, dpi) {


                    var params = new PrintParameters();
                    params.map = app.map;
                    params.template = template;

                    var jsonRequest = printTask._getPrintDefinition(app.map, params);

                    

                    // la proprietà operationalLayers di legendOptions deve contenere un riferimento a ogni layer presente in mappa
                    // altrimenti non stampa la legenda
                    var operationalLayers = [];
                    $.each(app.map.layerIds, function (i, layerId) {
                        if (self.legendLayerIds.indexOf(layerId) < 0) {
                            operationalLayers.push({ id: layerId, subLayerIds: [] });
                        }
                        else {
                            var ol = app.map.getLayer(layerId);

                            operationalLayers.push({ id: layerId, subLayerIds: ol.visibleLayers.slice() });
                        }
                    });

                    jsonRequest.layoutOptions = {
                        "titleText": title,
                        "authorText": self.options.authorText,
                        "copyrightText": self.options.copyrightText,
                        "scaleBarOptions":
                        {
                            "metricUnit": "meters",
                            "metricLabel": "m",
                            "nonMetricUnit": "miles",
                            "nonMetricLabel": "mi"
                        },
                        "legendOptions":
                        {
                            "operationalLayers": operationalLayers.slice()
                            
                        }
                    }

                    jsonRequest.exportOptions = {
                        "dpi": dpi,
                        "outputSize": [500, 500]// valido solo per MAP_ONLY
                    };

                    return JSON.stringify(jsonRequest);
                }

                function findLayerInfo(mapService, subLayerId) {
                    var infos = mapService.layerInfos;
                    for (var i = 0, il = infos.length; i < il; i++) {
                        if (infos[i].id == subLayerId) {
                            return infos[i];
                        }
                    }
                }
            }

        }

        function handleError(err) {
            console.log("Something broke: ", err);
        }

    });






    