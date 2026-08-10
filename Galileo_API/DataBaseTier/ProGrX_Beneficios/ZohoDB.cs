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

                var tickets = ObtenerTicketsZoho(httpClient, new ZohoTicketsConsultaRequest
                {
                    Token = token,
                    DepartamentoId = deptoId,
                    FechaInicio = fechaInicio,
                    FechaCorte = fechaCorte
                });

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
                    "Apremiante" => Apremiante_Guardar(CodEmpresa, datos, usuario, jsonZoho),
                    "Sepelios" => Sepelios_Guardar(CodEmpresa, datos, usuario, jsonZoho),
                    "Desastres" => Desastres_Guardar(CodEmpresa, datos, usuario, jsonZoho),
                    "FENA" => FENA_Guardar(CodEmpresa, datos, usuario, jsonZoho),
                    "Reconocimientos" => Reconocimientos_Guardar(CodEmpresa, datos, usuario, jsonZoho),
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
        private ErrorDto Apremiante_Guardar(int CodEmpresa, Dictionary<string, JsonElement> datos, string usuario, ZohoTicketAdd jsonZoho)
        {
            var response = new ErrorDto { Code = 0 };
            var msjError = string.Empty;

            try
            {
                using var connection = DbHelper.OpenConnection(CreatePortalDb(), CodEmpresa);

                var cedula = CfStr(datos, CampoCedulaZoho);
                if (string.IsNullOrEmpty(cedula))
                {
                    response.Code = -1;
                    msjError += MensajeCedulaRequerida;
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

                        // Los adjuntos de threads no se transfieren porque esta versión aún no
                        // dispone de la infraestructura HTTP de threads y attachments de Zoho Desk.

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
        private ErrorDto Desastres_Guardar(int CodEmpresa, Dictionary<string, JsonElement> datos, string usuario, ZohoTicketAdd jsonZoho)
        {
            var response = new ErrorDto { Code = 0 };
            var msjError = string.Empty;

            try
            {
                using var connection = DbHelper.OpenConnection(CreatePortalDb(), CodEmpresa);

                var cedula = CfStr(datos, CampoCedulaZoho);
                if (string.IsNullOrEmpty(cedula))
                {
                    response.Code = -1;
                    msjError += MensajeCedulaRequerida;
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

                    // Los adjuntos no se transfieren porque esta versión aún no dispone de la
                    // infraestructura HTTP de threads y attachments de Zoho Desk.

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
        private ErrorDto FENA_Guardar(int CodEmpresa, Dictionary<string, JsonElement> datos, string usuario, ZohoTicketAdd jsonZoho)
        {
            var response = new ErrorDto { Code = 0 };
            var msjError = string.Empty;

            try
            {
                using var connection = DbHelper.OpenConnection(CreatePortalDb(), CodEmpresa);

                var cedula = CfStr(datos, CampoCedulaZoho);
                if (string.IsNullOrEmpty(cedula))
                {
                    response.Code = -1;
                    msjError += MensajeCedulaRequerida;
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

                        // Los adjuntos no se transfieren porque esta versión aún no dispone de la
                        // infraestructura HTTP de threads y attachments de Zoho Desk.

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

                var cedula = CfStr(datos, CampoCedulaZoho) ?? string.Empty;

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
