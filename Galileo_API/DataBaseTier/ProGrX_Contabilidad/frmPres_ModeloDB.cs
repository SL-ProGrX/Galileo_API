using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.VisualBasic;
using PgxAPI.Models;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.PRES;

namespace PgxAPI.DataBaseTier
{
    public class frmPres_ModeloDB
    {
        private readonly IConfiguration _config;

        public frmPres_ModeloDB(IConfiguration config)
        {
            _config = config;
        }


        /// <summary>
        /// Obtener las contabilidades por empresa
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<CntxCData>> CntxContabilidades_Obtener(int CodEmpresa)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var resp = new ErrorDto<List<CntxCData>>
            {
                Code = 0,
                Result = new List<CntxCData>()
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"select cod_contabilidad as 'IdX', Nombre as 'ItmX' from CNTX_Contabilidades order by cod_Contabilidad";
                    resp.Result = connection.Query<CntxCData>(query).ToList();
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
                resp.Result = null;
            }
            return resp;
        }


        /// <summary>
        /// Obtiene cierres
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="CodContab"></param>
        /// <returns></returns>
        public ErrorDto<List<CntxCData>> CntxCierres_Obtener(int CodEmpresa, int CodContab)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var resp = new ErrorDto<List<CntxCData>>
            {
                Code = 0,
                Result = new List<CntxCData>()
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"select ID_CIERRE as 'IdX',DESCRIPCION as 'ItmX' From CNTX_CIERRES Where COD_CONTABILIDAD = '{CodContab}' order by INICIO_ANIO desc";
                    resp.Result = connection.Query<CntxCData>(query).ToList();
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
                resp.Result = null;
            }
            return resp;
        }


        /// <summary>
        /// Obtener Modelo
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="CodModelo"></param>
        /// <param name="CodContab"></param>
        /// <returns></returns>
        public ErrorDto<PresModeloData> Pres_Modelo_Obtener(int CodEmpresa, string CodModelo, int CodContab)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var resp = new ErrorDto<PresModeloData>
            {
                Code = 0,
                Result = new PresModeloData()
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"exec spPres_ModelosConsulta '{CodModelo}', '{CodContab}'";
                    resp.Result = connection.Query<PresModeloData>(query).FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
                resp.Result = null;
            }

            return resp;
        }


        /// <summary>
        /// Hacer scroll en los modelos
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="scrollValue"></param>
        /// <param name="CodModelo"></param>
        /// <param name="CodContab"></param>
        /// <returns></returns>
        public ErrorDto<PresModeloData> Pres_Modelo_scroll(int CodEmpresa, int scrollValue, string? CodModelo, int CodContab)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var resp = new ErrorDto<PresModeloData>
            {
                Code = 0,
                Result = new PresModeloData()
            };
            try
            {
                string filtro = $"where cod_contabilidad = '{CodContab}' ";

                if (scrollValue == 1)
                {
                    filtro += $"and COD_MODELO > '{CodModelo}' order by COD_MODELO asc";
                }
                else
                {
                    filtro += $"and COD_MODELO < '{CodModelo}' order by COD_MODELO desc";
                }

                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"select Top 1 COD_MODELO from PRES_MODELOS {filtro}";
                    resp.Result = connection.Query<PresModeloData>(query).FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
                resp.Result = null;
            }
            return resp;
        }


        /// <summary>   
        /// Lista de Modelos    
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="CodContab"></param>
        /// <returns></returns>
        public ErrorDto<List<PresModeloData>> Pres_Modelos_Lista(int CodEmpresa, int CodContab)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var resp = new ErrorDto<List<PresModeloData>>
            {
                Code = 0,
                Result = new List<PresModeloData>()
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                var query = @"
                            SELECT COD_MODELO, Descripcion 
                            FROM PRES_MODELOS 
                            WHERE COD_CONTABILIDAD = @CodContab 
                            ORDER BY COD_MODELO";

                var parametros = new { CodContab };
                var result = connection.Query<PresModeloData>(query, parametros);

                resp.Result = result.ToList();
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
                resp.Result = null;
            }

            return resp;
        }


        /// <summary>
        /// Insertar Modelo
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto Pres_Modelo_Insertar(int CodEmpresa, PresModeloInsert request)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var resp = new ErrorDto
            {
                Code = 0,
                Description = "Ok"
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"exec spPres_ModelosRegistra '{request.Cod_Modelo}', {request.Cod_Contabilidad}, 
                        {request.ID_Cierre}, '{request.Descripcion}', '{request.Notas}', '{Strings.Mid(request.Estado, 1, 1)}', '{request.Usuario}'";

                    connection.Execute(query);
                    resp.Description = "Información guardada satisfactoriamente...";
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
        /// Mapea Cuentas sin Centro Costo
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="CodModelo"></param>
        /// <param name="CodContab"></param>
        /// <param name="Usuario"></param>
        /// <returns></returns>
        public ErrorDto Pres_MapeaCuentasSinCentroCosto_SP(int CodEmpresa, string CodModelo, int CodContab, string Usuario)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            var resp = new ErrorDto
            {
                Code = 0,
                Description = "Ok"
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"exec spPres_MapeaCuentasSinCentroCosto '{CodModelo}',{CodContab},'{Usuario}'";

                    connection.Execute(query);
                    resp.Description = "Revisión de Mapeo de Cuentas sin Centro de Costo, realizado satisfactoriamente!";
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
        /// Reiniciar Modelo
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="CodModelo"></param>
        /// <returns></returns>
        public ErrorDto Pres_Model_Reiniciar(int CodEmpresa, string CodModelo)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            ErrorDto resp = new ErrorDto
            {
                Code = 0,
                Description = "Ok"
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"Delete PRES_PRESUPUESTO where COD_MODELO = '{CodModelo}'";

                    var query2 = $@"Delete PRES_PRESUPUESTO_AJUSTES where COD_MODELO = '{CodModelo}'";

                    connection.Execute(query);
                    connection.Execute(query2);
                    resp.Description = "Modelo de Presupuesto inicializado, vuelva a cargar las cuentas!";
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
        /// Obtiene los usuarios y ajustes de un modelo
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="CodModelo"></param>
        /// <param name="CodContab"></param>
        /// <returns></returns>
        public ErrorDto<List<PressModeloUsuarios>> Pres_Modelo_Usuarios_SP(int CodEmpresa, string CodModelo, int CodContab)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var resp = new ErrorDto<List<PressModeloUsuarios>>
            {
                Code = 0,
                Result = new List<PressModeloUsuarios>()
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"exec spPres_Modelo_Usuarios {CodContab},'{CodModelo}'";
                    resp.Result = connection.Query<PressModeloUsuarios>(query).ToList();
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
                resp.Result = null;
            }
            return resp;
        }


        /// <summary>
        /// Obtiene los ajustes de un modelo
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="CodModelo"></param>
        /// <param name="CodContab"></param>
        /// <returns></returns>
        public ErrorDto<List<PressModeloAjustes>> Pres_Modelo_Ajustes_SP(int CodEmpresa, string CodModelo, int CodContab)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var resp = new ErrorDto<List<PressModeloAjustes>>
            {
                Code = 0,
                Result = new List<PressModeloAjustes>()
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"exec spPres_Modelo_Ajustes {CodContab},'{CodModelo}'";
                    resp.Result = connection.Query<PressModeloAjustes>(query).ToList();
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
                resp.Result = null;
            }
            return resp;
        }


        /// <summary>       
        /// Obtiene los ajustes y usuarios autorizados de un modelo
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="CodModelo"></param>
        /// <param name="CodContab"></param>
        /// <returns></returns>
        public ErrorDto<List<PressModeloAjustes>> Pres_Modelo_Ajustes_Autorizados_SP(int CodEmpresa, string CodModelo, int CodContab)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var resp = new ErrorDto<List<PressModeloAjustes>>
            {
                Code = 0,
                Result = new List<PressModeloAjustes>()
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"exec spPres_Modelo_Ajustes_Autorizados {CodContab},'{CodModelo}'";
                    resp.Result = connection.Query<PressModeloAjustes>(query).ToList();
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
                resp.Result = null;
            }
            return resp;
        }


        /// <summary>
        /// Obtiene los usuarios autorizados de un modelo
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="CodModelo"></param>    
        public ErrorDto<List<PressModeloUsuarios>> Pres_Modelo_Usuarios_Autorizados_SP(int CodEmpresa, string CodModelo, int CodContab)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var resp = new ErrorDto<List<PressModeloUsuarios>>
            {
                Code = 0,
                Result = new List<PressModeloUsuarios>()
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"exec spPres_Modelo_Usuarios_Autorizados {CodContab},'{CodModelo}'";
                    resp.Result = connection.Query<PressModeloUsuarios>(query).ToList();
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
                resp.Result = null;
            }
            return resp;
        }


        /// <summary>
        /// Obtiene los ajustes y usuarios de un modelo
        /// </summary>
        /// <param name="CodEmpresa"></param>   
        /// <param name="CodModelo"></param>
        /// <param name="CodContab"></param>
        /// <param name="Usuario"></param>
        /// <returns></returns>
        public ErrorDto<List<PressModeloAjustes>> Pres_Modelo_AjUs_Ajustes_SP(int CodEmpresa, string CodModelo, int CodContab, string Usuario)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var resp = new ErrorDto<List<PressModeloAjustes>>
            {
                Code = 0,
                Result = new List<PressModeloAjustes>()
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"exec spPres_Modelo_AjUs_Ajustes {CodContab},'{CodModelo}','{Usuario}'";
                    resp.Result = connection.Query<PressModeloAjustes>(query).ToList();
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
                resp.Result = null;
            }
            return resp;
        }


        /// <summary>
        /// Obtiene los usuarios y ajustes de un modelo
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="CodModelo"></param>
        /// <param name="CodContab"></param>
        /// <param name="CodAjuste"></param>
        /// <returns></returns>
        public ErrorDto<List<PressModeloUsuarios>> Pres_Modelo_AjUs_Usuarios_SP(int CodEmpresa, string CodModelo, int CodContab, string CodAjuste)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var resp = new ErrorDto<List<PressModeloUsuarios>>
            {
                Code = 0,
                Result = new List<PressModeloUsuarios>()
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"exec spPres_Modelo_AjUs_Usuarios {CodContab},'{CodModelo}','{CodAjuste}'";
                    resp.Result = connection.Query<PressModeloUsuarios>(query).ToList();
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
                resp.Result = null;
            }
            return resp;
        }


        /// <summary>
        /// Ajuste Modelo
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto Pres_Modelo_AjUs_Registro_SP(int CodEmpresa, PressModeloAjUsRegistro request)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var resp = new ErrorDto
            {
                Code = 0
            };
            int activoValue;

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    if (request.Activo != true)
                    {
                        activoValue = 0;
                    }
                    else
                    {
                        activoValue = 1;
                    }

                    var query = $@"exec spPres_Modelo_AjUs_Registro {request.CodContab},'{request.CodModelo}','{request.Cod_Ajuste}'
                        ,'{request.Usuario}','{request.UsuarioReg}','{activoValue}'";

                    connection.Execute(query);
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
        /// Ajuste Modelo
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto Pres_Modelo_Ajustes_Registro_SP(int CodEmpresa, PressModeloAjUsRegistro request)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var resp = new ErrorDto
            {
                Code = 0
            };
            int activoValue;

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    if (request.Activo != true)
                    {
                        activoValue = 0;
                    }
                    else
                    {
                        activoValue = 1;
                    }

                    var query = $@"exec spPres_Modelo_Ajustes_Registro {request.CodContab},'{request.CodModelo}'
                        ,'{request.Cod_Ajuste}','{request.UsuarioReg}','{activoValue}'";

                    connection.Execute(query);
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
        /// Usuario Modelo Registro
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto Pres_Modelo_Usuarios_Registro_SP(int CodEmpresa, PressModeloAjUsRegistro request)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var resp = new ErrorDto
            {
                Code = 0
            };
            int activoValue;

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    if (request.Activo != true)
                    {
                        activoValue = 0;
                    }
                    else
                    {
                        activoValue = 1;
                    }

                    var query = $@"exec spPres_Modelo_Usuarios_Registro {request.CodContab},'{request.CodModelo}'
                        ,'{request.Usuario}','{request.UsuarioReg}','{activoValue}'";

                    connection.Execute(query);
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
        /// Eliminar Modelo
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="CodModelo"></param>
        /// <returns></returns>
        public ErrorDto Pres_Model_Eliminar(int CodEmpresa, string CodModelo)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var resp = new ErrorDto
            {
                Code = 0
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"Delete PRES_MODELOS where COD_MODELO = '{CodModelo}'";

                    connection.Execute(query);
                    resp.Description = "Modelo eliminado satisfactoriamente.";
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }
            return resp;
        }

    }
}