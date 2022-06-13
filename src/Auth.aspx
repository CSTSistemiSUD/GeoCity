<%@ Page Title="" Language="vb" AutoEventWireup="false" MasterPageFile="~/Site.Master" CodeBehind="Auth.aspx.vb" Inherits="PITER.Auth" %>
<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
	<script src="Scripts/utils.js"></script>

</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
	<div class="well-lg">
		<h1><asp:Label ID="lblResult" runat="server" ClientIDMode="Static"></asp:Label></h1>
	</div>

	<div style="display:none">
		<asp:CheckBox ID="CheckIsValid" runat="server" ClientIDMode="Static" />
	</div>
	<script>
        var postBackUrl = '<%=Page.ResolveUrl("~/WebForms/Mappa/Viewer") %>';
		$(document).ready(function () {
			
			if ($('#CheckIsValid').prop('checked')) {
				$('#lblResult').html("Reindirizzamento in corso...");
				submitForm(postBackUrl, [

				{ Name: 'PRJGUID', Value: _PRJGUID },
				{ Name: 'PRJITEMGUID', Value: _PRJITEMGUID },
                { Name: 'PRJDATABASE', Value: _PRJDATABASE },
                { Name: 'INSIDE', Value: _INSIDE },
                { Name: 'INSIDE_PLAN', Value: _INSIDE_PLAN },
                { Name: 'LIVELLO', Value: _LIVELLO },
                { Name: 'HOSTID', Value: _HOSTID },
                { Name: 'viewerType', Value: _VIEWER }
            ], 'post');
			}

            

             
        });
    </script>
</asp:Content>
