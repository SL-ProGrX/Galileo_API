using Dapper;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using PgxAPI.Models;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.ProGrX.Bancos;


namespace PgxAPI.DataBaseTier
{
    public class frmTES_GeneraAsientosDB
    {
        private readonly IConfiguration? _config;
        private mSecurityMainDb DBBitacora;

        public frmTES_GeneraAsientosDB(IConfiguration config)
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
        public ErrorDTO<List<DropDownListaBancosGA>> Tes_Bancos_Obtener(int CodEmpresa)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDTO<List<DropDownListaBancosGA>>
            {
                Code = 0,
                Result = new List<DropDownListaBancosGA>()
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = "SELECT id_banco, descripcion FROM Tes_Bancos WHERE estado = 'A'";
                    response.Result = connection.Query<DropDownListaBancosGA>(query).ToList();
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
        /// Obtiene los tipos de documentos de los bancos
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="cod_Banco"></param>
        /// <returns></returns>
        public ErrorDTO<List<DropDownListaTiposGA>> Tes_Tipos_Obtener(int CodEmpresa, string cod_Banco)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDTO<List<DropDownListaTiposGA>>
            {
                Code = 0,
                Result = new List<DropDownListaTiposGA>()
            };

            try
            {
                using var connection = new SqlConnection(stringConn);

                string query = @"SELECT 
                                RTRIM(T.Tipo) + ' - ' + RTRIM(T.Descripcion) AS ItmY,
                                T.Tipo AS IdX,
                                RTRIM(T.Descripcion) AS ItmX
                            FROM 
                                tes_banco_docs A
                            INNER JOIN 
                                Tes_Tipos_Doc T ON A.tipo = T.tipo
                            WHERE 
                                A.ID_BANCO = @CodBanco
                                AND (
                                    (@CodBanco IN (1, 3) AND A.REG_EMISION = 1) OR
                                    (@CodBanco NOT IN (1, 3) AND A.REG_EMISION = 0))
                                        ORDER BY t.Tipo asc";

                response.Result = connection
                    .Query<DropDownListaTiposGA>(query, new { CodBanco = cod_Banco })
                    .ToList();
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
        /// Obtener información de las transacciones con asiento
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtrosTransacciones"></param>
        /// <returns></returns>
        public ErrorDTO<TablasListaGenericaModel> TES_transaccionesAsientos_Obtener(int CodEmpresa, string filtrosTransacciones, FiltrosLazyLoadData filtros)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            TES_TransaccionesFiltros filtro = JsonConvert.DeserializeObject<TES_TransaccionesFiltros>(filtrosTransacciones) ?? new TES_TransaccionesFiltros();
            var response = new ErrorDTO<TablasListaGenericaModel>
            {
                Code = 0,
                Result = new TablasListaGenericaModel()
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var queryT = @"
                                SELECT COUNT(T.nsolicitud) 
                                FROM Tes_Transacciones T 
                                INNER JOIN Tes_Bancos B ON T.id_Banco = B.id_Banco
                                WHERE
                                    T.Estado_Asiento = 'P'";

                    if (filtro.tipo_mov == "1")
                    {
                        queryT += " AND T.Estado IN ('T', 'I')";
                    }
                    else
                    {
                        queryT += " AND T.Estado IN ('A')";
                    }

                    queryT += " AND T.Fecha_Emision BETWEEN @fechainicio AND @fechafin";

                    if (!filtro.chk_todasCuentas)
                    {
                        queryT += " AND T.id_Banco = @banco";
                    }
                    if (!filtro.chk_todosDocumentos)
                    {
                        queryT += " AND T.Tipo = @tipo";
                    }

                    response.Result.total = connection.QueryFirstOrDefault<int>(queryT, new
                    {
                        fechainicio = filtro.fecha_desde.Date.ToString("yyyy-MM-dd"),
                        fechafin = filtro.fecha_hasta.Date.AddHours(23).AddMinutes(59).AddSeconds(59).ToString("yyyy-MM-dd HH:mm:ss"),
                    banco = filtro.chk_todasCuentas ? null : filtro.cod_banco,
                        tipo = filtro.chk_todosDocumentos ? null : filtro.tipo_doc
                    });

                    var query = @"
                                SELECT 
                                    T.nsolicitud,
                                    T.ndocumento,
                                    T.monto,
                                    T.fecha_emision,
                                    T.beneficiario,
                                    T.tipo,
                                    B.descripcion AS bancodesc,
                                    SUM(T.monto) OVER () AS monto_total
                                FROM Tes_Transacciones T 
                                INNER JOIN Tes_Bancos B ON T.id_Banco = B.id_Banco
                                WHERE 
                                    T.Estado_Asiento = 'P'";

                    if (filtro.tipo_mov == "1")
                    {
                        query += " AND T.Estado IN ('T', 'I')";
                    }
                    else
                    {
                        query += " AND T.Estado IN ('A')";
                    }

                    query += " AND T.Fecha_Emision BETWEEN @fechainicio AND @fechafin";

                    if (!filtro.chk_todasCuentas)
                    {
                        query += " AND T.id_Banco = @banco";
                    }
                    if (!filtro.chk_todosDocumentos)
                    {
                        query += " AND T.Tipo = @tipo";
                    }

                    if (filtros.filtro != null && filtros.filtro != "")
                    {
                        filtros.filtro = $@" AND (
                                    T.nsolicitud LIKE '%{filtros.filtro}%'
                                    OR T.ndocumento LIKE '%{filtros.filtro}%' OR T.beneficiario LIKE '%{filtros.filtro}%')";
                        query += filtros.filtro;
                    }

                    if (filtros.pagina != null)
                    {
                        query += $@"
                                ORDER BY T.nsolicitud DESC
                                OFFSET {filtros.pagina} ROWS
                                FETCH NEXT {filtros.paginacion} ROWS ONLY";
                    }

                    response.Result.lista = connection.Query<TES_TrasladoTransaccionDTO>(query, new
                    {
                        fechainicio = filtro.fecha_desde.Date.ToString("yyyy-MM-dd"),
                        fechafin = filtro.fecha_hasta.Date.AddHours(23).AddMinutes(59).AddSeconds(59).ToString("yyyy-MM-dd HH:mm:ss"),
                        banco = filtro.chk_todasCuentas ? null : filtro.cod_banco,
                        tipo = filtro.chk_todosDocumentos ? null : filtro.tipo_doc
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


        /// <summary>
        /// Genera traslado de asientos a Contabilidad
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="trasladoLista"></param>
        /// <returns></returns>
        public ErrorDTO TES_Traslado_Generar(int CodEmpresa,string trasladoLista)
        {
            List<int> lista = JsonConvert.DeserializeObject<List<int>>(trasladoLista) ?? new List<int>();
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDTO
            {
                Code = 0
            };

            try
            {
                var querySP = "";
                using var connection = new SqlConnection(stringConn);
                {
                    
                    foreach (var solicitud in lista)
                    {
                            querySP = "exec spTES_Asientos_Traslado_Individual @nsolicitud";
                            connection.Execute(querySP, new { nsolicitud = solicitud });
                    }

                    response.Description = "Traslado procesado correctamente!";
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }
            return response;
        }

    }
}