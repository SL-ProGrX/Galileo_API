using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX.Credito;

namespace Galileo_API.DataBaseTier.ProGrX.Creditos
{
    public partial class FrmCrArregloPagoDb
    {
        /// <summary>
        /// Aplica capitalizacion de deuda.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<CrArregloPagoAplicacionResultadoData> Cr_ArregloPago_Capitaliza_Aplicar(
            int codEmpresa,
            CrArregloPagoCapitalizaRequest request)
        {
            request.usuario = NormalizarTexto(request.usuario);
            request.caja = NormalizarTexto(request.caja);
            request.tipo_doc = NormalizarTexto(request.tipo_doc);
            request.tiquete = (request.tiquete ?? string.Empty).Trim();
            request.unidad = NormalizarTexto(request.unidad);
            request.divisa = NormalizarTexto(request.divisa);
            request.notas = (request.notas ?? string.Empty).Trim();

            var validacion = ValidarNotasYOperacion(request.operacion, request.usuario, request.notas);
            if (validacion.Code != 0)
            {
                return DbHelper.CreateErrorResponse(
                    validacion.Description ?? "Datos invalidos.",
                    validacion.Code.GetValueOrDefault(-1),
                    new CrArregloPagoAplicacionResultadoData());
            }

            var operacionResp = Cr_ArregloPago_Operacion_Obtener(
                codEmpresa,
                request.operacion,
                request.usuario,
                request.tipo_intereses);

            if (operacionResp.Code != 0 || operacionResp.Result is null)
            {
                return DbHelper.CreateErrorResponse(
                    operacionResp.Description ?? "No se encontr&oacute; la operaci&oacute;n.",
                    operacionResp.Code.GetValueOrDefault(-2),
                    new CrArregloPagoAplicacionResultadoData());
            }

            var operacion = operacionResp.Result;

            if (request.total_cajas > operacion.int_cor + operacion.cargos + operacion.int_mor + operacion.saldo + operacion.polizas)
            {
                return DbHelper.CreateErrorResponse(
                    "Total en cajas es mayor que la deuda.",
                    -2,
                    new CrArregloPagoAplicacionResultadoData());
            }

            if (operacion.retencion)
            {
                return DbHelper.CreateErrorResponse(
                    "No se pueden procesar capitalizaciones de deudas a retenciones.",
                    -2,
                    new CrArregloPagoAplicacionResultadoData());
            }

            if (operacion.mora_count <= 0)
            {
                return DbHelper.CreateErrorResponse(
                    "Esta operaci&oacute;n no puede realizar una capitalizaci&oacute;n de deuda porque est&aacute; al d&iacute;a.",
                    -2,
                    new CrArregloPagoAplicacionResultadoData());
            }

            if (request.total_cajas > 0 && string.IsNullOrWhiteSpace(request.tipo_doc))
            {
                return DbHelper.CreateErrorResponse(
                    "Debe indicar el tipo de documento.",
                    -2,
                    new CrArregloPagoAplicacionResultadoData());
            }

            var cajaCtx = Cr_ArregloPago_CajaContexto_Crear(
                request.caja,
                request.apertura,
                request.tiquete,
                request.unidad,
                request.divisa);

            var requiereContextoCaja = request.total_cajas > 0;

            if (!operacion.sys_plan_pagos && request.total_cajas <= 0)
            {
                requiereContextoCaja = true;
            }

            if (requiereContextoCaja)
            {
                var cajaReqResp = Cr_ArregloPago_CajaContexto_Validar(cajaCtx);
                if (cajaReqResp.Code != 0)
                {
                    return DbHelper.CreateErrorResponse(
                        cajaReqResp.Description ?? "Datos de caja invalidos.",
                        cajaReqResp.Code.GetValueOrDefault(-1),
                        new CrArregloPagoAplicacionResultadoData());
                }

                if (request.total_cajas > 0)
                {
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
                }
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
            var tipoDocumento = request.total_cajas > 0 ? request.tipo_doc : "0";
            var numDocumento = request.total_cajas > 0
                ? _mRecibos.FxDocumentoConsecutivo(codEmpresa, request.tipo_doc).ToString()
                : "0";

            var numNota = string.Empty;

            try
            {
                using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);
                conn.Open();

                using var tx = conn.BeginTransaction();

                if (operacion.sys_plan_pagos)
                {
                    var cuotaVencida = request.tipo_intereses ? 0 : 1;

                    if (request.total_cajas > 0)
                    {
                        conn.Execute(
                            @"exec spCrdPlanPagoAbonoOrdinario
                        @Operacion,
                        'CRD014',
                        @Usuario,
                        @TipoDoc,
                        @NumDoc,
                        @Monto,
                        @Fecha,
                        '',
                        'V',
                        @CuotaVencida;",
                            new
                            {
                                Operacion = request.operacion,
                                Usuario = request.usuario,
                                TipoDoc = tipoDocumento,
                                NumDoc = numDocumento,
                                Monto = request.total_cajas,
                                Fecha = operacion.fecha_servidor,
                                CuotaVencida = cuotaVencida
                            },
                            tx);
                    }

                    var deudaPendiente = operacion.total_pagar - request.total_cajas;

                    if (deudaPendiente > 0)
                    {
                        numNota = _mRecibos.FxDocumentoConsecutivo(codEmpresa, "REA").ToString();

                        conn.Execute(
                            @"exec spCrdPlanPagoAbonoOrdinario
                        @Operacion,
                        'CBR011',
                        @Usuario,
                        'REA',
                        @NumNota,
                        @Monto,
                        @Fecha,
                        '',
                        'V',
                        @CuotaVencida;",
                            new
                            {
                                Operacion = request.operacion,
                                Usuario = request.usuario,
                                NumNota = numNota,
                                Monto = deudaPendiente,
                                Fecha = operacion.fecha_servidor,
                                CuotaVencida = cuotaVencida
                            },
                            tx);

                        conn.Execute(
                            @"exec spCrdPlanPagoAnulaAbono
                        @Operacion,
                        'CBR011',
                        @Usuario,
                        'REA',
                        @NumNota,
                        1,
                        0,
                        0,
                        @Monto,
                        0,
                        0,
                        @Fecha,
                        '';",
                            new
                            {
                                Operacion = request.operacion,
                                Usuario = request.usuario,
                                NumNota = numNota,
                                Monto = deudaPendiente,
                                Fecha = operacion.fecha_servidor
                            },
                            tx);

                        if (request.trasladar)
                        {
                            conn.Execute(
                                "exec spCrdPlanPagoPrincipalTraslado @Operacion;",
                                new { Operacion = request.operacion },
                                tx);
                        }
                    }
                }
                else
                {
                    var notaResp = conn.QueryFirstOrDefault<dynamic>(
                        @"exec spCrdOperacionArreglo_Capitaliza
                    @Operacion,
                    @TipoDoc,
                    @NumDoc,
                    @Monto,
                    @Usuario,
                    @Caja,
                    @Trasladar;",
                        new
                        {
                            Operacion = request.operacion,
                            TipoDoc = tipoDocumento,
                            NumDoc = numDocumento,
                            Monto = request.total_cajas,
                            Usuario = request.usuario,
                            Caja = request.caja,
                            Trasladar = request.trasladar ? 1 : 0
                        },
                        tx);

                    if (notaResp != null)
                    {
                        numNota = Convert.ToString(notaResp.NumDoc) ?? string.Empty;
                    }
                }

                if (request.total_cajas > 0 && !string.IsNullOrWhiteSpace(tipoDocumento) && numDocumento != "0")
                {
                    var docAbonoResp = Cr_ArregloPago_DocumentoAbono_Generar(
                        conn,
                        tx,
                        codEmpresa,
                        globales,
                        operacion,
                        cajaCtx,
                        request.usuario,
                        tipoDocumento,
                        numDocumento,
                        "CRD014",
                        request.notas);

                    if (docAbonoResp.Code != 0)
                    {
                        tx.Rollback();
                        return DbHelper.CreateErrorResponse(
                            docAbonoResp.Description ?? "No fue posible generar el documento de abono.",
                            docAbonoResp.Code.GetValueOrDefault(-1),
                            new CrArregloPagoAplicacionResultadoData());
                    }
                }

                if (!string.IsNullOrWhiteSpace(numNota))
                {
                    var docReadecuacionResp = Cr_ArregloPago_DocumentoReadecuacion_Generar(
                        conn,
                        tx,
                        codEmpresa,
                        globales,
                        operacion,
                        cajaCtx,
                        request.usuario,
                        "REA",
                        numNota,
                        "CBR011",
                        request.trasladar,
                        request.notas);

                    if (docReadecuacionResp.Code != 0)
                    {
                        tx.Rollback();
                        return DbHelper.CreateErrorResponse(
                            docReadecuacionResp.Description ?? "No fue posible generar el documento de readecuacion.",
                            docReadecuacionResp.Code.GetValueOrDefault(-1),
                            new CrArregloPagoAplicacionResultadoData());
                    }
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

            RegistrarBitacora(
                codEmpresa,
                request.usuario,
                "Registra - WEB",
                $"Arreglo de Pago Op {request.operacion}");

            return DbHelper.CreateOkResponse(new CrArregloPagoAplicacionResultadoData
            {
                tipo_documento = tipoDocumento == "0" ? string.Empty : tipoDocumento,
                num_documento = numDocumento == "0" ? string.Empty : numDocumento,
                tipo_nota = string.IsNullOrWhiteSpace(numNota) ? string.Empty : "REA",
                num_nota = numNota,
                mensaje = "Arreglo de Pago realizado satisfactoriamente..."
            });
        }
    }
}