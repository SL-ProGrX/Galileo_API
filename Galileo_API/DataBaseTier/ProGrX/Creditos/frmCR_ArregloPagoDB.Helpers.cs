using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.Security;
using Galileo_API.Models.ProGrX.Credito;

namespace Galileo_API.DataBaseTier.ProGrX.Creditos
{
    public partial class FrmCrArregloPagoDb
    {
        public ErrorDto<Globales> ObtenerGlobales(int codEmpresa, string usuario)
        {
            if (string.IsNullOrWhiteSpace(usuario))
            {
                return DbHelper.CreateErrorResponse(
                    usuarioInvalido,
                    -2,
                    new Globales());
            }

            return _mProGrxMain.sbSifParametrosInicializa(codEmpresa, usuario);
        }

        private static string FormatearLineaDocumento(string titulo, decimal monto)
        {
            return $"{titulo,-18}..: {monto:N2}";
        }

        private static ErrorDto ValidarNotasYOperacion(
            int operacion,
            string usuario,
            string notas)
        {
            if (operacion <= 0)
            {
                return DbHelper.ErrorResponse("Debe indicar una operaci&oacute;n v&aacute;lida.", -2);
            }

            if (string.IsNullOrWhiteSpace(usuario))
            {
                return DbHelper.ErrorResponse(usuarioInvalido, -2);
            }

            if (string.IsNullOrWhiteSpace(notas) || notas.Trim().Length < 10)
            {
                return DbHelper.ErrorResponse("Indique una nota v&aacute;lida para la transacci&oacute;n.", -2);
            }

            return DbHelper.OkResponse("Ok");
        }

        private static string ObtenerTipoAplicacion(string tipoAplicacion)
        {
            return tipoAplicacion.StartsWith("P", StringComparison.OrdinalIgnoreCase)
                ? "P"
                : "T";
        }

        private static bool EsTipoExtraordinario(string tipoAbono)
        {
            return tipoAbono.StartsWith("E", StringComparison.OrdinalIgnoreCase);
        }

        private static long ObtenerProcesoCuota(string procesoCuota)
        {
            var limpio = new string((procesoCuota ?? string.Empty).Where(char.IsDigit).ToArray());

            if (long.TryParse(limpio, out var proceso))
            {
                return proceso;
            }

            return 0;
        }

        private static string Truncar(string valor, int largo)
        {
            valor = (valor ?? string.Empty).Trim();

            if (valor.Length > largo)
            {
                valor = valor[..largo];
            }

            return valor;
        }

        private static string NormalizarTexto(string valor)
        {
            return (valor ?? string.Empty).Trim().ToUpperInvariant();
        }

        private sealed class CrArregloPagoCajaContexto
        {
            public string caja { get; set; } = string.Empty;
            public int apertura { get; set; }
            public string tiquete { get; set; } = string.Empty;
            public string unidad { get; set; } = string.Empty;
            public string divisa { get; set; } = string.Empty;
        }

        private static CrArregloPagoCajaContexto Cr_ArregloPago_CajaContexto_Crear(
            string caja,
            int apertura,
            string tiquete,
            string unidad,
            string divisa)
        {
            return new CrArregloPagoCajaContexto
            {
                caja = NormalizarTexto(caja),
                apertura = apertura,
                tiquete = (tiquete ?? string.Empty).Trim(),
                unidad = NormalizarTexto(unidad),
                divisa = NormalizarTexto(divisa),
            };
        }

        private static ErrorDto Cr_ArregloPago_CajaContexto_Validar(CrArregloPagoCajaContexto ctx)
        {
            if (string.IsNullOrWhiteSpace(ctx.caja))
            {
                return DbHelper.ErrorResponse("Debe indicar la caja.", -2);
            }

            if (ctx.apertura <= 0)
            {
                return DbHelper.ErrorResponse("Debe indicar la apertura de caja.", -2);
            }

            if (string.IsNullOrWhiteSpace(ctx.tiquete))
            {
                return DbHelper.ErrorResponse("Debe indicar el tiquete de caja.", -2);
            }

            if (string.IsNullOrWhiteSpace(ctx.unidad))
            {
                return DbHelper.ErrorResponse("Debe indicar la unidad de caja.", -2);
            }

            if (string.IsNullOrWhiteSpace(ctx.divisa))
            {
                return DbHelper.ErrorResponse("Debe indicar la divisa de caja.", -2);
            }

            return DbHelper.OkResponse("Ok");
        }

        private ErrorDto Cr_ArregloPago_CajaMovimiento_Validar(
            int codEmpresa,
            CrArregloPagoCajaContexto ctx,
            string cedula)
        {
            if (_mAfilicacion.fxgCongelamiento(codEmpresa, cedula, "per_abono_cajas"))
            {
                return DbHelper.ErrorResponse(
                    "Esta persona se encuentra congelada, no puede realizar movimientos en cajas. Verifique.",
                    -2);
            }

            var estadoApertura = _mCajas.fxCajasAperturaEstado(codEmpresa, ctx.caja, ctx.apertura);
            if (string.Equals((estadoApertura ?? string.Empty).Trim(), "C", StringComparison.OrdinalIgnoreCase))
            {
                return DbHelper.ErrorResponse(
                    $"- La apertura ..:{ctx.apertura} de esta caja ha sido cerrada!",
                    -2);
            }

            return DbHelper.OkResponse("Ok");
        }

        private static decimal Cr_ArregloPago_AbonoEspecial_Total(CrArregloPagoAbonoEspecialRequest request)
        {
            return request.int_cor +
                   request.int_mor +
                   request.principal +
                   request.polizas +
                   request.cargos;
        }

        private void RegistrarBitacora(
            int codEmpresa,
            string usuario,
            string movimiento,
            string detalle)
        {
            _securityMainDb.Bitacora(new BitacoraInsertarDto
            {
                EmpresaId = codEmpresa,
                Usuario = usuario,
                Modulo = VModulo,
                Movimiento = movimiento,
                DetalleMovimiento = detalle
            });
        }

        private static CrArregloPagoDocumentoLineas Cr_ArregloPago_DocumentoLineas_BaseCrear(
            CrArregloPagoDocumentoContext ctx,
            CrArregloPagoDocumentoMontos montos,
            decimal saldoAnterior,
            decimal saldoActual,
            string linea5)
        {
            return new CrArregloPagoDocumentoLineas
            {
                linea1 = FormatearLineaDocumento(LineaSaldoAnterior, saldoAnterior),
                linea2 = FormatearLineaDocumento(LineaSaldoActual, saldoActual),
                linea3 = FormatearLineaDocumento(LineaInteresCorriente, montos.int_cor),
                linea4 = FormatearLineaDocumento(LineaInteresAtrasado, montos.int_mor),
                linea5 = linea5,
                linea6 = FormatearLineaDocumento(LineaCargosTotales, montos.cargos),
                linea7 = FormatearLineaDocumento(LineaPolizas, montos.polizas),
                linea8 = Cr_ArregloPago_DocumentoOperacion_Linea_Crear(ctx),
                linea9 = $"Descripcion       ..: {ctx.operacion.linea_desc}",
                linea10 = string.Empty,
                linea11 = string.Empty
            };
        }
    }
}