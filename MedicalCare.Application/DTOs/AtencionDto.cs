namespace MedicalCare.Application.DTOs;

public class AtencionDto
{
    public int AtencionId { get; set; }

    public string Documento { get; set; } = string.Empty;

    public string Paciente { get; set; } = string.Empty;

    public DateTime FechaAtencion { get; set; }

    public DateTime FechaRegistro { get; set; }

    public string Servicio { get; set; } = string.Empty;
}