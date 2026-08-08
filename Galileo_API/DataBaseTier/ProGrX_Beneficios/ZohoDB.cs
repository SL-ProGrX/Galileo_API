using Dapper;
using Galileo.Models.AF;
using Galileo.Models.ERROR;
using Galileo_Externo.Models.NewFolder;

namespace Galileo.DataBaseTier.ProGrX_Beneficios
{
    /// <summary>
    /// Acceso a datos de integración Zoho Desk (frmAF_Beneficios_Zoho).
    /// Consultas de tickets, sincronización, marca de visto e inclusión de tickets.
    /// </summary>
    public partial class ZohoDB
    {
        private readonly IConfiguration _config;

        /// <summary>
        /// Inicializa el acceso a datos con la configuración inyectada.
        /// </summary>
        public ZohoDB(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
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

                const string sqlCount = "SELECT COUNT(*) FROM AFI_BENE_ZOHO_TICKETS";
                response.total = connection.QueryFirstOrDefault<int>(sqlCount);

                var like = string.IsNullOrWhiteSpace(filtros.filtro) ? null : $"%{filtros.filtro}%";
                var estado = filtros.estado ?? "T";
                var offset = filtros.pagina * filtros.paginacion;
                var fetch = filtros.paginacion;

                const string sql = @"SELECT id_zoho, fecha_creacion, estado_zoho, web_url, categoria, tipo_tramite,
                                            cedula, n_expediente, consec, cod_beneficio, id_beneficio, msj_interface,
                                            estado, caso_id, i_visto, i_pendiente, visto_por, incluido_por, entrada,
                                            visto_fecha, incluido_fecha
                                     FROM AFI_BENE_ZOHO_TICKETS
                                     WHERE (@like IS NULL OR id_zoho LIKE @like OR cedula LIKE @like OR categoria LIKE @like)
                                       AND (@estado = 'T' OR estado_zoho = @estado)
                                       AND fecha_creacion BETWEEN @fechaInicio AND @fechaFin
                                     ORDER BY fecha_creacion DESC
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
                                            SUM(CASE WHEN i_pendiente = 1 THEN 1 ELSE 0 END) AS valorPendiente,
                                            SUM(CASE WHEN estado_zoho = 'Error' THEN 1 ELSE 0 END) AS valorError,
                                            SUM(CASE WHEN estado_zoho = 'Ingresado ProGrX' THEN 1 ELSE 0 END) AS valorIngresado
                                         FROM AFI_BENE_ZOHO_TICKETS
                                         WHERE fecha_creacion BETWEEN @fechaInicio AND @fechaFin";

                var resumen = connection.QueryFirstOrDefault(sqlResumen, new
                {
                    fechaInicio = filtros.fechaInicio,
                    fechaFin = filtros.fechaFin
                });

                if (resumen != null)
                {
                    response.valorPendiente = resumen.valorPendiente;
                    response.valorError = resumen.valorError;
                    response.valorIngresado = resumen.valorIngresado;
                }

                const string sqlTipos = @"SELECT tipo_tramite AS tipoTramite, COUNT(*) AS total
                                          FROM AFI_BENE_ZOHO_TICKETS
                                          WHERE fecha_creacion BETWEEN @fechaInicio AND @fechaFin
                                          GROUP BY tipo_tramite";

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
                var config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
                var deptoId = config["AFI_Beneficios:DepartamentoZoho"] ?? string.Empty;

                var httpClient = new HttpClient();
                var token = ObtenerTokenZoho(httpClient);

                if (string.IsNullOrEmpty(token))
                {
                    return DbHelper.ErrorResponse("No se pudo obtener token de Zoho Desk");
                }

                var tickets = ObtenerTicketsZoho(httpClient, token, deptoId, fechaInicio, fechaCorte);

                var insertados = 0;
                var actualizados = 0;

                foreach (var ticket in tickets)
                {
                    const string sqlExiste = "SELECT COUNT(*) FROM AFI_BENE_ZOHO_TICKETS WHERE id_zoho = @id_zoho";
                    var existe = connection.QueryFirstOrDefault<int>(sqlExiste, new { ticket.id_zoho });

                    if (existe > 0)
                    {
                        const string sqlUpdate = @"UPDATE AFI_BENE_ZOHO_TICKETS
                                                   SET estado_zoho = @estado_zoho, web_url = @web_url,
                                                       categoria = @categoria, tipo_tramite = @tipo_tramite,
                                                       modifica_fecha = GETDATE(), modifica_usuario = @modifica_usuario
                                                   WHERE id_zoho = @id_zoho";
                        connection.Execute(sqlUpdate, new
                        {
                            ticket.estado_zoho,
                            ticket.web_url,
                            ticket.categoria,
                            ticket.tipo_tramite,
                            modifica_usuario = usuario,
                            ticket.id_zoho
                        });
                        actualizados++;
                    }
                    else
                    {
                        const string sqlInsert = @"INSERT INTO AFI_BENE_ZOHO_TICKETS
                                                    (id_zoho, fecha_creacion, estado_zoho, web_url, categoria, tipo_tramite,
                                                     cedula, n_expediente, consec, cod_beneficio, id_beneficio, msj_interface,
                                                     estado, caso_id, i_visto, i_pendiente, entrada, registro_fecha, registro_usuario)
                                                   VALUES
                                                    (@id_zoho, @fecha_creacion, @estado_zoho, @web_url, @categoria, @tipo_tramite,
                                                     @cedula, @n_expediente, @consec, @cod_beneficio, @id_beneficio, @msj_interface,
                                                     @estado, @caso_id, 0, 1, @entrada, GETDATE(), @registro_usuario)";
                        connection.Execute(sqlInsert, new
                        {
                            ticket.id_zoho,
                            ticket.fecha_creacion,
                            ticket.estado_zoho,
                            ticket.web_url,
                            ticket.categoria,
                            ticket.tipo_tramite,
                            ticket.cedula,
                            ticket.n_expediente,
                            ticket.consec,
                            ticket.cod_beneficio,
                            ticket.id_beneficio,
                            ticket.msj_interface,
                            ticket.estado,
                            ticket.caso_id,
                            entrada,
                            registro_usuario = usuario
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
            const string sql = @"UPDATE AFI_BENE_ZOHO_TICKETS
                                 SET i_visto = @i_visto, visto_por = @visto_por, visto_fecha = GETDATE()
                                 WHERE id_zoho = @id_zoho";

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
                const string sql = "SELECT COUNT(*) FROM AFI_BENE_ZOHO_TICKETS WHERE i_pendiente = 1";
                return connection.QueryFirstOrDefault<int>(sql);
            });
        }

        /// <summary>
        /// Importa un ticket de Zoho como registro de beneficio en ProGrX.
        /// </summary>
        public ErrorDto IncluirTicket_Guardar(int CodEmpresa, ZohoTicketAdd jsonZoho)
        {
            using var connection = DbHelper.OpenConnection(CreatePortalDb(), CodEmpresa);
            try
            {
                const string sql = @"UPDATE AFI_BENE_ZOHO_TICKETS
                                     SET i_pendiente = 0, i_visto = 1,
                                         incluido_por = @usuario, incluido_fecha = GETDATE()
                                     WHERE id_zoho = @ticket";
                connection.Execute(sql, new { jsonZoho.usuario, jsonZoho.ticket });

                const string sqlUpdate = @"UPDATE AFI_BENE_ZOHO_TICKETS
                                           SET estado_zoho = 'Ingresado ProGrX'
                                           WHERE id_zoho = @ticket";
                connection.Execute(sqlUpdate, new { jsonZoho.ticket });

                return DbHelper.OkResponse("Ticket importado correctamente");
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        /// <summary>
        /// Obtiene un token de acceso a la API de Zoho Desk usando refresh_token.
        /// </summary>
        private string? ObtenerTokenZoho(HttpClient httpClient)
        {
            try
            {
                var config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
                var refreshToken = config["Zoho:refresh_token"] ?? string.Empty;
                var clientId = config["Zoho:client_id"] ?? string.Empty;
                var clientSecret = config["Zoho:client_secret"] ?? string.Empty;

                if (string.IsNullOrEmpty(refreshToken))
                {
                    return null;
                }

                var authModel = new ZohoAuthTokenModel
                {
                    refresh_token = refreshToken,
                    client_id = clientId,
                    client_secret = clientSecret
                };

                var json = System.Text.Json.JsonSerializer.Serialize(authModel);
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
                var response = httpClient.PostAsync("https://accounts.zoho.com/oauth/v2/token", content).Result;
                var responseString = response.Content.ReadAsStringAsync().Result;

                var zohoAuth = System.Text.Json.JsonSerializer.Deserialize<ZohoModel>(responseString);
                return zohoAuth?.access_token;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Obtiene tickets de Zoho Desk por departamento y rango de fechas.
        /// </summary>
        private List<AfiBeneTicketsDatos> ObtenerTicketsZoho(HttpClient httpClient, string token, string deptoId, DateTime fechaInicio, DateTime fechaCorte)
        {
            var tickets = new List<AfiBeneTicketsDatos>();
            var page = 1;
            var hasMore = true;

            while (hasMore)
            {
                var url = $"https://www.zohoapis.com/crm/v2/tickets?department_id={deptoId}&created_time>={fechaInicio:yyyy-MM-dd}&created_time<={fechaCorte:yyyy-MM-dd}&page={page}&per_page=100";
                httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Zoho-oauthtoken", token);

                var response = httpClient.GetAsync(url).Result;
                var responseString = response.Content.ReadAsStringAsync().Result;
                var dataModel = System.Text.Json.JsonSerializer.Deserialize<DataModel>(responseString);

                if (dataModel?.data != null && dataModel.data.Count > 0)
                {
                    foreach (var zohoTicket in dataModel.data)
                    {
                        tickets.Add(new AfiBeneTicketsDatos
                        {
                            id_zoho = zohoTicket.Id,
                            fecha_creacion = DateTime.TryParse(zohoTicket.CreatedTime, out var ct) ? ct : DateTime.MinValue,
                            estado_zoho = "Pendiente",
                            web_url = zohoTicket.WebUrl,
                            categoria = zohoTicket.DepartmentId,
                            tipo_tramite = zohoTicket.Subject,
                            cedula = "",
                            n_expediente = "",
                            consec = "",
                            cod_beneficio = "",
                            id_beneficio = 0,
                            msj_interface = "",
                            estado = "Pendiente",
                            caso_id = zohoTicket.Id,
                            i_visto = false,
                            i_pendiente = true,
                            entrada = "Zoho"
                        });
                    }
                    hasMore = dataModel.data.Count == 100;
                    page++;
                }
                else
                {
                    hasMore = false;
                }
            }

            return tickets;
        }
    }
}
