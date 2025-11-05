using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models;
using PgxAPI.Models.CxP;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.ProGrX.Clientes;

namespace PgxAPI.DataBaseTier.ProGrX.Clientes
{
    public class frmAF_ComisionesParametrosDB
    {
        private readonly IConfiguration? _config;
        private readonly mCntLinkDB _mCnt;
        private readonly mTESFuncionesDB _mFun;
        private readonly mAfilicacionDB _mAfi;
        mSecurityMainDb DBBitacora;

        public frmAF_ComisionesParametrosDB(IConfiguration config)
        {
            _config = config;
            _mCnt = new mCntLinkDB(_config);
            _mFun = new mTESFuncionesDB(_config);
            _mAfi = new mAfilicacionDB(_config);
            DBBitacora = new mSecurityMainDb(_config);
        }

        public ErrorDto Bitacora(BitacoraInsertarDTO data)
        {
            return DBBitacora.Bitacora(data);
        }

        /// <summary>
        /// Obtener lista de parametros de comisiones
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtro"></param>
        /// <returns></returns>
        public ErrorDto<TablasListaGenericaModel> AF_ComisionesParametros_Obtener(int CodEmpresa, FiltrosLazyLoadData filtro)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<TablasListaGenericaModel>
            {
                Code = 0,
                Description = "Ok",
                Result = new TablasListaGenericaModel()
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var querySP = "exec spAFIComisionesParametros";
                    connection.Execute(querySP);

                    var query = $@"select count(*) from AFI_COMISIONES_PARAMETROS";
                    response.Result.total = connection.Query<int>(query).FirstOrDefault();

                    if (filtro.filtro != null && filtro.filtro != "")
                    {
                        filtro.filtro = $@"WHERE ( 
                                                 cod_parametro like '%{filtro.filtro}%' 
                                              OR descripcion like '%{filtro.filtro}%'
                                              OR valor like '%{filtro.filtro}%'
                                          )";
                    }

                    if (filtro.sortField == "" || filtro.sortField == null)
                    {
                        filtro.sortField = "cod_parametro";
                    }

                    if (filtro.sortOrder == 0)
                    {
                        filtro.sortOrder = 1; //Por defecto orden ascendente
                    }

                    if (filtro.pagina != null)
                    {
                        query = $@"select cod_parametro,descripcion,valor from AFI_COMISIONES_PARAMETROS 
                           {filtro.filtro} order by {filtro.sortField} {(filtro.sortOrder == -1 ? "DESC" : "ASC")}  
                                      OFFSET {filtro.pagina} ROWS
                                      FETCH NEXT {filtro.paginacion} ROWS ONLY ";

                        response.Result.lista = connection.Query<AF_Comisiones_ParametrosDTO>(query).ToList();
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
        /// Actualiza el valor de un parametro
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Contabilidad"></param>
        /// <param name="Usuario"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public ErrorDto AF_ComisionesParametros_Guardar(int CodEmpresa, int Contabilidad, string Usuario, AF_Comisiones_ParametrosDTO param)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto()
            {
                Code = 0,
                Description = ""
            };
            try
            {
                string valida = fxValida(CodEmpresa, Contabilidad, param);
                if (valida != "")
                {
                    response.Code = -2;
                    response.Description = valida;
                    return response;
                }
                using var connection = new SqlConnection(stringConn);
                {
                    var query = @"update AFI_COMISIONES_PARAMETROS set valor = @valor
                        where cod_parametro = @parametro";
                    connection.Execute(query, new
                    {
                        parametro = param.cod_parametro,
                        valor = param.valor
                    });

                    Bitacora(new BitacoraInsertarDTO
                    {
                        EmpresaId = CodEmpresa,
                        Usuario = Usuario.ToUpper(),
                        DetalleMovimiento = "Parametro de Comisiones de Afiliación : " + param.cod_parametro,
                        Movimiento = "MODIFICA - WEB",
                        Modulo = 9
                    });

                    response.Description = "Registro actualizado correctamente";
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
        /// Valida si el valor editado del parametro es
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Contabilidad"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        private string fxValida(int CodEmpresa, int Contabilidad, AF_Comisiones_ParametrosDTO param)
        {
            string vMensaje = "";
            try
            {
                string vParametro = param.cod_parametro ?? "";

                switch (vParametro)
                {
                    case "01": //Cuenta Contable
                        if (!_mCnt.fxgCntCuentaValida(CodEmpresa, param.valor ?? ""))
                        {
                            vMensaje = " - Cuenta Contable no es v&aacute;lida...!";
                        }
                        break;
                    case "19": //Tesoreria Unidad
                        if (!_mFun.fxgTESValidaDatos(CodEmpresa, Contabilidad, "UNIDAD", param.valor ?? ""))
                        {
                            vMensaje = " - C&oacute;digo de Unidad no existe o se encuentra desactivado...!";
                        }
                        break;
                    case "20": //Tesoreria Centro de Costo
                        string vUnidad = _mAfi.fxgAFIParametroComision(CodEmpresa, "19");
                        if (!_mFun.fxgTESValidaDatos(CodEmpresa, Contabilidad, "CC", param.valor ?? "", vUnidad))
                        {
                            vMensaje = " - C&oacute;digo de Centro de Costo no existe o se encuentra desactivado, o no ha sido asignado a esta unidad: " + vUnidad + "...!";
                        }
                        break;
                    case "21": //Tesoreria Conceptos
                        if (!_mFun.fxgTESValidaDatos(CodEmpresa, Contabilidad, "CONCEPTO", param.valor ?? ""))
                        {
                            vMensaje = " - C&oacute;digo de Concepto no existe o se encuentra desactivado...!";
                        }
                        break;
                    default: 
                        break;
                }
            }
            catch (Exception ex)
            {
                vMensaje = ex.Message;
            }

            return vMensaje;
        }

        /// <summary>
        /// Obtiene una lista para busqueda del valor que se va a asignar al parametro segun corresponda.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Contabilidad"></param>
        /// <param name="Parametro"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> AF_ComisionesParametros_Busqueda(int CodEmpresa, int Contabilidad, string Parametro)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<List<DropDownListaGenericaModel>>
            {
                Code = 0,
                Description = "Ok",
                Result = new List<DropDownListaGenericaModel>()
            };
            try
            {
                switch (Parametro)
                {
                    case "19": //Tesoreria Unidad
                        response = _mFun.sbgTESBusqueda(CodEmpresa, Contabilidad, "UNIDAD");
                        break;
                    case "20": //Tesoreria Centro de Costo
                        string vUnidad = _mAfi.fxgAFIParametroComision(CodEmpresa, "19");
                        response = _mFun.sbgTESBusqueda(CodEmpresa, Contabilidad, "CC", vUnidad);
                        break;
                    case "21": //Tesoreria Conceptos
                        response = _mFun.sbgTESBusqueda(CodEmpresa, Contabilidad, "CONCEPTO");
                        break;
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
