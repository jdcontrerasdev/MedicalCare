namespace MedicalCare.Domain.Entities;

/// <summary>
/// Representa un servicio médico disponible en la IPS.
/// </summary>
public class ServicioMedico
{
    public int Id { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public string? Descripcion { get; set; }

    public bool Activo { get; set; }
}