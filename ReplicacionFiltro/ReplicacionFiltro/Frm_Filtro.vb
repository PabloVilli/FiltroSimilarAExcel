Public Class Frm_Filtro

    Dim banderaFiltroModificado As Integer
    Dim totalFiltro As Integer
    Public columnaActual As String
    Public dtReferenciaFlt As DataTable = New DataTable
    Public listaFiltro As String
    Public arregloDatosColumna As List(Of String) = New List(Of String)
    Public dtFiltrada As DataTable = New DataTable
    Public dtFiltrosAplicados As DataTable = New DataTable
    Public arrayColumnasConFiltros As List(Of String) = New List(Of String) 'Filtro

    Private Sub Frm_Filtro_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        lblFiltro.Text = "Filtrar por " & columnaActual
        DeterminarTablaParaTreeView()
        banderaFiltroModificado = 0
    End Sub

    Public Sub InicializarTablaReferencia(ByVal tabla As DataTable)
        dtReferenciaFlt = tabla.Copy()
    End Sub

    Private Sub DeterminarTablaParaTreeView()
        If dtFiltrosAplicados.Rows.Count = 0 Then
            'No hay filtros
            ObtenerNodosParaTreeView(dtReferenciaFlt, True)
        ElseIf dtFiltrosAplicados.Rows.Count >= 1 Then
            'Hay un filtro
            Dim indexUltimoFiltro As Integer = dtFiltrosAplicados.Rows.Count - 1
            'MsgBox("Ultimo filtro aplicado: " & dtFiltrosAplicados.Rows(indexUltimoFiltro).Item("columna") & " index:" & indexUltimoFiltro)
            If columnaActual <> dtFiltrosAplicados.Rows(indexUltimoFiltro).Item("columna") Then
                'columnas distintas
                'obtengo los nodos con relacion a la tabla filtrada
                ObtenerNodosParaTreeView(dtFiltrada, True)
            ElseIf columnaActual = dtFiltrosAplicados.Rows(indexUltimoFiltro).Item("columna") And indexUltimoFiltro = 0 Then
                'columnas iguales y ademas es el filtro raiz
                'Obtengo un arreglo antes de que cambie dtFiltrada
                Dim columnaFiltrada = From row In dtFiltrada.AsEnumerable()
                                      Select row.Field(Of Object)(columnaActual) Distinct
                'Obtengo los nodos para el filtro raiz 
                ObtenerNodosParaTreeView(dtReferenciaFlt, False)
                'Activo el check de los nodos que corresponden a la lista de datos que se obtuvo del grid
                MarcarCheckDeNodosDeLaListaDelGrid(columnaFiltrada)
                'Aplico el filtro padre, el que es anterior al filtro de la columna que acaba de entrar
                dtFiltrada = AplicarFiltro(dtReferenciaFlt, dtFiltrosAplicados.Rows(dtFiltrosAplicados.Rows.Count - 1).Item("filtro"))
                dtFiltrada.AcceptChanges()
            ElseIf columnaActual = dtFiltrosAplicados.Rows(indexUltimoFiltro).Item("columna") And indexUltimoFiltro >= 1 Then
                'columnas iguales, pero no es el filtro raiz
                'Obtengo un arreglo antes de que cambie dtFiltrada
                Dim columnaFiltrada = From row In dtFiltrada.AsEnumerable()
                                      Select row.Field(Of Object)(columnaActual) Distinct
                'Aplico el filtro padre, el que es anterior al filtro de la columna que acaba de entrar
                dtFiltrada = AplicarFiltro(dtReferenciaFlt, dtFiltrosAplicados.Rows(dtFiltrosAplicados.Rows.Count - 2).Item("filtro"))
                dtFiltrada.AcceptChanges()
                'lleno treeView con nodos en falso ya con el filtro padre aplicado
                ObtenerNodosParaTreeView(dtFiltrada, False)
                'Activo el check de los nodos que corresponden a la lista de datos que se obtuvo del grid
                MarcarCheckDeNodosDeLaListaDelGrid(columnaFiltrada)
            End If
        End If
    End Sub

    Private Sub ObtenerNodosParaTreeView(ByVal tabla As DataTable, ByVal checkBox As Boolean)
        If tabla.Columns.Contains(columnaActual) Then
            Dim columnaFiltrada = From row In tabla.AsEnumerable()
                                  Order By row.Field(Of Object)(columnaActual)
                                  Select row.Field(Of Object)(columnaActual) Distinct
            totalFiltro = columnaFiltrada.Count
            tvFiltro.BeginUpdate()
            tvFiltro.Nodes.Clear()
            tvFiltro.CheckBoxes = True
            tvFiltro.Nodes.Add("Seleccionar Todo").Checked = checkBox
            For Each valorUnico In columnaFiltrada
                tvFiltro.Nodes.Add(valorUnico, valorUnico).Checked = checkBox
            Next
            tvFiltro.EndUpdate()
        End If
    End Sub

    Public Sub MarcarCheckDeNodosDeLaListaDelGrid(ByVal columnaArrayGrid As IEnumerable)
        Dim nodes As TreeNodeCollection = tvFiltro.Nodes
        For Each nodo As TreeNode In nodes
            For Each li In columnaArrayGrid
                If nodo.Text = "Seleccionar Todo" Then
                ElseIf nodo.Text = li Then
                    nodo.Checked = True
                End If
            Next
        Next
    End Sub

    Public Sub InicializarTablaFiltros()
        If Not dtFiltrosAplicados.Columns.Contains("columna") Then
            dtFiltrosAplicados.Columns.Add("columna", Type.GetType("System.String"))
        End If
        If Not dtFiltrosAplicados.Columns.Contains("filtro") Then
            dtFiltrosAplicados.Columns.Add("filtro", Type.GetType("System.String"))
        End If
    End Sub

    Public Sub AgregarFiltro(ByVal columna As String, ByVal filtro As String)
        Dim row As DataRow
        row = dtFiltrosAplicados.NewRow()
        row("columna") = columna
        row("filtro") = filtro
        dtFiltrosAplicados.Rows.Add(row)
    End Sub

    Public Function AplicarFiltro(ByVal tbl As DataTable, Optional filtro As String = "", Optional ordenamiento As String = "")
        Dim columnaFiltro() As DataRow = tbl.Select(filtro, ordenamiento)
        dtFiltrada = tbl.Clone
        If columnaFiltro.Count > 0 Then
            dtFiltrada = columnaFiltro.CopyToDataTable()
        End If
        Return dtFiltrada
    End Function

    Public Function FiltroExistente(ByVal tbl As DataTable, ByVal columnaForm As String) As DataTable
        'Determino si hay algun filtro en la tabla dtFiltrosAplicados
        'no hay filtro
        If dtFiltrosAplicados.Rows.Count = 0 Then
            'obtengo todo el universo y lo mando al DataGrid
            tbl = dtReferenciaFlt
            tbl.AcceptChanges()
            'si hay filtro
        ElseIf dtFiltrosAplicados.Rows.Count >= 1 Then
            'si dtFiltrada tiene registros
            If dtFiltrada.Rows.Count > 0 Then
                'obtengo el index del ultimo filtro aplicado
                Dim ultimoIndexDelFiltro As Integer = dtFiltrosAplicados.Rows.Count - 1
                'si es la misma columna y es filtro raiz")
                If dtFiltrosAplicados.Rows(ultimoIndexDelFiltro).Item("columna") = columnaForm And dtFiltrosAplicados.Rows.Count = 1 Then
                    'entonces aplico el filtro anterior con dtReferencia porque si le mando dtFiltrada no cambiara al no contener todo el universo
                    dtFiltrada = AplicarFiltro(dtReferenciaFlt, dtFiltrosAplicados.Rows(dtFiltrosAplicados.Rows.Count - 1).Item("filtro"))
                    dtFiltrada.AcceptChanges()
                End If
                'aplico el filtro en base a la tabla del DataGrid ya que no es filtro raiz 
                tbl = AplicarFiltro(dtFiltrada, dtFiltrosAplicados.Rows(dtFiltrosAplicados.Rows.Count - 1).Item("filtro"))
                tbl.AcceptChanges()
            Else
                'si no tiene entonces mando dtReferencia que tiene todo el universo y aplico el filtro en base a la tabla del DataGrid
                tbl = AplicarFiltro(dtReferenciaFlt, dtFiltrosAplicados.Rows(dtFiltrosAplicados.Rows.Count - 1).Item("filtro"))
                tbl.AcceptChanges()
            End If
        End If
        Return tbl
    End Function

    Public Sub LimpiarObjetosFiltro()
        dtReferenciaFlt.Clear()
        dtFiltrosAplicados.Clear()
        dtFiltrada.Clear()
        arregloDatosColumna.Clear()
        arrayColumnasConFiltros.Clear()
        listaFiltro = ""
    End Sub
    Public Sub ObtenerFiltrosAplicadosEnArray(ByVal tablaFiltros As DataTable, ByVal grid As DataGridView)
        If arrayColumnasConFiltros.Count >= 0 Then
            arrayColumnasConFiltros.Clear()
            DeterminarColumnasConFiltro(grid, "", 0)
        End If
        For Each row As DataRow In tablaFiltros.Rows()
            arrayColumnasConFiltros.Add(row("columna"))
        Next
    End Sub
    Public Sub DeterminarColumnasConFiltro(ByVal grid As DataGridView, ByVal nombreColumna As String, ByVal indexFiltro As Integer)
        For Each col As DataGridViewColumn In grid.Columns()
            If grid.Columns(col.Name).Name = nombreColumna Then
                grid.Columns(nombreColumna).DefaultCellStyle.BackColor = Color.LightSkyBlue
                grid.Columns(nombreColumna).HeaderText = nombreColumna & "  *" & indexFiltro
            Else
                If grid.Columns(col.Name).Name = "sel" Then
                Else
                    grid.Columns(col.Name).DefaultCellStyle.BackColor = Color.White
                    grid.Columns(col.Name).HeaderText = col.Name
                End If
            End If
        Next
    End Sub
    Public Sub MarcarFiltroEnGrid(ByVal grid As DataGridView)
        If arrayColumnasConFiltros.Count > 0 Then
            For i = 0 To arrayColumnasConFiltros.Count() - 1
                DeterminarColumnasConFiltro(grid, arrayColumnasConFiltros(i), i)
            Next
        End If
    End Sub
    Private Sub CambiarValoresChecksTreeView(ByVal booleano As Boolean)
        For i As Integer = 0 To tvFiltro.Nodes.Count - 1
            If i = 0 Then
            Else
                tvFiltro.Nodes(i).Checked = booleano
            End If
        Next
    End Sub

    Private Sub tvFiltro_AfterCheck(sender As Object, e As TreeViewEventArgs) Handles tvFiltro.AfterCheck
        If e.Node.Text = "Seleccionar Todo" Then
            If e.Node.Checked = True Then
                CambiarValoresChecksTreeView(e.Node.Checked)
            End If
            If e.Node.Checked = False Then
                CambiarValoresChecksTreeView(e.Node.Checked)
            End If
        Else
            If e.Node.Checked = True Then
                If Not arregloDatosColumna.Contains(e.Node.Text) Then
                    arregloDatosColumna.Add(e.Node.Text)
                End If
            End If
            If e.Node.Checked = False Then
                arregloDatosColumna.Remove(e.Node.Text)
            End If
        End If
    End Sub

    Public Sub DeterminarFiltroParaSalir()
        listaFiltro = ""
        Select Case arregloDatosColumna.Count
            Case 1 To totalFiltro - 1
                InicializarTablaFiltros()
                For i As Integer = 0 To arregloDatosColumna.Count - 1
                    listaFiltro = listaFiltro & "'" & arregloDatosColumna(i) & "', "
                Next
                listaFiltro = listaFiltro.Remove(listaFiltro.Length - 2)
                Dim expresion As String = "[" & columnaActual & "]" & " IN (" & listaFiltro & ")"
                If Not dtFiltrosAplicados.Rows.Count > 0 Then
                    AgregarFiltro(columnaActual, expresion)
                Else
                    Dim ultimoIndexDelFiltro As Integer = dtFiltrosAplicados.Rows.Count - 1
                    For i As Integer = 0 To dtFiltrosAplicados.Rows.Count - 1
                        If dtFiltrosAplicados.Rows(i).Item("columna") = columnaActual And i = ultimoIndexDelFiltro Then
                            dtFiltrosAplicados.Rows(i)("filtro") = expresion
                            banderaFiltroModificado = 1
                        End If
                    Next
                    If banderaFiltroModificado = 0 Then
                        AgregarFiltro(columnaActual, expresion)
                    End If
                End If
            Case Is = totalFiltro
                'No se debe aplicar filtro
                If dtFiltrosAplicados.Rows.Count > 0 Then
                    Dim indexUltimoFiltro As Integer = dtFiltrosAplicados.Rows.Count - 1
                    If columnaActual = dtFiltrosAplicados.Rows(indexUltimoFiltro).Item("columna") Then
                        'Se elimina el filtro
                        dtFiltrosAplicados.Rows.Remove(dtFiltrosAplicados.Rows(indexUltimoFiltro))
                    End If
                End If
        End Select
        arregloDatosColumna.Clear()
        Me.Close()
    End Sub

    Private Sub btnAceptar_Click(sender As Object, e As EventArgs) Handles btnAceptar.Click
        DeterminarFiltroParaSalir()
    End Sub

    Private Sub btnCancelar_Click(sender As Object, e As EventArgs) Handles btnCancelar.Click
        arregloDatosColumna.Clear()
        DeterminarFiltroParaSalir()
    End Sub
End Class