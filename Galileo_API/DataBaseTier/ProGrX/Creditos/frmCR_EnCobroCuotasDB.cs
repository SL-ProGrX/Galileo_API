using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX.Creditos;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using System.Data;

namespace Galileo_API.DataBaseTier.ProGrX.Creditos
{
    public class FrmCrEnCobroCuotasDB
    {
        private readonly PortalDB _portalDB;
        private readonly MCobroDb _mCobroDb;
        private readonly MSeguimientoDB _mSeguimientoDB;
        private const int ScrollSiguiente = 1;
        private const int ScrollAnterior = 2;
        private const int MesesDefault = 12;
        private const string MensajeCedulaRequerida = "La cédula es requerida.";
        private const string MensajeInstitucionRequerida = "La institución es requerida.";
        private const string MensajeProcesoInvalido = "El proceso indicado no es válido.";
        private const string DIFERENCIA = "diferencia";

        public FrmCrEnCobroCuotasDB(IConfiguration config)
        {
            _portalDB = new PortalDB(config);
            _mCobroDb = new MCobroDb(config);
            _mSeguimientoDB = new MSeguimientoDB(config);
        }

        /// <summary>
        /// Obtiene la información inicial de la pantalla según la cédula recibida desde la pantalla padre.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="cedula"></param>
        /// <returns></returns>
        public ErrorDto<CrEnCobroCuotasInicialDto> CR_EnCobroCuotas_Inicial_Obtener(int CodEmpresa, string cedula)
        {
            if (string.IsNullOrWhiteSpace(cedula))
            {
                return DbHelper.CreateErrorResponse(
                    MensajeCedulaRequerida,
                    -2,
                    new CrEnCobroCuotasInicialDto());
            }

            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                const string sql = @"
                    select
                        rtrim(S.cedula) as cedula,
                        rtrim(S.nombre) as nombre,
                        isnull(S.cod_institucion,0) as cod_institucion,
                        isnull(I.frecuencia,'M') as frecuencia_id
                    from socios S
                    left join instituciones I on S.cod_institucion = I.cod_institucion
                    where S.cedula = @cedula;";

                var data = conn.QueryFirstOrDefault<CrEnCobroCuotasInicialDto>(
                    sql,
                    new { cedula = cedula.Trim() });

                if (data == null)
                {
                    return DbHelper.CreateErrorResponse(
                        "No se encontró la persona indicada.",
                        -2,
                        new CrEnCobroCuotasInicialDto());
                }

                data.proceso = _mSeguimientoDB.fxPrimerDeduccion(CodEmpresa);
                data.proceso_format = MCobroDb.fxFechaProcesoFormat(data.proceso);

                return DbHelper.CreateOkResponse(data);
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse(
                    ex.Message,
                    -1,
                    new CrEnCobroCuotasInicialDto());
            }
        }

        /// <summary>
        /// Obtiene las deductoras vinculadas a la institución base de la persona.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="codInstitucion"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CR_EnCobroCuotas_Deductoras_Dropdown_Obtener(int CodEmpresa, int codInstitucion)
        {
            if (codInstitucion <= 0)
            {
                return DbHelper.CreateErrorResponse(
                    MensajeInstitucionRequerida,
                    -2,
                    new List<DropDownListaGenericaModel>());
            }

            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                const string sql = @"exec spAFI_Institucion_Vinculadas @Institucion, 3;";

                var lista = conn.Query<DeductoraRow>(
                    sql,
                    new { Institucion = codInstitucion })
                    .Select(x => new DropDownListaGenericaModel
                    {
                        item = Convert.ToString(x.Idx) ?? string.Empty,
                        descripcion = (x.ItmX ?? string.Empty).Trim()
                    })
                    .ToList();

                return DbHelper.CreateOkResponse(lista);
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse(
                    ex.Message,
                    -1,
                    new List<DropDownListaGenericaModel>());
            }
        }

        public sealed class DeductoraRow
        {
            public int Idx { get; }
            public string ItmX { get; }

            public DeductoraRow(int idx, string itmX)
            {
                Idx = idx;
                ItmX = itmX;
            }
        }

        /// <summary>
        /// Obtiene frecuencia y último proceso de envío de una deductora; si no existe último envío devuelve el proceso actual del sistema.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="codInstitucion"></param>
        /// <returns></returns>
        public ErrorDto<CrEnCobroCuotasInicialDto> CR_EnCobroCuotas_Deductora_Info_Obtener(int CodEmpresa, int codInstitucion)
        {
            if (codInstitucion <= 0)
            {
                return DbHelper.CreateErrorResponse(
                    MensajeInstitucionRequerida,
                    -2,
                    new CrEnCobroCuotasInicialDto());
            }

            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                const string sql = @"
                    select
                        isnull(I.frecuencia,'M') as frecuencia_id,
                        isnull(E.proceso,0) as proceso
                    from instituciones I
                    left join vCrd_Deductora_Ultimo_Envio E
                        on I.cod_institucion = E.cod_institucion
                    where I.cod_institucion = @codInstitucion;";

                var data = conn.QueryFirstOrDefault<CrEnCobroCuotasInicialDto>(
                    sql,
                    new { codInstitucion })
                    ?? new CrEnCobroCuotasInicialDto();

                if (data.proceso <= 0)
                {
                    data.proceso = _mSeguimientoDB.fxPrimerDeduccion(CodEmpresa);
                }

                data.cod_institucion = codInstitucion;
                data.proceso_format = MCobroDb.fxFechaProcesoFormat(data.proceso);

                return DbHelper.CreateOkResponse(data);
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse(
                    ex.Message,
                    -1,
                    new CrEnCobroCuotasInicialDto());
            }
        }

        /// <summary>
        /// Navega al siguiente o anterior proceso.
        /// scrollCode: 1=siguiente, 2=anterior.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="scrollCode"></param>
        /// <param name="procesoActual"></param>
        /// <returns></returns>
        public ErrorDto<CrEnCobroCuotasProcesoScrollDto> CR_EnCobroCuotas_Proceso_Scroll_Obtener(int CodEmpresa, int scrollCode, decimal procesoActual)
        {
            if (procesoActual <= 0)
            {
                return DbHelper.CreateErrorResponse(
                    MensajeProcesoInvalido,
                    -2,
                    new CrEnCobroCuotasProcesoScrollDto());
            }

            try
            {
                decimal proceso = scrollCode switch
                {
                    ScrollSiguiente => _mCobroDb.fxFechaProcesoSiguiente(CodEmpresa, procesoActual),
                    ScrollAnterior => _mCobroDb.fxFechaProcesoAnterior(CodEmpresa, procesoActual),
                    _ => procesoActual
                };

                return DbHelper.CreateOkResponse(new CrEnCobroCuotasProcesoScrollDto
                {
                    proceso = proceso,
                    proceso_format = MCobroDb.fxFechaProcesoFormat(proceso)
                });
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse(
                    ex.Message,
                    -1,
                    new CrEnCobroCuotasProcesoScrollDto());
            }
        }

        /// <summary>
        /// Obtiene la lista de resumen comparativo por operación.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<CrEnCobroCuotasListaResult<CrEnCobroCuotasResumenData>> CR_EnCobroCuotas_Resumen_Lista_Obtener(int CodEmpresa,CrEnCobroCuotasConsultaRequest request)
        {
            var ctxResult = BuildContext(request);
            if (ctxResult.errorResumen != null) return ctxResult.errorResumen;

            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                const string sql = @"exec spPrm_Compara_Persona @Institucion, @Proceso, @Cedula;";

                var lista = conn.Query<CrEnCobroCuotasResumenRow>(
                    sql,
                    new
                    {
                        Institucion = ctxResult.ctx.codInstitucion,
                        Proceso = ctxResult.ctx.proceso,
                        Cedula = ctxResult.ctx.cedula
                    })
                    .Select(x => new CrEnCobroCuotasResumenData
                    {
                        operacion = x.Operacion,
                        linea = (x.Linea ?? string.Empty).Trim(),
                        envio = x.Envio,
                        recibido = x.Recibido,
                        diferencia = x.Recibido - x.Envio,
                        tipo_desc = (x.TipoDesc ?? string.Empty).Trim(),
                        linea_desc = (x.LineaDesc ?? string.Empty).Trim()
                    })
                    .ToList();

                return BuildResumenResult(lista, ctxResult.ctx.filtros);
            }
            catch (SqlException ex)
            {
                return ErrorLista<CrEnCobroCuotasResumenData>(ex.Message);
            }
        }
        public sealed class CrEnCobroCuotasResumenRow
        {
            public int Operacion { get; set; }
            public string Linea { get; set; } = string.Empty;
            public decimal Envio { get; set; }
            public decimal Recibido { get; set; }
            public string TipoDesc { get; set; } = string.Empty;
            public string LineaDesc { get; set; } = string.Empty;
        }
        /// <summary>
        /// Exporta la lista completa de resumen comparativo por operación.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<CrEnCobroCuotasListaResult<CrEnCobroCuotasResumenData>> CR_EnCobroCuotas_Resumen_Lista_Export(
            int CodEmpresa,
            CrEnCobroCuotasConsultaRequest request)
        {
            request.parametros = ResetPaginacion(request.parametros);
            return CR_EnCobroCuotas_Resumen_Lista_Obtener(CodEmpresa, request);
        }

        /// <summary>
        /// Obtiene la lista de detalle comparativo de planilla.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<CrEnCobroCuotasListaResult<CrEnCobroCuotasDetalleData>> CR_EnCobroCuotas_Detalle_Lista_Obtener(
            int CodEmpresa,
            CrEnCobroCuotasConsultaRequest request)
        {
            var ctxResult = BuildContext(request);
            if (ctxResult.errorDetalle != null) return ctxResult.errorDetalle;

            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                const string sql = @"exec spPrm_Planilla_Compara_Detalle @Cedula, @Proceso, @Institucion;";

                var lista = conn.Query<CrEnCobroCuotasDetalleData>(
                    sql,
                    new
                    {
                        ctxResult.ctx.Cedula,
                        ctxResult.ctx.Proceso,
                        ctxResult.ctx.Institucion
                    })
                    .Select(MapDetalle)
                    .ToList();

                return BuildDetalleResult(lista, ctxResult.ctx.filtros);
            }
            catch (SqlException ex)
            {
                return ErrorLista<CrEnCobroCuotasDetalleData>(ex.Message);
            }
        }

        /// <summary>
        /// Exporta la lista completa de detalle comparativo de planilla.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<CrEnCobroCuotasListaResult<CrEnCobroCuotasDetalleData>> CR_EnCobroCuotas_Detalle_Lista_Export(
            int CodEmpresa,
            CrEnCobroCuotasConsultaRequest request)
        {
            request.parametros = ResetPaginacion(request.parametros);
            return CR_EnCobroCuotas_Detalle_Lista_Obtener(CodEmpresa, request);
        }

        /// <summary>
        /// Obtiene la lista de cuotas enviadas.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<CrEnCobroCuotasListaResult<CrEnCobroCuotasEnvioData>> CR_EnCobroCuotas_Envio_Lista_Obtener(
            int CodEmpresa,
            CrEnCobroCuotasConsultaRequest request)
        {
            var ctxResult = BuildContext(request);
            if (ctxResult.errorEnvio != null) return ctxResult.errorEnvio;

            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                const string sql = @"
                    select
                        C.id_solicitud,
                        rtrim(R.codigo) as codigo,
                        rtrim(X.descripcion) as descripcion,
                        isnull(C.cuota,0) as cuota,
                        isnull(C.morosidad,0) as morosidad,
                        rtrim(isnull(C.cod_deduccion,'')) as cod_deduccion
                    from PRM_ENVIADO_DETALLE C
                    inner join reg_creditos R on C.id_solicitud = R.id_solicitud
                    inner join catalogo X on R.codigo = X.codigo
                    where C.fecpro = @Proceso
                      and C.cedula = @Cedula
                      and C.cod_institucion = @Institucion;";

                var lista = conn.Query<CrEnCobroCuotasEnvioData>(
                    sql,
                    new
                    {
                        ctxResult.ctx.Cedula,
                        ctxResult.ctx.Proceso,
                        ctxResult.ctx.Institucion
                    })
                    .Select(MapEnvio)
                    .ToList();

                return BuildEnvioResult(lista, ctxResult.ctx.filtros);
            }
            catch (SqlException ex)
            {
                return ErrorLista<CrEnCobroCuotasEnvioData>(ex.Message);
            }
        }

        /// <summary>
        /// Exporta la lista completa de cuotas enviadas.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<CrEnCobroCuotasListaResult<CrEnCobroCuotasEnvioData>> CR_EnCobroCuotas_Envio_Lista_Export(
            int CodEmpresa,
            CrEnCobroCuotasConsultaRequest request)
        {
            request.parametros = ResetPaginacion(request.parametros);
            return CR_EnCobroCuotas_Envio_Lista_Obtener(CodEmpresa, request);
        }

        /// <summary>
        /// Obtiene la lista de cuotas recibidas y sus totales asociados.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<CrEnCobroCuotasRecepcionResult> CR_EnCobroCuotas_Recepcion_Lista_Obtener(
            int CodEmpresa,
            CrEnCobroCuotasConsultaRequest request)
        {
            var ctxResult = BuildContext(request);
            if (ctxResult.errorRecepcion != null) return ctxResult.errorRecepcion;

            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                var lista = ObtenerRecepcion(conn, ctxResult.ctx);
                var result = BuildRecepcionResult(conn, ctxResult.ctx, lista);

                return DbHelper.CreateOkResponse(result);
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse(
                    ex.Message,
                    -1,
                    new CrEnCobroCuotasRecepcionResult());
            }
        }

        /// <summary>
        /// Exporta la lista completa de cuotas recibidas y sus totales asociados.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<CrEnCobroCuotasRecepcionResult> CR_EnCobroCuotas_Recepcion_Lista_Export(
            int CodEmpresa,
            CrEnCobroCuotasConsultaRequest request)
        {
            request.parametros = ResetPaginacion(request.parametros);
            return CR_EnCobroCuotas_Recepcion_Lista_Obtener(CodEmpresa, request);
        }

        /// <summary>
        /// Obtiene la lista histórica de deducciones por planilla.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<CrEnCobroCuotasListaResult<CrEnCobroCuotasHistorialData>> CR_EnCobroCuotas_Historial_Lista_Obtener(
            int CodEmpresa,
            CrEnCobroCuotasConsultaRequest request)
        {
            var ctxResult = BuildContext(request, validarInstitucion: false, validarProceso: false);
            if (ctxResult.errorHistorial != null) return ctxResult.errorHistorial;

            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                const string sql = @"exec spCRDHistoricoPlanilla @Cedula, @Periodos;";

                var lista = conn.Query<CrEnCobroCuotasHistorialData>(
                    sql,
                    new
                    {
                        ctxResult.ctx.Cedula,
                        Periodos = ctxResult.ctx.meses
                    })
                    .Select(MapHistorial)
                    .ToList();

                return BuildListaResult(lista, ctxResult.ctx.filtros, FiltrarHistorial, SortHistorial);
            }
            catch (SqlException ex)
            {
                return ErrorLista<CrEnCobroCuotasHistorialData>(ex.Message);
            }
        }

        /// <summary>
        /// Exporta la lista histórica completa de deducciones por planilla.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<CrEnCobroCuotasListaResult<CrEnCobroCuotasHistorialData>> CR_EnCobroCuotas_Historial_Lista_Export(
            int CodEmpresa,
            CrEnCobroCuotasConsultaRequest request)
        {
            request.parametros = ResetPaginacion(request.parametros);
            return CR_EnCobroCuotas_Historial_Lista_Obtener(CodEmpresa, request);
        }

        /// <summary>
        /// Obtiene la lista resumen por deductora.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<CrEnCobroCuotasListaResult<CrEnCobroCuotasResumenDeductoraData>> CR_EnCobroCuotas_ResumenDeductoras_Lista_Obtener(
            int CodEmpresa,
            CrEnCobroCuotasConsultaRequest request)
        {
            var ctxResult = BuildContext(request, validarInstitucion: false);
            if (ctxResult.errorResumenDeductora != null) return ctxResult.errorResumenDeductora;

            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                const string sql = @"exec spPrm_Resumen_Persona @Cedula, @Proceso;";

                var lista = conn.Query<CrEnCobroCuotasResumenDeductoraData>(
                    sql,
                    new
                    {
                        ctxResult.ctx.Cedula,
                        ctxResult.ctx.Proceso
                    })
                    .Select(MapResumenDeductora)
                    .ToList();

                return BuildListaResult(lista, ctxResult.ctx.filtros, FiltrarResumenDeductora, SortResumenDeductora);
            }
            catch (SqlException ex)
            {
                return ErrorLista<CrEnCobroCuotasResumenDeductoraData>(ex.Message);
            }
        }

        /// <summary>
        /// Exporta la lista completa resumen por deductora.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<CrEnCobroCuotasListaResult<CrEnCobroCuotasResumenDeductoraData>> CR_EnCobroCuotas_ResumenDeductoras_Lista_Export(
            int CodEmpresa,
            CrEnCobroCuotasConsultaRequest request)
        {
            request.parametros = ResetPaginacion(request.parametros);
            return CR_EnCobroCuotas_ResumenDeductoras_Lista_Obtener(CodEmpresa, request);
        }

        /// <summary>
        /// Obtiene la lista de bitácora de planilla por deductora y proceso.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<CrEnCobroCuotasListaResult<CrEnCobroCuotasBitacoraData>> CR_EnCobroCuotas_Bitacora_Lista_Obtener(
    int CodEmpresa,
    CrEnCobroCuotasConsultaRequest request)
        {
            var ctxResult = BuildContext(request);
            if (ctxResult.errorBitacora != null) return ctxResult.errorBitacora;

            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                const string sql = @"exec spPrm_Bitacora_Consulta @Institucion, @Proceso;";

                var lista = conn.Query<CrEnCobroCuotasBitacoraRow>(
                    sql,
                    new
                    {
                        ctxResult.ctx.Institucion,
                        ctxResult.ctx.Proceso
                    })
                    .Select(x => new CrEnCobroCuotasBitacoraData
                    {
                        id_seq = x.Id_seq,
                        gestion = (x.Gestion ?? string.Empty).Trim(),
                        gestion_desc = (x.GestionDesc ?? string.Empty).Trim(),
                        transaccion = (x.Transaccion ?? string.Empty).Trim(),
                        transaccion_desc = (x.TransaccionDesc ?? string.Empty).Trim(),
                        documento = (x.Documento ?? string.Empty).Trim(),
                        usuario = (x.Usuario ?? string.Empty).Trim(),
                        fecha = x.Fecha
                    })
                    .ToList();

                return BuildListaResult(lista, ctxResult.ctx.filtros, FiltrarBitacora, SortBitacora);
            }
            catch (SqlException ex)
            {
                return ErrorLista<CrEnCobroCuotasBitacoraData>(ex.Message);
            }
        }
        public sealed class CrEnCobroCuotasBitacoraRow
        {
            public int Id_seq { get; set; }
            public string Gestion { get; set; } = string.Empty;
            public string GestionDesc { get; set; } = string.Empty;
            public string Transaccion { get; set; } = string.Empty;
            public string TransaccionDesc { get; set; } = string.Empty;
            public string Documento { get; set; } = string.Empty;
            public string Usuario { get; set; } = string.Empty;
            public DateTime? Fecha { get; set; }
        }
        /// <summary>
        /// Exporta la lista completa de bitácora de planilla por deductora y proceso.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<CrEnCobroCuotasListaResult<CrEnCobroCuotasBitacoraData>> CR_EnCobroCuotas_Bitacora_Lista_Export(
            int CodEmpresa,
            CrEnCobroCuotasConsultaRequest request)
        {
            request.parametros = ResetPaginacion(request.parametros);
            return CR_EnCobroCuotas_Bitacora_Lista_Obtener(CodEmpresa, request);
        }

        /// <summary>
        /// Construye y valida el contexto común de consulta.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="validarInstitucion"></param>
        /// <param name="validarProceso"></param>
        /// <returns></returns>
        private static ContextResult BuildContext(
            CrEnCobroCuotasConsultaRequest request,
            bool validarInstitucion = true,
            bool validarProceso = true)
        {
            var filtrosResult = ParseFiltros(request?.parametros ?? string.Empty);
            if (filtrosResult.error != null)
            {
                return ContextResult.FromFiltroError(filtrosResult.error);
            }

            var ctx = new ConsultaContext
            {
                cedula = (request?.cedula ?? string.Empty).Trim(),
                proceso = request?.proceso ?? 0,
                codInstitucion = request?.cod_institucion ?? 0,
                meses = request?.meses > 0 ? request.meses : MesesDefault,
                filtros = filtrosResult.filtros
            };

            var validacion = ValidarContexto(ctx, validarInstitucion, validarProceso);
            return validacion ?? ContextResult.Ok(ctx);
        }

        /// <summary>
        /// Valida el contexto común de consulta.
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="validarInstitucion"></param>
        /// <param name="validarProceso"></param>
        /// <returns></returns>
        private static ContextResult? ValidarContexto(
            ConsultaContext ctx,
            bool validarInstitucion,
            bool validarProceso)
        {
            if (string.IsNullOrWhiteSpace(ctx.cedula))
            {
                return ContextResult.FromMessage(MensajeCedulaRequerida);
            }

            if (validarProceso && ctx.proceso <= 0)
            {
                return ContextResult.FromMessage(MensajeProcesoInvalido);
            }

            if (validarInstitucion && ctx.codInstitucion <= 0)
            {
                return ContextResult.FromMessage(MensajeInstitucionRequerida);
            }

            return null;
        }

        /// <summary>
        /// Deserializa filtros lazy load.
        /// </summary>
        /// <param name="parametros"></param>
        /// <returns></returns>
        private static FiltrosResult ParseFiltros(string parametros)
        {
            try
            {
                var filtros = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(parametros)
                              ?? new FiltrosLazyLoadData();

                return new FiltrosResult { filtros = filtros };
            }
            catch (JsonException jex)
            {
                return new FiltrosResult
                {
                    filtros = new FiltrosLazyLoadData(),
                    error = jex.Message
                };
            }
        }

        /// <summary>
        /// Elimina paginación para exportación.
        /// </summary>
        /// <param name="parametros"></param>
        /// <returns></returns>
        private static string ResetPaginacion(string parametros)
        {
            FiltrosLazyLoadData filtros;

            try
            {
                filtros = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(parametros)
                          ?? new FiltrosLazyLoadData();
            }
            catch (JsonException)
            {
                filtros = new FiltrosLazyLoadData();
            }

            filtros.pagina = 0;
            filtros.paginacion = 0;

            return JsonConvert.SerializeObject(filtros);
        }

        /// <summary>
        /// Obtiene texto de búsqueda desde FiltrosLazyLoadData.
        /// </summary>
        /// <param name="filtros"></param>
        /// <returns></returns>
        private static string GetTexto(FiltrosLazyLoadData filtros)
        {
            if (string.IsNullOrWhiteSpace(filtros.filtro))
            {
                return string.Empty;
            }

            try
            {
                var dto = JsonConvert.DeserializeObject<CrEnCobroCuotasFiltroDto>(filtros.filtro);
                return (dto?.texto ?? filtros.filtro ?? string.Empty).Trim();
            }
            catch (JsonException)
            {
                return (filtros.filtro ?? string.Empty).Trim();
            }
        }

        /// <summary>
        /// Construye resultado paginado, filtrado y ordenado.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="lista"></param>
        /// <param name="filtros"></param>
        /// <param name="filtrar"></param>
        /// <param name="ordenar"></param>
        /// <returns></returns>
        private static ErrorDto<CrEnCobroCuotasListaResult<T>> BuildListaResult<T>(
            List<T> lista,
            FiltrosLazyLoadData filtros,
            Func<List<T>, string, List<T>> filtrar,
            Func<List<T>, FiltrosLazyLoadData, List<T>> ordenar)
        {
            var texto = GetTexto(filtros);

            var data = filtrar(lista, texto);
            data = ordenar(data, filtros);

            var total = data.Count;
            data = AplicarPaginacion(data, filtros);

            return DbHelper.CreateOkResponse(new CrEnCobroCuotasListaResult<T>
            {
                total = total,
                lista = data
            });
        }

        /// <summary>
        /// Aplica paginación local.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="lista"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        private static List<T> AplicarPaginacion<T>(List<T> lista, FiltrosLazyLoadData filtros)
        {
            int pagina = filtros.pagina < 0 ? 0 : filtros.pagina;
            int fetch = filtros.paginacion < 0 ? 0 : filtros.paginacion;

            if (fetch <= 0) return lista;

            int offset = pagina * fetch;
            return lista.Skip(offset).Take(fetch).ToList();
        }

        /// <summary>
        /// Retorna respuesta de error estándar para listas.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="mensaje"></param>
        /// <returns></returns>
        private static ErrorDto<CrEnCobroCuotasListaResult<T>> ErrorLista<T>(string mensaje)
        {
            return DbHelper.CreateErrorResponse(
                mensaje,
                -1,
                new CrEnCobroCuotasListaResult<T>());
        }

        private static CrEnCobroCuotasDetalleData MapDetalle(CrEnCobroCuotasDetalleData item)
        {
            item.linea = (item.linea ?? string.Empty).Trim();
            item.descripcion = (item.descripcion ?? string.Empty).Trim();
            item.destino = (item.destino ?? string.Empty).Trim();
            item.caso = (item.caso ?? string.Empty).Trim();
            item.tipo = (item.tipo ?? string.Empty).Trim();
            item.total_abono = item.cargos + item.int_cor + item.int_mor + item.principal;
            item.diferencia = item.total_abono - item.enviado;
            item.proceso_format = MCobroDb.fxFechaProcesoFormat(item.proceso);
            return item;
        }

        private static CrEnCobroCuotasEnvioData MapEnvio(CrEnCobroCuotasEnvioData item)
        {
            item.codigo = (item.codigo ?? string.Empty).Trim();
            item.descripcion = (item.descripcion ?? string.Empty).Trim();
            item.cod_deduccion = (item.cod_deduccion ?? string.Empty).Trim();
            item.tipo_cuota = item.morosidad == 0 ? "Ordinario" : "Morosidad";
            return item;
        }

        private static CrEnCobroCuotasRecepcionData MapRecepcion(CrEnCobroCuotasRecepcionData item)
        {
            item.codigo = (item.codigo ?? string.Empty).Trim();
            item.descripcion = (item.descripcion ?? string.Empty).Trim();
            item.tipo = (item.tipo ?? string.Empty).Trim();
            item.tipo_cuota = item.tipo == "C" ? "Ordinario" : "Morosidad";
            return item;
        }

        private static CrEnCobroCuotasHistorialData MapHistorial(CrEnCobroCuotasHistorialData item)
        {
            item.institucion = (item.institucion ?? string.Empty).Trim();
            item.diferencia = item.recibido - item.enviado;
            item.proceso_format = MCobroDb.fxFechaProcesoFormat(item.proceso);
            return item;
        }

        private static CrEnCobroCuotasResumenDeductoraData MapResumenDeductora(CrEnCobroCuotasResumenDeductoraData item)
        {
            item.desc_corta = (item.desc_corta ?? string.Empty).Trim();
            item.descripcion = (item.descripcion ?? string.Empty).Trim();
            item.diferencia = item.recibido - item.enviado;
            return item;
        }
        private static List<CrEnCobroCuotasRecepcionData> ObtenerRecepcion(SqlConnection conn, ConsultaContext ctx)
        {
            const string sql = @"exec spPrm_Planilla_Recepcion_Aplicada @Institucion, @Proceso, @Cedula;";

            return conn.Query<CrEnCobroCuotasRecepcionData>(
                sql,
                new
                {
                    ctx.Institucion,
                    ctx.Proceso,
                    ctx.Cedula
                })
                .Select(MapRecepcion)
                .ToList();
        }

        private static CrEnCobroCuotasRecepcionResult BuildRecepcionResult(SqlConnection conn,ConsultaContext ctx,List<CrEnCobroCuotasRecepcionData> listaOriginal)
        {
            var lista = FiltrarRecepcion(listaOriginal, GetTexto(ctx.filtros));
            lista = SortRecepcion(lista, ctx.filtros);

            var total = lista.Count;
            var pagina = AplicarPaginacion(lista, ctx.filtros);

            var totalPlanilla = lista.Sum(x => x.abono);
            var totalNc = ObtenerMontoRecepcionOtros(conn, ctx, "I");
            var totalRecaudado = ObtenerMontoRecepcionOtros(conn, ctx, "C");

            return new CrEnCobroCuotasRecepcionResult
            {
                total = total,
                lista = pagina,
                total_planilla = totalPlanilla,
                total_nc = totalNc,
                total_general = totalPlanilla + totalNc,
                total_recaudado = totalRecaudado
            };
        }

        private static decimal ObtenerMontoRecepcionOtros(SqlConnection conn, ConsultaContext ctx, string origen)
        {
            const string sql = @"exec spPrm_Planilla_Recepcion_Aplicada_Otros @Institucion, @Proceso, @Cedula, @Origen;";

            return conn.QueryFirstOrDefault<decimal>(
                sql,
                new
                {
                    ctx.Institucion,
                    ctx.Proceso,
                    ctx.Cedula,
                    Origen = origen
                });
        }

        private static bool NumeroContiene(decimal valor, string texto)
        {
            return valor.ToString("0.00").Contains(texto, StringComparison.OrdinalIgnoreCase)
                || valor.ToString("N2").Contains(texto, StringComparison.OrdinalIgnoreCase);
        }

        private static bool NumeroContiene(int valor, string texto)
        {
            return valor.ToString().Contains(texto, StringComparison.OrdinalIgnoreCase);
        }

        private static List<CrEnCobroCuotasResumenData> FiltrarResumen(List<CrEnCobroCuotasResumenData> lista, string texto)
        {
            if (string.IsNullOrWhiteSpace(texto)) return lista;

            return lista.Where(x =>
                   NumeroContiene(x.operacion, texto)
                || (x.linea ?? string.Empty).Contains(texto, StringComparison.OrdinalIgnoreCase)
                || NumeroContiene(x.envio, texto)
                || NumeroContiene(x.recibido, texto)
                || NumeroContiene(x.diferencia, texto)
                || (x.tipo_desc ?? string.Empty).Contains(texto, StringComparison.OrdinalIgnoreCase)
                || (x.linea_desc ?? string.Empty).Contains(texto, StringComparison.OrdinalIgnoreCase)
            ).ToList();
        }

        private static List<CrEnCobroCuotasDetalleData> FiltrarDetalle(List<CrEnCobroCuotasDetalleData> lista, string texto)
        {
            if (string.IsNullOrWhiteSpace(texto)) return lista;

            return lista.Where(x =>
                   NumeroContiene(x.operacion, texto)
                || (x.linea ?? string.Empty).Contains(texto, StringComparison.OrdinalIgnoreCase)
                || NumeroContiene(x.proceso, texto)
                || (x.proceso_format ?? string.Empty).Contains(texto, StringComparison.OrdinalIgnoreCase)
                || NumeroContiene(x.int_cor, texto)
                || NumeroContiene(x.int_mor, texto)
                || NumeroContiene(x.cargos, texto)
                || NumeroContiene(x.principal, texto)
                || NumeroContiene(x.total_abono, texto)
                || NumeroContiene(x.enviado, texto)
                || NumeroContiene(x.diferencia, texto)
                || (x.fecha?.ToString("dd/MM/yyyy") ?? string.Empty).Contains(texto, StringComparison.OrdinalIgnoreCase)
                || (x.descripcion ?? string.Empty).Contains(texto, StringComparison.OrdinalIgnoreCase)
                || (x.destino ?? string.Empty).Contains(texto, StringComparison.OrdinalIgnoreCase)
                || (x.caso ?? string.Empty).Contains(texto, StringComparison.OrdinalIgnoreCase)
                || (x.tipo ?? string.Empty).Contains(texto, StringComparison.OrdinalIgnoreCase)
            ).ToList();
        }

        private static List<CrEnCobroCuotasEnvioData> FiltrarEnvio(List<CrEnCobroCuotasEnvioData> lista, string texto)
        {
            if (string.IsNullOrWhiteSpace(texto)) return lista;

            return lista.Where(x =>
                   NumeroContiene(x.id_solicitud, texto)
                || (x.codigo ?? string.Empty).Contains(texto, StringComparison.OrdinalIgnoreCase)
                || (x.descripcion ?? string.Empty).Contains(texto, StringComparison.OrdinalIgnoreCase)
                || NumeroContiene(x.cuota, texto)
                || NumeroContiene(x.morosidad, texto)
                || (x.tipo_cuota ?? string.Empty).Contains(texto, StringComparison.OrdinalIgnoreCase)
                || (x.cod_deduccion ?? string.Empty).Contains(texto, StringComparison.OrdinalIgnoreCase)
            ).ToList();
        }

        private static List<CrEnCobroCuotasRecepcionData> FiltrarRecepcion(List<CrEnCobroCuotasRecepcionData> lista, string texto)
        {
            if (string.IsNullOrWhiteSpace(texto)) return lista;

            return lista.Where(x =>
                   NumeroContiene(x.id_solicitud, texto)
                || (x.codigo ?? string.Empty).Contains(texto, StringComparison.OrdinalIgnoreCase)
                || (x.descripcion ?? string.Empty).Contains(texto, StringComparison.OrdinalIgnoreCase)
                || NumeroContiene(x.abono, texto)
                || (x.tipo ?? string.Empty).Contains(texto, StringComparison.OrdinalIgnoreCase)
                || (x.tipo_cuota ?? string.Empty).Contains(texto, StringComparison.OrdinalIgnoreCase)
            ).ToList();
        }

        private static List<CrEnCobroCuotasHistorialData> FiltrarHistorial(List<CrEnCobroCuotasHistorialData> lista, string texto)
        {
            if (string.IsNullOrWhiteSpace(texto)) return lista;

            return lista.Where(x =>
                   NumeroContiene(x.proceso, texto)
                || (x.proceso_format ?? string.Empty).Contains(texto, StringComparison.OrdinalIgnoreCase)
                || NumeroContiene(x.enviado, texto)
                || NumeroContiene(x.recibido, texto)
                || NumeroContiene(x.diferencia, texto)
                || (x.institucion ?? string.Empty).Contains(texto, StringComparison.OrdinalIgnoreCase)
            ).ToList();
        }

        private static List<CrEnCobroCuotasResumenDeductoraData> FiltrarResumenDeductora(List<CrEnCobroCuotasResumenDeductoraData> lista, string texto)
        {
            if (string.IsNullOrWhiteSpace(texto)) return lista;

            return lista.Where(x =>
                   NumeroContiene(x.cod_institucion, texto)
                || (x.desc_corta ?? string.Empty).Contains(texto, StringComparison.OrdinalIgnoreCase)
                || (x.descripcion ?? string.Empty).Contains(texto, StringComparison.OrdinalIgnoreCase)
                || NumeroContiene(x.enviado, texto)
                || NumeroContiene(x.recibido, texto)
                || NumeroContiene(x.diferencia, texto)
            ).ToList();
        }

        private static List<CrEnCobroCuotasBitacoraData> FiltrarBitacora(List<CrEnCobroCuotasBitacoraData> lista, string texto)
        {
            if (string.IsNullOrWhiteSpace(texto)) return lista;

            return lista.Where(x =>
                   NumeroContiene(x.id_seq, texto)
                || (x.gestion ?? string.Empty).Contains(texto, StringComparison.OrdinalIgnoreCase)
                || (x.gestion_desc ?? string.Empty).Contains(texto, StringComparison.OrdinalIgnoreCase)
                || (x.transaccion ?? string.Empty).Contains(texto, StringComparison.OrdinalIgnoreCase)
                || (x.transaccion_desc ?? string.Empty).Contains(texto, StringComparison.OrdinalIgnoreCase)
                || (x.documento ?? string.Empty).Contains(texto, StringComparison.OrdinalIgnoreCase)
                || (x.usuario ?? string.Empty).Contains(texto, StringComparison.OrdinalIgnoreCase)
                || (x.fecha?.ToString("dd/MM/yyyy") ?? string.Empty).Contains(texto, StringComparison.OrdinalIgnoreCase)
            ).ToList();
        }

        private static List<CrEnCobroCuotasResumenData> SortResumen(List<CrEnCobroCuotasResumenData> lista, FiltrosLazyLoadData filtros)
        {
            var sf = (filtros.sortField ?? string.Empty).Trim().ToLowerInvariant();
            var asc = filtros.sortOrder == 1;

            return sf switch
            {
                "operacion" => asc ? lista.OrderBy(x => x.operacion).ToList() : lista.OrderByDescending(x => x.operacion).ToList(),
                "linea" => asc ? lista.OrderBy(x => x.linea).ToList() : lista.OrderByDescending(x => x.linea).ToList(),
                "envio" => asc ? lista.OrderBy(x => x.envio).ToList() : lista.OrderByDescending(x => x.envio).ToList(),
                "recibido" => asc ? lista.OrderBy(x => x.recibido).ToList() : lista.OrderByDescending(x => x.recibido).ToList(),
                DIFERENCIA => asc ? lista.OrderBy(x => x.diferencia).ToList() : lista.OrderByDescending(x => x.diferencia).ToList(),
                "tipo_desc" => asc ? lista.OrderBy(x => x.tipo_desc).ToList() : lista.OrderByDescending(x => x.tipo_desc).ToList(),
                "linea_desc" => asc ? lista.OrderBy(x => x.linea_desc).ToList() : lista.OrderByDescending(x => x.linea_desc).ToList(),
                _ => lista.OrderBy(x => x.operacion).ToList()
            };
        }

        private static List<CrEnCobroCuotasDetalleData> SortDetalle(List<CrEnCobroCuotasDetalleData> lista, FiltrosLazyLoadData filtros)
        {
            var sf = (filtros.sortField ?? string.Empty).Trim().ToLowerInvariant();
            var asc = filtros.sortOrder == 1;

            return sf switch
            {
                "operacion" => asc ? lista.OrderBy(x => x.operacion).ToList() : lista.OrderByDescending(x => x.operacion).ToList(),
                "linea" => asc ? lista.OrderBy(x => x.linea).ToList() : lista.OrderByDescending(x => x.linea).ToList(),
                "proceso" => asc ? lista.OrderBy(x => x.proceso).ToList() : lista.OrderByDescending(x => x.proceso).ToList(),
                "total_abono" => asc ? lista.OrderBy(x => x.total_abono).ToList() : lista.OrderByDescending(x => x.total_abono).ToList(),
                "enviado" => asc ? lista.OrderBy(x => x.enviado).ToList() : lista.OrderByDescending(x => x.enviado).ToList(),
                DIFERENCIA => asc ? lista.OrderBy(x => x.diferencia).ToList() : lista.OrderByDescending(x => x.diferencia).ToList(),
                "fecha" => asc ? lista.OrderBy(x => x.fecha).ToList() : lista.OrderByDescending(x => x.fecha).ToList(),
                _ => lista.OrderBy(x => x.operacion).ToList()
            };
        }

        private static List<CrEnCobroCuotasEnvioData> SortEnvio(List<CrEnCobroCuotasEnvioData> lista, FiltrosLazyLoadData filtros)
        {
            var sf = (filtros.sortField ?? string.Empty).Trim().ToLowerInvariant();
            var asc = filtros.sortOrder == 1;

            return sf switch
            {
                "id_solicitud" => asc ? lista.OrderBy(x => x.id_solicitud).ToList() : lista.OrderByDescending(x => x.id_solicitud).ToList(),
                "codigo" => asc ? lista.OrderBy(x => x.codigo).ToList() : lista.OrderByDescending(x => x.codigo).ToList(),
                "descripcion" => asc ? lista.OrderBy(x => x.descripcion).ToList() : lista.OrderByDescending(x => x.descripcion).ToList(),
                "cuota" => asc ? lista.OrderBy(x => x.cuota).ToList() : lista.OrderByDescending(x => x.cuota).ToList(),
                "tipo_cuota" => asc ? lista.OrderBy(x => x.tipo_cuota).ToList() : lista.OrderByDescending(x => x.tipo_cuota).ToList(),
                "cod_deduccion" => asc ? lista.OrderBy(x => x.cod_deduccion).ToList() : lista.OrderByDescending(x => x.cod_deduccion).ToList(),
                _ => lista.OrderBy(x => x.id_solicitud).ToList()
            };
        }

        private static List<CrEnCobroCuotasRecepcionData> SortRecepcion(List<CrEnCobroCuotasRecepcionData> lista, FiltrosLazyLoadData filtros)
        {
            var sf = (filtros.sortField ?? string.Empty).Trim().ToLowerInvariant();
            var asc = filtros.sortOrder == 1;

            return sf switch
            {
                "id_solicitud" => asc ? lista.OrderBy(x => x.id_solicitud).ToList() : lista.OrderByDescending(x => x.id_solicitud).ToList(),
                "codigo" => asc ? lista.OrderBy(x => x.codigo).ToList() : lista.OrderByDescending(x => x.codigo).ToList(),
                "descripcion" => asc ? lista.OrderBy(x => x.descripcion).ToList() : lista.OrderByDescending(x => x.descripcion).ToList(),
                "abono" => asc ? lista.OrderBy(x => x.abono).ToList() : lista.OrderByDescending(x => x.abono).ToList(),
                "tipo_cuota" => asc ? lista.OrderBy(x => x.tipo_cuota).ToList() : lista.OrderByDescending(x => x.tipo_cuota).ToList(),
                _ => lista.OrderBy(x => x.id_solicitud).ToList()
            };
        }

        private static List<CrEnCobroCuotasHistorialData> SortHistorial(List<CrEnCobroCuotasHistorialData> lista, FiltrosLazyLoadData filtros)
        {
            var sf = (filtros.sortField ?? string.Empty).Trim().ToLowerInvariant();
            var asc = filtros.sortOrder == 1;

            return sf switch
            {
                "proceso" => asc ? lista.OrderBy(x => x.proceso).ToList() : lista.OrderByDescending(x => x.proceso).ToList(),
                "enviado" => asc ? lista.OrderBy(x => x.enviado).ToList() : lista.OrderByDescending(x => x.enviado).ToList(),
                "recibido" => asc ? lista.OrderBy(x => x.recibido).ToList() : lista.OrderByDescending(x => x.recibido).ToList(),
                DIFERENCIA => asc ? lista.OrderBy(x => x.diferencia).ToList() : lista.OrderByDescending(x => x.diferencia).ToList(),
                "institucion" => asc ? lista.OrderBy(x => x.institucion).ToList() : lista.OrderByDescending(x => x.institucion).ToList(),
                _ => lista.OrderByDescending(x => x.proceso).ToList()
            };
        }

        private static List<CrEnCobroCuotasResumenDeductoraData> SortResumenDeductora(List<CrEnCobroCuotasResumenDeductoraData> lista, FiltrosLazyLoadData filtros)
        {
            var sf = (filtros.sortField ?? string.Empty).Trim().ToLowerInvariant();
            var asc = filtros.sortOrder == 1;

            return sf switch
            {
                "cod_institucion" => asc ? lista.OrderBy(x => x.cod_institucion).ToList() : lista.OrderByDescending(x => x.cod_institucion).ToList(),
                "desc_corta" => asc ? lista.OrderBy(x => x.desc_corta).ToList() : lista.OrderByDescending(x => x.desc_corta).ToList(),
                "descripcion" => asc ? lista.OrderBy(x => x.descripcion).ToList() : lista.OrderByDescending(x => x.descripcion).ToList(),
                "enviado" => asc ? lista.OrderBy(x => x.enviado).ToList() : lista.OrderByDescending(x => x.enviado).ToList(),
                "recibido" => asc ? lista.OrderBy(x => x.recibido).ToList() : lista.OrderByDescending(x => x.recibido).ToList(),
                DIFERENCIA => asc ? lista.OrderBy(x => x.diferencia).ToList() : lista.OrderByDescending(x => x.diferencia).ToList(),
                _ => lista.OrderBy(x => x.desc_corta).ToList()
            };
        }

        private static List<CrEnCobroCuotasBitacoraData> SortBitacora(List<CrEnCobroCuotasBitacoraData> lista, FiltrosLazyLoadData filtros)
        {
            var sf = (filtros.sortField ?? string.Empty).Trim().ToLowerInvariant();
            var asc = filtros.sortOrder == 1;

            return sf switch
            {
                "id_seq" => asc ? lista.OrderBy(x => x.id_seq).ToList() : lista.OrderByDescending(x => x.id_seq).ToList(),
                "gestion_desc" => asc ? lista.OrderBy(x => x.gestion_desc).ToList() : lista.OrderByDescending(x => x.gestion_desc).ToList(),
                "transaccion_desc" => asc ? lista.OrderBy(x => x.transaccion_desc).ToList() : lista.OrderByDescending(x => x.transaccion_desc).ToList(),
                "documento" => asc ? lista.OrderBy(x => x.documento).ToList() : lista.OrderByDescending(x => x.documento).ToList(),
                "usuario" => asc ? lista.OrderBy(x => x.usuario).ToList() : lista.OrderByDescending(x => x.usuario).ToList(),
                "fecha" => asc ? lista.OrderBy(x => x.fecha).ToList() : lista.OrderByDescending(x => x.fecha).ToList(),
                _ => lista.OrderBy(x => x.id_seq).ToList()
            };
        }

        private sealed class ConsultaContext
        {
            public string cedula { get; set; } = string.Empty;
            public decimal proceso { get; set; }
            public int codInstitucion { get; set; }
            public int meses { get; set; }
            public FiltrosLazyLoadData filtros { get; set; } = new();

            public string Cedula => cedula;
            public decimal Proceso => proceso;
            public int Institucion => codInstitucion;
        }

        private sealed class FiltrosResult
        {
            public FiltrosLazyLoadData filtros { get; set; } = new();
            public string? error { get; set; }
        }
        private static ErrorDto<CrEnCobroCuotasListaResult<CrEnCobroCuotasResumenData>> BuildResumenResult(List<CrEnCobroCuotasResumenData> lista,FiltrosLazyLoadData filtros)
        {
            var data = SortResumen(FiltrarResumen(lista, GetTexto(filtros)), filtros);
            var totalEnviado = data.Sum(x => x.envio);
            var totalRecibido = data.Sum(x => x.recibido);

            return DbHelper.CreateOkResponse(new CrEnCobroCuotasListaResult<CrEnCobroCuotasResumenData>
            {
                total = data.Count,
                lista = AplicarPaginacion(data, filtros),
                total_enviado = totalEnviado,
                total_recibido = totalRecibido,
                total_diferencia = totalEnviado - totalRecibido
            });
        }

        private static ErrorDto<CrEnCobroCuotasListaResult<CrEnCobroCuotasDetalleData>> BuildDetalleResult(List<CrEnCobroCuotasDetalleData> lista,FiltrosLazyLoadData filtros)
        {
            var data = SortDetalle(FiltrarDetalle(lista, GetTexto(filtros)), filtros);
            var totalAbono = data.Sum(x => x.total_abono);
            var totalEnviado = data.Sum(x => x.enviado);

            return DbHelper.CreateOkResponse(new CrEnCobroCuotasListaResult<CrEnCobroCuotasDetalleData>
            {
                total = data.Count,
                lista = AplicarPaginacion(data, filtros),
                total_abono = totalAbono,
                total_enviado = totalEnviado,
                total_diferencia = totalAbono - totalEnviado
            });
        }

        private static ErrorDto<CrEnCobroCuotasListaResult<CrEnCobroCuotasEnvioData>> BuildEnvioResult(List<CrEnCobroCuotasEnvioData> lista,FiltrosLazyLoadData filtros)
        {
            var data = SortEnvio(FiltrarEnvio(lista, GetTexto(filtros)), filtros);

            return DbHelper.CreateOkResponse(new CrEnCobroCuotasListaResult<CrEnCobroCuotasEnvioData>
            {
                total = data.Count,
                lista = AplicarPaginacion(data, filtros),
                total_enviado = data.Sum(x => x.cuota)
            });
        }
        private sealed class ContextResult
        {
            public ConsultaContext ctx { get; set; } = new();

            public ErrorDto<CrEnCobroCuotasListaResult<CrEnCobroCuotasResumenData>>? errorResumen { get; set; }
            public ErrorDto<CrEnCobroCuotasListaResult<CrEnCobroCuotasDetalleData>>? errorDetalle { get; set; }
            public ErrorDto<CrEnCobroCuotasListaResult<CrEnCobroCuotasEnvioData>>? errorEnvio { get; set; }
            public ErrorDto<CrEnCobroCuotasRecepcionResult>? errorRecepcion { get; set; }
            public ErrorDto<CrEnCobroCuotasListaResult<CrEnCobroCuotasHistorialData>>? errorHistorial { get; set; }
            public ErrorDto<CrEnCobroCuotasListaResult<CrEnCobroCuotasResumenDeductoraData>>? errorResumenDeductora { get; set; }
            public ErrorDto<CrEnCobroCuotasListaResult<CrEnCobroCuotasBitacoraData>>? errorBitacora { get; set; }
            public static ContextResult Ok(ConsultaContext ctx)
            {
                return new ContextResult { ctx = ctx };
            }

            public static ContextResult FromFiltroError(string mensaje)
            {
                return FromMessage(mensaje);
            }

            public static ContextResult FromMessage(string mensaje)
            {
                return new ContextResult
                {
                    errorResumen = ErrorLista<CrEnCobroCuotasResumenData>(mensaje),
                    errorDetalle = ErrorLista<CrEnCobroCuotasDetalleData>(mensaje),
                    errorEnvio = ErrorLista<CrEnCobroCuotasEnvioData>(mensaje),
                    errorRecepcion = DbHelper.CreateErrorResponse(mensaje, -2, new CrEnCobroCuotasRecepcionResult()),
                    errorHistorial = ErrorLista<CrEnCobroCuotasHistorialData>(mensaje),
                    errorResumenDeductora = ErrorLista<CrEnCobroCuotasResumenDeductoraData>(mensaje),
                    errorBitacora = ErrorLista<CrEnCobroCuotasBitacoraData>(mensaje)
                };
            }
        }
    }
}