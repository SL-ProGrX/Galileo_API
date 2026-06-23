using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX.Creditos;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using System.Data;
using System.Text;

namespace Galileo_API.DataBaseTier.ProGrX.Creditos
{
    public class FrmCRConsultaDetalleDB
    {
        private readonly PortalDB _portalDB;
        private readonly MProGrxMain mProGrxDll;

        private const string ActaEstudioCredito = "Estudio Crédito";
        private const string ActaTramiteCredito = "Trámite Crédito";
        private const string TipoResolucion = "RES";
        private const string TipoAutorizacion = "AUT";
        private const string TipoAsistencia = "ASI";

        public FrmCRConsultaDetalleDB(IConfiguration config)
        {
            _portalDB = new PortalDB(config);
            mProGrxDll = new MProGrxMain(config);
        }

        /// <summary>
        /// Obtiene la información completa superior de la consulta detalle de crédito.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="operacion"></param>
        /// <param name="tipoActa"></param>
        /// <param name="tipoDetalle"></param>
        /// <returns></returns>
        public ErrorDto<CrConsultaDetalleCompletoDto> CR_ConsultaDetalle_Obtener(
    int CodEmpresa,
    int operacion,
    string? tipoActa,
    string? tipoDetalle,
    string usuario,
    int codContabilidad)
        {
            if (operacion <= 0)
            {
                return DbHelper.CreateErrorResponse<CrConsultaDetalleCompletoDto>(
                    "La operación es requerida.",
                    -1,
                    new CrConsultaDetalleCompletoDto());
            }

            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                var raw = ObtenerDetalleRaw(conn, operacion);
                if (raw == null)
                {
                    return DbHelper.CreateErrorResponse<CrConsultaDetalleCompletoDto>(
                        "No se encontró información para la operación indicada.",
                        -1,
                        new CrConsultaDetalleCompletoDto());
                }

                var request = NormalizarAprobacionRequest(tipoActa, tipoDetalle);
                var fechaProceso = ObtenerFechaProceso(CodEmpresa, usuario, codContabilidad);

                var result = new CrConsultaDetalleCompletoDto
                {
                    encabezado = MapEncabezado(raw),
                    formalizacion = MapFormalizacion(raw, fechaProceso),
                    otros = MapOtros(raw),
                    aprobacion = ObtenerAprobacion(conn, operacion, request.tipoActa, request.tipoDetalle),
                    bancos = ObtenerBancos(conn, operacion)
                };

                return DbHelper.CreateOkResponse(result);
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<CrConsultaDetalleCompletoDto>(
                    ex.Message,
                    -1,
                    new CrConsultaDetalleCompletoDto());
            }
        }

        /// <summary>
        /// Obtiene la lista de movimientos de la operación.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="operacion"></param>
        /// <param name="parametros"></param>
        /// <returns></returns>
        public ErrorDto<CrConsultaDetalleListaResult<CrConsultaDetalleMovimientoDto>> CR_ConsultaDetalle_Movimientos_Lista_Obtener(
            int CodEmpresa,
            int operacion,
            string parametros)
        {
            return EjecutarLista<CrConsultaDetalleMovimientoDto>(
                CodEmpresa,
                operacion,
                parametros,
                conn => conn.Query<CrConsultaDetalleMovimientoDto>(
                    "spCrd_Movimientos_New",
                    new { Operacion = operacion, Documento = 1 },
                    commandType: CommandType.StoredProcedure).ToList());
        }

        /// <summary>
        /// Exporta la lista de movimientos de la operación.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="operacion"></param>
        /// <param name="parametros"></param>
        /// <returns></returns>
        public ErrorDto<CrConsultaDetalleListaResult<CrConsultaDetalleMovimientoDto>> CR_ConsultaDetalle_Movimientos_Lista_Export(
            int CodEmpresa,
            int operacion,
            string parametros)
        {
            return CR_ConsultaDetalle_Movimientos_Lista_Obtener(CodEmpresa, operacion, ForzarExport(parametros));
        }

        /// <summary>
        /// Obtiene la lista de cuotas en morosidad de la operación.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="operacion"></param>
        /// <param name="parametros"></param>
        /// <returns></returns>
        public ErrorDto<CrConsultaDetalleListaResult<CrConsultaDetalleMorosidadDto>> CR_ConsultaDetalle_Morosidad_Lista_Obtener(
            int CodEmpresa,
            int operacion,
            string parametros)
        {
            return EjecutarLista<CrConsultaDetalleMorosidadDto>(
                CodEmpresa,
                operacion,
                parametros,
                conn =>
                {
                    var usaPlanPagos = ObtenerSysPlanPagos(conn) == 1;
                    return usaPlanPagos
                        ? ObtenerMorosidadPlanPagos(conn, operacion)
                        : ObtenerMorosidadSinPlanPagos(conn, operacion);
                });
        }

        /// <summary>
        /// Exporta la lista de cuotas en morosidad de la operación.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="operacion"></param>
        /// <param name="parametros"></param>
        /// <returns></returns>
        public ErrorDto<CrConsultaDetalleListaResult<CrConsultaDetalleMorosidadDto>> CR_ConsultaDetalle_Morosidad_Lista_Export(
            int CodEmpresa,
            int operacion,
            string parametros)
        {
            return CR_ConsultaDetalle_Morosidad_Lista_Obtener(CodEmpresa, operacion, ForzarExport(parametros));
        }

        /// <summary>
        /// Obtiene la lista de cierres mensuales de la operación.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="operacion"></param>
        /// <param name="parametros"></param>
        /// <returns></returns>
        public ErrorDto<CrConsultaDetalleListaResult<CrConsultaDetalleCierreDto>> CR_ConsultaDetalle_Cierre_Lista_Obtener(
            int CodEmpresa,
            int operacion,
            string parametros)
        {
            return EjecutarLista<CrConsultaDetalleCierreDto>(
                CodEmpresa,
                operacion,
                parametros,
                conn => conn.Query<CrConsultaDetalleCierreDto>(SqlCierre, new { operacion }).ToList());
        }

        /// <summary>
        /// Exporta la lista de cierres mensuales de la operación.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="operacion"></param>
        /// <param name="parametros"></param>
        /// <returns></returns>
        public ErrorDto<CrConsultaDetalleListaResult<CrConsultaDetalleCierreDto>> CR_ConsultaDetalle_Cierre_Lista_Export(
            int CodEmpresa,
            int operacion,
            string parametros)
        {
            return CR_ConsultaDetalle_Cierre_Lista_Obtener(CodEmpresa, operacion, ForzarExport(parametros));
        }

        /// <summary>
        /// Obtiene la lista de correcciones de la operación.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="operacion"></param>
        /// <param name="parametros"></param>
        /// <returns></returns>
        public ErrorDto<CrConsultaDetalleListaResult<CrConsultaDetalleCorreccionDto>> CR_ConsultaDetalle_Correcciones_Lista_Obtener(
            int CodEmpresa,
            int operacion,
            string parametros)
        {
            return EjecutarLista<CrConsultaDetalleCorreccionDto>(
                CodEmpresa,
                operacion,
                parametros,
                conn => conn.Query<CrConsultaDetalleCorreccionDto>(SqlCorrecciones, new { operacion }).ToList());
        }

        /// <summary>
        /// Exporta la lista de correcciones de la operación.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="operacion"></param>
        /// <param name="parametros"></param>
        /// <returns></returns>
        public ErrorDto<CrConsultaDetalleListaResult<CrConsultaDetalleCorreccionDto>> CR_ConsultaDetalle_Correcciones_Lista_Export(
            int CodEmpresa,
            int operacion,
            string parametros)
        {
            return CR_ConsultaDetalle_Correcciones_Lista_Obtener(CodEmpresa, operacion, ForzarExport(parametros));
        }

        /// <summary>
        /// Obtiene la lista de fiadores de la operación.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="operacion"></param>
        /// <param name="parametros"></param>
        /// <returns></returns>
        public ErrorDto<CrConsultaDetalleListaResult<CrConsultaDetalleFiadorDto>> CR_ConsultaDetalle_Fiadores_Lista_Obtener(
            int CodEmpresa,
            int operacion,
            string parametros)
        {
            return EjecutarLista<CrConsultaDetalleFiadorDto>(
                CodEmpresa,
                operacion,
                parametros,
                conn => conn.Query<CrConsultaDetalleFiadorDto>(SqlFiadores, new { operacion }).ToList());
        }

        /// <summary>
        /// Exporta la lista de fiadores de la operación.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="operacion"></param>
        /// <param name="parametros"></param>
        /// <returns></returns>
        public ErrorDto<CrConsultaDetalleListaResult<CrConsultaDetalleFiadorDto>> CR_ConsultaDetalle_Fiadores_Lista_Export(
            int CodEmpresa,
            int operacion,
            string parametros)
        {
            return CR_ConsultaDetalle_Fiadores_Lista_Obtener(CodEmpresa, operacion, ForzarExport(parametros));
        }

        /// <summary>
        /// Obtiene la lista de refundiciones de la operación.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="operacion"></param>
        /// <param name="parametros"></param>
        /// <returns></returns>
        public ErrorDto<CrConsultaDetalleListaResult<CrConsultaDetalleRefundicionDto>> CR_ConsultaDetalle_Refundiciones_Lista_Obtener(
            int CodEmpresa,
            int operacion,
            string parametros)
        {
            return EjecutarLista<CrConsultaDetalleRefundicionDto>(
                CodEmpresa,
                operacion,
                parametros,
                conn => conn.Query<CrConsultaDetalleRefundicionDto>(SqlRefundiciones, new { operacion }).ToList());
        }

        /// <summary>
        /// Exporta la lista de refundiciones de la operación.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="operacion"></param>
        /// <param name="parametros"></param>
        /// <returns></returns>
        public ErrorDto<CrConsultaDetalleListaResult<CrConsultaDetalleRefundicionDto>> CR_ConsultaDetalle_Refundiciones_Lista_Export(
            int CodEmpresa,
            int operacion,
            string parametros)
        {
            return CR_ConsultaDetalle_Refundiciones_Lista_Obtener(CodEmpresa, operacion, ForzarExport(parametros));
        }

        /// <summary>
        /// Obtiene la lista de desembolsos de la operación.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="operacion"></param>
        /// <param name="parametros"></param>
        /// <returns></returns>
        public ErrorDto<CrConsultaDetalleListaResult<CrConsultaDetalleDesembolsoDto>> CR_ConsultaDetalle_Desembolsos_Lista_Obtener(
            int CodEmpresa,
            int operacion,
            string parametros)
        {
            return EjecutarLista<CrConsultaDetalleDesembolsoDto>(
                CodEmpresa,
                operacion,
                parametros,
                conn => conn.Query<CrConsultaDetalleDesembolsoDto>(SqlDesembolsos, new { operacion }).ToList());
        }

        /// <summary>
        /// Exporta la lista de desembolsos de la operación.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="operacion"></param>
        /// <param name="parametros"></param>
        /// <returns></returns>
        public ErrorDto<CrConsultaDetalleListaResult<CrConsultaDetalleDesembolsoDto>> CR_ConsultaDetalle_Desembolsos_Lista_Export(
            int CodEmpresa,
            int operacion,
            string parametros)
        {
            return CR_ConsultaDetalle_Desembolsos_Lista_Obtener(CodEmpresa, operacion, ForzarExport(parametros));
        }

        /// <summary>
        /// Obtiene la lista de tags de la operación.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="operacion"></param>
        /// <param name="parametros"></param>
        /// <returns></returns>
        public ErrorDto<CrConsultaDetalleListaResult<CrConsultaDetalleTagDto>> CR_ConsultaDetalle_Tags_Lista_Obtener(
            int CodEmpresa,
            int operacion,
            string parametros)
        {
            return EjecutarLista<CrConsultaDetalleTagDto>(
                CodEmpresa,
                operacion,
                parametros,
                conn => conn.Query<CrConsultaDetalleTagDto>(SqlTags, new { operacion }).ToList());
        }

        /// <summary>
        /// Exporta la lista de tags de la operación.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="operacion"></param>
        /// <param name="parametros"></param>
        /// <returns></returns>
        public ErrorDto<CrConsultaDetalleListaResult<CrConsultaDetalleTagDto>> CR_ConsultaDetalle_Tags_Lista_Export(
            int CodEmpresa,
            int operacion,
            string parametros)
        {
            return CR_ConsultaDetalle_Tags_Lista_Obtener(CodEmpresa, operacion, ForzarExport(parametros));
        }

        private ErrorDto<CrConsultaDetalleListaResult<T>> EjecutarLista<T>(
            int CodEmpresa,
            int operacion,
            string parametros,
            Func<SqlConnection, List<T>> query)
        {
            if (operacion <= 0)
            {
                return CrearListaError<T>("La operación es requerida.");
            }

            var filtros = ParseFiltros(parametros);
            if (filtros.Code != 0)
            {
                return CrearListaError<T>(filtros.Description ?? "Parámetros inválidos.");
            }

            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                var lista = query(conn) ?? new List<T>();
                return DbHelper.CreateOkResponse(new CrConsultaDetalleListaResult<T>
                {
                    total = lista.Count,
                    lista = lista
                });
            }
            catch (SqlException ex)
            {
                return CrearListaError<T>(ex.Message);
            }
        }

        private static ErrorDto<FiltrosLazyLoadData> ParseFiltros(string parametros)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(parametros))
                {
                    return DbHelper.CreateOkResponse(new FiltrosLazyLoadData());
                }

                return DbHelper.CreateOkResponse(
                    JsonConvert.DeserializeObject<FiltrosLazyLoadData>(parametros) ?? new FiltrosLazyLoadData());
            }
            catch (JsonException ex)
            {
                return DbHelper.CreateErrorResponse<FiltrosLazyLoadData>(
                    ex.Message,
                    -1,
                    new FiltrosLazyLoadData());
            }
        }

        private static string ForzarExport(string parametros)
        {
            var filtros = ParseFiltros(parametros).Result ?? new FiltrosLazyLoadData();
            filtros.pagina = 0;
            filtros.paginacion = 0;
            return JsonConvert.SerializeObject(filtros);
        }

        private static ErrorDto<CrConsultaDetalleListaResult<T>> CrearListaError<T>(string mensaje)
        {
            return DbHelper.CreateErrorResponse<CrConsultaDetalleListaResult<T>>(
                mensaje,
                -1,
                new CrConsultaDetalleListaResult<T>());
        }

        private static (string tipoActa, string tipoDetalle) NormalizarAprobacionRequest(
            string? tipoActa,
            string? tipoDetalle)
        {
            var acta = string.IsNullOrWhiteSpace(tipoActa)
                ? ActaEstudioCredito
                : tipoActa.Trim();

            if (!acta.Equals(ActaTramiteCredito, StringComparison.OrdinalIgnoreCase))
            {
                acta = ActaEstudioCredito;
            }

            var detalle = (tipoDetalle ?? TipoResolucion).Trim().ToUpperInvariant();
            if (detalle != TipoResolucion && detalle != TipoAutorizacion && detalle != TipoAsistencia)
            {
                detalle = TipoResolucion;
            }

            return (acta, detalle);
        }

        private static CrConsultaDetalleOperacionRawDto? ObtenerDetalleRaw(SqlConnection conn, int operacion)
        {
            return conn.QueryFirstOrDefault<CrConsultaDetalleOperacionRawDto>(
                "spSys_Consulta_Integrada_Creditos_Detalle",
                new { Operacion = operacion },
                commandType: CommandType.StoredProcedure);
        }

        private static CrConsultaDetalleEncabezadoDto MapEncabezado(CrConsultaDetalleOperacionRawDto x)
        {
            var estadoPrestamo = S(x.Estado_Desc);
            estadoPrestamo += x.MoraCuotas == 0 ? "¦ Al Día" : "¦ Mora";

            return new CrConsultaDetalleEncabezadoDto
            {
                id_solicitud = x.ID_SOLICITUD,
                cedula = S(x.CEDULA),
                nombre = S(x.NOMBRE),
                codigo = S(x.CODIGO),
                descripcion = S(x.DESCRIPCION),
                cod_divisa = S(x.COD_DIVISA),
                garantia = S(x.GARANTIA),
                garantia_desc = S(x.GarantiaDesc),
                estado = S(x.ESTADO),
                estado_desc = S(x.Estado_Desc),
                proceso = S(x.PROCESO),
                proceso_desc = S(x.Proceso_Desc),
                estado_prestamo = estadoPrestamo,
                antiguedad = S(x.Antiguedad),
                monto_girado = D(x.MONTO_GIRADO),
                monto_credito = D(x.Monto_Credito),
                saldo_credito = D(x.Saldo_Credito),
                cuota = D(x.CUOTA),
                interesc = D(x.INTERESC),
                amortiza = D(x.AMORTIZA),
                poliza_cuota = D(x.Poliza_Cuota),
                mora_cuotas = x.MoraCuotas
            };
        }

        private static CrConsultaDetalleFormalizacionDto MapFormalizacion(
    CrConsultaDetalleOperacionRawDto x,
    string fechaProceso)
        {
            var tasa = CalcularTasa(x);

            return new CrConsultaDetalleFormalizacionDto
            {
                cod_destino = S(x.COD_DESTINO),
                destino_desc = S(x.DestinoDesc),
                cod_oficina_r = S(x.COD_OFICINA_R),
                oficina_desc = S(x.OficinaDesc),
                cod_grupo = S(x.COD_GRUPO),
                recurso_desc = S(x.RecursoDesc),
                cod_actividad = S(x.COD_ACTIVIDAD),
                actividad_desc = S(x.ActividadDesc),
                canal_tipo = S(x.CANAL_TIPO),
                canal_desc = S(x.CanalDesc),
                id_comite = S(x.ID_COMITE),
                comite_desc = S(x.ComiteDesc),
                id_promotor = S(x.ID_PROMOTOR),
                promotor_desc = S(x.PromotorDesc),
                userfor = S(x.USERFOR).ToUpperInvariant(),
                fechaforp = x.FECHAFORP,
                tdocumento = S(x.TDOCUMENTO),
                ndocumento = S(x.NDOCUMENTO),
                comprobante = $"{S(x.TDOCUMENTO)}-{S(x.NDOCUMENTO)}",
                plazo = x.PLAZO,
                interesv = D(x.INTERESV),
                txt_int_mora = tasa.intMora,
                tasa_label = tasa.label,
                tbp_puntos_add = tasa.tbpPuntosAdd,
                pts_add_mora = tasa.ptsAddMora,
                pts_add_liq = tasa.ptsAddLiq,
                tasa_piso = ObtenerTasaPiso(x.TASA_PISO),
                prideduc = S(x.PRIDEDUC),
                fecult = S(x.FECULT),
                anio_primer_abono = ObtenerAnioProceso(x.PRIDEDUC),
                mes_primer_abono = ObtenerMesProceso(x.PRIDEDUC),
                anio_ultimo_abono = ObtenerAnioProceso(x.FECULT),
                mes_ultimo_abono = ObtenerMesProceso(x.FECULT),
                anio_terminacion = ObtenerAnioTerminacion(x),
                mes_terminacion = ObtenerMesTerminacion(x),
                dia_pago_desc = x.DIA_PAGO == 32 ? "Ult.Día.Mes." : $"Todos los {x.DIA_PAGO}",
                base_calculo_desc = S(x.BASE_CALCULO_DESC),
                cuotas_planilla = S(x.CUOTAS_PLANILLA),
                cuotas_directas = S(x.CUOTAS_DIRECTAS),
                cuotas_anuladas = S(x.CUOTAS_ANULADAS),
                fecha_proceso = fechaProceso
            };
        }

        private static CrConsultaDetalleOtrosDto MapOtros(CrConsultaDetalleOperacionRawDto x)
        {
            var salidaTipo = S(x.Salida_Tipo);
            var documentoReferido = S(x.DOCUMENTO_REFERIDO);

            var salidaTipoDesc = string.IsNullOrWhiteSpace(documentoReferido)
                ? salidaTipo
                : $"{salidaTipo}..{documentoReferido}";

            return new CrConsultaDetalleOtrosDto
            {
                cuenta_iban = S(x.CUENTA_IBAN),
                iban = S(x.IBAN),
                salida_tipo = salidaTipo,
                salida_desc = S(x.Salida_Desc),
                documento_referido = documentoReferido,
                salida_tipo_desc = salidaTipoDesc,
                deductora_cod = S(x.DeductoraCod),
                deductora_desc = S(x.DEDUCTORA_DESC),
                deductora_desc_corta = S(x.DEDUCTORA_DESC_CORTA),
                divisa_desc = S(x.DIVISA_DESC),
                currency_sim = S(x.CURRENCY_SIM),
                cbr_externo = x.CbrExterno,
                cobro_fiador = x.CobroFiador
            };
        }
        private string ObtenerFechaProceso(int CodEmpresa, string usuario, int codContabilidad)
        {
            var globalesDto = mProGrxDll.sbSifParametrosInicializa(
                CodEmpresa,
                usuario,
                codContabilidad);

            var fechaProceso = globalesDto?.Result?.GlngFechaCR ?? 0;

            return fechaProceso > 0
                ? Convert.ToString(fechaProceso)
                : string.Empty;
        }
        private static CrConsultaDetalleAprobacionDto ObtenerAprobacion(
            SqlConnection conn,
            int operacion,
            string tipoActa,
            string tipoDetalle)
        {
            var expediente = tipoActa.Equals(ActaEstudioCredito, StringComparison.OrdinalIgnoreCase)
                ? ObtenerExpedienteEstudio(conn, operacion)
                : operacion.ToString();

            var lista = tipoActa.Equals(ActaEstudioCredito, StringComparison.OrdinalIgnoreCase)
                ? conn.Query<CrConsultaDetalleResolucionDto>(
                    "spCrd_Estudio_Resolucion_Detalle",
                    new { Expediente = expediente, Tipo = tipoDetalle },
                    commandType: CommandType.StoredProcedure).ToList()
                : conn.Query<CrConsultaDetalleResolucionDto>(
                    "spCrd_SGT_Resolucion_Detalle",
                    new { Operacion = operacion, Tipo = tipoDetalle },
                    commandType: CommandType.StoredProcedure).ToList();

            var first = lista.FirstOrDefault();

            return new CrConsultaDetalleAprobacionDto
            {
                tipo_acta = tipoActa,
                tipo_detalle = tipoDetalle,
                acta = first?.acta ?? string.Empty,
                acta_fecha = first?.acta_fecha,
                lista = lista
            };
        }

        private static string ObtenerExpedienteEstudio(SqlConnection conn, int operacion)
        {
            const string sql = @"
                select top 1 COD_PREANALISIS
                from CRD_PREA_PREANALISIS
                where ID_SOLICITUD = @operacion;";

            return conn.QueryFirstOrDefault<string>(sql, new { operacion }) ?? string.Empty;
        }

        private static CrConsultaDetalleBancosDto ObtenerBancos(SqlConnection conn, int operacion)
        {
            var baseInfo = conn.QueryFirstOrDefault<CrConsultaDetalleBancoBaseDto>(SqlBancoBase, new { operacion });
            if (baseInfo == null)
            {
                return new CrConsultaDetalleBancosDto();
            }

            var texto = new StringBuilder();

            texto.AppendLine($"Línea         : {S(baseInfo.codigo)}");
            texto.AppendLine($"Descripción   : {S(baseInfo.descripcion)}");
            texto.AppendLine($"Identificación: {S(baseInfo.cedula)}");
            texto.AppendLine($"Nombre        : {S(baseInfo.nombre)}");
            texto.AppendLine($"Monto a Girar : {D(baseInfo.monto_girado):N2}");
            texto.AppendLine();
            texto.AppendLine();

            AppendRemesa(conn, operacion, texto);
            AppendBancoTransaccion(conn, operacion, texto);

            return new CrConsultaDetalleBancosDto
            {
                texto = texto.ToString()
            };
        }

        private static void AppendRemesa(SqlConnection conn, int operacion, StringBuilder texto)
        {
            var remesa = conn.QueryFirstOrDefault<CrConsultaDetalleRemesaDto>(SqlRemesa, new { operacion });

            if (remesa == null)
            {
                texto.AppendLine(">>> REMESA DE PAGO: NO SE LOCALIZÓ NINGUNA <<<");
                texto.AppendLine();
                return;
            }

            texto.AppendLine(":::. REMESA DE PAGO .::");
            texto.AppendLine();
            texto.AppendLine($"Remesa Id      : {remesa.cod_remesa}");
            texto.AppendLine($"Estado         : {EstadoRemesa(remesa.estado)}");
            texto.AppendLine($"Fecha Creación : {remesa.fecha}");
            texto.AppendLine($"Usuario        : {S(remesa.usuario)}");
            texto.AppendLine($"Monto          : {D(remesa.monto):N2}");
            texto.AppendLine($"Desembolsos Add: {D(remesa.desembolsos):N2}");
            texto.AppendLine($"Tesorería Id   : {S(remesa.nsolicitud)}");
            texto.AppendLine();
            texto.AppendLine();
        }

        private static void AppendBancoTransaccion(SqlConnection conn, int operacion, StringBuilder texto)
        {
            var banco = conn.QueryFirstOrDefault<CrConsultaDetalleBancoTransaccionDto>(SqlBancoTransaccion, new { operacion });
            if (banco == null)
            {
                return;
            }

            texto.AppendLine(":::. BANCOS .::");
            texto.AppendLine();
            texto.AppendLine($"Estado       : {S(banco.estado_desc)}");
            texto.AppendLine($"Solicitud    : {S(banco.nsolicitud)}");
            texto.AppendLine($"Documento    : {S(banco.ndocumento)}     TF: {S(banco.documento_base)}");
            texto.AppendLine($"Beneficiario : {S(banco.beneficiario)}");
            texto.AppendLine($"Banco        : {S(banco.banco_desc)}");
            texto.AppendLine($"Cuenta       : {S(banco.cuenta_desc)}");
            texto.AppendLine($"Tipo         : {S(banco.tipo_desc)}");
            texto.AppendLine($"Fec.Solicita : {banco.fecha_solicitud:yyyy-MM-dd HH:mm:ss}");
            texto.AppendLine($"Fec.Emite    : {banco.fecha_emision:yyyy-MM-dd HH:mm:ss}");
        }

        private static int ObtenerSysPlanPagos(SqlConnection conn)
        {
            const string sql = @"select isnull(SysCrdPlanPago,0) from SIF_Empresa;";
            return conn.QueryFirstOrDefault<int>(sql);
        }

        private static List<CrConsultaDetalleMorosidadDto> ObtenerMorosidadPlanPagos(SqlConnection conn, int operacion)
        {
            const string sql = @"
                select
                    substring(convert(varchar(10), FECHA_PROCESO),1,4) + ' - ' +
                    substring(convert(varchar(10), FECHA_PROCESO),5,2) as proceso,
                    INTCOR as intcor,
                    INTMOR as intmor,
                    PRINCIPAL as principal,
                    POLIZA as poliza,
                    CARGOS as cargos,
                    (INTCOR + INTMOR + PRINCIPAL + CARGOS + POLIZA) as total
                from CRD_OPERACION_TRANSAC
                where MORA_DIAS > 0
                  and ESTADO = 'A'
                  and ID_SOLICITUD = @operacion
                order by FECHA_PROCESO;";

            return conn.Query<CrConsultaDetalleMorosidadDto>(sql, new { operacion }).ToList();
        }

        private static List<CrConsultaDetalleMorosidadDto> ObtenerMorosidadSinPlanPagos(SqlConnection conn, int operacion)
        {
            const string sql = @"
                select
                    substring(convert(varchar(10), FECHAP),1,4) + ' - ' +
                    substring(convert(varchar(10), FECHAP),5,2) as proceso,
                    INTC as intcor,
                    INTM as intmor,
                    AMORTIZA as principal,
                    cast(0 as decimal(18,2)) as poliza,
                    CARGO as cargos,
                    (INTC + INTM + AMORTIZA + CARGO) as total
                from morosidad
                where estado = 'A'
                  and id_solicitud = @operacion
                order by Fechap desc;";

            return conn.Query<CrConsultaDetalleMorosidadDto>(sql, new { operacion }).ToList();
        }

        private static (string label, string tbpPuntosAdd, string ptsAddMora, decimal ptsAddLiq, decimal intMora) CalcularTasa(
            CrConsultaDetalleOperacionRawDto x)
        {
            var label = x.TBP_PuntosAdd.HasValue
                ? $"Tasa :TBP + {x.TBP_PuntosAdd} pts"
                : "Tasa %";

            var tbp = x.TBP_PuntosAdd.HasValue ? x.TBP_PuntosAdd.Value.ToString() : "N/A";
            var mora = CalcularMora(x);
            var ptsLiq = x.LiqTasaX == 0 ? 0 : D(x.Liq_Valor);

            if (x.LiqTasaX == 0)
            {
                label += " + LiqPts";
            }

            return (label, tbp, mora.pts, ptsLiq, mora.tasa);
        }

        private static (string pts, decimal tasa) CalcularMora(CrConsultaDetalleOperacionRawDto x)
        {
            var add = D(x.Tasa_Mora_Add);
            var interes = D(x.INTERESV);

            return S(x.Tasa_Mora_Tipo) switch
            {
                "PTS" => ($"{add} pts", add + interes),
                "POR" => ($"{add}%", interes + (interes * add / 100)),
                "TF" => ("0", add),
                _ => ("0", 0)
            };
        }

        private static string ObtenerTasaPiso(decimal? tasaPiso)
        {
            if (!tasaPiso.HasValue || tasaPiso.Value == 0)
            {
                return "N/A";
            }

            return tasaPiso.Value.ToString();
        }

        private static int? ObtenerAnioProceso(object? valor)
        {
            var texto = S(valor);
            return texto.Length >= 4 && int.TryParse(texto[..4], out var result) ? result : null;
        }

        private static int? ObtenerMesProceso(object? valor)
        {
            var texto = S(valor);
            return texto.Length >= 6 && int.TryParse(texto.Substring(4, 2), out var result) ? result : null;
        }

        private static int? ObtenerAnioTerminacion(CrConsultaDetalleOperacionRawDto x)
        {
            if (S(x.ESTADO) == "C")
            {
                return ObtenerAnioProceso(x.FECULT);
            }

            return x.Termina?.Year;
        }

        private static int? ObtenerMesTerminacion(CrConsultaDetalleOperacionRawDto x)
        {
            if (S(x.ESTADO) == "C")
            {
                return ObtenerMesProceso(x.FECULT);
            }

            return x.Termina?.Month;
        }

        private static string EstadoRemesa(string? estado)
        {
            return S(estado) switch
            {
                "A" => "Abierta",
                "C" => "Cerrada",
                "T" => "Trasladada",
                _ => string.Empty
            };
        }
        private static string S(object? value)
        {
            return Convert.ToString(value)?.Trim() ?? string.Empty;
        }

        private static decimal D(decimal? value)
        {
            return value ?? 0;
        }

        private const string SqlCierre = @"
            select
                Anio as anio,
                case MES
                    when 1 then 'Enero'
                    when 2 then 'Febrero'
                    when 3 then 'Marzo'
                    when 4 then 'Abril'
                    when 5 then 'Mayo'
                    when 6 then 'Junio'
                    when 7 then 'Julio'
                    when 8 then 'Agosto'
                    when 9 then 'Septiembre'
                    when 10 then 'Octubre'
                    when 11 then 'Noviembre'
                    when 12 then 'Diciembre'
                    else ''
                end as mes,
                SALDO_FINAL as saldo_final,
                TOTAL_DEBITOS as total_debitos,
                TOTAL_CREDITOS as total_creditos,
                case when OPEX = 1 then 'Si' else 'No' end as opex,
                case
                    when PROCESO = 'N' then 'Normal'
                    when PROCESO = 'T' then 'Tra.Deuda'
                    when PROCESO = 'J' then 'Cbr.Jud.'
                    else 'Incobrable'
                end as proceso,
                case
                    when estado = 'A' then 'Activa'
                    when estado = 'C' then 'Cancelada'
                    when estado = 'N' then 'Anulada'
                    else 'En Tramite'
                end as estado,
                substring(convert(varchar(10), PRIDEDUC),1,4) + ' - ' +
                substring(convert(varchar(10), PRIDEDUC),5,2) as prideduc,
                substring(convert(varchar(10), FECULT),1,4) + ' - ' +
                substring(convert(varchar(10), FECULT),5,2) as fecult,
                TASA as tasa,
                PLAZO as plazo,
                CUOTA as cuota
            from ASE_PER_CERRADOS
            where ID_SOLICITUD = @operacion
            order by ANIO desc, MES desc;";

        private const string SqlCorrecciones = @"
            select
                FECHA as fecha,
                dbo.fxCrdMovimientoCorrectivo(MOVIMIENTO) as movimiento,
                USUARIO as usuario,
                DETALLE as detalle,
                NOTAS as notas
            from credito_suBit
            where id_solicitud = @operacion
            order by fecha desc;";

        private const string SqlFiadores = @"
            select
                S.cedula as cedula,
                S.nombre as nombre,
                E.descripcion as estado,
                I.descripcion as institucion
            from fiadores F
            inner join Socios S on F.cedulaf = S.cedula
            inner join Instituciones I on S.cod_institucion = I.cod_institucion
            inner join AFI_ESTADOS_PERSONA E on E.cod_estado = S.estadoActual
            where F.estado = 'A'
              and F.id_solicitud = @operacion;";

        private const string SqlRefundiciones = @"
            select
                ID_SOLICITUD as id_solicitud,
                CODIGO as codigo,
                cast(0 as decimal(18,2)) as intcor,
                cast(0 as decimal(18,2)) as intmor,
                cast(0 as decimal(18,2)) as cargo,
                MONTO as monto
            from REFUNDE_RETENCION
            where ID_SOLICITUDR = @operacion
            union
            select
                ID_SOLICITUD as id_solicitud,
                CODIGO as codigo,
                INTCOR as intcor,
                INTMOR as intmor,
                isnull(CARGOS,0) as cargo,
                MONTO as monto
            from REFUNDICIONES
            where ID_SOLICITUDR = @operacion;";

        private const string SqlDesembolsos = @"
            select
                CONCEPTO as concepto,
                MONTO as monto,
                CUENTA_CONTA as cuenta_conta,
                RETENER as retener,
                MODIFICA as modifica
            from DESEMBOLSOS
            where id_solicitud = @operacion;";

        private const string SqlTags = @"
            select
                T.TAG_CODIGO as tag_codigo,
                T.DESCRIPCION as descripcion,
                O.REGISTRO_FECHA as registro_fecha,
                O.REGISTRO_USUARIO as registro_usuario,
                O.NOTAS as notas
            from CRD_TAGS T
            inner join CRD_OPERACION_TAGS O on T.TAG_CODIGO = O.TAG_CODIGO
            where O.ID_SOLICITUD = @operacion;";

        private const string SqlBancoBase = @"
            select
                R.id_solicitud,
                R.codigo,
                R.cedula,
                R.monto_girado,
                C.descripcion,
                S.nombre
            from reg_creditos R
            inner join Socios S on R.cedula = S.cedula
            inner join Catalogo C on R.codigo = C.codigo
            where R.id_solicitud = @operacion
              and R.estado in ('A','C');";

        private const string SqlRemesa = @"
            select
                Td.cod_remesa,
                T.ESTADO as estado,
                T.USUARIO as usuario,
                T.FECHA as fecha,
                Td.Monto as monto,
                Td.DESEMBOLSOS as desembolsos,
                Td.NSolicitud as nsolicitud
            from CRD_REMESAS_TES T
            inner join CRD_REMESAS_TES_DETALLE Td on T.COD_REMESA = Td.COD_REMESA
            where Td.Id_solicitud = @operacion;";

        private const string SqlBancoTransaccion = @"
            select
                T.NSOLICITUD as nsolicitud,
                T.ndocumento as ndocumento,
                T.BENEFICIARIO as beneficiario,
                T.FECHA_SOLICITUD as fecha_solicitud,
                T.FECHA_EMISION as fecha_emision,
                '[' + B.CTA + '] ' + B.DESCRIPCION as cuenta_desc,
                Bg.DESCRIPCION as banco_desc,
                Td.DESCRIPCION as tipo_desc,
                case
                    when T.Estado in ('P','S') then 'SOLICITADA'
                    when T.Estado in ('T','E','I') then 'EMITIDO'
                    when T.Estado in ('A','N') then 'ANULADA'
                    else ''
                end as estado_desc,
                T.DOCUMENTO_BASE as documento_base
            from Tes_Transacciones T
            inner join TES_BANCOS B on T.ID_BANCO = B.ID_BANCO
            inner join TES_BANCOS_GRUPOS Bg on B.COD_GRUPO = Bg.COD_GRUPO
            inner join TES_TIPOS_DOC Td on T.tipo = Td.TIPO
            where T.op = @operacion
              and T.estado in ('I','T','P','E');";

        public sealed class CrConsultaDetalleOperacionRawDto
        {
            public int ID_SOLICITUD { get; set; }
            public string? CEDULA { get; set; }
            public string? NOMBRE { get; set; }
            public string? CODIGO { get; set; }
            public string? DESCRIPCION { get; set; }
            public string? COD_DIVISA { get; set; }
            public string? GARANTIA { get; set; }
            public string? GarantiaDesc { get; set; }
            public string? ESTADO { get; set; }
            public string? Estado_Desc { get; set; }
            public string? PROCESO { get; set; }
            public string? Proceso_Desc { get; set; }
            public string? Antiguedad { get; set; }
            public decimal? MONTO_GIRADO { get; set; }
            public decimal? Monto_Credito { get; set; }
            public decimal? Saldo_Credito { get; set; }
            public decimal? CUOTA { get; set; }
            public decimal? INTERESC { get; set; }
            public decimal? AMORTIZA { get; set; }
            public decimal? Poliza_Cuota { get; set; }
            public int MoraCuotas { get; set; }
            public string? COD_DESTINO { get; set; }
            public string? DestinoDesc { get; set; }
            public string? COD_OFICINA_R { get; set; }
            public string? OficinaDesc { get; set; }
            public string? COD_GRUPO { get; set; }
            public string? RecursoDesc { get; set; }
            public string? COD_ACTIVIDAD { get; set; }
            public string? ActividadDesc { get; set; }
            public string? CANAL_TIPO { get; set; }
            public string? CanalDesc { get; set; }
            public string? ID_COMITE { get; set; }
            public string? ComiteDesc { get; set; }
            public string? ID_PROMOTOR { get; set; }
            public string? PromotorDesc { get; set; }
            public string? USERFOR { get; set; }
            public DateTime? FECHAFORP { get; set; }
            public string? TDOCUMENTO { get; set; }
            public string? NDOCUMENTO { get; set; }
            public int PLAZO { get; set; }
            public decimal? INTERESV { get; set; }
            public decimal? TBP_PuntosAdd { get; set; }
            public decimal? Tasa_Mora_Add { get; set; }
            public string? Tasa_Mora_Tipo { get; set; }
            public decimal? TASA_PISO { get; set; }
            public decimal? Liq_Valor { get; set; }
            public int LiqTasaX { get; set; }
            public string? PRIDEDUC { get; set; }
            public string? FECULT { get; set; }
            public DateTime? Termina { get; set; }
            public int DIA_PAGO { get; set; }
            public string? BASE_CALCULO_DESC { get; set; }
            public string? CUOTAS_PLANILLA { get; set; }
            public string? CUOTAS_DIRECTAS { get; set; }
            public string? CUOTAS_ANULADAS { get; set; }
            public string? CUENTA_IBAN { get; set; }
            public string? IBAN { get; set; }
            public string? Salida_Tipo { get; set; }
            public string? Salida_Desc { get; set; }
            public string? DOCUMENTO_REFERIDO { get; set; }
            public string? DeductoraCod { get; set; }
            public string? DEDUCTORA_DESC { get; set; }
            public string? DEDUCTORA_DESC_CORTA { get; set; }
            public string? DIVISA_DESC { get; set; }
            public string? CURRENCY_SIM { get; set; }
            public int CbrExterno { get; set; }
            public int CobroFiador { get; set; }
        }

        public sealed class CrConsultaDetalleBancoBaseDto
        {
            public string? codigo { get; set; }
            public string? cedula { get; set; }
            public decimal? monto_girado { get; set; }
            public string? descripcion { get; set; }
            public string? nombre { get; set; }
        }

        public sealed class CrConsultaDetalleRemesaDto
        {
            public int cod_remesa { get; set; }
            public string? estado { get; set; }
            public string? usuario { get; set; }
            public DateTime? fecha { get; set; }
            public decimal? monto { get; set; }
            public decimal? desembolsos { get; set; }
            public string? nsolicitud { get; set; }
        }

        public sealed class CrConsultaDetalleBancoTransaccionDto
        {
            public string? nsolicitud { get; set; }
            public string? ndocumento { get; set; }
            public string? beneficiario { get; set; }
            public DateTime? fecha_solicitud { get; set; }
            public DateTime? fecha_emision { get; set; }
            public string? cuenta_desc { get; set; }
            public string? banco_desc { get; set; }
            public string? tipo_desc { get; set; }
            public string? estado_desc { get; set; }
            public string? documento_base { get; set; }
        }
    }
}