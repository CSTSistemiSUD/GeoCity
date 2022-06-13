<%@ Page Title="" Language="vb" AutoEventWireup="false" MasterPageFile="~/Account/Account.Master" CodeBehind="Login.aspx.vb" Inherits="PITER.Login" %>
<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <h2 style="color:darkgreen"><i class="fa fa-unlock-alt" aria-hidden="true"></i> Accedi all'area riservata</h2>
    <hr />
    <div class="row">

        <div class="col-sm-6 col-sm-offset-3">
            <div class="form-horizontal">
                <div class="form-group form-group-sm">
                    <label class="col-sm-2  control-label">Username:</label>
                    <div class="col-sm-8">
                        <asp:TextBox ID="UserName" runat="server" CssClass="form-control"></asp:TextBox>
                    </div>
                </div>
                <div class="form-group form-group-sm">
                    <label class="col-sm-2  control-label">Password:</label>
                    <div class="col-sm-8">
                        <asp:TextBox ID="Password" runat="server" CssClass="form-control" TextMode="Password"></asp:TextBox>
                    </div>
                </div>
            </div>
            
        </div>
        <div class="col-sm-6 col-sm-offset-3">
            <p><asp:HyperLink runat="server" Text="Se non lo hai già, registra nuovo account." NavigateUrl="~/Account/Register" Visible="false"></asp:HyperLink></p>
            <p><asp:HyperLink runat="server" Text="Non ricordi le tue credenziali di accesso?" NavigateUrl="~/Account/ResetPassword"></asp:HyperLink></p>
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
    </div>
    <div class="clearfix"></div>
    <div>
        <hr />
        <asp:Button ID="LoginButton" runat="server" Text="Accedi" CssClass="btn btn-primary btn-sm pull-right" />
    </div>
</asp:Content>
