using Dapper;
using Galileo.Models.AF;
using Galileo.Models.ERROR;
using Newtonsoft.Json;

namespace Galileo.DataBaseTier.ProGrX_Beneficios
{
    /// <summary>
    /// Acceso a datos de Remesas de Archivo de Beneficios (frmAF_BeneRemesasArchivo).
    /// Consultas aquí; persistencia en el parcial .Guardar.
    /// </summary>
    public partial class FrmAfBeneRemesasArchivoDB
    {
        private const string FechaFormat = "yyyy/MM/dd";
        private const int DepartamentoOrigenRemesa = 38;
        private const int EstadoRemesa = 6;

        private readonly IConfiguration _config;

        /// <summary>
        /// Inicializa el acceso a datos con la configuración inyectada.
        /// </summary>
        /// <param name="config">Configuración de la aplicación.</param>
        public FrmAfBeneRemesasArchivoDB(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        /// <summary>
        /// Crea una instancia de acceso al portal usando la configuración inyectada.
        /// </summary>
        private PortalDB CreatePortalDb() => new(_config);

        /// <summary>
        /// Obtiene los tipos de documento activos para remesas.
        /// </summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <returns>Lista de tipos de documento.</returns>
        public ErrorDto<List<TipoDocumentosLista>> TipoDocumentos_Obtener(int CodCliente)
        {
            return DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
            {
                const string sql = @"SELECT IdTipoDocumento AS item, Nombre AS descripcion
                                     FROM RMS_TiposDocumentos WHERE ACTIVO = 1";
                return connection.Query<TipoDocumentosLista>(sql).ToList();
            });
        }

        /// <summary>
        /// Obtiene las remesas de archivo en estado de origen configurado.
        /// </summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <returns>Lista de remesas y total.</returns>
        public ErrorDto<RmsRemesasDataLista> RemesasArchivo_Obtener(int CodCliente)
        {
            return DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
            {
                var response = new RmsRemesasDataLista();

                const string sql = @"SELECT * FROM RMS_Remesas
                                     WHERE CodDepartamentoOrigen = @origen AND IdEstado = @estado";
                response.lista = connection.Query<RmsRemesasData>(sql,
                    new { origen = DepartamentoOrigenRemesa, estado = EstadoRemesa }).ToList();
                response.total = response.lista.Count;
                return response;
            });
        }

        /// <summary>
        /// Obtiene los documentos de beneficios elegibles para remesa según categoría y rango de fechas.
        /// </summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <param name="filtros">JSON con categoría y rango de fechas.</param>
        /// <returns>Lista de documentos.</returns>
        public ErrorDto<List<RmsRemesaDocuementos>> RemesaDocumentos_Obtener(int CodCliente, string filtros)
        {
            var rmsCarga = JsonConvert.DeserializeObject<RmsCargaFiltros>(filtros) ?? new RmsCargaFiltros();

            var fechaIni = MProGrXAuxiliarDB.validaFechaGlobal(rmsCarga.fecha_inicio, FechaFormat);
            var fechaCorte = MProGrXAuxiliarDB.validaFechaGlobal(rmsCarga.fecha_corte, FechaFormat);

            return DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
            {
                const string sql = @"SELECT abo.ID_BENEFICIO AS id_beneficio,
                                        CONCAT(FORMAT(abo.ID_BENEFICIO, '00000'), TRIM(abo.COD_BENEFICIO), FORMAT(abo.CONSEC, '00000')) AS n_expediente,
                                        abo.REGISTRA_FECHA AS registra_fecha,
                                        abo.REGISTRA_USER AS registra_user,
                                        abo.ESTADO AS estado,
                                        (SELECT descripcion FROM AFI_BENE_ESTADOS abe WHERE abe.COD_ESTADO = abo.ESTADO) AS estado_desc,
                                        abo.CEDULA AS cedula,
                                        (SELECT DISTINCT nombre FROM socios WHERE cedula = abo.CEDULA) AS nombre,
                                        '' AS notaOrigen
                                     FROM AFI_BENE_OTORGA abo
                                     WHERE CAST(abo.ID_BENEFICIO AS VARCHAR(30)) NOT IN (
                                            SELECT Documento FROM RMS_RemesasDetalle
                                            WHERE IdRemesa IN (SELECT IdRemesa FROM RMS_Remesas
                                                               WHERE CodDepartamentoOrigen = @origen AND IdEstado = @estado))
                                       AND abo.REGISTRA_FECHA BETWEEN @fechaIni AND @fechaCorte
                                       AND abo.COD_BENEFICIO IN (
                                            SELECT COD_BENEFICIO FROM AFI_BENEFICIOS ab WHERE ab.COD_CATEGORIA = @cod_categoria)";

                return connection.Query<RmsRemesaDocuementos>(sql, new
                {
                    origen = DepartamentoOrigenRemesa,
                    estado = EstadoRemesa,
                    fechaIni,
                    fechaCorte,
                    rmsCarga.cod_categoria
                }).ToList();
            });
        }

        /// <summary>
        /// Obtiene el detalle de una remesa específica.
        /// </summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <param name="IdRemesa">Identificador de la remesa.</param>
        /// <returns>Detalle de la remesa y total.</returns>
        public ErrorDto<RmsRemesasDetalleDataLista> RemesaDetalle_Obtener(int CodCliente, int IdRemesa)
        {
            return DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
            {
                var response = new RmsRemesasDetalleDataLista();

                const string sql = "SELECT * FROM RMS_RemesasDetalle WHERE IdRemesa = @IdRemesa";
                response.lista = connection.Query<RmsRemesasDetalleData>(sql, new { IdRemesa }).ToList();
                response.total = response.lista.Count;
                return response;
            });
        }
    }
}
