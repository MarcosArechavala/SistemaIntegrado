export interface FamilyMember {
  nombre: string;
  vinculo: string;
  edad: string;
  ocupacion: string;
  observaciones: string;
}

export interface ParentData {
  documento: any;
  nombre: string;
  edad: string;
  dni: string;
  nivelInstruccion: string;
  ocupacion: string;
  domicilio: string;
  cel: string;
}

export interface FormState {
   idHistoria?: number;
   idPaciente?: number;  // ID de la tabla Pacientes de SQL
   idEpisodio?: number;  // ID de la tabla ADMISION (Si es > 0, .NET hará un UPDATE en vez de INSERT)
   idUsuario?: number;
   idProfesionalTurno: 0,
  // Profesionales 
  equipo: {
    medico: number;
    trabajadorSocial: number;
    psicologo: number;
    enfermero: number;
  };
  medicoForense: string;
  // Datos Paciente
  nombreApellido: string;
  edad: string;
  fechaNacimiento: string;
  dni: string;
  domicilio: string;
  centroSalud: string;
  ultimoControl: string;
  obraSocial: boolean;
  cualObraSocial: string;
  telefonoCelular: string;
  telefonoFijo: string;
  escolaridad: string;

  // Grupo Familiar
  madre: ParentData;
  padre: ParentData;
  otrosConvivientes: FamilyMember[];

  // Acompañante (Variables sin 'ñ' 2)
  acompananteTipo: string[]; 
  acompananteNombre: string;
  acompananteVinculo: string;
  acompananteEdad: string;
  acompananteDni: string;
  acompananteInstruccion: string;
  acompananteDomicilio: string;
  acompananteCel: string;

  // Ingreso
  modalidadIngreso: 'demanda_espontanea' | 'derivacion_institucional' | 'interconsulta';
  institucionDerivacion: string;
  servicioInterconsulta: string;
  presencialAdmisionObservaciones: string;
  presencialAbordajeObservaciones: string;
  motivosConsulta: string;

  // Diagnostico Presuntivo
  diagnosticoPresuntivo: string[];
  
  // Evaluación de Riesgo
  antecedentesServicio: 'sin_antecedentes' | 'con_antecedentes';
  resumenIntervencionesPrevias: string;
  archivoAntecedentes: string | null;
  descripcionArchivo: string; 
  fechaAntecedente: string;

  // Intervenciones
  intervenciones: {
    admisionReferente: string; 
    virtualInstitucional: string; 
    referenteFamiliar: string; 
    afectivo: string; 
    horaJuego: 'niña' | 'niño' | null;
    entrevistaAdolescente: string; 
    examenFisico: string; 
    conForense: boolean;
    laboratorio: string; 
  };
  observacionesAdherencia: string;
  ausencias: string;

  // Descripción de Técnicas
  entrevistaReferenteDetalle: string;
  atencionPsicologicaDetalle: string;

  // Atención Médica
  atencionMedicaExamen: 'niña' | 'niño' | 'adolescente' | null;
  genitalNormal: boolean;
  atencionMedicaOtro: string;
  fotografiaConsentimiento: string[];
  laboratorioEstado: 'solicitado' | 'espera' | 'recibido';
  hallazgosLaboratorio: string;
  tratamientoIndicado: string;
  archivoAtencionMedica: string | null;
  descripcionAtencionMedica: string;
  
  // Articulación
  coordinacionProteccion: string;
  dialogoInterdisciplinario: string;

  // Estrategia
  estrategiaTurnoCon: string;
  estrategiaFechaTurno: string;
  recomiendaTerapiaReferente: boolean;
  derivacionA: string;
  espacioGrupal: boolean;
  acompanamientoGesell: boolean; 
  medidasImpedimento: string;

  // NUEVOS CAMPOS DE OBSERVACIONES TIPO TEXTAREA
  observacionesIntervenciones: string; // Para el Paso 4
  observacionesEstrategia: string;      // Para el Paso 5

  // Conclusión
  conclusionAbordaje: string[];
  indicadoresHallados: string[];
  indicadoresInespecificos: string;
  compatibilidad: string[];
  adjuntos: string[];
  adjuntosNombres: { [key: string]: string };
  observacionesSituacion: string;
  remitidoA: string[];
  fiscaliaNro: string;
  oficioNro: string;
  juzgadoInterviniente: string;
  detalleRelato: string;
  detalleLesiones: boolean; // Si es checkbox simple
  detalleLesionesTexto: string;
  // Variables para los textos largos de la conclusión
  textoDichosNNyA: string; 
  textoPreventivas: string;
  textoInespecificos: string; // Para el campo "E indicadles inespecificos..."
  
  // Selección Multiple legal
  destinoLegal: string[]; // Para guardar "fiscalia", "ota", "juzgado"
  
  nombreJuzgadoInterviniente: string; // nombre específico
  esJuzgadoDeTurno: boolean;
  profesionalPaso0: number; // Paso 1: Identificación
  profesionalPaso1: number; // Paso 2: Grupo Familiar
  profesionalPaso2: number; // Paso 3: Ingreso
  profesionalPaso3: number; // Paso 4: Intervenciones
  profesionalPaso4: number; // Paso 5: Clínica
  profesionalPaso5: number; // Paso 6: Conclusión
}