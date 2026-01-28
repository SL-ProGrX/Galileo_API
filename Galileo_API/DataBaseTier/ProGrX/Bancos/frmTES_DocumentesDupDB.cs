using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Bancos;
using Newtonsoft.Json;

namespace Galileo_API.DataBaseTier
{
    public class FrmTesDocumentesDupDB
    {

        private readonly PortalDB _portalDB;

        public FrmTesDocumentesDupDB(IConfiguration config)
        {
            _portalDB = new PortalDB(config);
        }

        /// <summary>
        /// Obtiene la cuenta de los bancos
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaBancos>> Tes_Bancos_Obtener(int CodEmpresa)
        {
            return DbHelper.WithConn(_portalDB, CodEmpresa, conn =>
            {
                const string query = "SELECT id_banco, descripcion FROM Tes_Bancos WHERE estado = 'A'";

                return conn.Query<DropDownListaBancos>(query).ToList();
            });
        }

        /// <summary>
        /// Obtiene los tipos de documentos de los bancos
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="cod_Banco"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaTipos>> Tes_Tipos_Obtener(int CodEmpresa, string cod_Banco)
        {
            return DbHelper.WithConn(_portalDB, CodEmpresa, conn =>
            {
                const string query = @"
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

                return conn.Query<DropDownListaTipos>(query, new { CodBanco = cod_Banco }).ToList();
            });
        }

        /// <summary>
        /// Obtiene los documentos duplicados
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Cod_banco"></param>
        /// <returns></returns>
        public ErrorDto<List<DocumentoDuplicadosLista>> Documentos_Duplicados_Obtener(int CodEmpresa, string filtros)
        {
            TesDocumentosDuplicadosFiltros filtro = JsonConvert.DeserializeObject<TesDocumentosDuplicadosFiltros>(filtros) ?? new TesDocumentosDuplicadosFiltros();

            return DbHelper.WithConn(_portalDB, CodEmpresa, conn =>
            {
                const string query = @"
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

                return conn.Query<DocumentoDuplicadosLista>(query, new
                {
                    IdBanco = filtro.id_banco,
                    Tipo = filtro.tipo_doc,
                    FechaInicio = filtro.fecha_desde,
                    FechaCorte = filtro.fecha_hasta,
                    Documento = filtro.documento
                }).ToList();
            });
        }
    }
}