Imports System.Text
Imports System.Net
Imports System.Net.Sockets
Imports System.IO

Public Class FIS_Renndaten
    Public RaceCodex As String
    Public FISCategory As String
    Public FISPasswort As String
    Public FISPort As String

    Public Dg As String

    Public Bewerb As String
    Public Ort As String
    Public Disziplin As String
    Public Geschlecht As String
    Public Bewerbsdatum As String
    Public Start As String
    Public Ziel As String
    Public Hoehe As String
    Public DG1_Tore As String
    Public DG1_RichtAend As String
    Public DG1_Startzeit As String
    Public DG2_Tore As String
    Public DG2_RichtAend As String
    Public DG2_Startzeit As String

    Public Sequence As Integer
End Class

Public Class FIS_Startliste
    Public StartNr As String
    Public Nachname As String
    Public Vorname As String
    Public Nation As String
    Public FISCode As String
End Class

Public Class FIS_Ergebnisliste
    Public StartNr As String
    Public Nachname As String
    Public Vorname As String
    Public Nation As String
    Public FISCode As String
    Public Startzeit As Long     ' Tageszeit in Sekunden
    Public Status As String      ' dns / dnf / dq
    Public Laufzeit As String    ' [m:]ss,zh    DG2 = Gesamtzeit
End Class

Public Class FIS_Data
    Public Aktion As String      ' amStart / Start / Ziel / DelZiel / NaS / NiZ / Dis / Info
    Public StartNr As String
    Public Laufzeit As String    ' [m:]ss,zh    DG2 = Gesamtzeit
    Public Diffzeit As String    ' [m:]ss,zh    Abstand zur Bestzeit
    Public Platz As Integer
    Public Infotext As String
End Class

Public Class FIS_LiveTiming
    Public Function FIS_ClearRace(Renndaten As FIS_Renndaten) As String
        Dim xmltext As String

        Dim Fehlermeldung As String = Check_FIS_Par(Renndaten)
        If Not Fehlermeldung = "" Then Return Fehlermeldung

        xmltext = "<?xml version=""1.0"" encoding=""UTF-8""?>"
        xmltext &= "<livetiming codex=""" & Renndaten.RaceCodex & """ passwd=""" & Renndaten.FISPasswort & """ sequence=""" & Renndaten.Sequence.ToString("D5") & """ timesamp=""" & System.DateTime.Now.ToString("hh:mm:ss") & """>"
        Renndaten.Sequence += 1
        xmltext &= " <command>"
        xmltext &= "  <clear/>"
        xmltext &= " </command>"
        xmltext &= "</livetiming>"

        Fehlermeldung = TCPtoFIS(Renndaten, xmltext)
        Return Fehlermeldung
    End Function

    Public Function FIS_Renndaten(Renndaten As FIS_Renndaten) As String
        Dim value = 0
        Dim xmltext As String
        Dim Fehlermeldung As String = Check_FIS_Par(Renndaten)
        If Not Fehlermeldung = "" Then Return Fehlermeldung

        If Not Renndaten.Dg = "1" And Not Renndaten.Dg = "2" Then
            Return "Durch ist nicht 1 oder 2"
        End If
        If Not Renndaten.Disziplin.ToUpper = "DH" And Not Renndaten.Disziplin.ToUpper = "SG" And Not Renndaten.Disziplin.ToUpper = "GS" And Not Renndaten.Disziplin.ToUpper = "SL" Then
            Return "Disziplin ist nicht DH, SG, GS oder SL"
        End If
        If Not Renndaten.Geschlecht.ToUpper = "M" And Not Renndaten.Geschlecht.ToUpper = "W" And Not Renndaten.Geschlecht.ToUpper = "L" And Not Renndaten.Geschlecht.ToUpper = "A" Then
            Return "Geschlecht ist nicht M, W, L oder A"
        End If
        If Not Renndaten.Bewerbsdatum = "" Then
            If Not Renndaten.Bewerbsdatum.Length = 10 _
            Or Not Integer.TryParse(Renndaten.Bewerbsdatum.Substring(0, 2), value) _
            Or Not Integer.TryParse(Renndaten.Bewerbsdatum.Substring(3, 2), value) _
            Or Not Integer.TryParse(Renndaten.Bewerbsdatum.Substring(6, 4), value) Then
                Return "Bewerbsdatum nicht im Format TT.MM.JJJJ"
            End If
        End If
        If Not Renndaten.Start = "" And Not Integer.TryParse(Renndaten.Start, value) Then
            Return "Höhe Start ist nicht numerisch"
        End If
        If Not Renndaten.Ziel = "" And Not Integer.TryParse(Renndaten.Ziel, value) Then
            Return "Höhe Ziel ist nicht numerisch"
        End If
        If Not Renndaten.Hoehe = "" And Not Integer.TryParse(Renndaten.Hoehe, value) Then
            Return "Höhedifferenz ist nicht numerisch"
        End If
        If Renndaten.Dg = "1" Then
            If Not Renndaten.DG1_Tore = "" And Not Integer.TryParse(Renndaten.DG1_Tore, value) Then
                Return "Anzahl Tore DG1 ist nicht numerisch"
            End If
            If Not Renndaten.DG1_RichtAend = "" And Not Integer.TryParse(Renndaten.DG1_RichtAend, value) Then
                Return "Anzahl Richtungsänderngen DG1 ist nicht numerisch"
            End If
            If Not Renndaten.DG1_Startzeit = "" Then
                If Not Renndaten.DG1_Startzeit.Length = 5 _
                Or Not Integer.TryParse(Renndaten.DG1_Startzeit.Substring(0, 2), value) _
                Or Not Integer.TryParse(Renndaten.DG1_Startzeit.Substring(3, 2), value) Then
                    Return "Startzeit DG1 nicht im Format HH:MM"
                End If
            End If
        Else
            If Not Renndaten.DG2_Tore = "" And Not Integer.TryParse(Renndaten.DG2_Tore, value) Then
                Return "Anzahl Tore DG2 ist nicht numerisch"
            End If
            If Not Renndaten.DG2_RichtAend = "" And Not Integer.TryParse(Renndaten.DG2_RichtAend, value) Then
                Return "Anzahl Richtungsänderngen DG2 ist nicht numerisch"
            End If
            If Not Renndaten.DG2_Startzeit = "" Then
                If Not Renndaten.DG2_Startzeit.Length = 5 _
                Or Not Integer.TryParse(Renndaten.DG2_Startzeit.Substring(0, 2), value) _
                Or Not Integer.TryParse(Renndaten.DG2_Startzeit.Substring(3, 2), value) Then
                    Return "Startzeit DG2 nicht im Format HH:MM"
                End If
            End If
        End If

        xmltext = "<?xml version=""1.0"" encoding=""UTF-8""?>"
        xmltext &= "<livetiming codex=""" & Renndaten.RaceCodex & """ passwd=""" & Renndaten.FISPasswort & """ sequence=""" & Renndaten.Sequence.ToString("D5") & """ timesamp=""" & System.DateTime.Now.ToString("hh:mm:ss") & """>"
        Renndaten.Sequence += 1
        xmltext &= " <raceinfo>"
        xmltext &= "  <event>" & Renndaten.Bewerb & "</event>"
        xmltext &= "  <slope>" & Renndaten.Ort & "</slope>"
        xmltext &= "  <discipline>" & Renndaten.Disziplin.ToUpper & "</discipline>"
        xmltext &= "  <gender>" & Renndaten.Geschlecht.ToUpper & "</gender>"
        xmltext &= "  <category>" & Renndaten.FISCategory & "</category>"
        xmltext &= "  <place>" & Renndaten.Ort & "</place>"
        xmltext &= "  <tempunit>c</tempunit>"
        xmltext &= "  <longunit>m</longunit>"
        xmltext &= "  <speedunit>Kmh</speedunit>"
        xmltext &= "  <run no=""" & Renndaten.Dg & """>"
        xmltext &= "   <discipline>" & Renndaten.Disziplin.ToUpper & "</discipline>"
        xmltext &= "   <start>" & Renndaten.Start & "</start>"
        xmltext &= "   <finish>" & Renndaten.Ziel & "</finish>"
        xmltext &= "   <height>" & Renndaten.Hoehe & "</height>"
        If Renndaten.Dg = "1" Then
            xmltext &= "   <gates>" & Renndaten.DG1_Tore & "</gates>"
            xmltext &= "   <turninggates>" & Renndaten.DG1_RichtAend & "</turninggates>"
        Else
            xmltext &= "   <gates>" & Renndaten.DG2_Tore & "</gates>"
            xmltext &= "   <turninggates>" & Renndaten.DG2_RichtAend & "</turninggates>"
        End If
        If Not Renndaten.Bewerbsdatum = "" Then
            xmltext &= "   <day>" & Renndaten.Bewerbsdatum.Substring(0, 2) & "</day>"
            xmltext &= "   <month>" & Renndaten.Bewerbsdatum.Substring(3, 2) & "</month>"
            xmltext &= "   <year>" & Renndaten.Bewerbsdatum.Substring(6, 4) & "</year>"
        End If
        If Renndaten.Dg = "1" Then
            If Not Renndaten.DG1_Startzeit = "" Then
                xmltext &= "   <hour>" & Renndaten.DG1_Startzeit.Substring(0, 2) & "</hour>"
                xmltext &= "   <minute>" & Renndaten.DG1_Startzeit.Substring(3, 2) & "</minute>"
            End If
        Else
            If Not Renndaten.DG2_Startzeit = "" Then
                xmltext &= "   <hour>" & Renndaten.DG2_Startzeit.Substring(0, 2) & "</hour>"
                xmltext &= "   <minute>" & Renndaten.DG2_Startzeit.Substring(3, 2) & "</minute>"
            End If
        End If
        xmltext &= "    <racedef>"
        xmltext &= "    </racedef>"
        xmltext &= "  </run>"
        xmltext &= " </raceinfo>"
        xmltext &= "</livetiming>"

        Return TCPtoFIS(Renndaten, xmltext)
    End Function

    Public Function FIS_Startliste(Renndaten As FIS_Renndaten, Startliste As List(Of FIS_Startliste)) As String
        Dim xmltext As String
        Dim lfdnr As Integer

        xmltext = "<?xml version=""1.0"" encoding=""UTF-8""?>"
        xmltext &= "<livetiming codex=""" & Renndaten.RaceCodex & """ passwd=""" & Renndaten.FISPasswort & """ sequence=""" & Renndaten.Sequence.ToString("D5") & """ timesamp=""" & System.DateTime.Now.ToString("hh:mm:ss") & """>"
        Renndaten.Sequence += 1

        xmltext &= " <command>"
        xmltext &= "  <activerun no=""" & Renndaten.Dg & """/>"
        xmltext &= " </command>"

        xmltext &= " <startlist runno=""" & Renndaten.Dg & """>"

        For Each tn As FIS_Startliste In Startliste
            lfdnr += 1
            xmltext &= "  <racer order=""" & lfdnr & """>"
            xmltext &= "   <bib>" & tn.StartNr & "</bib>"
            xmltext &= "   <lastname>" & tn.Nachname & "</lastname>"
            xmltext &= "   <firstname>" & tn.Vorname & "</firstname>"
            xmltext &= "   <nat>" & tn.Nation & "</nat>"
            xmltext &= "   <fiscode>" & tn.FISCode & "</fiscode>"
            xmltext &= "  </racer>"
        Next

        xmltext &= " </startlist>"
        xmltext &= "</livetiming>"

        Return TCPtoFIS(Renndaten, xmltext)
    End Function

    Public Function FIS_Ergebnis(Renndaten As FIS_Renndaten, Ergebnisliste As List(Of FIS_Ergebnisliste)) As String
        Dim xmltext As String
        Dim Zeit As Long = 0
        Dim FIS_Zeit As String = ""
        Dim last_Startzeit As Long = 0
        Dim last_startnr As String = ""
        Dim last_FIS_Zeit As String = ""

        xmltext = "<?xml version=""1.0"" encoding=""UTF-8""?>"
        xmltext &= "<livetiming codex=""" & Renndaten.RaceCodex & """ passwd=""" & Renndaten.FISPasswort & """ sequence=""" & Renndaten.Sequence.ToString("D5") & """ timesamp=""" & System.DateTime.Now.ToString("hh:mm:ss") & """>"
        Renndaten.Sequence += 1
        xmltext &= " <raceevent>"

        For Each tn As FIS_Ergebnisliste In Ergebnisliste
            If Not tn.Status = "" Then
                xmltext &= "  <" & tn.Status & " bib=""" & tn.StartNr & """/>"
            Else
                xmltext &= "  <finish bib=""" & tn.StartNr & """>"
                FIS_Zeit = tn.Laufzeit.Replace(",", ".")
                xmltext &= "   <time>" & FIS_Zeit & "</time>"
                xmltext &= "   <diff></diff>"
                xmltext &= "   <rank></rank>"
                xmltext &= "  </finish>"
                If tn.Startzeit > last_Startzeit Then
                    last_Startzeit = tn.Startzeit
                    last_startnr = tn.StartNr
                    last_FIS_Zeit = FIS_Zeit
                End If
            End If
        Next
        xmltext &= " </raceevent>"
        xmltext &= "</livetiming>"

        Dim Fehlermeldung As String = TCPtoFIS(Renndaten, xmltext)

        If Fehlermeldung = "" And Not last_Startzeit = 0 Then
            ' Damit auf der FIS-LiveTiming-Seite auch die letzte Zeit erscheint
            xmltext = "<?xml version=""1.0"" encoding=""UTF-8""?>"
            xmltext &= "<livetiming codex=""" & Renndaten.RaceCodex & """ passwd=""" & Renndaten.FISPasswort & """ sequence=""" & Renndaten.Sequence.ToString("D5") & """ timesamp=""" & System.DateTime.Now.ToString("hh:mm:ss") & """>"
            Renndaten.Sequence += 1
            xmltext &= " <raceevent>"
            xmltext &= "  <finish bib=""" & last_startnr & """>"
            xmltext &= "   <time>" & last_FIS_Zeit & "</time>"
            xmltext &= "   <diff></diff>"
            xmltext &= "   <rank></rank>"
            xmltext &= "  </finish>"
            xmltext &= " </raceevent>"
            xmltext &= "</livetiming>"
            Fehlermeldung = TCPtoFIS(Renndaten, xmltext)
        End If

        Return Fehlermeldung
    End Function

    Public Function FIS_Daten(Renndaten As FIS_Renndaten, Data As FIS_Data) As String
        Dim xmltext As String

        xmltext = "<?xml version=""1.0"" encoding=""UTF-8""?>"
        xmltext &= "<livetiming codex=""" & Renndaten.RaceCodex & """ passwd=""" & Renndaten.FISPasswort & """ sequence=""" & Renndaten.Sequence.ToString("D5") & """ timesamp=""" & System.DateTime.Now.ToString("hh:mm:ss") & """>"
        Renndaten.Sequence += 1

        If Data.Aktion = "amStart" Then
            xmltext &= " <raceevent>"
            xmltext &= "  <nextstart bib=""" & Data.StartNr & """></nextstart>"
            xmltext &= " </raceevent>"
        ElseIf Data.Aktion = "Start" Then
            xmltext &= " <raceevent>"
            xmltext &= "  <start bib=""" & Data.StartNr & """/>"
            xmltext &= " </raceevent>"
        ElseIf Data.Aktion = "Ziel" Then
            Dim FIS_Zeit As String = Data.Laufzeit.Replace(",", ".")
            Dim FIS_Diff As String = Data.Diffzeit.Replace(",", ".")
            If FIS_Diff = "" Then FIS_Diff = "0.00"
            xmltext &= " <raceevent>"
            xmltext &= "  <finish bib=""" & Data.StartNr & """>"
            xmltext &= "   <time>" & FIS_Zeit & "</time>"
            xmltext &= "   <diff>" & FIS_Diff & "</diff>"
            xmltext &= "   <rank>" & Data.Platz & "</rank>"
            xmltext &= "  </finish>"
            xmltext &= " </raceevent>"
        ElseIf Data.Aktion = "DelZiel" Then
            xmltext &= " <raceevent>"
            xmltext &= "  <finish bib=""" & Data.StartNr & """ correction=""y"">"
            xmltext &= "   <time>0.00</time>"
            xmltext &= "  </finish>"
            xmltext &= " </raceevent>"
        ElseIf Data.Aktion = "NaS" Then
            xmltext &= " <raceevent>"
            xmltext &= "  <dns bib=""" & Data.StartNr & """/>"
            xmltext &= " </raceevent>"
        ElseIf Data.Aktion = "NiZ" Then
            xmltext &= " <raceevent>"
            xmltext &= "  <dnf bib=""" & Data.StartNr & """/>"
            xmltext &= " </raceevent>"
        ElseIf Data.Aktion = "Dis" Then
            xmltext &= " <raceevent>"
            xmltext &= "  <dq bib=""" & Data.StartNr & """/>"
            xmltext &= " </raceevent>"
        ElseIf Data.Aktion = "Info" Then
            xmltext &= " <message>"
            xmltext &= "  <text>" & Data.Infotext & "</text>"
            xmltext &= " </message>"
        End If
        xmltext &= "</livetiming>"

        Return TCPtoFIS(Renndaten, xmltext)
    End Function

    Private Function Check_FIS_Par(Renndaten As FIS_Renndaten) As String
        Dim FISCategories As New List(Of String)(New String() {"ANC", "AWG", "CISM", "CIT", "CITWC", "COM", "CORP", "EC", "ECOM", "ENL", "EQUA", "EYOF", "FEC", "FIS", "FQUA", "JUN", "NAC", "NC", "NJC", "NJR", "OWG", "SAC", "UNI", "UVS", "WC", "WJC", "WSC", "YOG"})
        Dim i As Integer
        Dim value = 0

        If Not Renndaten.RaceCodex.Length = 4 Or Not Integer.TryParse(Renndaten.RaceCodex, value) Then Return "Den RaceCodex bitte als 4-stellige Zahl übergeben"

        Dim FISCategorie_ok = False
        For i = 0 To FISCategories.Count - 1
            If Renndaten.FISCategory = FISCategories(i) Then
                FISCategorie_ok = True
                Exit For
            End If
        Next
        If Not FISCategorie_ok Then Return "Ungültige FIS-Category übergeben"

        If Renndaten.FISPasswort = "" Then Return "Kein Passwort für das Rennen übergeben"
        If Not Renndaten.FISPort.Length = 4 Or Not Integer.TryParse(Renndaten.FISPort, value) Then Return "Den FISPort bitte als 4-stellige Zahl übergeben"

        Return ""
    End Function

    Private Function TCPtoFIS(Renndaten As FIS_Renndaten, xmltext As String) As String
        Dim tcpClient As New TcpClient

        Try
            tcpClient.Connect("live.fisski.com", CInt(Renndaten.FISPort)) 'connecting the client the server
            Console.WriteLine(xmltext)

            Dim Data As Byte() = System.Text.Encoding.UTF8.GetBytes(xmltext)
            Dim stream As Stream = tcpClient.GetStream()
            stream.Write(Data, 0, Data.Length())

            tcpClient.Close()
        Catch ex As Exception
            Return "Daten konnten nicht zur FIS übertragen werden"
        End Try

        tcpClient.Dispose()
        Return ""
    End Function

End Class
