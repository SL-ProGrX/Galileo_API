using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Cobros;
using Galileo.Models.Security;
using Microsoft.Data.SqlClient;
using System.Globalization;

namespace Galileo_API.DataBaseTier.ProGrX.Cobros
{
    public class FrmCOTrasladoDeudaDB
    {
        private readonly PortalDB _portalDB;
        private readonly MSecurityMainDb _securityMainDb;
        private readonly MCobroDb _mCobroDb;
        private readonly MSeguimientoDB _mSeguimientoDb;
        private readonly MRecibos _mRecibosDb;
        private readonly int vModulo = 4;
        private const string FECHA = "yyyy/MM/dd";
        private const string OPERACION = "Operación requerida.";

        public FrmCOTrasladoDeudaDB(IConfiguration config)
        {
            _portalDB = new PortalDB(config);
            _securityMainDb = new MSecurityMainDb(config);
            _mCobroDb = new MCobroDb(config);
            _mSeguimientoDb = new MSeguimientoDB(config);
            _mRecibosDb = new MRecibos(config);
        }

        public ErrorDto Bitacora(BitacoraInsertarDto data)
        {
            return _securityMainDb.Bitacora(data);
        }
        /// <summary>
        /// Obtiene la informacion para hacer el traslado de deuda
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="id_solicitud"></param>
        /// <returns></returns>
        public ErrorDto<CoTrasladoDeudaObtenerDto> CO_TrasladoDeuda_Obtener(int CodEmpresa, long id_solicitud)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            var response = DbHelper.CreateOkResponse(new CoTrasladoDeudaObtenerDto
            {
                id_solicitud = id_solicitud,
                operacion = id_solicitud.ToString(CultureInfo.InvariantCulture)
            });

            response.Result ??= new CoTrasladoDeudaObtenerDto
            {
                id_solicitud = id_solicitud,
                operacion = id_solicitud.ToString(CultureInfo.InvariantCulture)
            };

            try
            {
                if (id_solicitud <= 0)
                    return DbHelper.CreateErrorResponse<CoTrasladoDeudaObtenerDto>(OPERACION);

                int sysPlanPagos = ObtenerSysPlanPagos(conn);
                DateTime fechaServidor = ObtenerFechaServidor(conn);

                response.Result.linea_cobro = _mCobroDb.fxCBRParametro(CodEmpresa, "16");
                response.Result.linea_cobro_descripcion = _mCobroDb.fxDescribeCodigo(CodEmpresa, response.Result.linea_cobro);
                response.Result.plazo = _mCobroDb.fxCBRPlazoRestante(CodEmpresa, id_solicitud);

                var cabecera = ObtenerCabeceraOperacion(conn, id_solicitud, sysPlanPagos);
                if (cabecera == null)
                    return DbHelper.CreateErrorResponse<CoTrasladoDeudaObtenerDto>("No se encontró número de solicitud.");

                CargarCabecera(response.Result, cabecera, id_solicitud);

                var resumen = ObtenerResumenDeuda(conn, id_solicitud, fechaServidor, sysPlanPagos, cabecera);
                response.Result.saldo = cabecera.saldo;
                CargarResumenDeuda(response.Result, resumen);

                response.Result.detalle = ObtenerDetalleGrilla(conn, id_solicitud, cabecera.num_fiadores);

                var calc = RecalcularDetalleInterno(new CoTrasladoDeudaCalcularRequest
                {
                    id_solicitud = id_solicitud,
                    plazo = response.Result.plazo,
                    tasa = response.Result.tasa,
                    total_deuda = response.Result.total_deuda,
                    detalle = response.Result.detalle
                });

                response.Result.porcentaje_asignado = calc.porcentaje_asignado;
                response.Result.total_recuperado = calc.total_recuperado;
                response.Result.detalle = calc.detalle;

                return response;
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<CoTrasladoDeudaObtenerDto>(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return DbHelper.CreateErrorResponse<CoTrasladoDeudaObtenerDto>(ex.Message);
            }
        }
        /// <summary>
        /// Se realiza el calculo de la couta de los rows que hay en la grilla
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public ErrorDto<CoTrasladoDeudaCalcularResponse> CO_TrasladoDeuda_Calcular(int CodEmpresa, CoTrasladoDeudaCalcularRequest data)
        {
            try
            {
                if (data == null)
                    return DbHelper.CreateErrorResponse<CoTrasladoDeudaCalcularResponse>("Datos requeridos.");

                if (!data.plazo.HasValue || data.plazo.Value < 1)
                    return DbHelper.CreateErrorResponse<CoTrasladoDeudaCalcularResponse>("El plazo es incorrecto.");

                if (!data.tasa.HasValue || data.tasa.Value < 0m || data.tasa.Value > 100m)
                    return DbHelper.CreateErrorResponse<CoTrasladoDeudaCalcularResponse>("La tasa es incorrecta.");

                if (!data.total_deuda.HasValue || data.total_deuda.Value == 0m)
                    return DbHelper.CreateErrorResponse<CoTrasladoDeudaCalcularResponse>("No existe un monto a trasladar.");

                return DbHelper.CreateOkResponse(RecalcularDetalleInterno(data));
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<CoTrasladoDeudaCalcularResponse>(ex.Message);
            }
            catch (Exception ex) when (ex is not SqlException)
            {
                System.Diagnostics.Trace.TraceError(
                    "Error inesperado en CO_TrasladoDeuda_Calcular: {0}", ex);

                return DbHelper.CreateErrorResponse<CoTrasladoDeudaCalcularResponse>(
                    "Se produjo un error inesperado al calcular la cuota.");
            }
        }
        /// <summary>
        /// Realiza la aplicacion del traslado de deuda
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        /// <summary>
        public ErrorDto<CoTrasladoDeudaAplicarResponse> CO_TrasladoDeuda_Aplicar(int CodEmpresa, CoTrasladoDeudaAplicarRequest data)
        {
            if (data == null)
                return DbHelper.CreateErrorResponse<CoTrasladoDeudaAplicarResponse>("Datos requeridos.");

            if (!data.id_solicitud.HasValue)
                return DbHelper.CreateErrorResponse<CoTrasladoDeudaAplicarResponse>(OPERACION);

            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);
            conn.Open();

            var response = DbHelper.CreateOkResponse(new CoTrasladoDeudaAplicarResponse
            {
                id_solicitud = data.id_solicitud.Value
            });

            response.Result ??= new CoTrasladoDeudaAplicarResponse
            {
                id_solicitud = data.id_solicitud.Value
            };

            SqlTransaction? tx = null;

            try
            {
                var validacion = ValidarAplicacion(CodEmpresa, conn, data);
                if (validacion != null)
                    return validacion;

                var preparacion = PrepararAplicacion(conn, CodEmpresa, data);

                if (preparacion.Error != null)
                    return preparacion.Error;

                if (preparacion.Contexto == null)
                {
                    return DbHelper.CreateErrorResponse<CoTrasladoDeudaAplicarResponse>(
                        "No fue posible preparar el contexto para aplicar el traslado de deuda.");
                }

                var contexto = preparacion.Contexto;

                EnsureConnectionOpen(conn);
                tx = conn.BeginTransaction();

                ProcesarAplicacion(conn, tx, contexto);

                tx.Commit();
                tx.Dispose();
                tx = null;

                EnsureConnectionOpen(conn);
                EjecutarPostAplicacion(conn, CodEmpresa, contexto);

                var errorRecibo = ValidarRecibo(CodEmpresa, contexto);
                if (errorRecibo != null)
                    return errorRecibo;

                response.Result ??= new CoTrasladoDeudaAplicarResponse
                {
                    id_solicitud = data.id_solicitud.Value
                };

                CompletarRespuestaAplicacion(response.Result, contexto);

                return response;
            }
            catch (SqlException ex)
            {
                RollbackTransaction(tx);
                return DbHelper.CreateErrorResponse<CoTrasladoDeudaAplicarResponse>(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                RollbackTransaction(tx);
                string message = $"Error inesperado al aplicar traslado de deuda: {ex.Message}";
                return DbHelper.CreateErrorResponse<CoTrasladoDeudaAplicarResponse>(message);
            }
            catch (ArgumentException ex)
            {
                RollbackTransaction(tx);
                string message = $"Error inesperado al aplicar traslado de deuda: {ex.Message}";
                return DbHelper.CreateErrorResponse<CoTrasladoDeudaAplicarResponse>(message);
            }
            catch (FormatException ex)
            {
                RollbackTransaction(tx);
                string message = $"Error inesperado al aplicar traslado de deuda: {ex.Message}";
                return DbHelper.CreateErrorResponse<CoTrasladoDeudaAplicarResponse>(message);
            }
        }
        /// <summary>
        /// Metodo para la exportacion de los datos de la grilla
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public ErrorDto<CoTrasladoDeudaExportResponse> CO_TrasladoDeuda_Export(int CodEmpresa, CoTrasladoDeudaExportRequest data)
        {
            try
            {
                if (data == null)
                    return DbHelper.CreateErrorResponse<CoTrasladoDeudaExportResponse>("Datos requeridos.");

                if (!data.id_solicitud.HasValue)
                    return DbHelper.CreateErrorResponse<CoTrasladoDeudaExportResponse>(OPERACION);

                if (!data.plazo.HasValue)
                    return DbHelper.CreateErrorResponse<CoTrasladoDeudaExportResponse>("El plazo es requerido.");

                if (!data.tasa.HasValue)
                    return DbHelper.CreateErrorResponse<CoTrasladoDeudaExportResponse>("La tasa es requerida.");

                var obtener = CO_TrasladoDeuda_Obtener(CodEmpresa, data.id_solicitud.Value);
                if (obtener.Code != 0 || obtener.Result == null)
                {
                    return DbHelper.CreateErrorResponse<CoTrasladoDeudaExportResponse>(
                        obtener.Description ?? "No fue posible obtener la información para exportar.");
                }

                var calc = RecalcularDetalleInterno(new CoTrasladoDeudaCalcularRequest
                {
                    id_solicitud = data.id_solicitud.Value,
                    plazo = data.plazo.Value,
                    tasa = data.tasa.Value,
                    total_deuda = obtener.Result.total_deuda,
                    detalle = data.detalle ?? new List<CoTrasladoDeudaDetalleDto>()
                });

                return DbHelper.CreateOkResponse(new CoTrasladoDeudaExportResponse
                {
                    id_solicitud = data.id_solicitud.Value,
                    operacion = obtener.Result.operacion,
                    linea = obtener.Result.linea,
                    identificacion = obtener.Result.identificacion,
                    nombre = obtener.Result.nombre,
                    porcentaje_asignado = calc.porcentaje_asignado,
                    total_deuda = calc.total_deuda,
                    total_recuperado = calc.total_recuperado,
                    detalle = calc.detalle
                });
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<CoTrasladoDeudaExportResponse>(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                string message = $"Error inesperado al exportar traslado de deuda: {ex.Message}";
                return DbHelper.CreateErrorResponse<CoTrasladoDeudaExportResponse>(message);
            }
            catch (ArgumentNullException ex)
            {
                string message = $"Error inesperado al exportar traslado de deuda: {ex.Message}";
                return DbHelper.CreateErrorResponse<CoTrasladoDeudaExportResponse>(message);
            }
        }
        private CoTrasladoDeudaCalcularResponse RecalcularDetalleInterno(CoTrasladoDeudaCalcularRequest data)
        {
            decimal totalDeuda = data.total_deuda ?? 0m;
            int plazo = data.plazo ?? 0;
            decimal tasa = data.tasa ?? 0m;

            var detalle = (data.detalle ?? new List<CoTrasladoDeudaDetalleDto>())
                .Select(x => new CoTrasladoDeudaDetalleDto
                {
                    tipo = (x.tipo ?? string.Empty).Trim(),
                    identificacion = (x.identificacion ?? string.Empty).Trim(),
                    nombre = (x.nombre ?? string.Empty).Trim(),
                    porcentaje = x.porcentaje,
                    monto = 0m,
                    cuota = 0m,
                    estado = (x.estado ?? string.Empty).Trim(),
                    recuperado = x.recuperado,
                    id_retencion = x.id_retencion,
                    estado_codigo = (x.estado_codigo ?? string.Empty).Trim(),
                    opex = x.opex
                })
                .ToList();

            decimal totalPorcentaje = 0m;
            decimal totalRecuperado = 0m;

            foreach (var item in detalle)
            {
                if (item.porcentaje <= 0m)
                {
                    item.monto = 0m;
                    item.cuota = 0m;
                    continue;
                }

                totalPorcentaje += item.porcentaje;
                decimal factor = item.porcentaje / 100m;
                item.monto = Math.Round(totalDeuda * factor, 2);
                item.cuota = MCobroDb.fxCalcula_Cuota(item.monto, plazo, tasa);
                item.recuperado = item.monto;
                totalRecuperado += item.monto;
            }

            return new CoTrasladoDeudaCalcularResponse
            {
                porcentaje_asignado = Math.Round(totalPorcentaje, 2),
                total_deuda = Math.Round(totalDeuda, 2),
                total_recuperado = Math.Round(totalRecuperado, 2),
                detalle = detalle
            };
        }
        private static int ResolverOpexItem(CoTrasladoDeudaDetalleDto item)
        {
            if (item.opex is 0 or 1)
                return item.opex;

            var estadoCodigo = (item.estado_codigo ?? string.Empty).Trim().ToUpperInvariant();
            return estadoCodigo == "S" ? 0 : 1;
        }
        private ErrorDto<CoTrasladoDeudaAplicarResponse>? ValidarAplicacion(int CodEmpresa, SqlConnection conn, CoTrasladoDeudaAplicarRequest data)
        {
            if (!data.id_solicitud.HasValue || data.id_solicitud.Value <= 0)
                return DbHelper.CreateErrorResponse<CoTrasladoDeudaAplicarResponse>(OPERACION);

            if (!data.plazo.HasValue || data.plazo.Value < 1 || data.plazo.Value > 300)
                return DbHelper.CreateErrorResponse<CoTrasladoDeudaAplicarResponse>("El plazo es incorrecto verifique.");

            if (!data.tasa.HasValue || data.tasa.Value < 0m || data.tasa.Value > 100m)
                return DbHelper.CreateErrorResponse<CoTrasladoDeudaAplicarResponse>("La Tasa es incorrecta verifique.");

            if (string.IsNullOrWhiteSpace(data.notas))
                return DbHelper.CreateErrorResponse<CoTrasladoDeudaAplicarResponse>("Especifique una nota para el traslado.");

            if (string.IsNullOrWhiteSpace(data.usuario))
                return DbHelper.CreateErrorResponse<CoTrasladoDeudaAplicarResponse>("Usuario requerido.");

            string lineaNueva = _mCobroDb.fxCBRParametro(CodEmpresa, "16");
            if (string.IsNullOrWhiteSpace(lineaNueva))
                return DbHelper.CreateErrorResponse<CoTrasladoDeudaAplicarResponse>("La Línea para Traslado de Deudas No Existe.");

            const string sqlLinea = @"select isnull(count(*),0) from catalogo where codigo = @codigo;";
            int existeLinea = conn.QueryFirstOrDefault<int>(sqlLinea, new { codigo = lineaNueva });
            if (existeLinea <= 0)
                return DbHelper.CreateErrorResponse<CoTrasladoDeudaAplicarResponse>("La Línea para Traslado de Deudas No Existe.");

            const string sqlOperacion = @"select isnull(count(*),0) from reg_creditos where proceso = 'N' and id_solicitud = @id_solicitud;";
            int existeOpNormal = conn.QueryFirstOrDefault<int>(sqlOperacion, new { id_solicitud = data.id_solicitud.Value });
            if (existeOpNormal <= 0)
            {
                return DbHelper.CreateErrorResponse<CoTrasladoDeudaAplicarResponse>(
                    "La operación no se encuentra en PROCESO NORMAL para realizar el traslado.");
            }

            return null;
        }
        private AplicacionContexto CrearContextoAplicacion(SqlConnection conn,int codEmpresa,CoTrasladoDeudaAplicarRequest data,CoTrasladoDeudaObtenerDto baseDto,CoTrasladoDeudaCalcularResponse detalle)
        {
            var usuario = (data.usuario ?? string.Empty).Trim().ToUpperInvariant();
            var notas = (data.notas ?? string.Empty).Trim();
            var fecha = ObtenerFechaServidor(conn);
            var contabilidad = ObtenerContabilidadEnlace(conn);
            var sysPlanPagos = ObtenerSysPlanPagos(conn);
            var oficina = ObtenerOficinaContexto(conn, data.id_solicitud ?? 0, contabilidad);
            var documento = ConstruirDocumentoInicial(conn, codEmpresa, baseDto, detalle, notas, usuario);
            var cuentasNuevaLinea = ObtenerCuentasNuevaLinea(conn, baseDto.linea_cobro);
            long priDeduc = ObtenerPrimeraDeduccion(conn, codEmpresa, baseDto.identificacion);

            return new AplicacionContexto
            {
                data = data,
                baseDto = baseDto,
                detalle = detalle,
                usuario = usuario,
                notas = notas,
                fecha = fecha,
                contabilidad = contabilidad,
                sysPlanPagos = sysPlanPagos,
                oficina = oficina,
                documento = documento,
                cuentasNuevaLinea = cuentasNuevaLinea,
                priDeduc = priDeduc
            };
        }
        private static DateTime ObtenerFechaServidor(SqlConnection conn)
        {
            return conn.QueryFirstOrDefault<DateTime>("select dbo.MyGetdate();");
        }
        private static int ObtenerContabilidadEnlace(SqlConnection conn)
        {
            return conn.QueryFirstOrDefault<int>("select isnull(COD_EMPRESA_ENLACE,0) from SIF_EMPRESA;");
        }
        private static int ObtenerSysPlanPagos(SqlConnection conn)
        {
            const string sqlExiste = @"
        select isnull(count(*),0)
        from INFORMATION_SCHEMA.COLUMNS
        where TABLE_NAME = 'SIF_EMPRESA'
          and COLUMN_NAME = 'SysCrdPlanPago';";

            int existe = conn.QueryFirstOrDefault<int>(sqlExiste);
            if (existe <= 0)
                return 0;

            return conn.QueryFirstOrDefault<int>(
                "select isnull(SysCrdPlanPago,0) from SIF_EMPRESA;");
        }
        private static CabeceraOperacionRow? ObtenerCabeceraOperacion(SqlConnection conn, long id_solicitud, int sysPlanPagos)
        {
            string sql = sysPlanPagos == 1
                ? @"
                select
                    r.cedula,
                    s.nombre,
                    r.saldo,
                    r.proceso,
                    r.interesv as tasa,
                    r.plazo,
                    r.int as tasa_original,
                    r.codigo,
                    c.descripcion,
                    isnull(r.liqtasa,0) as liq_tasa,
                    isnull(r.opex,0) as opex,
                    isnull(r.cod_divisa,'') as divisa,
                    dbo.fxCRDNumFiadores(r.id_solicitud) as num_fiadores,
                    isnull(r.tbp_puntosadd,0) as tbp_puntos_add,
                    isnull(v.amortiza,0) as mora_amortiza,
                    r.cod_oficina_r,
                    r.cod_oficina_f,
                    r.cod_oficina_comision,
                    dbo.MyGetdate() as fecha_server
                from socios s
                inner join reg_creditos r on s.cedula = r.cedula
                inner join catalogo c on r.codigo = c.codigo
                left join vista_morosidad v on r.id_solicitud = v.id_solicitud
                where r.id_solicitud = @id_solicitud;"
                : @"
                select
                    r.cedula,
                    s.nombre,
                    r.saldo,
                    r.proceso,
                    r.interesv as tasa,
                    r.plazo,
                    r.int as tasa_original,
                    r.codigo,
                    c.descripcion,
                    isnull(r.liqtasa,0) as liq_tasa,
                    isnull(r.opex,0) as opex,
                    isnull(r.cod_divisa,'') as divisa,
                    dbo.fxCRDNumFiadores(r.id_solicitud) as num_fiadores,
                    isnull(r.tbp_puntosadd,0) as tbp_puntos_add,
                    isnull(v.intc,0) as mora_intc,
                    isnull(v.intm,0) as mora_intm,
                    isnull(v.amortiza,0) as mora_amortiza,
                    isnull(v.cargos,0) as cargos,
                    dbo.fxCRDCalculoIntCorte(r.id_solicitud,dbo.MyGetdate()) as interes_total,
                    cast(0 as decimal(16,2)) as poliza,
                    r.cod_oficina_r,
                    r.cod_oficina_f,
                    r.cod_oficina_comision,
                    dbo.MyGetdate() as fecha_server
                from socios s
                inner join reg_creditos r on s.cedula = r.cedula
                inner join catalogo c on r.codigo = c.codigo
                left join vista_morosidad v on r.id_solicitud = v.id_solicitud
                where r.id_solicitud = @id_solicitud;";

            return conn.QueryFirstOrDefault<CabeceraOperacionRow>(sql, new { id_solicitud });
        }
        private ResumenDeudaRow ObtenerResumenDeuda(SqlConnection conn, long id_solicitud, DateTime fechaServidor, int sysPlanPagos, CabeceraOperacionRow cabecera)
        {
            if (sysPlanPagos == 1)
            {
                var plan = conn.QueryFirstOrDefault<PlanPagoCancelacionRow>(
                    "exec spCrdPlanPagosInfoCancelacion @Operacion, @Fecha",
                    new
                    {
                        Operacion = id_solicitud,
                        Fecha = fechaServidor.ToString(FECHA, CultureInfo.InvariantCulture)
                    }) ?? new PlanPagoCancelacionRow();

                return new ResumenDeudaRow
                {
                    interes_corriente = plan.int_cor,
                    interes_moratorio = plan.int_mor,
                    principal_mora = cabecera.mora_amortiza,
                    interes_pendiente = 0m,
                    cargos = plan.cargos,
                    poliza = plan.poliza
                };
            }

            decimal pendiente = cabecera.interes_total - (cabecera.mora_intc + cabecera.mora_intm);
            if (pendiente < 0m)
                pendiente = 0m;

            return new ResumenDeudaRow
            {
                interes_corriente = cabecera.mora_intc,
                interes_moratorio = cabecera.mora_intm,
                principal_mora = cabecera.mora_amortiza,
                interes_pendiente = pendiente,
                cargos = cabecera.cargos,
                poliza = cabecera.poliza
            };
        }
        private static void CargarCabecera(CoTrasladoDeudaObtenerDto dto, CabeceraOperacionRow row, long id_solicitud)
        {
            dto.id_solicitud = id_solicitud;
            dto.operacion = id_solicitud.ToString(CultureInfo.InvariantCulture);
            dto.linea = row.codigo;
            dto.linea_descripcion = row.descripcion;
            dto.identificacion = row.cedula;
            dto.nombre = row.nombre;
            dto.divisa = row.divisa;
            dto.tasa = row.tasa;
            dto.tasa_original = row.tasa_original;
            dto.plazo_original = row.plazo;
            dto.proceso = TraducirProceso(row.proceso);
            dto.opex = row.opex == 1 ? "Sí" : "No";
            dto.tbp_puntos_add = row.tbp_puntos_add;
            dto.liq_tasa = row.liq_tasa;
            dto.tasa_label = ConstruirEtiquetaTasa(row.tbp_puntos_add, row.liq_tasa);
        }
        private static void CargarResumenDeuda(CoTrasladoDeudaObtenerDto dto, ResumenDeudaRow deuda)
        {
            dto.interes_corriente = deuda.interes_corriente;
            dto.interes_moratorio = deuda.interes_moratorio;
            dto.principal_mora = deuda.principal_mora;
            dto.interes_pendiente = deuda.interes_pendiente;
            dto.cargos_registrados = deuda.cargos;
            dto.polizas_atrasadas = deuda.poliza;
            dto.intereses = deuda.interes_corriente + deuda.interes_moratorio + deuda.interes_pendiente;
            dto.total_deuda = dto.saldo + dto.intereses + dto.cargos_registrados + dto.polizas_atrasadas;
        }
        private List<CoTrasladoDeudaDetalleDto> ObtenerDetalleGrilla( SqlConnection conn,long id_solicitud,int numFiadores)
        {
            decimal porcentajeBase = numFiadores > 0
                ? Math.Round(100m / numFiadores, 2)
                : 0m;

            const string sql = @"
        select
            x.tipo,
            x.identificacion,
            x.nombre,
            x.porcentaje,
            cast(0 as decimal(16,2)) as monto,
            cast(0 as decimal(16,2)) as cuota,
            x.estado,
            cast(0 as decimal(16,2)) as recuperado,
            cast(0 as bigint) as id_retencion,
            x.estado_codigo,
            x.opex
        from
        (
            select
                'D' as tipo,
                s.cedula as identificacion,
                s.nombre,
                cast(0 as decimal(16,2)) as porcentaje,
                est.descripcion as estado,
                s.estadoactual as estado_codigo,
                case when s.estadoactual = 'S' then 0 else 1 end as opex
            from socios s
            inner join reg_creditos r on s.cedula = r.cedula
            inner join afi_estados_persona est on s.estadoactual = est.cod_estado
            where r.id_solicitud = @id_solicitud

            union

            select
                'F' as tipo,
                s.cedula as identificacion,
                s.nombre,
                @porcentaje as porcentaje,
                est.descripcion as estado,
                s.estadoactual as estado_codigo,
                case when s.estadoactual = 'S' then 0 else 1 end as opex
            from socios s
            inner join fiadores f on s.cedula = f.cedulaf
            inner join afi_estados_persona est on s.estadoactual = est.cod_estado
            where f.id_solicitud = @id_solicitud
              and f.estado = 'A'
        ) x
        order by case when x.tipo = 'D' then 0 else 1 end, x.identificacion;";

            return conn.Query<CoTrasladoDeudaDetalleDto>(sql, new
            {
                id_solicitud,
                porcentaje = porcentajeBase
            }).ToList();
        }
        private static string TraducirProceso(string? proceso)
        {
            return (proceso ?? string.Empty).Trim().ToUpperInvariant() switch
            {
                "N" => "Normal",
                "T" => "Traslado",
                "J" => "Cobro Judicial",
                "C" => "Cobro Judicial",
                _ => "Otro"
            };
        }
        private static string ConstruirEtiquetaTasa(decimal tbpPuntosAdd, int liqTasa)
        {
            string label = tbpPuntosAdd != 0m
                ? $"Tasa (TBP + {tbpPuntosAdd.ToString(CultureInfo.InvariantCulture)})"
                : "Tasa %";

            if (liqTasa == 1)
                label += " + PtsLiq";

            return label;
        }
        private OficinaContexto ObtenerOficinaContexto(SqlConnection conn, long id_solicitud, int contabilidad)
        {
            const string sql = @"
                select
                    isnull(o.cod_oficina,'') as cod_oficina,
                    isnull(o.cod_unidad,'') as cod_unidad,
                    isnull(o.cod_centro_costo,'') as cod_centro_costo,
                    isnull(r.cod_divisa,'COL') as cod_divisa,
                    isnull(r.dia_pago,32) as dia_pago,
                    isnull(r.base_calculo,'01') as base_calculo,
                    dbo.fxCntXTipoCambio(@Contabilidad, r.cod_divisa, getdate(), 'V') as tipo_cambio
                from reg_creditos r
                left join sif_oficinas o on r.cod_oficina_r = o.cod_oficina
                where r.id_solicitud = @id_solicitud;";

            var row = conn.QueryFirstOrDefault<OficinaContexto>(sql, new
            {
                Contabilidad = contabilidad,
                id_solicitud
            }) ?? new OficinaContexto();

            if (!string.IsNullOrWhiteSpace(row.cod_oficina))
            {
                row.oficina_titular = ObtenerOficinaTitular(conn);
                return row;
            }

            var op = conn.QueryFirstOrDefault<SpOperacionCtasRow>(
                "exec spCrdOperacionCtas @Operacion",
                new { Operacion = id_solicitud }) ?? new SpOperacionCtasRow();

            row.cod_oficina = op.cod_oficina_r;
            row.cod_unidad = op.cod_unidad;
            row.cod_centro_costo = op.cod_centro_costo;
            row.oficina_titular = ObtenerOficinaTitular(conn);

            return row;
        }
        private static string ObtenerOficinaTitular(SqlConnection conn)
        {
            const string sql = @"
                select top 1 isnull(cod_oficina,'')
                from sif_oficinas
                where estado = 1 and oficina_omision = 1;";
            return conn.QueryFirstOrDefault<string>(sql) ?? string.Empty;
        }
        private DocumentoContexto ConstruirDocumentoInicial(SqlConnection conn,int codEmpresa, CoTrasladoDeudaObtenerDto baseDto,CoTrasladoDeudaCalcularResponse calc,string notas,string usuario)
        {
            string tipo = "TRA";
            long consecutivo = _mRecibosDb.FxDocumentoConsecutivo(codEmpresa, tipo);

            if (consecutivo <= 0)
                throw new InvalidOperationException("No se pudo obtener el consecutivo del documento.");

            string documento = consecutivo.ToString(CultureInfo.InvariantCulture);
            string concepto = "CBR002";
            string deposito = "";

            var lineas = new string[12];
            lineas[1] = $"Saldo Anterior    {baseDto.saldo.ToString("N2", CultureInfo.InvariantCulture)}";
            lineas[2] = $"Interes Corriente {(baseDto.interes_corriente + baseDto.interes_pendiente).ToString("N2", CultureInfo.InvariantCulture)}";
            lineas[3] = $"Interes Moratorio {baseDto.interes_moratorio.ToString("N2", CultureInfo.InvariantCulture)}";
            lineas[4] = $"Cargos            {baseDto.cargos_registrados.ToString("N2", CultureInfo.InvariantCulture)}";
            lineas[5] = $"Amortizacion      {baseDto.saldo.ToString("N2", CultureInfo.InvariantCulture)}";
            lineas[6] = $"Saldo Actual      {0m.ToString("N2", CultureInfo.InvariantCulture)}";
            lineas[7] = $"Operación         {baseDto.operacion}";
            lineas[8] = $"Línea             {baseDto.linea}";
            lineas[9] = "Proc.Retencion    NO";
            lineas[10] = $"Usuario           {usuario}";
            lineas[11] = $"Póliza            {baseDto.polizas_atrasadas.ToString("N2", CultureInfo.InvariantCulture)}";

            const string sql = @"
                insert into SIF_TRANSACCIONES
                (
                    COD_TRANSACCION, TIPO_DOCUMENTO, REGISTRO_FECHA, REGISTRO_USUARIO,
                    CLIENTE_IDENTIFICACION, CLIENTE_NOMBRE, COD_CONCEPTO, MONTO, ESTADO,
                    REFERENCIA_01, REFERENCIA_02, REFERENCIA_03, COD_OFICINA,
                    LINEA1, LINEA2, LINEA3, LINEA4, LINEA5, LINEA6,
                    LINEA7, LINEA8, LINEA9, LINEA10, DETALLE, DOCUMENTO, LINEA11
                )
                values
                (
                    @COD_TRANSACCION, @TIPO_DOCUMENTO, dbo.MyGetdate(), @REGISTRO_USUARIO,
                    @CLIENTE_IDENTIFICACION, @CLIENTE_NOMBRE, @COD_CONCEPTO, @MONTO, 'P',
                    @REFERENCIA_01, @REFERENCIA_02, @REFERENCIA_03, @COD_OFICINA,
                    @LINEA1, @LINEA2, @LINEA3, @LINEA4, @LINEA5, @LINEA6,
                    @LINEA7, @LINEA8, @LINEA9, @LINEA10, @DETALLE, @DOCUMENTO, @LINEA11
                );";

            conn.Execute(sql, new
            {
                COD_TRANSACCION = documento,
                TIPO_DOCUMENTO = tipo,
                REGISTRO_USUARIO = usuario,
                CLIENTE_IDENTIFICACION = baseDto.identificacion,
                CLIENTE_NOMBRE = baseDto.nombre,
                COD_CONCEPTO = concepto,
                MONTO = calc.total_deuda,
                REFERENCIA_01 = baseDto.operacion,
                REFERENCIA_02 = baseDto.linea,
                REFERENCIA_03 = deposito,
                COD_OFICINA = ObtenerOficinaTitular(conn),
                LINEA1 = lineas[1],
                LINEA2 = lineas[2],
                LINEA3 = lineas[3],
                LINEA4 = lineas[4],
                LINEA5 = lineas[5],
                LINEA6 = lineas[6],
                LINEA7 = lineas[7],
                LINEA8 = lineas[8],
                LINEA9 = lineas[9],
                LINEA10 = lineas[10],
                DETALLE = notas,
                DOCUMENTO = deposito,
                LINEA11 = lineas[11]
            });

            return new DocumentoContexto
            {
                tipo_documento = tipo,
                documento = documento,
                concepto = concepto,
                detalle = notas,
                deposito = deposito
            };
        }
        private long ObtenerPrimeraDeduccion(SqlConnection conn, int codEmpresa, string cedula)
        {
            const string sql = @"
                select
                    isnull(dbo.fxSIFDateTimeToProceso(dbo.fxCrd_Primer_Deduccion(cod_deductora)), 0) as pri_deduc,
                    cod_deductora
                from vAfi_Persona_Deductora
                where cedula = @cedula;";

            var row = conn.QueryFirstOrDefault<PriDeducRow>(sql, new { cedula }) ?? new PriDeducRow();

            if (row.pri_deduc > 0)
                return row.pri_deduc;

            return (long)_mSeguimientoDb.fxPrimerDeduccion(codEmpresa, pDeductora: row.cod_deductora);
        }
        private static CuentasNuevaLineaRow ObtenerCuentasNuevaLinea(SqlConnection conn, string lineaNueva)
        {
            const string sql = @"
                select
                    isnull(ctanamort,'') as cta_n_amort,
                    isnull(ctaoamort,'') as cta_o_amort
                from catalogo
                where codigo = @codigo;";

            return conn.QueryFirstOrDefault<CuentasNuevaLineaRow>(sql, new { codigo = lineaNueva }) ?? new CuentasNuevaLineaRow();
        }
        private static long InsertarNuevaOperacion(SqlConnection conn, SqlTransaction tx, InsertNuevaOperacionArgs a)
        {
            const string sql = @"
                insert into reg_creditos
                (
                    codigo, id_comite, cedula, montosol, estadosol, fechares,
                    plazo, int, interesv, montoapr, prideduc, fechaforp, fechaforf,
                    saldo, amortiza, interesc, cuota, referencia, userrec, userres, userfor,
                    garantia, firma_deudor, monto_girado, cuotas_planilla, cuotas_directas,
                    cuotas_anuladas, tesoreria, opex, fecult, observacion, tbp_puntosadd,
                    liqtasa, cod_oficina_r, cod_oficina_f, base_calculo, cod_divisa,
                    cuota_fija, dia_pago
                )
                values
                (
                    @codigo, 1, @cedula, @montosol, 'F', @fechares,
                    @plazo, @int, @interesv, @montoapr, @prideduc, @fechaforp, @fechaforf,
                    @saldo, 0, 0, @cuota, @referencia, @userrec, @userres, @userfor,
                    'F', 1, 0, 0, 0, 0, @tesoreria, @opex, @fecult, @observacion,
                    @tbp_puntosadd, @liqtasa, @cod_oficina_r, @cod_oficina_f, @base_calculo,
                    @cod_divisa, 0, @dia_pago
                );";

            conn.Execute(sql, new
            {
                codigo = a.lineaNueva,
                cedula = a.identificacion,
                montosol = a.monto,
                fechares = a.fecha.ToString(FECHA, CultureInfo.InvariantCulture),
                plazo = a.plazo,
                @int = a.tasa,
                interesv = a.tasa,
                montoapr = a.monto,
                prideduc = a.priDeduc,
                fechaforp = a.fecha.ToString(FECHA, CultureInfo.InvariantCulture),
                fechaforf = a.fecha.ToString(FECHA, CultureInfo.InvariantCulture),
                saldo = a.monto,
                cuota = a.cuota,
                referencia = a.referencia,
                userrec = a.usuario,
                userres = a.usuario,
                userfor = a.usuario,
                tesoreria = a.fecha.ToString(FECHA, CultureInfo.InvariantCulture),
                opex = a.opex,
                fecult = a.fechaProceso,
                observacion = a.notas,
                tbp_puntosadd = a.tbpPuntosAdd == 0m ? (decimal?)null : a.tbpPuntosAdd,
                liqtasa = a.liqTasa,
                cod_oficina_r = a.codOficinaR,
                cod_oficina_f = a.codOficinaF,
                base_calculo = a.baseCalculo,
                cod_divisa = a.codDivisa,
                dia_pago = a.diaPago
            }, tx);

            const string sqlMax = @"
                select isnull(max(id_solicitud),0)
                from reg_creditos
                where cedula = @cedula
                  and codigo = @codigo;";

            return conn.QueryFirstOrDefault<long>(sqlMax, new
            {
                cedula = a.identificacion,
                codigo = a.lineaNueva
            }, tx);
        }
        private static decimal ObtenerFechaProcesoActual(SqlConnection conn, SqlTransaction tx)
        {
            return conn.QueryFirstOrDefault<decimal>(
                "select dbo.fxSIFDateTimeToProceso(dbo.MyGetdate());",
                transaction: tx);
        }
        private static void RegistrarAsiento(SqlConnection conn, SqlTransaction? tx, AsientoArgs args)
        {
            conn.Execute(
                "exec spSIFDocsAsiento @Tipo, @Transaccion, @Monto, @Movimiento, @Divisa, @TipoCambio, @Contabilidad, @Unidad, @CentroCosto, @Cuenta, @Referencia1, @Referencia2, @Referencia3",
                new
                {
                    Tipo = args.tipo,
                    Transaccion = args.transaccion,
                    Monto = args.monto,
                    Movimiento = args.movimiento,
                    Divisa = args.divisa,
                    TipoCambio = args.tipoCambio,
                    Contabilidad = args.contabilidad,
                    Unidad = args.unidad,
                    CentroCosto = args.centroCosto,
                    Cuenta = args.cuenta,
                    Referencia1 = args.referencia1,
                    Referencia2 = args.referencia2,
                    Referencia3 = args.referencia3
                },
                tx);
        }
        private void EjecutarFlujoPlanPagos(SqlConnection conn, SqlTransaction tx, AplicacionContexto ctx)
        {
            conn.Execute(
                "exec spCrdPlanPagosMoraActualizaOp @Operacion, @Fecha",
                new
                {
                    Operacion = ctx.data.id_solicitud,
                    Fecha = ctx.fecha.ToString(FECHA, CultureInfo.InvariantCulture)
                },
                tx);

            conn.Execute(
                "exec spCrdPlanPagoAbonoCancelacion @Operacion, @Concepto, @Usuario, @TipoCom, @NumCom, @Abono, @FechaChr, @Caja",
                new
                {
                    Operacion = ctx.data.id_solicitud,
                    Concepto = ctx.documento.concepto,
                    Usuario = ctx.usuario,
                    TipoCom = ctx.documento.tipo_documento,
                    NumCom = ctx.documento.documento,
                    Abono = ctx.detalle.total_deuda,
                    FechaChr = ctx.fecha.ToString("yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture),
                    Caja = ""
                },
                tx);

            conn.Execute(@"
                update reg_creditos
                   set estado = 'A',
                       proceso = 'T',
                       fecha_enviaproceso = dbo.MyGetdate(),
                       observacion_proceso = @notas
                 where id_solicitud = @id_solicitud",
                new
                {
                    notas = ctx.notas,
                    id_solicitud = ctx.data.id_solicitud
                },
                tx);
        }
        private void EjecutarFlujoSinPlanPagos(SqlConnection conn, SqlTransaction tx, AplicacionContexto ctx)
        {
            conn.Execute(@"
                update morosidad
                   set abintc = intc,
                       abintm = intm,
                       abamortiza = amortiza,
                       abcargo = cargo,
                       estado = 'C',
                       tcon = @tcon,
                       ncon = @ncon,
                       fecult = dbo.MyGetdate(),
                       cod_concepto = @cod_concepto,
                       usuario = @usuario
                 where estado = 'A'
                   and id_solicitud = @id_solicitud;",
                new
                {
                    tcon = ctx.documento.tipo_documento,
                    ncon = ctx.documento.documento,
                    cod_concepto = ctx.documento.concepto,
                    usuario = ctx.usuario,
                    id_solicitud = ctx.data.id_solicitud
                },
                tx);

            if (ctx.baseDto.interes_pendiente > 0m)
            {
                decimal proceso = ObtenerFechaProcesoActual(conn, tx);

                conn.Execute(@"
                    insert into MOROSIDAD
                    (
                        CODIGO, ID_SOLICITUD, FECHAP, FECAP, FECULT, ESTADO, ESTADOI,
                        INTC, INTM, AMORTIZA, CARGO, ABINTC, ABINTM, ABAMORTIZA, ABCARGO,
                        TCON, NCON, COD_CONCEPTO, USUARIO, COD_CAJA
                    )
                    values
                    (
                        @CODIGO, @ID_SOLICITUD, @FECHAP, @FECAP, dbo.MyGetdate(), 'C', 'A',
                        @INTC, 0, 0, 0, @ABINTC, 0, 0, 0,
                        @TCON, @NCON, @COD_CONCEPTO, @USUARIO, ''
                    );",
                    new
                    {
                        CODIGO = ctx.baseDto.linea,
                        ID_SOLICITUD = ctx.data.id_solicitud,
                        FECHAP = proceso,
                        FECAP = proceso,
                        INTC = ctx.baseDto.interes_pendiente,
                        ABINTC = ctx.baseDto.interes_pendiente,
                        TCON = ctx.documento.tipo_documento,
                        NCON = ctx.documento.documento,
                        COD_CONCEPTO = ctx.documento.concepto,
                        USUARIO = ctx.usuario
                    },
                    tx);
            }

            decimal diferenciaSaldo = ctx.baseDto.saldo - ctx.baseDto.principal_mora;
            if (diferenciaSaldo < 0m)
                diferenciaSaldo = 0m;

            conn.Execute(@"
                insert into creditos_dt
                (
                    CODIGO, ID_SOLICITUD, CUOTA, ABONO, INTCP, AMORTIZA,
                    FECHAS, FECHAP, TCON, NCON, ESTADO, ESTADO_ASIENTO,
                    COD_CONCEPTO, USUARIO, COD_CAJA
                )
                values
                (
                    @CODIGO, @ID_SOLICITUD, 0, @ABONO, 0, @AMORTIZA,
                    dbo.MyGetdate(), @FECHAP, @TCON, @NCON, 'A', 'G',
                    @COD_CONCEPTO, @USUARIO, ''
                );",
                new
                {
                    CODIGO = ctx.baseDto.linea,
                    ID_SOLICITUD = ctx.data.id_solicitud,
                    ABONO = diferenciaSaldo,
                    AMORTIZA = diferenciaSaldo,
                    FECHAP = ObtenerFechaProcesoActual(conn, tx),
                    TCON = ctx.documento.tipo_documento,
                    NCON = ctx.documento.documento,
                    COD_CONCEPTO = ctx.documento.concepto,
                    USUARIO = ctx.usuario
                },
                tx);

            conn.Execute(@"
                update reg_creditos
                   set saldo = saldo - @saldo,
                       amortiza = amortiza + @saldo,
                       interesc = interesc + @intereses,
                       estado = 'A',
                       proceso = 'T',
                       fecha_enviaproceso = dbo.MyGetdate(),
                       observacion_proceso = @notas
                 where id_solicitud = @id_solicitud;",
                new
                {
                    saldo = ctx.baseDto.saldo,
                    intereses = ctx.baseDto.intereses,
                    notas = ctx.notas,
                    id_solicitud = ctx.data.id_solicitud
                },
                tx);
        }
        private void EjecutarAsientosPostCommit(SqlConnection conn, int codEmpresa, AplicacionContexto ctx)
        {
            var cuentasBase = ObtenerCuentasOperacionBase(conn, ctx.baseDto.linea, ctx.baseDto.opex);

            RegistrarAsientoCreditoBase(conn, ctx, ctx.baseDto.saldo, cuentasBase.cta_amortiza);

            if (ctx.baseDto.cargos_registrados > 0m)
            {
                string cuentaCargos = _mCobroDb.fxCBRParametro(codEmpresa, "23");
                RegistrarAsientoCreditoBase(conn, ctx, ctx.baseDto.cargos_registrados, cuentaCargos);
            }

            if (ctx.baseDto.cargos_registrados != 0m)
            {
                var cargos = conn.Query<AfectacionRow>(
                    "exec spCrdDocumentoAfectacionCargos @Tipo, @Transaccion",
                    new
                    {
                        Tipo = ctx.documento.tipo_documento,
                        Transaccion = ctx.documento.documento
                    }).ToList();

                foreach (var item in cargos)
                {
                    decimal movMonto = item.mov_monto == 0m
                        ? ctx.baseDto.cargos_registrados
                        : item.mov_monto * item.tipo_cambio_aplicado;

                    RegistrarAsientoAfectacion(conn, ctx, movMonto, new AsientoAfectacionData
                    {
                        divisa = item.cod_divisa,
                        tipoCambio = item.tipo_cambio_aplicado,
                        unidad = item.cod_unidad,
                        centroCosto = item.cod_centro_costo,
                        cuenta = item.cod_cuenta,
                        referencia1 = item.id_solicitud.ToString(CultureInfo.InvariantCulture),
                        referencia2 = item.codigo
                    });
                }
            }

            if (ctx.baseDto.polizas_atrasadas != 0m)
            {
                var polizas = conn.Query<AfectacionRow>(
                    "exec spCrdDocumentoAfectacionPolizas @Tipo, @Transaccion",
                    new
                    {
                        Tipo = ctx.documento.tipo_documento,
                        Transaccion = ctx.documento.documento
                    }).ToList();

                foreach (var item in polizas)
                {
                    decimal movMonto = item.mov_monto * item.tipo_cambio_aplicado;

                    RegistrarAsientoAfectacion(conn, ctx, movMonto, new AsientoAfectacionData
                    {
                        divisa = item.cod_divisa,
                        tipoCambio = item.tipo_cambio_aplicado,
                        unidad = item.cod_unidad,
                        centroCosto = item.cod_centro_costo,
                        cuenta = item.cod_cuenta,
                        referencia1 = item.id_solicitud.ToString(CultureInfo.InvariantCulture),
                        referencia2 = item.codigo
                    });
                }
            }

            RegistrarAsientoCreditoBase(conn, ctx, ctx.baseDto.interes_corriente, cuentasBase.cta_intc);
            RegistrarAsientoCreditoBase(conn, ctx, ctx.baseDto.interes_pendiente, cuentasBase.cta_intc);
            RegistrarAsientoCreditoBase(conn, ctx, ctx.baseDto.interes_moratorio, cuentasBase.cta_intm);
        }
        private static void RegistrarAsientoCreditoBase(SqlConnection conn, AplicacionContexto ctx, decimal monto, string cuenta)
        {
            if (monto <= 0m || string.IsNullOrWhiteSpace(cuenta))
                return;

            var idSolicitud = ctx.data.id_solicitud ?? throw new InvalidOperationException("id_solicitud no puede ser null");

            RegistrarAsiento(conn, null, new AsientoArgs
            {
                tipo = ctx.documento.tipo_documento,
                transaccion = ctx.documento.documento,
                monto = monto,
                movimiento = "C",
                divisa = ctx.oficina.cod_divisa,
                tipoCambio = 1m,
                contabilidad = ctx.contabilidad,
                unidad = ctx.oficina.cod_unidad,
                centroCosto = "",
                cuenta = cuenta,
                referencia1 = idSolicitud.ToString(CultureInfo.InvariantCulture),
                referencia2 = ctx.baseDto.linea,
                referencia3 = ctx.documento.deposito
            });
        }
        private static void RegistrarAsientoAfectacion(SqlConnection conn,AplicacionContexto ctx,decimal monto,AsientoAfectacionData data)
        {
            if (monto == 0m || string.IsNullOrWhiteSpace(data.cuenta))
                return;

            RegistrarAsiento(conn, null, new AsientoArgs
            {
                tipo = ctx.documento.tipo_documento,
                transaccion = ctx.documento.documento,
                monto = monto,
                movimiento = "C",
                divisa = data.divisa,
                tipoCambio = data.tipoCambio,
                contabilidad = ctx.contabilidad,
                unidad = data.unidad,
                centroCosto = data.centroCosto,
                cuenta = data.cuenta,
                referencia1 = data.referencia1,
                referencia2 = data.referencia2,
                referencia3 = ctx.documento.deposito
            });
        }
        private sealed class AsientoAfectacionData
        {
            public string divisa { get; set; } = "COL";
            public decimal tipoCambio { get; set; } = 1m;
            public string unidad { get; set; } = "";
            public string centroCosto { get; set; } = "";
            public string cuenta { get; set; } = "";
            public string referencia1 { get; set; } = "";
            public string referencia2 { get; set; } = "";
        }
        private static CuentasOperacionBaseRow ObtenerCuentasOperacionBase(SqlConnection conn, string linea, string opexTexto)
        {
            bool opex = (opexTexto ?? string.Empty).Trim().Equals("Sí", StringComparison.OrdinalIgnoreCase);

            string sql = opex
                ? @"select isnull(ctaointc,'') as cta_intc, isnull(ctaointm,'') as cta_intm, isnull(ctaoamort,'') as cta_amortiza from catalogo where codigo = @codigo;"
                : @"select isnull(ctanintc,'') as cta_intc, isnull(ctanintm,'') as cta_intm, isnull(ctanamort,'') as cta_amortiza from catalogo where codigo = @codigo;";

            return conn.QueryFirstOrDefault<CuentasOperacionBaseRow>(sql, new { codigo = linea }) ?? new CuentasOperacionBaseRow();
        }
        private static void RegistrarHistorialCobro(SqlConnection conn, AplicacionContexto ctx)
        {
            conn.Execute(
                "exec spCBRRegTransac @Tipo, @Cedula, @Operacion, @Notas, @Saldo, @IntCor, @IntMor, @Cargo, @Poliza, @Amortiza, @TipoDoc, @Documento, @Usuario",
                new
                {
                    Tipo = "01",
                    Cedula = ctx.baseDto.identificacion,
                    Operacion = ctx.data.id_solicitud,
                    Notas = ctx.notas,
                    Saldo = ctx.baseDto.saldo,
                    IntCor = ctx.baseDto.interes_corriente + ctx.baseDto.interes_pendiente,
                    IntMor = ctx.baseDto.interes_moratorio,
                    Cargo = ctx.baseDto.cargos_registrados,
                    Poliza = ctx.baseDto.polizas_atrasadas,
                    Amortiza = ctx.baseDto.principal_mora,
                    TipoDoc = ctx.documento.tipo_documento,
                    Documento = ctx.documento.documento,
                    Usuario = ctx.usuario
                });
        }
        private PrepararAplicacionResult PrepararAplicacion(SqlConnection conn, int codEmpresa, CoTrasladoDeudaAplicarRequest data)
        {
            long idSolicitud = data.id_solicitud ?? throw new InvalidOperationException("id_solicitud no puede ser null");

            var obtener = CO_TrasladoDeuda_Obtener(codEmpresa, idSolicitud);
            if (obtener.Code != 0 || obtener.Result == null)
            {
                return new PrepararAplicacionResult
                {
                    Error = DbHelper.CreateErrorResponse<CoTrasladoDeudaAplicarResponse>(
                        obtener.Description ?? "No fue posible obtener la información de la operación.")
                };
            }

            var baseDto = obtener.Result;

            var detalleCalculado = RecalcularDetalleInterno(new CoTrasladoDeudaCalcularRequest
            {
                id_solicitud = idSolicitud,
                plazo = data.plazo,
                tasa = data.tasa,
                total_deuda = baseDto.total_deuda,
                detalle = data.detalle ?? new List<CoTrasladoDeudaDetalleDto>()
            });

            if (Math.Round(detalleCalculado.porcentaje_asignado, 2) != 100m)
            {
                return new PrepararAplicacionResult
                {
                    Error = DbHelper.CreateErrorResponse<CoTrasladoDeudaAplicarResponse>(
                        "El porcentaje de asignación tiene que ser 100%.")
                };
            }

            return new PrepararAplicacionResult
            {
                Contexto = CrearContextoAplicacion(conn, codEmpresa, data, baseDto, detalleCalculado)
            };
        }
        private void ProcesarAplicacion(SqlConnection conn, SqlTransaction tx, AplicacionContexto ctx)
        {
            foreach (var item in ctx.detalle.detalle.Where(x => x.porcentaje > 0m))
            {
                ProcesarDetalleAplicacion(conn, tx, ctx, item);
            }

            if (ctx.sysPlanPagos == 1)
            {
                EjecutarFlujoPlanPagos(conn, tx, ctx);
                return;
            }

            EjecutarFlujoSinPlanPagos(conn, tx, ctx);
        }
        private void ProcesarDetalleAplicacion(SqlConnection conn, SqlTransaction tx, AplicacionContexto ctx, CoTrasladoDeudaDetalleDto item)
        {
            int plazo = ctx.data.plazo ?? throw new InvalidOperationException("plazo no puede ser null");
            decimal tasa = ctx.data.tasa ?? throw new InvalidOperationException("tasa no puede ser null");
            long idSolicitud = ctx.data.id_solicitud ?? throw new InvalidOperationException("id_solicitud no puede ser null");

            int opexItem = ResolverOpexItem(item);

            long nuevaOperacion = InsertarNuevaOperacion(
                conn,
                tx,
                new InsertNuevaOperacionArgs
                {
                    lineaNueva = ctx.baseDto.linea_cobro,
                    identificacion = item.identificacion,
                    monto = item.monto,
                    plazo = plazo,
                    tasa = tasa,
                    priDeduc = ctx.priDeduc,
                    referencia = idSolicitud,
                    usuario = ctx.usuario,
                    fecha = ctx.fecha,
                    opex = opexItem,
                    fechaProceso = ObtenerFechaProcesoActual(conn, tx),
                    notas = ctx.notas,
                    tbpPuntosAdd = ctx.baseDto.tbp_puntos_add,
                    liqTasa = ctx.baseDto.liq_tasa,
                    codOficinaR = ctx.oficina.cod_oficina,
                    codOficinaF = ctx.oficina.oficina_titular,
                    baseCalculo = ctx.oficina.base_calculo,
                    codDivisa = ctx.oficina.cod_divisa,
                    diaPago = ctx.oficina.dia_pago,
                    cuota = item.cuota
                });

            RegistrarPlanPagosOperacion(conn, tx, ctx, nuevaOperacion);

            string cuentaNueva = opexItem == 1
                ? ctx.cuentasNuevaLinea.cta_o_amort
                : ctx.cuentasNuevaLinea.cta_n_amort;

            RegistrarAsiento(
                conn,
                tx,
                new AsientoArgs
                {
                    tipo = ctx.documento.tipo_documento,
                    transaccion = ctx.documento.documento,
                    monto = item.monto,
                    movimiento = "D",
                    divisa = ctx.oficina.cod_divisa,
                    tipoCambio = 1m,
                    contabilidad = ctx.contabilidad,
                    unidad = ctx.oficina.cod_unidad,
                    centroCosto = "",
                    cuenta = cuentaNueva,
                    referencia1 = nuevaOperacion.ToString(CultureInfo.InvariantCulture),
                    referencia2 = ctx.baseDto.linea_cobro,
                    referencia3 = ctx.documento.deposito
                });
        }
        private static void RegistrarPlanPagosOperacion(SqlConnection conn,SqlTransaction tx,AplicacionContexto ctx,long nuevaOperacion)
        {
            if (ctx.sysPlanPagos != 1)
                return;

            conn.Execute(
                "exec spCrdPlanPagos @Operacion",
                new { Operacion = nuevaOperacion },
                tx);
        }
        private void EjecutarPostAplicacion(SqlConnection conn, int codEmpresa, AplicacionContexto ctx)
        {
            EnsureConnectionOpen(conn);

            EjecutarAsientosPostCommit(conn, codEmpresa, ctx);
            RegistrarHistorialCobro(conn, ctx);

            Bitacora(new BitacoraInsertarDto
            {
                EmpresaId = codEmpresa,
                Usuario = ctx.usuario,
                DetalleMovimiento = $"Traspaso de Deudas de la Operación: {ctx.data.id_solicitud}",
                Movimiento = "Aplica - WEB",
                Modulo = vModulo
            });
        }
        private static void EnsureConnectionOpen(SqlConnection conn)
        {
            if (conn.State != System.Data.ConnectionState.Open)
                conn.Open();
        }
        private ErrorDto<CoTrasladoDeudaAplicarResponse>? ValidarRecibo(int codEmpresa, AplicacionContexto ctx)
        {
            var recibo = _mRecibosDb.sbImprimeRecibo(
                codEmpresa,
                ctx.documento.documento,
                ctx.documento.tipo_documento,
                ctx.usuario
            );

            if (recibo.Code == -1)
            {
                return DbHelper.CreateErrorResponse<CoTrasladoDeudaAplicarResponse>(
                    recibo.Description ?? "Error al generar el recibo.");
            }

            return null;
        }
        private static void CompletarRespuestaAplicacion(CoTrasladoDeudaAplicarResponse response,AplicacionContexto ctx)
        {
            response.tipo_documento = ctx.documento.tipo_documento;
            response.documento = ctx.documento.documento;
            response.total_aplicado = ctx.detalle.total_deuda;
            response.mensaje =
                $"Traspaso de Deudas realizado satisfactoriamente. Nota de cobro: {ctx.documento.tipo_documento}-{ctx.documento.documento}";
        }
        private static void RollbackTransaction(SqlTransaction? tx)
        {
            if (tx == null)
                return;

            tx.Rollback();
            tx.Dispose();
        }
        private sealed class PrepararAplicacionResult
        {
            public AplicacionContexto? Contexto { get; set; }
            public ErrorDto<CoTrasladoDeudaAplicarResponse>? Error { get; set; }
        }
        private sealed class CabeceraOperacionRow
        {
            public string cedula { get; set; } = "";
            public string nombre { get; set; } = "";
            public decimal saldo { get; set; } = 0m;
            public string proceso { get; set; } = "";
            public decimal tasa { get; set; } = 0m;
            public int plazo { get; set; } = 0;
            public decimal tasa_original { get; set; } = 0m;
            public string codigo { get; set; } = "";
            public string descripcion { get; set; } = "";
            public int liq_tasa { get; set; } = 0;
            public int opex { get; set; } = 0;
            public string divisa { get; set; } = "";
            public int num_fiadores { get; set; } = 0;
            public decimal tbp_puntos_add { get; set; } = 0m;
            public decimal mora_intc { get; set; } = 0m;
            public decimal mora_intm { get; set; } = 0m;
            public decimal mora_amortiza { get; set; } = 0m;
            public decimal cargos { get; set; } = 0m;
            public decimal interes_total { get; set; } = 0m;
            public decimal poliza { get; set; } = 0m;
        }
        private sealed class ResumenDeudaRow
        {
            public decimal interes_corriente { get; set; }
            public decimal interes_moratorio { get; set; }
            public decimal principal_mora { get; set; }
            public decimal interes_pendiente { get; set; }
            public decimal cargos { get; set; }
            public decimal poliza { get; set; }
        }
        private sealed class PlanPagoCancelacionRow
        {
            public decimal int_cor { get; set; } = 0m;
            public decimal int_mor { get; set; } = 0m;
            public decimal cargos { get; set; } = 0m;
            public decimal poliza { get; set; } = 0m;
        }
        private sealed class OficinaContexto
        {
            public string cod_oficina { get; set; } = "";
            public string cod_unidad { get; set; } = "";
            public string cod_centro_costo { get; set; } = "";
            public string cod_divisa { get; set; } = "COL";
            public int dia_pago { get; set; } = 32;
            public string base_calculo { get; set; } = "01";
            public string oficina_titular { get; set; } = "";
        }
        private sealed class SpOperacionCtasRow
        {
            public string cod_oficina_r { get; set; } = "";
            public string cod_unidad { get; set; } = "";
            public string cod_centro_costo { get; set; } = "";
        }
        private sealed class DocumentoContexto
        {
            public string tipo_documento { get; set; } = "";
            public string documento { get; set; } = "";
            public string concepto { get; set; } = "";
            public string detalle { get; set; } = "";
            public string deposito { get; set; } = "";
        }
        private sealed class PriDeducRow
        {
            public long pri_deduc { get; set; } = 0;
            public long cod_deductora { get; set; } = 0;
        }
        private sealed class CuentasNuevaLineaRow
        {
            public string cta_n_amort { get; set; } = "";
            public string cta_o_amort { get; set; } = "";
        }
        private sealed class InsertNuevaOperacionArgs
        {
            public string lineaNueva { get; set; } = "";
            public string identificacion { get; set; } = "";
            public decimal monto { get; set; }
            public int plazo { get; set; }
            public decimal tasa { get; set; }
            public long priDeduc { get; set; }
            public long referencia { get; set; }
            public string usuario { get; set; } = "";
            public DateTime fecha { get; set; }
            public int opex { get; set; }
            public decimal fechaProceso { get; set; }
            public string notas { get; set; } = "";
            public decimal tbpPuntosAdd { get; set; }
            public int liqTasa { get; set; }
            public string codOficinaR { get; set; } = "";
            public string codOficinaF { get; set; } = "";
            public string baseCalculo { get; set; } = "01";
            public string codDivisa { get; set; } = "COL";
            public int diaPago { get; set; } = 32;
            public decimal cuota { get; set; }
        }
        private sealed class CuentasOperacionBaseRow
        {
            public string cta_intc { get; set; } = "";
            public string cta_intm { get; set; } = "";
            public string cta_amortiza { get; set; } = "";
        }
        private sealed class AfectacionRow
        {
            public long id_solicitud { get; set; } = 0;
            public string codigo { get; set; } = "";
            public decimal mov_monto { get; set; } = 0m;
            public string cod_divisa { get; set; } = "COL";
            public decimal tipo_cambio { get; set; } = 1m;
            public string cod_unidad { get; set; } = "";
            public string cod_centro_costo { get; set; } = "";
            public string cod_cuenta { get; set; } = "";

            public decimal tipo_cambio_aplicado => tipo_cambio <= 0m ? 1m : tipo_cambio;
        }
        private sealed class AsientoArgs
        {
            public string tipo { get; set; } = "";
            public string transaccion { get; set; } = "";
            public decimal monto { get; set; }
            public string movimiento { get; set; } = "";
            public string divisa { get; set; } = "";
            public decimal tipoCambio { get; set; }
            public int contabilidad { get; set; }
            public string unidad { get; set; } = "";
            public string centroCosto { get; set; } = "";
            public string cuenta { get; set; } = "";
            public string referencia1 { get; set; } = "";
            public string referencia2 { get; set; } = "";
            public string referencia3 { get; set; } = "";
        }
        private sealed class AplicacionContexto
        {
            public CoTrasladoDeudaAplicarRequest data { get; set; } = new();
            public CoTrasladoDeudaObtenerDto baseDto { get; set; } = new();
            public CoTrasladoDeudaCalcularResponse detalle { get; set; } = new();
            public string usuario { get; set; } = "";
            public string notas { get; set; } = "";
            public DateTime fecha { get; set; }
            public int contabilidad { get; set; }
            public int sysPlanPagos { get; set; }
            public OficinaContexto oficina { get; set; } = new();
            public DocumentoContexto documento { get; set; } = new();
            public CuentasNuevaLineaRow cuentasNuevaLinea { get; set; } = new();
            public long priDeduc { get; set; }
        }
    }
}