<%@ Page Title="" Language="vb" AutoEventWireup="false" MasterPageFile="~/Account/Account.Master" CodeBehind="Register.aspx.vb" Inherits="PITER.Register" %>
<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
	
    <%: Scripts.Render("~/bundles/account/register") %>
	
	<style>
		.form-horizontal .control-label {
			text-align:left;
			text-transform:uppercase;
			
		}
	</style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <h2 style="color:darkgreen"><i class="fa fa-address-card-o" aria-hidden="true"></i> Registrazione</h2>
    <hr />
    <asp:Panel ID="RegistrationForm" runat="server" data-id="RegistrationForm">

<%--		<p><strong>Su quale Comune vuoi avere accesso?</strong></p>
		<div class="row">
			<div class="col-sm-6 form-group form-group-sm">
				<asp:DropDownList ID="ddlPRJ" runat="server" CssClass="form-control"></asp:DropDownList>
            </div>	
        </div>--%>

		<div class="row">
			<div class="col-sm-6 form-group form-group-sm">
				<label>Nome:</label>
				<asp:TextBox ID="NomeCtrl" runat="server" CssClass="form-control"  AutoCompleteType="None" data-required="true"></asp:TextBox>
                    
			</div>
			<div class="col-sm-6 form-group form-group-sm">
				<label>Cognome:</label>
				<asp:TextBox ID="CognomeCtrl" runat="server" CssClass="form-control" AutoCompleteType="None" data-required="true"></asp:TextBox>
			</div>
		</div>
        
        <div class="row">
			
			<div class="col-sm-6 form-group form-group-sm">
				<label>Indirizzo Email:</label>
				<asp:TextBox ID="EMailCtrl" runat="server" CssClass="form-control" AutoCompleteType="None" data-id="EMailCtrl" data-required="true"></asp:TextBox>
			</div>
			<div class="col-sm-6 form-group form-group-sm">
				<label>Conferma indirizzo Email:</label>
				<asp:TextBox ID="ConfirmEMailCtrl" runat="server" CssClass="form-control" AutoCompleteType="None"  data-id="ConfirmEMailCtrl" data-required="true"></asp:TextBox>
			</div>
		</div>
		
		
		<p class="text-muted">Una password temporanea verrà inviata all'indirizzo email specificato</p>
		<%--<div class="row">
              
                <div class="col-sm-6 form-group form-group-sm">
                    <label>Password:</label>
					<div class="input-group">
						<asp:TextBox ID="PasswordCtrl" runat="server" CssClass="form-control" TextMode="Password" AutoCompleteType="None" data-id="PasswordCtrl" data-required="true"></asp:TextBox>
						<span class="input-group-btn">
							<button class="btn btn-default toggle-input-type" type="button" data-ref="PasswordCtrl"><span class="glyphicon glyphicon-eye-open"></span></button>
						</span>
					</div> 
                </div>
				<div class="col-sm-6 form-group form-group-sm">
                    <label>Conferma Password:</label>
                    <div class="input-group">
						<asp:TextBox ID="ConfirmPasswordCtrl" runat="server" CssClass="form-control" TextMode="Password" AutoCompleteType="None"  data-id="ConfirmPasswordCtrl" data-required="true"></asp:TextBox>
						<span class="input-group-btn">
							<button class="btn btn-default toggle-input-type" type="button" data-ref="ConfirmPasswordCtrl"><span class="glyphicon glyphicon-eye-open"></span></button>
						</span>
					</div>
                </div>
        </div>--%>
            

		
		<div class="clearfix"></div>
		<div>
			<hr />
			<asp:Button ID="ConfirmButton" runat="server" Text="Conferma" CssClass="btn btn-primary btn-sm pull-right" data-id="ConfirmButton" />
		</div>
    
       

        
    </asp:Panel>
    
    
    <asp:Panel runat="server" ID="RegistrationError" CssClass="row" Visible="false">
        <div class="alert alert-danger alert-white rounded">
            <div class="icon">
                <i class="fa fa-times-circle"></i>
            </div>
            <asp:Label ID="ErrorLabel" runat="server"></asp:Label>
        </div>    
    </asp:Panel>
    
	<asp:Panel runat="server" ID="RegistrationComplete" CssClass="row" Visible="false">
        <div class="alert alert-success alert-white rounded">
            <div class="icon">
                <i class="fa fa-check" aria-hidden="true"></i>
            </div>
            <p>Una email di verifica è stata inviata all'indirizzo <strong><asp:Label ID="lblUserEmail" runat="server" Text=""></asp:Label></strong>.</p>
			<p> - Controlla la tua casella di posta, anche la cartella "Posta Indesiderata".<br />
				- Clicca sul link che trovi all'interno del testo per convalidare l'indirizzo email.<br />
			</p>				
			<p>Una volta convalidato l'indirizzo email, riceverai una <strong>password temporanea</strong> per accedere al sito.</p>
            
        </div>    
    </asp:Panel>

	
</asp:Content>
