using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX.Cajas;
using Galileo_API.Models.ProGrX.Credito;
using Microsoft.Data.SqlClient;

namespace Galileo_API.DataBaseTier.ProGrX.Creditos
{
    public partial class FrmCrArregloPagoDb
    {
        private enum CrArregloPagoDocumentoTipo
        {
            Abono = 1,
            Readecuacion = 2
        }

        private sealed class CrArregloPagoDocumentoContext
        {
            public int cod_empresa { get; set; }
            public Globales globales { get; set; } = new();
            public CrArregloPagoOperacionData operacion { get; set; } = new();
            public CrArregloPagoCajaContexto caja { get; set; } = new();
            public string usuario { get; set; } = string.Empty;
            public string tipo_doc { get; set; } = string.Empty;
            public string num_doc { get; set; } = string.Empty;
            public string concepto { get; set; } = string.Empty;
            public string notas { get; set; } = string.Empty;
            public bool trasladar { get; set; }
        }

        private sealed class CrArregloPagoTipoCambioContext
        {
            public decimal tipo_cambio { get; set; }
            public decimal factor { get; set; }
        }

        private sealed class CrArregloPagoDocumentoMontos
        {
            public decimal int_cor { get; set; }
            public decimal int_mor { get; set; }
            public decimal amortiza { get; set; }
            public decimal cargos { get; set; }
            public decimal polizas { get; set; }
            public decimal iva { get; set; }
            public decimal monto_documento { get; set; }
            public decimal monto_pago_final { get; set; }
        }

        private sealed class CrArregloPagoDocumentoLineas
        {
            public string linea1 { get; set; } = string.Empty;
            public string linea2 { get; set; } = string.Empty;
            public string linea3 { get; set; } = string.Empty;
            public string linea4 { get; set; } = string.Empty;
            public string linea5 { get; set; } = string.Empty;
            public string linea6 { get; set; } = string.Empty;
            public string linea7 { get; set; } = string.Empty;
            public string linea8 { get; set; } = string.Empty;
            public string linea9 { get; set; } = string.Empty;
            public string linea10 { get; set; } = string.Empty;
            public string linea11 { get; set; } = string.Empty;
        }

        private sealed class CrArregloPagoAsientoRequest
        {
            public string tipo_doc { get; set; } = string.Empty;
            public string num_doc { get; set; } = string.Empty;
            public decimal monto { get; set; }
            public string tipo { get; set; } = string.Empty;
            public string divisa { get; set; } = string.Empty;
            public decimal tipo_cambio { get; set; }
            public string unidad { get; set; } = string.Empty;
            public string centro_costo { get; set; } = string.Empty;
            public string cuenta { get; set; } = string.Empty;
            public int operacion { get; set; }
            public string codigo { get; set; } = string.Empty;
        }

        private sealed class CrArregloPagoAsientoFactoryData
        {
            public decimal monto { get; set; }
            public string tipo { get; set; } = string.Empty;
            public string divisa { get; set; } = string.Empty;
            public decimal tipo_cambio { get; set; }
            public string unidad { get; set; } = string.Empty;
            public string centro_costo { get; set; } = string.Empty;
            public string cuenta { get; set; } = string.Empty;
            public int operacion { get; set; }
            public string codigo { get; set; } = string.Empty;
        }

        private ErrorDto Cr_ArregloPago_DocumentoAbono_Generar(
            SqlConnection conn,
            SqlTransaction tx,
            CrArregloPagoDocumentoContext ctx)
        {
            return Cr_ArregloPago_DocumentoGenerar(
                conn,
                tx,
                ctx,
                CrArregloPagoDocumentoTipo.Abono);
        }

        private ErrorDto Cr_ArregloPago_DocumentoReadecuacion_Generar(
            SqlConnection conn,
            SqlTransaction tx,
            CrArregloPagoDocumentoContext ctx)
        {
            return Cr_ArregloPago_DocumentoGenerar(
                conn,
                tx,
                ctx,
                CrArregloPagoDocumentoTipo.Readecuacion);
        }

        private ErrorDto Cr_ArregloPago_DocumentoGenerar(
            SqlConnection conn,
            SqlTransaction tx,
            CrArregloPagoDocumentoContext ctx,
            CrArregloPagoDocumentoTipo tipoDocumento)
        {
            try
            {
                var tipoCambio = Cr_ArregloPago_TipoCambio_Obtener(ctx);
                var afectacion = Cr_ArregloPago_DocumentoAfectacion_Obtener(
                    conn,
                    tx,
                    ctx.operacion.sys_plan_pagos,
                    ctx.tipo_doc,
                    ctx.num_doc);

                var ctas = Cr_ArregloPago_OperacionCtas_Obtener(conn, tx, ctx.operacion.operacion);
                var proxPago = Cr_ArregloPago_Documento_ProxPago_Obtener(conn, tx, ctx, tipoDocumento);
                var saldoActual = Cr_ArregloPago_Documento_SaldoActual_Obtener(conn, tx, ctx, tipoDocumento);
                var montos = Cr_ArregloPago_Documento_Montos_Crear(afectacion, tipoDocumento);
                var lineas = Cr_ArregloPago_Documento_Lineas_Crear(
                    ctx,
                    montos,
                    proxPago,
                    saldoActual,
                    tipoDocumento);

                Cr_ArregloPago_Documento_Transaccion_Insertar(
                    conn,
                    tx,
                    ctx,
                    montos,
                    lineas,
                    tipoDocumento);

                Cr_ArregloPago_AsientosDocumento_Registrar(
                    conn,
                    tx,
                    ctx,
                    tipoCambio,
                    ctas,
                    montos,
                    tipoDocumento);

                if (tipoDocumento == CrArregloPagoDocumentoTipo.Abono)
                {
                    Cr_ArregloPago_DocumentoPagoFinal_Registrar(conn, tx, ctx, ctas, montos);
                }

                return DbHelper.CreateOkResponse();
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        private CrArregloPagoTipoCambioContext Cr_ArregloPago_TipoCambio_Obtener(
            CrArregloPagoDocumentoContext ctx)
        {
            var tipoCambio = _mCajas.fxCajasTipoCambio(
                ctx.cod_empresa,
                ctx.globales.GEnlace,
                ctx.caja.divisa);

            return new CrArregloPagoTipoCambioContext
            {
                tipo_cambio = tipoCambio,
                factor = Convert.ToDecimal(MProGrxMain.fxSys_Tipo_Cambio_Apl(tipoCambio))
            };
        }

        private static decimal Cr_ArregloPago_Documento_SaldoActual_Obtener(
            SqlConnection conn,
            SqlTransaction tx,
            CrArregloPagoDocumentoContext ctx,
            CrArregloPagoDocumentoTipo tipoDocumento)
        {
            if (tipoDocumento == CrArregloPagoDocumentoTipo.Abono)
            {
                return ctx.operacion.saldo;
            }

            return conn.QueryFirstOrDefault<decimal>(
                "select isnull(saldo, 0) from reg_creditos where id_solicitud = @Operacion;",
                new { Operacion = ctx.operacion.operacion },
                tx);
        }

        private static CajasCrdOperacionProxPagoData Cr_ArregloPago_Documento_ProxPago_Obtener(
            SqlConnection conn,
            SqlTransaction tx,
            CrArregloPagoDocumentoContext ctx,
            CrArregloPagoDocumentoTipo tipoDocumento)
        {
            if (tipoDocumento == CrArregloPagoDocumentoTipo.Readecuacion &&
                !ctx.operacion.sys_plan_pagos)
            {
                return new CajasCrdOperacionProxPagoData();
            }

            return conn.QueryFirstOrDefault<CajasCrdOperacionProxPagoData>(
                "exec spCrdOperacionFechaProxPago @Operacion;",
                new { Operacion = ctx.operacion.operacion },
                tx) ?? new CajasCrdOperacionProxPagoData();
        }

        private static CrArregloPagoDocumentoMontos Cr_ArregloPago_Documento_Montos_Crear(
            CajasCrdDocumentoAfectacionData afectacion,
            CrArregloPagoDocumentoTipo tipoDocumento)
        {
            return tipoDocumento == CrArregloPagoDocumentoTipo.Abono
                ? Cr_ArregloPago_DocumentoAbono_Montos_Crear(afectacion)
                : Cr_ArregloPago_DocumentoReadecuacion_Montos_Crear(afectacion);
        }

        private static CrArregloPagoDocumentoMontos Cr_ArregloPago_DocumentoAbono_Montos_Crear(
            CajasCrdDocumentoAfectacionData afectacion)
        {
            var montos = new CrArregloPagoDocumentoMontos
            {
                int_cor = afectacion.intcor,
                int_mor = afectacion.intmor,
                amortiza = afectacion.principal,
                cargos = afectacion.cargos,
                polizas = afectacion.polizas,
                iva = afectacion.iva
            };

            montos.monto_documento =
                montos.int_cor +
                montos.int_mor +
                montos.amortiza +
                montos.cargos;

            montos.monto_pago_final =
                montos.int_cor +
                montos.int_mor +
                montos.amortiza +
                montos.cargos +
                montos.polizas;

            return montos;
        }

        private static CrArregloPagoDocumentoMontos Cr_ArregloPago_DocumentoReadecuacion_Montos_Crear(
            CajasCrdDocumentoAfectacionData afectacion)
        {
            var montos = new CrArregloPagoDocumentoMontos
            {
                int_cor = afectacion.intcor,
                int_mor = afectacion.intmor,
                cargos = afectacion.cargos,
                polizas = afectacion.polizas
            };

            montos.amortiza =
                montos.int_cor +
                montos.int_mor +
                montos.cargos +
                montos.polizas;

            montos.monto_documento = montos.amortiza;
            montos.monto_pago_final = montos.amortiza;

            return montos;
        }

        private static CrArregloPagoDocumentoLineas Cr_ArregloPago_Documento_Lineas_Crear(
            CrArregloPagoDocumentoContext ctx,
            CrArregloPagoDocumentoMontos montos,
            CajasCrdOperacionProxPagoData proxPago,
            decimal saldoActual,
            CrArregloPagoDocumentoTipo tipoDocumento)
        {
            return tipoDocumento == CrArregloPagoDocumentoTipo.Abono
                ? Cr_ArregloPago_DocumentoAbono_Lineas_Crear(ctx, montos, proxPago, saldoActual)
                : Cr_ArregloPago_DocumentoReadecuacion_Lineas_Crear(ctx, montos, proxPago, saldoActual);
        }

        private static CrArregloPagoDocumentoLineas Cr_ArregloPago_DocumentoAbono_Lineas_Crear(
            CrArregloPagoDocumentoContext ctx,
            CrArregloPagoDocumentoMontos montos,
            CajasCrdOperacionProxPagoData proxPago,
            decimal saldoActual)
        {
            return new CrArregloPagoDocumentoLineas
            {
                linea1 = FormatearLineaDocumento(LineaSaldoAnterior, saldoActual),
                linea2 = FormatearLineaDocumento(LineaSaldoActual, saldoActual - montos.amortiza),
                linea3 = FormatearLineaDocumento(LineaInteresCorriente, montos.int_cor),
                linea4 = FormatearLineaDocumento(LineaInteresAtrasado, montos.int_mor),
                linea5 = FormatearLineaDocumento(LineaAmortizacion, montos.amortiza),
                linea6 = FormatearLineaDocumento(LineaCargosTotales, montos.cargos),
                linea7 = FormatearLineaDocumento(LineaPolizas, montos.polizas),
                linea8 = Cr_ArregloPago_DocumentoOperacion_Linea_Crear(ctx),
                linea9 = $"Descripcion       ..: {ctx.operacion.linea_desc}",
                linea10 = $"Notas: {proxPago.notas}",
                linea11 = montos.iva > 0
                    ? FormatearLineaDocumento("Monto IVA", montos.iva)
                    : string.Empty
            };
        }

        private static CrArregloPagoDocumentoLineas Cr_ArregloPago_DocumentoReadecuacion_Lineas_Crear(
            CrArregloPagoDocumentoContext ctx,
            CrArregloPagoDocumentoMontos montos,
            CajasCrdOperacionProxPagoData proxPago,
            decimal saldoActual)
        {
            var lineas = new CrArregloPagoDocumentoLineas
            {
                linea1 = FormatearLineaDocumento(LineaSaldoAnterior, saldoActual - montos.amortiza),
                linea2 = FormatearLineaDocumento(LineaSaldoActual, saldoActual),
                linea3 = FormatearLineaDocumento(LineaInteresCorriente, montos.int_cor),
                linea4 = FormatearLineaDocumento(LineaInteresAtrasado, montos.int_mor),
                linea5 = FormatearLineaDocumento(LineaCapitalizacion, montos.amortiza * -1),
                linea6 = FormatearLineaDocumento(LineaCargosTotales, montos.cargos),
                linea7 = FormatearLineaDocumento(LineaPolizas, montos.polizas),
                linea8 = Cr_ArregloPago_DocumentoOperacion_Linea_Crear(ctx),
                linea9 = $"Descripcion       ..: {ctx.operacion.linea_desc}",
                linea10 = string.Empty,
                linea11 = ctx.trasladar ? "Trasladar Principal" : ctx.operacion.linea_desc
            };

            if (!ctx.operacion.sys_plan_pagos)
            {
                return lineas;
            }

            lineas.linea9 = proxPago.fecha_pago.HasValue
                ? $"Prox.Pago..:{proxPago.fecha_pago.Value:dd/MM/yyyy} Cta.({proxPago.num_cuota}) {proxPago.cuota:N2}"
                : "Prox.Pago..: >> <<";
            lineas.linea10 = $"Notas: {proxPago.notas}";

            return lineas;
        }

        private static string Cr_ArregloPago_DocumentoOperacion_Linea_Crear(
            CrArregloPagoDocumentoContext ctx)
        {
            return $"Operacion/Linea   ..: Op.:{ctx.operacion.operacion} L.:{ctx.operacion.codigo}-{(ctx.operacion.opex == 1 ? "OPEX" : string.Empty)}";
        }

        private static void Cr_ArregloPago_Documento_Transaccion_Insertar(
            SqlConnection conn,
            SqlTransaction transac,
            CrArregloPagoDocumentoContext ctx,
            CrArregloPagoDocumentoMontos montos,
            CrArregloPagoDocumentoLineas lineas,
            CrArregloPagoDocumentoTipo tipoDocumento)
        {
            if (tipoDocumento == CrArregloPagoDocumentoTipo.Abono)
            {
                Cr_ArregloPago_DocumentoAbono_Transaccion_Insertar(conn, transac, ctx, montos, lineas);
                return;
            }

            Cr_ArregloPago_DocumentoReadecuacion_Transaccion_Insertar(conn, transac, ctx, montos, lineas);
        }

        private static void Cr_ArregloPago_DocumentoAbono_Transaccion_Insertar(
            SqlConnection conn,
            SqlTransaction tx,
            CrArregloPagoDocumentoContext ctx,
            CrArregloPagoDocumentoMontos montos,
            CrArregloPagoDocumentoLineas lineas)
        {
            const string sql = @"
                insert SIF_TRANSACCIONES
                (
                    COD_TRANSACCION,
                    TIPO_DOCUMENTO,
                    REGISTRO_FECHA,
                    REGISTRO_USUARIO,
                    Cliente_IDENTIFICACION,
                    CLIENTE_NOMBRE,
                    cod_concepto,
                    monto,
                    estado,
                    Referencia_01,
                    Referencia_02,
                    Referencia_03,
                    cod_oficina,
                    linea1,
                    linea2,
                    linea3,
                    linea4,
                    linea5,
                    linea6,
                    linea7,
                    linea8,
                    linea9,
                    linea10,
                    linea11,
                    detalle,
                    documento,
                    cod_caja,
                    cod_apertura
                )
                values
                (
                    @NumDoc,
                    @TipoDoc,
                    Getdate(),
                    @Usuario,
                    @Cedula,
                    @Nombre,
                    @Concepto,
                    @Monto,
                    'P',
                    @Operacion,
                    @Codigo,
                    '',
                    @OficinaTitular,
                    @Linea1,
                    @Linea2,
                    @Linea3,
                    @Linea4,
                    @Linea5,
                    @Linea6,
                    @Linea7,
                    @Linea8,
                    @Linea9,
                    @Linea10,
                    @Linea11,
                    @Detalle,
                    '',
                    @Caja,
                    @Apertura
                );";

            conn.Execute(sql, Cr_ArregloPago_DocumentoInsertParametros_Crear(ctx, montos, lineas));
        }

        private static void Cr_ArregloPago_DocumentoReadecuacion_Transaccion_Insertar(
            SqlConnection conn,
            SqlTransaction transac,
            CrArregloPagoDocumentoContext ctx,
            CrArregloPagoDocumentoMontos montos,
            CrArregloPagoDocumentoLineas lineas)
        {
            const string sql = @"
                insert SIF_TRANSACCIONES
                (
                    COD_TRANSACCION,
                    TIPO_DOCUMENTO,
                    REGISTRO_FECHA,
                    REGISTRO_USUARIO,
                    Cliente_IDENTIFICACION,
                    CLIENTE_NOMBRE,
                    cod_concepto,
                    monto,
                    estado,
                    Referencia_01,
                    Referencia_02,
                    Referencia_03,
                    cod_oficina,
                    linea1,
                    linea2,
                    linea3,
                    linea4,
                    linea5,
                    linea6,
                    linea7,
                    linea8,
                    linea9,
                    linea10,
                    detalle,
                    documento,
                    linea11
                )
                values
                (
                    @NumDoc,
                    @TipoDoc,
                    Getdate(),
                    @Usuario,
                    @Cedula,
                    @Nombre,
                    @Concepto,
                    @Monto,
                    'P',
                    @Operacion,
                    @Codigo,
                    '',
                    @OficinaTitular,
                    @Linea1,
                    @Linea2,
                    @Linea3,
                    @Linea4,
                    @Linea5,
                    @Linea6,
                    @Linea7,
                    @Linea8,
                    @Linea9,
                    @Linea10,
                    @Detalle,
                    '',
                    @Linea11
                );";

            conn.Execute(sql, Cr_ArregloPago_DocumentoInsertParametros_Crear(ctx, montos, lineas), transac);
        }

        private static object Cr_ArregloPago_DocumentoInsertParametros_Crear(
            CrArregloPagoDocumentoContext ctx,
            CrArregloPagoDocumentoMontos montos,
            CrArregloPagoDocumentoLineas lineas)
        {
            return new
            {
                NumDoc = ctx.num_doc,
                TipoDoc = ctx.tipo_doc,
                Usuario = ctx.usuario,
                Cedula = ctx.operacion.cedula.Trim(),
                Nombre = ctx.operacion.nombre.Trim(),
                Concepto = ctx.concepto,
                Monto = montos.monto_documento,
                Operacion = ctx.operacion.operacion.ToString(),
                Codigo = ctx.operacion.codigo,
                OficinaTitular = ctx.globales.GOficinaTitular ?? string.Empty,
                Linea1 = lineas.linea1,
                Linea2 = lineas.linea2,
                Linea3 = lineas.linea3,
                Linea4 = lineas.linea4,
                Linea5 = lineas.linea5,
                Linea6 = lineas.linea6,
                Linea7 = lineas.linea7,
                Linea8 = lineas.linea8,
                Linea9 = lineas.linea9,
                Linea10 = lineas.linea10,
                Linea11 = lineas.linea11,
                Detalle = ctx.notas,
                Caja = ctx.caja.caja,
                Apertura = ctx.caja.apertura
            };
        }

        private void Cr_ArregloPago_AsientosDocumento_Registrar(
            SqlConnection conn,
            SqlTransaction tx,
            CrArregloPagoDocumentoContext ctx,
            CrArregloPagoTipoCambioContext tipoCambio,
            CajasCrdOperacionCtasData ctas,
            CrArregloPagoDocumentoMontos montos,
            CrArregloPagoDocumentoTipo tipoDocumento)
        {
            var tipoAmortiza = tipoDocumento == CrArregloPagoDocumentoTipo.Readecuacion ? "D" : "C";

            Cr_ArregloPago_AsientoMonto_Registrar(
                conn,
                tx,
                ctx.globales,
                Cr_ArregloPago_AsientoRequest_Crear(
                    ctx,
                    new CrArregloPagoAsientoFactoryData
                    {
                        monto = montos.int_cor * tipoCambio.factor,
                        tipo = "C",
                        divisa = ctas.cod_divisa,
                        tipo_cambio = tipoCambio.tipo_cambio,
                        unidad = ctas.cod_unidad,
                        centro_costo = ctas.cod_centro_costo,
                        cuenta = ctas.ctaintc,
                        operacion = ctas.id_solicitud,
                        codigo = ctas.codigo
                    }));

            Cr_ArregloPago_AsientoMonto_Registrar(
                conn,
                tx,
                ctx.globales,
                Cr_ArregloPago_AsientoRequest_Crear(
                    ctx,
                    new CrArregloPagoAsientoFactoryData
                    {
                        monto = montos.int_mor * tipoCambio.factor,
                        tipo = "C",
                        divisa = ctas.cod_divisa,
                        tipo_cambio = tipoCambio.tipo_cambio,
                        unidad = ctas.cod_unidad,
                        centro_costo = ctas.cod_centro_costo,
                        cuenta = ctas.ctaintm,
                        operacion = ctas.id_solicitud,
                        codigo = ctas.codigo
                    }));

            Cr_ArregloPago_AsientoCargos_Registrar(
                conn,
                tx,
                ctx,
                tipoCambio,
                ctas,
                montos.cargos);

            Cr_ArregloPago_AsientoPoliza_Registrar(
                conn,
                tx,
                ctx,
                tipoCambio,
                ctas,
                montos.polizas);

            Cr_ArregloPago_AsientoMonto_Registrar(
                conn,
                tx,
                ctx.globales,
                Cr_ArregloPago_AsientoRequest_Crear(
                    ctx,
                    new CrArregloPagoAsientoFactoryData
                    {
                        monto = montos.amortiza * tipoCambio.factor,
                        tipo = tipoAmortiza,
                        divisa = ctas.cod_divisa,
                        tipo_cambio = tipoCambio.tipo_cambio,
                        unidad = ctas.cod_unidad,
                        centro_costo = ctas.cod_centro_costo,
                        cuenta = ctas.ctaamortiza,
                        operacion = ctas.id_solicitud,
                        codigo = ctas.codigo
                    }));
        }

        private void Cr_ArregloPago_AsientoCargos_Registrar(
            SqlConnection conn,
            SqlTransaction tx,
            CrArregloPagoDocumentoContext ctx,
            CrArregloPagoTipoCambioContext tipoCambio,
            CajasCrdOperacionCtasData ctas,
            decimal montoCargo)
        {
            if (montoCargo <= 0)
            {
                return;
            }

            if (!ctx.operacion.sys_plan_pagos)
            {
                Cr_ArregloPago_AsientoMonto_Registrar(
                    conn,
                    tx,
                    ctx.globales,
                    Cr_ArregloPago_AsientoRequest_Crear(
                        ctx,
                        new CrArregloPagoAsientoFactoryData
                        {
                            monto = montoCargo * tipoCambio.factor,
                            tipo = "C",
                            divisa = ctas.cod_divisa,
                            tipo_cambio = tipoCambio.tipo_cambio,
                            unidad = ctas.cod_unidad,
                            centro_costo = ctas.cod_centro_costo,
                            cuenta = ctas.ctacargos,
                            operacion = ctas.id_solicitud,
                            codigo = ctas.codigo
                        }));
                return;
            }

            var cargos = Cr_ArregloPago_DocumentoAfectacionCargos_Obtener(
                conn,
                tx,
                ctx.tipo_doc,
                ctx.num_doc);

            foreach (var item in cargos)
            {
                Cr_ArregloPago_AsientoMonto_Registrar(
                    conn,
                    tx,
                    ctx.globales,
                    Cr_ArregloPago_AsientoRequest_Crear(
                        ctx,
                        new CrArregloPagoAsientoFactoryData
                        {
                            monto = (item.mov_monto ?? 0) * tipoCambio.factor,
                            tipo = "C",
                            divisa = ctas.cod_divisa,
                            tipo_cambio = tipoCambio.tipo_cambio,
                            unidad = item.cod_unidad,
                            centro_costo = item.cod_centro_costo,
                            cuenta = item.cod_cuenta,
                            operacion = item.id_solicitud,
                            codigo = item.codigo
                        }));
            }
        }

        private void Cr_ArregloPago_AsientoPoliza_Registrar(
            SqlConnection conn,
            SqlTransaction tx,
            CrArregloPagoDocumentoContext ctx,
            CrArregloPagoTipoCambioContext tipoCambio,
            CajasCrdOperacionCtasData ctas,
            decimal montoPoliza)
        {
            if (montoPoliza <= 0)
            {
                return;
            }

            var cuentaPoliza = Cr_ArregloPago_CuentaPoliza_Obtener(
                conn,
                tx,
                ctx.operacion.operacion);

            Cr_ArregloPago_AsientoMonto_Registrar(
                conn,
                tx,
                ctx.globales,
                Cr_ArregloPago_AsientoRequest_Crear(
                    ctx,
                    new CrArregloPagoAsientoFactoryData
                    {
                        monto = montoPoliza * tipoCambio.factor,
                        tipo = "C",
                        divisa = ctas.cod_divisa,
                        tipo_cambio = tipoCambio.tipo_cambio,
                        unidad = ctas.cod_unidad,
                        centro_costo = ctas.cod_centro_costo,
                        cuenta = cuentaPoliza,
                        operacion = ctas.id_solicitud,
                        codigo = ctas.codigo
                    }));
        }

        private void Cr_ArregloPago_DocumentoPagoFinal_Registrar(
            SqlConnection conn,
            SqlTransaction tx,
            CrArregloPagoDocumentoContext ctx,
            CajasCrdOperacionCtasData ctas,
            CrArregloPagoDocumentoMontos montos)
        {
            if (montos.monto_pago_final <= 0)
            {
                return;
            }

            conn.Execute(
                @"exec spCajas_DesglocePagosDocFinal
                    @Caja,
                    @Apertura,
                    @Tiquete,
                    @Usuario,
                    @TipoDoc,
                    @NumDoc,
                    @Unidad,
                    @Operacion,
                    @Codigo;",
                new
                {
                    Caja = ctx.caja.caja,
                    Apertura = ctx.caja.apertura,
                    Tiquete = ctx.caja.tiquete,
                    Usuario = ctx.usuario,
                    TipoDoc = ctx.tipo_doc,
                    NumDoc = ctx.num_doc,
                    Unidad = ctx.caja.unidad,
                    Operacion = ctas.id_solicitud,
                    Codigo = ctas.codigo
                },
                tx);
        }

        private static CrArregloPagoAsientoRequest Cr_ArregloPago_AsientoRequest_Crear(
            CrArregloPagoDocumentoContext ctx,
            CrArregloPagoAsientoFactoryData data)
        {
            return new CrArregloPagoAsientoRequest
            {
                tipo_doc = ctx.tipo_doc,
                num_doc = ctx.num_doc,
                monto = data.monto,
                tipo = data.tipo,
                divisa = data.divisa,
                tipo_cambio = data.tipo_cambio,
                unidad = data.unidad,
                centro_costo = data.centro_costo,
                cuenta = data.cuenta,
                operacion = data.operacion,
                codigo = data.codigo
            };
        }

        private void Cr_ArregloPago_AsientoMonto_Registrar(
            SqlConnection conn,
            SqlTransaction tx,
            Globales globales,
            CrArregloPagoAsientoRequest request)
        {
            if (request.monto <= 0)
            {
                return;
            }

            conn.Execute(
                @"exec spSIFDocsAsiento
                    @TipoDoc,
                    @NumDoc,
                    @Monto,
                    @Tipo,
                    @Divisa,
                    @TipoCambio,
                    @Enlace,
                    @Unidad,
                    @CentroCosto,
                    @Cuenta,
                    @Operacion,
                    @Codigo,
                    '';",
                new
                {
                    TipoDoc = request.tipo_doc,
                    NumDoc = request.num_doc,
                    Monto = request.monto,
                    Tipo = request.tipo,
                    Divisa = request.divisa,
                    TipoCambio = request.tipo_cambio,
                    Enlace = globales.GEnlace,
                    Unidad = request.unidad,
                    CentroCosto = request.centro_costo,
                    Cuenta = request.cuenta,
                    Operacion = request.operacion,
                    Codigo = request.codigo
                },
                tx);
        }

        private CajasCrdDocumentoAfectacionData Cr_ArregloPago_DocumentoAfectacion_Obtener(
            SqlConnection conn,
            SqlTransaction tx,
            bool sysPlanPagos,
            string tipoDoc,
            string numDoc)
        {
            return conn.QueryFirstOrDefault<CajasCrdDocumentoAfectacionData>(
                sysPlanPagos
                    ? "exec spCrdDocumentoAfectacion @TipoDoc, @NumDoc, 'R';"
                    : "exec spCrdDocumentoAfectacionStP @TipoDoc, @NumDoc, 'R';",
                new { TipoDoc = tipoDoc, NumDoc = numDoc },
                tx) ?? new CajasCrdDocumentoAfectacionData();
        }

        private CajasCrdOperacionCtasData Cr_ArregloPago_OperacionCtas_Obtener(
            SqlConnection conn,
            SqlTransaction tx,
            int operacion)
        {
            return conn.QueryFirstOrDefault<CajasCrdOperacionCtasData>(
                "exec spCrdOperacionCtas @Operacion;",
                new { Operacion = operacion },
                tx) ?? new CajasCrdOperacionCtasData();
        }

        private List<CajasCrdDocAfectacionCargoRow> Cr_ArregloPago_DocumentoAfectacionCargos_Obtener(
            SqlConnection conn,
            SqlTransaction tx,
            string tipoDoc,
            string numDoc)
        {
            return conn.Query<CajasCrdDocAfectacionCargoRow>(
                "exec spCrdDocumentoAfectacionCargos @TipoDoc, @NumDoc;",
                new { TipoDoc = tipoDoc, NumDoc = numDoc },
                tx).ToList();
        }

        private string Cr_ArregloPago_CuentaPoliza_Obtener(
            SqlConnection conn,
            SqlTransaction tx,
            int operacion)
        {
            return conn.QueryFirstOrDefault<string>(
                "select dbo.fxCrdOperacionCtaContaPolizas(@Operacion);",
                new { Operacion = operacion },
                tx) ?? string.Empty;
        }
    }
}