using MedicalCare.Application.DTOs;

namespace MedicalCare.Application.Interfaces;

/// <summary>
/// Define las operaciones de acceso a pacientes.
/// </summary>
public interface IPacienteRepository
{

    List<PacienteDto> ObtenerActivos();

}