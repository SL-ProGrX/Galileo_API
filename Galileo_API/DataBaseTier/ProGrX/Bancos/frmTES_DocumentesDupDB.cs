using Dapper;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using PgxAPI.Models;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.ProGrX.Bancos;

namespace PgxAPI.DataBaseTier
{
    public class frmTES_DocumentesDupDB
    {
        private readonly IConfiguration? _config;
        private mSecurityMainDb DBBitacora;

        public frmTES_DocumentesDupDB(IConfiguration config)
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
        public ErrorDTO<List<DropDownListaBancos>> Tes_Bancos_Obtener(int CodEmpresa)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDTO<List<DropDownListaBancos>>
            {
                Code = 0,
                Result = new List<DropDownListaBancos>()
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = "SELECT id_banco, descripcion FROM Tes_Bancos WHERE estado = 'A'";
                    response.Result = connection.Query<DropDownListaBancos>(query).ToList();
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
        public ErrorDTO<List<DropDownListaTipos>> Tes_Tipos_Obtener(int CodEmpresa, string cod_Banco)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDTO<List<DropDownListaTipos>>
            {
                Code = 0,
                Result = new List<DropDownListaTipos>()
            };

            try
            {
                using var connection = new SqlConnection(stringConn);

                string query = @"
            SELECT 
                RTRIM(T.Tipo) + ' - ' + RTRIM(T.Descripcion) AS ItmY,
                T.Tipo AS IdX,
                RTRIM(T.Descripcion) AS ItmX
            FROM 
                tes_banco_docs A
            INNER JOIN 
                Tes_Tipos_Doc T ON A.tipo = T.tipo
            WHERE 
                A.ID_BANCO = @CodBanco";

                response.Result = connection
                    .Query<DropDownListaTipos>(query, new { CodBanco = cod_Banco })
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
        /// Obtiene los documentos duplicados
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Cod_banco"></param>
        /// <returns></returns>
        public ErrorDTO<List<DocumentoDuplicadosLista>> Documentos_Duplicados_Obtener(int CodEmpresa, string filtros)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            TES_DocumentosDuplicadosFiltros filtro = JsonConvert.DeserializeObject<TES_DocumentosDuplicadosFiltros>(filtros) ?? new TES_DocumentosDuplicadosFiltros();

            var response = new ErrorDTO<List<DocumentoDuplicadosLista>>
            {
                Code = 0,
                Result = new List<DocumentoDuplicadosLista>()
            };

            try
            {
                using var connection = new SqlConnection(stringConn);

                string query = @"
            SELECT 
                nsolicitud,
                id_banco,
                ndocumento,
                monto,
                fecha_emision,
                beneficiario,
                estado_asiento
            FROM 
                Tes_Transacciones
            WHERE 
                id_banco = @IdBanco
                AND tipo = @Tipo
                AND fecha_emision BETWEEN @FechaInicio AND @FechaCorte
                AND (
                    (@Documento IS NOT NULL AND @Documento <> '' AND ndocumento = @Documento)
                    OR
                    (@Documento IS NULL OR @Documento = '') AND ndocumento IN (
                        SELECT ndocumento
                        FROM Tes_Transacciones
                        WHERE 
                            id_banco = @IdBanco
                            AND tipo = @Tipo
                            AND fecha_emision BETWEEN @FechaInicio AND @FechaCorte
                        GROUP BY ndocumento
                        HAVING COUNT(*) > 1
                    )
                )";

                response.Result = connection.Query<DocumentoDuplicadosLista>(query, new
                {
                    IdBanco = filtro.id_banco,
                    Tipo = filtro.tipo_doc,
                    FechaInicio = filtro.fecha_desde,
                    FechaCorte = filtro.fecha_hasta,
                    Documento = filtro.documento
                }).ToList();
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