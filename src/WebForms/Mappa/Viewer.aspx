<%@ Page Title="" Language="vb" AutoEventWireup="false" MasterPageFile="~/WebForms/Page.Master" CodeBehind="Viewer.aspx.vb" Inherits="PITER.Viewer" %>
<asp:Content ID="Content1" ContentPlaceHolderID="HeadPlaceHolder" runat="server">
    <link href="css/viewer-bundle.min.css" rel="stylesheet" />
    <link href="<%=ResolveUrl("~/Content/daterangepicker.min.css") %>" rel="stylesheet" />

	
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="BodyPlaceHolder" runat="server">
    <%--Seconda nav--%>
                     <div id="layerList"></div>
    <div id="mainToolbar" class="navbar navbar-inverse navbar-static-top" role="navigation" style="left:0; top:50px; background-color:#fff; border:1px solid #eee; padding-left:50px; padding-top:10px;">	
	   
    </div>
        <%--Fine --%>
        <!-- Map Container  -->

    <div class="calcite-map" id="mapContainer">
        <div class="sidebar sidebar-left sidebar-open calcite-sidebar">
           
			<div class="btn-group-vertical" role="toolbar" aria-label="">
                <button id="zoomIn" title="Ingrandisci" type="button" class="btn btn-sm btn-default"><span class="glyphicon glyphicon-plus"></span></button>
                <button id="zoomFull" title="Vista iniziale" type="button" class="btn btn-sm btn-primary "><span class="glyphicon glyphicon-home"></span></button>
                <button id="zoomOut" title="Riduci" type="button" class="btn btn-sm btn-default "><span class="glyphicon glyphicon-minus"></span></button>
            </div>
            <div class="clearfix"></div>
            <br />
            
            <div id="sidebarToolbar" class="btn-group-vertical" role="toolbar" aria-label="">
                
              
            </div>
            
        </div>
        <div id="loadingImg">
            <div class="spinner spinner-md"></div>
        </div>
        
       

        <div id="logDiv" style="position:absolute; left:75px; bottom:0; z-index:10000;"></div>
        <div id="mapViewDiv" class="calcite-map-absolute"></div>
    </div><!-- /.container -->

 
    <!-- Panel Container -->

    <div id="panelContainer" class="calcite-panels panel-group calcite-panels-right calcite-bg-light calcite-text-dark" role="tablist" aria-multiselectable="true">

    </div> 
  
   <!-- /.calcite-panels -->
        




        <script>
            var _minute = 60 * 1000;
            
            
            var enumViewerType = {
                OUTSIDE: 'outside',
                INSIDE: 'inside'
            }

            var app = {}, userProfile;
            var app_path = "<%=IIf(Request.ApplicationPath = "/", "/", Request.ApplicationPath + "/") %>";
            var app_url = location.protocol + "//" + location.host + app_path;
//            var package_path = app_url + "/WebForms/Mappa/js/dojo";

            
            <%
			Dim req As HttpRequest = HttpContext.Current.Request
			Dim appPath As String = IIf(req.ApplicationPath = "/", "", req.ApplicationPath)

			Dim package_path = New Uri(HttpContext.Current.Request.Url, String.Format("{0}/WebForms/Mappa/js/dojo", appPath)).AbsoluteUri.ToString()
			Dim image_path = New Uri(HttpContext.Current.Request.Url, String.Format("{0}/WebForms/Mappa/img", appPath)).AbsoluteUri.ToString()
            %>
            var package_path = '<%=package_path%>';
            var image_path = '<%=image_path%>';

            var dojoConfig = {
                async: true,
                packages: [{
                    name: "pkg",
                    location: package_path
                }]
            };


            



            <%  If HttpContext.Current.IsDebuggingEnabled Then
            %>
            dojoConfig.map = {
				"*": {
					"pkg/calcitemaps.min": "pkg/calcitemaps",
                    "pkg/viewern.min": "pkg/viewern" ,
                    "pkg/legend.min": "pkg/legend",
                    "pkg/print.min": "pkg/print",
                    "pkg/identify.min": "pkg/identify",
                    "pkg/export.min": "pkg/export",
                    "pkg/fc_vincoli.min": "pkg/fc_vincoli",
                "pkg/fc_catasto.min": "pkg/fc_catasto",
								"pkg/fc_base.min": "pkg/fc_base",
                    "pkg/sued.min": "pkg/sued"
                }
            }
            <%
            Else
            %>
            
            <%
            End If
            %>
				</script>
        
        
        <%--<%: Scripts.Render("~/bundles/viewer") %>--%>
        <script src="<%=ResolveUrl("~/Scripts/bootstrap-sidebar.js") %>"></script>
        <script src="<%=ResolveUrl("~/Scripts/daterangepicker.min.js") %>"></script>
        <script src="<%=ResolveUrl("~/Scripts/daterangepicker_it.min.js") %>"></script>
    
		<script src="js/proj4js-combined.min.js"></script>
        <script src="js/proj4js-EPSG.min.js"></script>
        <script src="https://js.arcgis.com/3.34/"></script>
        

        <script>
			
            require([
                "pkg/viewern.min",
                "dojo/domReady!"
			], function (Viewer) {
				
                Viewer.startup();
            });
        </script>
       

	<script type="text/x-jsrender" id="tmplPanelButton">
		<button type="button" class="btn btn-sm btn-default toggle-panel" style="margin-right:3px;" data-target="#panel{{:Id}}" aria-haspopup="true" title="{{:Title}}"><span class="{{:Icon}}"></span> {{:Label}}</button>
	</script>
    <script type="text/x-jsrender" id="tmplPanel">
		<div id="panel{{:Id}}" class="panel collapse">
            <div id="heading{{:Id}}" class="panel-heading" role="tab">
                <div class="panel-title">
                    <a class="panel-toggle collapsed" role="button" data-toggle="collapse" href="#collapse{{:Id}}" aria-expanded="false" aria-controls="collapse{{:Id}}"><i class="{{:Icon}}" aria-hidden="true"></i><span class="panel-label">{{:Title}}</span></a> 
                    <a class="panel-close" role="button" data-toggle="collapse" tabindex="0" href="#panel{{:Id}}"><span class="esri-icon esri-icon-close" aria-hidden="true"></span></a> 
                </div>
            </div>
            <div id="collapse{{:Id}}" class="panel-collapse collapse" role="tabpanel" aria-labelledby="heading{{:Id}}">
                <div class="panel-body" id="panelContent{{:Id}}">
                    
                </div>
                
            </div>

			{{if HasFooter}}
				<div class="panel-footer" id="panelFooter{{:Id}}">

				</div>
			{{/if}}
        </div>
    </script>
	             

</asp:Content>


