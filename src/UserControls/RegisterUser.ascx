<%@ Control Language="vb" AutoEventWireup="false" CodeBehind="RegisterUser.ascx.vb" Inherits="PITER.RegisterUser" %>
<%@ Register Src="~/UserControls/Address.ascx" TagPrefix="uc1" TagName="Address" %>

<script type="text/javascript">

    var <%=Me.ID%>_Js = null;
    var userAddress_js = null;
    var userBirthday_js = null;
    $(document).ready(function () {
        <%=Me.ID%>_Js = new RegisterUser('<%=Me.ClientID%>');
        userAddress_js = new AddressSelector('<%=Me.UserAddress.ClientID%>');
        userBirthday_js = new AddressSelector('<%=Me.UserBirthday.ClientID%>');
    });

    

  
        

</script>

<asp:Panel ID="RegisterUserDialog" runat="server" >
    <div class="modal-body">
        <h4>DATI UTENTE</h4>
        <div class="row">
            <div class="form-group form-group-sm  col-md-4 col-sm-12">
                <label>NOME</label>
                <asp:TextBox ID="NOME" runat="server" CssClass="form-control"  ></asp:TextBox>
                    
            </div>
            
       
            <div class="form-group form-group-sm col-md-4 col-sm-12">
                <label>COGNOME</label>
                <asp:TextBox ID="COGNOME" runat="server" CssClass="form-control"  ></asp:TextBox>
                    
            </div>
       
            <div class="form-group form-group-sm col-md-4 col-sm-12">
                <label>CODICE FISCALE</label>
                <asp:TextBox ID="CODICE_FISCALE" runat="server" CssClass="form-control" data-type="codice-fiscale"  ></asp:TextBox>
            </div>
        </div>
        <div class="row">
            <div class="col-md-6 col-sm-12">
                <uc1:Address runat="server" ID="UserAddress" Caption="RESIDENZA" />
            </div>
            <div class="col-md-6 col-sm-12">
                <uc1:Address runat="server" ID="UserBirthday" Caption="LUOGO E DATA DI NASCITA" IsBirthdayInfo="true" />
            </div>
        </div>
       
        <h4>CREDENZIALI</h4>

        <div class="row">
            
            <div class="form-group form-group-sm col-md-6  col-sm-12">
                <label>USER NAME</label>
                <asp:TextBox ID="USERNAME" runat="server" CssClass="form-control"  ></asp:TextBox>
                    
            </div>
        </div>
        <div class="row">
            <div class="form-group form-group-sm col-md-6 col-sm-12">
                <label>EMAIL</label>
                <asp:TextBox ID="EMAIL" runat="server" CssClass="form-control" data-type="email"  ></asp:TextBox>
            </div>
        </div>
         
        
    </div>
</asp:Panel>