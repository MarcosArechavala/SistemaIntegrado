using Newtonsoft.Json;

namespace SainnavvAPI.Models
{
    public class FichaPacienteExterno
    {
        [JsonProperty("direccion")]
        public string? Direccion { get; set; }

        // Datos Padre
        [JsonProperty("edad_padre")]
        public string? EdadPadre { get; set; }

        [JsonProperty("ocupacion_padre")]
        public string? OcupacionPadre { get; set; }

        [JsonProperty("escolaridad_padre")]
        public string? EscolaridadPadre { get; set; }

        // Datos Madre
        [JsonProperty("edad_madre")]
        public string? EdadMadre { get; set; }

        [JsonProperty("ocupacion_madre")]
        public string? OcupacionMadre { get; set; }

        [JsonProperty("escolaridad_madre")]
        public string? EscolaridadMadre { get; set; }
    }
}