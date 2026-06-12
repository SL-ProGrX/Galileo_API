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
        private sealed class CrArregloPagoCapitalizaContexto
        {
            public Globales globales { get; set; } = new();
            public CrArregloPagoOperacionData operacion { get; set; } = new();
            public CrArregloPagoCajaContexto caja { get; set; } = new();
            public string tipo_documento { get; set; } = string.Empty;
            public string num_documento { get; set; } = string.Empty;
            public string num_nota { get; set; } = string.Empty;
        }

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
            Cr_ArregloPago_Capitaliza_Normalizar(request);

            var validacionSolicitud = Cr_ArregloPago_Capitaliza_ValidarSolicitud(request);
            if (validacionSolicitud.Code != 0)
            {
                return Cr_ArregloPago_Capitaliza_Error(validacionSolicitud);
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

            var validacionOperacion = Cr_ArregloPago_Capitaliza_ValidarOperacion(request, operacion);
            if (validacionOperacion.Code != 0)
            {
                return Cr_ArregloPago_Capitaliza_Error(validacionOperacion);
            }

            var cajaCtx = Cr_ArregloPago_CajaContexto_Crear(
                request.caja,
                request.apertura,
                request.tiquete,
                request.unidad,
                request.divisa);

            var validacionCaja = Cr_ArregloPago_Capitaliza_ValidarCaja(
                codEmpresa,
                request,
                operacion,
                cajaCtx);

            if (validacionCaja.Code != 0)
            {
                return Cr_ArregloPago_Capitaliza_Error(validacionCaja);
            }

            var globalesResp = ObtenerGlobales(codEmpresa, request.usuario);
            if (globalesResp.Code != 0 || globalesResp.Result is null)
            {
                return DbHelper.CreateErrorResponse(
                    globalesResp.Description ?? paramGlobalesNulos,
                    globalesResp.Code.GetValueOrDefault(-1),
                    new CrArregloPagoAplicacionResultadoData());
            }

            var contexto = Cr_ArregloPago_Capitaliza_Contexto_Crear(
                codEmpresa,
                request,
                operacion,
                cajaCtx,
                globalesResp.Result);

            var procesoResp = Cr_ArregloPago_Capitaliza_Procesar(
                codEmpresa,
                request,
                contexto);

            if (procesoResp.Code != 0)
            {
                return procesoResp;
            }

            RegistrarBitacora(
                codEmpresa,
                request.usuario,
                "Registra - WEB",
                $"Arreglo de Pago Op {request.operacion}");

            return DbHelper.CreateOkResponse(new CrArregloPagoAplicacionResultadoData
            {
                tipo_documento = contexto.tipo_documento == "0" ? string.Empty : contexto.tipo_documento,
                num_documento = contexto.num_documento == "0" ? string.Empty : contexto.num_documento,
                tipo_nota = string.IsNullOrWhiteSpace(contexto.num_nota) ? string.Empty : "REA",
                num_nota = contexto.num_nota,
                mensaje = "Arreglo de Pago realizado satisfactoriamente..."
            });
        }

        private static void Cr_ArregloPago_Capitaliza_Normalizar(
            CrArregloPagoCapitalizaRequest request)
        {
            request.usuario = NormalizarTexto(request.usuario);
            request.caja = NormalizarTexto(request.caja);
            request.tipo_doc = NormalizarTexto(request.tipo_doc);
            request.tiquete = (request.tiquete ?? string.Empty).Trim();
            request.unidad = NormalizarTexto(request.unidad);
            request.divisa = NormalizarTexto(request.divisa);
            request.notas = (request.notas ?? string.Empty).Trim();
        }

        private ErrorDto Cr_ArregloPago_Capitaliza_ValidarSolicitud(
            CrArregloPagoCapitalizaRequest request)
        {
            return ValidarNotasYOperacion(request.operacion, request.usuario, request.notas);
        }

        private ErrorDto Cr_ArregloPago_Capitaliza_ValidarOperacion(
            CrArregloPagoCapitalizaRequest request,
            CrArregloPagoOperacionData operacion)
        {
            var deuda = operacion.int_cor + operacion.cargos + operacion.int_mor + operacion.saldo + operacion.polizas;

            if (request.total_cajas > deuda)
            {
                return DbHelper.ErrorResponse("Total en cajas es mayor que la deuda.", -2);
            }

            if (operacion.retencion)
            {
                return DbHelper.ErrorResponse(
                    "No se pueden procesar capitalizaciones de deudas a retenciones.",
                    -2);
            }

            if (operacion.mora_count <= 0)
            {
                return DbHelper.ErrorResponse(
                    "Esta operaci&oacute;n no puede realizar una capitalizaci&oacute;n de deuda porque est&aacute; al d&iacute;a.",
                    -2);
            }

            if (request.total_cajas > 0 && string.IsNullOrWhiteSpace(request.tipo_doc))
            {
                return DbHelper.ErrorResponse("Debe indicar el tipo de documento.", -2);
            }

            return DbHelper.OkResponse("Ok");
        }

        private ErrorDto Cr_ArregloPago_Capitaliza_ValidarCaja(
            int codEmpresa,
            CrArregloPagoCapitalizaRequest request,
            CrArregloPagoOperacionData operacion,
            CrArregloPagoCajaContexto cajaCtx)
        {
            if (!Cr_ArregloPago_Capitaliza_RequiereCaja(request, operacion))
            {
                return DbHelper.OkResponse("Ok");
            }

            var cajaReqResp = Cr_ArregloPago_CajaContexto_Validar(cajaCtx);
            if (cajaReqResp.Code != 0)
            {
                return cajaReqResp;
            }

            if (request.total_cajas <= 0)
            {
                return DbHelper.OkResponse("Ok");
            }

            return Cr_ArregloPago_CajaMovimiento_Validar(
                codEmpresa,
                cajaCtx,
                operacion.cedula);
        }

        private static bool Cr_ArregloPago_Capitaliza_RequiereCaja(
            CrArregloPagoCapitalizaRequest request,
            CrArregloPagoOperacionData operacion)
        {
            if (request.total_cajas > 0)
            {
                return true;
            }

            return !operacion.sys_plan_pagos;
        }

        private CrArregloPagoCapitalizaContexto Cr_ArregloPago_Capitaliza_Contexto_Crear(
            int codEmpresa,
            CrArregloPagoCapitalizaRequest request,
            CrArregloPagoOperacionData operacion,
            CrArregloPagoCajaContexto cajaCtx,
            Globales globales)
        {
            var tipoDocumento = request.total_cajas > 0 ? request.tipo_doc : "0";
            var numDocumento = request.total_cajas > 0
                ? _mRecibos.FxDocumentoConsecutivo(codEmpresa, request.tipo_doc).ToString()
                : "0";

            return new CrArregloPagoCapitalizaContexto
            {
                globales = globales,
                operacion = operacion,
                caja = cajaCtx,
                tipo_documento = tipoDocumento,
                num_documento = numDocumento
            };
        }

        private ErrorDto<CrArregloPagoAplicacionResultadoData> Cr_ArregloPago_Capitaliza_Procesar(
            int codEmpresa,
            CrArregloPagoCapitalizaRequest request,
            CrArregloPagoCapitalizaContexto contexto)
        {
            try
            {
                using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);
                conn.Open();

                using var tx = conn.BeginTransaction();

                if (contexto.operacion.sys_plan_pagos)
                {
                    Cr_ArregloPago_Capitaliza_ProcesarPlanPagos(codEmpresa, conn, tx, request, contexto);
                }
                else
                {
                    Cr_ArregloPago_Capitaliza_ProcesarTradicional(conn, tx, request, contexto);
                }

                var documentosResp = Cr_ArregloPago_Capitaliza_Documentos_Generar(
                    codEmpresa,
                    conn,
                    tx,
                    request,
                    contexto);

                if (documentosResp.Code != 0)
                {
                    tx.Rollback();

                    return DbHelper.CreateErrorResponse(
                        documentosResp.Description ?? "No fue posible generar los documentos de capitalizaci&oacute;n.",
                        documentosResp.Code.GetValueOrDefault(-1),
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

        private void Cr_ArregloPago_Capitaliza_ProcesarPlanPagos(
            int codEmpresa,
            SqlConnection conn,
            SqlTransaction tx,
            CrArregloPagoCapitalizaRequest request,
            CrArregloPagoCapitalizaContexto contexto)
        {
            var cuotaVencida = request.tipo_intereses ? 0 : 1;

            Cr_ArregloPago_Capitaliza_AbonoCajaPlanPagos_Registrar(
                conn,
                tx,
                request,
                contexto,
                cuotaVencida);

            var deudaPendiente = contexto.operacion.total_pagar - request.total_cajas;
            if (deudaPendiente <= 0)
            {
                return;
            }

            contexto.num_nota = _mRecibos.FxDocumentoConsecutivo(codEmpresa, "REA").ToString();

            Cr_ArregloPago_Capitaliza_ReadecuacionPlanPagos_Registrar(
                conn,
                tx,
                request,
                contexto,
                cuotaVencida,
                deudaPendiente);

            if (request.trasladar)
            {
                conn.Execute(
                    "exec spCrdPlanPagoPrincipalTraslado @Operacion;",
                    new { Operacion = request.operacion },
                    tx);
            }
        }

        private static void Cr_ArregloPago_Capitaliza_AbonoCajaPlanPagos_Registrar(
            SqlConnection conn,
            SqlTransaction tx,
            CrArregloPagoCapitalizaRequest request,
            CrArregloPagoCapitalizaContexto contexto,
            int cuotaVencida)
        {
            if (request.total_cajas <= 0)
            {
                return;
            }

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
                    TipoDoc = contexto.tipo_documento,
                    NumDoc = contexto.num_documento,
                    Monto = request.total_cajas,
                    Fecha = contexto.operacion.fecha_servidor,
                    CuotaVencida = cuotaVencida
                },
                tx);
        }

        private static void Cr_ArregloPago_Capitaliza_ReadecuacionPlanPagos_Registrar(
            SqlConnection conn,
            SqlTransaction tx,
            CrArregloPagoCapitalizaRequest request,
            CrArregloPagoCapitalizaContexto contexto,
            int cuotaVencida,
            decimal deudaPendiente)
        {
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
                    NumNota = contexto.num_nota,
                    Monto = deudaPendiente,
                    Fecha = contexto.operacion.fecha_servidor,
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
                    NumNota = contexto.num_nota,
                    Monto = deudaPendiente,
                    Fecha = contexto.operacion.fecha_servidor
                },
                tx);
        }

        private static void Cr_ArregloPago_Capitaliza_ProcesarTradicional(
            SqlConnection conn,
            SqlTransaction tx,
            CrArregloPagoCapitalizaRequest request,
            CrArregloPagoCapitalizaContexto contexto)
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
                    TipoDoc = contexto.tipo_documento,
                    NumDoc = contexto.num_documento,
                    Monto = request.total_cajas,
                    Usuario = request.usuario,
                    Caja = request.caja,
                    Trasladar = request.trasladar ? 1 : 0
                },
                tx);

            if (notaResp != null)
            {
                contexto.num_nota = Convert.ToString(notaResp.NumDoc) ?? string.Empty;
            }
        }

        private ErrorDto Cr_ArregloPago_Capitaliza_Documentos_Generar(
            int codEmpresa,
            SqlConnection conn,
            SqlTransaction tx,
            CrArregloPagoCapitalizaRequest request,
            CrArregloPagoCapitalizaContexto contexto)
        {
            var docAbonoResp = Cr_ArregloPago_Capitaliza_DocumentoAbono_Generar(
                codEmpresa,
                conn,
                tx,
                request,
                contexto);

            if (docAbonoResp.Code != 0)
            {
                return docAbonoResp;
            }

            return Cr_ArregloPago_Capitaliza_DocumentoReadecuacion_Generar(
                codEmpresa,
                conn,
                tx,
                request,
                contexto);
        }

        private ErrorDto Cr_ArregloPago_Capitaliza_DocumentoAbono_Generar(
            int codEmpresa,
            SqlConnection conn,
            SqlTransaction tx,
            CrArregloPagoCapitalizaRequest request,
            CrArregloPagoCapitalizaContexto contexto)
        {
            if (request.total_cajas <= 0 ||
                string.IsNullOrWhiteSpace(contexto.tipo_documento) ||
                contexto.num_documento == "0")
            {
                return DbHelper.CreateOkResponse();
            }

            return Cr_ArregloPago_DocumentoAbono_Generar(
                conn,
                tx,
                new CrArregloPagoDocumentoContext
                {
                    cod_empresa = codEmpresa,
                    globales = contexto.globales,
                    operacion = contexto.operacion,
                    caja = contexto.caja,
                    usuario = request.usuario,
                    tipo_doc = contexto.tipo_documento,
                    num_doc = contexto.num_documento,
                    concepto = "CRD014",
                    notas = request.notas
                });
        }

        private ErrorDto Cr_ArregloPago_Capitaliza_DocumentoReadecuacion_Generar(
            int codEmpresa,
            SqlConnection conn,
            SqlTransaction tx,
            CrArregloPagoCapitalizaRequest request,
            CrArregloPagoCapitalizaContexto contexto)
        {
            if (string.IsNullOrWhiteSpace(contexto.num_nota))
            {
                return DbHelper.CreateOkResponse();
            }

            return Cr_ArregloPago_DocumentoReadecuacion_Generar(
                conn,
                tx,
                new CrArregloPagoDocumentoContext
                {
                    cod_empresa = codEmpresa,
                    globales = contexto.globales,
                    operacion = contexto.operacion,
                    caja = contexto.caja,
                    usuario = request.usuario,
                    tipo_doc = "REA",
                    num_doc = contexto.num_nota,
                    concepto = "CBR011",
                    notas = request.notas,
                    trasladar = request.trasladar
                });
        }

        private static ErrorDto<CrArregloPagoAplicacionResultadoData> Cr_ArregloPago_Capitaliza_Error(
            ErrorDto error)
        {
            return DbHelper.CreateErrorResponse(
                error.Description ?? "Datos invalidos.",
                error.Code.GetValueOrDefault(-1),
                new CrArregloPagoAplicacionResultadoData());
        }
    }
}