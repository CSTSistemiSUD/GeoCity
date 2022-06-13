Imports System.Web
Imports System.Web.Services

Public Class SimpleDownload
	Implements System.Web.IHttpHandler

	Sub ProcessRequest(ByVal context As HttpContext) Implements IHttpHandler.ProcessRequest

		Dim filePath As String = Encoding.UTF8.GetString(Convert.FromBase64String(context.Request.QueryString("Path")))


		If System.IO.File.Exists(filePath) Then
			context.Response.AddHeader("Content-Disposition", "attachment; filename=" + IO.Path.GetFileName(filePath))
			context.Response.ContentType = "application/octet-stream"
			context.Response.ClearContent()
			context.Response.WriteFile(filePath)
		Else
			context.Response.StatusCode = 404
		End If
	End Sub

	ReadOnly Property IsReusable() As Boolean Implements IHttpHandler.IsReusable
		Get
			Return False
		End Get
	End Property

End Class