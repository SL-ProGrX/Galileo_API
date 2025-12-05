using Dapper;
using Galileo.Models;
using Galileo.Models.ERROR;
using System.Data;
using Galileo.Models.ProGrX_Activos_Fijos;
using Galileo.Models.Security;

namespace Galileo.DataBaseTier.ProGrX_Activos_Fijos
{
    public class FrmActivosReasignacionesDB
    {
        private readonly int vModulo = 36;
        private readonly MSecurityMainDb _Security_MainDB;
        //private readonly mReportingServicesDB _mReporting;
        private readonly PortalDB _portalDB;

        private const string _where11 = " WHERE 1=1 ";
        private const string _formatoFecha = "yyyy-MM-dd";

        public FrmActivosReasignacionesDB(IConfiguration config)
        {
            _Security_MainDB = new MSecurityMainDb(config);
            //_mReporting = new mReportingServicesDB(_config);
            _portalDB = new PortalDB(config);
        }

     
        /// <summary>
        /// Obtiene el consecutivo de las boletas
        /// <param name="CodEmpresa"></param>
        /// </summary>
        /// <returns></returns>
        public ErrorDto<string> Activos_Reasignacion_SiguienteBoleta_Obtener(int CodEmpresa)
        {
            var resp = new ErrorDto<string> { Code = 0, Description = "Ok", Result = "" };

            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);

                const string sql = @"
            SELECT RIGHT('0000000000' + CAST(ISNULL(MAX(COD_TRASLADO), 0) + 1 AS VARCHAR(10)), 10)
            FROM ACTIVOS_TRASLADOS;";

                resp.Result = connection.ExecuteScalar<string>(sql); // ya 000000065
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
                resp.Result = "";
            }

            return resp;
        }


        /// <summary>
        /// Lista paginada de activos para el F4 de No. Placa
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// </summary>
        /// <returns></returns>

        public ErrorDto<ActivosReasignacionesActivosLista> Activos_Reasignacion_Activos_Lista_Obtener(int CodEmpresa,FiltrosLazyLoadData filtros)
        {
            
            var resp = new ErrorDto<ActivosReasignacionesActivosLista>
            {
                Code = 0,
                Description = "Ok",
                Result = new ActivosReasignacionesActivosLista()
            };

            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);

                var where = _where11;
                var p = new DynamicParameters();

                if (!string.IsNullOrWhiteSpace(filtros?.filtro))
                {
                    where += @"
                AND (
                       A.num_placa     LIKE @filtro
                    OR A.Placa_Alterna LIKE @filtro
                    OR A.Nombre        LIKE @filtro
                )";
                    p.Add("@filtro", "%" + filtros.filtro + "%");
                }
                var qTotal = $@"SELECT COUNT(1)
                        FROM Activos_Principal A
                        {where}";
                resp.Result.total = connection.ExecuteScalar<int>(qTotal, p);
                var sortField = string.IsNullOrWhiteSpace(filtros?.sortField)
                    ? "A.num_placa"
                    : filtros.sortField ?? "A.num_placa";

                var sortOrder = (filtros?.sortOrder ?? 0) == 0 ? "ASC" : "DESC";

                var pagina = filtros?.pagina ?? 1;
                var paginacion = filtros?.paginacion ?? 10;
                var offset = (pagina <= 1 ? 0 : (pagina - 1) * paginacion);

                var qDatos = $@"
            SELECT
                A.num_placa          AS num_placa,
                A.Placa_Alterna      AS placa_alterna,
                A.Nombre             AS nombre
            FROM Activos_Principal A
            {where}
            ORDER BY {sortField} {sortOrder}
            OFFSET {offset} ROWS
            FETCH NEXT {paginacion} ROWS ONLY;";

                resp.Result.lista = connection
                    .Query<ActivosReasignacionesActivoResumen>(qDatos, p)
                    .ToList();
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
                resp.Result.total = 0;
                resp.Result.lista = null;
            }

            return resp;
        }
        /// <summary>
        /// Trae la información de un activo por número de placa.
        /// <param name="CodEmpresa"></param>
        /// <param name="numPlaca"></param>
        /// </summary>
        /// <returns></returns>
        public ErrorDto<ActivosReasignacionesActivo> Activos_Reasignacion_Activo_Obtener(int CodEmpresa,string numPlaca)
        {
            
            var resp = new ErrorDto<ActivosReasignacionesActivo>
            {
                Code = 0,
                Description = "Ok",
                Result = null
            };

            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);

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

                resp.Result = connection.QueryFirstOrDefault<ActivosReasignacionesActivo>(
                    sql,
                    new { numPlaca });

                if (resp.Result is null)
                {
                    resp.Code = -2;
                    resp.Description = "Activo no encontrado.";
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
                resp.Result = null;
            }

            return resp;
        }
        /// <summary>
        /// Buscar personas para reasignación de responsables (sin paginación).
        /// <param name="CodEmpresa"></param>"
        /// <param name="filtros"></param>"
        /// </summary>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Activos_Reasignacion_Personas_Buscar(int CodEmpresa, FiltrosLazyLoadData filtros)
        {
            
            var resp = new ErrorDto<List<DropDownListaGenericaModel>>
            {
                Code = 0,
                Description = "Ok",
                Result = new List<DropDownListaGenericaModel>()
            };

            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);

                string where = _where11;
                if (!string.IsNullOrWhiteSpace(filtros?.filtro))
                    where += " AND (Nombre LIKE '%" + filtros.filtro + "%' OR Identificacion LIKE '%" + filtros.filtro + "%') ";

                if (!string.IsNullOrWhiteSpace(filtros?.sortField) &&
                    filtros.sortField.StartsWith("excluir:", StringComparison.OrdinalIgnoreCase))
                {
                    var splitArr = filtros.sortField.Split(':');
                    var excluir = splitArr[splitArr.Length - 1];
                    if (!string.IsNullOrWhiteSpace(excluir))
                        where += " AND Identificacion <> '" + excluir + "' ";
                }

                var query = $@"
                        SELECT Identificacion AS item, Nombre AS descripcion
                        FROM Activos_Personas
                        {where}
                        ORDER BY Nombre ASC";

                resp.Result = connection.Query<DropDownListaGenericaModel>(query).ToList();
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
                resp.Result = null;
            }

            return resp;
        }

        /// Obtener datos de persona por identificación (para hidratar depto/sección de destino).
        /// <param name="CodEmpresa"></param>
        /// <param name="identificacion"></param>
        /// </summary>
        /// <returns></returns>
        public ErrorDto<ActivosReasignacionesPersona> Activos_Reasignacion_Persona_Obtener(int CodEmpresa, string identificacion)
        {
            
            var resp = new ErrorDto<ActivosReasignacionesPersona>
            {
                Code = 0,
                Description = "Ok",
                Result = null
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
                    sql, new { identificacion });

                if (resp.Result is null)
                {
                    resp.Description = "No encontrado";
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
                resp.Result = null;
            }

            return resp;
        }
        /// <summary>
        /// Obtener catálogo de motivos activos para reasignación de activos.
        /// <param name="CodEmpresa"></param>"
        /// </summary>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Activos_Reasignacion_Motivos_Obtener(int CodEmpresa)
        {
            var resp = new ErrorDto<List<DropDownListaGenericaModel>>
            {
                Code = 0,
                Description = "Ok",
                Result = new List<DropDownListaGenericaModel>()
            };

            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);
                const string query = @"
                SELECT cod_motivo AS item, descripcion
                FROM ACTIVOS_TRASLADOS_MOTIVOS
                WHERE ACTIVO = 1
                ORDER BY descripcion ASC";

                resp.Result = connection.Query<DropDownListaGenericaModel>(query).ToList();
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
                resp.Result = null;
            }

            return resp;
        }
        /// <summary>
        /// Ejecuta el SP de cambio de responsable (Reasignaciones),
        /// <param name="CodEmpresa"></param>"
        /// <param name="data"></param>"
        /// </summary>
        /// <returns></returns>
        public ErrorDto<ActivosReasignacionesBoletaResult> Activos_Reasignacion_CambioResponsable(int CodEmpresa,ActivosReasignacionesCambioRequest data)
        {
            var resp = new ErrorDto<ActivosReasignacionesBoletaResult>
            {
                Code = 0,
                Description = "",
                Result = null
            };

            try
            {
                if (data == null)
                {
                    return new ErrorDto<ActivosReasignacionesBoletaResult>
                    {
                        Code = -1,
                        Description = "Datos no proporcionados.",
                        Result = null
                    };
                }

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

                if (errores.Count > 0)
                {
                    return new ErrorDto<ActivosReasignacionesBoletaResult>
                    {
                        Code = -1,
                        Description = string.Join(" | ", errores),
                        Result = null
                    };
                }

                
                using var connection = _portalDB.CreateConnection(CodEmpresa);

                var p = new DynamicParameters();
                p.Add("@Boleta", data.cod_traslado);                         
                p.Add("@Placa", data.num_placa);                              
                p.Add("@Motivo", data.cod_motivo);                            
                p.Add("@Identificacion", data.identificacion_destino);       
                p.Add("@Usuario", data.usuario);
                p.Add("@Notas", data.notas);
                p.Add("@Estado", "P");                                       
                p.Add("@FechaAplicacion", DateTime.ParseExact(data.fecha_aplicacion, _formatoFecha, System.Globalization.CultureInfo.InvariantCulture));

                var rs = connection.QueryFirstOrDefault<dynamic>(
                    "spActivos_ResponsableCambio",
                    p,
                    commandType: CommandType.StoredProcedure);

                string boleta = rs?.Boleta ?? "";

                if (string.IsNullOrWhiteSpace(boleta))
                {
                    return new ErrorDto<ActivosReasignacionesBoletaResult>
                    {
                        Code = -2,
                        Description = "No se pudo generar la boleta de reasignación.",
                        Result = null
                    };
                }

                // Bitácora (aquí sí usamos actual y nuevo solo para el mensaje)
                _Security_MainDB.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = data.usuario ?? "",
                    DetalleMovimiento = $"Reasignación de activo: {data.num_placa}, " +
                                        $"Persona Origen: {data.identificacion}, " +
                                        $"Persona Destino: {data.identificacion_destino}",
                    Movimiento = "Registra - WEB",
                    Modulo = vModulo
                });

                resp.Code = 0;
                resp.Description = "Traslado realizado satisfactoriamente.";
                resp.Result = new ActivosReasignacionesBoletaResult
                {
                    cod_traslado = boleta
                };
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
                resp.Result = null;
            }

            return resp;
        }

        /// <summary>
        /// Obtener lista paginada de boletas de reasignación (historial),
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// </summary>
        /// <returns></returns>
        public ErrorDto<ActivosReasignacionesBoletaHistorialLista> Activos_Reasignacion_BoletasLista_Obtener(int CodEmpresa,ActivosReasignacionesBoletasFiltros filtros)
        {
            
            var resp = new ErrorDto<ActivosReasignacionesBoletaHistorialLista>
            {
                Code = 0,
                Description = "Ok",
                Result = new ActivosReasignacionesBoletaHistorialLista()
            };

            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);
                var where = _where11;
                var p = new DynamicParameters();

                if (filtros.todosActivos == 0 && !string.IsNullOrWhiteSpace(filtros.numPlaca))
                {
                    where += " AND num_placa LIKE @numPlaca ";
                    p.Add("@numPlaca", "%" + filtros.numPlaca + "%");
                }
                if (!string.IsNullOrWhiteSpace(filtros.fechaInicio))
                {
                    where += " AND Fecha_Aplicacion >= @FechaInicioDesde ";
                    p.Add("@FechaInicioDesde", DateTime.ParseExact(filtros.fechaInicio, _formatoFecha, System.Globalization.CultureInfo.InvariantCulture).Date);
                }
                if (!string.IsNullOrWhiteSpace(filtros.fechaCorte))
                {
                    where += " AND Fecha_Aplicacion <= @FechaCorteHasta ";
                    var corte = DateTime.ParseExact(filtros.fechaCorte, _formatoFecha, System.Globalization.CultureInfo.InvariantCulture).Date.AddDays(1).AddSeconds(-1);
                    p.Add("@FechaCorteHasta", corte);
                }
                if (!string.IsNullOrWhiteSpace(filtros.boletaInicio))
                {
                    where += " AND cod_traslado >= @BoletaInicio ";
                    p.Add("@BoletaInicio", filtros.boletaInicio);
                }
                if (!string.IsNullOrWhiteSpace(filtros.boletaCorte))
                {
                    where += " AND cod_traslado <= @BoletaCorte ";
                    p.Add("@BoletaCorte", filtros.boletaCorte);
                }

                var qTotal = $"SELECT COUNT(1) FROM vActivos_TrasladosHistorico {where}";
                resp.Result.total = connection.ExecuteScalar<int>(qTotal, p);

                // Orden
                var sortField = string.IsNullOrWhiteSpace(filtros.sortField)
                    ? "cod_traslado"
                    : filtros.sortField;

                var sortOrder = (filtros.sortOrder == 0 ? "DESC" : "ASC");

                var offset = (filtros.pagina <= 1 ? 0 : (filtros.pagina - 1) * filtros.paginacion);

                var qDatos = $@"
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
{where}
ORDER BY {sortField} {sortOrder}
OFFSET {offset} ROWS
FETCH NEXT {filtros.paginacion} ROWS ONLY;";

                resp.Result.lista = connection.Query<ActivosReasignacionesBoletaHistorialItem>(
                    qDatos, p).ToList();
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
                resp.Result.total = 0;
                resp.Result.lista = null;
            }

            return resp;
        }
        /// <summary>
        /// Obtener una boleta específica de reasignación por código de traslado.
        /// <param name="CodEmpresa"></param>"
        /// <param name="cod_traslado"></param>"
        /// </summary>
        /// <returns></returns>
        public ErrorDto<ActivosReasignacionesBoleta> Activos_Reasignacion_Obtener(int CodEmpresa,string cod_traslado)
        {
            
            var resp = new ErrorDto<ActivosReasignacionesBoleta>
            {
                Code = 0,
                Description = "Ok",
                Result = null
            };

            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);

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
                    CONVERT(varchar(19), cerrado_fecha, 120)  AS cerrado_fecha,
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

                resp.Result = connection.QueryFirstOrDefault<ActivosReasignacionesBoleta>(
                    sql, new { cod_traslado });

                if (resp.Result == null)
                {
                    resp.Code = -2;
                    resp.Description = "Boleta no encontrada.";
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
                resp.Result = null;
            }

            return resp;
        }

        /// <summary>
        /// Obtener lista completa de boletas de reasignación (historial),
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDto<List<ActivosReasignacionesBoletaHistorialItem>> Activos_Reasignacion_Boletas_Export(int CodEmpresa,ActivosReasignacionesBoletasFiltros filtros)
        {
            
            var resp = new ErrorDto<List<ActivosReasignacionesBoletaHistorialItem>>
            {
                Code = 0,
                Description = "Ok",
                Result = new List<ActivosReasignacionesBoletaHistorialItem>()
            };

            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);
                var where = _where11;
                var p = new DynamicParameters();

                if (filtros.todosActivos == 0 && !string.IsNullOrWhiteSpace(filtros.numPlaca))
                {
                    where += " AND num_placa LIKE @numPlaca ";
                    p.Add("@numPlaca", "%" + filtros.numPlaca + "%");
                }

                if (!string.IsNullOrWhiteSpace(filtros.fechaInicio))
                {
                    where += " AND Fecha_Aplicacion >= @FechaInicioDesde ";
                    p.Add("@FechaInicioDesde", DateTime.ParseExact(filtros.fechaInicio, _formatoFecha, System.Globalization.CultureInfo.InvariantCulture).Date);
                }

                if (!string.IsNullOrWhiteSpace(filtros.fechaCorte))
                {
                    where += " AND Fecha_Aplicacion <= @FechaCorteHasta ";
                    var corte = DateTime.ParseExact(
                        filtros.fechaCorte,
                        _formatoFecha,
                        System.Globalization.CultureInfo.InvariantCulture
                    ).Date.AddDays(1).AddSeconds(-1);
                    p.Add("@FechaCorteHasta", corte);
                }

                if (!string.IsNullOrWhiteSpace(filtros.boletaInicio))
                {
                    where += " AND cod_traslado >= @BoletaInicio ";
                    p.Add("@BoletaInicio", filtros.boletaInicio);
                }

                if (!string.IsNullOrWhiteSpace(filtros.boletaCorte))
                {
                    where += " AND cod_traslado <= @BoletaCorte ";
                    p.Add("@BoletaCorte", filtros.boletaCorte);
                }

                var sortField = string.IsNullOrWhiteSpace(filtros.sortField)
                    ? "cod_traslado"
                    : filtros.sortField;

                var sortOrder = (filtros.sortOrder == 0 ? "DESC" : "ASC");

                var qDatos = $@"
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
                {where}
                ORDER BY {sortField} {sortOrder};";

                resp.Result = connection
                    .Query<ActivosReasignacionesBoletaHistorialItem>(qDatos, p)
                    .ToList();
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
                resp.Result = null;
            }

            return resp;
        }


        // /// <summary>
        // /// Generar emisión de boletas de reasignación en lote (PDF combinado).
        // /// <param name="codEmpresa"></param>
        // /// <param name="request"></param>
        // /// </summary>
        // /// <returns></returns>
        // public ErrorDto<object> Activos_Reasignacion_Boletas_Lote(int codEmpresa,ActivosReasignacionesBoletasLoteRequest request)
        // {
        //     var response = new ErrorDto<object> { Code = 0 };

        //     if (request.Boletas == null || request.Boletas.Count == 0)
        //     {
        //         response.Code = -1;
        //         response.Description = "No se recibieron boletas para generar el reporte.";
        //         return response;
        //     }

        //     try
        //     {
        //         var pdfs = new List<byte[]>();

        //         foreach (var boleta in request.Boletas.Distinct())
        //         {
        //             if (string.IsNullOrWhiteSpace(boleta))
        //                 continue;

        //             var parametros = new
        //             {
        //                 filtros =
        //                     $" WHERE ACTIVOS_TRASLADOS.COD_TRASLADO = '{boleta}'",
        //                 Empresa = (string?)null,
        //                 fxUsuario = request.Usuario,
        //                 fxSubTitulo = "TRASLADO DE ACTIVOS Y CAMBIO DE RESPONSABLES"
        //             };

        //             var reporteData = new FrmReporteGlobal
        //             {
        //                 codEmpresa = codEmpresa,
        //                 parametros = JsonConvert.SerializeObject(parametros),
        //                 nombreReporte = "Activos_BoletaTraslado",
        //                 usuario = request.Usuario,
        //                 cod_reporte = "P",
        //                 folder = "Activos"
        //             };

        //             var actionResult = _mReporting.ReporteRDLC_v2(reporteData);

        //             // Si viene un ObjectResult, interpretamos que hay ErrorDto adentro
        //             if (actionResult is ObjectResult objectResult)
        //             {
        //                 var res = objectResult.Value;
        //                 var jres = System.Text.Json.JsonSerializer.Serialize(res);
        //                 var err = System.Text.Json.JsonSerializer.Deserialize<ErrorDto>(jres);

        //                 response.Code = -1;
        //                 response.Description =
        //                     err?.Description ?? $"Error al generar boleta {boleta}.";
        //                 return response;
        //             }

        //             var fileResult = actionResult as FileContentResult;

        //             if (fileResult?.FileContents == null || fileResult.FileContents.Length == 0)
        //             {
        //                 response.Code = -1;
        //                 response.Description =
        //                     $"Ocurrió un error al generar la boleta, contenido nulo/vacío para boleta {boleta}.";
        //                 return response;
        //             }

        //             pdfs.Add(fileResult.FileContents);
        //         }

        //         if (!pdfs.Any())
        //         {
        //             response.Code = -1;
        //             response.Description = "No se generaron boletas para los códigos indicados.";
        //             return response;
        //         }

        //         // Combinar los bytes de todos los PDFs en uno solo
        //         var combinadoBytes = MProGrXAuxiliarDB.CombinarBytesPdfSharp(pdfs.ToArray());

        //         var fileCombinado = new FileContentResult(combinadoBytes, "application/pdf")
        //         {
        //             FileDownloadName = "Activos_Reasignacion_Boletas.pdf"
        //         };

        //         // Devolver el FileContentResult serializado (igual que en Personas/Tesorería)
        //         response.Result = JsonConvert.SerializeObject(fileCombinado, Formatting.Indented);
        //         return response;
        //     }
        //     catch (Exception ex)
        //     {
        //         response.Code = -1;
        //         response.Description = ex.Message;
        //         response.Result = null;
        //         return response;
        //     }
        // }


    }
}
