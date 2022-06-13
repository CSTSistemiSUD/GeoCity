Public Class DateRange
    Private _StartDate As Object
    Public ReadOnly Property StartDate() As Object
        Get
            Return _StartDate
        End Get

    End Property

    Private _EndDate As Object
    Public ReadOnly Property EndDate() As Object
        Get
            Return _EndDate
        End Get

    End Property

    Private _IsValid As Boolean
    Public ReadOnly Property IsValid() As Boolean
        Get
            Return _IsValid
        End Get

    End Property

    Private _FormattedStartDate As String
    Public ReadOnly Property FormattedStartDate() As String
        Get
            Return _FormattedStartDate
        End Get

    End Property

    Private _FormattedEndDate As String
    Public ReadOnly Property FormattedEndDate() As String
        Get
            Return _FormattedEndDate
        End Get

    End Property

    Sub New(startDate As String, endDate As String, dateFormat As String, dayInterval As Boolean)
        _StartDate = Nothing
        _EndDate = Nothing
        _IsValid = False

        Dim FormatParam As String = String.Concat("{0:", dateFormat, "}")

        '// Verifica che le date inserite siano valide
        If Date.TryParse(startDate, New Date) Then
            _StartDate = Date.Parse(startDate)
        End If
        If Date.TryParse(endDate, New Date) Then
            _EndDate = Date.Parse(endDate)
        End If


        If _StartDate Is Nothing And Not _EndDate Is Nothing Then

            '' RICERCA FINO AL

            _StartDate = New DateTime(Now.Year, 1, 1)
            _FormattedStartDate = String.Format(FormatParam, _StartDate)
            '// 
        End If

        If Not _StartDate Is Nothing Then
            If _EndDate Is Nothing Then
                '' RICERCA DAL

                _EndDate = New DateTime(Now.Year, Now.Month, Now.Day)

            Else
                '' RICERCA DAL AL
                If _EndDate < _StartDate Then
                    Dim temp = _EndDate
                    _EndDate = _StartDate
                    _StartDate = temp


                End If

            End If
            _IsValid = True
            If dayInterval Then
                _StartDate = New DateTime(_StartDate.Year, _StartDate.Month, _StartDate.Day, 0, 0, 59)
                _EndDate = New DateTime(_EndDate.Year, _EndDate.Month, _EndDate.Day, 23, 59, 59)
            End If
            _FormattedStartDate = String.Format(FormatParam, _StartDate)
            _FormattedEndDate = String.Format(FormatParam, _EndDate)
        End If

    End Sub



End Class