using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.Security;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using System.Data;
using static Galileo_API.Models.ProGrX_ControlTramites.frmSIFTagsModels;


namespace Galileo_API.DataBaseTier.ProGrX.ControlTramites
{
    public class FrmSifTagsDB
    {
        private const int ModuloControlTramites = 8;
        private const int CodigoValidacion = -2;
        private const int LongitudCodigo = 10;
        private const int LongitudDescripcion = 60;

        private const string MensajeCodigoRequerido =
            "Debe indicar el código de la etiqueta.";

        private const string MensajeEtiquetaSistema =
            "Este tag es de sistema, no puede modificarse.";

        private readonly PortalDB _portalDB;
        private readonly MSecurityMainDb _securityMainDb;

        public FrmSifTagsDB(IConfiguration config)
        {
            _portalDB = new PortalDB(config);
            _securityMainDb = new MSecurityMainDb(config);
        }

        #region Etiquetas

        /// <summary>
        /// Inicializa las etiquetas del sistema y obtiene la lista de etiquetas
        /// registradas.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtro"></param>
        /// <returns></returns>
        public ErrorDto<SifTagsListaResult> SIF_Tags_Lista_Obtener(
            int CodEmpresa,
            string filtro)
        {
            var filtrosResult = DeserializarFiltros(filtro);

            if (filtrosResult.Code != 0 || filtrosResult.Result == null)
            {
                return DbHelper.CreateErrorResponse(
                    filtrosResult.Description ?? "El filtro indicado no es válido.",
                    filtrosResult.Code ?? CodigoValidacion,
                    CrearListaVacia());
            }

            return EjecutarConsulta(
                CodEmpresa,
                connection => ObtenerLista(connection, filtrosResult.Result),
                CrearListaVacia());
        }

        /// <summary>
        /// Inserta o actualiza una etiqueta según el modo de edición indicado.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="vEdita"></param>
        /// <param name="Usuario"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public ErrorDto SIF_Tags_Guardar(
            int CodEmpresa,
            bool vEdita,
            string Usuario,
            SifTagsData param)
        {
            string? validacion = ValidarEtiqueta(param);

            if (!string.IsNullOrEmpty(validacion))
            {
                return DbHelper.ErrorResponse(validacion, CodigoValidacion);
            }

            string codigo = NormalizarCodigo(param.tag_codigo);

            if (EsEtiquetaSistema(codigo))
            {
                return DbHelper.ErrorResponse(
                    MensajeEtiquetaSistema,
                    CodigoValidacion);
            }

            return EjecutarAccion(
                CodEmpresa,
                connection => GuardarEtiqueta(
                    connection,
                    CodEmpresa,
                    Usuario,
                    vEdita,
                    codigo,
                    param));
        }

        /// <summary>
        /// Obtiene las etiquetas disponibles para los controles dropdown de
        /// notificaciones.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>>
            SIF_Tags_Dropdown_Obtener(int CodEmpresa)
        {
            return EjecutarConsulta(
                CodEmpresa,
                ObtenerEtiquetasDropdown,
                new List<DropDownListaGenericaModel>());
        }

        #endregion

        #region Notificaciones

        /// <summary>
        /// Obtiene la configuración de notificación de una etiqueta.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="tagCodigo"></param>
        /// <returns></returns>
        public ErrorDto<SifTagsNotificacionDto>
            SIF_Tags_Notificacion_Obtener(
                int CodEmpresa,
                string tagCodigo)
        {
            string codigo = NormalizarCodigo(tagCodigo);

            if (string.IsNullOrEmpty(codigo))
            {
                return DbHelper.CreateErrorResponse(
                    MensajeCodigoRequerido,
                    CodigoValidacion,
                    new SifTagsNotificacionDto());
            }

            return EjecutarConsulta(
                CodEmpresa,
                connection => ObtenerNotificacion(connection, codigo),
                new SifTagsNotificacionDto());
        }

        /// <summary>
        /// Inserta o actualiza la configuración de notificación de una
        /// etiqueta.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public ErrorDto SIF_Tags_Notificacion_Guardar(
            int CodEmpresa,
            SifTagsNotificacionDto param)
        {
            string? validacion = ValidarNotificacion(param);

            if (!string.IsNullOrEmpty(validacion))
            {
                return DbHelper.ErrorResponse(validacion, CodigoValidacion);
            }

            return EjecutarAccion(
                CodEmpresa,
                connection => GuardarNotificacion(connection, param));
        }

        /// <summary>
        /// Elimina la configuración de notificación asociada a una etiqueta.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="tagCodigo"></param>
        /// <returns></returns>
        public ErrorDto SIF_Tags_Notificacion_Eliminar(
            int CodEmpresa,
            string tagCodigo)
        {
            string codigo = NormalizarCodigo(tagCodigo);

            if (string.IsNullOrEmpty(codigo))
            {
                return DbHelper.ErrorResponse(
                    MensajeCodigoRequerido,
                    CodigoValidacion);
            }

            return EjecutarAccion(
                CodEmpresa,
                connection => EliminarNotificacion(connection, codigo));
        }

        #endregion

        #region Consultas privadas

        private static SifTagsListaResult ObtenerLista(
            SqlConnection connection,
            FiltrosLazyLoadData filtros)
        {
            connection.Execute(
                "dbo.spSIFTags",
                commandType: CommandType.StoredProcedure);

            string? texto = NormalizarFiltro(filtros.filtro);
            string? like = texto == null ? null : $"%{texto}%";
            string sortField = NormalizarSortField(filtros.sortField);
            int sortOrder = filtros.sortOrder == -1 ? -1 : 1;
            int offset = Math.Max(0, filtros.pagina);
            int fetch = filtros.paginacion > 0
                ? filtros.paginacion
                : int.MaxValue;

            const string query = """
                SELECT COUNT(1)
                FROM dbo.SIF_TAGS
                WHERE @texto IS NULL
                   OR TAG_CODIGO LIKE @like
                   OR DESCRIPCION LIKE @like;

                SELECT
                    RTRIM(TAG_CODIGO) AS tag_codigo,
                    RTRIM(DESCRIPCION) AS descripcion,
                    CAST(ACTIVO AS bit) AS activo
                FROM dbo.SIF_TAGS
                WHERE @texto IS NULL
                   OR TAG_CODIGO LIKE @like
                   OR DESCRIPCION LIKE @like
                ORDER BY
                    CASE
                        WHEN @sortField = 'tag_codigo' AND @sortOrder = 1
                        THEN TAG_CODIGO
                    END ASC,
                    CASE
                        WHEN @sortField = 'tag_codigo' AND @sortOrder = -1
                        THEN TAG_CODIGO
                    END DESC,
                    CASE
                        WHEN @sortField = 'descripcion' AND @sortOrder = 1
                        THEN DESCRIPCION
                    END ASC,
                    CASE
                        WHEN @sortField = 'descripcion' AND @sortOrder = -1
                        THEN DESCRIPCION
                    END DESC,
                    CASE
                        WHEN @sortField = 'activo' AND @sortOrder = 1
                        THEN ACTIVO
                    END ASC,
                    CASE
                        WHEN @sortField = 'activo' AND @sortOrder = -1
                        THEN ACTIVO
                    END DESC,
                    TAG_CODIGO ASC
                OFFSET @offset ROWS
                FETCH NEXT @fetch ROWS ONLY;
                """;

            using var reader = connection.QueryMultiple(
                query,
                new
                {
                    texto,
                    like,
                    sortField,
                    sortOrder,
                    offset,
                    fetch
                });

            return new SifTagsListaResult
            {
                total = reader.ReadSingle<int>(),
                lista = reader.Read<SifTagsData>().ToList()
            };
        }

        private static List<DropDownListaGenericaModel>
            ObtenerEtiquetasDropdown(SqlConnection connection)
        {
            const string query = """
                SELECT
                    RTRIM(TAG_CODIGO) AS item,
                    RTRIM(DESCRIPCION) AS descripcion
                FROM dbo.SIF_TAGS
                ORDER BY TAG_CODIGO;
                """;

            return connection
                .Query<DropDownListaGenericaModel>(query)
                .ToList();
        }

        private static SifTagsNotificacionDto ObtenerNotificacion(
            SqlConnection connection,
            string codigo)
        {
            const string query = """
                SELECT
                    RTRIM(CT.TAG_CODIGO) AS tag_codigo,
                    RTRIM(CT.PARA_TAG) AS para_tag,
                    RTRIM(TP.DESCRIPCION) AS para_tag_descripcion,
                    RTRIM(CT.PARA_EMAIL) AS para_email,
                    RTRIM(CT.CC_TAG) AS cc_tag,
                    RTRIM(TC.DESCRIPCION) AS cc_tag_descripcion,
                    RTRIM(CT.CC_EMAIL) AS cc_email,
                    RTRIM(CT.MENSAJE) AS mensaje
                FROM dbo.SIF_TAGS_AVISOS CT
                LEFT JOIN dbo.SIF_TAGS TP
                    ON CT.PARA_TAG = TP.TAG_CODIGO
                LEFT JOIN dbo.SIF_TAGS TC
                    ON CT.CC_TAG = TC.TAG_CODIGO
                WHERE CT.TAG_CODIGO = @codigo;
                """;

            return connection.QueryFirstOrDefault<SifTagsNotificacionDto>(
                query,
                new { codigo })
                ?? new SifTagsNotificacionDto
                {
                    tag_codigo = codigo
                };
        }

        #endregion

        #region Acciones privadas

        private ErrorDto GuardarEtiqueta(
            SqlConnection connection,
            int codEmpresa,
            string usuario,
            bool vEdita,
            string codigo,
            SifTagsData param)
        {
            string descripcion = NormalizarTexto(param.descripcion);
            short? activo = ConvertirActivo(param.activo);

            int afectados = vEdita
                ? ActualizarEtiqueta(
                    connection,
                    codigo,
                    descripcion,
                    activo)
                : InsertarEtiqueta(
                    connection,
                    codigo,
                    descripcion,
                    activo);

            if (afectados == 0)
            {
                return CrearErrorGuardarEtiqueta(vEdita);
            }

            RegistrarBitacora(
                codEmpresa,
                usuario,
                codigo,
                vEdita);

            string mensaje = vEdita
                ? "Información actualizada satisfactoriamente."
                : "Información guardada satisfactoriamente.";

            return DbHelper.OkResponse(mensaje);
        }

        private static int InsertarEtiqueta(
            SqlConnection connection,
            string codigo,
            string descripcion,
            short? activo)
        {
            const string query = """
                INSERT INTO dbo.SIF_TAGS
                (
                    TAG_CODIGO,
                    DESCRIPCION,
                    ACTIVO
                )
                SELECT
                    @codigo,
                    @descripcion,
                    @activo
                WHERE NOT EXISTS
                (
                    SELECT 1
                    FROM dbo.SIF_TAGS
                    WHERE TAG_CODIGO = @codigo
                );
                """;

            return connection.Execute(
                query,
                new
                {
                    codigo,
                    descripcion,
                    activo
                });
        }

        private static int ActualizarEtiqueta(
            SqlConnection connection,
            string codigo,
            string descripcion,
            short? activo)
        {
            const string query = """
                UPDATE dbo.SIF_TAGS
                SET DESCRIPCION = @descripcion,
                    ACTIVO = @activo
                WHERE TAG_CODIGO = @codigo;
                """;

            return connection.Execute(
                query,
                new
                {
                    codigo,
                    descripcion,
                    activo
                });
        }

        private static ErrorDto GuardarNotificacion(
            SqlConnection connection,
            SifTagsNotificacionDto param)
        {
            string codigo = NormalizarCodigo(param.tag_codigo);

            if (!EtiquetaExiste(connection, codigo))
            {
                return DbHelper.ErrorResponse(
                    "La etiqueta indicada no existe.",
                    CodigoValidacion);
            }

            var parameters = new
            {
                codigo,
                paraTag = NormalizarCodigoOpcional(param.para_tag),
                paraEmail = NormalizarTextoSinMayuscula(param.para_email),
                ccTag = NormalizarCodigoOpcional(param.cc_tag),
                ccEmail = NormalizarTextoSinMayuscula(param.cc_email),
                mensaje = NormalizarTextoSinMayuscula(param.mensaje)
            };

            const string query = """
                UPDATE dbo.SIF_TAGS_AVISOS
                SET PARA_TAG = @paraTag,
                    PARA_EMAIL = @paraEmail,
                    CC_TAG = @ccTag,
                    CC_EMAIL = @ccEmail,
                    MENSAJE = @mensaje
                WHERE TAG_CODIGO = @codigo;

                IF @@ROWCOUNT = 0
                BEGIN
                    INSERT INTO dbo.SIF_TAGS_AVISOS
                    (
                        TAG_CODIGO,
                        PARA_TAG,
                        PARA_EMAIL,
                        CC_TAG,
                        CC_EMAIL,
                        MENSAJE
                    )
                    VALUES
                    (
                        @codigo,
                        @paraTag,
                        @paraEmail,
                        @ccTag,
                        @ccEmail,
                        @mensaje
                    );
                END;
                """;

            connection.Execute(query, parameters);

            return DbHelper.OkResponse(
                "Información almacenada con éxito.");
        }

        private static ErrorDto EliminarNotificacion(
            SqlConnection connection,
            string codigo)
        {
            const string query = """
                DELETE FROM dbo.SIF_TAGS_AVISOS
                WHERE TAG_CODIGO = @codigo;
                """;

            connection.Execute(query, new { codigo });

            return DbHelper.OkResponse(
                "La notificación ha sido eliminada.");
        }

        private static bool EtiquetaExiste(
            SqlConnection connection,
            string codigo)
        {
            const string query = """
                SELECT COUNT(1)
                FROM dbo.SIF_TAGS
                WHERE TAG_CODIGO = @codigo;
                """;

            return connection.QuerySingle<int>(
                query,
                new { codigo }) > 0;
        }

        private void RegistrarBitacora(
            int codEmpresa,
            string usuario,
            string codigo,
            bool vEdita)
        {
            _securityMainDb.Bitacora(new BitacoraInsertarDto
            {
                EmpresaId = codEmpresa,
                Usuario = (usuario ?? string.Empty).ToUpperInvariant(),
                DetalleMovimiento = $"SIF Tipo de Etiqueta : {codigo}",
                Movimiento = vEdita
                    ? "MODIFICA-WEB"
                    : "REGISTRA-WEB",
                Modulo = ModuloControlTramites
            });
        }

        #endregion

        #region Validaciones

        private static string? ValidarEtiqueta(SifTagsData? param)
        {
            if (param == null)
            {
                return "Debe indicar la información de la etiqueta.";
            }

            string codigo = NormalizarCodigo(param.tag_codigo);

            if (string.IsNullOrEmpty(codigo))
            {
                return MensajeCodigoRequerido;
            }

            if (codigo.Length > LongitudCodigo)
            {
                return $"El código no puede superar {LongitudCodigo} caracteres.";
            }

            if ((param.descripcion?.Trim().Length ?? 0) > LongitudDescripcion)
            {
                return $"La descripción no puede superar {LongitudDescripcion} caracteres.";
            }

            return null;
        }

        private static string? ValidarNotificacion(SifTagsNotificacionDto? param)
        {
            if (param == null)
            {
                return "Debe indicar la información de la notificación.";
            }

            string codigo = NormalizarCodigo(param.tag_codigo);

            if (string.IsNullOrEmpty(codigo))
            {
                return MensajeCodigoRequerido;
            }

            return null;
        }
        private static ErrorDto CrearErrorGuardarEtiqueta(bool vEdita)
        {
            string mensaje = vEdita
                ? "La etiqueta indicada no existe."
                : "Ya existe una etiqueta con el código indicado.";

            return DbHelper.ErrorResponse(mensaje, CodigoValidacion);
        }

        #endregion

        #region Ejecución y normalización

        private ErrorDto<T> EjecutarConsulta<T>(
            int codEmpresa,
            Func<SqlConnection, T> action,
            T errorResult)
        {
            try
            {
                using var connection = DbHelper.OpenConnection(
                    _portalDB,
                    codEmpresa);

                return DbHelper.CreateOkResponse(action(connection));
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse(
                    ex.Message,
                    -1,
                    errorResult);
            }
            catch (InvalidOperationException ex)
            {
                return DbHelper.CreateErrorResponse(
                    ex.Message,
                    -1,
                    errorResult);
            }
        }

        private ErrorDto EjecutarAccion(
            int codEmpresa,
            Func<SqlConnection, ErrorDto> action)
        {
            try
            {
                using var connection = DbHelper.OpenConnection(
                    _portalDB,
                    codEmpresa);

                return action(connection);
            }
            catch (SqlException ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        private static ErrorDto<FiltrosLazyLoadData>
            DeserializarFiltros(string? filtro)
        {
            if (string.IsNullOrWhiteSpace(filtro))
            {
                return DbHelper.CreateOkResponse(
                    new FiltrosLazyLoadData());
            }

            try
            {
                var filtros = JsonConvert
                    .DeserializeObject<FiltrosLazyLoadData>(filtro)
                    ?? new FiltrosLazyLoadData();

                return DbHelper.CreateOkResponse(filtros);
            }
            catch (JsonException ex)
            {
                return DbHelper.CreateErrorResponse(
                    ex.Message,
                    CodigoValidacion,
                    new FiltrosLazyLoadData());
            }
        }

        private static SifTagsListaResult CrearListaVacia()
        {
            return new SifTagsListaResult
            {
                total = 0,
                lista = new List<SifTagsData>()
            };
        }

        private static string NormalizarCodigo(string? codigo)
        {
            return (codigo ?? string.Empty)
                .Trim()
                .ToUpperInvariant();
        }

        private static string NormalizarCodigoOpcional(string? codigo)
        {
            return NormalizarCodigo(codigo);
        }

        private static string NormalizarTexto(string? texto)
        {
            return (texto ?? string.Empty)
                .Trim()
                .ToUpperInvariant();
        }

        private static string NormalizarTextoSinMayuscula(string? texto)
        {
            return (texto ?? string.Empty).Trim();
        }

        private static string? NormalizarFiltro(string? filtro)
        {
            string texto = (filtro ?? string.Empty).Trim();

            return string.IsNullOrEmpty(texto)
                ? null
                : texto;
        }

        private static string NormalizarSortField(string? sortField)
        {
            return sortField switch
            {
                "descripcion" => "descripcion",
                "activo" => "activo",
                _ => "tag_codigo"
            };
        }

        private static bool EsEtiquetaSistema(string codigo)
        {
            return codigo.StartsWith(
                "S",
                StringComparison.Ordinal);
        }

        private static short? ConvertirActivo(bool? activo)
        {
            if (!activo.HasValue)
            {
                return null;
            }

            return activo.Value
                ? (short)1
                : (short)0;
        }

        #endregion
    }
}