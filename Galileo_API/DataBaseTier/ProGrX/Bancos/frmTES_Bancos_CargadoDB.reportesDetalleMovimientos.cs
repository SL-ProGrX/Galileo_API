using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Bancos;
using System.Data;

namespace Galileo_API.DataBaseTier.ProGrX.Bancos
{
    public partial class FrmTesBancosCargadoDB
    {
        public ErrorDto<List<TesReporteDetalleMovimientosBancariosDto>>
            TES_BancosCargado_ReporteDetalleMovimientosBancarios_Obtener(
                int CodEmpresa,
                TesReporteDetalleMovimientosRequest request)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                var result = conn.Query<TesReporteDetalleMovimientosBancariosDto>(
                    "dbo.spTesReporteDetalleMovimientosBancarios",
                    new
                    {
                        pIdBanco = request.IdBanco.GetValueOrDefault(0),
                        pTipoMovimiento = Normalizar(request.TipoMovimiento, "T"),
                        FechaDesde = request.FechaDesde.Date,
                        pFechaHasta = request.FechaHasta.Date,
                        pOrdenacion = Normalizar(request.Ordenacion, "FA"),
                        pEstado = Normalizar(request.Estado, "T"),
                        pListaConceptos = request.ListaConceptos ?? string.Empty
                    },
                    commandType: CommandType.StoredProcedure)
                    .ToList();

                return DbHelper.CreateOkResponse(result);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<List<TesReporteDetalleMovimientosBancariosDto>>(ex.Message);
            }
        }

        public ErrorDto<List<TesReporteDetalleMovimientosRegistradosDto>>
            TES_BancosCargado_ReporteDetalleMovimientosRegistrados_Obtener(
                int CodEmpresa,
                TesReporteDetalleMovimientosRequest request)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                var result = conn.Query<TesReporteDetalleMovimientosRegistradosDto>(
                    "dbo.spTesReporteDetalleMovimientosRegistrados",
                    new
                    {
                        pIdBanco = request.IdBanco.GetValueOrDefault(0),
                        pEstado = Normalizar(request.Estado, "T"),
                        pOrdenacion = Normalizar(request.Ordenacion, "FA"),
                        pListaConceptos = request.ListaConceptos ?? string.Empty,
                        pTipoMovimiento = Normalizar(request.TipoMovimiento, "TODOS"),
                        FechaDesde = request.FechaDesde.Date,
                        pFechaHasta = request.FechaHasta.Date,
                        pFiltraPeriodo = request.FiltraPeriodo,
                        pFechaEmisionPeriodo = request.FechaEmisionPeriodo?.Date
                    },
                    commandType: CommandType.StoredProcedure)
                    .ToList();

                return DbHelper.CreateOkResponse(result);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<List<TesReporteDetalleMovimientosRegistradosDto>>(ex.Message);
            }
        }

        private static string Normalizar(string? value, string fallback)
        {
            return string.IsNullOrWhiteSpace(value)
                ? fallback
                : value.Trim();
        }

        public ErrorDto<List<TesBancoCargadoConceptos>> Tes_BancosCargadoReporteConceptos_Obtener(int CodEmpresa, string? concepto = null)
        {
            return Tes_BancosCargadoConceptos_Obtener(CodEmpresa, concepto, false);
        }

        private ErrorDto<List<TesBancoCargadoConceptos>> Tes_BancosCargadoConceptos_Obtener(
            int CodEmpresa,
            string? concepto,
            bool filtraAutoRegistro)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);
            try
            {
                var conceptoTrim = concepto?.Trim();

                const string sql = @"
                    SELECT
                        COD_CONCEPTO,
                        DESCRIPCION,
                        COD_CUENTA_MASK,
                        DP_TRAMITE_APL,
                        CUENTA_DESC
                    FROM vTes_Conceptos
                    WHERE ESTADO = 'A'
                      AND (@filtraAutoRegistro = 0 OR AUTO_REGISTRO = 1)
                      AND (@concepto IS NULL OR COD_CONCEPTO = @concepto);";

                var response = conn.Query<TesBancoCargadoConceptos>(
                    sql,
                    new
                    {
                        concepto = string.IsNullOrWhiteSpace(conceptoTrim)
                            ? null
                            : conceptoTrim,
                        filtraAutoRegistro
                    }
                ).ToList();

                return DbHelper.CreateOkResponse(response);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<List<TesBancoCargadoConceptos>>(ex.Message);
            }

        }
    }
}
