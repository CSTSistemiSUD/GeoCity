'Imports System
'Imports System.Text
'Imports System.Text.RegularExpressions


'Public Class CodiceFiscale





'    Private Shared ReadOnly Months As String = "ABCDEHLMPRST"

'    Private Shared ReadOnly Vocals As String = "AEIOU"

'    Private Shared ReadOnly Consonants As String = "BCDFGHJKLMNPQRSTVWXYZ"

'    Private Shared ReadOnly OmocodeChars As String = "LMNPQRSTUV"

'    Private Shared ReadOnly ControlCodeArray As Integer() = {1, 0, 5, 7, 9, 13, 15, 17, 19, 21, 2, 4, 18, 20, 11, 3, 6, 8, 12, 14, 16, 10, 22, 25, 24, 23}

'    Private Shared ReadOnly CheckRegex As Regex = New Regex("^[A-Z]{6}[\d]{2}[A-Z][\d]{2}[A-Z][\d]{3}[A-Z]$")

'    Function CalcolaCodiceFiscale(ByVal nome As String, ByVal cognome As String, ByVal dataDiNascita As DateTime, ByVal genere As Char, ByVal codiceISTAT As String) As String
'        If String.IsNullOrEmpty(nome) Then Throw New NotSupportedException("ERRORE: Il parametro 'nome' è obbligatorio.")
'        If String.IsNullOrEmpty(cognome) Then Throw New NotSupportedException("ERRORE: Il parametro 'cognome' è obbligatorio.")
'        If genere <> "M"c AndAlso genere <> "F"c Then Throw New NotSupportedException("ERRORE: Il parametro 'genere' deve essere 'M' oppure 'F'.")
'        If String.IsNullOrEmpty(codiceISTAT) Then Throw New NotSupportedException("ERRORE: Il parametro 'codiceISTAT' è obbligatorio.")
'        Dim cf As String = String.Format("{0}{1}{2}{3}", CalcolaCodiceCognome(cognome), CalcolaCodiceNome(nome), CalcolaCodiceDataDiNascitaGenere(dataDiNascita, genere), codiceISTAT)
'        cf += CalcolaCarattereDiControllo(cf)
'        Return cf
'    End Function

'    Function ControlloFormaleOK(ByVal cf As String) As Boolean
'        If String.IsNullOrEmpty(cf) OrElse cf.Length < 16 Then Return False
'        cf = Normalize(cf, False)
'        If Not CheckRegex.Match(cf).Success Then
'            Dim cf_NoOmocodia As String = SostituisciLettereOmocodia(cf)
'            If Not CheckRegex.Match(cf_NoOmocodia).Success Then Return False
'        End If

'        Return cf(15) = CalcolaCarattereDiControllo(cf.Substring(0, 15))
'    End Function

'    Function ControlloFormaleOK(ByVal cf As String, ByVal nome As String, ByVal cognome As String, ByVal dataDiNascita As DateTime, ByVal genere As Char, ByVal codiceISTAT As String) As Boolean
'        If String.IsNullOrEmpty(cf) OrElse cf.Length < 16 Then Return False
'        cf = Normalize(cf, False)
'        Dim cf_NoOmocodia As String = String.Empty
'        If Not CheckRegex.Match(cf).Success Then
'            cf_NoOmocodia = SostituisciLettereOmocodia(cf)
'            If Not CheckRegex.Match(cf_NoOmocodia).Success Then Return False
'        Else
'            cf_NoOmocodia = cf
'        End If

'        If String.IsNullOrEmpty(nome) OrElse cf_NoOmocodia.Substring(3, 3) <> CalcolaCodiceNome(nome) Then Return False
'        If String.IsNullOrEmpty(cognome) OrElse cf_NoOmocodia.Substring(0, 3) <> CalcolaCodiceCognome(cognome) Then Return False
'        If cf_NoOmocodia.Substring(6, 5) <> CalcolaCodiceDataDiNascitaGenere(dataDiNascita, genere) Then Return False
'        If String.IsNullOrEmpty(codiceISTAT) OrElse cf_NoOmocodia.Substring(11, 4) <> Normalize(codiceISTAT, False) Then Return False
'        If cf(15) <> CalcolaCarattereDiControllo(cf.Substring(0, 15)) Then Return False
'        Return True
'    End Function

'    Private Function CalcolaCodiceCognome(ByVal s As String) As String
'        s = Normalize(s, True)
'        Dim code As String = String.Empty
'        Dim i As Integer = 0
'        While (code.Length < 3) AndAlso (i < s.Length)
'            For j As Integer = 0 To Consonants.Length - 1
'                If s(i) = Consonants(j) Then code += s(i)
'            Next

'            i += 1
'        End While

'        i = 0
'        While code.Length < 3 AndAlso i < s.Length
'            For j As Integer = 0 To Vocals.Length - 1
'                If s(i) = Vocals(j) Then code += s(i)
'            Next

'            i += 1
'        End While

'        Return If((code.Length < 3), code.PadRight(3, "X"c), code)
'    End Function

'    Private Function CalcolaCodiceNome(ByVal s As String) As String
'        s = Normalize(s, True)
'        Dim code As String = String.Empty
'        Dim cons As String = String.Empty
'        Dim i As Integer = 0
'        While (cons.Length < 4) AndAlso (i < s.Length)
'            For j As Integer = 0 To Consonants.Length - 1
'                If s(i) = Consonants(j) Then cons = cons & s(i)
'            Next

'            i += 1
'        End While

'        code = If((cons.Length > 3), cons(0).ToString() + cons(2).ToString() + cons(3).ToString(), __InlineAssignHelper(code, cons))
'        i = 0
'        While (code.Length < 3) AndAlso (i < s.Length)
'            For j As Integer = 0 To Vocals.Length - 1
'                If s(i) = Vocals(j) Then code += s(i)
'            Next

'            i += 1
'        End While

'        Return If((code.Length < 3), code.PadRight(3, "X"c), code)
'    End Function

'    Private Function CalcolaCodiceDataDiNascitaGenere(ByVal d As DateTime, ByVal g As Char) As String
'        Dim code As String = d.Year.ToString().Substring(2)
'        code += Months(d.Month - 1)
'        If g = "M"c OrElse g = "m"c Then
'            code += If((d.Day <= 9), "0" & d.Day.ToString(), d.Day.ToString())
'        ElseIf g = "F"c OrElse g = "f"c Then
'            code += (d.Day + 40).ToString()
'        Else
'            Throw New NotSupportedException("ERROR: genere must be either 'M' or 'F'.")
'        End If

'        Return code
'    End Function

'    Private Function CalcolaCarattereDiControllo(ByVal f15 As String) As Char
'        Dim tot As Integer = 0
'        Dim arrCode As Byte() = Encoding.ASCII.GetBytes(f15.ToUpper())
'        For i As Integer = 0 To f15.Length - 1
'            If (i + 1) Mod 2 = 0 Then tot += If((Char.IsLetter(f15, i)), arrCode(i) - CByte("A"c), arrCode(i) - CByte("0"c)) Else tot += If((Char.IsLetter(f15, i)), ControlCodeArray((arrCode(i) - CByte("A"c))), ControlCodeArray((arrCode(i) - CByte("0"c))))
'        Next

'        tot = tot Mod 26
'        Dim l As Char = CChar((tot + "A"c))
'        Return l
'    End Function

'    Private Function SostituisciLettereOmocodia(ByVal cf As String) As String
'        Dim cfChars As Char() = cf.ToCharArray()
'        Dim pos As Integer() = {6, 7, 9, 10, 12, 13, 14}
'        For Each i As Integer In pos
'            If Not Char.IsNumber(cfChars(i)) Then cfChars(i) = OmocodeChars.IndexOf(cfChars(i)).ToString()(0)
'        Next

'        Return New String(cfChars)
'    End Function

'    Private Function Normalize(ByVal s As String, ByVal normalizeDiacritics As Boolean) As String
'        If String.IsNullOrEmpty(s) Then Return s
'        s = s.Trim().ToUpper()
'        If normalizeDiacritics Then
'            Dim src As String = "ÀÈÉÌÒÙàèéìòù"
'            Dim rep As String = "AEEIOUAEEIOU"
'            For i As Integer = 0 To src.Length - 1
'                s = s.Replace(src(i), rep(i))
'            Next

'            Return s
'        End If

'        Return s
'    End Function

'    <Obsolete("Please refactor code that uses this function, it is a simple work-around to simulate inline assignment in VB!")>
'    Private Shared Function __InlineAssignHelper(Of T)(ByRef target As T, value As T) As T
'        target = value
'        Return value
'    End Function



'End Class
