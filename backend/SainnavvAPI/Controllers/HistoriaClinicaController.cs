using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SainnavvAPI.Data;
using SainnavvAPI.Models;
using SainnavvAPI.Services;
using Newtonsoft.Json.Linq;
using System.Globalization;

namespace SainnavvAPI.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class HistoriaClinicaController : ControllerBase
    {
        // 1. DECLARACIÓN DE VARIABLES
        private readonly HistoriaRepository _repository;
        private readonly IWebHostEnvironment _env;
        private readonly IConfiguration _config;
        private readonly HospitalService _hospitalService;

        //  CONSTRUCTOR ( CONFIGURACIÓN )
        public HistoriaClinicaController(
           HistoriaRepository repository,
           IWebHostEnvironment env,
           IConfiguration config,
           HospitalService hospitalService) 
        {
            _repository = repository;
            _env = env;
            _config = config;
            _hospitalService = hospitalService;
        }

        [HttpGet("buscar-paciente")]
        public async Task<IActionResult> BuscarPaciente([FromQuery] string tipo, [FromQuery] string valor)
        {
            if (string.IsNullOrEmpty(valor)) return BadRequest("Debe enviar un valor.");

            // Variables de trabajo
            dynamic? datosBase = null;
            string idPacienteParaApi = "";
            string origenFinal = "";

            // =========================================================================
            // 1. LÓGICA DE PERSISTENCIA (Sainnavv)
            // =========================================================================
            if (tipo == "dni")
            {
                var historiaLocal = await _repository.ObtenerHistoriaExistente(valor);
                if (historiaLocal != null)
                {
                    return Ok(new
                    {
                        origen = "local",
                        idHistoria = historiaLocal.Value.Id,
                        datos = historiaLocal.Value.Json
                    });
                }
            }

            // =========================================================================
            // 2. BUSCAR DATOS BASE (Local SQL o Hospital API)
            // =========================================================================
            var localSql = await _repository.BuscarPacienteLocalCompleto(tipo, valor);

            if (localSql != null)
            {
                datosBase = localSql;
                idPacienteParaApi = localSql.GetType().GetProperty("idPaciente")?.GetValue(localSql, null)?.ToString() ?? "";
                origenFinal = "local_sql";
            }
            else
            {
                var pacExt = await _hospitalService.BuscarPaciente(tipo, valor);
                if (pacExt == null) return NotFound("Paciente no encontrado.");

                datosBase = pacExt;
                idPacienteParaApi = pacExt.Idpaciente ?? "";
                origenFinal = "hospital_api";
            }

            // =========================================================================
            // 3. OBTENER DATOS DE FAMILIARES Y FICHA (SIEMPRE DESDE API)
            // =========================================================================
            // Según tu solicitud: Estos datos deben venir de la API para estar actualizados
            FichaSocialExterna? ficha = await _hospitalService.ObtenerFichaSocial(idPacienteParaApi);
            var listaFamiliares = await _hospitalService.ObtenerDatosFamiliares(idPacienteParaApi);

            // Filtramos Progenitores: 2 = Madre, 1 = Padre
            var m = listaFamiliares.FirstOrDefault(f => f.Parentesco == 2);
            var p = listaFamiliares.FirstOrDefault(f => f.Parentesco == 1);

            // =========================================================================
            // 4. MAPEO Y TRADUCCIÓN (Fechas, DNIs y Códigos)
            // =========================================================================
            string FormatDni(object? d) => (d == null || d.ToString() == "0" || d.ToString() == "") ? "" : d.ToString()!;
            string BoolToText(string? val) => (val?.ToLower() == "true") ? "Si" : "No";

            // --- Procesar Fecha de Nacimiento ---
            string fNac = ""; string edadC = "";
            string rawFec = (origenFinal == "local_sql") ? datosBase.fechaNacimiento : (datosBase.Fechanac ?? "");

            if (!string.IsNullOrEmpty(rawFec))
            {
                string soloF = rawFec.Length >= 10 ? rawFec.Substring(0, 10) : rawFec;
                string[] formats = { "dd/MM/yyyy", "yyyy-MM-dd", "MM/dd/yyyy" };
                if (DateTime.TryParseExact(soloF, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dt))
                {
                    fNac = dt.ToString("yyyy-MM-dd");
                    int age = DateTime.Today.Year - dt.Year;
                    if (dt.Date > DateTime.Today.AddYears(-age)) age--;
                    edadC = age.ToString();
                }
            }

            int.TryParse(idPacienteParaApi, out int idPacInt);

            string ultimoResumen = "";
            string ultimoArchivo = "";
            string ultimaDescArchivo = "";
            string ultimaFechaAntecedente = ""; // <--- NUEVA VARIABLE
            string antecedenteEstado = "sin_antecedentes";

            if (idPacInt > 0)
            {
                var ultAnt = await _repository.ObtenerUltimoAntecedente(idPacInt);
                if (ultAnt != null) // Quitamos la comprobación de texto para que cargue la fecha aunque no haya texto
                {
                    ultimoResumen = ultAnt.Value.Detalle;
                    ultimoArchivo = ultAnt.Value.Archivo;
                    ultimaDescArchivo = ultAnt.Value.Direccion;
                    ultimaFechaAntecedente = ultAnt.Value.Fecha; // <--- CAPTURAR LA FECHA DE LA TUPLA
                    antecedenteEstado = "con_antecedentes";
                }
            }

            // =========================================================================
            // 5. ARMADO DEL JSON UNIFICADO (Sainnavv + Social)
            // =========================================================================
            var respuestaFinal = new
            {
                origen = origenFinal,
                idPaciente = int.TryParse(idPacienteParaApi, out int pid) ? pid : 0,
                nombreApellido = (origenFinal == "local_sql") ? datosBase.nombreApellido : $"{datosBase.Apellido} {datosBase.Nombre}".Trim(),
                dni = (origenFinal == "local_sql") ? datosBase.dni : datosBase.Numerodocumento,
                fechaNacimiento = fNac,
                edad = edadC,
                sexo = (origenFinal == "local_sql") ? datosBase.sexo : (datosBase.Sexo ?? ""),
                domicilio = ficha?.Direccion ?? (origenFinal == "local_sql" ? datosBase.domicilio : ""),

                // Obra Social (Traducida de local o texto de API)
                obraSocial = (origenFinal == "local_sql") ? datosBase.obraSocial : "Ninguna",
                nroObraSocial = (origenFinal == "local_sql") ? datosBase.nroObraSocial : "",

                // DATOS DE PADRES (Prioridad absoluta a la API de familiares para el DNI)
                madre = new
                {
                    nombre = m?.ApeyNom ?? (origenFinal == "local_sql" ? datosBase.madre.nombre : datosBase.Nombremadre) ?? "",
                    dni = FormatDni(m?.Documento), // Aquí viene el 30551274 de la API
                    edad = m?.Edad ?? "",
                    nivelInstruccion = MapEscolaridad(m?.Escolaridad ?? 0),
                    ocupacion = m?.OcupacionObs ?? "",
                    domicilio = ficha?.Direccion ?? ""
                },
                padre = new
                {
                    nombre = p?.ApeyNom ?? (origenFinal == "local_sql" ? datosBase.padre.nombre : datosBase.Nombrepadre) ?? "",
                    dni = FormatDni(p?.Documento), // Aquí viene el 34567714 de la API
                    edad = p?.Edad ?? "",
                    nivelInstruccion = MapEscolaridad(p?.Escolaridad ?? 0),
                    ocupacion = p?.OcupacionObs ?? "",
                    domicilio = ficha?.Direccion ?? ""
                },

                // --- ENVIAR LOS ANTECEDENTES RECUPERADOS ---
                antecedentesServicio = antecedenteEstado,
                resumenIntervencionesPrevias = ultimoResumen,
                archivoAntecedentes = ultimoArchivo,
                descripcionArchivo = ultimaDescArchivo,
                fechaAntecedente = ultimaFechaAntecedente,
                // Datos Socio-Económicos (Campos para Servicio Social)
                areaProgramatica = (origenFinal == "local_sql") ? datosBase.areaProgramatica : "No Especificado",
                vivienda = (origenFinal == "local_sql") ? datosBase.vivienda : "Casa",
                luzElectrica = (origenFinal == "local_sql") ? datosBase.luzElectrica : BoolToText(ficha?.LuzElectrica),
                agua = (origenFinal == "local_sql") ? datosBase.agua : "Red Publica",
                basura = (origenFinal == "local_sql") ? datosBase.basura : BoolToText(ficha?.Basura),
                planSocial = (origenFinal == "local_sql") ? datosBase.planSocial : BoolToText(ficha?.AyudaSocial),
                numAmbientes = (origenFinal == "local_sql") ? datosBase.numAmbientes : (int.TryParse(ficha?.NroAmbientes, out int na) ? na : 0),
                habitantes = (origenFinal == "local_sql") ? datosBase.habitantes : (int.TryParse(ficha?.PersHabitan, out int ph) ? ph : 0),
                alergias = ficha?.Alergias ?? (origenFinal == "local_sql" ? datosBase.alergias : ""),
                observaciones = ficha?.Observaciones ?? (origenFinal == "local_sql" ? datosBase.observaciones : ""),
                correo = ficha?.Correo ?? (origenFinal == "local_sql" ? datosBase.correo : ""),
                telefonoCelular = ficha?.Celular ?? (origenFinal == "local_sql" ? datosBase.telefonoCelular : ""),
                telefonoFijo = ficha?.TelefonoFijo ?? (origenFinal == "local_sql" ? datosBase.telefonoFijo : "")
            };

            return Ok(respuestaFinal);
        }

        private string MapEscolaridad(int cod)
        {
            return cod switch
            {
                1 => "1ria Incomp.",
                2 => "1ria Comp.",
                3 => "2ria Incomp.",
                4 => "2ria Comp.",
                5 => "3ria Incomp.",
                6 => "3ria Comp.",
                7 => "Univ.Incomp.",
                8 => "Univ.Completo",
                9 => "Analfabeto",
                10 => "Nunca Asistio",
                _ => "Otro"
            };
        }

        private string MapOcupacionSocial(int cod)
        {
            return cod switch
            {
                1 => "Independiente",
                2 => "Dependiente",
                3 => "Jornalero",
                4 => "Desocupado",
                _ => "Otro"
            };
        }



        [HttpPost]
        public async Task<IActionResult> Guardar([FromForm] HistoriaClinicaRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.JsonData))
                    return BadRequest("No se recibieron datos del formulario.");

                var formData = JsonConvert.DeserializeObject<SainnavvFormDataDto>(request.JsonData);
                if (formData == null) return BadRequest("Error al deserializar los datos.");

                int idUsuarioActual = 1; // ID de ejemplo de tu sesión

                // Obtener la carpeta de red configurada
                string pathConfig = _config["RutaArchivos"];
                string folder = string.IsNullOrWhiteSpace(pathConfig)
                                ? Path.Combine(_env.WebRootPath ?? "wwwroot", "uploads")
                                : pathConfig;

                string? pathAnt = null;
                string? pathClinica = null;

                // Guardamos los archivos con su nombre original
                if (request.ArchivoAntecedentes != null)
                {
                    pathAnt = await GuardarArchivoEnDisco(request.ArchivoAntecedentes);

                    // Guardamos la DIRECCIÓN de red original en la propiedad que va a la columna @Direccion
                    formData.descripcionArchivo = folder;
                    formData.archivoAntecedentes = pathAnt; // Nombre original
                }

                if (request.ArchivoClinica != null)
                {
                    pathClinica = await GuardarArchivoEnDisco(request.ArchivoClinica);
                    // Si el formulario médico tuviera columna dirección para clínica, la guardarías aquí
                    formData.descripcionAtencionMedica = pathClinica;
                }

                // Guardar los adjuntos extras (Paso 6) con su nombre original
                if (Request.Form.Files.Count > 0)
                {
                    await GuardarAdjuntosExtras(Request.Form.Files);
                }

                // Guardar relacional
                int nuevoIdEpisodio = await _repository.GuardarSainnavvRelacional(formData, idUsuarioActual);

                return Ok(new { mensaje = "Guardado exitoso en tablas relacionales", episodio = nuevoIdEpisodio });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al guardar historia: {ex.Message}");
                return StatusCode(500, $"Error interno al procesar el guardado: {ex.Message}");
            }
        }

        // Guardador de adjuntos extras (Paso 6) - Máximo 50 caracteres
        private async Task GuardarAdjuntosExtras(IFormFileCollection files)
        {
            string pathConfig = _config["RutaArchivos"];
            string folder = string.IsNullOrWhiteSpace(pathConfig)
                            ? Path.Combine(_env.WebRootPath ?? "wwwroot", "uploads")
                            : pathConfig;

            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

            foreach (var file in files)
            {
                if (file.Name.StartsWith("Adjunto_"))
                {
                    try
                    {
                        string categoria = file.Name.Replace("Adjunto_", ""); // ej: "Fotografias"

                        // Generamos prefijo ultra corto de 6 caracteres para dejar más espacio al nombre del PDF
                        string shortGuid = Guid.NewGuid().ToString().Substring(0, 6);

                        string originalName = file.FileName;
                        // 50 - (6 del guid + 1 del '_' + longitud de la categoría + 1 del '_')
                        int espacioDisponible = 50 - (8 + categoria.Length);

                        if (originalName.Length > espacioDisponible)
                        {
                            string ext = Path.GetExtension(originalName);
                            string nameWithoutExt = Path.GetFileNameWithoutExtension(originalName);
                            originalName = nameWithoutExt.Substring(0, espacioDisponible - ext.Length) + ext;
                        }

                        string uniqueName = $"{shortGuid}_{categoria}_{originalName}";
                        string filePath = Path.Combine(folder, uniqueName);

                        Console.WriteLine($"--> Guardando adjunto protegido: {filePath} (Longitud: {uniqueName.Length})");

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[ERROR EN ADJUNTO] {file.FileName}: {ex.Message}");
                    }
                }
            }
        }




        [HttpGet("intervenciones/{idPaciente}")]
        public async Task<IActionResult> GetIntervenciones(int idPaciente)
        {
            if (idPaciente <= 0) return BadRequest("ID de paciente no válido.");

            var historial = await _repository.ObtenerHistorialIntervenciones(idPaciente);
            return Ok(historial);
        }

        // Endpoint para Angular: /HistoriaClinica/profesionales-servicio
        [HttpGet("profesionales-servicio")]
        public async Task<IActionResult> GetProfesionalesServicio()
        {
            // Siempre buscamos los profesionales del servicio 100 (Sainnavv)
            var lista = await _hospitalService.ObtenerProfesionalesServicio(100);
            return Ok(lista);
        }

        // Guarda el archivo con su NOMBRE ORIGINAL exacto
        private async Task<string> GuardarArchivoEnDisco(IFormFile archivo)
    {
        string pathConfig = _config["RutaArchivos"];
        string folder = string.IsNullOrWhiteSpace(pathConfig)
                        ? Path.Combine(_env.WebRootPath ?? "wwwroot", "uploads")
                        : pathConfig;

        if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

        // 1. Tomamos estrictamente el NOMBRE ORIGINAL
        string originalName = archivo.FileName;

        // 2. Control de seguridad: Si supera los 50 caracteres (límite de tu SQL),
        // lo recortamos conservando la extensión para que no falle la transacción.
        if (originalName.Length > 50)
        {
            string ext = Path.GetExtension(originalName); // .pdf
            string nameWithoutExt = Path.GetFileNameWithoutExtension(originalName);
            originalName = nameWithoutExt.Substring(0, 50 - ext.Length) + ext;
        }

        string filePath = Path.Combine(folder, originalName);

        // 3. Guardar físico en la red o local
        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await archivo.CopyToAsync(stream);
        }

        // Retornamos el nombre original (máximo de 50 caracteres)
        return originalName;
    }

        // Descarga y visualiza usando la dirección original
        [HttpGet("descargar-pdf")]
        public IActionResult DescargarPdf([FromQuery] string nombre)
        {
            if (string.IsNullOrEmpty(nombre)) return BadRequest("Nombre de archivo requerido.");

            string pathConfig = _config["RutaArchivos"];
            string folder = string.IsNullOrWhiteSpace(pathConfig)
                            ? Path.Combine(_env.WebRootPath ?? "wwwroot", "uploads")
                            : pathConfig;

            string nombreLimpio = Uri.UnescapeDataString(nombre);
            string filePath = Path.Combine(folder, nombreLimpio);

            // Si por alguna razón la BD tiene guardada la ruta absoluta directamente
            if (!System.IO.File.Exists(filePath))
            {
                if (System.IO.File.Exists(nombreLimpio))
                {
                    filePath = nombreLimpio;
                }
                else
                {
                    return NotFound($"El archivo PDF no existe en la dirección original: {filePath}");
                }
            }

            var bytes = System.IO.File.ReadAllBytes(filePath);
            return File(bytes, "application/pdf", Path.GetFileName(filePath));
        }



    }
}