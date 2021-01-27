Public Class Contenedores
    Public Class CONEXIONSettings
        Public Sub New()
        End Sub
        Public Property StringConn() As String
            Get
                Return m_StringConn
            End Get
            Set(ByVal value As String)
                m_StringConn = value
            End Set
        End Property
        Public m_StringConn As String


        Public Property Esquema() As String
            Get
                Return m_Esquema
            End Get
            Set(ByVal value As String)
                m_Esquema = value
            End Set
        End Property
        Public m_Esquema As String



        Public Property userID() As String
            Get
                Return m_userID
            End Get
            Set(ByVal value As String)
                m_userID = value
            End Set
        End Property
        Public m_userID As String

        Public Property password() As String
            Get
                Return m_password
            End Get
            Set(ByVal value As String)
                m_password = value
            End Set
        End Property
        Public m_password As String

        Public Property Sentencia() As String
            Get
                Return m_Sentencia
            End Get
            Set(ByVal value As String)
                m_Sentencia = value
            End Set
        End Property
        Public m_Sentencia As String

    End Class



    Public Class ParametrosHilos
        Inherits CONEXIONSettings
        'Public Property Sentencia As String
        'Public Property Columna As Integer
        Public Property Tienda As Integer
        'Public Property Caja As Integer
        Public Property Tipo As String
        Public Property DTable As DataTable
    End Class



End Class
