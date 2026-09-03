using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_Conciliacion;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Galileo_API.DataBaseTier.ProGrX.Conciliacion
{
    public class FrmVerificaSaldosASEDB
    {
        private const string TipoActual = "A";
        private const string TipoHistorico = "H";
        private const int CommandTimeoutSeconds = 360;
        private const string MensajeTipoBusqueda =
            "El tipo de búsqueda indicado no es válido.";
        private const string MensajePeriodoRequerido =
            "Debe seleccionar un período histórico válido.";
        private const string MensajePeriodoNoExiste =
            "El período histórico seleccionado no existe.";
        private const string MensajeFechaCorte =
            "No se encontró una fecha de corte válida en el parámetro 16.";

        private static readonly IReadOnlyDictionary<string, int> SortFields =
            new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
            {
                ["id_solicitud"] = 1,
                ["codigo"] = 2,
                ["cedula"] = 3,
                ["nombre"] = 4,
                ["saldo_inicial"] = 5,
                ["saldo_final"] = 6,
                ["debitos"] = 7,
                ["creditos"] = 8,
                ["diferencia"] = 9
            };

        private const string SqlConsulta = """
            set nocount on;

            declare @TotalEvaluados int = 0;
            declare @TotalDiferencias int = 0;

            with CreditosMovimientos as
            (
                select
                    ID_SOLICITUD,
                    sum(
                        case
                            when TCON = '8'
                                then isnull(AMORTIZA, 0)
                            else 0
                        end
                    ) as debitos,
                    sum(
                        case
                            when TCON = '8'
                                then 0
                            else isnull(AMORTIZA, 0)
                        end
                    ) as creditos
                from CREDITOS_DT
                where @TipoBusqueda = 'A'
                  and FECHAS >= @FechaCorte
                  and FECHAS <= @FechaFin
                group by ID_SOLICITUD
            ),
            MorosidadMovimientos as
            (
                select
                    ID_SOLICITUD,
                    sum(
                        case
                            when TCON = '8'
                                then isnull(ABAMORTIZA, 0)
                            else 0
                        end
                    ) as debitos,
                    sum(
                        case
                            when TCON = '8'
                                then 0
                            else isnull(ABAMORTIZA, 0)
                        end
                    ) as creditos
                from MOROSIDAD
                where @TipoBusqueda = 'A'
                  and FECULT >= @FechaCorte
                  and FECULT <= @FechaActual
                  and ESTADO <> 'A'
                group by ID_SOLICITUD
            ),
            ResultadosBase as
            (
                select
                    cast(R.ID_SOLICITUD as bigint) as id_solicitud,
                    rtrim(R.CODIGO) as codigo,
                    rtrim(S.CEDULA) as cedula,
                    rtrim(isnull(S.NOMBRE, '')) as nombre,
                    cast(R.SALDO_INICIAL as decimal(18, 2))
                        as saldo_inicial,
                    cast(R.SALDO as decimal(18, 2))
                        as saldo_final,
                    cast(
                        isnull(CD.debitos, 0)
                        + isnull(MD.debitos, 0)
                        as decimal(18, 2)
                    ) as debitos,
                    cast(
                        isnull(CD.creditos, 0)
                        + isnull(MD.creditos, 0)
                        as decimal(18, 2)
                    ) as creditos
                from REG_CREDITOS R
                inner join SOCIOS S
                    on R.CEDULA = S.CEDULA
                inner join CATALOGO C
                    on R.CODIGO = C.CODIGO
                left join CreditosMovimientos CD
                    on R.ID_SOLICITUD = CD.ID_SOLICITUD
                left join MorosidadMovimientos MD
                    on R.ID_SOLICITUD = MD.ID_SOLICITUD
                where @TipoBusqueda = 'A'
                  and R.SALDO is not null
                  and R.SALDO <> R.SALDO_INICIAL
                  and R.ESTADO in ('A', 'C')
                  and C.RETENCION = 'N'
                  and C.POLIZA = 'N'
                  and R.MONTOAPR > R.SALDO
                  and (
                        @ExcluirOperacionesNuevas = 0
                        or R.SALDO_INICIAL <> 0
                      )

                union all

                select
                    cast(H.ID_SOLICITUD as bigint) as id_solicitud,
                    rtrim(H.CODIGO) as codigo,
                    rtrim(S.CEDULA) as cedula,
                    rtrim(isnull(S.NOMBRE, '')) as nombre,
                    cast(H.SALDO_INICIAL as decimal(18, 2))
                        as saldo_inicial,
                    cast(H.SALDO_FINAL as decimal(18, 2))
                        as saldo_final,
                    cast(H.TOTAL_DEBITOS as decimal(18, 2))
                        as debitos,
                    cast(H.TOTAL_CREDITOS as decimal(18, 2))
                        as creditos
                from ASE_PER_CERRADOS H
                inner join ASE_PER_HISTORICO P
                    on P.ID_PER_HISTORICO = @IdPerHistorico
                   and H.ANIO = P.ANIO
                   and H.MES = P.MES
                inner join REG_CREDITOS R
                    on H.ID_SOLICITUD = R.ID_SOLICITUD
                inner join SOCIOS S
                    on R.CEDULA = S.CEDULA
                inner join CATALOGO C
                    on R.CODIGO = C.CODIGO
                where @TipoBusqueda = 'H'
                  and H.SALDO_FINAL <>
                      (
                          H.SALDO_INICIAL
                          + H.TOTAL_DEBITOS
                          - H.TOTAL_CREDITOS
                      )
                  and C.POLIZA = 'N'
                  and C.RETENCION = 'N'
                  and (
                        @ExcluirOperacionesNuevas = 0
                        or H.SALDO_INICIAL <> 0
                      )
            )
            select
                id_solicitud,
                codigo,
                cedula,
                nombre,
                saldo_inicial,
                saldo_final,
                debitos,
                creditos
            into #Evaluados
            from ResultadosBase
            option (recompile);

            select @TotalEvaluados = count(1)
            from #Evaluados;

            select
                E.id_solicitud,
                E.codigo,
                E.cedula,
                E.nombre,
                E.saldo_inicial,
                E.saldo_final,
                E.debitos,
                E.creditos,
                D.diferencia
            into #Diferencias
            from #Evaluados E
            cross apply
            (
                values
                (
                    cast(
                        E.saldo_final
                        - (
                            E.saldo_inicial
                            + E.debitos
                            - E.creditos
                        )
                        as decimal(18, 2)
                    )
                )
            ) D(diferencia)
            where abs(D.diferencia) > 1;

            select @TotalDiferencias = count(1)
            from #Diferencias;

            select
                id_solicitud,
                codigo,
                cedula,
                nombre,
                saldo_inicial,
                saldo_final,
                debitos,
                creditos,
                diferencia
            into #Filtrados
            from #Diferencias
            where
                @Filtro = ''
                or convert(varchar(30), id_solicitud) like @Like
                or codigo like @Like
                or cedula like @Like
                or nombre like @Like
                or convert(varchar(50), saldo_inicial) like @Like
                or convert(varchar(50), saldo_final) like @Like
                or convert(varchar(50), debitos) like @Like
                or convert(varchar(50), creditos) like @Like
                or convert(varchar(50), diferencia) like @Like;

            select count(1)
            from #Filtrados;

            select
                @TotalEvaluados as procesados,
                @TotalEvaluados as total_evaluados,
                @TotalDiferencias as diferencias,
                cast(
                    case
                        when @TotalEvaluados = 0 then 0
                        else 100
                    end
                    as decimal(5, 2)
                ) as porcentaje,
                cast(
                    isnull(sum(saldo_inicial), 0)
                    as decimal(38, 2)
                ) as total_saldo_inicial,
                cast(
                    isnull(sum(saldo_final), 0)
                    as decimal(38, 2)
                ) as total_saldo_final,
                cast(
                    isnull(sum(debitos), 0)
                    as decimal(38, 2)
                ) as total_debitos,
                cast(
                    isnull(sum(creditos), 0)
                    as decimal(38, 2)
                ) as total_creditos,
                cast(
                    isnull(sum(diferencia), 0)
                    as decimal(38, 2)
                ) as total_diferencia
            from #Filtrados;

            with Ordenados as
            (
                select
                    id_solicitud,
                    codigo,
                    cedula,
                    nombre,
                    saldo_inicial,
                    saldo_final,
                    debitos,
                    creditos,
                    diferencia,
                    row_number() over
                    (
                        order by
                            case
                                when @SortCode = 1
                                 and @SortAsc = 1
                                    then id_solicitud
                            end asc,
                            case
                                when @SortCode = 1
                                 and @SortAsc = 0
                                    then id_solicitud
                            end desc,
                            case
                                when @SortCode = 2
                                 and @SortAsc = 1
                                    then codigo
                            end asc,
                            case
                                when @SortCode = 2
                                 and @SortAsc = 0
                                    then codigo
                            end desc,
                            case
                                when @SortCode = 3
                                 and @SortAsc = 1
                                    then cedula
                            end asc,
                            case
                                when @SortCode = 3
                                 and @SortAsc = 0
                                    then cedula
                            end desc,
                            case
                                when @SortCode = 4
                                 and @SortAsc = 1
                                    then nombre
                            end asc,
                            case
                                when @SortCode = 4
                                 and @SortAsc = 0
                                    then nombre
                            end desc,
                            case
                                when @SortCode = 5
                                 and @SortAsc = 1
                                    then saldo_inicial
                            end asc,
                            case
                                when @SortCode = 5
                                 and @SortAsc = 0
                                    then saldo_inicial
                            end desc,
                            case
                                when @SortCode = 6
                                 and @SortAsc = 1
                                    then saldo_final
                            end asc,
                            case
                                when @SortCode = 6
                                 and @SortAsc = 0
                                    then saldo_final
                            end desc,
                            case
                                when @SortCode = 7
                                 and @SortAsc = 1
                                    then debitos
                            end asc,
                            case
                                when @SortCode = 7
                                 and @SortAsc = 0
                                    then debitos
                            end desc,
                            case
                                when @SortCode = 8
                                 and @SortAsc = 1
                                    then creditos
                            end asc,
                            case
                                when @SortCode = 8
                                 and @SortAsc = 0
                                    then creditos
                            end desc,
                            case
                                when @SortCode = 9
                                 and @SortAsc = 1
                                    then diferencia
                            end asc,
                            case
                                when @SortCode = 9
                                 and @SortAsc = 0
                                    then diferencia
                            end desc,
                            id_solicitud asc
                    ) as numero_fila
                from #Filtrados
            )
            select
                id_solicitud,
                codigo,
                cedula,
                nombre,
                saldo_inicial,
                saldo_final,
                debitos,
                creditos,
                diferencia
            from Ordenados
            where
                @UsarPaginacion = 0
                or (
                    numero_fila > @Offset
                    and numero_fila <= cast(@Offset as bigint)
                        + cast(@PageSize as bigint)
                )
            order by numero_fila;
            """;

        private readonly PortalDB _portalDb;

        public FrmVerificaSaldosASEDB(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
        }

        #region Carga inicial

        /// <summary>
        /// Obtiene la fecha del último corte de saldos y la fecha actual del
        /// servidor configurada para la empresa.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<AseVerificaSaldosInicialData>
            ASE_VerificaSaldos_Inicial_Obtener(int CodEmpresa)
        {
            return EjecutarConConexion(
                CodEmpresa,
                connection =>
                {
                    var informacion =
                        ObtenerInformacionInicial(connection);

                    return InformacionInicialValida(informacion)
                        ? DbHelper.CreateOkResponse(informacion!)
                        : DbHelper.CreateErrorResponse(
                            MensajeFechaCorte,
                            -2,
                            new AseVerificaSaldosInicialData());
                },
                new AseVerificaSaldosInicialData());
        }

        /// <summary>
        /// Obtiene los períodos históricos disponibles, ordenados del más
        /// reciente al más antiguo.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<AseVerificaSaldosPeriodoData>>
            ASE_VerificaSaldos_Periodos_Dropdown_Obtener(
                int CodEmpresa)
        {
            return EjecutarConConexion(
                CodEmpresa,
                connection => DbHelper.CreateOkResponse(
                    ObtenerPeriodos(connection)),
                new List<AseVerificaSaldosPeriodoData>());
        }

        #endregion

        #region Consulta y exportación

        /// <summary>
        /// Obtiene las diferencias de saldos actuales o históricas utilizando
        /// búsqueda, ordenamiento y paginación lazy loading.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<AseVerificaSaldosListaResult>
            ASE_VerificaSaldos_Lista_Obtener(
                int CodEmpresa,
                AseVerificaSaldosListaRequest? request)
        {
            return EjecutarLista(
                CodEmpresa,
                request,
                false);
        }

        /// <summary>
        /// Obtiene todas las diferencias de saldos actuales o históricas para
        /// su exportación, sin aplicar paginación.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<AseVerificaSaldosListaResult>
            ASE_VerificaSaldos_Lista_Export(
                int CodEmpresa,
                AseVerificaSaldosListaRequest? request)
        {
            return EjecutarLista(
                CodEmpresa,
                request,
                true);
        }

        #endregion

        #region Métodos privados

        private ErrorDto<AseVerificaSaldosListaResult> EjecutarLista(
            int codEmpresa,
            AseVerificaSaldosListaRequest? request,
            bool exportar)
        {
            string tipoBusqueda = NormalizarTipoBusqueda(
                request?.tipo_busqueda);

            string? mensajeValidacion = ValidarSolicitud(
                request,
                tipoBusqueda);

            if (mensajeValidacion != null)
            {
                return DbHelper.CreateErrorResponse(
                    mensajeValidacion,
                    -2,
                    CrearResultadoVacio());
            }

            return EjecutarConConexion(
                codEmpresa,
                connection => EjecutarConsulta(
                    connection,
                    request!,
                    tipoBusqueda,
                    exportar),
                CrearResultadoVacio());
        }

        private static ErrorDto<AseVerificaSaldosListaResult>
            EjecutarConsulta(
                SqlConnection connection,
                AseVerificaSaldosListaRequest request,
                string tipoBusqueda,
                bool exportar)
        {
            var informacion =
                ObtenerInformacionInicial(connection);

            if (!InformacionInicialValida(informacion))
            {
                return DbHelper.CreateErrorResponse(
                    MensajeFechaCorte,
                    -2,
                    CrearResultadoVacio());
            }

            if (tipoBusqueda == TipoHistorico
                && !PeriodoExiste(
                    connection,
                    request.id_per_historico.GetValueOrDefault()))
            {
                return DbHelper.CreateErrorResponse(
                    MensajePeriodoNoExiste,
                    -2,
                    CrearResultadoVacio());
            }

            if (tipoBusqueda == TipoActual)
            {
                using var transaction =
                    connection.BeginTransaction();

                ActualizarSaldosIniciales(
                    connection,
                    transaction,
                    informacion!);

                var resultado = ConsultarDiferencias(
                    connection,
                    transaction,
                    request,
                    tipoBusqueda,
                    informacion!,
                    exportar);

                transaction.Commit();

                return DbHelper.CreateOkResponse(resultado);
            }

            var resultadoHistorico = ConsultarDiferencias(
                connection,
                null,
                request,
                tipoBusqueda,
                informacion!,
                exportar);

            return DbHelper.CreateOkResponse(
                resultadoHistorico);
        }

        private static AseVerificaSaldosInicialData?
            ObtenerInformacionInicial(
                SqlConnection connection)
        {
            const string sql = """
                select top 1
                    convert(datetime, VALOR) as fecha_corte,
                    dbo.MyGetdate() as fecha_actual
                from CRD_PARAMETROS
                where COD_PARAMETRO = '16';
                """;

            return connection
                .QueryFirstOrDefault<AseVerificaSaldosInicialData>(
                    sql,
                    commandTimeout: CommandTimeoutSeconds);
        }

        private static List<AseVerificaSaldosPeriodoData>
            ObtenerPeriodos(
                SqlConnection connection)
        {
            const string sql = """
                select
                    ID_PER_HISTORICO as id_per_historico,
                    isnull(ANIO, 0) as anio,
                    MES as mes
                from ASE_PER_HISTORICO
                order by ANIO desc, MES desc;
                """;

            var periodos = connection
                .Query<AseVerificaSaldosPeriodoData>(
                    sql,
                    commandTimeout: CommandTimeoutSeconds)
                .ToList();

            foreach (var periodo in periodos)
            {
                periodo.descripcion =
                    $"{periodo.anio} - "
                    + MConciliacionDB.fxConvierteMES(
                        periodo.mes);
            }

            return periodos;
        }

        private static bool PeriodoExiste(
            SqlConnection connection,
            int idPerHistorico)
        {
            const string sql = """
                select case
                    when exists
                    (
                        select 1
                        from ASE_PER_HISTORICO
                        where ID_PER_HISTORICO = @IdPerHistorico
                    ) then cast(1 as bit)
                    else cast(0 as bit)
                end;
                """;

            return connection.QuerySingle<bool>(
                sql,
                new
                {
                    IdPerHistorico = idPerHistorico
                },
                commandTimeout: CommandTimeoutSeconds);
        }

        private static void ActualizarSaldosIniciales(
            SqlConnection connection,
            SqlTransaction transaction,
            AseVerificaSaldosInicialData informacion)
        {
            const string sql = """
                update REG_CREDITOS
                set SALDO_INICIAL = MONTOAPR
                where FECHAFORP >= @FechaCorte
                  and FECHAFORP <= @FechaFin
                  and SALDO_INICIAL = 0
                  and ESTADO in ('A', 'C', 'N');
                """;

            connection.Execute(
                sql,
                new
                {
                    FechaCorte =
                        informacion.fecha_corte.Date,
                    FechaFin =
                        informacion.fecha_actual.Date
                            .AddDays(1)
                            .AddSeconds(-1)
                },
                transaction,
                commandTimeout: CommandTimeoutSeconds);
        }

        private static AseVerificaSaldosListaResult
            ConsultarDiferencias(
                SqlConnection connection,
                SqlTransaction? transaction,
                AseVerificaSaldosListaRequest request,
                string tipoBusqueda,
                AseVerificaSaldosInicialData informacion,
                bool exportar)
        {
            var parameters = CrearParametrosConsulta(
                request,
                tipoBusqueda,
                informacion,
                exportar);

            using var multi = connection.QueryMultiple(
                SqlConsulta,
                parameters,
                transaction,
                commandTimeout: CommandTimeoutSeconds);

            int total = multi.ReadSingle<int>();

            var resumen = multi
                .ReadSingleOrDefault<AseVerificaSaldosResumen>()
                ?? new AseVerificaSaldosResumen();

            var lista = multi
                .Read<AseVerificaSaldosData>()
                .ToList();

            return new AseVerificaSaldosListaResult
            {
                total = total,
                lista = lista,
                resumen = resumen
            };
        }

        private static DynamicParameters
            CrearParametrosConsulta(
                AseVerificaSaldosListaRequest request,
                string tipoBusqueda,
                AseVerificaSaldosInicialData informacion,
                bool exportar)
        {
            var filtros =
                request.filtros
                ?? new FiltrosLazyLoadData();

            string filtro =
                filtros.filtro?.Trim()
                ?? string.Empty;

            int pageSize =
                Math.Max(0, filtros.paginacion);

            int pagina =
                Math.Max(0, filtros.pagina);

            bool usarPaginacion =
                !exportar && pageSize > 0;

            int offset =
                CalcularOffset(pagina, pageSize);

            int sortCode =
                ObtenerSortCode(filtros.sortField);

            bool sortAsc =
                filtros.sortOrder != -1;

            var parameters =
                new DynamicParameters();

            parameters.Add(
                "TipoBusqueda",
                tipoBusqueda,
                DbType.String);

            parameters.Add(
                "IdPerHistorico",
                request.id_per_historico
                    .GetValueOrDefault(),
                DbType.Int32);

            parameters.Add(
                "ExcluirOperacionesNuevas",
                request.excluir_operaciones_nuevas
                    .GetValueOrDefault()
                    ? 1
                    : 0,
                DbType.Int32);

            parameters.Add(
                "FechaCorte",
                informacion.fecha_corte.Date,
                DbType.DateTime);

            parameters.Add(
                "FechaActual",
                informacion.fecha_actual.Date,
                DbType.DateTime);

            parameters.Add(
                "FechaFin",
                informacion.fecha_actual.Date
                    .AddDays(1)
                    .AddSeconds(-1),
                DbType.DateTime);

            parameters.Add(
                "Filtro",
                filtro,
                DbType.String);

            parameters.Add(
                "Like",
                $"%{filtro}%",
                DbType.String);

            parameters.Add(
                "SortCode",
                sortCode,
                DbType.Int32);

            parameters.Add(
                "SortAsc",
                sortAsc ? 1 : 0,
                DbType.Int32);

            parameters.Add(
                "UsarPaginacion",
                usarPaginacion ? 1 : 0,
                DbType.Int32);

            parameters.Add(
                "Offset",
                offset,
                DbType.Int32);

            parameters.Add(
                "PageSize",
                pageSize,
                DbType.Int32);

            return parameters;
        }

        private static string? ValidarSolicitud(
            AseVerificaSaldosListaRequest? request,
            string tipoBusqueda)
        {
            if (request == null)
            {
                return "La solicitud indicada no es válida.";
            }

            if (tipoBusqueda != TipoActual
                && tipoBusqueda != TipoHistorico)
            {
                return MensajeTipoBusqueda;
            }

            if (tipoBusqueda == TipoHistorico
                && request.id_per_historico
                    .GetValueOrDefault() <= 0)
            {
                return MensajePeriodoRequerido;
            }

            return null;
        }

        private static string NormalizarTipoBusqueda(
            string? tipoBusqueda)
        {
            return tipoBusqueda?
                .Trim()
                .ToUpperInvariant()
                ?? string.Empty;
        }

        private static bool InformacionInicialValida(
            AseVerificaSaldosInicialData? informacion)
        {
            return informacion != null
                   && informacion.fecha_corte != default
                   && informacion.fecha_actual != default;
        }

        private static int ObtenerSortCode(
            string? sortField)
        {
            string campo =
                sortField?.Trim()
                ?? string.Empty;

            return SortFields.TryGetValue(
                campo,
                out int sortCode)
                ? sortCode
                : SortFields["id_solicitud"];
        }

        private static int CalcularOffset(
            int pagina,
            int pageSize)
        {
            long offset =
                (long)pagina * pageSize;

            return (int)Math.Min(
                offset,
                int.MaxValue);
        }

        private static AseVerificaSaldosListaResult
            CrearResultadoVacio()
        {
            return new AseVerificaSaldosListaResult
            {
                total = 0,
                lista = new List<AseVerificaSaldosData>(),
                resumen = new AseVerificaSaldosResumen()
            };
        }

        private ErrorDto<T> EjecutarConConexion<T>(
            int codEmpresa,
            Func<SqlConnection, ErrorDto<T>> action,
            T resultadoVacio)
        {
            if (codEmpresa <= 0)
            {
                return DbHelper.CreateErrorResponse(
                    "La empresa indicada no es válida.",
                    -2,
                    resultadoVacio);
            }

            try
            {
                using var connection =
                    DbHelper.OpenConnection(
                        _portalDb,
                        codEmpresa);

                connection.Open();

                return action(connection);
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse(
                    ex.Message,
                    -1,
                    resultadoVacio);
            }
            catch (InvalidOperationException ex)
            {
                return DbHelper.CreateErrorResponse(
                    ex.Message,
                    -1,
                    resultadoVacio);
            }
            catch (DataException ex)
            {
                return DbHelper.CreateErrorResponse(
                    ex.Message,
                    -1,
                    resultadoVacio);
            }
        }

        #endregion
    }
}