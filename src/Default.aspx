<%@ Page Title="" Language="vb" AutoEventWireup="false" MasterPageFile="~/Site.Master" CodeBehind="Default.aspx.vb" Inherits="PITER._Default" %>
<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
	<script type="text/javascript">

		$(document).ready(function () {
			var redirectUrl = $('#hfRedirectUrl').val();
			var token = $('#hfToken').val();
			if (redirectUrl != '' && token != '') {


					setTimeout(function () {
					redirectAndPost(redirectUrl, [{ Name: 'token', Value: token }]);
				}, 1000);
			}
			
				
			
		});

		function redirectAndPost(pageUrl, pageParams) {

			//construct htmlForm string
			var $htmlForm = $('<form>').attr({ 'id' : 'temp_form', 'method': 'POST', 'action':  pageUrl });
			//<input type="hidden" id="CurrentUserName" value="' + userName + '" />

			$.each(pageParams, function (i, p) {
				$htmlForm.append(
					$('<input>').attr({ 'id': p.Name, 'name': p.Name, 'type': 'hidden' }).val(p.Value)
				);
			});

			//Submit the form
			$htmlForm.appendTo("body").submit();
		}

	</script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
	<asp:HiddenField ID="hfToken" runat="server" ClientIDMode="Static" />
	<asp:HiddenField ID="hfRedirectUrl" runat="server" ClientIDMode="Static" />
    <!-- Masthead -->
    <header class="masthead text-white text-center">
     
      <div class="container">

	
        <div class="row">
          
          
            <div class="col-md-6 col-md-offset-3 col-sm-8 col-sm-offset-2 col-xs-10 col-xs-offset-1">
                <div class="panel">
                    <div class="panel-body">
                        <h4>Area Riservata</h4>

                            <asp:Panel ID="pnlLogin" runat="server">
                                    
                                    
                                <div class="form-horizontal">
                                    <div class="form-group form-group-sm">
                                        <%--<label class="col-sm-2  control-label">Username:</label>--%>
                                        <div class="col-sm-12">
                                            <asp:TextBox ID="UserName" runat="server" placeholder="Username" CssClass="form-control"></asp:TextBox>
                                        </div>
                                    </div>
                                    <div class="form-group form-group-sm">
                                        <%--<label class="col-sm-2  control-label">Password:</label>--%>
                                        <div class="col-sm-12">
                                            <asp:TextBox ID="Password" runat="server" placeholder="Password" CssClass="form-control" TextMode="Password"></asp:TextBox>
                                        </div>
                                    </div>
                                    <div>
                                        <asp:Button ID="LoginButton" runat="server" Text="Accedi" CssClass="btn btn-primary btn-block btn-lg " />
                                    </div>
                                </div>
                                <p style="margin-top:2em">
                                    Non ricordi le tue credenziali di accesso?<br /><asp:HyperLink runat="server" Text="RICHIEDI PASSWORD TEMPORANEA" NavigateUrl="~/Account/ResetPassword" Font-Bold="true"></asp:HyperLink>
                                </p>
								<p style="margin-top:2em">
                                    Se non disponi di account, <asp:HyperLink runat="server" Text="REGISTRATI" NavigateUrl="~/Account/Register" Font-Bold="true"></asp:HyperLink>
                                </p>
                                <asp:Panel runat="server" ID="ErrorPanel" Visible="false">
                                        <div class="alert alert-danger alert-white rounded">
                                            <asp:Label ID="ErrorLabel" runat="server"></asp:Label>
                                        </div>    
                                    </asp:Panel>
                                </asp:Panel>

                                <asp:Panel ID="pnlLogout" runat="server" CssClass="panel">
                                    Benvenuto <%=Page.User.Identity.Name %><br /><br />
                                    <div>
                                        <asp:Button ID="MainButton" runat="server" Text="Entra" CssClass="btn btn-success btn-block btn-lg " />
                                        <asp:Button ID="LogoutButton" runat="server" Text="Esci" CssClass="btn btn-danger btn-block btn-lg "  />
                                    </div>
                                </asp:Panel>
                    </div>
                </div>
            </div>
            <div class="col-md-8 col-xs-12">
					<asp:Button ID="Button1" runat="server" Text="TEST" CssClass="btn btn-danger btn-lg " Visible="false" />
                
            </div>
        </div>
      </div>
    </header>

    <!-- Icons Grid -->


    
    
</asp:Content>
