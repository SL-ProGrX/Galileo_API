using Dapper;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using PgxAPI.Models;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.ProGrX.Bancos;

namespace PgxAPI.DataBaseTier
{
    public class frmTES_ExplorerDB
    {
        private readonly IConfiguration? _config;
        private mSecurityMainDb DBBitacora;

        public frmTES_ExplorerDB(IConfiguration config)
        {
            _config = config;
            DBBitacora = new mSecurityMainDb(_config);
        }

        public ErrorDTO Bitacora(BitacoraInsertarDTO data)
        {
            return DBBitacora.Bitacora(data);
        }

        /// <summary>
        /// Obtiene la cuenta de los bancos
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDTO<List<TES_DropDownListaBancosExplorer>> Tes_Bancos_Obtener(int CodEmpresa)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDTO<List<TES_DropDownListaBancosExplorer>>
            {
                Code = 0,
                Result = new List<TES_DropDownListaBancosExplorer>()
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = "SELECT id_banco, descripcion FROM Tes_Bancos WHERE estado = 'A'";
                    response.Result = connection.Query<TES_DropDownListaBancosExplorer>(query).ToList();
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
        /// Obtiene la informacion del explorer
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtrosExplorer"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDTO<TablasListaGenericaModel> TES_explorer_Obtener(int CodEmpresa, string filtrosExplorer, FiltrosLazyLoadData filtros)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            TES_ExplorerFiltros filtro = JsonConvert.DeserializeObject<TES_ExplorerFiltros>(filtrosExplorer) ?? new TES_ExplorerFiltros();
            var response = new ErrorDTO<TablasListaGenericaModel>
            {
                Code = 0,
                Result = new TablasListaGenericaModel()
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var queryT = @"SELECT COUNT(C.nsolicitud) 
                                    FROM 
                                        Tes_Transacciones C
                                    INNER JOIN 
                                        Tes_Bancos B ON C.id_banco = B.id_banco
                                    WHERE  C.tipo = @Tipo
                                        AND C.id_banco = @Id_Banco";


                    if (filtro.estado == "Soli")
                    {
                        queryT += " AND C.FECHA_SOLICITUD BETWEEN @fechainicio AND @fechafin and C.estado in('P')";
                    }
                    if (filtro.estado == "Emit")
                    {
                        queryT += " AND C.FECHA_EMISION BETWEEN @fechainicio AND @fechafin and C.estado in('I','T')";
                    }
                    if (filtro.estado == "Anul")
                    {
                        queryT += " AND C.FECHA_ANULA BETWEEN @fechainicio AND @fechafin and C.estado in('A')";
                    }
                    if (filtro.estado == "Auto")
                    {
                        queryT += " AND C.FECHA_AUTORIZACION BETWEEN @fechainicio AND @fechafin and C.estado in('P')";
                    }

                    response.Result.total = connection.QueryFirstOrDefault<int>(queryT, new
                    {
                        Estado = filtro.estado,
                        Tipo = filtro.tipo_doc,
                        Id_Banco = filtro.cod_banco,
                        fechainicio = filtro.fecha_desde,
                        fechafin = filtro.fecha_hasta,

                    });

                    var query = @"SELECT 
                                        C.nsolicitud,
                                        C.ndocumento,
                                        C.tipo,
                                        C.codigo,
                                        C.beneficiario,
                                        C.monto,
                                        C.fecha_solicitud,
                                        C.fecha_anula,
                                        C.fecha_emision,
                                        C.fecha_autorizacion,
                                        B.descripcion,
                                        SUM(C.monto) OVER () AS monto_total
                                    FROM 
                                        Tes_Transacciones C
                                INNER JOIN 
                                    Tes_Bancos B ON C.id_banco = B.id_banco
                                WHERE    C.tipo = @Tipo
                                        AND C.id_banco = @Id_Banco";

                    if (filtro.estado == "Soli")
                    {
                        query += " AND C.FECHA_SOLICITUD BETWEEN @fechainicio AND @fechafin and C.estado in('P')";
                    }
                    if (filtro.estado == "Emit")
                    {
                        query += " AND C.FECHA_EMISION BETWEEN @fechainicio AND @fechafin and C.estado in('I','T')";
                    }
                    if (filtro.estado == "Anul")
                    {
                        query += " AND C.FECHA_ANULA BETWEEN @fechainicio AND @fechafin and C.estado in('A')";
                    }
                    if (filtro.estado == "Auto")
                    {
                        query += " AND C.FECHA_AUTORIZACION BETWEEN @fechainicio AND @fechafin and C.estado in('P')";
                    }

                    if (filtros.filtro != null && filtros.filtro != "")
                    {
                        filtros.filtro = $@" AND (
                                    C.nsolicitud LIKE '%{filtros.filtro}%'
                                    OR C.ndocumento LIKE '%{filtros.filtro}%' OR C.beneficiario LIKE '%{filtros.filtro}%')";
                        query += filtros.filtro;
                    }

                    if (filtros.pagina != null)
                    {
                        query += $@"
                                ORDER BY C.nsolicitud DESC
                                OFFSET {filtros.pagina} ROWS
                                FETCH NEXT {filtros.paginacion} ROWS ONLY";
                    }

                    response.Result.lista = connection.Query<TES_ListaExplorerDTO>(query, new
                    {
                        Estado = filtro.estado,
                        Tipo = filtro.tipo_doc,
                        Id_Banco = filtro.cod_banco,
                        fechainicio = filtro.fecha_desde,
                        fechafin = filtro.fecha_hasta,

                    }).ToList();
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