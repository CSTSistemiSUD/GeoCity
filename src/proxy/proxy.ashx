<%@ WebHandler Language="VB" Class="proxy" %>

Imports System.IO
Imports System.Web
Imports System.Xml.Serialization
Imports System.Web.Caching
Imports System.Collections.Concurrent
Imports System.Diagnostics
Imports System.Text.RegularExpressions
Public Class proxy
    Implements IHttpHandler

    Private Shared version As [String] = "1.1.1-beta"

    Private Class RateMeter
        Private _rate As Double
        'internal rate is stored in requests per second
        Private _countCap As Integer
        Private _count As Double = 0
        Private _lastUpdate As DateTime = DateTime.Now

        Public Sub New(rate_limit As Integer, rate_limit_period As Integer)
            _rate = CDbl(rate_limit) / rate_limit_period / 60
            _countCap = rate_limit
        End Sub

        'called when rate-limited endpoint is invoked
        Public Function click() As Boolean
            Dim ts As TimeSpan = DateTime.Now - _lastUpdate
            _lastUpdate = DateTime.Now
            'assuming uniform distribution of requests over time,
            'reducing the counter according to # of seconds passed
            'since last invocation
            _count = Math.Max(0, _count - ts.TotalSeconds * _rate)
            If _count <= _countCap Then
                'good to proceed
                _count += 1
                Return True
            End If
            Return False
        End Function

        Public Function canBeCleaned() As Boolean
            Dim ts As TimeSpan = DateTime.Now - _lastUpdate
            Return _count - ts.TotalSeconds * _rate <= 0
        End Function
    End Class

    Private Shared PROXY_REFERER As String = "http://localhost/proxy/proxy.ashx"
    Private Shared DEFAULT_OAUTH As String = "https://www.arcgis.com/sharing/oauth2/"
    Private Shared CLEAN_RATEMAP_AFTER As Integer = 10000
    'clean the rateMap every xxxx requests
    Private Shared SYSTEM_PROXY As System.Net.IWebProxy = System.Net.HttpWebRequest.DefaultWebProxy
    ' Use the default system proxy
    Private Shared logTraceListener As LogTraceListener = Nothing
    Private Shared _rateMapLock As New [Object]()

    Public Sub ProcessRequest(context As HttpContext) Implements IHttpHandler.ProcessRequest


        If logTraceListener Is Nothing Then
            logTraceListener = New LogTraceListener()
            Trace.Listeners.Add(logTraceListener)
        End If


        Dim response As HttpResponse = context.Response
        If context.Request.Url.Query.Length < 1 Then
            Dim errorMsg As String = "This proxy does not support empty parameters."
            log(TraceLevel.[Error], errorMsg)
            sendErrorResponse(context.Response, Nothing, errorMsg, System.Net.HttpStatusCode.BadRequest)
            Return
        End If

        Dim uri As String = context.Request.Url.Query.Substring(1)

        'if uri is ping
        If uri.Equals("ping", StringComparison.InvariantCultureIgnoreCase) Then
            Dim proxyConfig__1 As ProxyConfig = ProxyConfig.GetCurrentConfig()

            Dim checkConfig As [String] = If((proxyConfig__1 Is Nothing), "Not Readable", "OK")
            Dim checkLog As [String] = ""
            If checkConfig <> "OK" Then
                checkLog = "Can not verify"
            Else
                Dim filename As [String] = proxyConfig__1.LogFile
                checkLog = If((filename IsNot Nothing AndAlso filename <> ""), "OK", "Not Exist/Readable")

                If checkLog = "OK" Then
                    log(TraceLevel.Info, "Log from ping")

                End If
            End If

            sendPingResponse(response, version, checkConfig, checkLog)
            Return
        End If

        'if url is encoded, decode it.
        If uri.StartsWith("http%3a%2f%2f", StringComparison.InvariantCultureIgnoreCase) OrElse uri.StartsWith("https%3a%2f%2f", StringComparison.InvariantCultureIgnoreCase) Then
            uri = HttpUtility.UrlDecode(uri)
        End If

        log(TraceLevel.Info, uri)
        Dim serverUrl As ServerUrl
        Try
            serverUrl = getConfig().GetConfigServerUrl(uri)

            If serverUrl Is Nothing Then
                'if no serverUrl found, send error message and get out.
                Dim errorMsg As String = "The request URL does not match with the ServerUrl in proxy.config! Please check the proxy.config!"
                log(TraceLevel.[Error], errorMsg)
                sendErrorResponse(context.Response, Nothing, errorMsg, System.Net.HttpStatusCode.BadRequest)
                Return
            End If
            'if XML couldn't be parsed
        Catch ex As InvalidOperationException

            Dim errorMsg As String = ex.InnerException.Message & " " & uri
            log(TraceLevel.[Error], errorMsg)
            sendErrorResponse(context.Response, Nothing, errorMsg, System.Net.HttpStatusCode.InternalServerError)
            Return
            'if mustMatch was set to true and URL wasn't in the list
        Catch ex As ArgumentException
            Dim errorMsg As String = ex.Message & " " & uri
            log(TraceLevel.[Error], errorMsg)
            sendErrorResponse(context.Response, Nothing, errorMsg, System.Net.HttpStatusCode.Forbidden)
            Return
        End Try
        'use actual request header instead of a placeholder, if present
        If context.Request.Headers("referer") IsNot Nothing Then
            PROXY_REFERER = context.Request.Headers("referer")
        End If

        'referer
        'check against the list of referers if they have been specified in the proxy.config
        Dim allowedReferersArray As [String]() = ProxyConfig.GetAllowedReferersArray()
        If allowedReferersArray IsNot Nothing AndAlso allowedReferersArray.Length > 0 AndAlso context.Request.Headers("referer") IsNot Nothing Then
            PROXY_REFERER = context.Request.Headers("referer")
            Dim requestReferer As String = context.Request.Headers("referer")
            Try

                Dim checkValidUri As [String] = New UriBuilder(If(requestReferer.StartsWith("//"), requestReferer.Substring(requestReferer.IndexOf("//") + 2), requestReferer)).Host
            Catch e As Exception
                log(TraceLevel.Warning, "Proxy is being used from an invalid referer: " & context.Request.Headers("referer"))
                sendErrorResponse(context.Response, "Error verifying referer. ", "403 - Forbidden: Access is denied.", System.Net.HttpStatusCode.Forbidden)
                Return
            End Try

            If Not checkReferer(allowedReferersArray, requestReferer) Then
                log(TraceLevel.Warning, "Proxy is being used from an unknown referer: " & context.Request.Headers("referer"))
                sendErrorResponse(context.Response, "Unsupported referer. ", "403 - Forbidden: Access is denied.", System.Net.HttpStatusCode.Forbidden)


            End If
        End If

        'Check to see if allowed referer list is specified and reject if referer is null
        If context.Request.Headers("referer") Is Nothing AndAlso allowedReferersArray IsNot Nothing AndAlso Not allowedReferersArray(0).Equals("*") Then
            log(TraceLevel.Warning, "Proxy is being called by a null referer.  Access denied.")
            sendErrorResponse(response, "Current proxy configuration settings do not allow requests which do not include a referer header.", "403 - Forbidden: Access is denied.", System.Net.HttpStatusCode.Forbidden)
            Return
        End If

        'Throttling: checking the rate limit coming from particular client IP
        If serverUrl.RateLimit > -1 Then
            SyncLock _rateMapLock
                Dim ratemap As ConcurrentDictionary(Of String, RateMeter) = DirectCast(context.Application("rateMap"), ConcurrentDictionary(Of String, RateMeter))
                If ratemap Is Nothing Then
                    ratemap = New ConcurrentDictionary(Of String, RateMeter)()
                    context.Application("rateMap") = ratemap
                    context.Application("rateMap_cleanup_counter") = 0
                End If
                Dim key As String = "[" & serverUrl.Url & "]x[" & context.Request.UserHostAddress & "]"
                Dim rate As RateMeter = Nothing
                If Not ratemap.TryGetValue(key, rate) Then
                    rate = New RateMeter(serverUrl.RateLimit, serverUrl.RateLimitPeriod)
                    ratemap.TryAdd(key, rate)
                End If
                If Not rate.click() Then
                    log(TraceLevel.Warning, " Pair " & key & " is throttled to " & serverUrl.RateLimit & " requests per " & serverUrl.RateLimitPeriod & " minute(s). Come back later.")
                    sendErrorResponse(context.Response, "This is a metered resource, number of requests have exceeded the rate limit interval.", "Unable to proxy request for requested resource", CType(429, System.Net.HttpStatusCode))
                    Return
                End If

                'making sure the rateMap gets periodically cleaned up so it does not grow uncontrollably
                Dim cnt As Integer = CInt(context.Application("rateMap_cleanup_counter"))
                cnt += 1
                If cnt >= CLEAN_RATEMAP_AFTER Then
                    cnt = 0
                    cleanUpRatemap(ratemap)
                End If
                context.Application("rateMap_cleanup_counter") = cnt
            End SyncLock
        End If

        'readying body (if any) of POST request
        Dim postBody As Byte() = readRequestPostBody(context)
        Dim post As String = System.Text.Encoding.UTF8.GetString(postBody)

        Dim credentials As System.Net.NetworkCredential = Nothing
        Dim requestUri As String = uri
        Dim hasClientToken As Boolean = False
        Dim token As String = String.Empty
        Dim tokenParamName As String = Nothing

        If (serverUrl.HostRedirect IsNot Nothing) AndAlso (serverUrl.HostRedirect <> String.Empty) Then
            requestUri = serverUrl.HostRedirect & New Uri(requestUri).PathAndQuery
        End If

        If serverUrl.Domain IsNot Nothing Then
            credentials = New System.Net.NetworkCredential(serverUrl.Username, serverUrl.Password, serverUrl.Domain)
        Else
            'if token comes with client request, it takes precedence over token or credentials stored in configuration
            hasClientToken = requestUri.Contains("?token=") OrElse requestUri.Contains("&token=") OrElse post.Contains("?token=") OrElse post.Contains("&token=")

            If Not hasClientToken Then
                ' Get new token and append to the request.
                ' But first, look up in the application scope, maybe it's already there:
                token = DirectCast(context.Application("token_for_" & serverUrl.Url), [String])
                Dim tokenIsInApplicationScope As Boolean = Not [String].IsNullOrEmpty(token)

                'if still no token, let's see if there is an access token or if are credentials stored in configuration which we can use to obtain new token
                If Not tokenIsInApplicationScope Then
                    token = serverUrl.AccessToken
                    If [String].IsNullOrEmpty(token) Then
                        token = getNewTokenIfCredentialsAreSpecified(serverUrl, requestUri)
                    End If
                End If

                If Not [String].IsNullOrEmpty(token) AndAlso Not tokenIsInApplicationScope Then
                    'storing the token in Application scope, to do not waste time on requesting new one untill it expires or the app is restarted.
                    context.Application.Lock()
                    context.Application("token_for_" & serverUrl.Url) = token
                    context.Application.UnLock()
                End If
            End If

            'name by which token parameter is passed (if url actually came from the list)
            tokenParamName = If(serverUrl IsNot Nothing, serverUrl.TokenParamName, Nothing)

            If [String].IsNullOrEmpty(tokenParamName) Then
                tokenParamName = "token"
            End If

            requestUri = addTokenToUri(requestUri, token, tokenParamName)
        End If

        'forwarding original request
        Dim serverResponse As System.Net.WebResponse = Nothing
        Try
            serverResponse = forwardToServer(context, requestUri, postBody, credentials)
        Catch webExc As System.Net.WebException

            Dim errorMsg As String = webExc.Message & " " & uri
            log(TraceLevel.[Error], errorMsg)

            If webExc.Response IsNot Nothing Then
                copyHeaders(TryCast(webExc.Response, System.Net.HttpWebResponse), context.Response)

                Using responseStream As Stream = webExc.Response.GetResponseStream()
                    Dim bytes As Byte() = New Byte(32767) {}
                    Dim bytesRead As Integer = 0

                    While (InlineAssignHelper(bytesRead, responseStream.Read(bytes, 0, bytes.Length))) > 0
                        responseStream.Write(bytes, 0, bytesRead)
                    End While

                    context.Response.StatusCode = CInt(TryCast(webExc.Response, System.Net.HttpWebResponse).StatusCode)
                    context.Response.OutputStream.Write(bytes, 0, bytes.Length)
                End Using
            Else
                Dim statusCode As System.Net.HttpStatusCode = System.Net.HttpStatusCode.InternalServerError
                sendErrorResponse(context.Response, Nothing, errorMsg, statusCode)
            End If
            Return
        End Try

        If String.IsNullOrEmpty(token) OrElse hasClientToken Then
            'if token is not required or provided by the client, just fetch the response as is:
            fetchAndPassBackToClient(serverResponse, response, True)
        Else
            'credentials for secured service have come from configuration file:
            'it means that the proxy is responsible for making sure they were properly applied:

            'first attempt to send the request:
            Dim tokenRequired As Boolean = fetchAndPassBackToClient(serverResponse, response, False)


            'checking if previously used token has expired and needs to be renewed
            If tokenRequired Then
                log(TraceLevel.Info, "Renewing token and trying again.")
                'server returned error - potential cause: token has expired.
                'we'll do second attempt to call the server with renewed token:
                token = getNewTokenIfCredentialsAreSpecified(serverUrl, requestUri)
                serverResponse = forwardToServer(context, addTokenToUri(requestUri, token, tokenParamName), postBody)

                'storing the token in Application scope, to do not waste time on requesting new one untill it expires or the app is restarted.
                context.Application.Lock()
                context.Application("token_for_" & serverUrl.Url) = token
                context.Application.UnLock()

                fetchAndPassBackToClient(serverResponse, response, True)
            End If
        End If
        response.[End]()
    End Sub

    Public ReadOnly Property IsReusable() As Boolean Implements IHttpHandler.IsReusable
        Get
            Return True
        End Get
    End Property

    '*
    '* Private
    '

    Private Function readRequestPostBody(context As HttpContext) As Byte()
        If context.Request.InputStream.Length > 0 Then
            Dim bytes As Byte() = New Byte(context.Request.InputStream.Length - 1) {}
            context.Request.InputStream.Read(bytes, 0, CInt(context.Request.InputStream.Length))
            Return bytes
        End If
        Return New Byte(-1) {}
    End Function

    Private Function forwardToServer(context As HttpContext, uri As String, postBody As Byte(), Optional credentials As System.Net.NetworkCredential = Nothing) As System.Net.WebResponse
        Return If(postBody.Length > 0, doHTTPRequest(uri, postBody, "POST", context.Request.Headers("referer"), context.Request.ContentType, credentials), doHTTPRequest(uri, context.Request.HttpMethod, credentials))
    End Function

    ''' <summary>
    ''' Attempts to copy all headers from the fromResponse to the the toResponse.
    ''' </summary>
    ''' <param name="fromResponse">The response that we are copying the headers from</param>
    ''' <param name="toResponse">The response that we are copying the headers to</param>
    Private Sub copyHeaders(fromResponse As System.Net.WebResponse, toResponse As HttpResponse)
        For Each headerKey As String In fromResponse.Headers.AllKeys
            Select Case headerKey.ToLower()
                Case "content-type", "transfer-encoding", "accept-ranges"
                    ' Prevent requests for partial content
                    Continue For
                Case Else
                    toResponse.AddHeader(headerKey, fromResponse.Headers(headerKey))
                    Exit Select
            End Select
        Next
        toResponse.ContentType = fromResponse.ContentType
    End Sub

    Private Function fetchAndPassBackToClient(serverResponse As System.Net.WebResponse, clientResponse As HttpResponse, ignoreAuthenticationErrors As Boolean) As Boolean
        If serverResponse IsNot Nothing Then
            Using byteStream As Stream = serverResponse.GetResponseStream()
                ' Text response
                If serverResponse.ContentType.Contains("text") OrElse serverResponse.ContentType.Contains("json") OrElse serverResponse.ContentType.Contains("xml") Then
                    Using sr As New StreamReader(byteStream)
                        Dim strResponse As String = sr.ReadToEnd()
                        If Not ignoreAuthenticationErrors AndAlso strResponse.Contains("error") AndAlso (strResponse.Contains("""code"": 498") OrElse strResponse.Contains("""code"": 499") OrElse strResponse.Contains("""code"":498") OrElse strResponse.Contains("""code"":499")) Then
                            Return True
                        End If

                        'Copy the header info and the content to the reponse to client
                        copyHeaders(serverResponse, clientResponse)
                        clientResponse.Write(strResponse)
                    End Using
                Else
                    ' Binary response (image, lyr file, other binary file)

                    'Copy the header info to the reponse to client
                    copyHeaders(serverResponse, clientResponse)
                    ' Tell client not to cache the image since it's dynamic
                    clientResponse.CacheControl = "no-cache"
                    Dim buffer As Byte() = New Byte(32767) {}
                    Dim read As Integer
                    While (InlineAssignHelper(read, byteStream.Read(buffer, 0, buffer.Length))) > 0
                        clientResponse.OutputStream.Write(buffer, 0, read)
                    End While
                    clientResponse.OutputStream.Close()
                End If
                serverResponse.Close()
            End Using
        End If
        Return False
    End Function

    Private Function doHTTPRequest(uri As String, method As String, Optional credentials As System.Net.NetworkCredential = Nothing) As System.Net.WebResponse
        Dim bytes As Byte() = Nothing
        Dim contentType As [String] = Nothing
        log(TraceLevel.Info, "Sending request!")

        If method.Equals("POST") Then
            Dim uriArray As [String]() = uri.Split(New Char() {"?"c}, 2)
            uri = uriArray(0)
            If uriArray.Length > 1 Then
                contentType = "application/x-www-form-urlencoded"
                Dim queryString As [String] = uriArray(1)

                bytes = System.Text.Encoding.UTF8.GetBytes(queryString)
            End If
        End If

        Return doHTTPRequest(uri, bytes, method, PROXY_REFERER, contentType, credentials)
    End Function

    Private Function doHTTPRequest(uri As String, bytes As Byte(), method As String, referer As String, contentType As String, Optional credentials As System.Net.NetworkCredential = Nothing) As System.Net.WebResponse
        ''CALL THIS BEFORE ANY HTTPS CALLS THAT WILL FAIL WITH CERT ERROR


        '-- Per evitare errore: "Il certificato remoto non è stato ritenuto valido dalla procedura di convalida."
        System.Net.ServicePointManager.ServerCertificateValidationCallback =
            New System.Net.Security.RemoteCertificateValidationCallback(AddressOf customCertValidation)

        Dim req As System.Net.HttpWebRequest = DirectCast(System.Net.HttpWebRequest.Create(uri), System.Net.HttpWebRequest)
        req.ServicePoint.Expect100Continue = False
        req.Referer = referer
        req.Method = method

        ' Use the default system proxy
        req.Proxy = SYSTEM_PROXY

        If credentials IsNot Nothing Then
            req.Credentials = credentials
        End If

        If bytes IsNot Nothing AndAlso bytes.Length > 0 OrElse method = "POST" Then
            req.Method = "POST"
            req.ContentType = If(String.IsNullOrEmpty(contentType), "application/x-www-form-urlencoded", contentType)
            If bytes IsNot Nothing AndAlso bytes.Length > 0 Then
                req.ContentLength = bytes.Length
            End If
            Using outputStream As Stream = req.GetRequestStream()
                outputStream.Write(bytes, 0, bytes.Length)
            End Using
        End If
        Return req.GetResponse()
    End Function

    ''' <summary>
    ''' Errore doHTTPRequest per il certificato
    ''' "The underlying connection is closed: Could not establish trust relationship for the ssl/tls secure channel"
    ''' "The remote certificate is invalid according to the validation procedure"
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="cert"></param>
    ''' <param name="chain"></param>
    ''' <param name="errors"></param>
    ''' <returns></returns>
    Private Shared Function customCertValidation(ByVal sender As Object,
                                                    ByVal cert As System.Security.Cryptography.X509Certificates.X509Certificate,
                                                    ByVal chain As System.Security.Cryptography.X509Certificates.X509Chain,
                                                    ByVal errors As System.Net.Security.SslPolicyErrors) As Boolean
        Return True
    End Function

    Private Function webResponseToString(serverResponse As System.Net.WebResponse) As String
        Using byteStream As Stream = serverResponse.GetResponseStream()
            Using sr As New StreamReader(byteStream)
                Dim strResponse As String = sr.ReadToEnd()
                Return strResponse
            End Using
        End Using
    End Function

    Private Function getNewTokenIfCredentialsAreSpecified(su As ServerUrl, reqUrl As String) As String
        Dim token As String = ""
        Dim infoUrl As String = ""

        Dim isUserLogin As Boolean = Not [String].IsNullOrEmpty(su.Username) AndAlso Not [String].IsNullOrEmpty(su.Password)
        Dim isAppLogin As Boolean = Not [String].IsNullOrEmpty(su.ClientId) AndAlso Not [String].IsNullOrEmpty(su.ClientSecret)
        If isUserLogin OrElse isAppLogin Then
            log(TraceLevel.Info, "Matching credentials found in configuration file. OAuth 2.0 mode: " & isAppLogin)
            If isAppLogin Then
                'OAuth 2.0 mode authentication
                '"App Login" - authenticating using client_id and client_secret stored in config
                su.OAuth2Endpoint = If(String.IsNullOrEmpty(su.OAuth2Endpoint), DEFAULT_OAUTH, su.OAuth2Endpoint)
                If su.OAuth2Endpoint(su.OAuth2Endpoint.Length - 1) <> "/"c Then
                    su.OAuth2Endpoint += "/"
                End If
                log(TraceLevel.Info, "Service is secured by " & su.OAuth2Endpoint & ": getting new token...")
                Dim uri As String = su.OAuth2Endpoint & "token?client_id=" & su.ClientId & "&client_secret=" & su.ClientSecret & "&grant_type=client_credentials&f=json"
                Dim tokenResponse As String = webResponseToString(doHTTPRequest(uri, "POST"))
                token = extractToken(tokenResponse, "token")
                If Not String.IsNullOrEmpty(token) Then
                    token = exchangePortalTokenForServerToken(token, su)
                End If
            Else
                'standalone ArcGIS Server/ArcGIS Online token-based authentication

                'if a request is already being made to generate a token, just let it go
                If reqUrl.ToLower().Contains("/generatetoken") Then
                    Dim tokenResponse As String = webResponseToString(doHTTPRequest(reqUrl, "POST"))
                    token = extractToken(tokenResponse, "token")
                    Return token
                End If

                'lets look for '/rest/' in the requested URL (could be 'rest/services', 'rest/community'...)
                If reqUrl.ToLower().Contains("/rest/") Then
                    infoUrl = reqUrl.Substring(0, reqUrl.IndexOf("/rest/", StringComparison.OrdinalIgnoreCase))

                    'if we don't find 'rest', lets look for the portal specific 'sharing' instead
                ElseIf reqUrl.ToLower().Contains("/sharing/") Then
                    infoUrl = reqUrl.Substring(0, reqUrl.IndexOf("/sharing/", StringComparison.OrdinalIgnoreCase))
                    infoUrl = infoUrl & "/sharing"
                Else
                    Throw New ApplicationException("Unable to determine the correct URL to request a token to access private resources.")
                End If

                If infoUrl <> "" Then
                    log(TraceLevel.Info, " Querying security endpoint...")
                    infoUrl += "/rest/info?f=json"
                    'lets send a request to try and determine the URL of a token generator
                    Dim infoResponse As String = webResponseToString(doHTTPRequest(infoUrl, "GET"))
                    Dim tokenServiceUri As [String] = getJsonValue(infoResponse, "tokenServicesUrl")
                    If String.IsNullOrEmpty(tokenServiceUri) Then
                        Dim owningSystemUrl As String = getJsonValue(infoResponse, "owningSystemUrl")
                        If Not String.IsNullOrEmpty(owningSystemUrl) Then
                            tokenServiceUri = owningSystemUrl & "/sharing/generateToken"
                        End If
                    End If
                    If tokenServiceUri <> "" Then
                        log(TraceLevel.Info, " Service is secured by " & tokenServiceUri & ": getting new token...")
                        Dim uri As String = tokenServiceUri & "?f=json&request=getToken&referer=" & PROXY_REFERER & "&expiration=60&username=" & su.Username & "&password=" & su.Password
                        Dim tokenResponse As String = webResponseToString(doHTTPRequest(uri, "POST"))
                        token = extractToken(tokenResponse, "token")
                    End If


                End If
            End If
        End If
        Return token
    End Function

    Private Function checkWildcardSubdomain(allowedReferer As [String], requestedReferer As [String]) As Boolean
        Dim allowedRefererParts As [String]() = Regex.Split(allowedReferer, "(\.)")
        Dim refererParts As [String]() = Regex.Split(requestedReferer, "(\.)")

        If allowedRefererParts.Length <> refererParts.Length Then
            Return False
        End If

        Dim index As Integer = allowedRefererParts.Length - 1
        While index >= 0
            If allowedRefererParts(index).Equals(refererParts(index), StringComparison.OrdinalIgnoreCase) Then
                index = index - 1
            Else
                If allowedRefererParts(index).Equals("*") Then
                    index = index - 1
                    'next
                    Continue While
                End If
                Return False
            End If
        End While
        Return True
    End Function

    Private Function pathMatched(allowedRefererPath As [String], refererPath As [String]) As Boolean
        'If equal, return true
        If refererPath.Equals(allowedRefererPath) Then
            Return True
        End If

        'If the allowedRefererPath contain a ending star and match the begining part of referer, it is proper start with.
        If allowedRefererPath.EndsWith("*") Then
            Dim allowedRefererPathShort As [String] = allowedRefererPath.Substring(0, allowedRefererPath.Length - 1)
            If refererPath.ToLower().StartsWith(allowedRefererPathShort.ToLower()) Then
                Return True
            End If
        End If
        Return False
    End Function

    Private Function domainMatched(allowedRefererDomain As [String], refererDomain As [String]) As Boolean
        If allowedRefererDomain.Equals(refererDomain) Then
            Return True
        End If

        'try if the allowed referer contains wildcard for subdomain
        If allowedRefererDomain.Contains("*") Then
            If checkWildcardSubdomain(allowedRefererDomain, refererDomain) Then
                'return true if match wildcard subdomain
                Return True
            End If
        End If

        Return False
    End Function

    Private Function protocolMatch(allowedRefererProtocol As [String], refererProtocol As [String]) As Boolean
        Return allowedRefererProtocol.Equals(refererProtocol)
    End Function

    Private Function getDomainfromURL(url As [String], protocol As [String]) As [String]
        Dim domain As [String] = url.Substring(protocol.Length + 3)

        domain = If(domain.IndexOf("/"c) >= 0, domain.Substring(0, domain.IndexOf("/"c)), domain)

        Return domain
    End Function

    Private Function checkReferer(allowedReferers As [String](), referer As [String]) As Boolean
        If allowedReferers IsNot Nothing AndAlso allowedReferers.Length > 0 Then
            If allowedReferers.Length = 1 AndAlso allowedReferers(0).Equals("*") Then
                Return True
            End If
            'speed-up
            For Each allowedReferer As [String] In allowedReferers

                'Parse the protocol, domain and path of the referer
                Dim refererProtocol As [String] = If(referer.StartsWith("https://"), "https", "http")
                Dim refererDomain As [String] = getDomainfromURL(referer, refererProtocol)
                Dim refererPath As [String] = referer.Substring(refererProtocol.Length + 3 + refererDomain.Length)


                Dim allowedRefererCannonical As [String] = Nothing

                'since the allowedReferer can be a malformed URL, we first construct a valid one to be compared with referer
                'if allowedReferer starts with https:// or http://, then exact match is required
                If allowedReferer.StartsWith("https://") OrElse allowedReferer.StartsWith("http://") Then

                    allowedRefererCannonical = allowedReferer
                Else

                    Dim protocol As [String] = refererProtocol
                    'if allowedReferer starts with "//" or no protocol, we use the one from refererURL to prefix to allowedReferer.
                    If allowedReferer.StartsWith("//") Then
                        allowedRefererCannonical = protocol & ":" & allowedReferer
                    Else
                        'if the allowedReferer looks like "example.esri.com"
                        allowedRefererCannonical = protocol & "://" & allowedReferer
                    End If
                End If

                'parse the protocol, domain and the path of the allowedReferer
                Dim allowedRefererProtocol As [String] = If(allowedRefererCannonical.StartsWith("https://"), "https", "http")
                Dim allowedRefererDomain As [String] = getDomainfromURL(allowedRefererCannonical, allowedRefererProtocol)
                Dim allowedRefererPath As [String] = allowedRefererCannonical.Substring(allowedRefererProtocol.Length + 3 + allowedRefererDomain.Length)

                'Check if both domain and path match
                If protocolMatch(allowedRefererProtocol, refererProtocol) AndAlso domainMatched(allowedRefererDomain, refererDomain) AndAlso pathMatched(allowedRefererPath, refererPath) Then
                    Return True
                End If
            Next
            'no-match
            Return False
        End If
        Return True
        'when allowedReferer is null, then allow everything
    End Function

    Private Function exchangePortalTokenForServerToken(portalToken As String, su As ServerUrl) As String
        'ideally, we should POST the token request
        log(TraceLevel.Info, " Exchanging Portal token for Server-specific token for " & su.Url & "...")
        Dim uri As String = su.OAuth2Endpoint.Substring(0, su.OAuth2Endpoint.IndexOf("/oauth2/", StringComparison.OrdinalIgnoreCase)) & "/generateToken?token=" & portalToken & "&serverURL=" & su.Url & "&f=json"
        Dim tokenResponse As String = webResponseToString(doHTTPRequest(uri, "GET"))
        Return extractToken(tokenResponse, "token")
    End Function


    Private Shared Sub sendPingResponse(response As HttpResponse, version As [String], config As [String], log As [String])
        response.AddHeader("Content-Type", "application/json")
        response.AddHeader("Accept-Encoding", "gzip")
        Dim message As [String] = "{ " & """Proxy Version"": """ & version & """" & ", ""Configuration File"": """ & config & """" & ", ""Log File"": """ & log & """" & "}"
        response.StatusCode = 200
        response.Write(message)
        response.Flush()
    End Sub

    Private Shared Sub sendErrorResponse(response As HttpResponse, errorDetails As [String], errorMessage As [String], errorCode As System.Net.HttpStatusCode)
        Dim message As [String] = String.Format("{{""error"": {{""code"": {0},""message"":""{1}""", CInt(errorCode), errorMessage)
        If Not String.IsNullOrEmpty(errorDetails) Then
            message += String.Format(",""details"":[""message"":""{0}""]", errorDetails)
        End If
        message += "}}"
        response.StatusCode = CInt(errorCode)
        'custom status description for when the rate limit has been exceeded
        If response.StatusCode = 429 Then
            response.StatusDescription = "Too Many Requests"
        End If
        'this displays our customized error messages instead of IIS's custom errors
        response.TrySkipIisCustomErrors = True
        response.Write(message)
        response.Flush()
    End Sub

    Private Shared Function getClientIp(request As HttpRequest) As String
        If request Is Nothing Then
            Return Nothing
        End If
        Dim remoteAddr As String = request.ServerVariables("HTTP_X_FORWARDED_FOR")
        If String.IsNullOrWhiteSpace(remoteAddr) Then
            remoteAddr = request.ServerVariables("REMOTE_ADDR")
        Else
            ' the HTTP_X_FORWARDED_FOR may contain an array of IP, this can happen if you connect through a proxy.
            Dim ipRange As String() = remoteAddr.Split(","c)
            remoteAddr = ipRange(ipRange.Length - 1)
        End If
        Return remoteAddr
    End Function

    Private Function addTokenToUri(uri As String, token As String, tokenParamName As String) As String
        If Not [String].IsNullOrEmpty(token) Then
            uri += If(uri.Contains("?"), "&" & tokenParamName & "=" & token, "?" & tokenParamName & "=" & token)
        End If
        Return uri
    End Function

    Private Function extractToken(tokenResponse As String, key As String) As String
        Dim token As String = getJsonValue(tokenResponse, key)
        If String.IsNullOrEmpty(token) Then
            log(TraceLevel.[Error], " Token cannot be obtained: " & tokenResponse)
        Else
            log(TraceLevel.Info, " Token obtained: " & token)
        End If
        Return token
    End Function

    Private Function getJsonValue(text As String, key As String) As String
        Dim i As Integer = text.IndexOf(key)
        Dim value As [String] = ""
        If i > -1 Then
            value = text.Substring(text.IndexOf(":"c, i) + 1).Trim()
            value = If(value.Length > 0 AndAlso value(0) = """"c, value.Substring(1, value.IndexOf(""""c, 1) - 1), InlineAssignHelper(value, value.Substring(0, Math.Max(0, Math.Min(Math.Min(value.IndexOf(","), value.IndexOf("]")), value.IndexOf("}"))))))
        End If
        Return value
    End Function

    Private Sub cleanUpRatemap(ratemap As ConcurrentDictionary(Of String, RateMeter))
        For Each key As String In ratemap.Keys
            Dim rate As RateMeter = ratemap(key)
            If rate.canBeCleaned() Then
                ratemap.TryRemove(key, rate)
            End If
        Next
    End Sub

    '*
    '* Static
    '

    Private Shared Function getConfig() As ProxyConfig
        Dim config As ProxyConfig = ProxyConfig.GetCurrentConfig()
        If config IsNot Nothing Then
            Return config
        Else
            Throw New ApplicationException("The proxy configuration file cannot be found, or is not readable.")
        End If
    End Function

    'writing Log file
    Private Shared Sub log(logLevel As TraceLevel, msg As String)
        Dim logMessage As String = String.Format("{0} {1}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), msg)

        Dim config As ProxyConfig = ProxyConfig.GetCurrentConfig()
        Dim ts As TraceSwitch = Nothing

        If config.LogLevel IsNot Nothing Then
            ts = New TraceSwitch("TraceLevelSwitch2", "TraceSwitch in the proxy.config file", config.LogLevel)
        Else
            ts = New TraceSwitch("TraceLevelSwitch2", "TraceSwitch in the proxy.config file", "Error")
            config.LogLevel = "Error"
        End If
        'ts.Level = TraceLevel.Verbose '//modifica del 24/03/2017
        Trace.WriteLineIf(logLevel <= ts.Level, logMessage)
    End Sub

    Private Shared _lockobject As New Object()
    Private Shared Function InlineAssignHelper(Of T)(ByRef target As T, value As T) As T
        target = value
        Return value
    End Function

End Class

Class LogTraceListener
    Inherits TraceListener
    Private Shared _lockobject As New Object()
    Private Function GetLogFile() As String
        Dim config As ProxyConfig = ProxyConfig.GetCurrentConfig()
        Dim log As String = String.Empty
        If config.LogFile IsNot Nothing Then
            log = config.LogFile
            If Not log.Contains("\") OrElse log.Contains(".\") Then
                If log.Contains(".\") Then
                    'If this type of relative pathing .\log.txt
                    log = log.Replace(".\", "")
                End If


                log = IO.Path.Combine(ProxyConfig.GetProxyConfigFolderPath, log)
            End If

        End If
        Return log
    End Function
    Public Overrides Sub Write(message As String)
        Dim log As String = GetLogFile()
        If Not String.IsNullOrEmpty(log) Then
            SyncLock _lockobject
                Using sw As StreamWriter = File.AppendText(log)
                    sw.Write(message)
                End Using
            End SyncLock
        End If

    End Sub


    Public Overrides Sub WriteLine(message As String)
        Dim log As String = GetLogFile()
        If Not String.IsNullOrEmpty(log) Then
            SyncLock _lockobject
                Using sw As StreamWriter = File.AppendText(log)
                    sw.WriteLine(message)
                End Using
            End SyncLock
        End If
    End Sub

End Class


<XmlRoot("ProxyConfig")>
Public Class ProxyConfig
    Private Shared _lockobject As New Object()
    Public Shared Function LoadProxyConfig(fileName As String) As ProxyConfig
        Dim config As ProxyConfig = Nothing
        SyncLock _lockobject
            If System.IO.File.Exists(fileName) Then
                Dim reader As New XmlSerializer(GetType(ProxyConfig))
                Using file As New System.IO.StreamReader(fileName)
                    Try
                        config = DirectCast(reader.Deserialize(file), ProxyConfig)
                    Catch ex As Exception
                        Throw ex
                    End Try
                End Using
            End If
        End SyncLock
        Return config
    End Function

    Public Shared Function GetCurrentConfig() As ProxyConfig
        Dim config As ProxyConfig = TryCast(HttpRuntime.Cache("proxyConfig"), ProxyConfig)
        If config Is Nothing Then
            Dim fileName As String = GetProxyConfigFilePath()
            config = LoadProxyConfig(fileName)
            If config IsNot Nothing Then
                Dim dep As New CacheDependency(fileName)
                HttpRuntime.Cache.Insert("proxyConfig", config, dep)
            End If
        End If
        Return config
    End Function

    'referer
    'create an array with valid referers using the allowedReferers String that is defined in the proxy.config
    Public Shared Function GetAllowedReferersArray() As [String]()
        If m_allowedReferers Is Nothing Then
            Return Nothing
        End If

        Return m_allowedReferers.Split(","c)
    End Function

    'referer
    'check if URL starts with prefix...
    Public Shared Function isUrlPrefixMatch(prefix As [String], uri As [String]) As Boolean

        Return uri.ToLower().StartsWith(prefix.ToLower()) OrElse uri.ToLower().Replace("https://", "http://").StartsWith(prefix.ToLower()) OrElse uri.ToLower().Substring(uri.IndexOf("//")).StartsWith(prefix.ToLower())
    End Function

    Private m_serverUrls As ServerUrl()
    Public m_logFile As [String]
    Public m_logLevel As [String]
    Private m_mustMatch As Boolean
    'referer
    Shared m_allowedReferers As [String]

    <XmlArray("serverUrls")>
    <XmlArrayItem("serverUrl")>
    Public Property ServerUrls() As ServerUrl()
        Get
            Return Me.m_serverUrls
        End Get
        Set
            Me.m_serverUrls = Value
        End Set
    End Property
    <XmlAttribute("mustMatch")>
    Public Property MustMatch() As Boolean
        Get
            Return m_mustMatch
        End Get
        Set
            m_mustMatch = Value
        End Set
    End Property

    'logFile
    <XmlAttribute("logFile")>
    Public Property LogFile() As [String]
        Get
            Return m_logFile
        End Get
        Set
            m_logFile = Value
        End Set
    End Property

    'logLevel
    <XmlAttribute("logLevel")>
    Public Property LogLevel() As [String]
        Get
            Return m_logLevel
        End Get
        Set
            m_logLevel = Value
        End Set
    End Property


    'referer
    <XmlAttribute("allowedReferers")>
    Public Property AllowedReferers() As String
        Get
            Return m_allowedReferers
        End Get
        Set
            m_allowedReferers = Regex.Replace(Value, "\s", "")
        End Set
    End Property

    Public Function GetConfigServerUrl(uri As String) As ServerUrl
        'split both request and proxy.config urls and compare them
        Dim uriParts As String() = uri.Split(New Char() {"/"c, "?"c}, StringSplitOptions.RemoveEmptyEntries)
        Dim configUriParts As String() = New String() {}

        For Each su As ServerUrl In m_serverUrls
            'if a relative path is specified in the proxy.config, append what's in the request itself
            If Not su.Url.StartsWith("http") Then
                su.Url = su.Url.Insert(0, uriParts(0))
            End If

            configUriParts = su.Url.Split(New Char() {"/"c, "?"c}, StringSplitOptions.RemoveEmptyEntries)

            'if the request has less parts than the config, don't allow
            If configUriParts.Length > uriParts.Length Then
                Continue For
            End If

            Dim i As Integer = 0
            For i = 0 To configUriParts.Length - 1

                If Not configUriParts(i).ToLower().Equals(uriParts(i).ToLower()) Then
                    Exit For
                End If
            Next
            If i = configUriParts.Length Then
                'if the urls don't match exactly, and the individual matchAll tag is 'false', don't allow
                If configUriParts.Length = uriParts.Length OrElse su.MatchAll Then
                    Return su
                End If
            End If
        Next

        If Not m_mustMatch Then
            Return New ServerUrl(uri)
        Else
            Throw New ArgumentException("Proxy has not been set up for this URL. Make sure there is a serverUrl in the configuration file that matches: " & uri)
        End If
    End Function

    Public Shared Function GetProxyConfigFilePath()
        If HttpContext.Current.IsDebuggingEnabled Then
            Return HttpContext.Current.Server.MapPath("~/proxy/proxy.Debug.config")
        Else
            Return HttpContext.Current.Server.MapPath("~/proxy/proxy.Release.config")
        End If
    End Function

    Public Shared Function GetProxyConfigFolderPath()
        Return HttpContext.Current.Server.MapPath("~/proxy/")

    End Function
End Class

Public Class ServerUrl
    Private m_url As String
    Private m_hostRedirect As String
    Private m_matchAll As Boolean
    Private m_oauth2Endpoint As String
    Private m_domain As String
    Private m_username As String
    Private m_password As String
    Private m_clientId As String
    Private m_clientSecret As String
    Private m_accessToken As String
    Private m_tokenParamName As String
    Private m_rateLimit As String
    Private m_rateLimitPeriod As String

    Private Sub New()
    End Sub

    Public Sub New(url As [String])
        Me.m_url = url
    End Sub

    <XmlAttribute("url")>
    Public Property Url() As String
        Get
            Return m_url
        End Get
        Set
            m_url = Value
        End Set
    End Property
    <XmlAttribute("hostRedirect")>
    Public Property HostRedirect() As String
        Get
            Return m_hostRedirect
        End Get
        Set
            m_hostRedirect = Value
        End Set
    End Property
    <XmlAttribute("matchAll")>
    Public Property MatchAll() As Boolean
        Get
            Return m_matchAll
        End Get
        Set
            m_matchAll = Value
        End Set
    End Property
    <XmlAttribute("oauth2Endpoint")>
    Public Property OAuth2Endpoint() As String
        Get
            Return m_oauth2Endpoint
        End Get
        Set
            m_oauth2Endpoint = Value
        End Set
    End Property
    <XmlAttribute("domain")>
    Public Property Domain() As String
        Get
            Return m_domain
        End Get
        Set
            m_domain = Value
        End Set
    End Property
    <XmlAttribute("username")>
    Public Property Username() As String
        Get
            Return m_username
        End Get
        Set
            m_username = Value
        End Set
    End Property
    <XmlAttribute("password")>
    Public Property Password() As String
        Get
            Return m_password
        End Get
        Set
            m_password = Value
        End Set
    End Property
    <XmlAttribute("clientId")>
    Public Property ClientId() As String
        Get
            Return m_clientId
        End Get
        Set
            m_clientId = Value
        End Set
    End Property
    <XmlAttribute("clientSecret")>
    Public Property ClientSecret() As String
        Get
            Return m_clientSecret
        End Get
        Set
            m_clientSecret = Value
        End Set
    End Property
    <XmlAttribute("accessToken")>
    Public Property AccessToken() As String
        Get
            Return m_accessToken
        End Get
        Set
            m_accessToken = Value
        End Set
    End Property
    <XmlAttribute("tokenParamName")>
    Public Property TokenParamName() As String
        Get
            Return m_tokenParamName
        End Get
        Set
            m_tokenParamName = Value
        End Set
    End Property
    <XmlAttribute("rateLimit")>
    Public Property RateLimit() As Integer
        Get
            Return If(String.IsNullOrEmpty(m_rateLimit), -1, Integer.Parse(m_rateLimit))
        End Get
        Set
            m_rateLimit = Value.ToString()
        End Set
    End Property
    <XmlAttribute("rateLimitPeriod")>
    Public Property RateLimitPeriod() As Integer
        Get
            Return If(String.IsNullOrEmpty(m_rateLimitPeriod), 60, Integer.Parse(m_rateLimitPeriod))
        End Get
        Set
            m_rateLimitPeriod = Value.ToString()
        End Set
    End Property
End Class
