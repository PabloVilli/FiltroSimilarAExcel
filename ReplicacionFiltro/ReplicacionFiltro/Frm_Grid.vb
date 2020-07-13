Imports System.Data.SqlClient
Imports System.Configuration

Public Class Frm_Grid


    ' Declaro e inicializamos nuestra conexion
    Private conexionDb As SqlConnection = New SqlConnection(ConfigurationManager.ConnectionStrings("myConnection").ConnectionString)

    ' Declaro un adaptador para llenar una tabla
    Private adaptadorSql As SqlDataAdapter

    ' Declaro una tabla para almacenar los datos 
    Private tabla As DataTable

    ' Declaro una variable para alojar el nombre de la columna que vamos a filtrar
    Private columnaFiltro As String

    ' Declaro un objeto del FrmFiltrar para hacer uso del filtro, sus metodos y sus funciones.
    Private objFiltro As Frm_Filtro = New Frm_Filtro()


    Private Sub lblRecuperar_Click(sender As Object, e As EventArgs) Handles lblRecuperar.Click
        'Inicializo la tabla
        InicializarTabla()
        ' Inicializo la tabla de referencia del filtro
        objFiltro.InicializarTablaReferencia(tabla)
        ' Desactivo el boton de recuperar datos
        lblRecuperar.Enabled = False
        ' Activo el boton de restablecer
        lblLimpiar.Enabled = True
    End Sub


    Public Sub InicializarTabla()

        ' Inicializo la tabla del grid
        tabla = New DataTable

        ' Inicializo el adaptador 
        adaptadorSql = New SqlDataAdapter

        ' Declaro un string parra guardar el SELECT que me devolvera la tabla para filtrar
        Dim comandoSelect As String = "SELECT * FROM tbl_DatosFiltro"

        ' Inicializo el SelectCommand del adaptador, requiere del select como string y de una conexion a una bd
        adaptadorSql.SelectCommand = New SqlCommand(comandoSelect, conexionDb)

        ' Llenams la tabla con el metodo Fill del adaptador
        adaptadorSql.Fill(tabla)

        ' Comprobamos que la tabla tenga valores
        If tabla.Rows.Count = 0 Then
            ' Si el numero de filas de la tabla es igual a cero, entonces envio un mensaje de error
            MessageBox.Show("Problema al recuperar la información", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        Else
            ' Si el numero de filas de la tabla no es igual a cero, entonces muestrto los datos enn el grid 
            dgvDatos.DataSource = tabla
        End If
    End Sub
    ' Este metodo limpia la tabla y devuelve los botones a su estado inicial
    Public Sub Eliminarvaloreslistatablas()
        lblRecuperar.Enabled = True
        lblLimpiar.Enabled = False
        lblLimpiar.Enabled = False
        tabla.Clear()
    End Sub
    Public Sub ProcesosFiltro(ByVal objetoFiltro As Frm_Filtro, ByVal dgv As DataGridView)
        objetoFiltro.LimpiarObjetosFiltro()
        objetoFiltro.InicializarTablaReferencia(dgv.DataSource)
        objetoFiltro.ObtenerFiltrosAplicadosEnArray(objetoFiltro.dtFiltrosAplicados, dgv)
        objetoFiltro.MarcarFiltroEnGrid(dgv)
    End Sub

    Private Sub lblLimpiar_Click(sender As Object, e As EventArgs) Handles lblLimpiar.Click
        Eliminarvaloreslistatablas()
        ProcesosFiltro(objFiltro, dgvDatos)
    End Sub

    Public Sub DobleClickEnHeaderGrid(ByVal dgv As DataGridView, ByVal e As DataGridViewCellMouseEventArgs, ByVal objFiltro As Frm_Filtro)
        If dgv.DataSource Is Nothing Then
            MsgBox("No hay registros en la tabla")
        Else
            Dim nuevaColumna As DataGridViewColumn = dgv.Columns(e.ColumnIndex)
            objFiltro.columnaActual = nuevaColumna.DataPropertyName
            objFiltro.ShowDialog()
            dgv.DataSource = objFiltro.FiltroExistente(dgv.DataSource, nuevaColumna.DataPropertyName)
            objFiltro.ObtenerFiltrosAplicadosEnArray(objFiltro.dtFiltrosAplicados, dgv)
            objFiltro.MarcarFiltroEnGrid(dgv)
        End If
    End Sub

    Private Sub dgvDatos_ColumnHeaderMouseDoubleClick(sender As Object, e As DataGridViewCellMouseEventArgs) Handles dgvDatos.ColumnHeaderMouseDoubleClick
        DobleClickEnHeaderGrid(dgvDatos, e, objFiltro)
    End Sub

    Private Sub lblRestablecer_Click(sender As Object, e As EventArgs) Handles lblRestablecer.Click
        Eliminarvaloreslistatablas()
        lblRecuperar_Click(sender, e)
        ProcesosFiltro(objFiltro, dgvDatos)
    End Sub

End Class
