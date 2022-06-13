<%@ Page Title="" Language="vb" AutoEventWireup="false" MasterPageFile="~/WebForms/Page.Master" CodeBehind="Progetti.aspx.vb" Inherits="PITER.Progetti" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadPlaceHolder" runat="server">
    <script src="js/progetti.js"></script>
    <style>
        .calcite-maps {
            overflow:auto;
            -ms-overflow-style:auto;
        }
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="BodyPlaceHolder" runat="server">
    <div class ="container" style="position:relative; top:100px; overflow:auto;">
        <%If PITER.IdentityHelper.IsAdmin Then%>
        <asp:HyperLink ID="HyperLink1" runat="server" NavigateUrl="~/WebForms/Gestione/Utenti">Gestione utenti</asp:HyperLink>
        <%End If %>
        <hr />
		<table class="table table-condensed">
			<thead>
				<tr>
					<th>CODICE</th>
					<th>DESCRIZIONE</th>
					<th colspan="2">SERVIZIO</th>
				</tr>
			</thead>
			<tbody>
				<asp:Repeater ID="Repeater1" runat="server">
            <ItemTemplate>
                <tr>
					<td><%#Eval("CODICE") %></td>
					<td><%#Eval("DESCRIZIONE") %></td>
					<td><%#Eval("SERVIZIO") %></td>
					<td>
						<button type="button"  data-codice="<%#Eval("CODICE") %>" data-servizio="<%#Eval("SERVIZIO") %>" data-descrizione="<%#Eval("DESCRIZIONE") %>" data-viewer="<%=Page.ResolveUrl("~/WebForms/Mappa/Viewer") %>" class="btn btn-warning btn-outside"><span class="glyphicon glyphicon-globe"></span> OUTSIDE</button>
					</td>
                </tr>

                        <%If PITER.IdentityHelper.IsAdmin Then%>
                       
                        
                        <%Else %>
                            
                        <%End If %>
                        
                        
                </div>
            </ItemTemplate>
        </asp:Repeater>
			</tbody>
		</table>
        


        
    </div>
    


   
</asp:Content>
