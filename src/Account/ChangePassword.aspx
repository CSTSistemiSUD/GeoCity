<%@ Page Title="" Language="vb" AutoEventWireup="false" MasterPageFile="~/Account/Account.Master" CodeBehind="ChangePassword.aspx.vb" Inherits="PITER.ChangePassword" %>
<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
    <%: Scripts.Render("~/bundles/account/changepassword") %>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <h2 style="color:darkgreen"><i class="fa fa-key" aria-hidden="true"></i> Modifica la password</h2>
    <hr />
    <asp:UpdatePanel ID="UpdatePanel1" runat="server">
        <ContentTemplate>
             <div id="pwd-container">
                <div class="row">
                    <div class="col-sm-6">
                        <div class="form-horizontal" >
                        
                            <div class="form-group form-group-sm">
                                <label class="col-sm-5 control-label">Password corrente</label>
                                    <div class="col-sm-7">
                                    <asp:TextBox runat="server" ID="OldPassword" ClientIDMode="Static" CssClass="form-control" placeholder="Inserisci la tua password" required="required" TextMode="Password"></asp:TextBox>
                                </div>
                            </div>
                            <div class="form-group form-group-sm">
                                <label class="col-sm-5 control-label">Nuova Password</label>
                                    <div class="col-sm-7">
                                    <asp:TextBox runat="server" ID="NewPassword" ClientIDMode="Static" CssClass="form-control" placeholder="Inserisci la nuova password" required="required" TextMode="Password"></asp:TextBox>
                                </div>
                            </div>
                            <div class="form-group form-group-sm">
                                <label class="col-sm-5 control-label">Conferma Password</label>
                                    <div class="col-sm-7">
                                    <asp:TextBox runat="server" ID="ConfirmPassword" ClientIDMode="Static" CssClass="form-control" placeholder="Conferma la nuova password" required="required" TextMode="Password"></asp:TextBox>
                                </div>
                            </div>
                
                        </div>
                    </div>
        
                </div>
                <div class="row">
                    <div class="col-sm-6">
                        <div class="col-sm-7 col-sm-offset-5">
                            <div id="pwstrength_viewport_progress"></div>
                        </div>
                    </div>
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
                        Password modificata con successo! Sarai reindirizzato alla pagina di login tra alcuni secondi.<br />
                        Se non vieni reindirizzato automaticamente, fai clic <asp:HyperLink runat="server" NavigateUrl="~/Account/Logout">qui</asp:HyperLink>
                        
                    </div>    
                </asp:Panel>
            </div>
            <div class="clearfix"></div>
            <div>
                <hr />
                <asp:Button ID="UpdatePassword" runat="server" CssClass="btn btn-primary btn-sm pull-right" Text="Modifica" ClientIDMode="Static" />
            </div>
            <div style="display:none;">
                <asp:CheckBox ID="CheckRedirect" runat="server" Checked="false"  ClientIDMode="Static"/>
                <asp:HiddenField ID="UrlRedirect" runat="server" Value="" ClientIDMode="Static" />
            </div>
        </ContentTemplate>
    </asp:UpdatePanel>
</asp:Content>
