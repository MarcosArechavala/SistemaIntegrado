using Newtonsoft.Json;

namespace SainnavvAPI.Models
{
    public class FamiliarExterno
    {
        [JsonProperty("idpaciente")] public int? IdPaciente { get; set; }
        [JsonProperty("apeyNom")] public string? ApeyNom { get; set; }
        [JsonProperty("documento")]
        public object? Documento { get; set; }
        [JsonProperty("parentesco")]
        public int Parentesco { get; set; } // 1=Padre, 2=Madre
        [JsonProperty("edad")] public string? Edad { get; set; }
        [JsonProperty("estadoCivil")] public int EstadoCivil { get; set; }
        [JsonProperty("escolaridad")] public int Escolaridad { get; set; }
        [JsonProperty("ocupacion")] public int Ocupacion { get; set; }
        [JsonProperty("ocupacionObs")] public string? OcupacionObs { get; set; }
        [JsonProperty("ingresos")] public decimal Ingresos { get; set; }
    }
}