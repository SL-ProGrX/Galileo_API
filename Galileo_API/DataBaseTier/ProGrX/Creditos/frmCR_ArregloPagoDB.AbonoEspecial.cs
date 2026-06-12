using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX.Credito;
using Microsoft.Data.SqlClient;

namespace Galileo_API.DataBaseTier.ProGrX.Creditos
{
    public partial class FrmCrArregloPagoDb
    {
        private sealed class CrArregloPagoAbonoEspecialContexto
        {
            public Globales globales { get; set; } = new();
            public CrArregloPagoOperacionData operacion { get; set; } = new();
            public CrArregloPagoCajaContexto caja { get; set; } = new();
            public string num_documento { get; set; } = string.Empty;
            public DateTime fecha_servidor { get; set; }
            public decimal glng_fecha_cr { get; set; }
        }

        /// <summary>
        /// Aplica abono especial.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<CrArregloPagoAplicacionResultadoData> Cr_ArregloPago_AbonoEspecial_Aplicar(
            int codEmpresa,
            CrArregloPagoAbonoEspecialRequest request)
        {
            Cr_ArregloPago_AbonoEspecial_Normalizar(request);

            var validacionSolicitud = Cr_ArregloPago_AbonoEspecial_ValidarSolicitud(request);
            if (validacionSolicitud.Code != 0)
            {
                return Cr_ArregloPago_AbonoEspecial_Error(validacionSolicitud);
            }

            var operacionResp = Cr_ArregloPago_Operacion_Obtener(
                codEmpresa,
                request.operacion,
                request.usuario);

            if (operacionResp.Code != 0 || operacionResp.Result is null)
            {
                return DbHelper.CreateErrorResponse(
                    operacionResp.Description ?? "No se encontro la operacion.",
                    operacionResp.Code.GetValueOrDefault(-1),
                    new CrArregloPagoAplicacionResultadoData());
            }

            var operacion = operacionResp.Result;

            var validacionOperacion = Cr_ArregloPago_AbonoEspecial_ValidarOperacion(request, operacion);
            if (validacionOperacion.Code != 0)
            {
                return Cr_ArregloPago_AbonoEspecial_Error(validacionOperacion);
            }

            var cajaCtx = Cr_ArregloPago_CajaContexto_Crear(
                request.caja,
                request.apertura,
                request.tiquete,
                request.unidad,
                request.divisa);

            var validacionCaja = Cr_ArregloPago_AbonoEspecial_ValidarCaja(
                codEmpresa,
                cajaCtx,
                operacion.cedula);

            if (validacionCaja.Code != 0)
            {
                return Cr_ArregloPago_AbonoEspecial_Error(validacionCaja);
            }

            var globalesResp = ObtenerGlobales(codEmpresa, request.usuario);
            if (globalesResp.Code != 0 || globalesResp.Result is null)
            {
                return DbHelper.CreateErrorResponse(
                    globalesResp.Description ?? paramGlobalesNulos,
                    globalesResp.Code.GetValueOrDefault(-1),
                    new CrArregloPagoAplicacionResultadoData());
            }

            var contexto = Cr_ArregloPago_AbonoEspecial_Contexto_Crear(
                codEmpresa,
                request,
                operacion,
                cajaCtx,
                globalesResp.Result);

            var procesoResp = Cr_ArregloPago_AbonoEspecial_Procesar(
                codEmpresa,
                request,
                contexto);

            if (procesoResp.Code != 0)
            {
                return procesoResp;
            }

            Cr_ArregloPago_AbonoEspecial_RegistrarBitacoras(
                codEmpresa,
                request,
                operacion.codigo);

            return DbHelper.CreateOkResponse(new CrArregloPagoAplicacionResultadoData
            {
                tipo_documento = request.tipo_doc,
                num_documento = contexto.num_documento,
                mensaje = "Abono Especial aplicado satisfactoriamente!"
            });
        }

        private static void Cr_ArregloPago_AbonoEspecial_Normalizar(
            CrArregloPagoAbonoEspecialRequest request)
        {
            request.usuario = NormalizarTexto(request.usuario);
            request.caja = NormalizarTexto(request.caja);
            request.tipo_doc = NormalizarTexto(request.tipo_doc);
            request.tiquete = (request.tiquete ?? string.Empty).Trim();
            request.unidad = NormalizarTexto(request.unidad);
            request.divisa = NormalizarTexto(request.divisa);
            request.notas = (request.notas ?? string.Empty).Trim();
            request.tipo_abono = NormalizarTexto(request.tipo_abono);
        }

        private ErrorDto Cr_ArregloPago_AbonoEspecial_ValidarSolicitud(
            CrArregloPagoAbonoEspecialRequest request)
        {
            var validacion = ValidarNotasYOperacion(request.operacion, request.usuario, request.notas);
            if (validacion.Code != 0)
            {
                return validacion;
            }

            if (string.IsNullOrWhiteSpace(request.tipo_doc))
            {
                return DbHelper.ErrorResponse("Debe indicar el tipo de documento.", -2);
            }

            var totalAbonoEspecial = Cr_ArregloPago_AbonoEspecial_Total(request);
            if (totalAbonoEspecial <= 0)
            {
                return DbHelper.ErrorResponse(
                    "No se ha especificado ning&uacute;n rubro para el abono especial.",
                    -2);
            }

            if (request.total_cajas <= 0)
            {
                return DbHelper.ErrorResponse(
                    "Debe indicar el monto en cajas para el abono especial.",
                    -2);
            }

            if (totalAbonoEspecial > request.total_cajas)
            {
                return DbHelper.ErrorResponse(
                    "Monto en cajas no corresponde al monto a recaudar para el abono especial.",
                    -2);
            }

            return Cr_ArregloPago_AbonoEspecial_ValidarCuota(request);
        }

        private static ErrorDto Cr_ArregloPago_AbonoEspecial_ValidarCuota(
            CrArregloPagoAbonoEspecialRequest request)
        {
            if (EsTipoExtraordinario(request.tipo_abono))
            {
                return DbHelper.OkResponse("Ok");
            }

            if (string.IsNullOrWhiteSpace(request.proceso_cuota))
            {
                return DbHelper.ErrorResponse(
                    "Debe indicar el periodo de cuota para el abono ordinario.",
                    -2);
            }

            if (ObtenerProcesoCuota(request.proceso_cuota) <= 0)
            {
                return DbHelper.ErrorResponse(
                    "El periodo de cuota para el abono ordinario no es v&aacute;lido.",
                    -2);
            }

            if (request.num_cuota <= 0)
            {
                return DbHelper.ErrorResponse(
                    "Debe indicar el n&uacute;mero de cuota para el abono ordinario.",
                    -2);
            }

            return DbHelper.OkResponse("Ok");
        }

        private static ErrorDto Cr_ArregloPago_AbonoEspecial_ValidarOperacion(
            CrArregloPagoAbonoEspecialRequest request,
            CrArregloPagoOperacionData operacion)
        {
            if (operacion.retencion)
            {
                return DbHelper.ErrorResponse(
                    "No se pueden realizar abonos especiales a retenciones.",
                    -2);
            }

            if (request.principal > operacion.saldo)
            {
                return DbHelper.ErrorResponse(
                    "La amortizaci&oacute;n especificada es mayor al saldo.",
                    -2);
            }

            if (!operacion.sys_plan_pagos && operacion.mora_count > 0)
            {
                return DbHelper.ErrorResponse(
                    "No se puede aplicar Abono Especial porque esta operaci&oacute;n se encuentra en mora.",
                    -2);
            }

            return DbHelper.OkResponse("Ok");
        }

        private ErrorDto Cr_ArregloPago_AbonoEspecial_ValidarCaja(
            int codEmpresa,
            CrArregloPagoCajaContexto cajaCtx,
            string cedula)
        {
            var cajaReqResp = Cr_ArregloPago_CajaContexto_Validar(cajaCtx);
            if (cajaReqResp.Code != 0)
            {
                return cajaReqResp;
            }

            return Cr_ArregloPago_CajaMovimiento_Validar(
                codEmpresa,
                cajaCtx,
                cedula);
        }

        private CrArregloPagoAbonoEspecialContexto Cr_ArregloPago_AbonoEspecial_Contexto_Crear(
            int codEmpresa,
            CrArregloPagoAbonoEspecialRequest request,
            CrArregloPagoOperacionData operacion,
            CrArregloPagoCajaContexto cajaCtx,
            Globales globales)
        {
            return new CrArregloPagoAbonoEspecialContexto
            {
                globales = globales,
                operacion = operacion,
                caja = cajaCtx,
                num_documento = _mRecibos.FxDocumentoConsecutivo(codEmpresa, request.tipo_doc).ToString(),
                fecha_servidor = globales.fxFechaServidor ?? DateTime.Now,
                glng_fecha_cr = globales.GlngFechaCR
            };
        }

        private ErrorDto<CrArregloPagoAplicacionResultadoData> Cr_ArregloPago_AbonoEspecial_Procesar(
            int codEmpresa,
            CrArregloPagoAbonoEspecialRequest request,
            CrArregloPagoAbonoEspecialContexto contexto)
        {
            try
            {
                using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);
                conn.Open();

                using var tx = conn.BeginTransaction();

                if (contexto.operacion.sys_plan_pagos)
                {
                    Cr_ArregloPago_AbonoEspecial_ProcesarPlanPagos(conn, tx, request, contexto);
                }
                else
                {
                    Cr_ArregloPago_AbonoEspecial_ProcesarTradicional(conn, tx, request, contexto);
                }

                var docAbonoResp = Cr_ArregloPago_DocumentoAbono_Generar(
                    conn,
                    tx,
                    new CrArregloPagoDocumentoContext
                    {
                        cod_empresa = codEmpresa,
                        globales = contexto.globales,
                        operacion = contexto.operacion,
                        caja = contexto.caja,
                        usuario = request.usuario,
                        tipo_doc = request.tipo_doc,
                        num_doc = contexto.num_documento,
                        concepto = "CRD007",
                        notas = request.notas
                    });

                if (docAbonoResp.Code != 0)
                {
                    tx.Rollback();

                    return DbHelper.CreateErrorResponse(
                        docAbonoResp.Description ?? "No fue posible generar el documento de abono especial.",
                        docAbonoResp.Code.GetValueOrDefault(-1),
                        new CrArregloPagoAplicacionResultadoData());
                }

                tx.Commit();

                return DbHelper.CreateOkResponse(new CrArregloPagoAplicacionResultadoData());
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse(
                    ex.Message,
                    -1,
                    new CrArregloPagoAplicacionResultadoData());
            }
        }

        private void Cr_ArregloPago_AbonoEspecial_ProcesarPlanPagos(
            SqlConnection conn,
            SqlTransaction tx,
            CrArregloPagoAbonoEspecialRequest request,
            CrArregloPagoAbonoEspecialContexto contexto)
        {
            var cuota = Cr_ArregloPago_AbonoEspecial_Cuota_Obtener(request);

            conn.Execute(
                @"exec spCrdPlanPagoAbonoEspecial
                    @Operacion,
                    'CRD007',
                    @Usuario,
                    @TipoDoc,
                    @NumDoc,
                    0,
                    @IntCor,
                    @IntMor,
                    @Principal,
                    @Cargo,
                    @Poliza,
                    @Fecha,
                    '',
                    @Proceso,
                    @NumCuota,
                    1,
                    1,
                    1;",
                new
                {
                    Operacion = request.operacion,
                    Usuario = request.usuario,
                    TipoDoc = request.tipo_doc,
                    NumDoc = contexto.num_documento,
                    IntCor = request.int_cor,
                    IntMor = request.int_mor,
                    Principal = request.principal,
                    Cargo = request.cargos,
                    Poliza = request.polizas,
                    Fecha = contexto.fecha_servidor,
                    Proceso = cuota.proceso,
                    NumCuota = cuota.num_cuota
                },
                tx);

            conn.Execute(
                "exec spCrdPlanPagos @Operacion;",
                new { Operacion = request.operacion },
                tx);
        }

        private void Cr_ArregloPago_AbonoEspecial_ProcesarTradicional(
            SqlConnection conn,
            SqlTransaction tx,
            CrArregloPagoAbonoEspecialRequest request,
            CrArregloPagoAbonoEspecialContexto contexto)
        {
            conn.Execute(
                @"
                update reg_creditos
                set estado = @Estado,
                    saldo = saldo - @Principal,
                    amortiza = amortiza + @Principal,
                    interesc = interesc + @IntCor
                where id_solicitud = @Operacion;

                insert into creditos_dt
                (
                    codigo,
                    id_solicitud,
                    cuota,
                    abono,
                    intcp,
                    amortiza,
                    fechas,
                    fechap,
                    tcon,
                    ncon,
                    estado,
                    cod_concepto,
                    usuario,
                    cod_caja
                )
                values
                (
                    @Codigo,
                    @Operacion,
                    0,
                    @Abono,
                    @IntCor,
                    @Principal,
                    Getdate(),
                    @FechaProceso,
                    @TipoDoc,
                    @NumDoc,
                    'A',
                    'CRD007',
                    @Usuario,
                    @Caja
                );",
                new
                {
                    Estado = request.principal >= contexto.operacion.saldo ? "C" : "A",
                    Principal = request.principal,
                    IntCor = request.int_cor,
                    Operacion = request.operacion,
                    Codigo = contexto.operacion.codigo,
                    Abono = request.principal + request.int_cor,
                    FechaProceso = contexto.glng_fecha_cr,
                    TipoDoc = request.tipo_doc,
                    NumDoc = contexto.num_documento,
                    Usuario = request.usuario,
                    Caja = request.caja
                },
                tx);
        }

        private static (long proceso, short num_cuota) Cr_ArregloPago_AbonoEspecial_Cuota_Obtener(
            CrArregloPagoAbonoEspecialRequest request)
        {
            if (EsTipoExtraordinario(request.tipo_abono))
            {
                return (0, 0);
            }

            return (
                ObtenerProcesoCuota(request.proceso_cuota),
                Convert.ToInt16(request.num_cuota));
        }

        private void Cr_ArregloPago_AbonoEspecial_RegistrarBitacoras(
            int codEmpresa,
            CrArregloPagoAbonoEspecialRequest request,
            string codigoOperacion)
        {
            MCredito.SbBitacoraCredito(
            _portalDb,
            codEmpresa,
            new MCredito.CrBitacoraCreditoRequest
            {
                usuario = request.usuario,
                movimiento = "11",
                detalle = $"Int: {request.int_cor:N2} Amort: {request.principal:N2}",
                tipo = "C",
                operacion = request.operacion,
                codigo = codigoOperacion,
                notas = request.notas
            });

            RegistrarBitacora(
                codEmpresa,
                request.usuario,
                "Aplica - WEB",
                $"Abono Especial de la operacion : {request.operacion}");
        }

        private static ErrorDto<CrArregloPagoAplicacionResultadoData> Cr_ArregloPago_AbonoEspecial_Error(
            ErrorDto error)
        {
            return DbHelper.CreateErrorResponse(
                error.Description ?? "Datos invalidos.",
                error.Code.GetValueOrDefault(-1),
                new CrArregloPagoAplicacionResultadoData());
        }
    }
}