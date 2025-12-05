using Dapper;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Activos_Fijos;
using Galileo.Models.Security;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using System.Data;

namespace Galileo.DataBaseTier.ProGrX_Activos_Fijos
{
    public class FrmActivosCambioVidaUtilDb
    {
        private readonly int vModulo = 36;
        private readonly MSecurityMainDb _security_MainDB;
        private readonly PortalDB _portalDB;

        public FrmActivosCambioVidaUtilDb(IConfiguration config)
        {
            _security_MainDB = new MSecurityMainDb(config);
            _portalDB = new PortalDB(config);
        }

        /// <summary>
        /// Obtiene una lista de activos con paginacion y filtros.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDto<ActivoLiteLista> Activos_CambioVU_ActivoLista_Obtener(int CodEmpresa, string filtros)
        {
            var res = new ErrorDto<ActivoLiteLista>
            {
                Code = 0,
                Description = "Ok",
                Result = new ActivoLiteLista { total = 0, lista = new List<ActivoLite>() }
            };

            ActivosCambioVUFiltros? vfiltro;
            try
            {
                vfiltro = JsonConvert.DeserializeObject<ActivosCambioVUFiltros>(filtros ?? "{}")
                          ?? new ActivosCambioVUFiltros();
            }
            catch
            {
                vfiltro = new ActivosCambioVUFiltros();
            }

            try
            {
                using var cn = _portalDB.CreateConnection(CodEmpresa);

                // Normalizamos filtros a parámetros (null si vienen vacíos)
                var p = new DynamicParameters();

                string? g = string.IsNullOrWhiteSpace(vfiltro.filtro)
                    ? null
                    : $"%{vfiltro.filtro.Trim().ToUpper()}%";

                string? placa = string.IsNullOrWhiteSpace(vfiltro.placa)
                    ? null
                    : $"%{vfiltro.placa.Trim().ToUpper()}%";

                string? alterna = string.IsNullOrWhiteSpace(vfiltro.alterna)
                    ? null
                    : $"%{vfiltro.alterna.Trim().ToUpper()}%";

                string? nombre = string.IsNullOrWhiteSpace(vfiltro.nombre)
                    ? null
                    : $"%{vfiltro.nombre.Trim().ToUpper()}%";

                p.Add("@g", g, DbType.String);
                p.Add("@placa", placa, DbType.String);
                p.Add("@alterna", alterna, DbType.String);
                p.Add("@nombre", nombre, DbType.String);

                // Sort seguro: usamos CASE en el ORDER BY, sin concatenar nombres de columnas
                string sortField = (vfiltro.sortField ?? "placa").Trim().ToLowerInvariant();
                int sortOrder = vfiltro.sortOrder ?? 0; // 0 = ASC, 1 = DESC
                p.Add("@sortField", sortField, DbType.String);
                p.Add("@sortOrder", sortOrder, DbType.Int32);

                // Paginación
                bool usarPaginacion = vfiltro.pagina.HasValue && vfiltro.paginacion.HasValue;
                if (usarPaginacion)
                {
                    p.Add("@offset", vfiltro.pagina.GetValueOrDefault(), DbType.Int32);
                    p.Add("@rows", vfiltro.paginacion.GetValueOrDefault(), DbType.Int32);
                }

                const string selectBase = @"
                    SELECT
                        A.NUM_PLACA     AS numPlaca,
                        A.PLACA_ALTERNA AS placaAlterna,
                        A.NOMBRE        AS nombre
                    FROM ACTIVOS_PRINCIPAL A
                ";

                // WHERE totalmente estático. La activación depende de que el parámetro sea NULL o no.
                const string whereSql = @"
                    WHERE 1 = 1
                      AND (
                            @g IS NULL
                            OR UPPER(A.NUM_PLACA)     LIKE @g
                            OR UPPER(A.PLACA_ALTERNA) LIKE @g
                            OR UPPER(A.NOMBRE)        LIKE @g
                          )
                      AND (
                            @placa IS NULL
                            OR UPPER(A.NUM_PLACA) LIKE @placa
                          )
                      AND (
                            @alterna IS NULL
                            OR UPPER(A.PLACA_ALTERNA) LIKE @alterna
                          )
                      AND (
                            @nombre IS NULL
                            OR UPPER(A.NOMBRE) LIKE @nombre
                          )
                ";

                // ORDER BY estático, usando CASE sobre parámetros @sortField y @sortOrder
                const string orderSql = @"
                    ORDER BY
                        CASE 
                            WHEN @sortOrder = 0 AND @sortField = 'placa'   THEN A.NUM_PLACA
                            WHEN @sortOrder = 0 AND @sortField = 'alterna' THEN A.PLACA_ALTERNA
                            WHEN @sortOrder = 0 AND @sortField = 'nombre'  THEN A.NOMBRE
                        END ASC,
                        CASE 
                            WHEN @sortOrder = 1 AND @sortField = 'placa'   THEN A.NUM_PLACA
                            WHEN @sortOrder = 1 AND @sortField = 'alterna' THEN A.PLACA_ALTERNA
                            WHEN @sortOrder = 1 AND @sortField = 'nombre'  THEN A.NOMBRE
                        END DESC
                ";

                const string pagingSql = @"
                    OFFSET @offset ROWS FETCH NEXT @rows ROWS ONLY
                ";

                // COUNT: mismo WHERE, sin ORDER/PAGING
                string countSql = "SELECT COUNT(1) FROM ACTIVOS_PRINCIPAL A " + whereSql;
                res.Result.total = cn.ExecuteScalar<int>(countSql, p, commandTimeout: 60);

                // DATA: SELECT + WHERE + ORDER [+ PAGING]
                string dataSql = selectBase + whereSql + orderSql;
                if (usarPaginacion)
                {
                    dataSql += pagingSql;
                }

                res.Result.lista = cn.Query<ActivoLite>(dataSql, p, commandTimeout: 60).ToList();

                return res;
            }
            catch (Exception ex)
            {
                return new ErrorDto<ActivoLiteLista>
                {
                    Code = -1,
                    Description = ex.Message,
                    Result = new ActivoLiteLista { total = 0, lista = null }
                };
            }
        }

        /// <summary>
        /// Consulta un activo por número de placa desde ACTIVOS_PRINCIPAL y devuelve datos básicos para la vista.
        /// </summary>
        public ErrorDto<ActivoBuscarResponse> Activos_CambioVU_Activo_Obtener(int CodEmpresa, string numPlaca)
        {
            var result = new ErrorDto<ActivoBuscarResponse>
            {
                Code = 0,
                Description = "Ok",
                Result = new ActivoBuscarResponse()
            };

            try
            {
                using var cn = _portalDB.CreateConnection(CodEmpresa);

                const string sql = @"
                    SELECT  A.NUM_PLACA        AS numPlaca,
                            A.PLACA_ALTERNA    AS placaAlterna,
                            A.NOMBRE           AS nombre,
                            A.TIPO_ACTIVO      AS tipoActivo,
                            T.DESCRIPCION      AS tipoActivoDesc,
                            A.VIDA_UTIL        AS vidaUtil,
                            A.VIDA_UTIL_EN     AS vidaUtilEn,
                            A.MET_DEPRECIACION AS metDepreciacion
                    FROM    ACTIVOS_PRINCIPAL A
                    JOIN    ACTIVOS_TIPO_ACTIVO T ON A.TIPO_ACTIVO = T.TIPO_ACTIVO
                    WHERE   A.NUM_PLACA = @numPlaca;";

                var act = cn.QueryFirstOrDefault<ActivoData>(sql, new { numPlaca });

                if (act != null)
                {
                    var cod = (act.metDepreciacion ?? string.Empty).Trim().ToUpperInvariant();
                    var metodoTxt = cod switch
                    {
                        "L"  => "Línea Recta",
                        "N"  => "No Deprecia",
                        "SD" => "Suma Dígitos",
                        "DD" => "Doblemente Decreciente",
                        "UP" => "Unidades Producidas",
                        _    => cod
                    };

                    act.resumenActual = $"{metodoTxt}, VU: {act.vidaUtil} {(act.vidaUtilEn == "M" ? "meses" : "años")}";
                }

                result.Result.activo = act;
                if (act == null)
                {
                    result.Description = "No encontrado";
                }

            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = null;
            }

            return result;
        }

        /// <summary>
        /// Lista métodos de depreciación disponibles (DISTINCT desde ACTIVOS_PRINCIPAL) y devuelve item/descripcion.
        /// </summary>
        public ErrorDto<List<DropDownListaGenericaModel>> Activos_CambioVU_MetodosDepreciacion_Obtener(int CodEmpresa)
        {
            return new ErrorDto<List<DropDownListaGenericaModel>>
            {
                Code = 0,
                Description = "Ok",
                Result = new List<DropDownListaGenericaModel>
                {
                    new DropDownListaGenericaModel { item = "L",  descripcion = "Línea Recta" },
                    new DropDownListaGenericaModel { item = "N",  descripcion = "No Deprecia" },
                    new DropDownListaGenericaModel { item = "S",  descripcion = "Suma Dígitos (Años)" },
                    new DropDownListaGenericaModel { item = "D",  descripcion = "Doble Decreciente" },
                    new DropDownListaGenericaModel { item = "U",  descripcion = "Unidades Producidas" }
                }
            };
        }

        /// <summary>
        /// Aplica cambio de vida útil (spActivos_Cambio_Vida_Util) y sincroniza unidad/método en ACTIVOS_PRINCIPAL; registra bitácora.
        /// </summary>
        public ErrorDto<CambioVidaUtilAplicarResponse> Activos_CambioVU_Aplicar(int CodEmpresa, string usuario, CambioVidaUtilAplicarRequest dto)
        {
            var res = new ErrorDto<CambioVidaUtilAplicarResponse>
            {
                Code = 0,
                Description = "Ok",
                Result = null
            };

            try
            {
                if (string.IsNullOrWhiteSpace(dto.numPlaca))
                    return new ErrorDto<CambioVidaUtilAplicarResponse> { Code = -1, Description = "numPlaca es requerido" };

                if (dto.nuevaVidaUtil <= 0)
                    return new ErrorDto<CambioVidaUtilAplicarResponse> { Code = -1, Description = "nuevaVidaUtil debe ser > 0" };

                using var cn = _portalDB.CreateConnection(CodEmpresa);

                var p = new DynamicParameters();
                p.Add("@Num_Placa", dto.numPlaca);
                p.Add("@VidaUtil", dto.nuevaVidaUtil);
                p.Add("@Usuario", usuario);
                p.Add("@Fecha", (DateTime?)null);
                p.Add("@Notas", dto.notas ?? "");

                cn.Execute(
                    "spActivos_Cambio_Vida_Util",
                    p,
                    commandType: CommandType.StoredProcedure,
                    commandTimeout: 90
                );

                _security_MainDB.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = usuario,
                    Modulo = vModulo,
                    Movimiento = "Modifica - WEB",
                    DetalleMovimiento = $"Cambio Vida Útil (Placa: {dto.numPlaca}) -> VU:{dto.nuevaVidaUtil} {(dto.unidad ?? "").Trim()}"
                });

                const string sql = @"
                SELECT  A.NUM_PLACA        AS numPlaca,
                        A.PLACA_ALTERNA    AS placaAlterna,
                        A.NOMBRE           AS nombre,
                        A.TIPO_ACTIVO      AS tipoActivo,
                        T.DESCRIPCION      AS tipoActivoDesc,
                        A.VIDA_UTIL        AS vidaUtil,
                        A.VIDA_UTIL_EN     AS vidaUtilEn,
                        A.MET_DEPRECIACION AS metDepreciacion
                FROM    ACTIVOS_PRINCIPAL A
                JOIN    ACTIVOS_TIPO_ACTIVO T ON A.TIPO_ACTIVO = T.TIPO_ACTIVO
                WHERE   A.NUM_PLACA = @numPlaca;";

                var act = cn.QueryFirstOrDefault<ActivoData>(sql, new { numPlaca = dto.numPlaca });

                if (act != null)
                {
                    var cod = (act.metDepreciacion ?? string.Empty).Trim().ToUpperInvariant();
                    var metodoTxt = cod switch
                    {
                        "L"  => "Línea Recta",
                        "N"  => "No Deprecia",
                        "SD" => "Suma Dígitos",
                        "DD" => "Doble Decreciente",
                        "UP" => "Unidades Producidas",
                        _    => cod
                    };

                    act.resumenActual = $"{metodoTxt}, VU: {act.vidaUtil} {(act.vidaUtilEn == "M" ? "meses" : "años")}";

                    res.Result = new CambioVidaUtilAplicarResponse
                    {
                        numPlaca = act.numPlaca,
                        placaAlterna = act.placaAlterna,
                        nombre = act.nombre,
                        tipoActivo = act.tipoActivo,
                        tipoActivoDesc = act.tipoActivoDesc,
                        vidaUtil = act.vidaUtil,
                        vidaUtilEn = act.vidaUtilEn,
                        metDepreciacion = act.metDepreciacion,
                        resumenActual = act.resumenActual,
                        mensaje = "Ok"
                    };
                }
                else
                {
                    res.Result = new CambioVidaUtilAplicarResponse
                    {
                        numPlaca = dto.numPlaca,
                        mensaje = "Ok"
                    };
                }
            }
            catch (SqlException ex)
            {
                return new ErrorDto<CambioVidaUtilAplicarResponse>
                {
                    Code = -1,
                    Description = ex.Message,
                    Result = null
                };
            }
            catch (Exception ex)
            {
                return new ErrorDto<CambioVidaUtilAplicarResponse>
                {
                    Code = -1,
                    Description = ex.Message,
                    Result = null
                };
            }

            return res;
        }

    }
}