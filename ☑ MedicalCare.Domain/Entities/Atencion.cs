namespace MedicalCare.Domain.Entities;

/// <summary>
/// Representa una atención médica realizada a un paciente.
/// </summary>
public class Atencion
{
    public int Id { get; set; }

    public int PacienteId { get; set; }

    public DateTime FechaAtencion { get; set; }

    public DateTime FechaRegistro { get; set; }

    public List<int> ServiciosIds { get; set; } = new();
}