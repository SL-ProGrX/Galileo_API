using Dapper;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using PgxAPI.Models;
using PgxAPI.Models.AF;
using PgxAPI.Models.ERROR;
using System.Data;


namespace PgxAPI.DataBaseTier
{
    public class frmAF_Beneficios_IntegralDB
    {
        private readonly IConfiguration _config;
        private readonly mBeneficiosDB _mBeneficiosDB;
        private readonly EnvioCorreoDB _envioCorreoDB;
        public string sendEmail = "";
        public string nofiticacionCobros = "";

        public frmAF_Beneficios_IntegralDB(IConfiguration config)
        {
            _config = config;
            _mBeneficiosDB = new mBeneficiosDB(_config);
            _envioCorreoDB = new EnvioCorreoDB(_config);
            sendEmail = _config.GetSection("AppSettings").GetSection("EnviaEmail").Value.ToString();
            nofiticacionCobros = _config.GetSection("AFI_Beneficios").GetSection("NotificacionCobros").Value.ToString();
        }


        /// <summary>
        /// Obtengo catálogos de tabla SYS y BENE donde tipo es el tipo de catalogo y modulo es el código de la tabla
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="tipo"></param>
        /// <param name="modulo"></param>
        /// <returns></returns>
        public ErrorDTO<List<CatalogosLista>> Catalogo_Obtener(int CodEmpresa, int tipo, int modulo)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDTO<List<CatalogosLista>>();
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var procedure = "[spAFI_Bene_Catalogos_Consulta]";
                    var values = new
                    {
                        tipo = tipo,
                        Codigo = modulo
                    };
                    response.Result = connection.Query<CatalogosLista>(procedure, values, commandType: System.Data.CommandType.StoredProcedure).ToList();
                }

            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = "spAFI_Bene_Catalogos_Consulta: " + ex.Message;
                response.Result = null;
            }


            return response;
        }


        /// <summary>
        /// Lista de categorias de beneficios (apremiente, crece, sepelio, etc)
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <returns></returns>
        public ErrorDTO<List<AfBeneficioIntegralDropsLista>> BeneIntegralCategorias_Obtener(int CodCliente)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            var response = new ErrorDTO<List<AfBeneficioIntegralDropsLista>>();
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"SELECT COD_CATEGORIA AS item, DESCRIPCION 
                                  FROM AFI_BENE_CATEGORIAS ";

                    response.Result = connection.Query<AfBeneficioIntegralDropsLista>(query).ToList();
                }

            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = "BeneIntegralCategorias_Obtener: " + ex.Message;
                response.Result = null;
            }
            return response;
        }

        /// <summary>
        /// Obtengo la lista de observaciones por beneficio seleccionado
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="consec"></param>
        /// <param name="cod_beneficio"></param>
        /// <returns></returns>
        public ErrorDTO<List<AfiBeneObservaciones>> BeneIntegralObservaciones_Obtener(int CodCliente, int consec, string cod_beneficio)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            var response = new ErrorDTO<List<AfiBeneObservaciones>>();
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"SELECT [ID_OBSERVACION]
                                      ,[COD_BENEFICIO]
                                      ,[CONSEC]
                                      ,[OBSERVACION]
                                      ,[REGISTRO_FECHA]
                                      ,[REGISTRO_USUARIO]
                                  FROM AFI_BENE_REGISTRO_OBSERVACIONES WHERE 
                                       COD_BENEFICIO = '{cod_beneficio}' AND CONSEC = {consec} ";

                    response.Result = connection.Query<AfiBeneObservaciones>(query).ToList();
                }

            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = "BeneIntegralObservaciones_Obtener: " + ex.Message;
                response.Result = null;
            }
            return response;
        }

        /// <summary>
        /// Metodo para guardar la observación del beneficio seleccionado
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="observacion"></param>
        /// <returns></returns>
        public ErrorDTO BeneIntegralObservaciones_Guardar(int CodCliente, AfiBeneObservaciones observacion)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            ErrorDTO info = new ErrorDTO();
            try
            {
                //Valido si es una nueva observación o una actualización
                if (observacion.id_observacion == 0)
                {
                    info = BeneIntegralObservaciones_Insertar(CodCliente, observacion);
                }
                else
                {
                    info = BeneIntegralObservaciones_Actualizar(CodCliente, observacion);
                }

            }
            catch (Exception ex)
            {
                info.Code = -1;
                info.Description = ex.Message;
            }
            return info;
        }

        /// <summary>
        /// Inserto una observación nueva al beneficio seleccionado
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="observacion"></param>
        /// <returns></returns>
        private ErrorDTO BeneIntegralObservaciones_Insertar(int CodCliente, AfiBeneObservaciones observacion)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            ErrorDTO info = new ErrorDTO();
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"INSERT INTO [dbo].[AFI_BENE_REGISTRO_OBSERVACIONES]
                                           ([COD_BENEFICIO]
                                           ,[CONSEC]
                                           ,[OBSERVACION]
                                           ,[REGISTRO_FECHA]
                                           ,[REGISTRO_USUARIO])
                                     VALUES
                                           ('{observacion.cod_beneficio}'
                                           ,{observacion.consec}
                                           ,'{observacion.observacion}'
                                           ,getDate()
                                           ,'{observacion.registro_usuario}')";

                    info.Code = connection.Execute(query);

                    query = "SELECT IDENT_CURRENT('AFI_BENE_REGISTRO_OBSERVACIONES') as 'id'";
                    var id = connection.Query<int>(query).FirstOrDefault();
                    info.Description = id.ToString();

                    _mBeneficiosDB.BitacoraBeneficios(new BitacoraBeneInsertarDTO
                    {
                        EmpresaId = CodCliente,
                        cod_beneficio = observacion.cod_beneficio,
                        consec = observacion.consec,
                        movimiento = "Inserta",
                        detalle = $@"Inserta Observación [{observacion.observacion}]",
                        registro_usuario = observacion.registro_usuario
                    });
                }

            }
            catch (Exception ex)
            {
                info.Code = -1;
                info.Description = ex.Message;
            }
            return info;
        }

        /// <summary>
        /// Actualizo la observación del beneficio seleccionado
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="observacion"></param>
        /// <returns></returns>
        private ErrorDTO BeneIntegralObservaciones_Actualizar(int CodCliente, AfiBeneObservaciones observacion)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            ErrorDTO info = new ErrorDTO();
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"SELECT OBSERVACION FROM AFI_BENE_REGISTRO_OBSERVACIONES 
                    WHERE ID_OBSERVACION = {observacion.id_observacion}";
                    string observacionAnterior = connection.Query<string>(query).FirstOrDefault();

                    query = $@"UPDATE [AFI_BENE_REGISTRO_OBSERVACIONES]
                                   SET [OBSERVACION] = '{observacion.observacion}'
                                 WHERE ID_OBSERVACION = {observacion.id_observacion} ";

                    info.Code = connection.Execute(query);
                    info.Description = observacion.id_observacion.ToString();

                    _mBeneficiosDB.BitacoraBeneficios(new BitacoraBeneInsertarDTO
                    {
                        EmpresaId = CodCliente,
                        cod_beneficio = observacion.cod_beneficio,
                        consec = observacion.consec,
                        movimiento = "Actualiza",
                        detalle = $@"Actualiza Observación [{observacionAnterior}] por [{observacion.observacion}]",
                        registro_usuario = observacion.registro_usuario
                    });
                }

            }
            catch (Exception ex)
            {
                info.Code = -1;
                info.Description = ex.Message;
            }
            return info;
        }

        /// <summary>
        /// Eliminar la observación del beneficio seleccionado
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="id_observacion"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDTO BeneIntegralObservaciones_Eliminar(int CodCliente, int id_observacion, string usuario)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            ErrorDTO info = new ErrorDTO();
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    //Reviso el cod_beneficio y consec del registro a eliminar
                    var query = $@"SELECT COD_BENEFICIO, CONSEC, observacion FROM AFI_BENE_REGISTRO_OBSERVACIONES WHERE ID_OBSERVACION = {id_observacion} ";
                    var registro = connection.Query<AfiBeneObservaciones>(query).FirstOrDefault();

                    query = $@"DELETE FROM [AFI_BENE_REGISTRO_OBSERVACIONES] WHERE ID_OBSERVACION = {id_observacion} ";

                    info.Code = connection.Execute(query);

                    _mBeneficiosDB.BitacoraBeneficios(new BitacoraBeneInsertarDTO
                    {
                        EmpresaId = CodCliente,
                        cod_beneficio = registro.cod_beneficio,
                        consec = registro.consec,
                        movimiento = "Elimina",
                        detalle = $@"Elimina Observación [{registro.observacion}]",
                        registro_usuario = usuario
                    });
                }

            }
            catch (Exception ex)
            {
                info.Code = -1;
                info.Description = ex.Message;
            }
            return info;
        }

        /// <summary>
        /// Obtengo lista de registros de bitacora de un beneficio seleccionado
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Cod_Beneficio"></param>
        /// <param name="Consec"></param>
        /// <returns></returns>
        public ErrorDTO<List<BitacoraBeneficioIntegralDTO>> BitacoraBeneficioIntegral_Obtener(int CodEmpresa, string Cod_Beneficio, int Consec)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDTO<List<BitacoraBeneficioIntegralDTO>>();
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {

                    var query = $@"SELECT ID_BITACORA, CONSEC, REGISTRO_FECHA, COD_BENEFICIO,REGISTRO_USUARIO, DETALLE, MOVIMIENTO
                       FROM AFI_BENE_REGISTRO_BITACORA WHERE COD_BENEFICIO = '{Cod_Beneficio}' and CONSEC = {Consec} ORDER BY 1 ASC";

                    response.Result = connection.Query<BitacoraBeneficioIntegralDTO>(query).ToList();

                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = "BitacoraBeneficioIntegral_Obtener: " + ex.Message;
                response.Result = null;
            }
            return response;
        }

        /// <summary>
        /// Obtengo el expediente del beneficio seleccionado
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="cedula"></param>
        /// <param name="id_beneficio"></param>
        /// <param name="categoria"></param>
        /// <returns></returns>
        public ErrorDTO<object> BeneIntegralRepExpediente_Obtener(int CodEmpresa, string cedula, int id_beneficio, string categoria)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            // Crear un objeto DataSet para almacenar las tablas devueltas
            DataSet ds = new DataSet();
            // Crear un diccionario para almacenar las tablas como JSON
            var tablesAsJson = new Dictionary<string, object>();

            ErrorDTO<object> response = new ErrorDTO<object>();
            try
            {
                using (SqlConnection connection = new SqlConnection(clienteConnString))
                {
                    // Abrir la conexión
                    connection.Open();

                    // Crear el comando para ejecutar el procedimiento almacenado
                    using (SqlCommand command = new SqlCommand("spAFI_Bene_ExpExpediente_Consulta", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.Add(new SqlParameter("@cedula", cedula));
                        command.Parameters.Add(new SqlParameter("@id_beneficio", id_beneficio));
                        command.Parameters.Add(new SqlParameter("@categoria", categoria));

                        // Crear un SqlDataAdapter para llenar el DataSet
                        using (SqlDataAdapter da = new SqlDataAdapter(command))
                        {
                            // Llenar el DataSet con las tablas devueltas por el SP
                            da.Fill(ds);
                        }
                    }
                }
                // Iterar sobre las tablas del DataSet
                foreach (DataTable table in ds.Tables)
                {
                    // Convertir la tabla a JSON y agregarla al diccionario
                    tablesAsJson[table.TableName] = JsonConvert.SerializeObject(table);
                }

                response.Code = 0;
                response.Result = tablesAsJson;
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Result = null;
                response.Description = "BeneIntegralRepExpediente_Obtener: " + ex.Message;
            }
            return response;
        }

        /// <summary>
        /// Obtengo la lista de beneficios para aprobacion masiva
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Categoria"></param>
        /// <param name="filtroString"></param>
        /// <returns></returns>
        public ErrorDTO<BeneConsultaDatosLista> BeneficiosParaAprobacionMasiva_Obtener(int CodEmpresa, string Categoria, string filtroString)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var filtros = JsonConvert.DeserializeObject<AfiBeneFiltros>(filtroString);
            var response = new ErrorDTO<BeneConsultaDatosLista>
            {
                Result = new BeneConsultaDatosLista(),
                Code = 0
            };
            response.Result.total = 0;
            var validaBeneficio = new ErrorDTO();
            var beneficio = new BeneficioGeneralDatos();
            string paginaActual = " ", paginacionActual = " ", where = " ";
            try
            {
                if (filtros.filtro != null && filtros.filtro != "")
                {
                    where = " AND (Expediente LIKE '%" + filtros.filtro + "%' OR O.cedula LIKE '%" + filtros.filtro + "%' OR O.NOMBRE_BENEFICIARIO LIKE '%" + filtros.filtro + "%') ";
                }
                if (filtros.cod_grupo != null && filtros.cod_grupo != "TODOS")
                {
                    where += $" AND O.COD_BENEFICIO IN (select COD_BENEFICIO from AFI_BENEFICIOS where COD_GRUPO = {filtros.cod_grupo}) ";
                }
                if (filtros.pagina != null)
                {
                    paginaActual = " OFFSET " + filtros.pagina + " ROWS ";
                    paginacionActual = " FETCH NEXT " + ((filtros.paginacion == null) ? 30 : filtros.paginacion) + " ROWS ONLY ";
                }
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"select count(O.ID_BENEFICIO) 
                    from vBeneficios_W_Integral O 
                    LEFT JOIN AFI_BENE_ESTADOS E ON E.COD_ESTADO = O.ESTADO AND E.COD_ESTADO IN (
                    SELECT COD_ESTADO FROM AFI_BENE_GRUPO_ESTADOS WHERE COD_GRUPO IN (
                    SELECT COD_GRUPO FROM AFI_BENE_GRUPOS  Where COD_CATEGORIA like '%{Categoria}%')) 
                    WHERE O.COD_BENEFICIO IN (SELECT COD_BENEFICIO FROM AFI_BENEFICIOS Where COD_CATEGORIA like '%{Categoria}%') 
                    AND E.P_FINALIZA = 1 AND E.PROCESO = 'T' AND E.ACTIVO = '1' {where} ";

                    response.Result.total = connection.Query<int>(query).FirstOrDefault();

                    query = $@"select 
                    (SELECT CONCAT(RIGHT(CONCAT('00000', O.ID_BENEFICIO), 5), TRIM(O.COD_BENEFICIO) , RIGHT(CONCAT('00000',O.CONSEC), 5) )) AS Expediente, 
                    O.REGISTRA_FECHA, --Fecha solicitud / registro
                    O.AUTORIZA_FECHA, --Fecha Aprovacion
                    O.ID_BENEFICIO, -- Expediente
                    O.CONSEC,   -- Expediente
                    O.COD_BENEFICIO,   -- Expediente
                    O.Beneficio_Desc, -- Beneficio
                    O.MONTO, --Monto Aprobado
                    O.MONTO_APLICADO, -- Monto Aplicado
                    O.ESTADO, 
                    ISNULL(E.DESCRIPCION, 'SIN DEFINIR') AS estado_desc, -- Estado
                    O.cedula,  -- cedula
                    O.NOMBRE_BENEFICIARIO, -- nombre completo 
                    O.registra_user, --usuario
                    Categoria_Desc, 
                    Estado_Persona,
                    O.TIPO,
                    CASE 
				        WHEN O.TIPO = 'M' THEN 'Monetario'
                        WHEN O.TIPO = 'P' THEN 'Producto'
				        ELSE 'Ambos'
                    END AS TipoDesc
                    from vBeneficios_W_Integral O 
                    LEFT JOIN AFI_BENE_ESTADOS E ON E.COD_ESTADO = O.ESTADO AND E.COD_ESTADO IN (
                    SELECT COD_ESTADO FROM AFI_BENE_GRUPO_ESTADOS WHERE COD_GRUPO IN (
                    SELECT COD_GRUPO FROM AFI_BENE_GRUPOS  Where COD_CATEGORIA like '%{Categoria}%')) 
                    WHERE O.COD_BENEFICIO IN (SELECT COD_BENEFICIO FROM AFI_BENEFICIOS Where COD_CATEGORIA like '%{Categoria}%') 
                    AND E.P_FINALIZA = 1 AND E.PROCESO = 'T' AND E.ACTIVO = '1' {where} Order by O.REGISTRA_FECHA desc 
                    {paginaActual} {paginacionActual}";

                    response.Result.lista = connection.Query<BeneConsultaDatos>(query).ToList();

                    foreach (var item in response.Result.lista)
                    {
                        AfBeneficioIntegralDropsLista estado = new();
                        AfBeneficioIntegralDropsLista cod_beneficio = new();
                        estado.item = item.estado;
                        cod_beneficio.item = item.cod_beneficio;
                        beneficio.estado = estado;
                        beneficio.cedula = item.cedula;
                        beneficio.monto_aplicado = item.monto_aplicado;
                        beneficio.registra_user = item.registra_user;
                        beneficio.cod_beneficio = cod_beneficio;
                        validaBeneficio = _mBeneficiosDB.ValidarPersona(CodEmpresa, beneficio.cedula, beneficio.cod_beneficio.item);
                        if (validaBeneficio.Code == -1)
                        {
                            item.valida_beneficio = validaBeneficio.Description;
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
        /// Aprobar los beneficios seleccionados
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="lista"></param>
        /// <returns></returns>
        public ErrorDTO BeneIntegral_AprobacionMasiva(int CodEmpresa, string lista)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var beneficios = JsonConvert.DeserializeObject<List<BeneficioGuadar>>(lista);
            ErrorDTO info = new ErrorDTO();
            info.Code = 0;
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    foreach (BeneficioGuadar beneficio in beneficios)
                    {

                        var query = $@"update afi_bene_otorga set autoriza_fecha = getDate(), autoriza_user = '{beneficio.usuario}', 
                        estado = 'A' 
                        where id_beneficio = {beneficio.id_beneficio} ";
                        info.Code = connection.Execute(query);

                        query = @$"UPDATE [dbo].[AFI_BENE_REGISTRO_ESTADOS]
                               SET
                                   [COD_ESTADO]  =  'A' 
                                   ,[NOTAS] = '{beneficio.estadoObservaciones}'
                                   ,[REGISTRO_FECHA] = GETDATE()
                                   ,[REGISTRO_USUARIO] = '{beneficio.usuario}' 
                             WHERE CONSEC = {beneficio.consec} AND  [COD_BENEFICIO] = '{beneficio.cod_beneficio}' ";
                        connection.Execute(query);

                        _mBeneficiosDB.BitacoraBeneficios(new BitacoraBeneInsertarDTO
                        {
                            EmpresaId = CodEmpresa,
                            cod_beneficio = beneficio.cod_beneficio,
                            consec = beneficio.consec,
                            movimiento = "Actualizar",
                            detalle = $@"Actualiza Estado del Beneficio [{beneficio.id_beneficio}] - Nota: [{beneficio.estadoObservaciones}]",
                            registro_usuario = beneficio.usuario
                        });
                    }
                    info.Description = "Beneficios Aprobados Correctamente!";
                }

            }
            catch (Exception ex)
            {
                info.Code = -1;
                info.Description = ex.Message;
            }
            return info;
        }

        /// <summary>
        /// Obtengo la lista de beneficios para control mensual
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Categoria"></param>
        /// <param name="filtroString"></param>
        /// <returns></returns>
        public ErrorDTO<BeneConsultaDatosLista> BeneficiosControMensual_Obtener(int CodEmpresa, string Categoria, string filtroString)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var filtros = JsonConvert.DeserializeObject<AfiBeneFiltros>(filtroString);
            var response = new ErrorDTO<BeneConsultaDatosLista>
            {
                Result = new BeneConsultaDatosLista(),
                Code = 0
            };
            response.Result.total = 0;
            string paginaActual = " ", paginacionActual = " ", where = " ";
            try
            {
                if (filtros.filtro != null && filtros.filtro != "")
                {
                    where = " AND (Expediente LIKE '%" + filtros.filtro + "%' OR O.cedula LIKE '%" + filtros.filtro + "%' OR O.NOMBRE_BENEFICIARIO LIKE '%" + filtros.filtro + "%') ";
                }

                if (filtros.pagina != null)
                {
                    paginaActual = " OFFSET " + filtros.pagina + " ROWS ";
                    paginacionActual = " FETCH NEXT " + filtros.paginacion + " ROWS ONLY ";
                }
                using var connection = new SqlConnection(clienteConnString);
                {
                    var queryCodBene = "";
                    string cod_beneficio = "";
                    if (filtros.cod_grupo != null && filtros.cod_grupo != "TODOS")
                    {
                        queryCodBene = $"select COD_BENEFICIO from AFI_BENEFICIOS where COD_GRUPO = {filtros.cod_grupo} ";
                        cod_beneficio = connection.Query<string>(queryCodBene).FirstOrDefault();
                    }



                    var query = $@"SELECT  
                                           COUNT(*)
                                        FROM vBeneficios_W_Integral O  
                                        LEFT JOIN AFI_BENE_ESTADOS E  
                                            ON E.COD_ESTADO = O.ESTADO  
                                            AND E.COD_ESTADO IN (  
                                                SELECT COD_ESTADO  
                                                FROM AFI_BENE_GRUPO_ESTADOS  
                                                WHERE COD_GRUPO IN (  
                                                    SELECT COD_GRUPO FROM AFI_BENE_GRUPOS WHERE COD_CATEGORIA = '{Categoria}'  
                                                )  
                                            )  
                                        LEFT JOIN AFI_BENE_OTORGA OB  
                                            ON O.ID_BENEFICIO = OB.ID_BENEFICIO  
                                            AND OB.aplica_pago_masivo = 1  
                                        WHERE  
                                            O.COD_BENEFICIO IN (  
                                                SELECT COD_BENEFICIO FROM AFI_BENEFICIOS WHERE COD_CATEGORIA = '{Categoria}'  
                                            )  
                                            AND E.P_FINALIZA = 1  
                                            AND E.PROCESO = 'A'  
                                            AND OB.ID_BENEFICIO IS NOT NULL  -- Evita la subconsulta en IN  
                                            AND O.MONTO_APLICADO <> (  
                                                SELECT COALESCE(SUM(P.MONTO), 0)  
                                                FROM AFI_BENE_PAGO P  
                                                WHERE P.COD_BENEFICIO = O.COD_BENEFICIO  
                                                AND P.CONSEC = O.CONSEC  
                                            )";
                    response.Result.total = connection.Query<int>(query).FirstOrDefault();

                    query = $@"SELECT  
                                CONCAT(RIGHT(CONCAT('00000', O.ID_BENEFICIO), 5), TRIM(O.COD_BENEFICIO), RIGHT(CONCAT('00000', O.CONSEC), 5)) AS Expediente,  
                                O.REGISTRA_FECHA, -- Fecha solicitud / registro  
                                O.AUTORIZA_FECHA, -- Fecha Aprobación  
                                O.ID_BENEFICIO, -- Expediente  
                                O.CONSEC, -- Expediente  
                                O.COD_BENEFICIO, -- Expediente  
                                O.Beneficio_Desc, -- Beneficio  
                                O.MONTO, -- Monto Aprobado  
                                O.MONTO_APLICADO - COALESCE(
                                    (SELECT SUM(P.MONTO)  
                                     FROM AFI_BENE_PAGO P  
                                     WHERE P.COD_BENEFICIO = O.COD_BENEFICIO  
                                       AND P.CONSEC = O.CONSEC), 0) AS MONTO_APLICADO, -- Monto Aplicado  
                                O.ESTADO,  
                                COALESCE(E.DESCRIPCION, 'SIN DEFINIR') AS estado_desc, -- Estado  
                                O.CEDULA, -- Cédula  
                                O.NOMBRE_BENEFICIARIO, -- Nombre completo  
                                O.REGISTRA_USER, -- Usuario  
                                O.Categoria_Desc,  
                                O.Estado_Persona,  
                                O.TIPO,  
                                CASE  
                                    WHEN O.TIPO = 'M' THEN 'Monetario'  
                                    WHEN O.TIPO = 'P' THEN 'Producto'  
                                    ELSE 'Ambos'  
                                END AS TipoDesc  
                            FROM vBeneficios_W_Integral O  
                            LEFT JOIN AFI_BENE_ESTADOS E  
                                ON E.COD_ESTADO = O.ESTADO  
                                AND E.COD_ESTADO IN (  
                                    SELECT COD_ESTADO  
                                    FROM AFI_BENE_GRUPO_ESTADOS  
                                    WHERE COD_GRUPO IN (  
                                        SELECT COD_GRUPO FROM AFI_BENE_GRUPOS WHERE COD_CATEGORIA = '{Categoria}'  
                                    )  
                                )  
                            LEFT JOIN AFI_BENE_OTORGA OB  
                                ON O.ID_BENEFICIO = OB.ID_BENEFICIO  
                                AND OB.aplica_pago_masivo = 1  
                            WHERE  
                                O.COD_BENEFICIO IN (  
                                    SELECT COD_BENEFICIO FROM AFI_BENEFICIOS WHERE COD_CATEGORIA = '{Categoria}'  
                                )  
                                AND E.P_FINALIZA = 1  
                                AND E.PROCESO = 'A'  
                                AND OB.ID_BENEFICIO IS NOT NULL  -- Evita la subconsulta en IN  
                                AND O.MONTO_APLICADO <> (  
                                    SELECT COALESCE(SUM(P.MONTO), 0)  
                                    FROM AFI_BENE_PAGO P  
                                    WHERE P.COD_BENEFICIO = O.COD_BENEFICIO  
                                    AND P.CONSEC = O.CONSEC  
                                )  -- Excluir registros donde la suma de pagos es igual al monto aplicado  
                            {where}  
                            ORDER BY O.REGISTRA_FECHA DESC  
                            {paginaActual} {paginacionActual}";

                    //          query = $@"select 
                    //          (SELECT CONCAT(RIGHT(CONCAT('00000', O.ID_BENEFICIO), 5), TRIM(O.COD_BENEFICIO) , RIGHT(CONCAT('00000',O.CONSEC), 5) )) AS Expediente, 
                    //          O.REGISTRA_FECHA, --Fecha solicitud / registro
                    //                              O.AUTORIZA_FECHA, --Fecha Aprovacion
                    //                              O.ID_BENEFICIO, -- Expediente
                    //                              O.CONSEC,   -- Expediente
                    //                              O.COD_BENEFICIO,   -- Expediente
                    //                              O.Beneficio_Desc, -- Beneficio
                    //                              O.MONTO, --Monto Aprobado
                    //                              O.MONTO_APLICADO, -- Monto Aplicado
                    //                              O.ESTADO, 
                    //                              ISNULL(E.DESCRIPCION, 'SIN DEFINIR') AS estado_desc, -- Estado
                    //                              O.cedula,  -- cedula
                    //                              O.NOMBRE_BENEFICIARIO, -- nombre completo 
                    //                              O.registra_user, --usuario
                    //                              Categoria_Desc, 
                    //                              Estado_Persona,
                    //                              O.TIPO,
                    //                              CASE 
                    //                                  WHEN O.TIPO = 'M' THEN 'Monetario'
                    //                                  WHEN O.TIPO = 'P' THEN 'Producto'
                    //                                  ELSE 'Ambos'
                    //                              END AS TipoDesc
                    //                              from vBeneficios_W_Integral O 
                    //                              LEFT JOIN AFI_BENE_ESTADOS E ON E.COD_ESTADO = O.ESTADO AND E.COD_ESTADO IN (
                    //                              SELECT COD_ESTADO FROM AFI_BENE_GRUPO_ESTADOS WHERE COD_GRUPO IN (
                    //                              SELECT COD_GRUPO FROM AFI_BENE_GRUPOS  Where COD_CATEGORIA like '%{Categoria}%'
                    //                              )) 
                    //                              WHERE O.COD_BENEFICIO IN (SELECT COD_BENEFICIO FROM AFI_BENEFICIOS Where COD_CATEGORIA like '%{Categoria}%') 
                    //                              AND E.P_FINALIZA = 1 AND E.PROCESO = 'A' {where} 
                    //                              AND CONCAT(O.CONSEC,O.COD_BENEFICIO) NOT IN (
                    //SELECT CONCAT(P.CONSEC, P.COD_BENEFICIO) FROM AFI_BENE_PAGO_PROYECTA P
                    //	WHERE COD_BENEFICIO = O.COD_BENEFICIO
                    //	AND CONSEC = O.CONSEC AND CEDULA = O.CEDULA
                    //) 
                    //                              AND ID_BENEFICIO IN (select ID_BENEFICIO from AFI_BENE_OTORGA where aplica_pago_masivo = 1) 
                    //                              Order by O.REGISTRA_FECHA desc 
                    //                              {paginaActual} {paginacionActual}";

                    response.Result.lista = connection.Query<BeneConsultaDatos>(query).ToList();

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
        /// Generar la solicitud de deposito para los beneficios seleccionados
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="lista"></param>
        /// <param name="mes"></param>
        /// <returns></returns>
        public ErrorDTO BeneSolicitudDeposito_Generar(int CodEmpresa, string lista, int mes)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var beneficios = JsonConvert.DeserializeObject<List<BeneficioGuadar>>(lista);
            ErrorDTO info = new ErrorDTO();
            info.Code = 0;
            try
            {
                var query = "";
                using var connection = new SqlConnection(clienteConnString);
                {
                    foreach (BeneficioGuadar beneficio in beneficios)
                    {
                        query = $@"select B.MONTO, B.COD_BENEFICIO, B.TIPO, B.CONSEC, B.REGISTRA_USER as registro_usuario, 
                            B.NOMBRE_BENEFICIARIO as t_beneficiario, S.AF_EMAIL as t_email, A.ID_BANCO as COD_BANCO, 
                            C.CUENTA_INTERNA as cta_bancaria, B.cedula as t_identificacion 
                            from vBeneficios_W_Integral  B   
                            LEFT JOIN SOCIOS S ON B.CEDULA = S.CEDULA 
                            LEFT JOIN SYS_CUENTAS_BANCARIAS C ON B.CEDULA = C.IDENTIFICACION 
							 LEFT JOIN BANCOS A ON A.COD_GRUPO = C.COD_BANCO 
                            where B.cedula = '{beneficio.cedula}' and B.ID_BENEFICIO = {beneficio.id_beneficio}";
                        var datosBeneficiario = connection.Query<AfiBenePagoProyecta>(query).FirstOrDefault();

                        if (datosBeneficiario == null)
                        {
                            info.Code = -1;
                            info.Description = "Beneficiario con la cédula " + beneficio.cedula + " no tiene una cuenta asociada, por favor verifique";
                            return info;
                        }

                        query = $@"select TOP 1 fecha_corte, monto from AFI_BENE_FECHA_PAGO_AUTOMATICO where COD_BENEFICIO = '{beneficio.cod_beneficio}' 
                            and PERIODO  = YEAR(GETDATE()) and ACTIVO = 1 and mes = {mes} ORDER BY FECHA_CORTE ASC";
                        var fechasCorte = connection.Query<(DateTime, int?)>(query).FirstOrDefault();

                        if (fechasCorte.Item2 == null)
                        {
                            info.Code = -1;
                            info.Description = "Beneficio " + beneficio.cod_beneficio + " no tiene fecha de pago activa para el mes indicado, por favor verifique";
                            return info;
                        }
                    }

                    foreach (BeneficioGuadar beneficio in beneficios)
                    {

                        query = $"select ACTIVA from SYS_CUENTAS_BANCARIAS where IDENTIFICACION = '{beneficio.cedula}'";
                        int cuentaActiva = connection.Query<int>(query).FirstOrDefault();

                        if (cuentaActiva == 1)
                        {
                            query = $@"select B.MONTO, B.COD_BENEFICIO, B.TIPO, B.CONSEC, B.REGISTRA_USER as registro_usuario, 
                            B.NOMBRE_BENEFICIARIO as t_beneficiario, S.AF_EMAIL as t_email, A.ID_BANCO as COD_BANCO, 
                            C.CUENTA_INTERNA as cta_bancaria, B.cedula as t_identificacion 
                            from vBeneficios_W_Integral  B   
                            LEFT JOIN SOCIOS S ON B.CEDULA = S.CEDULA 
                            LEFT JOIN SYS_CUENTAS_BANCARIAS C ON B.CEDULA = C.IDENTIFICACION 
							 LEFT JOIN BANCOS A ON A.COD_GRUPO = C.COD_BANCO 
                            where B.cedula = '{beneficio.cedula}' and B.ID_BENEFICIO = {beneficio.id_beneficio}";
                            var datosBeneficiario = connection.Query<AfiBenePagoProyecta>(query).FirstOrDefault();

                            query = $@"select TOP 1 fecha_corte, monto from AFI_BENE_FECHA_PAGO_AUTOMATICO where COD_BENEFICIO = '{beneficio.cod_beneficio}' 
                            and PERIODO  = YEAR(GETDATE()) and ACTIVO = 1 and mes = {mes} ORDER BY FECHA_CORTE ASC";
                            var fechasCorte = connection.Query<(DateTime, int?)>(query).FirstOrDefault();

                            query = $@"insert AFI_BENE_PAGO_PROYECTA(
                                            cedula,
                                            consec,
                                            cod_beneficio,
                                            tipo,
                                            fecha_vence,
                                            monto,
                                            cod_banco, 
                                            tipo_emision,
                                            cta_bancaria,
                                            estado,
                                            t_identificacion,
                                            t_beneficiario, 
                                            t_email,
                                            registro_fecha,
                                            registro_usuario
                            )values(
                                            '{beneficio.cedula.Trim()}',
                                            {beneficio.consec},
                                            '{beneficio.cod_beneficio.Trim()}',
                                            '{datosBeneficiario.tipo}',
                                            '{fechasCorte.Item1}',
                                            {fechasCorte.Item2},
                                            {datosBeneficiario.cod_banco},
                                            'TE',
                                            '{datosBeneficiario.cta_bancaria}',
                                            'P',
                                            '{datosBeneficiario.t_identificacion.Trim()}',
                                            '{datosBeneficiario.t_beneficiario}',
                                            '{datosBeneficiario.t_email}',
                                            Getdate(),
                                            '{beneficio.usuario}'
                            )";
                            connection.Execute(query);

                            //obtengo notas anteriores
                            query = @$"SELECT [NOTAS] FROM [dbo].[AFI_BENE_REGISTRO_ESTADOS]
                                   WHERE CONSEC = {beneficio.consec} AND [COD_BENEFICIO] = '{beneficio.cod_beneficio}' ";
                            string notas = connection.Query<string>(query).FirstOrDefault();

                            notas += ", " + beneficio.notas;

                            query = @$"UPDATE [dbo].[AFI_BENE_REGISTRO_ESTADOS]
                                   SET [NOTAS] = '{notas}'
                                   WHERE CONSEC = {beneficio.consec} AND [COD_BENEFICIO] = '{beneficio.cod_beneficio}' ";

                            connection.Execute(query);

                            _mBeneficiosDB.BitacoraBeneficios(new BitacoraBeneInsertarDTO
                            {
                                EmpresaId = CodEmpresa,
                                cod_beneficio = beneficio.cod_beneficio,
                                consec = beneficio.consec,
                                movimiento = "Inserta",
                                detalle = $@"Autoriza Solicitud de Deposito - Nota: [{beneficio.notas}]",
                                registro_usuario = beneficio.usuario
                            });
                        }
                    }
                    info.Description = "Solicitudes de Deposito Generadas Correctamente";
                }

            }
            catch (Exception ex)
            {
                info.Code = -1;
                info.Description = ex.Message;
            }
            return info;
        }

        /// <summary>
        /// Devolver la solicitud de deposito para los beneficios seleccionados
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="lista"></param>
        /// <returns></returns>
        public ErrorDTO BeneSolicitudDeposito_Devolver(int CodEmpresa, string lista)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var beneficios = JsonConvert.DeserializeObject<List<BeneficioGuadar>>(lista);
            ErrorDTO info = new ErrorDTO();
            info.Code = 0;
            try
            {
                var query = "";
                using var connection = new SqlConnection(clienteConnString);
                {
                    foreach (BeneficioGuadar beneficio in beneficios)
                    {
                        query = $@"update afi_bene_otorga set estado = 'DR' where id_beneficio = {beneficio.id_beneficio} ";
                        info.Code = connection.Execute(query);

                        //obtengo notas anteriores
                        query = @$"SELECT [NOTAS] FROM [dbo].[AFI_BENE_REGISTRO_ESTADOS]
                                   WHERE CONSEC = {beneficio.consec} AND COD_BENEFICIO = '{beneficio.cod_beneficio}' ";
                        string notas = connection.Query<string>(query).FirstOrDefault();

                        notas += ", " + beneficio.notas;

                        query = @$"UPDATE [dbo].[AFI_BENE_REGISTRO_ESTADOS]
                               SET 
                                   [COD_ESTADO]  =  'DR' 
                                   ,[NOTAS] = '{notas}'
                                   ,[REGISTRO_FECHA] = GETDATE()
                                   ,[REGISTRO_USUARIO] = '{beneficio.usuario}' 
                             WHERE CONSEC = {beneficio.consec} AND COD_BENEFICIO = '{beneficio.cod_beneficio}' ";
                        connection.Execute(query);

                        _mBeneficiosDB.BitacoraBeneficios(new BitacoraBeneInsertarDTO
                        {
                            EmpresaId = CodEmpresa,
                            cod_beneficio = beneficio.cod_beneficio,
                            consec = beneficio.consec,
                            movimiento = "Actualiza",
                            detalle = $@"Devolución Solicitud Pago - Nota: [{beneficio.notas}]",
                            registro_usuario = beneficio.usuario
                        });
                    }
                }

            }
            catch (Exception ex)
            {
                info.Code = -1;
                info.Description = ex.Message;
            }
            return info;
        }

        /// <summary>
        /// Obtengo la lista de beneficios para control mensual
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Categoria"></param>
        /// <param name="filtroString"></param>
        /// <returns></returns>
        public ErrorDTO<BeneConsultaDatosLista> BeneficiosControMensual_Reporte(int CodEmpresa, string Categoria, string filtroString)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var filtros = JsonConvert.DeserializeObject<AfiBeneFiltros>(filtroString);
            var response = new ErrorDTO<BeneConsultaDatosLista>
            {
                Result = new BeneConsultaDatosLista(),
                Code = 0
            };
            response.Result.total = 0;
            string where = "";
            try
            {
                if (filtros.cod_grupo != null && filtros.cod_grupo != "TODOS")
                {
                    where += $" AND O.COD_BENEFICIO IN (select COD_BENEFICIO from AFI_BENEFICIOS where COD_GRUPO = {filtros.cod_grupo}) ";
                }
                if (filtros.periodo != null)
                {
                    where += $" AND YEAR(P.REGISTRO_FECHA) = {filtros.periodo} ";
                }
                if (filtros.mes != null)
                {
                    where += $" AND MONTH(P.REGISTRO_FECHA) = {filtros.mes} ";
                }

                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"select 
                    (SELECT CONCAT(RIGHT(CONCAT('00000', O.ID_BENEFICIO), 5), TRIM(O.COD_BENEFICIO) , RIGHT(CONCAT('00000',O.CONSEC), 5) )) AS Expediente, 
                    P.cedula, 
                    O.Nombre_beneficiario, 
                    O.Beneficio_Desc, 
                    P.monto,
                    O.Categoria_Desc,
                    P.REGISTRO_USUARIO as registra_user, 
                    P.REGISTRO_FECHA as registra_fecha, 
                    P.ESTADO,
                    P.ID_PAGO,
                    P.COD_REMESA
                    from AFI_BENE_PAGO P LEFT JOIN 
                    vBeneficios_W_Integral O 
                    ON P.CEDULA = O.CEDULA AND P.COD_BENEFICIO = O.COD_BENEFICIO AND P.CONSEC = O.CONSEC 
                    WHERE O.COD_BENEFICIO IN (SELECT COD_BENEFICIO FROM AFI_BENEFICIOS Where COD_CATEGORIA like '%{Categoria}%') 
                    {where} 
                    Order by P.REGISTRO_FECHA asc";

                    response.Result.lista = connection.Query<BeneConsultaDatos>(query).ToList();

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
        /// Obtengo la lista de grupos de beneficios
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Categoria"></param>
        /// <returns></returns>
        public ErrorDTO<List<AfBeneficioIntegralDropsLista>> BeneficioGrupos_Obtener(int CodEmpresa, string Categoria)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDTO<List<AfBeneficioIntegralDropsLista>>();

            try
            {
                using var connection = new SqlConnection(clienteConnString);
                var query = $@"SELECT COD_GRUPO as item, DESCRIPCION FROM AFI_BENE_GRUPOS WHERE COD_CATEGORIA = '{Categoria}'";
                response.Result = connection.Query<AfBeneficioIntegralDropsLista>(query).ToList();
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = "BeneficioGrupos_Obtener: " + ex.Message;
                response.Result = null;
            }
            return response;
        }

        /// <summary>
        /// Obtengo los permisos del usuario para la categoria de beneficios
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="cod_categoria"></param>
        /// <returns></returns>
        public ErrorDTO<Bene_CategoriaPermisos> ValidaUsuarioBeneficios_Obtener(int CodEmpresa, string usuario, string cod_categoria)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDTO<Bene_CategoriaPermisos>();

            try
            {
                using var connection = new SqlConnection(clienteConnString);
                var query = $@"SELECT
                            [USUARIO]
                           ,[I_CAMBIAR_ESTADO]
                           ,[I_MODIFICA_EXPEDIENTE]
                           ,[I_TRASLADO_TESORERIA]
                           ,[I_PAGO_PROGRAMAR]
                           ,[I_PAGO_APROBAR_M]
                           ,[I_PAGO_REALIZAR]
                           ,[I_INGRESAR_SOLICITUD]
                           ,[I_PERIODO]
                           ,[I_PAGO_CONSULTA]
                           ,[I_APROBAR]
                           ,[I_RECHAZAR]
                           ,[I_ANULAR]
                           ,[I_DEVOLVER_RESOLUCION]
                      FROM AFI_BENE_GRUPOS_ROLES
                      WHERE USUARIO = '{usuario}' AND COD_CATEGORIA = '{cod_categoria}'";
                response.Result = connection.Query<Bene_CategoriaPermisos>(query).FirstOrDefault();

                if (response.Result == null)
                {
                    response.Result = new Bene_CategoriaPermisos();
                }

            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = "ValidaUsuarioBeneficios_Obtener: " + ex.Message;
                response.Result = new Bene_CategoriaPermisos();
            }
            return response;
        }

        /// <summary>
        /// Enviar la solicitud de bloqueo al departamento de cobros
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="parametros"></param>
        /// <returns></returns>
        public async Task<ErrorDTO> BeneSolicitudBloqueo_Enviar(int CodCliente, DocArchivoBeneIntegralDTO parametros)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            ErrorDTO info = new ErrorDTO();
            info.Code = 0;
            EnvioCorreoModels eConfig = new();
            string emailCobros = "";
            try
            {

                using var connection = new SqlConnection(clienteConnString);
                {
                    var queryCodBene = @$"SELECT C.COD_SMTP FROM AFI_BENE_CATEGORIAS C
                                            WHERE C.COD_CATEGORIA = '{parametros.cod_beneficio}' ";
                    string codCategoria = connection.Query<string>(queryCodBene).FirstOrDefault();

                    eConfig = _envioCorreoDB.CorreoConfig(CodCliente, codCategoria);

                    var queryEmailCobros = @$"select VALOR from SIF_PARAMETROS where COD_PARAMETRO = '{nofiticacionCobros}'";
                    emailCobros = connection.Query<string>(queryEmailCobros).FirstOrDefault();
                }


                string expediente = parametros.id_beneficio.ToString().PadLeft(5, '0') + parametros.cod_beneficio + parametros.consec.ToString().PadLeft(5, '0');

                string body = @$"<html lang=""es"">
                                    <head>
                                        <meta charset=""UTF-8"">
                                        <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
                                        <title>Boleta para aplicaci�n del beneficio al Departamento de Cobros</title>
                                    </head>
                                    <body>
                                        <p>{parametros.body}</p>
                                        <p>ASECCSS</p>
                                    </body>
                                    </html>";

                List<IFormFile> Attachments = new List<IFormFile>();

                if (parametros.filecontent != null)
                {
                    //Convierto de base64 a byte[]
                    byte[] fileContent = Convert.FromBase64String(parametros.filecontent);

                    var file = ConvertByteArrayToIFormFileList(fileContent, parametros.filename);

                    Attachments.AddRange(file);
                }


                if (sendEmail == "Y")
                {
                    EmailRequest emailRequest = new EmailRequest();

                    emailRequest.To = emailCobros;
                    emailRequest.From = eConfig.User;
                    emailRequest.Subject = "Solicitud de Bloqueo de Asociado";
                    emailRequest.Body = body;
                    emailRequest.Attachments = Attachments;

                    if (eConfig != null)
                    {
                        await _envioCorreoDB.SendEmailAsync(emailRequest, eConfig, info);
                    }

                }

                _mBeneficiosDB.BitacoraBeneficios(new BitacoraBeneInsertarDTO
                {
                    EmpresaId = CodCliente,
                    cod_beneficio = parametros.cod_beneficio,
                    consec = parametros.consec,
                    movimiento = "Notifica",
                    detalle = $@"Notificación Solicitud de Bloqueo Asociado {emailCobros}",
                    registro_usuario = parametros.usuario
                });

            }
            catch (Exception ex)
            {
                info.Code = -1;
                info.Description = ex.Message;
            }
            return info;
        }

        /// <summary>
        /// Convierte un arreglo de bytes en una lista de IFormFile
        /// </summary>
        /// <param name="byteArray"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        private List<IFormFile> ConvertByteArrayToIFormFileList(byte[] byteArray, string fileName)
        {
            var formFiles = new List<IFormFile>();

            if (byteArray == null || byteArray.Length == 0)
                return formFiles;

            // Crear un stream a partir del arreglo de bytes
            var stream = new MemoryStream(byteArray);

            // Crear una instancia de FormFile con el stream
            var formFile = new FormFile(stream, 0, byteArray.Length, "file", fileName)
            {
                Headers = new HeaderDictionary(),
                ContentType = "application/octet-stream" // Puedes especificar el tipo de contenido si lo conoces
            };

            // Agregar el FormFile a la lista
            formFiles.Add(formFile);

            return formFiles;
        }
    }
}