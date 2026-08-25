using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.ControlTramites;
using Galileo.Models.Security;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Galileo_API.DataBaseTier.ProGrX.ControlTramites
{
    public class FrmSifTagsModulosDb
    {
        private const int ModuloControlTramites = 8;

        private const string MensajeProcesoRequerido =
            "Debe indicar el código del proceso.";

        private const string MensajeEtiquetaRequerida =
            "Debe indicar el código de la etiqueta.";

        private const string MensajeUsuarioRequerido =
            "Debe indicar el usuario.";

        private const string MensajeAsignacionRequerida =
            "Debe indicar si la etiqueta está asignada.";

        private readonly PortalDB _portalDB;
        private readonly MSecurityMainDb _securityMainDB;

        public FrmSifTagsModulosDb(
            IConfiguration config)
        {
            _portalDB = new PortalDB(config);
            _securityMainDB = new MSecurityMainDb(config);
        }

        #region Procesos

        /// <summary>
        /// Obtiene todos los procesos configurados para la asignación de
        /// etiquetas.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<SifTagsModulosProcesoData>>
            SIF_TagsModulos_Procesos_Lista_Obtener(
                int CodEmpresa)
        {
            const string sql = """
                SELECT
                    RTRIM(COD_MODULO) AS cod_modulo,
                    RTRIM(ISNULL(DESCRIPCION, '')) AS descripcion
                FROM SIF_MODULOS_TAGS
                ORDER BY COD_MODULO;
                """;

            return EjecutarConsulta(
                CodEmpresa,
                connection => connection.Query<
                    SifTagsModulosProcesoData>(
                        sql).ToList(),
                new List<SifTagsModulosProcesoData>());
        }

        /// <summary>
        /// Obtiene todos los procesos configurados para exportación.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<SifTagsModulosProcesoData>>
            SIF_TagsModulos_Procesos_Lista_Export(
                int CodEmpresa)
        {
            return SIF_TagsModulos_Procesos_Lista_Obtener(
                CodEmpresa);
        }

        /// <summary>
        /// Obtiene los procesos disponibles para el selector de asignación
        /// de etiquetas.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>>
            SIF_TagsModulos_Procesos_Dropdown_Obtener(
                int CodEmpresa)
        {
            const string sql = """
                SELECT
                    RTRIM(COD_MODULO) AS item,
                    RTRIM(ISNULL(DESCRIPCION, '')) AS descripcion
                FROM SIF_MODULOS_TAGS
                ORDER BY DESCRIPCION;
                """;

            return EjecutarConsulta(
                CodEmpresa,
                connection => connection.Query<
                    DropDownListaGenericaModel>(
                        sql).ToList(),
                new List<DropDownListaGenericaModel>());
        }

        /// <summary>
        /// Registra un proceso nuevo o actualiza la descripción de un
        /// proceso existente.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto SIF_TagsModulos_Proceso_Guardar(
            int CodEmpresa,
            string? usuario,
            SifTagsModulosProcesoGuardarRequest? request)
        {
            string usuarioActual = (usuario ?? string.Empty)
                .Trim()
                .ToUpperInvariant();

            string validacion = ValidarProceso(
                request,
                usuarioActual);

            if (validacion.Length > 0)
            {
                return DbHelper.ErrorResponse(
                    validacion,
                    -2);
            }

            string codigo = (
                request?.cod_modulo ?? string.Empty)
                .Trim()
                .ToUpperInvariant();

            string descripcion = (
                request?.descripcion ?? string.Empty)
                .Trim();

            ErrorDto<int> resultado = EjecutarConsulta(
                CodEmpresa,
                connection => GuardarProceso(
                    connection,
                    codigo,
                    descripcion),
                0);

            if (resultado.Code != 0)
            {
                return DbHelper.ErrorResponse(
                    resultado.Description ??
                    "No fue posible guardar el proceso.",
                    resultado.Code ?? -1);
            }

            bool esActualizacion = resultado.Result == 1;

            RegistrarBitacoraProceso(
                CodEmpresa,
                usuarioActual,
                codigo,
                esActualizacion);

            string mensaje = esActualizacion
                ? "Proceso actualizado satisfactoriamente."
                : "Proceso registrado satisfactoriamente.";

            return DbHelper.OkResponse(mensaje);
        }

        /// <summary>
        /// Registra un proceso nuevo o actualiza la descripción del proceso
        /// existente y devuelve el tipo de operación realizada.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="codigo"></param>
        /// <param name="descripcion"></param>
        /// <returns></returns>
        private static int GuardarProceso(
            SqlConnection connection,
            string codigo,
            string descripcion)
        {
            const string sql = """
                IF EXISTS
                (
                    SELECT 1
                    FROM SIF_MODULOS_TAGS
                    WHERE COD_MODULO = @codigo
                )
                BEGIN
                    UPDATE SIF_MODULOS_TAGS
                    SET DESCRIPCION = @descripcion
                    WHERE COD_MODULO = @codigo;

                    SELECT 1;
                END
                ELSE
                BEGIN
                    INSERT INTO SIF_MODULOS_TAGS
                    (
                        COD_MODULO,
                        DESCRIPCION
                    )
                    VALUES
                    (
                        @codigo,
                        @descripcion
                    );

                    SELECT 0;
                END;
                """;

            return connection.QuerySingle<int>(
                sql,
                new
                {
                    codigo,
                    descripcion
                });
        }

        /// <summary>
        /// Valida la información requerida para registrar o actualizar un
        /// proceso.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        private static string ValidarProceso(
            SifTagsModulosProcesoGuardarRequest? request,
            string usuario)
        {
            if (request == null)
            {
                return "Debe indicar la información del proceso.";
            }

            if (usuario.Length == 0)
            {
                return MensajeUsuarioRequerido;
            }

            string codigo = request.cod_modulo.Trim();

            if (codigo.Length == 0)
            {
                return MensajeProcesoRequerido;
            }

            if (codigo.Length > 10)
            {
                return "El código del proceso no puede superar " +
                       "los 10 caracteres.";
            }

            if (request.descripcion.Trim().Length > 80)
            {
                return "La descripción del proceso no puede superar " +
                       "los 80 caracteres.";
            }

            return string.Empty;
        }

        /// <summary>
        /// Registra en bitácora la creación o modificación del proceso.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="codigo"></param>
        /// <param name="esActualizacion"></param>
        private void RegistrarBitacoraProceso(
            int CodEmpresa,
            string usuario,
            string codigo,
            bool esActualizacion)
        {
            _securityMainDB.Bitacora(
                new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = usuario,
                    DetalleMovimiento =
                        $"Procesos y Tag's: {codigo}",
                    Movimiento = esActualizacion
                        ? "MODIFICA-WEB"
                        : "REGISTRA-WEB",
                    Modulo = ModuloControlTramites
                });
        }

        #endregion

        #region Etiquetas por proceso

        /// <summary>
        /// Obtiene todas las etiquetas activas e indica cuáles están
        /// asignadas al proceso seleccionado.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="codModulo"></param>
        /// <returns></returns>
        public ErrorDto<List<SifTagsModulosEtiquetaData>>
            SIF_TagsModulos_Etiquetas_Lista_Obtener(
                int CodEmpresa,
                string? codModulo)
        {
            string codigo = (codModulo ?? string.Empty)
                .Trim()
                .ToUpperInvariant();

            if (codigo.Length == 0)
            {
                return DbHelper.CreateErrorResponse(
                    MensajeProcesoRequerido,
                    -2,
                    new List<SifTagsModulosEtiquetaData>());
            }

            const string sql = """
                SELECT
                    RTRIM(S.TAG_CODIGO) AS tag_codigo,
                    RTRIM(ISNULL(S.DESCRIPCION, '')) AS descripcion,
                    CAST
                    (
                        CASE
                            WHEN M.TAG_CODIGO IS NULL THEN 0
                            ELSE 1
                        END
                        AS bit
                    ) AS asignado
                FROM SIF_TAGS S
                LEFT JOIN SIF_TAGS_MODULOS M
                    ON S.TAG_CODIGO = M.TAG_CODIGO
                    AND M.COD_MODULO = @codigo
                WHERE S.ACTIVO = 1
                ORDER BY
                    CASE
                        WHEN M.TAG_CODIGO IS NULL THEN 0
                        ELSE 1
                    END DESC,
                    S.TAG_CODIGO;
                """;

            return EjecutarConsulta(
                CodEmpresa,
                connection => connection.Query<
                    SifTagsModulosEtiquetaData>(
                        sql,
                        new
                        {
                            codigo
                        }).ToList(),
                new List<SifTagsModulosEtiquetaData>());
        }

        /// <summary>
        /// Asigna o desasigna una etiqueta al proceso indicado.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto SIF_TagsModulos_Etiqueta_Guardar(
            int CodEmpresa,
            SifTagsModulosEtiquetaGuardarRequest? request)
        {
            string validacion = ValidarAsignacionEtiqueta(
                request);

            if (validacion.Length > 0)
            {
                return DbHelper.ErrorResponse(
                    validacion,
                    -2);
            }

            string codigoModulo = (
                request?.cod_modulo ?? string.Empty)
                .Trim()
                .ToUpperInvariant();

            string codigoEtiqueta = (
                request?.tag_codigo ?? string.Empty)
                .Trim()
                .ToUpperInvariant();

            bool asignado = request?.asignado ?? false;

            ErrorDto<int> resultado = EjecutarConsulta(
                CodEmpresa,
                connection => GuardarAsignacionEtiqueta(
                    connection,
                    codigoModulo,
                    codigoEtiqueta,
                    asignado),
                0);

            if (resultado.Code != 0)
            {
                return DbHelper.ErrorResponse(
                    resultado.Description ??
                    "No fue posible actualizar la asignación.",
                    resultado.Code ?? -1);
            }

            string mensaje = asignado
                ? "Etiqueta asignada satisfactoriamente."
                : "Etiqueta desasignada satisfactoriamente.";

            return DbHelper.OkResponse(mensaje);
        }

        /// <summary>
        /// Inserta o elimina la relación entre el proceso y la etiqueta.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="codigoModulo"></param>
        /// <param name="codigoEtiqueta"></param>
        /// <param name="asignado"></param>
        /// <returns></returns>
        private static int GuardarAsignacionEtiqueta(
            SqlConnection connection,
            string codigoModulo,
            string codigoEtiqueta,
            bool asignado)
        {
            const string sql = """
                IF @asignado = 1
                BEGIN
                    IF NOT EXISTS
                    (
                        SELECT 1
                        FROM SIF_TAGS_MODULOS
                        WHERE COD_MODULO = @codigoModulo
                          AND TAG_CODIGO = @codigoEtiqueta
                    )
                    BEGIN
                        INSERT INTO SIF_TAGS_MODULOS
                        (
                            COD_MODULO,
                            TAG_CODIGO
                        )
                        VALUES
                        (
                            @codigoModulo,
                            @codigoEtiqueta
                        );
                    END;
                END
                ELSE
                BEGIN
                    DELETE FROM SIF_TAGS_MODULOS
                    WHERE COD_MODULO = @codigoModulo
                      AND TAG_CODIGO = @codigoEtiqueta;
                END;

                SELECT @@ROWCOUNT;
                """;

            return connection.QuerySingle<int>(
                sql,
                new
                {
                    codigoModulo,
                    codigoEtiqueta,
                    asignado
                });
        }

        /// <summary>
        /// Valida la información requerida para asignar o desasignar una
        /// etiqueta.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        private static string ValidarAsignacionEtiqueta(
            SifTagsModulosEtiquetaGuardarRequest? request)
        {
            if (request == null)
            {
                return "Debe indicar la información de la etiqueta.";
            }

            string codigoModulo = request.cod_modulo.Trim();

            if (codigoModulo.Length == 0)
            {
                return MensajeProcesoRequerido;
            }

            if (codigoModulo.Length > 10)
            {
                return "El código del proceso no puede superar " +
                       "los 10 caracteres.";
            }

            string codigoEtiqueta = request.tag_codigo.Trim();

            if (codigoEtiqueta.Length == 0)
            {
                return MensajeEtiquetaRequerida;
            }

            if (codigoEtiqueta.Length > 10)
            {
                return "El código de la etiqueta no puede superar " +
                       "los 10 caracteres.";
            }

            if (!request.asignado.HasValue)
            {
                return MensajeAsignacionRequerida;
            }

            return string.Empty;
        }

        #endregion

        #region Ejecución común

        /// <summary>
        /// Ejecuta una operación sobre la conexión empresarial y centraliza
        /// el manejo de las excepciones esperadas.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="CodEmpresa"></param>
        /// <param name="operacion"></param>
        /// <param name="resultadoError"></param>
        /// <returns></returns>
        private ErrorDto<T> EjecutarConsulta<T>(
            int CodEmpresa,
            Func<SqlConnection, T> operacion,
            T resultadoError)
        {
            try
            {
                using var connection =
                    DbHelper.OpenConnection(
                        _portalDB,
                        CodEmpresa);

                return DbHelper.CreateOkResponse(
                    operacion(connection));
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse(
                    ex.Message,
                    -1,
                    resultadoError);
            }
            catch (InvalidOperationException ex)
            {
                return DbHelper.CreateErrorResponse(
                    ex.Message,
                    -1,
                    resultadoError);
            }
            catch (DataException ex)
            {
                return DbHelper.CreateErrorResponse(
                    ex.Message,
                    -1,
                    resultadoError);
            }
        }

        #endregion
    }
}