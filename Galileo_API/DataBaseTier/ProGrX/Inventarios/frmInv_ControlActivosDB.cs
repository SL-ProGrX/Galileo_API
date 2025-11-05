using PgxAPI.Models.INV;
using System.Data;
using Dapper;
using PgxAPI.Models.ERROR;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;

namespace PgxAPI.DataBaseTier
{
    public class frmInv_ControlActivosDB
    {
        private readonly IConfiguration? _config;
        private readonly mProGrX_AuxiliarDB _mAuxiliarDB;

        public frmInv_ControlActivosDB(IConfiguration config)
        {
            _config = config;
            _mAuxiliarDB = new mProGrX_AuxiliarDB(config);
        }

        /// <summary>
        /// Metodo para obtener la lista de activos
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDTO<InvControlActivosLista> InvControlActivosLista_Obtener(int CodEmpresa, string usuario, string filtros)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            InvControlActivosFiltros filtro = JsonConvert.DeserializeObject<InvControlActivosFiltros>(filtros);

            var response = new ErrorDTO<InvControlActivosLista>();
            response.Result = new InvControlActivosLista();
            response.Result.total = 0;
            try
            {
                var query = "";
                string paginaActual = " ", paginacionActual = " ";

                using var connection = new SqlConnection(stringConn);
                {
                    //Busco Total
                    query = $@"SELECT COUNT(*) FROM PV_CONTROL_ACTIVOS WHERE ENTREGA_USUARIO = '' 
                                AND ESTADO IN ('P', 'R') AND REGISTRO_USUARIO = '{usuario}' ";
                    response.Result.total = connection.Query<int>(query).FirstOrDefault();

                    if(filtro.filtro != null)
                    {
                        filtro.filtro = $@" AND ( A.COD_PRODUCTO like '%{filtro.filtro}%' 
                                            OR A.FACTURA like '%{filtro.filtro}%' 
                                            OR A.COD_COMPRA like '%{filtro.filtro}%' )";
                    }

                    if (filtro.pagina != null)
                    {
                        paginaActual = " OFFSET " + filtro.pagina + " ROWS ";
                        paginacionActual = " FETCH NEXT " + filtro.paginacion + " ROWS ONLY ";
                    }

                    query = $@"SELECT A.*, P.DESCRIPCION
                                    FROM PV_CONTROL_ACTIVOS A left join PV_PRODUCTOS P ON P.COD_PRODUCTO = A.COD_PRODUCTO
                                    WHERE A.ENTREGA_USUARIO = '' AND A.ESTADO IN ('P', 'R') AND A.REGISTRO_USUARIO =  '{usuario}'
                                    {filtro.filtro}
                                    ORDER BY A.ID_CONTROL
                                    {paginaActual}
                                    {paginacionActual}";

                    response.Result.lista = connection.Query<InvControlActivosDTO>(query).ToList();

                    foreach (var item in response.Result.lista)
                    {
                        query = $@"SELECT COD_UNIDAD FROM CPR_SOLICITUD_BS 
                                WHERE CPR_ID IN (
                                SELECT CPR_ID FROM CPR_SOLICITUD_PROV WHERE ADJUDICA_IND = 1
                                AND ADJUDICA_ORDEN IN (SELECT COD_ORDEN FROM CPR_COMPRAS WHERE COD_FACTURA = '{item.factura}')
                                ) ";

                        item.cod_uen = connection.Query<string>(query).FirstOrDefault();
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
        /// Metodo para actualizar el control de activos
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="activo"></param>
        /// <returns></returns>
        public ErrorDTO InvControlActivos_Actualizar(int CodEmpresa, InvControlActivosDTO activo)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
             ErrorDTO resp = new();
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var procedure = "[spCPR_CONTROL_ACTIVOS_ACTUALIZAR]";
                    var values = new
                    {
                        id_control = activo.id_control ,
                        cod_producto = activo.cod_producto,
                        descripcion = activo.descripcion ,
                        costo_total = activo.costo_total,
                        costo_unitario = activo.costo_unitario,
                        factura = activo.factura,
                        cod_compra = activo.cod_compra,
                        fecha_compra = activo.fecha_compra,
                        cod_proveedor = activo.cod_proveedor,
                        cod_bodega = activo.cod_bodega,
                        estado = activo.estado,
                        numero_placa = activo.numero_placa,
                        cod_localizacion = activo.cod_localizacion,
                        marca = activo.marca,
                        modelo = activo.modelo,
                        serie = activo.serie,
                        observaciones = activo.observaciones,
                        cod_uen = activo.cod_uen,
                        id_responsable = activo.id_responsable,
                        cod_requesicion = activo.cod_requesicion,
                        activo_usuario = activo.activo_usuario,
                        registro_usuario = activo.registro_usuario,
                        departamento = activo.departamento,
                        seccion = activo.seccion
                    };

                    resp.Code = connection.Query<int>(procedure, values, commandType: CommandType.StoredProcedure).FirstOrDefault();
                    resp.Description = "Activo actualizado correctamente";

                    string fecha = _mAuxiliarDB.validaFechaGlobal(activo.fecha_compra);

                    var query = $@"select TOP 1 tipo_activo FROM PV_PRODUCTOS WHERE COD_PRODUCTO = '{activo.cod_producto}'";
                    var tipo_activo = connection.Query<string>(query).FirstOrDefault();


                    query = $@"select MET_DEPRECIACION, VIDA_UTIL, TIPO_VIDA_UTIL from Activos_tipo_activo where TIPO_ACTIVO = '{tipo_activo}' ";
                    var tipo_activo_data = connection.Query<InvDatosActivos>(query).FirstOrDefault();

                    //Valida si el proveedor existe en Activos_Proveedores
                    query = $@"select count(*) from Activos_Proveedores where COD_PROVEEDOR = '{activo.cod_proveedor}'";
                    var proveedor = connection.Query<int>(query).FirstOrDefault();

                    if (proveedor == 0)
                    {
                        //Busco nombre del preveedor
                        query = $@"select NOMBRE from CXP_PROVEEDORES where COD_PROVEEDOR = '{activo.cod_proveedor}'";
                        var nombre_proveedor = connection.Query<string>(query).FirstOrDefault();

                        query = $@"INSERT INTO Activos_Proveedores (COD_PROVEEDOR, NOMBRE, ACTIVO, 
                                    REGISTRO_FECHA, REGISTRO_USUARIO) VALUES ('{activo.cod_proveedor}', '{nombre_proveedor}', 1,
                                    getDate(), '{activo.activo_usuario}')";
                        resp.Code = connection.Execute(query);
                    }


                    query = $@"
                           INSERT INTO ACTIVOS_PRINCIPAL
                                       ([NUM_PLACA]
                                       ,[TIPO_ACTIVO]
                                       ,[COD_DEPARTAMENTO]
                                       ,[COD_SECCION]
                                       ,[COD_PROVEEDOR]
									   ,[NOMBRE]
                                       ,[DESCRIPCION]
                                       ,[VALOR_HISTORICO]
                                       ,[VALOR_DESECHO]
                                       ,[FECHA_ADQUISICION]
                                       ,[MODELO]
                                       ,[NUM_SERIE]
                                       ,[MARCA]
                                       ,[COMPRA_DOCUMENTO]
                                       ,[ESTADO]
                                       ,[REGISTRO_FECHA]
                                       ,[REGISTRO_USUARIO]
                                       ,[COD_LOCALIZA]
									   ,UD_ANIO 
									   ,UD_PRODUCCION 
									   ,IDENTIFICACION
									   ,VIDA_UTIL
									   ,VIDA_UTIL_EN
									   ,MET_DEPRECIACION
									   ,DEPRECIACION_MES
									   ,DEPRECIACION_ACUM
									   ,DEPRECIACION_PERIODO
									   ,VALOR_LIBROS_PERIODO
									   ,FECHA_INSTALACION
                                      )
                                 VALUES (
                                        '{activo.numero_placa}'
                                       ,'{tipo_activo}'
                                       ,'{activo.departamento}'
                                       ,'{activo.seccion}'
                                       ,'{activo.cod_proveedor}'
									   ,'{activo.descripcion}'
                                       ,'{activo.descripcion}'
                                       ,{activo.costo_unitario}
                                       ,{activo.costo_unitario}
                                       ,'{fecha}'
                                       ,'{activo.modelo}'
                                       ,'{activo.serie}'
                                       ,'{activo.marca}'
                                       ,'{activo.cod_compra}'
                                       ,'R'
                                       ,getDate()
                                       ,'{activo.activo_usuario}'
                                       ,'{activo.cod_localizacion}'
									   ,0
									   ,0
									   ,'{activo.id_responsable}'
									   ,{tipo_activo_data.vida_util}
									   ,'{tipo_activo_data.tipo_vida_util}'
									   ,'{tipo_activo_data.met_depreciacion}'
									   ,0
									   ,0
									   ,GETDATE()
									   ,{activo.costo_unitario}
									   ,getDate() +1
                                     )";

                    resp.Code = connection.Execute(query);
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
        /// Metodo para obtener el id de la placa
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDTO InvNumeroPlacaId_Obtener(int CodEmpresa)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            ErrorDTO resp = new();
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"select dbo.fxActivos_W_Placa_Id() as 'PLACA_ID'";
                    resp.Description = connection.Query<string>(query).FirstOrDefault();
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
        /// Metodo para obtener los departamentos para activos
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDTO<List<InvCntrActvivosCombos>> InvActivosDepartamentos_Obtener(int CodEmpresa)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDTO<List<InvCntrActvivosCombos>>(); 
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"select rtrim(cod_departamento) as 'item' , rtrim(descripcion) as 'descripcion' 
	                                  from Activos_departamentos order by cod_departamento";
                    response.Result = connection.Query<InvCntrActvivosCombos>(query).ToList();
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
        /// Metodo para obtener las secciones de activos
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="departamento"></param>
        /// <returns></returns>
        public ErrorDTO<List<InvCntrActvivosCombos>> InvActivosSeccion_Obtener(int CodEmpresa, string? departamento)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDTO<List<InvCntrActvivosCombos>>();
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"select rtrim(cod_Seccion) as 'item', rtrim(descripcion) as 'descripcion' from Activos_Secciones
                                           Where cod_departamento = '{departamento}' order by cod_Seccion";
                    response.Result = connection.Query<InvCntrActvivosCombos>(query).ToList();
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
        /// Metodo para obtener los responsables de activos
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="departamento"></param>
        /// <param name="seccion"></param>
        /// <returns></returns>
        public ErrorDTO<List<InvCntrActvivosCombos>> InvActivosResponsable_Obtener(int CodEmpresa, string? departamento, string? seccion)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDTO<List<InvCntrActvivosCombos>>();
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"select rtrim(Identificacion) as 'item', rtrim(Nombre) as 'descripcion' from Activos_Personas
                                    Where cod_departamento = '{departamento}' 
			                        and cod_Seccion = '{seccion}' order by identificacion";
                    response.Result = connection.Query<InvCntrActvivosCombos>(query).ToList();
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
        /// Metodo para obtener las localizaciones de activos
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDTO<List<InvCntrActvivosCombos>> InvActivosLocalizaciones_Obtener(int CodEmpresa)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDTO<List<InvCntrActvivosCombos>>();
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"select rtrim(COD_LOCALIZA) as 'item', rtrim(descripcion) as 'descripcion'
                                   from ACTIVOS_LOCALIZACIONES Where Activa = 1 order by descripcion";
                    response.Result = connection.Query<InvCntrActvivosCombos>(query).ToList();
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