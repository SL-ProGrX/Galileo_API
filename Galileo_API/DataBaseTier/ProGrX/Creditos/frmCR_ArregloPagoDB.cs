using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.Security;
using Galileo_API.DataBaseTier.ProGrX.Cajas;
using Galileo_API.Models.ProGrX.Cajas;
using Galileo_API.Models.ProGrX.Credito;
using Microsoft.Data.SqlClient;

namespace Galileo_API.DataBaseTier.ProGrX.Creditos
{
    public class FrmCrArregloPagoDb
    {
        private readonly PortalDB _portalDb;
        private readonly MProGrxMain _mProGrxMain;
        private readonly MRecibos _mRecibos;
        private readonly MCajas _mCajas;
        private readonly MAfilicacionDB _mAfilicacion;
        private readonly MSecurityMainDb _securityMainDb;
        private const int VModulo = 3;

        public FrmCrArregloPagoDb(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
            _mProGrxMain = new MProGrxMain(config);
            _mRecibos = new MRecibos(config);
            _mCajas = new MCajas(config);
            _mAfilicacion = new MAfilicacionDB(config);
            _securityMainDb = new MSecurityMainDb(config);
        }

        /// <summary>
        /// Obtiene los tipos de documento de caja y los parametros base del formulario.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="caja"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto<CrArregloPagoCajaInicialData> Cr_ArregloPago_CajaInicial_Obtener(
            int codEmpresa,
            string caja,
            string usuario)
        {
            caja = NormalizarTexto(caja);
            usuario = NormalizarTexto(usuario);

            if (string.IsNullOrWhiteSpace(caja))
            {
                return DbHelper.CreateErrorResponse(
                    "Debe indicar la caja.",
                    -2,
                    new CrArregloPagoCajaInicialData());
            }

            if (string.IsNullOrWhiteSpace(usuario))
            {
                return DbHelper.CreateErrorResponse(
                    "Debe indicar el usuario.",
                    -2,
                    new CrArregloPagoCajaInicialData());
            }

            var globalesResp = ObtenerGlobales(codEmpresa, usuario);
            if (globalesResp.Code != 0 || globalesResp.Result is null)
            {
                return DbHelper.CreateErrorResponse(
                    globalesResp.Description ?? "No fue posible obtener los parametros globales.",
                    globalesResp.Code.GetValueOrDefault(-1),
                    new CrArregloPagoCajaInicialData());
            }

            const string sql = @"
                select
                    rtrim(C.tipo_documento) as item,
                    rtrim(D.descripcion) as descripcion
                from SIF_DOCUMENTOS D
                inner join CAJAS_DOCUMENTOS C
                    on D.tipo_documento = C.tipo_documento
                where C.cod_caja = @Caja
                  and D.tipo_movimiento in ('A','C')
                order by C.tipo_documento;";

            var tiposDocResp = DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                _portalDb,
                codEmpresa,
                sql,
                new { Caja = caja });

            if (tiposDocResp.Code != 0)
            {
                return DbHelper.CreateErrorResponse(
                    tiposDocResp.Description ?? "No fue posible obtener los tipos de documento.",
                    tiposDocResp.Code.GetValueOrDefault(-1),
                    new CrArregloPagoCajaInicialData());
            }

            return DbHelper.CreateOkResponse(new CrArregloPagoCajaInicialData
            {
                tipos_documento = tiposDocResp.Result ?? new List<DropDownListaGenericaModel>(),
                fecha_servidor = globalesResp.Result.fxFechaServidor ?? DateTime.Now,
                sys_plan_pagos = globalesResp.Result.SysPlanPagos == 1
            });
        }

        /// <summary>
        /// Obtiene la operacion activa y el estado mostrado por el formulario.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="operacion"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto<CrArregloPagoOperacionData?> Cr_ArregloPago_Operacion_Obtener(
            int codEmpresa,
            int operacion,
            string usuario,
            bool tipoIntereses = false)
        {
            usuario = NormalizarTexto(usuario);

            if (operacion <= 0)
            {
                return DbHelper.CreateErrorResponse<CrArregloPagoOperacionData?>(
                    "Debe indicar una operacion valida.",
                    -2,
                    null);
            }

            if (string.IsNullOrWhiteSpace(usuario))
            {
                return DbHelper.CreateErrorResponse<CrArregloPagoOperacionData?>(
                    "Debe indicar el usuario.",
                    -2,
                    null);
            }

            var globalesResp = ObtenerGlobales(codEmpresa, usuario);
            if (globalesResp.Code != 0 || globalesResp.Result is null)
            {
                return DbHelper.CreateErrorResponse<CrArregloPagoOperacionData?>(
                    globalesResp.Description ?? "No fue posible obtener los parametros globales.",
                    globalesResp.Code.GetValueOrDefault(-1),
                    null);
            }

            const string sql = @"
                select top 1
                    R.id_solicitud as operacion,
                    rtrim(R.cedula) as cedula,
                    rtrim(S.nombre) as nombre,
                    rtrim(R.codigo) as codigo,
                    rtrim(C.descripcion) as linea_desc,
                    rtrim(isnull(R.proceso, '')) as proceso,
                    isnull(R.opex, 0) as opex,
                    cast(case when (C.retencion = 'S' or C.poliza = 'S') then 1 else 0 end as bit) as retencion,
                    isnull(R.montoapr, 0) as monto,
                    isnull(R.saldo, 0) as saldo,
                    isnull(R.plazo, 0) as plazo,
                    isnull(isnull(R.interesv, R.[int]), 0) as tasa,
                    isnull(R.cuota, 0) as cuota,
                    rtrim(isnull(R.cod_divisa, 'COL')) as divisa,
                    Getdate() as fecha_servidor,
                    dbo.fxSIFCorteAFecha(isnull(R.fecult, R.prideduc)) as fecha_ult_mov,
                    isnull(R.prideduc, 0) as prideduc,
                    isnull(V.amortiza, 0) as amortiza,
                    isnull(V.intC, 0) as int_cor,
                    isnull(V.intM, 0) as int_mor,
                    isnull(V.cargos, 0) as cargos,
                    cast(0 as decimal(16,2)) as polizas,
                    cast(0 as bit) as sys_plan_pagos,
                    cast(0 as int) as mora_count,
                    cast(0 as decimal(16,2)) as cargos_intereses,
                    cast(0 as decimal(16,2)) as deuda,
                    cast(0 as decimal(16,2)) as total_pagar
                from reg_creditos R
                inner join socios S
                    on R.cedula = S.cedula
                inner join catalogo C
                    on R.codigo = C.codigo
                   and C.retencion = 'N'
                   and C.poliza = 'N'
                left join vista_morosidad V
                    on R.id_solicitud = V.id_solicitud
                where R.id_solicitud = @Operacion
                  and R.estado = 'A';";

            var response = DbHelper.ExecuteSingleQuery(
                _portalDb,
                codEmpresa,
                sql,
                (CrArregloPagoOperacionData?)null,
                new { Operacion = operacion });

            if (response.Code != 0)
            {
                return DbHelper.CreateErrorResponse<CrArregloPagoOperacionData?>(
                    response.Description ?? "No fue posible obtener la operacion.",
                    response.Code.GetValueOrDefault(-1),
                    null);
            }

            if (response.Result is null)
            {
                return DbHelper.CreateErrorResponse<CrArregloPagoOperacionData?>(
                    "No se encontr&oacute; registro de la operaci&oacute;n activa o no es un cr&eacute;dito.",
                    -2,
                    null);
            }

            var resultado = response.Result;
            var globales = globalesResp.Result;
            resultado.sys_plan_pagos = globales.SysPlanPagos == 1;
            resultado.fecha_servidor = globales.fxFechaServidor ?? resultado.fecha_servidor;

            if (resultado.sys_plan_pagos)
            {
                const string sqlCancelacion = @"
                    exec spCrdPlanPagosInfoCancelacion @OperacionId, @FechaCancelacion;";

                var cancelacionResp = DbHelper.ExecuteSingleQuery<CrArregloPagoOperacionData>(
                    _portalDb,
                    codEmpresa,
                    sqlCancelacion,
                    null,
                    new
                    {
                        OperacionId = operacion,
                        FechaCancelacion = resultado.fecha_servidor.Date
                    });

                if (cancelacionResp.Code == 0 && cancelacionResp.Result is not null)
                {
                    resultado.int_cor = cancelacionResp.Result.int_cor;
                    resultado.int_mor = cancelacionResp.Result.int_mor;
                    resultado.cargos = cancelacionResp.Result.cargos;
                    resultado.polizas = cancelacionResp.Result.polizas;
                    resultado.amortiza = cancelacionResp.Result.amortiza;
                }
            }
            else
            {
                const string sqlInteres = @"
                    select isnull(dbo.fxCRDCalculoIntCorte(@Operacion, Getdate()), 0);";

                var interesResp = DbHelper.ExecuteSingleQuery<decimal>(
                    _portalDb,
                    codEmpresa,
                    sqlInteres,
                    0,
                    new { Operacion = operacion });

                if (interesResp.Code == 0)
                {
                    resultado.int_cor = interesResp.Result - resultado.int_mor;
                }
            }

            resultado.tipo_intereses = tipoIntereses;

            resultado.mora = SbCargaMora(
                codEmpresa,
                operacion,
                resultado.sys_plan_pagos,
                resultado.fecha_servidor,
                tipoIntereses);

            resultado.mora_count = resultado.mora.Count;

            decimal totalIntCor = 0;
            decimal totalIntMor = 0;
            decimal totalCargos = 0;
            decimal totalPolizas = 0;
            decimal totalPrincipal = 0;

            foreach (var item in resultado.mora)
            {
                totalIntCor += item.int_c;
                totalIntMor += item.int_m;
                totalCargos += item.cargo;
                totalPolizas += item.poliza;
                totalPrincipal += item.amortiza;
            }

            resultado.int_cor = totalIntCor;
            resultado.int_mor = totalIntMor;
            resultado.cargos = totalCargos;
            resultado.polizas = totalPolizas;
            resultado.amortiza = totalPrincipal;

            resultado.cargos_intereses =
                resultado.int_cor +
                resultado.int_mor +
                resultado.cargos +
                resultado.polizas;

            resultado.deuda =
                resultado.saldo +
                resultado.int_cor +
                resultado.int_mor +
                resultado.cargos +
                resultado.polizas;

            resultado.total_pagar =
                resultado.int_cor +
                resultado.int_mor +
                resultado.cargos +
                resultado.polizas +
                resultado.amortiza;

            return DbHelper.CreateOkResponse<CrArregloPagoOperacionData?>(resultado);
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
                    - 2,
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
                    globalesResp.Description ?? "No fue posible obtener los parametros globales.",
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
                    "Esta operacion no puede realizar una capitalizaci&oacute;n de deuda porque est&aacute; al d&iacute;a.",
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
                    globalesResp.Description ?? "No fue posible obtener los parametros globales.",
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
                    codEmpresa,
                    globales,
                    operacion,
                    cajaCtx,
                    request.usuario,
                    request.tipo_doc,
                    numDocumento,
                    "CRD007",
                    request.notas);

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

        private ErrorDto<Globales> ObtenerGlobales(int codEmpresa, string usuario)
        {
            if (string.IsNullOrWhiteSpace(usuario))
            {
                return DbHelper.CreateErrorResponse(
                    "Debe indicar el usuario.",
                    -2,
                    new Globales());
            }

            return _mProGrxMain.sbSifParametrosInicializa(codEmpresa, usuario);
        }

        private List<CrArregloPagoMoraData> SbCargaMora(
            int codEmpresa,
            int operacion,
            bool sysPlanPagos,
            DateTime fechaServidor,
            bool tipoIntereses)
        {
            if (sysPlanPagos)
            {
                if (tipoIntereses)
                {
                    DbHelper.ExecuteNonQuery(
                        _portalDb,
                        codEmpresa,
                        "exec spCrdPlanPagosProyectaCuota @Operacion, @Fecha, 1;",
                        new
                        {
                            Operacion = operacion,
                            Fecha = fechaServidor.Date
                        });

                    DbHelper.ExecuteNonQuery(
                        _portalDb,
                        codEmpresa,
                        @"
                        update CRD_OPERACION_TRANSAC_CAL
                        set principal = 0
                        where id_solicitud = @Operacion
                          and id_seq in
                          (
                              select max(id_seq)
                              from CRD_OPERACION_TRANSAC_CAL
                              where id_solicitud = @Operacion
                          );",
                        new { Operacion = operacion });

                    const string sqlCal = @"
                    select
                        Det.id_seq as id_moro,
                        Det.id_solicitud,
                        Det.fecha_proceso as fecha_p,
                        Det.intcor as int_c,
                        Det.intmor as int_m,
                        Det.cargos as cargo,
                        Det.poliza as poliza,
                        Det.principal as amortiza,
                        Det.intcor + Det.intmor + Det.principal + Det.poliza + Det.cargos as cuota_morosa,
                        rtrim(Det.estado) as estado,
                        cast(0 as decimal(16,2)) as ab_int_c,
                        cast(0 as decimal(16,2)) as ab_int_m,
                        cast(0 as decimal(16,2)) as ab_amortiza,
                        cast(0 as decimal(16,2)) as ab_cargo,
                        cast(0 as decimal(16,2)) as ab_poliza
                    from CRD_OPERACION_TRANSAC_CAL Det
                    inner join REG_CREDITOS Reg
                        on Det.id_solicitud = Reg.id_solicitud
                    where Reg.proceso <> 'J'
                      and Det.estado = 'A'
                      and Det.id_solicitud = @Operacion
                    order by Det.fecha_proceso, Det.id_seq;";

                    return DbHelper.ExecuteListQuery<CrArregloPagoMoraData>(
                        _portalDb,
                        codEmpresa,
                        sqlCal,
                        new { Operacion = operacion }).Result ?? new List<CrArregloPagoMoraData>();
                }

                const string sql = @"
                select
                    Det.id_seq as id_moro,
                    Det.id_solicitud,
                    Det.fecha_proceso as fecha_p,
                    Det.intcor as int_c,
                    Det.intmor as int_m,
                    Det.cargos as cargo,
                    Det.poliza as poliza,
                    Det.principal as amortiza,
                    Det.intcor + Det.intmor + Det.principal + Det.poliza + Det.cargos as cuota_morosa,
                    rtrim(Det.estado) as estado,
                    cast(0 as decimal(16,2)) as ab_int_c,
                    cast(0 as decimal(16,2)) as ab_int_m,
                    cast(0 as decimal(16,2)) as ab_amortiza,
                    cast(0 as decimal(16,2)) as ab_cargo,
                    cast(0 as decimal(16,2)) as ab_poliza
                from CRD_OPERACION_TRANSAC Det
                inner join REG_CREDITOS Reg
                    on Det.id_solicitud = Reg.id_solicitud
                where Reg.proceso <> 'J'
                  and Det.estado = 'A'
                  and Det.id_solicitud = @Operacion
                  and Det.fecha_corte <= @Fecha
                order by Det.fecha_proceso, Det.id_seq;";

                return DbHelper.ExecuteListQuery<CrArregloPagoMoraData>(
                    _portalDb,
                    codEmpresa,
                    sql,
                    new
                    {
                        Operacion = operacion,
                        Fecha = fechaServidor.Date
                    }).Result ?? new List<CrArregloPagoMoraData>();
            }

            const string sqlMora = @"
            select
                id_moro,
                id_solicitud,
                fechap as fecha_p,
                intc as int_c,
                intm as int_m,
                isnull(cargo, 0) as cargo,
                cast(0 as decimal(16,2)) as poliza,
                amortiza,
                cuota_morosa,
                rtrim(estado) as estado,
                abintc as ab_int_c,
                abintm as ab_int_m,
                isnull(abamortiza, 0) as ab_amortiza,
                isnull(abcargo, 0) as ab_cargo,
                cast(0 as decimal(16,2)) as ab_poliza
            from MOROSIDAD
            where id_solicitud = @Operacion
              and estado = 'A'
            order by fechap;";

            return DbHelper.ExecuteListQuery<CrArregloPagoMoraData>(
                _portalDb,
                codEmpresa,
                sqlMora,
                new { Operacion = operacion }).Result ?? new List<CrArregloPagoMoraData>();
        }

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
                return DbHelper.ErrorResponse("Debe indicar el usuario.", -2);
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
            public int apertura { get; set; } = 0;
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

        private ErrorDto Cr_ArregloPago_CajaContexto_Validar(CrArregloPagoCajaContexto ctx)
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

        private CajasCrdDocumentoAfectacionData Cr_ArregloPago_DocumentoAfectacion_Obtener(
            SqlConnection conn,
            SqlTransaction tx,
            bool sysPlanPagos,
            string tipoDoc,
            string numDoc)
        {
            var sql = sysPlanPagos
                ? "exec spCrdDocumentoAfectacion @TipoDoc, @NumDoc, 'R';"
                : "exec spCrdDocumentoAfectacionStP @TipoDoc, @NumDoc, 'R';";

            return conn.QueryFirstOrDefault<CajasCrdDocumentoAfectacionData>(
                sql,
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
    }
}