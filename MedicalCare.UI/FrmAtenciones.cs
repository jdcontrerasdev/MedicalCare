using MedicalCare.Application.DTOs;
using MedicalCare.Application.Interfaces;
using MedicalCare.Application.Services;

namespace MedicalCare.UI;
public class FrmAtenciones : Form
{
    private readonly AtencionService _atencionService;
    private readonly IPacienteRepository _pacienteRepository;
    private readonly IServicioMedicoRepository _servicioRepository;

    private ComboBox cmbPaciente = null!;
    private DateTimePicker dtpFechaAtencion = null!;
    private CheckedListBox clbServicios = null!;
    private Button btnRegistrar = null!;
    private DataGridView dgvAtenciones = null!;

    public FrmAtenciones(
        AtencionService atencionService,
        IPacienteRepository pacienteRepository,
        IServicioMedicoRepository servicioRepository)
    {
        _atencionService = atencionService;
        _pacienteRepository = pacienteRepository;
        _servicioRepository = servicioRepository;

        ConfigurarFormulario();
        CargarDatos();
    }

    private void ConfigurarFormulario()
    {
        Text = "Gestión de Atenciones Médicas";
        StartPosition = FormStartPosition.CenterScreen;
        Size = new Size(900, 680);

        var lblTitulo = new Label
        {
            Text = "GESTIÓN DE ATENCIONES MÉDICAS",
            Location = new Point(30, 10),
            AutoSize = true,
            Font = new Font("Segoe UI", 16, FontStyle.Bold)
        };

        var lblPaciente = new Label
        {
            Text = "Paciente:",
            Location = new Point(30, 55),
            AutoSize = true
        };

        cmbPaciente = new ComboBox
        {
            Name = "cmbPaciente",
            Location = new Point(150, 50),
            Width = 400,
            DropDownStyle = ComboBoxStyle.DropDownList
        };

        var lblFecha = new Label
        {
            Text = "Fecha atención:",
            Location = new Point(30, 100),
            AutoSize = true
        };

        dtpFechaAtencion = new DateTimePicker
        {
            Name = "dtpFechaAtencion",
            Location = new Point(150, 95),
            Width = 200,
            Format = DateTimePickerFormat.Short,
            Value = DateTime.Today
        };

        var lblServicios = new Label
        {
            Text = "Servicios médicos:",
            Location = new Point(30, 145),
            AutoSize = true
        };

        clbServicios = new CheckedListBox
        {
            Name = "clbServicios",
            Location = new Point(150, 140),
            Width = 400,
            Height = 120
        };

        btnRegistrar = new Button
        {
            Name = "btnRegistrar",
            Text = "Registrar Atención",
            Location = new Point(150, 280),
            Width = 180,
            Height = 38,
            Font = new Font("Segoe UI", 9, FontStyle.Bold)
        };

        btnRegistrar.Click += BtnRegistrar_Click;

        var lblAtenciones = new Label
        {
            Text = "Atenciones registradas",
            Location = new Point(30, 345),
            AutoSize = true,
            Font = new Font("Segoe UI", 10, FontStyle.Bold)
        };

        dgvAtenciones = new DataGridView
        {
            Name = "dgvAtenciones",
            Location = new Point(30, 375),
            Width = 820,
            Height = 220,
            ReadOnly = true,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = false
        };

        Controls.AddRange(
        [
            lblTitulo,
        lblPaciente,
        cmbPaciente,
        lblFecha,
        dtpFechaAtencion,
        lblServicios,
        clbServicios,
        btnRegistrar,
        lblAtenciones,
        dgvAtenciones
        ]);
    }

    private void CargarDatos()
    {
        try
        {
            var pacientes = _pacienteRepository.ObtenerActivos();

            cmbPaciente.DataSource = pacientes;
            cmbPaciente.DisplayMember = "NombreCompleto";
            cmbPaciente.ValueMember = "Id";

            var servicios = _servicioRepository.ObtenerActivos();

            clbServicios.Items.Clear();

            foreach (var servicio in servicios)
            {
                clbServicios.Items.Add(
                    new ServicioItem(
                        servicio.Id,
                        servicio.Nombre));
            }

            CargarAtenciones();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"No fue posible cargar la información inicial.\n\n{ex.Message}",
                "Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    private void CargarAtenciones()
    {
        dgvAtenciones.DataSource = _atencionService.ObtenerAtenciones();

        if (dgvAtenciones.Columns.Count == 0)
            return;

        dgvAtenciones.Columns["AtencionId"].Visible = false;

        dgvAtenciones.Columns["Documento"].HeaderText = "Documento";
        dgvAtenciones.Columns["Paciente"].HeaderText = "Paciente";
        dgvAtenciones.Columns["FechaAtencion"].HeaderText = "Fecha de atención";
        dgvAtenciones.Columns["FechaRegistro"].HeaderText = "Fecha de registro";
        dgvAtenciones.Columns["Servicio"].HeaderText = "Servicio";

        dgvAtenciones.Columns["FechaAtencion"].DefaultCellStyle.Format = "dd/MM/yyyy";
        dgvAtenciones.Columns["FechaRegistro"].DefaultCellStyle.Format = "dd/MM/yyyy HH:mm";
    }

    private void BtnRegistrar_Click(object? sender, EventArgs e)
    {
        try
        {
            if (cmbPaciente.SelectedItem is not PacienteDto paciente)
            {
                MessageBox.Show(
                    "Debe seleccionar un paciente.",
                    "Validación",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            var serviciosSeleccionados = clbServicios.CheckedItems
                .Cast<ServicioItem>()
                .Select(x => x.Id)
                .ToList();

            if (serviciosSeleccionados.Count == 0)
            {
                MessageBox.Show(
                    "Debe seleccionar al menos un servicio.",
                    "Validación",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            var request = new RegistrarAtencionRequest
            {
                PacienteId = paciente.Id,
                FechaAtencion = dtpFechaAtencion.Value.Date,
                ServiciosIds = serviciosSeleccionados
            };

            var atencionId = _atencionService.RegistrarAtencion(request);

            MessageBox.Show(
                $"La atención fue registrada correctamente.\n\nNúmero de atención: {atencionId}",
                "Registro exitoso",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);

            LimpiarFormulario();
            CargarAtenciones();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                ex.Message,
                "Atención no registrada",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
        }
    }

    private void LimpiarFormulario()
    {
        cmbPaciente.SelectedIndex = -1;

        for (var i = 0; i < clbServicios.Items.Count; i++)
        {
            clbServicios.SetItemChecked(i, false);
        }

        dtpFechaAtencion.Value = DateTime.Today;
    }

    private class ServicioItem
    {
        public int Id { get; }
        public string Nombre { get; }

        public ServicioItem(int id, string nombre)
        {
            Id = id;
            Nombre = nombre;
        }

        public override string ToString() => Nombre;
    }
}