using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.ProGrX.Clientes;
using System.Reflection;

namespace PgxAPI.DataBaseTier.ProGrX.Clientes
{
    public class frmAF_Telemarketing_ConsultasDB
    {
        private readonly IConfiguration _config;

        public frmAF_Telemarketing_ConsultasDB(IConfiguration config)
        {
            _config = config;
        }

        #region Colocacion

        /// <summary>
        /// Metodo para obtener las categorias de telemarketing
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> AF_Telemarketing_Categoria_Obtener(int CodEmpresa)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDto<List<DropDownListaGenericaModel>>
            {
                Code = 0,
                Description = "Ok",
                Result = new List<DropDownListaGenericaModel>()
            };
            try
            {

                using var connection = new SqlConnection(stringConn);
                {
                    var query = @$"select COD_MORA as 'item', COD_MORA as 'descripcion' From Cbr_Clasificacion_Mora";
                    result.Result = connection.Query<DropDownListaGenericaModel>(query).ToList();

                    //agrego el valor TODOS en la primera posicion
                    result.Result.Insert(0, new DropDownListaGenericaModel { item = "T", descripcion = "TODOS" });

                }

            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = null;
            }
            return result;
        }

        /// <summary>
        /// Metodo para obtener las colocaciones de telemarketing
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDto<List<AfTelemarketingColocacionData>> AF_Telemarketing_Colocacion_Obtener(int CodEmpresa, ColocacionFiltros filtros)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDto<List<AfTelemarketingColocacionData>>
            {
                Code = 0,
                Description = "Ok",
                Result = new List<AfTelemarketingColocacionData>()
            };
            try
            {
                var query = string.Empty;
                using var connection = new SqlConnection(stringConn);
                {
                    switch (filtros.fechaTipo)
                    {
                        case 1://Formalizados
                            query = $@"exec spAFI_Telemarketing_Consulta 'CRD_01'";
                            break;
                        case 2://Cancelados
                            query = $@"exec spAFI_Telemarketing_Consulta 'CRD_02'";
                            break;
                        case 3://Finalizados
                            query = $@"exec spAFI_Telemarketing_Consulta 'CRD_03'";
                            break;
                    }

                    if (filtros.chkFechas)
                    {
                        query += ", NULL, NULL";
                    }
                    else
                    {
                        query += $@", '{filtros.fechaInicio:yyyy/MM/dd} 00:00:00', '{filtros.fechaCorte:yyyy/MM/dd} 23:59:00'";
                    }

                    if (string.IsNullOrEmpty(filtros.credito))
                    {
                        query += ", NULL";
                    }
                    else
                    {
                        query += $", '{filtros.credito}' ";
                    }

                    if (string.IsNullOrEmpty(filtros.destino))
                    {
                        query += ", NULL";
                    }
                    else
                    {
                        query += $", '{filtros.destino}' ";
                    }

                    if (string.IsNullOrEmpty(filtros.producto))
                    {
                        query += ", NULL";
                    }
                    else
                    {
                        query += $", '{filtros.producto}' ";
                    }

                    if (string.IsNullOrEmpty(filtros.canal))
                    {
                        query += ", NULL";
                    }
                    else
                    {
                        query += $", '{filtros.canal}' ";
                    }

                    if (string.IsNullOrEmpty(filtros.institucion))
                    {
                        query += ", NULL";
                    }
                    else
                    {
                        query += $", '{filtros.institucion}' ";
                    }

                    int chkSinMora = 0;
                    int chkEmail = 0;
                    int chkMovil = 0;
                    foreach (var item in filtros.validaciones)
                    {
                        if (item.item == "M")
                        {
                            chkSinMora = 1;
                        }
                        else if (item.item == "E")
                        {
                            chkEmail = 1;
                        }
                        else if (item.item == "T")
                        {
                            chkMovil = 1;
                        }
                    }
                    query += $", {chkSinMora}, {chkEmail}, {chkMovil}, {filtros.mFecUltMovUpdate}";

                    if(filtros.categoria == "T")
                    {
                        query += ", NULL";
                    }
                    else
                    {
                        query += $", '{filtros.categoria}' ";
                    }

                    if (string.IsNullOrEmpty(filtros.gyp))
                    {
                        query += ", NULL";
                    }
                    else
                    {
                        query += $", '{filtros.gyp}' ";
                    }

                    result.Result = connection.Query<AfTelemarketingColocacionData>(query).ToList();

                }
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = null;
            }
            return result;
        }

        /// <summary>
        /// Metodo para obtener los catalogos de telemarketing
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="tipo"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> AF_Telemarketing_Catalogos_Obtener(int CodEmpresa, string tipo)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDto<List<DropDownListaGenericaModel>>
            {
                Code = 0,
                Description = "Ok",
                Result = new List<DropDownListaGenericaModel>()
            };
            try
            {
                var query = string.Empty;
                using var connection = new SqlConnection(stringConn);
                {
                    switch (tipo)
                    {
                        case "Actividad":
                            query = "Select COD_ACTIVIDAD as 'item',DESCRIPCION From AFI_ACTIVIDADES_ECO ORDER BY DESCRIPCION";
                            break;
                        case "Canal":
                            query = "Select CANAL_TIPO as 'item',DESCRIPCION From AFI_CANALES_TIPOS ORDER BY DESCRIPCION";
                            break;
                        case "Destino":
                            query = "Select COD_DESTINO as 'item',DESCRIPCION From CATALOGO_DESTINOS ORDER BY DESCRIPCION";
                            break;
                        case "Institucion":
                            query = "Select COD_INSTITUCION as 'item',DESCRIPCION From INSTITUCIONES ORDER BY DESCRIPCION";
                            break;
                        case "Preferencias":
                            query = "Select COD_PREFERENCIA as 'item',DESCRIPCION From AFI_PREFERENCIAS ORDER BY DESCRIPCION";
                            break;
                        case "Linea":
                            query = "Select CODIGO as 'item',DESCRIPCION From CATALOGO WHERE LINEA_INTERNA = 1 AND RETENCION = 'N' AND POLIZA = 'N' ORDER BY DESCRIPCION";
                            break;
                    }

                    result.Result = connection.Query<DropDownListaGenericaModel>(query).ToList();
                }
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = null;
            }
            return result;
        }

        #endregion

        #region Clientes

        /// <summary>
        /// Metodo para obtener las lineas de telemarketing
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="combo"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> AF_Telemarketing_Lineas_Obtener(int CodEmpresa, int combo)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDto<List<DropDownListaGenericaModel>>
            {
                Code = 0,
                Description = "Ok",
                Result = new List<DropDownListaGenericaModel>()
            };
            try
            {

                using var connection = new SqlConnection(stringConn);
                {
                    var query = @$"Select Codigo as 'item', Codigo +' - '+ Descripcion as 'descripcion' from Catalogo where (codigo like '%%' or descripcion like '%%')";

                    if(combo == 1)
                    {
                        query += " and Poliza = 'N' and Retencion = 'N'";
                    }
                    else
                    {
                        query += "  and Poliza = 'S' or Retencion = 'S'";
                    }

                    result.Result = connection.Query<DropDownListaGenericaModel>(query).ToList();
                }

            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = null;
            }
            return result;
        }

        /// <summary>
        /// Metodo para obtener los clientes de telemarketing
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDto<List<AfTelemarketingClientesData>> AF_Telemarketing_Clientes_Obtener(int CodEmpresa, ClientesFiltros filtros)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDto<List<AfTelemarketingClientesData>>
            {
                Code = 0,
                Description = "Ok",
                Result = new List<AfTelemarketingClientesData>()
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    //Registrando Filtros
                    var query = @$"delete SYS_REPORT_PIVOT_01 where usuario = '{filtros.usuario}'";
                    connection.Execute(query);

                    //recorro las lineas seleccionadas
                    foreach (var linea in filtros.lineas)
                    {
                        query = @$"insert SYS_REPORT_PIVOT_01(USUARIO,CODIGO,REGISTRO_FECHA,COD_REPORTE) 
                                 values ('{filtros.usuario}','{linea.item}',getdate(),'MKD_Clc')";
                        connection.Execute(query);
                    }

                    //recorro los codigos seleccionados
                    foreach (var codigo in filtros.codigos)
                    {
                        query = @$"insert SYS_REPORT_PIVOT_01(USUARIO,CODIGO,REGISTRO_FECHA,COD_REPORTE) 
                                 values ('{filtros.usuario}','{codigo.item}',getdate(),'MKD_Cod')";
                        connection.Execute(query);
                    }

                    int chkIntegral = filtros.chkAnalisis == true ? 1 : 0;

                    query = $@"exec spMKD_ClientesComun '{filtros.usuario}', {chkIntegral}";

                    result.Result = connection.Query<AfTelemarketingClientesData>(query).ToList();

                }
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = null;
            }
            return result;
        }


        /// <summary>
        /// Metodo para obtener el detalle de los clientes de telemarketing
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDto<List<AfTelemarketingClientesDetalleData>> AF_Telemarketing_ClientesDetalle_Obtener(int CodEmpresa, string vCadena, string usuario)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDto<List<AfTelemarketingClientesDetalleData>>
            {
                Code = 0,
                Description = "Ok",
                Result = new List<AfTelemarketingClientesDetalleData>()
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    //Registrando Filtros
                    var query = @$"exec spMKD_ClientesComun_Detalle '{vCadena}', '{usuario}'";
                    result.Result = connection.Query<AfTelemarketingClientesDetalleData>(query).ToList();

                }
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = null;
            }
            return result;
        }

        #endregion

        #region Contactos
        /// <summary>
        /// Metodo para obtener los estados de las personas
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> AF_Telemarketing_EstadosPer_Obtener(int CodEmpresa)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDto<List<DropDownListaGenericaModel>>
            {
                Code = 0,
                Description = "Ok",
                Result = new List<DropDownListaGenericaModel>()
            };
            try
            {

                using var connection = new SqlConnection(stringConn);
                {
                    var query = @$"select COD_ESTADO as 'item', DESCRIPCION from AFI_ESTADOS_PERSONA";
                    result.Result = connection.Query<DropDownListaGenericaModel>(query).ToList();

                    //agrego el valor TODOS en la primera posicion
                    result.Result.Insert(0, new DropDownListaGenericaModel { item = "T", descripcion = "TODOS" });
                }

            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = null;
            }
            return result;
        }

        /// <summary>
        /// Metodo para obtener los contactos de telemarketing
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDto<List<AfTelemarketingContactoData>> AF_Telemarketing_Contacto_Obtener(int CodEmpresa, ContactosFiltros filtros)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDto<List<AfTelemarketingContactoData>>
            {
                Code = 0,
                Description = "Ok",
                Result = new List<AfTelemarketingContactoData>()
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    
                    var query = @$"exec spAFI_Telemarketing_Contactos";

                    if(filtros.fechaTipo == "T")
                    {
                        query += " NULL, NULL, 'T'";
                    }
                    else
                    {
                       query += $@" '{filtros.fechaInicio:yyyy/MM/dd} 00:00:00', '{filtros.fechaCorte:yyyy/MM/dd} 23:59:00', '{filtros.fechaTipo}'";
                    }

                    if (filtros.estado == "T")
                    {
                        query += ", NULL";
                    }
                    else
                    {
                        query += $", '{filtros.estado}' ";
                    }

                    result.Result = connection.Query<AfTelemarketingContactoData>(query).ToList();

                }
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = null;
            }
            return result;
        }

        #endregion

    }
}
