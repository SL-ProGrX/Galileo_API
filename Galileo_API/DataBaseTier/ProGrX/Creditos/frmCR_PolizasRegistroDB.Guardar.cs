using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.Security;
using Galileo_API.Models.ProGrX.Creditos;

namespace Galileo_API.DataBaseTier.ProGrX.Creditos
{
    public partial class FrmCrPolizasRegistroDb
    {
        /// <summary>
        /// Guarda los datos de la poliza integrada al plan de pagos.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<int> CrPolizasRegistro_PolizaIntegrada_Guardar(
            int codEmpresa,
            CrPolizasRegistroPolizaIntegradaGuardarRequest request)
        {
            if (request.operacion <= 0 || string.IsNullOrWhiteSpace(request.poliza_linea))
            {
                return DbHelper.CreateErrorResponse(
                    "Debe indicar la operaci&oacute;n y la l&iacute;nea de p&oacute;liza.",
                    -2,
                    0);
            }

            var verificaResp = CrPolizasRegistro_FxVerifica(codEmpresa, request);
            if (verificaResp.Code != 0)
            {
                return DbHelper.CreateErrorResponse(
                    verificaResp.Description ?? "No fue posible validar la p&oacute;liza integrada.",
                    verificaResp.Code.GetValueOrDefault(-1),
                    0);
            }

            if (verificaResp.Result == null || !verificaResp.Result.valido)
            {
                return DbHelper.CreateErrorResponse(
                    verificaResp.Result?.mensaje ?? "No fue posible validar la p&oacute;liza integrada.",
                    -2,
                    0);
            }

            int? contrato = CrPolizasRegistro_ContratoNumero_Obtener(request.poliza_contrato);
            if (!string.IsNullOrWhiteSpace(request.poliza_contrato) && !contrato.HasValue)
            {
                return DbHelper.CreateErrorResponse(
                    "El No. de contrato no es v&aacute;lido.",
                    -2,
                    0);
            }

            using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);
            conn.Open();

            using var tx = conn.BeginTransaction();

            try
            {
                int numPoliza = request.poliza_id > 0
                    ? request.poliza_id
                    : CrPolizasRegistro_NumeroPolizaSiguiente_Obtener(codEmpresa, request.operacion);

                int seqCorte = request.poliza_plan + request.poliza_ctas_deduce;
                string frecuenciaId = MapearFrecuenciaId(request.poliza_pago_frecuencia);
                string estadoId = CrPolizasRegistro_EstadoPoliza_Obtener(request.poliza_estado);

                if (request.poliza_id <= 0)
                {
                    const string sqlInsert = @"
            insert into CRD_OPERACION_POLIZAS
            (
                id_solicitud_poliza,
                cod_poliza,
                id_solicitud,
                codigo,
                cuota,
                registro_fecha,
                registro_usuario,
                estado,
                num_poliza,
                monto,
                cobertura_inicio,
                cobertura_vence,
                pago_frecuencia,
                pago_fecha,
                pago_monto,
                pago_realizado,
                pago_saldo,
                pago_ultimo,
                recaudado_monto,
                recaudado_corte,
                recaudado_saldo,
                num_seq_inicio,
                num_ctas_deduce,
                num_seq_corte,
                num_contrato,
                deduce_plazo_credito,
                cuota_rst_plan
            )
            values
            (
                0,
                @PolizaLinea,
                @Operacion,
                @Codigo,
                @Cuota,
                Getdate(),
                @Usuario,
                @Estado,
                @NumPoliza,
                @Monto,
                @CoberturaInicio,
                @CoberturaCorte,
                @Frecuencia,
                @FechaPago,
                @PagoMonto,
                0,
                @Monto,
                '',
                0,
                null,
                0,
                @Plan,
                @CtasDeduce,
                @SeqCorte,
                @Contrato,
                @PlazoCredito,
                @CuotaRestoPlazo
            );";

                    conn.Execute(sqlInsert, new
                    {
                        PolizaLinea = request.poliza_linea.Trim(),
                        Operacion = request.operacion,
                        Codigo = request.codigo.Trim(),
                        Cuota = request.poliza_cuota,
                        Usuario = request.usuario.Trim(),
                        Estado = estadoId,
                        NumPoliza = numPoliza,
                        Monto = request.poliza_monto,
                        CoberturaInicio = request.poliza_cobertura_inicio,
                        CoberturaCorte = request.poliza_cobertura_corte,
                        Frecuencia = frecuenciaId,
                        FechaPago = request.poliza_fecha_pago,
                        PagoMonto = request.poliza_pago_monto,
                        Plan = request.poliza_plan,
                        CtasDeduce = request.poliza_ctas_deduce,
                        SeqCorte = seqCorte,
                        Contrato = contrato,
                        PlazoCredito = request.poliza_plazo_credito ? 1 : 0,
                        CuotaRestoPlazo = request.poliza_cuota_resto_plazo
                    }, tx);
                }
                else
                {
                    const string sqlUpdate = @"
            update CRD_OPERACION_POLIZAS
               set estado = @Estado,
                   cuota = @Cuota,
                   monto = @Monto,
                   cobertura_inicio = @CoberturaInicio,
                   cobertura_vence = @CoberturaCorte,
                   deduce_plazo_credito = @PlazoCredito,
                   cuota_rst_plan = @CuotaRestoPlazo,
                   num_seq_inicio = @Plan,
                   num_ctas_deduce = @CtasDeduce,
                   num_seq_corte = @SeqCorte,
                   pago_frecuencia = @Frecuencia,
                   pago_fecha = @FechaPago,
                   pago_monto = @PagoMonto,
                   num_contrato = @Contrato
             where id_solicitud = @Operacion
               and num_poliza = @NumPoliza;";

                    conn.Execute(sqlUpdate, new
                    {
                        Estado = estadoId,
                        Cuota = request.poliza_cuota,
                        Monto = request.poliza_monto,
                        CoberturaInicio = request.poliza_cobertura_inicio,
                        CoberturaCorte = request.poliza_cobertura_corte,
                        PlazoCredito = request.poliza_plazo_credito ? 1 : 0,
                        CuotaRestoPlazo = request.poliza_cuota_resto_plazo,
                        Plan = request.poliza_plan,
                        CtasDeduce = request.poliza_ctas_deduce,
                        SeqCorte = seqCorte,
                        Frecuencia = frecuenciaId,
                        FechaPago = request.poliza_fecha_pago,
                        PagoMonto = request.poliza_pago_monto,
                        Contrato = contrato,
                        Operacion = request.operacion,
                        NumPoliza = numPoliza
                    }, tx);
                }

                conn.Execute(
                    "exec spCrdPolizaRegistroDetalle @Operacion,@NumPoliza,@Usuario;",
                    new
                    {
                        Operacion = request.operacion,
                        NumPoliza = numPoliza,
                        Usuario = request.usuario.Trim()
                    },
                    tx);

                tx.Commit();

                return DbHelper.CreateOkResponse(numPoliza);
            }
            catch (Exception ex)
            {
                tx.Rollback();
                return DbHelper.CreateErrorResponse(
                    ex.Message,
                    -1,
                    0);
            }
        }

        /// <summary>
        /// Guarda los datos de la poliza de retencion.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<int> CrPolizasRegistro_PolizaRetencion_Guardar(
            int codEmpresa,
            CrPolizasRegistroPolizaRetencionGuardarRequest request)
        {
            if (request.operacion <= 0 || string.IsNullOrWhiteSpace(request.poliza_linea))
            {
                return DbHelper.CreateErrorResponse(
                    "Debe indicar la operaci&oacute;n y la l&iacute;nea de p&oacute;liza.",
                    -2, 0);
            }

            var verificaResp = CrPolizasRegistro_FxVerifica(codEmpresa, request);
            if (verificaResp.Code != 0)
            {
                return DbHelper.CreateErrorResponse(
                    verificaResp.Description ?? "No fue posible validar la p&oacute;liza de retenci&oacute;n.",
                    verificaResp.Code.GetValueOrDefault(-1),
                    0);
            }

            if (verificaResp.Result == null || !verificaResp.Result.valido)
            {
                return DbHelper.CreateErrorResponse(
                    verificaResp.Result?.mensaje ?? "No fue posible validar la p&oacute;liza de retenci&oacute;n.",
                    -2, 0);
            }

            int priDeduc = CrPolizasRegistro_PriDeduc_Crear(request.anio, request.mes);
            if (priDeduc <= 0)
            {
                return DbHelper.CreateErrorResponse(
                    "La primer deducci&oacute;n no es v&aacute;lida.",
                    -2,
                    0);
            }

            using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);
            conn.Open();

            using var tx = conn.BeginTransaction();

            try
            {
                var operacionBaseResp = CrPolizasRegistro_OperacionBase_Obtener(codEmpresa, request.operacion);
                if (operacionBaseResp.Code != 0 || operacionBaseResp.Result is null)
                {
                    return DbHelper.CreateErrorResponse(
                        operacionBaseResp.Description ?? "No se encontr&oacute; la operaci&oacute;n base.",
                        -2, 0);
                }

                var operacionBase = operacionBaseResp.Result;

                var lineaDataResp = CrPolizasRegistro_PolizaRetencionData_Obtener(codEmpresa, request.poliza_linea);
                if (lineaDataResp.Code != 0 || lineaDataResp.Result is null || string.IsNullOrWhiteSpace(lineaDataResp.Result.codigo_retencion))
                {
                    return DbHelper.CreateErrorResponse(
                        lineaDataResp.Description ?? "No se encontr&oacute; la definici&oacute;n de la l&iacute;nea de p&oacute;liza.",
                        -2, 0);
                }

                var lineaData = lineaDataResp.Result;

                int comite = MCredito.fxCrdIdComiteLinea(_portalDb, codEmpresa, operacionBase.codigo);
                int numPoliza = CrPolizasRegistro_NumeroPolizaSiguiente_Obtener(codEmpresa, request.operacion);
                decimal fechaProcesoAnterior = _cobroDb.fxFechaProcesoAnterior(codEmpresa, priDeduc);
                DateTime fechaServidor = conn.QueryFirst<DateTime>("select Getdate();", transaction: tx);

                const string sqlInsertOperacion = @"
                insert into reg_creditos
                (
                    codigo,
                    id_comite,
                    cedula,
                    montosol,
                    montoapr,
                    monto_girado,
                    saldo,
                    amortiza,
                    interesc,
                    saldo_mes,
                    cuota,
                    int,
                    interesv,
                    plazo,
                    userrec,
                    userres,
                    userfor,
                    usertesoreria,
                    tesoreria,
                    fechasol,
                    fechares,
                    fechaforp,
                    fechaforf,
                    fecha_calculo_int,
                    garantia,
                    primer_cuota,
                    tdocumento,
                    ndocumento,
                    pagare,
                    firma_deudor,
                    premio,
                    observacion,
                    estado,
                    prideduc,
                    fecult,
                    estadosol,
                    documento_referido,
                    cod_destino
                )
                values
                (
                    @CodigoPolizaRet,
                    @Comite,
                    @Cedula,
                    @Monto,
                    @Monto,
                    0,
                    @Monto,
                    0,
                    0,
                    0,
                    @Monto,
                    @Monto,
                    0,
                    0,
                    @Plazo,
                    @Usuario,
                    @Usuario,
                    @Usuario,
                    @Usuario,
                    0,
                    @FechaServidor,
                    @FechaServidor,
                    @FechaServidor,
                    @FechaServidor,
                    @FechaServidor,
                    @Garantia,
                    'N',
                    'OT',
                    '',
                    0,
                    1,
                    0,
                    @Observacion,
                    'A',
                    @PriDeduc,
                    @Fecult,
                    'F',
                    @Documento,
                    @CodDestino
                );";

                conn.Execute(sqlInsertOperacion, new
                {
                    CodigoPolizaRet = lineaData.codigo_retencion.Trim().ToUpperInvariant(),
                    Comite = comite,
                    Cedula = operacionBase.cedula.Trim(),
                    Monto = request.monto,
                    Plazo = request.plazo,
                    Usuario = request.usuario.Trim(),
                    FechaServidor = fechaServidor,
                    Garantia = request.garantia.Trim(),
                    Observacion = request.observaciones.Trim().ToUpperInvariant(),
                    PriDeduc = priDeduc,
                    Fecult = fechaProcesoAnterior,
                    Documento = request.documento.Trim(),
                    CodDestino = string.IsNullOrWhiteSpace(request.destino) ? null : request.destino.Trim()
                }, tx);

                int nuevaOperacion = CrPolizasRegistro_UltimaOperacion_Obtener(codEmpresa, operacionBase.cedula);

                const string sqlInsertPoliza = @"
                insert into CRD_OPERACION_POLIZAS
                (
                    id_solicitud_poliza,
                    cod_poliza,
                    num_poliza,
                    id_solicitud,
                    codigo,
                    cuota,
                    registro_fecha,
                    registro_usuario
                )
                values
                (
                    @OperacionPoliza,
                    @CodigoPolizaRet,
                    @NumPoliza,
                    @OperacionMadre,
                    @CodigoMadre,
                    @Monto,
                    Getdate(),
                    @Usuario
                );";

                conn.Execute(sqlInsertPoliza, new
                {
                    OperacionPoliza = nuevaOperacion,
                    CodigoPolizaRet = lineaData.codigo_retencion.Trim().ToUpperInvariant(),
                    NumPoliza = numPoliza,
                    OperacionMadre = request.operacion,
                    CodigoMadre = operacionBase.codigo.Trim(),
                    Monto = request.monto,
                    Usuario = request.usuario.Trim()
                }, tx);

                int sysPlanPagos = _mainDb.sbSifParametrosInicializa(codEmpresa, request.usuario.Trim()).Result?.SysPlanPagos ?? 0;
                if (sysPlanPagos == 1)
                {
                    conn.Execute(
                        "exec spCrdPlanPagos @Operacion;",
                        new { Operacion = nuevaOperacion },
                        tx);
                }

                tx.Commit();

                MCredito.SbBitacoraCredito(
                    _portalDb,
                    codEmpresa,
                    new MCredito.CrBitacoraCreditoRequest
                    {
                        usuario = request.usuario.Trim(),
                        movimiento = "08",
                        detalle = $"Op: {nuevaOperacion} - Monto {request.monto} - Plazo: {request.plazo}",
                        tipo = "R",
                        operacion = nuevaOperacion,
                        codigo = operacionBase.codigo.Trim()
                    });

                _securityMainDb.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = codEmpresa,
                    Usuario = request.usuario.Trim(),
                    Movimiento = "Registra - WEB",
                    DetalleMovimiento = $"Retencion en la OP : {nuevaOperacion}",
                    Modulo = 10
                });

                return DbHelper.CreateOkResponse(nuevaOperacion);
            }
            catch (Exception ex)
            {
                tx.Rollback();
                return DbHelper.CreateErrorResponse(
                    ex.Message,
                    -1,
                    0);
            }
        }
    }
}