using Microsoft.AspNetCore.Mvc;
using SainnavvAPI.Services;
using Newtonsoft.Json;

namespace SainnavvAPI.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly HospitalService _hospitalService;

        public AuthController(HospitalService hospitalService)
        {
            _hospitalService = hospitalService;
        }

        // Definimos una clase simple para recibir de Angular
        public class LoginRequestAngular
        {
            public string Usuario { get; set; }
            public string Clave { get; set; }
        }
        [HttpPost("iniciar-sesion")]
        public async Task<IActionResult> IniciarSesion([FromBody] LoginRequestAngular request)
        {

            
            // 1. Intentar login
            var dataUsuarioStr = await _hospitalService.LoginPersonalHospital(request.Usuario, request.Clave);

            if (dataUsuarioStr != null)
            {
                // 2. IMPORTANTE: Parseamos la respuesta del hospital para sacar el 'result'
                // Según tu ejemplo, el hospital devuelve: {"result": 1364, "data": "...", "message": "..."}
                dynamic resHospital = JsonConvert.DeserializeObject(dataUsuarioStr);

                // Extraemos el código numérico (ej: 1364)
                string codigoResult = resHospital.result.ToString();

                // 3. Buscamos los permisos usando ese código obtenido en el login
                var listaPermisos = await _hospitalService.ObtenerPermisosUsuario(codigoResult);

                // 4. Respondemos a Angular con una estructura limpia
                return Ok(new
                {
                    token = dataUsuarioStr,
                    codigoUsuario = codigoResult, // Mandamos el 1364 aquí
                    permisos = listaPermisos     // Mandamos la lista (aunque venga [])
                });
            }

            return Unauthorized(new { mensaje = "Usuario o contraseña incorrectos" });
        }
    }
}