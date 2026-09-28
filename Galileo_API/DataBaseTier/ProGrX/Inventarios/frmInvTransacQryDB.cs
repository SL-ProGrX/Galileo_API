using System.Globalization;
using Dapper;
using Galileo.Models.ERROR;
using Galileo.Models.INV;

namespace Galileo.DataBaseTier
{
    public class FrmInvTransacQryDB
    {
        private const string ConsultaTransacciones = """
            WITH Filtradas AS
            (
                SELECT T.boleta,
                       T.fecha,
                       T.estado,
                       T.documento,
                       T.notas,
                       T.genera_user,
                       T.genera_fecha,
                       T.autoriza_user,
                       T.autoriza_fecha,
                       T.procesa_user,
                       T.procesa_fecha
                FROM pv_invTransac AS T
                WHERE (@tipo = '' OR T.tipo = @tipo)
                  AND (@estado = 'T' OR T.estado = @estado)
                  AND (
                        @tipo_fecha = 'T'
                        OR (
                            @tipo_fecha = 'I'
                            AND T.fecha >= @fecha_inicio
                            AND T.fecha < @fecha_fin
                        )
                        OR (
                            @tipo_fecha = 'S'
                            AND T.genera_fecha >= @fecha_inicio
                            AND T.genera_fecha < @fecha_fin
                        )
                        OR (
                            @tipo_fecha = 'A'
                            AND T.autoriza_fecha >= @fecha_inicio
                            AND T.autoriza_fecha < @fecha_fin
                        )
                        OR (
                            @tipo_fecha = 'P'
                            AND T.procesa_fecha >= @fecha_inicio
                            AND T.procesa_fecha < @fecha_fin
                        )
                      )
                  AND (
                        @tipo_usuario = 'T'
                        OR (
                            @tipo_usuario = 'S'
                            AND T.genera_user LIKE @usuario_prefijo
                        )
                        OR (
                            @tipo_usuario = 'A'
                            AND T.autoriza_user LIKE @usuario_prefijo
                        )
                        OR (
                            @tipo_usuario = 'P'
                            AND T.procesa_user LIKE @usuario_prefijo
                        )
                      )
                  AND (
                        @filtro_general IS NULL
                        OR CONVERT(nvarchar(50), T.boleta)
                            LIKE @filtro_general
                        OR T.documento LIKE @filtro_general
                        OR T.notas LIKE @filtro_general
                      )
            ),
            Numeradas AS
            (
                SELECT F.boleta,
                       F.fecha,
                       F.estado,
                       F.documento,
                       F.notas,
                       F.genera_user,
                       F.genera_fecha,
                       F.autoriza_user,
                       F.autoriza_fecha,
                       F.procesa_user,
                       F.procesa_fecha,
                       ROW_NUMBER() OVER (
                           ORDER BY F.fecha DESC, F.boleta DESC
                       ) AS fila
                FROM Filtradas AS F
            ),
            Totales AS
            (
                SELECT COUNT(*) AS total_registros
                FROM Numeradas
            )
            SELECT Totales.total_registros,
                   Pagina.fila,
                   Pagina.boleta,
                   Pagina.fecha,
                   Pagina.estado,
                   Pagina.documento,
                   Pagina.notas,
                   Pagina.genera_user,
                   Pagina.genera_fecha,
                   Pagina.autoriza_user,
                   Pagina.autoriza_fecha,
                   Pagina.procesa_user,
                   Pagina.procesa_fecha
            FROM Totales
            OUTER APPLY
            (
                SELECT N.fila,
                       N.boleta,
                       N.fecha,
                       N.estado,
                       N.documento,
                       N.notas,
                       N.genera_user,
                       N.genera_fecha,
                       N.autoriza_user,
                       N.autoriza_fecha,
                       N.procesa_user,
                       N.procesa_fecha
                FROM Numeradas AS N
                WHERE N.fila > @offset
                  AND N.fila <= @hasta
            ) AS Pagina
            ORDER BY Pagina.fila
            """;

        private readonly IConfiguration _config;

        private sealed class TransacQryFiltros
        {
            public string tipo { get; set; } = string.Empty;
            public string estado { get; set; } = "T";
            public string tipo_fecha { get; set; } = "T";
            public DateTime? fecha_inicio { get; set; }
            public DateTime? fecha_fin { get; set; }
            public string tipo_usuario { get; set; } = "T";
            public string usuario_prefijo { get; set; } = "%";
            public string? filtro_general { get; set; }
            public int offset { get; set; } = 0;
            public int hasta { get; set; } = 0;
        }

        private sealed class TransacQryFila : TransacQryData
        {
            public TransacQryFila()
            {
            }

            public int total_registros { get; set; } = 0;
            public long? fila { get; set; }
        }

        /// <summary>
        /// Inicializa el acceso a la consulta de transacciones.
        /// </summary>
        /// <param name="config">Configuración de la aplicación.</param>
        public FrmInvTransacQryDB(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        /// <summary>
        /// Obtiene las transacciones de inventario y el total de registros.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="parametros">Filtros y paginación de la consulta.</param>
        /// <returns>Transacciones encontradas y cantidad total.</returns>
        public ErrorDto<TransacQryDataList> TransacInv_Obtener(
            int CodEmpresa,
            TransacQryParametros? parametros)
        {
            var preparacion =
                INV_TransacQry_Filtros_Preparar(parametros);

            if (preparacion.Code != 0)
            {
                return DbHelper.CreateErrorResponse(
                    preparacion.Description
                        ?? "Los filtros de la consulta no son v&aacute;lidos.",
                    preparacion.Code.GetValueOrDefault(-2),
                    new TransacQryDataList());
            }

            if (preparacion.Result is null)
            {
                return DbHelper.CreateErrorResponse(
                    "No fue posible preparar los filtros de la consulta.",
                    -1,
                    new TransacQryDataList());
            }

            TransacQryFiltros filtros = preparacion.Result;

            var resultado = DbHelper.WithConn(
                new PortalDB(_config),
                CodEmpresa,
                connection =>
                {
                    connection.Open();

                    List<TransacQryFila> filas = connection
                        .Query<TransacQryFila>(
                            ConsultaTransacciones,
                            filtros)
                        .ToList();

                    return new TransacQryDataList
                    {
                        total = filas.FirstOrDefault()
                            ?.total_registros ?? 0,
                        transacciones = filas
                            .Where(item => item.fila.HasValue)
                            .Cast<TransacQryData>()
                            .ToList()
                    };
                });

            return resultado.Code == 0
                ? DbHelper.CreateOkResponse(
                    resultado.Result ?? new TransacQryDataList())
                : DbHelper.CreateErrorResponse(
                    resultado.Description
                        ?? "Error al consultar las transacciones de inventario.",
                    resultado.Code.GetValueOrDefault(-1),
                    new TransacQryDataList());
        }

        /// <summary>
        /// Normaliza y valida los filtros enviados por el cliente.
        /// </summary>
        /// <param name="parametros">Filtros recibidos.</param>
        /// <returns>Filtros listos para ejecutar la consulta.</returns>
        private static ErrorDto<TransacQryFiltros>
            INV_TransacQry_Filtros_Preparar(
                TransacQryParametros? parametros)
        {
            if (parametros is null)
            {
                return DbHelper.CreateErrorResponse(
                    "Los filtros de la consulta son requeridos.",
                    -2,
                    new TransacQryFiltros());
            }

            var filtros = new TransacQryFiltros
            {
                tipo = INV_TransacQry_Codigo_Normalizar(
                    parametros.tipo,
                    string.Empty),
                estado = INV_TransacQry_Codigo_Normalizar(
                    parametros.estado,
                    "T"),
                tipo_fecha = INV_TransacQry_Codigo_Normalizar(
                    parametros.tipo_fecha,
                    "T"),
                tipo_usuario = INV_TransacQry_Codigo_Normalizar(
                    parametros.tipo_usuario,
                    "T")
            };

            string? errorOpciones =
                INV_TransacQry_Opciones_Validar(filtros);

            if (errorOpciones is not null)
            {
                return DbHelper.CreateErrorResponse(
                    errorOpciones,
                    -2,
                    new TransacQryFiltros());
            }

            if (
                parametros.pagina < 0 ||
                parametros.paginacion <= 0 ||
                parametros.paginacion > 5000 ||
                parametros.pagina >
                    int.MaxValue - parametros.paginacion
            )
            {
                return DbHelper.CreateErrorResponse(
                    "La paginaci&oacute;n no es v&aacute;lida.",
                    -2,
                    new TransacQryFiltros());
            }

            if (filtros.tipo_fecha != "T")
            {
                if (
                    !DateTime.TryParseExact(
                        parametros.fecha_inicio?.Trim(),
                        "yyyy-MM-dd",
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.None,
                        out DateTime inicio
                    ) ||
                    !DateTime.TryParseExact(
                        parametros.fecha_corte?.Trim(),
                        "yyyy-MM-dd",
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.None,
                        out DateTime corte
                    )
                )
                {
                    return DbHelper.CreateErrorResponse(
                        "Las fechas deben utilizar el formato yyyy-MM-dd.",
                        -2,
                        new TransacQryFiltros());
                }

                if (
                    inicio > corte ||
                    corte.Date == DateTime.MaxValue.Date
                )
                {
                    return DbHelper.CreateErrorResponse(
                        "El rango de fechas no es v&aacute;lido.",
                        -2,
                        new TransacQryFiltros());
                }

                filtros.fecha_inicio = inicio.Date;
                filtros.fecha_fin = corte.Date.AddDays(1);
            }

            string usuario =
                (parametros.usuario ?? string.Empty).Trim();
            string filtroGeneral =
                (parametros.vfiltro ?? string.Empty).Trim();

            filtros.usuario_prefijo = $"{usuario}%";
            filtros.filtro_general =
                filtroGeneral.Length == 0
                    ? null
                    : $"%{filtroGeneral}%";
            filtros.offset = parametros.pagina;
            filtros.hasta =
                parametros.pagina + parametros.paginacion;

            return DbHelper.CreateOkResponse(filtros);
        }

        /// <summary>
        /// Normaliza un código de filtro y aplica su valor predeterminado.
        /// </summary>
        /// <param name="valor">Código recibido.</param>
        /// <param name="predeterminado">Valor si el código está vacío.</param>
        /// <returns>Código normalizado.</returns>
        private static string INV_TransacQry_Codigo_Normalizar(
            string? valor,
            string predeterminado)
        {
            return string.IsNullOrWhiteSpace(valor)
                ? predeterminado
                : valor.Trim().ToUpperInvariant();
        }

        /// <summary>
        /// Valida las opciones de tipo, estado, fecha y usuario.
        /// </summary>
        /// <param name="filtros">Opciones normalizadas.</param>
        /// <returns>Descripción del error o null si son válidas.</returns>
        private static string? INV_TransacQry_Opciones_Validar(
            TransacQryFiltros filtros)
        {
            if (filtros.tipo is not ("" or "E" or "S" or "T" or "R"))
            {
                return "El tipo de transacci&oacute;n no es v&aacute;lido.";
            }

            if (
                filtros.estado is not
                    ("T" or "S" or "A" or "P" or "R" or "N")
            )
            {
                return "El estado de la transacci&oacute;n no es v&aacute;lido.";
            }

            if (
                filtros.tipo_fecha is not
                    ("T" or "I" or "S" or "A" or "P")
            )
            {
                return "La fecha base no es v&aacute;lida.";
            }

            if (
                filtros.tipo_usuario is not
                    ("T" or "S" or "A" or "P")
            )
            {
                return "El tipo de usuario no es v&aacute;lido.";
            }

            return null;
        }
    }
}