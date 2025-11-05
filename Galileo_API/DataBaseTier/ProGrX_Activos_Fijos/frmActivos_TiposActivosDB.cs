using Dapper;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using PgxAPI.Models;
using PgxAPI.Models.ERROR;
using static PgxAPI.Models.ProGrX_Activos_Fijos.frmActivos_TiposActivosModels;


namespace PgxAPI.DataBaseTier.ProGrX_Activos_Fijos
{
    public class frmActivos_TiposActivoDB
    {
        private readonly IConfiguration? _config;
        private readonly int vModulo = 36;
        private readonly mSecurityMainDb _Security_MainDB;

        public frmActivos_TiposActivoDB(IConfiguration? config)
        {
            _config = config;
            _Security_MainDB = new mSecurityMainDb(_config);
        }
        /// <summary>
        /// Obtener lista de tipos de activo.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDto<ActivosTiposActivosLista> Activos_TiposActivosLista_Obtener(int CodEmpresa, string filtros)
        {
            var vfiltro = JsonConvert.DeserializeObject<ActivosTiposActivosFiltros>(filtros);
            var connStr = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            var resp = new ErrorDto<ActivosTiposActivosLista>
            {
                Code = 0,
                Description = "",
                Result = new ActivosTiposActivosLista()
            };

            try
            {
                string where = "", pagina = "", paginacion = "";

                if (vfiltro != null)
                {
                    if (!string.IsNullOrWhiteSpace(vfiltro.filtro))
                    {
                        where = " WHERE TIPO_ACTIVO LIKE @like OR DESCRIPCION LIKE @like ";
                    }
                    if (vfiltro.pagina != null)
                    {
                        pagina = " OFFSET " + vfiltro.pagina + " ROWS ";
                        paginacion = " FETCH NEXT " + (vfiltro.paginacion ?? 10) + " ROWS ONLY ";
                    }
                }

                using var cn = new SqlConnection(connStr);
                {
                    var qTotal = $"SELECT COUNT(*) FROM dbo.ACTIVOS_TIPO_ACTIVO {where}";
                    resp.Result.total = cn.QueryFirstOrDefault<int>(qTotal, new { like = "%" + (vfiltro?.filtro ?? "").Trim() + "%" });

                    var qDatos = $@"
                    SELECT
                    TIPO_ACTIVO AS tipo_activo,
                    ISNULL(DESCRIPCION,'') AS descripcion
                    FROM dbo.ACTIVOS_TIPO_ACTIVO
                    {where}
                    ORDER BY TIPO_ACTIVO
                    {pagina} {paginacion}";

                    resp.Result.lista = cn.Query<ActivosTiposActivosData>(
                        qDatos,
                        new { like = "%" + (vfiltro?.filtro ?? "").Trim() + "%" }
                    ).ToList();
                }
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
        /// Verifica si un tipo de activo ya existe en la base de datos.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="tipo_activo"></param>
        /// <returns></returns>
        public ErrorDto Activos_TiposActivosExiste_Obtener(int CodEmpresa, string tipo_activo)
        {
            var connStr = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var resp = new ErrorDto { Code = 0, Description = "" };

            try
            {
                using var cn = new SqlConnection(connStr);
                const string q = @"
                SELECT COUNT(1)
                FROM dbo.ACTIVOS_TIPO_ACTIVO
                WHERE UPPER(TIPO_ACTIVO) = @cod";
                int n = cn.QueryFirstOrDefault<int>(q, new { cod = (tipo_activo ?? "").ToUpper() });

                (resp.Code, resp.Description) = (n == 0)
                    ? (0, "TIPO_ACTIVO: Libre!")
                    : (-2, "TIPO_ACTIVO: Ocupado!");
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }
            return resp;
        }
        /// <summary>
        /// Obtiene los detalles de un tipo de activo específico.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="tipo_activo"></param>
        /// <returns></returns>
        public ErrorDto<ActivosTiposActivosData> Activos_TiposActivos_Obtener(int CodEmpresa, string tipo_activo)
        {
            var connStr = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var resp = new ErrorDto<ActivosTiposActivosData> { Code = 0, Description = "" };

            try
            {
                using var cn = new SqlConnection(connStr);
                var q = @"
            SELECT
                a.TIPO_ACTIVO                                            AS tipo_activo,
                ISNULL(a.DESCRIPCION,'')                                 AS descripcion,
                ISNULL(a.MET_DEPRECIACION,'')                            AS met_depreciacion,
                ISNULL(a.TIPO_VIDA_UTIL,'')                              AS tipo_vida_util,
                ISNULL(CONVERT(varchar(10), a.VIDA_UTIL), '')            AS vida_util,
                ISNULL(a.ASIENTO_GENERA,'')                              AS asiento_genera,

                ISNULL(a.COD_CUENTA_ACTIVO,'')                           AS cod_cuenta_actvo,
                ISNULL(a.COD_CUENTA_GASTOS,'')                           AS cod_cuenta_gastos,
                ISNULL(a.COD_CUENTA_DEPACUM,'')                          AS cod_cuenta_depacum,
                ISNULL(a.COD_CUENTA_TRANSITORIA,'')                      AS cod_cuenta_transitoria,

                ISNULL(a.REGISTRO_USUARIO,'')                            AS registro_usuario,
                ISNULL(CONVERT(varchar(19), a.REGISTRO_FECHA,120),'')    AS registro_fecha,
                ISNULL(a.MODIFICA_USUARIO,'')                            AS modifica_usuario,
                ISNULL(CONVERT(varchar(19), a.MODIFICA_FECHA,120),'')    AS modifica_fecha,

                ISNULL(ta.DESCRIPCION,'')                                AS tipo_asiento_desc,

                ISNULL(ca.COD_CUENTA_MASK,'')                            AS cod_cuenta_activo_mask,
                ISNULL(ca.DESCRIPCION,'')                                AS cod_cuenta_activo_desc,

                ISNULL(cg.COD_CUENTA_MASK,'')                            AS cod_cuenta_gastos_mask,
                ISNULL(cg.DESCRIPCION,'')                                AS cod_cuenta_gastos_desc,

                ISNULL(cd.COD_CUENTA_MASK,'')                            AS cod_cuenta_depacum_mask,
                ISNULL(cd.DESCRIPCION,'')                                AS cod_cuenta_depacum_desc,

                ISNULL(ct.COD_CUENTA_MASK,'')                            AS cod_cuenta_transitoria_mask,
                ISNULL(ct.DESCRIPCION,'')                                AS cod_cuenta_transitoria_desc
            FROM dbo.ACTIVOS_TIPO_ACTIVO a
            LEFT JOIN dbo.CNTX_TIPOS_ASIENTOS ta ON ta.TIPO_ASIENTO = a.ASIENTO_GENERA
            LEFT JOIN dbo.vCNTX_CUENTAS_LOCAL ca ON ca.COD_CUENTA   = a.COD_CUENTA_ACTIVO
            LEFT JOIN dbo.vCNTX_CUENTAS_LOCAL cg ON cg.COD_CUENTA   = a.COD_CUENTA_GASTOS
            LEFT JOIN dbo.vCNTX_CUENTAS_LOCAL cd ON cd.COD_CUENTA   = a.COD_CUENTA_DEPACUM
            LEFT JOIN dbo.vCNTX_CUENTAS_LOCAL ct ON ct.COD_CUENTA   = a.COD_CUENTA_TRANSITORIA
            WHERE a.TIPO_ACTIVO = @cod;";

                resp.Result = cn.QueryFirstOrDefault<ActivosTiposActivosData>(
                    q, new { cod = (tipo_activo ?? string.Empty).Trim().ToUpper() });

                if (resp.Result == null)
                {
                    resp.Code = -2;
                    resp.Description = "Tipo de Activo no encontrado.";
                }
                else
                {
                    resp.Description = "Ok";
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = "Error al obtener el Tipo de Activo: " + ex.Message;
                resp.Result = null;
            }
            return resp;
        }
        /// <summary>
        /// Navegación (scroll) entre tipos de activo.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="scroll"></param>
        /// <param name="tipo_activo"></param>
        /// <returns></returns>
        public ErrorDto<ActivosTiposActivosData> Activos_TiposActivos_Scroll(int CodEmpresa, int scroll, string? tipo_activo)
        {
            var connStr = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var resp = new ErrorDto<ActivosTiposActivosData> { Code = 0, Description = "" };

            try
            {
                var cod = (tipo_activo ?? string.Empty).Trim().ToUpper();

                // Si no hay código de referencia, devolvemos el primer/último registro según scroll
                string where = string.IsNullOrEmpty(cod)
                    ? ""
                    : (scroll == 1 ? " WHERE a.TIPO_ACTIVO > @cod " : " WHERE a.TIPO_ACTIVO < @cod ");

                string order = (scroll == 1)
                    ? " ORDER BY a.TIPO_ACTIVO ASC "
                    : " ORDER BY a.TIPO_ACTIVO DESC ";

                using var cn = new SqlConnection(connStr);
                var q = $@"
            SELECT TOP 1
                a.TIPO_ACTIVO                                            AS tipo_activo,
                ISNULL(a.DESCRIPCION,'')                                 AS descripcion,
                ISNULL(a.MET_DEPRECIACION,'')                            AS met_depreciacion,
                ISNULL(a.TIPO_VIDA_UTIL,'')                              AS tipo_vida_util,
                ISNULL(CONVERT(varchar(10), a.VIDA_UTIL),'')             AS vida_util,
                ISNULL(a.ASIENTO_GENERA,'')                              AS asiento_genera,

                ISNULL(a.COD_CUENTA_ACTIVO,'')                           AS cod_cuenta_actvo,
                ISNULL(a.COD_CUENTA_GASTOS,'')                           AS cod_cuenta_gastos,
                ISNULL(a.COD_CUENTA_DEPACUM,'')                          AS cod_cuenta_depacum,
                ISNULL(a.COD_CUENTA_TRANSITORIA,'')                      AS cod_cuenta_transitoria,

                ISNULL(a.REGISTRO_USUARIO,'')                            AS registro_usuario,
                ISNULL(CONVERT(varchar(19), a.REGISTRO_FECHA,120),'')    AS registro_fecha,
                ISNULL(a.MODIFICA_USUARIO,'')                            AS modifica_usuario,
                ISNULL(CONVERT(varchar(19), a.MODIFICA_FECHA,120),'')    AS modifica_fecha,

                ISNULL(ta.DESCRIPCION,'')                                AS tipo_asiento_desc,

                ISNULL(ca.COD_CUENTA_MASK,'')                            AS cod_cuenta_activo_mask,
                ISNULL(ca.DESCRIPCION,'')                                AS cod_cuenta_activo_desc,

                ISNULL(cg.COD_CUENTA_MASK,'')                            AS cod_cuenta_gastos_mask,
                ISNULL(cg.DESCRIPCION,'')                                AS cod_cuenta_gastos_desc,

                ISNULL(cd.COD_CUENTA_MASK,'')                            AS cod_cuenta_depacum_mask,
                ISNULL(cd.DESCRIPCION,'')                                AS cod_cuenta_depacum_desc,

                ISNULL(ct.COD_CUENTA_MASK,'')                            AS cod_cuenta_transitoria_mask,
                ISNULL(ct.DESCRIPCION,'')                                AS cod_cuenta_transitoria_desc
            FROM dbo.ACTIVOS_TIPO_ACTIVO a
            LEFT JOIN dbo.CNTX_TIPOS_ASIENTOS ta ON ta.TIPO_ASIENTO = a.ASIENTO_GENERA
            LEFT JOIN dbo.vCNTX_CUENTAS_LOCAL ca ON ca.COD_CUENTA   = a.COD_CUENTA_ACTIVO
            LEFT JOIN dbo.vCNTX_CUENTAS_LOCAL cg ON cg.COD_CUENTA   = a.COD_CUENTA_GASTOS
            LEFT JOIN dbo.vCNTX_CUENTAS_LOCAL cd ON cd.COD_CUENTA   = a.COD_CUENTA_DEPACUM
            LEFT JOIN dbo.vCNTX_CUENTAS_LOCAL ct ON ct.COD_CUENTA   = a.COD_CUENTA_TRANSITORIA
            {where}
            {order};";

                resp.Result = cn.QueryFirstOrDefault<ActivosTiposActivosData>(q, new { cod });

                if (resp.Result == null)
                {
                    resp.Code = -2;
                    resp.Description = "No se encontraron más resultados.";
                }
                else
                {
                    resp.Description = "Ok";
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = "Error al desplazar el Tipo de Activo: " + ex.Message;
                resp.Result = null;
            }
            return resp;
        }
        /// <summary>
        /// Guarda (inserta o actualiza) un tipo de activo en la base de datos.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public ErrorDto Activos_TiposActivos_Guardar(int CodEmpresa, ActivosTiposActivosData data)
        {
            var connStr = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var resp = new ErrorDto { Code = 0, Description = "" };

            try
            {
                if (data == null)
                    return new ErrorDto { Code = -1, Description = "Datos no proporcionados." };

                var errores = new List<string>();
                if (string.IsNullOrWhiteSpace(data.tipo_activo)) errores.Add("Debe indicar el Tipo de Activo.");
                if (string.IsNullOrWhiteSpace(data.descripcion)) errores.Add("Debe indicar la descripción.");
                if (!string.IsNullOrWhiteSpace(data.tipo_vida_util))
                {
                    var t = data.tipo_vida_util.Trim().ToUpper();
                    if (t != "A" && t != "M") errores.Add("Tipo de Vida Útil debe ser 'A' (Años) o 'M' (Meses).");
                }
                if (!string.IsNullOrWhiteSpace(data.vida_util) && !int.TryParse(data.vida_util, out _))
                    errores.Add("Vida Útil debe ser un número entero.");

                if (errores.Count > 0)
                    return new ErrorDto { Code = -1, Description = string.Join(" | ", errores) };

                using var cn = new SqlConnection(connStr);

                const string qExiste = "SELECT COUNT(1) FROM dbo.ACTIVOS_TIPO_ACTIVO WHERE TIPO_ACTIVO = @cod";
                int existe = cn.QueryFirstOrDefault<int>(qExiste, new { cod = data.tipo_activo.ToUpper() });

                if (data.isNew)
                {
                    if (existe > 0)
                        return new ErrorDto { Code = -2, Description = $"El Tipo de Activo {data.tipo_activo.ToUpper()} ya existe." };
                    return Activos_TiposActivos_Insertar(CodEmpresa, data);
                }
                else
                {
                    if (existe == 0)
                        return new ErrorDto { Code = -2, Description = $"El Tipo de Activo {data.tipo_activo.ToUpper()} no existe." };
                    return Activos_TiposActivos_Actualizar(CodEmpresa, data);
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }
            return resp;
        }
        /// <summary>
        /// Inserta un nuevo tipo de activo en la base de datos.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        private ErrorDto Activos_TiposActivos_Insertar(int CodEmpresa, ActivosTiposActivosData data)
        {
            var connStr = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var resp = new ErrorDto { Code = 0, Description = "" };

            try
            {
                using var cn = new SqlConnection(connStr);
                var q = @"
            INSERT INTO dbo.ACTIVOS_TIPO_ACTIVO
                (TIPO_ACTIVO, DESCRIPCION, MET_DEPRECIACION, TIPO_VIDA_UTIL, VIDA_UTIL,
                 ASIENTO_GENERA, COD_CUENTA_ACTIVO, COD_CUENTA_DEPACUM, COD_CUENTA_GASTOS, COD_CUENTA_TRANSITORIA,
                 REGISTRO_USUARIO, REGISTRO_FECHA, MODIFICA_USUARIO, MODIFICA_FECHA)
            VALUES
                (@cod, @descripcion, @met, @tvu, @vu,
                 @tasiento, @cta_activo, @cta_depacum, @cta_gastos, @cta_trans,
                 @reg_usuario, SYSDATETIME(), NULL, NULL);";

                cn.Execute(q, new
                {
                    cod = data.tipo_activo.ToUpper(),
                    descripcion = data.descripcion?.ToUpper(),
                    met = (data.met_depreciacion ?? "").ToUpper(),
                    tvu = string.IsNullOrWhiteSpace(data.tipo_vida_util) ? null : data.tipo_vida_util.ToUpper(),
                    vu = string.IsNullOrWhiteSpace(data.vida_util) ? null : data.vida_util,
                    tasiento = string.IsNullOrWhiteSpace(data.asiento_genera) ? null : data.asiento_genera.ToUpper(),
                    cta_activo = string.IsNullOrWhiteSpace(data.cod_cuenta_actvo) ? null : data.cod_cuenta_actvo.ToUpper(),
                    cta_depacum = string.IsNullOrWhiteSpace(data.cod_cuenta_depacum) ? null : data.cod_cuenta_depacum.ToUpper(),
                    cta_gastos = string.IsNullOrWhiteSpace(data.cod_cuenta_gastos) ? null : data.cod_cuenta_gastos.ToUpper(),
                    cta_trans = string.IsNullOrWhiteSpace(data.cod_cuenta_transitoria) ? null : data.cod_cuenta_transitoria.ToUpper(),
                    reg_usuario = string.IsNullOrWhiteSpace(data.registro_usuario) ? null : data.registro_usuario
                });

                _Security_MainDB.Bitacora(new BitacoraInsertarDTO
                {
                    EmpresaId = CodEmpresa,
                    Usuario = string.IsNullOrWhiteSpace(data.registro_usuario) ? "" : data.registro_usuario,
                    DetalleMovimiento = $"Tipo Activo: {data.tipo_activo} - {data.descripcion}",
                    Movimiento = "Registra - WEB",
                    Modulo = vModulo
                });

                resp.Description = "Tipo de Activo ingresado satisfactoriamente.";
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }
            return resp;
        }

        /// <summary>
        /// Actualiza los detalles de un tipo de activo existente en la base de datos.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        private ErrorDto Activos_TiposActivos_Actualizar(int CodEmpresa, ActivosTiposActivosData data)
        {
            var connStr = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var resp = new ErrorDto { Code = 0, Description = "" };

            try
            {
                using var cn = new SqlConnection(connStr);
                var q = @"
            UPDATE dbo.ACTIVOS_TIPO_ACTIVO
               SET DESCRIPCION        = @descripcion,
                   MET_DEPRECIACION   = @met,
                   TIPO_VIDA_UTIL     = @tvu,
                   VIDA_UTIL          = @vu,
                   ASIENTO_GENERA     = @tasiento,
                   COD_CUENTA_ACTIVO  = @cta_activo,
                   COD_CUENTA_DEPACUM = @cta_depacum,
                   COD_CUENTA_GASTOS  = @cta_gastos,
                   COD_CUENTA_TRANSITORIA = @cta_trans,
                   MODIFICA_USUARIO   = @mod_usuario,
                   MODIFICA_FECHA     = SYSDATETIME()
             WHERE TIPO_ACTIVO = @cod;";

                cn.Execute(q, new
                {
                    cod = data.tipo_activo.ToUpper(),
                    descripcion = data.descripcion?.ToUpper(),
                    met = (data.met_depreciacion ?? "").ToUpper(),
                    tvu = string.IsNullOrWhiteSpace(data.tipo_vida_util) ? null : data.tipo_vida_util.ToUpper(),
                    vu = string.IsNullOrWhiteSpace(data.vida_util) ? null : data.vida_util,
                    tasiento = string.IsNullOrWhiteSpace(data.asiento_genera) ? null : data.asiento_genera.ToUpper(),
                    cta_activo = string.IsNullOrWhiteSpace(data.cod_cuenta_actvo) ? null : data.cod_cuenta_actvo.ToUpper(),
                    cta_depacum = string.IsNullOrWhiteSpace(data.cod_cuenta_depacum) ? null : data.cod_cuenta_depacum.ToUpper(),
                    cta_gastos = string.IsNullOrWhiteSpace(data.cod_cuenta_gastos) ? null : data.cod_cuenta_gastos.ToUpper(),
                    cta_trans = string.IsNullOrWhiteSpace(data.cod_cuenta_transitoria) ? null : data.cod_cuenta_transitoria.ToUpper(),
                    mod_usuario = string.IsNullOrWhiteSpace(data.modifica_usuario) ? null : data.modifica_usuario
                });

                _Security_MainDB.Bitacora(new BitacoraInsertarDTO
                {
                    EmpresaId = CodEmpresa,
                    Usuario = string.IsNullOrWhiteSpace(data.modifica_usuario) ? "" : data.modifica_usuario,
                    DetalleMovimiento = $"Tipo Activo: {data.tipo_activo} - {data.descripcion}",
                    Movimiento = "Modifica - WEB",
                    Modulo = vModulo
                });

                resp.Description = "Tipo de Activo actualizado satisfactoriamente.";
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }
            return resp;
        }

        /// <summary>
        /// Elimina un tipo de activo del sistema (no se permite de momento).
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="tipo_activo"></param>
        /// <returns></returns>
        public ErrorDto Activos_TiposActivos_Eliminar(int CodEmpresa, string usuario, string tipo_activo)
        {
            var connStr = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var resp = new ErrorDto { Code = 0, Description = "Ok" };

            try
            {
                using var cn = new SqlConnection(connStr);
                var q = @"DELETE FROM dbo.ACTIVOS_TIPO_ACTIVO WHERE TIPO_ACTIVO = @cod";
                int rows = cn.Execute(q, new { cod = (tipo_activo ?? "").ToUpper() });

                if (rows == 0)
                {
                    resp.Code = -2;
                    resp.Description = $"El Tipo de Activo {tipo_activo.ToUpper()} no existe.";
                    return resp;
                }

                _Security_MainDB.Bitacora(new BitacoraInsertarDTO
                {
                    EmpresaId = CodEmpresa,
                    Usuario = usuario ?? "",
                    DetalleMovimiento = $"Tipo Activo: {tipo_activo}",
                    Movimiento = "Elimina - WEB",
                    Modulo = vModulo
                });
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }
            return resp;
        }
        /// <summary>
        /// Obtiene una lista de Tipos de depreciación que se guardan en la tabla de tipos de activo.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Activos_TiposActivos_MetodosDepreciacion_Obtener(int CodEmpresa)
        {
            var connStr = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var resp = new ErrorDto<List<DropDownListaGenericaModel>>
            {
                Code = 0,
                Description = "Ok",
                Result = new List<DropDownListaGenericaModel>()
            };

            try
            {
                using var cn = new SqlConnection(connStr);
                // var q = "SELECT CODIGO as item, DESCRIPCION FROM dbo.ACTIVOS_METODOS_DEPRECIACION ORDER BY DESCRIPCION";
                // resp.Result = cn.Query<DropDownListaGenericaModel>(q).ToList();

                if (resp.Result.Count == 0)
                {
                    resp.Result = new List<DropDownListaGenericaModel>
                {
                      new() { item = "N", descripcion = "No Deprecia" },
                      new() { item = "L", descripcion = "Línea Recta" },
                      new() { item = "S", descripcion = "Suma de Dígitos" },
                      new() { item = "D", descripcion = "Doblemente Decreciente" },
                      new() { item = "U", descripcion = "Unidades Producidas" },
                };
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
        /// Obtiene una lista de Tipos de vida util que se guardan en la tabla de tipos de activo.
        /// </summary>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Activos_TiposActivos_TipoVidaUtil_Obtener()
        {
            return new ErrorDto<List<DropDownListaGenericaModel>>
            {
                Code = 0,
                Description = "Ok",
                Result = new List<DropDownListaGenericaModel>
            {
                new() { item = "A", descripcion = "Años" },
                new() { item = "M", descripcion = "Meses" }
            }
            };
        }
        /// <summary>
        /// Obtener lista de tipos de asientos para tipos de activo.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="contabilidad"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Activos_TiposActivos_TiposAsientos_Obtener(int CodEmpresa, int contabilidad)
        {
            var connStr = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var resp = new ErrorDto<List<DropDownListaGenericaModel>>
            {
                Code = 0,
                Description = "",
                Result = new List<DropDownListaGenericaModel>()
            };

            try
            {
                using var cn = new SqlConnection(connStr);
                var q = @"

                SELECT TIPO_ASIENTO as item, DESCRIPCION
                FROM dbo.CNTX_TIPOS_ASIENTOS
                WHERE COD_CONTABILIDAD = @cont AND ACTIVO = 1
                ORDER BY DESCRIPCION ASC";
                resp.Result = cn.Query<DropDownListaGenericaModel>(q, new { cont = contabilidad }).ToList();
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
                resp.Result = null;
            }
            return resp;
        }
    }
}