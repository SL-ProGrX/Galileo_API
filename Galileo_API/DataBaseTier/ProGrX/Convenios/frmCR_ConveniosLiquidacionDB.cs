using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.Security;
using Galileo_API.Models.ProGrX.Convenios;
using Microsoft.Data.SqlClient;

namespace Galileo_API.DataBaseTier.ProGrX.Convenios
{
    public class FrmCrConveniosLiquidacionDb
    {
        private readonly PortalDB _portalDb;
        private readonly MSecurityMainDb _bitacora;
        private readonly FrmCrConveniosConsultasDb _consultasDb;
        private const int VModulo = 16; // Módulo de Convenios (vModulo del formulario VB6)
        private const string EstadoCerrada = "C";
        private const string EstadoAbierta = "A";

        private const string SqlConvenio = @"
            select
                rtrim(COD_CONVENIO) as cod_convenio,
                rtrim(isnull(DESCRIPCION, '')) as descripcion,
                rtrim(isnull(CONTRATO_NUMERO, '')) as contrato_numero,
                isnull(FECHA_VENCIMIENTO, dateadd(YEAR, 1, dbo.MyGetdate())) as fecha_vence,
                FECHA_INICIO as fecha_inicio,
                PORC_COMISION_CREDITOS as porc_comision_creditos,
                PORC_COMISION_RECAUDACION as porc_comision_recaudacion,
                isnull(COMISION_INFORMATIVA, 0) as comision_informativa,
                isnull(COBRA_CREDITOS_ANULADOS, 0) as cobra_creditos_anulados,
                dbo.fxConvenio_CxP_CargosFlotantes(COD_CONVENIO) as flotante,
                convert(date, dateadd(d, -1, dbo.MyGetdate())) as fecha_corte_general
            from CRD_CONVENIOS";

        public FrmCrConveniosLiquidacionDb(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
            _bitacora = new MSecurityMainDb(config);
            _consultasDb = new FrmCrConveniosConsultasDb(_portalDb);
        }

        /// <summary>
        /// Lista de convenios para la búsqueda F4 (frmBusquedas).
        /// </summary>
        public ErrorDto<List<CrConvenioConsultaData>> CrConveniosLiquidacion_Convenios_Obtener(int codEmpresa)
            => _consultasDb.CrConveniosConsultas_Convenios_Obtener(codEmpresa);

        /// <summary>
        /// Convenio anterior o siguiente al indicado (FlatScrollBar_Change).
        /// </summary>
        public ErrorDto<CrConvenioConsultaData?> CrConveniosLiquidacion_Convenio_Navegar(
            int codEmpresa,
            string? codConvenio,
            bool siguiente)
        {
            const string sqlQuery = @"
                select top 1
                    rtrim(COD_CONVENIO) as cod_convenio,
                    rtrim(isnull(DESCRIPCION, '')) as descripcion
                from CRD_CONVENIOS
                where @CodConvenio = ''
                   or (@Siguiente = 1 and COD_CONVENIO > @CodConvenio)
                   or (@Siguiente = 0 and COD_CONVENIO < @CodConvenio)
                order by
                    case when @Siguiente = 1 then COD_CONVENIO end asc,
                    case when @Siguiente = 0 then COD_CONVENIO end desc;";

            return DbHelper.ExecuteSingleQuery<CrConvenioConsultaData>(
                _portalDb,
                codEmpresa,
                sqlQuery,
                null,
                new
                {
                    CodConvenio = (codConvenio ?? string.Empty).Trim(),
                    Siguiente = siguiente ? 1 : 0
                });
        }

        /// <summary>
        /// Consulta el convenio por código o por descripción exacta (sbConsulta).
        /// </summary>
        public ErrorDto<CrLiqConvenioData?> CrConveniosLiquidacion_Convenio_Obtener(
            int codEmpresa,
            string? codConvenio,
            string? descripcion)
        {
            const string sqlQuery = SqlConvenio + @"
                where (@CodConvenio <> '' and COD_CONVENIO = @CodConvenio)
                   or (@CodConvenio = '' and DESCRIPCION = @Descripcion);";

            return DbHelper.ExecuteSingleQuery<CrLiqConvenioData>(
                _portalDb,
                codEmpresa,
                sqlQuery,
                null,
                new
                {
                    CodConvenio = (codConvenio ?? string.Empty).Trim(),
                    Descripcion = (descripcion ?? string.Empty).Trim()
                });
        }

        /// <summary>
        /// Últimas N órdenes del convenio (sbConsultaOrdenes).
        /// </summary>
        public ErrorDto<List<CrLiqOrdenListaData>> CrConveniosLiquidacion_Ordenes_Obtener(
            int codEmpresa,
            string codConvenio,
            int lineas)
        {
            const string sqlQuery = @"
                select top (@Lineas)
                    COD_ORDEN as cod_orden,
                    rtrim(COD_CONVENIO) as cod_convenio,
                    ESTADO as estado,
                    case ESTADO when 'A' then 'Abierta' when 'C' then 'Cerrada' else '' end as estado_desc,
                    rtrim(isnull(NOTAS, '')) as notas,
                    FECHA_INICIO as fecha_inicio,
                    FECHA_CORTE as fecha_corte,
                    isnull(RECAUDACION_CUOTAS, 0) as recaudacion_cuotas,
                    isnull(RECAUDACION_CARGOS, 0) as recaudacion_cargos,
                    isnull(NUEVOS_CREDITOS, 0) as nuevos_creditos,
                    isnull(RETENCION_AHORROS, 0) as retencion_ahorros,
                    isnull(COMISIONES_RECAUDACION, 0) as comisiones_recaudacion,
                    isnull(COMISIONES_NUEVOS_CREDITOS, 0) as comisiones_nuevos_creditos,
                    isnull(TOTAL_RESERVA, 0) as total_reserva,
                    isnull(TOTAL_REBAJOS_CXP, 0) as total_rebajos_cxp,
                    isnull(TOTAL_PAGAR, 0) as total_pagar,
                    rtrim(isnull(DOCUMENTO, '')) as documento,
                    REGISTRO_USUARIO as registro_usuario,
                    REGISTRO_FECHA as registro_fecha
                from CRD_CONVENIOS_ORDENES
                where COD_CONVENIO = @CodConvenio
                order by COD_ORDEN desc;";

            return DbHelper.ExecuteListQuery<CrLiqOrdenListaData>(
                _portalDb,
                codEmpresa,
                sqlQuery,
                new { CodConvenio = codConvenio.Trim(), Lineas = lineas > 0 ? lineas : 50 });
        }

        /// <summary>
        /// Carga una orden; si está abierta toma el flotante y la comisión informativa vigentes del convenio (sbConsultaOrden).
        /// </summary>
        public ErrorDto<CrLiqOrdenData?> CrConveniosLiquidacion_Orden_Obtener(
            int codEmpresa,
            string codConvenio,
            int codOrden)
        {
            try
            {
                using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);
                return DbHelper.CreateOkResponse<CrLiqOrdenData?>(ConsultarOrden(conn, codConvenio.Trim(), codOrden));
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<CrLiqOrdenData?>(ex.Message);
            }
        }

        /// <summary>
        /// Valida que la última orden esté cerrada y propone las fechas de la nueva orden (fxValidaUltimaOrdenCerrada).
        /// </summary>
        public ErrorDto<CrLiqNuevaOrdenData> CrConveniosLiquidacion_Orden_Nueva(int codEmpresa, string codConvenio)
        {
            try
            {
                using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);

                var convenio = ConsultarConvenio(conn, codConvenio.Trim());
                if (convenio == null)
                    return DbHelper.CreateErrorResponse<CrLiqNuevaOrdenData>("No se ha indicado ningun convenio?", -2);

                const string sqlUltima = @"
                    select top 1
                        COD_ORDEN as cod_orden,
                        ESTADO as estado,
                        dateadd(d, 1, FECHA_CORTE) as inicio,
                        dateadd(d, -1, dbo.MyGetdate()) as corte
                    from CRD_CONVENIOS_ORDENES
                    where COD_CONVENIO = @codConvenio
                    order by FECHA_CORTE desc;";

                var ultima = conn.QueryFirstOrDefault<UltimaOrden>(sqlUltima, new { codConvenio = codConvenio.Trim() });

                return DbHelper.CreateOkResponse(ArmarNuevaOrden(convenio, ultima));
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<CrLiqNuevaOrdenData>(ex.Message);
            }
        }

        /// <summary>
        /// Guarda (inserta o actualiza) la orden y recalcula su detalle (sbGuardaOrden).
        /// </summary>
        public ErrorDto<CrLiqOrdenData?> CrConveniosLiquidacion_Orden_Guardar(
            int codEmpresa,
            CrLiqOrdenGuardarRequest request)
        {
            try
            {
                using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);

                var error = ValidarOrden(conn, request);
                if (error.Length > 0)
                    return DbHelper.CreateErrorResponse<CrLiqOrdenData?>(error, -2);

                var orden = GuardarOrden(conn, codEmpresa, request);
                var mensaje = (request.cod_orden ?? 0) > 0
                    ? "Información actualizada satisfactoriamente!!!"
                    : "Información Guardada satisfactoriamente!!!";

                return DbHelper.CreateOkResponse<CrLiqOrdenData?>(
                    ConsultarOrden(conn, request.cod_convenio.Trim(), orden), mensaje);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<CrLiqOrdenData?>(ex.Message);
            }
        }

        /// <summary>
        /// Recalcula el detalle de una orden abierta (spConvenio_Orden_Detalle).
        /// </summary>
        public ErrorDto<CrLiqOrdenData?> CrConveniosLiquidacion_Orden_Calcular(
            int codEmpresa,
            string codConvenio,
            int codOrden)
        {
            try
            {
                using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);

                var estado = ConsultarEstado(conn, codConvenio.Trim(), codOrden);
                if (codOrden <= 0 || estado == null || estado == EstadoCerrada)
                    return DbHelper.CreateErrorResponse<CrLiqOrdenData?>(
                        "La orden no existe o ya fue cerrada...verifique!", -2);

                RecalcularDetalle(conn, codConvenio.Trim(), codOrden);

                return DbHelper.CreateOkResponse<CrLiqOrdenData?>(ConsultarOrden(conn, codConvenio.Trim(), codOrden));
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<CrLiqOrdenData?>(ex.Message);
            }
        }

        /// <summary>
        /// Guarda la orden y la cierra enviándola a CxP (spConvenio_Orden_Cierra).
        /// </summary>
        public ErrorDto<CrLiqOrdenData?> CrConveniosLiquidacion_Orden_Cerrar(
            int codEmpresa,
            CrLiqOrdenGuardarRequest request)
        {
            try
            {
                using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);
                var codConvenio = request.cod_convenio.Trim();
                var codOrden = request.cod_orden ?? 0;

                var error = ValidarOrden(conn, request);
                if (error.Length > 0)
                    return DbHelper.CreateErrorResponse<CrLiqOrdenData?>(error, -2);

                if (codOrden <= 0)
                    return DbHelper.CreateErrorResponse<CrLiqOrdenData?>("La Orden no existe...Verifique!!", -2);

                GuardarOrden(conn, codEmpresa, request);

                var guardada = ConsultarOrden(conn, codConvenio, codOrden);
                if ((guardada?.total_pagar ?? 0) < 0)
                    return DbHelper.CreateErrorResponse<CrLiqOrdenData?>(
                        " - No se puede Cerrar! Monto a Liquidar es NEGATIVO revisar las deducciones!", -2);

                conn.Execute(
                    "exec spConvenio_Orden_Cierra @codConvenio, @codOrden, @usuario",
                    new { codConvenio, codOrden, usuario = request.usuario.Trim() });

                return DbHelper.CreateOkResponse<CrLiqOrdenData?>(
                    ConsultarOrden(conn, codConvenio, codOrden),
                    "La orden se cerro Correctamente!!");
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<CrLiqOrdenData?>(ex.Message);
            }
        }

        /// <summary>
        /// Cargos flotantes por cobrar del convenio (spConvenio_CxP_CargosFlotantes).
        /// </summary>
        public ErrorDto<List<CrLiqFlotanteData>> CrConveniosLiquidacion_Flotante_Obtener(
            int codEmpresa,
            string codConvenio)
        {
            return DbHelper.ExecuteListQuery<CrLiqFlotanteData>(
                _portalDb,
                codEmpresa,
                "exec spConvenio_CxP_CargosFlotantes @CodConvenio",
                new { CodConvenio = codConvenio.Trim() });
        }

        private static CrLiqNuevaOrdenData ArmarNuevaOrden(CrLiqConvenioData convenio, UltimaOrden? ultima)
        {
            var resultado = new CrLiqNuevaOrdenData
            {
                puede_crear = true,
                fecha_inicio = convenio.fecha_inicio,
                fecha_corte = convenio.fecha_vence,
                fecha_corte_max = convenio.fecha_vence,
                inicio_habilitado = true
            };

            if (ultima != null)
            {
                resultado.inicio_habilitado = false;
                AplicarUltimaOrden(resultado, convenio, ultima);
            }

            if (resultado.fecha_corte > convenio.fecha_corte_general)
                resultado.fecha_corte = convenio.fecha_corte_general;

            return resultado;
        }

        private static void AplicarUltimaOrden(CrLiqNuevaOrdenData resultado, CrLiqConvenioData convenio, UltimaOrden ultima)
        {
            if (ultima.estado == EstadoAbierta)
            {
                resultado.puede_crear = false;
                resultado.mensaje = $"La Orden {ultima.cod_orden} no se encuentra cerrada,verifique!!";
                return;
            }

            resultado.fecha_inicio = ultima.inicio;
            resultado.fecha_inicio_min = ultima.inicio;

            if (ultima.corte > convenio.fecha_vence)
            {
                resultado.fecha_corte = convenio.fecha_vence;
                resultado.mensaje = "El Contrato de este convenio se encuentra vencido, la fecha de corte fue puesta al vencimiento!!";
                return;
            }

            resultado.fecha_corte = ultima.corte;
            resultado.fecha_corte_max = ultima.corte;
        }

        private static CrLiqConvenioData? ConsultarConvenio(SqlConnection conn, string codConvenio)
            => conn.QueryFirstOrDefault<CrLiqConvenioData>(
                SqlConvenio + " where COD_CONVENIO = @codConvenio;", new { codConvenio });

        private static string? ConsultarEstado(SqlConnection conn, string codConvenio, int codOrden)
            => conn.QueryFirstOrDefault<string?>(@"
                select ESTADO from CRD_CONVENIOS_ORDENES
                where COD_CONVENIO = @codConvenio and COD_ORDEN = @codOrden;",
                new { codConvenio, codOrden })?.Trim();

        private static CrLiqOrdenData? ConsultarOrden(SqlConnection conn, string codConvenio, int codOrden)
        {
            const string sqlOrden = @"
                select
                    COD_ORDEN as cod_orden,
                    rtrim(COD_CONVENIO) as cod_convenio,
                    ESTADO as estado,
                    case ESTADO when 'A' then 'Abierta' when 'C' then 'Cerrada' when 'T' then 'Tramitada' else '' end as estado_desc,
                    rtrim(isnull(NOTAS, '')) as notas,
                    rtrim(isnull(DOCUMENTO, '')) as documento,
                    FECHA_INICIO as fecha_inicio,
                    FECHA_CORTE as fecha_corte,
                    isnull(RECAUDACION_CARGOS, 0) as recaudacion_cargos,
                    isnull(RECAUDACION_CUOTAS, 0) as recaudacion_cuotas,
                    isnull(RECAUDACION_DEVOLUCION, 0) as recaudacion_devolucion,
                    isnull(NUEVOS_CREDITOS, 0) as nuevos_creditos,
                    isnull(COMISIONES_RECAUDACION, 0) as comisiones_recaudacion,
                    isnull(COMISIONES_NUEVOS_CREDITOS, 0) as comisiones_nuevos_creditos,
                    isnull(TOTAL_RESERVA, 0) as total_reserva,
                    isnull(TOTAL_REBAJOS_CXP, 0) as total_rebajos_cxp,
                    isnull(RETENCION_AHORROS, 0) as retencion_ahorros,
                    isnull(TOTAL_PAGAR, 0) as total_pagar,
                    isnull(IVA_TOTAL, 0) as iva_total,
                    isnull(IVA_REFERENCIA_CRD, 0) as iva_referencia_crd,
                    isnull(FLOTANTE_ALCOBRO, 0) as flotante_alcobro,
                    isnull(APL_COM_INFORMATIVA, 0) as apl_com_informativa,
                    isnull(COBRA_CREDITOS_ANULADOS, 0) as cobra_creditos_anulados,
                    REGISTRO_USUARIO as registro_usuario,
                    REGISTRO_FECHA as registro_fecha
                from CRD_CONVENIOS_ORDENES
                where COD_ORDEN = @codOrden and COD_CONVENIO = @codConvenio;";

            var orden = conn.QueryFirstOrDefault<CrLiqOrdenData>(sqlOrden, new { codConvenio, codOrden });
            if (orden == null)
                return null;

            // Orden abierta: se muestran el flotante y la comisión informativa vigentes del convenio.
            if (orden.estado.Trim() == EstadoAbierta)
            {
                var convenio = ConsultarConvenio(conn, codConvenio);
                if (convenio != null)
                {
                    orden.flotante_alcobro = convenio.flotante;
                    orden.apl_com_informativa = convenio.comision_informativa;
                }
            }

            return orden;
        }

        /// <summary>
        /// Equivalente a fxValida del VB6; devuelve los mensajes concatenados (vacío si es válida).
        /// </summary>
        private static string ValidarOrden(SqlConnection conn, CrLiqOrdenGuardarRequest request)
        {
            var codConvenio = request.cod_convenio.Trim();
            var codOrden = request.cod_orden ?? 0;
            var mensajes = new List<string>();

            var convenio = codConvenio.Length > 0 ? ConsultarConvenio(conn, codConvenio) : null;
            if (convenio == null)
            {
                mensajes.Add("- No se ha consultado ningun convenio?");
                return string.Join(Environment.NewLine, mensajes);
            }

            if (codOrden > 0 && ConsultarEstado(conn, codConvenio, codOrden) == EstadoCerrada)
                mensajes.Add("- No se puede modificar una orden Cerrada.");

            if (string.IsNullOrWhiteSpace(request.notas))
                mensajes.Add(" - Debe indicar una Nota para la Orden, verifique!");

            var corte = (request.fecha_corte ?? DateTime.Today).Date;
            var inicio = (request.fecha_inicio ?? DateTime.Today).Date;

            if (corte > convenio.fecha_corte_general)
                mensajes.Add($" - La fecha de corte de la remesa no puede ser igual o mayor a : {convenio.fecha_corte_general:dd/MM/yyyy}, verifique!");

            if ((request.iva_referencia_crd ?? 0) > (request.total_pagar ?? 0))
                mensajes.Add(" - El Monto a Pagar es menor al monto del IVA de Referencia!");

            var conflictos = conn.QueryFirst<int>(@"
                select count(*)
                from CRD_CONVENIOS_ORDENES
                where COD_CONVENIO = @codConvenio
                  and COD_ORDEN <> @codOrden
                  and @inicio between FECHA_INICIO and FECHA_CORTE;",
                new { codConvenio, codOrden, inicio });
            if (conflictos > 0)
                mensajes.Add(" - Conflicto de Fechas con otra Liquidación, revise el rango de fechas!");

            return string.Join(Environment.NewLine, mensajes);
        }

        private int GuardarOrden(SqlConnection conn, int codEmpresa, CrLiqOrdenGuardarRequest request)
        {
            var codConvenio = request.cod_convenio.Trim();
            var convenio = ConsultarConvenio(conn, codConvenio)!;
            var codOrden = request.cod_orden ?? 0;
            var usuario = request.usuario.Trim();

            var totalOrden = (request.recaudacion_cargos ?? 0) + (request.recaudacion_cuotas ?? 0)
                           + (request.nuevos_creditos ?? 0) + (request.recaudacion_devolucion ?? 0);

            var parametros = new DynamicParameters();
            parametros.Add("codConvenio", codConvenio);
            parametros.Add("notas", request.notas.Trim());
            parametros.Add("inicio", (request.fecha_inicio ?? DateTime.Today).Date);
            parametros.Add("corte", (request.fecha_corte ?? DateTime.Today).Date.AddHours(23).AddMinutes(59).AddSeconds(59));
            parametros.Add("recaudacionCuotas", request.recaudacion_cuotas ?? 0);
            parametros.Add("recaudacionCargos", request.recaudacion_cargos ?? 0);
            parametros.Add("nuevosCreditos", request.nuevos_creditos ?? 0);
            parametros.Add("retencionAhorros", request.retencion_ahorros ?? 0);
            parametros.Add("comisionesRecaudacion", request.comisiones_recaudacion ?? 0);
            parametros.Add("comisionesNuevosCreditos", request.comisiones_nuevos_creditos ?? 0);
            parametros.Add("totalReserva", request.total_reserva ?? 0);
            parametros.Add("totalOrden", totalOrden);
            parametros.Add("totalRebajosCxp", request.total_rebajos_cxp ?? 0);
            parametros.Add("totalPagar", request.total_pagar ?? 0);
            parametros.Add("documento", request.documento.Trim());
            parametros.Add("porcComisionRecaudacion", convenio.porc_comision_recaudacion ?? 0);
            parametros.Add("porcComisionCreditos", convenio.porc_comision_creditos ?? 0);
            parametros.Add("recaudacionDevolucion", request.recaudacion_devolucion ?? 0);
            parametros.Add("aplComInformativa", request.apl_com_informativa ?? 0);
            parametros.Add("flotanteAlcobro", request.flotante_alcobro ?? 0);
            parametros.Add("cobraCreditosAnulados", request.cobra_creditos_anulados ?? 0);

            if (codOrden > 0)
            {
                parametros.Add("codOrden", codOrden);
                conn.Execute(SqlActualizarOrden, parametros);
            }
            else
            {
                codOrden = conn.QueryFirst<int>(@"
                    select isnull(max(COD_ORDEN), 0) + 1
                    from CRD_CONVENIOS_ORDENES
                    where COD_CONVENIO = @codConvenio;", new { codConvenio });

                parametros.Add("codOrden", codOrden);
                parametros.Add("usuario", usuario);
                conn.Execute(SqlInsertarOrden, parametros);
            }

            RecalcularDetalle(conn, codConvenio, codOrden);

            _bitacora.Bitacora(new BitacoraInsertarDto
            {
                EmpresaId = codEmpresa,
                Usuario = usuario,
                DetalleMovimiento = $"Liquidación Convenio.: {codConvenio} - Orden.:{codOrden}",
                Movimiento = "Modifica - WEB",
                Modulo = VModulo
            });

            return codOrden;
        }

        private static void RecalcularDetalle(SqlConnection conn, string codConvenio, int codOrden)
            => conn.Execute("exec spConvenio_Orden_Detalle @codConvenio, @codOrden", new { codConvenio, codOrden });

        private const string SqlActualizarOrden = @"
            update CRD_CONVENIOS_ORDENES
            set NOTAS = @notas,
                FECHA_INICIO = @inicio,
                FECHA_CORTE = @corte,
                RECAUDACION_CUOTAS = @recaudacionCuotas,
                RECAUDACION_CARGOS = @recaudacionCargos,
                NUEVOS_CREDITOS = @nuevosCreditos,
                RETENCION_AHORROS = @retencionAhorros,
                COMISIONES_RECAUDACION = @comisionesRecaudacion,
                COMISIONES_NUEVOS_CREDITOS = @comisionesNuevosCreditos,
                TOTAL_RESERVA = @totalReserva,
                TOTAL_ORDEN = @totalOrden,
                TOTAL_REBAJOS_CXP = @totalRebajosCxp,
                TOTAL_PAGAR = @totalPagar,
                DOCUMENTO = @documento,
                PORC_COMISION_RECAUDACION = @porcComisionRecaudacion,
                PORC_COMISION_CREDITOS = @porcComisionCreditos,
                RECAUDACION_DEVOLUCION = @recaudacionDevolucion,
                APL_COM_INFORMATIVA = @aplComInformativa,
                FLOTANTE_ALCOBRO = @flotanteAlcobro,
                COBRA_CREDITOS_ANULADOS = @cobraCreditosAnulados
            where COD_CONVENIO = @codConvenio and COD_ORDEN = @codOrden;";

        private const string SqlInsertarOrden = @"
            insert CRD_CONVENIOS_ORDENES (
                COD_ORDEN, COD_CONVENIO, REGISTRO_USUARIO, REGISTRO_FECHA, ESTADO, NOTAS, FECHA_INICIO, FECHA_CORTE,
                RECAUDACION_CUOTAS, RECAUDACION_CARGOS, NUEVOS_CREDITOS, RETENCION_AHORROS, COMISIONES_RECAUDACION,
                TOTAL_ORDEN, TOTAL_RESERVA, TOTAL_REBAJOS_CXP, TOTAL_PAGAR, DOCUMENTO, COMISIONES_NUEVOS_CREDITOS,
                PORC_COMISION_RECAUDACION, PORC_COMISION_CREDITOS, RECAUDACION_DEVOLUCION, APL_COM_INFORMATIVA,
                FLOTANTE_ALCOBRO, COBRA_CREDITOS_ANULADOS)
            values (
                @codOrden, @codConvenio, @usuario, dbo.MyGetdate(), 'A', @notas, @inicio, @corte,
                @recaudacionCuotas, @recaudacionCargos, @nuevosCreditos, @retencionAhorros, @comisionesRecaudacion,
                @totalOrden, @totalReserva, @totalRebajosCxp, @totalPagar, @documento, @comisionesNuevosCreditos,
                @porcComisionRecaudacion, @porcComisionCreditos, @recaudacionDevolucion, @aplComInformativa,
                @flotanteAlcobro, @cobraCreditosAnulados);";

        private sealed class UltimaOrden
        {
            public int cod_orden { get; set; }
            public string estado { get; set; } = string.Empty;
            public DateTime? inicio { get; set; }
            public DateTime? corte { get; set; }
        }
    }
}
