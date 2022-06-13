<%@ Page Title="" Language="vb" AutoEventWireup="false" MasterPageFile="~/Account/Account.Master" CodeBehind="Confirm.aspx.vb" Inherits="PITER.Confirm" %>
<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <h2 style="color:darkgreen"><i class="fa fa-user-o" aria-hidden="true"></i> Conferma registrazione</h2>
    <hr />

    <div class="row">
        
        <asp:Panel runat="server" ID="SuccessPanel" Visible="false">
            <div class="alert alert-success alert-white rounded">
                <div class="icon">
                    <i class="fa fa-check"></i>
                </div>

                <strong>Grazie per aver confermato la sua registrazione.</strong>
                <p>Entro i prossimi 10 minuti (solitamente è istantaneo) riceverà una email con le credenziali (UserName e una password temporanea) per accedere al sito.</p>
                <p>Una volta effettuato il login verrà reindirizzato alla pagina per la modifica della password.</p>
            </div>    
        </asp:Panel>
    
        <asp:Panel runat="server" ID="ErrorPanel" Visible="false">
            <div class="alert alert-danger alert-white rounded">
                <div class="icon">
                    <i class="fa fa-times-circle"></i>
                </div>
                <asp:Label ID="ErrorLabel" runat="server"></asp:Label>
            </div>    
        </asp:Panel> 
    </div>
</asp:Content>
