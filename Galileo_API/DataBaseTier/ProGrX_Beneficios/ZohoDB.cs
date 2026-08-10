using Dapper;
using Galileo.Models.AF;
using Galileo.Models.ERROR;
using Galileo_Externo.Models.NewFolder;
using Microsoft.Data.SqlClient;
using System.Globalization;
using System.Net.Http.Headers;
using System.Text.Json;

namespace Galileo.DataBaseTier.ProGrX_Beneficios
{
    /// <summary>
    /// Acceso a datos de integración Zoho Desk (frmAF_Beneficios_Zoho).
    /// Consultas de tickets, sincronización, marca de visto e inclusión de tickets.
    /// </summary>
    public partial class ZohoDB
    {
        private readonly IConfiguration _config;
        private readonly FrmAfBeneficiosIntegralGenDB _beneIntegral;
        private readonly MBeneficiosDB _mBeneficiosDB;

        /// <summary>
        /// Opciones de deserialización para los modelos "Ticket" y "DataModel" de Zoho Desk: la API real
        /// devuelve JSON en camelCase (id, subject, departmentId, webUrl, createdTime, status, ticketNumber...)
        /// mientras que las clases del modelo están en PascalCase sin [JsonPropertyName]. System.Text.Json es
        /// case-sensitive por defecto (a diferencia de Newtonsoft, usado en v1), así que sin esta opción todas
        /// esas propiedades quedaban en null tras deserializar. NO usar para ZohoModel (token OAuth): sus
        /// propiedades ya están en snake_case/lowercase coincidente con la respuesta real.
        /// </summary>
        private static readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

        /// <summary>
        /// Inicializa el acceso a datos con la configuración inyectada.
        /// </summary>
        public ZohoDB(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _beneIntegral = new FrmAfBeneficiosIntegralGenDB(_config);
            _mBeneficiosDB = new MBeneficiosDB(_config);
        }

        /// <summary>
        /// Crea una instancia de acceso al portal usando la configuración inyectada.
        /// </summary>
        private PortalDB CreatePortalDb() => new(_config);

        /// <summary>
        /// Obtiene la lista paginada de tickets Zoho con filtros de búsqueda.
        /// </summary>
        public ErrorDto<AfiBeneTicketsLista> AfiBeneTicketsLista_Obtener(int CodEmpresa, string jFiltros)
        {
            return DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
            {
                var response = new AfiBeneTicketsLista();
                var filtros = System.Text.Json.JsonSerializer.Deserialize<AfiBeneTicketFiltros>(jFiltros) ?? new AfiBeneTicketFiltros();

                const string sqlCount = "SELECT COUNT(*) FROM AFI_BENE_OTORGA_INT WHERE TIPO_TRAMITE != 'Consultas Generales'";
                response.total = connection.QueryFirstOrDefault<int>(sqlCount);

                var like = string.IsNullOrWhiteSpace(filtros.filtro) ? null : $"%{filtros.filtro}%";
                var estado = filtros.estado ?? "T";
                var offset = filtros.pagina * filtros.paginacion;
                var fetch = filtros.paginacion;

                const string sql = @"SELECT ID_ZOHO as id_zoho, FECHA_CREACION as fecha_creacion, ESTADO_ZOHO as estado_zoho,
                                            WEB_URL as web_url, CATEGORIA as categoria, TIPO_TRAMITE as tipo_tramite,
                                            CEDULA as cedula, N_EXPEDIENTE as n_expediente, CONSEC as consec,
                                            COD_BENEFICIO as cod_beneficio, ID_BENEFICIO as id_beneficio,
                                            MSJ_INTERFACE as msj_interface, ESTADO as estado, CASO_ID as caso_id,
                                            I_VISTO as i_visto, I_PENDIENTE as i_pendiente, VISTO_POR as visto_por,
                                            INCLUIDO_POR as incluido_por, ENTRADA as entrada,
                                            VISTO_FECHA as visto_fecha, INCLUIDO_FECHA as incluido_fecha
                                     FROM AFI_BENE_OTORGA_INT
                                     WHERE TIPO_TRAMITE != 'Consultas Generales'
                                       AND (@like IS NULL OR ID_ZOHO LIKE @like OR CEDULA LIKE @like OR CATEGORIA LIKE @like OR TIPO_TRAMITE LIKE @like OR CASO_ID LIKE @like)
                                       AND (@estado = 'T' OR ESTADO = @estado)
                                       AND FECHA_CREACION BETWEEN @fechaInicio AND @fechaFin
                                     ORDER BY FECHA_CREACION DESC
                                     OFFSET @offset ROWS FETCH NEXT @fetch ROWS ONLY";

                response.lista = connection.Query<AfiBeneTicketsDatos>(sql, new
                {
                    like,
                    estado,
                    fechaInicio = filtros.fechaInicio,
                    fechaFin = filtros.fechaFin,
                    offset,
                    fetch
                }).ToList();

                const string sqlResumen = @"SELECT
                                            SUM(CASE WHEN I_PENDIENTE = 1 THEN 1 ELSE 0 END) AS valorPendiente,
                                            SUM(CASE WHEN ESTADO = 'E' THEN 1 ELSE 0 END) AS valorError,
                                            SUM(CASE WHEN N_EXPEDIENTE IS NOT NULL AND N_EXPEDIENTE != '' THEN 1 ELSE 0 END) AS valorIngresado
                                         FROM AFI_BENE_OTORGA_INT
                                         WHERE TIPO_TRAMITE != 'Consultas Generales'
                                           AND FECHA_CREACION BETWEEN @fechaInicio AND @fechaFin";

                var resumen = connection.QueryFirstOrDefault(sqlResumen, new
                {
                    fechaInicio = filtros.fechaInicio,
                    fechaFin = filtros.fechaFin
                });

                if (resumen != null)
                {
                    response.valorPendiente = resumen.valorPendiente ?? 0;
                    response.valorError = resumen.valorError ?? 0;
                    response.valorIngresado = resumen.valorIngresado ?? 0;
                }

                const string sqlTipos = @"SELECT TIPO_TRAMITE AS tipoTramite, COUNT(*) AS total
                                          FROM AFI_BENE_OTORGA_INT
                                          WHERE TIPO_TRAMITE != 'Consultas Generales'
                                            AND FECHA_CREACION BETWEEN @fechaInicio AND @fechaFin
                                          GROUP BY TIPO_TRAMITE";

                response.tiposTramite = connection.Query<AfiBeneTicketTipos>(sqlTipos, new
                {
                    fechaInicio = filtros.fechaInicio,
                    fechaFin = filtros.fechaFin
                }).ToList();

                return response;
            });
        }

        /// <summary>
        /// Sincroniza tickets desde la API de Zoho Desk hacia la tabla local.
        /// </summary>
        public ErrorDto Casos_Sincronizar(int CodEmpresa, DateTime fechaInicio, DateTime fechaCorte, string entrada, string usuario)
        {
            using var connection = DbHelper.OpenConnection(CreatePortalDb(), CodEmpresa);
            try
            {
                var deptoId = _config["AFI_Beneficios:DepartamentoZoho"] ?? string.Empty;

                var httpClient = new HttpClient();
                var token = ObtenerTokenZoho(httpClient, out var tokenError);

                if (string.IsNullOrEmpty(token))
                {
                    return DbHelper.ErrorResponse(string.IsNullOrEmpty(tokenError)
                        ? "No se pudo obtener token de Zoho Desk"
                        : $"No se pudo obtener token de Zoho Desk: {tokenError}");
                }

                var tickets = ObtenerTicketsZoho(httpClient, token, deptoId, fechaInicio, fechaCorte);

                var insertados = 0;
                var actualizados = 0;

                foreach (var ticket in tickets)
                {
                    const string sqlExiste = "SELECT COUNT(*) FROM AFI_BENE_OTORGA_INT WHERE ID_ZOHO = @id_zoho";
                    var existe = connection.QueryFirstOrDefault<int>(sqlExiste, new { ticket.id_zoho });

                    if (existe > 0)
                    {
                        const string sqlUpdate = @"UPDATE AFI_BENE_OTORGA_INT
                                                   SET ESTADO_ZOHO = @estado_zoho, WEB_URL = @web_url,
                                                       CATEGORIA = @categoria, TIPO_TRAMITE = @tipo_tramite,
                                                       CEDULA = @cedula
                                                   WHERE ID_ZOHO = @id_zoho";
                        connection.Execute(sqlUpdate, new
                        {
                            ticket.estado_zoho,
                            ticket.web_url,
                            ticket.categoria,
                            ticket.tipo_tramite,
                            ticket.cedula,
                            ticket.id_zoho
                        });
                        actualizados++;
                    }
                    else
                    {
                        const string sqlInsert = @"INSERT INTO AFI_BENE_OTORGA_INT
                                                    (ID_ZOHO, FECHA_CREACION, ESTADO_ZOHO, WEB_URL, CATEGORIA, TIPO_TRAMITE,
                                                     CEDULA, N_EXPEDIENTE, CONSEC, COD_BENEFICIO, ID_BENEFICIO, MSJ_INTERFACE,
                                                     ESTADO, CASO_ID, I_VISTO, I_PENDIENTE, ENTRADA)
                                                   VALUES
                                                    (@id_zoho, @fecha_creacion, @estado_zoho, @web_url, @categoria, @tipo_tramite,
                                                     @cedula, NULL, NULL, NULL, NULL, '',
                                                     'P', @caso_id, 0, 0, @entrada)";
                        connection.Execute(sqlInsert, new
                        {
                            ticket.id_zoho,
                            ticket.fecha_creacion,
                            ticket.estado_zoho,
                            ticket.web_url,
                            ticket.categoria,
                            ticket.tipo_tramite,
                            ticket.cedula,
                            ticket.caso_id,
                            entrada
                        });
                        insertados++;
                    }
                }

                return DbHelper.OkResponse($"Sincronización completada: {insertados} insertados, {actualizados} actualizados");
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        /// <summary>
        /// Obtiene los nombres de campos custom de Zoho para homologación.
        /// </summary>
        public ErrorDto<List<string>> CamposCustom_Obtener(int CodEmpresa)
        {
            return DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
            {
                const string sql = "SELECT campo_homologado FROM AFI_BENE_ZOHO_CAMPOS_CUSTOM WHERE activo = 1";
                return connection.Query<string>(sql).ToList();
            });
        }

        /// <summary>
        /// Marca un ticket como visto por un usuario.
        /// </summary>
        public ErrorDto MarcaVisto_Actualizar(int CodEmpresa, string ticket, string visto, string usuario)
        {
            const string sql = @"UPDATE AFI_BENE_OTORGA_INT
                                 SET I_VISTO = @i_visto, VISTO_POR = @visto_por, VISTO_FECHA = GETDATE()
                                 WHERE ID_ZOHO = @id_zoho";

            var parametros = new
            {
                i_visto = visto == "1" || visto.ToLower() == "true",
                visto_por = usuario,
                id_zoho = ticket
            };

            return DbHelper.ExecuteNonQuery(CreatePortalDb(), CodEmpresa, sql, parametros);
        }

        /// <summary>
        /// Retorna el conteo de tickets pendientes para el badge del toolbar.
        /// </summary>
        public ErrorDto<int> TicketsContador_Obtener(int CodEmpresa)
        {
            return DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
            {
                const string sql = @"SELECT COUNT(ID_ZOHO) FROM AFI_BENE_OTORGA_INT
                                     WHERE (I_VISTO IS NULL OR I_VISTO = 0)
                                       AND N_EXPEDIENTE IS NULL
                                       AND TIPO_TRAMITE != 'Consultas Generales'";
                return connection.QueryFirstOrDefault<int>(sql);
            });
        }

        /// <summary>
        /// Importa un ticket de Zoho como registro de beneficio en ProGrX: obtiene el ticket completo
        /// de Zoho Desk, despacha según tipo de trámite al método que crea el beneficio real
        /// (Apremiante/Sepelios/Desastres/FENA/Reconocimientos) y actualiza el estado del ticket.
        /// Puerto de PgxAPI_Externo.DataBaseTier.InterfaceZoho.ZohoDB.IncluirTicket_Guardar (v1).
        /// </summary>
        public ErrorDto IncluirTicket_Guardar(int CodEmpresa, ZohoTicketAdd jsonZoho)
        {
            if (string.IsNullOrEmpty(jsonZoho.ticket))
            {
                return DbHelper.ErrorResponse("El ticket es requerido");
            }

            var usuario = jsonZoho.usuario ?? string.Empty;

            try
            {
                using var httpClient = new HttpClient();
                var token = ObtenerTokenZoho(httpClient, out var tokenError);

                if (string.IsNullOrEmpty(token))
                {
                    return DbHelper.ErrorResponse(string.IsNullOrEmpty(tokenError)
                        ? "No se pudo obtener token de Zoho Desk"
                        : $"No se pudo obtener token de Zoho Desk: {tokenError}");
                }

                var ticket = ObtenerTicketPorId(httpClient, token, jsonZoho.ticket);
                if (ticket == null)
                {
                    return DbHelper.ErrorResponse("Error al obtener el ticket de Zoho Desk");
                }

                var error = Expediente_Guardar(CodEmpresa, ticket, usuario, jsonZoho);

                if (error.Code == -1)
                {
                    ActualizaError(CodEmpresa, jsonZoho.ticket, error.Description ?? string.Empty, usuario);
                    return DbHelper.ErrorResponse(error.Description ?? "Error al procesar el ticket");
                }

                const string sql = @"UPDATE AFI_BENE_OTORGA_INT
                                     SET VISTO_POR = @usuario, I_VISTO = 1, VISTO_FECHA = GETDATE(), I_PENDIENTE = 1,
                                         INCLUIDO_POR = @usuario, INCLUIDO_FECHA = GETDATE()
                                     WHERE ID_ZOHO = @ticket";
                DbHelper.ExecuteNonQuery(CreatePortalDb(), CodEmpresa, sql, new { usuario, ticket = jsonZoho.ticket });

                return DbHelper.OkResponse("Ticket Guardado");
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        /// <summary>
        /// Obtiene un ticket puntual de Zoho Desk por su Id (GET /tickets/{id}).
        /// </summary>
        private static Ticket? ObtenerTicketPorId(HttpClient httpClient, string token, string ticketId)
        {
            try
            {
                httpClient.DefaultRequestHeaders.Remove("orgId");
                httpClient.DefaultRequestHeaders.Add("orgId", "691715214");
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Zoho-oauthtoken", token);

                var url = $"https://desk.zoho.com/api/v1/tickets/{ticketId}";
                var response = httpClient.GetAsync(url).Result;

                if (!response.IsSuccessStatusCode)
                {
                    return null;
                }

                var body = response.Content.ReadAsStringAsync().Result;
                return JsonSerializer.Deserialize<Ticket>(body, _jsonOptions);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Despacha el guardado del expediente según el tipo de trámite del ticket (campo custom
        /// cf_tipo_de_tramite_2) al método que crea el beneficio real correspondiente.
        /// </summary>
        private ErrorDto Expediente_Guardar(int CodEmpresa, Ticket ticket, string usuario, ZohoTicketAdd jsonZoho)
        {
            var response = new ErrorDto { Code = 0 };

            try
            {
                var datos = ParseCf(ticket.cf) ?? new Dictionary<string, JsonElement>();
                var tipoTramite = CfStr(datos, "cf_tipo_de_tramite_2");

                response = tipoTramite switch
                {
                    "Apremiante" => Apremiante_Guardar(CodEmpresa, ticket, datos, usuario, jsonZoho),
                    "Sepelios" => Sepelios_Guardar(CodEmpresa, ticket, datos, usuario, jsonZoho),
                    "Desastres" => Desastres_Guardar(CodEmpresa, ticket, datos, usuario, jsonZoho),
                    "FENA" => FENA_Guardar(CodEmpresa, ticket, datos, usuario, jsonZoho),
                    "Reconocimientos" => Reconocimientos_Guardar(CodEmpresa, ticket, datos, usuario, jsonZoho),
                    _ => response
                };
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }

        /// <summary>
        /// Guarda el expediente de un ticket de tipo Apremiante.
        /// </summary>
        private ErrorDto Apremiante_Guardar(int CodEmpresa, Ticket ticket, Dictionary<string, JsonElement> datos, string usuario, ZohoTicketAdd jsonZoho)
        {
            var response = new ErrorDto { Code = 0 };
            var msjError = string.Empty;

            try
            {
                using var connection = DbHelper.OpenConnection(CreatePortalDb(), CodEmpresa);

                var cedula = CfStr(datos, "cf_numero_de_cedula");
                if (string.IsNullOrEmpty(cedula))
                {
                    response.Code = -1;
                    msjError += "Cédula no puede ser nula...";
                }

                var validaPersona = _mBeneficiosDB.ValidarPersona(CodEmpresa, (cedula ?? string.Empty).Trim(), null);
                if (validaPersona.Code == -1)
                {
                    response.Code = -1;
                    msjError += validaPersona.Description + "...";
                }

                if (response.Code != -1)
                {
                    var codBeneficio = connection.QueryFirstOrDefault<string>(
                        "SELECT TOP 1 COD_BENEFICIO FROM AFI_BENEFICIOS WHERE COD_CATEGORIA = 'B_APRE'") ?? string.Empty;

                    var beneficio = new BeneficioGeneralDatos
                    {
                        cod_beneficio = new AfBeneficioIntegralDropsLista { item = codBeneficio },
                        id_beneficio = 0,
                        cedula = (cedula ?? string.Empty).Trim(),
                        monto_aplicado = 0,
                        registra_user = usuario,
                        // NOTA: v1 no seteaba modifica_usuario en este flujo (Guarda_Beneficio directo, sin
                        // validación de permisos). Se setea aquí porque BeneficioIntegralGeneral_Guardar (v2)
                        // sí ejecuta fxBeneficio_ValidacionPermisos(usuario, cod_categoria, estado) antes de
                        // guardar. Ver reporte final: riesgo de que el usuario del botón "Importar" no tenga
                        // el permiso que antes no se exigía.
                        modifica_usuario = usuario,
                        sepelio_identificacion = null,
                        estado = new AfBeneficioIntegralDropsLista { item = string.Empty },
                        consec = 0,
                        requiere_justificacion = jsonZoho.justificacion != null,
                        notas = jsonZoho.justificacion ?? string.Empty
                    };

                    beneficio.monto = connection.QueryFirstOrDefault<float>(@"SELECT [MONTO]
                                  FROM [AFI_BENE_GRUPOS] WHERE COD_CATEGORIA = 'B_APRE'
                                  AND COD_GRUPO in (
                                      SELECT COD_GRUPO
                                      FROM [AFI_BENEFICIOS] WHERE COD_CATEGORIA = 'B_APRE'
                                      AND COD_BENEFICIO = @codBeneficio
                                  )", new { codBeneficio });

                    beneficio.tipo = new AfBeneficioIntegralDropsLista { item = "A" };

                    var respBeneficio = _beneIntegral.BeneficioIntegralGeneral_Guardar(CodEmpresa, "API", beneficio).Result;

                    if (respBeneficio.Code == -1)
                    {
                        response.Code = -1;
                        msjError += respBeneficio.Description + "...";
                    }
                    else
                    {
                        var expediente = (respBeneficio.Description ?? "0@0").Split('@');
                        var nExpediente = expediente[0].PadLeft(6, '0') + codBeneficio.Trim() + expediente[1].PadLeft(6, '0');

                        connection.Execute(@"UPDATE [dbo].[AFI_BENE_OTORGA_INT]
                                   SET [N_EXPEDIENTE] = @nExpediente, [CONSEC] = @consec, COD_BENEFICIO = @codBeneficio,
                                       ID_BENEFICIO = @idBeneficio, I_PENDIENTE = 1, I_VISTO = 1, VISTO_POR = @usuario, VISTO_FECHA = getdate(),
                                       [ESTADO] = 'S', INCLUIDO_POR = @usuario, INCLUIDO_FECHA = getdate()
                                 WHERE ID_ZOHO = @idZoho",
                            new { nExpediente, consec = expediente[1], codBeneficio = codBeneficio.Trim(), idBeneficio = expediente[0], usuario, idZoho = jsonZoho.ticket });

                        // TODO: BuscaArchivos (adjuntos de threads de Zoho Desk) no está portado en v2:
                        // falta la infraestructura HTTP de threads/attachments. Ver reporte final.

                        if (expediente[0] != "0")
                        {
                            var filtros = new FrmFiltros
                            {
                                codCliente = CodEmpresa,
                                cod_beneficio = codBeneficio.Trim(),
                                id_beneficio = beneficio.id_beneficio,
                                socio = beneficio.cedula,
                                usuario = usuario
                            };

                            IncluirRespuestasFormularios(filtros, datos);
                        }
                    }
                }

                if (msjError.Trim() != string.Empty)
                {
                    response.Code = -1;
                    response.Description = msjError;

                    connection.Execute(@"UPDATE [dbo].[AFI_BENE_OTORGA_INT]
                               SET [MSJ_INTERFACE] = @msjError, [ESTADO] = 'E', VISTO_POR = @usuario, VISTO_FECHA = getdate()
                             WHERE ID_ZOHO = @idZoho", new { msjError, usuario, idZoho = jsonZoho.ticket });
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }

        /// <summary>
        /// Guarda el expediente de un ticket de tipo Sepelios.
        /// </summary>
        private ErrorDto Sepelios_Guardar(int CodEmpresa, Ticket ticket, Dictionary<string, JsonElement> datos, string usuario, ZohoTicketAdd jsonZoho)
        {
            var response = new ErrorDto { Code = 0 };
            var msjError = string.Empty;

            try
            {
                using var connection = DbHelper.OpenConnection(CreatePortalDb(), CodEmpresa);

                var cedula = CfStr(datos, "cf_numero_de_cedula");
                if (string.IsNullOrEmpty(cedula))
                {
                    response.Code = -1;
                    response.Description = "Cédula no puede ser nula...";
                    return response;
                }

                var estadoSocio = _mBeneficiosDB.ValidaEstadoSocio(CodEmpresa, cedula.Trim());
                if (estadoSocio.Code == -1)
                {
                    response.Code = -1;
                    msjError += estadoSocio.Description + "...";
                }

                if (response.Code != -1)
                {
                    var parentesco = (CfStr(datos, "cf_parentesco_de_la_persona_fallecida") ?? string.Empty).Trim().ToUpper();
                    var codBeneficio = string.Empty;

                    if (parentesco.Contains("PADRE")) codBeneficio = "MPAD";
                    if (parentesco.Contains("MADRE")) codBeneficio = "MMADRE";
                    if (parentesco.Contains("HIJO")) codBeneficio = "MHIJO";
                    if (parentesco.Contains("CONYUGUE")) codBeneficio = "MCON";

                    var beneficio = new BeneficioGeneralDatos
                    {
                        cod_beneficio = new AfBeneficioIntegralDropsLista { item = codBeneficio },
                        id_beneficio = 0,
                        cedula = cedula.Trim(),
                        monto_aplicado = 0,
                        registra_user = usuario,
                        modifica_usuario = usuario,
                        sepelio_identificacion = CfStr(datos, "cf_numero_de_identificacion_de_persona_fallecida")?.Trim(),
                        sepelio_nombre = CfStr(datos, "cf_nombre_completo_de_persona_fallecida")?.Trim(),
                        estado = new AfBeneficioIntegralDropsLista { item = string.Empty },
                        consec = 0,
                        requiere_justificacion = jsonZoho.justificacion != null,
                        notas = jsonZoho.justificacion ?? string.Empty
                    };

                    var fechaDefuncion = CfStr(datos, "cf_fecha_de_la_defuncion");
                    if (fechaDefuncion != null && DateTime.TryParse(fechaDefuncion, out var fDefuncion))
                    {
                        beneficio.sepelio_fecha_fallecimiento = fDefuncion;
                    }

                    beneficio.monto = connection.QueryFirstOrDefault<float>(@"SELECT [MONTO]
                              FROM [AFI_BENE_GRUPOS] WHERE COD_CATEGORIA = 'B_SEPE'
                              AND COD_GRUPO in (
                                  SELECT COD_GRUPO
                                  FROM [AFI_BENEFICIOS] WHERE COD_CATEGORIA = 'B_SEPE'
                                  AND COD_BENEFICIO = @codBeneficio
                              )", new { codBeneficio });
                    beneficio.monto_aplicado = beneficio.monto;

                    beneficio.tipo = new AfBeneficioIntegralDropsLista { item = "M" };

                    var respBeneficio = _beneIntegral.BeneficioIntegralGeneral_Guardar(CodEmpresa, "API", beneficio).Result;

                    if (respBeneficio.Code == -1)
                    {
                        response.Code = -1;
                        msjError += respBeneficio.Description + "...";
                    }
                    else
                    {
                        var expediente = (respBeneficio.Description ?? "0@0").Split('@');
                        var nExpediente = expediente[0].PadLeft(6, '0') + codBeneficio.Trim() + expediente[1].PadLeft(6, '0');

                        connection.Execute(@"UPDATE [dbo].[AFI_BENE_OTORGA_INT]
                                   SET [N_EXPEDIENTE] = @nExpediente, [CONSEC] = @consec, COD_BENEFICIO = @codBeneficio,
                                       ID_BENEFICIO = @idBeneficio, [ESTADO] = 'S', INCLUIDO_POR = @usuario, INCLUIDO_FECHA = getdate()
                                 WHERE ID_ZOHO = @idZoho",
                            new { nExpediente, consec = expediente[1], codBeneficio = codBeneficio.Trim(), idBeneficio = expediente[0], usuario, idZoho = jsonZoho.ticket });

                        // TODO: BuscaArchivos (adjuntos de Zoho Desk) no está portado en v2.

                        if (expediente[0] != "0")
                        {
                            var filtros = new FrmFiltros
                            {
                                codCliente = CodEmpresa,
                                cod_beneficio = codBeneficio.Trim(),
                                id_beneficio = beneficio.id_beneficio,
                                socio = beneficio.cedula,
                                usuario = usuario
                            };

                            IncluirRespuestasFormularios(filtros, datos);
                        }
                    }
                }

                if (msjError.Trim() != string.Empty)
                {
                    response.Code = -1;
                    response.Description = msjError;

                    connection.Execute(@"UPDATE [dbo].[AFI_BENE_OTORGA_INT]
                               SET [MSJ_INTERFACE] = @msjError, [ESTADO] = 'E', VISTO_POR = @usuario, VISTO_FECHA = getdate()
                             WHERE ID_ZOHO = @idZoho", new { msjError, usuario, idZoho = jsonZoho.ticket });
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }

        /// <summary>
        /// Guarda el expediente de un ticket de tipo Desastres (Natural / No Natural).
        /// </summary>
        private ErrorDto Desastres_Guardar(int CodEmpresa, Ticket ticket, Dictionary<string, JsonElement> datos, string usuario, ZohoTicketAdd jsonZoho)
        {
            var response = new ErrorDto { Code = 0 };
            var msjError = string.Empty;

            try
            {
                using var connection = DbHelper.OpenConnection(CreatePortalDb(), CodEmpresa);

                var cedula = CfStr(datos, "cf_numero_de_cedula");
                if (string.IsNullOrEmpty(cedula))
                {
                    response.Code = -1;
                    msjError += "Cédula no puede ser nula...";
                }

                var estadoSocio = _mBeneficiosDB.ValidaEstadoSocio(CodEmpresa, (cedula ?? string.Empty).Trim());
                if (estadoSocio.Code == -1)
                {
                    response.Code = -1;
                    msjError += estadoSocio.Description + "...";
                }

                var datosCat = new BeneficioGeneralDatos();
                var tipoDesastre1 = CfStr(datos, "cf_tipo_de_desastre_1");

                if (tipoDesastre1 != null)
                {
                    datosCat.cod_categoria = tipoDesastre1 == "Natural" ? "B_DESA" : "B_DESN";
                }
                else
                {
                    var tipoNoNatural = CfStr(datos, "cf_tipo_desastre_no_natural_acontecio_en_su_vivienda");
                    if (tipoNoNatural != null)
                    {
                        // NOTA: v1 tenía hardcodeado el nombre de base de datos "[ASECCSS].[dbo]." en esta
                        // consulta; se corrige aquí para usar la conexión dinámica por CodEmpresa (dbo.),
                        // consistente con el resto de este archivo.
                        datosCat = connection.QueryFirstOrDefault<BeneficioGeneralDatos>(
                            "SELECT COD_CATEGORIA FROM AFI_BENEFICIOS WHERE UPPER(DESCRIPCION) = UPPER(@descripcion)",
                            new { descripcion = tipoNoNatural }) ?? new BeneficioGeneralDatos();
                    }
                }

                var tipoDesastreDesc = CfStr(datos, "cf_indique_que_tipo_de_desastre") ?? string.Empty;

                var codBeneficio = connection.QueryFirstOrDefault<string>(
                    "SELECT COD_BENEFICIO FROM AFI_BENEFICIOS WHERE COD_CATEGORIA = @categoria AND UPPER(DESCRIPCION) LIKE @desc",
                    new { categoria = datosCat.cod_categoria, desc = $"%{tipoDesastreDesc.ToUpper()}%" }) ?? string.Empty;

                var beneficio = new BeneficioGeneralDatos
                {
                    id_beneficio = 0,
                    cod_beneficio = new AfBeneficioIntegralDropsLista { item = codBeneficio },
                    cedula = (cedula ?? string.Empty).Trim(),
                    desa_nombre = tipoDesastreDesc,
                    desa_descripcion = tipoDesastreDesc,
                    monto_aplicado = 0,
                    registra_user = usuario,
                    modifica_usuario = usuario,
                    estado = new AfBeneficioIntegralDropsLista { item = string.Empty },
                    consec = 0,
                    requiere_justificacion = jsonZoho.justificacion != null,
                    notas = jsonZoho.justificacion ?? string.Empty
                };

                beneficio.monto = connection.QueryFirstOrDefault<float>(@"SELECT [MONTO]
                      FROM [AFI_BENE_GRUPOS] WHERE COD_CATEGORIA = @categoria
                      AND COD_GRUPO in (
                          SELECT COD_GRUPO
                          FROM [AFI_BENEFICIOS] WHERE COD_CATEGORIA = @categoria
                          AND COD_BENEFICIO = @codBeneficio
                      )", new { categoria = datosCat.cod_categoria, codBeneficio });
                beneficio.monto_aplicado = beneficio.monto;

                beneficio.tipo = new AfBeneficioIntegralDropsLista { item = "M" };

                var respBeneficio = _beneIntegral.BeneficioIntegralGeneral_Guardar(CodEmpresa, "API", beneficio).Result;

                if (respBeneficio.Code == -1)
                {
                    response.Code = -1;
                    msjError += respBeneficio.Description + "...";
                }
                else
                {
                    var expediente = (respBeneficio.Description ?? "0@0").Split('@');
                    var nExpediente = expediente[0].PadLeft(6, '0') + codBeneficio.Trim() + expediente[1].PadLeft(6, '0');

                    connection.Execute(@"UPDATE [dbo].[AFI_BENE_OTORGA_INT]
                           SET [N_EXPEDIENTE] = @nExpediente, [CONSEC] = @consec, COD_BENEFICIO = @codBeneficio,
                               ID_BENEFICIO = @idBeneficio, [ESTADO] = 'S', INCLUIDO_POR = @usuario, INCLUIDO_FECHA = getdate()
                         WHERE ID_ZOHO = @idZoho",
                        new { nExpediente, consec = expediente[1], codBeneficio = codBeneficio.Trim(), idBeneficio = expediente[0], usuario, idZoho = jsonZoho.ticket });

                    // TODO: BuscaArchivos (adjuntos de Zoho Desk) no está portado en v2.

                    if (expediente[0] != "0")
                    {
                        var filtros = new FrmFiltros
                        {
                            codCliente = CodEmpresa,
                            cod_beneficio = datosCat.cod_categoria,
                            id_beneficio = beneficio.id_beneficio,
                            socio = beneficio.cedula,
                            usuario = usuario
                        };

                        IncluirRespuestasFormularios(filtros, datos);
                    }
                }

                if (msjError.Trim() != string.Empty)
                {
                    response.Code = -1;
                    response.Description = msjError;

                    connection.Execute(@"UPDATE [dbo].[AFI_BENE_OTORGA_INT]
                           SET [MSJ_INTERFACE] = @msjError, [ESTADO] = 'E'
                         WHERE ID_ZOHO = @idZoho", new { msjError, idZoho = jsonZoho.ticket });
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }

        /// <summary>
        /// Guarda el expediente de un ticket de tipo FENA.
        /// </summary>
        private ErrorDto FENA_Guardar(int CodEmpresa, Ticket ticket, Dictionary<string, JsonElement> datos, string usuario, ZohoTicketAdd jsonZoho)
        {
            var response = new ErrorDto { Code = 0 };
            var msjError = string.Empty;

            try
            {
                using var connection = DbHelper.OpenConnection(CreatePortalDb(), CodEmpresa);

                var cedula = CfStr(datos, "cf_numero_de_cedula");
                if (string.IsNullOrEmpty(cedula))
                {
                    response.Code = -1;
                    msjError += "Cédula no puede ser nula...";
                }

                if (response.Code != -1)
                {
                    var codBeneficio = connection.QueryFirstOrDefault<string>(
                        "SELECT TOP 1 COD_BENEFICIO FROM AFI_BENEFICIOS WHERE COD_CATEGORIA = 'B_FENA'") ?? string.Empty;

                    var beneficio = new BeneficioGeneralDatos
                    {
                        cod_beneficio = new AfBeneficioIntegralDropsLista { item = codBeneficio },
                        id_beneficio = 0,
                        cedula = (cedula ?? string.Empty).Trim(),
                        monto_aplicado = 0,
                        registra_user = usuario,
                        modifica_usuario = usuario,
                        sepelio_identificacion = null,
                        estado = new AfBeneficioIntegralDropsLista { item = "S" },
                        consec = 0,
                        requiere_justificacion = jsonZoho.justificacion != null,
                        notas = jsonZoho.justificacion ?? string.Empty
                    };

                    beneficio.monto = connection.QueryFirstOrDefault<float>(@"SELECT [MONTO]
                                  FROM [AFI_BENE_GRUPOS] WHERE COD_CATEGORIA = 'B_FENA'
                                  AND COD_GRUPO in (
                                      SELECT COD_GRUPO
                                      FROM [AFI_BENEFICIOS] WHERE COD_CATEGORIA = 'B_FENA'
                                      AND COD_BENEFICIO = @codBeneficio
                                  )", new { codBeneficio });

                    beneficio.tipo = new AfBeneficioIntegralDropsLista { item = "M" };

                    var respBeneficio = _beneIntegral.BeneficioIntegralGeneral_Guardar(CodEmpresa, "API", beneficio).Result;

                    if (respBeneficio.Code == -1)
                    {
                        response.Code = -1;
                        msjError += respBeneficio.Description + "...";
                    }
                    else
                    {
                        var expediente = (respBeneficio.Description ?? "0@0").Split('@');
                        var nExpediente = expediente[0].PadLeft(6, '0') + codBeneficio.Trim() + expediente[1].PadLeft(6, '0');

                        connection.Execute(@"UPDATE [dbo].[AFI_BENE_OTORGA_INT]
                               SET [N_EXPEDIENTE] = @nExpediente, [CONSEC] = @consec, COD_BENEFICIO = @codBeneficio,
                                   ID_BENEFICIO = @idBeneficio, [ESTADO] = 'S', INCLUIDO_POR = @usuario, INCLUIDO_FECHA = getdate()
                             WHERE ID_ZOHO = @idZoho",
                            new { nExpediente, consec = expediente[1], codBeneficio = codBeneficio.Trim(), idBeneficio = expediente[0], usuario, idZoho = jsonZoho.ticket });

                        // TODO: BuscaArchivos (adjuntos de Zoho Desk) no está portado en v2.

                        if (expediente[0] != "0")
                        {
                            var filtros = new FrmFiltros
                            {
                                codCliente = CodEmpresa,
                                cod_beneficio = codBeneficio.Trim(),
                                id_beneficio = beneficio.id_beneficio,
                                socio = beneficio.cedula,
                                usuario = usuario
                            };

                            IncluirRespuestasFormularios(filtros, datos);
                        }
                    }
                }

                if (msjError.Trim() != string.Empty)
                {
                    response.Code = -1;
                    response.Description = msjError;

                    connection.Execute(@"UPDATE [dbo].[AFI_BENE_OTORGA_INT]
                               SET [MSJ_INTERFACE] = @msjError, [ESTADO] = 'E'
                             WHERE ID_ZOHO = @idZoho", new { msjError, idZoho = jsonZoho.ticket });
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }

        /// <summary>
        /// Guarda el expediente de un ticket de tipo Reconocimientos e inserta el registro de reconocimiento asociado.
        /// </summary>
        private ErrorDto Reconocimientos_Guardar(int CodEmpresa, Ticket ticket, Dictionary<string, JsonElement> datos, string usuario, ZohoTicketAdd jsonZoho)
        {
            var response = new ErrorDto { Code = 0 };
            var msjError = string.Empty;

            try
            {
                using var connection = DbHelper.OpenConnection(CreatePortalDb(), CodEmpresa);

                var cedula = CfStr(datos, "cf_numero_de_cedula");
                if (string.IsNullOrEmpty(cedula))
                {
                    response.Code = -1;
                    msjError += "Cédula no puede ser nula...";
                }

                var estadoSocio = _mBeneficiosDB.ValidaEstadoSocio(CodEmpresa, (cedula ?? string.Empty).Trim());
                if (estadoSocio.Code == -1)
                {
                    response.Code = -1;
                    msjError += estadoSocio.Description + "...";
                }

                if (response.Code != -1)
                {
                    var reconocimiento = (CfStr(datos, "cf_tipo_de_reconocimiento") ?? string.Empty).Trim();
                    var codBeneficio = reconocimiento switch
                    {
                        "Académico" => "MEAC",
                        "Científico" => "MERC",
                        "Artístico" => "MERA",
                        "Deportivo" => "MERD",
                        _ => string.Empty
                    };

                    var beneficio = new BeneficioGeneralDatos
                    {
                        cod_beneficio = new AfBeneficioIntegralDropsLista { item = codBeneficio },
                        id_beneficio = 0,
                        cedula = (cedula ?? string.Empty).Trim(),
                        monto_aplicado = 0,
                        registra_user = usuario,
                        modifica_usuario = usuario,
                        sepelio_identificacion = null,
                        estado = new AfBeneficioIntegralDropsLista { item = string.Empty },
                        consec = 0,
                        requiere_justificacion = jsonZoho.justificacion != null,
                        notas = jsonZoho.justificacion ?? string.Empty
                    };

                    beneficio.monto = connection.QueryFirstOrDefault<float>(@"SELECT [MONTO]
                                  FROM [AFI_BENE_GRUPOS] WHERE COD_CATEGORIA = 'B_RECO'
                                  AND COD_GRUPO in (
                                      SELECT COD_GRUPO
                                      FROM [AFI_BENEFICIOS] WHERE COD_CATEGORIA = 'B_RECO'
                                      AND COD_BENEFICIO = @codBeneficio
                                  )", new { codBeneficio });
                    beneficio.monto_aplicado = beneficio.monto;
                    beneficio.tipo = new AfBeneficioIntegralDropsLista { item = "M" };

                    var respBeneficio = _beneIntegral.BeneficioIntegralGeneral_Guardar(CodEmpresa, "API", beneficio).Result;

                    if (respBeneficio.Code == -1)
                    {
                        response.Code = -1;
                        msjError += respBeneficio.Description + "...";
                    }
                    else
                    {
                        var expediente = (respBeneficio.Description ?? "0@0").Split('@');
                        var nExpediente = expediente[0].PadLeft(6, '0') + codBeneficio.Trim() + expediente[1].PadLeft(6, '0');

                        connection.Execute(@"UPDATE [dbo].[AFI_BENE_OTORGA_INT]
                                   SET [N_EXPEDIENTE] = @nExpediente, [CONSEC] = @consec, COD_BENEFICIO = @codBeneficio,
                                       ID_BENEFICIO = @idBeneficio, [ESTADO] = 'S', INCLUIDO_POR = @usuario, INCLUIDO_FECHA = getdate()
                                 WHERE ID_ZOHO = @idZoho",
                            new { nExpediente, consec = expediente[1], codBeneficio = codBeneficio.Trim(), idBeneficio = expediente[0], usuario, idZoho = jsonZoho.ticket });

                        if (expediente[0] != "0")
                        {
                            var afiReconocimientos = new FrmAfBeneficiosIntegralRecDB(_config);
                            var reconocimientoDatos = new AfiBeneReconocimientos
                            {
                                id_beneficio = Convert.ToInt32(expediente[0]),
                                consec = Convert.ToInt32(expediente[1]),
                                cod_beneficio = codBeneficio,
                                cedula_estudiante = (CfStr(datos, "cf_identificacion_de_estudiante") ?? string.Empty).Trim()
                            };

                            var nombreEstudiante = CfStr(datos, "cf_nombre_de_estudiantes")?.Trim()
                                ?? CfStr(datos, "cf_nombre_y_apellidos_de_estudiante")?.Trim()
                                ?? string.Empty;

                            var nombres = nombreEstudiante.Split(' ');
                            if (nombres.Length > 1)
                            {
                                reconocimientoDatos.nombre = nombres[0].Trim();
                                reconocimientoDatos.primer_apellido = nombres.Length > 1 ? nombres[1].Trim() : null;
                                reconocimientoDatos.segundo_apellido = nombres.Length > 2 ? nombres[2].Trim() : null;
                            }
                            else
                            {
                                reconocimientoDatos.nombre = nombreEstudiante;
                            }

                            var fechaNacEstudiante = CfStr(datos, "cf_fecha_nacimiento_del_estudiante");
                            reconocimientoDatos.fecha_nacimiento = fechaNacEstudiante != null && DateTime.TryParse(fechaNacEstudiante.Trim(), out var fNac)
                                ? fNac
                                : DateTime.Now;

                            var genero = (CfStr(datos, "cf_genero") ?? string.Empty).Trim();
                            reconocimientoDatos.genero = genero switch
                            {
                                "Masculino" => new AfBeneficioIntegralDropsLista { item = "M", descripcion = "Masculino" },
                                "Femenino" => new AfBeneficioIntegralDropsLista { item = "F", descripcion = "Femenino" },
                                _ => new AfBeneficioIntegralDropsLista { item = "O", descripcion = "Otro" }
                            };

                            reconocimientoDatos.edad = DateTime.Now.Year - reconocimientoDatos.fecha_nacimiento.Value.Year;

                            var centroEducativo = (CfStr(datos, "cf_tipo_de_centro_educativo") ?? string.Empty).Trim();
                            reconocimientoDatos.tipo_centro = centroEducativo switch
                            {
                                "Privado" => new AfBeneficioIntegralDropsLista { item = "PR", descripcion = "Privado" },
                                "Público" => new AfBeneficioIntegralDropsLista { item = "PU", descripcion = "Público" },
                                _ => new AfBeneficioIntegralDropsLista()
                            };

                            reconocimientoDatos.nivel_academico = new AfBeneficioIntegralDropsLista { item = (CfStr(datos, "cf_grado_cursado_en_el_presente_ano") ?? string.Empty).Trim() };
                            reconocimientoDatos.grado = new AfBeneficioIntegralDropsLista { item = (CfStr(datos, "cf_grado_cursado_el_ano_anterior") ?? string.Empty).Trim() };

                            reconocimientoDatos.tipo_reconocimiento = new AfBeneficioIntegralDropsLista
                            {
                                item = codBeneficio switch
                                {
                                    "MEAC" => "AC",
                                    "MERC" => "CI",
                                    "MERA" => "CUA",
                                    "MERD" => "DE",
                                    _ => string.Empty
                                }
                            };

                            reconocimientoDatos.matematicas = ParseIntCf(datos, "cf_promedio_matematica");
                            reconocimientoDatos.ciencias = ParseIntCf(datos, "cf_promedio_ciencia_as");
                            reconocimientoDatos.estudios_sociales = ParseIntCf(datos, "cf_promedio_estudios_sociales");
                            reconocimientoDatos.espanol = ParseIntCf(datos, "cf_promedio_espanol");
                            reconocimientoDatos.idioma = ParseIntCf(datos, "cf_promedio_un_idioma_secundaria");
                            reconocimientoDatos.centro_educativo = (CfStr(datos, "cf_nombre_del_centro_educativo") ?? string.Empty).Trim();
                            reconocimientoDatos.registro_usuario = usuario;

                            afiReconocimientos.BeneReconocimiento_Ingresar(CodEmpresa, reconocimientoDatos);
                        }

                        // TODO: BuscaArchivos (adjuntos de Zoho Desk) no está portado en v2.

                        if (expediente[0] != "0")
                        {
                            var filtros = new FrmFiltros
                            {
                                codCliente = CodEmpresa,
                                cod_beneficio = codBeneficio.Trim(),
                                id_beneficio = beneficio.id_beneficio,
                                socio = beneficio.cedula,
                                usuario = usuario
                            };

                            IncluirRespuestasFormularios(filtros, datos);
                        }
                    }
                }

                if (msjError.Trim() != string.Empty)
                {
                    response.Code = -1;
                    response.Description = msjError;

                    connection.Execute(@"UPDATE [dbo].[AFI_BENE_OTORGA_INT]
                               SET [MSJ_INTERFACE] = @msjError, [ESTADO] = 'E'
                             WHERE ID_ZOHO = @idZoho", new { msjError, idZoho = jsonZoho.ticket });
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }

        /// <summary>
        /// Actualiza el mensaje de error en la tabla de tickets.
        /// </summary>
        private ErrorDto ActualizaError(int CodEmpresa, string ticket, string error, string usuario)
        {
            const string sql = @"UPDATE AFI_BENE_OTORGA_INT SET MSJ_INTERFACE = @error,
                                     ESTADO = 'E', VISTO_POR = @usuario, I_VISTO = 1, VISTO_FECHA = getdate()
                                 WHERE ID_ZOHO = @ticket";

            return DbHelper.ExecuteNonQuery(CreatePortalDb(), CodEmpresa, sql, new { error, usuario, ticket });
        }

        /// <summary>
        /// Incluye las respuestas de los formularios homologados del beneficio en la base de datos.
        /// </summary>
        private void IncluirRespuestasFormularios(FrmFiltros filtros, Dictionary<string, JsonElement> datos)
        {
            var frmRespuestas = new FrmAfBeneFormulariosDB(_config);
            var jDatos = Newtonsoft.Json.JsonConvert.SerializeObject(filtros);

            var formularios = frmRespuestas.AfBeneFormSocios_Obtener(jDatos).Result ?? new List<Formulario>();

            foreach (var item in formularios)
            {
                var form1 = new Form
                {
                    id = item.id_form,
                    questions = item.formulario.questions
                };

                foreach (var question in item.formulario.questions ?? new List<FormQuestion>())
                {
                    var requerido = question.requerido == true;
                    var homologado = false;
                    object? value = null;

                    if (!string.IsNullOrEmpty(question.campo_homologado) && datos.TryGetValue(question.campo_homologado, out var el))
                    {
                        value = el.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined ? null : el.ToString();
                        homologado = true;
                    }

                    if (requerido && homologado)
                    {
                        question.respuesta = RegresaRespuesta(question, value);
                    }
                    else if (requerido && !homologado)
                    {
                        value = (question.opciones != null && question.opciones.Count > 0) ? question.opciones[0] : "NA";
                        question.respuesta = RegresaRespuesta(question, value);
                    }
                    else if (!requerido && homologado)
                    {
                        question.respuesta = RegresaRespuesta(question, value);
                    }
                    else
                    {
                        question.respuesta = null;
                    }
                }

                frmRespuestas.AfBeneFrmRespuesta_Agregar(jDatos, form1);
            }
        }

        /// <summary>
        /// Convierte la respuesta homologada al formato esperado por el tipo de pregunta del formulario.
        /// </summary>
        private static object? RegresaRespuesta(FormQuestion question, object? value)
        {
            object? respuesta = null;
            var resList = new List<OptionabledQuestion>();

            switch (question.pregunta_tipo)
            {
                case "radio":
                    var resUserCk = value?.ToString() ?? string.Empty;
                    foreach (var opcion in question.opciones ?? new List<OptionabledQuestion>())
                    {
                        resList.Add(new OptionabledQuestion
                        {
                            id_opciones = opcion.id_opciones,
                            item = opcion.item,
                            descripcion = opcion.descripcion,
                            selected = (opcion.descripcion ?? string.Empty).ToUpper().Contains(resUserCk.ToUpper())
                        });
                        break;
                    }
                    respuesta = resList.Count > 0 ? resList[0].item : null;
                    break;

                case "text":
                case "textarea":
                case "date":
                case "number":
                case "email":
                    respuesta = value?.ToString() ?? string.Empty;
                    break;

                case "select":
                case "multiSelect":
                case "checkbox":
                    var resUser = value?.ToString() ?? string.Empty;
                    var resListUser = resUser.Split(';');
                    foreach (var opcion in question.opciones ?? new List<OptionabledQuestion>())
                    {
                        foreach (var res in resListUser)
                        {
                            if (res == null)
                            {
                                continue;
                            }

                            if ((opcion.descripcion ?? string.Empty).ToUpper().Contains(res.ToUpper()))
                            {
                                resList.Add(new OptionabledQuestion
                                {
                                    id_opciones = opcion.id_opciones,
                                    item = opcion.item,
                                    descripcion = opcion.descripcion,
                                    selected = true
                                });
                                break;
                            }
                        }
                    }

                    respuesta = Newtonsoft.Json.JsonConvert.SerializeObject(resList);
                    break;
            }

            return respuesta;
        }

        /// <summary>
        /// Convierte el objeto dinámico "cf" (campos custom) de un ticket de Zoho Desk en un
        /// diccionario de JsonElement para su lectura homogénea.
        /// </summary>
        private static Dictionary<string, JsonElement>? ParseCf(object? cf)
        {
            if (cf == null)
            {
                return null;
            }

            try
            {
                var raw = cf is JsonElement je ? je.GetRawText() : JsonSerializer.Serialize(cf);
                return JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(raw);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Obtiene el valor de un campo custom del ticket como string (o null si no existe / es nulo).
        /// </summary>
        private static string? CfStr(Dictionary<string, JsonElement>? datos, string key)
        {
            if (datos == null || !datos.TryGetValue(key, out var el))
            {
                return null;
            }

            return el.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined ? null : el.ToString();
        }

        /// <summary>
        /// Obtiene el valor de un campo custom del ticket como entero (0 si no existe / no es numérico).
        /// </summary>
        private static int ParseIntCf(Dictionary<string, JsonElement> datos, string key)
        {
            var value = CfStr(datos, key);
            return value != null && int.TryParse(value, out var i) ? i : 0;
        }

        /// <summary>
        /// Obtiene un token de acceso a la API de Zoho Desk usando refresh_token.
        /// </summary>
        private string? ObtenerTokenZoho(HttpClient httpClient, out string? errorDetail)
        {
            errorDetail = null;
            try
            {
                var refreshToken = _config["Zoho:refresh_token"] ?? string.Empty;
                var clientId = _config["Zoho:client_id"] ?? string.Empty;
                var clientSecret = _config["Zoho:client_secret"] ?? string.Empty;
                var grantType = _config["Zoho:grant_type"] ?? "refresh_token";

                if (string.IsNullOrEmpty(refreshToken) || string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret))
                {
                    errorDetail = "Falta configurar Zoho:refresh_token, Zoho:client_id o Zoho:client_secret";
                    return null;
                }

                // El endpoint OAuth de Zoho requiere application/x-www-form-urlencoded, no JSON.
                var formData = new Dictionary<string, string>
                {
                    { "refresh_token", refreshToken },
                    { "client_id", clientId },
                    { "client_secret", clientSecret },
                    { "grant_type", grantType }
                };

                using var content = new FormUrlEncodedContent(formData);
                var response = httpClient.PostAsync("https://accounts.zoho.com/oauth/v2/token", content).Result;
                var responseString = response.Content.ReadAsStringAsync().Result;

                var zohoAuth = System.Text.Json.JsonSerializer.Deserialize<ZohoModel>(responseString);

                if (!string.IsNullOrEmpty(zohoAuth?.error))
                {
                    errorDetail = $"Zoho devolvió error '{zohoAuth.error}' (HTTP {(int)response.StatusCode})";
                    return null;
                }

                if (string.IsNullOrEmpty(zohoAuth?.access_token))
                {
                    errorDetail = $"Zoho respondió sin access_token (HTTP {(int)response.StatusCode}): {responseString}";
                    return null;
                }

                return zohoAuth.access_token;
            }
            catch (Exception ex)
            {
                errorDetail = ex.Message;
                return null;
            }
        }

        /// <summary>
        /// Obtiene tickets de Zoho Desk por departamento y rango de fechas usando el endpoint real de
        /// búsqueda de Zoho Desk (GET /api/v1/tickets/search), con filtro por customField1 de "producto
        /// solidario" y paginación por "from". Puerto funcional (síncrono) de
        /// PgxAPI_Externo.DataBaseTier.InterfaceZoho.ZohoDB.Casos_Sincronizar (v1, líneas ~302-410), que usa
        /// este mismo endpoint probado en producción. v1 es async con SemaphoreSlim/Task.WhenAll para
        /// concurrencia; v2 mantiene el patrón síncrono ya establecido en este archivo (.Result), sin
        /// replicar la concurrencia paralela.
        /// </summary>
        private List<AfiBeneTicketsDatos> ObtenerTicketsZoho(HttpClient httpClient, string token, string deptoId, DateTime fechaInicio, DateTime fechaCorte)
        {
            var tickets = new List<AfiBeneTicketsDatos>();
            const int pageSize = 10;

            httpClient.DefaultRequestHeaders.Remove("orgId");
            httpClient.DefaultRequestHeaders.Add("orgId", "691715214");
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Zoho-oauthtoken", token);

            var fechaIniISO = fechaInicio.ToUniversalTime().ToString("yyyy-MM-ddT00:00:00.000Z");
            var fechaCorISO = fechaCorte.ToUniversalTime().ToString("yyyy-MM-ddT11:59:59.000Z");

            const string baseUrl = "https://desk.zoho.com/api/v1/tickets/search";
            var baseQuery = $"departmentId={deptoId}&createdTimeRange={fechaIniISO},{fechaCorISO}&customField1=cf_productos_servicio_al_asociado:Beneficios solidarios";

            // Llamada inicial (sin parámetro de página) únicamente para conocer el total de resultados
            // ("count") y así calcular la cantidad de páginas a recorrer. Réplica funcional de v1
            // (líneas ~329-351), que hace lo mismo antes del bucle de paginación.
            var initialResponse = httpClient.GetAsync($"{baseUrl}?{baseQuery}").Result;
            if (!initialResponse.IsSuccessStatusCode)
            {
                return tickets;
            }

            var initialBody = initialResponse.Content.ReadAsStringAsync().Result;
            var initialData = JsonSerializer.Deserialize<DataModel>(initialBody, _jsonOptions);

            var totalPages = 0;
            if (initialData != null)
            {
                totalPages = initialData.count == pageSize ? 1 : (int)Math.Ceiling((double)initialData.count / pageSize);
            }

            for (var page = 1; page <= totalPages; page++)
            {
                // Réplica de la particularidad de v1 (líneas ~358-365): el parámetro "&from={page}" solo
                // se agrega cuando hay más de una página; si totalPages == 1, la URL va sin ese parámetro.
                var paginaActual = totalPages > 1 ? $"&from={page}" : string.Empty;
                var url = $"{baseUrl}?{baseQuery}{paginaActual}";

                var response = httpClient.GetAsync(url).Result;
                if (!response.IsSuccessStatusCode)
                {
                    continue;
                }

                var body = response.Content.ReadAsStringAsync().Result;
                var dataModel = JsonSerializer.Deserialize<DataModel>(body, _jsonOptions);

                ProcesarTicketsZoho(dataModel, tickets);
            }

            return tickets;
        }

        /// <summary>
        /// Extrae de cada ticket de una página de resultados de Zoho Desk los campos custom relevantes
        /// (cf_productos_servicio_al_asociado, cf_numero_de_cedula, cf_tipo_de_tramite_2) reutilizando los
        /// helpers ParseCf/CfStr ya existentes, y descarta los tickets que no son "Beneficios solidarios"
        /// o cuyo trámite es nulo / "Consultas Generales". Puerto funcional de
        /// PgxAPI_Externo.DataBaseTier.InterfaceZoho.ZohoDB.TicketRegistro_Guardar (v1, líneas ~137-196).
        /// </summary>
        private static void ProcesarTicketsZoho(DataModel? dataModel, List<AfiBeneTicketsDatos> tickets)
        {
            if (dataModel?.data == null)
            {
                return;
            }

            foreach (var zohoTicket in dataModel.data)
            {
                var datos = ParseCf(zohoTicket.cf) ?? new Dictionary<string, JsonElement>();

                var producto = CfStr(datos, "cf_productos_servicio_al_asociado");
                if (string.IsNullOrEmpty(producto) || !producto.Contains("Beneficios"))
                {
                    // Ticket no solidario: se descarta, igual que v1.
                    continue;
                }

                var tipoTramite = CfStr(datos, "cf_tipo_de_tramite_2");
                if (string.IsNullOrEmpty(tipoTramite) || tipoTramite == "Consultas Generales")
                {
                    continue;
                }

                var cedula = CfStr(datos, "cf_numero_de_cedula") ?? string.Empty;

                tickets.Add(new AfiBeneTicketsDatos
                {
                    id_zoho = zohoTicket.Id,
                    fecha_creacion = DateTime.TryParse(
                        zohoTicket.CreatedTime,
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.RoundtripKind,
                        out var ct) ? ct : DateTime.MinValue,
                    estado_zoho = zohoTicket.Status,
                    web_url = zohoTicket.WebUrl,
                    categoria = producto,
                    tipo_tramite = tipoTramite,
                    cedula = cedula,
                    n_expediente = "",
                    consec = "",
                    cod_beneficio = "",
                    id_beneficio = 0,
                    msj_interface = "",
                    estado = "P",
                    caso_id = zohoTicket.TicketNumber,
                    i_visto = false,
                    i_pendiente = false
                });
            }
        }
    }
}
