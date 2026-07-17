import { Component, EventEmitter, Input, Output, OnInit, PLATFORM_ID, inject } from '@angular/core';
import { CommonModule, isPlatformBrowser } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { FormState, ParentData } from '../../models/types';
import { HttpClient } from '@angular/common/http';
import { ChangeDetectorRef,  } from '@angular/core';
import { finalize } from 'rxjs/operators';


@Component({
  selector: 'app-medical-form',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './historia-clinica.component.html'
  
})
export class MedicalFormComponent {

    canEdit: boolean = false; // Permiso 1197
  isReadOnly: boolean = true; // Por defecto bloqueado
  private platformId = inject(PLATFORM_ID);

  ngOnInit() {
    this.checkPermissions();
    this.cargarProfesionalesServicio();
  }

  selectedTipoIntervencion: string = ''; // 'Familiar', 'Psicologica', 'Medica'
  profesionalesSeleccionados: number[] = [];

  toggleProfesional(id: number) {
      const index = this.profesionalesSeleccionados.indexOf(id);
      if (index > -1) this.profesionalesSeleccionados.splice(index, 1);
      else this.profesionalesSeleccionados.push(id);
  }

  checkPermissions() {
    if (isPlatformBrowser(this.platformId)) {
      const rawUser = localStorage.getItem('usuarioAHS');
      if (rawUser) {
        const userData = JSON.parse(rawUser);
        const codigos = userData.permisos.map((p: any) => p.accesos.toString());

        // 1197: Puede Editar y Guardar
       this.canEdit = codigos.includes('1197') || codigos.includes('1364')
        
        // Si tiene 1197, NO es solo lectura. 
        // Si NO tiene 1197 pero SI tiene 1198, es solo lectura.
        this.isReadOnly = !this.canEdit;
        this.cdr.detectChanges();
      }
    }
  }
  
  @Input() activeStep: number = 0;
  @Output() stepChange = new EventEmitter<number>();

  selectedFileAntecedentes: File | null = null;
  selectedFileClinica: File | null = null;

  initialParentData: ParentData = {
    nombre: '', edad: '', dni: '', nivelInstruccion: '', ocupacion: '', domicilio: '', cel: '',
    documento:'' 
  };
  // Helper para la vista de impresión
  toDate() {
    return new Date();
  }

  // Variables para la consulta histórica
  filtroFechaHistorico: string = '';
  selectedHistoricIntervention: any = null; // Guarda la intervención seleccionada para el modal
  showHistoryModal: boolean = false; // Controla si se muestra el modal de lectura
  // Lista de profesionales del servicio con sus IDs de usuario reales en la BD (Ajusta los IDs)
 profesionalesServicio: any[] = []; 

 

  cargarProfesionalesServicio() {
    const url = 'http://localhost:5000/HistoriaClinica/profesionales-servicio';
    this.http.get(url).subscribe({
      next: (res: any) => {
        this.profesionalesServicio = res;
        this.cdr.detectChanges();
      },
      error: (err) => console.error("Error al cargar profesionales del servicio ", err)
    });
  }

  buscandoPaciente = false;
  filesMap: { [key: string]: File } = {};
  // Inicialización del estado completo
  formData: FormState = {
    idHistoria: 0,
    idPaciente: 0,
    idEpisodio: 0,
    idUsuario: 0,
    idProfesionalTurno: 0,
    equipo: { medico: 0, trabajadorSocial: 0, psicologo: 0, enfermero: 0 }, medicoForense: '',
    nombreApellido: '', edad: '', fechaNacimiento: '', dni: '', domicilio: '', centroSalud: '', ultimoControl: '',telefonoCelular: '',
    telefonoFijo: '',  
    obraSocial: false, cualObraSocial: '', escolaridad: '',
    madre: { ...this.initialParentData },
    padre: { ...this.initialParentData },
    otrosConvivientes: [],
    // NOTA: Variables cambiadas a n en lugar de ñ
    acompananteTipo: [], acompananteNombre: '', acompananteVinculo: '', acompananteEdad: '', acompananteDni: '', acompananteInstruccion: '', acompananteDomicilio: '', acompananteCel: '',
    modalidadIngreso: 'demanda_espontanea', institucionDerivacion: '', servicioInterconsulta: '', motivosConsulta: '', presencialAdmisionObservaciones : '', presencialAbordajeObservaciones : '',
    diagnosticoPresuntivo: [],
    antecedentesServicio: 'sin_antecedentes', resumenIntervencionesPrevias: '',archivoAntecedentes: null, descripcionArchivo: '', fechaAntecedente: '',
    intervenciones: {
      admisionReferente: '', virtualInstitucional: '', referenteFamiliar: '', afectivo: '', horaJuego: null, entrevistaAdolescente: '', examenFisico: '', conForense: false, laboratorio: ''
    },
    observacionesAdherencia: '', ausencias: '',
    entrevistaReferenteDetalle: '', atencionPsicologicaDetalle: '',
    atencionMedicaExamen: null, genitalNormal: false, atencionMedicaOtro: '', fotografiaConsentimiento: [], laboratorioEstado: 'solicitado', hallazgosLaboratorio: '',archivoAtencionMedica: null,descripcionAtencionMedica: '', tratamientoIndicado: '',
    coordinacionProteccion: '', dialogoInterdisciplinario: '',
    estrategiaTurnoCon: '', estrategiaFechaTurno: '', recomiendaTerapiaReferente: false, derivacionA: '', espacioGrupal: false, acompanamientoGesell: false, medidasImpedimento: '',
    conclusionAbordaje: [], indicadoresHallados: [], indicadoresInespecificos: '', compatibilidad: [], adjuntos: [], adjuntosNombres: {}, observacionesSituacion: '', remitidoA: [], fiscaliaNro: '', oficioNro: '', juzgadoInterviniente: '', detalleRelato: '',
    detalleLesiones: false, observacionesIntervenciones: '', observacionesEstrategia: '',
    detalleLesionesTexto: '',
    textoDichosNNyA: '',
    textoPreventivas: '',
    textoInespecificos: '',
    destinoLegal: [],
    nombreJuzgadoInterviniente: '',
    esJuzgadoDeTurno: false, profesionalPaso0: 0, profesionalPaso1: 0, profesionalPaso2: 0, profesionalPaso3: 0, profesionalPaso4: 0, profesionalPaso5: 0 
  };

  

  constructor(private http: HttpClient, private cdr: ChangeDetectorRef) {}
    calcularEdad(fechaNacimiento: string): string {
      if (!fechaNacimiento) return '';

      const nacimiento = new Date(fechaNacimiento);
      const hoy = new Date();

      let edad = hoy.getFullYear() - nacimiento.getFullYear();

      const mes = hoy.getMonth() - nacimiento.getMonth();

      if (
        mes < 0 ||
        (mes === 0 && hoy.getDate() < nacimiento.getDate())
      ) {
        edad--;
      }

      return edad.toString();
    }


     

     // Variables de control de vista
  vistaActual: 'buscador' | 'formulario' = 'buscador';
  episodiosPaciente: any[] = []; // Aquí guardaremos los episodios previos

  // Modifica tu función buscarPaciente para que NO entre directo al formulario, sino que muestre la grilla:
          

   cargarEpisodiosDelPaciente(idPaciente: number) {
    const url = `http://localhost:5000/HistoriaClinica/episodios/${idPaciente}`;
    this.http.get(url).subscribe({
      next: (res: any) => {
        this.episodiosPaciente = res; // Guardamos los episodios que vienen de la BD
        this.cdr.detectChanges(); // Pintamos la tabla
      },
      error: (err) => console.error("Error al cargar episodios", err)
    });
  }

  // 2. CORRECCIÓN: Botón NUEVO EPISODIO
  iniciarNuevoEpisodio() {
    this.formData.idEpisodio = 0; // Obligamos a que sea un INSERT en la BD
    this.activeStep = 0;
    this.vistaActual = 'formulario';
    this.cdr.detectChanges(); // <--- ESTO FALTABA: Obliga a Angular a mostrar el formulario YA
  }

  // 3. CORRECCIÓN: Botón CONTINUAR CARGA
  continuarEpisodio(episodio: any) {
    // Como ahora usamos SQL relacional y no JSON, al continuar un episodio 
    // simplemente le decimos al sistema el ID del episodio.
    // Al guardar, el backend actualizará este mismo episodio en lugar de crear uno nuevo.
    this.formData.idEpisodio = episodio.idEpisodio;
    
    this.activeStep = 0; 
    this.vistaActual = 'formulario';
    this.cdr.detectChanges(); // <--- ESTO FALTABA
    
    alert(`Continuando el episodio #${episodio.idEpisodio}. Todo lo que guarde se asociará a esta admisión.`);
  }

  volverAlBuscador() {
    this.vistaActual = 'buscador';
    this.formData = this.initialState();
    this.episodiosPaciente = [];
  }

          buscarPaciente(tipo: string, valor: string) {
          if (!valor) return;
    this.buscandoPaciente = true;

    // Respaldo de usuario logueado
    const idMedicoActual = this.formData.idUsuario; 
    this.formData = this.initialState(); 
    this.formData.idUsuario = idMedicoActual; 

    const url = `http://localhost:5000/HistoriaClinica/buscar-paciente?tipo=${tipo}&valor=${valor}`;

    this.http.get(url).subscribe({
      next: (resp: any) => {
        this.buscandoPaciente = false;
        
        this.formData.idHistoria = 0;
        this.formData.idPaciente = resp.idPaciente || 0;
        this.mapearDatosFisicos(resp);

        // --- ESTA ES LA MAGIA: LLAMAMOS A LOS EPISODIOS REALES ---
        if ((this.formData?.idPaciente ?? 0) > 0) {
            this.cargarEpisodiosDelPaciente(this.formData.idPaciente ?? 0);
        }

        // Nos aseguramos de quedarnos en la vista del buscador para ver la grilla
        this.vistaActual = 'buscador';
        this.cdr.detectChanges(); 
      },
      error: (err) => {
        this.buscandoPaciente = false;
        alert('No se encontró el paciente en el sistema.');
      }
          });

        }


  /**
   * Esta función realiza el mapeo de los datos que vienen de la API/SQL
   * garantizando que los nombres de los padres, DNI y Obra Social se pinten bien.
   */
  private mapearDatosFisicos(data: any) {
    // 1. Datos básicos del Paciente
    this.formData.nombreApellido = data.nombreApellido || '';
    this.formData.dni = data.dni || '';
    this.formData.fechaNacimiento = data.fechaNacimiento?.substring(0, 10) || '';
    this.formData.edad = data.edad || this.calcularEdad(this.formData.fechaNacimiento);
    this.formData.domicilio = data.domicilio || '';
    this.formData.centroSalud = data.centroSalud || '';
    this.formData.ultimoControl = data.ultimoControl ? data.ultimoControl.substring(0, 10) : '';
    this.formData.escolaridad = data.escolaridad || '';
    this.formData.idPaciente = data.idPaciente || 0;
    this.formData.idHistoria = data.idHistoria || 0;
    this.formData.idEpisodio = data.idEpisodio || 0;
    this.formData.idUsuario = data.idUsuario || this.formData.idUsuario || 0; // Mantenemos el ID de usuario actual si no viene del backend
    this.formData.equipo = {
      medico: data.medicoInterviniente || 0,
      trabajadorSocial: data.trabajadorSocial || 0,
      psicologo: data.psicologo || 0,
      enfermero: data.enfermero || 0
    };
    this.formData.medicoForense = data.medicoForense || '';
    
    // NUEVO: Mapear los antecedentes clínicos del paciente recuperados de la BD
    this.formData.antecedentesServicio = data.antecedentesServicio || 'sin_antecedentes';
    this.formData.resumenIntervencionesPrevias = data.resumenIntervencionesPrevias || '';
    this.formData.archivoAntecedentes = data.archivoAntecedentes || '';
    this.formData.descripcionArchivo = data.descripcionArchivo || '';
    this.formData.fechaAntecedente = data.fechaAntecedente || '';
    // 2. Teléfonos (Prioridad a lo que venga de la ficha)
    this.formData.telefonoCelular = data.telefono || '';
    this.formData.telefonoFijo = data.telefonoFijo || '';

    this.formData.observacionesIntervenciones = data.observacionesIntervenciones || '';
    this.formData.observacionesEstrategia = data.observacionesEstrategia || '';

    // 3. LOGICA OBRA SOCIAL (Transformar string a Boolean + Texto para Sainnavv)
    // Si el backend envía "Plan Nacer", "INSSSEP", etc.
    if (data.obraSocial && data.obraSocial !== 'Ninguna' && data.obraSocial !== '0') {
      this.formData.obraSocial = true; // Activa el radio "SI"
      this.formData.cualObraSocial = data.obraSocial; // Muestra el nombre
    } else {
      this.formData.obraSocial = false; // Activa el radio "NO"
      this.formData.cualObraSocial = '';
    }

    // 4. DATOS DE LA MADRE (Solo de la API)
    if (data.madre) {
      this.formData.madre.nombre = data.madre.nombre || '';
      this.formData.madre.dni = data.madre.dni || ''; // Aquí llegará el DNI si el backend lo encuentra
      this.formData.madre.edad = data.madre.edad || '';
      this.formData.madre.nivelInstruccion = data.madre.nivelInstruccion || '';
      this.formData.madre.ocupacion = data.madre.ocupacion || '';
      this.formData.madre.domicilio = data.madre.domicilio || data.domicilio || '';
    }

    // 5. DATOS DEL PADRE (Solo de la API)
    if (data.padre) {
      this.formData.padre.nombre = data.padre.nombre || '';
      this.formData.padre.dni = data.padre.dni || '';
      this.formData.padre.edad = data.padre.edad || '';
      this.formData.padre.nivelInstruccion = data.padre.nivelInstruccion || '';
      this.formData.padre.ocupacion = data.padre.ocupacion || '';
      this.formData.padre.domicilio = data.padre.domicilio || data.domicilio || '';
    }
  }

  // Helper para resetear estructura base si hiciera falta
  initialState(): FormState {
      return { 
          idHistoria: 0, 
          idPaciente: 0,
          idEpisodio: 0,
          idUsuario: 0,
          idProfesionalTurno: 0,
          equipo: { medico: 0, trabajadorSocial: 0, psicologo: 0, enfermero: 0 },
          nombreApellido: '', edad: '', fechaNacimiento: '', dni: '', domicilio: '', centroSalud: '', ultimoControl: '', telefonoCelular: '', telefonoFijo: '',
          obraSocial: false, cualObraSocial: '', escolaridad: '', medicoForense: '',
          madre: { ...this.initialParentData },
          padre: { ...this.initialParentData },
          otrosConvivientes: [],
    // NOTA: Variables cambiadas a n en lugar de ñ
          acompananteTipo: [], acompananteNombre: '', acompananteVinculo: '', acompananteEdad: '', acompananteDni: '', acompananteInstruccion: '', acompananteDomicilio: '', acompananteCel: '',
          modalidadIngreso: 'demanda_espontanea', institucionDerivacion: '', servicioInterconsulta: '', motivosConsulta: '', presencialAdmisionObservaciones: '',presencialAbordajeObservaciones: '', presencialAbordaje: '', virtualAdmisión: '', referenteFamiliar: false, afectivo: false, horaJuego: null, entrevistaAdolescente: false, examenFisico: false, conForense: false, laboratorio: '',
          diagnosticoPresuntivo: [],
          antecedentesServicio: 'sin_antecedentes', resumenIntervencionesPrevias: '',archivoAntecedentes: null, descripcionArchivo: '', fechaAntecedente: '',
          intervenciones: {
            admisionReferente: '', virtualInstitucional: '', referenteFamiliar: '', afectivo: '', horaJuego: null, entrevistaAdolescente: '', examenFisico: '', conForense: false, laboratorio: ''
          },
          observacionesAdherencia: '', ausencias: '',
          entrevistaReferenteDetalle: '', atencionPsicologicaDetalle: '',
          atencionMedicaExamen: null, genitalNormal: false, atencionMedicaOtro: '', fotografiaConsentimiento: [], laboratorioEstado: 'solicitado', hallazgosLaboratorio: '',archivoAtencionMedica: null,descripcionAtencionMedica: '', tratamientoIndicado: '',
          coordinacionProteccion: '', dialogoInterdisciplinario: '',
          estrategiaTurnoCon: '', estrategiaFechaTurno: '', recomiendaTerapiaReferente: false, derivacionA: '', espacioGrupal: false, acompanamientoGesell: false, medidasImpedimento: '',
          conclusionAbordaje: [], indicadoresHallados: [], indicadoresInespecificos: '', compatibilidad: [], adjuntos: [], adjuntosNombres: {}, observacionesSituacion: '', remitidoA: [], fiscaliaNro: '', oficioNro: '', juzgadoInterviniente: '', detalleRelato: '',
          detalleLesiones: false, detalleLesionesTexto: '', observacionesEstrategia: '', observacionesIntervenciones: '',
          
          textoDichosNNyA: '',
          textoPreventivas: '',
          textoInespecificos: '',
          destinoLegal: [],
          nombreJuzgadoInterviniente: '',
          esJuzgadoDeTurno: false,
          profesionalPaso0: 0,  profesionalPaso1: 0, profesionalPaso2: 0, profesionalPaso3: 0, profesionalPaso4: 0, profesionalPaso5: 0,
          // etc...
      } as FormState; // 'as' temporal
  }
  
    // Opciones de adjuntos disponibles
  adjuntoOpciones = [
    'Pruebas Médicas',
    'Fotografías',
    'Informes Salud Mental',
    'Informes Sociales',
    'Consentimientos',
    'Protocolo Médico'
  ];
  // Manejo de Checkboxes múltiples
  handleCheckboxList(listName: keyof FormState, value: string): void {
     const currentList = this.formData[listName] as string[];
     const index = currentList.indexOf(value);
     if (index > -1) currentList.splice(index, 1);
     else currentList.push(value);
  }
 // Función para agregar un conviviente vacío a la lista
  addConviviente() {
    this.formData.otrosConvivientes.push({ nombre: '', vinculo: '', edad: '', ocupacion: '', observaciones: '' });
  }

    removeConviviente(index: number) {
    this.formData.otrosConvivientes.splice(index, 1);
  }

 onFileSelected(event: any) {
    const file: File = event.target.files[0];
    if (file) {
      if (file.type !== 'application/pdf') {
        alert('Por favor, suba únicamente archivos PDF.');
        return;
      }
      // UI: Guardamos solo el nombre para mostrar "Cargado"
      this.formData.archivoAntecedentes = file.name;
      
      // LOGICA: Guardamos el archivo real para enviar al backend
      this.selectedFileAntecedentes = file; 
    }
  }

  // 3. Método para quitar el archivo si se equivocó
  removeFile() {
    this.formData.archivoAntecedentes = null;
    this.selectedFileAntecedentes = null; // Limpiamos la variable real
  }

   // PASO 4 (CLÍNICA)
  onMedicalFileSelected(event: any) {
    const file: File = event.target.files[0];
    if (file) {
      if (file.type !== 'application/pdf') {
        alert('Por favor, suba únicamente archivos PDF.');
        return;
      }
      // UI
      this.formData.archivoAtencionMedica = file.name;
      
      // LOGICA
      this.selectedFileClinica = file;
    }
  }

  // Elimina archivo del paso CLÍNICA
  removeMedicalFile() {
    this.formData.archivoAtencionMedica = null;
    this.selectedFileClinica = null; // Limpiamos variable real
  }

  // Abre el PDF en una pestaña nueva llamando a la API
  verPdf(nombreArchivo: string) {
    if (!nombreArchivo) {
      alert('No hay ningún archivo asociado.');
      return;
    }

    // Ruta de tu API local (Recuerda cambiar a la IP del Servidor IIS en producción: http://10.4.50.70/HistoriaClinica/...)
    const baseUrl = 'http://localhost:5000/HistoriaClinica';  
    const url = `${baseUrl}/descargar-pdf?nombre=${encodeURIComponent(nombreArchivo)}`;

    // Abre una nueva pestaña en el navegador en segundo plano
    window.open(url, '_blank');
  }

  onAdjuntoFileSelected(event: any, categoria: string) {
    const file: File = event.target.files[0];
    if (file) {
        if (file.type !== 'application/pdf') {
            alert('Solo se permiten archivos PDF.');
            return;
        }
        // Guardamos el objeto real para enviarlo luego
        this.filesMap[categoria] = file;
        
        // Guardamos el nombre en el formData para que quede en el JSON de base de datos
        this.formData.adjuntosNombres[categoria] = file.name;
    }
  }

  // B) Eliminar archivo
  removeAdjuntoFile(categoria: string) {
      delete this.filesMap[categoria];
      delete this.formData.adjuntosNombres[categoria];
      
      // Opcional: Si quieres que al borrar el archivo se destilde la opción
      // const idx = this.formData.adjuntos.indexOf(categoria);
      // if (idx > -1) this.formData.adjuntos.splice(idx, 1);
  }
   toggleAdjunto(categoria: string) {
      const idx = this.formData.adjuntos.indexOf(categoria);
      if (idx > -1) {
          // Si estaba marcado y se desmarca, borramos el archivo asociado
          this.formData.adjuntos.splice(idx, 1);
          this.removeAdjuntoFile(categoria);
      } else {
          // Marcar
          this.formData.adjuntos.push(categoria);
      }
  }
  
  // Opciones
  acompananteOptions = ['Madre', 'Padre', 'Abuelo/a', 'Tío/a', 'Referente Afectivo', 'Otro'];

  // Opciones para el selector de "Paciente explorado" con tipos correctos
  pacienteOptions: { label: string; val: 'niña' | 'niño' | 'adolescente' }[] = [
    { label: 'Niña', val: 'niña' },
    { label: 'Niño', val: 'niño' },
    { label: 'Adolescente', val: 'adolescente' },
  ];

  parentLabels: {[key: string]: string} = {
    nombre: 'Nombre y Apellido', edad: 'Edad', dni: 'D.N.I.', 
    nivelInstruccion: 'Nivel de Instrucción', ocupacion: 'Ocupación', 
    domicilio: 'Domicilio', cel: 'Teléfono / Celular'
  };

  diagnosisOptions = [
    { label: "Violencia física (T74.1)", color: "text-rose-600" },
    { label: "Violencia Psicológica (T74.3)", color: "text-amber-600" },
    { label: "Negligencia (T74.0)", color: "text-orange-600" },
    { label: "Violencia sexual (T74.2)", color: "text-rose-700" },
    { label: "Conductas sexualizadas abusivas", color: "text-rose-600" },
    { label: "Incesto paterno filial", color: "text-rose-800" },
    { label: "Acoso sexual", color: "text-indigo-600" },
    { label: "Ciberacoso / Grooming", color: "text-indigo-700" }, // Destacado
    { label: "Explotación sexual", color: "text-red-800 font-black" }, // Destacado
    { label: "Embarazo forzado (Z33)", color: "text-purple-600" },
    { label: "Munchausen by Proxy (T74.8)", color: "text-blue-600" },
    { label: "Carencia material (Z59)", color: "text-slate-600" },
    { label: "Examen y prueba de embarazo (Z33)", color: "text-slate-600" },
    { label: "Sin indicaciones de riesgo (Z10) ", color: "text-slate-600" },
    { label: "Historial personal de factores de riesgo (Z91)", color: "text-slate-600" },
    { label: "Tricomoniasis (A59) ", color: "text-slate-600" },
    { label: "Agresión por otros medios especificados (Y08)", color: "text-slate-600" },
    { label: "ITS / Sifilis (A53.9)", color: "text-teal-600" }
  ];

  historialIntervenciones: any[] = [];
  cargandoHistorial = false;

  // Modificamos el método de cambio de paso para cargar el historial automáticamente al entrar al paso 4 (activeStep === 3)
  // 1. Modificar para que cargue con ID PACIENTE
  cargarHistorialIntervenciones() {
    if (!this.formData.idPaciente || this.formData.idPaciente <= 0) return;

    this.cargandoHistorial = true;
    
    // Cambiamos el endpoint para pasar idPaciente
    const url = `http://localhost:5000/HistoriaClinica/intervenciones/${this.formData.idPaciente}`;
    
    this.http.get(url).subscribe({
      next: (res: any) => {
        this.historialIntervenciones = res;
        this.cargandoHistorial = false;
        this.cdr.detectChanges();
      },
      error: (err) => {
        console.error("Error al cargar historial", err);
        this.cargandoHistorial = false;
      }
    });
  }

  // 2. Modificar el cambiador de paso
  cambiarPaso(nuevoPaso: number) {
    this.stepChange.emit(nuevoPaso);
    
    // Ahora Historial está en el Paso 5 (Index 4)
    if (nuevoPaso === 4 && this.formData.idPaciente && this.formData.idPaciente > 0) {
      this.cargarHistorialIntervenciones();
    }
  }

  // 3. ACTUALIZAR EN TU MÉTODO buscarPaciente (dentro del success)
  // Añade esta línea al final del "next" del buscador cuando mapees los datos:
  // (Esto cargará el historial silenciosamente apenas buscas al paciente)
  /*
     next: (resp: any) => {
        ... (tu logica de mapeo) ...
        this.formData.idPaciente = resp.idPaciente;
        
        // AGREGAR ESTA LÍNEA AQUÍ:
        this.cargarHistorialIntervenciones(); 
        
        this.cdr.detectChanges();
     }
  */

 // Se ejecuta al hacer clic en una tarjeta de historial
  seleccionarIntervencionHistorial(item: any) {
    this.selectedHistoricIntervention = item;
    this.showHistoryModal = true; // Abre el modal de lectura
    this.cdr.detectChanges();
  }

  cerrarModalHistorico() {
    this.selectedHistoricIntervention = null;
    this.showHistoryModal = false; // Cierra el modal
    this.cdr.detectChanges();
  }



getHistorialFiltrado() {
    if (!this.filtroFechaHistorico) {
        return this.historialIntervenciones;
    }
    // Comparamos el string de fecha (ej: '2026-06-22') con el item.fecha que viene del backend
    return this.historialIntervenciones.filter(item => 
        item.fecha.includes(this.filtroFechaHistorico)
    );
}

  // Modificación del Guardado Rápido (Guardar progreso de paso)
  guardarPasoActual() {
    this.buscandoPaciente = true; // Activa spinner visual en el botón

    const rawUser = localStorage.getItem('usuarioAHS');
    const userObj = rawUser ? JSON.parse(rawUser) : null;
    this.formData.idUsuario = userObj?.codigoUsuario ? parseInt(userObj.codigoUsuario) : 1;

    const formDataApi = new FormData();
    formDataApi.append('JsonData', JSON.stringify(this.formData));

    // Adjuntar archivos si existen en memoria
    if (this.selectedFileAntecedentes) formDataApi.append('ArchivoAntecedentes', this.selectedFileAntecedentes);
    if (this.selectedFileClinica) formDataApi.append('ArchivoClinica', this.selectedFileClinica);

     const url = 'http://localhost:5000/HistoriaClinica';

    this.http.post(url, formDataApi).subscribe({
      next: (response: any) => {
        this.buscandoPaciente = false;
        
        // Sincronizamos el ID del episodio para las siguientes llamadas/pasos
        this.formData.idEpisodio = response.episodio; 
        
        this.cdr.detectChanges();
        alert('Progreso del formulario guardado correctamente en la base de datos.');
        
        // Si estamos en el paso 4, refrescar el historial después de guardar
        if (this.activeStep === 3) {
          this.cargarHistorialIntervenciones();
        }
      },
      error: (error) => {
        this.buscandoPaciente = false;
        alert('Error al guardar el progreso.');
      }
    });
  }

  // Helper para iterar campos de padre/madre en el HTML
  parentFields = Object.keys(this.initialParentData) as (keyof ParentData)[];

  // Navegación
  prevStep() { this.stepChange.emit(this.activeStep - 1); }
  nextStep() { this.stepChange.emit(this.activeStep + 1); }

   triggerPrint() { 
    // A) Primero intentamos guardar en el backend
    this.enviarAlBackend();

    // B) Luego imprimimos (puedes mover esto dentro del 'next' del subscribe si quieres imprimir solo si se guardó ok)
    setTimeout(() => {
        window.print();
    }, 1000);
  }

  enviarAlBackend() {
    this.buscandoPaciente = true;

    const rawUser = localStorage.getItem('usuarioAHS');
    const userObj = rawUser ? JSON.parse(rawUser) : null;
    
    // Sincronizamos ID del usuario actual
    this.formData.idUsuario = userObj?.codigoUsuario ? parseInt(userObj.codigoUsuario) : 1;

    // Creamos payload temporal con la lista de profesionales seleccionados
    const payload = { ...this.formData, profesionalesSeleccionados: this.profesionalesSeleccionados };

    const formDataApi = new FormData();
    formDataApi.append('JsonData', JSON.stringify(payload));

    // Adjuntar archivos si existen
    if (this.selectedFileAntecedentes) {
      formDataApi.append('ArchivoAntecedentes', this.selectedFileAntecedentes);
    }
    if (this.selectedFileClinica) {
      formDataApi.append('ArchivoClinica', this.selectedFileClinica);
    }
    
    Object.keys(this.filesMap).forEach(key => {
      const file = this.filesMap[key];
      formDataApi.append(`Adjunto_${key}`, file);
    });

    // ========================================================
    // CORRECCIÓN DE LA URL: Quitar el "/api"
    // ========================================================
    const url = 'http://localhost:5000/HistoriaClinica'; // 
    // ========================================================

    this.http.post(url, formDataApi).subscribe({
      next: (response: any) => {
        this.buscandoPaciente = false;
        this.formData.idEpisodio = response.episodio; 
        
        this.cdr.detectChanges();
        alert('Datos de la consulta y archivos guardados correctamente.');
      },
      error: (error) => {
        this.buscandoPaciente = false;
        console.error('Error al guardar:', error);
        alert('Atención: Hubo un error al guardar en la base de datos.');
      }
    });
  }
 // Helper para el PDF: Devuelve el nombre completo del profesional dado su ID
  getNombreProfesional(idProf: number): string {
    if (!idProf || idProf === 0) return 'No intervino';
    
    // Buscamos el profesional en la lista que nos dio la API
    const profesional = this.profesionalesServicio.find(p => p.idProfesional === Number(idProf));
    
    if (profesional) {
      return `${profesional.apellido} ${profesional.nombreProf}`;
    }
    
    return 'Profesional no encontrado';
  }
  
}