using Dapper;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using PgxAPI.Models;
using PgxAPI.Models.CPR;
using PgxAPI.Models.ERROR;

namespace PgxAPI.DataBaseTier
{
    public class frmCpr_ContratosDB
    {
        private readonly IConfiguration? _config;
        private readonly EnvioCorreoDB _envioCorreoDB;
        public string sendEmail = "";
        public string nofiticacionConfeccionContrato = "";
        public string codNotificaciones = "";

        public frmCpr_ContratosDB(IConfiguration config)
        {
            _config = config;
            _envioCorreoDB = new EnvioCorreoDB(_config);
            sendEmail = _config.GetSection("AppSettings").GetSection("EnviaEmail").Value.ToString();
            nofiticacionConfeccionContrato = _config.GetSection("Crp_Compras").GetSection("NotiConfeccionContrato").Value.ToString();
            codNotificaciones = _config.GetSection("AppSettings").GetSection("Notificaciones").Value.ToString();
        }

        /// <summary>
        /// Obtiene un contrato mediante el código de contrato.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="cod_contrato"></param>
        /// <returns></returns>
        public ErrorDto<CprContratosDTO> CprContrato_Obtener(int CodEmpresa, string cod_contrato)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<CprContratosDTO>
            {
                Code = 0
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"select C.*, P.DESCRIPCION as PROVEEDOR from CPR_CONTRATOS C INNER JOIN CXP_PROVEEDORES P 
                    ON C.COD_PROVEEDOR = P.COD_PROVEEDOR WHERE C.COD_CONTRATO = '{cod_contrato}'";
                    response.Result = connection.Query<CprContratosDTO>(query).FirstOrDefault();
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
        /// Obtiene la lista de contratos.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDto<CprContratosLista> CprContratosLista_Obtener(int CodEmpresa, string filtros)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            CprContratosFiltros filtro = JsonConvert.DeserializeObject<CprContratosFiltros>(filtros) ?? new CprContratosFiltros();
            var response = new ErrorDto<CprContratosLista>
            {
                Code = 0,
                Result = new CprContratosLista
                {
                    total = 0
                }
            };

            try
            {
                var query = "";
                string where = " ", paginaActual = " ", paginacionActual = " ";
                if (filtro.filtro != null)
                {
                    where = "WHERE ( C.cod_contrato LIKE '%" + filtro.filtro + "%' OR C.descripcion LIKE '%" + filtro.filtro + "%' OR P.DESCRIPCION LIKE '%" + filtro.filtro + "%' ) ";
                }

                if (filtro.pagina != null)
                {
                    paginaActual = " OFFSET " + filtro.pagina + " ROWS ";
                    paginacionActual = " FETCH NEXT " + filtro.paginacion + " ROWS ONLY ";
                }

                using var connection = new SqlConnection(stringConn);
                {
                    query = $"select COUNT(*) from CPR_CONTRATOS C INNER JOIN CXP_PROVEEDORES P ON C.COD_PROVEEDOR = P.COD_PROVEEDOR {where}";
                    response.Result.total = connection.Query<int>(query).First();

                    query = $"select C.*, P.DESCRIPCION as PROVEEDOR from CPR_CONTRATOS C INNER JOIN CXP_PROVEEDORES P ON C.COD_PROVEEDOR = P.COD_PROVEEDOR {where} order by cod_contrato desc {paginaActual} {paginacionActual}";
                    response.Result.contratos = connection.Query<CprContratosDTO>(query).ToList();

                    foreach (var item in response.Result.contratos)
                    {
                        query = $@"SELECT c.ESTADO
                        FROM CPR_CONTRATOS_ESTADOS c
                        JOIN (
                            SELECT COD_CONTRATO, MAX(FECHA_INICIO) AS FECHA_INICIO_VIGENTE
                            FROM CPR_CONTRATOS_ESTADOS
                            WHERE FECHA_INICIO <= GETDATE()
                            GROUP BY COD_CONTRATO
                        ) vigente
                        ON c.COD_CONTRATO = vigente.COD_CONTRATO
                        AND c.FECHA_INICIO = vigente.FECHA_INICIO_VIGENTE
                        where c.COD_CONTRATO = '{item.cod_contrato}';";
                        item.estado = connection.Query<string>(query).FirstOrDefault();
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
        /// Inserta un contrato.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="contrato"></param>
        /// <returns></returns>
        public ErrorDto CprContrato_Insertar(int CodEmpresa, CprContratosDTO contrato)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto
            {
                Code = 0
            };
            string columnsExtra = "";
            string valuesExtra = "";
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = @$"select COUNT(*) from CPR_CONTRATOS where COD_CONTRATO = '{contrato.cod_contrato.Trim()}'";
                    var existe = connection.Query<int>(query).First();
                    if (existe > 0)
                    {
                        response.Code = -1;
                        response.Description = "Ya existe el registro de un contrato con el código: " + contrato.cod_contrato;
                        return response;
                    }

                    if (contrato.fecha_inicio != null)
                    {
                        columnsExtra += ", FECHA_INICIO";
                        valuesExtra += @$", '{contrato.fecha_inicio}'";
                    }
                    if (contrato.fecha_corte != null)
                    {
                        columnsExtra += ", FECHA_CORTE";
                        valuesExtra += @$", '{contrato.fecha_corte}'";
                    }
                    if (contrato.plazo != null)
                    {
                        columnsExtra += ", PLAZO, CANTIDAD_PLAZO";
                        valuesExtra += @$", {contrato.plazo}, '{contrato.cantidad_plazo}'";
                    }
                    if (contrato.periodo_garantia != null)
                    {
                        columnsExtra += ", PERIODO_GARANTIA";
                        valuesExtra += @$", {contrato.periodo_garantia}";
                    }
                    if (contrato.fecha_vencimiento != null)
                    {
                        columnsExtra += ", FECHA_VENCIMIENTO";
                        valuesExtra += @$", '{contrato.fecha_vencimiento}'";
                    }

                    query = $@"INSERT INTO CPR_CONTRATOS (
                        COD_CONTRATO, 
                        DESCRIPCION, 
                        COD_PROVEEDOR, 
                        TIPO_CONTRATO,  
                        REGISTRO_FECHA, 
                        REGISTRO_USUARIO,
                        MONTO, 
                        CTA_CONTABLE, 
                        NOTAS,
                        DIVISA,
                        COD_CENTRO_COSTO,
                        FISCAL,
                        PORCENTAJE_GARANTIA,
                        MONTO_GARANTIA,
                        DIVISA_GARANTIA
                        {columnsExtra}
                    ) 
                    VALUES (
                        '{contrato.cod_contrato}',
                        '{contrato.descripcion}',
                        {contrato.cod_proveedor},
                        '{contrato.tipo_contrato}', 
                        getDate(),  
                        '{contrato.registro_usuario}',
                        {contrato.monto},
                        '{contrato.cta_contable}', 
                        '{contrato.notas}',
                        '{contrato.divisa}',
                        '{contrato.cod_centro_costo}',
                        '{contrato.fiscal}',
                        {contrato.porcentaje_garantia},
                        {contrato.monto_garantia},
                        '{contrato.divisa_garantia}'
                        {valuesExtra}
                    );
                    ";
                    connection.Query(query);

                    var query2 = $@"INSERT INTO CPR_CONTRATOS_ESTADOS (
                            COD_CONTRATO, 
                            ESTADO, 
                            FECHA_INICIO, 
                            NOTAS, 
                            REGISTRO_FECHA, 
                            REGISTRO_USUARIO
                        ) VALUES (
                            '{contrato.cod_contrato}',
                            'B',     
                            getDate(),
                            'Se crea borrador',   
                            getDate(),  
                            '{contrato.registro_usuario}'
                        );";
                    connection.Query(query2);

                    BitacoraContratos( CodEmpresa,
                        new CprContratosBitacoraDTO
                    {
                        cod_contrato = contrato.cod_contrato,
                        movimiento = "Inserta",
                        detalle = $@"Ingresa Contrato",
                        registro_usuario = contrato.registro_usuario
                    });

                    response.Description = "Contrato agregado correctamente";
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
        /// Actualiza información del contrato.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="contrato"></param>
        /// <returns></returns>
        public ErrorDto CprContrato_Actualizar(int CodEmpresa, CprContratosDTO contrato)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto
            {
                Code = 0
            };
            string valuesExtra = "";
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    if (contrato.fecha_inicio != null)
                    {
                        valuesExtra += @$", FECHA_INICIO = '{contrato.fecha_inicio}'";
                    }
                    if (contrato.fecha_corte != null)
                    {
                        valuesExtra += @$", FECHA_CORTE = '{contrato.fecha_corte}'";
                    }
                    if (contrato.plazo != null)
                    {
                        valuesExtra += @$", PLAZO = {contrato.plazo} 
                        , CANTIDAD_PLAZO = '{contrato.cantidad_plazo}'";
                    }
                    if (contrato.periodo_garantia != null)
                    {
                        valuesExtra += @$", PERIODO_GARANTIA = {contrato.periodo_garantia}";
                    }
                    if (contrato.fecha_vencimiento != null)
                    {
                        valuesExtra += @$", FECHA_VENCIMIENTO = '{contrato.fecha_vencimiento}'";
                    }

                    var query = $@"UPDATE CPR_CONTRATOS
                    SET 
                        DESCRIPCION = '{contrato.descripcion}',
                        COD_PROVEEDOR = {contrato.cod_proveedor},  
                        TIPO_CONTRATO = '{contrato.tipo_contrato}',  
                        MODIFICA_FECHA = getDate(),
                        MODIFICA_USUARIO = '{contrato.modifica_usuario}' ,
                        MONTO = {contrato.monto},
                        CTA_CONTABLE = '{contrato.cta_contable}',
                        NOTAS = '{contrato.notas}',
                        DIVISA = '{contrato.divisa}',
                        COD_CENTRO_COSTO = '{contrato.cod_centro_costo}',
                        FISCAL = '{contrato.fiscal}',
                        PORCENTAJE_GARANTIA = {contrato.porcentaje_garantia},
                        MONTO_GARANTIA = {contrato.monto_garantia},
                        DIVISA_GARANTIA = '{contrato.divisa_garantia}'
                        {valuesExtra}
                    WHERE 
                        COD_CONTRATO = '{contrato.cod_contrato}';
                    ";
                    connection.Query(query);

                    BitacoraContratos( CodEmpresa,
                        new CprContratosBitacoraDTO
                    {
                        cod_contrato = contrato.cod_contrato,
                        movimiento = "Actualiza",
                        detalle = $@"Modifica datos de Contrato",
                        registro_usuario = contrato.registro_usuario
                    });

                    response.Description = "Contrato actualizado correctamente";
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
        /// Elimina información del contrato.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="cod_contrato"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto CprContrato_Eliminar(int CodEmpresa, string cod_contrato, string usuario)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            ErrorDto response = new()
            {
                Code = 0
            };
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"delete from CPR_CONTRATOS WHERE COD_CONTRATO = '{cod_contrato}'";

                    response.Code = connection.Execute(query);

                    BitacoraContratos( CodEmpresa,
                        new CprContratosBitacoraDTO
                    {
                        cod_contrato = cod_contrato,
                        movimiento = "Elimina",
                        detalle = $@"Elimina Contrato",
                        registro_usuario = usuario
                    });
                    response.Description = "Contrato eliminado correctamente";
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
        /// Obtiene adendums de un contrato. 
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="cod_contrato"></param>
        /// <returns></returns>
        public ErrorDto<List<CprContratosAdendumsDTO>> CprContrato_Adendums_Obtener(int CodEmpresa, string cod_contrato)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<List<CprContratosAdendumsDTO>>
            {
                Code = 0
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"select * from CPR_CONTRATOS_ADENDUMS WHERE COD_CONTRATO = '{cod_contrato}'";
                    response.Result = connection.Query<CprContratosAdendumsDTO>(query).ToList();
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
        /// Guarda información de un adendum, si el contrato ya tiene un registro del contrato madre relacionado entonces actualiza las notas, 
        /// sino agrega un nuevo registro con el contrato madre relacionado.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="adendum"></param>
        /// <returns></returns>
        public ErrorDto CprContrato_Adendum_Guardar(int CodEmpresa, CprContratosAdendumsDTO adendum)
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
                    var query = @$"select COUNT(*) from CPR_CONTRATOS_ADENDUMS where COD_CONTRATO = '{adendum.cod_contrato}' and COD_CONTRATO_MADRE = '{adendum.cod_contrato_madre}'";
                    var existe = connection.Query<int>(query).First();

                    if (existe > 0)
                    {
                        query = $@"UPDATE CPR_CONTRATOS_ADENDUMS 
                        SET
                            NOTAS = '{adendum.notas}' 
                        WHERE 
                            COD_CONTRATO = '{adendum.cod_contrato}' and  COD_CONTRATO_MADRE = '{adendum.cod_contrato_madre}'";

                        BitacoraContratos(CodEmpresa,
                        new CprContratosBitacoraDTO
                        {
                            cod_contrato = adendum.cod_contrato,
                            movimiento = "Actualiza",
                            detalle = $@"Modifica Adendum con Contrato " + adendum.cod_contrato_madre,
                            registro_usuario = adendum.registro_usuario
                        });
                    }
                    else
                    {
                        query = $@"INSERT INTO CPR_CONTRATOS_ADENDUMS (
                            COD_CONTRATO, 
                            COD_CONTRATO_MADRE, 
                            NOTAS, 
                            REGISTRO_FECHA, 
                            REGISTRO_USUARIO
                        ) VALUES (
                            '{adendum.cod_contrato}',
                            '{adendum.cod_contrato_madre}',     
                            '{adendum.notas}',   
                            getDate(),  
                            '{adendum.registro_usuario}'
                        );";

                        BitacoraContratos(CodEmpresa,
                        new CprContratosBitacoraDTO
                        {
                            cod_contrato = adendum.cod_contrato,
                            movimiento = "Insertar",
                            detalle = $@"Ingresa Adendum con Contrato " + adendum.cod_contrato_madre,
                            registro_usuario = adendum.registro_usuario
                        });
                    }
                    connection.Query(query); 

                    response.Description = "Adendum guardado correctamente";
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
        /// Elimina el adendum mediante el id_adendum
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="id_adendum"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto CprContrato_Adendum_Eliminar(int CodEmpresa, int id_adendum, string usuario)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            ErrorDto response = new()
            {
                Code = 0
            };
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"select COD_CONTRATO from CPR_CONTRATOS_ADENDUMS WHERE ID_ADDENDUM = {id_adendum}";
                    string cod_contrato = connection.Query<string>(query).First();
                    query = $@"select COD_CONTRATO_MADRE from CPR_CONTRATOS_ADENDUMS WHERE ID_ADDENDUM = {id_adendum}";
                    string cod_contrato_madre = connection.Query<string>(query).First();

                    query = $@"delete from CPR_CONTRATOS_ADENDUMS WHERE ID_ADDENDUM = {id_adendum}";
                    response.Code = connection.Execute(query);

                    BitacoraContratos( CodEmpresa,
                        new CprContratosBitacoraDTO
                    {
                        cod_contrato = cod_contrato,
                        movimiento = "Elimina",
                        detalle = $@"Elimina de Adendum con Contrato "+cod_contrato_madre,
                        registro_usuario = usuario
                    });
                    response.Description = "Adendum eliminado correctamente";
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
        /// Obtiene la información de los estado mediante el código de contrato
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="cod_contrato"></param>
        /// <returns></returns>
        public ErrorDto<List<CprContratosEstadosDTO>> CprContrato_Estados_Obtener(int CodEmpresa, string cod_contrato)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<List<CprContratosEstadosDTO>>
            {
                Code = 0
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"select * from CPR_CONTRATOS_ESTADOS WHERE COD_CONTRATO = '{cod_contrato}'";
                    response.Result = connection.Query<CprContratosEstadosDTO>(query).ToList();
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
        /// Guarda información del estado, si el contrato ya tiene un registro de ese estado entonces actualiza la información de este, sino agrega un nuevo registro.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="estado"></param>
        /// <returns></returns>
        public ErrorDto CprContrato_Estados_Guardar(int CodEmpresa, CprContratosEstadosDTO estado)
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
                    var query = @$"select COUNT(*) from CPR_CONTRATOS_ESTADOS where COD_CONTRATO = '{estado.cod_contrato}' and ESTADO = '{estado.estado}'";
                    var existe = connection.Query<int>(query).First();

                    if (existe > 0)
                    {
                        query = $@"UPDATE CPR_CONTRATOS_ESTADOS 
                        SET
                            FECHA_INICIO = '{estado.fecha_inicio}', 
                            NOTAS = '{estado.notas}',  
                            REGISTRO_FECHA = getDate(), 
                            REGISTRO_USUARIO = '{estado.registro_usuario}'
                        WHERE 
                            COD_CONTRATO = '{estado.cod_contrato}' and ESTADO = '{estado.estado}'";

                        BitacoraContratos(CodEmpresa,
                        new CprContratosBitacoraDTO
                        {
                            cod_contrato = estado.cod_contrato,
                            movimiento = "Actualiza",
                            detalle = $@"Modifica datos de estado: " + EstadoDescripcion_Obtener(CodEmpresa, estado.estado),
                            registro_usuario = estado.registro_usuario
                        });
                    } 
                    else 
                    { 
                        query = $@"INSERT INTO CPR_CONTRATOS_ESTADOS (
                            COD_CONTRATO, 
                            ESTADO, 
                            FECHA_INICIO, 
                            NOTAS, 
                            REGISTRO_FECHA, 
                            REGISTRO_USUARIO
                        ) VALUES (
                            '{estado.cod_contrato}',
                            '{estado.estado}',     
                            '{estado.fecha_inicio}',
                            '{estado.notas}',   
                            getDate(),  
                            '{estado.registro_usuario}'
                        );";

                        BitacoraContratos(CodEmpresa,
                        new CprContratosBitacoraDTO
                        {
                            cod_contrato = estado.cod_contrato,
                            movimiento = "Inserta",
                            detalle = $@"Agrega estado: " + EstadoDescripcion_Obtener(CodEmpresa, estado.estado),
                            registro_usuario = estado.registro_usuario
                        });
                    }
                    connection.Query(query);
                    response.Description = "Estado guardado correctamente";
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
        /// Elimina el estado del contrato mediante la linea_id
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="linea_id"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto CprContrato_Estados_Eliminar(int CodEmpresa, int linea_id, string usuario)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            ErrorDto response = new()
            {
                Code = 0
            };
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {

                    var query = $@"select COD_CONTRATO from CPR_CONTRATOS_ESTADOS WHERE LINEA_ID = {linea_id}";
                    string cod_contrato = connection.Query<string>(query).First();
                    query = $@"select ESTADO from CPR_CONTRATOS_ESTADOS WHERE LINEA_ID = {linea_id}";
                    string estado = connection.Query<string>(query).First();
                    
                    query = $@"delete from CPR_CONTRATOS_ESTADOS WHERE LINEA_ID = {linea_id}";

                    response.Code = connection.Execute(query);

                    BitacoraContratos(CodEmpresa,
                        new CprContratosBitacoraDTO
                    {
                        cod_contrato = cod_contrato,
                        movimiento = "Elimina",
                        detalle = $@"Elimina estado: " + EstadoDescripcion_Obtener(CodEmpresa, estado),
                        registro_usuario = usuario
                    });

                    response.Description = "Estado eliminado correctamente";
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
        /// Obtiene información de los productos relacionados al contrato
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="cod_contrato"></param>
        /// <returns></returns>
        public ErrorDto<List<CprContratosProductosDTO>> CprContrato_Productos_Obtener(int CodEmpresa, string cod_contrato)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<List<CprContratosProductosDTO>>
            {
                Code = 0
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"select C.*, P.DESCRIPCION
                        from CPR_CONTRATOS_PRODUCTOS C LEFT JOIN PV_PRODUCTOS P ON C.COD_PRODUCTO = P.COD_PRODUCTO 
                        WHERE C.COD_CONTRATO = '{cod_contrato}'";
                    response.Result = connection.Query<CprContratosProductosDTO>(query).ToList();
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
        /// Guarda información del producto agregado, si esta ya existe entonces devuelve un mensaje.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="producto"></param>
        /// <returns></returns>
        public ErrorDto CprContrato_Producto_Guardar(int CodEmpresa, CprContratosProductosDTO producto)
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
                    var query = @$"select COUNT(*) from CPR_CONTRATOS_PRODUCTOS where COD_CONTRATO = '{producto.cod_contrato}' and COD_PRODUCTO = '{producto.cod_producto}'";
                    var existe = connection.Query<int>(query).First();

                    if (existe > 0)
                    {
                        response.Code = -1;
                        response.Description = "El producto código "+ producto.cod_producto+" ya se encontraba agregado";
                        return response;
                    }
                    else
                    {
                        query = $@"INSERT INTO CPR_CONTRATOS_PRODUCTOS (
                            COD_CONTRATO, 
                            COD_PRODUCTO, 
                            REGISTRO_FECHA, 
                            REGISTRO_USUARIO
                        ) VALUES (
                            '{producto.cod_contrato}',
                            '{producto.cod_producto}',      
                            getDate(),  
                            '{producto.registro_usuario}'
                        );";
                    }
                    connection.Query(query);

                    BitacoraContratos(CodEmpresa,
                        new CprContratosBitacoraDTO
                    {
                        cod_contrato = producto.cod_contrato,
                        movimiento = "Inserta",
                        detalle = $@"Agrega producto Cod. " + producto.cod_producto,
                        registro_usuario = producto.registro_usuario
                    });

                    response.Description = "Producto agregado correctamente";
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
        /// Elimina el producto relacionado al contrato mediante la linea_id.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="linea_id"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto CprContrato_Producto_Eliminar(int CodEmpresa, int linea_id, string usuario)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            ErrorDto response = new()
            {
                Code = 0
            };
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"select COD_CONTRATO from CPR_CONTRATOS_PRODUCTOS WHERE LINEA_ID = {linea_id}";
                    string cod_contrato = connection.Query<string>(query).First();
                    query = $@"select COD_PRODUCTO from CPR_CONTRATOS_PRODUCTOS WHERE LINEA_ID = {linea_id}";
                    string cod_producto = connection.Query<string>(query).First();

                    query = $@"delete from CPR_CONTRATOS_PRODUCTOS WHERE LINEA_ID = {linea_id}";

                    response.Code = connection.Execute(query);

                    BitacoraContratos(CodEmpresa,
                        new CprContratosBitacoraDTO
                    {
                        cod_contrato = cod_contrato,
                        movimiento = "Elimina",
                        detalle = $@"Elimina producto Cod. " + cod_producto,
                        registro_usuario = usuario
                    });

                    response.Description = "Linea eliminada correctamente";
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
        /// Obtiene información de las prórrogas del contrato.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="cod_contrato"></param>
        /// <returns></returns>
        public ErrorDto<List<CprContratosProrrogasDTO>> CprContrato_Prorroga_Obtener(int CodEmpresa, string cod_contrato)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<List<CprContratosProrrogasDTO>>
            {
                Code = 0
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"select * from CPR_CONTRATOS_PRORROGAS WHERE COD_CONTRATO = '{cod_contrato}'";
                    response.Result = connection.Query<CprContratosProrrogasDTO>(query).ToList();
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
        /// Guarda información de la prorroga del contrato, si el id_prorroga es igual a 0 entonces agrega un nuevo registro, sino actualiza la información.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="prorroga"></param>
        /// <returns></returns>
        public ErrorDto CprContrato_Prorroga_Guardar(int CodEmpresa, CprContratosProrrogasDTO prorroga)
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
                    var query = "";

                    if (prorroga.id_prorroga == 0)
                    {
                        query = $@"INSERT INTO CPR_CONTRATOS_PRORROGAS (
                            COD_CONTRATO, 
                            FECHA, 
                            MOTIVOS,
                            REGISTRO_FECHA, 
                            REGISTRO_USUARIO
                        ) VALUES (
                            '{prorroga.cod_contrato}',
                            '{prorroga.fecha}', 
                            '{prorroga.motivos}', 
                            getDate(),  
                            '{prorroga.registro_usuario}'
                        );";
                        response.Description = "Prorroga agregada correctamente";
                        connection.Query(query);

                        query = "select MAX(ID_PRORROGA) from CPR_CONTRATOS_PRORROGAS";
                        int id_prorroga = connection.Query<int>(query).First();

                        BitacoraContratos(CodEmpresa,
                        new CprContratosBitacoraDTO
                        {
                            cod_contrato = prorroga.cod_contrato,
                            movimiento = "Inserta",
                            detalle = $@"Ingresa prorroga Id: " + id_prorroga,
                            registro_usuario = prorroga.registro_usuario
                        });
                    }
                    else
                    {
                        query = $@"UPDATE CPR_CONTRATOS_PRORROGAS 
                        SET
                            FECHA = '{prorroga.fecha}', 
                            MOTIVOS = '{prorroga.motivos}'
                        WHERE ID_PRORROGA = {prorroga.id_prorroga}";
                        response.Description = "Prorroga actualizada correctamente";
                        connection.Query(query);

                        BitacoraContratos(CodEmpresa,
                        new CprContratosBitacoraDTO
                        {
                            cod_contrato = prorroga.cod_contrato,
                            movimiento = "Actualiza",
                            detalle = $@"Modifica datos de prorroga Id: " + prorroga.id_prorroga,
                            registro_usuario = prorroga.registro_usuario
                        });
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
        /// Elimina la prorroga mediante el id_prorroga.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="id_prorroga"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto CprContrato_Prorroga_Eliminar(int CodEmpresa, int id_prorroga, string usuario)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            ErrorDto response = new()
            {
                Code = 0
            };
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"select COD_CONTRATO from CPR_CONTRATOS_PRORROGAS WHERE id_prorroga = {id_prorroga}";
                    string cod_contrato = connection.Query<string>(query).First();

                    query = $@"delete from CPR_CONTRATOS_PRORROGAS WHERE id_prorroga = {id_prorroga}";

                    response.Code = connection.Execute(query);
                    response.Description = "Prorroga eliminada correctamente";

                    BitacoraContratos(CodEmpresa,
                    new CprContratosBitacoraDTO
                    {
                        cod_contrato = cod_contrato,
                        movimiento = "Elimina",
                        detalle = $@"Elimina prorroga Id: " + id_prorroga,
                        registro_usuario = usuario
                    });
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
        /// Obtiene información de la bitácora de un contrato.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="cod_contrato"></param>
        /// <returns></returns>
        public ErrorDto<List<CprContratosBitacoraDTO>> CprContrato_Bitacora_Obtener(int CodEmpresa, string cod_contrato)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<List<CprContratosBitacoraDTO>>
            {
                Code = 0
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"select * from CPR_CONTRATOS_BITACORA WHERE COD_CONTRATO = '{cod_contrato}'";
                    response.Result = connection.Query<CprContratosBitacoraDTO>(query).ToList();
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
        /// Inserta la acción realiza en bitacora.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="req"></param>
        /// <returns></returns>
        private ErrorDto BitacoraContratos(int CodEmpresa, CprContratosBitacoraDTO req)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            ErrorDto resp = new ErrorDto();
            resp.Code = 0;
            try
            {

                using var connection = new SqlConnection(stringConn);
                {

                    var strSQL = $@"INSERT INTO CPR_CONTRATOS_BITACORA
                                           ([COD_CONTRATO]
                                           ,[MOVIMIENTO]
                                           ,[DETALLE]
                                           ,[REGISTRO_FECHA]
                                           ,[REGISTRO_USUARIO])
                                     VALUES
                                           ('{req.cod_contrato}'
                                           ,'{req.movimiento}' 
                                           , '{req.detalle}'
                                           , getdate()
                                           , '{req.registro_usuario}' )";

                    resp.Code = connection.Execute(strSQL);
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
        /// Obtiene la descripción del estado mediante el codigo del estado (CATALOGO_ID).
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="estado"></param>
        /// <returns></returns>
        private string EstadoDescripcion_Obtener(int CodEmpresa, string estado)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            string response = "";
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"select DESCRIPCION from CPR_CATALOGOS 
                    where Tipo_Id = (select TIPO_ID from CPR_CATALOGOS_TIPOS where DESCRIPCION = 'Estados Contrato')
                    AND CATALOGO_ID = '{estado}'";
                    response = connection.Query<string>(query).First();
                }
            }
            catch (Exception ex)
            {
                response = estado;
            }

            return response;
        }

        /// <summary>
        /// Envia una notificación al rol de confección del contrato con la información del mismo.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="cod_contrato"></param>
        /// <param name="mensaje"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public async Task<ErrorDto> CprContratoNotificacion_Enviar(int CodEmpresa, string cod_contrato, string mensaje, string usuario)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            ErrorDto response = new ErrorDto
            {
                Code = 0
            };
            var InfoContrato = new CprContratosDTO();
            EnvioCorreoModels eConfig = new();
            string emailConfeccionContrato = "";
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"select COD_CONTRATO, DESCRIPCION,  
                        CTA_CONTABLE, FISCAl, NOTAS, MONTO, DIVISA,
                        (select top 1 DESCRIPCION from CXP_PROVEEDORES WHERE COD_PROVEEDOR = C.COD_PROVEEDOR) AS PROVEEDOR,
                        (select top 1 DESCRIPCION from CNTX_CENTRO_COSTOS WHERE COD_CENTRO_COSTO = C.COD_CENTRO_COSTO) AS COD_CENTRO_COSTO,
                        (select top 1 DESCRIPCION from CPR_CATALOGOS 
                        where Tipo_Id = (select TIPO_ID from CPR_CATALOGOS_TIPOS where DESCRIPCION = 'Contratos') 
                        and CATALOGO_ID = C.TIPO_CONTRATO) AS TIPO_CONTRATO,
                        PORCENTAJE_GARANTIA, MONTO_GARANTIA, DIVISA_GARANTIA
                        from CPR_CONTRATOS C where COD_CONTRATO = '{cod_contrato}'";
                    InfoContrato = connection.Query<CprContratosDTO>(query).First();

                    eConfig = _envioCorreoDB.CorreoConfig(CodEmpresa, codNotificaciones);

                    var queryEmail = @$"select VALOR from SIF_PARAMETROS where COD_PARAMETRO = '{nofiticacionConfeccionContrato}'";
                    emailConfeccionContrato = connection.Query<string>(queryEmail).FirstOrDefault();
                }

                switch (InfoContrato.divisa) { 
                    case "C":
                        InfoContrato.divisa = "Colones";
                        break;
                    case "D":
                        InfoContrato.divisa = "Dólares";
                        break;
                    default:
                        break;
                }

                string body = @$"<html lang=""es"">
                    <head>
                        <meta charset=""UTF-8"">
                        <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
                        <title>Solicitud para Confección de Contrato</title>
                        <style>
                            table {{
                                border-collapse: collapse;
                                width: 100%;
                            }}
                            td {{
                                border: 1px solid #000;
                                padding: 8px;
                                vertical-align: top;
                            }}
                            td.label {{
                                font-weight: bold;
                                width: 30%;
                                background-color: #f0f0f0;
                            }}
                        </style>
                    </head>
                    <body>
                        <p>{mensaje}</p>

                        <table>
                            <tr>
                                <td class=""label"">No. Contrato</td>
                                <td>{InfoContrato.cod_contrato}</td>
                            </tr>
                            <tr>
                                <td class=""label"">Descripción</td>
                                <td>{InfoContrato.descripcion}</td>
                            </tr>
                            <tr>
                                <td class=""label"">Proveedor</td>
                                <td>{InfoContrato.proveedor}</td>
                            </tr>
                            <tr>
                                <td class=""label"">Tipo Contrato</td>
                                <td>{InfoContrato.tipo_contrato}</td>
                            </tr>
                            <tr>
                                <td class=""label"">Monto</td>
                                <td>{InfoContrato.monto}</td>
                            </tr>
                            <tr>
                                <td class=""label"">Divisa</td>
                                <td>{InfoContrato.divisa}</td>
                            </tr>
                            <tr>
                                <td class=""label"">CTA Contable</td>
                                <td>{InfoContrato.cta_contable}</td>
                            </tr>
                            <tr>
                                <td class=""label"">Centro Costo</td>
                                <td>{InfoContrato.cod_centro_costo}</td>
                            </tr>
                            <tr>
                                <td class=""label"">Garantía de Cumplimiento</td>
                                <td>{InfoContrato.porcentaje_garantia} %</td>
                            </tr>
                            <tr>
                                <td class=""label"">Monto de Garantía</td>
                                <td>{InfoContrato.monto_garantia}</td>
                            </tr>
                            <tr>
                                <td class=""label"">Divisa Garantía</td>
                                <td>{InfoContrato.divisa_garantia}</td>
                            </tr>
                            <tr>
                                <td class=""label"">Fiscalizador</td>
                                <td>{InfoContrato.fiscal}</td>
                            </tr>
                            <tr>
                                <td class=""label"">Notas</td>
                                <td>{InfoContrato.notas}</td>
                            </tr>
                        </table>
                    </body>
                </html>
                ";

                if (sendEmail == "Y")
                {
                    EmailRequest emailRequest = new EmailRequest();

                    emailRequest.To = emailConfeccionContrato;
                    emailRequest.From = eConfig.User;
                    emailRequest.Subject = "Confección de Contrato "+cod_contrato;
                    emailRequest.Body = body;

                    if (eConfig != null)
                    {
                        await _envioCorreoDB.SendEmailAsync(emailRequest, eConfig, response);
                    }
                }

                using var connection2 = new SqlConnection(clienteConnString);
                {
                    var query = $@"update CPR_CONTRATOS set FECHA_NOTIFICACION = getdate() where COD_CONTRATO = '{cod_contrato}'";
                    connection.Execute(query);
                }

                response.Description = "Notificación enviada correctamente";

                BitacoraContratos(CodEmpresa,
                new CprContratosBitacoraDTO
                {
                    cod_contrato = cod_contrato,
                    movimiento = "Notifica",
                    detalle = $@"Se envía noticiación para Confección del Contrato",
                    registro_usuario = usuario
                });
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }

        /// <summary>
        /// Obtiene la lista de contratos de los proveedores de una solicitud de compra mediante el cpr_id.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="cpr_id"></param>
        /// <returns></returns>
        public ErrorDto<List<CprContratosDTO>> CprContratosPorSolicitud_Obtener(int CodEmpresa, int cpr_id)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<List<CprContratosDTO>>
            {
                Code = 0
            };

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"select C.*, P.DESCRIPCION as PROVEEDOR from CPR_CONTRATOS C 
                        INNER JOIN CXP_PROVEEDORES P ON C.COD_PROVEEDOR = P.COD_PROVEEDOR 
                        WHERE C.COD_PROVEEDOR IN (select PROVEEDOR_CODIGO from CPR_SOLICITUD_PROV where CPR_ID = {cpr_id})";
                    response.Result = connection.Query<CprContratosDTO>(query).ToList();

                    foreach (var item in response.Result)
                    {
                        query = $@"SELECT c.ESTADO
                        FROM CPR_CONTRATOS_ESTADOS c
                        JOIN (
                            SELECT COD_CONTRATO, MAX(FECHA_INICIO) AS FECHA_INICIO_VIGENTE
                            FROM CPR_CONTRATOS_ESTADOS
                            WHERE FECHA_INICIO <= GETDATE()
                            GROUP BY COD_CONTRATO
                        ) vigente
                        ON c.COD_CONTRATO = vigente.COD_CONTRATO
                        AND c.FECHA_INICIO = vigente.FECHA_INICIO_VIGENTE
                        where c.COD_CONTRATO = '{item.cod_contrato}';";
                        item.estado = connection.Query<string>(query).FirstOrDefault();
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
    }
}