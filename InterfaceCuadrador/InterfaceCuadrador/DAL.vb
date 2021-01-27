Imports System.Xml
Imports Microsoft.SqlServer
Imports System.Text
Imports MySql.Data.MySqlClient
Imports System.Threading

Public Module DAL
    Private cn As MySqlConnection
    Private da As MySqlDataAdapter
    Private dt As DataTable
    Private cm As MySqlCommand
    Private dr As MySqlDataAdapter
    Private mt As MySqlTransaction


    Public Enum TipoProcesamiento
        NonQuery = 1
        Scalar = 2
        DataTable = 3
        Reader = 4
    End Enum

    Public Enum BaseDeDatos
        DBSqlServer = 1
        DBOracle = 2
        DBMySql = 3
    End Enum



    Public Sub GeneraCadena(ByVal cConnectionString As String)
        cn = New MySqlConnection(cConnectionString)
    End Sub

    Public Sub conectar(ByVal StringDeConexion As String)
        GeneraCadena(StringDeConexion)
        cn.Open()
    End Sub

    Public Sub Desconectar()
        cn.Close()
    End Sub



    Public Function EjecutarSP(ByVal cSentencia As String, ByVal CONEXION As Object, ByVal cTipoProceso As TipoProcesamiento, Optional ByVal bAlerta As Boolean = True, Optional ByVal cBaseDeDatos As BaseDeDatos = 1) As Object
        Try
            conectar(IIf(cBaseDeDatos = BaseDeDatos.DBMySql, CONEXION.StringConn & "Uid=" & CONEXION.userID & ";Pwd=" & CONEXION.password & ";", ""))

            Dim cm As New MySqlCommand
            cm = New MySqlCommand(cSentencia, cn)


            If cTipoProceso = TipoProcesamiento.NonQuery Then
                cm.ExecuteNonQuery()
                Return True
            ElseIf cTipoProceso = TipoProcesamiento.Reader Then
                Dim reader As MySqlDataReader = cm.ExecuteReader

                Return reader
            ElseIf cTipoProceso = TipoProcesamiento.Scalar Then
                Return cm.ExecuteScalar
            ElseIf cTipoProceso = TipoProcesamiento.DataTable Then
                da = New MySqlDataAdapter(cm)
                dt = New DataTable()
                da.Fill(dt)
                Return dt
            Else
                Throw New Exception("Tipo de procesamiento no válido")
            End If
        Catch ex As Exception
            If bAlerta = True Then
                Console.WriteLine("DAL:EjectutarProcedure: " + ex.StackTrace & "Descripcion: " & ex.Message)
            End If
            Return Nothing
        Finally
            Desconectar()
        End Try
    End Function


End Module


