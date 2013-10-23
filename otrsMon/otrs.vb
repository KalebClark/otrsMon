Imports MySql.Data.MySqlClient
Imports System.Data
Public Class otrs
    Public mysqlConn As MySqlConnection

    Private otrsSV = Settings.GetSetting("MySQLServer")
    Private otrsDB = Settings.GetSetting("MySQLDB")
    Private otrsUN = Settings.GetSetting("MySQLUsername")
    Private otrsPW = Settings.GetSetting("MySQLPassword")
    Public Sub New()
        Dim connStr = "server=" & otrsSV & ";" _
         & "user id=" & otrsUN & ";" _
         & "password=" & otrsPW & ";" _
         & "database=" & otrsDB

        'Public mysqlConn As New MySqlConnection(connStr)
        Me.mysqlConn = New MySqlConnection(connStr)

        Me.mysqlConn.Open()


    End Sub
    'Public Function connect()
    ' 'mysqlConn = New MySqlConnection
    ' Dim conn = Me.mysqlConn.ConnectionString = "server=" & otrsSV & ";" _
    '  & "user id=" & otrsUN & ";" _
    '  & "password=" & otrsPW & ";" _
    '  & "database=" & otrsDB
    '     Return mysqlConn
    ' End Function
    Function disconnect()
        Me.mysqlConn.Close()
        Return 0
    End Function
    Public Function getRows(ByVal query As String)

        Try
            'Dim mysqlConn = Me.connect
        Catch ex As Exception
            MsgBox(ex.Message)
        End Try

        Try
            'mysqlConn.open()
            Debug.Print("GETROWS - query: " & query)
            Dim cmd As New MySqlCommand(query, mysqlConn)
            Dim reader As MySqlDataReader
            reader = cmd.ExecuteReader()

            Try
                reader.Read()
                Return reader
                'Return mArray
            Catch ex As Exception
                MsgBox("GETROWS - read: " & ex.Message)
                Return 1
            Finally
                reader.Close()
            End Try

        Catch ex As Exception
            MsgBox("GetRows - Open: " & ex.Message)
            Return 0
        End Try
    End Function
    Public Function getQueueName(ByVal qID As String)
        'Dim oi = New otrs
        Dim qName As String
        Dim rdr As MySqlDataReader
        rdr = Me.getRows("SELECT name FROM queue WHERE id = '" & qID & "'")
        qName = rdr.GetString(0)
        rdr.Close()
        Return qName
    End Function
    Public Function getQueueID(ByVal qName As String)
        Dim qID As String
        Dim rdr As MySqlDataReader
        '''Dim rdr() As String
        Dim q As String = "SELECT id FROM queue WHERE name = '" & qName & "'"
        rdr = Me.getRows(q)
        qID = rdr.GetString(0)
        '''qID = rdr(0)
        rdr.Close()
        Return (qID)
    End Function
    Public Function getLastTicket()
        Dim rdr As MySqlDataReader
        '''Dim rdr As String
        Dim qs As String = getQueueList()
        Dim lTicket As String
        Dim q As String = "SELECT tn FROM ticket WHERE name NOT LIKE '%closed%' AND queue_id IN('" & qs & "') ORDER BY tn DESC LIMIT 1"
        rdr = Me.getRows(q)
        lTicket = rdr.GetString(0)
        '''lTicket = rdr(0)

        rdr.Close()

        Return lTicket
    End Function

    Public Function getQueueList()
        Dim qList As String = Settings.GetSetting("otrsQueues")
        Debug.Print("getQueueList - qList: " & qList)
        Dim mArray() As String = qList.Split(",")
        Dim qids As String
        For Each qName In mArray
            Dim qid As String = Me.getQueueID(qName)

            qids = qids & qid & ","
        Next
        qids = qids.Substring(0, qids.Length - 1)
        Return qids
    End Function
    Public Function getQueue()
        mainForm.ListView1.Items.Clear()
        Debug.Print("START getQueue() Run...")
        Dim qs As String = getQueueList()
        Debug.Print("getQueue - qs: " & qs)

        '''Dim mysqlConn = Me.connect
        Try
            '''mysqlConn.Open()

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
                "ticket.tn " & _
                "FROM ticket " & _
                "JOIN ticket_type ON ticket.type_id = ticket_type.id " & _
                "JOIN ticket_state ON ticket.ticket_state_id = ticket_state.id " & _
                "JOIN ticket_priority ON ticket.ticket_priority_id = ticket_priority.id " & _
                "WHERE ticket_state.name NOT LIKE '%closed%' " & _
                "AND queue_id IN(" & qs & ")" & _
                "ORDER BY ticket.tn DESC"
            'MsgBox(mQuery)

            '''Dim cmd As New MySqlCommand(mQuery, mysqlConn)

            Dim reader As MySqlDataReader
            '''reader = cmd.ExecuteReader()
            reader = Me.getRows(mQuery)

            Dim cnt As Integer
            '''While reader.Read()
            For Each item In reader




                Dim tn As String
                Dim title As String
                Dim complainer As String
                Dim timeCreated As String
                Dim tktType As String
                Dim tktState As String
                Dim tktPriority As String
                Dim tktQueue As String
                Dim tktNumber As String


                tn = reader.GetValue(0)
                title = reader.GetString(1)
                complainer = reader.GetString(2)
                timeCreated = reader.GetString(3)
                tktType = reader.GetString(4)
                tktState = reader.GetString(5)
                tktPriority = reader.GetString(6)
                tktQueue = reader.GetString(7)
                tktNumber = reader.GetString(8)

                'mainForm.lastTicket = tktNumber
                If cnt = 0 Then
                    mainForm.lastTicket = tktNumber
                End If


                Dim itmStr(7) As String
                Dim itm As ListViewItem

                itmStr(0) = tn
                itmStr(1) = title
                itmStr(2) = complainer
                itmStr(3) = timeCreated
                itmStr(4) = tktType
                itmStr(5) = tktState
                itmStr(6) = tktPriority
                itmStr(7) = Me.getQueueName(tktQueue)
                '
                itm = New ListViewItem(itmStr)
                mainForm.ListView1.Items.Add(itm)


                cnt = cnt + 1
            Next
            reader.Close()


        Catch ex As Exception
            MessageBox.Show("busted: " & ex.Message)
        Finally
            'mysqlConn.Dispose()

        End Try
        Return 0
    End Function
End Class
