using MedicalCare.Application.DTOs;
using MedicalCare.Application.Interfaces;
using MedicalCare.Infrastructure.Database;
using Microsoft.Data.SqlClient;

namespace MedicalCare.Infrastructure.Repositories;

/// <summary>
/// Implementa el acceso a datos de los servicios médicos.
/// </summary>
public class ServicioMedicoRepository : IServicioMedicoRepository
{
    private readonly DatabaseConnectionFactory _connectionFactory;

    /// <summary>
    /// Inicializa el repositorio con las conexiones.
    /// </summary>
    public ServicioMedicoRepository(DatabaseConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    /// <summary>
    /// Obtiene los servicios médicos activos.
    /// </summary>
    public List<ServicioMedicoDto> ObtenerActivos()
    {
        var servicios = new List<ServicioMedicoDto>();

        using var connection = _connectionFactory.CreateConnection();

        using var command = new SqlCommand(
            "dbo.sp_Servicio_ListarActivos",
            connection);

        command.CommandType = System.Data.CommandType.StoredProcedure;

        connection.Open();

        using var reader = command.ExecuteReader();

        while (reader.Read())
        {
            servicios.Add(new ServicioMedicoDto
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                Nombre = reader.GetString(reader.GetOrdinal("Nombre")),
                Descripcion = reader.IsDBNull(
                    reader.GetOrdinal("Descripcion"))
                    ? null
                    : reader.GetString(
                        reader.GetOrdinal("Descripcion"))
            });
        }

        return servicios;
    }
}