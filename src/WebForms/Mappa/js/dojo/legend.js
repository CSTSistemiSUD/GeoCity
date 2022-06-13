define([
    "esri/request",
    "dojo/domReady!"],
    function (esriRequest) {
        "use strict";
        var _dynamicLayer;
        var _opened = 'glyphicon glyphicon-triangle-bottom';
        var _closed = 'glyphicon glyphicon-triangle-right';

        var _currentIndex = 0;
        var _legend = [];
        

        $(document).on('click', 'a.legend-item', function (e) {
            e.stopPropagation(); // prevent links from toggling the nodes
            var $a = $(this);
            $(this).closest('li').children('ul').slideToggle(function () {
                if ($a.data('state') == 'opened') {
                    $a.data('state', 'closed');
                    $a.prev('span').removeClass(_opened).addClass(_closed);
                }
                else {
                    $a.data('state', 'opened');
                    $a.prev('span').removeClass(_closed).addClass(_opened);
                }
            });
        });

        $(document).on('change', '.chk-legend-item', function (e) {

            updateCheckBoxStatus(this);
            updateLayerVisibility();
        });

        //$(document).on('change', 'input:checkbox[class=GroupCheck]', function (e) {
        //    var checks = $(this).closest('li').children('ul').find('input:checkbox[class=LayerCheck]');
        //    var checked = $(this).prop('checked');
        //    $(checks).each(function (i, c) {
        //        $(c).prop('checked', checked);
        //    });
        //    updateLayerVisibility();
        //});

        return {
            build: function (dynamicLayer, containerId) {
                _currentIndex = 0;
                _legend = [];
                $(containerId).addClass('legend-container');

                _dynamicLayer = dynamicLayer;
                

                var legendRequest = esriRequest({
                    url: dynamicLayer.url + "/legend",
                    content: { f: "pjson" },
                    handleAs: "json",
                    callbackParamName: "callback"
                });

                legendRequest.then(
                    function (response) {
                                                                                        // The requested data
                        // **** TREEVIEW
                        
                        $.each(response.layers, function (i, layer) {
                            _legend[layer.layerId] = [];// crea una hashtable che ha come key l'id del layer
                            $.each(layer.legend, function (j, item) {
                                _legend[layer.layerId].push(item);// il valore dell'item della hahstable è un array di oggetti legend
                            });
                        });
                        // Loop su tutti i livelli del map service
                        var $tree = $('<ul class="legend-tree">');

                        for (_currentIndex = 0; _currentIndex < dynamicLayer.layerInfos.length; _currentIndex++) {
                            var info = dynamicLayer.layerInfos[_currentIndex];

                            var $li = getLegendSwatch(info, _legend[info.id]);
                                                        

                            $tree.append($li);


                        }
                        
                        $(containerId).empty().append($tree);
                        // Bisogna settare lo stato iniziale dei check sui GroupLayer (checked, unchecked o indeterminate)
                        // Per fare questo simulo l'evento change su tutti i checkbox GroupLauer
                        $('.chk-group-layer', $tree).each(function (i, checkbox) {
                            setGroupLayerCheck(checkbox);
                        });
                        
                    },
                    function (err) {
                        alert(err);
                    }
                );
            }
        }

        function getLegendSwatch(info, legend) {
            var $li = $('<li>');
            


            if (info.subLayerIds) {
                $li
                    .append('<input type="checkbox" class="chk-legend-item chk-group-layer">&nbsp;')
                    .append('<span class="' + _closed + '"></span>&nbsp;<a href="#" class="legend-item" data-state="closed">' + info.name + ' [Gruppo]</a>');

                var $subLayers = $('<ul style="display:none;">');
                $.each(info.subLayerIds, function (index, subLayerId) {
                    var subLayer = _dynamicLayer.layerInfos[subLayerId];
                    $subLayers.append(getLegendSwatch(subLayer, _legend[subLayer.id]));
                    _currentIndex = subLayer.id;
                });
                $li.append($subLayers);
            }
            else {
                $li
                    .append('<input type="checkbox" ' + (info.defaultVisibility ? "checked=checked" : "") + ' value="' + info.id + '" class="chk-legend-item chk-layer">&nbsp;')
                    .append('<span class="' + _closed + '"></span>&nbsp;<a href="#" class="legend-item" data-state="closed">' + info.name + '</a>');
                    //.append(info.name);

                var $legend = $('<ul style="display:none;">');
                $.each(legend, function (j, item) {
                    $legend.append('<li><img alt="' + item.label + '" src="data:' + item.contentType + ';base64,' + item.imageData + '" height="' + item.height + '" width="' + item.width + '">&nbsp;' + item.label + '</li>');

                });

                $li.append($legend);
            }

           
            return $li;
        }

        

        function updateLayerVisibility() {
            var visible = [];
            $('.chk-layer:checked').each(function (i, checkbox) {
                
                visible.push($(checkbox).attr('value'));
                
            });
            //if there aren't any layers visible set the array to be -1
            if (visible.length === 0) {
                visible.push(-1);
            }
            _dynamicLayer.setVisibleLayers(visible);

        }

        function updateCheckBoxStatus(checkbox) {
            var checked = $(checkbox).prop("checked"),
                container = $(checkbox).parent(),
                siblings = container.siblings();

            container.find('input[type="checkbox"]').prop({
                indeterminate: false,
                checked: checked
            });

            function checkSiblings(el) {

                var parent = el.parent().parent(),
                    all = true;

                el.siblings().each(function () {
                    return all = ($(this).children('input[type="checkbox"]').prop("checked") === checked);
                });

                if (all && checked) {

                    parent.children('input[type="checkbox"]').prop({
                        indeterminate: false,
                        checked: checked
                    });

                    checkSiblings(parent);

                } else if (all && !checked) {

                    parent.children('input[type="checkbox"]').prop("checked", checked);
                    parent.children('input[type="checkbox"]').prop("indeterminate", (parent.find('input[type="checkbox"]:checked').length > 0));
                    checkSiblings(parent);

                } else {

                    el.parents("li").children('input[type="checkbox"]').prop({
                        indeterminate: true,
                        checked: false
                    });

                }

            }

            checkSiblings(container);
        }

        function setGroupLayerCheck(group) {
            var checks = $(group).closest('li').children('ul').find('input:checkbox');
            if (checks.length > 0) {
                updateCheckBoxStatus(checks[0]);
            }
        }
    });


