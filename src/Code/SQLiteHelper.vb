Imports System.Data.SQLite

Class SQLiteHelper
	Private ConnectionString As String
	Sub New(DatabasePath As String)

		Me.ConnectionString = String.Format("Data Source={0}; Version=3;", DatabasePath) 'Journal Mode=Off; Compress=True; 
	End Sub



	Public Function GetConnectionString(dbName As String) As String
		Return Me.ConnectionString
	End Function

	Public Function GetData(sqlQuery As String) As DatabaseOperationResult
		Return GetData(New String() {sqlQuery})
	End Function

	Public Function GetData(sqlQuery As List(Of String)) As DatabaseOperationResult
		Return GetData(sqlQuery.ToArray)
	End Function

	Public Function GetData(sqlQuery() As String) As DatabaseOperationResult

		Dim lst As New List(Of SQLiteCommand)

		For Each query As String In sqlQuery
			lst.Add(New SQLiteCommand With {.CommandText = query, .CommandType = CommandType.Text})
		Next

		Return GetData(lst)
	End Function

	Public Function GetData(sqlCMD As SQLiteCommand) As DatabaseOperationResult
		Dim lst As New List(Of SQLiteCommand)
		lst.Add(sqlCMD)
		Return GetData(lst)
	End Function

	Public Function GetData(ht As Hashtable) As DatabaseOperationResult

		Dim sqlCnn As New SQLiteConnection(Me.ConnectionString)
		Dim sqlAdp As SQLiteDataAdapter

		Dim dbResult As New DatabaseOperationResult
		Try
			sqlCnn.Open()

			For Each de As DictionaryEntry In ht
				Dim sqlCmd As New SQLiteCommand With {.CommandText = de.Value, .CommandType = CommandType.Text}
				Dim sqlDT As New DataTable
				sqlCmd.Connection = sqlCnn
				sqlAdp = New SQLiteDataAdapter(sqlCmd)
				sqlAdp.Fill(sqlDT)
				sqlDT.TableName = de.Key
				dbResult.ReturnDataset.Tables.Add(sqlDT)
			Next

		Catch ex As Exception
			dbResult.ErrorMessage = ex.Message
		Finally
			sqlCnn.Close()
		End Try
		Return dbResult
	End Function

	Public Function GetData(sqlCmdList As List(Of SQLiteCommand)) As DatabaseOperationResult
		Dim sqlCnn As New SQLiteConnection(Me.ConnectionString)
		Dim sqlAdp As SQLiteDataAdapter

		Dim dbResult As New DatabaseOperationResult
		Try
			sqlCnn.Open()

			For Each sqlCmd As SQLiteCommand In sqlCmdList

				Dim sqlDT As New DataTable
				sqlCmd.Connection = sqlCnn
				sqlAdp = New SQLiteDataAdapter(sqlCmd)
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

	Public Function ExecuteStored(cmdList As List(Of SQLiteCommand)) As DatabaseOperationResult

		Dim sqlCnn As New SQLiteConnection(Me.ConnectionString)
		Dim sqlTRANS As SQLiteTransaction = Nothing
		Dim dbResult As New DatabaseOperationResult
		Try
			sqlCnn.Open()
			sqlTRANS = sqlCnn.BeginTransaction

			For Each cmd As SQLiteCommand In cmdList
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


	Public Function ExecuteStored(cmd As SQLiteCommand) As DatabaseOperationResult

		Dim cmdList As New List(Of SQLiteCommand)
		cmdList.Add(cmd)
		Return ExecuteStored(cmdList)

	End Function

	Public Function FillDataset(sqlCmd As SQLiteCommand) As DatabaseOperationResult
		Dim sqlCnn As New SQLiteConnection(Me.ConnectionString)
		Dim sqlAdp As SQLiteDataAdapter

		Dim dbResult As New DatabaseOperationResult
		Try
			sqlCnn.Open()

			sqlCmd.Connection = sqlCnn
			sqlAdp = New SQLiteDataAdapter(sqlCmd)
			sqlAdp.Fill(dbResult.ReturnDataset)

		Catch ex As Exception
			dbResult.ErrorMessage = ex.Message
		Finally
			sqlCnn.Close()
		End Try

		Return dbResult
	End Function


End Class



