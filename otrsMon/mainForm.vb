Imports MySql.Data.MySqlClient
Imports System.Data
Imports System.Net.Mail
Imports Microsoft.Win32
Public Class mainForm
    Public lastTicket As String
    Private otrsSV As String
    Private otrsDB As String
    Private otrsUN As String
    Private otrsPW As String
    Private soundOn As String
    Private soundFile As String
    Private sound As System.Media.SoundPlayer
    Private m_SortingColumn As ColumnHeader
    Private Sub Form1_Load(sender As System.Object, e As System.EventArgs) Handles MyBase.Load
        'Web Browser
        setWebReg()

        'status strip
        ToolStripStatusLabel1.Text = "Waiting for new ticket..."

        'Set mainForm Title
        'Dim omVersion As String = My.Application.Deployment.CurrentVersion.ToString()

        'Me.Text = "OTRS Helpdesk Monitor System - Version: " & omVersion
        '/Set mainForm Title




        'Setup Timer
        Dim refreshRate As String = Settings.GetSetting("RefreshRate")
        Timer1.Interval = refreshRate * 1000
        Timer1.Enabled = True
        Try
            otrsSV = Settings.GetSetting("MySQLServer")
            otrsDB = Settings.GetSetting("MySQLDB")
            otrsUN = Settings.GetSetting("MySQLUsername")
            otrsPW = encrypto.Decrypt(Settings.GetSetting("MySQLPassword"))
            soundOn = Settings.GetSetting("soundOn")
            soundFile = Settings.GetSetting("soundFile")


        Catch ex As Exception
            MsgBox(ex.Message)
            setupForm.Show()
        End Try

        'Dim foo As Array
        'foo = functions.getQueues()
        'Do main run
        getQueue()

    End Sub
    Private Sub setWebReg()
        Dim regKey As RegistryKey
        Dim newKey As RegistryKey
        Dim regVal As Integer = "8000"
        regKey = Registry.CurrentUser.OpenSubKey("Software\Microsoft\Internet Explorer\Main\FeatureControl", True)
        regKey.CreateSubKey("FEATURE_BROWSER_EMULATION")
        newKey = Registry.CurrentUser.OpenSubKey("Software\Microsoft\Internet Explorer\Main\FeatureControl\FEATURE_BROWSER_EMULATION", True)
        newKey.SetValue("otrsMon.exe", 8000)
        regKey.Close()
        newKey.Close()

    End Sub
    Private Function checkForNew()
        Dim ltConnect As MySqlConnection = mConnect()
        Dim qids As String = getQids()

        'Dim ltQuery = "SELECT tn FROM ticket WHERE name NOT LIKE '%closed%' AND queue_id IN('1') ORDER BY tn DESC LIMIT 1"
        Dim ltQuery = "SELECT " & _
    "ticket.tn as tktNumber, " & _
    "ticket_state.name as tktName " & _
        "FROM(ticket) " & _
    "JOIN ticket_state ON ticket.ticket_state_id = ticket_state.id " & _
        "AND ticket.queue_id IN(" & qids & ") " & _
    "ORDER BY tn DESC " & _
    "LIMIT 1"
        'Debug.Print(ltQuery)
        '"WHERE ticket_state.name NOT LIKE '%closed%' " & _

        Dim ltCmd As New MySqlCommand(ltQuery, ltConnect)
        Dim ltReader As MySqlDataReader
        ltReader = ltCmd.ExecuteReader
        ltReader.Read()
        Dim newTicket As String
        Try
            newTicket = ltReader.GetString(0)
        Catch ex As Exception

        End Try

        ltReader.Close()
        ltCmd.Dispose()
        ltConnect.Dispose()

        'Checks if ticket is NEWER. Do not have action on close.
        If newTicket <= lastTicket Then
            Return 0
        Else
            ' New Ticket. Do actions!!!!
            'Grab SQL Data for alerts
            Dim ntConnect As MySqlConnection = mConnect()
            Dim ntQuery = "SELECT title, customer_user_id FROM ticket WHERE tn = '" & newTicket & "'"
            Dim ntCmd As New MySqlCommand(ntQuery, ntConnect)
            Dim ntReader As MySqlDataReader
            ntReader = ntCmd.ExecuteReader
            ntReader.Read()
            Dim ntTitle As String = ntReader.GetString(0)
            Dim ntFrom As String = ntReader.GetString(1)
            '/Grab SQL Data for alerts

            'Set time on status Strip
            ToolStripStatusLabel1.Text = "Newest Ticket from:" & ntFrom & " @ " & DateTime.Now.ToLongTimeString
            '/Set time on status strip

            'Email Alert
            'Dim emailEnable As String = Settings.GetSetting("enableEmailAlert")
            'If emailEnable = "Checked" Then
            'sendMail("New ticket from: " & ntFrom, ntTitle)
            'End If
            '/Email Alert

            ntConnect.Dispose()
            ntCmd.Dispose()
            ntReader.Close()

            Me.Activate()

            Me.BringToFront()
            If soundOn = True Then
                Dim sound As New System.Media.SoundPlayer()
                sound.SoundLocation = soundFile
                sound.Load()
                If soundOn = True Then
                    sound.Play()
                End If
                sound.Dispose()
            End If
        End If
    End Function
 
    Private Sub Timer1_Tick(sender As Object, e As System.EventArgs) Handles Timer1.Tick
        If CheckBox1.Checked Then
            Me.checkForNew()
            Me.getQueue()
        End If
    End Sub

    Private Sub ListView1_Click(sender As Object, e As System.EventArgs) Handles ListView1.Click
        Dim itemPoint As Point = ListView1.PointToClient(Cursor.Position)
        Dim sItem As ListViewItem = ListView1.GetItemAt(itemPoint.X, itemPoint.Y)
        Dim tip As New ToolTip

        tip.ToolTipTitle = "Ticket Details"
        tip.IsBalloon = False


        tip.Show("Feature coming soon!", Me, 0, itemPoint.Y + 55, 2000)
    End Sub

  
    Private Sub listview1_doubleclick(ByVal sender As Object, ByVal e As System.EventArgs) Handles ListView1.DoubleClick
        Dim itemPoint As Point = ListView1.PointToClient(Cursor.Position)
        Dim sItem As ListViewItem = ListView1.GetItemAt(itemPoint.X, itemPoint.Y)
        Dim ticketNumber As String = sItem.SubItems.Item(0).Text

        WebBrowser1.Navigate("http://helpdesk.sacnativehealth.org/otrs/index.pl?Action=AgentTicketZoom;TicketID=" & ticketNumber)



        'Process.Start("http://helpdesk.sacnativehealth.org/otrs/index.pl?Action=AgentTicketZoom;TicketID=" & ticketNumber)
    End Sub
    Function getQids()
        Dim qList As String = Settings.GetSetting("otrsQueues")
        'Debug.Print("getQueueList - qList: " & qList)
        Dim mArray() As String = qList.Split(",")
        Dim qids As String
        For Each qName In mArray
            Dim qlConnect As MySqlConnection = mConnect()
            Dim qlQuery = "SELECT id FROM queue WHERE name = '" & qName & "'"
            Dim qlCmd As New MySqlCommand(qlQuery, qlConnect)
            Dim qlReader As MySqlDataReader
            qlReader = qlCmd.ExecuteReader()
            qlReader.Read()
            Dim qid As String = qlReader.GetString(0)
            qids = qids & qid & ","
            qlReader.Close()
            qlCmd.Dispose()
            qlConnect.Dispose()
        Next
        qids = qids.Substring(0, qids.Length - 1)
        'Debug.Print(qids)
        Return qids
    End Function
    Function getStateIDs()
        Dim qList As String = Settings.GetSetting("otrsStates")
        'Debug.Print("getQueueList - qList: " & qList)
        Dim mArray() As String = qList.Split(",")
        Dim qids As String
        For Each qName In mArray
            Dim qlConnect As MySqlConnection = mConnect()
            Dim qlQuery = "SELECT id FROM ticket_state WHERE name = '" & qName & "'"
            Dim qlCmd As New MySqlCommand(qlQuery, qlConnect)
            Dim qlReader As MySqlDataReader
            qlReader = qlCmd.ExecuteReader()
            qlReader.Read()
            Dim qid As String = qlReader.GetString(0)
            qids = qids & qid & ","
            qlReader.Close()
            qlCmd.Dispose()
            qlConnect.Dispose()
        Next
        qids = qids.Substring(0, qids.Length - 1)
        'Debug.Print(qids)
        Return qids
    End Function
    Function mConnect()
        Dim connStr = "server=" & otrsSV & ";" _
         & "user id=" & otrsUN & ";" _
         & "password=" & otrsPW & ";" _
         & "database=" & otrsDB
        ' Connect to mysql
        Dim mysqlConn As New MySqlConnection(connStr)
        mysqlConn.Open()
        Return mysqlConn
    End Function
    Function getQueue()
        ListView1.Items.Clear()
        ' Setup Connection string
        Dim mainConnect As MySqlConnection
        mainConnect = mConnect()

        Dim qids As String
        qids = getQids()

        Dim sids As String
        sids = getStateIDs()


        'Main Query 
        'If adding any new fields, add to bottom!
        Dim mQuery As String = _
     "SELECT " & _
     "ticket.id," & _
     "ticket.title, " & _
     "ticket.customer_user_id," & _
     "ticket.create_time, " & _
     "ticket_type.name as tktType, " & _
     "ticket_state.name as tktState, " & _
     "ticket_priority.name as tktPriority, " & _
     "ticket.queue_id, " & _
     "ticket.tn, " & _
     "ticket.change_time, " & _
     "users.first_name " & _
     "FROM ticket " & _
     "JOIN ticket_type ON ticket.type_id = ticket_type.id " & _
     "JOIN ticket_state ON ticket.ticket_state_id = ticket_state.id " & _
     "JOIN ticket_priority ON ticket.ticket_priority_id = ticket_priority.id " &
     "JOIN users ON ticket.user_id = users.id " & _
     "AND queue_id IN(" & qids & ")" & _
     "AND ticket_state_id IN(" & sids & ")" & _
     "ORDER BY ticket.tn DESC"
        '     '"WHERE ticket_state.name NOT LIKE 'frankandcharlie' " & _
        'Setup reader
        Dim cmd As New MySqlCommand(mQuery, mainConnect)
        Dim reader As MySqlDataReader
        reader = cmd.ExecuteReader()


        Dim cnt As Integer = 0
        While reader.Read

            Dim tn As String = ""
            Dim title As String = ""
            Dim complainer As String = ""
            Dim timeCreated As String = ""
            Dim tktType As String = ""
            Dim tktState As String = ""
            Dim tktPriority As String = ""
            Dim tktQueue As String = ""
            Dim tktNumber As String = ""
            Dim timeChanged As String = ""
            Dim tktUserID As String = ""



            tn = reader.GetValue(0)
            title = reader.GetString(1)
            complainer = reader.GetString(2)
            timeCreated = reader.GetString(3)
            tktType = reader.GetString(4)
            tktState = reader.GetString(5)
            tktPriority = reader.GetString(6)
            tktQueue = reader.GetString(7)
            tktNumber = reader.GetString(8)
            timeChanged = reader.GetString(9)
            tktUserID = reader.GetString(10)





            'Debug.Print("TN: " & tn)

            'mainForm.lastTicket = tktNumber
            If cnt = 0 Then
                lastTicket = tktNumber
            End If

            'Get Queue Name
            Dim qnConnect As MySqlConnection = mConnect()
            Dim qnQuery = "SELECT name FROM queue WHERE id = '" & tktQueue & "'"
            'Debug.Print("QueueNameQ: " & qnQuery)
            Dim qnCmd As New MySqlCommand(qnQuery, qnConnect)
            Dim qnReader As MySqlDataReader
            qnReader = qnCmd.ExecuteReader()
            qnReader.Read()
            Dim qName As String
            qName = qnReader.GetString(0)
            'Debug.Print("Queue Name: " & qName)
            qnReader.Close()
            qnCmd.Dispose()
            qnConnect.Dispose()

            '<get Age of ticket>
            Dim dFrom As DateTime
            Dim dTo As DateTime
            Dim timeNow As String = Date.Now()
            Dim age As String
            If DateTime.TryParse(timeCreated, dFrom) AndAlso DateTime.TryParse(timeNow, dTo) Then
                Dim TS As TimeSpan = dTo - dFrom
                Dim days As Integer = TS.Days
                Dim hour As Integer = TS.Hours
                Dim mins As Integer = TS.Minutes
                Dim secs As Integer = TS.Seconds

                Dim timeDiff As String = ((days.ToString("00") & " - ") + (hour.ToString("00") & ":") + mins.ToString("00"))
                age = timeDiff
            Else
                age = "00"
            End If
            '</get age of ticket>


            Dim itmStr(10) As String
            Dim itm As ListViewItem
            itmStr(0) = tn
            itmStr(1) = title
            itmStr(2) = complainer
            itmStr(3) = timeCreated
            itmStr(4) = tktType
            itmStr(5) = tktState
            itmStr(6) = tktPriority
            itmStr(7) = qName
            itmStr(8) = timeChanged
            itmStr(9) = age
            itmStr(10) = tktUserID

            'Column Ordering
            ListView1.Columns(0).DisplayIndex = 0   'tn
            ListView1.Columns(1).DisplayIndex = 1   'title
            ListView1.Columns(2).DisplayIndex = 2   'complainer
            ListView1.Columns(3).DisplayIndex = 3   'timeCreated
            ListView1.Columns(4).DisplayIndex = 5   'tktType
            ListView1.Columns(5).DisplayIndex = 6   'tktState
            ListView1.Columns(6).DisplayIndex = 7   'tktPriority
            ListView1.Columns(7).DisplayIndex = 8   'qName
            ListView1.Columns(8).DisplayIndex = 4   ' timeChanged
            ListView1.Columns(9).DisplayIndex = 9   ' age
            ListView1.Columns(10).DisplayIndex = 10 ' Owner


            '
            itm = New ListViewItem(itmStr)

            Select Case tktPriority
                Case "1 very low"
                    itm.ForeColor = Color.Blue
                Case "2 low"
                    itm.ForeColor = Color.DarkBlue
                Case "3 normal"
                    itm.ForeColor = Color.Black
                Case "4 high"
                    itm.ForeColor = Color.DarkOrange
                Case "5 very high"
                    itm.ForeColor = Color.Red

            End Select

            ListView1.Items.Add(itm)

            'Set open ticket number
            ToolStripStatusLabel2.Text = "Open Tickets: " & cnt
            cnt = cnt + 1
        End While
        reader.Close()
        cmd.Dispose()
        mainConnect.Dispose()
        Return 0

    End Function
    Public Sub sendMail(ByVal mailSub As String, ByVal mailMessage As String)
        Try
            Dim mServer As String = Settings.GetSetting("smtpServer")
            Dim mPort As String = Settings.GetSetting("smtpPort")
            Dim sendTo As String = Settings.GetSetting("emailAddress")
            Dim emailUN As String = Settings.GetSetting("emailUsername")
            Dim emailPW As String = encrypto.Decrypt(Settings.GetSetting("emailPassword"))

            Dim SmtpServer As New SmtpClient()
            Dim mail As New MailMessage()
            SmtpServer.Credentials = New Net.NetworkCredential("kalebc", "SomfAG:D")
            SmtpServer.Port = mPort
            SmtpServer.Host = mServer
            mail = New MailMessage()
            mail.From = New MailAddress("otrs@snahc.org")
            mail.To.Add(sendTo)
            mail.Subject = mailSub
            mail.Body = mailMessage
            SmtpServer.Send(mail)
        Catch ex As Exception
            MsgBox(ex.ToString)
        End Try
    End Sub

    Private Sub SetupToolStripMenuItem_Click(sender As System.Object, e As System.EventArgs) Handles SetupToolStripMenuItem.Click
        setupForm.Show()
    End Sub

    Private Sub ExitToolStripMenuItem_Click(sender As System.Object, e As System.EventArgs) Handles ExitToolStripMenuItem.Click
        Application.Exit()
    End Sub

    Private Sub ManualRefreshToolStripMenuItem_Click(sender As System.Object, e As System.EventArgs) Handles ManualRefreshToolStripMenuItem.Click
        Me.checkForNew()
        Me.getQueue()
    End Sub

    Private Sub AutoRefreshToolStripMenuItem_Click(sender As System.Object, e As System.EventArgs) Handles AutoRefreshToolStripMenuItem.Click
        If CheckBox1.Checked = True Then
            CheckBox1.Checked = False
            AutoRefreshToolStripMenuItem.Checked = False
        ElseIf CheckBox1.Checked = False Then
            CheckBox1.Checked = True
            AutoRefreshToolStripMenuItem.Checked = True
        End If
    End Sub

    Private Sub WebBrowser1_DocumentCompleted(sender As System.Object, e As System.Windows.Forms.WebBrowserDocumentCompletedEventArgs) Handles WebBrowser1.DocumentCompleted
        'Do things after page has loaded.
    End Sub
    Private Sub ListView1_ColumnClick(ByVal sender As System.Object, ByVal e As System.Windows.Forms.ColumnClickEventArgs) Handles ListView1.ColumnClick
        ' Get the new sorting column.
        Dim new_sorting_column As ColumnHeader = ListView1.Columns(e.Column)
        ' Figure out the new sorting order.
        Dim sort_order As System.Windows.Forms.SortOrder
        If m_SortingColumn Is Nothing Then
            ' New column. Sort ascending.
            sort_order = SortOrder.Ascending
        Else ' See if this is the same column.
            If new_sorting_column.Equals(m_SortingColumn) Then
                ' Same column. Switch the sort order.
                If m_SortingColumn.Text.StartsWith("> ") Then
                    sort_order = SortOrder.Descending
                Else
                    sort_order = SortOrder.Ascending
                End If
            Else
                ' New column. Sort ascending.
                sort_order = SortOrder.Ascending
            End If
            ' Remove the old sort indicator.
            m_SortingColumn.Text = m_SortingColumn.Text.Substring(2)
        End If
        ' Display the new sort order.
        m_SortingColumn = new_sorting_column
        If sort_order = SortOrder.Ascending Then
            m_SortingColumn.Text = "> " & m_SortingColumn.Text
        Else
            m_SortingColumn.Text = "< " & m_SortingColumn.Text
        End If
        ' Create a comparer.
        ListView1.ListViewItemSorter = New clsListviewSorter(e.Column, sort_order)
        ' Sort.
        ListView1.Sort()
    End Sub

    Private Sub ListView1_ItemMouseHover(sender As Object, e As System.Windows.Forms.ListViewItemMouseHoverEventArgs) Handles ListView1.ItemMouseHover
        
        
    End Sub

    Private Sub RefreshListToolStripMenuItem_Click(sender As System.Object, e As System.EventArgs) Handles RefreshListToolStripMenuItem.Click
        Me.checkForNew()
        Me.getQueue()
    End Sub
   
End Class
