namespace MedicalCare.Application.DTOs;

public class RegistrarAtencionRequest
{
    public int PacienteId { get; set; }

    public DateTime FechaAtencion { get; set; }

    public List<int> ServiciosIds { get; set; } = new();
}