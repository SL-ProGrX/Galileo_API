using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.VisualBasic;
using PgxAPI.Models;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.INV;
using System.Data;


namespace PgxAPI.DataBaseTier
{
    public class frmInvTiposProductosDB
    {
        private readonly IConfiguration _config;

        public frmInvTiposProductosDB(IConfiguration config)
        {
            _config = config;
        }


        #region Tipo Productos
        


        /// <summary>
        /// Obtiene la lista lazy de tipos de producto 
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="pagina"></param>
        /// <param name="paginacion"></param>
        /// <param name="filtro"></param>
        /// <returns></returns>
        public ErrorDto<TipoProductoDataLista> TipoProducto_Obtener(int CodCliente, int? pagina, int? paginacion, string? filtro, int cod_contabilidad)
        {

            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);

            var info = new ErrorDto<TipoProductoDataLista>();
            info.Result = new TipoProductoDataLista();
            info.Result.Total = 0;
            try
            {
                var query = "";
                string paginaActual = " ", paginacionActual = " ";
                using var connection = new SqlConnection(clienteConnString);
                {

                    //Busco Total
                    query = $@"SELECT Count(*) FROM pv_prod_clasifica T LEFT JOIN CntX_cuentas C ON T.cod_cuenta = C.cod_cuenta and C.cod_contabilidad = '{cod_contabilidad}' ";
                    info.Result.Total = connection.Query<int>(query).FirstOrDefault();

                    if (filtro != null)
                    {
                        filtro = " WHERE T.cod_prodclas LIKE '%" + filtro + "%' OR t.DESCRIPCION LIKE '%" + filtro + "%' OR t.costeo LIKE '% " + filtro + "%' OR t.valuacion LIKE '%" + filtro + "%' ";
                    }

                    if (pagina != null)
                    {
                        paginaActual = " OFFSET " + pagina + " ROWS ";
                        paginacionActual = " FETCH NEXT " + paginacion + " ROWS ONLY ";
                    }

                    query = $@"SELECT T.cod_prodclas,T.descripcion,T.costeo,T.valuacion,C.cod_cuenta_Mask as cod_cuenta,T.cod_Alter,C.descripcion as Cta_Desc,
                            (SELECT COUNT(Cod_Prodclas) FROM PV_PROD_CLASIFICA_SUB where COD_PRODCLAS=T.cod_prodclas) AS Cantidad_Sub 
                            FROM pv_prod_clasifica T LEFT JOIN CntX_cuentas C ON T.cod_cuenta = C.cod_cuenta and C.cod_contabilidad = '{cod_contabilidad}'
                                         {filtro} 
                                       ORDER BY T.cod_prodclas desc
                                        {paginaActual}
                                        {paginacionActual} ";


                    info.Result.Lista = connection.Query<TipoProductoDTO>(query).ToList();

                }
            }
            catch (Exception ex)
            {
                info.Code = -1;
                info.Description = ex.Message;
                info.Result = new TipoProductoDataLista();
            }
            return info;
        }



        /// <summary>
        /// Obtiene la lista de tipos producto 
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public List<TipoProductoDTO> TipoProducto_ObtenerTodos(int CodEmpresa, int cod_contabilidad)
        {

            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            List<TipoProductoDTO> info = new List<TipoProductoDTO>();

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"SELECT T.cod_prodclas,T.descripcion,T.costeo,T.valuacion,C.cod_cuenta_Mask as cod_cuenta,T.cod_Alter,C.descripcion as Cta_Desc,
                    (SELECT COUNT(Cod_Prodclas) FROM PV_PROD_CLASIFICA_SUB where COD_PRODCLAS=T.cod_prodclas) AS Cantidad_Sub 
                    FROM pv_prod_clasifica T LEFT JOIN CntX_cuentas C ON T.cod_cuenta = C.cod_cuenta and C.cod_contabilidad = {cod_contabilidad} ORDER BY T.cod_prodclas";

                    info = connection.Query<TipoProductoDTO>(query).ToList();
                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return info;
        }


        /// <summary>
        /// Actualiza el tipo producto
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto TipoProducto_Actualizar(int CodEmpresa, TipoProductoDTO request)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            ErrorDto resp = new ErrorDto();
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = "Update pv_prod_clasifica set descripcion = @Descripcion, costeo = @Costeo, valuacion = @Valuacion, " +
                        "cod_cuenta = @Cod_Cuenta, cod_alter = @Cod_Alter where Cod_Prodclas = @Cod_Prodclas";

                    var parameters = new DynamicParameters();
                    parameters.Add("Cod_Prodclas", request.Cod_Prodclas, DbType.Int32);
                    parameters.Add("Descripcion", request.Descripcion, DbType.String);
                    parameters.Add("Costeo", request.Costeo, DbType.String);
                    parameters.Add("Valuacion", request.Valuacion, DbType.String);
                    parameters.Add("Cod_Cuenta", request.Cod_Cuenta.Replace("-", ""), DbType.String);
                    parameters.Add("Cod_Alter", request.Cod_Alter, DbType.String);

                    resp.Code = connection.ExecuteAsync(query, parameters).Result;
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
        /// Inserta un tipo producto
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto TipoProducto_Insertar(int CodEmpresa, TipoProductoDTO request)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            ErrorDto resp = new ErrorDto();
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = "insert into pv_prod_clasifica(descripcion,costeo,valuacion,cod_cuenta,cod_alter)values(@Descripcion, @Costeo, @Valuacion,@Cod_Cuenta,@Cod_Alter)";

                    var parameters = new DynamicParameters();
                    parameters.Add("Descripcion", request.Descripcion, DbType.String);
                    parameters.Add("Costeo", request.Costeo, DbType.String);
                    parameters.Add("Valuacion", request.Valuacion, DbType.String);
                    parameters.Add("Cod_Cuenta", request.Cod_Cuenta.Replace("-", ""), DbType.String);
                    parameters.Add("Cod_Alter", request.Cod_Alter, DbType.String);


                    resp.Code = connection.ExecuteAsync(query, parameters).Result;
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
        /// Elimina un tipo producto
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="producto"></param>
        /// <returns></returns>
        public ErrorDto TipoProducto_Eliminar(int CodEmpresa, string producto)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            ErrorDto resp = new ErrorDto();
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = "DELETE pv_prod_clasifica where cod_prodclas = @Cod_Prodclas";

                    var parameters = new DynamicParameters();
                    parameters.Add("Cod_Prodclas", producto, DbType.Int32);

                    resp.Code = connection.ExecuteAsync(query, parameters).Result;
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

        #endregion


        #region SubCategorías


        /// <summary>
        /// Obtiene la lista lazy de subcategorias de tipos de producto
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="ProdClas"></param>
        /// <param name="pagina"></param>
        /// <param name="paginacion"></param>
        /// <param name="filtro"></param>
        /// <returns></returns>
        public ErrorDto<TipoProductoSubDataLista> TipoProductoSub_Obtener(int CodCliente, int ProdClas, int? pagina, int? paginacion, string? filtro)
        {

            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);

            var info = new ErrorDto<TipoProductoSubDataLista>();
            info.Result = new TipoProductoSubDataLista();
            info.Result.Total = 0;
            try
            {
                var query = "";
                string paginaActual = " ", paginacionActual = " ";
                using var connection = new SqlConnection(clienteConnString);
                {

                    //Busco Total
                    query = $@"SELECT count(*) FROM PV_PROD_CLASIFICA_SUB WHERE Cod_Prodclas = {ProdClas}";
                    info.Result.Total = connection.Query<int>(query).FirstOrDefault();

                    if (filtro != null)
                    {
                        filtro = " WHERE Descripcion LIKE '%" + filtro + "%' OR Niveles  LIKE '%" + filtro + "%' ";
                    }

                    if (pagina != null)
                    {
                        paginaActual = " OFFSET " + pagina + " ROWS ";
                        paginacionActual = " FETCH NEXT " + paginacion + " ROWS ONLY ";
                    }

                    query = $@"WITH RecursiveHierarchy AS (
                                -- Paso base: Selecciona los registros raíz (los que no tienen madre)
                                SELECT 
                                    COD_LINEA_SUB,
		                            Cod_Prodclas,
                                    DESCRIPCION,
                                    COD_LINEA_SUB_MADRE,
		                            Activo,
		                            Cabys,
		                            COD_CUENTA,
		                            NIVEL,
		                            COD_LINEA_SUB_MADRE,
                                    CAST(CAST(COD_LINEA_SUB AS VARCHAR(MAX)) AS VARCHAR(MAX)) AS Niveles -- Usa COD_LINEA_SUB como base
                                FROM 
                                    PV_PROD_CLASIFICA_SUB
                                WHERE 
                                    COD_LINEA_SUB_MADRE IS NULL AND COD_PRODCLAS = {ProdClas} 

                                UNION ALL

                                -- Paso recursivo: Construye el nivel jerárquico concatenando COD_LINEA_SUB del padre y NIVEL del hijo
                                SELECT 
                                    p.COD_LINEA_SUB,
		                            p.Cod_Prodclas,
                                    p.DESCRIPCION,
                                    p.COD_LINEA_SUB_MADRE,
		                            p.Activo,
		                            p.Cabys,
		                            p.COD_CUENTA,
	                                p.NIVEL,
		                            p.COD_LINEA_SUB_MADRE,
                                    CONCAT(rh.Niveles, '.', p.NIVEL) AS Niveles -- Concatenar COD_LINEA_SUB raíz con NIVEL
                                FROM 
                                    PV_PROD_CLASIFICA_SUB p
                                INNER JOIN 
                                    RecursiveHierarchy rh ON p.COD_LINEA_SUB_MADRE = rh.COD_LINEA_SUB
                                WHERE 
                                    p.COD_PRODCLAS = {ProdClas} 
                            )
                            SELECT 
                                COD_LINEA_SUB,
	                            Cod_Prodclas,
                                DESCRIPCION,
                                COD_LINEA_SUB_MADRE,
	                            Activo,
	                            Cabys,
	                            COD_CUENTA,
	                            NIVEL,
	                            COD_LINEA_SUB_MADRE,
                                Niveles
                            FROM 
                                RecursiveHierarchy
                              {filtro} 
                            ORDER BY 
                                Niveles ASC {paginaActual}
                                        {paginacionActual}";


                    info.Result.Lista = connection.Query<TipoProductoSubDTO>(query).ToList();

                }
            }
            catch (Exception ex)
            {
                info.Code = -1;
                info.Description = ex.Message;
                info.Result = new TipoProductoSubDataLista();
            }
            return info;
        }



        public ErrorDto<List<TipoProductoSubGradaData>> TipoProductoSub_ObtenerTodos(int CodEmpresa, string Cod_Prodclas)
        {

            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<List<TipoProductoSubGradaData>>
            {
                Code = 0,
                Result = new List<TipoProductoSubGradaData>()
            };


            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"SELECT Cod_Prodclas,Cod_Linea_Sub, Descripcion, Activo, Cabys 
                                            ,COD_CUENTA, NIVEL, COD_LINEA_SUB_MADRE
                                                FROM PV_PROD_CLASIFICA_SUB 
                                          WHERE Cod_Prodclas = '{Cod_Prodclas}'";

                    var info = connection.Query<TipoProductoSubDTO>(query).ToList();
                    foreach (TipoProductoSubDTO dt in info)
                    {
                        dt.Estado = dt.Activo ? "ACTIVO" : "INACTIVO";

                        if (dt.Nivel == 1)
                        {
                            response.Result.Add(new TipoProductoSubGradaData
                            {
                                key = dt.Cod_Linea_Sub,
                                icon = "",
                                label = dt.Descripcion,
                                data = dt,
                                children = TipoProductoSub_SeguienteNivel(CodEmpresa, dt)
                            });
                        }
                        
                    }
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = new List<TipoProductoSubGradaData>();
            }
            return response;
        }

        public List<TipoProductoSubGradaData> TipoProductoSub_SeguienteNivel(int CodEmpresa, TipoProductoSubDTO padre)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new List<TipoProductoSubGradaData>();
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"SELECT Cod_Prodclas,Cod_Linea_Sub, Descripcion, Activo, Cabys 
                                            ,COD_CUENTA, NIVEL, COD_LINEA_SUB_MADRE
                                                FROM PV_PROD_CLASIFICA_SUB 
                                          WHERE Cod_Prodclas = '{padre.Cod_Prodclas}' 
                                          AND COD_LINEA_SUB_MADRE = '{padre.Cod_Linea_Sub}' ";

                    var info = connection.Query<TipoProductoSubDTO>(query).ToList();
                    foreach (TipoProductoSubDTO dt in info)
                    {
                        dt.Estado = dt.Activo ? "ACTIVO" : "INACTIVO";

                        response.Add(new TipoProductoSubGradaData
                        {
                            key = dt.Cod_Linea_Sub,
                            icon = "",
                            label = dt.Descripcion,
                            data = dt,
                            children = TipoProductoSub_SeguienteNivel(CodEmpresa, dt)
                        });
                    }
                }
                
            }
            catch (Exception)
            {
                response = null;
            }
            return response;
        }

        public ErrorDto TipoProductoSub_Actualizar(int CodEmpresa, TipoProductoSubDTO request)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            ErrorDto resp = new();
            try
            {

                resp = fxValidaProfundidadRaiz(CodEmpresa, request.Cod_Linea_Sub);
                if (resp.Code == -1)
                {
                    
                    return resp;
                }

                using var connection = new SqlConnection(stringConn);
                {



                    int nivel = 0;
                    if (request.Cod_Linea_Sub_Madre != "")
                    {
                        //Busco nivel de linea
                        var qNivel = $@"SELECT NIVEL FROM PV_PROD_CLASIFICA_SUB WHERE Cod_Linea_Sub = '{request.Cod_Linea_Sub_Madre}' ";
                        nivel = connection.Query<int>(qNivel).FirstOrDefault();
                    }
                    

                    string UpdateCodLinea = "";
                    
                    if(nivel > 0)
                    {
                        UpdateCodLinea = " COD_LINEA_SUB_MADRE = @COD_LINEA_SUB_MADRE, ";
                        nivel = nivel + 1;

                        if(nivel > 5)
                        {
                            return new ErrorDto { Code = -1, Description = "No se puede agregar mas subcategorias" };
                        }

                    }
                    else
                    {
                        nivel = request.Nivel;
                    }

                    var query = $@"Update pv_prod_clasifica_Sub set 
                                    descripcion = @Descripcion, 
                                    activo = @Activo, 
                                    CABYS = @CABYS,
                                    COD_CUENTA = @COD_CUENTA,
                                    {UpdateCodLinea}
                                    NIVEL = @NIVEL
                                    WHERE Cod_Prodclas = @Cod_Prodclas AND COD_LINEA_SUB = @Cod_Linea_Sub ";

                    var parameters = new DynamicParameters();
                    parameters.Add("Cod_Prodclas", request.Cod_Prodclas, DbType.Int32);
                    parameters.Add("Cod_Linea_Sub", request.Cod_Linea_Sub, DbType.String);
                    parameters.Add("Descripcion", request.Descripcion, DbType.String);
                    parameters.Add("Activo", request.Activo, DbType.Int32);
                    parameters.Add("CABYS", request.Cabys, DbType.String);
                    parameters.Add("COD_CUENTA", request.Cod_Cuenta, DbType.String);
                    parameters.Add("NIVEL", nivel, DbType.Int32);

                    if (request.Cod_Linea_Sub_Madre != "")
                    {
                        parameters.Add("COD_LINEA_SUB_MADRE", request.Cod_Linea_Sub_Madre, DbType.Int32);
                    }

                    resp.Code = connection.ExecuteAsync(query, parameters).Result;
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

        public ErrorDto TipoProductoSub_Insertar(int CodEmpresa, TipoProductoSubDTO request)

        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            ErrorDto resp = new ErrorDto();
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    //Busco ultimo consecutivo
                    var qConsecutivo = $@"SELECT ISNULL(MAX(COD_LINEA_SUB),0) + 1 FROM PV_PROD_CLASIFICA_SUB";
                    int consecutivo = connection.Query<int>(qConsecutivo).FirstOrDefault();

                    var query = "insert into pv_prod_clasifica_Sub(COD_PRODCLAS,COD_LINEA_SUB, DESCRIPCION, Activo, CABYS, REGISTRO_FECHA, REGISTRO_USUARIO, NIVEL)" +
                        "values(@Cod_Prodclas,@Cod_Linea_Sub,@Descripcion, @Activo, @CABYS,@Registro_Fecha,@Registro_Usuario, 1)";

                    var parameters = new DynamicParameters();
                    parameters.Add("Cod_Prodclas", request.Cod_Prodclas, DbType.Int32);
                    parameters.Add("Cod_Linea_Sub", consecutivo, DbType.String);
                    parameters.Add("Descripcion", request.Descripcion, DbType.String);
                    parameters.Add("Activo", request.Activo, DbType.Int32);
                    parameters.Add("CABYS", request.Cabys, DbType.String);
                    parameters.Add("Registro_Usuario", request.Registro_Usuario, DbType.String);
                    parameters.Add("Registro_Fecha", DateTime.Now, DbType.DateTime);

                    resp.Code = connection.ExecuteAsync(query, parameters).Result;
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

        public ErrorDto<List<InvCabys>> Cabys_ObtenerTodos(int CodEmpresa)
        {

            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            var info = new ErrorDto<List<InvCabys>>();

            try
            {
                using var connection = new SqlConnection(stringConn);
                {

                    var query = "SELECT COD_BYS,DESCRIPCION FROM vINV_Cabys";
                    info.Result = connection.Query<InvCabys>(query).ToList();

                }
            }
            catch (Exception ex)
            {
                info.Code = -1;
                info.Description = ex.Message;
                info.Result = new List<InvCabys>();
            }
            return info;
        }

        public ErrorDto<List<InvCabys>> Cabys_Obtener(int CodEmpresa, string filtro)
        {

            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            var info = new ErrorDto<List<InvCabys>>();

            try
            {
                using var connection = new SqlConnection(stringConn);
                {

                    var query = "SELECT TOP 30 COD_BYS,DESCRIPCION FROM vINV_Cabys WHERE COD_BYS like @filtro OR DESCRIPCION like @filtro";

                    var parameters = new DynamicParameters();
                    parameters.Add("filtro", "%" + filtro + "%", DbType.String);


                    info.Result = connection.Query<InvCabys>(query, parameters).ToList();

                }
            }
            catch (Exception ex)
            {
                info.Code = -1;
                info.Description = ex.Message;
                info.Result = new List<InvCabys>();
            }
            return info;
        }

        #endregion


        /// <summary>
        /// Formatea la cuenta (No se usa ya existe campo con el mask en la bd)
        /// </summary>
        /// <param name="blnMascara"></param>
        /// <param name="pCuenta"></param>
        /// <param name="optMensaje"></param>
        /// <returns></returns>
        public string fxgCntCuentaFormato(bool blnMascara, string pCuenta, int optMensaje = 1)
        {
            int i;
            string strResultado;

            pCuenta = Strings.Trim(pCuenta);
            strResultado = "";
            for (i = 1; i <= Strings.Len(pCuenta); i++)
            {
                if (Strings.Mid(pCuenta, i, 1) != "-")
                    strResultado = strResultado + Strings.Mid(pCuenta, i, 1);
            }

            pCuenta = strResultado;

            //if (!Information.IsNumeric(pCuenta))
            //{
            //    fxgCntCuentaFormato = pCuenta;
            //    if (optMensaje == 1)
            //        Interaction.MsgBox("Código de cuenta inválido...", Constants.vbCritical);
            //}

            //for (i = Strings.Len(pCuenta); i <= GLOBALES.gMascaraTChar - 1; i++)
            //    pCuenta = pCuenta + "0";

            //if (blnMascara)
            //    pCuenta = Format(pCuenta, GLOBALES.gstrMascara);

            //fxgCntCuentaFormato = pCuenta;

            return strResultado;
        }


        private ErrorDto fxValidaProfundidadRaiz(int CodCliente, string codigoRaiz)
        {
            var stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            ErrorDto error = new();
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    // Consulta para verificar si es el raíz o primer item
                    string queryRaiz = $@"
                        SELECT 
                            COD_LINEA_SUB,
                            COD_LINEA_SUB_MADRE,
                            (SELECT COUNT(*) FROM PV_PROD_CLASIFICA_SUB WHERE COD_LINEA_SUB_MADRE = p.COD_LINEA_SUB) AS HIJOS
                        FROM 
                            PV_PROD_CLASIFICA_SUB p
                        WHERE 
                            COD_LINEA_SUB = {codigoRaiz}";

                    var registro =  connection.Query(queryRaiz).FirstOrDefault();

                    if (registro == null)
                    {
                        error.Code = -1;
                        error.Description = "El registro no existe.";
                    }

                    // Si COD_LINEA_SUB_MADRE es NULL, validamos si es el primer ítem de la jerarquía
                    if (registro.COD_LINEA_SUB_MADRE == null)
                    {
                        // Si no tiene hijos (es el primer ítem de la jerarquía), se puede cambiar el nivel
                        if (registro.HIJOS == 0)
                        {
                            error.Code = 0;
                            error.Description = "Es el primer ítem, se puede cambiar de nivel.";
                        }
                        else
                        {
                            error.Code = -1;
                            error.Description = "No se puede cambiar el nivel del item raíz si tiene hijos.";
                        }
                    }
                    else
                    {
                        error.Code = 0;
                        error.Description = "El cambio de nivel es permitido.";
                        // Si el COD_LINEA_SUB_MADRE no es NULL, se permite el cambio de nivel
                    }
                }
            }
            catch (Exception ex)
            {
                error.Code = -1;
                error.Description = ex.Message;
            }
            return error;
        }

    }
}
