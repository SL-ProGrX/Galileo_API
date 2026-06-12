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
        private ErrorDto SbDocumentoReadecuacion(
            int codEmpresa,
            string usuario,
            CrArregloPagoOperacionData operacion,
            string tipoDoc,
            string numDoc,
            string concepto,
            bool trasladar)
        {
            try
            {
                using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);

                decimal tipoCambio = 1;
                string aseDocDeposito = string.Empty;
                string aseDocDetalle = string.Empty;
                string oficinaTitular = string.Empty;
                string cuentaPoliza = string.Empty;

                var globalesResp = ObtenerGlobales(codEmpresa, usuario);
                if (globalesResp.Code == 0 && globalesResp.Result is not null)
                {
                    oficinaTitular = globalesResp.Result.GOficinaTitular ?? string.Empty;
                }

                var afectacion = operacion.sys_plan_pagos
                    ? ObtenerAfectacionDocumento(codEmpresa, tipoDoc, numDoc)
                    : ObtenerAfectacionDocumentoStp(codEmpresa, tipoDoc, numDoc);

                decimal curIntC = afectacion.intcor;
                decimal curIntM = afectacion.intmor;
                decimal curCargo = afectacion.cargos;
                decimal curPoliza = afectacion.polizas;
                decimal curAmortiza = afectacion.intcor + afectacion.intmor + afectacion.cargos + afectacion.polizas;

                var saldoResp = DbHelper.ExecuteSingleQuery<decimal>(
                    _portalDb,
                    codEmpresa,
                    "select isnull(saldo, 0) from reg_creditos where id_solicitud = @Operacion;",
                    0,
                    new { Operacion = operacion.operacion });

                decimal curSaldo = saldoResp.Result;

                var ctas = ObtenerCuentasOperacion(codEmpresa, operacion.operacion);

                string linea1 = FormatearLineaDocumento("Saldo Anterior", curSaldo - curAmortiza);
                string linea2 = FormatearLineaDocumento("Saldo Actual", curSaldo);
                string linea3 = FormatearLineaDocumento("Interes Corriente", curIntC);
                string linea4 = FormatearLineaDocumento("Interes Atrasado", curIntM);
                string linea5 = FormatearLineaDocumento("Capitalizacion", curAmortiza * -1);
                string linea6 = FormatearLineaDocumento("Cargos Totales", curCargo);
                string linea7 = FormatearLineaDocumento("Polizas", curPoliza);
                string linea8 = $"Operacion/Linea   ..: Op.:{operacion.operacion} L.:{operacion.codigo}-{(operacion.opex == 1 ? "OPEX" : string.Empty)}";
                string linea9 = $"Descripcion       ..: {operacion.linea_desc}";
                string linea10 = string.Empty;
                string linea11 = trasladar ? "Trasladar Principal" : operacion.linea_desc;

                if (operacion.sys_plan_pagos)
                {
                    var proxPago = ObtenerProxPago(codEmpresa, operacion.operacion);
                    linea9 = proxPago.fecha_pago.HasValue
                        ? $"Prox.Pago..:{proxPago.fecha_pago.Value:dd/MM/yyyy} Cta.({proxPago.num_cuota}) {proxPago.cuota:N2}"
                        : "Prox.Pago..: >> <<";
                    linea10 = $"Notas: {proxPago.notas}";
                }

                const string sqlInsert = @"
                insert into SIF_TRANSACCIONES
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
                    @AseDocDeposito,
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
                    @Documento,
                    @Linea11
                );";

                var insertResp = DbHelper.ExecuteNonQuery(
                    _portalDb,
                    codEmpresa,
                    sqlInsert,
                    new
                    {
                        NumDoc = numDoc,
                        TipoDoc = tipoDoc,
                        Usuario = usuario,
                        Cedula = operacion.cedula.Trim(),
                        Nombre = operacion.nombre.Trim(),
                        Concepto = concepto,
                        Monto = curAmortiza,
                        Operacion = operacion.operacion.ToString(),
                        Codigo = operacion.codigo,
                        AseDocDeposito = aseDocDeposito,
                        OficinaTitular = oficinaTitular,
                        Linea1 = linea1,
                        Linea2 = linea2,
                        Linea3 = linea3,
                        Linea4 = linea4,
                        Linea5 = linea5,
                        Linea6 = linea6,
                        Linea7 = linea7,
                        Linea8 = linea8,
                        Linea9 = linea9,
                        Linea10 = linea10,
                        Detalle = aseDocDetalle,
                        Documento = aseDocDeposito,
                        Linea11 = linea11
                    });

                if (insertResp.Code != 0)
                {
                    return insertResp;
                }

                if (curAmortiza > 0)
                {
                    var asientoResp = RegistrarAsientoDocumento(
                        codEmpresa,
                        tipoDoc,
                        numDoc,
                        curAmortiza * tipoCambio,
                        "D",
                        ctas.cod_divisa,
                        tipoCambio,
                        ctas.cod_unidad,
                        ctas.cod_centro_costo,
                        ctas.ctaamortiza,
                        ctas.id_solicitud,
                        ctas.codigo,
                        aseDocDeposito);

                    if (asientoResp.Code != 0)
                    {
                        return asientoResp;
                    }
                }

                if (curIntC > 0)
                {
                    var asientoResp = RegistrarAsientoDocumento(
                        codEmpresa,
                        tipoDoc,
                        numDoc,
                        curIntC * tipoCambio,
                        "C",
                        ctas.cod_divisa,
                        tipoCambio,
                        ctas.cod_unidad,
                        ctas.cod_centro_costo,
                        ctas.ctaintc,
                        ctas.id_solicitud,
                        ctas.codigo,
                        aseDocDeposito);

                    if (asientoResp.Code != 0)
                    {
                        return asientoResp;
                    }
                }

                if (curIntM > 0)
                {
                    var asientoResp = RegistrarAsientoDocumento(
                        codEmpresa,
                        tipoDoc,
                        numDoc,
                        curIntM * tipoCambio,
                        "C",
                        ctas.cod_divisa,
                        tipoCambio,
                        ctas.cod_unidad,
                        ctas.cod_centro_costo,
                        ctas.ctaintm,
                        ctas.id_solicitud,
                        ctas.codigo,
                        aseDocDeposito);

                    if (asientoResp.Code != 0)
                    {
                        return asientoResp;
                    }
                }

                if (curCargo > 0)
                {
                    if (!operacion.sys_plan_pagos)
                    {
                        var asientoResp = RegistrarAsientoDocumento(
                            codEmpresa,
                            tipoDoc,
                            numDoc,
                            curCargo * tipoCambio,
                            "C",
                            ctas.cod_divisa,
                            tipoCambio,
                            ctas.cod_unidad,
                            ctas.cod_centro_costo,
                            ctas.ctacargos,
                            ctas.id_solicitud,
                            ctas.codigo,
                            aseDocDeposito);

                        if (asientoResp.Code != 0)
                        {
                            return asientoResp;
                        }
                    }
                    else
                    {
                        var cargos = ObtenerAfectacionDocumentoCargos(codEmpresa, tipoDoc, numDoc);
                        foreach (var item in cargos)
                        {
                            var asientoResp = RegistrarAsientoDocumento(
                                codEmpresa,
                                tipoDoc,
                                numDoc,
                                (item.mov_monto ?? 0) * tipoCambio,
                                "C",
                                ctas.cod_divisa,
                                tipoCambio,
                                item.cod_unidad,
                                item.cod_centro_costo,
                                item.cod_cuenta,
                                item.id_solicitud,
                                item.codigo,
                                aseDocDeposito);

                            if (asientoResp.Code != 0)
                            {
                                return asientoResp;
                            }
                        }
                    }
                }

                if (curPoliza > 0)
                {
                    var cuentaPolizaResp = DbHelper.ExecuteSingleQuery<string>(
                        _portalDb,
                        codEmpresa,
                        "select dbo.fxCrdOperacionCtaContaPolizas(@Operacion);",
                        string.Empty,
                        new { Operacion = operacion.operacion });

                    cuentaPoliza = cuentaPolizaResp.Result ?? string.Empty;

                    var asientoResp = RegistrarAsientoDocumento(
                        codEmpresa,
                        tipoDoc,
                        numDoc,
                        curPoliza * tipoCambio,
                        "C",
                        ctas.cod_divisa,
                        tipoCambio,
                        ctas.cod_unidad,
                        ctas.cod_centro_costo,
                        cuentaPoliza,
                        ctas.id_solicitud,
                        ctas.codigo,
                        aseDocDeposito);

                    if (asientoResp.Code != 0)
                    {
                        return asientoResp;
                    }
                }

                return DbHelper.CreateOkResponse();
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        private ErrorDto SbDocumentoAbono(
            int codEmpresa,
            string usuario,
            CrArregloPagoOperacionData operacion,
            string tipoDoc,
            string numDoc,
            string concepto)
        {
            try
            {
                decimal tipoCambio = 1;
                string aseDocDeposito = string.Empty;
                string aseDocDetalle = string.Empty;
                string oficinaTitular = string.Empty;
                string cuentaPoliza = string.Empty;

                var globalesResp = ObtenerGlobales(codEmpresa, usuario);
                if (globalesResp.Code == 0 && globalesResp.Result is not null)
                {
                    oficinaTitular = globalesResp.Result.GOficinaTitular ?? string.Empty;
                }

                var afectacion = operacion.sys_plan_pagos
                    ? ObtenerAfectacionDocumento(codEmpresa, tipoDoc, numDoc)
                    : ObtenerAfectacionDocumentoStp(codEmpresa, tipoDoc, numDoc);

                var proxPago = ObtenerProxPago(codEmpresa, operacion.operacion);
                var ctas = ObtenerCuentasOperacion(codEmpresa, operacion.operacion);

                decimal curIntC = afectacion.intcor;
                decimal curIntM = afectacion.intmor;
                decimal curAmortiza = afectacion.principal;
                decimal curCargo = afectacion.cargos;
                decimal curPoliza = afectacion.polizas;
                decimal montoTotal = curIntC + curIntM + curAmortiza + curCargo + curPoliza + afectacion.iva;

                string linea1 = FormatearLineaDocumento("Saldo Anterior", operacion.saldo);
                string linea2 = FormatearLineaDocumento("Saldo Actual", operacion.saldo - curAmortiza);
                string linea3 = FormatearLineaDocumento("Interes Corriente", curIntC);
                string linea4 = FormatearLineaDocumento("Interes Atrasado", curIntM);
                string linea5 = FormatearLineaDocumento("Amortizacion", curAmortiza);
                string linea6 = FormatearLineaDocumento("Cargos Totales", curCargo);
                string linea7 = FormatearLineaDocumento("Polizas", curPoliza);
                string linea8 = $"Operacion/Linea   ..: Op.:{operacion.operacion} L.:{operacion.codigo}-{(operacion.retencion ? "Ret.:SI" : string.Empty)}";
                string linea9 = $"Descripcion       ..: {operacion.linea_desc}";
                string linea10 = $"Notas: {proxPago.notas}";
                string linea11 = afectacion.iva > 0
                    ? FormatearLineaDocumento("Monto IVA", afectacion.iva)
                    : string.Empty;

                const string sqlInsert = @"
            insert into SIF_TRANSACCIONES
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
                documento
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
                @AseDocDeposito,
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
                @Documento
            );";

                var insertResp = DbHelper.ExecuteNonQuery(
                    _portalDb,
                    codEmpresa,
                    sqlInsert,
                    new
                    {
                        NumDoc = numDoc,
                        TipoDoc = tipoDoc,
                        Usuario = usuario,
                        Cedula = operacion.cedula.Trim(),
                        Nombre = operacion.nombre.Trim(),
                        Concepto = concepto,
                        Monto = montoTotal,
                        Operacion = operacion.operacion.ToString(),
                        Codigo = operacion.codigo,
                        AseDocDeposito = aseDocDeposito,
                        OficinaTitular = oficinaTitular,
                        Linea1 = linea1,
                        Linea2 = linea2,
                        Linea3 = linea3,
                        Linea4 = linea4,
                        Linea5 = linea5,
                        Linea6 = linea6,
                        Linea7 = linea7,
                        Linea8 = linea8,
                        Linea9 = linea9,
                        Linea10 = linea10,
                        Linea11 = linea11,
                        Detalle = aseDocDetalle,
                        Documento = aseDocDeposito
                    });

                if (insertResp.Code != 0)
                {
                    return insertResp;
                }

                if (curIntC > 0)
                {
                    var resp = RegistrarAsientoDocumento(
                        codEmpresa, tipoDoc, numDoc, curIntC * tipoCambio, "C",
                        ctas.cod_divisa, tipoCambio, ctas.cod_unidad, ctas.cod_centro_costo,
                        ctas.ctaintc, ctas.id_solicitud, ctas.codigo, aseDocDeposito);

                    if (resp.Code != 0) return resp;
                }

                if (curIntM > 0)
                {
                    var resp = RegistrarAsientoDocumento(
                        codEmpresa, tipoDoc, numDoc, curIntM * tipoCambio, "C",
                        ctas.cod_divisa, tipoCambio, ctas.cod_unidad, ctas.cod_centro_costo,
                        ctas.ctaintm, ctas.id_solicitud, ctas.codigo, aseDocDeposito);

                    if (resp.Code != 0) return resp;
                }

                if (curAmortiza > 0)
                {
                    var resp = RegistrarAsientoDocumento(
                        codEmpresa, tipoDoc, numDoc, curAmortiza * tipoCambio, "C",
                        ctas.cod_divisa, tipoCambio, ctas.cod_unidad, ctas.cod_centro_costo,
                        ctas.ctaamortiza, ctas.id_solicitud, ctas.codigo, aseDocDeposito);

                    if (resp.Code != 0) return resp;
                }

                if (curCargo > 0)
                {
                    if (!operacion.sys_plan_pagos)
                    {
                        var resp = RegistrarAsientoDocumento(
                            codEmpresa, tipoDoc, numDoc, curCargo * tipoCambio, "C",
                            ctas.cod_divisa, tipoCambio, ctas.cod_unidad, ctas.cod_centro_costo,
                            ctas.ctacargos, ctas.id_solicitud, ctas.codigo, aseDocDeposito);

                        if (resp.Code != 0) return resp;
                    }
                    else
                    {
                        var cargos = ObtenerAfectacionDocumentoCargos(codEmpresa, tipoDoc, numDoc);

                        foreach (var item in cargos)
                        {
                            var resp = RegistrarAsientoDocumento(
                                codEmpresa,
                                tipoDoc,
                                numDoc,
                                (item.mov_monto ?? 0) * tipoCambio,
                                "C",
                                ctas.cod_divisa,
                                tipoCambio,
                                item.cod_unidad,
                                item.cod_centro_costo,
                                item.cod_cuenta,
                                item.id_solicitud,
                                item.codigo,
                                aseDocDeposito);

                            if (resp.Code != 0) return resp;
                        }
                    }
                }

                if (curPoliza > 0)
                {
                    var cuentaPolizaResp = DbHelper.ExecuteSingleQuery<string>(
                        _portalDb,
                        codEmpresa,
                        "select dbo.fxCrdOperacionCtaContaPolizas(@Operacion);",
                        string.Empty,
                        new { Operacion = operacion.operacion });

                    cuentaPoliza = cuentaPolizaResp.Result ?? string.Empty;

                    var resp = RegistrarAsientoDocumento(
                        codEmpresa, tipoDoc, numDoc, curPoliza * tipoCambio, "C",
                        ctas.cod_divisa, tipoCambio, ctas.cod_unidad, ctas.cod_centro_costo,
                        cuentaPoliza, ctas.id_solicitud, ctas.codigo, aseDocDeposito);

                    if (resp.Code != 0) return resp;
                }

                return DbHelper.CreateOkResponse();
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        private ErrorDto Cr_ArregloPago_DocumentoAbono_Generar(
            SqlConnection conn,
            SqlTransaction tx,
            int codEmpresa,
            Globales globales,
            CrArregloPagoOperacionData operacion,
            CrArregloPagoCajaContexto cajaCtx,
            string usuario,
            string tipoDoc,
            string numDoc,
            string concepto,
            string notas)
        {
            try
            {
                var tipoCambio = _mCajas.fxCajasTipoCambio(codEmpresa, globales.GEnlace, cajaCtx.divisa);
                var factor = Convert.ToDecimal(MProGrxMain.fxSys_Tipo_Cambio_Apl(tipoCambio));

                var afectacion = Cr_ArregloPago_DocumentoAfectacion_Obtener(
                    conn,
                    tx,
                    operacion.sys_plan_pagos,
                    tipoDoc,
                    numDoc);

                var ctas = Cr_ArregloPago_OperacionCtas_Obtener(conn, tx, operacion.operacion);

                var curIntC = afectacion.intcor;
                var curIntM = afectacion.intmor;
                var curAmortiza = afectacion.principal;
                var curCargo = afectacion.cargos;
                var curPoliza = afectacion.polizas;
                var montoDocumento = curIntC + curIntM + curAmortiza + curCargo;
                var montoPagoFinal = curIntC + curIntM + curPoliza + curCargo + curAmortiza;

                var linea1 = FormatearLineaDocumento("Saldo Anterior", operacion.saldo);
                var linea2 = FormatearLineaDocumento("Saldo Actual", operacion.saldo - curAmortiza);
                var linea3 = FormatearLineaDocumento("Interes Corriente", curIntC);
                var linea4 = FormatearLineaDocumento("Interes Atrasado", curIntM);
                var linea5 = FormatearLineaDocumento("Amortizacion", curAmortiza);
                var linea6 = FormatearLineaDocumento("Cargos Totales", curCargo);
                var linea7 = FormatearLineaDocumento("Polizas", curPoliza);
                var linea8 = $"Operacion/Linea   ..: Op.:{operacion.operacion} L.:{operacion.codigo}-{(operacion.opex == 1 ? "OPEX" : string.Empty)}";
                var linea9 = $"Descripcion       ..: {operacion.linea_desc}";
                var linea10 = string.Empty;
                var linea11 = string.Empty;

                conn.Execute(
                    @"
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
                    );",
                    new
                    {
                        NumDoc = numDoc,
                        TipoDoc = tipoDoc,
                        Usuario = usuario,
                        Cedula = operacion.cedula.Trim(),
                        Nombre = operacion.nombre.Trim(),
                        Concepto = concepto,
                        Monto = montoDocumento,
                        Operacion = operacion.operacion.ToString(),
                        Codigo = operacion.codigo,
                        OficinaTitular = globales.GOficinaTitular ?? string.Empty,
                        Linea1 = linea1,
                        Linea2 = linea2,
                        Linea3 = linea3,
                        Linea4 = linea4,
                        Linea5 = linea5,
                        Linea6 = linea6,
                        Linea7 = linea7,
                        Linea8 = linea8,
                        Linea9 = linea9,
                        Linea10 = linea10,
                        Linea11 = linea11,
                        Detalle = notas,
                        Caja = cajaCtx.caja,
                        Apertura = cajaCtx.apertura
                    },
                    tx);

                if (curIntC > 0)
                {
                    Cr_ArregloPago_AsientoDocumento_Registrar(
                        conn, tx, globales, tipoDoc, numDoc, curIntC * factor, "C", ctas.cod_divisa,
                        tipoCambio, ctas.cod_unidad, ctas.cod_centro_costo, ctas.ctaintc,
                        ctas.id_solicitud, ctas.codigo);
                }

                if (curIntM > 0)
                {
                    Cr_ArregloPago_AsientoDocumento_Registrar(
                        conn, tx, globales, tipoDoc, numDoc, curIntM * factor, "C", ctas.cod_divisa,
                        tipoCambio, ctas.cod_unidad, ctas.cod_centro_costo, ctas.ctaintm,
                        ctas.id_solicitud, ctas.codigo);
                }

                if (curCargo > 0)
                {
                    if (!operacion.sys_plan_pagos)
                    {
                        Cr_ArregloPago_AsientoDocumento_Registrar(
                            conn, tx, globales, tipoDoc, numDoc, curCargo * factor, "C", ctas.cod_divisa,
                            tipoCambio, ctas.cod_unidad, ctas.cod_centro_costo, ctas.ctacargos,
                            ctas.id_solicitud, ctas.codigo);
                    }
                    else
                    {
                        var cargos = Cr_ArregloPago_DocumentoAfectacionCargos_Obtener(conn, tx, tipoDoc, numDoc);
                        foreach (var item in cargos)
                        {
                            Cr_ArregloPago_AsientoDocumento_Registrar(
                                conn, tx, globales, tipoDoc, numDoc, (item.mov_monto ?? 0) * factor, "C", ctas.cod_divisa,
                                tipoCambio, item.cod_unidad, item.cod_centro_costo, item.cod_cuenta,
                                item.id_solicitud, item.codigo);
                        }
                    }
                }

                if (curPoliza > 0)
                {
                    var cuentaPoliza = Cr_ArregloPago_CuentaPoliza_Obtener(conn, tx, operacion.operacion);

                    Cr_ArregloPago_AsientoDocumento_Registrar(
                        conn, tx, globales, tipoDoc, numDoc, curPoliza * factor, "C", ctas.cod_divisa,
                        tipoCambio, ctas.cod_unidad, ctas.cod_centro_costo, cuentaPoliza,
                        ctas.id_solicitud, ctas.codigo);
                }

                if (curAmortiza > 0)
                {
                    Cr_ArregloPago_AsientoDocumento_Registrar(
                        conn, tx, globales, tipoDoc, numDoc, curAmortiza * factor, "C", ctas.cod_divisa,
                        tipoCambio, ctas.cod_unidad, ctas.cod_centro_costo, ctas.ctaamortiza,
                        ctas.id_solicitud, ctas.codigo);
                }

                if (montoPagoFinal > 0)
                {
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
                            Caja = cajaCtx.caja,
                            Apertura = cajaCtx.apertura,
                            Tiquete = cajaCtx.tiquete,
                            Usuario = usuario,
                            TipoDoc = tipoDoc,
                            NumDoc = numDoc,
                            Unidad = cajaCtx.unidad,
                            Operacion = ctas.id_solicitud,
                            Codigo = ctas.codigo
                        },
                        tx);
                }

                return DbHelper.CreateOkResponse();
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        private ErrorDto Cr_ArregloPago_DocumentoReadecuacion_Generar(
            SqlConnection conn,
            SqlTransaction tx,
            int codEmpresa,
            Globales globales,
            CrArregloPagoOperacionData operacion,
            CrArregloPagoCajaContexto cajaCtx,
            string usuario,
            string tipoDoc,
            string numDoc,
            string concepto,
            bool trasladar,
            string notas)
        {
            try
            {
                var tipoCambio = _mCajas.fxCajasTipoCambio(codEmpresa, globales.GEnlace, cajaCtx.divisa);
                var factor = Convert.ToDecimal(MProGrxMain.fxSys_Tipo_Cambio_Apl(tipoCambio));

                var afectacion = Cr_ArregloPago_DocumentoAfectacion_Obtener(
                    conn,
                    tx,
                    operacion.sys_plan_pagos,
                    tipoDoc,
                    numDoc);

                var ctas = Cr_ArregloPago_OperacionCtas_Obtener(conn, tx, operacion.operacion);
                var curSaldo = conn.QueryFirstOrDefault<decimal>(
                    "select isnull(saldo, 0) from reg_creditos where id_solicitud = @Operacion;",
                    new { Operacion = operacion.operacion },
                    tx);

                var curIntC = afectacion.intcor;
                var curIntM = afectacion.intmor;
                var curCargo = afectacion.cargos;
                var curPoliza = afectacion.polizas;
                var curAmortiza = curIntC + curIntM + curCargo + curPoliza;

                var linea1 = FormatearLineaDocumento("Saldo Anterior", curSaldo - curAmortiza);
                var linea2 = FormatearLineaDocumento("Saldo Actual", curSaldo);
                var linea3 = FormatearLineaDocumento("Interes Corriente", curIntC);
                var linea4 = FormatearLineaDocumento("Interes Atrasado", curIntM);
                var linea5 = FormatearLineaDocumento("Capitalizacion", curAmortiza * -1);
                var linea6 = FormatearLineaDocumento("Cargos Totales", curCargo);
                var linea7 = FormatearLineaDocumento("Polizas", curPoliza);
                var linea8 = $"Operacion/Linea   ..: Op.:{operacion.operacion} L.:{operacion.codigo}-{(operacion.opex == 1 ? "OPEX" : string.Empty)}";
                var linea9 = $"Descripcion       ..: {operacion.linea_desc}";
                var linea10 = string.Empty;
                var linea11 = trasladar ? "Trasladar Principal" : operacion.linea_desc;

                if (operacion.sys_plan_pagos)
                {
                    var proxPago = Cr_ArregloPago_ProxPago_Obtener(conn, tx, operacion.operacion);
                    linea9 = proxPago.fecha_pago.HasValue
                        ? $"Prox.Pago..:{proxPago.fecha_pago.Value:dd/MM/yyyy} Cta.({proxPago.num_cuota}) {proxPago.cuota:N2}"
                        : "Prox.Pago..: >> <<";
                    linea10 = $"Notas: {proxPago.notas}";
                }

                conn.Execute(
                    @"
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
            );",
                    new
                    {
                        NumDoc = numDoc,
                        TipoDoc = tipoDoc,
                        Usuario = usuario,
                        Cedula = operacion.cedula.Trim(),
                        Nombre = operacion.nombre.Trim(),
                        Concepto = concepto,
                        Monto = curAmortiza,
                        Operacion = operacion.operacion.ToString(),
                        Codigo = operacion.codigo,
                        OficinaTitular = globales.GOficinaTitular ?? string.Empty,
                        Linea1 = linea1,
                        Linea2 = linea2,
                        Linea3 = linea3,
                        Linea4 = linea4,
                        Linea5 = linea5,
                        Linea6 = linea6,
                        Linea7 = linea7,
                        Linea8 = linea8,
                        Linea9 = linea9,
                        Linea10 = linea10,
                        Detalle = notas,
                        Linea11 = linea11
                    },
                    tx);

                if (curAmortiza > 0)
                {
                    Cr_ArregloPago_AsientoDocumento_Registrar(
                        conn, tx, globales, tipoDoc, numDoc, curAmortiza * factor, "D", ctas.cod_divisa,
                        tipoCambio, ctas.cod_unidad, ctas.cod_centro_costo, ctas.ctaamortiza,
                        ctas.id_solicitud, ctas.codigo);
                }

                if (curIntC > 0)
                {
                    Cr_ArregloPago_AsientoDocumento_Registrar(
                        conn, tx, globales, tipoDoc, numDoc, curIntC * factor, "C", ctas.cod_divisa,
                        tipoCambio, ctas.cod_unidad, ctas.cod_centro_costo, ctas.ctaintc,
                        ctas.id_solicitud, ctas.codigo);
                }

                if (curIntM > 0)
                {
                    Cr_ArregloPago_AsientoDocumento_Registrar(
                        conn, tx, globales, tipoDoc, numDoc, curIntM * factor, "C", ctas.cod_divisa,
                        tipoCambio, ctas.cod_unidad, ctas.cod_centro_costo, ctas.ctaintm,
                        ctas.id_solicitud, ctas.codigo);
                }

                if (curCargo > 0)
                {
                    if (!operacion.sys_plan_pagos)
                    {
                        Cr_ArregloPago_AsientoDocumento_Registrar(
                            conn, tx, globales, tipoDoc, numDoc, curCargo * factor, "C", ctas.cod_divisa,
                            tipoCambio, ctas.cod_unidad, ctas.cod_centro_costo, ctas.ctacargos,
                            ctas.id_solicitud, ctas.codigo);
                    }
                    else
                    {
                        var cargos = Cr_ArregloPago_DocumentoAfectacionCargos_Obtener(conn, tx, tipoDoc, numDoc);
                        foreach (var item in cargos)
                        {
                            Cr_ArregloPago_AsientoDocumento_Registrar(
                                conn, tx, globales, tipoDoc, numDoc, (item.mov_monto ?? 0) * factor, "C", ctas.cod_divisa,
                                tipoCambio, item.cod_unidad, item.cod_centro_costo, item.cod_cuenta,
                                item.id_solicitud, item.codigo);
                        }
                    }
                }

                if (curPoliza > 0)
                {
                    var cuentaPoliza = Cr_ArregloPago_CuentaPoliza_Obtener(conn, tx, operacion.operacion);

                    Cr_ArregloPago_AsientoDocumento_Registrar(
                        conn, tx, globales, tipoDoc, numDoc, curPoliza * factor, "C", ctas.cod_divisa,
                        tipoCambio, ctas.cod_unidad, ctas.cod_centro_costo, cuentaPoliza,
                        ctas.id_solicitud, ctas.codigo);
                }

                return DbHelper.CreateOkResponse();
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        private CajasCrdDocumentoAfectacionData ObtenerAfectacionDocumento(
            int codEmpresa,
            string tipoDoc,
            string numDoc)
        {
            return DbHelper.ExecuteSingleQuery<CajasCrdDocumentoAfectacionData>(
                _portalDb,
                codEmpresa,
                "exec spCrdDocumentoAfectacion @TipoDoc, @NumDoc, 'R';",
                new CajasCrdDocumentoAfectacionData(),
                new
                {
                    TipoDoc = tipoDoc,
                    NumDoc = numDoc
                }).Result ?? new CajasCrdDocumentoAfectacionData();
        }

        private CajasCrdDocumentoAfectacionData ObtenerAfectacionDocumentoStp(
            int codEmpresa,
            string tipoDoc,
            string numDoc)
        {
            return DbHelper.ExecuteSingleQuery<CajasCrdDocumentoAfectacionData>(
                _portalDb,
                codEmpresa,
                "exec spCrdDocumentoAfectacionStP @TipoDoc, @NumDoc, 'R';",
                new CajasCrdDocumentoAfectacionData(),
                new
                {
                    TipoDoc = tipoDoc,
                    NumDoc = numDoc
                }).Result ?? new CajasCrdDocumentoAfectacionData();
        }

        private CajasCrdOperacionProxPagoData ObtenerProxPago(int codEmpresa, int operacion)
        {
            return DbHelper.ExecuteSingleQuery<CajasCrdOperacionProxPagoData>(
                _portalDb,
                codEmpresa,
                "exec spCrdOperacionFechaProxPago @Operacion;",
                new CajasCrdOperacionProxPagoData(),
                new { Operacion = operacion }).Result ?? new CajasCrdOperacionProxPagoData();
        }

        private CajasCrdOperacionCtasData ObtenerCuentasOperacion(int codEmpresa, int operacion)
        {
            return DbHelper.ExecuteSingleQuery<CajasCrdOperacionCtasData>(
                _portalDb,
                codEmpresa,
                "exec spCrdOperacionCtas @Operacion;",
                new CajasCrdOperacionCtasData(),
                new { Operacion = operacion }).Result ?? new CajasCrdOperacionCtasData();
        }

        private List<CajasCrdDocAfectacionCargoRow> ObtenerAfectacionDocumentoCargos(
            int codEmpresa,
            string tipoDoc,
            string numDoc)
        {
            return DbHelper.ExecuteListQuery<CajasCrdDocAfectacionCargoRow>(
                _portalDb,
                codEmpresa,
                "exec spCrdDocumentoAfectacionCargos @TipoDoc, @NumDoc;",
                new
                {
                    TipoDoc = tipoDoc,
                    NumDoc = numDoc
                }).Result ?? new List<CajasCrdDocAfectacionCargoRow>();
        }

        private ErrorDto RegistrarAsientoDocumento(
            int codEmpresa,
            string tipoDoc,
            string numDoc,
            decimal monto,
            string tipo,
            string divisa,
            decimal tipoCambio,
            string unidad,
            string centroCosto,
            string cuenta,
            int operacion,
            string codigo,
            string documento)
        {
            const string sql = @"
            exec spSIFDocsAsiento
                @TipoDoc,
                @NumDoc,
                @Monto,
                @Tipo,
                @Divisa,
                @TipoCambio,
                0,
                @Unidad,
                @CentroCosto,
                @Cuenta,
                @Operacion,
                @Codigo,
                @Documento;";

            return DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                sql,
                new
                {
                    TipoDoc = tipoDoc,
                    NumDoc = numDoc,
                    Monto = monto,
                    Tipo = tipo,
                    Divisa = divisa,
                    TipoCambio = tipoCambio,
                    Unidad = unidad,
                    CentroCosto = centroCosto,
                    Cuenta = cuenta,
                    Operacion = operacion,
                    Codigo = codigo,
                    Documento = documento
                });
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

        private CajasCrdOperacionProxPagoData Cr_ArregloPago_ProxPago_Obtener(
            SqlConnection conn,
            SqlTransaction tx,
            int operacion)
        {
            return conn.QueryFirstOrDefault<CajasCrdOperacionProxPagoData>(
                "exec spCrdOperacionFechaProxPago @Operacion;",
                new { Operacion = operacion },
                tx) ?? new CajasCrdOperacionProxPagoData();
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

        private void Cr_ArregloPago_AsientoDocumento_Registrar(
            SqlConnection conn,
            SqlTransaction tx,
            Globales globales,
            string tipoDoc,
            string numDoc,
            decimal monto,
            string tipo,
            string divisa,
            decimal tipoCambio,
            string unidad,
            string centroCosto,
            string cuenta,
            int operacion,
            string codigo)
        {
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
                    TipoDoc = tipoDoc,
                    NumDoc = numDoc,
                    Monto = monto,
                    Tipo = tipo,
                    Divisa = divisa,
                    TipoCambio = tipoCambio,
                    Enlace = globales.GEnlace,
                    Unidad = unidad,
                    CentroCosto = centroCosto,
                    Cuenta = cuenta,
                    Operacion = operacion,
                    Codigo = codigo
                },
                tx);
        }
    }
}