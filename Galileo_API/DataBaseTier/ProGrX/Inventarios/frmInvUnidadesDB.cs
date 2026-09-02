using Dapper;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.INV;
using Galileo.Models.Security;

namespace Galileo.DataBaseTier
{
    public sealed class FrmInvUnidadesDb
    {
        private const int CodigoValidacion = -2;
        private const int ModuloInventarios = 32;
        private const int PaginacionPredeterminada = 30;
        private const int PaginacionMaxima = 100;
        private const int LongitudFiltroMaxima = 150;

        private const string MensajeOk = "Ok";
        private const string ErrorConsultarLista =
            "Ocurri&oacute; un error al consultar las unidades de medici&oacute;n.";
        private const string ErrorConsultarDetalle =
            "Ocurri&oacute; un error al consultar el detalle de las unidades de medici&oacute;n.";
        private const string ErrorConsultarCatalogo =
            "Ocurri&oacute; un error al consultar el cat&aacute;logo de unidades de medici&oacute;n.";
        private const string ErrorRegistrar =
            "Ocurri&oacute; un error al registrar la unidad de medici&oacute;n.";
        private const string ErrorActualizar =
            "Ocurri&oacute; un error al actualizar la unidad de medici&oacute;n.";
        private const string ErrorEliminar =
            "Ocurri&oacute; un error al eliminar la unidad de medici&oacute;n.";

        private const string QueryTotal = """
            SELECT COUNT(1)
            FROM pv_unidades
            WHERE
                @Filtro IS NULL
                OR cod_unidad LIKE @Filtro ESCAPE '\'
                OR descripcion LIKE @Filtro ESCAPE '\'
                OR ISNULL(Unidad_Hacienda_Id, 'Unid') LIKE @Filtro ESCAPE '\';
            """;

        private const string QueryLista = """
            SELECT
                RTRIM(ISNULL(cod_unidad, '')) AS cod_unidad,
                RTRIM(ISNULL(descripcion, '')) AS descripcion,
                RTRIM(ISNULL(Unidad_Hacienda_Id, 'Unid')) AS hacienda,
                CAST(ISNULL(activo, 0) AS bit) AS activo
            FROM pv_unidades
            WHERE
                @Filtro IS NULL
                OR cod_unidad LIKE @Filtro ESCAPE '\'
                OR descripcion LIKE @Filtro ESCAPE '\'
                OR ISNULL(Unidad_Hacienda_Id, 'Unid') LIKE @Filtro ESCAPE '\'
            ORDER BY
                CASE
                    WHEN @SortField = 'cod_unidad' AND @SortOrder = 1
                    THEN cod_unidad
                END ASC,
                CASE
                    WHEN @SortField = 'cod_unidad' AND @SortOrder = 2
                    THEN cod_unidad
                END DESC,
                CASE
                    WHEN @SortField = 'descripcion' AND @SortOrder = 1
                    THEN descripcion
                END ASC,
                CASE
                    WHEN @SortField = 'descripcion' AND @SortOrder = 2
                    THEN descripcion
                END DESC,
                CASE
                    WHEN @SortField = 'hacienda' AND @SortOrder = 1
                    THEN ISNULL(Unidad_Hacienda_Id, 'Unid')
                END ASC,
                CASE
                    WHEN @SortField = 'hacienda' AND @SortOrder = 2
                    THEN ISNULL(Unidad_Hacienda_Id, 'Unid')
                END DESC,
                CASE
                    WHEN @SortField = 'activo' AND @SortOrder = 1
                    THEN activo
                END ASC,
                CASE
                    WHEN @SortField = 'activo' AND @SortOrder = 2
                    THEN activo
                END DESC,
                cod_unidad ASC
            OFFSET @Pagina ROWS
            FETCH NEXT @Paginacion ROWS ONLY;
            """;

        private const string QueryDetalle = """
            SELECT
                RTRIM(ISNULL(cod_unidad, '')) AS cod_unidad,
                RTRIM(ISNULL(descripcion, '')) AS descripcion,
                RTRIM(ISNULL(Unidad_Hacienda_Id, 'Unid')) AS hacienda,
                CAST(ISNULL(activo, 0) AS bit) AS activo
            FROM pv_unidades
            ORDER BY cod_unidad ASC;
            """;

        private const string QueryCatalogo = """
            SELECT
                RTRIM(ISNULL(cod_unidad, '')) AS cod_unidad,
                RTRIM(ISNULL(descripcion, '')) AS descripcion
            FROM pv_unidades
            ORDER BY cod_unidad ASC;
            """;

        private const string QueryExiste = """
            SELECT COUNT(1)
            FROM pv_unidades
            WHERE cod_unidad = @CodUnidad;
            """;

        private const string QueryDescripcion = """
            SELECT RTRIM(ISNULL(descripcion, ''))
            FROM pv_unidades
            WHERE cod_unidad = @CodUnidad;
            """;

        private const string QueryRegistrar = """
            INSERT INTO pv_unidades
            (
                cod_unidad,
                descripcion,
                Unidad_Hacienda_Id,
                activo,
                registro_fecha,
                registro_usuario
            )
            VALUES
            (
                @CodUnidad,
                @Descripcion,
                @Hacienda,
                @Activo,
                dbo.MyGetdate(),
                @Usuario
            );
            """;

        private const string QueryActualizar = """
            UPDATE pv_unidades
            SET
                descripcion = @Descripcion,
                Unidad_Hacienda_Id = @Hacienda,
                activo = @Activo
            WHERE cod_unidad = @CodUnidad;
            """;

        private const string QueryEliminar = """
            DELETE FROM pv_unidades
            WHERE cod_unidad = @CodUnidad;
            """;

        private readonly PortalDB _portalDb;
        private readonly MSecurityMainDb _securityMainDb;

        public FrmInvUnidadesDb(IConfiguration config)
        {
            ArgumentNullException.ThrowIfNull(config);

            _portalDb = new PortalDB(config);
            _securityMainDb = new MSecurityMainDb(config);
        }

        /// <summary>
        /// Obtiene las unidades de medición paginadas.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="filtros">Filtros aplicados a la consulta.</param>
        /// <returns>Listado paginado de unidades de medición.</returns>
        public ErrorDto<UnidadesDataLista> INV_Unidades_Lista_Obtener(
            int CodEmpresa,
            FiltrosLazyLoadData? filtros)
        {
            var respuesta = INV_Unidades_Lista_Vacia();
            int pagina = Math.Max(0, filtros?.pagina ?? 0);
            int paginacionSolicitada = filtros?.paginacion ?? 0;
            int paginacion = paginacionSolicitada <= 0
                ? PaginacionPredeterminada
                : Math.Min(paginacionSolicitada, PaginacionMaxima);

            string filtro = filtros?.filtro?.Trim() ?? string.Empty;

            if (filtro.Length > LongitudFiltroMaxima)
            {
                return DbHelper.CreateErrorResponse(
                    $"El filtro no puede superar los {LongitudFiltroMaxima} caracteres.",
                    CodigoValidacion,
                    respuesta);
            }

            string? filtroLike = string.IsNullOrEmpty(filtro)
                ? null
                : $"%{INV_Unidades_Filtro_Escapar(filtro)}%";

            var parametros = new
            {
                Filtro = filtroLike,
                Pagina = pagina,
                Paginacion = paginacion,
                SortField = INV_Unidades_Orden_Campo_Obtener(
                    filtros?.sortField),
                SortOrder = INV_Unidades_Orden_Direccion_Obtener(
                    filtros?.sortOrder ?? 0)
            };

            var result = DbHelper.WithConn(
                _portalDb,
                CodEmpresa,
                connection =>
                {
                    var lista = INV_Unidades_Lista_Vacia();

                    lista.total = connection.QueryFirstOrDefault<int>(
                        QueryTotal,
                        parametros);

                    lista.unidades = connection
                        .Query<UnidadMedicionDto>(
                            QueryLista,
                            parametros)
                        .ToList();

                    return lista;
                });

            return result.Code == 0
                ? DbHelper.CreateOkResponse(result.Result ?? respuesta)
                : DbHelper.CreateErrorResponse(
                    result.Description ?? ErrorConsultarLista,
                    result.Code.GetValueOrDefault(-1),
                    respuesta);
        }

        /// <summary>
        /// Obtiene todas las unidades de medición con su detalle.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <returns>Listado detallado de unidades de medición.</returns>
        public ErrorDto<List<UnidadMedicionDto>> INV_Unidades_Detalle_Obtener(
            int CodEmpresa)
        {
            var result = DbHelper.ExecuteListQuery<UnidadMedicionDto>(
                _portalDb,
                CodEmpresa,
                QueryDetalle);

            return result.Code == 0
                ? DbHelper.CreateOkResponse(
                    result.Result ?? [])
                : DbHelper.CreateErrorResponse(
                    result.Description ?? ErrorConsultarDetalle,
                    result.Code.GetValueOrDefault(-1),
                    new List<UnidadMedicionDto>());
        }

        /// <summary>
        /// Obtiene las unidades de medición utilizadas en catálogos.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <returns>Catálogo de unidades de medición.</returns>
        public ErrorDto<List<UnidadMedicion>> INV_Unidades_Catalogo_Obtener(
            int CodEmpresa)
        {
            var result = DbHelper.ExecuteListQuery<UnidadMedicion>(
                _portalDb,
                CodEmpresa,
                QueryCatalogo);

            return result.Code == 0
                ? DbHelper.CreateOkResponse(
                    result.Result ?? [])
                : DbHelper.CreateErrorResponse(
                    result.Description ?? ErrorConsultarCatalogo,
                    result.Code.GetValueOrDefault(-1),
                    new List<UnidadMedicion>());
        }

        /// <summary>
        /// Registra una unidad de medición.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="request">Información de la unidad de medición.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto INV_Unidades_Registrar(
            int CodEmpresa,
            UnidadMedicionDto? request)
        {
            string validacion = INV_Unidades_Datos_Validar(
                request,
                true);

            if (!string.IsNullOrEmpty(validacion))
            {
                return DbHelper.ErrorResponse(
                    validacion,
                    CodigoValidacion);
            }

            var parametros = INV_Unidades_Parametros_Obtener(request!);

            var result = DbHelper.WithConn(
                _portalDb,
                CodEmpresa,
                connection =>
                {
                    connection.Open();

                    using var transaction = connection.BeginTransaction();

                    try
                    {
                        int existe = connection.QueryFirstOrDefault<int>(
                            QueryExiste,
                            parametros,
                            transaction);

                        if (existe > 0)
                        {
                            transaction.Rollback();

                            return DbHelper.ErrorResponse(
                                "El c&oacute;digo de la unidad ya existe.",
                                CodigoValidacion);
                        }

                        connection.Execute(
                            QueryRegistrar,
                            parametros,
                            transaction);

                        transaction.Commit();

                        return DbHelper.OkResponse(MensajeOk);
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                });

            var respuesta = INV_Unidades_Resultado_Obtener(
                result,
                ErrorRegistrar);

            if (respuesta.Code == 0)
            {
                INV_Unidades_Bitacora_Registrar(
                    CodEmpresa,
                    request!.registro_usuario,
                    "Registra - WEB",
                    $"Unidad de Medida : {request.cod_unidad}");
            }

            return respuesta;
        }

        /// <summary>
        /// Actualiza una unidad de medición existente.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="request">Información de la unidad de medición.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto INV_Unidades_Actualizar(
            int CodEmpresa,
            UnidadMedicionDto? request)
        {
            string validacion = INV_Unidades_Datos_Validar(
                request,
                true);

            if (!string.IsNullOrEmpty(validacion))
            {
                return DbHelper.ErrorResponse(
                    validacion,
                    CodigoValidacion);
            }

            var parametros = INV_Unidades_Parametros_Obtener(request!);

            var result = DbHelper.WithConn(
                _portalDb,
                CodEmpresa,
                connection =>
                {
                    int registros = connection.Execute(
                        QueryActualizar,
                        parametros);

                    return registros == 0
                        ? DbHelper.ErrorResponse(
                            "La unidad indicada no existe.",
                            CodigoValidacion)
                        : DbHelper.OkResponse(MensajeOk);
                });

            var respuesta = INV_Unidades_Resultado_Obtener(
                result,
                ErrorActualizar);

            if (respuesta.Code == 0)
            {
                INV_Unidades_Bitacora_Registrar(
                    CodEmpresa,
                    request!.registro_usuario,
                    "Modifica - WEB",
                    $"Unidad de Medida : {request.cod_unidad}");
            }

            return respuesta;
        }

        /// <summary>
        /// Elimina una unidad de medición.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="unidad">Código de la unidad de medición.</param>
        /// <param name="usuario">Usuario que ejecuta la eliminación.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto INV_Unidades_Eliminar(
            int CodEmpresa,
            string? unidad,
            string? usuario)
        {
            if (string.IsNullOrWhiteSpace(unidad))
            {
                return DbHelper.ErrorResponse(
                    "El c&oacute;digo de la unidad es requerido.",
                    CodigoValidacion);
            }

            if (string.IsNullOrWhiteSpace(usuario))
            {
                return DbHelper.ErrorResponse(
                    "El usuario es requerido.",
                    CodigoValidacion);
            }

            string codigo = unidad.Trim();
            string descripcion = string.Empty;

            var result = DbHelper.WithConn(
                _portalDb,
                CodEmpresa,
                connection =>
                {
                    connection.Open();

                    using var transaction = connection.BeginTransaction();

                    try
                    {
                        descripcion = connection.QueryFirstOrDefault<string>(
                                QueryDescripcion,
                                new { CodUnidad = codigo },
                                transaction)
                            ?? string.Empty;

                        if (string.IsNullOrEmpty(descripcion))
                        {
                            transaction.Rollback();

                            return DbHelper.ErrorResponse(
                                "La unidad indicada no existe.",
                                CodigoValidacion);
                        }

                        int registros = connection.Execute(
                            QueryEliminar,
                            new { CodUnidad = codigo },
                            transaction);

                        if (registros == 0)
                        {
                            transaction.Rollback();

                            return DbHelper.ErrorResponse(
                                "La unidad indicada no existe.",
                                CodigoValidacion);
                        }

                        transaction.Commit();

                        return DbHelper.OkResponse(MensajeOk);
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                });

            var respuesta = INV_Unidades_Resultado_Obtener(
                result,
                ErrorEliminar);

            if (respuesta.Code == 0)
            {
                INV_Unidades_Bitacora_Registrar(
                    CodEmpresa,
                    usuario.Trim(),
                    "Elimina - WEB",
                    $"Unidad de Medida: {codigo} - {descripcion}");
            }

            return respuesta;
        }

        /// <summary>
        /// Crea una respuesta vacía para la lista paginada.
        /// </summary>
        /// <returns>Respuesta vacía inicializada.</returns>
        private static UnidadesDataLista INV_Unidades_Lista_Vacia()
        {
            return new UnidadesDataLista
            {
                total = 0,
                unidades = []
            };
        }

        /// <summary>
        /// Obtiene los parámetros normalizados de una unidad.
        /// </summary>
        /// <param name="request">Información de la unidad de medición.</param>
        /// <returns>Parámetros utilizados por las consultas.</returns>
        private static object INV_Unidades_Parametros_Obtener(
            UnidadMedicionDto request)
        {
            return new
            {
                CodUnidad = request.cod_unidad.Trim(),
                Descripcion = request.descripcion.Trim(),
                Hacienda = string.IsNullOrWhiteSpace(request.hacienda)
                    ? "Unid"
                    : request.hacienda.Trim(),
                Activo = request.activo,
                Usuario = request.registro_usuario.Trim()
            };
        }

        /// <summary>
        /// Valida la información requerida de una unidad.
        /// </summary>
        /// <param name="request">Información de la unidad de medición.</param>
        /// <param name="validarUsuario">Indica si debe validar el usuario.</param>
        /// <returns>Mensaje de validación o una cadena vacía.</returns>
        private static string INV_Unidades_Datos_Validar(
            UnidadMedicionDto? request,
            bool validarUsuario)
        {
            if (request is null)
            {
                return "La informaci&oacute;n de la unidad es requerida.";
            }

            if (string.IsNullOrWhiteSpace(request.cod_unidad))
            {
                return "El c&oacute;digo de la unidad es requerido.";
            }

            if (string.IsNullOrWhiteSpace(request.descripcion))
            {
                return "La descripci&oacute;n de la unidad es requerida.";
            }

            if (validarUsuario &&
                string.IsNullOrWhiteSpace(request.registro_usuario))
            {
                return "El usuario es requerido.";
            }

            return string.Empty;
        }

        /// <summary>
        /// Normaliza el campo utilizado para ordenar la lista.
        /// </summary>
        /// <param name="sortField">Campo de ordenamiento recibido.</param>
        /// <returns>Campo permitido para el ordenamiento.</returns>
        private static string INV_Unidades_Orden_Campo_Obtener(
            string? sortField)
        {
            return sortField?.Trim().ToLowerInvariant() switch
            {
                "descripcion" => "descripcion",
                "hacienda" => "hacienda",
                "activo" => "activo",
                _ => "cod_unidad"
            };
        }

        /// <summary>
        /// Normaliza la dirección utilizada para ordenar la lista.
        /// </summary>
        /// <param name="sortOrder">Dirección de ordenamiento recibida.</param>
        /// <returns>Uno para ascendente o dos para descendente.</returns>
        private static int INV_Unidades_Orden_Direccion_Obtener(
            int sortOrder)
        {
            return sortOrder == 2 ? 2 : 1;
        }

        /// <summary>
        /// Escapa los caracteres especiales utilizados por LIKE.
        /// </summary>
        /// <param name="filtro">Texto del filtro recibido.</param>
        /// <returns>Filtro normalizado para la consulta.</returns>
        private static string INV_Unidades_Filtro_Escapar(
            string filtro)
        {
            return filtro
                .Replace(
                    "\\",
                    "\\\\",
                    StringComparison.Ordinal)
                .Replace(
                    "%",
                    "\\%",
                    StringComparison.Ordinal)
                .Replace(
                    "_",
                    "\\_",
                    StringComparison.Ordinal)
                .Replace(
                    "[",
                    "\\[",
                    StringComparison.Ordinal);
        }

        /// <summary>
        /// Obtiene el resultado final generado por DbHelper.
        /// </summary>
        /// <param name="result">Resultado interno de DbHelper.</param>
        /// <param name="mensajeError">Mensaje de error predeterminado.</param>
        /// <returns>Resultado final de la operación.</returns>
        private static ErrorDto INV_Unidades_Resultado_Obtener(
            ErrorDto<ErrorDto> result,
            string mensajeError)
        {
            return result.Code == 0 && result.Result is not null
                ? result.Result
                : DbHelper.ErrorResponse(
                    result.Description ?? mensajeError,
                    result.Code.GetValueOrDefault(-1));
        }

        /// <summary>
        /// Registra en bitácora una operación realizada sobre una unidad.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="usuario">Usuario que ejecutó la operación.</param>
        /// <param name="movimiento">Tipo de movimiento realizado.</param>
        /// <param name="detalle">Detalle del movimiento.</param>
        /// <returns>No devuelve un valor.</returns>
        private void INV_Unidades_Bitacora_Registrar(
            int CodEmpresa,
            string usuario,
            string movimiento,
            string detalle)
        {
            _securityMainDb.Bitacora(
                new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = usuario.Trim(),
                    Movimiento = movimiento,
                    DetalleMovimiento = detalle,
                    Modulo = ModuloInventarios
                });
        }
    }
}