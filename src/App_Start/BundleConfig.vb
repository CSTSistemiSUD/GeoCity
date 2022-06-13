Imports System.Collections.Generic
Imports System.Linq
Imports System.Web
Imports System.Web.Optimization

Public Class BundleConfig
    ' Per altre informazioni sulla creazione di bundle, vedere https://go.microsoft.com/fwlink/?LinkID=303951
    Public Shared Sub RegisterBundles(ByVal bundles As BundleCollection)
        '  // Creazione dei jQuery Bundles -----------------------------


        bundles.Add(New ScriptBundle("~/bundles/MsAjaxJs").Include(
                "~/Scripts/WebForms/MsAjax/MicrosoftAjax.js",
                "~/Scripts/WebForms/MsAjax/MicrosoftAjaxApplicationServices.js",
                "~/Scripts/WebForms/MsAjax/MicrosoftAjaxTimer.js",
                "~/Scripts/WebForms/MsAjax/MicrosoftAjaxWebForms.js"))

        bundles.Add(New ScriptBundle("~/bundles/IEfixups").Include(
                        "~/Scripts/html5shiv.min.js",
                        "~/Scripts/html5shiv-printshiv.min.js",
                        "~/Scripts/respond.min.js",
                        "~/Scripts/excanvas.min.js"))


        bundles.Add(New ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"))


        bundles.Add(New ScriptBundle("~/bundles/main").Include(
                    "~/Scripts/jquery-{version}.js",
                    "~/Scripts/bootstrap.min.js",
                    "~/Scripts/bootstrap-dialog.min.js"))

        bundles.Add(New ScriptBundle("~/bundles/plugin").Include(
                    "~/Scripts/jquery.numeric.min.js",
                    "~/Scripts/jquery.maskedinput.min.js",
                    "~/Scripts/jsrender.min.js",
                    "~/Scripts/bootstrap3-typeahead.min.js",
                    "~/Scripts/accounting.min.js",
                    "~/Scripts/moment-with-locales.min.js",
                    "~/Scripts/jsvat.min.js",
                    "~/Scripts/bootstrap-table-expandable.min.js",
                    "~/Scripts/Plugin/ValidInputChecker.js",
                    "~/Scripts/utils.min.js"
                    ))

        bundles.Add(New ScriptBundle("~/bundles/datatables").Include(
                    "~/Scripts/DataTables/jquery.dataTables.min.js",
                    "~/Scripts/DataTables/dataTables.bootstrap.min.js"
                    ))

        '//
        bundles.Add(New ScriptBundle("~/bundles/account/register").Include(
                   "~/Account/js/Register.js"
                   ))

        bundles.Add(New ScriptBundle("~/bundles/account/changepassword").Include(
                   "~/Account/js/pwstrength-bootstrap.min.js",
                   "~/Account/js/ChangePassword.js"
                   ))


        'bundles.Add(New ScriptBundle("~/bundles/viewer").Include(
        '          "~/Scripts/utils.js",
        '          "~/WebForms/Mappa/js/viewer.js"
        '          ))
    End Sub
End Class
