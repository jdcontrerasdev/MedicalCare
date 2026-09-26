
# MedicalCare

Aplicación de escritorio desarrollada para la gestión de atenciones médicas en una IPS.

## 1. Arquitectura

La solución implementa una arquitectura por capas, separando responsabilidades entre la interfaz de usuario, la lógica de aplicación, el dominio y el acceso a datos.

```text
┌──────────────────────────────┐
│       MedicalCare.UI         │
│          WinForms            │
│                              │
│  Interfaz y eventos de UI    │
└──────────────┬───────────────┘
               │
               ▼
┌──────────────────────────────┐
│    MedicalCare.Application   │
│                              │
│  Services / DTOs / Interfaces│
│                              │
│     Lógica de aplicación     │
└──────────────┬───────────────┘
               │
               ▼
┌──────────────────────────────┐
│  MedicalCare.Infrastructure  │
│                              │
│     Repositories / ADO.NET   │
│                              │
│       Acceso a datos         │
└──────────────┬───────────────┘
               │
               ▼
┌──────────────────────────────┐
│          SQL Server          │
│                              │
│    Stored Procedures / TVP   │
└──────────────────────────────┘
```

### Capas de la solución

**MedicalCare.UI**

Contiene la interfaz gráfica desarrollada con Windows Forms. Se encarga de la interacción con el usuario y de presentar la información.

**MedicalCare.Application**

Contiene los DTOs, interfaces de repositorios y servicios de aplicación. Esta capa coordina las operaciones y mantiene desacoplada la lógica de aplicación del acceso concreto a datos.

**MedicalCare.Domain**

Contiene las entidades principales del dominio:

- `Paciente`
- `ServicioMedico`
- `Atencion`

**MedicalCare.Infrastructure**

Contiene las implementaciones de los repositorios y el acceso a SQL Server mediante ADO.NET.

---

## 2. Patrón de diseño utilizado

### Repository Pattern

Se implementó el patrón **Repository** para desacoplar la lógica de aplicación del mecanismo de acceso a datos.

Las interfaces se encuentran en `MedicalCare.Application`:

```text
IPacienteRepository
IServicioMedicoRepository
IAtencionRepository
```

Sus implementaciones se encuentran en `MedicalCare.Infrastructure`:

```text
PacienteRepository
ServicioMedicoRepository
AtencionRepository
```

La dependencia se establece mediante interfaces, permitiendo que la capa de aplicación no dependa directamente de SQL Server ni de ADO.NET.

El flujo utilizado es:

```text
Application
     │
     ▼
Repository Interface
     │
     ▼
Infrastructure Repository
     │
     ▼
ADO.NET
     │
     ▼
Stored Procedure
     │
     ▼
SQL Server
```

Este patrón permite separar responsabilidades y facilita el mantenimiento y eventual reemplazo de la implementación de acceso a datos.

---

## 3. Acceso a datos

El acceso a datos se implementó utilizando **ADO.NET** mediante `Microsoft.Data.SqlClient`.

No se utiliza Entity Framework para esta solución, ya que el requerimiento solicita el uso de Stored Procedures.

Los repositorios ejecutan los siguientes procedimientos almacenados:

```text
sp_Paciente_Listar
sp_Servicio_ListarActivos
sp_Atencion_Listar
sp_Atencion_Crear
```

### Registro de una atención

Para registrar una atención se utiliza:

```text
sp_Atencion_Crear
```

El procedimiento recibe:

- Identificador del paciente.
- Fecha de atención.
- Lista de servicios seleccionados.

Para enviar múltiples servicios desde la aplicación hacia SQL Server se utiliza un **Table-Valued Parameter (TVP)**:

```text
dbo.ServicioIdTable
```

El flujo de registro es:

```text
FrmAtenciones
     │
     ▼
AtencionService
     │
     ▼
IAtencionRepository
     │
     ▼
AtencionRepository
     │
     ▼
ADO.NET / SqlCommand
     │
     ▼
sp_Atencion_Crear
     │
     ▼
SQL Server
```

El procedimiento almacenado maneja la transacción y valida las reglas relacionadas con la persistencia, incluyendo la restricción de no registrar el mismo servicio para el mismo paciente en la misma fecha.

### Conexión a SQL Server

La conexión se configura mediante `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "MedicalCare": "Server=localhost;Database=MedicalCare;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

La conexión es encapsulada mediante `DatabaseConnectionFactory`, evitando que los repositorios tengan que construir directamente la cadena de conexión.

---

## 4. Estructura de proyectos

```text
MedicalCare
│
├── Database
│   └── MedicalCare.sql
│
├── MedicalCare.Domain
│   └── Entities
│
├── MedicalCare.Application
│   ├── DTOs
│   ├── Interfaces
│   └── Services
│
├── MedicalCare.Infrastructure
│   ├── Database
│   └── Repositories
│
└── MedicalCare.UI
    ├── appsettings.json
    ├── Program.cs
    └── FrmAtenciones.cs
```