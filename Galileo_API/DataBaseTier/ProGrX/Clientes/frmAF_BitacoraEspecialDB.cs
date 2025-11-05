using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.ProGrX.Clientes;
using System.Collections.Generic;

namespace PgxAPI.DataBaseTier.ProGrX.Clientes
{
    public class frmAF_BitacoraEspecialDB
    {
        private readonly IConfiguration? _config;
        private readonly int vModulo = 1;

        public frmAF_BitacoraEspecialDB(IConfiguration? config)
        {
            _config = config;
        }

        /// <summary>
        /// Metodo para obtener los tipos de movimiento de la bitacora especial
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDTO<List<DropDownListaGenericaModel>> AF_BitacoraEspecialMov_Obtener(int CodEmpresa)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDTO<List<DropDownListaGenericaModel>>
            {
                Code = 0,
                Description = "Ok",
                Result = new List<DropDownListaGenericaModel>()
            };
            try
            {

                using var connection = new SqlConnection(stringConn);
                {
                    var query = @$"select MOVIMIENTO as 'item',DESCRIPCION from US_MOVIMIENTOS_BE WHERE MODULO = {vModulo} ORDER BY MOVIMIENTO ";
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
        /// Metodo para revisar los registros de la bitacora especial
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="bitacora"></param>
        /// <returns></returns>
        public ErrorDTO AF_BitacoraEspecial_Revisar(int CodEmpresa, string usuario, List<AF_BitacoraEspecialData> bitacora )
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDTO
            {
                Code = 0,
                Description = "Ok",
            };
            try
            {

                using var connection = new SqlConnection(stringConn);
                {
                    foreach (var item in bitacora)
                    {
                        var query = @$"update AFI_BITACORA_ESPECIAL set revisado_usuario = '{usuario}', revisado_fecha = dbo.MyGetdate()  where id_Bitacora = {item.id_bitacora} ";
                        connection.Execute(query);
                    }
                }

            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
            }
            return result;
        }

        /// <summary>
        /// Metodo para obtener las busquedas de la bitacora especial
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="campo"></param>
        /// <returns></returns>
        public ErrorDTO<List<DropDownListaGenericaModel>> AF_BitacoraEspecialBusquedas_Obtener(int CodEmpresa, string campo)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDTO<List<DropDownListaGenericaModel>>
            {
                Code = 0,
                Description = "Ok",
                Result = new List<DropDownListaGenericaModel>()
            };
            try
            {
                var query = "";
                using var connection = new SqlConnection(stringConn);
                {
                    if(campo.ToUpper() == "SOCIOS")
                    {
                        query = @$"Select Cedula as 'item',Nombre as 'descripcion' From Socios order by Nombre";
                    }
                    else
                    {
                        query = @$"Select Nombre as 'item', Descripcion  From  Usuarios order by Nombre";
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
        /// Metodo para obtener las entradas de la bitacora especial segun los filtros aplicados.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDTO<List<AF_BitacoraEspecialData>> AF_BitacoraEspecial_Obtener(int CodEmpresa, AF_BitacoraEspecialFiltros filtros)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDTO<List<AF_BitacoraEspecialData>>
            {
                Code = 0,
                Description = "Ok",
                Result = new List<AF_BitacoraEspecialData>()
            };
            try
            {

                using var connection = new SqlConnection(stringConn);
                {
                    var query = @$" select C.*,S.cedula,S.nombre,M.Descripcion as MovimientoDesc, case when C.revisado_fecha is null then 0 else 1 end as 'Revisado'
		                                from Afi_Bitacora_especial C inner join  Socios S on S.cedula = C.cedula
		                                inner join US_MOVIMIENTOS_BE M on C.Movimiento = M.Movimiento
		                                Where M.Modulo = {vModulo}";
                   
                    string whereClause = " ";

                    if(!String.IsNullOrEmpty(filtros.cedula) && filtros.cedula.Length > 0)
                    {
                        whereClause += $" and C.cedula like  '%{filtros.cedula}%' ";
                    }


                    if(filtros.chkFechas != null && filtros.chkFechas == false)
                    {
                        if (filtros.chkRevisados == true)
                        {
                            whereClause += $" and C.Revisado_fecha between '{filtros.fecha_inicio:yyyy-MM-dd} 00:00:00' and '{filtros.fecha_corte:yyyy-MM-dd} 23:59:00' ";
                        }
                        else
                        {
                            whereClause += $" and C.fecha between '{filtros.fecha_inicio:yyyy-MM-dd} 00:00:00' and '{filtros.fecha_corte:yyyy-MM-dd} 23:59:00' ";
                        }
                    }

                    //'Lista de Tipos de Movimientos
                    string vCadena = "";

                    if(filtros.movimientos.Count > 0)
                    {
                        vCadena = " and C.Movimiento in (";
                    }

                    foreach (var item in filtros.movimientos)
                    {
                        vCadena += $@"'{item.item}',";
                    }
                    vCadena = vCadena.TrimEnd(',') + ") ";

                    whereClause += vCadena;

                    if (filtros.chkUsuario == false)
                    {
                        if(filtros.usuario != null || filtros.usuario != "")
                        {
                            if (filtros.chkRevisados == true)
                            {
                                whereClause += $" and C.Revisado_Usuario = '{filtros.usuario}' ";
                            }
                            else
                            {
                                whereClause += $" and C.Usuario = '{filtros.cedula}' ";
                            }
                        }
                    }

                    if(filtros.revision != null)
                    {
                        switch (filtros.revision)
                        {
                            case "P": //Pendientes
                                whereClause += " and C.Revisado_Fecha is null";
                                break;
                            case "R": //Revisados
                                whereClause += " and C.Revisado_Fecha is not null";
                                break;
                            case "T": //Todos
                                //No aplica filtro
                                break;
                        }
                    }

                    string order = "";
                    if (filtros.chkRevisados == true)
                    {
                        order += $" order by C.Revisado_fecha ";
                    }
                    else
                    {
                        order += $" order by C.fecha";
                    }

                    result.Result = connection.Query<AF_BitacoraEspecialData>(query + whereClause + order).ToList();

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

    }
}
