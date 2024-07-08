Imports System.Net
Imports System.Web.Http
Imports System.Data.SqlClient
Imports System.Configuration
Imports Newtonsoft.Json.Linq

Public Class APISController
    Inherits ApiController

    Private ReadOnly _connectionString As String = ConfigurationManager.ConnectionStrings("DefaultConnection").ConnectionString

    <HttpPost>
    <Route("api/turnos/generar_turnos")>
    Public Function GenerarTurnos(<FromBody> parametros As JObject) As IHttpActionResult
        Try
            Dim fechaInicio As Date = parametros.GetValue("fechaInicio").ToObject(Of Date)
            Dim fechaFin As Date = parametros.GetValue("fechaFin").ToObject(Of Date)
            Dim idServicio As Integer = parametros.GetValue("idServicio").ToObject(Of Integer)

            Dim turnosGenerados As New List(Of Object)

            Using conn As New SqlConnection(_connectionString)
                conn.Open()

                Using cmd As New SqlCommand("GenerarTurnos", conn)
                    cmd.CommandType = CommandType.StoredProcedure
                    cmd.Parameters.AddWithValue("@FechaInicio", fechaInicio)
                    cmd.Parameters.AddWithValue("@FechaFin", fechaFin)
                    cmd.Parameters.AddWithValue("@IdServicio", idServicio)

                    Using reader As SqlDataReader = cmd.ExecuteReader()
                        While reader.Read()
                            Dim turno As New With {
                                .TurnoID = reader.GetInt32(0),
                                .Fecha = reader.GetDateTime(1),
                                .HoraInicio = reader.GetTimeSpan(2).ToString(),
                                .HoraFin = reader.GetTimeSpan(3).ToString()
                            }
                            turnosGenerados.Add(turno)
                        End While
                    End Using
                End Using
            End Using

            Return Ok(turnosGenerados)
        Catch ex As Exception
            Dim errorResponse As Object = New With {.error = ex.Message}
            Return Me.BadRequest(errorResponse)
        End Try
    End Function
End Class
