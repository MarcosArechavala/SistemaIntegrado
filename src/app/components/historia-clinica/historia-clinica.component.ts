import { Component, EventEmitter, Input, Output, OnInit, PLATFORM_ID, inject } from '@angular/core';
import { CommonModule, isPlatformBrowser } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { FormState, ParentData } from '../../models/types';
import { HttpClient } from '@angular/common/http';
import { ChangeDetectorRef,  } from '@angular/core';


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
    nombre: '', edad: '', dni: '', nivelInstruccion: '', ocupacion: '', domicilio: '', cel: ''
  };
  // Helper para la vista de impresión
  toDate() {
    return new Date();
  }
  buscandoPaciente = false;
  filesMap: { [key: string]: File } = {};
  // Inicialización del estado completo
  formData: FormState = {
    idHistoria: 0,
    medicoInterviniente: '', trabajadorSocial: '', psicologo: '', enfermero: '',
    nombreApellido: '', edad: '', fechaNacimiento: '', dni: '', domicilio: '', centroSalud: '', ultimoControl: '',
    obraSocial: false, cualObraSocial: '', escolaridad: '',
    madre: { ...this.initialParentData },
    padre: { ...this.initialParentData },
    otrosConvivientes: [],
    // NOTA: Variables cambiadas a n en lugar de ñ
    acompananteTipo: [], acompananteNombre: '', acompananteVinculo: '', acompananteEdad: '', acompananteDni: '', acompananteInstruccion: '', acompananteDomicilio: '', acompananteCel: '',
    modalidadIngreso: 'demanda_espontanea', institucionDerivacion: '', servicioInterconsulta: '', motivosConsulta: '',
    diagnosticoPresuntivo: [],
    antecedentesServicio: 'sin_antecedentes', resumenIntervencionesPrevias: '',archivoAntecedentes: null, descripcionArchivo: '',
    intervenciones: {
      admisionReferente: '', virtualInstitucional: '', referenteFamiliar: '', afectivo: '', horaJuego: null, entrevistaAdolescente: '', examenFisico: '', conForense: false, laboratorio: ''
    },
    observacionesAdherencia: '', ausencias: '',
    entrevistaReferenteDetalle: '', atencionPsicologicaDetalle: '',
    atencionMedicaExamen: null, genitalNormal: false, atencionMedicaOtro: '', fotografiaConsentimiento: [], laboratorioEstado: 'solicitado', hallazgosLaboratorio: '',archivoAtencionMedica: null,descripcionAtencionMedica: '', tratamientoIndicado: '',
    coordinacionProteccion: '', dialogoInterdisciplinario: '',
    estrategiaTurnoCon: '', estrategiaFechaTurno: '', recomiendaTerapiaReferente: false, derivacionA: '', espacioGrupal: false, acompanamientoGesell: false, medidasImpedimento: '',
    conclusionAbordaje: [], indicadoresHallados: [], indicadoresInespecificos: '', compatibilidad: [], adjuntos: [], adjuntosNombres: {}, observacionesSituacion: '', remitidoA: [], fiscaliaNro: '', oficioNro: '', juzgadoInterviniente: '', detalleRelato: '',
    detalleLesiones: false,
    detalleLesionesTexto: '',
    textoDichosNNyA: '',
    textoPreventivas: '',
    textoInespecificos: '',
    destinoLegal: [],
    nombreJuzgadoInterviniente: '',
    esJuzgadoDeTurno: false,
  };
  constructor(private http: HttpClient, private cdr: ChangeDetectorRef) {}


  buscarPaciente(tipo: string, valor: string) {
    if (!valor) return;
    this.buscandoPaciente = true; 

    const url = `http://localhost:5000/HistoriaClinica/buscar-paciente?tipo=${tipo}&valor=${valor}`;

    this.http.get(url).subscribe({
      next: (resp: any) => {
        
        // CASO A: PACIENTE YA EXISTE LOCALMENTE (Resumen/Edición)
        if (resp.origen === 'local') {
            const datosGuardados = JSON.parse(resp.datos);
            // Sobreescribimos TODO el formData con lo guardado
            this.formData = { 
                ...this.initialState(), // Asegura estructura base por si hay campos nuevos
                ...datosGuardados,
                idHistoria: resp.idHistoria // IMPORTANTE: Guardamos el ID para hacer UPDATE luego
            };
            
            // Alerta visual sutil
            console.log('Cargando historia local ID: ' + resp.idHistoria);
        } 
        // CASO B: PACIENTE NUEVO (Viene del Hospital)
        else {
            // Mantenemos la estructura actual pero reseteamos el ID
            this.formData.idHistoria = 0; 
            this.formData.nombreApellido = resp.nombreApellido || '';
            this.formData.dni = resp.dni || '';
            this.formData.fechaNacimiento = resp.fechaNacimiento || '';
            this.formData.edad = resp.edad || '';
            this.formData.domicilio = resp.domicilio || '';
            
            // ... resto de tu mapeo de API hospital ...
            // (dni, fechaNacimiento, domicilio, madre, padre...)
            if(resp.madre) this.formData.madre = { ...this.initialState().madre, ...resp.madre };
            if(resp.padre) this.formData.padre = { ...this.initialState().padre, ...resp.padre };
        }

        this.buscandoPaciente = false;
        this.cdr.detectChanges(); 
        
        // Auto-foco
        setTimeout(() => document.getElementById('inputNombrePaciente')?.focus(), 100);
      },
      error: (err) => {
        this.buscandoPaciente = false;
        alert('No se encontraron datos.');
      }
    });
  }

  // Helper para resetear estructura base si hiciera falta
  initialState(): FormState {
      return { 
          idHistoria: 0, 
          medicoInterviniente: '', trabajadorSocial: '', psicologo: '', enfermero: '',
          nombreApellido: '', edad: '', fechaNacimiento: '', dni: '', domicilio: '', centroSalud: '', ultimoControl: '',
          obraSocial: false, cualObraSocial: '', escolaridad: '',
          madre: { ...this.initialParentData },
          padre: { ...this.initialParentData },
          otrosConvivientes: [],
    // NOTA: Variables cambiadas a n en lugar de ñ
          acompananteTipo: [], acompananteNombre: '', acompananteVinculo: '', acompananteEdad: '', acompananteDni: '', acompananteInstruccion: '', acompananteDomicilio: '', acompananteCel: '',
          modalidadIngreso: 'demanda_espontanea', institucionDerivacion: '', servicioInterconsulta: '', motivosConsulta: '',
          diagnosticoPresuntivo: [],
          antecedentesServicio: 'sin_antecedentes', resumenIntervencionesPrevias: '',archivoAntecedentes: null, descripcionArchivo: '',
          intervenciones: {
            admisionReferente: '', virtualInstitucional: '', referenteFamiliar: '', afectivo: '', horaJuego: null, entrevistaAdolescente: '', examenFisico: '', conForense: false, laboratorio: ''
          },
          observacionesAdherencia: '', ausencias: '',
          entrevistaReferenteDetalle: '', atencionPsicologicaDetalle: '',
          atencionMedicaExamen: null, genitalNormal: false, atencionMedicaOtro: '', fotografiaConsentimiento: [], laboratorioEstado: 'solicitado', hallazgosLaboratorio: '',archivoAtencionMedica: null,descripcionAtencionMedica: '', tratamientoIndicado: '',
          coordinacionProteccion: '', dialogoInterdisciplinario: '',
          estrategiaTurnoCon: '', estrategiaFechaTurno: '', recomiendaTerapiaReferente: false, derivacionA: '', espacioGrupal: false, acompanamientoGesell: false, medidasImpedimento: '',
          conclusionAbordaje: [], indicadoresHallados: [], indicadoresInespecificos: '', compatibilidad: [], adjuntos: [], adjuntosNombres: {}, observacionesSituacion: '', remitidoA: [], fiscaliaNro: '', oficioNro: '', juzgadoInterviniente: '', detalleRelato: '',
          detalleLesiones: false, detalleLesionesTexto: '',
          
          textoDichosNNyA: '',
          textoPreventivas: '',
          textoInespecificos: '',
          destinoLegal: [],
          nombreJuzgadoInterviniente: '',
          esJuzgadoDeTurno: false,
          // etc...
      } as FormState; // 'as' temporal
  }
  
    // Opciones de adjuntos disponibles
  adjuntoOpciones = [
    'Pruebas Médicas',
    'Fotografías',
    'Informes Salud Mental',
    'Informes Sociales',
    'Consentimientos'
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
    { label: "Ciberacoso / Grooming", color: "text-indigo-700" },
    { label: "Violación", color: "text-red-700 font-black" }, // Destacado
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
    const formDataApi = new FormData();
    
    // Agregar el JSON normal
    formDataApi.append('JsonData', JSON.stringify(this.formData));

    // Agregar Archivos Principales (Antecedentes / Clinica) - Lógica vieja
    if (this.selectedFileAntecedentes) formDataApi.append('ArchivoAntecedentes', this.selectedFileAntecedentes);
    if (this.selectedFileClinica) formDataApi.append('ArchivoClinica', this.selectedFileClinica);

    // NUEVO: Agregar archivos dinámicos de Adjuntos
    // Los enviaremos con una clave especial ej: "Adjunto_Fotografias"
    Object.keys(this.filesMap).forEach(key => {
        const file = this.filesMap[key];
        // Usamos una clave reconocible para el backend o simplemente un array
        // Recomiendo usar prefijo para que el backend sepa que es de esta lista
        formDataApi.append(`Adjunto_${key}`, file);
    });

    // 4. Enviar (Asegurate que el puerto 5000/5001 coincida con tu .NET launchSettings.json)
    // Usualmente .NET Core usa: https://localhost:7001 o http://localhost:5000
    const url = 'http://localhost:5000/HistoriaClinica'; 

    this.http.post(url, formDataApi).subscribe({
        next: (response: any) => {
            console.log('Guardado exitoso ID:', response.idHistoria);
            alert('Datos guardados correctamente en el sistema.');
        },
        error: (error) => {
            console.error('Error al guardar:', error);
            alert('Atención: Se imprimirá el reporte pero hubo un error al guardar en la base de datos.');
        }
    });
  }
 
  
}