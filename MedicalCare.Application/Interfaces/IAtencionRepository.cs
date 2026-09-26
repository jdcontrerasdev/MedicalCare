using MedicalCare.Application.DTOs;

namespace MedicalCare.Application.Interfaces;

/// <summary>
/// Define las operaciones de persistencia relacionadas con las atenciones.
/// </summary>
public interface IAtencionRepository
{

    int Crear(RegistrarAtencionRequest request);

    List<AtencionDto> ObtenerTodas();

}