Imports InterfaceCuadrador.Contenedores
Imports MySql.Data.MySqlClient
Imports System.Threading
Imports System.Configuration


'Imports System
'Imports System.Runtime.InteropServices
'Imports System.Security.Principal
'Imports System.Security.Permissions


Module Module1
    Dim cn_server, cn_caja As New Contenedores.CONEXIONSettings
    Dim reader As New AppSettingsReader()
    Dim des As New Cifrado.ClaseCifrarConexion
    Dim fechaString As String
    Dim dt_tiendas, dt_cajas, dt_sumatorias, dt_queries As New DataTable
    Dim dt_sumatorias_tienda As New DataTable
    Dim Sentencia_MySql As String
    Dim ServerCaja As String
    Dim ListaTiendas As String
    Dim VectorHilos_1 As New Generic.List(Of System.Threading.Thread)
    Dim VectorHilos_2 As New Generic.List(Of System.Threading.Thread)

    Sub Main()

        If Environment.GetCommandLineArgs.Length > 1 Then
            ServerCaja = Environment.GetCommandLineArgs(1).ToUpper()
        End If


        If Environment.GetCommandLineArgs.Length > 2 Then
            ListaTiendas = " tienda BETWEEN " + Environment.GetCommandLineArgs(2) + " AND " + Environment.GetCommandLineArgs(3)
        End If

        If Environment.GetCommandLineArgs.Length > 4 Then
            fechaString = "'" + Environment.GetCommandLineArgs(4) + "'"
        Else
            fechaString = "'" + Now.ToString("yyyyMMdd") + "'"


            ' PRUEBAS
            ' - - - - -- - - - - - - - - - - -- - - -
            'fechaString = "'20201208'"  'PARA DEBUG CON FECHA ESPECIFICA
            'ServerCaja = "C"
            'ListaTiendas = " tienda BETWEEN 6 AND 6"
            'MsgBox("Modo debug")
            ' - - - - -- - - - - - - - - - - -- - - -
        End If





        cn_server.StringConn = reader.GetValue("MYSQL_CONEXION", GetType(String)).ToString() '"Data Source = 192.168.101.66; Initial Catalog = EXACTUS;"
        cn_server.userID = reader.GetValue("MYSQL_USUARIO", GetType(String)).ToString() '""
        cn_server.password = des.DesEncriptar(reader.GetValue("MYSQL_CLAVE", GetType(String)).ToString())
        cn_server.Esquema = reader.GetValue("MYSQL_ESQUEMA", GetType(String)).ToString()
        Console.WriteLine(Now())


        'CierresDobles()
        'Return


        If Environment.GetCommandLineArgs(2) = "0" And Environment.GetCommandLineArgs(3) = "0" Then
            CargaGuatefacturas()
        End If



        If ServerCaja = "S" Then
            ReiniciarResumen()
        End If
        Console.WriteLine(Now())
        actualizarResumen()
        Console.WriteLine("Proceso terminado...")
        Console.WriteLine(Now())
    End Sub


    Private Sub actualizarResumen()
        'Dim VectorHilos_1 As New Generic.List(Of System.Threading.Thread)
        'Dim dt_sumatorias As DataTable
        Dim parametros, parametros_tienda As New ParametrosHilos
        parametros.StringConn = cn_server.StringConn
        parametros.userID = cn_server.userID
        parametros.password = cn_server.password
        parametros.Esquema = cn_server.Esquema










        Dim query_Mysql As String
        Sentencia_MySql = ""
        Sentencia_MySql = Sentencia_MySql & "SELECT consulta FROM cuadrador.sumatorias WHERE orden=1 ORDER BY orden"
        query_Mysql = EjecutarSP(Sentencia_MySql, cn_server, TipoProcesamiento.Scalar, True, BaseDeDatos.DBMySql)


        Sentencia_MySql = ""

        If ServerCaja = "C" Then
            Sentencia_MySql = Sentencia_MySql & "SELECT * FROM cuadrador.cajas WHERE" + ListaTiendas
        Else
            Sentencia_MySql = Sentencia_MySql & "SELECT * FROM cuadrador.tiendas WHERE tienda IN (SELECT tienda FROM cuadrador.cajas) AND (" + ListaTiendas + ")"
        End If
        Console.WriteLine(Sentencia_MySql)


        Try
            dt_cajas = CType(EjecutarSP(Sentencia_MySql, cn_server, TipoProcesamiento.DataTable, True, BaseDeDatos.DBMySql), DataTable)
        Catch ex As Exception
            Console.WriteLine("Error generando dt_cajas")
            Console.WriteLine(ex.Message)

        End Try


        'Dim conteoHilos_0 As Integer = 0



        Sentencia_MySql = ""
        Sentencia_MySql = query_Mysql.Replace("&fecha", fechaString)
        cn_caja.Sentencia = Sentencia_MySql

        For Each row_caja As DataRow In dt_cajas.Rows
            '0 tienda 1 caja 2 ip 3 puerto 4 usuario 5 clave

            If My.Computer.Network.Ping(row_caja(2)) Then
                Console.WriteLine(Now().ToString + "->" + row_caja(2))
                cn_caja.StringConn = "Server=" + row_caja(2) + ";port=" + row_caja(3) + ";Connect Timeout=10;"
                cn_caja.userID = row_caja(4)
                cn_caja.password = des.DesEncriptar(row_caja(5))
                cn_caja.Esquema = "superpos"

                'dt_sumatorias = CType(EjecutarSP(Sentencia_MySql, cn_caja, TipoProcesamiento.DataTable, True, BaseDeDatos.DBMySql), DataTable)

                VectorHilos_1.Add(New System.Threading.Thread(AddressOf SelectCaja))
                VectorHilos_1(VectorHilos_1.Count - 1).Start(cn_caja)
                VectorHilos_1(VectorHilos_1.Count - 1).Join()

            Else
                Console.WriteLine("Sin respuesta: " + row_caja(2))
                '(fecha, tienda, caja, ip, puerto, comentario)
                EscribirLog(fechaString, row_caja(0), row_caja(1), row_caja(2), row_caja(3), "No se tuvo respuesta a PING")
            End If
        Next
    End Sub


    Public Sub SelectCaja(ByVal PconexionCaja As Object)
        'Dim conexion As New CONEXIONSettings
        'conexion = PconexionCaja

        Try
            dt_sumatorias = CType(EjecutarSP(PconexionCaja.Sentencia, PconexionCaja, TipoProcesamiento.DataTable, True, BaseDeDatos.DBMySql), DataTable)
            'dt_sumatorias = CType(EjecutarSP(conexion.Sentencia, conexion, TipoProcesamiento.DataTable, True, BaseDeDatos.DBMySql), DataTable)
            Dim parametros, parametros_tienda As New ParametrosHilos
            parametros.StringConn = cn_server.StringConn
            parametros.userID = cn_server.userID
            parametros.password = cn_server.password
            parametros.Esquema = cn_server.Esquema
            parametros.DTable = dt_sumatorias
            parametros.Tipo = ServerCaja

            If dt_sumatorias.Rows.Count > 0 Then
                'VectorHilos_2.Add(New System.Threading.Thread(AddressOf UpdateCaja))
                'VectorHilos_2(VectorHilos_2.Count - 1).Start(parametros)
                'Thread.Sleep(500)
                UpdateCaja(parametros)
            Else
                Console.WriteLine("Datatable sumatorias vacio")
            End If
        Catch ex As Exception
            Console.WriteLine("No se pudo generar tabla de sumatorias - Inicio")
            Console.WriteLine(ex.Message)
            Console.WriteLine(ex.StackTrace)
            Console.WriteLine("No se pudo generar tabla de sumatorias - Fin")
        End Try


    End Sub

    Public Sub SelectCaja_(ByVal PconexionCaja As Object)
        Dim conexion As New CONEXIONSettings
        conexion = PconexionCaja

        Try

            'dt_sumatorias = CType(EjecutarSP(PconexionCaja.Sentencia, PconexionCaja, TipoProcesamiento.DataTable, True, BaseDeDatos.DBMySql), DataTable)

            dt_sumatorias = CType(EjecutarSP(conexion.Sentencia, conexion, TipoProcesamiento.DataTable, True, BaseDeDatos.DBMySql), DataTable)

            Dim parametros, parametros_tienda As New ParametrosHilos
            parametros.StringConn = cn_server.StringConn
            parametros.userID = cn_server.userID
            parametros.password = cn_server.password
            parametros.Esquema = cn_server.Esquema
            parametros.DTable = dt_sumatorias
            parametros.Tipo = ServerCaja

            If dt_sumatorias.Rows.Count > 0 Then
                'VectorHilos_2.Add(New System.Threading.Thread(AddressOf UpdateCaja))
                'VectorHilos_2(VectorHilos_2.Count - 1).Start(parametros)
                'Thread.Sleep(500)
                UpdateCaja(parametros)
            Else
                Console.WriteLine("Datatable sumatorias vacio")
            End If
        Catch ex As Exception
            Console.WriteLine("No se pudo generar tabla de sumatorias - Inicio")
            Console.WriteLine(ex.Message)
            Console.WriteLine(ex.StackTrace)
            Console.WriteLine("No se pudo generar tabla de sumatorias - Fin")
        End Try


    End Sub




    Public Sub UpdateCaja(ByVal Pconexion As Object)

        Sentencia_MySql = ""
        Sentencia_MySql = "UPDATE cuadrador.resumen SET "
            Dim flagCampos As Integer = 0
            For Each row_sumatoria As DataRow In Pconexion.DTable.Rows
                If flagCampos > 0 Then
                    Sentencia_MySql &= ","
                End If
                Sentencia_MySql &= "C" + row_sumatoria(2).ToString + "=" + row_sumatoria(4).ToString
                flagCampos += 1
                If flagCampos = 8 Or flagCampos = Pconexion.dtable.rows.count() Then
                    Sentencia_MySql &= " WHERE tienda=" + row_sumatoria(0).ToString + " AND caja=" + row_sumatoria(1).ToString + " AND tipo='" + Pconexion.tipo + "' And  fecha=" + fechaString
                Try
                    EjecutarSP(Sentencia_MySql, Pconexion, TipoProcesamiento.NonQuery, True, BaseDeDatos.DBMySql)
                    Sentencia_MySql = ""
                    Sentencia_MySql = "UPDATE cuadrador.resumen Set "
                    flagCampos = 0
                Catch ex As Exception
                    Console.WriteLine("Error en actualizacion en caja")
                    Console.WriteLine(ex.Message)
                    End Try
                End If
            Next

    End Sub





    Public Sub ReiniciarResumen()
        Dim Sentencia_MySql As String
        Sentencia_MySql = ""
        Sentencia_MySql = Sentencia_MySql & "DELETE FROM cuadrador.resumen WHERE fecha=" + fechaString + " AND " + ListaTiendas
        Sentencia_MySql = EjecutarSP(Sentencia_MySql, cn_server, TipoProcesamiento.NonQuery, True, BaseDeDatos.DBMySql)

        Sentencia_MySql = ""
        Sentencia_MySql = Sentencia_MySql & "INSERT INTO cuadrador.resumen (fecha, tienda, caja, tipo) "
        Sentencia_MySql = Sentencia_MySql & "Select " + fechaString + ",tienda,caja,'C' "
        Sentencia_MySql = Sentencia_MySql & "From cuadrador.cajas WHERE" + ListaTiendas
        Sentencia_MySql = Sentencia_MySql & " UNION ALL "
        Sentencia_MySql = Sentencia_MySql & "Select " + fechaString + ",tienda,caja,'S' "
        Sentencia_MySql = Sentencia_MySql & "From cuadrador.cajas WHERE" + ListaTiendas
        Sentencia_MySql = EjecutarSP(Sentencia_MySql, cn_server, TipoProcesamiento.NonQuery, True, BaseDeDatos.DBMySql)
    End Sub


    Public Sub EscribirLog(ByVal Pfecha As String, ByVal Ptienda As Integer, ByVal Pcaja As Integer, ByVal Pip As String, ByVal Ppuerto As String, ByVal Pcomentario As String)
        Dim Sentencia_MySql As String
        Sentencia_MySql = ""

        Sentencia_MySql = ""
        Sentencia_MySql = Sentencia_MySql & "INSERT INTO cuadrador.resumen_log (fecha,tienda,caja,ip,puerto,comentario)  VALUES "
        Sentencia_MySql = Sentencia_MySql & "(" + fechaString + "," + Ptienda.ToString() + "," + Pcaja.ToString()
        Sentencia_MySql = Sentencia_MySql & ",'" + Pip + "','" + Ppuerto + "','" + Pcomentario + "')"

        Console.WriteLine(Sentencia_MySql)

        Sentencia_MySql = EjecutarSP(Sentencia_MySql, cn_server, TipoProcesamiento.NonQuery, True, BaseDeDatos.DBMySql)

    End Sub



    Public Sub CargaGuatefacturas()
        Dim ruta As String = reader.GetValue("SITIO_GUATEFACTURAS", GetType(String)).ToString()
        Dim archivo As String = reader.GetValue("IDENTIFICADOR_SITIO", GetType(String)).ToString()
        Dim Sentencia_Load As String

        '//srvfile/FTP UNISUPERGT/Guatefacturas/2020-12/17

        'LOAD Data LOCAL INFILE 'C:/Users/hmarin/source/repos/Cuadrador/Cuadrador/bin/Debug/26532476_20201215.txt'
        'INTO TABLE guatefacturas
        'FIELDS TERMINATED BY '|' 
        'LINES TERMINATED BY '\n'
        'SET fecha='2020-12-15'
        '20201217


        ruta &= Mid(fechaString, 2, 4) + "-" + Mid(fechaString, 6, 2) + "\" + Mid(fechaString, 8, 2) + "\"
        archivo = archivo + Mid(fechaString, 2, 8) + ".txt"

        Console.WriteLine(ruta + archivo)

        If My.Computer.FileSystem.FileExists(ruta + archivo) Then
            System.IO.File.Copy(ruta + archivo, "C:\temp\" + archivo, True)
        Else
            Console.WriteLine("No existe el archivo")
            System.IO.File.Copy(ruta + archivo, "C:\temp\" + archivo, True)
        End If


        'Console.WriteLine("C:/Users/hmarin/source/repos/Cuadrador/Cuadrador/bin/Debug/" + archivo)

        'archivo = "C:/temp/" + archivo


        'archivo = "C:/Users/hmarin/source/repos/Cuadrador/Cuadrador/bin/Debug/26532476_20201215.txt"


        If My.Computer.FileSystem.FileExists("C:\temp\" + archivo) Then

            Sentencia_Load = "LOAD Data LOCAL INFILE '" + "C:/temp/" + archivo + "' " + vbLf ' C:/Users/hmarin/source/repos/Cuadrador/Cuadrador/bin/Debug/26532476_20201215.txt'
            Sentencia_Load &= "INTO TABLE cuadrador.guatefacturas " + vbLf
            Sentencia_Load &= "FIELDS TERMINATED BY '|' " + vbLf
            Sentencia_Load &= "LINES TERMINATED BY '\n' " + vbLf
            Sentencia_Load &= "SET fecha=" + fechaString

            Using cn_estado_caja As New MySqlConnection(cn_server.StringConn & "Uid=" & cn_server.userID & ";Pwd=" & cn_server.password & ";")
                cn_estado_caja.Open()
                Using cmd_estado_caja As New MySqlCommand(Sentencia_Load, cn_estado_caja)
                    cmd_estado_caja.ExecuteNonQuery()
                End Using
            End Using
        Else
            Console.WriteLine("No se encuentra el archivo " + "C:\temp\" + archivo)

        End If


        'EjecutarSP(Sentencia_Load, cn_server, TipoProcesamiento.NonQuery, True, BaseDeDatos.DBMySql)


        If My.Computer.FileSystem.FileExists("C:\temp\" + archivo) Then
            My.Computer.FileSystem.DeleteFile("C:\temp\" + archivo)
        Else
            Console.WriteLine("No se eliminó archivo " + "C:\temp\" + archivo)
        End If






    End Sub


    Public Sub CierresDobles()
        Dim dt_resultados As New DataTable
        Sentencia_MySql = Sentencia_MySql & "SELECT * FROM cuadrador.tiendas  WHERE tienda NOT IN (2,96,97,98,119,137,996)"
        dt_cajas = CType(EjecutarSP(Sentencia_MySql, cn_server, TipoProcesamiento.DataTable, True, BaseDeDatos.DBMySql), DataTable)


        Sentencia_MySql = "SELECT DATE(fecha),COUNT(1) CONTEO FROM superpos.cierreventas_log "
        Sentencia_MySql &= "WHERE proceso LIKE 'Inicio cierre en:%' AND  fecha>='20210101' "
        Sentencia_MySql &= "GROUP BY DATE(fecha) "
        Sentencia_MySql &= "HAVING COUNT(1)>1 "


        Dim Flag As Integer
        For Each server In dt_cajas.Rows
            cn_caja.StringConn = "Server=" + server(2) + ";port=" + server(3) + ";Connect Timeout=10;"
            cn_caja.userID = server(4)
            cn_caja.password = des.DesEncriptar(server(5))
            cn_caja.Esquema = "superpos"

            'Console.WriteLine(cn_caja.StringConn, cn_caja.userID, cn_caja.password)



            Try
                dt_resultados = CType(EjecutarSP(Sentencia_MySql, cn_caja, TipoProcesamiento.DataTable, True, BaseDeDatos.DBMySql), DataTable)
            Catch ex As Exception
                Console.WriteLine(ex.Message)
            End Try

            Flag = 0
            For Each resultado In dt_resultados.Rows
                If Flag = 0 Then
                    Console.WriteLine(server(0).ToString() + "->" + server(1).ToString())
                    Flag += 1
                End If

                Console.WriteLine("       " + resultado(0).ToString() + "->" + resultado(1).ToString())
            Next


        Next





    End Sub



End Module
