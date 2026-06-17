using Newtonsoft.Json;

namespace SainnavvAPI.Models
{
    public class FichaSocialExterna
    {
        // --- IDs ---
        [JsonProperty("iDpaciente")] public string? IDPaciente { get; set; }
        [JsonProperty("numhistclinica")] public string? NumHistClinica { get; set; }

        // --- UBICACIÓN QUE FALTABA (CS1061 Error) ---
        // A veces la API manda 'direccion', a veces solo viviendaTipo. Lo agregamos para que compile.
        [JsonProperty("direccion")] public string? Direccion { get; set; }

        [JsonProperty("nombremadre")] public string? NombreMadre { get; set; }
        [JsonProperty("nombrepadre")] public string? NombrePadre { get; set; }
        [JsonProperty("edad_madre")] public string? EdadMadre { get; set; }
        [JsonProperty("edad_padre")] public string? EdadPadre { get; set; }

        // --- DATOS PADRES QUE FALTABAN (CS1061 Error) ---
        [JsonProperty("ocupacion_padre")] public string? OcupacionPadre { get; set; }
        [JsonProperty("ocupacion_madre")] public string? OcupacionMadre { get; set; }

        [JsonProperty("doc_madre")] public string? DocMadre { get; set; }
        [JsonProperty("doc_padre")] public string? DocPadre { get; set; }
        [JsonProperty("docmadre")] public string? DocMadreAlt { get; set; }
        [JsonProperty("docpadre")] public string? DocPadreAlt { get; set; }

        // --- RESTO DE PROPIEDADES ---
        [JsonProperty("apellido")] public string? Apellido { get; set; }
        [JsonProperty("nombre")] public string? Nombre { get; set; }
        [JsonProperty("numerodocumento")] public string? Documento { get; set; }
        [JsonProperty("escolaridad_madre")] public string? EscolaridadMadre { get; set; }
        [JsonProperty("escolaridad_padre")] public string? EscolaridadPadre { get; set; }
        [JsonProperty("telCel")] public string? Celular { get; set; }
        [JsonProperty("telFijo")] public string? TelefonoFijo { get; set; }
        [JsonProperty("correo")] public string? Correo { get; set; }

        [JsonProperty("areaProg")] public string? AreaProg { get; set; }
        [JsonProperty("viviendaTipo")] public string? ViviendaTipo { get; set; }
        [JsonProperty("nroAmbientes")] public string? NroAmbientes { get; set; }
        [JsonProperty("persHabitan")] public string? PersHabitan { get; set; }

        [JsonProperty("luzElectrica")] public string? LuzElectrica { get; set; }
        [JsonProperty("agua")] public string? Agua { get; set; }
        [JsonProperty("basura")] public string? Basura { get; set; }
        [JsonProperty("ayudaSocial")] public string? AyudaSocial { get; set; }

        [JsonProperty("alergias")] public string? Alergias { get; set; }
        [JsonProperty("observaciones")] public string? Observaciones { get; set; }
    }
}