Imports System.Drawing
Imports System.Drawing.Imaging
Imports System.Web
Imports System.Web.Services

Public Class Logo
	Implements System.Web.IHttpHandler

	Sub ProcessRequest(ByVal context As HttpContext) Implements IHttpHandler.ProcessRequest

		Dim CodComune As String = context.Request.QueryString("CodComune")

		Dim filePath As String = context.Server.MapPath("~/Content/images/logo.png")
		Dim fileExtension As String = ".png"

		If Not String.IsNullOrEmpty(CodComune) Then
			Dim path As String = String.Format(ConfigurationManager.AppSettings("LogoPath"), CodComune)
			If IO.File.Exists(path) Then

				filePath = path
				fileExtension = IO.Path.GetExtension(filePath)

			End If
		End If

		'context.Response.Cache.SetCacheability(HttpCacheability.Public)
		'context.Response.Cache.SetMaxAge(New TimeSpan(365, 0, 0, 0, 0))
		context.Response.ContentType = String.Format("image/{0}", fileExtension.Remove(0, 1))

		Dim fileFormat As ImageFormat = ImageFormat.Jpeg
		Select Case fileExtension
			Case ".gif"
				fileFormat = ImageFormat.Gif
			Case ".jpg", ".jpeg"
				fileFormat = ImageFormat.Jpeg
			Case ".png"
				fileFormat = ImageFormat.Png
		End Select

		Using img As New Bitmap(filePath)
			Using ms As New IO.MemoryStream
				img.Save(ms, fileFormat)
				ms.WriteTo(context.Response.OutputStream)
			End Using
		End Using

	End Sub

	ReadOnly Property IsReusable() As Boolean Implements IHttpHandler.IsReusable
		Get
			Return False
		End Get
	End Property

End Class