using Microsoft.Data.SqlClient;

namespace MedicalCare.Infrastructure.Database;

/// <summary>
/// Centraliza la creación de conexiones a la base de datos.
/// </summary>
public class DatabaseConnectionFactory
{

    private readonly string _connectionString;

    /// <summary>
    /// Inicializa con la cadena de conexión.
    /// </summary>
    public DatabaseConnectionFactory(string connectionString)
    {
        _connectionString = connectionString;
    }

    /// <summary>
    /// Crea una conexión a SQL Server.
    /// </summary>
    public SqlConnection CreateConnection()
    {
        return new SqlConnection(_connectionString);
    }
}