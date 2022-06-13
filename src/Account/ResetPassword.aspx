<%@ Page Title="" Language="vb" AutoEventWireup="false" MasterPageFile="~/Account/Account.Master" CodeBehind="ResetPassword.aspx.vb" Inherits="PITER.ResetPassword" %>
<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
	<script>
		$(document).ready(function () {
				var email = $('#<%=EmailCtrl.ClientId%>');
			var resetPwdButton = $('#<%=ResetPasswordButton.ClientId%>');

			$(document).on('focus', '.has-error', function () {
				$(this).closest('.form-group').removeClass('has-error');
			});

		
			resetPwdButton.click(function () {
				if (email.val() == '') {
					$(email).closest('.form-group').addClass('has-error');

					return false;
				}
				else {
					return true;
				}
			});
		});
	</script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    
    <h2 style="color:darkgreen"><i class="fa fa-paper-plane-o" aria-hidden="true"></i> Recupera i dettagli del tuo account</h2>
    <hr />
    <asp:UpdatePanel ID="UpdatePanel1" runat="server">
        <ContentTemplate>
            
			<asp:Panel ID="ResetPasswordForm" runat="server" CssClass="row">
                <div class="col-sm-6 col-sm-offset-3">
                    <div class="form-horizontal">
                        
                        <div class="form-group form-group-sm">
                            
                            <label class="col-sm-2 control-label">Email</label>
                                <div class="col-sm-10">
								<asp:TextBox runat="server" ID="EmailCtrl" ClientIDMode="Static" CssClass="form-control" placeholder="Indirizzo Email dell'Account" ></asp:TextBox>
                                <p class="help-block text-danger"></p>
                            </div>
                        </div>
                        
                    </div>
                </div>

				<div class="clearfix"></div>
				<div class="text-center">
					<hr />
					<asp:Button ID="ResetPasswordButton" runat="server" CssClass="btn btn-primary btn-sm" Text="Inviami una password temporanea" ClientIDMode="Static" OnClick="ResetPasswordButton_Click" />
				</div>
            </asp:Panel>
                
			<div style="height:30px;"></div>
            
            <asp:Panel runat="server" ID="ErrorPanel" Visible="false"  CssClass="row">
                <div class="alert alert-danger alert-white rounded">
                    <div class="icon">
                        <i class="fa fa-times-circle"></i>
                    </div>
                    <asp:Label ID="ErrorLabel" runat="server"></asp:Label>
                </div>
            </asp:Panel>
            
            

        
            <asp:Panel runat="server" ID="SuccessPanel" Visible="false" CssClass="row">
                <div class="alert alert-success alert-white rounded">
                    <div class="icon">
                        <i class="fa fa-check"></i>
                    </div>

                    <p>Entro i prossimi 10 minuti (solitamente è istantaneo) riceverai una email con le istruzioni per completare la procedura di reimpostazione password.<br />All'indirizzo email <strong><asp:Label ID="lblUserEMail" runat="server" Text=""></asp:Label></strong> è stata inviata una password temporanea da utilizzare per l'accesso.</p>
                </div>    
            </asp:Panel>
    
                
        
        </ContentTemplate>
    </asp:UpdatePanel>
                
                
</asp:Content>
