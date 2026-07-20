using Dapper;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Fondos;
using System.Data;

namespace Galileo.DataBaseTier.ProGrX.Fondos
{
    public partial class FrmFndTraspasoTesoreriaDb
    {
        private const int FndTraspasoTesoreriaTamanoMaximoLote = 200;

        private const string SqlFndTraspasoTesoreriaRetener = @"
            UPDATE dbo.Fnd_Liquidacion
            SET Traspaso_Tesoreria = dbo.MyGetdate(),
                Traspaso_Usuario = @Usuario,
                Solicitud_Tesoreria = 0,
                RETENCION_CODIGO = @RetencionCodigo,
                NOTAS = ''
            WHERE Consec = @Consec;";

        /// <summary>
        /// Procesa un lote controlado de liquidaciones para desembolso o retención.
        /// </summary>
        /// <param name="request">Empresa, consecutivos y datos de la acción solicitada.</param>
        /// <returns>Resultado acumulado con errores individuales por consecutivo.</returns>
        public ErrorDto<FndTraspasoTesoreriaProcesarLoteResult> FND_TraspasoTesoreria_ProcesarLote(
            FndTraspasoTesoreriaProcesarLoteRequest request)
        {
            var validacion = ValidarFndTraspasoTesoreriaLote(request);
            if (!string.IsNullOrWhiteSpace(validacion))
            {
                return DbHelper.CreateErrorResponse(
                    validacion,
                    -2,
                    new FndTraspasoTesoreriaProcesarLoteResult());
            }

            var consecutivos = request.Consecutivos.Distinct().ToList();
            var accion = request.Accion.Trim().ToUpperInvariant();

            return DbHelper.WithConn(new PortalDB(_config), request.CodEmpresa, connection =>
            {
                connection.Open();
                var resultado = new FndTraspasoTesoreriaProcesarLoteResult();

                foreach (var consecutivo in consecutivos)
                {
                    ProcesarFndTraspasoTesoreriaRegistro(
                        connection,
                        request,
                        accion,
                        consecutivo,
                        resultado);
                }

                resultado.ConErrores = resultado.Errores.Count;
                return resultado;
            });
        }

        private static void ProcesarFndTraspasoTesoreriaRegistro(
            Microsoft.Data.SqlClient.SqlConnection connection,
            FndTraspasoTesoreriaProcesarLoteRequest request,
            string accion,
            int consecutivo,
            FndTraspasoTesoreriaProcesarLoteResult resultado)
        {
            try
            {
                if (accion == "D")
                {
                    connection.Execute(
                        "spFndRetLiqTesoreria",
                        new
                        {
                            LiqNum = consecutivo,
                            request.Usuario,
                            request.Token
                        },
                        commandType: CommandType.StoredProcedure);
                }
                else
                {
                    connection.Execute(
                        SqlFndTraspasoTesoreriaRetener,
                        new
                        {
                            Consec = consecutivo,
                            request.Usuario,
                            request.RetencionCodigo
                        });
                }

                resultado.Procesados++;
            }
            catch (Exception ex)
            {
                resultado.Errores.Add(new FndTraspasoTesoreriaProcesoError
                {
                    Consec = consecutivo,
                    Descripcion = ex.Message
                });
            }
        }

        private static string? ValidarFndTraspasoTesoreriaLote(
            FndTraspasoTesoreriaProcesarLoteRequest request)
        {
            if (request is null) return "La solicitud es requerida.";
            if (request.Consecutivos.Count == 0) return "Debe seleccionar liquidaciones.";
            if (request.Consecutivos.Count > FndTraspasoTesoreriaTamanoMaximoLote)
                return $"El lote no puede superar {FndTraspasoTesoreriaTamanoMaximoLote} registros.";
            if (request.Consecutivos.Any(consecutivo => consecutivo <= 0))
                return "Los consecutivos deben ser mayores que cero.";
            if (string.IsNullOrWhiteSpace(request.Usuario)) return "El usuario es requerido.";

            var accion = request.Accion.Trim().ToUpperInvariant();
            if (accion is not ("D" or "R")) return "La acción no es válida.";
            if (accion == "D" && string.IsNullOrWhiteSpace(request.Token))
                return "El token es requerido para desembolsar.";
            if (accion == "R" && string.IsNullOrWhiteSpace(request.RetencionCodigo))
                return "El código de retención es requerido.";

            return null;
        }
    }
}
