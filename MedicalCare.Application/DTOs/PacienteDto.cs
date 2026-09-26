namespace MedicalCare.Application.DTOs;

public class PacienteDto
{

    public int Id { get; set; }
    public string Documento { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public DateTime? FechaNacimiento { get; set; }

    public string NombreCompleto =>
        $"{Documento} - {Nombre} {Apellido}";

}