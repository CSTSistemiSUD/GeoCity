<%@ Page Title="" Language="vb" AutoEventWireup="false" MasterPageFile="~/Account/Account.Master" CodeBehind="ProfileManager.aspx.vb" Inherits="PITER.ProfileManager" %>
<%@ Register Src="~/UserControls/Address.ascx" TagPrefix="uc1" TagName="Address" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
    <script type="text/javascript">

    
    var userAddress_js = null;
    var userBirthday_js = null;
    $(document).ready(function () {
        
        userAddress_js = new AddressSelector('<%=Me.UserAddress.ClientID%>');
        userBirthday_js = new AddressSelector('<%=Me.UserBirthday.ClientID%>');
    });

    

  
        

</script>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <h2 style="color:darkgreen"><i class="fa fa-user-o" aria-hidden="true"></i> Modifica il tuo profilo</h2>
    <hr />
    <p class="text-muted">Data di registrazione: <asp:Label ID="lblRegistrationDate" runat="server"></asp:Label> - Data di scadenza: <asp:Label ID="lblExpirationDate" runat="server"></asp:Label></p>
    <asp:UpdatePanel ID="UpdatePanel1" runat="server">
        <ContentTemplate>
            <div class="row">
                <div class="form-group form-group-sm col-md-6 col-sm-6">
                    <label>CODICE FISCALE</label>
                    <asp:TextBox ID="txtAnagraficaCodFis" runat="server" CssClass="form-control" ClientIDMode="Static"></asp:TextBox>
                </div>
                <div class="form-group form-group-sm col-md-6 col-sm-6">
                    <label>EMAIL</label>
                    <asp:TextBox ID="txtAnagraficaEmail" runat="server" CssClass="form-control" ReadOnly="true"></asp:TextBox>
                </div>
                            
            </div>
            <div class="row">
                <div class="form-group form-group-sm col-md-6 col-sm-6">
                    <label>COGNOME</label>
                    <asp:TextBox ID="txtAnagraficaCognome" runat="server" CssClass="form-control" ClientIDMode="Static"></asp:TextBox>
                </div>
                <div class="form-group form-group-sm col-md-6 col-sm-6">
                    <label>NOME</label>
                    <asp:TextBox ID="txtAnagraficaNome" runat="server" CssClass="form-control" ClientIDMode="Static"></asp:TextBox>
                </div>
                            
            </div>
            <div class="row">
                <div class="col-md-6 col-sm-12">
                    <uc1:Address runat="server" id="UserAddress" Caption="RESIDENZA" />
                </div>
                <div class="col-md-6 col-sm-12">
                    <uc1:Address runat="server" id="UserBirthday" Caption="DATA E LUOGO DI NASCITA" IsBirthdayInfo="true" />
                </div>
            </div>
            <div class="row">
                <asp:Panel runat="server" ID="ErrorPanel" Visible="false">
                    <div class="alert alert-danger alert-white rounded">
                        <div class="icon">
                            <i class="fa fa-times-circle"></i>
                        </div>
                        <asp:Label ID="ErrorLabel" runat="server"></asp:Label>
                    </div>    
                </asp:Panel>
                <asp:Panel runat="server" ID="SuccessPanel" Visible="false">
                    <div class="alert alert-success alert-white rounded">
                        <div class="icon">
                            <i class="fa fa-times-circle"></i>
                        </div>
                        Dati modificati con successo!
                    </div>    
                </asp:Panel>
            </div>
            <div class="clearfix"></div>
            <div>
                <hr />
                <asp:Button ID="UpdateUser" runat="server" CssClass="btn btn-primary btn-sm pull-right" Text="Aggiorna i miei dati" ClientIDMode="Static" />
            </div>
        </ContentTemplate>
    </asp:UpdatePanel>
    
</asp:Content>
