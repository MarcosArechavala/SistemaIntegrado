using Newtonsoft.Json;

namespace SainnavvAPI.Models
{
    public class PacienteExterno
    {
        [JsonProperty("numhistclinica")]
        public string? Numhistclinica { get; set; }

        [JsonProperty("numerodocumento")]
        public string? Numerodocumento { get; set; }

        [JsonProperty("sexo")]
        public string? Sexo { get; set; }

        [JsonProperty("fechanac")]
        public string? Fechanac { get; set; }

        [JsonProperty("apellido")]
        public string? Apellido { get; set; }

        [JsonProperty("nombre")]
        public string? Nombre { get; set; }

        [JsonProperty("nombrepadre")]
        public string? Nombrepadre { get; set; }

        [JsonProperty("nombremadre")]
        public string? Nombremadre { get; set; }

        [JsonProperty("docmadre")]
        public string? Docmadre { get; set; }

        [JsonProperty("docpadre")]
        public string? Docpadre { get; set; }

        [JsonProperty("idpaciente")]
        public string? Idpaciente { get; set; }
    }
}