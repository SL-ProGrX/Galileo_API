using Dapper;
using Microsoft.CodeAnalysis;
using Microsoft.Data.SqlClient;
using PgxAPI.Models;
using PgxAPI.Models.CxP;
using PgxAPI.Models.ERROR;
using System.Data;

namespace PgxAPI.DataBaseTier
{
    public class frmCxPProveedoresDB
    {

        private readonly IConfiguration _config;
        private mProGrX_AuxiliarDB mAuxiliarDB;
        private readonly EnvioCorreoDB _envioCorreoDB;
        public string sendEmail = "";
        public string TestMail = "";
        public string Notificaciones = "";

        public frmCxPProveedoresDB(IConfiguration config)
        {
            _config = config;
            mAuxiliarDB = new mProGrX_AuxiliarDB(_config);
            _envioCorreoDB = new EnvioCorreoDB(_config);
            sendEmail = _config.GetSection("AppSettings").GetSection("EnviaEmail").Value.ToString();
            Notificaciones = _config.GetSection("AppSettings").GetSection("Notificaciones").Value.ToString();


        }

        /// <summary>
        /// Obtiene detalle Proveedor
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Cod_Proveedor"></param>
        /// <returns></returns>
        public ErrorDto<ProveedorDto> ProveedorDetalle_Obtener(int CodEmpresa, int Cod_Proveedor)
        {

            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            var response = new ErrorDto<ProveedorDto>
            {
                Code = 0
            };

            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"SELECT P.*,rtrim(C.descripcion) AS 'TipoProv',isnull(Cta.Descripcion,'') AS 'CuentaConta', dbo.fxSys_Cuenta_Bancos_Desc(P.cod_Banco) AS 'Banco_Desc' 
                                FROM cxp_proveedores P INNER JOIN cxp_prov_clas C ON P.cod_clasificacion = C.cod_clasificacion LEFT JOIN CntX_Cuentas Cta ON P.cod_Cuenta = Cta.cod_Cuenta and Cta.cod_contabilidad = 1 
                                WHERE P.cod_proveedor = {Cod_Proveedor}";

                    response.Result = connection.Query<ProveedorDto>(query).FirstOrDefault();

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
        /// Obtiene tipos Proveedor
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<TipoProveedor>> TiposProveedor_Obtener(int CodEmpresa)
        {

            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            var response = new ErrorDto<List<TipoProveedor>>
            {
                Code = 0
            };

            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = "SELECT cod_clasificacion ,descripcion FROM cxp_prov_clas ORDER BY cod_clasificacion";

                    response.Result = connection.Query<TipoProveedor>(query).ToList();

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
        /// Obtiene cuentas desembolso
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<CuentaDesembolso>> CuentasDesembolso_Obtener(int CodEmpresa)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<List<CuentaDesembolso>>
            {
                Code = 0
            };

            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var procedure = "[spCxP_Bancos_Autorizados]";
                    response.Result = connection.Query<CuentaDesembolso>(procedure, commandType: CommandType.StoredProcedure).ToList();
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
        /// Obtiene cuentas
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Identificacion"></param>
        /// <returns></returns>
        public ErrorDto<List<Cuenta>> Cuentas_Obtener(int CodEmpresa, string? Identificacion)
        {

            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            var response = new ErrorDto<List<Cuenta>>
            {
                Code = 0
            };

            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"SELECT RTRIM(B.Descripcion) AS 'Banco', CASE WHEN C.tipo = 'A' THEN 'Ahorros' ELSE 'Corriente' END AS 'Tipo_Desc'
                                ,C.cod_Divisa,C.CUENTA_INTERNA, C.CUENTA_INTERBANCA, C.ACTIVA, C.DESTINO, C.REGISTRO_FECHA , C.REGISTRO_USUARIO 
                                FROM SYS_CUENTAS_BANCARIAS C INNER JOIN TES_BANCOS_GRUPOS B ON C.cod_banco = B.cod_grupo WHERE C.Identificacion = '{Identificacion}'";

                    response.Result = connection.Query<Cuenta>(query).ToList();

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
        /// Actualiza el Proveedor
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto Proveedor_Actualizar(int CodEmpresa, ProveedorDto request)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            ErrorDto resp = new()
            {
                Code = 0
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {

                    //Estado Anterior
                    var queryEstado = $@"SELECT ESTADO FROM CXP_PROVEEDORES WHERE COD_PROVEEDOR = {request.Cod_Proveedor}";
                    var cod_estado = connection.Query<string>(queryEstado).FirstOrDefault();


                    int gestion = request.Web_Auto_Gestion == true ? 1 : 0;
                    int ferias = request.Web_Ferias == true ? 1 : 0;

                    var query = $@"UPDATE cxp_proveedores SET 
                                descripcion = '{request.Descripcion}'
                                ,cod_alter =  '{request.Cod_Alter}'
                                ,cedJur = '{request.Cedjur}'
                                ,tipo = '{request.Tipo}'
                                ,observacion = '{request.Observacion}'
                                ,estado = '{request.Estado}'
                                ,direccion = '{request.Direccion}'
                                ,aptopostal = '{request.Aptopostal}'
                                ,email = '{request.Email}'
                                ,telefono = '{request.Telefono}'
                                ,email_02 = '{request.Email_02}'
                                ,fax = '{request.Fax} '
                                ,contacto_compras = '{request.Contacto_Compras}'
                                ,contacto_ventas = '{request.Contacto_Ventas}'
                                ,cod_cuenta = '{request.Cod_Cuenta}'
                                ,descuento_porc = {request.Descuento_Porc}
                                ,credito_plazo = {request.Credito_Plazo}
                                ,credito_monto = {request.Credito_Monto}
                                ,cod_clasificacion = '{request.Cod_Clasificacion}'
                                ,nit_Codigo = '{request.Nit_Codigo}'
                                ,nit_nombre = '{request.Nit_Nombre}'
                                ,cod_divisa = '{request.Cod_Divisa}'
                                ,cod_Banco = {request.Cod_Banco}
                                ,web_auto_gestion = '{gestion}'
                                ,web_ferias = '{ferias}'
                                ,registro_fecha = '{request.registro_fecha}'
                                ,fecha_vencimiento = '{request.fecha_vencimiento}'
                                ,representante_legal = '{request.representante_legal}'
                                ,convenio = {request.convenio}
                                ,plazo = {request.plazo},
                                criticidad = '{request.criticidad}'
                                WHERE cod_proveedor = {request.Cod_Proveedor}";

                    resp.Code = connection.Query<int>(query).FirstOrDefault();
                    resp.Description = "Ok";


                    if (cod_estado != request.Estado)
                    {
                        mAuxiliarDB.BitacoraProveedor(new BitacoraProveedorInsertarDto
                        {
                            EmpresaId = CodEmpresa,
                            cod_proveedor = $@"{request.Cod_Proveedor.ToString()}",
                            consec = 0,
                            movimiento = "Inserta",
                            detalle = $@"{request.justificacion_estado}",
                            registro_usuario = request.user_modifica
                        });
                    }

                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }
            return resp;
        }

        /// <summary>
        /// Inserta el Proveedor
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto Proveedor_Insertar(int CodEmpresa, ProveedorDto request)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            ErrorDto resp = new()
            {
                Code = 0
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {

                    int siguiente = ObtenerSequencia(CodEmpresa);


                    var query = $@"INSERT INTO cxp_proveedores(cod_proveedor, tipo, cod_clasificacion, descripcion, cod_alter, observacion,
                       estado,contacto_ventas,contacto_compras,telefono,telefono_ext,fax,fax_ext,email,email_02,aptopostal,
                       direccion,credito_plazo,credito_monto,descuento_porc,saldo,cod_cuenta,
                       cedJur,Nit_Codigo,Nit_Nombre,cod_divisa,saldo_divisa_real,cod_banco, fecha_vencimiento, registro_fecha,plazo,convenio,representante_legal, criticidad) values({siguiente},'{request.Tipo}','{request.Cod_Clasificacion}','{request.Descripcion}'
                       ,'{request.Cod_Alter}','{request.Observacion}','{request.Estado}','{request.Contacto_Ventas}','{request.Contacto_Compras}','{request.Telefono}'
                       ,'{request.Telefono_Ext}','{request.Fax}','{request.Fax_Ext}', '{request.Email}','{request.Email_02}','{request.Aptopostal}','{request.Direccion}'
                       ,{request.Credito_Plazo},{request.Credito_Monto},{request.Descuento_Porc},{request.Saldo},'{request.Cod_Cuenta}','{request.Cedjur}'
                       ,'{request.Nit_Codigo}','{request.Nit_Nombre}','{request.Cod_Divisa}',{request.Saldo_Divisa_Real},{request.Cod_Banco}, '{request.fecha_vencimiento}','{request.registro_fecha}',
                         {request.plazo},{request.convenio}, '{request.representante_legal}', '{request.criticidad}')";


                    resp.Code = connection.Query<int>(query).FirstOrDefault();
                    resp.Description = "Ok";
                    resp.Code = siguiente;

                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }
            return resp;
        }

        /// <summary>
        /// Borra el Proveedor
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Cod_Proveedor"></param>
        /// <returns></returns>
        public ErrorDto Proveedor_Borrar(int CodEmpresa, int Cod_Proveedor)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            ErrorDto resp = new()
            {
                Code = 0
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {

                    var query = $@"delete cxp_proveedores where cod_proveedor = {Cod_Proveedor}";

                    resp.Code = connection.Query<int>(query).FirstOrDefault();
                    resp.Description = "Ok";

                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }
            return resp;
        }

        /// <summary>
        /// Obtiene Sencuencia
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public int ObtenerSequencia(int CodEmpresa)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            int result = 0;

            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"Select isnull(max(cast(cod_proveedor as int)),0) + 1 from cxp_proveedores";

                    result = connection.Query<int>(query).FirstOrDefault();

                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return result;
        }

        /// <summary>
        /// Consulta Ascendente o Descendente
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Cod_Proveedor"></param>
        /// <param name="tipo"></param>
        /// <param name="filtro"></param>
        /// <returns></returns>
        public int ConsultaAscDesc(int CodEmpresa, int Cod_Proveedor, string tipo, ProveedorDataFiltros filtro)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            int result = 0;

            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = "";
                    string valWhere = " ";
                    if (filtro != null)
                    {
                        valWhere = $"WHERE (ESTADO = 'A' OR ESTADO = 'T' OR ESTADO = 'S' OR ESTADO = 'I')";

                        if (filtro.autoGestion == true && filtro.ventas == true)
                        {
                            valWhere += " AND ( WEB_AUTO_GESTION = 1 OR WEB_FERIAS = 1 ) ";
                        }
                        else if (filtro.autoGestion == true)
                        {
                            valWhere += " AND WEB_AUTO_GESTION = 1 ";
                        }
                        else if (filtro.ventas == true)
                        {
                            valWhere += " AND WEB_FERIAS = 1 ";
                        }
                    }
                    else
                    {
                        valWhere = $" WHERE ESTADO = 'A' ";
                    }

                    if (tipo == "desc")
                    {
                        if (Cod_Proveedor == 0)
                        {
                            query = $@"select Top 1 cod_proveedor from cxp_proveedores {valWhere}
                                    order by cod_proveedor desc";
                        }
                        else
                        {
                            query = $@"select Top 1 cod_proveedor from cxp_proveedores
                                    {valWhere} AND cod_proveedor < {Cod_Proveedor} order by cod_proveedor desc";
                        }

                    }
                    else
                    {
                        query = $@"select Top 1 cod_proveedor from cxp_proveedores 
                                    {valWhere} AND cod_proveedor > {Cod_Proveedor} order by cod_proveedor asc";
                    }


                    result = connection.Query<int>(query).FirstOrDefault();

                    result = result == 0 || result == Cod_Proveedor ? Cod_Proveedor : result;

                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return result;
        }

        /// <summary>
        /// Valida cédula juridica
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Cod_Proveedor"></param>
        /// <param name="cedula"></param>
        /// <returns></returns>
        public int ValidaCedJuridica(int CodEmpresa, int Cod_Proveedor, string cedula)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            int result = 0;

            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"select isnull(count(*),0) from cxp_proveedores where cod_proveedor not in({Cod_Proveedor}) and cedJur = '{cedula}'";

                    result = connection.Query<int>(query).FirstOrDefault();

                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return result;
        }
    
        /// <summary>
        ///  Obtiene divisa de cuenta
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Cuenta"></param>
        /// <returns></returns>
        public ErrorDto<CuentaDivisa> ObtenerDivisaCuenta(int CodEmpresa, string Cuenta)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            var response = new ErrorDto<CuentaDivisa>
            {
                Code = 0
            };

            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"select cod_divisa from Cntx_Cuentas where cod_contabilidad = 1 and cod_cuenta = '{Cuenta}'";

                    response.Result = connection.Query<CuentaDivisa>(query).FirstOrDefault();

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
        /// Obtiene autorizaciones
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Cod_Proveedor"></param>
        /// <returns></returns>
        public ErrorDto<List<Autorizacion>> Autorizaciones_Obtener(int CodEmpresa, int Cod_Proveedor)
        {

            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            var response = new ErrorDto<List<Autorizacion>>
            {
                Code = 0
            };

            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"SELECT (cedula + ' - ' + CONVERT(VARCHAR(10),cod_proveedor) ) as dataKey ,cod_proveedor, cedula,nombre FROM cxp_autorizaciones WHERE cod_proveedor = {Cod_Proveedor} order by cedula";

                    response.Result = connection.Query<Autorizacion>(query).ToList();

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
        /// Actualiza autorizaciones
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto Autorizacion_Actualizar(int CodEmpresa, Autorizacion request)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            ErrorDto resp = new()
            {
                Code = 0
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {

                    var query = $@"UPDATE cxp_autorizaciones SET 
                                nombre = '{request.Nombre}'
                                WHERE cod_proveedor = {request.Cod_Proveedor}
                                AND cedula = '{request.Cedula}'";

                    resp.Code = connection.Query<int>(query).FirstOrDefault();
                    resp.Description = "Ok";

                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }
            return resp;
        }

        /// <summary>
        /// Inserta autorización
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto Autorizacion_Insertar(int CodEmpresa, Autorizacion request)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            ErrorDto resp = new()
            {
                Code = 0
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {

                    var query = $@"INSERT INTO cxp_autorizaciones(cod_proveedor, cedula, nombre)
                                values({request.Cod_Proveedor},'{request.Cedula}','{request.Nombre}')"
                            ;

                    resp.Code = connection.Query<int>(query).FirstOrDefault();
                    resp.Description = "Autorización agregada correctamente";

                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }
            return resp;
        }

        /// <summary>
        /// Borra autorización
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto Autorizacion_Borrar(int CodEmpresa, Autorizacion request)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            ErrorDto resp = new()
            {
                Code = 0
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"delete cxp_autorizaciones where cod_proveedor = {request.Cod_Proveedor} AND cedula = '{request.Cedula}'";


                    resp.Code = connection.Query<int>(query).FirstOrDefault();
                    resp.Description = "Autorización eliminada correctamente";
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }
            return resp;
        }

        /// <summary>
        /// Obtiene los tipo de suspensiones
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<TipoSuspension>> TipoSuspension_Obtener(int CodEmpresa)
        {

            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            var response = new ErrorDto<List<TipoSuspension>>
            {
                Code = 0
            };

            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"SELECT cod_suspension, descripcion FROM CXP_SUSPENSION_TIPOS";

                    response.Result = connection.Query<TipoSuspension>(query).ToList();

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
        /// Obtiene suspensiones
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Cod_Proveedor"></param>
        /// <param name="pagina"></param>
        /// <param name="paginacion"></param>
        /// <param name="filtro"></param>
        /// <returns></returns>
        public ErrorDto<SuspensionLista> Suspensiones_Obtener(int CodEmpresa, int Cod_Proveedor, int? pagina, int? paginacion, string? filtro)
        {

            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            var response = new ErrorDto<SuspensionLista>
            {
                Code = 0,
                Result = new SuspensionLista()
            };
            string paginaActual = " ", paginacionActual = " ";
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"SELECT COUNT(*) FROM vCxP_Suspensiones WHERE cod_proveedor = {Cod_Proveedor}";
                    response.Result.Total = connection.Query<int>(query).FirstOrDefault();

                    if (filtro != null)
                    {
                        filtro = " AND (NOTAS LIKE '%" + filtro + "%' OR suspension_Desc LIKE '%" + filtro + "%' OR registro_Usuario LIKE '%" + filtro + "%')";
                    }
                    if (pagina != null)
                    {
                        paginaActual = "order by VENCIMIENTO OFFSET " + pagina + " ROWS ";
                        paginacionActual = " FETCH NEXT " + paginacion + " ROWS ONLY ";
                    }

                    query = $@"SELECT * FROM vCxP_Suspensiones WHERE cod_proveedor = {Cod_Proveedor} {filtro} {paginaActual} {paginacionActual}";

                    response.Result.Suspensiones = connection.Query<Suspension>(query).ToList();

                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result.Suspensiones = null;
                response.Result.Total = 0;
            }
            return response;
        }

        /// <summary>
        /// Inserta o actualiza la suspensión
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto Suspencion_InsertUpdate(int CodEmpresa, Suspension request)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            ErrorDto resp = new()
            {
                Code = 0
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {

                    var procedure = "[spCxP_Suspension]";
                    var values = new
                    {
                        ProveedorId = request.Cod_Proveedor,
                        Codigo = request.Cod_Suspension,
                        Activa = request.Activa,
                        Notas = request.Activa == 1 ? request.Notas : request.Reactiva_Notas,
                        Vencimiento = request.Activa == 1 ? request.Vencimiento : null,
                        Usuario = request.Activa == 1 ? request.Registro_Usuario : request.Reactiva_Usuario,

                    };

                    resp.Code = connection.Query<int>(procedure, values, commandType: CommandType.StoredProcedure).FirstOrDefault();
                    resp.Description = "Suspensión procesada correctamente";
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }
            return resp;
        }

        /// <summary>
        /// Obtiene la lista de Proveedor fusion detalle
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Cod_Proveedor"></param>
        /// <returns></returns>
        public ErrorDto<ProveedorFusion> ProveedorFusion_ObtenerDetalle(int CodEmpresa, int Cod_Proveedor)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            var response = new ErrorDto<ProveedorFusion>();

            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"select P.cod_proveedor,P.descripcion
                                from cxp_fusiones F inner join cxp_proveedores P on F.cod_proveedor = P.cod_proveedor
                                inner join cxp_proveedores X On F.cod_proveedor_fus = X.cod_proveedor
                                Where F.cod_proveedor_fus = {Cod_Proveedor}";

                    response.Result = connection.Query<ProveedorFusion>(query).FirstOrDefault();

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
        /// Obtiene la lista de Proveedor fusion lista
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Cod_Proveedor"></param>
        /// <param name="pagina"></param>
        /// <param name="paginacion"></param>
        /// <param name="filtro"></param>
        /// <returns></returns>
        public ErrorDto<ProveedorFusionLista> ProveedorFusion_ObtenerLista(int CodEmpresa, int Cod_Proveedor, int? pagina, int? paginacion, string? filtro)
        {

            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            var response = new ErrorDto<ProveedorFusionLista>
            {
                Code = 0,
                Result = new ProveedorFusionLista()
            };

            string paginaActual = " ", paginacionActual = " ";
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"SELECT COUNT(*) FROM vCxP_Suspensiones WHERE cod_proveedor = {Cod_Proveedor}";
                    response.Result.Total = connection.Query<int>(query).FirstOrDefault();

                    if (filtro != null)
                    {
                        filtro = " AND (X.cod_proveedor LIKE '%" + filtro + "%' OR X.descripcion LIKE '%" + filtro + "%' OR X.fusion LIKE '%" + filtro + "%')";
                    }
                    if (pagina != null)
                    {
                        paginaActual = "order by F.cod_proveedor OFFSET " + pagina + " ROWS ";
                        paginacionActual = " FETCH NEXT " + paginacion + " ROWS ONLY ";
                    }
                    query = $@"select X.cod_proveedor,X.descripcion,X.fusion
                                from cxp_fusiones F inner join cxp_proveedores P on F.cod_proveedor = P.cod_proveedor
                                inner join cxp_proveedores X On F.cod_proveedor_fus = X.cod_proveedor
                                Where F.cod_proveedor = {Cod_Proveedor}
                                {filtro} {paginaActual} {paginacionActual}";

                    response.Result.Fusiones = connection.Query<ProveedorFusion>(query).ToList();

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
        /// Obtiene la lista de usuarios proveedor
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Cod_Proveedor"></param>
        /// <returns></returns>
        public ErrorDto<List<ProveedorUsuariosListaDatos>> ProveedorUsuariosLista_Obtener(int CodEmpresa, int Cod_Proveedor)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            var response = new ErrorDto<List<ProveedorUsuariosListaDatos>>
            {
                Code = 0
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {

                    var procedure = "[spCxP_Proveedores_Usuarios_List]";
                    var values = new
                    {
                        Proveedor = Cod_Proveedor,
                    };

                    response.Result = connection.Query<ProveedorUsuariosListaDatos>(procedure, values, commandType: CommandType.StoredProcedure).ToList();
                    response.Description = "Ok";
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
        ///  Obtiene la lista de eventos por proveedor
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Cod_Proveedor"></param>
        /// <returns></returns>
        public ErrorDto<List<ProveedorEventosListaDatos>> ProveedorEventosLista_Obtener(int CodEmpresa, int Cod_Proveedor)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            var response = new ErrorDto<List<ProveedorEventosListaDatos>>
            {
                Code = 0
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {

                    var procedure = "[spCxP_Proveedores_Eventos_List]";
                    var values = new
                    {
                        Proveedor = Cod_Proveedor,
                    };

                    response.Result = connection.Query<ProveedorEventosListaDatos>(procedure, values, commandType: CommandType.StoredProcedure).ToList();
                    response.Description = "Ok";
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
        /// Asigna proveedor a evento
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Cod_Proveedor"></param>
        /// <param name="Evento"></param>
        /// <param name="Activa"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto ProveedorEventos_Asigna(int CodEmpresa, int Cod_Proveedor,
            int Evento, bool Activa, string usuario)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            var response = new ErrorDto()
            {
                Code = 0
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {

                    var procedure = "[spCxP_Proveedores_Eventos_Asigna]";
                    var values = new
                    {
                        Proveedor = Cod_Proveedor,
                        Evento = Evento,
                        Activa = Activa,
                        Usuario = usuario
                    };

                    response.Code = connection.Query<int>(procedure, values, commandType: CommandType.StoredProcedure).FirstOrDefault();
                    response.Description = "Ok";
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
        /// Agrega usuario proveedor
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="datos"></param>
        /// <returns></returns>
        public ErrorDto CxPProveedoresUsuario_Agregar(int CodEmpresa, ProveedorUsuariosListaDatos datos)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            var response = new ErrorDto
            {
                Code = 0
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    int portal = datos.web_auto_gestion == true ? 1 : 0;
                    int ferias = datos.web_ferias == true ? 1 : 0;
                    int activo = datos.activo == true ? 1 : 0;

                    var procedure = "[spCxP_Proveedores_Usuario_Add]";
                    var values = new
                    {
                        Proveedor = datos.cod_proveedor,
                        Usuario = datos.usuario,
                        Nombre = datos.nombre,
                        Email = datos.email,
                        Portal = portal,
                        Ferias = ferias,
                        Activo = activo,
                        Registro_Usuario = datos.registro_usuario

                    };

                    response.Code = connection.Query<int>(procedure, values, commandType: CommandType.StoredProcedure).FirstOrDefault();
                    response.Description = "Ok";
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
        ///  Obtiene bitacora de producto
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="cod_proveedor"></param>
        /// <returns></returns>
        public ErrorDto<List<BitacoraProveedorDto>> BitacoraProducto_Obtener(int CodEmpresa, int cod_proveedor)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<List<BitacoraProveedorDto>>();
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {

                    var query = $@"SELECT ID_BITACORA, CONSEC, REGISTRO_FECHA, COD_PROVEEDOR,REGISTRO_USUARIO, DETALLE, MOVIMIENTO
                  FROM BITACORA_PROVEEDOR WHERE cod_proveedor = '{cod_proveedor}' ORDER BY 1 ASC";

                    response.Result = connection.Query<BitacoraProveedorDto>(query).ToList();

                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = "BitacoraProveedor_Obtener: " + ex.Message;
                response.Result = null;
            }
            return response;
        }

        /// <summary>
        /// Envia la notificación de vencimiento al Proveedor
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<ProveedorDto>> Proveedor_NotificacionVencimiento(int CodEmpresa)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<List<ProveedorDto>>();
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var querydias = $@"SELECT valor FROM SIF_PARAMETROS where COD_PARAMETRO = 'VCXP'";
                    int dias = connection.QueryFirstOrDefault<int>(querydias);

                    var query = $@"SELECT * FROM cxp_proveedores WHERE fecha_vencimiento BETWEEN GETDATE() AND DATEADD(DAY, {dias}, GETDATE());";
                    response.Result = connection.Query<ProveedorDto>(query).ToList();

                    CorreoNotificacionVencimiento_Enviar(CodEmpresa, response.Result, dias);

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
        /// Generación de correo electrónico por vencimiento de proveedor
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="proveedores"></param>
        /// <param name="dias"></param>
        /// <returns></returns>
        public async Task<ErrorDto> CorreoNotificacionVencimiento_Enviar(int CodCliente, List<ProveedorDto> proveedores, int dias)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            ErrorDto info = new ErrorDto();
            info.Code = 0;
            EnvioCorreoModels eConfig = new();
            string emailCobros = "";

            foreach (var proveedor in proveedores)
            {
                try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    eConfig = _envioCorreoDB.CorreoConfig(CodCliente, Notificaciones);

                    string body = @$"<html lang=""es"">
                            <head>
                                <meta charset=""UTF-8"">
                                <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
                                <title>Solicitud de Cotización</title>
                                <style>
                                    body {{
                                        font-family: Arial, sans-serif;
                                    }}
                                    .container {{
                                        width: 600px;
                                        margin: 0 auto;
                                        border: 1px solid #eaeaea;
                                        padding: 20px;
                                    }}
                                    .header {{
                                        background-color: #e8f3ff;
                                        padding: 10px;
                                    }}
                                    .header img {{
                                        width: auto;
                                        height: 50px;
                                    }}
                                    .content {{
                                        margin-top: 20px;
                                    }}
                                    .content h2 {{
                                        font-size: 16px;
                                        color: #0072ce;
                                    }}
                                    .table {{
                                        width: 100%;
                                        margin-top: 20px;
                                        border-collapse: collapse;
                                    }}
                                    .table th, .table td {{
                                        padding: 10px;
                                        border: 1px solid #dcdcdc;
                                        text-align: left;
                                    }}
                                    .table th {{
                                        background-color: #0072ce;
                                        color: white;
                                    }}
                                    
                                </style>
                            </head>
                            <body>
                                <div class=""container"">
                                    <div class=""header"">
                                        <img src=""https://www.aseccssenlinea.com/Content/Login/ASECCSSLogo.png"" alt=""Logo"">
                                    </div>
                                    <div class=""content"">
                                        <h2><strong>Notificación de vencimiento de registro</strong> </h2>
                                        <p>Estimado Proveedor <strong>{proveedor.Descripcion}</strong> </p>
                                        <p>Mediante la presente se le comunica el vencimiento de su registro</p>";

                    List<IFormFile> Attachments = new List<IFormFile>();


                    if (sendEmail == "Y")
                    {
                        EmailRequest emailRequest = new EmailRequest();

                        emailRequest.To = proveedor.Email;
                        emailRequest.From = eConfig.User;
                        emailRequest.Subject = "Notificación de vencimiento de registro";
                        emailRequest.Body = body;
                        //emailRequest.Attachments = Attachments;

                        if (eConfig != null)
                        {
                            await _envioCorreoDB.SendEmailAsync(emailRequest, eConfig, info);
                        }

                    }

                }
            }
            catch (Exception ex)
            {
                info.Code = -1;
                info.Description = ex.Message;
            }
           
        }
            return info;
        }

        #region NO SÉ

        /// <summary>
        /// Obtiene estado del proveedor
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="CodProveedor"></param>
        /// <returns></returns>
        public ErrorDto ProveedorEstado_Obtener(int CodCliente, int CodProveedor)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);

            ErrorDto info = new ErrorDto
            {
                Code = 0
            };

            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"SELECT ESTADO FROM CXP_PROVEEDORES WHERE COD_PROVEEDOR = {CodProveedor}";
                    info.Description = connection.Query<string>(query).FirstOrDefault();
                    info.Code = 0;
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
        /// Obtener proveedores
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <returns></returns>
        public ErrorDto<List<ProveedorDto>> ObtenerProveedores(int CodCliente)
        {

            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);

            var response = new ErrorDto<List<ProveedorDto>>();

            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = "SELECT COD_PROVEEDOR, DESCRIPCION FROM CXP_PROVEEDORES WHERE ESTADO = 'A' ORDER BY COD_PROVEEDOR";

                    response.Result = connection.Query<ProveedorDto>(query).ToList();

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

        #endregion

    }
}
