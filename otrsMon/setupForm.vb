Imports MySql.Data.MySqlClient
Imports System.Data

Public Class setupForm

    Private Sub Button1_Click(sender As System.Object, e As System.EventArgs) Handles Button1.Click
        Me.Close()

    End Sub

    Private Sub setupForm_Load(sender As System.Object, e As System.EventArgs) Handles MyBase.Load

        txtServer.Text = Settings.GetSetting("MySQLServer")
        txtUserName.Text = Settings.GetSetting("MySQLUsername")
        txtPassword.Text = encrypto.Decrypt(Settings.GetSetting("MySQLPassword"))
        txtDatabase.Text = Settings.GetSetting("MySQLDB")
        txtSmtpServer.Text = Settings.GetSetting("smtpServer")
        txtSmtpPort.Text = Settings.GetSetting("smtpPort")
        txtEmailAddress.Text = Settings.GetSetting("emailAddress")
        txtEmailUN.Text = Settings.GetSetting("emailUsername")
        txtEmailPW.Text = Settings.GetSetting("emailPassword")
        txtSound.Text = Settings.GetSetting("soundFile")
        cbSoundOn.Checked = Settings.GetSetting("soundOn")
        cbEmailAlert.Checked = Settings.GetSetting("enableEmailAlert")

        '<Get Queues>
        Dim qnConnect As MySqlConnection = mainForm.mConnect()
        Dim qnQuery = "SELECT name FROM queue WHERE valid_id = '1'"
        Dim qnCmd As New MySqlCommand(qnQuery, qnConnect)
        Dim qnReader As MySqlDataReader
        qnReader = qnCmd.ExecuteReader()
        While qnReader.Read
            Dim qName As String = qnReader.GetValue(0)
            ListBox1.Items.Add(qName)
        End While
        qnReader.Close()
        qnConnect.Dispose()
        qnCmd.Dispose()

        Dim qs As String
        qs = Settings.GetSetting("otrsQueues")
        Dim qArray() As String
        qArray = qs.Split(",")
        '</get queues>
        '<Get States>
        Dim snConnect As MySqlConnection = mainForm.mConnect()
        Dim snQuery = "SELECT name FROM ticket_state WHERE valid_id = '1'"
        Dim snCmd As New MySqlCommand(snQuery, snConnect)
        Dim snReader As MySqlDataReader
        snReader = snCmd.ExecuteReader()
        While snReader.Read
            Dim sName As String = snReader.GetValue(0)
            ListBox2.Items.Add(sName)
        End While
        snReader.Close()
        snConnect.Dispose()
        snCmd.Dispose()
        Dim ss As String
        ss = Settings.GetSetting("otrsStates")
        Dim sArray() As String
        sArray = ss.Split(",")
        '</get states>
 
        'set refresh rate
        Dim refreshRate As String = Settings.GetSetting("RefreshRate")
        For i = 0 To ComboBox1.Items.Count - 1
            If ComboBox1.Items(i).ToString = refreshRate Then
                ComboBox1.SelectedItem = ComboBox1.Items(i)
            End If
        Next


        'Set queues
        For i = 0 To ListBox1.Items.Count - 1
            For Each q As String In qArray
                If ListBox1.Items(i).ToString = q Then
                    ListBox1.SetSelected(i, True)
                End If
            Next
        Next

        'Set States
        For i = 0 To ListBox2.Items.Count - 1
            For Each q As String In sArray
                If ListBox2.Items(i).ToString = q Then
                    ListBox2.SetSelected(i, True)
                End If
            Next
        Next


    End Sub

    Private Sub Button2_Click(sender As System.Object, e As System.EventArgs) Handles Button2.Click
        Settings.SetSetting("MySQLServer", txtServer.Text)
        Settings.SetSetting("MySQLUsername", txtUserName.Text)
        Dim sMySQLPassword As String = encrypto.Encrypt(txtPassword.Text)
        'MsgBox(sMySQLPassword)
        'MsgBox(encrypto.Decrypt(sMySQLPassword))

        Settings.SetSetting("MySQLPassword", sMySQLPassword)
        Settings.SetSetting("MySQLDB", txtDatabase.Text)
        Settings.SetSetting("smtpServer", txtSmtpServer.Text)
        Settings.SetSetting("smtpPort", txtSmtpPort.Text)
        Settings.SetSetting("emailAddress", txtEmailAddress.Text)
        Settings.SetSetting("emailUsername", txtEmailUN.Text)
        Settings.SetSetting("emailPassword", encrypto.Encrypt(txtEmailPW.Text))
        Settings.SetSetting("soundFile", txtSound.Text)
        Settings.SetSetting("soundOn", cbSoundOn.Checked)
        Settings.SetSetting("enableEmailAlert", cbEmailAlert.Checked)

        '<Save Queues>
        Dim qs As String
        For Each lbItem As Object In Me.ListBox1.SelectedItems
            qs = qs & lbItem.ToString & ","
        Next
        qs = qs.Substring(0, qs.Length - 1)
        Settings.SetSetting("otrsQueues", qs)
        '</save queues>

        '<Save states>
        Dim ss As String
        For Each lbItem As Object In Me.ListBox2.SelectedItems
            ss = ss & lbItem.ToString & ","
        Next
        ss = ss.Substring(0, ss.Length - 1)
        Settings.SetSetting("otrsStates", ss)
        '</save states>
        Settings.SetSetting("refreshRate", ComboBox1.SelectedItem.ToString)
        mainForm.Timer1.Interval = ComboBox1.SelectedItem.ToString * 1000
    End Sub

    Private Sub ComboBox1_SelectedIndexChanged(sender As System.Object, e As System.EventArgs) Handles ComboBox1.SelectedIndexChanged

    End Sub

    Private Sub Button3_Click(sender As System.Object, e As System.EventArgs) Handles Button3.Click
        mainForm.sendMail("test Message", "otrsMon Test Message")
    End Sub

    Private Sub Button4_Click(sender As System.Object, e As System.EventArgs) Handles Button4.Click
        OpenFileDialog1.Title = "Choose a sound file"
        OpenFileDialog1.InitialDirectory = "C:\Windows"
        OpenFileDialog1.ShowDialog()
    End Sub

    Private Sub OpenFileDialog1_FileOk(sender As Object, e As System.ComponentModel.CancelEventArgs) Handles OpenFileDialog1.FileOk
        txtSound.Text = OpenFileDialog1.FileName.ToString
    End Sub
End Class