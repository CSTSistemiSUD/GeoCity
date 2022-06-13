<%@ Page Title="" Language="vb" AutoEventWireup="false" MasterPageFile="~/WebForms/Page.Master" CodeBehind="Utenti.aspx.vb" Inherits="PITER.Utenti" %>
<%@ Register Src="~/UserControls/RegisterUser.ascx" TagPrefix="uc1" TagName="RegisterUser" %>
<asp:Content ID="Content1" ContentPlaceHolderID="HeadPlaceHolder" runat="server">
    <script src="js/utenti.js"></script>

</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="BodyPlaceHolder" runat="server">
    <div class ="container" style="position:relative; top:100px; background-color:white;">
        <script type="text/x-jsrender" id="tmpl_User">
            <tr>
                <td><button type="button" class="btn btn-danger btn-delete" data-id="{{:UserId}}">Elimina</button></td>
                <td><button type="button" class="btn btn-primary btn-attach" data-id="{{:UserId}}">Progetti</button></td>
                            
                <td>{{:UserName}}</td>
                <td>{{:NOME}}</td>
                <td>{{:COGNOME}}</td>
                <td>{{:Email}}</td>
                <td>{{:RegistrationDate}}</td>
            </tr>
        </script>

        <button type="button" class="btn btn-success btn-lg" id="btnCreateUser">Crea utente</button>
        <button type="button" class="btn btn-success btn-lg" id="btnCreateFakeUser">Crea utente Fake</button>
        <hr />

        <table class="table table-bordered table-condensed">
            <thead>
                <tr>
                    <th style="width:1%;" colspan="2"></th>
                    <th>USER</th>
                    <th>NOME</th>
                    <th>COGNOME</th>
                    <th>EMAIL</th>
                    <th>DATA DI REGISTRAZIONE</th>
                </tr>
            </thead>
            <tbody id="tbl_Users">
               
            </tbody>   
        </table>
        
        <%--<uc1:RegisterUser runat="server" id="NewUser"/>--%>
        <script type="text/javascript">
            $(document).ready(function () {
                <%--var regUser = <%=NewUser.ID%>_Js;
                $('#btnCreateUser').click(function () {

                    regUser.showDialog(-1, 'SUPER_USER');
                });--%>

               

            });
        </script>
    </div>
</asp:Content>
