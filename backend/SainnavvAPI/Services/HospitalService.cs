using Newtonsoft.Json;
using System.Text;
using System.Net.Http.Headers;
using SainnavvAPI.Models;

namespace SainnavvAPI.Services
{
    public class HospitalService
    {
        private readonly HttpClient _http;
        private readonly IConfiguration _config;
        private static string? _tokenCache;

        public HospitalService(HttpClient http, IConfiguration config)
        {
            _config = config;
            _http = http; // <--- Inyección

            // Configurar base aquí
            string baseUrl = _config["HospitalApi:BaseUrl"] ?? "http://10.4.50.38/SI_Apis/api/";
            if (!baseUrl.EndsWith("/")) baseUrl += "/";
            _http.BaseAddress = new Uri(baseUrl);
        }

        private async Task<string> GetToken()
        {
            if (!string.IsNullOrEmpty(_tokenCache)) return _tokenCache;

            
            _http.DefaultRequestHeaders.Authorization = null;

            var loginData = new
            {
                Username = _config["HospitalApi:Usuario"],
                Password = _config["HospitalApi:Password"]
            };

            Console.WriteLine($"[HospitalService] Intentando autenticar con: {loginData.Username} en {_http.BaseAddress}auth/login");

            try
            {
                var jsonContent = JsonConvert.SerializeObject(loginData);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var response = await _http.PostAsync("auth/login", content);

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"[Error Auth] Código: {response.StatusCode}");
                    return "";
                }

                var responseString = await response.Content.ReadAsStringAsync();

                // Intentar leer token
                try
                {
                    dynamic? obj = JsonConvert.DeserializeObject(responseString);
                    // Adaptarse a posibles respuestas: "token" o directo string
                    string token = obj?.token?.ToString() ?? responseString.Trim('"');
                    _tokenCache = token;
                    Console.WriteLine("[HospitalService] Token obtenido correctamente.");
                }
                catch
                {
                    _tokenCache = responseString.Trim('"');
                }

                return _tokenCache ?? "";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Error Grave Auth] {ex.Message}");
                throw;
            }
        }


        // MÉTODO NUEVO PARA LOGUEAR MÉDICOS
        public async Task<string?> LoginPersonalHospital(string usuario, string clave)
        {
            try
            {
                // =========================================================================
                // 1. OBTENER EL TOKEN DEL SISTEMA PRIMERO
                // Como 'api/login' es una ruta protegida, necesitamos el token de acceso
                // =========================================================================
                var token = await GetToken();
                if (string.IsNullOrEmpty(token))
                {
                    Console.WriteLine("[DEBUG LOGIN] No se pudo obtener el token de autorización del sistema.");
                    return null;
                }

                // 2. Configurar la cabecera con el token obtenido
                _http.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                // =========================================================================

                var payload = new
                {
                    usuario = usuario,
                    clave = clave,
                    respuesta = 0
                };

                var jsonContent = JsonConvert.SerializeObject(payload);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                string endpoint = "login";
                string urlCompleta = $"{_http.BaseAddress}{endpoint}";

                Console.WriteLine("------------------------------------------------");
                Console.WriteLine($"[DEBUG LOGIN] Llamando a la API del Hospital (Protegida): {urlCompleta}");
                Console.WriteLine($"[DEBUG LOGIN] Datos enviados: {jsonContent}");

                var response = await _http.PostAsync(endpoint, content);

                Console.WriteLine($"[DEBUG LOGIN] Respuesta de la API Hospitalaria: {(int)response.StatusCode} {response.StatusCode}");

                if (response.IsSuccessStatusCode)
                {
                    var responseBody = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"[DEBUG LOGIN] ÉXITO: {responseBody}");
                    return responseBody;
                }
                else
                {
                    var errorBody = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"[DEBUG LOGIN] ERROR DETALLADO: {errorBody}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[ERROR GRAVE LOGIN]: {ex.Message}");
                Console.ResetColor();
                return null;
            }
        }

        public async Task<List<dynamic>> ObtenerPermisosUsuario(string codigoUsuario)
        {
            try
            {
                var token = await GetToken();
                _http.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                // URL proporcionada: .../buscarpermisosdelusuario?CodigoUsuario=79
                var response = await _http.GetAsync($"Consultas/buscarpermisosdelusuario?CodigoUsuario={codigoUsuario}");

                if (!response.IsSuccessStatusCode) return new List<dynamic>();

                var jsonStr = await response.Content.ReadAsStringAsync();

                // Deserializamos como lista dinámica para leer "accesos" fácilmente
                var permisos = JsonConvert.DeserializeObject<List<dynamic>>(jsonStr);

                return permisos ?? new List<dynamic>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Error al recuperar permisos]: {ex.Message}");
                return new List<dynamic>();
            }
        }

        public async Task<PacienteExterno?> BuscarPaciente(string tipoBusqueda, string valor)
        {
            try
            {
                var token = await GetToken();
                if (string.IsNullOrEmpty(token)) return null;

                _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                string endpoint;
                if (tipoBusqueda == "hc")
                {
                    endpoint = $"Consultas/DatosBasicosPacientesPorHC?hc={valor}";
                }
                else
                {
                    // Atención: API dice parametro Nrodocumento
                    endpoint = $"Consultas/DatosBasicosPacientesPorDocumento?Nrodocumento={valor}";
                }

                Console.WriteLine($"[HospitalService] Consultando: {_http.BaseAddress}{endpoint}");

                var response = await _http.GetAsync(endpoint);

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"[Error API Externa] Código: {response.StatusCode}");
                    return null;
                }

                var jsonResponse = await response.Content.ReadAsStringAsync();
                // Console.WriteLine($"[Respuesta Raw]: {jsonResponse}"); // Descomentar si sigue fallando para ver qué llega

                // Deserializar lista
                var listaPacientes = JsonConvert.DeserializeObject<List<PacienteExterno>>(jsonResponse);

                if (listaPacientes != null && listaPacientes.Count > 0)
                {
                    Console.WriteLine($"[Éxito] Paciente encontrado: {listaPacientes[0].Apellido}");
                    return listaPacientes[0];
                }
                else
                {
                    Console.WriteLine("[HospitalService] La API respondió 200 OK pero la lista está vacía.");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Excepción HospitalService]: {ex.Message}");
                return null;
            }

        }
        // Método nuevo para obtener detalles con ID
        public async Task<FichaPacienteExterno?> ObtenerFicha(string idPaciente)
        {
            try
            {
                var token = await GetToken();
                _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                // Endpoint nuevo
                string endpoint = $"Consultas/DatosBasicosFichaGFPacientes?idpaciente={idPaciente}";

                var response = await _http.GetAsync(endpoint);

                if (!response.IsSuccessStatusCode) return null;

                var jsonResponse = await response.Content.ReadAsStringAsync();

                // Como devuelve un array [], deserializamos lista y tomamos el primero
                var listaFicha = JsonConvert.DeserializeObject<List<FichaPacienteExterno>>(jsonResponse);

                return listaFicha?.FirstOrDefault();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[HospitalService - Ficha] Error: {ex.Message}");
                return null; // Si falla, no rompe todo, solo faltarán datos extra
            }
        }
        public async Task<FichaSocialExterna?> ObtenerFichaSocial(string idPaciente)
        {
            try
            {
                var token = await GetToken(); // Asume que tienes el método GetToken
                _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                // Llamada a la API que trae detalles por ID
                string endpoint = $"Consultas/DatosBasicosFichaPacientes?IdPaciente={idPaciente}";

                var response = await _http.GetAsync(endpoint);
                if (!response.IsSuccessStatusCode) return null;

                var jsonStr = await response.Content.ReadAsStringAsync();

                // Intentamos deserializar lista
                var lista = JsonConvert.DeserializeObject<List<FichaSocialExterna>>(jsonStr);
                return lista?.FirstOrDefault();
            }
            catch
            {
                return null;
            }
        }

        public async Task<List<FamiliarExterno>> ObtenerDatosFamiliares(string idPaciente)
        {
            try
            {
                var token = await GetToken();
                _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                // Nueva URL: Consultas/DatosFamiliaresDelPaciente
                string endpoint = $"Consultas/DatosFamiliaresDelPaciente?IdPaciente={idPaciente}";
                var response = await _http.GetAsync(endpoint);

                if (!response.IsSuccessStatusCode) return new List<FamiliarExterno>();

                var jsonStr = await response.Content.ReadAsStringAsync();

                // Deserializamos el Array de familiares
                return JsonConvert.DeserializeObject<List<FamiliarExterno>>(jsonStr) ?? new List<FamiliarExterno>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Error API Familiares]: {ex.Message}");
                return new List<FamiliarExterno>();
            }
        }

        // 1. PUT: Actualizar Ficha Completa en Servidor Central
        public async Task<string> ActualizarFichaCentral(object payloadFicha)
        {
            try
            {
                var token = await GetToken();
                _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var json = JsonConvert.SerializeObject(payloadFicha);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // Método PUT
                var response = await _http.PutAsync("Pacientes/InsUp_PacienteFicha", content);

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsStringAsync();
                }

                var errBody = await response.Content.ReadAsStringAsync();
                return $"Error Central: {response.StatusCode} - {errBody}";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Error PUT Ficha Central]: {ex.Message}");
                return $"Exception: {ex.Message}";
            }
        }

        // 2. PUT: Actualizar Obra Social en Servidor Central
        public async Task<string> ActualizarObraSocialCentral(object payloadOS)
        {
            try
            {
                var token = await GetToken();
                _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var json = JsonConvert.SerializeObject(payloadOS);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // Método PUT
                var response = await _http.PutAsync("Pacientes/InsUp_PacienteOS", content);

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsStringAsync();
                }

                var errBody = await response.Content.ReadAsStringAsync();
                return $"Error Central OS: {response.StatusCode} - {errBody}";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Error PUT OS Central]: {ex.Message}");
                return $"Exception: {ex.Message}";
            }
        }

        // =========================================================================
        // MÉTODO POST: Insertar/Modificar Familiar en Servidor Central
        // =========================================================================
        public async Task<string> GuardarFamiliarCentral(object familiarPayload)
        {
            try
            {
                // 1. Obtener el token de autorización
                var token = await GetToken();
                _http.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                // 2. Serializar el payload del familiar
                var jsonContent = JsonConvert.SerializeObject(familiarPayload);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                // 3. Enviar la petición POST al endpoint del hospital
                var response = await _http.PostAsync("Pacientes/Ins_PacienteFamiliares", content);
                response.EnsureSuccessStatusCode();

                // 4. Leer la respuesta del hospital (ej: "Registro agregado con id: 98247")
                var resultStr = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[Hospital Central - Familiar] Exito: {resultStr}");

                return resultStr;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Error POST Familiar Central]: {ex.Message}");
                return $"Error: {ex.Message}";
            }
        }

        // =========================================================================
        // GET: Buscar Profesional por ID en el Servidor Central
        // =========================================================================
        // GET: Obtener profesionales del servicio (Sainnavv = 100)
        public async Task<List<dynamic>> ObtenerProfesionalesServicio(int idServicio)
        {
            try
            {
                var token = await GetToken();
                _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                // Endpoint proporcionado: Admision/Buscarprofesionalesdelservicio?idservicio=100
                string endpoint = $"Admision/Buscarprofesionalesdelservicio?idservicio={idServicio}";

                var response = await _http.GetAsync(endpoint);
                if (!response.IsSuccessStatusCode) return new List<dynamic>();

                var jsonStr = await response.Content.ReadAsStringAsync();

                // Deserializamos la lista dinámica recibida [{idServicio, idProfesional, apellido, nombreProf}]
                return JsonConvert.DeserializeObject<List<dynamic>>(jsonStr) ?? new List<dynamic>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Error ObtenerProfesionalesServicio]: {ex.Message}");
                return new List<dynamic>();
            }
        }


    }

}