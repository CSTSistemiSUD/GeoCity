Imports System.Web
Imports System.Web.Services

Public Class GetInfo
	Implements System.Web.IHttpHandler

	Sub ProcessRequest(ByVal context As HttpContext) Implements IHttpHandler.ProcessRequest

		Dim op As String = context.Request.QueryString("op")
		Dim token As String = context.Request.QueryString("token")
		Dim authResult = Nothing 'ws_Main.fn_CheckToken(token)
		Dim json As String = ""
		If Not IsNothing(authResult) AndAlso Not IsNothing(authResult.data) Then
			Select Case op.ToLower
				Case "sensorlist"
					json = GetSensorList()
				Case Else
					authResult.status_message="Undefined operation"
					json = Utility.JsonSerialize(authResult)
			End Select
		Else
			json = Utility.JsonSerialize(authResult)
		End If
		context.Response.ContentType = "text/json"
		context.Response.Write(json)
	End Sub

	ReadOnly Property IsReusable() As Boolean Implements IHttpHandler.IsReusable
		Get
			Return False
		End Get
	End Property


	Private Function GetSensorList() As String
		Dim query As String = "SELECT * FROM Geosafety_UTM33N.dbo.VW_SENSORI"
		Dim ssh As New SSHelper
		Dim dbResult As DatabaseOperationResult = ssh.GetData(query)
		Return dbResult.SerializeTable(0)
	End Function
End Class