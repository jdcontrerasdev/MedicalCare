using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MedicalCare.Application.Interfaces;
using MedicalCare.Application.Services;
using MedicalCare.Infrastructure.Database;
using MedicalCare.Infrastructure.Repositories;

namespace MedicalCare.UI;

internal static class Program
{

    /// <summary>
    /// Punto de entrada principal de la aplicación.
    /// </summary>
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();

        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile(
                "appsettings.json",
                optional: false,
                reloadOnChange: false)
            .Build();

        var connectionString = configuration
            .GetConnectionString("MedicalCare");

        var services = new ServiceCollection();

        services.AddSingleton(
            new DatabaseConnectionFactory(connectionString!));

        services.AddScoped<IPacienteRepository, PacienteRepository>();
        services.AddScoped<IServicioMedicoRepository, ServicioMedicoRepository>();
        services.AddScoped<IAtencionRepository, AtencionRepository>();

        services.AddScoped<AtencionService>();

        services.AddTransient<FrmAtenciones>();

        using var serviceProvider = services.BuildServiceProvider();

        var form = serviceProvider.GetRequiredService<FrmAtenciones>();

        System.Windows.Forms.Application.Run(form);
    }

}