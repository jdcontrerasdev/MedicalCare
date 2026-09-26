using MedicalCare.Application.DTOs;

namespace MedicalCare.Application.Interfaces;

/// <summary>
/// Define las operaciones de acceso a servicios médicos.
/// </summary>
public interface IServicioMedicoRepository
{

    List<ServicioMedicoDto> ObtenerActivos();

}