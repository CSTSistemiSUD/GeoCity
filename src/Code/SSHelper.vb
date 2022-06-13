Imports System.Data.SqlClient

Class SSHelper
    Private ConnectionString As String
    Sub New(Optional DatabaseName As String = "")
        If String.IsNullOrEmpty(DatabaseName) Then
            ConnectionString = ConfigurationManager.ConnectionStrings("DefaultConnection").ConnectionString
        Else
			ConnectionString = GetConnectionString(DatabaseName)
		End If
    End Sub

  Public Function GetConnectionString(dbName As String) As String
    Return String.Format(Me.ConnectionString, dbName)
  End Function

  Public Function GetData(sqlQuery As String) As DatabaseOperationResult
        Return GetData(New String() {sqlQuery})
    End Function

    Public Function GetData(sqlQuery As List(Of String)) As DatabaseOperationResult
        Return GetData(sqlQuery.ToArray)
    End Function

    Public Function GetData(sqlQuery() As String) As DatabaseOperationResult

        Dim lst As New List(Of SqlCommand)

        For Each query As String In sqlQuery
            lst.Add(New SqlCommand With {.CommandText = query, .CommandType = CommandType.Text})
        Next

        Return GetData(lst)
    End Function

    Public Function GetData(sqlCMD As SqlClient.SqlCommand) As DatabaseOperationResult
        Dim lst As New List(Of SqlCommand)
        lst.Add(sqlCMD)
        Return GetData(lst)
    End Function

    Public Function GetData(sqlCmdList As List(Of SqlClient.SqlCommand)) As DatabaseOperationResult
        Dim sqlCnn As New SqlClient.SqlConnection(Me.ConnectionString)
        Dim sqlAdp As SqlClient.SqlDataAdapter

        Dim dbResult As New DatabaseOperationResult
        Try
            sqlCnn.Open()

            For Each sqlCmd As SqlClient.SqlCommand In sqlCmdList
                Dim sqlDT As New DataTable
                sqlCmd.Connection = sqlCnn
                sqlAdp = New SqlClient.SqlDataAdapter(sqlCmd)
                sqlAdp.Fill(sqlDT)

                dbResult.ReturnDataset.Tables.Add(sqlDT)
            Next

        Catch ex As Exception
            dbResult.ErrorMessage = ex.Message
        Finally
            sqlCnn.Close()
        End Try

        Return dbResult
    End Function

	Public Function ExecuteStored(cmdList As List(Of SqlClient.SqlCommand)) As DatabaseOperationResult

		Dim sqlCnn As New SqlClient.SqlConnection(Me.ConnectionString)
		Dim sqlTRANS As SqlClient.SqlTransaction = Nothing
		Dim dbResult As New DatabaseOperationResult
		Try
			sqlCnn.Open()
			sqlTRANS = sqlCnn.BeginTransaction

			For Each cmd As SqlCommand In cmdList
				cmd.Connection = sqlCnn
				cmd.Transaction = sqlTRANS
				cmd.ExecuteNonQuery()
			Next

			sqlTRANS.Commit()

		Catch ex As Exception
			dbResult.ErrorMessage = ex.Message
			sqlTRANS.Rollback()
		Finally
			sqlCnn.Close()
		End Try
		Return dbResult

	End Function


	Public Function ExecuteStored(cmd As SqlClient.SqlCommand) As DatabaseOperationResult

		Dim cmdList As New List(Of SqlClient.SqlCommand)
		cmdList.Add(cmd)
		Return ExecuteStored(cmdList)

	End Function

    Public Function FillDataset(sqlCmd As SqlClient.SqlCommand) As DatabaseOperationResult
        Dim sqlCnn As New SqlClient.SqlConnection(Me.ConnectionString)
        Dim sqlAdp As SqlClient.SqlDataAdapter

        Dim dbResult As New DatabaseOperationResult
        Try
            sqlCnn.Open()

            sqlCmd.Connection = sqlCnn
            sqlAdp = New SqlClient.SqlDataAdapter(sqlCmd)
            sqlAdp.Fill(dbResult.ReturnDataset)

        Catch ex As Exception
            dbResult.ErrorMessage = ex.Message
        Finally
            sqlCnn.Close()
        End Try

        Return dbResult
    End Function


End Class



