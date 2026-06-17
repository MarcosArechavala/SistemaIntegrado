using System.Collections.Generic;

namespace SainnavvAPI.Models
{
    public class SocialFormDataDto
    {
        public int? IdPaciente { get; set; }

        // --- IDENTIFICACIÓN DEL PACIENTE ---
        public string? TipoDoc { get; set; }
        public string? Dni { get; set; }
        public string? HcNumber { get; set; }
        public string? Apellido { get; set; }
        public string? Nombres { get; set; }
        public string? Sexo { get; set; }
        public string? FechaNacimiento { get; set; }
        public string? Direccion { get; set; }
        public string? Localidad { get; set; }
        public string? Provincia { get; set; }
        public string? Departamento { get; set; }
        public string? Telefono { get; set; }
        public string? TelefonoFijo { get; set; }
        public string? Correo { get; set; }

        // --- DATOS FAMILIARES ---
        public string? MadreNombre { get; set; }
        public string? MadreDoc { get; set; }
        public string? PadreNombre { get; set; }
        public string? PadreDoc { get; set; }

        // --- DATOS FICHA ---
        public string? Nacionalidad { get; set; }
        public string? AreaProgramatica { get; set; }
        public string? Vivienda { get; set; }
        public int NumAmbientes { get; set; }
        public int PersonasHabitantes { get; set; }
        public string? LuzElectrica { get; set; }
        public string? Agua { get; set; }
        public string? Basura { get; set; }
        public string? PlanSocial { get; set; }
        public string? Observaciones { get; set; }
        public string? Alergias { get; set; }
        public string? NroObraSocial { get; set; }

        // --- NUEVOS CAMPOS DE OBRA SOCIAL (FALTANTES) ---
        public string? ObraSocial { get; set; } // Nombre en string
        public string? credential { get; set; } // Credencial en minúscula
        public string? titularNombre { get; set; } // Nombre titular en minúscula
        public string? titularDni { get; set; } // DNI titular

        // LISTA DE FAMILIARES
        public List<FamiliarDto>? Convivientes { get; set; }

        public bool GetBool(string? value) => value?.ToLower().Trim() == "si";
    }

    public class FamiliarDto
    {
        public int? IdFila { get; set; }
        public string? Nombre { get; set; }
        public string? Documento { get; set; }
        public string? Vinculo { get; set; }
        public string? Edad { get; set; }
        public string? Ocupacion { get; set; }
        public string? Observaciones { get; set; }
        public decimal Ingresos { get; set; }
    }
}