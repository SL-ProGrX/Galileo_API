using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX.Creditos;
using Dapper;

namespace Galileo_API.DataBaseTier.ProGrX.Creditos
{
    public class FrmCrCarteraSensibilizacionDb
    {
        private readonly PortalDB _portalDb;
        private readonly MProGrxMain _mProGrxMain;

        public FrmCrCarteraSensibilizacionDb(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
            _mProGrxMain = new MProGrxMain(config);
        }

        /// <summary>
        /// Obtiene la configuracion inicial de pantalla.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto<CrCarteraSensibilizacionPantallaData> CrCarteraSensibilizacion_Pantalla_Obtener(
            int codEmpresa,
            string usuario)
        {
            usuario = NormalizarTexto(usuario);

            if (string.IsNullOrWhiteSpace(usuario))
            {
                return DbHelper.CreateErrorResponse(
                    "Debe indicar el usuario.",
                    -2,
                    new CrCarteraSensibilizacionPantallaData());
            }

            var globalesResp = _mProGrxMain.sbSifParametrosInicializa(codEmpresa, usuario);
            if (globalesResp.Code != 0 || globalesResp.Result is null)
            {
                return DbHelper.CreateErrorResponse(
                    globalesResp.Description ?? "No fue posible obtener los parámetros globales.",
                    globalesResp.Code.GetValueOrDefault(-1),
                    new CrCarteraSensibilizacionPantallaData());
            }

            DateTime fechaServidor = globalesResp.Result.fxFechaServidor ?? DateTime.Now;

            var institucionesResp = ObtenerInstituciones(codEmpresa);
            if (institucionesResp.Code != 0)
            {
                return DbHelper.CreateErrorResponse(
                    institucionesResp.Description ?? "No fue posible obtener las instituciones.",
                    institucionesResp.Code.GetValueOrDefault(-1),
                    new CrCarteraSensibilizacionPantallaData());
            }

            var recursosResp = ObtenerRecursosTodos(codEmpresa);
            if (recursosResp.Code != 0)
            {
                return DbHelper.CreateErrorResponse(
                    recursosResp.Description ?? "No fue posible obtener los recursos.",
                    recursosResp.Code.GetValueOrDefault(-1),
                    new CrCarteraSensibilizacionPantallaData());
            }

            var destinosResp = ObtenerDestinosTodos(codEmpresa);
            if (destinosResp.Code != 0)
            {
                return DbHelper.CreateErrorResponse(
                    destinosResp.Description ?? "No fue posible obtener los destinos.",
                    destinosResp.Code.GetValueOrDefault(-1),
                    new CrCarteraSensibilizacionPantallaData());
            }

            return DbHelper.CreateOkResponse(new CrCarteraSensibilizacionPantallaData
            {
                instituciones = institucionesResp.Result ?? new(),
                recursos = recursosResp.Result ?? new(),
                destinos = destinosResp.Result ?? new(),
                fecha_inicio_default = fechaServidor.Date,
                fecha_corte_default = fechaServidor.Date,
                todas_lineas_default = false,
                todas_fechas_default = false,
                filtros_add_default = false,
                tbp_pts_add_default = false,
                pts_add_default = 0m
            });
        }

        /// <summary>
        /// Obtiene recursos y destinos segun el codigo y la opcion de todas las lineas
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codigo"></param>
        /// <param name="todasLineas"></param>
        /// <returns></returns>
        public ErrorDto<CrCarteraSensibilizacionPantallaData> CrCarteraSensibilizacion_Linea_Combos_Obtener(
            int codEmpresa,
            string codigo,
            bool todasLineas)
        {
            codigo = NormalizarTexto(codigo);

            ErrorDto<List<DropDownListaGenericaModel>> recursosResp = todasLineas
                ? ObtenerRecursosTodos(codEmpresa)
                : ObtenerRecursosPorCodigo(codEmpresa, codigo);

            if (recursosResp.Code != 0)
            {
                return DbHelper.CreateErrorResponse(
                    recursosResp.Description ?? "No fue posible obtener los recursos.",
                    recursosResp.Code.GetValueOrDefault(-1),
                    new CrCarteraSensibilizacionPantallaData());
            }

            ErrorDto<List<DropDownListaGenericaModel>> destinosResp = todasLineas
                ? ObtenerDestinosTodos(codEmpresa)
                : ObtenerDestinosPorCodigo(codEmpresa, codigo);

            if (destinosResp.Code != 0)
            {
                return DbHelper.CreateErrorResponse(
                    destinosResp.Description ?? "No fue posible obtener los destinos.",
                    destinosResp.Code.GetValueOrDefault(-1),
                    new CrCarteraSensibilizacionPantallaData());
            }

            return DbHelper.CreateOkResponse(new CrCarteraSensibilizacionPantallaData
            {
                recursos = recursosResp.Result ?? new(),
                destinos = destinosResp.Result ?? new()
            });
        }

        /// <summary>
        /// Obtiene el catalogo de lineas para busqueda de codigo
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CrCarteraSensibilizacion_Catalogo_Obtener(
            int codEmpresa)
        {
            const string sql = @"
                select
                    rtrim(codigo) as item,
                    descripcion
                from catalogo
                order by codigo;";

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                        _portalDb,
                        codEmpresa,
                        sql);
        }

        /// <summary>
        /// Consulta la cartera sensibilizada segun filtros
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<CrCarteraSensibilizacionResultadoData> CrCarteraSensibilizacion_Buscar(
            int codEmpresa,
            CrCarteraSensibilizacionRequest request)
        {
            var validacion = ValidarBusqueda(request);
            if (validacion.Code != 0)
            {
                return DbHelper.CreateErrorResponse(
                    validacion.Description ?? "Filtros inválidos.",
                    validacion.Code.GetValueOrDefault(-2),
                    new CrCarteraSensibilizacionResultadoData());
            }

            string sql = ConstruirSqlBusqueda(request);
            object parametros = ConstruirParametrosBusqueda(request);

            var baseResp = DbHelper.ExecuteListQuery<CrCarteraSensibilizacionOperacionBase>(
                _portalDb,
                codEmpresa,
                sql,
                parametros);

            if (baseResp.Code != 0)
            {
                return DbHelper.CreateErrorResponse(
                    baseResp.Description ?? "No fue posible obtener la cartera.",
                    baseResp.Code.GetValueOrDefault(-1),
                    new CrCarteraSensibilizacionResultadoData());
            }

            decimal puntos = request.pts_add ?? 0m;
            List<CrCarteraSensibilizacionGridItem> detalle = (baseResp.Result ?? new())
                .Select(x => MapearResultado(x, puntos))
                .ToList();

            return DbHelper.CreateOkResponse(new CrCarteraSensibilizacionResultadoData
            {
                detalle = detalle,
                casos = detalle.Count,
                cuotas_actuales = detalle.Sum(x => x.cuota),
                cuotas_nuevas = detalle.Sum(x => x.cuota_01)
            });
        }

        /// <summary>
        /// Genera la informacion persistida de sensibilizacion
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<CrCarteraSensibilizacionGenerarData> CrCarteraSensibilizacion_Generar(
            int codEmpresa,
            CrCarteraSensibilizacionResultadoData request)
        {
            if (request.detalle is null || request.detalle.Count == 0)
            {
                return DbHelper.CreateErrorResponse(
                    "No existen datos para generar.",
                    -2,
                    new CrCarteraSensibilizacionGenerarData());
            }

            try
            {
                using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);
                conn.Open();

                using var tx = conn.BeginTransaction();

                conn.Execute("delete CRD_SENSIBILIZA_PF", transaction: tx);
                conn.Execute("delete CRD_SENSIBILIZA_PL", transaction: tx);

                const string sqlInsert = @"
                    insert into CRD_SENSIBILIZA_PF
                    (
                        id_solicitud,
                        cuota_01,
                        cuota_02,
                        cuota_03,
                        cuota_04,
                        tasa_01,
                        tasa_02,
                        tasa_03,
                        tasa_04
                    )
                    values
                    (
                        @operacion,
                        @cuota_01,
                        @cuota_02,
                        @cuota_03,
                        @cuota_04,
                        @tasa_01,
                        @tasa_02,
                        @tasa_03,
                        @tasa_04
                    );";

                conn.Execute(sqlInsert, request.detalle, tx);
                tx.Commit();

                return DbHelper.CreateOkResponse(new CrCarteraSensibilizacionGenerarData
                {
                    registros_generados = request.detalle.Count
                });
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse(
                    ex.Message,
                    -1,
                    new CrCarteraSensibilizacionGenerarData());
            }
        }

        /// <summary>
        /// Obtiene la tabla de liquidez generada para sensibilizacion
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<CrCarteraSensibilizacionLiquidezItem>> CrCarteraSensibilizacion_Liquidez_Obtener(
            int codEmpresa)
        {
            const string sql = @"
            select
                R.cedula as cedula,
                sum(S.cuota_01) as cuota_01,
                sum(S.cuota_02) as cuota_02,
                sum(S.cuota_03) as cuota_03,
                sum(S.cuota_04) as cuota_04,
                isnull(L.devengado_mes, 0) as devengado_mes,
                isnull(L.liquidez_simple, 0) as liquidez_simple,
                isnull(L.liquidez_confianza, 0) as liquidez_confianza,
                isnull(L.total_carga_ccss, 0) as total_carga_ccss,
                isnull(
                    L.deducciones
                    - (
                        isnull(L.refundiciones_cuota, 0)
                        + isnull(L.desembolsos_cuota, 0)
                        + isnull(L.crd_transito_cancelados, 0)
                    ),
                    0
                ) as deducciones,
                isnull(dbo.fxCRD_Sensilidad(R.cedula, 1, 'F'), 0) as saldo_fijo,
                isnull(dbo.fxCRD_Sensilidad(R.cedula, 0, 'F'), 0) as cuota_fija
            from CRD_SENSIBILIZA_PF S
            inner join REG_CREDITOS R on S.ID_SOLICITUD = R.ID_SOLICITUD
            inner join CRD_SENSIBILIZA_LIQ L on R.CEDULA = L.CEDULA
            group by
                R.cedula,
                L.devengado_mes,
                L.liquidez_simple,
                L.liquidez_confianza,
                L.total_carga_ccss,
                L.deducciones,
                L.refundiciones_cuota,
                L.desembolsos_cuota,
                L.crd_transito_cancelados
            order by R.cedula;";

            return DbHelper.ExecuteListQuery<CrCarteraSensibilizacionLiquidezItem>(
                _portalDb,
                codEmpresa,
                sql);
        }

        private ErrorDto<List<DropDownListaGenericaModel>> ObtenerInstituciones(int codEmpresa)
        {
            const string sql = @"
                select item, descripcion
                from
                (
                    select 0 as orden, '0' as item, 'TODOS' as descripcion
                    union all
                    select 1 as orden, cast(cod_institucion as varchar(20)) as item, rtrim(descripcion) as descripcion
                    from instituciones
                ) x
                order by x.orden, x.descripcion;";

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(_portalDb, codEmpresa, sql);
        }

        private ErrorDto<List<DropDownListaGenericaModel>> ObtenerRecursosTodos(int codEmpresa)
        {
            const string sql = @"
                select item, descripcion
                from
                (
                    select 0 as orden, 'TODOS' as item, 'TODOS' as descripcion
                    union all
                    select 1 as orden,
                           rtrim(cod_grupo) as item,
                           rtrim(cod_grupo) + ' - ' + rtrim(descripcion) as descripcion
                    from catalogo_grupos
                ) x
                order by x.orden, x.descripcion;";

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(_portalDb, codEmpresa, sql);
        }

        private ErrorDto<List<DropDownListaGenericaModel>> ObtenerDestinosTodos(int codEmpresa)
        {
            const string sql = @"
                select item, descripcion
                from
                (
                    select 0 as orden, 'TODOS' as item, 'TODOS' as descripcion
                    union all
                    select 1 as orden,
                           rtrim(cod_destino) as item,
                           rtrim(cod_destino) + ' - ' + rtrim(descripcion) as descripcion
                    from catalogo_destinos
                ) x
                order by x.orden, x.descripcion;";

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(_portalDb, codEmpresa, sql);
        }

        private ErrorDto<List<DropDownListaGenericaModel>> ObtenerRecursosPorCodigo(int codEmpresa, string codigo)
        {
            const string sql = @"
                select
                    rtrim(R.cod_grupo) as item,
                    rtrim(R.cod_grupo) + ' - ' + rtrim(R.descripcion) as descripcion
                from catalogo_grupos R
                inner join catalogo_AsignaGrp A on R.cod_grupo = A.cod_grupo
                where A.codigo = @Codigo
                order by descripcion;";

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                _portalDb,
                codEmpresa,
                sql,
                new { Codigo = codigo });
        }

        private ErrorDto<List<DropDownListaGenericaModel>> ObtenerDestinosPorCodigo(int codEmpresa, string codigo)
        {
            const string sql = @"
                select
                    rtrim(R.cod_destino) as item,
                    rtrim(R.cod_destino) + ' - ' + rtrim(R.descripcion) as descripcion
                from catalogo_destinos R
                inner join catalogo_destinosAsg A on R.cod_destino = A.cod_destino
                where A.codigo = @Codigo
                order by descripcion;";

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                _portalDb,
                codEmpresa,
                sql,
                new { Codigo = codigo });
        }

        private static ErrorDto ValidarBusqueda(CrCarteraSensibilizacionRequest request)
        {
            request.codigo = NormalizarTexto(request.codigo);
            request.destino = NormalizarTexto(request.destino);
            request.recurso = NormalizarTexto(request.recurso);

            if (!request.todas_lineas && string.IsNullOrWhiteSpace(request.codigo))
            {
                return DbHelper.ErrorResponse("Debe indicar el código cuando no aplica a todas las líneas.", -2);
            }

            if (!request.todas_fechas)
            {
                if (!request.fecha_inicio.HasValue || !request.fecha_corte.HasValue)
                {
                    return DbHelper.ErrorResponse("Debe indicar fecha inicio y fecha corte.", -2);
                }

                if (request.fecha_corte.Value.Date < request.fecha_inicio.Value.Date)
                {
                    return DbHelper.ErrorResponse("La fecha corte no puede ser menor que la fecha inicio.", -2);
                }
            }

            if (request.usar_plazos && (!request.plazo_inicio.HasValue || !request.plazo_corte.HasValue))
            {
                return DbHelper.ErrorResponse("Debe indicar el rango de plazos.", -2);
            }

            if (request.usar_tasas && (!request.tasa_inicio.HasValue || !request.tasa_corte.HasValue))
            {
                return DbHelper.ErrorResponse("Debe indicar el rango de tasas.", -2);
            }

            return DbHelper.CreateOkResponse();
        }

        private static string ConstruirSqlBusqueda(CrCarteraSensibilizacionRequest request)
        {
            var sql = @"
                select Top 100
                    R.id_solicitud,
                    R.cedula,
                    S.nombre,
                    R.montoapr,
                    R.Saldo - isnull(V.amortiza,0) as saldo,
                    R.cuota,
                    R.plazo,
                    R.interesv,
                    R.prideduc,
                    R.codigo,
                    R.fechaforp,
                    R.int as tasaoriginal,
                    C.Liq_Valor,
                    R.plazo + DATEDIFF(mm, dbo.MyGetdate(),
                        CONVERT(DATETIME,
                            substring(convert(varchar(6), R.prideduc), 1,4) + '/' +
                            substring(convert(varchar(6), R.prideduc), 5,2) + '/28'
                        )
                    ) as plazofaltante,
                    isnull(R.liqTasa,0) as liqtasa,
                    isnull(R.TBP_PuntosAdd,0) as tbp_puntosadd,
                    isnull(R.Tasa_Piso,0) as tasa_piso
                from socios S
                inner join reg_creditos R on S.cedula = R.cedula
                inner join catalogo C on R.codigo = C.codigo
                left join vista_morosidad V on R.id_solicitud = V.id_solicitud
                where R.estado = 'A'
                  and R.proceso = 'N'
                  and R.saldo > 0";

            if (!request.todas_lineas)
            {
                sql += " and R.codigo = @Codigo";
            }

            if (!string.Equals(request.destino, "TODOS", StringComparison.OrdinalIgnoreCase) &&
                !string.IsNullOrWhiteSpace(request.destino))
            {
                sql += " and R.cod_destino = @Destino";
            }

            if (!string.Equals(request.recurso, "TODOS", StringComparison.OrdinalIgnoreCase) &&
                !string.IsNullOrWhiteSpace(request.recurso))
            {
                sql += " and R.cod_grupo = @Recurso";
            }

            if (request.cod_institucion > 0)
            {
                sql += " and S.cod_institucion = @CodInstitucion";
            }

            if (request.usar_plazos)
            {
                sql += " and R.plazo between @PlazoInicio and @PlazoCorte";
            }

            if (request.usar_tasas)
            {
                sql += " and R.interesv between @TasaInicio and @TasaCorte";
            }

            if (!request.todas_fechas)
            {
                sql += " and R.fechaforp between @FechaInicio and @FechaCorte";
            }

            if (request.aplicar_tbp_pts_add)
            {
                sql += " and R.TBP_PuntosAdd is not null";
            }
            else
            {
                sql += " and R.TBP_PuntosAdd is null";
            }

            return sql;
        }

        private static object ConstruirParametrosBusqueda(CrCarteraSensibilizacionRequest request)
        {
            return new
            {
                Codigo = request.codigo,
                Destino = request.destino,
                Recurso = request.recurso,
                CodInstitucion = request.cod_institucion,
                PlazoInicio = request.plazo_inicio,
                PlazoCorte = request.plazo_corte,
                TasaInicio = request.tasa_inicio,
                TasaCorte = request.tasa_corte,
                FechaInicio = request.fecha_inicio?.Date,
                FechaCorte = request.fecha_corte?.Date.AddHours(23).AddMinutes(59).AddSeconds(59)
            };
        }

        private static CrCarteraSensibilizacionGridItem MapearResultado(
            CrCarteraSensibilizacionOperacionBase row,
            decimal puntos)
        {
            decimal tasa1 = AplicarPiso(row.interesv + (puntos * 1), row.tasa_piso);
            decimal tasa2 = AplicarPiso(row.interesv + (puntos * 2), row.tasa_piso);
            decimal tasa3 = AplicarPiso(row.interesv + (puntos * 3), row.tasa_piso);
            decimal tasa4 = AplicarPiso(row.interesv + (puntos * 4), row.tasa_piso);

            return new CrCarteraSensibilizacionGridItem
            {
                operacion = row.id_solicitud,
                codigo = row.codigo,
                cedula = row.cedula,
                nombre = row.nombre,
                montoapr = row.montoapr,
                saldo = row.saldo,
                plazo = row.plazo,
                interesv = row.interesv,
                cuota = row.cuota,
                fechaforp = row.fechaforp,
                tasa_original = row.tasaoriginal,
                tbp_puntos_add = row.tbp_puntosadd,
                tasa_piso = row.tasa_piso,
                plazo_faltante = row.plazofaltante,
                cuota_01 = FxCalculaCuota(row.saldo, row.plazofaltante, tasa1),
                cuota_02 = FxCalculaCuota(row.saldo, row.plazofaltante, tasa2),
                cuota_03 = FxCalculaCuota(row.saldo, row.plazofaltante, tasa3),
                cuota_04 = FxCalculaCuota(row.saldo, row.plazofaltante, tasa4),
                tasa_01 = tasa1,
                tasa_02 = tasa2,
                tasa_03 = tasa3,
                tasa_04 = tasa4
            };
        }

        private static decimal AplicarPiso(decimal tasa, decimal tasaPiso)
        {
            return tasa < tasaPiso ? tasaPiso : tasa;
        }

        private static decimal FxCalculaCuota(decimal saldo, int plazoFaltante, decimal tasa)
        {
            if (saldo <= 0 || plazoFaltante <= 0)
            {
                return 0m;
            }

            decimal tasaMensual = (tasa / 100m) / 12m;
            if (tasaMensual <= 0)
            {
                return Math.Round(saldo / plazoFaltante, 2);
            }

            decimal potencia = (decimal)Math.Pow((double)(1 + tasaMensual), plazoFaltante);
            decimal cuota = saldo * ((tasaMensual * potencia) / (potencia - 1));

            return Math.Round(cuota, 2);
        }

        private static string NormalizarTexto(string? valor)
        {
            return (valor ?? string.Empty).Trim();
        }
    }
}