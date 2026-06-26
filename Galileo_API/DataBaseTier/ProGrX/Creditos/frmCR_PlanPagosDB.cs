using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.Security;
using Galileo_API.Models.ProGrX.Creditos;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Galileo_API.DataBaseTier.ProGrX.Creditos
{
    public class FrmCRPlanPagosDB
    {
        private const int ModuloCreditos = 3;
        private const string MsgOperacionRequerida = "La operación es requerida.";

        private readonly PortalDB _portalDB;
        private readonly MSecurityMainDb _securityMainDb;

        public FrmCRPlanPagosDB(IConfiguration config)
        {
            _portalDB = new PortalDB(config);
            _securityMainDb = new MSecurityMainDb(config);
        }

        /// <summary>
        /// Registra en bitácora un movimiento del módulo.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public ErrorDto Bitacora(BitacoraInsertarDto data)
        {
            return _securityMainDb.Bitacora(data);
        }

        /// <summary>
        /// Obtiene los datos principales, totales y plan de pagos de la operación.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="operacion"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto<CrPlanPagosObtenerDto> CR_PlanPagos_Obtener(
            int CodEmpresa,
            int operacion,
            string? usuario)
        {
            var result = new CrPlanPagosObtenerDto();

            if (operacion <= 0)
            {
                return DbHelper.CreateErrorResponse(MsgOperacionRequerida, -2, result);
            }

            try
            {
                using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);
                EnsureOpen(conn);

                result.header = ObtenerHeader(conn, operacion);
                result.totales = ObtenerTotales(conn, operacion);
                result.plan_pagos = ObtenerPlanPagos(conn, operacion);
                result.reporte = ObtenerReporteInfo(conn, usuario);

                return DbHelper.CreateOkResponse(result);
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse(ex.Message, -1, result);
            }
        }

        /// <summary>
        /// Obtiene la lista completa de cargos asociados a una línea del plan de pagos.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="operacion"></param>
        /// <param name="idSeq"></param>
        /// <param name="parametros"></param>
        /// <returns></returns>
        public ErrorDto<CrPlanPagosListaResult<CrPlanPagosCargosData>> CR_PlanPagos_Cargos_Lista_Obtener(
            int CodEmpresa,
            int operacion,
            decimal idSeq,
            FiltrosLazyLoadData parametros)
        {
            return ObtenerListaSp<CrPlanPagosCargosData>(
                CodEmpresa,
                "spCrd_Operacion_Consulta_Cargos",
                new { Operacion = operacion, IdSeq = idSeq },
                AplicarPendienteCargos);
        }

        /// <summary>
        /// Obtiene la lista completa de cargos para exportación.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="operacion"></param>
        /// <param name="idSeq"></param>
        /// <param name="parametros"></param>
        /// <returns></returns>
        public ErrorDto<CrPlanPagosListaResult<CrPlanPagosCargosData>> CR_PlanPagos_Cargos_Lista_Export(
            int CodEmpresa,
            int operacion,
            decimal idSeq,
            FiltrosLazyLoadData parametros)
        {
            return CR_PlanPagos_Cargos_Lista_Obtener(CodEmpresa, operacion, idSeq, parametros);
        }

        /// <summary>
        /// Obtiene la lista completa de pólizas asociadas a una línea del plan de pagos.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="operacion"></param>
        /// <param name="idSeq"></param>
        /// <param name="parametros"></param>
        /// <returns></returns>
        public ErrorDto<CrPlanPagosListaResult<CrPlanPagosPolizasData>> CR_PlanPagos_Polizas_Lista_Obtener(
            int CodEmpresa,
            int operacion,
            decimal idSeq,
            FiltrosLazyLoadData parametros)
        {
            return ObtenerListaSp<CrPlanPagosPolizasData>(
                CodEmpresa,
                "spCrd_Operacion_Consulta_Polizas",
                new { Operacion = operacion, IdSeq = idSeq });
        }

        /// <summary>
        /// Obtiene la lista completa de pólizas para exportación.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="operacion"></param>
        /// <param name="idSeq"></param>
        /// <param name="parametros"></param>
        /// <returns></returns>
        public ErrorDto<CrPlanPagosListaResult<CrPlanPagosPolizasData>> CR_PlanPagos_Polizas_Lista_Export(
            int CodEmpresa,
            int operacion,
            decimal idSeq,
            FiltrosLazyLoadData parametros)
        {
            return CR_PlanPagos_Polizas_Lista_Obtener(CodEmpresa, operacion, idSeq, parametros);
        }

        /// <summary>
        /// Obtiene la lista completa de documentos asociados al plan de pagos.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="operacion"></param>
        /// <param name="idSeq"></param>
        /// <param name="todos"></param>
        /// <param name="parametros"></param>
        /// <returns></returns>
        public ErrorDto<CrPlanPagosListaResult<CrPlanPagosDocumentosData>> CR_PlanPagos_Documentos_Lista_Obtener(
            int CodEmpresa,
            int operacion,
            decimal idSeq,
            bool todos,
            FiltrosLazyLoadData parametros)
        {
            return ObtenerListaSp<CrPlanPagosDocumentosData>(
                CodEmpresa,
                "spCrd_Operacion_Consulta_Documento",
                new { Operacion = operacion, IdSeq = todos ? 0 : idSeq });
        }

        /// <summary>
        /// Obtiene la lista completa de documentos para exportación.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="operacion"></param>
        /// <param name="idSeq"></param>
        /// <param name="todos"></param>
        /// <param name="parametros"></param>
        /// <returns></returns>
        public ErrorDto<CrPlanPagosListaResult<CrPlanPagosDocumentosData>> CR_PlanPagos_Documentos_Lista_Export(
            int CodEmpresa,
            int operacion,
            decimal idSeq,
            bool todos,
            FiltrosLazyLoadData parametros)
        {
            return CR_PlanPagos_Documentos_Lista_Obtener(CodEmpresa, operacion, idSeq, todos, parametros);
        }

        /// <summary>
        /// Obtiene los valores registrados para un documento.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="tipoDocumento"></param>
        /// <param name="transaccion"></param>
        /// <returns></returns>
        public ErrorDto<List<CrPlanPagosValoresData>> CR_PlanPagos_DocumentoValores_Obtener(
            int CodEmpresa,
            string tipoDocumento,
            string transaccion)
        {
            var lista = new List<CrPlanPagosValoresData>();

            try
            {
                using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);
                EnsureOpen(conn);

                lista = conn.Query<CrPlanPagosValoresData>(
                    "spSys_Documento_Valores",
                    new
                    {
                        TipoDoc = (tipoDocumento ?? string.Empty).Trim(),
                        Transaccion = (transaccion ?? string.Empty).Trim()
                    },
                    commandType: CommandType.StoredProcedure).ToList();

                return DbHelper.CreateOkResponse(lista);
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse(ex.Message, -1, lista);
            }
        }

        /// <summary>
        /// Obtiene la lista completa de ajustes de la operación.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="operacion"></param>
        /// <param name="parametros"></param>
        /// <returns></returns>
        public ErrorDto<CrPlanPagosListaResult<CrPlanPagosAjustesData>> CR_PlanPagos_Ajustes_Lista_Obtener(
            int CodEmpresa,
            int operacion,
            FiltrosLazyLoadData parametros)
        {
            return ObtenerListaSp<CrPlanPagosAjustesData>(
                CodEmpresa,
                "spCrd_Operacion_Consulta_Ajustes",
                new { Operacion = operacion, Codigo = string.Empty });
        }

        /// <summary>
        /// Obtiene la lista completa de ajustes para exportación.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="operacion"></param>
        /// <param name="parametros"></param>
        /// <returns></returns>
        public ErrorDto<CrPlanPagosListaResult<CrPlanPagosAjustesData>> CR_PlanPagos_Ajustes_Lista_Export(
            int CodEmpresa,
            int operacion,
            FiltrosLazyLoadData parametros)
        {
            return CR_PlanPagos_Ajustes_Lista_Obtener(CodEmpresa, operacion, parametros);
        }

        /// <summary>
        /// Activa las cuotas pendientes de la operación según la fecha indicada.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto CR_PlanPagos_Activar(int CodEmpresa, CrPlanPagosActivarRequest request)
        {
            var validacion = ValidarActivarRequest(request);
            if (validacion.Code != 0)
            {
                return validacion;
            }

            try
            {
                using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);
                EnsureOpen(conn);

                var aplicado = request.plazo == 999
                    ? ActivarRetencionOperacion(conn, request)
                    : ActivarCuotasPendientes(conn, request);

                if (!aplicado)
                {
                    return DbHelper.ErrorResponse("No se encontraron cuotas pendientes para activar.", -2);
                }

                RegistrarBitacora(
                    CodEmpresa,
                    request.usuario,
                    "Activa",
                    $"Cuotas Operacion: {request.operacion} al {request.fecha_activacion:yyyy-MM-dd}");

                return DbHelper.OkResponse("Activación de Cuotas Procesada!");
            }
            catch (SqlException ex)
            {
                return DbHelper.ErrorResponse(ex.Message, -1);
            }
        }

        /// <summary>
        /// Revisa y reconstruye el plan de pagos de la operación.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto CR_PlanPagos_Revisar(int CodEmpresa, CrPlanPagosRevisarRequest request)
        {
            var validacion = ValidarRevisarRequest(request);
            if (validacion.Code != 0)
            {
                return validacion;
            }

            try
            {
                using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);
                EnsureOpen(conn);

                var ajustaCuota = request.ajusta_cuota.GetValueOrDefault();
                var cuotaManual = request.cuota_manual.GetValueOrDefault();
                var plazoExt = request.plazo_ext.GetValueOrDefault() >= 0
                    ? request.plazo_ext.GetValueOrDefault()
                    : 0;

                var cuotaFija = ajustaCuota ? cuotaManual : 0;

                EjecutarRevision(conn, request, cuotaFija, plazoExt);

                RegistrarBitacora(
                    CodEmpresa,
                    request.usuario,
                    "Aplica",
                    ConstruirDetalleRevision(request, cuotaFija, plazoExt));

                return DbHelper.OkResponse("Plan de Pagos Revisado Satisfactoriamente!");
            }
            catch (SqlException ex)
            {
                return DbHelper.ErrorResponse(ex.Message, -1);
            }
        }

        /// <summary>
        /// Envía por correo el estado de cuenta de la operación.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto CR_PlanPagos_Email_Enviar(int CodEmpresa, CrPlanPagosEmailRequest request)
        {
            if (request == null || request.operacion <= 0)
            {
                return DbHelper.ErrorResponse(MsgOperacionRequerida, -2);
            }

            try
            {
                using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);
                EnsureOpen(conn);

                conn.Execute(
                    "spSys_Notifica_Credito_Estado_Operacion",
                    new
                    {
                        Operacion = request.operacion,
                        Usuario = (request.usuario ?? string.Empty).Trim()
                    },
                    commandType: CommandType.StoredProcedure);

                RegistrarBitacora(
                    CodEmpresa,
                    request.usuario,
                    "Aplica",
                    $"Notificación Email de Estado de la Operación: {request.operacion}");

                return DbHelper.OkResponse("Estado de Cuenta de la Operacion enviado al correo de la persona!");
            }
            catch (SqlException ex)
            {
                return DbHelper.ErrorResponse(ex.Message, -1);
            }
        }

        private static CrPlanPagosHeaderDto ObtenerHeader(SqlConnection conn, int operacion)
        {
            const string sql = @"
                select
                    R.ID_SOLICITUD as operacion,
                    S.CEDULA as cedula,
                    S.NOMBRE as nombre,
                    R.CODIGO as codigo,
                    C.DESCRIPCION as descripcion,
                    isnull(Ofi.DESCRIPCION, '') as oficina,
                    R.MONTOAPR as monto,
                    R.SALDO as saldo,
                    R.CUOTA as cuota,
                    R.PLAZO as plazo,
                    R.INTERESV as tasa,
                    R.INT as tasa_original,
                    convert(varchar(20), R.DIA_PAGO) as dia_pago,
                    R.BASE_CALCULO as base_calculo,
                    R.PRIDEDUC as pri_deduc,
                    dbo.fxCrd_Operacion_Cta_Ultimo_Corte(R.ID_SOLICITUD) as fec_ult_cta
                from SOCIOS S
                    inner join REG_CREDITOS R on S.CEDULA = R.CEDULA
                    inner join CATALOGO C on R.CODIGO = C.CODIGO
                    left join SIF_OFICINAS Ofi on R.COD_OFICINA_R = Ofi.COD_OFICINA
                where R.ID_SOLICITUD = @operacion;";

            var header = conn.QueryFirstOrDefault<CrPlanPagosHeaderDto>(sql, new { operacion })
                ?? new CrPlanPagosHeaderDto();

            AplicarFormatoHeader(header);
            return header;
        }

        private static void AplicarFormatoHeader(CrPlanPagosHeaderDto header)
        {
            header.dia_pago = header.dia_pago == "32"
                ? "Ultimo Día del Mes"
                : header.dia_pago;

            header.factor_calculo = MCredito.FxCrdFactorCalculo(header.base_calculo);

            header.pri_deduc_format = header.base_calculo == "06"
                ? header.pri_deduc.ToString("####-##.0")
                : header.pri_deduc.ToString("####-##");

            if (header.fec_ult_cta.HasValue)
            {
                header.activacion_fecha = header.fec_ult_cta.Value.AddDays(1);
                header.activacion_min = header.fec_ult_cta.Value.AddDays(1);
                header.activacion_max = header.fec_ult_cta.Value.AddDays(32);
            }
        }

        private static CrPlanPagosTotalesDto ObtenerTotales(SqlConnection conn, int operacion)
        {
            const string sql = @"
                select
                    isnull(max(NUM_CUOTA), 0) as cuotas,
                    min(FECHA_PAGO) as inicio,
                    max(FECHA_PAGO) as corte,
                    isnull(sum(DIAS_CALCULO), 0) as dias,
                    isnull(sum(INTCOR + INTMOR), 0) as intereses,
                    isnull(sum(CARGOS), 0) as cargos,
                    isnull(sum(MORA_DIAS), 0) as mora_dias
                from CRD_OPERACION_PLAN_PAGOS
                where ID_SOLICITUD = @operacion;";

            return conn.QueryFirstOrDefault<CrPlanPagosTotalesDto>(sql, new { operacion })
                ?? new CrPlanPagosTotalesDto();
        }

        private static List<CrPlanPagosData> ObtenerPlanPagos(SqlConnection conn, int operacion)
        {
            const string sql = @"
                select
                    0 as sep1,
                    TP.ID_SEQ as id_seq,
                    TP.NUM_CUOTA as num_cuota,
                    TP.FECHA_PROCESO as fecha_proceso,
                    TP.FECHA_INICIO as fecha_inicio,
                    TP.FECHA_CORTE as fecha_corte,
                    TP.FECHA_PAGO as fecha_pago,
                    TP.TASA as tasa,
                    TP.PLAZO as plazo,
                    TP.CUOTA as cuota,
                    isnull(TP.IVA, 0) as iva,
                    TP.CARGOS as cargos,
                    TP.POLIZA as poliza,
                    TP.INTCOR as intcor,
                    TP.INTMOR as intmor,
                    TP.PRINCIPAL as principal,
                    TP.SALDO_ANTERIOR as saldo_anterior,
                    TP.SALDO_ACTUAL as saldo_actual,
                    TP.DIAS_CALCULO as dias_calculo,
                    case TP.ESTADO
                        when 'A' then 'Activa'
                        when 'P' then 'Pendiente'
                        when 'C' then 'Cancelada'
                        when 'N' then 'Anulada'
                        else isnull(TP.ESTADO, '')
                    end as estado,
                    isnull(Mov.MORA_DIAS, 0) as mora_dias,
                    Mov.MOV_FECHA as mov_fecha,
                    isnull(Mov.MOV_MONTO, 0) as mov_monto,
                    isnull(Mov.MOV_IVA, 0) as mov_iva,
                    isnull(Mov.MOV_CARGOS, 0) as mov_cargos,
                    isnull(Mov.MOV_POLIZA, 0) as mov_poliza,
                    isnull(Mov.MOV_INTCOR, 0) as mov_intcor,
                    isnull(Mov.MOV_INTMOR, 0) as mov_intmor,
                    isnull(Mov.MOV_PRINCIPAL, 0) as mov_principal,
                    isnull(Mov.COD_CAJA, '') + '/' + isnull(Mov.MOV_USUARIO, '') as usuario_caja,
                    isnull(Mov.TIPO_DOCUMENTO, TP.TIPO_DOCUMENTO) as tipo_documento,
                    isnull(Mov.NUM_COMPROBANTE, TP.NUM_COMPROBANTE) as num_comprobante,
                    0 as sep2,
                    isnull(Con.DESCRIPCION, '') as concepto
                from CRD_OPERACION_PLAN_PAGOS TP
                    left join CRD_OPERACION_TRANSAC Mov
                        on TP.ID_SEQ = Mov.ID_SEQ
                       and TP.ID_SOLICITUD = Mov.ID_SOLICITUD
                    left join SIF_CONCEPTOS Con
                        on isnull(Mov.COD_CONCEPTO, TP.COD_CONCEPTO) = Con.COD_CONCEPTO
                where TP.ID_SOLICITUD = @operacion
                order by TP.ID_SEQ;";

            return conn.Query<CrPlanPagosData>(sql, new { operacion }).ToList();
        }

        private ErrorDto<CrPlanPagosListaResult<T>> ObtenerListaSp<T>(
            int CodEmpresa,
            string storedProcedure,
            object parametros,
            Action<List<T>>? postProcess = null)
        {
            var result = new CrPlanPagosListaResult<T>();

            try
            {
                using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);
                EnsureOpen(conn);

                var lista = conn.Query<T>(
                    storedProcedure,
                    parametros,
                    commandType: CommandType.StoredProcedure).ToList();

                postProcess?.Invoke(lista);

                result.lista = lista;
                result.total = lista.Count;

                return DbHelper.CreateOkResponse(result);
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse(ex.Message, -1, result);
            }
        }

        private static void AplicarPendienteCargos(List<CrPlanPagosCargosData> lista)
        {
            foreach (var item in lista)
            {
                item.pendiente = item.monto - item.abono;
            }
        }

        private static ErrorDto ValidarActivarRequest(CrPlanPagosActivarRequest request)
        {
            if (request == null || request.operacion <= 0)
            {
                return DbHelper.ErrorResponse(MsgOperacionRequerida, -2);
            }

            if (!request.fecha_activacion.HasValue)
            {
                return DbHelper.ErrorResponse("La fecha de activación es requerida.", -2);
            }

            return DbHelper.CreateOkResponse();
        }

        private static bool ActivarRetencionOperacion(SqlConnection conn, CrPlanPagosActivarRequest request)
        {
            var proceso = conn.QueryFirstOrDefault<int>(
                "select dbo.fxSIFDateTimeToProceso(@fecha) as proceso;",
                new { fecha = request.fecha_activacion });

            conn.Execute(
                "spCrdPlanPagosActivaRetenciones_Operacion",
                new
                {
                    Operacion = request.operacion,
                    Proceso = proceso,
                    CuotaH = (decimal?)null
                },
                commandType: CommandType.StoredProcedure);

            return true;
        }

        private static bool ActivarCuotasPendientes(SqlConnection conn, CrPlanPagosActivarRequest request)
        {
            const string sql = @"
                select ID_SEQ
                from CRD_OPERACION_PLAN_PAGOS
                where ID_SOLICITUD = @operacion
                  and ESTADO = 'P'
                  and FECHA_CORTE <= @fecha
                order by ID_SEQ asc;";

            var cuotas = conn.Query<decimal>(
                sql,
                new
                {
                    request.operacion,
                    fecha = request.fecha_activacion
                }).ToList();

            foreach (var idSeq in cuotas)
            {
                conn.Execute(
                    "spCrdPlanPagosActivaCuota",
                    new
                    {
                        Operacion = request.operacion,
                        Seq = idSeq
                    },
                    commandType: CommandType.StoredProcedure);
            }

            return cuotas.Count > 0;
        }

        private static ErrorDto ValidarRevisarRequest(CrPlanPagosRevisarRequest request)
        {
            if (request == null || request.operacion <= 0)
            {
                return DbHelper.ErrorResponse(MsgOperacionRequerida, -2);
            }

            if (!request.ajusta_cuota.GetValueOrDefault())
            {
                return DbHelper.CreateOkResponse();
            }

            var factor = MCredito.FxCrdFactorCalculo(request.factor_calculo);
            var divisor = factor == "06" ? 2400 : 1200;
            var interesMinimo = request.saldo * request.tasa / divisor;

            if (request.cuota_manual < interesMinimo)
            {
                return DbHelper.ErrorResponse("La Cuota Manual no es válida porque es menor al cobro de intereses mínimo!", -2);
            }

            return DbHelper.CreateOkResponse();
        }

        private static void EjecutarRevision(
    SqlConnection conn,
    CrPlanPagosRevisarRequest request,
    decimal cuotaFija,
    int plazoExt)
        {
            conn.Execute(
                "spCrdPlanPagosRevision",
                new
                {
                    Operacion = request.operacion.GetValueOrDefault(),
                    Usuario = (request.usuario ?? string.Empty).Trim(),
                    Bitacora = 1,
                    CuotaFija = cuotaFija,
                    PlazoExt = plazoExt,
                    CtaDerivada = request.cuota_derivada.GetValueOrDefault() ? 1 : 0,
                    PlazoAumenta = request.plazo_aumenta_auto.GetValueOrDefault() ? 1 : 0
                },
                commandType: CommandType.StoredProcedure);
        }

        private static string ConstruirDetalleRevision(
            CrPlanPagosRevisarRequest request,
            decimal cuotaFija,
            int plazoExt)
        {
            var detalle =
                $"Revisión de Plan de Pago, Operacion: {request.operacion.GetValueOrDefault()}";

            if (request.ajusta_cuota.GetValueOrDefault())
            {
                detalle += $", Aj.Cta.Manual: {cuotaFija}";
            }

            detalle += $", Ext.Plazo: {plazoExt}";
            detalle += $", Cta.Deriv: {(request.cuota_derivada.GetValueOrDefault() ? 1 : 0)}";
            detalle += $", Plazo Aumenta: {(request.plazo_aumenta_auto.GetValueOrDefault() ? 1 : 0)}";

            return detalle;
        }

        private void RegistrarBitacora(
     int codEmpresa,
     string? usuario,
     string movimiento,
     string detalle)
        {
            _ = Bitacora(new BitacoraInsertarDto
            {
                EmpresaId = codEmpresa,
                Usuario = (usuario ?? string.Empty).Trim(),
                DetalleMovimiento = detalle,
                Movimiento = $"{movimiento} - WEB",
                Modulo = ModuloCreditos
            });
        }

        private static void EnsureOpen(IDbConnection conn)
        {
            if (conn.State != ConnectionState.Open)
            {
                conn.Open();
            }
        }
        private static CrPlanPagosReporteDto ObtenerReporteInfo(
    SqlConnection conn,
    string? usuario)
        {
            var reporte = new CrPlanPagosReporteDto
            {
                fecha_servidor = ObtenerFechaServidor(conn)
            };

            var usuarioReporte = (usuario ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(usuarioReporte))
            {
                return reporte;
            }

            const string sql = @"
        exec sbSIFOficinasUsuario @Usuario;";

            var oficina = conn.QueryFirstOrDefault<SifOficinaUsuarioReporteDto>(
                sql,
                new { Usuario = usuarioReporte });

            reporte.oficina = oficina?.descripcion ?? string.Empty;

            return reporte;
        }

        private static DateTime ObtenerFechaServidor(SqlConnection conn)
        {
            const string sql = @"
        select dbo.MyGetdate() as fecha;";

            return conn.QueryFirstOrDefault<DateTime>(sql);
        }

        private class SifOficinaUsuarioReporteDto
        {
            public string descripcion { get; set; } = string.Empty;
        }
    }
}