using Dapper;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using PgxAPI.DataBaseTier;
using PgxAPI.Models.AF;
using PgxAPI.Models.ERROR;
using PgxAPI_Externo.Models.NewFolder;
using System.Net.Http.Headers;
using PgxAPI.Models.GA;


namespace PgxAPI_Externo.DataBaseTier.InterfaceZoho
{
    public class ZohoDB
    {
        private readonly IConfiguration _config;
        private readonly mBeneficiosDB _mBeneficiosDB;
        private readonly mProGrX_AuxiliarDB _mUtils;
        private readonly frmGA_DocumentosDB _mDocumentosDB;
        private readonly AF_Beneficios_Integral_ReqDB _mBeneIntegralReq;

        public ZohoDB(IConfiguration config)
        {
            _config = config;
            _mBeneficiosDB = new mBeneficiosDB(config);
            _mUtils = new mProGrX_AuxiliarDB(config);
            _mDocumentosDB = new frmGA_DocumentosDB(config);
            _mBeneIntegralReq = new AF_Beneficios_Integral_ReqDB(config);
        }

        /// <summary>
        /// Obtiene el token de autenticacion de Zoho
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        private async Task<ErrorDto<ZohoAuthModel>> ObtenerAuthZohoAsync(int CodEmpresa)
        {
            var response = new ErrorDto<ZohoAuthModel>();
            string URLAuth = "https://accounts.zoho.com/oauth/v2/token";

            try
            {
                //reviso el ultimo token guardado en la base de datos
                var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

                ZohoAuthTokenModel zohoAuthToken = new ZohoAuthTokenModel();
                ZohoAuthModel zohoAuth = new ZohoAuthModel();

                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"SELECT TOKEN, FECHA FROM AFI_BENE_INT_TOKEN WHERE INTERFACE = 'ZOHO' ";
                    var lastToken = connection.QueryFirstOrDefault<tokenModel>(query);

                    //si la fecha es mayor a 1 hora, se debe renovar el token
                    if (lastToken.token != null)
                    {
                        if (DateTime.Now.Subtract((DateTime)lastToken.fecha).TotalSeconds < zohoAuth.expires_in)
                        {
                            zohoAuth.access_token = lastToken.token;
                            response.Result = zohoAuth;
                            return response;
                        }
                    }
                }

                //genero el token
                using (HttpClient client = new HttpClient())
                {
                    var formData = new Dictionary<string, string>
                    {
                        { "refresh_token", zohoAuthToken.refresh_token },
                        { "client_id", zohoAuthToken.client_id },
                        { "client_secret", zohoAuthToken.client_secret },
                        { "grant_type", zohoAuthToken.grant_type }
                    };

                    using (var content = new FormUrlEncodedContent(formData))
                    {
                        try
                        {
                            HttpResponseMessage resp = await client.PostAsync(URLAuth, content);
                            string responseBody = await resp.Content.ReadAsStringAsync();

                            zohoAuth = JsonConvert.DeserializeObject<ZohoAuthModel>(responseBody);
                            response.Result = zohoAuth;

                            //guardo el token en la base de datos
                            using var conn = new SqlConnection(clienteConnString);
                            {
                                var query = $@"UPDATE AFI_BENE_INT_TOKEN SET TOKEN = '{zohoAuth.access_token}'
                                                    , FECHA = getdate() WHERE INT_ID = '1' ";
                                conn.Execute(query);
                            }

                        }
                        catch (Exception ex)
                        {
                            response.Code = -1;
                            response.Description = ex.Message;
                            response.Result = null;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = null;
            }

           return response;
        }

        /// <summary>
        /// Guarda el ticket en la base de datos ProGrX
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="ticket"></param>
        /// <param name="entrada"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public async Task<ErrorDto<string>> TicketRegistro_Guardar(int CodEmpresa, Ticket ticket, string entrada, string usuario)
        {
            var response = new ErrorDto<string>();
            //var token = ObtenerAuthZohoAsync();
            try
            {
                var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

                using var connection = new SqlConnection(clienteConnString);
                {
                    //Valida si el ticket ya existe
                    var query = $@"SELECT 'X' FROM AFI_BENE_OTORGA_INT WHERE ID_ZOHO = '{ticket.Id}' ";
                    var existe = connection.QueryFirstOrDefault<string>(query);

                    //Busco cedula dentro del objeto CustomFields
                    string cedula = null;
                    string tipo_tramite = null;
                    string producto = null;

                    //ticket cf es un objeto que contiene propiedades dinamicas que pueden ser de cualquier tipo 
                    //y no se puede deserializar directamente a un objeto fuertemente tipado
                    if (ticket.cf != null)
                    {
                        // Convertir el objeto a JSON
                        Dictionary<string, object> datos = JsonConvert.DeserializeObject<Dictionary<string, object>>(ticket.cf.ToString());

                        // Iterar sobre las propiedades del diccionario
                        foreach (var kvp in datos)
                        {
                            switch (kvp.Key)
                            {
                                case "cf_productos_servicio_al_asociado":
                                    if (kvp.Value != null)
                                    {
                                        if (kvp.Value.ToString().Contains("Beneficios"))
                                        {
                                            producto = kvp.Value.ToString();
                                        }
                                        else
                                        {
                                            response.Code = 0;
                                            response.Description = "Ticket no Solidario";
                                            return response;
                                        }
                                    }
                                    else
                                    {
                                        response.Code = 0;
                                        response.Description = "Ticket no Solidario";
                                        return response;
                                    }
                                    break;
                                case "cf_numero_de_cedula":
                                    if (kvp.Value != null)
                                    {
                                        cedula = kvp.Value.ToString();
                                    }

                                    break;
                                case "cf_tipo_de_tramite_2":
                                    if (kvp.Value != null)
                                    {
                                        tipo_tramite = kvp.Value.ToString();
                                    }
                                    break;
                            }
                        }

                    }

                    if(tipo_tramite == "Consultas Generales" || tipo_tramite == null )
                    {
                        return response;
                    }

                    if (existe == null)
                    {
                        query = $@"INSERT INTO [dbo].[AFI_BENE_OTORGA_INT]
                                       ([ID_ZOHO]
                                       ,[FECHA_CREACION]
                                       ,[ESTADO_ZOHO]
                                       ,[WEB_URL]
                                       ,[CATEGORIA]
                                       ,[TIPO_TRAMITE]
                                       ,[CEDULA]
                                       ,[N_EXPEDIENTE]
                                       ,[CONSEC]
                                       ,[COD_BENEFICIO]
                                       ,[ID_BENEFICIO]
                                       ,[MSJ_INTERFACE]
                                       ,[ESTADO]
                                       ,[CASO_ID]
                                        ,I_VISTO
                                        ,I_PENDIENTE
                                       ,ENTRADA)
                                 VALUES
                                       ('{ticket.Id}'
                                       ,'{ticket.CreatedTime}'
                                       ,'{ticket.Status}'
                                       ,'{ticket.WebUrl}'
                                       ,'{producto}'
                                       ,'{tipo_tramite}'
                                       ,'{cedula}'
                                       ,null
                                       ,null
                                       ,null
                                       ,null
                                       ,''
                                       ,'P'
                                       ,{ticket.TicketNumber},0,0,'{entrada}' )";

                        response.Code = connection.Execute(query);
                        response.Description = "Ok";
                        response.Result = "Ticket guardado exitosamente";
                    }
                    else
                    {
                        query = $@"UPDATE [dbo].[AFI_BENE_OTORGA_INT]
                                       SET [ESTADO_ZOHO] = '{ticket.Status}'
                                          ,[CEDULA] = '{cedula}'
                                     WHERE ID_ZOHO = '{ticket.Id}'";
                        response.Code = connection.Execute(query);
                        response.Description = "Ok";
                        response.Result = "Ticket actualizado exitosamente";
                    }
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = null;
            }

            return response;
        }

        /// <summary>
        /// Actualiza el estado del ticket en la base de datos ProGrX
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public ErrorDto EstadoTikcet_Actualizar(string Id)
        {
            var jwtSettings = _config.GetSection("JwtSettings");
            int CodEmpresa = Convert.ToInt32(jwtSettings["CodEmpresa"]);
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto();
            response.Code = 0;

            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {

                    var query = $@"SELECT GETDATE()";
                    response.Description = "Ok";
                    response.Code = connection.Execute(query);

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
        /// Sincroniza los tickets de Zoho a la base de datos ProGrX
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="fechaInicio"></param>
        /// <param name="fechaCorte"></param>
        /// <param name="entrada"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public async Task<ErrorDto> Casos_Sincronizar(int CodEmpresa, DateTime fechaInicio, DateTime fechaCorte, string entrada, string usuario)
        {
            var response = new ErrorDto();
            var token = await ObtenerAuthZohoAsync(CodEmpresa);
            int pageSize = 10;
            int maxConcurrentRequests = 5; // Máximo de peticiones en paralelo
            int delayBetweenBatches = 500;

            if (token.Code == -1) return response;

            try
            {
                var jwtSettings = _config.GetSection("AFI_Beneficios");
                string departamenId = jwtSettings["DepartamentoZoho"];
                var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

                string fechaIniISO = fechaInicio.ToUniversalTime().ToString("yyyy-MM-ddT00:00:00.000Z");
                string fechaCorISO = fechaCorte.ToUniversalTime().ToString("yyyy-MM-ddT11:59:59.000Z");

                string baseUrl = "https://desk.zoho.com/api/v1/tickets/search";
                string initialUrl = $"{baseUrl}?departmentId={departamenId}&createdTimeRange={fechaIniISO},{fechaCorISO}&customField1=cf_productos_servicio_al_asociado:Beneficios solidarios";

                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("orgId", "691715214");
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Zoho-oauthtoken", token.Result.access_token);

                    var respList = await client.GetAsync(initialUrl);
                    if (!respList.IsSuccessStatusCode)
                    {
                        response.Code = -1;
                        response.Description = "Error al obtener los tickets";
                        return response;
                    }

                    string result = await respList.Content.ReadAsStringAsync();
                    var ticket = JsonConvert.DeserializeObject<dataModel>(result);
                    int totalPages = 0;

                    if (ticket != null)
                    {
                        if(ticket.count == pageSize)
                        {
                            totalPages = 1;
                        }
                        else
                        {
                            totalPages = (int)Math.Ceiling((double)ticket.count / pageSize);
                        }
                    }

                    

                    var tasks = new List<Task>();
                    var semaphore = new SemaphoreSlim(maxConcurrentRequests);

                    for (int page = 1; page <= totalPages; page++)
                    {
                        string paginaActual = "";
                        //paginacion:
                        if (totalPages > 1)
                        {
                            paginaActual = $"&from={page}";
                        }
  
                        await semaphore.WaitAsync(); // Controla la concurrencia

                        tasks.Add(Task.Run(async () =>
                        {
                            try
                            {
                                string urlTask = $"{baseUrl}?departmentId={departamenId}&createdTimeRange={fechaIniISO},{fechaCorISO}&customField1=cf_productos_servicio_al_asociado:Beneficios solidarios{paginaActual}";
                                var responseTask = await client.GetAsync(urlTask);

                                if (responseTask.IsSuccessStatusCode)
                                {
                                    string pageResult = await responseTask.Content.ReadAsStringAsync();
                                    var ticketPage = JsonConvert.DeserializeObject<dataModel>(pageResult);

                                    foreach (var item in ticketPage.data)
                                    {
                                        await TicketRegistro_Guardar(CodEmpresa, item, entrada, usuario);
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Error en página {page}: {ex.Message}");
                            }
                            finally
                            {
                                semaphore.Release(); // Libera el semáforo
                            }
                        }));

                        await Task.Delay(delayBetweenBatches); // Pausa entre lotes
                    }

                    await Task.WhenAll(tasks); // Espera a que todas las solicitudes finalicen

                    response.Code = 0;
                    response.Description = "Tickets Sincronizados";
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
        /// Obtiene los tickets de la base de datos ProGrX
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="Filtros"></param>
        /// <returns></returns>
        public ErrorDto<AfiBeneTicketsLista> AfiBeneTickets_Obtener(int CodCliente, string Filtros)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            var response = new ErrorDto<AfiBeneTicketsLista>();
            AfiBeneTicketFiltros jFiltros = JsonConvert.DeserializeObject<AfiBeneTicketFiltros>(Filtros);
            response.Result = new AfiBeneTicketsLista();
            response.Result.total = 0;
            string paginaActual = " ", paginacionActual = " ", valWhere = " ";
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {

                    //Busco Total
                   

                    if (jFiltros.pagina != null)
                    {
                        paginaActual = " OFFSET " + jFiltros.pagina + " ROWS ";
                        paginacionActual = " FETCH NEXT " + jFiltros.paginacion + " ROWS ONLY ";
                    }

                    if (jFiltros.filtro != null)
                    {
                        valWhere = $@" AND (CEDULA like '%{jFiltros.filtro}%' 
                                        OR CASO_ID like '%{jFiltros.filtro}%' 
                                        OR CATEGORIA like '%{jFiltros.filtro}%' 
                                        OR TIPO_TRAMITE like '%{jFiltros.filtro}%' )";
                    }

                    if(jFiltros.estado != "T")
                    {
                        valWhere += $@" AND ESTADO = '{jFiltros.estado}' ";
                    }

                    string fechaInicio = _mUtils.validaFechaGlobal((DateTime)jFiltros.fechaInicio);
                    string fechaFin = _mUtils.validaFechaGlobal((DateTime)jFiltros.fechaFin);

                    var query = $@"SELECT COUNT(*) FROM AFI_BENE_OTORGA_INT WHERE TIPO_TRAMITE != 'Consultas Generales' AND FECHA_CREACION BETWEEN '{fechaInicio}' AND '{fechaFin}' {valWhere}";
                    response.Result.total = connection.Query<int>(query).FirstOrDefault();

                    query = $@"SELECT COUNT([ID_ZOHO]) FROM [AFI_BENE_OTORGA_INT]
	                            WHERE  TIPO_TRAMITE != 'Consultas Generales' AND ESTADO = 'E' AND FECHA_CREACION BETWEEN '{fechaInicio}' AND '{fechaFin}' ";
                    response.Result.valorError = connection.Query<int>(query).FirstOrDefault();

                    query = $@"SELECT COUNT([ID_ZOHO]) FROM [AFI_BENE_OTORGA_INT]
	                            WHERE TIPO_TRAMITE != 'Consultas Generales' AND N_EXPEDIENTE != '' AND FECHA_CREACION BETWEEN '{fechaInicio}' AND '{fechaFin}' ";
                    response.Result.valorIngresado = connection.Query<int>(query).FirstOrDefault();

                    query = $@"SELECT COUNT([ID_ZOHO]) FROM [AFI_BENE_OTORGA_INT]
                         	WHERE TIPO_TRAMITE != 'Consultas Generales' AND N_EXPEDIENTE is null AND I_VISTO != 1 AND FECHA_CREACION BETWEEN '{fechaInicio}' AND '{fechaFin}' ";
                    response.Result.valorPendiente = connection.Query<int>(query).FirstOrDefault();

                    query = $@"SELECT COUNT([ID_ZOHO]) FROM [AFI_BENE_OTORGA_INT]
	                       WHERE TIPO_TRAMITE != 'Consultas Generales' AND I_VISTO != 1 AND FECHA_CREACION BETWEEN '{fechaInicio}' AND '{fechaFin}' ";
                    response.Result.valorConsultado = connection.Query<int>(query).FirstOrDefault();

                    query = $@"SELECT TIPO_TRAMITE as tipoTramite, COUNT(TIPO_TRAMITE) Total FROM [AFI_BENE_OTORGA_INT]
                                WHERE TIPO_TRAMITE != 'Consultas Generales'	AND FECHA_CREACION BETWEEN '{fechaInicio}' AND '{fechaFin}'                            
                                GROUP BY TIPO_TRAMITE ";
                    response.Result.tiposTramite = new List<AfiBeneTicketTipos>();
                    response.Result.tiposTramite = connection.Query<AfiBeneTicketTipos>(query).ToList();

                    query = $@"SELECT *
                                  FROM [AFI_BENE_OTORGA_INT] WHERE TIPO_TRAMITE != 'Consultas Generales' AND
                                    FECHA_CREACION BETWEEN '{fechaInicio}' AND '{fechaFin}' {valWhere}  
                                    ORDER BY FECHA_CREACION DESC
                                        {paginaActual}
                                        {paginacionActual}";
                    response.Result.lista = connection.Query<AfiBeneTicketsDatos>(query).ToList();

                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = null;
            }
            return response;
        }

        /// <summary>
        /// Obtiene los campos custom de un ticket
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public async Task<ErrorDto<List<string>>> CamposCustom_Obtener(int CodEmpresa)
        {
            var token = await ObtenerAuthZohoAsync(CodEmpresa);
            var response = new ErrorDto<List<string>>();
            response.Result = new List<string>();

            if (token.Code == -1) return response;

            try
            {
                string baseUrl = "https://desk.zoho.com/api/v1/tickets/403776000215609035";
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("orgId", "691715214");
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Zoho-oauthtoken", token.Result.access_token);

                    var respList = await client.GetAsync(baseUrl);
                    if (!respList.IsSuccessStatusCode)
                    {
                        response.Code = -1;
                        response.Description = "Error al obtener los tickets";
                        return response;
                    }

                    string result = await respList.Content.ReadAsStringAsync();
                    var ticket = JsonConvert.DeserializeObject<Ticket>(result);

                    //lleno la lista con los nombres de los campos custom
                    if (ticket.cf != null)
                    {
                        // Convertir el objeto a JSON
                        Dictionary<string, object> datos = JsonConvert.DeserializeObject<Dictionary<string, object>>(ticket.cf.ToString());

                        // Iterar sobre las propiedades del diccionario
                        foreach (var kvp in datos)
                        {
                            response.Result.Add(kvp.Key);
                        }

                    }

                    response.Code = 0;
                    response.Description = "Tickets Sincronizados";
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = null;
            }

            return response;
        }

        /// <summary>
        /// Guarda el expediente en la base de datos ProGrX
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="ticket"></param>
        /// <param name="usuario"></param>
        /// <param name="jsonZoho"></param>
        /// <returns></returns>
        public ErrorDto Expediente_Guardar(int CodEmpresa, Ticket ticket, string usuario, ZohoTicketAdd jsonZoho)
        {
            var response = new ErrorDto();
            response.Code = 0;
            string tipo_tramite = null;
            Dictionary<string, object> datos = new Dictionary<string, object>();
            try {
                if (ticket.cf != null)
                {
                    // Convertir el objeto a JSON
                    datos = JsonConvert.DeserializeObject<Dictionary<string, object>>(ticket.cf.ToString());

                    // Iterar sobre las propiedades del diccionario
                    foreach (var kvp in datos)
                    {
                        if (kvp.Key == "cf_tipo_de_tramite_2")
                        {
                            tipo_tramite = kvp.Value.ToString();
                        }
                    }

                }
                switch (tipo_tramite)
                {
                    case "Apremiante":
                        response = Apremiante_Guardar(CodEmpresa, ticket, datos, usuario, jsonZoho);
                        break;
                    case "Sepelios":
                        response = Sepelios_Guardar(CodEmpresa, ticket, datos, usuario, jsonZoho);
                        break;
                    case "Desastres":
                        response = Desastres_Guardar(CodEmpresa, ticket, datos, usuario, jsonZoho);
                        break;
                    case "FENA":
                        response = FENA_Guardar(CodEmpresa, ticket, datos, usuario, jsonZoho);
                        break;
                    case "Reconocimientos":
                        response = Reconocimientos_Guardar(CodEmpresa, ticket, datos, usuario, jsonZoho);
                        break;

                    default:
                        break;
                }
            }
            catch(Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }
            

            return response;
        }

        /// <summary>
        /// Guarda el expediente de un ticket de tipo Apremiante
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="ticket"></param>
        /// <param name="datos"></param>
        /// <param name="usuario"></param>
        /// <param name="jsonZoho"></param>
        /// <returns></returns>
        private ErrorDto Apremiante_Guardar(int CodEmpresa, Ticket ticket, Dictionary<string, object> datos, string usuario, ZohoTicketAdd jsonZoho)
        {
            var response = new ErrorDto();
            var respBeneficio = new ErrorDto<BeneficioGeneralDatos>();
            response.Code = 0;
            
            string msjError = "";
            try
            {
                var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var connection = new SqlConnection(clienteConnString);
                {
                    //primer filtro de validaciones
                    if (datos["cf_numero_de_cedula"].ToString() == "" || datos["cf_numero_de_cedula"] == null)
                    {
                        response.Code = -1;
                        msjError += "Cédula no puede ser nula...";
                    }

                    //segundo filtro de validaciones
                    var validaPersona = _mBeneficiosDB.ValidarPersona(CodEmpresa,datos["cf_numero_de_cedula"].ToString().Trim(), null);
                    if (validaPersona.Code == -1)
                    {
                        response.Code = -1;
                        msjError += validaPersona.Description + "...";
                    }

                    if(response.Code != -1)
                    {
                        //selecciono el beneficio de la base de datos
                        var query = $@"SELECT TOP 1 COD_BENEFICIO FROM AFI_BENEFICIOS WHERE COD_CATEGORIA = 'B_APRE'";
                        var codBeneficio = connection.QueryFirstOrDefault<string>(query);

                        BeneficioGeneralDatos beneficio = new BeneficioGeneralDatos();
                        beneficio.cod_beneficio = new AfBeneficioIntegralDropsLista();
                        beneficio.cod_beneficio.item = codBeneficio;
                        beneficio.id_beneficio = 0;
                        beneficio.cedula = datos["cf_numero_de_cedula"].ToString().Trim();
                        beneficio.monto_aplicado = 0;
                        beneficio.registra_user = usuario;
                        beneficio.sepelio_identificacion = null;
                        beneficio.estado = new AfBeneficioIntegralDropsLista();
                        beneficio.estado.item = null;
                        beneficio.consec = 0;

                        beneficio.requiere_justificacion = (jsonZoho.justificacion != null) ? true : false;
                        beneficio.notas = (jsonZoho.justificacion != null) ? jsonZoho.justificacion : "";
                        
                        //obtengo el consecutivo monto del grupo
                        query = $@"SELECT [MONTO]
                                  FROM [AFI_BENE_GRUPOS] WHERE COD_CATEGORIA = 'B_APRE'
                                  AND COD_GRUPO in (
	                                  SELECT COD_GRUPO
	                                  FROM [AFI_BENEFICIOS] WHERE COD_CATEGORIA = 'B_APRE'
	                                  AND COD_BENEFICIO = '{codBeneficio}'
                                  )";
                        beneficio.monto = connection.QueryFirstOrDefault<float>(query);

                        beneficio.tipo = new AfBeneficioIntegralDropsLista();
                        beneficio.tipo.item = "A";

                        frmAF_Beneficios_Integral_GenDB _beneIntegral = new frmAF_Beneficios_Integral_GenDB(_config);
                        respBeneficio = _beneIntegral.Guarda_Beneficio(CodEmpresa, beneficio, "Y", "API").Result;

                        if(respBeneficio.Code == -1)
                        {
                            response.Code = -1;
                            msjError += respBeneficio.Description + "...";
                        }
                        else
                        {
                            string[] expediente = respBeneficio.Description.Split('@');
                            //armo el numero de expediente
                            string nExpediente = expediente[0].ToString().PadLeft(6, '0') + codBeneficio.Trim() + expediente[1].ToString().PadLeft(6, '0');

                            //actualizo registro de ticket
                            query = $@"UPDATE [dbo].[AFI_BENE_OTORGA_INT]
                                       SET [N_EXPEDIENTE] = '{nExpediente}',
                                           [CONSEC] = '{expediente[1]}',
                                           COD_BENEFICIO = '{codBeneficio.Trim()}',
                                           ID_BENEFICIO = '{expediente[0]}', I_PENDIENTE = 1, I_VISTO = 1, VISTO_POR = '{usuario}', VISTO_FECHA = getdate(),
                                           [ESTADO] = 'S' , INCLUIDO_POR = '{usuario}',INCLUIDO_FECHA = getdate() 
                                     WHERE ID_ZOHO = '{ticket.Id}'";
                            response.Code = connection.Execute(query);

                            _ = BuscaArchivos(CodEmpresa, 
                                ticket.Id, usuario, datos["cf_numero_de_cedula"].ToString().Trim(), expediente[0], codBeneficio.Trim());


                            if (expediente[0].ToString() != "0")
                            {
                                FrmFiltros filtros = new FrmFiltros();

                                filtros.codCliente = CodEmpresa;
                                filtros.cod_beneficio = codBeneficio.Trim();
                                filtros.id_beneficio = beneficio.id_beneficio;
                                filtros.socio = beneficio.cedula;
                                filtros.usuario = usuario;

                                IncluirRespuestasFormularios(filtros, datos);
                            }
                        }

                        

                    }


                    if (msjError.Trim() != "")
                    {
                        response.Code = -1;
                        response.Description = msjError;

                        //actualizo registro de ticket
                        var query = $@"UPDATE [dbo].[AFI_BENE_OTORGA_INT]
                                       SET [MSJ_INTERFACE] = '{msjError}'
                                          ,[ESTADO] = 'E' , VISTO_POR = '{usuario}',VISTO_FECHA = getdate()
                                     WHERE ID_ZOHO = '{ticket.Id}'";
                        connection.Execute(query);

                    }
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
        /// Guarda el expediente de un ticket de tipo Sepelios
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="ticket"></param>
        /// <param name="datos"></param>
        /// <param name="usuario"></param>
        /// <param name="jsonZoho"></param>
        /// <returns></returns>
        private ErrorDto Sepelios_Guardar(int CodEmpresa, Ticket ticket, Dictionary<string, object> datos, string usuario, ZohoTicketAdd jsonZoho)
        {
            var response = new ErrorDto();
            var respBeneficio = new ErrorDto<BeneficioGeneralDatos>();
            response.Code = 0;

            string msjError = "";
            int grupo = 0;
            try
            {
                var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var connection = new SqlConnection(clienteConnString);
                {
                    try
                    {
                        //primer filtro de validaciones
                        if (datos["cf_numero_de_cedula"].ToString() == "" || datos["cf_numero_de_cedula"] == null)
                        {
                            response.Code = -1;
                            msjError += "Cédula no puede ser nula...";
                        }
                    }
                    catch (Exception)
                    {
                        response.Code = -1;
                        response.Description =  "Cédula no puede ser nula...";
                        return response;
                    }
                    

                    //segundo filtro de validaciones
                    respBeneficio = _mBeneficiosDB.ValidaEstadoSocio(CodEmpresa, datos["cf_numero_de_cedula"].ToString().Trim());
                    if (respBeneficio.Code == -1)
                    {
                        response.Code = -1;
                        msjError += respBeneficio.Description + "...";
                    }

                    if (response.Code != -1)
                    {
                        string parentesco = datos["cf_parentesco_de_la_persona_fallecida"].ToString().Trim();
                        string descripcion = "";
                        var codBeneficio = "";
                        if (parentesco.ToUpper().Contains("PADRE"))
                        {
                            codBeneficio = "MPAD";
                            grupo = 13;
                        }
                        if (parentesco.ToUpper().Contains("MADRE"))
                        {
                            codBeneficio = "MMADRE";
                            grupo = 14;
                        }
                        if (parentesco.ToUpper().Contains("HIJO"))
                        {
                            codBeneficio = "MHIJO";
                            grupo = 11;
                        }

                        if (parentesco.ToUpper().Contains("CONYUGUE"))
                        {
                            codBeneficio = "MCON";
                            grupo = 10;
                        }

                        BeneficioGeneralDatos beneficio = new BeneficioGeneralDatos();
                        beneficio.cod_beneficio = new AfBeneficioIntegralDropsLista();
                        beneficio.cod_beneficio.item = codBeneficio;
                        beneficio.id_beneficio = 0;
                        beneficio.cedula = datos["cf_numero_de_cedula"].ToString().Trim();
                        beneficio.monto_aplicado = 0;
                        beneficio.registra_user = usuario;
                        beneficio.sepelio_identificacion = datos["cf_numero_de_identificacion_de_persona_fallecida"].ToString().Trim(); 
                        beneficio.sepelio_nombre = datos["cf_nombre_completo_de_persona_fallecida"].ToString().Trim();
                    
                        if(datos["cf_fecha_de_la_defuncion"] != null)
                        {
                            beneficio.sepelio_fecha_fallecimiento = Convert.ToDateTime(datos["cf_fecha_de_la_defuncion"]);
                        }

                        beneficio.estado = new AfBeneficioIntegralDropsLista();
                        beneficio.estado.item = null;
                        beneficio.consec = 0;

                        beneficio.requiere_justificacion = (jsonZoho.justificacion != null) ? true : false;
                        beneficio.notas = (jsonZoho.justificacion != null) ? jsonZoho.justificacion : "";

                        //obtengo el consecutivo monto del grupo
                        var query = $@"SELECT [MONTO]
                                  FROM [AFI_BENE_GRUPOS] WHERE COD_CATEGORIA = 'B_SEPE'
                                  AND COD_GRUPO in (
	                                  SELECT COD_GRUPO
	                                  FROM [AFI_BENEFICIOS] WHERE COD_CATEGORIA = 'B_SEPE'
	                                  AND COD_BENEFICIO = '{codBeneficio}'
                                  )";
                        beneficio.monto = connection.QueryFirstOrDefault<float>(query);

                        beneficio.monto_aplicado = beneficio.monto;

                        beneficio.tipo = new AfBeneficioIntegralDropsLista();
                        beneficio.tipo.item = "M";

                        frmAF_Beneficios_Integral_GenDB _beneIntegral = new frmAF_Beneficios_Integral_GenDB(_config);
                        respBeneficio = _beneIntegral.Guarda_Beneficio(CodEmpresa, beneficio, "Y", "API").Result;

                        if (respBeneficio.Code == -1)
                        {
                            response.Code = -1;
                            msjError += respBeneficio.Description + "...";
                        }
                        else
                        {
                            string[] expediente = respBeneficio.Description.Split('@');
                            //armo el numero de expediente
                            string nExpediente = expediente[0].ToString().PadLeft(6, '0') + codBeneficio.Trim() + expediente[1].ToString().PadLeft(6, '0');

                            //actualizo registro de ticket
                            query = $@"UPDATE [dbo].[AFI_BENE_OTORGA_INT]
                                       SET [N_EXPEDIENTE] = '{nExpediente}',
                                           [CONSEC] = '{expediente[1]}',
                                           COD_BENEFICIO = '{codBeneficio.Trim()}',
                                           ID_BENEFICIO = '{expediente[0]}',
                                           [ESTADO] = 'S' , INCLUIDO_POR = '{usuario}',INCLUIDO_FECHA = getdate() 
                                     WHERE ID_ZOHO = '{ticket.Id}'";
                            response.Code = connection.Execute(query);

                            
                           _ = BuscaArchivos(CodEmpresa,
                                ticket.Id, usuario, datos["cf_numero_de_cedula"].ToString().Trim(), 
                                expediente[0], codBeneficio.Trim());

                            if (expediente[0].ToString() != "0")
                            {
                                FrmFiltros filtros = new FrmFiltros();

                                filtros.codCliente = CodEmpresa;
                                filtros.cod_beneficio = codBeneficio.Trim();
                                filtros.id_beneficio = beneficio.id_beneficio;
                                filtros.socio = beneficio.cedula;
                                filtros.usuario = usuario;

                                IncluirRespuestasFormularios(filtros, datos);
                            }

                        }

                    }


                    if (msjError.Trim() != "")
                    {
                        response.Code = -1;
                        response.Description = msjError;

                        //actualizo registro de ticket
                        var query = $@"UPDATE [dbo].[AFI_BENE_OTORGA_INT]
                                       SET [MSJ_INTERFACE] = '{msjError}'
                                          ,[ESTADO] = 'E' , VISTO_POR = '{usuario}',VISTO_FECHA = getdate()
                                     WHERE ID_ZOHO = '{ticket.Id}'";
                        connection.Execute(query);

                    }
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
        /// Guarda el expediente de un ticket de tipo Desastres
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="ticket"></param>
        /// <param name="datos"></param>
        /// <param name="usuario"></param>
        /// <param name="jsonZoho"></param>
        /// <returns></returns>
        private ErrorDto Desastres_Guardar(int CodEmpresa, Ticket ticket, Dictionary<string, object> datos, string usuario, ZohoTicketAdd jsonZoho)
        {
            var response = new ErrorDto();
            var respBeneficio = new ErrorDto<BeneficioGeneralDatos>();
            response.Code = 0;

            string msjError = "", categoria = "";
            try
            {
                var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var connection = new SqlConnection(clienteConnString);
                {
                    //primer filtro de validaciones
                    if (datos["cf_numero_de_cedula"].ToString() == "" || datos["cf_numero_de_cedula"] == null)
                    {
                        response.Code = -1;
                        msjError += "Cédula no puede ser nula...";
                    }
                    BeneficioGeneralDatos beneficio = new BeneficioGeneralDatos();
                    //segundo filtro de validaciones
                    respBeneficio = _mBeneficiosDB.ValidaEstadoSocio(CodEmpresa, datos["cf_numero_de_cedula"].ToString().Trim());
                    if (respBeneficio.Code == -1)
                    {
                        response.Code = -1;
                        msjError += respBeneficio.Description + "...";
                    }

                    string codBeneficio = "";
                    var datosCat = new BeneficioGeneralDatos();
                    try
                    {
                        switch (datos["cf_tipo_de_desastre_1"].ToString())
                        {
                            case "Natural":
                                categoria = "B_DESA";
                                break;
                            default:
                                categoria = "B_DESN";
                                break;
                        }

                        datosCat.cod_categoria = categoria;
                    }
                    catch (Exception ex)
                    {
                        if (datos["cf_tipo_desastre_no_natural_acontecio_en_su_vivienda"] != null)
                        {
                            //consulta desastres disponibles.
                            var queryCat = $@"SELECT COD_CATEGORIA
                                          FROM [ASECCSS].[dbo].[AFI_BENEFICIOS]
                                          WHERE UPPER(DESCRIPCION) = UPPER('{datos["cf_tipo_desastre_no_natural_acontecio_en_su_vivienda"].ToString()}')";
                            datosCat = connection.QueryFirstOrDefault<BeneficioGeneralDatos>(queryCat);
                        }
                    }
                    

                    string tipoDesastre = datos["cf_indique_que_tipo_de_desastre"].ToString();

                    //consulta desastres disponibles.
                    var query = $@"SELECT COD_BENEFICIO FROM AFI_BENEFICIOS WHERE COD_CATEGORIA = '{datosCat.cod_categoria}' AND UPPER(DESCRIPCION) like '%{tipoDesastre.ToUpper()}%'";
                    codBeneficio = connection.QueryFirstOrDefault<string>(query);

                    beneficio.id_beneficio = 0;
                    beneficio.cod_beneficio = new AfBeneficioIntegralDropsLista();
                    beneficio.cod_beneficio.item = codBeneficio;
                    beneficio.cedula = datos["cf_numero_de_cedula"].ToString().Trim();
                    beneficio.desa_nombre = datos["cf_indique_que_tipo_de_desastre"].ToString();
                    beneficio.desa_descripcion = datos["cf_indique_que_tipo_de_desastre"].ToString();
                   
                    beneficio.monto_aplicado = 0;
                    beneficio.registra_user = usuario;
                    beneficio.estado = new AfBeneficioIntegralDropsLista();
                    beneficio.estado.item = null;
                    beneficio.consec = 0;

                    beneficio.requiere_justificacion = (jsonZoho.justificacion != null) ? true : false;
                    beneficio.notas = (jsonZoho.justificacion != null) ? jsonZoho.justificacion : "";

                    //obtengo el consecutivo monto del grupo
                    query = $@"SELECT [MONTO]
                          FROM [AFI_BENE_GRUPOS] WHERE COD_CATEGORIA = '{datosCat.cod_categoria}'
                          AND COD_GRUPO in (
                              SELECT COD_GRUPO
                              FROM [AFI_BENEFICIOS] WHERE COD_CATEGORIA = '{datosCat.cod_categoria}'
                              AND COD_BENEFICIO = '{codBeneficio}'
                          )";
                    beneficio.monto = connection.QueryFirstOrDefault<float>(query);
                    beneficio.monto_aplicado = beneficio.monto;

                    beneficio.tipo = new AfBeneficioIntegralDropsLista();
                    beneficio.tipo.item = "M";

                    frmAF_Beneficios_Integral_GenDB _beneIntegral = new frmAF_Beneficios_Integral_GenDB(_config);
                    respBeneficio = _beneIntegral.Guarda_Beneficio(CodEmpresa, beneficio, "Y", "API").Result;

                    if (respBeneficio.Code == -1)
                    {
                        response.Code = -1;
                        msjError += respBeneficio.Description + "...";
                    }
                    else
                    {
                        string[] expediente = respBeneficio.Description.Split('@');
                        //armo el numero de expediente  
                        string nExpediente = expediente[0].ToString().PadLeft(6, '0') + codBeneficio.Trim() + expediente[1].ToString().PadLeft(6, '0');

                        //actualizo registro de ticket
                        query = $@"UPDATE [dbo].[AFI_BENE_OTORGA_INT]
                               SET [N_EXPEDIENTE] = '{nExpediente}',
                                   [CONSEC] = '{expediente[1]}',
                                   COD_BENEFICIO = '{codBeneficio.Trim()}',
                                   ID_BENEFICIO = '{expediente[0]}',
                                   [ESTADO] = 'S', INCLUIDO_POR = '{usuario}',INCLUIDO_FECHA = getdate() 
                             WHERE ID_ZOHO = '{ticket.Id}'";
                        response.Code = connection.Execute(query);

                        _ = BuscaArchivos(CodEmpresa,
                                ticket.Id, usuario, datos["cf_numero_de_cedula"].ToString().Trim(), expediente[0], codBeneficio.Trim());
                        
                        
                        
                        if (expediente[0].ToString() != "0")
                        {
                            FrmFiltros filtros = new FrmFiltros();

                            filtros.codCliente = CodEmpresa;
                            filtros.cod_beneficio = datosCat.cod_categoria;
                            filtros.id_beneficio = beneficio.id_beneficio;
                            filtros.socio = beneficio.cedula;
                            filtros.usuario = usuario;

                            IncluirRespuestasFormularios(filtros, datos);
                        }


                    }

                    if (msjError.Trim() != "")
                    {
                        response.Code = -1;
                        response.Description = msjError;

                        //actualizo registro de ticket
                        query = $@"UPDATE [dbo].[AFI_BENE_OTORGA_INT]
                               SET [MSJ_INTERFACE] = '{msjError}'
                                  ,[ESTADO] = 'E'
                             WHERE ID_ZOHO = '{ticket.Id}'";
                        response.Code = connection.Execute(query);

                    }
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
        /// Guarda el expediente de un ticket de tipo FENA
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="ticket"></param>
        /// <param name="datos"></param>
        /// <param name="usuario"></param>
        /// <param name="jsonZoho"></param>
        /// <returns></returns>
        private ErrorDto FENA_Guardar(int CodEmpresa, Ticket ticket, Dictionary<string, object> datos, string usuario, ZohoTicketAdd jsonZoho)
        {
            var response = new ErrorDto();
            var respBeneficio = new ErrorDto<BeneficioGeneralDatos>();
            response.Code = 0;
            string msjError = "";
            try
            {
                var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var connection = new SqlConnection(clienteConnString);
                {
                    //primer filtro de validaciones
                    if (datos["cf_numero_de_cedula"].ToString() == "" || datos["cf_numero_de_cedula"] == null)
                    {
                        response.Code = -1;
                        msjError += "Cédula no puede ser nula...";
                    }

                    if (response.Code != -1)
                    {
                        //selecciono el beneficio de la base de datos
                        var query = $@"SELECT TOP 1 COD_BENEFICIO FROM AFI_BENEFICIOS WHERE COD_CATEGORIA = 'B_FENA'";
                        var codBeneficio = connection.QueryFirstOrDefault<string>(query);

                        BeneficioGeneralDatos beneficio = new BeneficioGeneralDatos();
                        beneficio.cod_beneficio = new AfBeneficioIntegralDropsLista();
                        beneficio.cod_beneficio.item = codBeneficio;
                        beneficio.id_beneficio = 0;
                        beneficio.cedula = datos["cf_numero_de_cedula"].ToString().Trim();
                        beneficio.monto_aplicado = 0;
                        beneficio.registra_user = usuario;
                        beneficio.sepelio_identificacion = null;
                        beneficio.estado = new AfBeneficioIntegralDropsLista();
                        beneficio.estado.item = "S";
                        beneficio.consec = 0;

                        beneficio.requiere_justificacion = (jsonZoho.justificacion != null) ? true : false;
                        beneficio.notas = (jsonZoho.justificacion != null) ? jsonZoho.justificacion : "";

                        //obtengo el consecutivo monto del grupo
                        query = $@"SELECT [MONTO]
                                  FROM [AFI_BENE_GRUPOS] WHERE COD_CATEGORIA = 'B_FENA'
                                  AND COD_GRUPO in (
	                                  SELECT COD_GRUPO
	                                  FROM [AFI_BENEFICIOS] WHERE COD_CATEGORIA = 'B_FENA'
	                                  AND COD_BENEFICIO = '{codBeneficio}'
                                  )";
                        beneficio.monto = connection.QueryFirstOrDefault<float>(query);

                        beneficio.tipo = new AfBeneficioIntegralDropsLista();
                        beneficio.tipo.item = "M";

                        frmAF_Beneficios_Integral_GenDB _beneIntegral = new frmAF_Beneficios_Integral_GenDB(_config);
                        respBeneficio = _beneIntegral.Guarda_Beneficio(CodEmpresa, beneficio, "Y", "API").Result;

                        if (respBeneficio.Code == -1)
                        {
                            response.Code = -1;
                            msjError += respBeneficio.Description + "...";
                        }
                        else
                        {
                            string[] expediente = respBeneficio.Description.Split('@');
                            //armo el numero de expediente  
                            string nExpediente = expediente[0].ToString().PadLeft(6, '0') + codBeneficio.Trim() + expediente[1].ToString().PadLeft(6, '0');

                            //actualizo registro de ticket
                            query = $@"UPDATE [dbo].[AFI_BENE_OTORGA_INT]
                               SET [N_EXPEDIENTE] = '{nExpediente}',
                                   [CONSEC] = '{expediente[1]}',
                                   COD_BENEFICIO = '{codBeneficio.Trim()}',
                                   ID_BENEFICIO = '{expediente[0]}',
                                   [ESTADO] = 'S', INCLUIDO_POR = '{usuario}',INCLUIDO_FECHA = getdate() 
                             WHERE ID_ZOHO = '{ticket.Id}'";
                            response.Code = connection.Execute(query);

                            _ = BuscaArchivos(CodEmpresa,
                                    ticket.Id, usuario, datos["cf_numero_de_cedula"].ToString().Trim(), expediente[0], codBeneficio.Trim());



                            if (expediente[0].ToString() != "0")
                            {
                                FrmFiltros filtros = new FrmFiltros();

                                filtros.codCliente = CodEmpresa;
                                filtros.cod_beneficio = codBeneficio.Trim();
                                filtros.id_beneficio = beneficio.id_beneficio;
                                filtros.socio = beneficio.cedula;
                                filtros.usuario = usuario;

                                IncluirRespuestasFormularios(filtros, datos);
                            }

                        }

                    }

                    if (msjError.Trim() != "")
                    {
                        response.Code = -1;
                        response.Description = msjError;

                        //actualizo registro de ticket
                        var query = $@"UPDATE [dbo].[AFI_BENE_OTORGA_INT]
                                       SET [MSJ_INTERFACE] = '{msjError}'
                                          ,[ESTADO] = 'E'
                                     WHERE ID_ZOHO = '{ticket.Id}'";
                        response.Code = connection.Execute(query);

                    }
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
        /// Guarda el expediente de un ticket de tipo Reconocimientos
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="ticket"></param>
        /// <param name="datos"></param>
        /// <param name="usuario"></param>
        /// <param name="jsonZoho"></param>
        /// <returns></returns>
        private ErrorDto Reconocimientos_Guardar(int CodEmpresa, Ticket ticket, Dictionary<string, object> datos, string usuario, ZohoTicketAdd jsonZoho)
        {
            var response = new ErrorDto();
            var respBeneficio = new ErrorDto<BeneficioGeneralDatos>();
            response.Code = 0;

            string msjError = "";
            try
            {
                var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var connection = new SqlConnection(clienteConnString);
                {
                    //primer filtro de validaciones
                    if (datos["cf_numero_de_cedula"].ToString() == "" || datos["cf_numero_de_cedula"] == null)
                    {
                        response.Code = -1;
                        msjError += "Cédula no puede ser nula...";
                    }

                    //segundo filtro de validaciones
                    respBeneficio = _mBeneficiosDB.ValidaEstadoSocio(CodEmpresa, datos["cf_numero_de_cedula"].ToString().Trim());
                    if (respBeneficio.Code == -1)
                    {
                        response.Code = -1;
                        msjError += respBeneficio.Description + "...";
                    }

                    if (response.Code != -1)
                    {
                        string reconocimiento = datos["cf_tipo_de_reconocimiento"].ToString().Trim();
                        string descripcion = "";
                        var codBeneficio = "";
                        var codGrupo = "";
                        switch (reconocimiento)
                        {
                            case "Académico":
                                codBeneficio = "MEAC";
                                break;
                            case "Científico":
                                codBeneficio = "MERC";
                                break;
                            case "Artístico":
                                codBeneficio = "MERA";
                                break;
                            case "Deportivo":
                                codBeneficio = "MERD";
                                break;
                        }

                         BeneficioGeneralDatos beneficio = new BeneficioGeneralDatos();
                        beneficio.cod_beneficio = new AfBeneficioIntegralDropsLista();
                        beneficio.cod_beneficio.item = codBeneficio;
                        beneficio.id_beneficio = 0;
                        beneficio.cedula = datos["cf_numero_de_cedula"].ToString().Trim();
                        beneficio.monto_aplicado = 0;
                        beneficio.registra_user = usuario;
                        beneficio.sepelio_identificacion = null;
                        beneficio.estado = new AfBeneficioIntegralDropsLista();
                        beneficio.estado.item = null;
                        beneficio.consec = 0;

                        beneficio.requiere_justificacion = (jsonZoho.justificacion != null) ? true : false;
                        beneficio.notas = (jsonZoho.justificacion != null) ? jsonZoho.justificacion : "";

                        //obtengo el consecutivo monto del grupo
                        var query = $@"SELECT [MONTO]
                                  FROM [AFI_BENE_GRUPOS] WHERE COD_CATEGORIA = 'B_RECO'
                                  AND COD_GRUPO in (
	                                  SELECT COD_GRUPO
	                                  FROM [AFI_BENEFICIOS] WHERE COD_CATEGORIA = 'B_RECO'
	                                  AND COD_BENEFICIO = '{codBeneficio}'
                                  )";
                        beneficio.monto = connection.QueryFirstOrDefault<float>(query);
                        beneficio.monto_aplicado = beneficio.monto;
                        beneficio.tipo = new AfBeneficioIntegralDropsLista();
                        beneficio.tipo.item = "M";

                        frmAF_Beneficios_Integral_GenDB _beneIntegral = new frmAF_Beneficios_Integral_GenDB(_config);
                        respBeneficio = _beneIntegral.Guarda_Beneficio(CodEmpresa, beneficio, "Y", "API").Result;

                        if (respBeneficio.Code == -1)
                        {
                            response.Code = -1;
                            msjError += respBeneficio.Description + "...";
                        }
                        else
                        {
                            string[] expediente = respBeneficio.Description.Split('@');
                            //armo el numero de expediente
                            string nExpediente = expediente[0].ToString().PadLeft(6, '0') + codBeneficio.Trim() + expediente[1].ToString().PadLeft(6, '0');

                            //actualizo registro de ticket
                            query = $@"UPDATE [dbo].[AFI_BENE_OTORGA_INT]
                                       SET [N_EXPEDIENTE] = '{nExpediente}',
                                           [CONSEC] = '{expediente[1]}',
                                           COD_BENEFICIO = '{codBeneficio.Trim()}',
                                           ID_BENEFICIO = '{expediente[0]}',
                                           [ESTADO] = 'S', INCLUIDO_POR = '{usuario}',INCLUIDO_FECHA = getdate() 
                                     WHERE ID_ZOHO = '{ticket.Id}'";
                            response.Code = connection.Execute(query);

                            //si contengo numero de id_beneficio incluyo registro de reconocimiento
                            if (expediente[0].ToString() != "0")
                            {
                                AF_Beneficios_Integral_RecDB afireconocimientos = new AF_Beneficios_Integral_RecDB(_config);
                                AfiBeneReconocimientos reconocimientoDatos = new AfiBeneReconocimientos();

                                reconocimientoDatos.id_beneficio = Convert.ToInt32(expediente[0]);
                                reconocimientoDatos.consec = Convert.ToInt32(expediente[1]);
                                reconocimientoDatos.cod_beneficio = codBeneficio;
                                reconocimientoDatos.cedula_estudiante = datos["cf_identificacion_de_estudiante"].ToString().Trim();
                                string nombreEstudiante = "";
                                try
                                {
                                    nombreEstudiante = datos["cf_nombre_de_estudiantes"].ToString().Trim(); 
                                }
                                catch (Exception)
                                {
                                    nombreEstudiante = datos["cf_nombre_y_apellidos_de_estudiante"].ToString().Trim();
                                }

                                //divido el nombre del estudiante en nombre y apellidos en 2 campos 
                                string[] nombres = nombreEstudiante.Split(' ');
                                if (nombres.Length > 1)
                                {
                                    try
                                    {
                                        reconocimientoDatos.nombre = nombres[0].ToString().Trim();
                                        reconocimientoDatos.primer_apellido = nombres[1].ToString().Trim();
                                        reconocimientoDatos.segundo_apellido = nombres[2].ToString().Trim();
                                    }
                                    catch (Exception)
                                    {
                                        reconocimientoDatos.nombre = nombres[0].ToString().Trim();
                                        reconocimientoDatos.primer_apellido = nombres[1].ToString().Trim();
                                    }
                                    
                                }
                                else
                                {
                                    reconocimientoDatos.nombre = datos["cf_nombre_de_estudiantes"].ToString().Trim();
                                }


                                reconocimientoDatos.fecha_nacimiento = (datos["cf_fecha_nacimiento_del_estudiante"] == null)? DateTime.Now : Convert.ToDateTime(datos["cf_fecha_nacimiento_del_estudiante"].ToString().Trim());
                                string genero = datos["cf_genero"].ToString().Trim();
                                reconocimientoDatos.genero = new AfBeneficioIntegralDropsLista();
                                switch (genero)
                                {
                                    
                                    case "Masculino":
                                        reconocimientoDatos.genero.descripcion = "Masculino";
                                        reconocimientoDatos.genero.item = "M";
                                        break;
                                    case "Femenino":
                                        reconocimientoDatos.genero.descripcion = "Femenino";
                                        reconocimientoDatos.genero.item = "F";
                                        break;
                                    default:
                                        reconocimientoDatos.genero.descripcion = "Otro";
                                        reconocimientoDatos.genero.item = "O";
                                        break;
                                }

                                //Obtengo la edad segun fecha de nacimiento
                                int edad = DateTime.Now.Year - reconocimientoDatos.fecha_nacimiento.Value.Year;
                                reconocimientoDatos.edad = edad;

                                string centroEducativo = datos["cf_tipo_de_centro_educativo"].ToString().Trim();
                                reconocimientoDatos.tipo_centro = new AfBeneficioIntegralDropsLista();
                                switch (centroEducativo)
                                {
                                    case "Privado":
                                        reconocimientoDatos.tipo_centro.item  = "PR";
                                        reconocimientoDatos.tipo_centro.descripcion = "Privado";
                                        break;
                                    case "Público":
                                        reconocimientoDatos.tipo_centro.item = "PU";
                                        reconocimientoDatos.tipo_centro.descripcion = "Público";
                                        break;
                                }
                                reconocimientoDatos.nivel_academico = new AfBeneficioIntegralDropsLista();
                                reconocimientoDatos.nivel_academico.item = datos["cf_grado_cursado_en_el_presente_ano"].ToString().Trim();
                                reconocimientoDatos.grado = new AfBeneficioIntegralDropsLista();
                                reconocimientoDatos.grado.item = datos["cf_grado_cursado_el_ano_anterior"].ToString().Trim();
                                reconocimientoDatos.tipo_reconocimiento = new AfBeneficioIntegralDropsLista();
                                switch (codBeneficio)
                                {
                                    case "MEAC":
                                        reconocimientoDatos.tipo_reconocimiento.item = "AC";
                                        break;
                                    case "MERC":
                                        reconocimientoDatos.tipo_reconocimiento.item = "CI";
                                        break;
                                    case "MERA":
                                        reconocimientoDatos.tipo_reconocimiento.item = "CUA";
                                        break;
                                    case "MERD":
                                        reconocimientoDatos.tipo_reconocimiento.item = "DE";
                                        break;
                                }
                                
                                reconocimientoDatos.matematicas = (datos["cf_promedio_matematica"] == null) ? 0 : Convert.ToInt32(datos["cf_promedio_matematica"].ToString());
                                reconocimientoDatos.ciencias = (datos["cf_promedio_ciencia_as"] == null) ? 0 : Convert.ToInt32(datos["cf_promedio_ciencia_as"].ToString());
                                reconocimientoDatos.estudios_sociales = (datos["cf_promedio_estudios_sociales"] == null) ? 0 : Convert.ToInt32(datos["cf_promedio_estudios_sociales"].ToString());
                                reconocimientoDatos.espanol = (datos["cf_promedio_espanol"] == null) ? 0 : Convert.ToInt32(datos["cf_promedio_espanol"].ToString());
                                reconocimientoDatos.idioma = (datos["cf_promedio_un_idioma_secundaria"] == null) ? 0 : Convert.ToInt32(datos["cf_promedio_un_idioma_secundaria"].ToString());

                                reconocimientoDatos.centro_educativo = datos["cf_nombre_del_centro_educativo"].ToString().Trim();

                                var errReconocimiento = afireconocimientos.BeneReconocimiento_Ingresar(CodEmpresa, reconocimientoDatos);
                            }

                           _ = BuscaArchivos(CodEmpresa,
                                ticket.Id, usuario, datos["cf_numero_de_cedula"].ToString().Trim(), expediente[0], codBeneficio.Trim());

                            if (expediente[0].ToString() != "0")
                            {
                                FrmFiltros filtros = new FrmFiltros();

                                filtros.codCliente = CodEmpresa;
                                filtros.cod_beneficio = codBeneficio.Trim();
                                filtros.id_beneficio = beneficio.id_beneficio;
                                filtros.socio = beneficio.cedula;
                                filtros.usuario = usuario;

                                IncluirRespuestasFormularios(filtros, datos);
                            }
                        }

                    }


                    if (msjError.Trim() != "")
                    {
                        response.Code = -1;
                        response.Description = msjError;

                        //actualizo registro de ticket
                        var query = $@"UPDATE [dbo].[AFI_BENE_OTORGA_INT]
                                       SET [MSJ_INTERFACE] = '{msjError}'
                                          ,[ESTADO] = 'E'
                                     WHERE ID_ZOHO = '{ticket.Id}'";
                        response.Code = connection.Execute(query);

                    }
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
        /// Marca un ticket como visto o no visto
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="ticket"></param>
        /// <param name="visto"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto<AfiBeneTicketsDatos> MarcaVisto_Actualizar(int CodEmpresa, string ticket, string visto, string usuario)
        {
            var response = new ErrorDto<AfiBeneTicketsDatos>();
            response.Code = 0;
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = "";
                    if (visto == "T")
                    {
                         query = $@"UPDATE AFI_BENE_OTORGA_INT
                                   SET [I_VISTO] = 1 , [VISTO_POR] = '{usuario}', [VISTO_FECHA] = GETDATE() 
                                   WHERE [VISTO_FECHA] is null";
                    }
                    else
                    {
                         query = $@"UPDATE AFI_BENE_OTORGA_INT
                                   SET [I_VISTO] = {((visto == "Y") ? 1 : 0)}
                                   , [VISTO_POR] = '{usuario}', [VISTO_FECHA] = GETDATE()
                                 WHERE ID_ZOHO  = '{ticket}'";
                    }
                    connection.Execute(query);
                    response.Result = TicketActualizado(CodEmpresa, ticket).Result;
                    response.Code = connection.Execute(query);
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
        /// Obtiene el contador de tickets pendientes por procesar
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<int> TicketsContador_Obtener(int CodEmpresa)
        {
            var response = new ErrorDto<int>();
            response.Code = 0;
            response.Result = 0;
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                      var  query = $@"select Count(ID_ZOHO) from AFI_BENE_OTORGA_INT
                                        WHERE (I_VISTO IS NULL OR I_VISTO = 0 ) 
                                        AND N_EXPEDIENTE IS NULL ";

                    response.Result = connection.Query<int>(query).FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = 0;
            }

            return response;
        }

        /// <summary>
        /// Guarda un ticket en la base de datos
        /// </summary>
        /// <param name="jsonZoho"></param>
        /// <returns></returns>
        public async Task<ErrorDto<AfiBeneTicketsDatos>> IncluirTicket_Guardar(ZohoTicketAdd jsonZoho)
        {
            var response = new ErrorDto<AfiBeneTicketsDatos>();
            var token = await ObtenerAuthZohoAsync(jsonZoho.CodEmpresa);

            if (token.Code == -1) return response;

            try
            {
                var jwtSettings = _config.GetSection("AppSettings");
                string departamenId = jwtSettings["DepartamentoZoho"];
                var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(jsonZoho.CodEmpresa);

                string baseUrl = $"https://desk.zoho.com/api/v1/tickets/{jsonZoho.ticket}";
                
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("orgId", "691715214");
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Zoho-oauthtoken", token.Result.access_token);

                    var respList = await client.GetAsync(baseUrl);
                    if (!respList.IsSuccessStatusCode)
                    {
                        response.Code = -1;
                        response.Description = "Error al obtener los tickets";
                        return response;
                    }

                    string result = await respList.Content.ReadAsStringAsync();
                    Ticket ticketJson = JsonConvert.DeserializeObject<Ticket>(result);
                    
        
                   
                    ErrorDto error =  Expediente_Guardar(jsonZoho.CodEmpresa, ticketJson, jsonZoho.usuario, jsonZoho);

                    if (error.Code == -1)
                    {
                        response.Code = -1;
                        response.Description = error.Description;
                        ActualizaError(jsonZoho.CodEmpresa, jsonZoho.ticket, error.Description, jsonZoho.usuario);
                    }
                    else
                    {

                        using var connection = new SqlConnection(clienteConnString);
                        {
                            var query = $@"UPDATE AFI_BENE_OTORGA_INT
                                           SET [VISTO_POR] = '{jsonZoho.usuario}', I_VISTO = 1, VISTO_FECHA = GETDATE() ,[I_PENDIENTE] = 1
                                           , [INCLUIDO_POR] = '{jsonZoho.usuario}', [INCLUIDO_FECHA] = GETDATE()
                                         WHERE ID_ZOHO  = '{jsonZoho.ticket}'";

                            connection.Execute(query);
                        }

                        response.Code = 0;
                        
                        response.Description = "Ticket Guardado";
                    }

                    response.Result = TicketActualizado(jsonZoho.CodEmpresa, jsonZoho.ticket).Result;
                    response.Description = "Tickets Sincronizados";
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
        /// Obtiene el ticket actualizado
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="ticket"></param>
        /// <returns></returns>
        private ErrorDto<AfiBeneTicketsDatos> TicketActualizado(int CodEmpresa, string ticket)
        {
            ErrorDto<AfiBeneTicketsDatos> response = new ErrorDto<AfiBeneTicketsDatos>();
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"select * from AFI_BENE_OTORGA_INT
                                        WHERE ID_ZOHO = '{ticket}' ";

                    response.Result = connection.Query<AfiBeneTicketsDatos>(query).FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = null;
            }

            return response;
        }

        /// <summary>
        /// Actualiza el mensaje de error en la tabla de tickets
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="ticket"></param>
        /// <param name="error"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        private ErrorDto ActualizaError(int CodEmpresa, string ticket, string error, string usuario)
        {
            var response = new ErrorDto();

            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"UPDATE AFI_BENE_OTORGA_INT SET MSJ_INTERFACE = '{error}'
                                     , ESTADO = 'E' , VISTO_POR = '{usuario}', I_VISTO = 1, VISTO_FECHA = getdate() 
                                   WHERE ID_ZOHO = '{ticket}'";

                    response.Code = connection.Execute(query);
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
        /// Carga los tickets pendientes de la tabla AFI_BENE_OTORGA_INT
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public async Task<ErrorDto> CargaTickets_Pendientes(int CodEmpresa)
        {
            var response = new ErrorDto();
            List<AfiBeneTicketsDatos> tikets = new List<AfiBeneTicketsDatos>();
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"SELECT *
                                      FROM [AFI_BENE_OTORGA_INT]
                                      WHERE ESTADO IN ('E', 'P') AND ENTRADA = 'INTERFACE'
                                      AND FECHA_CREACION >= getdate() -90";
                    tikets = connection.Query<AfiBeneTicketsDatos>(query).ToList();

                    if(tikets != null)
                    {
                        foreach(var ticket in tikets)
                        {
                            ZohoTicketAdd jsonZoho = new ZohoTicketAdd();
                            jsonZoho.CodEmpresa = CodEmpresa;
                            jsonZoho.ticket = ticket.id_zoho;
                            jsonZoho.usuario = "demo";
                            var error = await IncluirTicket_Guardar(jsonZoho);
                            
                        }
                    }

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
        /// Busca los archivos adjuntos de un ticket
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="ticket"></param>
        /// <param name="usuario"></param>
        /// <param name="llave1"></param>
        /// <param name="llave2"></param>
        /// <param name="cod_beneficio"></param>
        /// <returns></returns>
        private async Task<ErrorDto> BuscaArchivos(int CodEmpresa, string ticket, string usuario, 
            string llave1, string llave2, string cod_beneficio)
        {
            var response = new ErrorDto();
            var token = await ObtenerAuthZohoAsync(CodEmpresa);

            if (token.Code == -1) return response;

            try
            {
                var jwtSettings = _config.GetSection("AppSettings");
                string departamenId = jwtSettings["DepartamentoZoho"];
                var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

                string ThreadsUrl = $"https://desk.zoho.com/api/v1/tickets/{ticket}/threads";

                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("orgId", "691715214");
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Zoho-oauthtoken", token.Result.access_token);


                    var dataAttachments = await client.GetAsync(ThreadsUrl);
                    if (!dataAttachments.IsSuccessStatusCode)
                    {
                        response.Code = -1;
                        response.Description = "Error al obtener los Attachments";
                        return response;
                    }

                    string result = await dataAttachments.Content.ReadAsStringAsync();
                    var threadsBase = JsonConvert.DeserializeObject<AttachmentBase>(result);

                    int trardCount = 0;
                    foreach (var thread in threadsBase.data)
                    {

                        if(thread.attachmentCount > 0)
                        {
                            string baseUrl = $"https://desk.zoho.com/api/v1/tickets/{ticket}/threads/{thread.id}";
                            trardCount += 1;
                            var respList = await client.GetAsync(baseUrl);
                            if (!respList.IsSuccessStatusCode)
                            {
                                response.Code = -1;
                                response.Description = "Error al obtener los tickets";
                                return response;
                            }

                            result = await respList.Content.ReadAsStringAsync();
                            TicketAttachments ticketJson = JsonConvert.DeserializeObject<TicketAttachments>(result);

                            int fileCount = 0;
                            foreach (var archivo in ticketJson.attachments)
                            {
                                byte[] file = await client.GetByteArrayAsync(archivo.href);
                                if (file.Length == 0)
                                {
                                    response.Code = -1;
                                    response.Description = "Error al obtener los tickets";
                                    return response;
                                }

                                DocumentosArchivoDTO documento = new DocumentosArchivoDTO();
                                documento.fileid = null;

                                documento.llave_01 = llave1;
                                documento.llave_02 = Convert.ToInt32(llave2).ToString();
                                documento.llave_03 = fileCount.ToString();

                                documento.moduloid = "CL_01";
                                documento.typeid = "999";
                                documento.filename = archivo.name;
                                //obtengo la extencion del archivo
                                documento.filetype = archivo.name.Substring(archivo.name.LastIndexOf('.') + 1);
                                documento.filecontent = file;
                                documento.fechaemision = DateTime.Now.ToUniversalTime().ToString("yyyy-MM-ddT00:00:00.000Z");
                                documento.vencimiento = DateTime.Now.ToUniversalTime().ToString("yyyy-MM-ddT00:00:00.000Z");
                                documento.registrofecha = DateTime.Now.ToUniversalTime().ToString("yyyy-MM-ddT00:00:00.000Z");
                                documento.registrousuario = usuario;
                                documento.empresaid = CodEmpresa.ToString();
                                documento.enable = "1";

                                _mDocumentosDB.Documentos_Insertar(CodEmpresa, documento);
                                fileCount++;
                            }
                        }
                        
                    }

                    
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
        /// Incluye las respuestas de los formularios en la base de datos
        /// </summary>
        /// <param name="filtros"></param>
        /// <param name="datos"></param>
        private void IncluirRespuestasFormularios(FrmFiltros filtros, Dictionary<string, object> datos)
        {
            frmAF_Bene_FormulariosDB _frmRespuestas = new frmAF_Bene_FormulariosDB(_config);
            //convierto en string el objeto
            string Jdatos = JsonConvert.SerializeObject(filtros);

            List<Formulario> form = _frmRespuestas.AfBeneFormSocios_Obtener(Jdatos).Result;

            foreach (var item in form)
            {
                Form form1 = new Form();
                form1.id = item.id_form;
                form1.questions = new List<FormQuestion>();
                form1.questions = item.formulario.questions;


                foreach (var question in item.formulario.questions)
                {
                    bool requerido = false;
                    if (question.requerido == true)
                    {
                        requerido = true;
                    }

                    bool homologado = false;
                    object value = null;
                    if(question.campo_homologado != null)
                    {
                        if (datos.TryGetValue(question.campo_homologado, out value))
                        {
                            homologado = true;
                        }
                    }
                    

                    if (requerido && homologado)
                    {
                       question.respuesta = RegresaRespuesta(question, value);
                    }
                    else if(requerido && !homologado)
                    {
                        if(question.opciones != null)
                        {
                            value = question.opciones[0];
                        }
                        else
                        {
                            value = "NA";
                        }
                        question.respuesta = RegresaRespuesta(question, value);
                    }
                    else if (!requerido && homologado)
                    {
                        question.respuesta = RegresaRespuesta(question, value);
                    }
                    else if (!requerido && !homologado)
                    {
                        question.respuesta = null;
                    }
                }
                _frmRespuestas.AfBeneFrmRespuesta_Agregar(Jdatos, form1);
            }

        }

        /// <summary>
        /// Regresa la respuesta del formulario
        /// </summary>
        /// <param name="question"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        private object RegresaRespuesta(FormQuestion question, object value)
        {
            object respuesta = null;
            var resList = new List<OptionabledQuestion>();
            switch (question.pregunta_tipo)
            {
                case "radio":
                    resList = new List<OptionabledQuestion>();
                    string resUserCk = value?.ToString() ?? string.Empty;

                    foreach (var opcion in question.opciones)
                    {
                        if (opcion.descripcion.ToUpper().Contains(resUserCk.ToUpper()))
                        {
                            resList.Add(new OptionabledQuestion
                            {
                                id_opciones = opcion.id_opciones,
                                item = opcion.item,
                                descripcion = opcion.descripcion,
                                selected = true,
                            });
                        }
                        else
                        {
                            resList.Add(new OptionabledQuestion
                            {
                                id_opciones = opcion.id_opciones,
                                item = opcion.item,
                                descripcion = opcion.descripcion,
                                selected = false,
                            });
                        }
                        break;
                    }
                    respuesta = resList[0].item;
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
                    
                    string resUser = value?.ToString() ?? string.Empty;
                    //busco respuesta en las opciones
                    if (resUser != null)
                    {
                        string[] resListUser = resUser.Split(';');
                        foreach (var opcion in question.opciones)
                        {
                            foreach (var res in resListUser)
                            {
                                if (res == null)
                                {
                                    continue;
                                }
                                if (opcion.descripcion.ToUpper().Contains(res.ToUpper()))
                                {
                                    resList.Add(new OptionabledQuestion
                                    {
                                        id_opciones = opcion.id_opciones,
                                        item = opcion.item,
                                        descripcion = opcion.descripcion,
                                        selected = true,
                                    });
                                    break;
                                }
                            }
                        }
                    }

                    respuesta = JsonConvert.SerializeObject(resList);

                    break;
            }
            return respuesta;
        }

    }
}
