Public Class DatabaseOperationResult
    Public ErrorMessage As String
    Public ReturnDataset As DataSet
    Sub New()
        ErrorMessage = String.Empty
        ReturnDataset = New DataSet("DataSet")

    End Sub

    Public Function Serialize() As String

        Dim jsonResult = New With {.ErrorMessage = ErrorMessage, .ReturnDataset = ReturnDataset}

        Return Newtonsoft.Json.JsonConvert.SerializeObject(jsonResult, Newtonsoft.Json.Formatting.Indented)

    End Function

    Public Function GetTable(TableIndex As Integer) As DataTable


        Dim dt As DataTable
        If String.IsNullOrEmpty(Me.ErrorMessage) Then
            dt = Me.ReturnDataset.Tables(TableIndex)
        Else
            dt = New DataTable
        End If


        Return dt
    End Function



    Public Function GetRow(TableIndex As Integer, RowIndex As Integer) As DataRow
        Dim dt As DataTable
        If String.IsNullOrEmpty(Me.ErrorMessage) Then
            dt = Me.ReturnDataset.Tables(TableIndex)



        Else
            dt = New DataTable
        End If
		'Dim row = dt.NewRow

		'For Each dc As DataColumn In dt.Columns
		'	row(dc.ColumnName) = DBNull.Value
		'Next

		'dt.Rows.Add(row)
		'dt.AcceptChanges()
		If dt.Rows.Count = 1 Then
			Return dt.Rows(0)
		Else
			Return Nothing
		End If

	End Function

    Public Function SerializeRow(TableIndex As Integer, RowIndex As Integer) As String

        Dim serialized As String = String.Empty
        Dim dr As DataRow = Me.GetRow(TableIndex, RowIndex)
        Dim dt As DataTable = dr.Table
        Dim row As String = New Newtonsoft.Json.Linq.JObject(
            dt.Columns.Cast(Of DataColumn)().Select(
            Function(c) New Newtonsoft.Json.Linq.JProperty(
            c.ColumnName, Newtonsoft.Json.Linq.JToken.FromObject(dt.Rows(RowIndex)(c))))).ToString(Newtonsoft.Json.Formatting.None)

        Dim jsonResult = New With {
            .ErrorMessage = Me.ErrorMessage,
            .DataRow = row
        }
        'Return Newtonsoft.Json.JsonConvert.SerializeObject(jsonResult, Newtonsoft.Json.Formatting.Indented)
        'Dim json As String
        'Dim dt As DataTable
        'If String.IsNullOrEmpty(Me.ErrorMessage) Then
        '    dt = Me.ReturnDataset.Tables(TableIndex)

        '    If dt.Rows.Count = 0 Then
        '        Dim row = dt.NewRow

        '        For Each dc As DataColumn In dt.Columns
        '            row(dc.ColumnName) = DBNull.Value
        '        Next

        '        dt.Rows.Add(row)
        '        dt.AcceptChanges()
        '    End If

        'Else
        '    dt = New DataTable
        'End If
        'json = New Newtonsoft.Json.Linq.JObject(dt.Columns.Cast(Of DataColumn)().Select(Function(c) New Newtonsoft.Json.Linq.JProperty(c.ColumnName, Newtonsoft.Json.Linq.JToken.FromObject(dt.Rows(RowIndex)(c))))).ToString(Newtonsoft.Json.Formatting.None)
        Try
            serialized = Newtonsoft.Json.JsonConvert.SerializeObject(jsonResult, Newtonsoft.Json.Formatting.None)
        Catch ex As Exception
            Debug.WriteLine(ex.Message)
        End Try
        Return serialized
    End Function

    Public Function SerializeTable(TableIndex As Integer) As String
        Dim jsonResult = New With {.ErrorMessage = Me.ErrorMessage, .DataTable = Me.GetTable(TableIndex)}
        'Return Newtonsoft.Json.JsonConvert.SerializeObject(jsonResult, Newtonsoft.Json.Formatting.Indented)
        Return Newtonsoft.Json.JsonConvert.SerializeObject(jsonResult, Newtonsoft.Json.Formatting.None)
    End Function
End Class