Imports System.Net

Public Class AuthTrack
    Public Shared Sub LoggedIn(UserName As String)
        AuthTrack.Store(UserName, "Login")
    End Sub

    Public Shared Sub LoggedOut(UserName As String)
        AuthTrack.Store(UserName, "Logout")
    End Sub

    Public Shared Sub Store(UserName As String, ActivityType As String)
        Dim insert As String = "INSERT INTO  usp_AuthTrack
			(UserName
			,ActivityDate
			,IpAddress
			,ActivityType)
		VALUES
			(@UserName
			,@ActivityDate
			,@IpAddress
			,@ActivityType)"

        Dim cmd As New SqlClient.SqlCommand With {.CommandText = insert, .CommandType = CommandType.Text}
        With cmd.Parameters
            .AddWithValue("@UserName", UserName)
            .AddWithValue("@ActivityDate", Now)
            .AddWithValue("@IpAddress", GetIPAddress)
            .AddWithValue("@ActivityType", ActivityType)
        End With
        Dim cnnString As String = ConfigurationManager.ConnectionStrings("DefaultConnection").ConnectionString
        Dim sqlCnn As New SqlClient.SqlConnection(cnnString)
        Dim sqlTRANS As SqlClient.SqlTransaction = Nothing

        Try
            sqlCnn.Open()
            sqlTRANS = sqlCnn.BeginTransaction


            cmd.Connection = sqlCnn
            cmd.Transaction = sqlTRANS
            cmd.ExecuteNonQuery()

            sqlTRANS.Commit()

        Catch ex As Exception

            sqlTRANS.Rollback()
        Finally
            sqlCnn.Close()
        End Try
    End Sub

    Public Shared Function GetIPAddress() As String
        Dim ipAddr As String = String.Empty
        If HttpContext.Current IsNot Nothing AndAlso HttpContext.Current.Request IsNot Nothing Then
            If HttpContext.Current IsNot Nothing Then
                For Each IPA As IPAddress In Dns.GetHostAddresses(HttpContext.Current.Request.UserHostAddress)
                    If IPA.AddressFamily.ToString() = "InterNetwork" Then
                        ipAddr = IPA.ToString()
                        Exit For
                    End If
                Next
            End If

            If Not String.IsNullOrEmpty(ipAddr) Then Return ipAddr
            For Each IPA As IPAddress In Dns.GetHostAddresses(Dns.GetHostName())
                If IPA.AddressFamily.ToString() = "InterNetwork" Then
                    ipAddr = IPA.ToString()
                    Exit For
                End If
            Next
        End If

        Return ipAddr
    End Function

    Public Shared Function GetBrowserStats() As String
        Dim retResult As String = String.Empty
        If HttpContext.Current IsNot Nothing AndAlso HttpContext.Current.Request IsNot Nothing Then
            Dim browser As HttpBrowserCapabilities = HttpContext.Current.Request.Browser
            retResult = "Browser " & vbLf &
                "Type = " & browser.Type & vbLf &
                "Name = " & browser.Browser & vbLf &
                "Version = " & browser.Version & vbLf &
                "Major Version = " & browser.MajorVersion & vbLf &
                "Minor Version = " & browser.MinorVersion & vbLf &
                "Platform = " & browser.Platform & vbLf &
                "Is Beta = " & browser.Beta & vbLf &
                "Is Crawler = " & browser.Crawler & vbLf &
                "Is AOL = " & browser.AOL & vbLf &
                "Is Win16 = " & browser.Win16 & vbLf &
                "Is Win32 = " & browser.Win32 & vbLf &
                "Supports Frames = " & browser.Frames & vbLf &
                "Supports Tables = " & browser.Tables & vbLf &
                "Supports Cookies = " & browser.Cookies & vbLf &
                "Supports VBScript = " & browser.VBScript & vbLf &
                "Supports JavaScript = " & browser.EcmaScriptVersion.ToString() & vbLf &
                "Supports Java Applets = " & browser.JavaApplets & vbLf &
                "Supports ActiveX Controls = " & browser.ActiveXControls & vbLf
        End If

        Return retResult
    End Function
End Class
