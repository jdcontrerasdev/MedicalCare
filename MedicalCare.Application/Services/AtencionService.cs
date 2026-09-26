using MedicalCare.Application.DTOs;
using MedicalCare.Application.Interfaces;

namespace MedicalCare.Application.Services;

public class AtencionService
{

    private readonly IAtencionRepository _atencionRepository;

    public AtencionService(IAtencionRepository atencionRepository)
    {
        _atencionRepository = atencionRepository;
    }

    /// <summary>
    /// Registra una nueva atención.
    /// Las validaciones definitivas de integridad y reglas de negocio
    /// también son reforzadas en la base de datos.
    /// </summary>
    public int RegistrarAtencion(RegistrarAtencionRequest request)
    {
        if (request.PacienteId <= 0)
            throw new ArgumentException("Debe seleccionar un paciente.");

        if (request.ServiciosIds == null || request.ServiciosIds.Count == 0)
            throw new ArgumentException(
                "Debe seleccionar al menos un servicio.");

        return _atencionRepository.Crear(request);
    }

    public List<AtencionDto> ObtenerAtenciones()
    {
        return _atencionRepository.ObtenerTodas();
    }

}