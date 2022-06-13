<%@ Control Language="vb" AutoEventWireup="false" CodeBehind="Address.ascx.vb" Inherits="PITER.Address" %>
<script type="text/javascript">
    var <%=Me.ID%>_Js = null;
    $(document).ready(function () {
        <%=Me.ID%>_Js = new AddressSelector('<%=Me.ClientID%>');
    });
        
</script>

<h5 id="Titolo" runat="server"><%=Me.Caption %></h5>
<div class="row">
    <div class="form-group form-group-sm col-xs-4">
        <label>NAZIONE</label>
        <asp:DropDownList ID="Nazione" runat="server" CssClass="form-control"></asp:DropDownList>
        <%--<select ID="Nazione" runat="server" class="form-control"></select>--%>
    </div>
    <div class="form-group form-group-sm col-xs-8">
        <label>PROVINCIA</label>
        <asp:HiddenField ID="Sigla" runat="server" ></asp:HiddenField>
        <asp:TextBox ID="Provincia" runat="server" CssClass="form-control"></asp:TextBox>
    </div>
</div>

<div class="row">
    <div class="form-group form-group-sm col-xs-12">
        <label>COMUNE</label>
        <asp:TextBox ID="Comune" runat="server" CssClass="form-control" ></asp:TextBox>
    </div>
</div>

<% If Not Me.IsBirthdayInfo Then %>
<div class="row">
    <div class="form-group form-group-sm col-xs-3">
        <label>CAP</label>
        <asp:TextBox ID="Cap" runat="server" CssClass="form-control"></asp:TextBox>
    </div>
    <div class="form-group form-group-sm col-xs-7">
        <label>INDIRIZZO</label>
        <asp:TextBox ID="Indirizzo" runat="server" CssClass="form-control"></asp:TextBox>
    </div>
    <div class="form-group form-group-sm col-xs-2">
        <label>CIVICO</label>
        <asp:TextBox ID="Civico" runat="server" CssClass="form-control"></asp:TextBox>
    </div>
</div>
<% Else %>
<div class="row">
    <div class="form-group form-group-sm col-xs-6">
        <label id="TitoloData" runat="server">DATA</label>
        <asp:TextBox ID="Data" runat="server" CssClass="form-control" data-type="data-ora" ></asp:TextBox>
    </div>
</div>
<% End if %>