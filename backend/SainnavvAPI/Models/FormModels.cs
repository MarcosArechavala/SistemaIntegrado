using Microsoft.AspNetCore.Http;

namespace SainnavvAPI.Models
{
    public class HistoriaClinicaRequest
    {
        // El ? permite que sea nulo si angular no manda el dato o los archivos
        public string? JsonData { get; set; }
        public IFormFile? ArchivoAntecedentes { get; set; }
        public IFormFile? ArchivoClinica { get; set; }
    }

    public class DatosClaveFormulario
    {
        // Ponemos ? a todos los strings para que C# no se queje si vienen vacíos
        public string? MedicoInterviniente { get; set; }
        public string? TrabajadorSocial { get; set; }
        public string? Psicologo { get; set; }
        public string? NombreApellido { get; set; }
        public string? Dni { get; set; }
        public string? FechaNacimiento { get; set; }
        public string? Edad { get; set; }
        public string? DescripcionArchivo { get; set; }
        public string? DescripcionAtencionMedica { get; set; }
    }
}