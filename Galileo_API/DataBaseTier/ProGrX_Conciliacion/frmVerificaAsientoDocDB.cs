using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_Conciliacion;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Galileo_API.DataBaseTier.ProGrX.Conciliacion
{
    public class FrmVerificaAsientosDocumentoDB
    {
        private const int CommandTimeoutSeconds = 360;

        private const string MensajeEmpresaInvalida =
            "La empresa indicada no es válida.";

        private const string MensajeSolicitudInvalida =
            "La solicitud indicada no es válida.";

        private const string MensajeFechaInicio =
            "Debe indicar una fecha de inicio válida.";

        private const string MensajeFechaCorte =
            "Debe indicar una fecha de corte válida.";

        private const string MensajeRangoFechas =
            "La fecha de inicio no puede ser mayor que la fecha de corte.";

        private const string MensajeFechaServidor =
            "No fue posible obtener la fecha actual del servidor.";

        private static readonly IReadOnlyDictionary<string, int> SortFields =
            new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
            {
                ["fecha"] = 1,
                ["tipo_documento"] = 2,
                ["cod_transaccion"] = 3,
                ["debitos"] = 4,
                ["creditos"] = 5,
                ["diferencia"] = 6,
                ["concepto_desc"] = 7,
                ["registro_usuario"] = 8
            };

        private const string SqlConsulta = """
    set nocount on;

    with MovimientosAgrupados as
    (
        select
            D.REGISTRO_FECHA as fecha,
            rtrim(A.TIPO_DOCUMENTO) as tipo_documento,
            ltrim(rtrim(A.COD_TRANSACCION)) as cod_transaccion,
            cast(
                sum(
                    case
                        when A.TIPO_MOVIMIENTO = 'D'
                            then isnull(A.MONTO, 0)
                        else 0
                    end
                )
                as decimal(38, 2)
            ) as debitos,
            cast(
                sum(
                    case
                        when A.TIPO_MOVIMIENTO = 'C'
                            then isnull(A.MONTO, 0)
                        else 0
                    end
                )
                as decimal(38, 2)
            ) as creditos,
            rtrim(Con.DESCRIPCION) as concepto_desc,
            rtrim(D.REGISTRO_USUARIO) as registro_usuario
        from SIF_TRANSACCIONES D
        inner join SIF_TRANSACCIONES_ASIENTO A
            on D.TIPO_DOCUMENTO = A.TIPO_DOCUMENTO
           and D.COD_TRANSACCION = A.COD_TRANSACCION
        inner join SIF_CONCEPTOS Con
            on D.COD_CONCEPTO = Con.COD_CONCEPTO
        where D.REGISTRO_FECHA >= @FechaInicio
          and D.REGISTRO_FECHA < @FechaFin
        group by
            A.TIPO_DOCUMENTO,
            A.COD_TRANSACCION,
            Con.DESCRIPCION,
            D.REGISTRO_USUARIO,
            D.REGISTRO_FECHA
        having
            sum(
                case
                    when A.TIPO_MOVIMIENTO = 'D'
                        then isnull(A.MONTO, 0)
                    else 0
                end
            )
            <>
            sum(
                case
                    when A.TIPO_MOVIMIENTO <> 'D'
                        then isnull(A.MONTO, 0)
                    else 0
                end
            )
    ),
    Diferencias as
    (
        select
            fecha,
            tipo_documento,
            cod_transaccion,
            debitos,
            creditos,
            cast(
                debitos - creditos
                as decimal(38, 2)
            ) as diferencia,
            concepto_desc,
            registro_usuario
        from MovimientosAgrupados
        where debitos <> creditos
    )
    select
        fecha,
        tipo_documento,
        cod_transaccion,
        case
            when len(cod_transaccion) between 1 and 38
             and cod_transaccion not like '%[^0-9]%'
                then convert(
                    decimal(38, 0),
                    cod_transaccion
                )
            else null
        end as cod_transaccion_numero,
        debitos,
        creditos,
        diferencia,
        concepto_desc,
        registro_usuario
    into #Filtrados
    from Diferencias
    where
        @Filtro = ''
        or convert(varchar(10), fecha, 103) like @Like
        or tipo_documento like @Like
        or cod_transaccion like @Like
        or convert(varchar(50), debitos) like @Like
        or convert(varchar(50), creditos) like @Like
        or convert(varchar(50), diferencia) like @Like
        or concepto_desc like @Like
        or registro_usuario like @Like;

    select count(1)
    from #Filtrados;

    with Ordenados as
    (
        select
            fecha,
            tipo_documento,
            cod_transaccion,
            debitos,
            creditos,
            diferencia,
            concepto_desc,
            registro_usuario,
            row_number() over
            (
                order by
                    case
                        when @SortCode = 1
                         and @SortAsc = 1
                            then fecha
                    end asc,
                    case
                        when @SortCode = 1
                         and @SortAsc = 0
                            then fecha
                    end desc,

                    case
                        when @SortCode = 2
                         and @SortAsc = 1
                            then tipo_documento
                    end asc,
                    case
                        when @SortCode = 2
                         and @SortAsc = 0
                            then tipo_documento
                    end desc,

                    case
                        when @SortCode = 3
                            then case
                                when cod_transaccion_numero is null
                                    then 1
                                else 0
                            end
                    end asc,
                    case
                        when @SortCode = 3
                         and @SortAsc = 1
                            then cod_transaccion_numero
                    end asc,
                    case
                        when @SortCode = 3
                         and @SortAsc = 0
                            then cod_transaccion_numero
                    end desc,
                    case
                        when @SortCode = 3
                         and @SortAsc = 1
                            then cod_transaccion
                    end asc,
                    case
                        when @SortCode = 3
                         and @SortAsc = 0
                            then cod_transaccion
                    end desc,

                    case
                        when @SortCode = 4
                         and @SortAsc = 1
                            then debitos
                    end asc,
                    case
                        when @SortCode = 4
                         and @SortAsc = 0
                            then debitos
                    end desc,

                    case
                        when @SortCode = 5
                         and @SortAsc = 1
                            then creditos
                    end asc,
                    case
                        when @SortCode = 5
                         and @SortAsc = 0
                            then creditos
                    end desc,

                    case
                        when @SortCode = 6
                         and @SortAsc = 1
                            then diferencia
                    end asc,
                    case
                        when @SortCode = 6
                         and @SortAsc = 0
                            then diferencia
                    end desc,

                    case
                        when @SortCode = 7
                         and @SortAsc = 1
                            then concepto_desc
                    end asc,
                    case
                        when @SortCode = 7
                         and @SortAsc = 0
                            then concepto_desc
                    end desc,

                    case
                        when @SortCode = 8
                         and @SortAsc = 1
                            then registro_usuario
                    end asc,
                    case
                        when @SortCode = 8
                         and @SortAsc = 0
                            then registro_usuario
                    end desc,

                    fecha asc,
                    tipo_documento asc,
                    case
                        when cod_transaccion_numero is null
                            then 1
                        else 0
                    end asc,
                    cod_transaccion_numero asc,
                    cod_transaccion asc
            ) as numero_fila
        from #Filtrados
    )
    select
        fecha,
        tipo_documento,
        cod_transaccion,
        debitos,
        creditos,
        diferencia,
        concepto_desc,
        registro_usuario
    from Ordenados
    where
        @UsarPaginacion = 0
        or (
            numero_fila > @Offset
            and numero_fila <=
                cast(@Offset as bigint)
                + cast(@PageSize as bigint)
        )
    order by numero_fila;
    """;

        private readonly PortalDB _portalDb;
        private readonly MProGrxMain _proGrxMain;

        public FrmVerificaAsientosDocumentoDB(
            IConfiguration config)
        {
            _portalDb = new PortalDB(config);
            _proGrxMain = new MProGrxMain(config);
        }

        #region Carga inicial

        /// <summary>
        /// Obtiene la fecha actual del servidor para inicializar las fechas
        /// de inicio y corte de la pantalla.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<AseVerificaAsientosDocumentoInicialData>
            ASE_VerificaAsientosDocumento_Inicial_Obtener(
                int CodEmpresa)
        {
            return EjecutarOperacion(
                CodEmpresa,
                () =>
                {
                    DateTime fechaServidor =
                        _proGrxMain.fxFechaServidor(
                            CodEmpresa,
                            0);

                    if (fechaServidor == default)
                    {
                        return DbHelper.CreateErrorResponse(
                            MensajeFechaServidor,
                            -2,
                            new AseVerificaAsientosDocumentoInicialData());
                    }

                    var resultado =
                        new AseVerificaAsientosDocumentoInicialData
                        {
                            fecha_servidor = fechaServidor
                        };

                    return DbHelper.CreateOkResponse(resultado);
                },
                new AseVerificaAsientosDocumentoInicialData());
        }

        #endregion

        #region Consulta y exportación

        /// <summary>
        /// Obtiene los documentos con diferencias entre débitos y créditos,
        /// aplicando filtro, ordenamiento y paginación lazy loading.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<AseVerificaAsientosDocumentoListaResult>
            ASE_VerificaAsientosDocumento_Lista_Obtener(
                int CodEmpresa,
                AseVerificaAsientosDocumentoListaRequest? request)
        {
            return EjecutarLista(
                CodEmpresa,
                request,
                false);
        }

        /// <summary>
        /// Obtiene todos los documentos con diferencias entre débitos y
        /// créditos para su exportación, sin aplicar paginación.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<AseVerificaAsientosDocumentoListaResult>
            ASE_VerificaAsientosDocumento_Lista_Export(
                int CodEmpresa,
                AseVerificaAsientosDocumentoListaRequest? request)
        {
            return EjecutarLista(
                CodEmpresa,
                request,
                true);
        }

        #endregion

        #region Métodos privados

        private ErrorDto<AseVerificaAsientosDocumentoListaResult>
            EjecutarLista(
                int codEmpresa,
                AseVerificaAsientosDocumentoListaRequest? request,
                bool exportar)
        {
            string? mensajeValidacion =
                ValidarSolicitud(request);

            if (mensajeValidacion != null)
            {
                return DbHelper.CreateErrorResponse(
                    mensajeValidacion,
                    -2,
                    CrearResultadoVacio());
            }

            return EjecutarOperacion(
                codEmpresa,
                () =>
                {
                    using var connection =
                        DbHelper.OpenConnection(
                            _portalDb,
                            codEmpresa);

                    connection.Open();

                    var resultado =
                        ConsultarDiferencias(
                            connection,
                            request!,
                            exportar);

                    return DbHelper.CreateOkResponse(resultado);
                },
                CrearResultadoVacio());
        }

        private static AseVerificaAsientosDocumentoListaResult
            ConsultarDiferencias(
                SqlConnection connection,
                AseVerificaAsientosDocumentoListaRequest request,
                bool exportar)
        {
            DynamicParameters parameters =
                CrearParametrosConsulta(
                    request,
                    exportar);

            using var multi = connection.QueryMultiple(
                SqlConsulta,
                parameters,
                commandTimeout: CommandTimeoutSeconds);

            int total = multi.ReadSingle<int>();

            List<AseVerificaAsientosDocumentoData> lista =
                multi
                    .Read<AseVerificaAsientosDocumentoData>()
                    .ToList();

            return new AseVerificaAsientosDocumentoListaResult
            {
                total = total,
                lista = lista
            };
        }

        private static DynamicParameters CrearParametrosConsulta(
     AseVerificaAsientosDocumentoListaRequest request,
     bool exportar)
        {
            var filtros =
                request.filtros
                ?? new FiltrosLazyLoadData();

            string filtro =
                filtros.filtro?.Trim()
                ?? string.Empty;

            int pagina =
                Math.Max(
                    0,
                    filtros.pagina);

            int pageSize =
                Math.Max(
                    0,
                    filtros.paginacion);

            bool usarPaginacion =
                !exportar
                && pageSize > 0;

            int offset =
                usarPaginacion
                    ? CalcularOffset(
                        pagina,
                        pageSize)
                    : 0;

            int sortCode =
                ObtenerSortCode(
                    filtros.sortField);

            bool sortAsc =
                filtros.sortOrder != -1;

            DateTime fechaInicio =
                request.fecha_inicio
                    .GetValueOrDefault()
                    .Date;

            DateTime fechaFin =
                request.fecha_corte
                    .GetValueOrDefault()
                    .Date
                    .AddDays(1);

            var parameters = new DynamicParameters();

            AgregarParametro(
                parameters,
                "FechaInicio",
                fechaInicio,
                DbType.DateTime);

            AgregarParametro(
                parameters,
                "FechaFin",
                fechaFin,
                DbType.DateTime);

            AgregarParametro(
                parameters,
                "Filtro",
                filtro,
                DbType.String);

            AgregarParametro(
                parameters,
                "Like",
                $"%{filtro}%",
                DbType.String);

            AgregarParametro(
                parameters,
                "SortCode",
                sortCode,
                DbType.Int32);

            AgregarParametro(
                parameters,
                "SortAsc",
                sortAsc ? 1 : 0,
                DbType.Int32);

            AgregarParametro(
                parameters,
                "UsarPaginacion",
                usarPaginacion ? 1 : 0,
                DbType.Int32);

            AgregarParametro(
                parameters,
                "Offset",
                offset,
                DbType.Int32);

            AgregarParametro(
                parameters,
                "PageSize",
                pageSize,
                DbType.Int32);

            return parameters;
        }

        private static void AgregarParametro(
            DynamicParameters parameters,
            string nombre,
            object? valor,
            DbType tipo)
        {
            parameters.Add(
                nombre,
                valor,
                tipo);
        }

        private static string? ValidarSolicitud(
            AseVerificaAsientosDocumentoListaRequest? request)
        {
            if (request == null)
            {
                return MensajeSolicitudInvalida;
            }

            if (!request.fecha_inicio.HasValue
                || request.fecha_inicio.Value == default)
            {
                return MensajeFechaInicio;
            }

            if (!request.fecha_corte.HasValue
                || request.fecha_corte.Value == default)
            {
                return MensajeFechaCorte;
            }

            if (request.fecha_inicio.Value.Date
                > request.fecha_corte.Value.Date)
            {
                return MensajeRangoFechas;
            }

            if (request.fecha_corte.Value.Date
                >= DateTime.MaxValue.Date)
            {
                return MensajeFechaCorte;
            }

            return null;
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
                : SortFields["fecha"];
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

        private static AseVerificaAsientosDocumentoListaResult
            CrearResultadoVacio()
        {
            return new AseVerificaAsientosDocumentoListaResult
            {
                total = 0,
                lista =
                    new List<AseVerificaAsientosDocumentoData>()
            };
        }

        private static ErrorDto<T> EjecutarOperacion<T>(
            int codEmpresa,
            Func<ErrorDto<T>> action,
            T resultadoVacio)
        {
            if (codEmpresa <= 0)
            {
                return DbHelper.CreateErrorResponse(
                    MensajeEmpresaInvalida,
                    -2,
                    resultadoVacio);
            }

            try
            {
                return action();
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
            catch (ArgumentOutOfRangeException ex)
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