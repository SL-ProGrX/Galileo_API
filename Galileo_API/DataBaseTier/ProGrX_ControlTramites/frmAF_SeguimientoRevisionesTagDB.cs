using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.ControlTramites;
using Galileo.Models.Security;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using System.Data;
using System.Globalization;

namespace Galileo_API.DataBaseTier.ProGrX.ControlTramites
{
    public class FrmAfSeguimientoRevisionesTagDB
    {
        private const int ModuloControlTramites = 8;
        private const string CodigoModulo = "AFI";
        private const string MensajeCedulaRequerida =
            "Debe indicar la cédula de la afiliación.";
        private const string MensajeConsecutivoRequerido =
            "Debe indicar una boleta válida.";
        private const string MensajeRegistroNoEncontrado =
            "No se encontró la afiliación indicada.";

        private static readonly IReadOnlyDictionary<string, int>
            OrdenAfiliaciones = new Dictionary<string, int>(
                StringComparer.OrdinalIgnoreCase)
            {
                ["cedula"] = 1,
                ["nombre"] = 2,
                ["usuario_registra"] = 3,
                ["numero_remesa"] = 4,
                ["usuario_remesa"] = 5,
                ["consecutivo"] = 6
            };

        private readonly PortalDB _portalDB;
        private readonly MProGrxMain _proGrxMain;
        private readonly MSecurityMainDb _securityMainDB;

        public FrmAfSeguimientoRevisionesTagDB(
            IConfiguration config)
        {
            _portalDB = new PortalDB(config);
            _proGrxMain = new MProGrxMain(config);
            _securityMainDB = new MSecurityMainDb(config);
        }

        #region Afiliaciones

        /// <summary>
        /// Obtiene las afiliaciones pendientes de revisión mediante lazy
        /// loading, filtro global, ordenamiento y paginación.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="parametros"></param>
        /// <returns></returns>
        public ErrorDto<AfSeguimientoRevisionesTagAfiliacionesListaResult>
            AF_SeguimientoRevisionesTag_Afiliaciones_Lista_Obtener(
                int CodEmpresa,
                string parametros)
        {
            if (!TryObtenerFiltrosAfiliaciones(
                parametros,
                out FiltrosLazyLoadData filtros,
                out string mensajeError))
            {
                return DbHelper.CreateErrorResponse(
                    mensajeError,
                    -1,
                    CrearListaAfiliacionesVacia());
            }

            return EjecutarConsulta(
                CodEmpresa,
                connection => ObtenerAfiliaciones(
                    connection,
                    filtros),
                CrearListaAfiliacionesVacia());
        }

        /// <summary>
        /// Exporta todas las afiliaciones pendientes de revisión respetando
        /// el filtro y el ordenamiento indicados.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="parametros"></param>
        /// <returns></returns>
        public ErrorDto<AfSeguimientoRevisionesTagAfiliacionesListaResult>
            AF_SeguimientoRevisionesTag_Afiliaciones_Lista_Export(
                int CodEmpresa,
                string parametros)
        {
            if (!TryObtenerFiltrosAfiliaciones(
                parametros,
                out FiltrosLazyLoadData filtros,
                out string mensajeError))
            {
                return DbHelper.CreateErrorResponse(
                    mensajeError,
                    -1,
                    CrearListaAfiliacionesVacia());
            }

            filtros.pagina = 0;
            filtros.paginacion = 0;

            return AF_SeguimientoRevisionesTag_Afiliaciones_Lista_Obtener(
                CodEmpresa,
                JsonConvert.SerializeObject(filtros));
        }

        /// <summary>
        /// Ejecuta la consulta paginada de afiliaciones pendientes de
        /// revisión y obtiene el total general.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        private static AfSeguimientoRevisionesTagAfiliacionesListaResult
            ObtenerAfiliaciones(
                SqlConnection connection,
                FiltrosLazyLoadData filtros)
        {
            string texto = (filtros.filtro ?? string.Empty).Trim();
            string? filtro = texto.Length == 0 ? null : texto;
            string? like = filtro == null ? null : $"%{filtro}%";

            int offset = Math.Max(0, filtros.pagina);
            int fetch = Math.Max(0, filtros.paginacion);
            bool usarPaginacion = fetch > 0;

            int orden = ResolverOrdenAfiliaciones(
                filtros.sortField);

            int ascendente = filtros.sortOrder == 1 ? 1 : 0;

            const string sqlCount = """
                SELECT COUNT(1)
                FROM AFI_INGRESOS A
                INNER JOIN SOCIOS S
                    ON A.CEDULA = S.CEDULA
                LEFT JOIN AFI_REMESAS_ING R
                    ON A.COD_REMESA = R.COD_REMESA
                WHERE A.ANALISTA_REVISION IS NULL
                  AND S.ESTADOACTUAL IN ('S', 'A', 'P')
                  AND
                  (
                      @filtro IS NULL
                      OR A.CEDULA LIKE @like
                      OR S.NOMBRE LIKE @like
                      OR A.USUARIO LIKE @like
                      OR R.USUARIO LIKE @like
                      OR CONVERT(VARCHAR(30), A.COD_REMESA) LIKE @like
                      OR CONVERT(VARCHAR(30), A.CONSEC) LIKE @like
                  );
                """;

            int total = connection.QuerySingle<int>(
                sqlCount,
                new
                {
                    filtro,
                    like
                });

            string sqlLista = """
                SELECT
                    RTRIM(A.CEDULA) AS cedula,
                    RTRIM(S.NOMBRE) AS nombre,
                    ISNULL(RTRIM(A.USUARIO), '') AS usuario_registra,
                    A.COD_REMESA AS numero_remesa,
                    ISNULL(RTRIM(R.USUARIO), '') AS usuario_remesa,
                    A.CONSEC AS consecutivo
                FROM AFI_INGRESOS A
                INNER JOIN SOCIOS S
                    ON A.CEDULA = S.CEDULA
                LEFT JOIN AFI_REMESAS_ING R
                    ON A.COD_REMESA = R.COD_REMESA
                WHERE A.ANALISTA_REVISION IS NULL
                  AND S.ESTADOACTUAL IN ('S', 'A', 'P')
                  AND
                  (
                      @filtro IS NULL
                      OR A.CEDULA LIKE @like
                      OR S.NOMBRE LIKE @like
                      OR A.USUARIO LIKE @like
                      OR R.USUARIO LIKE @like
                      OR CONVERT(VARCHAR(30), A.COD_REMESA) LIKE @like
                      OR CONVERT(VARCHAR(30), A.CONSEC) LIKE @like
                  )
                ORDER BY
                    CASE
                        WHEN @orden = 1 AND @ascendente = 1
                        THEN A.CEDULA
                    END ASC,
                    CASE
                        WHEN @orden = 1 AND @ascendente = 0
                        THEN A.CEDULA
                    END DESC,

                    CASE
                        WHEN @orden = 2 AND @ascendente = 1
                        THEN S.NOMBRE
                    END ASC,
                    CASE
                        WHEN @orden = 2 AND @ascendente = 0
                        THEN S.NOMBRE
                    END DESC,

                    CASE
                        WHEN @orden = 3 AND @ascendente = 1
                        THEN A.USUARIO
                    END ASC,
                    CASE
                        WHEN @orden = 3 AND @ascendente = 0
                        THEN A.USUARIO
                    END DESC,

                    CASE
                        WHEN @orden = 4 AND @ascendente = 1
                        THEN A.COD_REMESA
                    END ASC,
                    CASE
                        WHEN @orden = 4 AND @ascendente = 0
                        THEN A.COD_REMESA
                    END DESC,

                    CASE
                        WHEN @orden = 5 AND @ascendente = 1
                        THEN R.USUARIO
                    END ASC,
                    CASE
                        WHEN @orden = 5 AND @ascendente = 0
                        THEN R.USUARIO
                    END DESC,

                    CASE
                        WHEN @orden = 6 AND @ascendente = 1
                        THEN A.CONSEC
                    END ASC,
                    CASE
                        WHEN @orden = 6 AND @ascendente = 0
                        THEN A.CONSEC
                    END DESC,

                    A.CONSEC DESC
                """;

            if (usarPaginacion)
            {
                sqlLista += """

                    OFFSET @offset ROWS
                    FETCH NEXT @fetch ROWS ONLY;
                    """;
            }

            var lista = connection.Query<
                AfSeguimientoRevisionesTagAfiliacionData>(
                    sqlLista,
                    new
                    {
                        filtro,
                        like,
                        orden,
                        ascendente,
                        offset,
                        fetch
                    }).ToList();

            return new AfSeguimientoRevisionesTagAfiliacionesListaResult
            {
                total = total,
                lista = lista
            };
        }

        /// <summary>
        /// Resuelve el campo permitido para ordenar la tabla de afiliaciones.
        /// </summary>
        /// <param name="sortField"></param>
        /// <returns></returns>
        private static int ResolverOrdenAfiliaciones(
            string? sortField)
        {
            string campo = (sortField ?? string.Empty).Trim();

            return OrdenAfiliaciones.TryGetValue(
                campo,
                out int orden)
                ? orden
                : OrdenAfiliaciones["consecutivo"];
        }

        /// <summary>
        /// Deserializa los filtros utilizados por la lista de afiliaciones.
        /// </summary>
        /// <param name="parametros"></param>
        /// <param name="filtros"></param>
        /// <param name="mensajeError"></param>
        /// <returns></returns>
        private static bool TryObtenerFiltrosAfiliaciones(
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

        /// <summary>
        /// Crea el resultado vacío estándar de la lista de afiliaciones.
        /// </summary>
        /// <returns></returns>
        private static
            AfSeguimientoRevisionesTagAfiliacionesListaResult
            CrearListaAfiliacionesVacia()
        {
            return new AfSeguimientoRevisionesTagAfiliacionesListaResult
            {
                total = 0,
                lista =
                    new List<
                        AfSeguimientoRevisionesTagAfiliacionData>()
            };
        }

        #endregion

        #region Detalle

        /// <summary>
        /// Obtiene el detalle completo de una afiliación. Si no se recibe
        /// consecutivo, utiliza la última boleta registrada para la cédula.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="cedula"></param>
        /// <param name="consecutivo"></param>
        /// <returns></returns>
        public ErrorDto<AfSeguimientoRevisionesTagDetalleData>
            AF_SeguimientoRevisionesTag_Detalle_Obtener(
                int CodEmpresa,
                string? cedula,
                long? consecutivo)
        {
            string identificacion = (cedula ?? string.Empty).Trim();

            if (identificacion.Length == 0)
            {
                return DbHelper.CreateErrorResponse(
                    MensajeCedulaRequerida,
                    -2,
                    new AfSeguimientoRevisionesTagDetalleData());
            }

            ErrorDto<bool> validacion = _proGrxMain.fxSIFValidaCadena(identificacion);

            int codigoValidacion = validacion.Code ?? -1;
            string mensajeValidacion =
                validacion.Description ??
                "La identificación indicada no es válida.";

            if (codigoValidacion != 0 || !validacion.Result)
            {
                return DbHelper.CreateErrorResponse(
                    mensajeValidacion,
                    codigoValidacion,
                    new AfSeguimientoRevisionesTagDetalleData());
            }

            var response = EjecutarConsulta(
            CodEmpresa,
            connection => ObtenerDetalle(
                connection,
                CodEmpresa,
                identificacion,
                consecutivo),
            new AfSeguimientoRevisionesTagDetalleData());

            if (response.Code == 0 &&
                (response.Result == null ||
                 string.IsNullOrWhiteSpace(response.Result.cedula)))
            {
                return DbHelper.CreateErrorResponse(
                    MensajeRegistroNoEncontrado,
                    -2,
                    new AfSeguimientoRevisionesTagDetalleData());
            }

            return response;
        }

        /// <summary>
        /// Resuelve la versión ASE, el consecutivo y la información detallada
        /// correspondiente a la persona.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="CodEmpresa"></param>
        /// <param name="cedula"></param>
        /// <param name="consecutivo"></param>
        /// <returns></returns>
        private AfSeguimientoRevisionesTagDetalleData ObtenerDetalle(
            SqlConnection connection,
            int CodEmpresa,
            string cedula,
            long? consecutivo)
        {
            bool sysASEVersion = ObtenerSysASEVersion(connection);
            long boleta = ResolverConsecutivo(
                connection,
                cedula,
                consecutivo);

            DateTime fechaServidor =
                _proGrxMain.fxFechaServidor(CodEmpresa, 0);

            string sql = sysASEVersion
                ? SqlDetalleASE
                : SqlDetalleGeneral;

            var detalle = connection.QueryFirstOrDefault<
                AfSeguimientoRevisionesTagDetalleData>(
                    sql,
                    new
                    {
                        cedula,
                        fechaServidor,
                        consecutivo = boleta
                    })
                ?? new AfSeguimientoRevisionesTagDetalleData();

            if (!string.IsNullOrWhiteSpace(detalle.cedula))
            {
                detalle.consecutivo = boleta;
                detalle.estado_civil_descripcion =
                    MControlTramitesDB.fxEstadoCivil(
                        detalle.estado_civil);
            }

            return detalle;
        }

        /// <summary>
        /// Determina si la empresa trabaja con la estructura ASE/CCSS.
        /// </summary>
        /// <param name="connection"></param>
        /// <returns></returns>
        private static bool ObtenerSysASEVersion(
            SqlConnection connection)
        {
            const string sql = """
                SELECT TOP 1 ISNULL(SYS_CCSS_IND, 0)
                FROM SIF_EMPRESA;
                """;

            return connection.QueryFirstOrDefault<int>(sql) == 1;
        }

        /// <summary>
        /// Utiliza el consecutivo seleccionado o resuelve la última boleta
        /// registrada para la cédula.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="cedula"></param>
        /// <param name="consecutivo"></param>
        /// <returns></returns>
        private static long ResolverConsecutivo(
            SqlConnection connection,
            string cedula,
            long? consecutivo)
        {
            long valor = consecutivo.GetValueOrDefault();

            if (valor > 0)
            {
                return valor;
            }

            const string sql = """
                SELECT ISNULL(MAX(CONSEC), 0)
                FROM AFI_INGRESOS
                WHERE CEDULA = @cedula;
                """;

            return connection.QueryFirstOrDefault<long>(
                sql,
                new
                {
                    cedula
                });
        }

        private const string SqlDetalleGeneral = """
            SELECT
                RTRIM(S.CEDULA) AS cedula,
                RTRIM(S.NOMBRE) AS nombre,
                @consecutivo AS consecutivo,
                ISNULL(S.ID_BOLETA_AF, 0) AS numero_boleta,

                ISNULL(RTRIM(S.ESTADOACTUAL), '') AS estado_actual,
                ISNULL(
                    RTRIM(EST.COD_ESTADO) + ' - ' +
                    RTRIM(EST.DESCRIPCION),
                    ''
                ) AS estado_descripcion,

                S.FECHAINGRESO AS fecha_ingreso,
                S.FECHA_NAC AS fecha_nacimiento,

                ISNULL(RTRIM(S.SEXO), '') AS sexo,
                CASE
                    WHEN S.SEXO = 'M' THEN 'Masculino'
                    ELSE 'Femenino'
                END AS sexo_descripcion,

                ISNULL(RTRIM(S.ESTADOCIVIL), '') AS estado_civil,
                '' AS estado_civil_descripcion,

                ISNULL(
                    RTRIM(CONVERT(VARCHAR(20), S.PROVINCIA)),
                    ''
                ) AS provincia,
                ISNULL(RTRIM(PROV.DESCRIPCION), '')
                    AS provincia_descripcion,

                ISNULL(
                    RTRIM(CONVERT(VARCHAR(20), S.CANTON)),
                    ''
                ) AS canton,
                ISNULL(RTRIM(CANT.DESCRIPCION), '')
                    AS canton_descripcion,

                ISNULL(
                    RTRIM(CONVERT(VARCHAR(20), S.DISTRITO)),
                    ''
                ) AS distrito,
                ISNULL(RTRIM(DIST.DESCRIPCION), '')
                    AS distrito_descripcion,

                ISNULL(RTRIM(S.DIRECCION), '') AS direccion,
                ISNULL(RTRIM(S.AF_EMAIL), '') AS correo,
                ISNULL(RTRIM(S.APTO), '') AS apartado,

                CASE
                    WHEN ISNULL(S.ESTADOLABORAL, 1) = 1
                    THEN 'Interino'
                    ELSE 'Propiedad'
                END AS nombramiento,

                ISNULL(
                    S.NOMBRAMIENTO_FECHA,
                    S.FECHAINGRESO
                ) AS fecha_nombramiento,

                dbo.fxAFIAnioServicio(
                    S.CEDULA,
                    @fechaServidor
                ) AS anios_servicio,

                ISNULL(
                    RTRIM(
                        CONVERT(VARCHAR(30), S.ID_PROMOTOR)
                    ),
                    ''
                ) AS promotor,
                ISNULL(RTRIM(P.NOMBRE), '')
                    AS promotor_descripcion,

                ISNULL(RTRIM(S.NOTIFICACIONES), '')
                    AS notificaciones,

                ISNULL(
                    RTRIM(
                        CONVERT(
                            VARCHAR(30),
                            S.COD_INSTITUCION
                        )
                    ),
                    ''
                ) AS institucion,
                ISNULL(RTRIM(I.DESCRIPCION), '')
                    AS institucion_descripcion,

                ISNULL(
                    RTRIM(
                        CONVERT(
                            VARCHAR(30),
                            S.COD_PROFESION
                        )
                    ),
                    ''
                ) AS profesion,
                ISNULL(RTRIM(R.DESCRIPCION), '')
                    AS profesion_descripcion,

                ISNULL(
                    RTRIM(
                        CONVERT(
                            VARCHAR(30),
                            S.COD_SECTOR
                        )
                    ),
                    ''
                ) AS sector,
                ISNULL(RTRIM(Q.DESCRIPCION), '')
                    AS sector_descripcion,

                ISNULL(
                    RTRIM(
                        CONVERT(
                            VARCHAR(30),
                            S.COD_DEPARTAMENTO
                        )
                    ),
                    ''
                ) AS departamento,
                ISNULL(RTRIM(D.DESCRIPCION), '')
                    AS departamento_descripcion,

                ISNULL(
                    RTRIM(
                        CONVERT(
                            VARCHAR(30),
                            S.COD_SECCION
                        )
                    ),
                    ''
                ) AS seccion,
                ISNULL(RTRIM(X.DESCRIPCION), '')
                    AS seccion_descripcion,

                '' AS unidad_programatica,
                '' AS unidad_programatica_descripcion,
                '' AS unidad_trabajo,
                '' AS unidad_trabajo_descripcion,
                '' AS centro_trabajo,
                '' AS centro_trabajo_descripcion,

                ISNULL(
                    RTRIM(
                        CONVERT(
                            VARCHAR(30),
                            S.COD_OFICINA
                        )
                    ),
                    ''
                ) AS oficina,
                ISNULL(RTRIM(O.DESCRIPCION), '')
                    AS oficina_descripcion,

                ISNULL(
                    RTRIM(
                        CONVERT(VARCHAR(30), S.TIPO_ID)
                    ),
                    ''
                ) AS tipo_identificacion,
                ISNULL(RTRIM(TID.DESCRIPCION), '')
                    AS tipo_identificacion_descripcion,

                ISNULL(
                    RTRIM(
                        CONVERT(
                            VARCHAR(30),
                            S.COD_SOCIEDAD
                        )
                    ),
                    ''
                ) AS tipo_sociedad,
                ISNULL(
                    RTRIM(SOC.COD_SOCIEDAD) + ' - ' +
                    RTRIM(SOC.DESCRIPCION),
                    ''
                ) AS tipo_sociedad_descripcion,

                ISNULL(
                    RTRIM(
                        CONVERT(
                            VARCHAR(30),
                            S.COD_ACTIVIDAD
                        )
                    ),
                    ''
                ) AS actividad_economica,
                ISNULL(
                    RTRIM(ACT.COD_ACTIVIDAD) + ' - ' +
                    RTRIM(ACT.DESCRIPCION),
                    ''
                ) AS actividad_economica_descripcion,

                ISNULL(S.HIJOS, 0) AS hijos,
                ISNULL(S.AF_NPAGOS, 0) AS numero_pagos
            FROM SOCIOS S
            INNER JOIN INSTITUCIONES I
                ON S.COD_INSTITUCION = I.COD_INSTITUCION
            LEFT JOIN AFDEPARTAMENTOS D
                ON S.COD_INSTITUCION = D.COD_INSTITUCION
                AND S.COD_DEPARTAMENTO = D.COD_DEPARTAMENTO
            LEFT JOIN AFSECCIONES X
                ON S.COD_INSTITUCION = X.COD_INSTITUCION
                AND S.COD_DEPARTAMENTO = X.COD_DEPARTAMENTO
                AND S.COD_SECCION = X.COD_SECCION
            INNER JOIN PROMOTORES P
                ON S.ID_PROMOTOR = P.ID_PROMOTOR
            INNER JOIN AFI_PROFESIONES R
                ON S.COD_PROFESION = R.COD_PROFESION
            INNER JOIN AFI_SECTORES Q
                ON S.COD_SECTOR = Q.COD_SECTOR
            INNER JOIN AFI_ESTADOS_PERSONA EST
                ON S.ESTADOACTUAL = EST.COD_ESTADO
            LEFT JOIN PROVINCIAS PROV
                ON S.PROVINCIA = PROV.PROVINCIA
            LEFT JOIN CANTONES CANT
                ON S.PROVINCIA = CANT.PROVINCIA
                AND S.CANTON = CANT.CANTON
            LEFT JOIN DISTRITOS DIST
                ON S.PROVINCIA = DIST.PROVINCIA
                AND CONVERT(INT, S.CANTON) =
                    CONVERT(INT, DIST.CANTON)
                AND S.DISTRITO = DIST.DISTRITO
            LEFT JOIN SIF_OFICINAS O
                ON S.COD_OFICINA = O.COD_OFICINA
            LEFT JOIN AFI_TIPOS_IDS TID
                ON S.TIPO_ID = TID.TIPO_ID
            LEFT JOIN AFI_SOCIEDADES_TIPOS SOC
                ON S.COD_SOCIEDAD = SOC.COD_SOCIEDAD
            LEFT JOIN AFI_ACTIVIDADES_ECO ACT
                ON S.COD_ACTIVIDAD = ACT.COD_ACTIVIDAD
            WHERE S.CEDULA = @cedula;
            """;

        private const string SqlDetalleASE = """
            SELECT
            RTRIM(S.CEDULA) AS cedula,
            RTRIM(S.NOMBRE) AS nombre,
            @consecutivo AS consecutivo,
            ISNULL(S.ID_BOLETA_AF, 0) AS numero_boleta,

                ISNULL(RTRIM(S.ESTADOACTUAL), '') AS estado_actual,
                ISNULL(
                    RTRIM(EST.COD_ESTADO) + ' - ' +
                    RTRIM(EST.DESCRIPCION),
                    ''
                ) AS estado_descripcion,

                S.FECHAINGRESO AS fecha_ingreso,
                S.FECHA_NAC AS fecha_nacimiento,

                ISNULL(RTRIM(S.SEXO), '') AS sexo,
                CASE
                    WHEN S.SEXO = 'M' THEN 'Masculino'
                    ELSE 'Femenino'
                END AS sexo_descripcion,

                ISNULL(RTRIM(S.ESTADOCIVIL), '') AS estado_civil,
                '' AS estado_civil_descripcion,

                ISNULL(
                    RTRIM(CONVERT(VARCHAR(20), S.PROVINCIA)),
                    ''
                ) AS provincia,
                ISNULL(RTRIM(PROV.DESCRIPCION), '')
                    AS provincia_descripcion,

                ISNULL(
                    RTRIM(CONVERT(VARCHAR(20), S.CANTON)),
                    ''
                ) AS canton,
                ISNULL(RTRIM(CANT.DESCRIPCION), '')
                    AS canton_descripcion,

                ISNULL(
                    RTRIM(CONVERT(VARCHAR(20), S.DISTRITO)),
                    ''
                ) AS distrito,
                ISNULL(RTRIM(DIST.DESCRIPCION), '')
                    AS distrito_descripcion,

                ISNULL(RTRIM(S.DIRECCION), '') AS direccion,
                ISNULL(RTRIM(S.AF_EMAIL), '') AS correo,
                ISNULL(RTRIM(S.APTO), '') AS apartado,

                CASE
                    WHEN ISNULL(S.ESTADOLABORAL, 1) = 1
                    THEN 'Interino'
                    ELSE 'Propiedad'
                END AS nombramiento,

                ISNULL(
                    S.NOMBRAMIENTO_FECHA,
                    S.FECHAINGRESO
                ) AS fecha_nombramiento,

                dbo.fxAFIAnioServicio(
                    S.CEDULA,
                    @fechaServidor
                ) AS anios_servicio,

                ISNULL(
                    RTRIM(
                        CONVERT(VARCHAR(30), S.ID_PROMOTOR)
                    ),
                    ''
                ) AS promotor,
                ISNULL(RTRIM(P.NOMBRE), '')
                    AS promotor_descripcion,

                ISNULL(RTRIM(S.NOTIFICACIONES), '')
                    AS notificaciones,

                ISNULL(
                    RTRIM(
                        CONVERT(
                            VARCHAR(30),
                            S.COD_INSTITUCION
                        )
                    ),
                    ''
                ) AS institucion,
                ISNULL(RTRIM(I.DESCRIPCION), '')
                    AS institucion_descripcion,

                ISNULL(
                    RTRIM(
                        CONVERT(
                            VARCHAR(30),
                            S.COD_PROFESION
                        )
                    ),
                    ''
                ) AS profesion,
                ISNULL(RTRIM(R.DESCRIPCION), '')
                    AS profesion_descripcion,

                ISNULL(
                    RTRIM(
                        CONVERT(
                            VARCHAR(30),
                            S.COD_SECTOR
                        )
                    ),
                    ''
                ) AS sector,
                ISNULL(RTRIM(Q.DESCRIPCION), '')
                    AS sector_descripcion,

                ISNULL(RTRIM(CONVERT(VARCHAR(30), S.UP)), '')
                    AS departamento,
                ISNULL(RTRIM(D.DESCRIPCION), '')
                    AS departamento_descripcion,

                ISNULL(RTRIM(CONVERT(VARCHAR(30), S.UT)), '')
                    AS seccion,
                ISNULL(RTRIM(X.UT_DESCRIPCION), '')
                    AS seccion_descripcion,

                ISNULL(RTRIM(CONVERT(VARCHAR(30), S.UP)), '')
                    AS unidad_programatica,
                ISNULL(RTRIM(D.DESCRIPCION), '')
                    AS unidad_programatica_descripcion,

                ISNULL(RTRIM(CONVERT(VARCHAR(30), S.UT)), '')
                    AS unidad_trabajo,
                ISNULL(RTRIM(X.UT_DESCRIPCION), '')
                    AS unidad_trabajo_descripcion,

                ISNULL(RTRIM(CONVERT(VARCHAR(30), S.CT)), '')
                    AS centro_trabajo,
                ISNULL(RTRIM(C.DESCRIPCION), '')
                    AS centro_trabajo_descripcion,

                ISNULL(
                    RTRIM(
                        CONVERT(
                            VARCHAR(30),
                            S.COD_OFICINA
                        )
                    ),
                    ''
                ) AS oficina,
                ISNULL(RTRIM(O.DESCRIPCION), '')
                    AS oficina_descripcion,

                ISNULL(
                    RTRIM(
                        CONVERT(VARCHAR(30), S.TIPO_ID)
                    ),
                    ''
                ) AS tipo_identificacion,
                ISNULL(RTRIM(TID.DESCRIPCION), '')
                    AS tipo_identificacion_descripcion,

                ISNULL(
                    RTRIM(
                        CONVERT(
                            VARCHAR(30),
                            S.COD_SOCIEDAD
                        )
                    ),
                    ''
                ) AS tipo_sociedad,
                ISNULL(
                    RTRIM(SOC.COD_SOCIEDAD) + ' - ' +
                    RTRIM(SOC.DESCRIPCION),
                    ''
                ) AS tipo_sociedad_descripcion,

                ISNULL(
                    RTRIM(
                        CONVERT(
                            VARCHAR(30),
                            S.COD_ACTIVIDAD
                        )
                    ),
                    ''
                ) AS actividad_economica,
                ISNULL(
                    RTRIM(ACT.COD_ACTIVIDAD) + ' - ' +
                    RTRIM(ACT.DESCRIPCION),
                    ''
                ) AS actividad_economica_descripcion,

                ISNULL(S.HIJOS, 0) AS hijos,
                ISNULL(S.AF_NPAGOS, 0) AS numero_pagos
            FROM SOCIOS S
            INNER JOIN INSTITUCIONES I
                ON S.COD_INSTITUCION = I.COD_INSTITUCION
            LEFT JOIN UPROGRAMATICA D
                ON S.UP = D.CODIGO
            LEFT JOIN UTRABAJO X
                ON S.UT = X.UT_CODIGO
            LEFT JOIN UPROGRAMATICA C
                ON S.CT = C.CODIGO
            INNER JOIN PROMOTORES P
                ON S.ID_PROMOTOR = P.ID_PROMOTOR
            INNER JOIN AFI_PROFESIONES R
                ON S.COD_PROFESION = R.COD_PROFESION
            INNER JOIN AFI_SECTORES Q
                ON S.COD_SECTOR = Q.COD_SECTOR
            INNER JOIN AFI_ESTADOS_PERSONA EST
                ON S.ESTADOACTUAL = EST.COD_ESTADO
            LEFT JOIN PROVINCIAS PROV
                ON S.PROVINCIA = PROV.PROVINCIA
            LEFT JOIN CANTONES CANT
                ON S.PROVINCIA = CANT.PROVINCIA
                AND S.CANTON = CANT.CANTON
            LEFT JOIN DISTRITOS DIST
                ON S.PROVINCIA = DIST.PROVINCIA
                AND S.CANTON = DIST.CANTON
                AND S.DISTRITO = DIST.DISTRITO
            LEFT JOIN SIF_OFICINAS O
                ON S.COD_OFICINA = O.COD_OFICINA
            LEFT JOIN AFI_TIPOS_IDS TID
                ON S.TIPO_ID = TID.TIPO_ID
            LEFT JOIN AFI_SOCIEDADES_TIPOS SOC
                ON S.COD_SOCIEDAD = SOC.COD_SOCIEDAD
            LEFT JOIN AFI_ACTIVIDADES_ECO ACT
                ON S.COD_ACTIVIDAD = ACT.COD_ACTIVIDAD
            WHERE S.CEDULA = @cedula;
            """;

        #endregion

        #region Seguimiento

        /// <summary>
        /// Obtiene el historial completo de etiquetas registradas para la
        /// afiliación y la boleta indicadas.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="cedula"></param>
        /// <param name="consecutivo"></param>
        /// <returns></returns>
        public ErrorDto<
            List<AfSeguimientoRevisionesTagSeguimientoData>>
            AF_SeguimientoRevisionesTag_Seguimiento_Lista_Obtener(
                int CodEmpresa,
                string? cedula,
                long consecutivo)
        {
            string identificacion = (cedula ?? string.Empty).Trim();

            if (identificacion.Length == 0)
            {
                return DbHelper.CreateErrorResponse(
                    MensajeCedulaRequerida,
                    -2,
                    new List<
                        AfSeguimientoRevisionesTagSeguimientoData>());
            }

            if (consecutivo <= 0)
            {
                return DbHelper.CreateErrorResponse(
                    MensajeConsecutivoRequerido,
                    -2,
                    new List<
                        AfSeguimientoRevisionesTagSeguimientoData>());
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
                WHERE OT.CODIGO = @cedula
                  AND OT.COD_MODULO = @modulo
                  AND OT.DOCUMENTO = @documento;
                """;

            string documento = Convert.ToString(
                consecutivo,
                CultureInfo.InvariantCulture);

            return EjecutarConsulta(
                CodEmpresa,
                connection => connection.Query<
                    AfSeguimientoRevisionesTagSeguimientoData>(
                        sql,
                        new
                        {
                            cedula = identificacion,
                            modulo = CodigoModulo,
                            documento
                        }).ToList(),
                new List<
                    AfSeguimientoRevisionesTagSeguimientoData>());
        }

        /// <summary>
        /// Exporta el historial completo de etiquetas registradas para la
        /// afiliación y la boleta indicadas.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="cedula"></param>
        /// <param name="consecutivo"></param>
        /// <returns></returns>
        public ErrorDto<
            List<AfSeguimientoRevisionesTagSeguimientoData>>
            AF_SeguimientoRevisionesTag_Seguimiento_Lista_Export(
                int CodEmpresa,
                string? cedula,
                long consecutivo)
        {
            return AF_SeguimientoRevisionesTag_Seguimiento_Lista_Obtener(
                CodEmpresa,
                cedula,
                consecutivo);
        }

        #endregion

        #region Revisión

        /// <summary>
        /// Obtiene las etiquetas activas del módulo de Afiliaciones
        /// autorizadas para el usuario.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto<List<AfSeguimientoRevisionesTagEtiquetaData>>
            AF_SeguimientoRevisionesTag_Etiquetas_Dropdown_Obtener(
                int CodEmpresa,
                string? usuario)
        {
            string usuarioActual = (usuario ?? string.Empty)
                .Trim()
                .ToUpperInvariant();

            if (usuarioActual.Length == 0)
            {
                return DbHelper.CreateErrorResponse(
                    "Debe indicar el usuario.",
                    -2,
                    new List<
                        AfSeguimientoRevisionesTagEtiquetaData>());
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

            return EjecutarConsulta(
                CodEmpresa,
                connection => connection.Query<
                    AfSeguimientoRevisionesTagEtiquetaData>(
                        sql,
                        new
                        {
                            usuario = usuarioActual,
                            modulo = CodigoModulo
                        }).ToList(),
                new List<
                    AfSeguimientoRevisionesTagEtiquetaData>());
        }

        /// <summary>
        /// Obtiene todas las omisiones configuradas para Afiliaciones y
        /// determina cuáles están seleccionadas o aplicadas en la boleta.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="cedula"></param>
        /// <param name="consecutivo"></param>
        /// <returns></returns>
        public ErrorDto<List<AfSeguimientoRevisionesTagRevisionData>>
            AF_SeguimientoRevisionesTag_Revisiones_Lista_Obtener(
                int CodEmpresa,
                string? cedula,
                long consecutivo)
        {
            string identificacion = (cedula ?? string.Empty).Trim();

            if (identificacion.Length == 0)
            {
                return DbHelper.CreateErrorResponse(
                    MensajeCedulaRequerida,
                    -2,
                    new List<
                        AfSeguimientoRevisionesTagRevisionData>());
            }

            if (consecutivo <= 0)
            {
                return DbHelper.CreateErrorResponse(
                    MensajeConsecutivoRequerido,
                    -2,
                    new List<
                        AfSeguimientoRevisionesTagRevisionData>());
            }

            const string sql = """
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
                    AND ER.CODIGO = @cedula
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

            return EjecutarConsulta(
                CodEmpresa,
                connection => connection.Query<
                    AfSeguimientoRevisionesTagRevisionData>(
                        sql,
                        new
                        {
                            cedula = identificacion,
                            modulo = CodigoModulo,
                            documento
                        }).ToList(),
                new List<
                    AfSeguimientoRevisionesTagRevisionData>());
        }

        /// <summary>
        /// Exporta todas las omisiones configuradas para Afiliaciones con su
        /// estado dentro de la boleta indicada.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="cedula"></param>
        /// <param name="consecutivo"></param>
        /// <returns></returns>
        public ErrorDto<List<AfSeguimientoRevisionesTagRevisionData>>
            AF_SeguimientoRevisionesTag_Revisiones_Lista_Export(
                int CodEmpresa,
                string? cedula,
                long consecutivo)
        {
            return AF_SeguimientoRevisionesTag_Revisiones_Lista_Obtener(
                CodEmpresa,
                cedula,
                consecutivo);
        }

        #endregion

        #region Aplicar

        /// <summary>
        /// Registra la etiqueta y sincroniza las omisiones seleccionadas
        /// dentro de una única transacción empresarial.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto AF_SeguimientoRevisionesTag_Aplicar(
            int CodEmpresa,
            string? usuario,
            AfSeguimientoRevisionesTagAplicarRequest? request)
        {
            string usuarioActual = (usuario ?? string.Empty)
                .Trim()
                .ToUpperInvariant();

            string validacion = ValidarAplicacion(
                request,
                usuarioActual);

            if (validacion.Length > 0)
            {
                return DbHelper.ErrorResponse(
                    validacion,
                    -2);
            }

            string cedula = (
                request?.cedula ?? string.Empty)
                .Trim();

            string tagCodigo = (
                request?.tag_codigo ?? string.Empty)
                .Trim();

            long consecutivo =
                request?.consecutivo ?? 0L;

            string documento = Convert.ToString(
                consecutivo,
                CultureInfo.InvariantCulture);

            string observacion = (
                request?.observacion ?? string.Empty)
                .Trim();

            List<int> erroresSeleccionados =
                request?.revisiones
                    .Where(item =>
                        item.seleccionado == true &&
                        item.id_error.GetValueOrDefault() > 0)
                    .Select(item =>
                        item.id_error.GetValueOrDefault())
                    .Distinct()
                    .ToList() ??
                new List<int>();

            ErrorDto<bool> validacionCadena =
                _proGrxMain.fxSIFValidaCadena(cedula);

            int codigoValidacion =
                validacionCadena.Code ?? -1;

            string mensajeValidacion =
                validacionCadena.Description ??
                "La identificación indicada no es válida.";

            if (codigoValidacion != 0 ||
                !validacionCadena.Result)
            {
                return DbHelper.ErrorResponse(
                    mensajeValidacion,
                    codigoValidacion);
            }

            string connectionString =
                _portalDB.ObtenerDbConnStringEmpresa(
                    CodEmpresa);

            try
            {
                using var connection =
                    new SqlConnection(connectionString);

                connection.Open();

                using var transaction =
                    connection.BeginTransaction();

                BloquearAplicacion(
                    connection,
                    transaction,
                    cedula,
                    documento);

                if (!EtiquetaPermitida(
                    connection,
                    transaction,
                    tagCodigo,
                    usuarioActual))
                {
                    return DbHelper.ErrorResponse(
                        "La etiqueta seleccionada no está disponible " +
                        "para el usuario.",
                        -2);
                }

                if (!ErroresSeleccionadosValidos(
                    connection,
                    transaction,
                    erroresSeleccionados))
                {
                    return DbHelper.ErrorResponse(
                        "La selección contiene errores que no están " +
                        "activos para el módulo de Afiliaciones.",
                        -2);
                }

                SincronizarOmisiones(
                    connection,
                    transaction,
                    cedula,
                    documento,
                    usuarioActual,
                    erroresSeleccionados);

                RegistrarEtiqueta(
                    connection,
                    transaction,
                    cedula,
                    documento,
                    usuarioActual,
                    tagCodigo,
                    observacion);

                MarcarOmisionesAplicadas(
                    connection,
                    transaction,
                    cedula,
                    documento);

                transaction.Commit();

                RegistrarBitacora(
                    CodEmpresa,
                    usuarioActual,
                    cedula,
                    documento,
                    tagCodigo);

                return DbHelper.OkResponse(
                    "Etiqueta aplicada satisfactoriamente.");
            }
            catch (SqlException ex)
            {
                return DbHelper.ErrorResponse(
                    ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return DbHelper.ErrorResponse(
                    ex.Message);
            }
            catch (DataException ex)
            {
                return DbHelper.ErrorResponse(
                    ex.Message);
            }
        }

        /// <summary>
        /// Valida los campos requeridos para aplicar una etiqueta.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        private static string ValidarAplicacion(
            AfSeguimientoRevisionesTagAplicarRequest? request,
            string usuario)
        {
            if (request == null)
            {
                return "No se recibió la información que desea aplicar.";
            }

            if (string.IsNullOrWhiteSpace(request.cedula))
            {
                return MensajeCedulaRequerida;
            }

            if (!request.consecutivo.HasValue ||
                request.consecutivo.Value <= 0)
            {
                return MensajeConsecutivoRequerido;
            }

            if (string.IsNullOrWhiteSpace(request.tag_codigo))
            {
                return "Debe seleccionar la etiqueta que desea aplicar.";
            }

            if (usuario.Length == 0)
            {
                return "Debe indicar el usuario.";
            }

            return string.Empty;
        }

        /// <summary>
        /// Serializa la aplicación de una etiqueta para evitar movimientos
        /// concurrentes sobre la misma afiliación y boleta.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="transaction"></param>
        /// <param name="cedula"></param>
        /// <param name="documento"></param>
        private static void BloquearAplicacion(
            SqlConnection connection,
            SqlTransaction transaction,
            string cedula,
            string documento)
        {
            const string sql = """
                DECLARE @resultado INT;

                EXEC @resultado = sys.sp_getapplock
                    @Resource = @recurso,
                    @LockMode = 'Exclusive',
                    @LockOwner = 'Transaction',
                    @LockTimeout = 10000;

                SELECT @resultado;
                """;

            string recurso =
                $"AF_SeguimientoRevisionesTag:{cedula}:{documento}";

            int resultado = connection.QuerySingle<int>(
                sql,
                new
                {
                    recurso
                },
                transaction);

            if (resultado < 0)
            {
                throw new InvalidOperationException(
                    "No fue posible bloquear la afiliación para aplicar " +
                    "la revisión.");
            }
        }
        /// <summary>
        /// Guarda o elimina la selección de una omisión para la afiliación y
        /// boleta indicadas, sin marcarla como aplicada.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<long?>
            AF_SeguimientoRevisionesTag_Seleccion_Actualizar(
                int CodEmpresa,
                string? usuario,
                AfSeguimientoRevisionesTagSeleccionRequest? request)
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

            string cedula = (
                request?.cedula ?? string.Empty)
                .Trim();

            long consecutivo =
                request?.consecutivo ?? 0L;

            int idError =
                request?.id_error ?? 0;

            bool seleccionado =
                request?.seleccionado ?? false;

            long lineaError =
                request?.linea_err ?? 0L;

            string documento = Convert.ToString(
                consecutivo,
                CultureInfo.InvariantCulture);

            ErrorDto<bool> validacionCadena =
                _proGrxMain.fxSIFValidaCadena(cedula);

            int codigoValidacion =
                validacionCadena.Code ?? -1;

            if (codigoValidacion != 0 ||
                !validacionCadena.Result)
            {
                return DbHelper.CreateErrorResponse<long?>(
                    validacionCadena.Description ??
                    "La identificación indicada no es válida.",
                    codigoValidacion,
                    null);
            }

            string connectionString =
                _portalDB.ObtenerDbConnStringEmpresa(
                    CodEmpresa);

            try
            {
                using var connection =
                    new SqlConnection(connectionString);

                connection.Open();

                using var transaction =
                    connection.BeginTransaction();

                ErrorDto<long?> resultado = seleccionado
                    ? SeleccionarOmision(
                        connection,
                        transaction,
                        cedula,
                        documento,
                        usuarioActual,
                        idError)
                    : DeseleccionarOmision(
                        connection,
                        transaction,
                        cedula,
                        documento,
                        idError,
                        lineaError);

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

        /// <summary>
        /// Valida la información necesaria para seleccionar o deseleccionar una
        /// omisión.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        private static string ValidarSeleccion(
            AfSeguimientoRevisionesTagSeleccionRequest? request,
            string usuario)
        {
            if (request == null)
            {
                return "No se recibió la omisión que desea actualizar.";
            }

            if (string.IsNullOrWhiteSpace(request.cedula))
            {
                return MensajeCedulaRequerida;
            }

            if (!request.consecutivo.HasValue ||
                request.consecutivo.Value <= 0)
            {
                return MensajeConsecutivoRequerido;
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

        /// <summary>
        /// Registra una omisión pendiente y devuelve su número de línea.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="transaction"></param>
        /// <param name="cedula"></param>
        /// <param name="documento"></param>
        /// <param name="usuario"></param>
        /// <param name="idError"></param>
        /// <returns></returns>
        private static ErrorDto<long?> SeleccionarOmision(
            SqlConnection connection,
            SqlTransaction transaction,
            string cedula,
            string documento,
            string usuario,
            int idError)
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
                    "La omisión indicada no está activa para Afiliaciones.",
                    -2,
                    null);
            }

            const string sqlExistente = """
        SELECT TOP 1 LINEA_ERR
        FROM SIF_OMISIONESG WITH (UPDLOCK, HOLDLOCK)
        WHERE CEDULA = @cedula
          AND ID_ERROR = @idError
          AND MODULO = @modulo
          AND CODIGO = @cedula
          AND DOCUMENTO = @documento;
        """;

            long? lineaExistente = connection.QueryFirstOrDefault<long?>(
                sqlExistente,
                new
                {
                    cedula,
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
            @cedula,
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
                    documento,
                    usuario,
                    idError,
                    modulo = CodigoModulo
                },
                transaction);

            return DbHelper.CreateOkResponse<long?>(
                lineaErr);
        }

        /// <summary>
        /// Elimina una omisión pendiente mediante su número de línea.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="transaction"></param>
        /// <param name="cedula"></param>
        /// <param name="documento"></param>
        /// <param name="idError"></param>
        /// <param name="lineaErr"></param>
        /// <returns></returns>
        private static ErrorDto<long?> DeseleccionarOmision(
            SqlConnection connection,
            SqlTransaction transaction,
            string cedula,
            string documento,
            int idError,
            long lineaErr)
        {
            const string sqlEstado = """
        SELECT TOP 1
            CONVERT(
                INT,
                CASE
                    WHEN ISNULL(APLICADO, 'N') = 'S' THEN 1
                    ELSE 0
                END
            )
        FROM SIF_OMISIONESG WITH (UPDLOCK, HOLDLOCK)
        WHERE LINEA_ERR = @lineaErr
          AND CEDULA = @cedula
          AND ID_ERROR = @idError
          AND MODULO = @modulo
          AND CODIGO = @cedula
          AND DOCUMENTO = @documento;
        """;

            int? aplicada = connection.QueryFirstOrDefault<int?>(
                sqlEstado,
                new
                {
                    lineaErr,
                    cedula,
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
          AND CODIGO = @cedula
          AND DOCUMENTO = @documento
          AND ISNULL(APLICADO, 'N') <> 'S';
        """;

            connection.Execute(
                sqlEliminar,
                new
                {
                    lineaErr,
                    cedula,
                    documento,
                    idError,
                    modulo = CodigoModulo
                },
                transaction);

            return DbHelper.CreateOkResponse<long?>(
                null);
        }
        /// <summary>
        /// Verifica que la etiqueta pertenezca al módulo y esté autorizada
        /// para el usuario actual.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="transaction"></param>
        /// <param name="tagCodigo"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        private static bool EtiquetaPermitida(
            SqlConnection connection,
            SqlTransaction transaction,
            string tagCodigo,
            string usuario)
        {
            const string sql = """
                SELECT COUNT(1)
                FROM SIF_TAGS CT
                INNER JOIN SIF_TAGS_GRUPOS CTG
                    ON CT.TAG_CODIGO = CTG.TAG_CODIGO
                INNER JOIN SIF_GRPUSERS CGU
                    ON CTG.COD_GRUPO = CGU.COD_GRUPO
                WHERE CT.TAG_CODIGO = @tagCodigo
                  AND CT.ACTIVO = 1
                  AND CGU.USUARIO = @usuario
                  AND EXISTS
                  (
                      SELECT 1
                      FROM SIF_TAGS_MODULOS CTM
                      WHERE CTM.TAG_CODIGO = CT.TAG_CODIGO
                        AND CTM.COD_MODULO = @modulo
                  );
                """;

            return connection.QuerySingle<int>(
                sql,
                new
                {
                    tagCodigo,
                    usuario,
                    modulo = CodigoModulo
                },
                transaction) > 0;
        }

        /// <summary>
        /// Verifica que todos los errores seleccionados estén activos y
        /// asociados al módulo de Afiliaciones.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="transaction"></param>
        /// <param name="erroresSeleccionados"></param>
        /// <returns></returns>
        private static bool ErroresSeleccionadosValidos(
            SqlConnection connection,
            SqlTransaction transaction,
            IReadOnlyCollection<int> erroresSeleccionados)
        {
            if (erroresSeleccionados.Count == 0)
            {
                return true;
            }

            const string sql = """
                SELECT COUNT(DISTINCT E.ID_ERROR)
                FROM SIF_OMISIONES E
                WHERE E.ID_ERROR IN @erroresSeleccionados
                  AND E.ACTIVO = '1'
                  AND EXISTS
                  (
                      SELECT 1
                      FROM SIF_OMISIONES_MODULOS EM
                      WHERE EM.ID_ERROR = E.ID_ERROR
                        AND EM.COD_MODULO = @modulo
                  );
                """;

            int total = connection.QuerySingle<int>(
                sql,
                new
                {
                    erroresSeleccionados,
                    modulo = CodigoModulo
                },
                transaction);

            return total == erroresSeleccionados.Count;
        }

        /// <summary>
        /// Inserta omisiones seleccionadas que aún no existen y elimina
        /// únicamente las omisiones no aplicadas que fueron desmarcadas.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="transaction"></param>
        /// <param name="cedula"></param>
        /// <param name="documento"></param>
        /// <param name="usuario"></param>
        /// <param name="erroresSeleccionados"></param>
        private static void SincronizarOmisiones(
            SqlConnection connection,
            SqlTransaction transaction,
            string cedula,
            string documento,
            string usuario,
            IReadOnlyCollection<int> erroresSeleccionados)
        {
            EliminarOmisionesDesmarcadas(
                connection,
                transaction,
                cedula,
                documento,
                erroresSeleccionados);

            if (erroresSeleccionados.Count == 0)
            {
                return;
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
                SELECT
                    @cedula,
                    E.ID_ERROR,
                    @modulo,
                    @cedula,
                    @documento,
                    dbo.MyGetdate(),
                    @usuario
                FROM SIF_OMISIONES E
                WHERE E.ID_ERROR IN @erroresSeleccionados
                  AND E.ACTIVO = '1'
                  AND EXISTS
                  (
                      SELECT 1
                      FROM SIF_OMISIONES_MODULOS EM
                      WHERE EM.ID_ERROR = E.ID_ERROR
                        AND EM.COD_MODULO = @modulo
                  )
                  AND NOT EXISTS
                  (
                      SELECT 1
                      FROM SIF_OMISIONESG ER
                      WHERE ER.CEDULA = @cedula
                        AND ER.ID_ERROR = E.ID_ERROR
                        AND ER.MODULO = @modulo
                        AND ER.CODIGO = @cedula
                        AND ER.DOCUMENTO = @documento
                  );
                """;

            connection.Execute(
                sqlInsertar,
                new
                {
                    cedula,
                    documento,
                    usuario,
                    modulo = CodigoModulo,
                    erroresSeleccionados
                },
                transaction);
        }

        /// <summary>
        /// Elimina las omisiones desmarcadas siempre que todavía no hayan
        /// sido aplicadas.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="transaction"></param>
        /// <param name="cedula"></param>
        /// <param name="documento"></param>
        /// <param name="erroresSeleccionados"></param>
        private static void EliminarOmisionesDesmarcadas(
            SqlConnection connection,
            SqlTransaction transaction,
            string cedula,
            string documento,
            IReadOnlyCollection<int> erroresSeleccionados)
        {
            const string sqlBase = """
                DELETE FROM SIF_OMISIONESG
                WHERE CEDULA = @cedula
                  AND MODULO = @modulo
                  AND CODIGO = @cedula
                  AND DOCUMENTO = @documento
                  AND ISNULL(APLICADO, 'N') <> 'S'
                """;

            string sql = erroresSeleccionados.Count == 0
                ? sqlBase + ";"
                : sqlBase +
                    "\n  AND ID_ERROR NOT IN @erroresSeleccionados;";

            connection.Execute(
                sql,
                new
                {
                    cedula,
                    documento,
                    modulo = CodigoModulo,
                    erroresSeleccionados
                },
                transaction);
        }

        /// <summary>
        /// Ejecuta spSIFRegistraTags con la misma conexión y transacción
        /// utilizada para las omisiones.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="transaction"></param>
        /// <param name="cedula"></param>
        /// <param name="documento"></param>
        /// <param name="usuario"></param>
        /// <param name="tagCodigo"></param>
        /// <param name="observacion"></param>
        private static void RegistrarEtiqueta(SqlConnection connection,SqlTransaction transaction,string cedula,string documento,string usuario,string tagCodigo,string observacion)
        {
            var parametros = new
            {
                Codigo = cedula,
                Tag = tagCodigo,
                Usuario = usuario,
                Notas = observacion,
                Documento = documento,
                Modulo = CodigoModulo,
                Llave_01 = cedula,
                Llave_02 = documento,
                Llave_03 = string.Empty
            };

            connection.Execute(
                "spSIFRegistraTags",
                parametros,
                transaction,
                commandType: CommandType.StoredProcedure);
        }

        /// <summary>
        /// Marca como aplicadas todas las omisiones asociadas a la
        /// afiliación y boleta procesadas.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="transaction"></param>
        /// <param name="cedula"></param>
        /// <param name="documento"></param>
        private static void MarcarOmisionesAplicadas(
            SqlConnection connection,
            SqlTransaction transaction,
            string cedula,
            string documento)
        {
            const string sql = """
                UPDATE SIF_OMISIONESG
                SET APLICADO = 'S'
                WHERE CEDULA = @cedula
                  AND MODULO = @modulo
                  AND CODIGO = @cedula
                  AND DOCUMENTO = @documento;
                """;

            connection.Execute(
                sql,
                new
                {
                    cedula,
                    documento,
                    modulo = CodigoModulo
                },
                transaction);
        }

        /// <summary>
        /// Registra en bitácora la aplicación de la etiqueta.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="cedula"></param>
        /// <param name="documento"></param>
        /// <param name="tagCodigo"></param>
        private void RegistrarBitacora(
            int CodEmpresa,
            string usuario,
            string cedula,
            string documento,
            string tagCodigo)
        {
            _securityMainDB.Bitacora(
                new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = usuario,
                    DetalleMovimiento =
                        $"Aplicó etiqueta {tagCodigo} a la afiliación " +
                        $"{cedula}, boleta {documento}.",
                    Movimiento = "APLICA-WEB",
                    Modulo = ModuloControlTramites
                });
        }

        #endregion

        #region Ejecución común

        /// <summary>
        /// Ejecuta consultas de lectura sobre la conexión empresarial y
        /// centraliza el manejo de excepciones esperadas.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="CodEmpresa"></param>
        /// <param name="consulta"></param>
        /// <param name="resultadoError"></param>
        /// <returns></returns>
        private ErrorDto<T> EjecutarConsulta<T>(
            int CodEmpresa,
            Func<SqlConnection, T> consulta,
            T resultadoError)
        {
            try
            {
                using var connection =
                    DbHelper.OpenConnection(
                        _portalDB,
                        CodEmpresa);

                return DbHelper.CreateOkResponse(
                    consulta(connection));
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