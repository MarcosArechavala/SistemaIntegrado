using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SainnavvAPI.Data;
using SainnavvAPI.Models;
using SainnavvAPI.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SainnavvAPI.Controllers
{
	[Route("[controller]")]
	[ApiController]
	public class ServicioSocialController : ControllerBase
	{
        private readonly HistoriaRepository _repository;
        private readonly HospitalService _hospitalService;// INYECTADO CORRECTAMENTE

        public ServicioSocialController(HistoriaRepository repository, HospitalService hospitalService)
        {
            _repository = repository;
            _hospitalService = hospitalService;
        }

        [HttpPost("guardar")]
        public async Task<IActionResult> Guardar([FromBody] SocialFormDataDto data)
        {
            int idUsuario = 1; // ID de ejemplo de usuario

            try
            {
                // 1. Guardar localmente en tu base de datos SQL (AHS)
                // Esto guarda Paciente, Ficha y recorre los familiares en tu repositorio sin SQL en el controller
                int idPaciente = await _repository.GestionarPacienteSocial(data, idUsuario);
                await _repository.GuardarFichaCompleta(idPaciente, data, idUsuario);

                // =========================================================================
                // 2. SINCRONIZACI�N DIN�MICA POR "PUT/POST" AL SERVIDOR CENTRAL DEL HOSPITAL
                // =========================================================================

                // A. Mapeo para la Ficha (InsUp_PacienteFicha)
                string area = data.AreaProgramatica ?? "";
                int idArea = area.Contains("Sur") ? 1 : 123;

                string viv = data.Vivienda ?? "";
                int idViv = viv.Contains("Casa") ? 1 : (viv.Contains("Departamento") ? 2 : 10);

                string agua = data.Agua ?? "";
                int idAgua = agua.Contains("Red") ? 1 : 7;

                var payloadFicha = new
                {
                    iDpaciente = idPaciente,
                    nacionalidad = data.Nacionalidad == "Argentina" ? 1 : 200,
                    docPadre = int.TryParse(data.PadreDoc, out int dp) ? dp : 0,
                    docMadre = int.TryParse(data.MadreDoc, out int dm) ? dm : 0,
                    telCel = data.Telefono ?? "",
                    telFijo = data.TelefonoFijo ?? "",
                    areaProg = idArea,
                    viviendaTipo = idViv,
                    nroAmbientes = data.NumAmbientes,
                    persHabitan = data.PersonasHabitantes,
                    luzElectrica = data.GetBool(data.LuzElectrica) ? 1 : 0,
                    agua = idAgua,
                    aguaUbicacion = 0,
                    basura = data.GetBool(data.Basura) ? 1 : 0,
                    ayudaSocial = data.GetBool(data.PlanSocial) ? 1 : 0,
                    observaciones = data.Observaciones ?? "",
                    idusuario = idUsuario.ToString(),
                    fechaCarga = DateTime.Now.ToString("yyyy-MM-dd"),
                    alergias = data.Alergias ?? "",
                    correo = data.Correo ?? "",
                    obsMod = "Actualizacion Ficha Web (.NET 8)"
                };

                // Enviamos el PUT de la Ficha
                await _hospitalService.ActualizarFichaCentral(payloadFicha);


                // B. Mapeo para la Obra Social (InsUp_PacienteOS)
                int idOS = 0;
                string os = data.ObraSocial ?? "";
                if (os.Contains("INSSSEP")) idOS = 999;
                else if (os.Contains("Plan Nacer -R")) idOS = 90;
                else if (os.Contains("Plan Nacer")) idOS = 9;
                else if (os.Contains("Profe")) idOS = 99;
                else if (os.Contains("PAMI")) idOS = 12;

                var payloadOS = new
                {
                    idPaciente = idPaciente,
                    idos = idOS,
                    nroOS = data.NroObraSocial ?? "",
                    credencial = data.credential ?? "",
                    titular = data.titularNombre ?? "",
                    dniTitular = int.TryParse(data.titularDni?.ToString(), out int dt) ? dt : 0,
                    idUsuario = idUsuario,
                    ultimaactualizacion = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                };

                // Enviamos el PUT de Obra Social
                await _hospitalService.ActualizarObraSocialCentral(payloadOS);


                // C. Mapeo para los Familiares Grilla (Ins_PacienteFamiliares por POST)
                if (data.Convivientes != null)
                {
                    foreach (var fam in data.Convivientes)
                    {
                        var payloadFamiliar = new
                        {
                            idFlia = fam.IdFila ?? 0,
                            idpaciente = idPaciente,
                            apeyNom = fam.Nombre ?? "",
                            documento = int.TryParse(fam.Documento, out int doc) ? doc : 0,
                            parentesco = MapParentescoTexto(fam.Vinculo),
                            edad = int.TryParse(fam.Edad, out int ed) ? ed : 0,
                            estadoCivil = 0,
                            
                            ocupacion = MapOcupacionTexto(fam.Ocupacion),
                            ocupacionObs = fam.Observaciones ?? "",
                            ingresos = fam.Ingresos
                        };

                        await _hospitalService.GuardarFamiliarCentral(payloadFamiliar);
                    }
                }

                return Ok(new { message = "Ficha guardada localmente y sincronizada con �xito" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Error en guardado: " + ex.Message);
            }
        }

        // --- TRADUCTORES DE TEXTO A ID PARA LA API DEL HOSPITAL ---
        private int MapParentescoTexto(string? v)
        {
            if (string.IsNullOrEmpty(v)) return 0;
            string val = v.ToLower().Trim();
            if (val.Contains("padre")) return 1;
            if (val.Contains("madre")) return 2;
            if (val.Contains("tio") || val.Contains("tia")) return 3;
            if (val.Contains("tutor")) return 4;
            if (val.Contains("herman")) return 5;
            if (val.Contains("abuel")) return 6;
            return 7; // Otros
        }

        private int MapEscolaridadTexto(string? v)
        {
            if (string.IsNullOrEmpty(v)) return 0;
            string val = v.ToLower();
            if (val.Contains("1ria incomp")) return 1;
            if (val.Contains("1ria comp")) return 2;
            if (val.Contains("2ria incomp")) return 3;
            if (val.Contains("2ria comp")) return 4;
            if (val.Contains("3ria incomp")) return 5;
            if (val.Contains("3ria comp")) return 6;
            if (val.Contains("univ.incomp")) return 7;
            if (val.Contains("univ.completo")) return 8;
            return 0;
        }

        private int MapOcupacionTexto(string? v)
        {
            if (string.IsNullOrEmpty(v)) return 0;
            string val = v.ToLower();
            if (val.Contains("independiente")) return 1;
            if (val.Contains("dependiente")) return 2;
            if (val.Contains("jornalero")) return 3;
            if (val.Contains("desocupado")) return 4;
            return 0;
        }
    }
}