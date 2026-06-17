using System.Data;
using Microsoft.Data.SqlClient;
using SainnavvAPI.Models;

namespace SainnavvAPI.Data
{
    public class HistoriaRepository
    {
        // 1. Solución warning connectionString
        private readonly string _connectionString;

        public HistoriaRepository(IConfiguration configuration)
        {
            // Si es null, lanza error o pone cadena vacía para que no se queje el compilador
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                                ?? throw new Exception("Cadena de conexión no encontrada");
        }

        private readonly Dictionary<int, string> _localidadesMap = new Dictionary<int, string>
        {
            // CHACO
            {1, "PUERTO BERMEJO"}, {2, "GENERAL VEDIA"}, {3, "LA LEONESA"}, {4, "LAS PALMAS"}, {5, "ISLA DEL CERRITO"},
            {6, "COLONIA BENITEZ"}, {7, "MARGARITA BELEN"}, {8, "BARRANQUERAS"}, {9, "FONTANA"}, {10, "PUERTO VILELAS"},
            {11, "RESISTENCIA"}, {12, "COLONIA BARANDA"}, {13, "BASAIL"}, {14, "LAS GARCITAS"}, {15, "COLONIAS UNIDAS"},
            {17, "COLONIA ELISA"}, {18, "LA VERDE"}, {19, "LAPACHITO"}, {20, "LA ESCONDIDA"}, {21, "MAKALLE"},
            {22, "PUERTO TIROL"}, {23, "COTE-LAI"}, {24, "CHARADAI"}, {25, "TACO POZO"}, {26, "LOS FRENTONES"},
            {27, "PAMPA DEL INFIERNO"}, {28, "CONCEPCION DEL BERMEJO"}, {29, "AVIA TERAI"}, {30, "NAPENAY"}, {31, "CAMPO LARGO"},
            {32, "SAENZ PEÑA"}, {33, "EL PALMAR"}, {34, "QUITILIPI"}, {35, "COLONIA ABORIGEN OESTE"}, {36, "COLONIA ABORIGEN ESTE"},
            {37, "MACHAGAI"}, {38, "PCIA. DE LA PLAZA"}, {39, "LA TIGRA"}, {40, "LA CLOTILDE"}, {41, "SAN BERNARDO"},
            {42, "VILLA BERTHET"}, {43, "SAMUHU"}, {44, "VILLA ANGELA"}, {45, "CORONEL DU GRATTY"}, {46, "PAMPA DEL INDIO"},
            {47, "PRESIDENCIA ROCA"}, {48, "LAGUNA LIMPIA"}, {49, "GENERAL SAN MARTIN"}, {50, "CIERVO PETISO"}, {51, "PAMPA ALMIRON"},
            {52, "SELVA RIO DE ORO"}, {53, "LA EDUVIGES"}, {54, "CORZUELA"}, {55, "LAS BREÑAS"}, {56, "CHARATA"},
            {57, "EL SAUZALITO"}, {58, "EL SAUZAL"}, {59, "COMANDANCIA FRIAS"}, {60, "MISION NUEVA POMPEYA"}, {61, "FUERTE ESPERANZA"},
            {62, "EL ESPINILLO"}, {63, "VILLA RIO BERMEJITO"}, {64, "MIRAFLORES"}, {65, "JUAN JOSE CASTELLI"}, {66, "TRES ISLETAS"},
            {67, "SANTA SYLVINA"}, {68, "CHOROTIS"}, {69, "PINEDO"}, {70, "GANCEDO"}, {71, "HERMOSO CAMPO"}, {116, "CAPITAN SOLARI"},
            
            // CORRIENTES (Muestra representativa basada en tu lista)
            {118, "Bella Vista"}, {121, "Berón de Astrada"}, {123, "Corrientes Capital"}, {126, "Concepción"},
            {132, "Curuzú Cuatiá"}, {137, "Empedrado"}, {139, "Esquina"}, {142, "Alvear"}, {150, "Goya"},
            {151, "Itatí"}, {160, "Ituzaingó"}, {167, "Lavalle"}, {172, "Mburucuyá"}, {175, "Mercedes"},
            {183, "Monte Caseros"}, {187, "Paso de los Libres"}, {189, "Saladas"}, {193, "San Cosme"},
            {196, "San Luis del Palmar"}, {202, "San Miguel"}, {207, "San Roque"}, {211, "Santo Tomé"}, {213, "Sauce"},

            // FORMOSA (Muestra representativa)
            {217, "Formosa"}, {238, "Clorinda"}, {242, "Laguna Blanca"}, {262, "Pirané"}, {254, "El Colorado"},
            {280, "Ing. Juárez"}, {292, "Gral. Belgrano"}, {297, "Las Lomitas"}, {301, "Pozo del Tigre"}
        };

        // Helper para obtener NOMBRE dado el ID (Lectura)
         private string GetNombreLocalidad(int id)
        {
            // Si encuentra el ID, devuelve el nombre. Si no, devuelve RESISTENCIA
            return _localidadesMap.ContainsKey(id) ? _localidadesMap[id].ToUpper() : "RESISTENCIA";
        }

        // Helper Escritura (Texto -> ID)
        private int GetIdLocalidad(string? nombre)
        {
            // El IDLocalidad de Resistencia es 11
            if (string.IsNullOrEmpty(nombre)) return 11;

            var item = _localidadesMap.FirstOrDefault(x => x.Value.Equals(nombre.Trim(), StringComparison.OrdinalIgnoreCase));

            // Si lo encuentra devuelve la key, sino devuelve 11 (Resistencia)
            return item.Key > 0 ? item.Key : 11;
        }

        private readonly Dictionary<int, string> _departamentosMap = new Dictionary<int, string>
        {
            {7, "ALMIRANTE BROWN"}, {14, "BERMEJO"}, {21, "COMANDANTE FERNANDEZ"}, {28, "CHACABUCO"},
            {36, "12 DE OCTUBRE"}, {39, "2 DE ABRIL"}, {43, "F. JUSTO STA MARIA DE ORO"}, {49, "GRAL. BELGRANO"},
            {56, "GENERAL DONOVAN"}, {63, "GRAL. GÜEMES"}, {70, "INDEPENDENCIA"}, {77, "LIBERTAD"},
            {84, "LIBERTADOR GRAL. SAN MARTIN"}, {91, "MAIPU"}, {98, "MAYOR LUIS FONTANA"}, {105, "9 DE JULIO"},
            {112, "O` HIGGINS"}, {119, "PCIA. DE LA PLAZA"}, {126, "PRIMERO DE MAYO"}, {133, "QUITILIPI"},
            {140, "SAN FERNANDO"}, {147, "SAN LORENZO"}, {154, "SARGENTO CABRAL"}, {161, "TAPENAGA"},
            {168, "25 DE MAYO"}, {201, "BELLA VISTA"}, {202, "BERÓN DE ASTRADA"}, {203, "CAPITAL"},
            {204, "CONCEPCIÓN"}, {205, "CURUZÚ CUATIÁ"}, {206, "EMPEDRADO"}, {207, "ESQUINA"},
            {208, "GENERAL ALVEAR"}, {209, "GENERAL PAZ"}, {210, "GOYA"}, {211, "ITATÍ"}, {212, "ITUZAINGÓ"},
            {213, "LAVALLE"}, {214, "MBURUCUYÁ"}, {215, "MERCEDES"}, {216, "MONTE CASEROS"},
            {217, "PASO DE LOS LIBRES"}, {218, "SALADAS"}, {219, "SAN COSME"}, {220, "SAN LUIS DEL PALMAR"},
            {221, "SAN MARTÍN"}, {222, "SAN MIGUEL"}, {223, "SAN ROQUE"}, {224, "SANTO TOMÉ"}, {225, "SAUCE"},
            {251, "FORMOSA"}, {252, "LAISHI"}, {253, "PILAGAS"}, {254, "PILCOMAYO"}, {255, "PIRANÉ"},
            {256, "BERMEJO"}, {257, "MATACOS"}, {258, "PATIÑO"}, {259, "RAMÓN LISTA"}, {301, "ANTA"},
            {302, "CACHI"}, {303, "CAFAYATE"}, {304, "CAPITAL"}, {305, "CERRILLOS"}, {306, "CHICOANA"},
            {307, "GENERAL GÜEMES"}, {308, "GENERAL JOSE DE SAN MARTÍN"}, {309, "GUACHIPAS"}, {310, "LA CALDERA"},
            {311, "LA CANDELARIA"}, {312, "LA POMA"}, {313, "LA VIÑA"}, {314, "LOS ANDES"}, {315, "METÁN"},
            {316, "MOLINOS"}, {317, "ORÁN"}, {318, "RIVADAVIA"}, {319, "ROSARIO DE LA FRONTERA"},
            {320, "ROSARIO DE LERMA"}, {321, "ROSARIO DE LERMA"}, {322, "SAN CARLOS"}, {351, "CASTELLANOS"},
            {352, "GARAY"}, {353, "GENERAL OBLIGADO (1)"}, {354, "LA CAPITAL"}, {355, "LAS COLONIAS"},
            {356, "NUEVE DE JULIO"}, {357, "SAN CRISTÓBAL"}, {358, "SAN JAVIER"}, {359, "SAN JERÓNIMO"},
            {360, "SAN JUSTO"}, {361, "SAN MARTÍN"}, {362, "VERA"}, {363, "BELGRANO"}, {364, "CASEROS"},
            {365, "CONSTITUCIÓN"}, {366, "GENERAL LÓPEZ"}, {367, "IRIONDO"}, {368, "ROSARIO"},
            {369, "SAN LORENZO"}, {401, "ALBERDI (1)"}, {402, "ATAMISQUI"}, {403, "BANDA"}, {404, "CAPITAL"},
            {405, "CHOYA"}, {406, "FIGUEROA"}, {407, "GUASAYÁN"}, {408, "JIMÉNEZ"}, {409, "JUAN FELIPE IBARRA"},
            {410, "LORETO"}, {411, "MORENO"}, {412, "OJO DE AGUA"}, {413, "PELLEGRINI"}, {414, "RÍO HONDO"},
            {415, "ROBLES"}, {416, "SAN MARTIN"}, {417, "SARMIENTO"}, {1517, "ASUNCION"},
            {50000, "BUENOS AIRES"}, {50001, "CORDOBA"}
        };

        // Helper Lectura (ID -> Texto)
        private string GetNombreDepartamento(int id)
        {
            return _departamentosMap.ContainsKey(id) ? _departamentosMap[id] : "SAN FERNANDO";
        }

        // Helper Escritura (Texto -> ID)
        private int GetIdDepartamento(string? nombre)
        {
            if (string.IsNullOrEmpty(nombre)) return 140; // 140 es San Fernando por defecto
            var item = _departamentosMap.FirstOrDefault(x => x.Value.Equals(nombre.Trim(), StringComparison.OrdinalIgnoreCase));
            return item.Key > 0 ? item.Key : 140;
        }

        private readonly Dictionary<int, string> _provinciasMap = new Dictionary<int, string>
        {
            {2, "CAPITAL FEDERAL"}, {6, "BUENOS AIRES"}, {10, "CATAMARCA"}, {14, "CORDOBA"},
            {18, "CORRIENTES"}, {22, "CHACO"}, {26, "CHUBUT"}, {30, "ENTRE RIOS"},
            {34, "FORMOSA"}, {38, "JUJUY"}, {42, "LA PAMPA"}, {46, "LA RIOJA"},
            {50, "MENDOZA"}, {54, "MISIONES"}, {58, "NEUQUEN"}, {62, "RIO NEGRO"},
            {66, "SALTA"}, {70, "SAN JUAN"}, {74, "SAN LUIS"}, {78, "SANTA CRUZ"},
            {82, "SANTA FE"}, {86, "STGO. DEL ESTERO"}, {90, "TUCUMAN"}, {94, "TIERRA DEL FUEGO"},
            {98, "OTRO PAIS"}, {99, "NO ESPECIFICADO"}, {1500, "ALTO PARAGUAY"}, {1502, "AMAMBAY"},
            {1503, "BOQUERON"}, {1504, "CAAGUAZU"}, {1505, "CAAZAPA"}, {1506, "CANINDEYU"},
            {1507, "CENTRAL"}, {1508, "CONCEPCION"}, {1509, "CORDILLERA"}, {1510, "GUAIRA"},
            {1511, "ITAPUA"}, {1512, "MISIONES (PY)"}, {1513, "ÑEEMBUCU"}, {1514, "PARAGUARI"},
            {1515, "PTE. HAYES"}, {1516, "SAN PEDRO"}, {1517, "ASUNCION"}
        };

        // Helper Lectura (ID -> Texto)
        private string GetNombreProvincia(int id)
        {
            return _provinciasMap.ContainsKey(id) ? _provinciasMap[id] : "CHACO";
        }

        // Helper Escritura (Texto -> ID)
        private int GetIdProvincia(string? nombre)
        {
            if (string.IsNullOrEmpty(nombre)) return 22; // 22 es CHACO por defecto
            var item = _provinciasMap.FirstOrDefault(x => x.Value.Equals(nombre.Trim(), StringComparison.OrdinalIgnoreCase));
            return item.Key > 0 ? item.Key : 22;
        }

        private readonly Dictionary<int, string> _paisesMap = new Dictionary<int, string>
        {
            {200, "ARGENTINA"},
            {221, "PARAGUAY"}
            
            
        };

        // Helper Lectura (ID -> Texto)
        private string GetNombrePais(int id)
        {
            return _paisesMap.ContainsKey(id) ? _paisesMap[id] : "ARGENTINA";
        }

        // Helper Escritura (Texto -> ID)
        private int GetIdPais(string? nombre)
        {
            if (string.IsNullOrEmpty(nombre)) return 200; // 200 es Argentina por defecto

            // Compara ignorando mayúsculas y acentos si es posible, pero con OrdinalIgnoreCase funciona bien para listas estáticas
            var item = _paisesMap.FirstOrDefault(x => x.Value.Equals(nombre.Trim(), StringComparison.OrdinalIgnoreCase));
            return item.Key > 0 ? item.Key : 200;
        }

        public async Task<object?> BuscarPacienteLocalCompleto(string tipo, string valor)
        {
            // Validamos tipo de búsqueda y convertimos valor
            string columnaBusqueda = tipo == "dni" ? "NumeroDocumento" : "NumHistClinica";

            // Convertir a long por seguridad (DNI o HC pueden ser largos)
            long.TryParse(valor, out long valorNumerico);

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();

                // -------------------------------------------------------------
                // CONSULTA SEGURA CON ALIAS
                // Evitamos SELECT * para no confundir IDPaciente de una tabla con la otra
                // -------------------------------------------------------------
                string sql = $@"
            SELECT 
                -- TABLA PACIENTES (P)
                P.IDPaciente, 
                P.NumHistClinica, 
                P.Apellido, 
                P.Nombre, 
                P.NumeroDocumento, 
                P.Direccion, 
                P.FechaNac, 
                P.Sexo, 
                P.Telefono, 
                P.NroAfiliado,
                P.IDObraSocial, -- INT: Necesita traducción
                p.IDLocalidad,
                P.NombrePadre, 
                P.NombreMadre,
                
                -- TABLA FICHA (F) (Usamos Alias 'F_' para evitar conflictos)
                F.IDpaciente as FichaID,
                F.TelCel,       
                F.TelFijo,
                F.AreaProg,
                F.ViviendaTipo,
                F.NroAmbientes,
                F.PersHabitan,
                F.LuzElectrica,
                F.Agua,
                F.Basura,
                F.AyudaSocial,
                F.Observaciones,
                F.Alergias,
                F.Correo

            FROM [AHS].[dbo].[Pacientes] P
            LEFT JOIN [AHS].[dbo].[Mio_Pacientes_Ficha] F ON P.IDPaciente = F.IDpaciente
            WHERE P.{columnaBusqueda} = @Valor";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@Valor", valorNumerico);

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            // === Helpers Internos de Lectura (Evita caídas por nulos) ===
                            string Str(string c) { try { return reader[c] != DBNull.Value ? reader[c].ToString().Trim() : ""; } catch { return ""; } }
                            int Int(string c) { try { return reader[c] != DBNull.Value ? Convert.ToInt32(reader[c]) : 0; } catch { return 0; } }
                            bool Bool(string c) { try { return reader[c] != DBNull.Value && Convert.ToBoolean(reader[c]); } catch { return false; } }

                            // === MAPEOS (TRADUCCIONES ID -> TEXTO) ===

                            // 1. Obra Social (Tabla Paciente usa INT, Frontend usa String)
                            // (Aquí deberías ajustar los IDs reales de tu tabla ObrasSociales)
                            int idOS = Int("IDObraSocial"); // Si es NULL en BD, esto devuelve 0

                            string obraSocialTxt; // Declaramos variable

                            switch (idOS)
                            {
                                case 0:
                                    // Caso CRÍTICO: Si en BD es NULL o 0, el texto debe coincidir con Angular
                                    obraSocialTxt = "Ninguna";
                                    break;
                                case 999:
                                    obraSocialTxt = "INSSSEP";
                                    break;
                                case 9:
                                    obraSocialTxt = "Plan Nacer";
                                    break;
                                case 90:
                                    obraSocialTxt = "Plan Nacer -R";
                                    break;
                                case 99:
                                    obraSocialTxt = "Profe";
                                    break;
                                case 500807:
                                    obraSocialTxt = "PAMI";
                                    break;
                                default:
                                    // Cualquier otro ID desconocido > 0 se marca como "Otra"
                                    obraSocialTxt = "Otra";
                                    break;
                            }

                            // 2. Traducciones de Ficha
                            int idViv = Int("ViviendaTipo");
                            string vivTxt = idViv == 1 ? "Casa" : (idViv == 2 ? "Departamento" : "Otro");

                            int idArea = Int("AreaProg");
                            string areaTxt = idArea == 1 ? "Zona Sur" : (idArea == 2 ? "Zona Norte" : "No Especificado");

                            int idAgua = Int("Agua");
                            string aguaTxt = idAgua == 1 ? "Red Publica" : "Otro";
                            int idLoc = Int("IDLocalidad");
                            string localidadTxt = GetNombreLocalidad(idLoc);
                            int idPais = Int("Nacionalidad");
                            string paisTxt = GetNombrePais(idPais);
                            // === Armado del Objeto JSON ===
                            return new
                            {
                                origen = "local_sql",
                                idPaciente = Int("IDPaciente"),

                                // Datos Personales
                                nombreApellido = $"{Str("Apellido")} {Str("Nombre")}".Trim(),
                                dni = Str("NumeroDocumento"),
                                hcNumber = Str("NumHistClinica"),
                                sexo = Str("Sexo"),
                                fechaNacimiento = DateTime.TryParse(Str("FechaNac"), out var dt) ? dt.ToString("yyyy-MM-dd") : "",
                                domicilio = Str("Direccion"),
                                localidad = localidadTxt,
                                nacionalidad = paisTxt,
                                provincia = "CHACO",
                                // Contacto (Prioridad Ficha -> Paciente)
                                telefono = !string.IsNullOrEmpty(Str("TelCel")) ? Str("TelCel") : Str("Telefono"),
                                telefonoFijo = Str("TelFijo"),
                                correo = Str("Correo"),

                                // Familia
                                madre = new { nombre = Str("NombreMadre"), dni = "" }, // DocMadre no está en tabla Paciente provista
                                padre = new { nombre = Str("NombrePadre"), dni = "" },

                                // Cobertura
                                obraSocial = obraSocialTxt,
                                nroObraSocial = Str("NroAfiliado"),

                                // Datos Sociales / Ficha (Pueden estar vacíos si no se cargó ficha aún)
                                areaProgramatica = areaTxt,
                                vivienda = vivTxt,
                                numAmbientes = Int("NroAmbientes"),
                                habitantes = Int("PersHabitan"),
                                luzElectrica = Bool("LuzElectrica") ? "Si" : "No",
                                agua = aguaTxt,
                                basura = Bool("Basura") ? "Si" : "No",
                                planSocial = Bool("AyudaSocial") ? "Si" : "No",

                                alergias = Str("Alergias"),
                                observaciones = Str("Observaciones")
                            };
                        }
                    }
                }
            }
            return null;
        }



        // Guardado Relacional de 9 Tablas usando el DTO específico de Sainnavv
        // =========================================================================
        // GUARDADO RELACIONAL 
        // =========================================================================
        // GUARDADO RELACIONAL COMPLETO CON REGLAS DE NEGOCIO DETALLADAS
        // =========================================================================
        public async Task<int> GuardarSainnavvRelacional(SainnavvFormDataDto data, int idUsuarioActual)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                SqlTransaction trans = conn.BeginTransaction();

                try
                {
                    int idEpisodio;
                    bool esEdicion = data.IdEpisodio > 0;

                    // 1. OBTENEMOS LOS USUARIOS RESPONSABLES POR PASO
                    int usuPaso1 = data.profesionalPaso0 > 0 ? data.profesionalPaso0 : idUsuarioActual; // Identificación
                    int usuPaso2 = data.profesionalPaso1 > 0 ? data.profesionalPaso1 : idUsuarioActual; // Familia
                    int usuPaso3 = data.profesionalPaso2 > 0 ? data.profesionalPaso2 : idUsuarioActual; // Ingreso/Diag
                    int usuPaso4 = data.profesionalPaso3 > 0 ? data.profesionalPaso3 : idUsuarioActual; // Intervenciones
                    int usuPaso5 = data.profesionalPaso4 > 0 ? data.profesionalPaso4 : idUsuarioActual; // Clínica/Estrategias
                    int usuPaso6 = data.profesionalPaso5 > 0 ? data.profesionalPaso5 : idUsuarioActual; // Conclusiones/Artic

                    // ------------------------------------------------------------------------
                    // A. TABLA: ADMISION (Episodio)
                    // ------------------------------------------------------------------------
                    string spAdmision = esEdicion ? "ModAdmision" : "InsertAdmision";
                    using (SqlCommand cmd = new SqlCommand(spAdmision, conn, trans))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        if (esEdicion) cmd.Parameters.AddWithValue("@IdEpisodio", data.IdEpisodio);

                        cmd.Parameters.AddWithValue("@IdPaciente", data.idPaciente ?? 0);
                        cmd.Parameters.AddWithValue("@FechaAdmision", DateTime.Now);
                        cmd.Parameters.AddWithValue("@IdServicio", 100); // Fijo Sainnavv
                        cmd.Parameters.AddWithValue("@ModoIngreso", data.modalidadIngreso == "demanda_espontanea" ? 1 : 2);
                        cmd.Parameters.AddWithValue("@IngresoObs", (data.servicioInterconsulta ?? data.institucionDerivacion ?? "").PadRight(50).Substring(0, 50).Trim());

                        int codAcomp = data.acompananteTipo.Contains("Madre") ? 2 : (data.acompananteTipo.Contains("Padre") ? 1 : 0);
                        cmd.Parameters.AddWithValue("@Acompanante", codAcomp);

                        // Deducción dinámica de Forma de Contacto
                        int formaContactoVal = 3;
                        if (data.modalidadIngreso == "interconsulta") formaContactoVal = 5;
                        else if (data.intervenciones != null && !string.IsNullOrEmpty(data.intervenciones.virtualInstitucional)) formaContactoVal = 2;
                        else if (data.intervenciones != null && !string.IsNullOrEmpty(data.intervenciones.referenteFamiliar)) formaContactoVal = 4;
                        else if (data.intervenciones != null && !string.IsNullOrEmpty(data.intervenciones.admisionReferente)) formaContactoVal = 3;

                        cmd.Parameters.AddWithValue("@FormaContacto", formaContactoVal);
                        cmd.Parameters.AddWithValue("@MotivoConsulta", data.motivosConsulta?.Length > 50 ? data.motivosConsulta.Substring(0, 50) : data.motivosConsulta ?? "");
                        cmd.Parameters.AddWithValue("@IdUsuario", usuPaso3);

                        if (esEdicion)
                        {
                            await cmd.ExecuteNonQueryAsync();
                            idEpisodio = data.IdEpisodio!.Value;
                        }
                        else
                        {
                            idEpisodio = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                        }
                    }

                    // ------------------------------------------------------------------------
                    // B. TABLA: ACOMPAÑANTE
                    // ------------------------------------------------------------------------
                    if (esEdicion)
                    {
                        int? idAcompanante = await ObtenerIdFila(conn, trans, "SELECT IdAcompanante FROM ACOMPANANTE WHERE IdEpisodio = @Id", idEpisodio);
                        string spAcomp = idAcompanante > 0 ? "ModAcompanante" : "InsertAcompanante";
                        using (SqlCommand cmd = new SqlCommand(spAcomp, conn, trans))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            if (idAcompanante > 0) cmd.Parameters.AddWithValue("@IdAcompanante", idAcompanante);
                            cmd.Parameters.AddWithValue("@IdEpisodio", idEpisodio);
                            cmd.Parameters.AddWithValue("@ApellidoyNombre", data.acompananteNombre ?? "");
                            cmd.Parameters.AddWithValue("@DNI", data.acompananteDni ?? "");
                            cmd.Parameters.AddWithValue("@Edad", data.acompananteEdad ?? "");
                            cmd.Parameters.AddWithValue("@Telefono", data.acompananteCel ?? "");
                            cmd.Parameters.AddWithValue("@Domicilio", data.acompananteDomicilio ?? "");
                            cmd.Parameters.AddWithValue("@Vinculo", data.acompananteVinculo ?? "");
                            cmd.Parameters.AddWithValue("@Observaciones", "");
                            cmd.Parameters.AddWithValue("@IdUsuario", usuPaso2);
                            await cmd.ExecuteNonQueryAsync();
                        }
                    }
                    else if (!string.IsNullOrEmpty(data.acompananteNombre))
                    {
                        using (SqlCommand cmd = new SqlCommand("InsertAcompanante", conn, trans))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@IdEpisodio", idEpisodio);
                            cmd.Parameters.AddWithValue("@ApellidoyNombre", data.acompananteNombre);
                            cmd.Parameters.AddWithValue("@DNI", data.acompananteDni ?? "");
                            cmd.Parameters.AddWithValue("@Edad", data.acompananteEdad ?? "");
                            cmd.Parameters.AddWithValue("@Telefono", data.acompananteCel ?? "");
                            cmd.Parameters.AddWithValue("@Domicilio", data.acompananteDomicilio ?? "");
                            cmd.Parameters.AddWithValue("@Vinculo", data.acompananteVinculo ?? "");
                            cmd.Parameters.AddWithValue("@Observaciones", "");
                            cmd.Parameters.AddWithValue("@IdUsuario", usuPaso2);
                            await cmd.ExecuteNonQueryAsync();
                        }
                    }
                    // ------------------------------------------------------------------------
                    if (esEdicion)
                    {
                        await EjecutarQueryRaw(conn, trans, "DELETE FROM PROFINTER WHERE IdEpisodio = @Id", idEpisodio);
                    }

                    // Compilamos una lista con los IDs seleccionados en los pasos de la consulta
                    int[] idsProfesionalesPasos = {
                        data.profesionalPaso0,
                        data.profesionalPaso1,
                        data.profesionalPaso2,
                        data.profesionalPaso3,
                        data.profesionalPaso4,
                        data.profesionalPaso5
                    };

                    // Insertamos de forma única (Distinct) solo aquellos que fueron seleccionados (Id > 0)
                    foreach (var idProf in idsProfesionalesPasos.Distinct().Where(id => id > 0))
                    {
                        using (SqlCommand cmd = new SqlCommand("InsertProfInter", conn, trans))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@IdEpisodio", idEpisodio);
                            cmd.Parameters.AddWithValue("@IdProfesional", idProf); // <--- ID REAL DEL HOSPITAL
                            cmd.Parameters.AddWithValue("@FechaIntervencion", DateTime.Now);
                            cmd.Parameters.AddWithValue("@Idusuario", idUsuarioActual);
                            cmd.Parameters.AddWithValue("@Observaciones", "Carga de profesional por paso de formulario");
                            await cmd.ExecuteNonQueryAsync();
                        }
                    }
                    // ------------------------------------------------------------------------
                    // 4. TABLA: ANTECEDENTES
                    // ------------------------------------------------------------------------
                    if (esEdicion)
                    {
                        int? idAnt = await ObtenerIdFila(conn, trans, "SELECT IdLinea FROM ANTEDECENTES WHERE IdReferencia = @Id AND IdServicio = 100", idEpisodio);
                        string spAnt = idAnt > 0 ? "ModAntecedentes" : "InsertAntecedentes";
                        using (SqlCommand cmd = new SqlCommand(spAnt, conn, trans))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            if (idAnt > 0) cmd.Parameters.AddWithValue("@IdLinea", idAnt);

                            cmd.Parameters.AddWithValue("@IdReferencia", idEpisodio);
                            cmd.Parameters.AddWithValue("@IdServicio", 100);
                            cmd.Parameters.AddWithValue("@FechaAntecedente", DateTime.Now);
                            cmd.Parameters.AddWithValue("@Detalle", data.resumenIntervencionesPrevias ?? "");
                            cmd.Parameters.AddWithValue("@Archivo", data.archivoAntecedentes ?? "");
                            cmd.Parameters.AddWithValue("@Direccion", data.descripcionArchivo ?? "");
                            cmd.Parameters.AddWithValue("@Idusuario", idUsuarioActual);
                            await cmd.ExecuteNonQueryAsync();
                        }
                    }
                    else if (!string.IsNullOrEmpty(data.resumenIntervencionesPrevias))
                    {
                        using (SqlCommand cmd = new SqlCommand("InsertAntecedentes", conn, trans))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@IdReferencia", idEpisodio);
                            cmd.Parameters.AddWithValue("@IdServicio", 100);
                            cmd.Parameters.AddWithValue("@FechaAntecedente", DateTime.Now);
                            cmd.Parameters.AddWithValue("@Detalle", data.resumenIntervencionesPrevias);
                            cmd.Parameters.AddWithValue("@Archivo", data.archivoAntecedentes ?? "");
                            cmd.Parameters.AddWithValue("@Direccion", data.descripcionArchivo ?? "");
                            cmd.Parameters.AddWithValue("@Idusuario", idUsuarioActual);
                            await cmd.ExecuteNonQueryAsync();
                        }
                    }
                    // ------------------------------------------------------------------------
                    // 5. TABLA: DiagnosticosPresuntivos (Limpiar y Re-insertar)
                    // ------------------------------------------------------------------------
                    if (esEdicion)
                    {
                        await EjecutarQueryRaw(conn, trans, "DELETE FROM DiagnosticosPresuntivos WHERE IdEpisodio = @Id", idEpisodio);
                    }

                    foreach (var diag in data.diagnosticoPresuntivo)
                    {
                        using (SqlCommand cmd = new SqlCommand("InsertDiagPresun", conn, trans))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@IdEpisodio", idEpisodio);
                            cmd.Parameters.AddWithValue("@IdDiag", 0); // Ajustar ID real
                            cmd.Parameters.AddWithValue("@Fecha", DateTime.Now);
                            cmd.Parameters.AddWithValue("@Idusuario", idUsuarioActual);
                            cmd.Parameters.AddWithValue("@Observaciones", diag);
                            await cmd.ExecuteNonQueryAsync();
                        }
                    }


                    // ... [Sección de 3. PROFINTER, 4. ANTECEDENTES y 5. DIAGNÓSTICOS se mantiene igual con sus respectivos 'usuPasoX'] ...

                    // ------------------------------------------------------------------------
                    // 6. TABLA: INTERVENCIONES (Generamos los IDs específicos de la sesión)
                    // ------------------------------------------------------------------------
                    if (esEdicion)
                    {
                        // A. DESVINCULAMOS TEMPORALMENTE las llaves foráneas para evitar el conflicto "fk_intervenciones"
                        // Ponemos en NULL la referencia para poder borrar el registro padre de forma segura
                        await EjecutarQueryRaw(conn, trans, "UPDATE IntervencionMedica SET IdIntervencion = NULL WHERE IdEpisodio = @Id", idEpisodio);
                        await EjecutarQueryRaw(conn, trans, "UPDATE ESTRATEGIASTERAP SET IdIntervencion = NULL WHERE IdEpisodio = @Id", idEpisodio);
                        await EjecutarQueryRaw(conn, trans, "UPDATE Articulaciones SET IdIntervencion = NULL WHERE IdEpisodio = @Id", idEpisodio);

                        // B. Ahora que no hay dependencias activas, BORRAMOS las intervenciones de hoy
                        await EjecutarQueryRaw(conn, trans, "DELETE FROM INTERVENCIONES WHERE IdEpisodio = @Id AND CAST(Fecha AS DATE) = CAST(GETDATE() AS DATE)", idEpisodio);
                    }

                    int idIntervencionFamiliar = 0;
                    int idIntervencionPsicologica = 0;
                    int idIntervencionMedica = 0;
                    int idIntervencionArticulacion = 0;
                    int idIntervencionEstrategia = 0;

                    // A. Guardar Entrevista Familiar (Tipo 1)
                    if (!string.IsNullOrEmpty(data.entrevistaReferenteDetalle))
                    {
                        idIntervencionFamiliar = await RegistrarIntervencionDiaria(conn, trans, idEpisodio, 1, data.entrevistaReferenteDetalle, 0, usuPaso4);
                    }

                    // B. Guardar Atención Psicología (Tipo 2)
                    if (!string.IsNullOrEmpty(data.atencionPsicologicaDetalle))
                    {
                        int tecnica = !string.IsNullOrEmpty(data.intervenciones.horaJuego) ? 1 : (!string.IsNullOrEmpty(data.intervenciones.entrevistaAdolescente) ? 2 : 0);
                        idIntervencionPsicologica = await RegistrarIntervencionDiaria(conn, trans, idEpisodio, 2, data.atencionPsicologicaDetalle, tecnica, usuPaso4);
                    }

                    // =========================================================================
                    
                    // =========================================================================
                    bool tieneDatosMedicos = !string.IsNullOrEmpty(data.atencionMedicaExamen) || !string.IsNullOrEmpty(data.tratamientoIndicado) || !string.IsNullOrEmpty(data.observacionesEstrategia);
                    if (tieneDatosMedicos)
                    {
                        idIntervencionMedica = await RegistrarIntervencionDiaria(
                            conn,
                            trans,
                            idEpisodio,
                            3,
                            data.observacionesEstrategia ?? "", // 
                            0,
                            usuPaso5
                        );
                    }

                    // D. Guardar Observaciones Generales de Intervenciones en la tabla INTERVENCIONES 
                    if (!string.IsNullOrEmpty(data.observacionesIntervenciones))
                    {
                        await RegistrarIntervencionDiaria(conn, trans, idEpisodio, 1, data.observacionesIntervenciones, 0, usuPaso4);
                    }
                    // ------------------------------------------------------------------------
                    // 7. TABLA: IntervencionMedica 
                    // ------------------------------------------------------------------------
                    int? idMed = esEdicion ? await ObtenerIdFila(conn, trans, "SELECT IdIntervencion FROM IntervencionMedica WHERE IdEpisodio = @Id", idEpisodio) : 0;
                    string spMed = idMed > 0 ? "ModIntervencion" : "InsertIntervMedica";

                    using (SqlCommand cmd = new SqlCommand(spMed, conn, trans))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        if (idMed > 0) cmd.Parameters.AddWithValue("@IdIntervencionMedica", idMed);

                        cmd.Parameters.AddWithValue("@IdEpisodio", idEpisodio);

                        // Campos de Forense
                        cmd.Parameters.AddWithValue("@IntervieneForense", data.intervenciones.conForense); // Bit

                        
                        string forenseNombreVal = "No";
                        if (data.intervenciones.conForense && !string.IsNullOrEmpty(data.medicoForense))
                        {
                            string nombreLimpio = data.medicoForense.Trim();
                            
                            forenseNombreVal = nombreLimpio.Length > 20 ? nombreLimpio.Substring(0, 20) : nombreLimpio;
                        }
                        cmd.Parameters.AddWithValue("@MedicoForense", forenseNombreVal); // Varchar(20)
                        // ---------------------------------------------------

                        cmd.Parameters.AddWithValue("@IdIntervencion", idIntervencionMedica > 0 ? (object)idIntervencionMedica : DBNull.Value);
                        cmd.Parameters.AddWithValue("@ExamenFisico", !string.IsNullOrEmpty(data.atencionMedicaExamen) ? "Si" : "No");
                        cmd.Parameters.AddWithValue("@ExamenFisicoEstado", data.atencionMedicaExamen ?? "");
                        cmd.Parameters.AddWithValue("@Fotografia", data.fotografiaConsentimiento.Count > 0 ? "Si" : "No");
                        cmd.Parameters.AddWithValue("@Laboratorio", data.laboratorioEstado == "recibido");
                        cmd.Parameters.AddWithValue("@RecepcionLaboratorio", data.hallazgosLaboratorio ?? "en espera de resultados");
                        cmd.Parameters.AddWithValue("@FechaRecepcionLab", DateTime.Now);
                        cmd.Parameters.AddWithValue("@TratamientoMedicoCon", data.tratamientoIndicado ?? "");
                        cmd.Parameters.AddWithValue("@Idusuario", usuPaso5);
                        cmd.Parameters.AddWithValue("@Observaciones", data.observacionesSituacion ?? "");

                        await cmd.ExecuteNonQueryAsync();
                    }

                    // ------------------------------------------------------------------------
                    // 8. TABLA: Articulaciones 
                    // ------------------------------------------------------------------------
                    if (idIntervencionArticulacion > 0)
                    {
                        int? idArt = esEdicion ? await ObtenerIdFila(conn, trans, "SELECT IdArticulaciones FROM Articulaciones WHERE IdEpisodio = @Id", idEpisodio) : 0;
                        string spArt = idArt > 0 ? "ModArticulaciones" : "InsertArticulaciones";
                        using (SqlCommand cmd = new SqlCommand(spArt, conn, trans))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            if (idArt > 0) cmd.Parameters.AddWithValue("@IdArticulaciones", idArt);

                            cmd.Parameters.AddWithValue("@IdEpisodio", idEpisodio);
                            cmd.Parameters.AddWithValue("@IdIntervencion", idIntervencionArticulacion); 

                            cmd.Parameters.AddWithValue("@Coordinacion", data.coordinacionProteccion ?? "");
                            cmd.Parameters.AddWithValue("@DialogoCon", data.dialogoInterdisciplinario ?? "");
                            cmd.Parameters.AddWithValue("@Fecha", DateTime.Now);

                            //  destinos en observaciones
                            List<string> destinosTxt = new List<string>();
                            if (data.destinoLegal != null)
                            {
                                foreach (var dest in data.destinoLegal)
                                {
                                    if (dest == "fiscalia") destinosTxt.Add($"Fiscalía N° {data.fiscaliaNro} (Oficio: {data.oficioNro})");
                                    else if (dest == "ota") destinosTxt.Add("OTA");
                                    else if (dest == "juzgado") destinosTxt.Add($"Juzgado: {(data.esJuzgadoDeTurno ? "De Turno" : data.nombreJuzgadoInterviniente)}");
                                    else if (dest == "asesoria_civil") destinosTxt.Add("Asesoría de NNyA Civil");
                                    else if (dest == "asesoria_penal") destinosTxt.Add("Asesoría de NNyA Penal");
                                }
                            }
                            string obsFinal = string.Join(" | ", destinosTxt);
                            cmd.Parameters.AddWithValue("@Observaciones", obsFinal.Length > 200 ? obsFinal.Substring(0, 200) : obsFinal);

                            cmd.Parameters.AddWithValue("@IdUsuario", usuPaso6);
                            await cmd.ExecuteNonQueryAsync();
                        }
                    }

                    // ------------------------------------------------------------------------
                    // 9. TABLA: ESTRATEGIASTERAP 
                    // ------------------------------------------------------------------------
                    if (idIntervencionEstrategia > 0)
                    {
                        // 9. TABLA: ESTRATEGIASTERAP
                        int? idEst = esEdicion ? await ObtenerIdFila(conn, trans, "SELECT IdEstrategias FROM ESTRATEGIASTERAP WHERE IdEpisodio = @Id", idEpisodio) : 0;
                        string spEst = idEst > 0 ? "ModEstrategiaP" : "InsertEstrategiaP";
                        using (SqlCommand cmd = new SqlCommand(spEst, conn, trans))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            if (idEst > 0) cmd.Parameters.AddWithValue("@IdEstrategias", idEst);

                            cmd.Parameters.AddWithValue("@IdEpisodio", idEpisodio);
                            cmd.Parameters.AddWithValue("@IdIntervencion", idIntervencionEstrategia);
                            cmd.Parameters.AddWithValue("@Turno", data.idProfesionalTurno > 0);

                            // CAMBIADO: Usar el ID del profesional asignado al turno
                            cmd.Parameters.AddWithValue("@IdProfesional", data.idProfesionalTurno); 

                            DateTime fTurno;
                            cmd.Parameters.AddWithValue("@FechaTurno", DateTime.TryParse(data.estrategiaFechaTurno, out fTurno) ? (object)fTurno : DBNull.Value);

                            cmd.Parameters.AddWithValue("@TerapiaPsicoF", data.recomiendaTerapiaReferente);
                            cmd.Parameters.AddWithValue("@TerapiaPsicoDer", data.derivacionA ?? "");
                            cmd.Parameters.AddWithValue("@TerapiaPsicoInv", data.espacioGrupal);
                            cmd.Parameters.AddWithValue("@AcompCamaraGesell", data.acompanamientoGesell);
                            cmd.Parameters.AddWithValue("@MedidaImpContAcer", string.IsNullOrEmpty(data.medidasImpedimento) ? "No" : data.medidasImpedimento);
                            cmd.Parameters.AddWithValue("@Fecha", DateTime.Now);
                            cmd.Parameters.AddWithValue("@Observaciones", "");
                            cmd.Parameters.AddWithValue("@Idusuario", idUsuarioActual);

                            await cmd.ExecuteNonQueryAsync();
                        }
                    }

                    trans.Commit();
                    return idEpisodio;
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    throw new Exception("Error en Transacción SQL: " + ex.Message);
                }
            }
        }

        // ==========================================
        // HELPERS ADICIONALES PARA LA TRANSACCIÓN
        // ==========================================
        private async Task<int?> ObtenerIdFila(SqlConnection conn, SqlTransaction trans, string sql, int idEpisodio)
        {
            using (SqlCommand cmd = new SqlCommand(sql, conn, trans))
            {
                cmd.Parameters.AddWithValue("@Id", idEpisodio);
                var res = await cmd.ExecuteScalarAsync();
                return res != null && res != DBNull.Value ? Convert.ToInt32(res) : (int?)null;
            }
        }

        private async Task EjecutarQueryRaw(SqlConnection conn, SqlTransaction trans, string sql, int idEpisodio)
        {
            using (SqlCommand cmd = new SqlCommand(sql, conn, trans))
            {
                cmd.Parameters.AddWithValue("@Id", idEpisodio);
                await cmd.ExecuteNonQueryAsync();
            }
        }

        private async Task<int> RegistrarIntervencionDiaria(SqlConnection conn, SqlTransaction trans, int idEpisodio, int tipo, string obs, int tecnica, int idUsuario)
        {
            using (SqlCommand cmd = new SqlCommand("InsertIntervencion", conn, trans))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@IdEpisodio", idEpisodio);
                cmd.Parameters.AddWithValue("@Tipo", tipo);
                cmd.Parameters.AddWithValue("@Fecha", DateTime.Now);
                cmd.Parameters.AddWithValue("@Tecnica", tecnica);
                cmd.Parameters.AddWithValue("@Idusuario", idUsuario);
                cmd.Parameters.AddWithValue("@Observaciones", obs);

                
                var res = await cmd.ExecuteScalarAsync();
                return res != null && res != DBNull.Value ? Convert.ToInt32(res) : 0;
            }
        }


        // 1. OBTENER DATOS EXISTENTES
        public async Task<(string Json, int Id)?> ObtenerHistoriaExistente(string valorBusqueda)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                // Asegúrate que esta tabla 'HistoriasClinicas' exista en tu base, es la del primer formulario
                // Si está en AHS o en otra, ajusta el nombre.
                string sql = "SELECT TOP 1 Id, DatosCompletosJson FROM HistoriasClinicas WHERE DNI = @Val OR PacienteNombre LIKE '%' + @Val + '%' ORDER BY Id DESC";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@Val", valorBusqueda);
                    await conn.OpenAsync();
                    using (var r = await cmd.ExecuteReaderAsync())
                    {
                        if (await r.ReadAsync())
                        {
                            string json = r["DatosCompletosJson"] != DBNull.Value ? r["DatosCompletosJson"].ToString() : "";
                            int id = Convert.ToInt32(r["Id"]);
                            return (json, id); // Ahora los tipos coinciden
                        }
                    }
                }
            }
            return null;
        }

        // 2. ACTUALIZAR (UPDATE)
        public async Task ActualizarHistoria(int id, string fullJson, string? pathAnt, string? pathCli)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                // Usamos el Store Procedure que acabamos de crear
                using (SqlCommand cmd = new SqlCommand("sp_ActualizarHistoriaConHistorial", connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@Id", id);
                    cmd.Parameters.AddWithValue("@JsonNuevo", fullJson);

                    // Si es nulo, mandamos DBNull para que SQL sepa usar ISNULL()
                    cmd.Parameters.AddWithValue("@RutaAntNueva", (object?)pathAnt ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@RutaCliNueva", (object?)pathCli ?? DBNull.Value);

                    await connection.OpenAsync();
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }


        

        // ==========================================
        // MÉTODOS DE NEGOCIO (Los que faltaban/fallaban)
        // ==========================================

       public async Task<int> GestionarPacienteSocial(SocialFormDataDto data, int idUsuario)
{
    using (SqlConnection conn = new SqlConnection(_connectionString))
    {
        await conn.OpenAsync();

        // -------------------------------------------------------------
        // PASO CRÍTICO: DETECCIÓN DE DUPLICADOS O EXISTENTES POR DNI
        // -------------------------------------------------------------
        // Aunque Angular mande IdPaciente = 0 (nuevo), verificamos si el DNI ya existe en SQL
        // para no crear un duplicado y pisar el registro existente.
        
        int idExistente = 0;
        
        if (int.TryParse(data.Dni, out int dniInt))
        {
            string sqlCheck = "SELECT TOP 1 IDPaciente FROM [AHS].[dbo].[Pacientes] WHERE NumeroDocumento = @Dni";
            using (SqlCommand cmdCheck = new SqlCommand(sqlCheck, conn))
            {
                cmdCheck.Parameters.AddWithValue("@Dni", dniInt);
                object? result = await cmdCheck.ExecuteScalarAsync();
                if (result != null && result != DBNull.Value)
                {
                    idExistente = Convert.ToInt32(result);
                }
            }
        }

        // LÓGICA DE DECISIÓN:
        // Si encontramos ID por DNI, forzamos que sea ese ID (Modo Edición).
        // Si no encontramos ID por DNI pero Angular mandó un ID, usamos el de Angular.
        // Si no hay ninguno, entonces sí es 0 (Modo Inserción).
        
        int idFinal = (idExistente > 0) ? idExistente : (data.IdPaciente ?? 0);

        // -------------------------------------------------------------
        // 1. CASO UPDATE (Si encontramos ID previo o Angular mandó uno válido)
        // -------------------------------------------------------------
        if (idFinal > 0)
        {
            // Usamos el SP Mio_U_Paciente
            using (SqlCommand cmd = new SqlCommand("[AHS].[dbo].[Mio_U_Paciente]", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                
                cmd.Parameters.AddWithValue("@pIDPaciente", idFinal);
                
                // NOTA: Para no borrar el nombre/apellido si Angular lo manda vacío al estar bloqueado,
                // usamos el helper Str(). Pero el SP hará UPDATE. Asegúrate que Angular mande el nombre
                // aunque esté 'readonly'. (Angular SI manda los valores de inputs readonly).
                
                cmd.Parameters.AddWithValue("@pNumHistClinica", Int(data.HcNumber));
                cmd.Parameters.AddWithValue("@pApellido", Str(data.Apellido));
                cmd.Parameters.AddWithValue("@pNombre", Str(data.Nombres));
                cmd.Parameters.AddWithValue("@pIDTipoDoc", 1); // Asumiendo DNI es ID 1 en tu tabla TiposDoc
                cmd.Parameters.AddWithValue("@pNumeroDocumento", Int(data.Dni));
                
                // DATOS ACTUALIZABLES CLAVE:
                cmd.Parameters.AddWithValue("@pDireccion", Str(data.Direccion));

                        // IMPORTANTE: Asegurar IDLocalidad. Si 0 falla por FK, poner 1 (Default) o el que corresponda a "Resistencia"
                        cmd.Parameters.AddWithValue("@pIDLocalidad", GetIdLocalidad(data.Localidad));

                        cmd.Parameters.AddWithValue("@pFechaNac", Date(data.FechaNacimiento));
                cmd.Parameters.AddWithValue("@pSexo", Str(data.Sexo));
                cmd.Parameters.AddWithValue("@pTelefono", Str(data.Telefono));
                
                // OBRA SOCIAL Y AFILIADO (Campos NroAfiliado en tabla Pacientes)
                cmd.Parameters.AddWithValue("@pIDObraSocial", DBNull.Value); // Null porque no tenemos el ID numérico
                cmd.Parameters.AddWithValue("@pNroAfiliado", Str(data.NroObraSocial));
                
                cmd.Parameters.AddWithValue("@pNombrePadre", Str(data.PadreNombre));
                cmd.Parameters.AddWithValue("@pNombreMadre", Str(data.MadreNombre));
                
                cmd.Parameters.AddWithValue("@pIDUsuario", idUsuario);
                cmd.Parameters.AddWithValue("@UsuarioSIMod", idUsuario);
                cmd.Parameters.AddWithValue("@Obs", "Act. Web");

                await cmd.ExecuteNonQueryAsync();
                
                return idFinal; // Retornamos el ID que acabamos de actualizar
            }
        }
        
        // -------------------------------------------------------------
        // 2. CASO INSERT (Nuevo Paciente Real)
        // -------------------------------------------------------------
        else
        {
            string sqlInsert = @"
                INSERT INTO [AHS].[dbo].[Pacientes] 
                (NumHistClinica, Apellido, Nombre, IDTipoDoc, NumeroDocumento, 
                 Direccion, IDLocalidad, FechaNac, Sexo, Telefono, IDObraSocial, 
                 NroAfiliado, NombrePadre, NombreMadre, IDUsuario, FechaAlta, FechaModif)
                OUTPUT INSERTED.IDPaciente
                VALUES 
                (@Hc, @Ape, @Nom, 1, @Dni, @Dir, @IdLoc, @Fec, @Sex, @Tel, @IdOS, @NroSoc, @Padre, @Madre, @Usu, GETDATE(), GETDATE())";

            using (SqlCommand cmd = new SqlCommand(sqlInsert, conn))
            {
                cmd.Parameters.AddWithValue("@Hc", Int(data.HcNumber)); 
                cmd.Parameters.AddWithValue("@Ape", Str(data.Apellido));
                cmd.Parameters.AddWithValue("@Nom", Str(data.Nombres));
                cmd.Parameters.AddWithValue("@Dni", Int(data.Dni)); 
                cmd.Parameters.AddWithValue("@Dir", Str(data.Direccion));
                cmd.Parameters.AddWithValue("@Fec", Date(data.FechaNacimiento)); 
                cmd.Parameters.AddWithValue("@Sex", Str(data.Sexo));
                cmd.Parameters.AddWithValue("@Tel", Str(data.Telefono));
                        cmd.Parameters.AddWithValue("@pIDLocalidad", GetIdLocalidad(data.Localidad));
                        cmd.Parameters.AddWithValue("@IdOS", DBNull.Value); // Importante si hay FK
                cmd.Parameters.AddWithValue("@NroSoc", Str(data.NroObraSocial));
                cmd.Parameters.AddWithValue("@Padre", Str(data.PadreNombre));
                cmd.Parameters.AddWithValue("@Madre", Str(data.MadreNombre));
                cmd.Parameters.AddWithValue("@Usu", idUsuario);

                return (int)await cmd.ExecuteScalarAsync();
            }
        }
    }
}

        public async Task GuardarFichaCompleta(int idPaciente, SocialFormDataDto data, int idUsuario)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                var tran = conn.BeginTransaction();

                try
                {
                    // 1. CABECERA FICHA
                    using (SqlCommand cmd = new SqlCommand("[AHS].[dbo].[Mio_Mod_PacientesFicha]", conn, tran))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@IdPaciente", idPaciente);
                        cmd.Parameters.AddWithValue("@Nacion", GetIdPais(data.Nacionalidad));

                        cmd.Parameters.AddWithValue("@DocPadre", Int(data.PadreDoc));
                        cmd.Parameters.AddWithValue("@DocMadre", Int(data.MadreDoc));
                        cmd.Parameters.AddWithValue("@TelCel", Str(data.Telefono));
                        cmd.Parameters.AddWithValue("@TelFijo", Str(data.TelefonoFijo));

                        int idArea = Str(data.AreaProgramatica).Contains("Sur") ? 1 : 123;
                        cmd.Parameters.AddWithValue("@Area", idArea);

                        // Fix: Corregido para que 'Vivienda' acepte string y mapee bien
                        string v = Str(data.Vivienda);
                        int idViv = v.Contains("Casa") ? 1 : v.Contains("Departamento") ? 2 : 10;
                        cmd.Parameters.AddWithValue("@Vivienda", idViv);

                        cmd.Parameters.AddWithValue("@NroAmb", data.NumAmbientes);
                        cmd.Parameters.AddWithValue("@PersHabitan", data.PersonasHabitantes);

                        cmd.Parameters.AddWithValue("@Luz", data.GetBool(data.LuzElectrica));
                        cmd.Parameters.AddWithValue("@Basura", data.GetBool(data.Basura));
                        cmd.Parameters.AddWithValue("@Ayuda", data.GetBool(data.PlanSocial));

                        string a = Str(data.Agua);
                        int idAgua = a.Contains("Red") ? 1 : 7;
                        cmd.Parameters.AddWithValue("@Agua", idAgua);

                        cmd.Parameters.AddWithValue("@Obs", Str(data.Observaciones));
                        cmd.Parameters.AddWithValue("@Alergias", Str(data.Alergias));
                        cmd.Parameters.AddWithValue("@Correo", Str(data.Correo));

                        cmd.Parameters.AddWithValue("@Usuario", idUsuario);
                        cmd.Parameters.AddWithValue("@ObsMod", "Web");

                        await cmd.ExecuteNonQueryAsync();
                    }

                    // 2. FAMILIARES
                    if (data.Convivientes != null)
                    {
                        foreach (var fam in data.Convivientes)
                        {
                            using (SqlCommand cmdF = new SqlCommand("[AHS].[dbo].[Mio_InsMod_PacientesFichaFamiliar]", conn, tran))
                            {
                                cmdF.CommandType = CommandType.StoredProcedure;
                                cmdF.Parameters.AddWithValue("@IdFlia", fam.IdFila ?? 0);
                                cmdF.Parameters.AddWithValue("@Idpaciente", idPaciente);
                                cmdF.Parameters.AddWithValue("@ApeyNom", Str(fam.Nombre));
                                cmdF.Parameters.AddWithValue("@Documento", Int(fam.Documento)); // Helper Int
                                cmdF.Parameters.AddWithValue("@Parentesco", MapParentesco(fam.Vinculo));
                                cmdF.Parameters.AddWithValue("@Edad", Str(fam.Edad)); // Helper Str
                                cmdF.Parameters.AddWithValue("@EstadoCivil", 0);
                                cmdF.Parameters.AddWithValue("@Escolaridad", 0);
                                cmdF.Parameters.AddWithValue("@Ocupacion", MapOcupacion(fam.Ocupacion));
                                cmdF.Parameters.AddWithValue("@OcupacionObs", Str(fam.Observaciones));
                                cmdF.Parameters.AddWithValue("@Ingresos", 0.00m); // Usamos literal decimal
                                cmdF.Parameters.AddWithValue("@Usuario", idUsuario);
                                cmdF.Parameters.AddWithValue("@ObsMod", "Web Fam");

                                await cmdF.ExecuteNonQueryAsync();
                            }
                        }
                    }

                    tran.Commit();
                }
                catch (Exception ex)
                {
                    tran.Rollback();
                    // ESTO HARÁ QUE PUEDAS VER EL ERROR REAL EN LA ALERTA
                    throw new Exception($"Error SQL en {ex.TargetSite?.Name}: {ex.Message}");
                }
            }
        }


        // Obtiene el historial de todas las intervenciones diarias de este PACIENTE (No de un episodio)
        public async Task<List<object>> ObtenerHistorialIntervenciones(int idPaciente)
        {
            var lista = new List<object>();
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                // Unimos INTERVENCIONES con ADMISION para filtrar por Idpaciente
                string sql = @"
                    SELECT I.Tipo, I.Fecha, I.Observaciones, I.Tecnica 
                    FROM [dbo].[INTERVENCIONES] I
                    INNER JOIN [dbo].[ADMISION] A ON I.IdEpisodio = A.Idepisodio
                    WHERE A.Idpaciente = @Id 
                    ORDER BY I.Fecha DESC";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", idPaciente);
                    await conn.OpenAsync();
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            lista.Add(new
                            {
                                tipo = Convert.ToInt32(reader["Tipo"]),
                                fecha = Convert.ToDateTime(reader["Fecha"]).ToString("yyyy-MM-dd HH:mm"),
                                observaciones = reader["Observaciones"].ToString() ?? "",
                                tecnica = Convert.ToInt32(reader["Tecnica"])
                            });
                        }
                    }
                }
            }
            return lista;
        }

        // Obtiene el ÚLTIMO antecedente clínico cargado de cualquier episodio anterior de este PACIENTE (Con Fecha)
        public async Task<(string Detalle, string Archivo, string Direccion, string Fecha)?> ObtenerUltimoAntecedente(int idPaciente)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                // Unimos ANTEDECENTES con ADMISION y seleccionamos la FechaAntecedente
                string sql = @"
                    SELECT TOP 1 AN.Detalle, AN.Archivo, AN.Direccion, AN.FechaAntecedente
                    FROM [dbo].[ANTECEDENTES] AN
                    INNER JOIN [dbo].[ADMISION] AD ON AN.IdReferencia = AD.Idepisodio
                    WHERE AD.Idpaciente = @Id AND AN.IdServicio = 100
                    ORDER BY AN.FechaAntecedente DESC";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", idPaciente);
                    await conn.OpenAsync();
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            // Convertimos la fecha a formato string YYYY-MM-DD para el input de Angular
                            string fechaFormat = "";
                            if (reader["FechaAntecedente"] != DBNull.Value)
                            {
                                fechaFormat = Convert.ToDateTime(reader["FechaAntecedente"]).ToString("yyyy-MM-dd");
                            }

                            return (
                                reader["Detalle"] != DBNull.Value ? reader["Detalle"].ToString()! : "",
                                reader["Archivo"] != DBNull.Value ? reader["Archivo"].ToString()! : "",
                                reader["Direccion"] != DBNull.Value ? reader["Direccion"].ToString()! : "",
                                fechaFormat
                            );
                        }
                    }
                }
            }
            return null;
        }


        // ==========================================
        // HELPERS PRIVADOS (Para limpieza de datos)
        // ==========================================

        // Convierte nulo a "" para mantener valores viejos en SP
        // Texto: si es nulo devuelve cadena vacía
        private string Str(string? val) => string.IsNullOrWhiteSpace(val) ? "" : val.Trim();

        // Enteros: si falla devuelve 0
        private int Int(string? val) => int.TryParse(val, out int v) ? v : 0;

        // Decimales: si falla devuelve 0
        private decimal Dec(string? val) => decimal.TryParse(val, out decimal v) ? v : 0;

        // Fechas: ESENCIAL. Si falla o es antigua, devuelve una fecha segura para SQL (1900-01-01)
        private DateTime Date(string? val)
        {
            if (DateTime.TryParse(val, out DateTime dt))
            {
                // SQL Server 'smalldatetime' explota antes de 1900.
                if (dt.Year < 1900) return new DateTime(1900, 1, 1);
                return dt;
            }
            return new DateTime(1900, 1, 1);
        }


        private bool Bit(string? val) => val?.ToLower().Trim() == "si";

        // Mapeo Parentesco seguro
        private int MapParentesco(string? vinculo)
        {
            if (string.IsNullOrEmpty(vinculo)) return 0;
            string v = vinculo.ToLower();
            if (v.Contains("padre") || v.Contains("madre")) return 1;
            if (v.Contains("hijo")) return 2;
            if (v.Contains("herman")) return 3;
            if (v.Contains("abuel")) return 4;
            if (v.Contains("tio")) return 5;
            if (v.Contains("pareja")) return 6;
            return 99;
        }

        private int MapOcupacion(string? oc) => string.IsNullOrEmpty(oc) ? 0 : 1;

        
       
    }
}