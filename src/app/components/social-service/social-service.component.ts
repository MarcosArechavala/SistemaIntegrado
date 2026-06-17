import { Component, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { SocialFormData, initialSocialData } from '../../models/social.types';


@Component({
  selector: 'app-social-service',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './social-service.component.html'
})
export class SocialServiceComponent {
  
  formData: SocialFormData = { ...initialSocialData };
  
  // Estados
  loading = false;
  isLocked = false; // Bloquea Apellido/Nombre/HC si el paciente ya existe

  // Listas desplegables
  options = {
    tipoDoc: ['DNI', 'LC', 'LE', 'PASAPORTE'],
    sexo: [{ v: 'M', l: 'Masculino' }, { v: 'F', l: 'Femenino' }, { v: 'I', l: 'Indeterminado' }],
    nacionalidad: ['Argentina', 'Paraguay'],
       provincia:[
        'ALTO PARAGUAY', 'AMAMBAY', 'ASUNCION', 'BOQUERON', 'BUENOS AIRES', 'CAAGUAZU', 
        'CAAZAPA', 'CANINDEYU', 'CAPITAL FEDERAL', 'CATAMARCA', 'CENTRAL', 'CHACO', 
        'CHUBUT', 'CONCEPCION', 'CORDOBA', 'CORDILLERA', 'CORRIENTES', 'ENTRE RIOS', 
        'FORMOSA', 'GUAIRA', 'ITAPUA', 'JUJUY', 'LA PAMPA', 'LA RIOJA', 'MENDOZA', 
        'MISIONES', 'MISIONES (PY)', 'NEUQUEN', 'NO ESPECIFICADO', 'ÑEEMBUCU', 
        'OTRO PAIS', 'PARAGUARI', 'PTE. HAYES', 'RIO NEGRO', 'SALTA', 'SAN JUAN', 
        'SAN LUIS', 'SAN PEDRO', 'SANTA CRUZ', 'SANTA FE', 'STGO. DEL ESTERO', 
        'TIERRA DEL FUEGO', 'TUCUMAN'
    ].sort(),
     departamento:[
        '12 DE OCTUBRE', '2 DE ABRIL', '25 DE MAYO', '9 DE JULIO', 'ALBERDI (1)', 'ALMIRANTE BROWN', 'ANTA', 
        'ASUNCION', 'ATAMISQUI', 'BANDA', 'BELGRANO', 'BELLA VISTA', 'BERMEJO', 'BERÓN DE ASTRADA', 
        'BUENOS AIRES', 'CACHI', 'CAFAYATE', 'CAPITAL', 'CASEROS', 'CASTELLANOS', 'CERRILLOS', 'CHACABUCO', 
        'CHICOANA', 'CHOYA', 'COMANDANTE FERNANDEZ', 'CONCEPCIÓN', 'CONSTITUCIÓN', 'CORDOBA', 'CURUZÚ CUATIÁ', 
        'EMPEDRADO', 'ESQUINA', 'F. JUSTO STA MARIA DE ORO', 'FIGUEROA', 'FORMOSA', 'GARAY', 'GENERAL ALVEAR', 
        'GENERAL DONOVAN', 'GENERAL GÜEMES', 'GENERAL JOSE DE SAN MARTÍN', 'GENERAL LÓPEZ', 'GENERAL OBLIGADO (1)', 
        'GENERAL PAZ', 'GOYA', 'GRAL. BELGRANO', 'GRAL. GÜEMES', 'GUACHIPAS', 'GUASAYÁN', 'INDEPENDENCIA', 
        'IRIONDO', 'ITATÍ', 'ITUZAINGÓ', 'JIMÉNEZ', 'JUAN FELIPE IBARRA', 'LA CALDERA', 'LA CANDELARIA', 
        'LA CAPITAL', 'LA POMA', 'LA VIÑA', 'LAISHI', 'LAS COLONIAS', 'LAVALLE', 'LIBERTAD', 
        'LIBERTADOR GRAL. SAN MARTIN', 'LORETO', 'LOS ANDES', 'MAIPU', 'MATACOS', 'MAYOR LUIS FONTANA', 
        'MBURUCUYÁ', 'MERCEDES', 'METÁN', 'MOLINOS', 'MONTE CASEROS', 'MORENO', 'NUEVE DE JULIO', 'O` HIGGINS', 
        'OJO DE AGUA', 'ORÁN', 'PASO DE LOS LIBRES', 'PATIÑO', 'PCIA. DE LA PLAZA', 'PELLEGRINI', 'PILAGAS', 
        'PILCOMAYO', 'PIRANÉ', 'PRIMERO DE MAYO', 'QUITILIPI', 'RAMÓN LISTA', 'RIVADAVIA', 'RÍO HONDO', 
        'ROBLES', 'ROSARIO', 'ROSARIO DE LA FRONTERA', 'ROSARIO DE LERMA', 'SAENZ PEÑA', 'SALADAS', 'SAMUHU', 
        'SAN CARLOS', 'SAN COSME', 'SAN CRISTÓBAL', 'SAN FERNANDO', 'SAN JAVIER', 'SAN JERÓNIMO', 'SAN JUSTO', 
        'SAN LORENZO', 'SAN LUIS DEL PALMAR', 'SAN MARTIN', 'SAN MARTÍN', 'SAN MIGUEL', 'SAN ROQUE', 'SANTO TOMÉ', 
        'SARGENTO CABRAL', 'SARMIENTO', 'SAUCE', 'TAPENAGA', 'VERA'
    ].sort(),
    localidad: [
        'RESISTENCIA', 'BARRANQUERAS', 'FONTANA', 'PUERTO VILELAS', 'SAENZ PEÑA', 
        'VILLA ANGELA', 'CHARATA', 'JUAN JOSE CASTELLI', 'GENERAL SAN MARTIN', 
        'QUITILIPI', 'MACHAGAI', 'LAS BREÑAS', 'TRES ISLETAS', 'VILLA BERTHET',
        'PUERTO TIROL', 'GENERAL VEDIA', 'LA LEONESA', 'LAS PALMAS', 
        'ISLA DEL CERRITO', 'COLONIA BENITEZ', 'MARGARITA BELEN', 
        'COLONIA BARANDA', 'BASAIL', 'LAS GARCITAS', 'COLONIAS UNIDAS', 
        'COLONIA ELISA', 'LA VERDE', 'LAPACHITO', 'LA ESCONDIDA', 'MAKALLE', 
        'COTE-LAI', 'CHARADAI', 'TACO POZO', 'LOS FRENTONES', 'PAMPA DEL INFIERNO', 
        'CONCEPCION DEL BERMEJO', 'AVIA TERAI', 'NAPENAY', 'CAMPO LARGO', 
        'EL PALMAR', 'COLONIA ABORIGEN OESTE', 'COLONIA ABORIGEN ESTE', 
        'PCIA. DE LA PLAZA', 'LA TIGRA', 'LA CLOTILDE', 'SAN BERNARDO', 'SAMUHU', 
        'CORONEL DU GRATTY', 'PAMPA DEL INDIO', 'PRESIDENCIA ROCA', 'LAGUNA LIMPIA', 
        'CIERVO PETISO', 'PAMPA ALMIRON', 'SELVA RIO DE ORO', 'LA EDUVIGES', 
        'CORZUELA', 'EL SAUZALITO', 'EL SAUZAL', 'COMANDANCIA FRIAS', 
        'MISION NUEVA POMPEYA', 'FUERTE ESPERANZA', 'EL ESPINILLO', 
        'VILLA RIO BERMEJITO', 'MIRAFLORES', 'SANTA SYLVINA', 'CHOROTIS', 
        'PINEDO', 'GANCEDO', 'HERMOSO CAMPO', 'CAPITAN SOLARI', 'PUERTO BERMEJO'
    ].sort(),
    area: ['No Especificado', 'Zona Norte', 'Zona Sur'],
     obraSocial: [
        'Ninguna',        // <--- Tiene que existir y ser idéntico al Backend
        'INSSSEP', 
        'Plan Nacer', 
        'Plan Nacer -R', 
        'Profe', 
        'PAMI', 
        'Otra'
    ],
    vivienda: ['Casa', 'Departamento', 'Rancho', 'Otro'],
    agua: ['Red Publica', 'Pozo', 'Cisterna']
  };

  constructor(private http: HttpClient, private cdr: ChangeDetectorRef) {} 

  // --- FUNCIÓN CORREGIDA Y PROBADA PARA EL JSON REPORTADO ---
  buscar(tipo: 'dni' | 'hc') {
    const valor = tipo === 'dni' ? this.formData.dni : this.formData.hcNumber;
    
    if (!valor || valor.trim().length < 3) {
        return alert('Por favor, ingrese un número válido.');
    }

    this.loading = true;
    console.log(`🔎 Buscando: ${valor}...`);

    const url = `/api/HistoriaClinica/buscar-paciente?tipo=${tipo}&valor=${valor}`;

    this.http.get(url).subscribe({
      next: (response: any) => {
        console.log("📥 RESPUESTA SERVIDOR:", response);

        if (response) {
          this.isLocked = true;
          
          // ==============================================================
          // 1. REINICIAR EL FORMULARIO COMPLETO ANTES DE MAPEAR (LA SOLUCIÓN)
          // Esto limpia todos los campos del paciente anterior
          // ==============================================================
          this.formData = { ...initialSocialData }; 
          // ==============================================================

          let datosFuente: any = {};

          if (response.origen === 'local' && response.datos) {
              console.log("📂 Detectado registro LOCAL. Desempaquetando JSON...");
              try {
                  datosFuente = JSON.parse(response.datos);
                  this.formData.idPaciente = response.idHistoria; 
              } catch (err) { console.error("Error JSON local", err); }
          } 
          else {
              console.log("globe Detectado registro EXTERNO (Hospital).");
              datosFuente = response;
              this.formData.idPaciente = undefined; 
          }

          console.log("📂 FUENTE DE DATOS:", datosFuente);

          // ----------------------------------------------------
          // 2. MAPEO EXHAUSTIVO
          // ----------------------------------------------------

          // --- PERSONAL ---
          if (datosFuente.apellido || datosFuente.Apellido) {
              this.formData.apellido = datosFuente.apellido || datosFuente.Apellido;
              this.formData.nombres = datosFuente.nombres || datosFuente.Nombres;
          } else {
              const full = (datosFuente.nombreApellido || datosFuente.NombreApellido || '').trim();
              if (full) {
                  const p = full.split(' ');
                  this.formData.apellido = p.length > 1 ? p[0] : full;
                  this.formData.nombres = p.length > 1 ? p.slice(1).join(' ') : '';
              }
          }

          this.formData.dni = datosFuente.dni || datosFuente.Dni || this.formData.dni;
          this.formData.tipoDoc = datosFuente.tipoDoc || 'DNI';
          this.formData.fechaNacimiento = datosFuente.fechaNacimiento || datosFuente.FechaNacimiento || '';
          this.formData.sexo = datosFuente.sexo || datosFuente.Sexo || 'M';

          // --- UBICACIÓN (Nacionalidad, Provincia, Localidad) ---
          this.formData.direccion = datosFuente.direccion || datosFuente.Direccion || datosFuente.domicilio || '';
          
          const nac = datosFuente.nacionalidad || datosFuente.Nacionalidad || 'Argentina';
          this.formData.nacionalidad = nac.trim().toUpperCase() === 'ARGENTINA' ? 'Argentina' : nac.trim();

          // LOCALIDAD
          const loc = datosFuente.localidad || datosFuente.Localidad || 'RESISTENCIA';
          this.formData.localidad = loc.trim().toUpperCase(); 
          
          // === CAMBIOS CLAVES AQUÍ: Deducción Dinámica ===
          
          // 1. Usar el dato que manda el Backend, O deducirlo por la localidad encontrada.
          const dptoBackend = datosFuente.departamento || datosFuente.Departamento;
          if (dptoBackend) {
              this.formData.departamento = dptoBackend.trim().toUpperCase();
          } else {
              // Deducir desde nuestra lista
              this.formData.departamento = this.getDepartamentoPorLocalidad(this.formData.localidad);
          }

          // 2. Lo mismo para provincia
          const provBackend = datosFuente.provincia || datosFuente.Provincia;
          if (provBackend) {
              this.formData.provincia = provBackend.trim().toUpperCase();
          } else {
               // Deducir
               this.formData.provincia = this.getProvinciaPorDepartamento(this.formData.departamento);
          }

          // --- FAMILIARES (Padres) ---
          // Verifica estructura anidada "madre" vs plana "MadreNombre"
          const mad = datosFuente.madre || datosFuente.Madre || datosFuente; 
          this.formData.madreNombre = mad.nombre || mad.Nombre || mad.MadreNombre || '';
          this.formData.madreDoc = mad.dni || mad.Dni || mad.MadreDoc || '';

          const pad = datosFuente.padre || datosFuente.Padre || datosFuente; 
          this.formData.padreNombre = pad.nombre || pad.Nombre || pad.PadreNombre || '';
          this.formData.padreDoc = pad.dni || pad.Dni || pad.PadreDoc || '';

          // --- CONTACTO ---
          this.formData.telefono = datosFuente.telefono || datosFuente.Telefono || datosFuente.cel || '';
          this.formData.telefonoFijo = datosFuente.telefonoFijo || datosFuente.TelefonoFijo || '';
          this.formData.correo = datosFuente.correo || datosFuente.Correo || '';

          // --- OBRA SOCIAL ---
          // Manejo especial: A veces viene booleano (External) o string (Local)
         let osText = datosFuente.obraSocial || datosFuente.ObraSocial || 'Ninguna';
          
          // Si por alguna razón viene como número o booleano (compatibilidad vieja)
          if (osText === true) osText = 'INSSSEP';
          if (osText === false || osText === 0) osText = 'Ninguna';
          
          // Asignar al formData. IMPORTANTE: La propiedad debe llamarse igual que en el ngModel del HTML
          // Si en tu HTML usas [(ngModel)]="formData.obraSocial", asigna aquí a this.formData.obraSocial
          this.formData.obraSocial = osText;
              
          
          this.formData.nroObraSocial = datosFuente.nroObraSocial || datosFuente.NroObraSocial || '';
          this.formData.credential = datosFuente.credential || datosFuente.Credential || '';
          this.formData.titularNombre = datosFuente.titularNombre || datosFuente.TitularNombre || '';
          this.formData.titularDni = datosFuente.titularDni || datosFuente.TitularDni || '';

          // --- DATOS CLÍNICOS ---
          this.formData.areaProgramatica = datosFuente.areaProgramatica || datosFuente.AreaProgramatica || 'No Especificado';
          this.formData.alergias = datosFuente.alergias || datosFuente.Alergias || '';
          this.formData.observaciones = datosFuente.observaciones || datosFuente.Observaciones || '';

          // --- SOCIO-ECONÓMICOS ---
          // Se busca tanto con mayúscula como con minúscula
          this.formData.vivienda = datosFuente.vivienda || datosFuente.Vivienda || 'Casa';
          this.formData.luzElectrica = datosFuente.luzElectrica || datosFuente.LuzElectrica || 'No';
          this.formData.agua = datosFuente.agua || datosFuente.Agua || 'Red Publica';
          this.formData.basura = datosFuente.basura || datosFuente.Basura || 'No';
          this.formData.planSocial = datosFuente.planSocial || datosFuente.PlanSocial || 'No';
          
          // Numericos: parsear a entero o 0
          this.formData.numAmbientes = parseInt(datosFuente.numAmbientes || datosFuente.NumAmbientes || '0');
          // En DTO backend era PersonasHabitantes, en Form angular es 'habitantes'. Chequeamos ambos.
          this.formData.habitantes = parseInt(datosFuente.habitantes || datosFuente.personasHabitantes || datosFuente.PersonasHabitantes || '0');

          // ----------------------------------------------------
          // 3. ACTUALIZACIÓN VISUAL
          // ----------------------------------------------------
          console.log("✅ DATOS LISTOS PARA UI:", this.formData);
          this.cdr.detectChanges(); 

        } else {
            this.handleNotFound();
        }
        this.loading = false;
      },
      error: (e) => {
        if(e.status === 404) {
            this.handleNotFound();
        } else {
            console.error(e);
            alert('Error al buscar paciente.');
        }
        this.loading = false;
      }
    });
  }

  guardar() {
    if(!this.formData.apellido || !this.formData.dni) {
        return alert('Datos mínimos requeridos: Apellido y DNI.');
    }

    const url = `/ServicioSocial/guardar`;
    
    this.http.post(url, this.formData).subscribe({
        next: (res) => alert('Ficha Social guardada/actualizada correctamente.'),
        error: (err) => alert('Error al guardar: ' + err.message)
    });
  }

  limpiar() {
    if(confirm('¿Limpiar formulario?')) {
        this.formData = { ...initialSocialData };
        this.isLocked = false;
    }
  }
    handleNotFound() {
      this.isLocked = false; 
      this.formData.apellido = '';
      this.formData.nombres = '';
      this.formData.idPaciente = undefined;
      alert('Paciente no registrado. Habilitado para CARGA NUEVA.');
      this.cdr.detectChanges();
  }
  // Helper: Dado el nombre de una localidad, deduce su departamento
  private getDepartamentoPorLocalidad(localidad: string): string {
    const loc = localidad ? localidad.toUpperCase().trim() : '';

    // San Fernando
    if (['RESISTENCIA', 'BARRANQUERAS', 'FONTANA', 'PUERTO VILELAS', 'COLONIA BARANDA', 'BASAIL'].includes(loc)) return 'SAN FERNANDO';
    
    // Comandante Fernandez
    if (['SAENZ PEÑA'].includes(loc)) return 'COMANDANTE FERNANDEZ';
    
    // Mayor Luis Fontana
    if (['VILLA ANGELA', 'CORONEL DU GRATTY', 'ENRIQUE URIEN'].includes(loc)) return 'MAYOR LUIS FONTANA';
    
    // Chacabuco
    if (['CHARATA'].includes(loc)) return 'CHACABUCO';
    
    // General Güemes
    if (['JUAN JOSE CASTELLI', 'MIRAFLORES', 'VILLA RIO BERMEJITO', 'EL ESPINILLO', 'FUERTE ESPERANZA', 'MISION NUEVA POMPEYA', 'COMANDANCIA FRIAS', 'EL SAUZAL', 'EL SAUZALITO', 'WICHI', 'FORTIN LAVALLE', 'ZAPARINQUI', 'PUERTO LAVALLE'].includes(loc)) return 'GRAL. GÜEMES';
    
    // Libertador Gral San Martin
    if (['GENERAL SAN MARTIN', 'PAMPA DEL INDIO', 'PRESIDENCIA ROCA', 'LAGUNA LIMPIA', 'CIERVO PETISO', 'PAMPA ALMIRON', 'SELVA RIO DE ORO', 'LA EDUVIGES', 'CAMPO BERMEJO'].includes(loc)) return 'LIBERTADOR GRAL. SAN MARTIN';
    
    // Quitilipi
    if (['QUITILIPI', 'EL PALMAR', 'COLONIA ABORIGEN OESTE'].includes(loc)) return 'QUITILIPI';
    
    // 25 de Mayo
    if (['MACHAGAI', 'COLONIA ABORIGEN ESTE', 'NAPALPI'].includes(loc)) return '25 DE MAYO';
    
    // Almirante Brown
    if (['TACO POZO', 'LOS FRENTONES', 'PAMPA DEL INFIERNO', 'CONCEPCION DEL BERMEJO', 'RIO MUERTO'].includes(loc)) return 'ALMIRANTE BROWN';
    
    // Sargento Cabral
    if (['LAS GARCITAS', 'COLONIAS UNIDAS', 'CAPITAN SOLARI', 'COLONIA ELISA', 'INGENIERO BARBET'].includes(loc)) return 'SARGENTO CABRAL';
    
    // General Donovan
    if (['MAKALLE', 'LA VERDE', 'LAPACHITO', 'LA ESCONDIDA'].includes(loc)) return 'GENERAL DONOVAN';
    
    // Bermejo
    if (['LA LEONESA', 'LAS PALMAS', 'PUERTO BERMEJO', 'GENERAL VEDIA', 'ISLA DEL CERRITO', 'PUERTO EVA PERON'].includes(loc)) return 'BERMEJO';
    
    // 12 de Octubre
    if (['GENERAL PINEDO', 'GANCEDO', 'GENERAL CAPDEVILA', 'PAMPA LANDRIEL', 'MESON DE FIERRO', 'PINEDO'].includes(loc)) return '12 DE OCTUBRE';
    
    // Libertad
    if (['PUERTO TIROL', 'COLONIA POPULAR', 'LAGUNA BLANCA', 'ESTACION GRAL OBLIGADO'].includes(loc)) return 'LIBERTAD';
    
    // Independencia
    if (['AVIA TERAI', 'NAPENAY', 'CAMPO LARGO', 'FORTIN LAS CHUÑAS'].includes(loc)) return 'INDEPENDENCIA';
    
    // Tapenaga
    if (['COTE-LAI', 'CHARADAI', 'LA SABANA', 'HORQUILLA', 'HAUMONIA'].includes(loc)) return 'TAPENAGA';
    
    // Primero de Mayo
    if (['COLONIA BENITEZ', 'MARGARITA BELEN'].includes(loc)) return 'PRIMERO DE MAYO';
    
    // O' Higgins
    if (['SAN BERNARDO', 'LA CLOTILDE', 'LA TIGRA'].includes(loc)) return 'O` HIGGINS';
    
    // San Lorenzo
    if (['VILLA BERTHET', 'SAMUHU'].includes(loc)) return 'SAN LORENZO';
    
    // F. Justo Santa Maria de Oro
    if (['SANTA SYLVINA', 'CHOROTIS', 'VENADOS GRANDES'].includes(loc)) return 'F. JUSTO STA MARIA DE ORO';
    
    // Gral. Belgrano
    if (['CORZUELA'].includes(loc)) return 'GRAL. BELGRANO';
    
    // 9 de Julio
    if (['LAS BREÑAS'].includes(loc)) return '9 DE JULIO';
    
    // 2 de Abril
    if (['HERMOSO CAMPO', 'ITIN'].includes(loc)) return '2 DE ABRIL';
    
    // Maipu
    if (['TRES ISLETAS'].includes(loc)) return 'MAIPU';
    
    // Pcia de la Plaza
    if (['PCIA. DE LA PLAZA', 'PRESIDENCIA DE LA PLAZA'].includes(loc)) return 'PCIA. DE LA PLAZA';

    return 'SAN FERNANDO'; // Por defecto
  }

  // Helper para Provincia basado en el Departamento o Localidad
  private getProvinciaPorDepartamento(departamento: string): string {
     // Simplificación: Asumimos que todo lo de arriba es CHACO,
     // A menos que detectemos otra cosa de forma explícita si expandes la lógica.
     return 'CHACO'; 
  }
}