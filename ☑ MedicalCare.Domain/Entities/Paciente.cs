namespace MedicalCare.Domain.Entities;

/// <summary>
/// Representa un paciente registrado en la IPS.
/// </summary>
public class Paciente
{
    public int Id { get; set; }

    public string Documento { get; set; } = string.Empty;

    public string Nombre { get; set; } = string.Empty;

    public string Apellido { get; set; } = string.Empty;

    public DateTime? FechaNacimiento { get; set; }

    public bool Activo { get; set; }
}