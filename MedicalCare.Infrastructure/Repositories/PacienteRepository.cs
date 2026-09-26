using MedicalCare.Application.DTOs;
using MedicalCare.Application.Interfaces;
using MedicalCare.Infrastructure.Database;
using Microsoft.Data.SqlClient;

namespace MedicalCare.Infrastructure.Repositories;

/// <summary>
/// Implementa el acceso a datos de los pacientes.
/// </summary>
public class PacienteRepository : IPacienteRepository
{
    private readonly DatabaseConnectionFactory _connectionFactory;

    /// <summary>
    /// Inicializa el repositorio con las conexiones.
    /// </summary>
    public PacienteRepository(DatabaseConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    /// <summary>
    /// Obtiene los pacientes activos.
    /// </summary>
    public List<PacienteDto> ObtenerActivos()
    {
        var pacientes = new List<PacienteDto>();

        using var connection = _connectionFactory.CreateConnection();

        using var command = new SqlCommand(
            "dbo.sp_Paciente_Listar",
            connection);

        command.CommandType = System.Data.CommandType.StoredProcedure;

        connection.Open();

        using var reader = command.ExecuteReader();

        while (reader.Read())
        {
            pacientes.Add(new PacienteDto
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                Documento = reader.GetString(reader.GetOrdinal("Documento")),
                Nombre = reader.GetString(reader.GetOrdinal("Nombre")),
                Apellido = reader.GetString(reader.GetOrdinal("Apellido")),
                FechaNacimiento = reader.IsDBNull(
                    reader.GetOrdinal("FechaNacimiento"))
                    ? null
                    : reader.GetDateTime(
                        reader.GetOrdinal("FechaNacimiento"))
            });
        }

        return pacientes;
    }
}