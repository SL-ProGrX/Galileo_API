using Dapper;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Clientes;

namespace Galileo.DataBaseTier.ProGrX.Clientes
{
    public partial class FrmFndTrasladoPatrimonioDB
    {
        private ErrorDto<SimpleSuccessResult> EjecutarSimple(
            int codEmpresa,
            string sql,
            object parameters,
            string errorMessage)
        {
            var result = DbHelper.ExecuteNonQueryWithResult(
                CreatePortalDb(),
                codEmpresa,
                sql,
                parameters);

            return result.Code == 0
                ? DbHelper.CreateOkResponse(
                    new SimpleSuccessResult
                    {
                        Success = result.Result > 0
                    })
                : ErrorSimple(
                    result.Description ?? errorMessage,
                    result.Code.GetValueOrDefault(-1));
        }

        private static ErrorDto<SimpleSuccessResult> CrearRespuestaSimple(
            ErrorDto<bool> result,
            string errorMessage)
        {
            return result.Code == 0
                ? DbHelper.CreateOkResponse(
                    new SimpleSuccessResult
                    {
                        Success = result.Result
                    })
                : ErrorSimple(
                    result.Description ?? errorMessage,
                    result.Code.GetValueOrDefault(-1));
        }

        private static ErrorDto<SimpleSuccessResult> ErrorSimple(
            string message,
            int code)
        {
            return DbHelper.CreateErrorResponse(
                message,
                code,
                new SimpleSuccessResult
                {
                    Success = false
                });
        }

        private static object CrearParametrosContratos(
            string idOperadora,
            string codPlan,
            string destino,
            bool marcado)
        {
            var destinoSeguro = NormalizarTexto(destino);

            return new
            {
                IdOperadora = NormalizarTexto(idOperadora),
                CodPlan = NormalizarTexto(codPlan),
                Marcado = marcado,
                FiltroEstado = destinoSeguro is "Aporte Obrero" or "Capitalización" ? 1 : 0,
                FiltroPatronal = destinoSeguro == "Aporte Patronal" ? 1 : 0
            };
        }

        private static string NormalizarTipoDocumento(string? tipo)
        {
            return NormalizarTexto(tipo).ToUpperInvariant() switch
            {
                "RECIBO" or "FRE" => TipoRecibo,
                "NOTA CREDITO" or "FNC" => TipoNotaCredito,
                "NOTA DEBITO" or "FND" => TipoNotaDebito,
                var valor => valor
            };
        }

        private static object CrearParametrosContratoDetalle(
            FndContratoDetalleInsertRequest request)
        {
            return new
            {
                request.CodOperadora,
                request.CodPlan,
                request.CodContrato,
                Monto = request.Monto * -1,
                FechaProceso = request.Fecha.HasValue ? request.Fecha.Value.Year * 100 + request.Fecha.Value.Month : 0,
                request.Tcon,
                request.Ncon,
                request.Usuario
            };
        }

        private static object CrearParametrosDocsAsiento(
            SifDocsAsientoRequest request)
        {
            return new
            {
                request.Tipo,
                request.Transaccion,
                request.Monto,
                request.Movimiento,
                request.Divisa,
                request.TipoCambio,
                request.Contabilidad,
                request.Unidad,
                request.CentroCosto,
                request.Cuenta,
                request.Referencia1,
                request.Referencia2,
                request.Referencia3,
                request.DivisaRev,
                request.NoReversa
            };
        }

        private static object CrearParametrosTransaccionPatrimonio(
            SifTransaccionPatrimonioInsertRequest request)
        {
            return new
            {
                request.NC_Pat,
                request.TipoDoc,
                request.Usuario,
                request.Concepto,
                request.Operadora,
                request.Plan,
                request.OficinaTitular,
                Linea1 = "Op.:" + NormalizarTexto(request.OperadoraText),
                Linea2 = "Plan :" + NormalizarTexto(request.Plan),
                Linea3 = NormalizarTexto(request.Descripcion),
                Detalle = NormalizarTexto(request.Destino),
                Documento = NormalizarTexto(request.Destino)
            };
        }

        private static (string Tipo, string SqlConsolidado)?
            ObtenerMovimientoAhorro(
                string destino,
                string estadoActual,
                string existe)
        {
            var existeConsolidado =
                !string.Equals(
                    NormalizarTexto(existe),
                    ExisteNoEncontrado,
                    StringComparison.OrdinalIgnoreCase);

            return NormalizarTexto(destino).ToUpperInvariant() switch
            {
                "O" => (
                    "O",
                    existeConsolidado
                        ? SqlAhorroObreroUpdate
                        : SqlAhorroObreroInsert),

                "P" when string.Equals(
                    NormalizarTexto(estadoActual),
                    "S",
                    StringComparison.OrdinalIgnoreCase)
                    => (
                        "P",
                        existeConsolidado
                            ? SqlAportePatronalUpdate
                            : SqlAportePatronalInsert),

                "P" when string.Equals(
                    NormalizarTexto(estadoActual),
                    "A",
                    StringComparison.OrdinalIgnoreCase)
                    => (
                        "X",
                        existeConsolidado
                            ? SqlCustodiaUpdate
                            : SqlCustodiaInsert),

                "C" => (
                    "C",
                    existeConsolidado
                        ? SqlCapitalizaUpdate
                        : SqlCapitalizaInsert),

                _ => null
            };
        }

        private static object CrearParametrosAhorroDetalle(
            FndAhorroConsolidadoRequest request,
            FndAhorroConsolidadoSocio socio,
            string tipo)
        {
            return new
            {
                Cedula = NormalizarTexto(socio.Cedula),
                Tipo = tipo,
                socio.Monto,
                FechaProc = request.Fecha.HasValue
                    ? request.Fecha.Value.Year * 100 + request.Fecha.Value.Month
                    : 0,
                NumCom = "NC-" + NormalizarTexto(request.NC_Pat),
                Tcon = NormalizarTexto(request.TipoDoc),
                Ncon = NormalizarTexto(request.NC_Pat),
                Usuario = NormalizarTexto(request.Usuario),
                Concepto = NormalizarTexto(request.Concepto)
            };
        }

        private ErrorDto<FndDocumentoConsecutivoAseResult?>
            ObtenerConsecutivoAseVersionUno(
                int codEmpresa,
                string tipo)
        {
            var sql = ObtenerSqlConsecutivoAse(tipo);

            if (sql is null)
            {
                return DbHelper.CreateErrorResponse<
                    FndDocumentoConsecutivoAseResult?>(
                    "Tipo de documento no soportado para SysDocVersion 1.",
                    -2,
                    null);
            }

            var result = DbHelper.WithConn(
                CreatePortalDb(),
                codEmpresa,
                connection =>
                {
                    var consecutivo =
                        connection.QueryFirstOrDefault<long>(
                            sql.Value.SelectSql);

                    connection.Execute(sql.Value.UpdateSql);

                    return new FndDocumentoConsecutivoAseResult
                    {
                        Consecutivo = consecutivo
                    };
                });

            return result.Code == 0
                ? DbHelper.CreateOkResponse<
                    FndDocumentoConsecutivoAseResult?>(
                    result.Result)
                : DbHelper.CreateErrorResponse<
                    FndDocumentoConsecutivoAseResult?>(
                    result.Description
                        ?? "Error obteniendo consecutivo ASE.",
                    result.Code.GetValueOrDefault(-1),
                    null);
        }

        private static (string SelectSql, string UpdateSql)?
            ObtenerSqlConsecutivoAse(string tipo)
        {
            return NormalizarTexto(tipo).ToUpperInvariant() switch
            {
                "RE" => (
                    SqlAseReciboSelect,
                    SqlAseReciboUpdate),

                "DP" => (
                    SqlAseDepositoSelect,
                    SqlAseDepositoUpdate),

                "ND" => (
                    SqlAseNotaDebitoSelect,
                    SqlAseNotaDebitoUpdate),

                "NC" => (
                    SqlAseNotaCreditoSelect,
                    SqlAseNotaCreditoUpdate),

                _ => null
            };
        }

        private PortalDB CreatePortalDb() => new(_config);

        private static string NormalizarTexto(string? valor)
            => (valor ?? string.Empty).Trim();
    }
}