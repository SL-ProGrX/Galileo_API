using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Bancos;
using Galileo.Models.Security;
using Newtonsoft.Json;


namespace Galileo_API.DataBaseTier
{
    public class FrmTesExplorerDB
    {
        private readonly PortalDB _portalDB;
        private readonly MSecurityMainDb DBBitacora;

        public FrmTesExplorerDB(IConfiguration config)
        {
            _portalDB = new PortalDB(config);
            DBBitacora = new MSecurityMainDb(config);
        }

        public ErrorDto Bitacora(BitacoraInsertarDto data)
        {
            return DBBitacora.Bitacora(data);
        }

        /// <summary>
        /// Obtiene la cuenta de los bancos
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<TesDropDownListaBancosExplorer>> Tes_Bancos_Obtener(int CodEmpresa)
        {
            return DbHelper.WithConn(_portalDB, CodEmpresa, conn =>
            {
                const string query = "SELECT id_banco, descripcion FROM Tes_Bancos WHERE estado = 'A'";

                return conn.Query<TesDropDownListaBancosExplorer>(query).ToList();
            });
        }

        /// <summary>
        /// Obtiene la informacion del explorer
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtrosExplorer"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDto<TablasListaGenericaModel> TES_explorer_Obtener(int CodEmpresa, string filtrosExplorer, FiltrosLazyLoadData filtros)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            var filtro = JsonConvert.DeserializeObject<TesExplorerFiltros>(filtrosExplorer) ?? new TesExplorerFiltros();
            filtros ??= new FiltrosLazyLoadData();

            try
            {
                var texto = filtros.filtro?.Trim();
                var hasFiltroTexto = !string.IsNullOrWhiteSpace(texto);
                var like = hasFiltroTexto ? $"%{texto}%" : null;

                var offset = (filtros.pagina < 0) ? 0 : filtros.pagina;
                var fetch = (filtros.paginacion <= 0) ? 50 : filtros.paginacion;

                // Si no te importa paginar siempre, puedes quitar el IF y siempre aplicar OFFSET/FETCH.
                // Aquí mantengo tu comportamiento: solo pagina si filtros.pagina != null
                var usarPaginacion = filtros.pagina > 0;

                const string sqlCount = @"
SELECT COUNT(C.nsolicitud)
FROM Tes_Transacciones C
INNER JOIN Tes_Bancos B ON C.id_banco = B.id_banco
WHERE
    C.tipo = @Tipo
    AND C.id_banco = @Id_Banco
    AND (
        -- Si Estado no matchea nada, no filtra por fechas/estado
        @Estado NOT IN ('Soli','Emit','Anul','Auto')

        OR (@Estado = 'Soli' AND C.FECHA_SOLICITUD     BETWEEN @fechainicio AND @fechafin AND C.estado IN ('P'))
        OR (@Estado = 'Emit' AND C.FECHA_EMISION      BETWEEN @fechainicio AND @fechafin AND C.estado IN ('I','T'))
        OR (@Estado = 'Anul' AND C.FECHA_ANULA        BETWEEN @fechainicio AND @fechafin AND C.estado IN ('A'))
        OR (@Estado = 'Auto' AND C.FECHA_AUTORIZACION BETWEEN @fechainicio AND @fechafin AND C.estado IN ('P'))
    )
    AND (
        @FiltroTexto IS NULL
        OR CAST(C.nsolicitud AS NVARCHAR(50)) LIKE @like
        OR CAST(C.ndocumento AS NVARCHAR(50)) LIKE @like
        OR C.beneficiario LIKE @like
    );";

                const string sqlList = @"
SELECT
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
FROM Tes_Transacciones C
INNER JOIN Tes_Bancos B ON C.id_banco = B.id_banco
WHERE
    C.tipo = @Tipo
    AND C.id_banco = @Id_Banco
    AND (
        @Estado NOT IN ('Soli','Emit','Anul','Auto')

        OR (@Estado = 'Soli' AND C.FECHA_SOLICITUD     BETWEEN @fechainicio AND @fechafin AND C.estado IN ('P'))
        OR (@Estado = 'Emit' AND C.FECHA_EMISION      BETWEEN @fechainicio AND @fechafin AND C.estado IN ('I','T'))
        OR (@Estado = 'Anul' AND C.FECHA_ANULA        BETWEEN @fechainicio AND @fechafin AND C.estado IN ('A'))
        OR (@Estado = 'Auto' AND C.FECHA_AUTORIZACION BETWEEN @fechainicio AND @fechafin AND C.estado IN ('P'))
    )
    AND (
        @FiltroTexto IS NULL
        OR CAST(C.nsolicitud AS NVARCHAR(50)) LIKE @like
        OR CAST(C.ndocumento AS NVARCHAR(50)) LIKE @like
        OR C.beneficiario LIKE @like
    )
ORDER BY C.nsolicitud DESC
OFFSET (CASE WHEN @UsarPaginacion = 1 THEN @offset ELSE 0 END) ROWS
FETCH NEXT (CASE WHEN @UsarPaginacion = 1 THEN @fetch ELSE 2147483647 END) ROWS ONLY;";

                var parameters = new
                {
                    Estado = filtro.estado,
                    Tipo = filtro.tipo_doc,
                    Id_Banco = filtro.cod_banco,
                    fechainicio = filtro.fecha_desde,
                    fechafin = filtro.fecha_hasta,

                    FiltroTexto = hasFiltroTexto ? texto : null,
                    like,

                    UsarPaginacion = usarPaginacion ? 1 : 0,
                    offset,
                    fetch
                };

                var resultModel = new TablasListaGenericaModel
                {
                    total = conn.QuerySingle<int>(sqlCount, parameters),
                    lista = conn.Query<TesListaExplorerDto>(sqlList, parameters).ToList()
                };

                return DbHelper.CreateOkResponse(resultModel);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<TablasListaGenericaModel>($"Error al obtener los datos del explorer: {ex.Message}");
            }
        }

    }
}