using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX.Credito;

namespace Galileo_API.DataBaseTier.ProGrX.Creditos
{
    public partial class FrmCrArregloPagoDb
    {
        /// <summary>
        /// Aplica periodo de gracia.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto Cr_ArregloPago_PeriodoGracia_Aplicar(
            int codEmpresa,
            CrArregloPagoPeriodoGraciaRequest request)
        {
            request.usuario = NormalizarTexto(request.usuario);
            request.notas = (request.notas ?? string.Empty).Trim();
            request.tipo_aplicacion = NormalizarTexto(request.tipo_aplicacion);

            var validacion = ValidarNotasYOperacion(request.operacion, request.usuario, request.notas);
            if (validacion.Code != 0)
            {
                return validacion;
            }

            if (!request.fecha_inicio.HasValue || !request.fecha_corte.HasValue)
            {
                return DbHelper.ErrorResponse(
                    "Debe indicar la fecha de inicio y corte.",
                    -2);
            }

            if (request.fecha_corte.Value.Date < request.fecha_inicio.Value.Date)
            {
                return DbHelper.ErrorResponse(
                    "La fecha corte no puede ser menor que la fecha inicio.",
                    -2);
            }

            var operacionResp = Cr_ArregloPago_Operacion_Obtener(
                codEmpresa,
                request.operacion,
                request.usuario);

            if (operacionResp.Code != 0 || operacionResp.Result is null)
            {
                return DbHelper.ErrorResponse(
                    operacionResp.Description ?? "No se encontro la operacion.",
                    operacionResp.Code.GetValueOrDefault(-1));
            }

            if (operacionResp.Result.mora_count > 0 && !request.retroactivo)
            {
                return DbHelper.ErrorResponse(
                    "A esta operaci&oacute;n no se le puede dar periodo de gracia porque no est&aacute; al d&iacute;a.",
                    -2);
            }

            const string sql = @"
                exec spCrd_Operacion_Arreglos_Periodo_Gracia
                    @Operacion,
                    @TipoAplicacion,
                    @AplicaIntereses,
                    @AplicaCargos,
                    @AplicaPolizas,
                    @Retroactivo,
                    @AjustaPlazo,
                    @FechaInicio,
                    @FechaCorte,
                    @Usuario,
                    @Notas;";

            var execResp = DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                sql,
                new
                {
                    Operacion = request.operacion,
                    TipoAplicacion = ObtenerTipoAplicacion(request.tipo_aplicacion),
                    AplicaIntereses = request.aplica_intereses ? 1 : 0,
                    AplicaCargos = request.aplica_cargos ? 1 : 0,
                    AplicaPolizas = request.aplica_polizas ? 1 : 0,
                    Retroactivo = request.retroactivo ? 1 : 0,
                    AjustaPlazo = request.ajusta_plazo ? 1 : 0,
                    FechaInicio = request.fecha_inicio.Value.Date,
                    FechaCorte = request.fecha_corte.Value.Date.AddHours(23).AddMinutes(59).AddSeconds(59),
                    Usuario = request.usuario,
                    Notas = Truncar(request.notas, 500)
                });

            if (execResp.Code != 0)
            {
                return execResp;
            }

            RegistrarBitacora(
                codEmpresa,
                request.usuario,
                "Registra - WEB",
                $"Periodo de Gracia, Operacion: {request.operacion} Cta Rang: {request.fecha_inicio:dd/MM/yyyy} - {request.fecha_corte:dd/MM/yyyy}");

            return DbHelper.OkResponse("Periodo de Gracia aplicado satisfactoriamente!");
        }

        /// <summary>
        /// Aplica vencimiento de intereses.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<CrArregloPagoAplicacionResultadoData> Cr_ArregloPago_VencimientoIntereses_Aplicar(
            int codEmpresa,
            CrArregloPagoVencimientoInteresesRequest request)
        {
            request.usuario = NormalizarTexto(request.usuario);
            request.notas = (request.notas ?? string.Empty).Trim();

            var validacion = ValidarNotasYOperacion(request.operacion, request.usuario, request.notas);
            if (validacion.Code != 0)
            {
                return DbHelper.CreateErrorResponse(
                    validacion.Description ?? "Datos invalidos.",
                    validacion.Code.GetValueOrDefault(-1),
                    new CrArregloPagoAplicacionResultadoData());
            }

            if (!request.fecha_corte.HasValue)
            {
                return DbHelper.CreateErrorResponse(
                    "Debe indicar la fecha de corte.",
                    -2,
                    new CrArregloPagoAplicacionResultadoData());
            }

            const string sql = @"
                exec spCrdOperacionArreglo_InteresVence
                    @Operacion,
                    @FechaCorte,
                    @Usuario;";

            var response = DbHelper.ExecuteSingleQuery<CrArregloPagoAplicacionResultadoData>(
                _portalDb,
                codEmpresa,
                sql,
                new CrArregloPagoAplicacionResultadoData(),
                new
                {
                    Operacion = request.operacion,
                    FechaCorte = request.fecha_corte.Value.Date.AddHours(23).AddMinutes(59).AddSeconds(59),
                    Usuario = request.usuario
                });

            if (response.Code != 0)
            {
                return DbHelper.CreateErrorResponse(
                    response.Description ?? "No fue posible aplicar vencimiento de intereses.",
                    response.Code.GetValueOrDefault(-1),
                    new CrArregloPagoAplicacionResultadoData());
            }

            RegistrarBitacora(
                codEmpresa,
                request.usuario,
                "Registra - WEB",
                $"Vencimiento de Intereses, Operacion: {request.operacion} Corte: {request.fecha_corte:dd/MM/yyyy}");

            var resultado = response.Result ?? new CrArregloPagoAplicacionResultadoData();
            resultado.mensaje = "Vencimiento de Intereses aplicado satisfactoriamente!";

            return DbHelper.CreateOkResponse(resultado);
        }
    }
}