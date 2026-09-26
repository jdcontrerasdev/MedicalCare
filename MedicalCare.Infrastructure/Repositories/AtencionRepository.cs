using System.Data;
using MedicalCare.Application.DTOs;
using MedicalCare.Application.Interfaces;
using MedicalCare.Infrastructure.Database;
using Microsoft.Data.SqlClient;

namespace MedicalCare.Infrastructure.Repositories;

/// <summary>
/// Implementa el acceso a datos de las atenciones médicas.
/// </summary>
public class AtencionRepository : IAtencionRepository
{
    private readonly DatabaseConnectionFactory _connectionFactory;

    /// <summary>
    /// Inicializa el repositorio con las conexiones.
    /// </summary>
    public AtencionRepository(DatabaseConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    /// <summary>
    /// Registra una nueva atención médica.
    /// </summary>
    public int Crear(RegistrarAtencionRequest request)
    {
        using var connection = _connectionFactory.CreateConnection();

        using var command = new SqlCommand(
            "dbo.sp_Atencion_Crear",
            connection);

        command.CommandType = CommandType.StoredProcedure;

        command.Parameters.AddWithValue(
            "@PacienteId",
            request.PacienteId);

        command.Parameters.AddWithValue(
            "@FechaAtencion",
            request.FechaAtencion.Date);

        var serviciosTable = new DataTable();
        serviciosTable.Columns.Add("ServicioId", typeof(int));

        foreach (var servicioId in request.ServiciosIds)
        {
            serviciosTable.Rows.Add(servicioId);
        }

        var serviciosParameter = command.Parameters.Add(
            "@Servicios",
            SqlDbType.Structured);

        serviciosParameter.TypeName = "dbo.ServicioIdTable";
        serviciosParameter.Value = serviciosTable;

        connection.Open();

        var result = command.ExecuteScalar();

        return Convert.ToInt32(result);
    }

    /// <summary>
    /// Obtiene todas las atenciones registradas.
    /// </summary>
    public List<AtencionDto> ObtenerTodas()
    {
        var atenciones = new List<AtencionDto>();

        using var connection = _connectionFactory.CreateConnection();

        using var command = new SqlCommand(
            "dbo.sp_Atencion_Listar",
            connection);

        command.CommandType = CommandType.StoredProcedure;

        connection.Open();

        using var reader = command.ExecuteReader();

        while (reader.Read())
        {
            atenciones.Add(new AtencionDto
            {
                AtencionId = reader.GetInt32(
                    reader.GetOrdinal("AtencionId")),

                Documento = reader.GetString(
                    reader.GetOrdinal("Documento")),

                Paciente = reader.GetString(
                    reader.GetOrdinal("Paciente")),

                FechaAtencion = reader.GetDateTime(
                    reader.GetOrdinal("FechaAtencion")),

                FechaRegistro = reader.GetDateTime(
                    reader.GetOrdinal("FechaRegistro")),

                Servicio = reader.GetString(
                    reader.GetOrdinal("Servicio"))
            });
        }

        return atenciones;
    }
}