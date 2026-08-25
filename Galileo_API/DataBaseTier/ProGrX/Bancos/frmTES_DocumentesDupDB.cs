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
            var documento = (filtro.documento ?? string.Empty).Trim();
            var fechaInicio = filtro.fecha_desde.Date;
            var fechaCorte = filtro.fecha_hasta.Date.AddDays(1).AddTicks(-1);

            return DbHelper.WithConn(_portalDB, CodEmpresa, conn =>
            {
                if (filtro.todos)
                {
                    return Documentos_Duplicados_PorRango_Obtener(conn, filtro, fechaInicio, fechaCorte);
                }

                return Documentos_Duplicados_PorFiltros_Obtener(conn, filtro, documento, fechaInicio, fechaCorte);
            });
        }

        private static List<DocumentoDuplicadosLista> Documentos_Duplicados_PorFiltros_Obtener(
            System.Data.IDbConnection conn,
            TesDocumentosDuplicadosFiltros filtro,
            string documento,
            DateTime fechaInicio,
            DateTime fechaCorte)
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
                AND (@Documento = '' OR ndocumento = @Documento)";

            return conn.Query<DocumentoDuplicadosLista>(query, new
            {
                IdBanco = filtro.id_banco,
                Tipo = filtro.tipo_doc,
                FechaInicio = fechaInicio,
                FechaCorte = fechaCorte,
                Documento = documento
            }).ToList();
        }

        private static List<DocumentoDuplicadosLista> Documentos_Duplicados_PorRango_Obtener(
            System.Data.IDbConnection conn,
            TesDocumentosDuplicadosFiltros filtro,
            DateTime fechaInicio,
            DateTime fechaCorte)
        {
            const string query = @"
            WITH DocumentosFiltrados AS
            (
                SELECT 
                    nsolicitud,
                    id_banco,
                    ndocumento,
                    monto,
                    fecha_emision,
                    beneficiario,
                    estado_asiento,
                    COUNT(*) OVER (PARTITION BY ndocumento) AS total_documentos
                FROM 
                    Tes_Transacciones
                WHERE 
                    id_banco = @IdBanco
                    AND tipo = @Tipo
                    AND ndocumento IS NOT NULL
                    AND fecha_emision BETWEEN @FechaInicio AND @FechaCorte
            )
            SELECT
                nsolicitud,
                id_banco,
                ndocumento,
                monto,
                fecha_emision,
                beneficiario,
                estado_asiento
            FROM
                DocumentosFiltrados
            WHERE
                total_documentos > 1";

            return conn.Query<DocumentoDuplicadosLista>(query, new
            {
                IdBanco = filtro.id_banco,
                Tipo = filtro.tipo_doc,
                FechaInicio = fechaInicio,
                FechaCorte = fechaCorte
            }).ToList();
        }
    }
}
