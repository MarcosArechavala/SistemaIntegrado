export interface SocialFormData {
  // Identificación interna
  idPaciente?: number; 
  idFicha?: number;

  // Búsqueda y Datos Principales
  tipoDoc: string;
  dni: string;
  hcNumber: string;
  hcType: 'Auto' | 'Libre';
  
  // Personales
  apellido: string;
  nombres: string;
  sexo: string;
  fechaNacimiento: string;
  
  // Ubicación
  nacionalidad: string;
  provincia: string;
  departamento: string;
  localidad: string;
  direccion: string;
  
  // Contacto
  telefono: string;
  telefonoFijo: string;
  correo: string;
  
  // Familiares (Datos fijos solicitados)
  madreNombre: string;
  madreDoc: string;
  padreNombre: string;
  padreDoc: string;
  
  // Clínicos y Social
  areaProgramatica: string;
  alergias: string;
  credential: string;
  titularNombre:string;
  titularDni:number;
  observaciones: string;
  obraSocial: string;
  nroObraSocial: string;
  
  // Socioeconómicos
  planSocial: string; // 'Si' | 'No'
  vivienda: string;
  numAmbientes: number;
  habitantes: number;
  luzElectrica: string;
  agua: string;
  basura: string;
}

// Valores iniciales para limpiar formulario
export const initialSocialData: SocialFormData = {
  tipoDoc: 'DNI', dni: '', hcNumber: '', hcType: 'Auto',
  apellido: '', nombres: '', sexo: 'M', fechaNacimiento: '',
  nacionalidad: 'Argentina', provincia: 'CHACO', departamento: 'SAN FERNANDO', localidad: 'RESISTENCIA', direccion: '',
  telefono: '', telefonoFijo: '', correo: '',
  madreNombre: '', madreDoc: '', padreNombre: '', padreDoc: '',
  areaProgramatica: 'No Especificado', alergias: '', observaciones: '', obraSocial: 'Ninguna', nroObraSocial: '',credential: '', titularNombre:'', titularDni:0,
  planSocial: 'No', vivienda: 'Casa', numAmbientes: 0, habitantes: 0, luzElectrica: 'Si', agua: 'Red Publica', basura: 'Si'
};