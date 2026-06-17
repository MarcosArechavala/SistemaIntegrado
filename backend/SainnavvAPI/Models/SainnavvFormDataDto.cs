using System.Collections.Generic;

namespace SainnavvAPI.Models
{
    // DTO exclusivo para el Formulario Médico de SAINNAVV (9 Tablas)
    public class SainnavvFormDataDto
    {
        public int? idPaciente { get; set; }
        public int? IdEpisodio { get; set; }
        public int idProfesionalTurno { get; set; }

        // Profesionales
        public string? medicoInterviniente { get; set; }
        public string? trabajadorSocial { get; set; }
        public string? psicologo { get; set; }
        public string? medicoForense { get; set; }

        // Paciente
        public string? nombreApellido { get; set; }
        public string? dni { get; set; }
        public string? fechaNacimiento { get; set; }
        public string? edad { get; set; }
        public string? domicilio { get; set; }
        public string? telefonoCelular { get; set; }
        public string? telefonoFijo { get; set; }

        // Acompañante
        public List<string> acompananteTipo { get; set; } = new();
        public string? acompananteNombre { get; set; }
        public string? acompananteDni { get; set; }
        public string? acompananteEdad { get; set; }
        public string? acompananteCel { get; set; }
        public string? acompananteDomicilio { get; set; }
        public string? acompananteVinculo { get; set; }

        // Ingreso
        public string? modalidadIngreso { get; set; }
        public string? servicioInterconsulta { get; set; }
        public string? institucionDerivacion { get; set; }
        public string? presencialAdmisionObservaciones { get; set; }
        public string? presencialAbordajeObservaciones { get; set; }
        public string? motivosConsulta { get; set; }

        // Diagnósticos
        public List<string> diagnosticoPresuntivo { get; set; } = new();

        // Antecedentes
        public string? resumenIntervencionesPrevias { get; set; }
        public string? archivoAntecedentes { get; set; }
        public string? descripcionArchivo { get; set; }
        public string? archivoAtencionMedica { get; set; }
        public string? descripcionAtencionMedica { get; set; }
        public string? fechaAntecedente { get; set; }

        public string? entrevistaReferenteDetalle { get; set; }
        public string? atencionPsicologicaDetalle { get; set; }

        // Intervenciones Fechas
        public IntervencionesFechasDto intervenciones { get; set; } = new();
        // --- NUEVOS CAMPOS DE OBSERVACIONES GENERALES (FALTANTES) ---
        public string? observacionesIntervenciones { get; set; }
        public string? observacionesEstrategia { get; set; }

        // Médica
        public string? atencionMedicaExamen { get; set; }
        public bool genitalNormal { get; set; }
        public string? atencionMedicaOtro { get; set; }
        public List<string> fotografiaConsentimiento { get; set; } = new();
        public string? laboratorioEstado { get; set; }
        public string? hallazgosLaboratorio { get; set; }
        public string? tratamientoIndicado { get; set; }
        public string? observacionesSituacion { get; set; }

        // Articulaciones
        public string? coordinacionProteccion { get; set; }
        public string? dialogoInterdisciplinario { get; set; }

        // Estrategias
        public string? estrategiaTurnoCon { get; set; }
        public string? estrategiaFechaTurno { get; set; }
        public bool recomiendaTerapiaReferente { get; set; }
        public string? derivacionA { get; set; }
        public bool espacioGrupal { get; set; }
        public bool acompanamientoGesell { get; set; }
        public string? medidasImpedimento { get; set; }
        public string? detalleLesionesTexto { get; set; }

        // Obra Social
        public string? obraSocial { get; set; }
        public string? nroObraSocial { get; set; }

         // --- CAMPOS JUDICIALES  ---
        public List<string> destinoLegal { get; set; } = new();
        public string? fiscaliaNro { get; set; }
        public string? oficioNro { get; set; }
        public bool esJuzgadoDeTurno { get; set; }
        public string? nombreJuzgadoInterviniente { get; set; }

        // --- PROFESIONALES RESPONSABLES POR PASO (AGREGAR AL FINAL DE LA CLASE) ---
        public int profesionalPaso0 { get; set; }
        public int profesionalPaso1 { get; set; }
        public int profesionalPaso2 { get; set; }
        public int profesionalPaso3 { get; set; }
        public int profesionalPaso4 { get; set; }
        public int profesionalPaso5 { get; set; }
    }

    public class IntervencionesFechasDto
    {
        public string? admisionReferente { get; set; }
        public string? virtualInstitucional { get; set; }
        public string? referenteFamiliar { get; set; }
        public string? afectivo { get; set; }
        public string? horaJuego { get; set; }
        public string? entrevistaAdolescente { get; set; }
        public string? examenFisico { get; set; }
        public bool conForense { get; set; }
        public string? laboratorio { get; set; }
    }


}