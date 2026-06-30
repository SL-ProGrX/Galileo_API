using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX.Credito;

namespace Galileo_API.DataBaseTier.ProGrX.Creditos
{
    public partial class FrmCrCatalogoCreditosDb
    {

        private static void NormalizarRequest(CrCatalogoCreditoGuardarRequest request)
        {
            request.codigo = request.codigo.Trim().ToUpperInvariant();
            request.codigoa = request.codigoa?.Trim().ToUpperInvariant() ?? string.Empty;
            request.descripcion = request.descripcion?.Trim().ToUpperInvariant() ?? string.Empty;
            request.notas = request.notas?.Trim() ?? string.Empty;
            request.oficina = request.oficina?.Trim().ToUpperInvariant() ?? string.Empty;
            request.oficina_desc = request.oficina_desc?.Trim() ?? string.Empty;
            request.reserva_codigo = request.reserva_codigo?.Trim().ToUpperInvariant() ?? string.Empty;
            request.reserva_plan_desc = request.reserva_plan_desc?.Trim() ?? string.Empty;
            request.revolutiva_plan_ahorro = request.revolutiva_plan_ahorro?.Trim().ToUpperInvariant() ?? string.Empty;
            request.plan_ahorro_desc = request.plan_ahorro_desc?.Trim() ?? string.Empty;
            request.convenio = NormalizarSiNo(request.convenio);
            request.poliza = NormalizarSiNo(request.poliza);
            request.refunde = NormalizarSiNo(request.refunde);
            request.retencion = NormalizarSiNo(request.retencion);
            request.aceptarefun = NormalizarSiNo(request.aceptarefun);
            request.primer_cuota = NormalizarSiNo(request.primer_cuota);
            request.pidecheque = NormalizarSiNo(request.pidecheque);
            request.tramite = string.IsNullOrWhiteSpace(request.tramite) ? "C" : request.tramite.Trim().ToUpperInvariant()[..1];
            request.requisitos_tipo = string.IsNullOrWhiteSpace(request.requisitos_tipo) ? "L" : request.requisitos_tipo.Trim().ToUpperInvariant()[..1];
            request.refunde_tipo = string.IsNullOrWhiteSpace(request.refunde_tipo) ? "P" : request.refunde_tipo.Trim().ToUpperInvariant()[..1];
            request.liq_tipoaumento = string.IsNullOrWhiteSpace(request.liq_tipoaumento) ? "F" : request.liq_tipoaumento.Trim().ToUpperInvariant()[..1];
            request.cobro_tipo_aplicacion = string.IsNullOrWhiteSpace(request.cobro_tipo_aplicacion) ? "V" : request.cobro_tipo_aplicacion.Trim().ToUpperInvariant()[..1];
            request.tasa_mora_tipo = string.IsNullOrWhiteSpace(request.tasa_mora_tipo) ? "N/A" : request.tasa_mora_tipo.Trim().ToUpperInvariant();
            request.auto_gestion_tipo = string.IsNullOrWhiteSpace(request.auto_gestion_tipo) ? "C" : request.auto_gestion_tipo.Trim().ToUpperInvariant()[..1];
            request.mov_sinpe_tipos = request.mov_sinpe_tipos <= 0 ? 3 : request.mov_sinpe_tipos;
        }

        private static ErrorDto ValidarGuardarRequest(CrCatalogoCreditoGuardarRequest request)
        {
            var errores = new List<string>();

            if (string.IsNullOrWhiteSpace(request.codigo))
                errores.Add("Codigo de Linea no es valido.");

            if (string.IsNullOrWhiteSpace(request.descripcion))
                errores.Add("Descripcion no valida.");

            if (request.id_comite <= 0)
                errores.Add("Comite de Evaluacion no es valido.");

            if (string.IsNullOrWhiteSpace(request.cod_institucion))
                errores.Add("Entidad Deductora no es valida.");

            if (request.codigo.Length > 4)
                errores.Add("Codigo Corriente [excede letras] Invalido.");

            ValidarPorcentaje(
                request.porc_anticipo_ext,
                "El % de Anticipo Extraordinario, no es valido.",
                errores);
            ValidarPorcentaje(
                request.porc_cargo_cancelacion,
                "El % de Comision x Cancelacion (Anticipo), no es valido.",
                errores);
            ValidarPorcentaje(
                request.refunde_porc,
                "El % de Amortizacion para Permitir Refundiciones, no es valido.",
                errores);
            ValidarPorcentaje(
                request.tasa_mora_add,
                "Los Puntos Adicionales para Morosidad, no es valido.",
                errores);
            ValidarPorcentaje(
                request.tbp_adicional,
                "Los Puntos Adicionales Sobre TBP, no es valido.",
                errores);

            if (request.oficina_linea && string.IsNullOrWhiteSpace(request.oficina))
                errores.Add("La Oficina Fija no fue especificada para esta linea.");

            return errores.Count == 0
                ? new ErrorDto { Code = 0, Description = "OK" }
                : new ErrorDto
                {
                    Code = -1,
                    Description = string.Join(Environment.NewLine, errores.Select(error => $" - {error}"))
                };
        }

        private static void ValidarPorcentaje(decimal valor, string mensaje, List<string> errores)
        {
            if (valor < 0 || valor > 100)
                errores.Add(mensaje);
        }


        private static void NormalizarPeLRequest(CrCatalogoCreditoPeLGuardarRequest request)
        {
            request.codigo = request.codigo?.Trim().ToUpperInvariant() ?? string.Empty;
            request.df_descripcion_linea = request.df_descripcion_linea?.Trim() ?? string.Empty;
            request.df_uso_destino_linea = request.df_uso_destino_linea?.Trim() ?? string.Empty;
            request.df_logo_url = request.df_logo_url?.Trim() ?? string.Empty;
            request.df_etiqueta_aprobacion = request.df_etiqueta_aprobacion?.Trim() ?? string.Empty;
            request.df_etiqueta_monto_max = request.df_etiqueta_monto_max?.Trim() ?? string.Empty;
            request.df_etiqueta_plazo_tasa = request.df_etiqueta_plazo_tasa?.Trim() ?? string.Empty;
            request.df_etiqueta_deposito = request.df_etiqueta_deposito?.Trim() ?? string.Empty;
            request.df_color_caja = string.IsNullOrWhiteSpace(request.df_color_caja)
                ? "#415CBF"
                : request.df_color_caja.Trim().ToUpperInvariant();
            request.usuario = request.usuario?.Trim() ?? string.Empty;
        }


        private static void NormalizarAsignacionRequest(CrCatalogoCreditoAsignacionGuardarRequest request)
        {
            request.tipo = request.tipo?.Trim().ToLowerInvariant() ?? string.Empty;
            request.codigo = request.codigo?.Trim().ToUpperInvariant() ?? string.Empty;
            request.codigo_asignacion = request.codigo_asignacion?.Trim().ToUpperInvariant() ?? string.Empty;
            request.usuario = request.usuario?.Trim() ?? string.Empty;
        }


        private static void NormalizarRangoRequest(CrCatalogoCreditoRangoBaseGuardarRequest request)
        {
            request.codigo = request.codigo?.Trim().ToUpperInvariant() ?? string.Empty;
            request.usuario = request.usuario?.Trim() ?? string.Empty;
        }


        private static void NormalizarRangoRequest(CrCatalogoCreditoRangoPlazoGuardarRequest request)
        {
            request.codigo = request.codigo?.Trim().ToUpperInvariant() ?? string.Empty;
            request.usuario = request.usuario?.Trim() ?? string.Empty;
        }


        private static void NormalizarRangoGarantiaRequest(CrCatalogoCreditoRangoGarantiaGuardarRequest request)
        {
            request.codigo = request.codigo?.Trim().ToUpperInvariant() ?? string.Empty;
            request.usuario = request.usuario?.Trim() ?? string.Empty;
            request.garantia.garantia = request.garantia.garantia?.Trim().ToUpperInvariant() ?? string.Empty;
        }


        private static void NormalizarLiquidezBonoRequest(CrCatalogoCreditoLiquidezBonoGuardarRequest request)
        {
            request.codigo = request.codigo?.Trim().ToUpperInvariant() ?? string.Empty;
            request.usuario = request.usuario?.Trim() ?? string.Empty;
        }


        private static void NormalizarLiquidezCapacidadRequest(CrCatalogoCreditoLiquidezCapacidadGuardarRequest request)
        {
            request.codigo = request.codigo?.Trim().ToUpperInvariant() ?? string.Empty;
            request.usuario = request.usuario?.Trim() ?? string.Empty;
        }


        private static void NormalizarComiteEstudioRequest(CrCatalogoCreditoComiteEstudioGuardarRequest request)
        {
            request.codigo = request.codigo?.Trim().ToUpperInvariant() ?? string.Empty;
            request.usuario = request.usuario?.Trim() ?? string.Empty;
            request.comite.linea = request.comite.linea?.Trim().ToUpperInvariant() ?? string.Empty;
            request.comite.comite = request.comite.comite?.Trim() ?? string.Empty;
        }


        private static string NormalizarSiNo(string? valor)
        {
            return string.Equals(valor?.Trim(), "S", StringComparison.OrdinalIgnoreCase) ? "S" : "N";
        }


        private static ErrorDto<CrCatalogoCreditoAsignacionesData> ErrorAsignaciones(string? descripcion)
        {
            return new ErrorDto<CrCatalogoCreditoAsignacionesData>
            {
                Code = -1,
                Description = descripcion ?? "Ocurrio un error al obtener asignaciones de la linea."
            };
        }


        private static ErrorDto<CrCatalogoCreditoRangosBaseData> ErrorRangosBase(string? descripcion)
        {
            return new ErrorDto<CrCatalogoCreditoRangosBaseData>
            {
                Code = -1,
                Description = descripcion ?? "Ocurrio un error al obtener rangos base de la linea."
            };
        }


        private void RegistrarBitacora(int codEmpresa, string usuario, string movimiento, string detalle)
        {
            _bitacora.Bitacora(new Galileo.Models.Security.BitacoraInsertarDto
            {
                EmpresaId = codEmpresa,
                Usuario = usuario,
                DetalleMovimiento = detalle,
                Movimiento = movimiento,
                Modulo = vModulo
            });
        }
    }
}
