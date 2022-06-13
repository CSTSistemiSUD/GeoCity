Imports System.Web
Imports System.Web.Services
Imports System.Web.Services.Protocols
Imports System.Data
Imports System.Web.Script.Services

' Per consentire la chiamata di questo servizio Web dallo script utilizzando ASP.NET AJAX, rimuovere il commento dalla riga seguente.
' <System.Web.Script.Services.ScriptService()> _
<System.Web.Services.WebService(Namespace:="http://tempuri.org/")>
<System.Web.Services.WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)>
<ScriptService()>
Public Class ws_Util
    Inherits System.Web.Services.WebService

    Public Shared Function fn_ElencaNazioni() As DatabaseOperationResult
        Dim queryNazioni As String = "SELECT 
                 [CODICE_NAZIONE]
                ,[DESCRIZIONE]
                ,[CHECK_PIVA]
                ,[CHECK_CF]
                ,[CCVATCode]
                FROM [dbo].[CAT_NAZIONI]
                ORDER BY 
                    CASE 
                        WHEN CODICE_NAZIONE = 'IT' THEN 1 
                    ELSE 
	                    CASE WHEN NOT DATA_CEE IS NULL THEN 2
	                    ELSE 
		                    CASE WHEN CHECK_PIVA = 1 THEN 3
		                    ELSE 4
		                    END
	                    END
                    END, DESCRIZIONE"
        Dim ssh As New SSHelper()
        Dim dbResult As DatabaseOperationResult = ssh.GetData(queryNazioni)
        Return dbResult

    End Function

    <WebMethod()>
    Public Function ElencaNazioni() As String
        Dim dbResult As DatabaseOperationResult = fn_ElencaNazioni()
        Return dbResult.SerializeTable(0)

    End Function

    <WebMethod()>
    Public Function ElencaProvince(cod_nazione As String) As String
        Dim query As String = "SELECT SIGLA as id, UPPER(DESCRIZIONE) as [name]  FROM [CAT_PROVINCE] Where CODICE_NAZIONE = @cod_nazione"
        Dim sqlCmd As New SqlClient.SqlCommand With {.CommandText = query, .CommandType = CommandType.Text}
        With sqlCmd.Parameters

            .AddWithValue("@cod_nazione", cod_nazione)

        End With
        Dim ssh As New SSHelper()
        Dim dbResult As DatabaseOperationResult = ssh.GetData(sqlCmd)
        Return dbResult.SerializeTable(0)
    End Function

    <WebMethod()>
    Public Function ElencaComuni(sigla_provincia As String) As String
        Dim query As String = "SELECT  UPPER(nome) as [name] FROM [CAT_Comuni] Where SIGLA_PROVINCIA = @SIGLA_PROVINCIA"
        Dim sqlCmd As New SqlClient.SqlCommand With {.CommandText = query, .CommandType = CommandType.Text}
        With sqlCmd.Parameters

            .AddWithValue("@SIGLA_PROVINCIA", sigla_provincia)

        End With
        Dim ssh As New SSHelper()
        Dim dbResult As DatabaseOperationResult = ssh.GetData(sqlCmd)
        Return dbResult.SerializeTable(0)
    End Function

    <WebMethod()>
    Public Function ElencaCap(sigla_provincia As String, comune As String) As String
        Dim query As String = "SELECT DISTINCT CAP as [name] FROM [CAT_CAP] Where PROVINCIA = @SIGLA_PROVINCIA AND COMUNE = @COMUNE"
        Dim sqlCmd As New SqlClient.SqlCommand With {.CommandText = query, .CommandType = CommandType.Text}
        With sqlCmd.Parameters

            .AddWithValue("@SIGLA_PROVINCIA", sigla_provincia)
            .AddWithValue("@COMUNE", comune)

        End With
        Dim ssh As New SSHelper()
        Dim dbResult As DatabaseOperationResult = ssh.GetData(sqlCmd)
        Return dbResult.SerializeTable(0)
    End Function





    <WebMethod()>
    <ScriptMethod(ResponseFormat:=ResponseFormat.Json, UseHttpGet:=True)>
    Public Function GetDataTablesRows() As String



        'bRegex: false
        'bRegex_0: false
        'bRegex_1: false
        'bSearchable_0: true
        'bSearchable_1: true
        'bSortable_0: true
        'bSortable_1: true
        'iColumns: 2
        'iDisplayLength: 10
        'iDisplayStart: 0
        'iSortCol_0: 0
        'iSortingCols: 1
        'mDataProp_0: 0
        'mDataProp_1: 1
        'sColumns: %2C
        'sEcho: 3
        'sSearch: col
        'sSearch_0:
        'sSearch_1:
        'sSortDir_0: Asc()

        Dim echo = Integer.Parse(HttpContext.Current.Request.Params("sEcho"))
        Dim displayLength = Integer.Parse(HttpContext.Current.Request.Params("iDisplayLength"))
        Dim displayStart = Integer.Parse(HttpContext.Current.Request.Params("iDisplayStart"))

        Dim sortColIndex = Integer.Parse(HttpContext.Current.Request.Params("iSortCol_0"))
        Dim sortOrder = HttpContext.Current.Request.Params("sSortDir_0").ToString(Globalization.CultureInfo.CurrentCulture)

        ''***************************************************************************************************************
        ''** LINQ

        'Dim query As String = "SELECT CODICE, ISNULL(DESCRIZIONE, '') AS DESCRIZIONE FROM EG_CAT_ATECO ORDER BY CODICE"
        'Dim sqlCmd As New SqlClient.SqlCommand With {.CommandText = query, .CommandType = CommandType.Text}
        'Dim dbResult As DatabaseOperationResult = SSHelper.GetData(sqlCmd)
        'Dim dt As DataTable = dbResult.ReturnDataset.Tables(0)

        'Dim sortColName = dt.Columns(sortColIndex).ColumnName
        'Dim orderedResults As OrderedEnumerableRowCollection(Of DataRow)
        'If sortOrder = "asc" Then
        '    orderedResults = dt.AsEnumerable.OrderBy(Function(r) r(sortColName))
        'Else
        '    orderedResults = dt.AsEnumerable.OrderByDescending(Function(r) r(sortColName))
        'End If
        'Dim itemsToSkip = IIf(displayStart = 0, 0, displayStart + 1)
        'Dim pagedResults = orderedResults.Skip(itemsToSkip).Take(displayLength).ToList
        'Dim hasMoreRecords = pagedResults.Count > 0

        ''** SQL
        Dim tableName As String = HttpContext.Current.Request.Params("tableName").ToString(Globalization.CultureInfo.CurrentCulture)
        Dim columnNames As String = HttpContext.Current.Request.Params("columnNames").ToString(Globalization.CultureInfo.CurrentCulture)
        Dim search As String = HttpContext.Current.Request.Params("sSearch").ToString(Globalization.CultureInfo.CurrentCulture)

        Dim columns() As String = columnNames.Split(",")
        Dim sortColName As String = columns(sortColIndex)

        Dim startIndex As Integer = ((displayStart) * displayLength) + 1
        Dim lastIndex As Integer = ((((displayStart) * displayLength) + 1) + displayLength) - 1

        Dim queryRows As New StringBuilder
        Dim filter As New List(Of String)
        Dim whereClause As String = String.Empty
        With queryRows

            .AppendFormat(" SELECT * FROM (SELECT ROW_NUMBER() OVER (ORDER BY {0} {1}) AS NUM, ", sortColName, sortOrder)

            .AppendFormat(" {0}", columnNames)


            If Not String.IsNullOrEmpty(search) Then
                For Each c As String In columns
                    filter.Add(String.Format("{0} like '%{1}%' ", c, search))
                Next
                whereClause = String.Concat(" WHERE ", Join(filter.ToArray, " OR "))
            End If
            .AppendFormat(" FROM [{0}] {1}) A", tableName, whereClause)
            .AppendFormat(" WHERE A.NUM BETWEEN {0} AND {1}", startIndex, lastIndex)

        End With

        Dim queryCount As String = String.Format("SELECT COUNT(*) FROM [{0}] {1}", tableName, whereClause)
        Dim ssh As New SSHelper()
        Dim dbResult As DatabaseOperationResult = ssh.GetData(New String() {queryRows.ToString, queryCount})
        Dim pagedResults As DataTable = dbResult.GetTable(0)

        Dim recordsCount As Integer = dbResult.GetTable(1).Rows(0)(0)

        pagedResults.Columns.Remove("NUM")
        Dim json = New With {
            .sEcho = echo,
            .recordsTotal = recordsCount,
            .recordsFiltered = recordsCount,
            .iTotalRecords = recordsCount,
            .iTotalDisplayRecords = recordsCount,
            .aaData = New List(Of Object)
        }

        For Each row As DataRow In pagedResults.Rows
            json.aaData.Add(row.ItemArray)
        Next
        Return Newtonsoft.Json.JsonConvert.SerializeObject(json, Newtonsoft.Json.Formatting.None)

    End Function


    <WebMethod>
    Public Function CodFis_Inverso(codice_fiscale As String) As AddressJson

        Dim d As New AddressJson With {
            .DATA = String.Empty,
            .COMUNE = String.Empty,
            .SIGLA_PROVINCIA = String.Empty,
            .PROVINCIA = String.Empty,
            .NAZIONE = "IT"
        }

        Try
            '// Calcola la data di nascita
            Dim anno_di_nascita As String = Mid(codice_fiscale, 7, 2)
            Dim anno_corrente As String = CLng(Mid(Now.Year, 3, 2))
            If CLng(anno_di_nascita) <= anno_corrente Then
                anno_di_nascita = "20" + anno_di_nascita
            Else
                anno_di_nascita = "19" + anno_di_nascita
            End If
            Dim mese_di_nascita As String = Mid(codice_fiscale.ToUpper, 9, 1)
            Select Case mese_di_nascita
                Case "A" : mese_di_nascita = "01"
                Case "B" : mese_di_nascita = "02"
                Case "C" : mese_di_nascita = "03"
                Case "D" : mese_di_nascita = "04"
                Case "E" : mese_di_nascita = "05"
                Case "H" : mese_di_nascita = "06"
                Case "L" : mese_di_nascita = "07"
                Case "M" : mese_di_nascita = "08"
                Case "P" : mese_di_nascita = "09"
                Case "R" : mese_di_nascita = "10"
                Case "S" : mese_di_nascita = "11"
                Case "T" : mese_di_nascita = "12"
            End Select
            Dim giorno_di_nascita As String = Mid(codice_fiscale, 10, 2)
            If CLng(giorno_di_nascita) > 40 Then
                giorno_di_nascita = CLng(giorno_di_nascita) - 40
            End If

            Dim data_di_nascita As String = String.Format("{0}/{1}/{2}", giorno_di_nascita, mese_di_nascita, anno_di_nascita)

            '// Calcola il comune
            Dim codice_comune As String = Mid(codice_fiscale, 12, 4)
            Dim query As String = "SELECT '" & data_di_nascita & "' AS DATA, [CAT_Comuni].NOME AS COMUNE,
            [CAT_Comuni].SIGLA_PROVINCIA, UPPER([CAT_PROVINCE].DESCRIZIONE) AS PROVINCIA
            FROM [CAT_Comuni]
            INNER JOIN [CAT_PROVINCE] ON [CAT_Comuni].SIGLA_PROVINCIA = [CAT_PROVINCE].SIGLA
            WHERE ID_CODICE_FISCALE = @ID_CODICE_FISCALE; "
            Dim cmd As New SqlClient.SqlCommand With {.CommandText = query, .CommandType = CommandType.Text}
            cmd.Parameters.AddWithValue("@ID_CODICE_FISCALE", codice_comune)
            Dim ssh As New SSHelper()
            Dim dbResult As DatabaseOperationResult = ssh.GetData(cmd)
            Dim row As DataRow = dbResult.GetRow(0, 0)
            With d
                .DATA = row("DATA")
                .COMUNE = row("COMUNE")
                .SIGLA_PROVINCIA = row("SIGLA_PROVINCIA")
                .PROVINCIA = row("PROVINCIA")
                .NAZIONE = "IT"
            End With

        Catch ex As Exception

        End Try

        Return d


    End Function




End Class