<%@ Page Title="" Language="vb" AutoEventWireup="false" MasterPageFile="~/Account/Account.Master" CodeBehind="NotFound.aspx.vb" Inherits="PITER.NotFound" %>
<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">

	<h2 style="color:darkgreen"><i class="fa fa-user-o" aria-hidden="true"></i> OOPS!!!</h2>
    <hr />

    <div class="row">
        
        <div class="alert alert-danger alert-white rounded">
                <div class="icon">
                    <i class="fa fa-times-circle"></i>
                </div>
                <asp:Label ID="ErrorLabel" runat="server" Text="L'indirizzo cercato non è valido"></asp:Label>
            </div>    
    </div>

</asp:Content>
