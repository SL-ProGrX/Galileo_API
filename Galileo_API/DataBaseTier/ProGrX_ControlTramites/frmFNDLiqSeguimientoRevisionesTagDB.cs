using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX_ControlTramites;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using System.Data;
using System.Globalization;

namespace Galileo_API.DataBaseTier.ProGrX_ControlTramites
{
    public class frmFNDLiqSeguimientoRevisionesTagDB
    {
        private const int ModuloControlTramites = 8;
        private const string CodigoModulo = "FLQ";
        private const string MensajeConsecutivoRequerido =
            "Debe indicar una liquidación válida.";
        private const string MensajeUsuarioRequerido =
            "Debe indicar el usuario.";

        private static readonly IReadOnlyDictionary<string, int>
            OrdenLiquidaciones = new Dictionary<string, int>(
                StringComparer.OrdinalIgnoreCase)
            {
                ["consecutivo"] = 1,
                ["cedula"] = 2,
                ["nombre"] = 3,
                ["usuario"] = 4,
                ["fecha"] = 5,
                ["cod_plan"] = 6,
                ["cod_contrato"] = 7,
                ["retiene"] = 8,
                ["banco_descripcion"] = 9
            };

        private readonly PortalDB _portalDB;
        private readonly MSecurityMainDb _securityMainDB;

        public frmFNDLiqSeguimientoRevisionesTagDB(
            IConfiguration config)
        {
            _portalDB = new PortalDB(config);
            _securityMainDB = new MSecurityMainDb(config);
        }

        #region Liquidaciones

        /// <summary>
        /// Obtiene las liquidaciones de fondos pendientes de revisión mediante
        /// lazy loading, filtro global, ordenamiento y paginación.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="parametros"></param>
        /// <param name="soloSinRetencion"></param>
        /// <returns></returns>
        public ErrorDto<
            FndLiqSeguimientoRevisionesTagLiquidacionesListaResult>
            FND_LiqSeguimientoRevisionesTag_Liquidaciones_Lista_Obtener(
                int CodEmpresa,
                string parametros,
                bool soloSinRetencion)
        {
            if (!TryObtenerFiltros(
                parametros,
                out FiltrosLazyLoadData filtros,
                out string mensajeError))
            {
                return DbHelper.CreateErrorResponse(
                    mensajeError,
                    -1,
                    CrearListaLiquidacionesVacia());
            }

            return EjecutarOperacion(
                CodEmpresa,
                connection =>
                {
                    FndLiqSeguimientoRevisionesTagLiquidacionesListaResult
                        resultado = ObtenerLiquidaciones(
                            connection,
                            filtros,
                            soloSinRetencion);

                    return DbHelper.CreateOkResponse(resultado);
                },
                mensaje => DbHelper.CreateErrorResponse(
                    mensaje,
                    -1,
                    CrearListaLiquidacionesVacia()));
        }

        /// <summary>
        /// Exporta todas las liquidaciones pendientes de revisión respetando
        /// el filtro, ordenamiento y tipo de salida indicados.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="parametros"></param>
        /// <param name="soloSinRetencion"></param>
        /// <returns></returns>
        public ErrorDto<
            FndLiqSeguimientoRevisionesTagLiquidacionesListaResult>
            FND_LiqSeguimientoRevisionesTag_Liquidaciones_Lista_Export(
                int CodEmpresa,
                string parametros,
                bool soloSinRetencion)
        {
            if (!TryObtenerFiltros(
                parametros,
                out FiltrosLazyLoadData filtros,
                out string mensajeError))
            {
                return DbHelper.CreateErrorResponse(
                    mensajeError,
                    -1,
                    CrearListaLiquidacionesVacia());
            }

            filtros.pagina = 0;
            filtros.paginacion = 0;

            return FND_LiqSeguimientoRevisionesTag_Liquidaciones_Lista_Obtener(
                CodEmpresa,
                JsonConvert.SerializeObject(filtros),
                soloSinRetencion);
        }

        /// <summary>
        /// Obtiene la última liquidación pendiente de revisión para una cédula.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="cedula"></param>
        /// <returns></returns>
        public ErrorDto<FndLiqSeguimientoRevisionesTagLiquidacionData> FND_LiqSeguimientoRevisionesTag_Nombre_Obtener(int CodEmpresa, string? cedula)
        {
            return DbHelper.WithConn(_portalDB, CodEmpresa, connection =>
            {
                const string sql = """
            SELECT TOP (1)
                ISNULL(L.CONSEC, 0) AS consecutivo,
                RTRIM(ISNULL(F.CEDULA, '')) AS cedula,
                RTRIM(ISNULL(S.NOMBRE, '')) AS nombre,
                RTRIM(ISNULL(L.USUARIO, '')) AS usuario,
                L.FECHA AS fecha,
                RTRIM(ISNULL(L.COD_PLAN, '')) AS cod_plan,
                ISNULL(L.COD_CONTRATO, 0) AS cod_contrato,
                CASE
                    WHEN L.RETENCION_CODIGO IS NULL THEN 'No'
                    ELSE 'Sí'
                END AS retiene,
                RTRIM(ISNULL(BAN.DESCRIPCION, '')) AS banco_descripcion
            FROM FND_LIQUIDACION L
            INNER JOIN FND_CONTRATOS F
                ON L.COD_PLAN = F.COD_PLAN
                AND L.COD_CONTRATO = F.COD_CONTRATO
                AND L.COD_OPERADORA = F.COD_OPERADORA
            INNER JOIN SOCIOS S
                ON F.CEDULA = S.CEDULA
            LEFT JOIN TES_BANCOS BAN
                ON L.COD_BANCO = BAN.ID_BANCO
            WHERE ISNULL(L.ANALISTA_REVISION, 'N') = 'N'
              AND F.CEDULA = @cedula
            ORDER BY L.CONSEC DESC;
            """;

                return connection.QueryFirstOrDefault<FndLiqSeguimientoRevisionesTagLiquidacionData>(
                    sql,
                    new
                    {
                        cedula = (cedula ?? string.Empty).Trim()
                    })
                    ?? new FndLiqSeguimientoRevisionesTagLiquidacionData();
            });
        }

        private static
            FndLiqSeguimientoRevisionesTagLiquidacionesListaResult
            ObtenerLiquidaciones(
                SqlConnection connection,
                FiltrosLazyLoadData filtros,
                bool soloSinRetencion)
        {
            string texto = (filtros.filtro ?? string.Empty).Trim();
            string? filtro = texto.Length == 0 ? null : texto;
            string? like = filtro == null ? null : $"%{filtro}%";

            int offset = Math.Max(0, filtros.pagina);
            int fetch = Math.Max(0, filtros.paginacion);
            bool usarPaginacion = fetch > 0;

            int orden = ResolverOrdenLiquidaciones(
                filtros.sortField);

            int ascendente = filtros.sortOrder == 1 ? 1 : 0;

            const string sqlCount = """
                SELECT COUNT(1)
                FROM FND_LIQUIDACION L
                INNER JOIN FND_CONTRATOS F
                    ON L.COD_PLAN = F.COD_PLAN
                    AND L.COD_CONTRATO = F.COD_CONTRATO
                    AND L.COD_OPERADORA = F.COD_OPERADORA
                INNER JOIN SOCIOS S
                    ON F.CEDULA = S.CEDULA
                LEFT JOIN SIF_OFICINAS O
                    ON L.COD_OFICINA = O.COD_OFICINA
                LEFT JOIN TES_BANCOS BAN
                    ON L.COD_BANCO = BAN.ID_BANCO
                WHERE ISNULL(L.ANALISTA_REVISION, 'N') = 'N'
                  AND
                  (
                      @soloSinRetencion = 0
                      OR L.RETENCION_CODIGO IS NULL
                  )
                  AND
                  (
                      @filtro IS NULL
                      OR F.CEDULA LIKE @like
                      OR S.NOMBRE LIKE @like
                      OR L.USUARIO LIKE @like
                      OR L.COD_PLAN LIKE @like
                      OR CONVERT(
                          VARCHAR(30),
                          L.COD_CONTRATO
                      ) LIKE @like
                      OR CONVERT(
                          VARCHAR(30),
                          L.CONSEC
                      ) LIKE @like
                      OR BAN.DESCRIPCION LIKE @like
                  );
                """;

            var parametros = new
            {
                filtro,
                like,
                soloSinRetencion
            };

            int total = connection.QuerySingle<int>(
                sqlCount,
                parametros);

            string sqlLista = """
                SELECT
                    L.CONSEC AS consecutivo,
                    RTRIM(F.CEDULA) AS cedula,
                    ISNULL(RTRIM(S.NOMBRE), '') AS nombre,
                    ISNULL(RTRIM(L.USUARIO), '') AS usuario,
                    L.FECHA AS fecha,
                    ISNULL(RTRIM(L.COD_PLAN), '') AS cod_plan,
                    L.COD_CONTRATO AS cod_contrato,
                    CASE
                        WHEN L.RETENCION_CODIGO IS NULL
                        THEN 'No'
                        ELSE 'Sí'
                    END AS retiene,
                    ISNULL(RTRIM(BAN.DESCRIPCION), '')
                        AS banco_descripcion
                FROM FND_LIQUIDACION L
                INNER JOIN FND_CONTRATOS F
                    ON L.COD_PLAN = F.COD_PLAN
                    AND L.COD_CONTRATO = F.COD_CONTRATO
                    AND L.COD_OPERADORA = F.COD_OPERADORA
                INNER JOIN SOCIOS S
                    ON F.CEDULA = S.CEDULA
                LEFT JOIN SIF_OFICINAS O
                    ON L.COD_OFICINA = O.COD_OFICINA
                LEFT JOIN TES_BANCOS BAN
                    ON L.COD_BANCO = BAN.ID_BANCO
                WHERE ISNULL(L.ANALISTA_REVISION, 'N') = 'N'
                  AND
                  (
                      @soloSinRetencion = 0
                      OR L.RETENCION_CODIGO IS NULL
                  )
                  AND
                  (
                      @filtro IS NULL
                      OR F.CEDULA LIKE @like
                      OR S.NOMBRE LIKE @like
                      OR L.USUARIO LIKE @like
                      OR L.COD_PLAN LIKE @like
                      OR CONVERT(
                          VARCHAR(30),
                          L.COD_CONTRATO
                      ) LIKE @like
                      OR CONVERT(
                          VARCHAR(30),
                          L.CONSEC
                      ) LIKE @like
                      OR BAN.DESCRIPCION LIKE @like
                  )
                ORDER BY
                    CASE
                        WHEN @orden = 1 AND @ascendente = 1
                        THEN L.CONSEC
                    END ASC,
                    CASE
                        WHEN @orden = 1 AND @ascendente = 0
                        THEN L.CONSEC
                    END DESC,

                    CASE
                        WHEN @orden = 2 AND @ascendente = 1
                        THEN F.CEDULA
                    END ASC,
                    CASE
                        WHEN @orden = 2 AND @ascendente = 0
                        THEN F.CEDULA
                    END DESC,

                    CASE
                        WHEN @orden = 3 AND @ascendente = 1
                        THEN S.NOMBRE
                    END ASC,
                    CASE
                        WHEN @orden = 3 AND @ascendente = 0
                        THEN S.NOMBRE
                    END DESC,

                    CASE
                        WHEN @orden = 4 AND @ascendente = 1
                        THEN L.USUARIO
                    END ASC,
                    CASE
                        WHEN @orden = 4 AND @ascendente = 0
                        THEN L.USUARIO
                    END DESC,

                    CASE
                        WHEN @orden = 5 AND @ascendente = 1
                        THEN L.FECHA
                    END ASC,
                    CASE
                        WHEN @orden = 5 AND @ascendente = 0
                        THEN L.FECHA
                    END DESC,

                    CASE
                        WHEN @orden = 6 AND @ascendente = 1
                        THEN L.COD_PLAN
                    END ASC,
                    CASE
                        WHEN @orden = 6 AND @ascendente = 0
                        THEN L.COD_PLAN
                    END DESC,

                    CASE
                        WHEN @orden = 7 AND @ascendente = 1
                        THEN L.COD_CONTRATO
                    END ASC,
                    CASE
                        WHEN @orden = 7 AND @ascendente = 0
                        THEN L.COD_CONTRATO
                    END DESC,

                    CASE
                        WHEN @orden = 8 AND @ascendente = 1
                        THEN
                            CASE
                                WHEN L.RETENCION_CODIGO IS NULL
                                THEN 0
                                ELSE 1
                            END
                    END ASC,
                    CASE
                        WHEN @orden = 8 AND @ascendente = 0
                        THEN
                            CASE
                                WHEN L.RETENCION_CODIGO IS NULL
                                THEN 0
                                ELSE 1
                            END
                    END DESC,

                    CASE
                        WHEN @orden = 9 AND @ascendente = 1
                        THEN BAN.DESCRIPCION
                    END ASC,
                    CASE
                        WHEN @orden = 9 AND @ascendente = 0
                        THEN BAN.DESCRIPCION
                    END DESC,

                    L.CONSEC DESC
                """;

            if (usarPaginacion)
            {
                sqlLista += """

                    OFFSET @offset ROWS
                    FETCH NEXT @fetch ROWS ONLY;
                    """;
            }

            var lista = connection.Query<
                FndLiqSeguimientoRevisionesTagLiquidacionData>(
                    sqlLista,
                    new
                    {
                        filtro,
                        like,
                        soloSinRetencion,
                        orden,
                        ascendente,
                        offset,
                        fetch
                    }).ToList();

            return new
                FndLiqSeguimientoRevisionesTagLiquidacionesListaResult
            {
                total = total,
                lista = lista
            };
        }

        private static int ResolverOrdenLiquidaciones(
            string? sortField)
        {
            string campo = (sortField ?? string.Empty).Trim();

            return OrdenLiquidaciones.TryGetValue(
                campo,
                out int orden)
                ? orden
                : OrdenLiquidaciones["consecutivo"];
        }

        private static bool TryObtenerFiltros(
            string parametros,
            out FiltrosLazyLoadData filtros,
            out string mensajeError)
        {
            try
            {
                filtros = string.IsNullOrWhiteSpace(parametros)
                    ? new FiltrosLazyLoadData()
                    : JsonConvert.DeserializeObject<FiltrosLazyLoadData>(
                        parametros) ?? new FiltrosLazyLoadData();

                mensajeError = string.Empty;
                return true;
            }
            catch (JsonException ex)
            {
                filtros = new FiltrosLazyLoadData();
                mensajeError = ex.Message;
                return false;
            }
        }

        private static
            FndLiqSeguimientoRevisionesTagLiquidacionesListaResult
            CrearListaLiquidacionesVacia()
        {
            return new
                FndLiqSeguimientoRevisionesTagLiquidacionesListaResult
            {
                total = 0,
                lista = new List<
                    FndLiqSeguimientoRevisionesTagLiquidacionData>()
            };
        }

        #endregion

        #region Seguimiento

        /// <summary>
        /// Obtiene el historial de etiquetas registradas para la liquidación.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="consecutivo"></param>
        /// <returns></returns>
        public ErrorDto<
            List<FndLiqSeguimientoRevisionesTagSeguimientoData>>
            FND_LiqSeguimientoRevisionesTag_Seguimiento_Lista_Obtener(
                int CodEmpresa,
                long consecutivo)
        {
            if (consecutivo <= 0)
            {
                return DbHelper.CreateErrorResponse(
                    MensajeConsecutivoRequerido,
                    -2,
                    new List<
                        FndLiqSeguimientoRevisionesTagSeguimientoData>());
            }

            const string sql = """
                SELECT
                    ISNULL(RTRIM(OT.TAG_CODIGO), '') AS tag_codigo,
                    ISNULL(RTRIM(T.DESCRIPCION), '')
                        AS tag_descripcion,
                    ISNULL(OT.NOTAS, '') AS notas,
                    OT.REGISTRO_FECHA AS registro_fecha,
                    ISNULL(RTRIM(OT.REGISTRO_USUARIO), '')
                        AS registro_usuario
                FROM SIF_CONTROL_TAGS OT
                INNER JOIN SIF_TAGS T
                    ON OT.TAG_CODIGO = T.TAG_CODIGO
                WHERE OT.DOCUMENTO = @documento
                  AND OT.COD_MODULO = @modulo;
                """;

            string documento = Convert.ToString(
                consecutivo,
                CultureInfo.InvariantCulture);

            return EjecutarOperacion(
                CodEmpresa,
                connection =>
                {
                    var lista = connection.Query<
                        FndLiqSeguimientoRevisionesTagSeguimientoData>(
                            sql,
                            new
                            {
                                documento,
                                modulo = CodigoModulo
                            }).ToList();

                    return DbHelper.CreateOkResponse(lista);
                },
                mensaje => DbHelper.CreateErrorResponse(
                    mensaje,
                    -1,
                    new List<
                        FndLiqSeguimientoRevisionesTagSeguimientoData>()));
        }

        /// <summary>
        /// Exporta el historial completo de etiquetas de la liquidación.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="consecutivo"></param>
        /// <returns></returns>
        public ErrorDto<
            List<FndLiqSeguimientoRevisionesTagSeguimientoData>>
            FND_LiqSeguimientoRevisionesTag_Seguimiento_Lista_Export(
                int CodEmpresa,
                long consecutivo)
        {
            return
                FND_LiqSeguimientoRevisionesTag_Seguimiento_Lista_Obtener(
                    CodEmpresa,
                    consecutivo);
        }

        #endregion

        #region Revisión

        /// <summary>
        /// Obtiene las etiquetas activas de Liquidación de Fondos autorizadas
        /// para el usuario.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto<List<FndLiqSeguimientoRevisionesTagEtiquetaData>>FND_LiqSeguimientoRevisionesTag_Etiquetas_Dropdown_Obtener(int CodEmpresa, string? usuario)
        {
            string usuarioActual = (usuario ?? string.Empty)
                .Trim()
                .ToUpperInvariant();

            if (usuarioActual.Length == 0)
            {
                return DbHelper.CreateErrorResponse(MensajeUsuarioRequerido,-2,new List<FndLiqSeguimientoRevisionesTagEtiquetaData>());
            }

            const string sql = """
                SELECT DISTINCT
                    RTRIM(CT.TAG_CODIGO) AS tag_codigo,
                    RTRIM(CT.DESCRIPCION) AS tag_descripcion,
                    ISNULL(AV.MENSAJE, '') AS mensaje
                FROM SIF_TAGS CT
                INNER JOIN SIF_TAGS_GRUPOS CTG
                    ON CT.TAG_CODIGO = CTG.TAG_CODIGO
                INNER JOIN SIF_GRPUSERS CGU
                    ON CTG.COD_GRUPO = CGU.COD_GRUPO
                INNER JOIN SIF_TAGS_MODULOS CTM
                    ON CT.TAG_CODIGO = CTM.TAG_CODIGO
                    AND CTM.COD_MODULO = @modulo
                OUTER APPLY
                (
                    SELECT TOP 1
                        ISNULL(STA.MENSAJE, '') AS MENSAJE
                    FROM SIF_TAGS_AVISOS STA
                    WHERE STA.TAG_CODIGO = CT.TAG_CODIGO
                ) AV
                WHERE CT.ACTIVO = 1
                  AND CGU.USUARIO = @usuario
                ORDER BY tag_codigo;
                """;

            return EjecutarOperacion(
                CodEmpresa,
                connection =>
                {
                    var etiquetas = connection.Query<
                        FndLiqSeguimientoRevisionesTagEtiquetaData>(
                            sql,
                            new
                            {
                                usuario = usuarioActual,
                                modulo = CodigoModulo
                            }).ToList();

                    return DbHelper.CreateOkResponse(etiquetas);
                },
                mensaje => DbHelper.CreateErrorResponse(
                    mensaje,
                    -1,
                    new List<
                        FndLiqSeguimientoRevisionesTagEtiquetaData>()));
        }

        /// <summary>
        /// Obtiene las omisiones configuradas para Liquidación de Fondos y su
        /// estado dentro de la liquidación seleccionada.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="cedula"></param>
        /// <param name="consecutivo"></param>
        /// <returns></returns>
        public ErrorDto<List<FndLiqSeguimientoRevisionesTagRevisionData>> FND_LiqSeguimientoRevisionesTag_Revisiones_Lista_Obtener(int CodEmpresa, string? cedula, long consecutivo)
        {
            string identificacion =
                (cedula ?? string.Empty).Trim();

            if (identificacion.Length == 0)
            {
                return DbHelper.CreateErrorResponse(
                    "Debe indicar la cédula.",
                    -2,
                    new List<
                        FndLiqSeguimientoRevisionesTagRevisionData>());
            }

            if (consecutivo <= 0)
            {
                return DbHelper.CreateErrorResponse(
                    "Debe indicar la liquidación.",
                    -2,
                    new List<
                        FndLiqSeguimientoRevisionesTagRevisionData>());
            }

            const string sql = """
            DECLARE @codigo VARCHAR(60);

            SELECT TOP (1)
                @codigo =
                    RTRIM(ISNULL(L.COD_PLAN, ''))
                    + '-'
                    + CONVERT(VARCHAR(30), L.COD_CONTRATO)
            FROM FND_LIQUIDACION L
            INNER JOIN FND_CONTRATOS F
                ON L.COD_PLAN = F.COD_PLAN
                AND L.COD_CONTRATO = F.COD_CONTRATO
                AND L.COD_OPERADORA = F.COD_OPERADORA
            WHERE L.CONSEC = @consecutivo
              AND F.CEDULA = @cedula;

            SELECT
                E.ID_ERROR AS id_error,
                ISNULL(RTRIM(E.DESCRIPCION), '')
                    AS descripcion,
                CONVERT(
                    BIT,
                    CASE
                        WHEN ER.ID_ERROR IS NULL THEN 0
                        ELSE 1
                    END
                ) AS seleccionado,
                CONVERT(
                    BIT,
                    CASE
                        WHEN ISNULL(ER.APLICADO, 'N') = 'S'
                        THEN 1
                        ELSE 0
                    END
                ) AS aplicado,
                ISNULL(E.MENSAJE, '') AS mensaje,
                ER.LINEA_ERR AS linea_err
            FROM SIF_OMISIONES E
            LEFT JOIN SIF_OMISIONESG ER
                ON E.ID_ERROR = ER.ID_ERROR
                AND ER.CEDULA = @cedula
                AND ER.MODULO = @modulo
                AND ER.CODIGO = @codigo
                AND ER.DOCUMENTO = @documento
            WHERE E.ACTIVO = '1'
              AND EXISTS
              (
                  SELECT 1
                  FROM SIF_OMISIONES_MODULOS EM
                  WHERE EM.ID_ERROR = E.ID_ERROR
                    AND EM.COD_MODULO = @modulo
              )
            ORDER BY E.ID_ERROR;
            """;

            string documento = Convert.ToString(
                consecutivo,
                CultureInfo.InvariantCulture);

            return DbHelper.WithConn(
                _portalDB,
                CodEmpresa,
                connection => connection
                    .Query<
                        FndLiqSeguimientoRevisionesTagRevisionData>(
                        sql,
                        new
                        {
                            cedula = identificacion,
                            consecutivo,
                            modulo = CodigoModulo,
                            documento
                        })
                    .ToList());
        }

        /// <summary>
        /// Exporta las omisiones configuradas para Liquidación de Fondos.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="cedula"></param>
        /// <param name="consecutivo"></param>
        /// <returns></returns>
        public ErrorDto<List<FndLiqSeguimientoRevisionesTagRevisionData>>FND_LiqSeguimientoRevisionesTag_Revisiones_Lista_Export(int CodEmpresa,string? cedula,long consecutivo)
        {
            return FND_LiqSeguimientoRevisionesTag_Revisiones_Lista_Obtener(CodEmpresa,cedula,consecutivo);
        }

        #endregion

        #region Selección

        /// <summary>
        /// Guarda o elimina inmediatamente la selección de una omisión sin
        /// marcarla como aplicada.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<long?> FND_LiqSeguimientoRevisionesTag_Seleccion_Actualizar(int CodEmpresa, string? usuario, FndLiqSeguimientoRevisionesTagSeleccionRequest? request)
        {
            string usuarioActual = (usuario ?? string.Empty)
                .Trim()
                .ToUpperInvariant();

            string validacion = ValidarSeleccion(
                request,
                usuarioActual);

            if (validacion.Length > 0)
            {
                return DbHelper.CreateErrorResponse<long?>(
                    validacion,
                    -2,
                    null);
            }

            string cedula = request!.cedula!.Trim();
            long consecutivo = request.consecutivo!.Value;
            int idError = request.id_error!.Value;

            string documento = Convert.ToString(
                consecutivo,
                CultureInfo.InvariantCulture);

            string connectionString =
                _portalDB.ObtenerDbConnStringEmpresa(CodEmpresa);

            try
            {
                using var connection =
                    new SqlConnection(connectionString);

                connection.Open();

                using var transaction =
                    connection.BeginTransaction();

                string codigo = ObtenerLlaveLiquidacion(
                    connection,
                    transaction,
                    cedula,
                    consecutivo);

                if (codigo.Length == 0)
                {
                    transaction.Rollback();

                    return DbHelper.CreateErrorResponse<long?>(
                        "No se encontró el plan y contrato de la liquidación indicada.",
                        -2,
                        null);
                }

                ErrorDto<long?> resultado =
                    request.seleccionado.GetValueOrDefault()
                        ? SeleccionarOmision(
                            connection,
                            transaction,
                            cedula,
                            codigo,
                            documento,
                            usuarioActual,
                            idError)
                        : DeseleccionarOmision(
                            connection,
                            transaction,
                            cedula,
                            codigo,
                            documento,
                            idError,
                            request.linea_err!.Value);

                if ((resultado.Code ?? -1) != 0)
                {
                    transaction.Rollback();
                    return resultado;
                }

                transaction.Commit();
                return resultado;
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<long?>(
                    ex.Message,
                    -1,
                    null);
            }
            catch (InvalidOperationException ex)
            {
                return DbHelper.CreateErrorResponse<long?>(
                    ex.Message,
                    -1,
                    null);
            }
            catch (DataException ex)
            {
                return DbHelper.CreateErrorResponse<long?>(
                    ex.Message,
                    -1,
                    null);
            }
        }

        private static string ObtenerLlaveLiquidacion(SqlConnection connection,SqlTransaction transaction,string cedula, long consecutivo)
        {
            const string sql = """
        SELECT TOP (1)
            RTRIM(ISNULL(L.COD_PLAN, ''))
            + '-'
            + CONVERT(VARCHAR(30), L.COD_CONTRATO)
        FROM FND_LIQUIDACION L
        INNER JOIN FND_CONTRATOS F
            ON L.COD_PLAN = F.COD_PLAN
            AND L.COD_CONTRATO = F.COD_CONTRATO
            AND L.COD_OPERADORA = F.COD_OPERADORA
        WHERE L.CONSEC = @consecutivo
          AND F.CEDULA = @cedula;
        """;

            return connection.QueryFirstOrDefault<string>(
                sql,
                new
                {
                    cedula,
                    consecutivo
                },
                transaction) ?? string.Empty;
        }
        private static string ValidarSeleccion(FndLiqSeguimientoRevisionesTagSeleccionRequest? request,string usuario)
        {
            if (request == null)
            {
                return "No se recibió la omisión que desea actualizar.";
            }

            if (string.IsNullOrWhiteSpace(request.cedula))
            {
                return "Debe indicar la cédula.";
            }

            if (!request.consecutivo.HasValue ||
                request.consecutivo.Value <= 0)
            {
                return "Debe indicar la liquidación.";
            }

            if (!request.id_error.HasValue ||
                request.id_error.Value <= 0)
            {
                return "Debe indicar una omisión válida.";
            }

            if (!request.seleccionado.HasValue)
            {
                return "Debe indicar el estado de la selección.";
            }

            if (!request.seleccionado.Value &&
                (!request.linea_err.HasValue ||
                 request.linea_err.Value <= 0))
            {
                return "No se encontró la línea de la omisión que desea eliminar.";
            }

            if (usuario.Length == 0)
            {
                return "Debe indicar el usuario.";
            }

            return string.Empty;
        }

        private static ErrorDto<long?> SeleccionarOmision(SqlConnection connection, SqlTransaction transaction,string cedula,string codigo,string documento,string usuario,int idError)
        {
            const string sqlOmisionValida = """
        SELECT COUNT(1)
        FROM SIF_OMISIONES E
        WHERE E.ID_ERROR = @idError
          AND E.ACTIVO = '1'
          AND EXISTS
          (
              SELECT 1
              FROM SIF_OMISIONES_MODULOS EM
              WHERE EM.ID_ERROR = E.ID_ERROR
                AND EM.COD_MODULO = @modulo
          );
        """;

            int omisionValida = connection.QuerySingle<int>(
                sqlOmisionValida,
                new
                {
                    idError,
                    modulo = CodigoModulo
                },
                transaction);

            if (omisionValida == 0)
            {
                return DbHelper.CreateErrorResponse<long?>(
                    "La omisión indicada no está activa para Liquidaciones de Fondos.",
                    -2,
                    null);
            }

            const string sqlExistente = """
                SELECT TOP (1)
                    LINEA_ERR
                FROM SIF_OMISIONESG WITH (UPDLOCK, HOLDLOCK)
                WHERE CEDULA = @cedula
                  AND ID_ERROR = @idError
                  AND MODULO = @modulo
                  AND CODIGO = @codigo
                  AND DOCUMENTO = @documento;
                """;

            long? lineaExistente =
                connection.QueryFirstOrDefault<long?>(
                    sqlExistente,
                    new
                    {
                        cedula,
                        codigo,
                        documento,
                        idError,
                        modulo = CodigoModulo
                    },
                    transaction);

            if (lineaExistente.HasValue)
            {
                return DbHelper.CreateOkResponse<long?>(
                    lineaExistente.Value);
            }

            const string sqlInsertar = """
                INSERT INTO SIF_OMISIONESG
                (
                    CEDULA,
                    ID_ERROR,
                    MODULO,
                    CODIGO,
                    DOCUMENTO,
                    REGISTRO_FECHA,
                    REGISTRO_USUARIO
                )
                OUTPUT INSERTED.LINEA_ERR
                VALUES
                (
                    @cedula,
                    @idError,
                    @modulo,
                    @codigo,
                    @documento,
                    dbo.MyGetdate(),
                    @usuario
                );
                """;

            long lineaErr = connection.QuerySingle<long>(
                sqlInsertar,
                new
                {
                    cedula,
                    codigo,
                    documento,
                    usuario,
                    idError,
                    modulo = CodigoModulo
                },
                transaction);

            return DbHelper.CreateOkResponse<long?>(
                lineaErr);
        }

        private static ErrorDto<long?> DeseleccionarOmision(SqlConnection connection,SqlTransaction transaction,string cedula,string codigo,string documento,int idError,long lineaErr)
        {
            const string sqlEstado = """
        SELECT TOP (1)
            CONVERT(
                INT,
                CASE
                    WHEN ISNULL(APLICADO, 'N') = 'S'
                    THEN 1
                    ELSE 0
                END
            )
        FROM SIF_OMISIONESG WITH (UPDLOCK, HOLDLOCK)
        WHERE LINEA_ERR = @lineaErr
          AND CEDULA = @cedula
          AND ID_ERROR = @idError
          AND MODULO = @modulo
          AND CODIGO = @codigo
          AND DOCUMENTO = @documento;
        """;

            int? aplicada = connection.QueryFirstOrDefault<int?>(
                sqlEstado,
                new
                {
                    lineaErr,
                    cedula,
                    codigo,
                    documento,
                    idError,
                    modulo = CodigoModulo
                },
                transaction);

            if (!aplicada.HasValue)
            {
                return DbHelper.CreateOkResponse<long?>(
                    null);
            }

            if (aplicada.Value == 1)
            {
                return DbHelper.CreateErrorResponse<long?>(
                    "La omisión ya fue aplicada y no puede desmarcarse.",
                    -2,
                    lineaErr);
            }

            const string sqlEliminar = """
        DELETE FROM SIF_OMISIONESG
        WHERE LINEA_ERR = @lineaErr
          AND CEDULA = @cedula
          AND ID_ERROR = @idError
          AND MODULO = @modulo
          AND CODIGO = @codigo
          AND DOCUMENTO = @documento
          AND ISNULL(APLICADO, 'N') <> 'S';
        """;

            connection.Execute(
                sqlEliminar,
                new
                {
                    lineaErr,
                    cedula,
                    codigo,
                    documento,
                    idError,
                    modulo = CodigoModulo
                },
                transaction);

            return DbHelper.CreateOkResponse<long?>(
                null);
        }

        #endregion

        #region Aplicar

        /// <summary>
        /// Registra la etiqueta y marca como aplicadas las omisiones de la
        /// liquidación seleccionada.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto FND_LiqSeguimientoRevisionesTag_Aplicar(int CodEmpresa, string? usuario, FndLiqSeguimientoRevisionesTagAplicarRequest? request)
        {
            string usuarioActual = (usuario ?? string.Empty)
                .Trim()
                .ToUpperInvariant();

            if (request == null)
            {
                return DbHelper.ErrorResponse(
                    "No se recibió la información que desea aplicar.",
                    -2);
            }

            if (string.IsNullOrWhiteSpace(request.cedula))
            {
                return DbHelper.ErrorResponse(
                    "Debe indicar la cédula.",
                    -2);
            }

            if (!request.consecutivo.HasValue ||
                request.consecutivo.Value <= 0)
            {
                return DbHelper.ErrorResponse(
                    "Debe indicar la liquidación.",
                    -2);
            }

            if (string.IsNullOrWhiteSpace(request.tag_codigo))
            {
                return DbHelper.ErrorResponse(
                    "Debe seleccionar la etiqueta que desea aplicar.",
                    -2);
            }

            if (usuarioActual.Length == 0)
            {
                return DbHelper.ErrorResponse(
                    "Debe indicar el usuario.",
                    -2);
            }

            string cedula = request.cedula.Trim();
            long consecutivo = request.consecutivo.Value;
            string tagCodigo = request.tag_codigo.Trim();

            string observacion =
                (request.observacion ?? string.Empty).Trim();

            string documento = Convert.ToString(
                consecutivo,
                CultureInfo.InvariantCulture);

            string connectionString =
                _portalDB.ObtenerDbConnStringEmpresa(CodEmpresa);

            try
            {
                using var connection =
                    new SqlConnection(connectionString);

                connection.Open();

                using var transaction =
                    connection.BeginTransaction();

                string codigoOmision = ObtenerLlaveLiquidacion(
                    connection,
                    transaction,
                    cedula,
                    consecutivo);

                if (codigoOmision.Length == 0)
                {
                    transaction.Rollback();

                    return DbHelper.ErrorResponse(
                        "No se encontró el plan y contrato de la liquidación indicada.",
                        -2);
                }

                const string sqlEtiquetaPermitida = """
            SELECT COUNT(1)
            FROM SIF_TAGS T
            INNER JOIN SIF_TAGS_GRUPOS TG
                ON T.TAG_CODIGO = TG.TAG_CODIGO
            INNER JOIN SIF_GRPUSERS GU
                ON TG.COD_GRUPO = GU.COD_GRUPO
            WHERE T.TAG_CODIGO = @tagCodigo
              AND T.ACTIVO = 1
              AND GU.USUARIO = @usuario
              AND EXISTS
              (
                  SELECT 1
                  FROM SIF_TAGS_MODULOS TM
                  WHERE TM.TAG_CODIGO = T.TAG_CODIGO
                    AND TM.COD_MODULO = @modulo
              );
            """;

                int etiquetaPermitida =
                    connection.QuerySingle<int>(
                        sqlEtiquetaPermitida,
                        new
                        {
                            tagCodigo,
                            usuario = usuarioActual,
                            modulo = CodigoModulo
                        },
                        transaction);

                if (etiquetaPermitida == 0)
                {
                    transaction.Rollback();

                    return DbHelper.ErrorResponse(
                        "La etiqueta seleccionada no está disponible para el usuario.",
                        -2);
                }

                connection.Execute(
                    "spSIFRegistraTags",
                    new
                    {
                        Codigo = cedula,
                        Tag = tagCodigo,
                        Usuario = usuarioActual,
                        Notas = observacion,
                        Documento = documento,
                        Modulo = CodigoModulo,
                        Llave_01 = documento,
                        Llave_02 = string.Empty,
                        Llave_03 = string.Empty
                    },
                    transaction,
                    commandType: CommandType.StoredProcedure);

                const string sqlAplicarOmisiones = """
            UPDATE SIF_OMISIONESG
            SET APLICADO = 'S'
            WHERE CEDULA = @cedula
              AND MODULO = @modulo
              AND CODIGO = @codigo
              AND DOCUMENTO = @documento;
            """;

                connection.Execute(
                    sqlAplicarOmisiones,
                    new
                    {
                        cedula,
                        codigo = codigoOmision,
                        documento,
                        modulo = CodigoModulo
                    },
                    transaction);

                transaction.Commit();

                return DbHelper.OkResponse(
                    "Etiqueta aplicada satisfactoriamente.");
            }
            catch (SqlException ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
            catch (DataException ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }
      
        #endregion

        #region Contexto de liquidación
        private class LiquidacionContexto
        {
            public string cedula { get; set; } = string.Empty;
            public long consecutivo { get; set; }
            public string cod_plan { get; set; } = string.Empty;
            public long cod_contrato { get; set; }
            public string codigo { get; set; } = string.Empty;
            public string documento { get; set; } = string.Empty;
        }

        #endregion

        #region Ejecución común

        private TRespuesta EjecutarOperacion<TRespuesta>(
            int CodEmpresa,
            Func<SqlConnection, TRespuesta> operacion,
            Func<string, TRespuesta> crearError)
        {
            try
            {
                using var connection =
                    DbHelper.OpenConnection(
                        _portalDB,
                        CodEmpresa);

                return operacion(connection);
            }
            catch (SqlException ex)
            {
                return crearError(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return crearError(ex.Message);
            }
            catch (DataException ex)
            {
                return crearError(ex.Message);
            }
        }

        #endregion
    }
}