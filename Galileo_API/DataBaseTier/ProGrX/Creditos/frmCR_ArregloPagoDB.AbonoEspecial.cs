using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX.Credito;

namespace Galileo_API.DataBaseTier.ProGrX.Creditos
{
    public partial class FrmCrArregloPagoDb
    {
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
            request.usuario = NormalizarTexto(request.usuario);
            request.caja = NormalizarTexto(request.caja);
            request.tipo_doc = NormalizarTexto(request.tipo_doc);
            request.tiquete = (request.tiquete ?? string.Empty).Trim();
            request.unidad = NormalizarTexto(request.unidad);
            request.divisa = NormalizarTexto(request.divisa);
            request.notas = (request.notas ?? string.Empty).Trim();
            request.tipo_abono = NormalizarTexto(request.tipo_abono);

            var validacion = ValidarNotasYOperacion(request.operacion, request.usuario, request.notas);
            if (validacion.Code != 0)
            {
                return DbHelper.CreateErrorResponse(
                    validacion.Description ?? "Datos invalidos.",
                    validacion.Code.GetValueOrDefault(-1),
                    new CrArregloPagoAplicacionResultadoData());
            }

            if (string.IsNullOrWhiteSpace(request.tipo_doc))
            {
                return DbHelper.CreateErrorResponse(
                    "Debe indicar el tipo de documento.",
                    -2,
                    new CrArregloPagoAplicacionResultadoData());
            }

            var totalAbonoEspecial = Cr_ArregloPago_AbonoEspecial_Total(request);
            if (totalAbonoEspecial <= 0)
            {
                return DbHelper.CreateErrorResponse(
                    "No se ha especificado ning&uacute;n rubro para el abono especial.",
                    -2,
                    new CrArregloPagoAplicacionResultadoData());
            }

            if (request.total_cajas <= 0)
            {
                return DbHelper.CreateErrorResponse(
                    "Debe indicar el monto en cajas para el abono especial.",
                    -2,
                    new CrArregloPagoAplicacionResultadoData());
            }

            if (totalAbonoEspecial > request.total_cajas)
            {
                return DbHelper.CreateErrorResponse(
                    "Monto en cajas no corresponde al monto a recaudar para el abono especial.",
                    -2,
                    new CrArregloPagoAplicacionResultadoData());
            }

            if (!EsTipoExtraordinario(request.tipo_abono))
            {
                if (string.IsNullOrWhiteSpace(request.proceso_cuota))
                {
                    return DbHelper.CreateErrorResponse(
                        "Debe indicar el periodo de cuota para el abono ordinario.",
                        -2,
                        new CrArregloPagoAplicacionResultadoData());
                }

                if (ObtenerProcesoCuota(request.proceso_cuota) <= 0)
                {
                    return DbHelper.CreateErrorResponse(
                        "El periodo de cuota para el abono ordinario no es v&aacute;lido.",
                        -2,
                        new CrArregloPagoAplicacionResultadoData());
                }

                if (request.num_cuota <= 0)
                {
                    return DbHelper.CreateErrorResponse(
                        "Debe indicar el n&uacute;mero de cuota para el abono ordinario.",
                        -2,
                        new CrArregloPagoAplicacionResultadoData());
                }
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

            if (operacion.retencion)
            {
                return DbHelper.CreateErrorResponse(
                    "No se pueden realizar abonos especiales a retenciones.",
                    -2,
                    new CrArregloPagoAplicacionResultadoData());
            }

            if (request.principal > operacion.saldo)
            {
                return DbHelper.CreateErrorResponse(
                    "La amortizaci&oacute;n especificada es mayor al saldo.",
                    -2,
                    new CrArregloPagoAplicacionResultadoData());
            }

            if (!operacion.sys_plan_pagos && operacion.mora_count > 0)
            {
                return DbHelper.CreateErrorResponse(
                    "No se puede aplicar Abono Especial porque esta operaci&oacute;n se encuentra en mora.",
                    -2,
                    new CrArregloPagoAplicacionResultadoData());
            }

            var cajaCtx = Cr_ArregloPago_CajaContexto_Crear(
                request.caja,
                request.apertura,
                request.tiquete,
                request.unidad,
                request.divisa);

            var cajaReqResp = Cr_ArregloPago_CajaContexto_Validar(cajaCtx);
            if (cajaReqResp.Code != 0)
            {
                return DbHelper.CreateErrorResponse(
                    cajaReqResp.Description ?? "Datos de caja invalidos.",
                    cajaReqResp.Code.GetValueOrDefault(-1),
                    new CrArregloPagoAplicacionResultadoData());
            }

            var cajaValResp = Cr_ArregloPago_CajaMovimiento_Validar(
                codEmpresa,
                cajaCtx,
                operacion.cedula);

            if (cajaValResp.Code != 0)
            {
                return DbHelper.CreateErrorResponse(
                    cajaValResp.Description ?? "La caja no esta disponible para registrar el movimiento.",
                    cajaValResp.Code.GetValueOrDefault(-1),
                    new CrArregloPagoAplicacionResultadoData());
            }

            var globalesResp = ObtenerGlobales(codEmpresa, request.usuario);
            if (globalesResp.Code != 0 || globalesResp.Result is null)
            {
                return DbHelper.CreateErrorResponse(
                    globalesResp.Description ?? paramGlobalesNulos,
                    globalesResp.Code.GetValueOrDefault(-1),
                    new CrArregloPagoAplicacionResultadoData());
            }

            var globales = globalesResp.Result;
            var fechaServidor = globales.fxFechaServidor ?? DateTime.Now;
            var glngFechaCr = globales.GlngFechaCR;
            var numDocumento = _mRecibos.FxDocumentoConsecutivo(codEmpresa, request.tipo_doc).ToString();

            try
            {
                using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);
                conn.Open();

                using var tx = conn.BeginTransaction();

                if (operacion.sys_plan_pagos)
                {
                    long proceso = 0;
                    short numCuota = 0;

                    if (!EsTipoExtraordinario(request.tipo_abono))
                    {
                        proceso = ObtenerProcesoCuota(request.proceso_cuota);
                        numCuota = Convert.ToInt16(request.num_cuota);
                    }

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
                            NumDoc = numDocumento,
                            IntCor = request.int_cor,
                            IntMor = request.int_mor,
                            Principal = request.principal,
                            Cargo = request.cargos,
                            Poliza = request.polizas,
                            Fecha = fechaServidor,
                            Proceso = proceso,
                            NumCuota = numCuota
                        },
                        tx);

                    conn.Execute(
                        "exec spCrdPlanPagos @Operacion;",
                        new { Operacion = request.operacion },
                        tx);
                }
                else
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
                            Estado = request.principal >= operacion.saldo ? "C" : "A",
                            Principal = request.principal,
                            IntCor = request.int_cor,
                            Operacion = request.operacion,
                            Codigo = operacion.codigo,
                            Abono = request.principal + request.int_cor,
                            FechaProceso = glngFechaCr,
                            TipoDoc = request.tipo_doc,
                            NumDoc = numDocumento,
                            Usuario = request.usuario,
                            Caja = request.caja
                        },
                        tx);
                }

                var docAbonoResp = Cr_ArregloPago_DocumentoAbono_Generar(
                    conn,
                    tx,
                    new CrArregloPagoDocumentoContext
                    {
                        cod_empresa = codEmpresa,
                        globales = globales,
                        operacion = operacion,
                        caja = cajaCtx,
                        usuario = request.usuario,
                        tipo_doc = request.tipo_doc,
                        num_doc = numDocumento,
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
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse(
                    ex.Message,
                    -1,
                    new CrArregloPagoAplicacionResultadoData());
            }

            MCredito.SbBitacoraCredito(
                _portalDb,
                codEmpresa,
                request.usuario,
                "11",
                $"Int: {request.int_cor:N2} Amort: {request.principal:N2}",
                "C",
                request.operacion,
                operacion.codigo,
                request.notas);

            RegistrarBitacora(
                codEmpresa,
                request.usuario,
                "Aplica - WEB",
                $"Abono Especial de la operacion : {request.operacion}");

            return DbHelper.CreateOkResponse(new CrArregloPagoAplicacionResultadoData
            {
                tipo_documento = request.tipo_doc,
                num_documento = numDocumento,
                mensaje = "Abono Especial aplicado satisfactoriamente!"
            });
        }
    }
}