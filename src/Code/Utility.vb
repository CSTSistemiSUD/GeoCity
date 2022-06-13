Imports Microsoft.VisualBasic
Imports System.Data
Public Class Utility


    Public Shared Function VerificaAppPath() As String
    Dim codice As String = ""
    If HttpContext.Current.IsDebuggingEnabled Then
      codice = "B644"
    Else
      Dim appPath As String = HttpContext.Current.Request.ApplicationPath.Substring(1)
      Dim ssh As New SSHelper
      Dim dbr As DatabaseOperationResult = ssh.GetData(String.Format("SELECT CODICE FROM PRJ WHERE SERVIZIO = '{0}'", appPath))
      Dim row As DataRow = dbr.GetRow(0, 0)
      If Not row Is Nothing Then
        codice = row("CODICE")
      End If
    End If

    Return codice
  End Function


    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="year"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Function GetCalendarTable(year As Int32) As DataTable
        Dim curCulture = System.Globalization.CultureInfo.CurrentCulture
        Dim firstYearDate = New Date(year, 1, 1)
        Dim currentDate = firstYearDate
        Dim tblCalendar = New DataTable
        tblCalendar.Columns.Add(New DataColumn("MonthName"))

        Dim maxDiff = 0
        For m = 1 To 12
            'find max difference between first year's weekday and month's first weekday
            'if the latter is earlier in the week, it is considered to be in the next week
            Dim monthFirstWeekDay = New Date(year, m, 1).DayOfWeek
            Dim diff = (7 + (monthFirstWeekDay - firstYearDate.DayOfWeek)) Mod 7
            If diff > maxDiff Then
                maxDiff = diff
            End If
        Next

        Dim weekDayNum = curCulture.Calendar.GetDaysInMonth(year, 1) + maxDiff
        ' Create DataColumns with weekday as ColumnsName
        For wd = 1 To weekDayNum
            Dim weekday = currentDate.ToString("ddd")
            Dim weekInMonth = (From col In tblCalendar.Columns.Cast(Of DataColumn)()
                               Where col.ColumnName Like String.Format("{0} W#", weekday)).Count + 1
            Dim columnName = String.Format("{0} W{1}", weekday, weekInMonth)
            tblCalendar.Columns.Add(New DataColumn(columnName))
            currentDate = currentDate.AddDays(1)
        Next

        ' Create the DataRows(every month)
        For m = 1 To 12
            Dim daysInMonth = curCulture.Calendar.GetDaysInMonth(year, m)
            Dim firstMonthDate = New Date(year, m, 1)
            Dim daysBefore = (7 + (firstMonthDate.DayOfWeek - firstYearDate.DayOfWeek)) Mod 7
            Dim daysBehind = tblCalendar.Columns.Count - (daysBefore + daysInMonth) - 1

            Dim monthDays = From d In Enumerable.Range(1, daysInMonth)
                            Select New With {.Day = d.ToString}
            Dim emptyDaysBefore = From d In Enumerable.Range(1, daysBefore)
                                  Select New With {.Day = ""}
            Dim emptyDaysAfter = From d In Enumerable.Range(1, daysBehind)
                                 Select New With {.Day = ""}

            Dim monthName = curCulture.DateTimeFormat.GetMonthName(m)
            ' piece together parts
            Dim allFields = ({New With {.Day = monthName}}.
               Union(emptyDaysBefore).
               Union(monthDays).
               Union(emptyDaysAfter).
               Select(Function(d) d.Day)).ToArray
            tblCalendar.Rows.Add(allFields)
        Next

        Return tblCalendar

        '// How to
        '		Dim calendario As DataTable = GetCalendarTable(Now.Year)
        '		Dim dg As New GridView
        '		With dg
        '			.ID = "tabella"
        '
        '			.AutoGenerateColumns = True
        '
        '			.BorderStyle = BorderStyle.None
        '
        '			.UseAccessibleHeader = True
        '			.Width = New Unit(100, UnitType.Percentage)
        '			.DataSource = calendario
        '			.ShowHeader = True
        '			.ShowFooter = True
        '			.DataBind()
        '		End With
        '		Me.PlaceHolder1.Controls.Clear()
        '		Me.PlaceHolder1.Controls.Add(dg)

    End Function

    ''' <summary>
    ''' Controllo VALUE ADDED TAX (Partita IVA)
    ''' </summary>
    ''' <param name="country"></param>
    ''' <param name="vat_number"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Function IsVATValid(country As String, vat_number As String) As Boolean

        Dim regExp As String = String.Empty

        Select Case country

            Case "DE", "EE", "EL", "PT"
                'VAT Numbers format verification (Estonia, Germany, Greece, Portugal) with support for optional member state definition.
                'VAT Numbers format verification (DE, EE, EL, PT): "((EE|EL|DE|PT)-?)?[0-9]{9}"

                regExp = "((EE|EL|DE|PT)-?)?[0-9]{9}"

            Case "FI", "HU", "LU", "MT", "SI"
                'VAT Numbers format verification (Finland, Hungary, Luxemburg, Malta, Slovenia) with support for optional member state definition.
                'VAT Numbers format verification (FI, HU, LU, MT, SI): "((FI|HU|LU|MT|SI)-?)?[0-9]{8}"

                regExp = "((FI|HU|LU|MT|SI)-?)?[0-9]{8}"

            Case "PL", "SK"
                'VAT Numbers format verification (Poland, Slovakia) with support for optional member state definition.
                'VAT Numbers format verification (PL, SK): "((PL|SK)-?)?[0-9]{10}"

                regExp = "((PL|SK)-?)?[0-9]{10}"

            Case "IT", "LV"
                'VAT Numbers format verification (Italy, Latvia) with support for optional member state definition.
                'VAT Numbers format verification (IT, LV): "((IT|LV)-?)?[0-9]{11}"

                regExp = "((IT|LV)-?)?[0-9]{11}"

            Case "SE"
                'VAT Numbers format verification (Sweden) with support for optional member state definition.
                'VAT Numbers format verification (SE): "(SE-?)?[0-9]{12}"

                regExp = "(SE-?)?[0-9]{12}"

            Case "BE"
                'VAT Numbers format verification (Belgium) with support for optional member state definition.
                'VAT Numbers format verification (BE): "(BE-?)?0?[0-9]{9}"

                regExp = "(BE-?)?0?[0-9]{9}"

            Case "CY"
                'VAT Numbers format verification (Cyprus) with support for optional member state definition.
                'VAT Numbers format verification (CY): "(CY-?)?[0-9]{8}[A-Z]"

                regExp = "(CY-?)?[0-9]{8}[A-Z]"

            Case "CZ"
                'VAT Numbers format verification (Czech Republic) with support for optional member state definition.
                'VAT Numbers format verification (CZ): "(CZ-?)?[0-9]{8,10}"

                regExp = "(CZ-?)?[0-9]{8,10}"

            Case "DK"
                'VAT Numbers format verification (Denmark) with support for optional member state definition.
                'VAT Numbers format verification (DK): "(DK-?)?([0-9]{2}\ ?){3}[0-9]{2}"

                regExp = "(DK-?)?([0-9]{2}\ ?){3}[0-9]{2}"

            Case "ES"
                'VAT Numbers format verification (Spain) with support for optional member state definition.
                'VAT Numbers format verification (ES): "(ES-?)?([0-9A-Z][0-9]{7}[A-Z])|([A-Z][0-9]{7}[0-9A-Z])"

                regExp = "(ES-?)?([0-9A-Z][0-9]{7}[A-Z])|([A-Z][0-9]{7}[0-9A-Z])"

            Case "FR"
                'VAT Numbers format verification (France) with support for optional member state definition.
                'VAT Numbers format verification (FR): "(FR-?)?[0-9A-Z]{2}\ ?[0-9]{9}"

                regExp = "(FR-?)?[0-9A-Z]{2}\ ?[0-9]{9}"

            Case "GB"
                'VAT Numbers format verification (United Kingdom) with support for optional member state definition.
                'VAT Numbers format verification (GB)

                regExp = "(GB-?)?([1-9][0-9]{2}\ ?[0-9]{4}\ ?[0-9]{2})|([1-9][0-9]{2}\ ?[0-9]{4}\ ?[0-9]{2}\ ?[0-9]{3})|((GD|HA)[0-9]{3})"

            Case "IE"
                'VAT Numbers format verification (Ireland) with support for optional member state definition.
                'VAT Numbers format verification (IE)

                regExp = "(IE-?)?[0-9][0-9A-Z\+\*][0-9]{5}[A-Z]"

            Case "LT"
                'VAT Numbers format verification (Lithuania) with support for optional member state definition.
                'VAT Numbers format verification (LT)

                regExp = "(LT-?)?([0-9]{9}|[0-9]{12})"

            Case "NL"
                'VAT Numbers format verification (The Netherlands) with support for optional member state definition.
                'VAT Numbers format verification (NL)

                regExp = "(NL-?)?[0-9]{9}B[0-9]{2}"

        End Select

        If String.IsNullOrEmpty(regExp) Then
            Return True
        Else
            Dim rgx As New Regex(regExp)
            Dim vat As Match = rgx.Match(vat_number)
            Return vat.Value = vat_number

        End If


    End Function

    ''' <summary>
    ''' Pattern per verificare Codice Fiscale Italiano (TIN, Tax Identification Number). 
    ''' Verifica del codice fiscale per persone fisiche e persone giuridiche, anche in caso di omocodia. 
    ''' Codice Fiscale, CF, omocodia, persone fisiche, persone giuridiche, italian fiscal code, TIN, Tax Identification Number.
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Function IsTINValid(tin_number As String) As Boolean
        Dim pattern As String = "^([A-Za-z]{6}[0-9lmnpqrstuvLMNPQRSTUV]{2}[abcdehlmprstABCDEHLMPRST]{1}[0-9lmnpqrstuvLMNPQRSTUV]{2}[A-Za-z]{1}[0-9lmnpqrstuvLMNPQRSTUV]{3}[A-Za-z]{1})|([0-9]{11})$"
        Dim rgx As New Regex(pattern)
        Dim tin As Match = rgx.Match(tin_number)
        Return tin.Value = tin_number
    End Function

    Public Shared Function IsEMailValid(email As String) As Boolean
        Dim pattern As String = "(?:[a-z0-9!#$%&'*+/=?^_'{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_'{|}~-]+)*|""(?:[\x01-\x08\x0b\x0c\x0e-\x1f\x21\x23-\x5b\x5d-\x7f]|\\[\x01-\x09\x0b\x0c\x0e-\x7f])*"")@(?:(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?|\[(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?|[a-z0-9-]*[a-z0-9]:(?:[\x01-\x08\x0b\x0c\x0e-\x1f\x21-\x5a\x53-\x7f]|\\[\x01-\x09\x0b\x0c\x0e-\x7f])+)\])"
        Return Regex.IsMatch((email.ToString().ToLower()), pattern)
    End Function

    Public Shared Function GetCountryCode() As String
        Dim languages As String() = HttpContext.Current.Request.UserLanguages
        Dim culture As Globalization.CultureInfo = Nothing
        Dim region As Globalization.RegionInfo = Nothing
        Dim lang As String = String.Empty
        Dim code As String = String.Empty
        Try
            lang = languages(0).ToLowerInvariant.Trim
            culture = Globalization.CultureInfo.CreateSpecificCulture(lang)
            region = New Globalization.RegionInfo(culture.LCID)
            code = culture.TwoLetterISOLanguageName.ToUpper()
        Catch ex As Exception

        End Try
        Return code
    End Function

    Public Shared Function ParseDate(DateString As String, Required As Boolean)
        Dim result As Object
        Try
            If String.IsNullOrEmpty(DateString) Then
                Throw New FormatException("DATE IS NOTHING")
            Else

                result = Date.ParseExact(DateString.Substring(0, 10), "dd/MM/yyyy", Globalization.CultureInfo.InvariantCulture)

            End If


        Catch e As FormatException
            If Required Then
                result = Now
            Else
                result = DBNull.Value
            End If

        End Try
        Return result
    End Function

    Public Shared Function IsDouble(col As DataColumn) As Boolean

        If col Is Nothing Then
            Return False
        End If
        ' Make this const
        'Dim numericTypes = New Type() {GetType([Byte]), GetType([Decimal]), GetType([Double]), GetType(Int16), GetType(Int32), GetType(Int64), _
        'GetType([SByte]), GetType([Single]), GetType(UInt16), GetType(UInt32), GetType(UInt64)}
        Dim numericTypes = New Type() {GetType([Byte]), GetType([Decimal]), GetType([Double]),
         GetType([SByte]), GetType([Single])}

        Return numericTypes.Contains(col.DataType)
        'Boolean
        'Byte 
        'Char 
        'DateTime 
        'Decimal
        'Double
        'Int16
        'Int32
        'Int64
        'SByte
        'Single
        'String
        'TimeSpan
        'UInt16
        'UInt32
        'UInt64 
        'Byte[]

    End Function

    Public Shared Function GetLastDayOfMonth(yyyy As Integer, MM As Integer) As DateTime
        Return DateSerial(yyyy, MM + 1, 0)
    End Function

    Public Shared Function GenerateThumbnail(loBMP As Drawing.Image,
                                             Optional thumbnailWidth As Integer = 32,
                                             Optional thumbnailHeight As Integer = 32) As Drawing.Bitmap



        Dim bmpOut As System.Drawing.Bitmap = Nothing

        Try
            'loBMP = Drawing.Image.FromStream(New IO.MemoryStream(img_buffer))
            'Dim loFormat As Drawing.Imaging.ImageFormat = loBMP.RawFormat
            'Dim lnRatio As Decimal
            'Dim lnNewWidth As Integer = 0
            'Dim lnNewHeight As Integer = 0

            ''*** If the image is smaller than a thumbnail just return it

            'If loBMP.Width < thumbnailWidth AndAlso loBMP.Height < thumbnailHeight Then
            '	lnNewWidth = loBMP.Width
            '	lnNewHeight = loBMP.Height
            'Else
            '	If loBMP.Width < loBMP.Height Then
            '		lnRatio = CDec(thumbnailWidth) / loBMP.Width
            '		lnNewWidth = thumbnailWidth
            '		Dim lnTemp As Decimal = loBMP.Height * lnRatio
            '		lnNewHeight = CInt(lnTemp)
            '	Else
            '		lnRatio = CDec(thumbnailHeight) / loBMP.Height
            '		lnNewHeight = thumbnailHeight
            '		Dim lnTemp As Decimal = loBMP.Width * lnRatio
            '		lnNewWidth = CInt(lnTemp)
            '	End If
            'End If

            '' System.Drawing.Image imgOut = loBMP.GetThumbnailImage(lnNewWidth,lnNewHeight, null,IntPtr.Zero);

            '' *** This code creates cleaner (though bigger) thumbnails and properly
            '' *** and handles GIF files better by generating a white background for
            '' *** transparent images (as opposed to black)

            'bmpOut = New Drawing.Bitmap(lnNewWidth, lnNewHeight)
            'Dim g As Drawing.Graphics = Drawing.Graphics.FromImage(bmpOut)
            'g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic
            'g.FillRectangle(Drawing.Brushes.White, 0, 0, lnNewWidth, lnNewHeight)
            'g.DrawImage(loBMP, 0, 0, lnNewWidth, lnNewHeight)


            Dim imgWidth As Integer = loBMP.PhysicalDimension.Width
            Dim imgHeight As Integer = loBMP.PhysicalDimension.Height
            Dim imgOriginalSize As Double = IIf(imgHeight > imgWidth, imgHeight, imgWidth)
            Dim imgDesiredSize As Double = IIf(thumbnailHeight > thumbnailWidth, thumbnailHeight, thumbnailWidth)
            Dim imgResize As Double = IIf(imgOriginalSize <= imgDesiredSize, 1, imgDesiredSize / imgOriginalSize)
            imgWidth *= imgResize
            imgHeight *= imgResize

            'bmpOut = loBMP.GetThumbnailImage(CInt(imgWidth), CInt(imgHeight), Nothing, IntPtr.Zero)
            bmpOut = New Drawing.Bitmap(imgWidth, imgHeight)
            Dim g As Drawing.Graphics = Drawing.Graphics.FromImage(bmpOut)
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic
            g.FillRectangle(Drawing.Brushes.White, 0, 0, imgWidth, imgHeight)
            g.DrawImage(loBMP, 0, 0, imgWidth, imgHeight)

        Catch

            Return Nothing
        Finally
            If Not loBMP Is Nothing Then loBMP.Dispose()
        End Try




        Return bmpOut
    End Function

    Public Shared Function CreateArray(num_elements As Integer, int_value As Integer, element_type As Type) As Array
        Dim arr As Array = Array.CreateInstance(element_type, num_elements)
        For i As Integer = 0 To num_elements - 1
            arr.SetValue(int_value, i)
        Next
        Return arr
    End Function

    ''' <summary>
    ''' trims text to a maximum length, splitting at last word break, and (optionally) appending ellipses 
    ''' </summary>
    ''' <param name="s_input"></param>
    ''' <param name="i_length"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>

    Public Shared Function TrimBetter(s_input As String, i_length As Integer) As String
        '//strip leading and trailing whitespace
        s_input = Trim(s_input)
        '//no need to trim, already shorter than trim length
        If (s_input.Length <= i_length) Then
            Return s_input
        End If
        '//leave space for the ellipses (...)
        i_length -= 3
        '//this would be dumb, but I've seen dumber
        If i_length <= 0 Then
            Return ""
        End If

        Dim s_trimmed As String
        ' //find last space within length
        ' //(add 1 to length to allow space after last character - it may be your lucky day)

        Dim i_last_space As Integer = s_input.LastIndexOf(" ", i_length + 1)
        If i_last_space < 0 Then
            '//lame, no spaces - fallback to pure substring
            s_trimmed = s_input.Substring(0, i_length)
        Else
            s_trimmed = s_input.Substring(0, i_last_space)
        End If

        s_trimmed += "..."
        Return s_trimmed
    End Function

    Public Shared Function GetJsUrl() As String
        Dim jsParams As New Hashtable
        'Dim page_Url As String = Page.ResolveUrl(String.Concat("~", HttpContext.Current.Request.Url.AbsolutePath))
        Dim page_Url As String = HttpContext.Current.Request.Url.AbsoluteUri

        If Not page_Url.EndsWith(".aspx") Then
            page_Url = String.Concat(page_Url, ".aspx")
        End If
        jsParams.Add("page_Url", page_Url)
        jsParams.Add("ws_Main", System.Web.VirtualPathUtility.ToAbsolute("~/webservices/ws_Main.asmx"))
        jsParams.Add("ws_Util", System.Web.VirtualPathUtility.ToAbsolute("~/webservices/ws_Util.asmx"))
        jsParams.Add("ws_Catasto", System.Web.VirtualPathUtility.ToAbsolute("~/webservices/ws_Catasto.asmx"))

        'Dim props() As Reflection.PropertyInfo = HttpContext.Current.Request.Url.GetType.GetProperties
        'For Each prop In props
        '    jsParams.Add(prop.Name, prop.GetValue(HttpContext.Current.Request.Url).ToString)
        'Next
        '            var Port = '80';
        'var HostNameType = 'Dns';
        '            var IsFile = 'False';
        'var DnsSafeHost = 'www.geosafety360.it';
        '            var LocalPath = '/PITER/WebForms/Gestione/LottiAssegnati';
        'var IsLoopback = 'False';
        '            var ws_Util = '/PITER/webservices/ws_Util.asmx';
        'var Host = 'www.geosafety360.it';
        '            var AbsolutePath = '/PITER/WebForms/Gestione/LottiAssegnati';
        'var IdnHost = 'www.geosafety360.it';
        '            var AbsoluteUri = 'http://www.geosafety360.it/PITER/WebForms/Gestione/LottiAssegnati';
        'var Scheme = 'http';
        '            var PathAndQuery = '/PITER/WebForms/Gestione/LottiAssegnati';
        'var page_Url = '~http://www.geosafety360.it/PITER/WebForms/Gestione/LottiAssegnati.aspx';
        '            var IsDefaultPort = 'True';
        'var OriginalString = 'http://www.geosafety360.it:80/PITER/WebForms/Gestione/LottiAssegnati';
        '            var IsAbsoluteUri = 'True';
        'var UserEscaped = 'False';
        '            var Query = '';
        'var Authority = 'www.geosafety360.it';
        '            var UserInfo = '';
        'var Fragment = '';
        '            var Segments = 'System.String[]';
        'var IsUnc = 'False';

        Dim jsCode As New Text.StringBuilder
        For Each de As DictionaryEntry In jsParams
            jsCode.AppendFormat("var {0} = '{1}';", de.Key, de.Value)
            jsCode.Append(Environment.NewLine)
        Next
        Return jsCode.ToString
    End Function

    Public Shared Function GetJsNumberFormat()
        Dim nfi = Globalization.CultureInfo.CurrentCulture.NumberFormat

        Dim obj = New With {
        .NumberGroupSeparator = nfi.NumberGroupSeparator,
        .NumberDecimalSeparator = nfi.NumberDecimalSeparator,
        .CurrencySymbol = nfi.CurrencySymbol
        }

        Return String.Format("var nfi = {0};", Newtonsoft.Json.JsonConvert.SerializeObject(obj))

    End Function

    Public Shared Function DoubleInvariant(value As Object)
        If Not IsDBNull(value) Then
            Return CDbl(Math.Round(value, 8)).ToString(Globalization.CultureInfo.InvariantCulture)
        Else
            Return Nothing
        End If

    End Function

    Public Shared Function JsonSerialize(obj As Object)
        Return Newtonsoft.Json.JsonConvert.SerializeObject(obj, Newtonsoft.Json.Formatting.None)
    End Function
End Class
