using System.Data;
using Dapper;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Activos_Fijos;
using Galileo.Models.Security;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Galileo.DataBaseTier.ProGrX_Activos_Fijos
{
    public class FrmActivosReasignacionesDB
    {
        private readonly int vModulo = 36;
        private readonly MSecurityMainDb _Security_MainDB;
        private readonly MReportingServicesDB _mReporting;
        private readonly PortalDB _portalDB;

        private const string FormatoFecha          = "yyyy-MM-dd";

        private const string ColA_NumPlaca         = "A.num_placa";
        private const string ColA_Nombre           = "A.Nombre";
        private const string ColA_PlacaAlterna     = "A.Placa_Alterna";
        private const string ColCodTraslado        = "cod_traslado";

        // columnas para boletas
        private const string ColNumPlaca           = "num_placa";
        private const string ColPlacaAlterna       = "PLACA_ALTERNA";
        private const string ColDescripcion        = "Descripcion";
        private const string ColRegistroFecha      = "Registro_fecha";
        private const string ColRegistroUsuario    = "Registro_Usuario";
        private const string ColPersona            = "Persona";
        private const string ColPersonaDestino     = "Persona_Destino";
        private const string ColMotivo             = "Motivo";
        private const string ColEstadoDesc         = "Estado_Desc";

        private const string MensajeOk             = "Ok";
        private const string MensajeNoEncontrado   = "No encontrado";
        private const string MensajeDatosNoProv    = "Datos no proporcionados.";
        private const string MensajeActivoNoEnc    = "Activo no encontrado.";
        private const string MensajeBoletaNoGen    = "No se pudo generar la boleta de reasignación.";
        private const string MensajeTrasladoOk     = "Traslado realizado satisfactoriamente.";

        // Lista blanca de campos para ORDER BY en Activos_Principal
        private static readonly Dictionary<string, string> SortFieldActivosMap =
            new(StringComparer.OrdinalIgnoreCase)
            {
                // Columnas en BD
                { ColA_NumPlaca,        ColA_NumPlaca },
                { ColA_PlacaAlterna,    ColA_PlacaAlterna },
                { ColA_Nombre,          ColA_Nombre },

                // Propiedades del modelo / nombres sin alias
                { "num_placa",          ColA_NumPlaca },
                { "placa_alterna",      ColA_PlacaAlterna },
                { "Placa_Alterna",      ColA_PlacaAlterna },
                { "nombre",             ColA_Nombre },
                { "Nombre",             ColA_Nombre }
            };

        // Lista blanca de campos para ORDER BY en vActivos_TrasladosHistorico
        private static readonly Dictionary<string, string> SortFieldBoletasMap =
            new(StringComparer.OrdinalIgnoreCase)
            {
                // Columnas en la vista
                { ColCodTraslado,       ColCodTraslado },
                { ColNumPlaca,          ColNumPlaca },
                { ColPlacaAlterna,      ColPlacaAlterna },
                { ColDescripcion,       ColDescripcion },
                { ColRegistroFecha,     ColRegistroFecha },
                { ColRegistroUsuario,   ColRegistroUsuario },
                { ColPersona,           ColPersona },
                { ColPersonaDestino,    ColPersonaDestino },
                { ColMotivo,            ColMotivo },
                { ColEstadoDesc,        ColEstadoDesc },

                // Posibles nombres de propiedades de DTO
                { "placa_alterna",      ColPlacaAlterna },
                { "descripcion",        ColDescripcion },
                { "registro_fecha",     ColRegistroFecha },
                { "registro_usuario",   ColRegistroUsuario },
                { "persona_origen",     ColPersona },
                { "persona_destino",    ColPersonaDestino },
                { "motivo",             ColMotivo },
                { "estado_desc",        ColEstadoDesc }
            };

        public FrmActivosReasignacionesDB(IConfiguration config)
        {
            _Security_MainDB = new MSecurityMainDb(config);
            _mReporting = new MReportingServicesDB(config);
            _portalDB = new PortalDB(config);
        }

        #region Helpers comunes

        private static int GetBoletaSortIndex(string sortFieldCanonical)
        {
            return sortFieldCanonical switch
            {
                ColNumPlaca        => 2,
                ColPlacaAlterna    => 3,
                ColDescripcion     => 4,
                ColRegistroFecha   => 5,
                ColRegistroUsuario => 6,
                ColPersona         => 7,
                ColPersonaDestino  => 8,
                ColMotivo          => 9,
                ColEstadoDesc      => 10,
                _                  => 1 // cod_traslado
            };
        }

        #endregion

        /// <summary>
        /// Obtiene el consecutivo de las boletas.
        /// </summary>
        public ErrorDto<string> Activos_Reasignacion_SiguienteBoleta_Obtener(int CodEmpresa)
        {
            var resp = new ErrorDto<string>
            {
                Code        = 0,
                Description = MensajeOk,
                Result      = string.Empty
            };

            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);

                const string sql = @"
                    SELECT RIGHT('0000000000' + CAST(ISNULL(MAX(COD_TRASLADO), 0) + 1 AS VARCHAR(10)), 10)
                    FROM ACTIVOS_TRASLADOS;";

                resp.Result = connection.ExecuteScalar<string>(sql);
            }
            catch (Exception ex)
            {
                resp.Code        = -1;
                resp.Description = ex.Message;
                resp.Result      = string.Empty;
            }

            return resp;
        }

        /// <summary>
        /// Lista paginada de activos para el F4 de No. Placa.
        /// </summary>
        public ErrorDto<ActivosReasignacionesActivosLista> Activos_Reasignacion_Activos_Lista_Obtener(
            int CodEmpresa,
            FiltrosLazyLoadData filtros)
        {
            var resp = new ErrorDto<ActivosReasignacionesActivosLista>
            {
                Code        = 0,
                Description = MensajeOk,
                Result      = new ActivosReasignacionesActivosLista()
            };

            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);

                var p           = new DynamicParameters();
                var filtroTexto = filtros?.filtro;
                var tieneFiltro = !string.IsNullOrWhiteSpace(filtroTexto);

                p.Add("@tieneFiltro", tieneFiltro ? 1 : 0);
                p.Add("@filtro",      tieneFiltro ? $"%{filtroTexto!.Trim()}%" : null);

                const string qTotal = @"
                    SELECT COUNT(1)
                    FROM Activos_Principal A
                    WHERE (@tieneFiltro = 0
                           OR A.num_placa     LIKE @filtro
                           OR A.Placa_Alterna LIKE @filtro
                           OR A.Nombre        LIKE @filtro);";

                resp.Result.total = connection.ExecuteScalar<int>(qTotal, p);

                var sortFieldKey = string.IsNullOrWhiteSpace(filtros?.sortField)
                    ? ColA_NumPlaca
                    : filtros.sortField!;

                if (!SortFieldActivosMap.TryGetValue(sortFieldKey, out var sortFieldCanonical))
                    sortFieldCanonical = ColA_NumPlaca;

                var sortIndex = sortFieldCanonical switch
                {
                    ColA_PlacaAlterna => 2,
                    ColA_Nombre       => 3,
                    _                 => 1
                };
                p.Add("@sortIndex", sortIndex);

                var sortDir = (filtros?.sortOrder ?? 0) == 0 ? 0 : 1;
                p.Add("@sortDir", sortDir);

                var pagina     = filtros?.pagina     ?? 1;
                var paginacion = filtros?.paginacion ?? 10;
                var offset     = pagina <= 1 ? 0 : (pagina - 1) * paginacion;

                p.Add("@offset", offset);
                p.Add("@fetch",  paginacion);

                const string qDatos = @"
                    SELECT
                        A.num_placa          AS num_placa,
                        A.Placa_Alterna      AS placa_alterna,
                        A.Nombre             AS nombre
                    FROM Activos_Principal A
                    WHERE (@tieneFiltro = 0
                           OR A.num_placa     LIKE @filtro
                           OR A.Placa_Alterna LIKE @filtro
                           OR A.Nombre        LIKE @filtro)
                    ORDER BY
                        -- ASC
                        CASE @sortDir WHEN 1 THEN
                            CASE @sortIndex
                                WHEN 1 THEN A.num_placa
                                WHEN 2 THEN A.Placa_Alterna
                                WHEN 3 THEN A.Nombre
                            END
                        END ASC,
                        -- DESC
                        CASE @sortDir WHEN 0 THEN
                            CASE @sortIndex
                                WHEN 1 THEN A.num_placa
                                WHEN 2 THEN A.Placa_Alterna
                                WHEN 3 THEN A.Nombre
                            END
                        END DESC
                    OFFSET @offset ROWS
                    FETCH NEXT @fetch ROWS ONLY;";

                resp.Result.lista = connection
                    .Query<ActivosReasignacionesActivoResumen>(qDatos, p)
                    .ToList();
            }
            catch (Exception ex)
            {
                resp.Code              = -1;
                resp.Description       = ex.Message;
                resp.Result.total      = 0;
                resp.Result.lista      = [];
            }

            return resp;
        }

        /// <summary>
        /// Trae la información de un activo por número de placa.
        /// </summary>
        public ErrorDto<ActivosReasignacionesActivo> Activos_Reasignacion_Activo_Obtener(
            int CodEmpresa,
            string numPlaca)
        {
            var resp = new ErrorDto<ActivosReasignacionesActivo>
            {
                Code        = 0,
                Description = MensajeOk,
                Result      = null
            };

            const string sql = @"
                SET NOCOUNT ON;

                SELECT TOP (1)
                    A.NUM_PLACA        AS num_placa,
                    A.NOMBRE           AS nombre,
                    A.TIPO_ACTIVO      AS tipo_activo,
                    A.COD_DEPARTAMENTO AS cod_departamento,
                    ISNULL(D.DESCRIPCION, '') AS departamento,
                    A.COD_SECCION      AS cod_seccion,
                    ISNULL(S.DESCRIPCION, '') AS seccion,
                    A.IDENTIFICACION   AS identificacion,
                    ISNULL(P.NOMBRE, '')      AS persona
                FROM ACTIVOS_PRINCIPAL A
                LEFT JOIN ACTIVOS_PERSONAS P
                       ON P.IDENTIFICACION = A.IDENTIFICACION
                LEFT JOIN ACTIVOS_DEPARTAMENTOS D
                       ON D.COD_DEPARTAMENTO = A.COD_DEPARTAMENTO
                LEFT JOIN ACTIVOS_SECCIONES S
                       ON S.COD_DEPARTAMENTO = A.COD_DEPARTAMENTO
                      AND S.COD_SECCION      = A.COD_SECCION
                WHERE A.NUM_PLACA = @numPlaca;";

            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);

                resp.Result = connection.QueryFirstOrDefault<ActivosReasignacionesActivo>(
                    sql,
                    new { numPlaca });

                if (resp.Result is null)
                {
                    resp.Code        = -2;
                    resp.Description = MensajeActivoNoEnc;
                }
            }
            catch (Exception ex)
            {
                resp.Code        = -1;
                resp.Description = ex.Message;
                resp.Result      = null;
            }

            return resp;
        }

        /// <summary>
        /// Buscar personas para reasignación de responsables (sin paginación).
        /// </summary>
        public ErrorDto<List<DropDownListaGenericaModel>> Activos_Reasignacion_Personas_Buscar(
            int CodEmpresa,
            FiltrosLazyLoadData filtros)
        {
            var resp = new ErrorDto<List<DropDownListaGenericaModel>>
            {
                Code        = 0,
                Description = MensajeOk,
                Result      = new List<DropDownListaGenericaModel>()
            };

            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);

                var p           = new DynamicParameters();
                var filtroTexto = filtros?.filtro;
                var tieneFiltro = !string.IsNullOrWhiteSpace(filtroTexto);

                p.Add("@tieneFiltro", tieneFiltro ? 1 : 0);
                p.Add("@filtro",      tieneFiltro ? $"%{filtroTexto!.Trim()}%" : null);

                string? excluir = null;
                if (!string.IsNullOrWhiteSpace(filtros?.sortField) &&
                    filtros.sortField.StartsWith("excluir:", StringComparison.OrdinalIgnoreCase))
                {
                    var splitArr = filtros.sortField.Split(':');
                    excluir      = splitArr[^1];
                }

                var tieneExcluir = !string.IsNullOrWhiteSpace(excluir);
                p.Add("@tieneExcluir", tieneExcluir ? 1 : 0);
                p.Add("@excluir",      tieneExcluir ? excluir : null);

                const string query = @"
                    SELECT Identificacion AS item, Nombre AS descripcion
                    FROM Activos_Personas
                    WHERE (@tieneFiltro = 0
                           OR Nombre         LIKE @filtro
                           OR Identificacion LIKE @filtro)
                      AND (@tieneExcluir = 0
                           OR Identificacion <> @excluir)
                    ORDER BY Nombre ASC;";

                resp.Result = connection
                    .Query<DropDownListaGenericaModel>(query, p)
                    .ToList();
            }
            catch (Exception ex)
            {
                resp.Code        = -1;
                resp.Description = ex.Message;
                resp.Result      = null;
            }

            return resp;
        }

        /// <summary>
        /// Obtener datos de persona por identificación.
        /// </summary>
        public ErrorDto<ActivosReasignacionesPersona> Activos_Reasignacion_Persona_Obtener(
            int CodEmpresa,
            string identificacion)
        {
            var resp = new ErrorDto<ActivosReasignacionesPersona>
            {
                Code        = 0,
                Description = MensajeOk,
                Result      = null
            };

            const string sql = @"
                SET NOCOUNT ON;

                SELECT TOP (1)
                    p.IDENTIFICACION                         AS identificacion,
                    p.NOMBRE                                 AS persona,
                    p.COD_DEPARTAMENTO                       AS cod_departamento,
                    ISNULL(d.DESCRIPCION, '')                AS departamento,
                    p.COD_SECCION                            AS cod_seccion,
                    ISNULL(s.DESCRIPCION, '')                AS seccion
                FROM ACTIVOS_PERSONAS p
                LEFT JOIN ACTIVOS_DEPARTAMENTOS d
                       ON d.COD_DEPARTAMENTO = p.COD_DEPARTAMENTO
                LEFT JOIN ACTIVOS_SECCIONES s
                       ON s.COD_DEPARTAMENTO = p.COD_DEPARTAMENTO
                      AND s.COD_SECCION      = p.COD_SECCION
                WHERE p.IDENTIFICACION = @identificacion;";

            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);

                resp.Result = connection.QueryFirstOrDefault<ActivosReasignacionesPersona>(
                    sql,
                    new { identificacion });

                if (resp.Result is null)
                {
                    resp.Description = MensajeNoEncontrado;
                }
            }
            catch (Exception ex)
            {
                resp.Code        = -1;
                resp.Description = ex.Message;
                resp.Result      = null;
            }

            return resp;
        }

        /// <summary>
        /// Obtener catálogo de motivos activos para reasignación de activos.
        /// </summary>
        public ErrorDto<List<DropDownListaGenericaModel>> Activos_Reasignacion_Motivos_Obtener(int CodEmpresa)
        {
            var resp = new ErrorDto<List<DropDownListaGenericaModel>>
            {
                Code        = 0,
                Description = MensajeOk,
                Result      = new List<DropDownListaGenericaModel>()
            };

            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);

                const string query = @"
                    SELECT cod_motivo AS item, descripcion
                    FROM ACTIVOS_TRASLADOS_MOTIVOS
                    WHERE ACTIVO = 1
                    ORDER BY descripcion ASC";

                resp.Result = connection
                    .Query<DropDownListaGenericaModel>(query)
                    .ToList();
            }
            catch (Exception ex)
            {
                resp.Code        = -1;
                resp.Description = ex.Message;
                resp.Result      = null;
            }

            return resp;
        }

        // ---- Validación de datos de cambio de responsable ----

        private static List<string> ValidarCambioResponsable(ActivosReasignacionesCambioRequest data)
        {
            var errores = new List<string>();

            if (string.IsNullOrWhiteSpace(data.num_placa))
                errores.Add("No se especificó ningún activo.");
            if (string.IsNullOrWhiteSpace(data.identificacion))
                errores.Add("No se especificó el responsable actual.");
            if (string.IsNullOrWhiteSpace(data.identificacion_destino))
                errores.Add("No se especificó el responsable nuevo.");
            if (string.IsNullOrWhiteSpace(data.cod_motivo))
                errores.Add("No se indicó el motivo.");
            if (string.IsNullOrWhiteSpace(data.fecha_aplicacion))
                errores.Add("No se indicó la fecha de aplicación (YYYY-MM-DD).");

            return errores;
        }

        /// <summary>
        /// Ejecuta el SP de cambio de responsable (Reasignaciones).
        /// </summary>
        public ErrorDto<ActivosReasignacionesBoletaResult> Activos_Reasignacion_CambioResponsable(
            int CodEmpresa,
            ActivosReasignacionesCambioRequest data)
        {
            var resp = new ErrorDto<ActivosReasignacionesBoletaResult>
            {
                Code        = 0,
                Description = string.Empty,
                Result      = null
            };

            try
            {
                if (data == null)
                {
                    return new ErrorDto<ActivosReasignacionesBoletaResult>
                    {
                        Code        = -1,
                        Description = MensajeDatosNoProv,
                        Result      = null
                    };
                }

                var errores = ValidarCambioResponsable(data);
                if (errores.Count > 0)
                {
                    return new ErrorDto<ActivosReasignacionesBoletaResult>
                    {
                        Code        = -1,
                        Description = string.Join(" | ", errores),
                        Result      = null
                    };
                }

                using var connection = _portalDB.CreateConnection(CodEmpresa);

                var p = new DynamicParameters();
                p.Add("@Boleta",          data.cod_traslado);
                p.Add("@Placa",           data.num_placa);
                p.Add("@Motivo",          data.cod_motivo);
                p.Add("@Identificacion",  data.identificacion_destino);
                p.Add("@Usuario",         data.usuario);
                p.Add("@Notas",           data.notas);
                p.Add("@Estado",          "P");
                p.Add("@FechaAplicacion", DateTime.ParseExact(
                    data.fecha_aplicacion,
                    FormatoFecha,
                    System.Globalization.CultureInfo.InvariantCulture));

                var rs = connection.QueryFirstOrDefault<dynamic>(
                    "spActivos_ResponsableCambio",
                    p,
                    commandType: CommandType.StoredProcedure);

                string boleta = rs?.Boleta ?? string.Empty;

                if (string.IsNullOrWhiteSpace(boleta))
                {
                    return new ErrorDto<ActivosReasignacionesBoletaResult>
                    {
                        Code        = -2,
                        Description = MensajeBoletaNoGen,
                        Result      = null
                    };
                }

                _Security_MainDB.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId         = CodEmpresa,
                    Usuario           = data.usuario ?? string.Empty,
                    DetalleMovimiento = $"Reasignación de activo: {data.num_placa}, " +
                                        $"Persona Origen: {data.identificacion}, " +
                                        $"Persona Destino: {data.identificacion_destino}",
                    Movimiento        = "Registra - WEB",
                    Modulo            = vModulo
                });

                resp.Code        = 0;
                resp.Description = MensajeTrasladoOk;
                resp.Result      = new ActivosReasignacionesBoletaResult
                {
                    cod_traslado = boleta
                };
            }
            catch (Exception ex)
            {
                resp.Code        = -1;
                resp.Description = ex.Message;
                resp.Result      = null;
            }

            return resp;
        }

        /// <summary>
        /// Lista paginada de boletas de reasignación (historial).
        /// </summary>
        public ErrorDto<ActivosReasignacionesBoletaHistorialLista> Activos_Reasignacion_BoletasLista_Obtener(
            int CodEmpresa,
            ActivosReasignacionesBoletasFiltros filtros)
        {
            var resp = new ErrorDto<ActivosReasignacionesBoletaHistorialLista>
            {
                Code        = 0,
                Description = MensajeOk,
                Result      = new ActivosReasignacionesBoletaHistorialLista()
            };

            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);

                // Normalizar filtros a parámetros opcionales
                string? numPlacaLike = null;
                if (filtros.todosActivos == 0 && !string.IsNullOrWhiteSpace(filtros.numPlaca))
                    numPlacaLike = "%" + filtros.numPlaca.Trim() + "%";

                DateTime? fechaInicioDesde = null;
                if (!string.IsNullOrWhiteSpace(filtros.fechaInicio))
                {
                    fechaInicioDesde = DateTime.ParseExact(
                        filtros.fechaInicio,
                        FormatoFecha,
                        System.Globalization.CultureInfo.InvariantCulture
                    ).Date;
                }

                DateTime? fechaCorteHasta = null;
                if (!string.IsNullOrWhiteSpace(filtros.fechaCorte))
                {
                    fechaCorteHasta = DateTime.ParseExact(
                        filtros.fechaCorte,
                        FormatoFecha,
                        System.Globalization.CultureInfo.InvariantCulture
                    ).Date.AddDays(1).AddSeconds(-1);
                }

                string? boletaInicio = string.IsNullOrWhiteSpace(filtros.boletaInicio)
                    ? null
                    : filtros.boletaInicio.Trim();

                string? boletaCorte = string.IsNullOrWhiteSpace(filtros.boletaCorte)
                    ? null
                    : filtros.boletaCorte.Trim();

                var sortFieldKey = string.IsNullOrWhiteSpace(filtros.sortField)
                    ? ColCodTraslado
                    : filtros.sortField!;

                if (!SortFieldBoletasMap.TryGetValue(sortFieldKey, out var sortFieldCanonical))
                    sortFieldCanonical = ColCodTraslado;

                var sortIndex = GetBoletaSortIndex(sortFieldCanonical);
                var sortDir   = filtros.sortOrder == 0 ? 0 : 1;

                var offset = filtros.pagina <= 1
                    ? 0
                    : (filtros.pagina - 1) * filtros.paginacion;

                var parametros = new
                {
                    NumPlacaLike     = numPlacaLike,
                    FechaInicioDesde = fechaInicioDesde,
                    FechaCorteHasta  = fechaCorteHasta,
                    BoletaInicio     = boletaInicio,
                    BoletaCorte      = boletaCorte,
                    sortIndex,
                    sortDir,
                    offset,
                    fetch            = filtros.paginacion
                };

                const string qTotal = @"
                    SELECT COUNT(1)
                    FROM vActivos_TrasladosHistorico
                    WHERE 1 = 1
                      AND (@NumPlacaLike     IS NULL OR num_placa LIKE @NumPlacaLike)
                      AND (@FechaInicioDesde IS NULL OR Fecha_Aplicacion >= @FechaInicioDesde)
                      AND (@FechaCorteHasta  IS NULL OR Fecha_Aplicacion <= @FechaCorteHasta)
                      AND (@BoletaInicio     IS NULL OR cod_traslado >= @BoletaInicio)
                      AND (@BoletaCorte      IS NULL OR cod_traslado <= @BoletaCorte);";

                resp.Result.total = connection.ExecuteScalar<int>(qTotal, parametros);

                const string qDatos = @"
                    SELECT
                        cod_traslado,
                        num_placa,
                        PLACA_ALTERNA     AS placa_alterna,
                        Descripcion       AS descripcion,
                        CONVERT(varchar(19), Registro_fecha, 120) AS registro_fecha,
                        Registro_Usuario  AS registro_usuario,
                        Persona           AS persona_origen,
                        Persona_Destino   AS persona_destino,
                        Motivo,
                        Estado_Desc       AS estado_desc
                    FROM vActivos_TrasladosHistorico
                    WHERE 1 = 1
                      AND (@NumPlacaLike     IS NULL OR num_placa LIKE @NumPlacaLike)
                      AND (@FechaInicioDesde IS NULL OR Fecha_Aplicacion >= @FechaInicioDesde)
                      AND (@FechaCorteHasta  IS NULL OR Fecha_Aplicacion <= @FechaCorteHasta)
                      AND (@BoletaInicio     IS NULL OR cod_traslado >= @BoletaInicio)
                      AND (@BoletaCorte      IS NULL OR cod_traslado <= @BoletaCorte)
                    ORDER BY
                        -- ASC
                        CASE @sortDir WHEN 1 THEN
                            CASE @sortIndex
                                WHEN 1  THEN cod_traslado
                                WHEN 2  THEN num_placa
                                WHEN 3  THEN PLACA_ALTERNA
                                WHEN 4  THEN Descripcion
                                WHEN 5  THEN Registro_fecha
                                WHEN 6  THEN Registro_Usuario
                                WHEN 7  THEN Persona
                                WHEN 8  THEN Persona_Destino
                                WHEN 9  THEN Motivo
                                WHEN 10 THEN Estado_Desc
                            END
                        END ASC,
                        -- DESC
                        CASE @sortDir WHEN 0 THEN
                            CASE @sortIndex
                                WHEN 1  THEN cod_traslado
                                WHEN 2  THEN num_placa
                                WHEN 3  THEN PLACA_ALTERNA
                                WHEN 4  THEN Descripcion
                                WHEN 5  THEN Registro_fecha
                                WHEN 6  THEN Registro_Usuario
                                WHEN 7  THEN Persona
                                WHEN 8  THEN Persona_Destino
                                WHEN 9  THEN Motivo
                                WHEN 10 THEN Estado_Desc
                            END
                        END DESC
                    OFFSET @offset ROWS
                    FETCH NEXT @fetch ROWS ONLY;";

                resp.Result.lista = connection
                    .Query<ActivosReasignacionesBoletaHistorialItem>(qDatos, parametros)
                    .ToList();
            }
            catch (Exception ex)
            {
                resp.Code              = -1;
                resp.Description       = ex.Message;
                resp.Result.total      = 0;
                resp.Result.lista      = [];
            }

            return resp;
        }

        /// <summary>
        /// Obtener una boleta específica de reasignación por código de traslado.
        /// </summary>
        public ErrorDto<ActivosReasignacionesBoleta> Activos_Reasignacion_Obtener(
            int CodEmpresa,
            string cod_traslado)
        {
            var resp = new ErrorDto<ActivosReasignacionesBoleta>
            {
                Code        = 0,
                Description = MensajeOk,
                Result      = null
            };

            const string sql = @"
                SELECT TOP (1)
                    cod_traslado,
                    num_placa,
                    cod_motivo,
                    motivo,
                    notas,
                    estado,
                    estado_desc,
                    registro_usuario,
                    CONVERT(varchar(19), registro_fecha, 120) AS registro_fecha,
                    cerrado_usuario,
                    CONVERT(varchar(19), cerrado_fecha, 120)   AS cerrado_fecha,
                    procesado_usuario,
                    CONVERT(varchar(19), procesado_fecha, 120) AS procesado_fecha,
                    CONVERT(varchar(10), fecha_aplicacion, 120) AS fecha_aplicacion,
                    identificacion,
                    persona,
                    cod_departamento,
                    departamento,
                    cod_seccion,
                    seccion,
                    identificacion_destino,
                    persona_destino,
                    cod_departamento_destino,
                    departamento_destino,
                    cod_seccion_destino,
                    seccion_destino
                FROM vActivos_TrasladosHistorico
                WHERE cod_traslado = @cod_traslado;";

            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);

                resp.Result = connection.QueryFirstOrDefault<ActivosReasignacionesBoleta>(
                    sql,
                    new { cod_traslado });

                if (resp.Result == null)
                {
                    resp.Code        = -2;
                    resp.Description = "Boleta no encontrada.";
                }
            }
            catch (Exception ex)
            {
                resp.Code        = -1;
                resp.Description = ex.Message;
                resp.Result      = null;
            }

            return resp;
        }

        /// <summary>
        /// Obtener lista completa de boletas de reasignación (historial) para exportar.
        /// </summary>
        public ErrorDto<List<ActivosReasignacionesBoletaHistorialItem>> Activos_Reasignacion_Boletas_Export(
            int CodEmpresa,
            ActivosReasignacionesBoletasFiltros filtros)
        {
            var resp = new ErrorDto<List<ActivosReasignacionesBoletaHistorialItem>>
            {
                Code        = 0,
                Description = MensajeOk,
                Result      = new List<ActivosReasignacionesBoletaHistorialItem>()
            };

            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);

                // Normalizar filtros a parámetros opcionales
                string? numPlacaLike = null;
                if (filtros.todosActivos == 0 && !string.IsNullOrWhiteSpace(filtros.numPlaca))
                    numPlacaLike = "%" + filtros.numPlaca.Trim() + "%";

                DateTime? fechaInicioDesde = null;
                if (!string.IsNullOrWhiteSpace(filtros.fechaInicio))
                {
                    fechaInicioDesde = DateTime.ParseExact(
                        filtros.fechaInicio,
                        FormatoFecha,
                        System.Globalization.CultureInfo.InvariantCulture
                    ).Date;
                }

                DateTime? fechaCorteHasta = null;
                if (!string.IsNullOrWhiteSpace(filtros.fechaCorte))
                {
                    fechaCorteHasta = DateTime.ParseExact(
                        filtros.fechaCorte,
                        FormatoFecha,
                        System.Globalization.CultureInfo.InvariantCulture
                    ).Date.AddDays(1).AddSeconds(-1);
                }

                string? boletaInicio = string.IsNullOrWhiteSpace(filtros.boletaInicio)
                    ? null
                    : filtros.boletaInicio.Trim();

                string? boletaCorte = string.IsNullOrWhiteSpace(filtros.boletaCorte)
                    ? null
                    : filtros.boletaCorte.Trim();

                var sortFieldKey = string.IsNullOrWhiteSpace(filtros.sortField)
                    ? ColCodTraslado
                    : filtros.sortField!;

                if (!SortFieldBoletasMap.TryGetValue(sortFieldKey, out var sortFieldCanonical))
                    sortFieldCanonical = ColCodTraslado;

                var sortIndex = GetBoletaSortIndex(sortFieldCanonical);
                var sortDir   = filtros.sortOrder == 0 ? 0 : 1;

                var parametros = new
                {
                    NumPlacaLike     = numPlacaLike,
                    FechaInicioDesde = fechaInicioDesde,
                    FechaCorteHasta  = fechaCorteHasta,
                    BoletaInicio     = boletaInicio,
                    BoletaCorte      = boletaCorte,
                    sortIndex,
                    sortDir
                };

                const string qDatos = @"
                    SELECT
                        cod_traslado,
                        num_placa,
                        PLACA_ALTERNA     AS placa_alterna,
                        Descripcion       AS descripcion,
                        CONVERT(varchar(19), Registro_fecha, 120) AS registro_fecha,
                        Registro_Usuario  AS registro_usuario,
                        Persona           AS persona_origen,
                        Persona_Destino   AS persona_destino,
                        Motivo,
                        Estado_Desc       AS estado_desc
                    FROM vActivos_TrasladosHistorico
                    WHERE 1 = 1
                      AND (@NumPlacaLike     IS NULL OR num_placa LIKE @NumPlacaLike)
                      AND (@FechaInicioDesde IS NULL OR Fecha_Aplicacion >= @FechaInicioDesde)
                      AND (@FechaCorteHasta  IS NULL OR Fecha_Aplicacion <= @FechaCorteHasta)
                      AND (@BoletaInicio     IS NULL OR cod_traslado >= @BoletaInicio)
                      AND (@BoletaCorte      IS NULL OR cod_traslado <= @BoletaCorte)
                    ORDER BY
                        -- ASC
                        CASE @sortDir WHEN 1 THEN
                            CASE @sortIndex
                                WHEN 1  THEN cod_traslado
                                WHEN 2  THEN num_placa
                                WHEN 3  THEN PLACA_ALTERNA
                                WHEN 4  THEN Descripcion
                                WHEN 5  THEN Registro_fecha
                                WHEN 6  THEN Registro_Usuario
                                WHEN 7  THEN Persona
                                WHEN 8  THEN Persona_Destino
                                WHEN 9  THEN Motivo
                                WHEN 10 THEN Estado_Desc
                            END
                        END ASC,
                        -- DESC
                        CASE @sortDir WHEN 0 THEN
                            CASE @sortIndex
                                WHEN 1  THEN cod_traslado
                                WHEN 2  THEN num_placa
                                WHEN 3  THEN PLACA_ALTERNA
                                WHEN 4  THEN Descripcion
                                WHEN 5  THEN Registro_fecha
                                WHEN 6  THEN Registro_Usuario
                                WHEN 7  THEN Persona
                                WHEN 8  THEN Persona_Destino
                                WHEN 9  THEN Motivo
                                WHEN 10 THEN Estado_Desc
                            END
                        END DESC;";

                resp.Result = connection
                    .Query<ActivosReasignacionesBoletaHistorialItem>(qDatos, parametros)
                    .ToList();
            }
            catch (Exception ex)
            {
                resp.Code        = -1;
                resp.Description = ex.Message;
                resp.Result      = null;
            }

            return resp;
        }

        /// <summary>
        /// Generar emisión de boletas de reasignación en lote (PDF combinado).
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// </summary>
        /// <returns></returns>
        public ErrorDto<object> Activos_Reasignacion_Boletas_Lote(int codEmpresa,ActivosReasignacionesBoletasLoteRequest request)
        {
            var response = new ErrorDto<object> { Code = 0 };
        
            if (request.Boletas == null || request.Boletas.Count == 0)
            {
                response.Code = -1;
                response.Description = "No se recibieron boletas para generar el reporte.";
                return response;
            }
        
            try
            {
                var pdfs = new List<byte[]>();
        
                foreach (var boleta in request.Boletas.Distinct())
                {
                    if (string.IsNullOrWhiteSpace(boleta))
                        continue;
        
                    var parametros = new
                    {
                        filtros =
                            $" WHERE ACTIVOS_TRASLADOS.COD_TRASLADO = '{boleta}'",
                        Empresa = (string?)null,
                        fxUsuario = request.Usuario,
                        fxSubTitulo = "TRASLADO DE ACTIVOS Y CAMBIO DE RESPONSABLES"
                    };
        
                    var reporteData = new FrmReporteGlobal
                    {
                        codEmpresa = codEmpresa,
                        parametros = JsonConvert.SerializeObject(parametros),
                        nombreReporte = "Activos_BoletaTraslado",
                        usuario = request.Usuario,
                        cod_reporte = "P",
                        folder = "Activos"
                    };
        
                    var actionResult = _mReporting.ReporteRDLC_v2(reporteData);
        
                    // Si viene un ObjectResult, interpretamos que hay ErrorDto adentro
                    if (actionResult is ObjectResult objectResult)
                    {
                        var res = objectResult.Value;
                        var jres = System.Text.Json.JsonSerializer.Serialize(res);
                        var err = System.Text.Json.JsonSerializer.Deserialize<ErrorDto>(jres);
        
                        response.Code = -1;
                        response.Description =
                            err?.Description ?? $"Error al generar boleta {boleta}.";
                        return response;
                    }
        
                    var fileResult = actionResult as FileContentResult;
        
                    if (fileResult?.FileContents == null || fileResult.FileContents.Length == 0)
                    {
                        response.Code = -1;
                        response.Description =
                            $"Ocurrió un error al generar la boleta, contenido nulo/vacío para boleta {boleta}.";
                        return response;
                    }
        
                    pdfs.Add(fileResult.FileContents);
                }
        
                if (!pdfs.Any())
                {
                    response.Code = -1;
                    response.Description = "No se generaron boletas para los códigos indicados.";
                    return response;
                }
        
                // Combinar los bytes de todos los PDFs en uno solo
                var combinadoBytes = MProGrXAuxiliarDB.CombinarBytesPdfSharp(pdfs.ToArray());
        
                var fileCombinado = new FileContentResult(combinadoBytes, "application/pdf")
                {
                    FileDownloadName = "Activos_Reasignacion_Boletas.pdf"
                };
        
                // Devolver el FileContentResult serializado (igual que en Personas/Tesorería)
                response.Result = JsonConvert.SerializeObject(fileCombinado, Formatting.Indented);
                return response;
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = null;
                return response;
            }
        }

    }
}