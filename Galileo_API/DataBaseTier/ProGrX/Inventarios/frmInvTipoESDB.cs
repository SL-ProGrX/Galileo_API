using Dapper;
using Galileo.Models.ERROR;
using Galileo.Models.INV;
using Galileo.Models.Security;
using Microsoft.Data.SqlClient;

namespace Galileo.DataBaseTier
{
    public sealed class FrmInvTipoEsDb
    {
        private const int CodigoValidacion = -2;
        private const int ModuloInventarios = 32;
        private const int PaginacionPredeterminada = 30;
        private const int PaginacionMaxima = 500;
        private const int LongitudFiltroMaxima = 100;

        private const string MensajeListaError =
            "Ocurri&oacute; un error al consultar los tipos de movimientos.";

        private const string MensajeBuscarError =
            "Ocurri&oacute; un error al buscar los tipos de movimientos.";

        private const string MensajeRegistrarError =
            "Ocurri&oacute; un error al registrar el tipo de movimiento.";

        private const string MensajeActualizarError =
            "Ocurri&oacute; un error al actualizar el tipo de movimiento.";

        private const string MensajeEliminarError =
            "Ocurri&oacute; un error al eliminar el tipo de movimiento.";

        private readonly PortalDB _portalDb;
        private readonly MSecurityMainDb _securityMainDb;

        public FrmInvTipoEsDb(IConfiguration config)
        {
            ArgumentNullException.ThrowIfNull(config);

            _portalDb = new PortalDB(config);
            _securityMainDb = new MSecurityMainDb(config);
        }

        /// <summary>
        /// Obtiene los tipos de movimientos paginados.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="CodContabilidad"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDto<TipoESList> INV_TipoES_Lista_Obtener(
            int CodEmpresa,
            int CodContabilidad,
            TipoESFiltros? filtros)
        {
            var respuesta = INV_TipoES_Lista_Vacia();

            if (CodContabilidad <= 0)
            {
                return DbHelper.CreateErrorResponse(
                    "El c&oacute;digo de contabilidad es requerido.",
                    CodigoValidacion,
                    respuesta);
            }

            var pagina = Math.Max(
                0,
                filtros?.pagina ?? 0);

            var paginacionSolicitada =
                filtros?.paginacion ?? 0;

            var paginacion = paginacionSolicitada <= 0
                ? PaginacionPredeterminada
                : Math.Min(
                    paginacionSolicitada,
                    PaginacionMaxima);

            var filtro =
                filtros?.filtro?.Trim() ??
                string.Empty;

            if (filtro.Length > LongitudFiltroMaxima)
            {
                return DbHelper.CreateErrorResponse(
                    $"El filtro no puede superar los {LongitudFiltroMaxima} caracteres.",
                    CodigoValidacion,
                    respuesta);
            }

            var filtroLike = string.IsNullOrWhiteSpace(filtro)
                ? null
                : $"%{INV_TipoES_Filtro_Escapar(filtro)}%";

            const string queryTotal = """
                select count(1)
                from pv_entrada_salida T
                left join CntX_cuentas C
                    on C.cod_cuenta = T.cod_cuenta
                   and C.cod_contabilidad = @CodContabilidad
                where
                    @Filtro is null
                    or T.cod_entsal like @Filtro escape '\'
                    or T.descripcion like @Filtro escape '\'
                    or T.tipo like @Filtro escape '\'
                    or T.cod_cuenta like @Filtro escape '\'
                    or C.descripcion like @Filtro escape '\';
                """;

            const string queryLista = """
                select
                    rtrim(isnull(T.cod_entsal, ''))
                        as cod_entsal,
                    rtrim(isnull(T.descripcion, ''))
                        as descripcion,
                    rtrim(isnull(T.tipo, ''))
                        as tipo,
                    rtrim(isnull(T.cod_cuenta, ''))
                        as cod_cuenta,
                    cast(isnull(T.activo, 0) as bit)
                        as activo,
                    rtrim(isnull(C.descripcion, ''))
                        as cta_desc
                from pv_entrada_salida T
                left join CntX_cuentas C
                    on C.cod_cuenta = T.cod_cuenta
                   and C.cod_contabilidad = @CodContabilidad
                where
                    @Filtro is null
                    or T.cod_entsal like @Filtro escape '\'
                    or T.descripcion like @Filtro escape '\'
                    or T.tipo like @Filtro escape '\'
                    or T.cod_cuenta like @Filtro escape '\'
                    or C.descripcion like @Filtro escape '\'
                order by T.cod_entsal
                offset @Pagina rows
                fetch next @Paginacion rows only;
                """;

            var parametros = new
            {
                CodContabilidad,
                Filtro = filtroLike,
                Pagina = pagina,
                Paginacion = paginacion
            };

            var resultado = DbHelper.WithConn(
                _portalDb,
                CodEmpresa,
                connection =>
                {
                    var lista = INV_TipoES_Lista_Vacia();

                    lista.total =
                        connection.QueryFirstOrDefault<int>(
                            queryTotal,
                            parametros);

                    lista.lista =
                        connection.Query<TipoEsDto>(
                            queryLista,
                            parametros)
                        .ToList();

                    return lista;
                });

            if (resultado.Code != 0)
            {
                return DbHelper.CreateErrorResponse(
                    resultado.Description ??
                    MensajeListaError,
                    resultado.Code.GetValueOrDefault(-1),
                    respuesta);
            }

            return DbHelper.CreateOkResponse(
                resultado.Result ??
                respuesta);
        }

        /// <summary>
        /// Obtiene los tipos de movimientos de una categor&iacute;a.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="CodContabilidad"></param>
        /// <param name="tipo"></param>
        /// <returns></returns>
        public ErrorDto<List<TipoEsDto>> INV_TipoES_Tipo_Buscar(
            int CodEmpresa,
            int CodContabilidad,
            string? tipo)
        {
            var lista = new List<TipoEsDto>();

            if (CodContabilidad <= 0)
            {
                return DbHelper.CreateErrorResponse(
                    "El c&oacute;digo de contabilidad es requerido.",
                    CodigoValidacion,
                    lista);
            }

            var tipoNormalizado =
                INV_TipoES_Tipo_Normalizar(tipo);

            if (!InvTipoEsTiposMovimiento
                    .INV_TipoES_Tipo_Valido(tipoNormalizado))
            {
                return DbHelper.CreateErrorResponse(
                    "El tipo de movimiento no es v&aacute;lido.",
                    CodigoValidacion,
                    lista);
            }

            const string query = """
                select
                    rtrim(isnull(T.cod_entsal, ''))
                        as cod_entsal,
                    rtrim(isnull(T.descripcion, ''))
                        as descripcion,
                    rtrim(isnull(T.tipo, ''))
                        as tipo,
                    rtrim(isnull(T.cod_cuenta, ''))
                        as cod_cuenta,
                    cast(isnull(T.activo, 0) as bit)
                        as activo,
                    rtrim(isnull(C.descripcion, ''))
                        as cta_desc
                from pv_entrada_salida T
                left join CntX_cuentas C
                    on C.cod_cuenta = T.cod_cuenta
                   and C.cod_contabilidad = @CodContabilidad
                where T.tipo = @Tipo
                order by T.cod_entsal;
                """;

            var resultado =
                DbHelper.ExecuteListQuery<TipoEsDto>(
                    _portalDb,
                    CodEmpresa,
                    query,
                    new
                    {
                        CodContabilidad,
                        Tipo = tipoNormalizado
                    });

            if (resultado.Code != 0)
            {
                return DbHelper.CreateErrorResponse(
                    resultado.Description ??
                    MensajeBuscarError,
                    resultado.Code.GetValueOrDefault(-1),
                    lista);
            }

            return DbHelper.CreateOkResponse(
                resultado.Result ??
                lista);
        }

        /// <summary>
        /// Registra un tipo de movimiento.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto INV_TipoES_Registrar(
            int CodEmpresa,
            TipoEsGuardarRequest? request)
        {
            return INV_TipoES_Guardar(
                CodEmpresa,
                request,
                true);
        }

        /// <summary>
        /// Actualiza un tipo de movimiento.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto INV_TipoES_Actualizar(
            int CodEmpresa,
            TipoEsGuardarRequest? request)
        {
            return INV_TipoES_Guardar(
                CodEmpresa,
                request,
                false);
        }

        /// <summary>
        /// Elimina un tipo de movimiento.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto INV_TipoES_Eliminar(
            int CodEmpresa,
            TipoEsEliminarRequest? request)
        {
            var validacion =
                INV_TipoES_Eliminar_Validar(request);

            if (validacion is not null)
            {
                return validacion;
            }

            var codigo =
                INV_TipoES_Codigo_Normalizar(
                    request!.cod_entsal);

            var usuario = request.usuario.Trim();

            const string queryExiste = """
                select count(1)
                from pv_entrada_salida
                where cod_entsal = @cod_entsal;
                """;

            const string queryEliminar = """
                delete from pv_entrada_salida
                where cod_entsal = @cod_entsal;
                """;

            var resultado = DbHelper.WithConn(
                _portalDb,
                CodEmpresa,
                connection =>
                {
                    var parametros = new
                    {
                        cod_entsal = codigo
                    };

                    var existe =
                        connection.QueryFirstOrDefault<int>(
                            queryExiste,
                            parametros);

                    if (existe == 0)
                    {
                        return DbHelper.ErrorResponse(
                            "El tipo de movimiento indicado no existe.",
                            CodigoValidacion);
                    }

                    var filasAfectadas =
                        connection.Execute(
                            queryEliminar,
                            parametros);

                    return filasAfectadas > 0
                        ? DbHelper.OkResponse(
                            "Registro eliminado correctamente.")
                        : DbHelper.ErrorResponse(
                            MensajeEliminarError);
                });

            var respuesta =
                INV_TipoES_Resultado_Obtener(
                    resultado,
                    MensajeEliminarError);

            if (respuesta.Code == 0)
            {
                INV_TipoES_Bitacora_Registrar(
                    CodEmpresa,
                    usuario,
                    "Elimina - WEB",
                    codigo);
            }

            return respuesta;
        }

        private ErrorDto INV_TipoES_Guardar(
    int CodEmpresa,
    TipoEsGuardarRequest? request,
    bool esNuevo)
        {
            var validacion = INV_TipoES_Guardar_Validar(request);

            if (validacion is not null)
            {
                return validacion;
            }

            var solicitud = request!;
            var codigo = INV_TipoES_Codigo_Normalizar(solicitud.cod_entsal);
            var usuario = solicitud.usuario.Trim();
            var mensajeError = esNuevo
                ? MensajeRegistrarError
                : MensajeActualizarError;

            var resultado = DbHelper.WithConn(
                _portalDb,
                CodEmpresa,
                connection => INV_TipoES_Guardar_Ejecutar(
                    connection,
                    solicitud,
                    esNuevo));

            var respuesta = INV_TipoES_Resultado_Obtener(
                resultado,
                mensajeError);

            if (respuesta.Code == 0)
            {
                INV_TipoES_Bitacora_Registrar(
                    CodEmpresa,
                    usuario,
                    esNuevo ? "Registra - WEB" : "Modifica - WEB",
                    codigo);
            }

            return respuesta;
        }

        /// <summary>
        /// Ejecuta el registro o la actualizacion del tipo de movimiento.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="request"></param>
        /// <param name="esNuevo"></param>
        /// <returns></returns>
        private static ErrorDto INV_TipoES_Guardar_Ejecutar(
            SqlConnection connection,
            TipoEsGuardarRequest request,
            bool esNuevo)
        {
            const string queryExiste = """
            select count(1)
            from pv_entrada_salida
            where cod_entsal = @cod_entsal;
            """;

            const string queryInsertar = """
            insert into pv_entrada_salida
            (
                cod_entsal,
                descripcion,
                cod_cuenta,
                tipo,
                activo
            )
            values
            (
                @cod_entsal,
                @descripcion,
                @cod_cuenta,
                @tipo,
                @activo
            );
            """;

            const string queryActualizar = """
            update pv_entrada_salida
            set
                descripcion = @descripcion,
                cod_cuenta = @cod_cuenta,
                tipo = @tipo,
                activo = @activo
            where cod_entsal = @cod_entsal;
            """;

            var parametros = new
            {
                cod_entsal = INV_TipoES_Codigo_Normalizar(
                    request.cod_entsal),
                descripcion = request.descripcion
                    .Trim()
                    .ToUpperInvariant(),
                cod_cuenta = INV_TipoES_Cuenta_Normalizar(
                    request.cod_cuenta),
                tipo = INV_TipoES_Tipo_Normalizar(
                    request.tipo),
                request.activo
            };

            var existe = connection.QueryFirstOrDefault<int>(
                queryExiste,
                new
                {
                    parametros.cod_entsal
                }) > 0;

            var errorExistencia = INV_TipoES_Guardar_Existencia_Validar(
                esNuevo,
                existe);

            if (errorExistencia is not null)
            {
                return errorExistencia;
            }

            var query = esNuevo
                ? queryInsertar
                : queryActualizar;

            var filasAfectadas = connection.Execute(
                query,
                parametros);

            if (filasAfectadas <= 0)
            {
                return DbHelper.ErrorResponse(
                    esNuevo
                        ? MensajeRegistrarError
                        : MensajeActualizarError);
            }

            return DbHelper.OkResponse(
                esNuevo
                    ? "Registro agregado correctamente."
                    : "Registro actualizado correctamente.");
        }

        /// <summary>
        /// Valida la existencia del tipo de movimiento segun la operacion solicitada.
        /// </summary>
        /// <param name="esNuevo"></param>
        /// <param name="existe"></param>
        /// <returns></returns>
        private static ErrorDto? INV_TipoES_Guardar_Existencia_Validar(
            bool esNuevo,
            bool existe)
        {
            if (esNuevo && existe)
            {
                return DbHelper.ErrorResponse(
                    "El c&oacute;digo del tipo de movimiento ya existe.",
                    CodigoValidacion);
            }

            if (!esNuevo && !existe)
            {
                return DbHelper.ErrorResponse(
                    "El tipo de movimiento indicado no existe.",
                    CodigoValidacion);
            }

            return null;
        }

        private static ErrorDto? INV_TipoES_Guardar_Validar(
            TipoEsGuardarRequest? request)
        {
            if (request is null)
            {
                return DbHelper.ErrorResponse(
                    "La solicitud es requerida.",
                    CodigoValidacion);
            }

            if (string.IsNullOrWhiteSpace(
                    request.cod_entsal))
            {
                return DbHelper.ErrorResponse(
                    "El c&oacute;digo del tipo de movimiento es requerido.",
                    CodigoValidacion);
            }

            if (string.IsNullOrWhiteSpace(
                    request.descripcion))
            {
                return DbHelper.ErrorResponse(
                    "La descripci&oacute;n del tipo de movimiento es requerida.",
                    CodigoValidacion);
            }

            var tipo =
                INV_TipoES_Tipo_Normalizar(
                    request.tipo);

            if (!InvTipoEsTiposMovimiento
                    .INV_TipoES_Tipo_Valido(tipo))
            {
                return DbHelper.ErrorResponse(
                    "El tipo de movimiento no es v&aacute;lido.",
                    CodigoValidacion);
            }

            if (string.IsNullOrWhiteSpace(
                    request.usuario))
            {
                return DbHelper.ErrorResponse(
                    "El usuario es requerido.",
                    CodigoValidacion);
            }

            return null;
        }

        private static ErrorDto? INV_TipoES_Eliminar_Validar(
            TipoEsEliminarRequest? request)
        {
            if (request is null)
            {
                return DbHelper.ErrorResponse(
                    "La solicitud es requerida.",
                    CodigoValidacion);
            }

            if (string.IsNullOrWhiteSpace(
                    request.cod_entsal))
            {
                return DbHelper.ErrorResponse(
                    "El c&oacute;digo del tipo de movimiento es requerido.",
                    CodigoValidacion);
            }

            if (string.IsNullOrWhiteSpace(
                    request.usuario))
            {
                return DbHelper.ErrorResponse(
                    "El usuario es requerido.",
                    CodigoValidacion);
            }

            return null;
        }

        private static ErrorDto INV_TipoES_Resultado_Obtener(
            ErrorDto<ErrorDto> resultado,
            string mensajeError)
        {
            if (resultado.Code != 0)
            {
                return DbHelper.ErrorResponse(
                    resultado.Description ??
                    mensajeError,
                    resultado.Code.GetValueOrDefault(-1));
            }

            return resultado.Result ??
                   DbHelper.ErrorResponse(
                       mensajeError);
        }

        private static TipoESList INV_TipoES_Lista_Vacia()
        {
            return new TipoESList
            {
                total = 0,
                lista = []
            };
        }

        private static string INV_TipoES_Codigo_Normalizar(
            string? codigo)
        {
            return (codigo ?? string.Empty)
                .Trim()
                .ToUpperInvariant();
        }

        private static string INV_TipoES_Tipo_Normalizar(
            string? tipo)
        {
            var valor = (tipo ?? string.Empty)
                .Trim()
                .ToUpperInvariant();

            return string.IsNullOrEmpty(valor)
                ? string.Empty
                : valor[..1];
        }

        private static string INV_TipoES_Cuenta_Normalizar(
            string? cuenta)
        {
            return string.Concat(
                (cuenta ?? string.Empty)
                .Where(char.IsLetterOrDigit));
        }

        private static string INV_TipoES_Filtro_Escapar(
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

        private void INV_TipoES_Bitacora_Registrar(
            int CodEmpresa,
            string usuario,
            string movimiento,
            string codigo)
        {
            _securityMainDb.Bitacora(
                new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = usuario,
                    Movimiento = movimiento,
                    DetalleMovimiento =
                        $"Tipo de E/S/T Cod: {codigo}",
                    Modulo = ModuloInventarios
                });
        }
    }
}