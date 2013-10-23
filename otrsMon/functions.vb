Imports MySql.Data.MySqlClient
Module functions
    Function getQueues()
        Dim retArray() As String
        Dim i As Integer
        Dim qnConnect As MySqlConnection = mainForm.mConnect()
        Dim qnQuery = "SELECT name FROM queue WHERE valid_id = '1'"
        Dim qnCmd As New MySqlCommand(qnQuery, qnConnect)
        Dim qnReader As MySqlDataReader
        qnReader = qnCmd.ExecuteReader
        While qnReader.Read
            Dim qName As String = qnReader.GetValue(0)
            i = i + 1
            retArray(i) = qName

        End While
        qnReader.Close()
        qnConnect.Dispose()
        qnCmd.Dispose()
        Return retArray
    End Function
    Function getQueues2()
        'TEST
        '<Get Queues>
        Dim retArray() As String
        Dim i As Integer = 0
        Dim qnConnect As MySqlConnection = mainForm.mConnect()
        Dim qnQuery = "SELECT name FROM queue WHERE valid_id = '1'"
        Dim qnCmd As New MySqlCommand(qnQuery, qnConnect)
        Dim qnReader As MySqlDataReader
        qnReader = qnCmd.ExecuteReader()
        While qnReader.Read
            Dim qName As String = qnReader.GetValue(0)
            retArray(i) = qName
            i = i + 1
            'MenuStrip1.Items.Add(qName)
            'ListBox1.Items.Add(qName)
        End While
        qnReader.Close()
        qnConnect.Dispose()
        qnCmd.Dispose()
        Return retArray
        '</get queues>
        '/TEST
    End Function

End Module
