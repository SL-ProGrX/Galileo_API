using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.Security;
using System.Data;
using static PgxAPI.Models.ProGrX_Nucleo.FrmSysIvaParametrosModels;

namespace PgxAPI.DataBaseTier.ProGrX_Nucleo
{
    public class frmSYS_IVA_ParametrosDB
    {
        private readonly IConfiguration _config;
        private readonly MSecurityMainDb _security_MainDB;
        private readonly int vModulo = 10;

        public frmSYS_IVA_ParametrosDB(IConfiguration config)
        {
            _config = config;
            _security_MainDB = new MSecurityMainDb(_config);
        }

        /// <summary>
        /// Obtiene lista paginada (lazy) de parámetros IVA desde SYS_IVA_PARAMETROS; ejecuta spSys_IVA_Parametros antes del SELECT.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDto<SysIvaParametrosLista> Sys_Iva_Parametros_Lista_Obtener(int CodEmpresa, FiltrosLazyLoadData filtros)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDto<SysIvaParametrosLista>
            {
                Code = 0,
                Description = "Ok",
                Result = new SysIvaParametrosLista { total = 0, lista = new List<SysIvaParametrosData>() }
            };

            try
            {
                using var connection = new SqlConnection(stringConn);
                connection.Open();

                // Refresca datos
                connection.Execute("dbo.spSys_IVA_Parametros", commandType: CommandType.StoredProcedure, commandTimeout: 60);

                var p = new DynamicParameters();
                string where = "";
                if (!string.IsNullOrWhiteSpace(filtros?.filtro))
                {
                    where = @"
                    WHERE (
                           p.cod_parametro     LIKE @query
                        OR p.descripcion       LIKE @query
                        OR p.valor             LIKE @query
                        OR p.tipo              LIKE @query
                        OR cta.Cod_Cuenta_Mask LIKE @query
                        OR m.mask10            LIKE @query
                    )";
                    p.Add("@query", "%" + filtros.filtro.Trim() + "%");
                }

                string sort = (filtros?.sortField ?? "").Trim().ToLowerInvariant();
                string sortFieldSql = sort switch
                {
                    "cod_parametro" => "p.cod_parametro",
                    "descripcion" => "p.descripcion",
                    "valor" => "p.valor",
                    "tipo" => "p.tipo",
                    "modifica_fecha" => "p.modifica_fecha",
                    _ => "p.cod_parametro"
                };
                string sortDir = (filtros?.sortOrder ?? 1) == 0 ? "DESC" : "ASC";
                int pagina = Math.Max(0, filtros?.pagina ?? 0);
                int paginacion = Math.Max(1, filtros?.paginacion ?? 30);

                // ---------- COUNT ----------
                var sqlCount = $@"
                    SELECT COUNT(*)
                    FROM SYS_IVA_PARAMETROS p
                    OUTER APPLY (
                        SELECT valorDigits = REPLACE(REPLACE(REPLACE(LTRIM(RTRIM(p.valor)),'-',''),' ',''),'.','')
                    ) nd
                    OUTER APPLY (
                    SELECT valor10 = CASE
                               WHEN p.tipo IN ('CTA','cta')
                                 AND nd.valorDigits IS NOT NULL
                                 AND nd.valorDigits <> ''
                                 AND PATINDEX('%[^0-9]%', nd.valorDigits) = 0
                               THEN
                                   CASE WHEN LEN(nd.valorDigits) >= 10
                                        THEN LEFT(nd.valorDigits, 10)
                                        ELSE nd.valorDigits + REPLICATE('0', 10 - LEN(nd.valorDigits))
                                   END
                               ELSE NULL
                             END
                    ) v
                    OUTER APPLY (
                        SELECT mask10 = CASE WHEN v.valor10 IS NULL THEN NULL ELSE
                             SUBSTRING(v.valor10,1,1) + '-' +
                             SUBSTRING(v.valor10,2,1) + '-' +
                             SUBSTRING(v.valor10,3,1) + '-' +
                             SUBSTRING(v.valor10,4,2) + '-' +
                             SUBSTRING(v.valor10,6,1) + '-' +
                             SUBSTRING(v.valor10,7,2) + '-' +
                             SUBSTRING(v.valor10,9,2) END
                    ) m
                    OUTER APPLY (
                        SELECT TOP 1
                               c.Cod_Cuenta_Mask,
                               COALESCE(NULLIF(c.Descripcion_Alterna,''), c.Descripcion) AS CuentaNombre
                        FROM vCNTX_CUENTAS_LOCAL c
                        WHERE m.mask10 IS NOT NULL
                          AND c.Cod_Cuenta_Mask = m.mask10
                        ORDER BY c.COD_CONTABILIDAD
                    ) cta
                    {where}";
                 result.Result.total = connection.Query<int>(sqlCount, p).FirstOrDefault();

                // ---------- LISTA ----------
                var sql = $@"
                SELECT
                    p.cod_parametro    AS codParametro,
                    p.descripcion      AS descripcion,
                    p.valor            AS valor,
                    p.tipo             AS tipo,
                    p.visible          AS visible,
                    p.notas            AS notas,
                    p.registro_usuario AS registroUsuario,
                    p.registro_fecha   AS registroFecha,
                    p.modifica_usuario AS modificaUsuario,
                    p.modifica_fecha   AS modificaFecha,
                    CASE WHEN UPPER(p.tipo)='CTA' THEN COALESCE(cta.Cod_Cuenta_Mask, m.mask10) ELSE NULL END AS valorMask,
                    CASE WHEN UPPER(p.tipo)='CTA' THEN cta.CuentaNombre ELSE NULL END AS cuentaDescripcion
                FROM SYS_IVA_PARAMETROS p
                OUTER APPLY (
                    SELECT valorDigits = REPLACE(REPLACE(REPLACE(LTRIM(RTRIM(p.valor)),'-',''),' ',''),'.','')
                ) nd
                OUTER APPLY (
                    SELECT valor10 = CASE
                               WHEN p.tipo IN ('CTA','cta')
                                 AND nd.valorDigits IS NOT NULL
                                 AND nd.valorDigits <> ''
                                 AND PATINDEX('%[^0-9]%', nd.valorDigits) = 0
                               THEN
                                   CASE WHEN LEN(nd.valorDigits) >= 10
                                        THEN LEFT(nd.valorDigits, 10)
                                        ELSE nd.valorDigits + REPLICATE('0', 10 - LEN(nd.valorDigits))
                                   END
                               ELSE NULL
                             END
                ) v
                OUTER APPLY (
                    SELECT mask10 = CASE WHEN v.valor10 IS NULL THEN NULL ELSE
                         SUBSTRING(v.valor10,1,1) + '-' +
                         SUBSTRING(v.valor10,2,1) + '-' +
                         SUBSTRING(v.valor10,3,1) + '-' +
                         SUBSTRING(v.valor10,4,2) + '-' +
                         SUBSTRING(v.valor10,6,1) + '-' +
                         SUBSTRING(v.valor10,7,2) + '-' +
                         SUBSTRING(v.valor10,9,2) END
                ) m
                OUTER APPLY (
                    SELECT TOP 1
                           c.Cod_Cuenta_Mask,
                           COALESCE(NULLIF(c.Descripcion_Alterna,''), c.Descripcion) AS CuentaNombre
                    FROM vCNTX_CUENTAS_LOCAL c
                    WHERE m.mask10 IS NOT NULL
                      AND c.Cod_Cuenta_Mask = m.mask10
                    ORDER BY c.COD_CONTABILIDAD
                ) cta
                {where}
                ORDER BY {sortFieldSql} {sortDir}
                OFFSET {pagina} ROWS FETCH NEXT {paginacion} ROWS ONLY";

                result.Result.lista = connection.Query<SysIvaParametrosData>(sql, p).ToList();
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result.total = 0;
                result.Result.lista = null;
            }

            return result;
        }

        /// <summary>
        /// Obtiene lista completa (sin paginar) de parámetros IVA para export; ejecuta spSys_IVA_Parametros antes del SELECT.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDto<List<SysIvaParametrosData>> Sys_Iva_Parametros_Obtener(int CodEmpresa, FiltrosLazyLoadData filtros)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDto<List<SysIvaParametrosData>>
            {
                Code = 0,
                Description = "Ok",
                Result = new List<SysIvaParametrosData>()
            };

            try
            {
                using var connection = new SqlConnection(stringConn);
                connection.Open();

                connection.Execute("dbo.spSys_IVA_Parametros", commandType: CommandType.StoredProcedure, commandTimeout: 60);

                string where = "";
                var p = new DynamicParameters();
                if (!string.IsNullOrWhiteSpace(filtros?.filtro))
                {
                    where = @"
                    WHERE (
                           p.cod_parametro LIKE @query
                        OR p.descripcion   LIKE @query
                        OR p.valor         LIKE @query
                        OR p.tipo          LIKE @query
                        OR cta.Cod_Cuenta_Mask LIKE @query
                    )";
                    p.Add("@query", "%" + filtros.filtro.Trim() + "%");
                }

                var query = $@"
                SELECT
                    p.cod_parametro    AS codParametro,
                    p.descripcion      AS descripcion,
                    p.valor            AS valor,
                    p.tipo             AS tipo,
                    p.visible          AS visible,
                    p.notas            AS notas,
                    p.registro_usuario AS registroUsuario,
                    p.registro_fecha   AS registroFecha,
                    p.modifica_usuario AS modificaUsuario,
                    p.modifica_fecha   AS modificaFecha,
                    CASE WHEN UPPER(p.tipo)='CTA' THEN cta.Cod_Cuenta_Mask ELSE NULL END AS valorMask,
                    CASE WHEN UPPER(p.tipo)='CTA' THEN cta.CuentaNombre   ELSE NULL END AS cuentaDescripcion
               FROM SYS_IVA_PARAMETROS p
                OUTER APPLY (
                    SELECT TOP 1
                           Cod_Cuenta_Mask,
                           COALESCE(NULLIF(Descripcion_Alterna,''), Descripcion) AS CuentaNombre
                    FROM vCNTX_CUENTAS_LOCAL c
                    WHERE p.tipo IN ('CTA','cta')
                      AND p.valor IS NOT NULL
                      AND p.valor <> ''
                      AND PATINDEX('%[^0-9]%', p.valor) = 0   -- solo dígitos
                      AND c.COD_CUENTA = CONVERT(BIGINT, p.valor)
                ) cta
                {where}
                ORDER BY p.cod_parametro";

                result.Result = connection.Query<SysIvaParametrosData>(query, p).ToList();
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
        /// Actualiza el valor de un parámetro IVA (UPDATE + Bitácora) validando según tipo (DEC, NUM, POR, CTA, CHR, PSN, DTS).
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="codParametro"></param>
        /// <param name="dto"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto<SysIvaParametrosData> Sys_Iva_Parametro_Actualizar(int CodEmpresa, string codParametro, SysIvaParametrosUpdateRequest dto, string usuario)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDto<SysIvaParametrosData> { Code = 0, Description = "Ok" };

            try
            {
                using var connection = new SqlConnection(stringConn);
                connection.Open();
                using var tx = connection.BeginTransaction();

                var tipo = connection.ExecuteScalar<string>(
                    "SELECT tipo FROM SYS_IVA_PARAMETROS WHERE cod_parametro=@cod;",
                    new { cod = codParametro }, tx);

                if (string.IsNullOrWhiteSpace(tipo))
                {
                    result.Code = 1;
                    result.Description = "No existe el parámetro";
                    return result;
                }

                string valor = dto.valor?.Trim() ?? string.Empty;
                switch (tipo.ToUpperInvariant())
                {
                    case "DEC":
                    case "POR":
                        if (!decimal.TryParse(valor, out _))
                        { result.Code = 2; result.Description = "Valor inválido (decimal)."; return result; }
                        break;

                    case "NUM":
                        if (!long.TryParse(valor, out _))
                        { result.Code = 2; result.Description = "Valor inválido (entero)."; return result; }
                        break;

                    case "CTA":
                        {
                            var n = connection.ExecuteScalar<int>(@"
                            SELECT COUNT(1)
                            FROM vCNTX_CUENTAS_LOCAL
                            WHERE COD_CUENTA = @cod AND ACEPTA_MOVIMIENTOS = 1;",
                                new { cod = valor }, tx);

                            if (n <= 0)
                            {
                                result.Code = 2;
                                result.Description = "La cuenta indicada no es válida o no acepta movimientos.";
                                return result;
                            }
                            break;
                        }



                    case "CHR":
                        if (valor.Contains('\''))
                        { result.Code = 2; result.Description = "El valor contiene caracteres no válidos."; return result; }
                        break;

                    case "PSN":
                        var c = valor.Length > 0 ? char.ToUpperInvariant(valor[0]) : '\0';
                        if (c != 'S' && c != 'N')
                        { result.Code = 2; result.Description = "Indique [S] o [N]."; return result; }
                        valor = c.ToString();
                        break;

                    case "DTS":
                        if (!DateTime.TryParse(valor, out _))
                        { result.Code = 2; result.Description = "Fecha inválida."; return result; }
                        break;
                }

                // UPDATE
                const string sqlUpd = @"
                UPDATE SYS_IVA_PARAMETROS
                   SET modifica_usuario = @usr,
                       modifica_fecha   = dbo.MyGetdate(),
                       valor            = @val
                 WHERE cod_parametro   = @cod;";
                connection.Execute(sqlUpd, new { usr = usuario, val = valor, cod = codParametro }, tx);

                // Bitácora
                _security_MainDB.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = usuario,
                    Modulo = vModulo,
                    Movimiento = "Modifica - WEB",
                    DetalleMovimiento = $"Parámetro de IVA: {codParametro} -> {valor}"
                });

                // Fila actualizada
                var sqlRow = @"
                SELECT
                    p.cod_parametro    AS codParametro,
                    p.descripcion      AS descripcion,
                    p.valor            AS valor,
                    p.tipo             AS tipo,
                    p.visible          AS visible,
                    p.notas            AS notas,
                    p.registro_usuario AS registroUsuario,
                    p.registro_fecha   AS registroFecha,
                    p.modifica_usuario AS modificaUsuario,
                    p.modifica_fecha   AS modificaFecha,
                    CASE WHEN UPPER(p.tipo)='CTA' THEN COALESCE(cta.Cod_Cuenta_Mask, m.mask10) ELSE NULL END AS valorMask,
                    CASE WHEN UPPER(p.tipo)='CTA' THEN cta.CuentaNombre ELSE NULL END AS cuentaDescripcion
                FROM SYS_IVA_PARAMETROS p
                OUTER APPLY ( SELECT valorDigits = REPLACE(REPLACE(REPLACE(LTRIM(RTRIM(p.valor)),'-',''),' ',''),'.','') ) nd
                OUTER APPLY (
                    SELECT valor10 = CASE
                        WHEN p.tipo IN ('CTA','cta') AND nd.valorDigits <> '' AND PATINDEX('%[^0-9]%', nd.valorDigits)=0
                        THEN CASE WHEN LEN(nd.valorDigits)>=10 THEN LEFT(nd.valorDigits,10)
                                  ELSE nd.valorDigits + REPLICATE('0', 10-LEN(nd.valorDigits)) END
                        ELSE NULL END
                ) v
                OUTER APPLY (
                    SELECT mask10 = CASE WHEN v.valor10 IS NULL THEN NULL ELSE
                         SUBSTRING(v.valor10,1,1) + '-' +
                         SUBSTRING(v.valor10,2,1) + '-' +
                         SUBSTRING(v.valor10,3,1) + '-' +
                         SUBSTRING(v.valor10,4,2) + '-' +
                         SUBSTRING(v.valor10,6,1) + '-' +
                         SUBSTRING(v.valor10,7,2) + '-' +
                         SUBSTRING(v.valor10,9,2) END
                ) m
                OUTER APPLY (
                    SELECT TOP 1 c.Cod_Cuenta_Mask,
                           COALESCE(NULLIF(c.Descripcion_Alterna,''), c.Descripcion) AS CuentaNombre
                    FROM vCNTX_CUENTAS_LOCAL c
                    WHERE m.mask10 IS NOT NULL AND c.Cod_Cuenta_Mask = m.mask10
                    ORDER BY c.COD_CONTABILIDAD
                ) cta
                WHERE p.cod_parametro=@cod;";

                result.Result = connection.QueryFirstOrDefault<SysIvaParametrosData>(sqlRow, new { cod = codParametro }, tx);

                tx.Commit();
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
        /// Busca cuentas (lazy) en vCNTX_CUENTAS_LOCAL por código con máscara o descripción para el diálogo F4.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="codContabilidad"></param>
        /// <param name="filtros"></param>
        /// <param name="nivelMask"></param>
        /// <param name="divisaRaw"></param>
        /// <returns></returns>
        public ErrorDto<SysIvaCuentasResumenLista> Sys_Iva_Cuentas_Buscar(int CodEmpresa,int codContabilidad,FiltrosLazyLoadData filtros,int? nivelMask = null,string? divisaRaw = null)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDto<SysIvaCuentasResumenLista>
            {
                Code = 0,
                Description = "Ok",
                Result = new SysIvaCuentasResumenLista { total = 0, lista = new List<SysIvaCuentasResumenData>() }
            };

            try
            {
                using var connection = new SqlConnection(stringConn);
                connection.Open();

                var where = "WHERE COD_CONTABILIDAD = @conta";
                var p = new DynamicParameters(new { conta = codContabilidad });

                if (!string.IsNullOrWhiteSpace(filtros?.filtro))
                {
                    where += " AND (COD_CUENTA LIKE @q OR Cod_Cuenta_Mask LIKE @q OR Descripcion LIKE @q OR Descripcion_Alterna LIKE @q)";
                    p.Add("@q", "%" + filtros.filtro.Trim() + "%");
                }

                var div = (divisaRaw ?? "").ToUpperInvariant();
                if (div == "MN") where += " AND COD_DIVISA = 'COL'";
                else if (div == "ME") where += " AND COD_DIVISA = 'DOL'";

                var nlev = nivelMask ?? 0;
                if (nlev > 0)
                {
                    string wNivel = nlev switch
                    {
                        // Máscara: 1-0-0-00-0-00-00  (posiciones de dígitos: 1,3,5,7-8,10,12-13,15-16)
                        1 => "SUBSTRING(Cod_Cuenta_Mask,3,1)='0'  AND SUBSTRING(Cod_Cuenta_Mask,5,1)='0' " +
                             "AND SUBSTRING(Cod_Cuenta_Mask,7,2)='00' AND SUBSTRING(Cod_Cuenta_Mask,10,1)='0' " +
                             "AND SUBSTRING(Cod_Cuenta_Mask,12,2)='00' AND SUBSTRING(Cod_Cuenta_Mask,15,2)='00'",

                        2 => "SUBSTRING(Cod_Cuenta_Mask,3,1)<>'0' AND SUBSTRING(Cod_Cuenta_Mask,5,1)='0' " +
                             "AND SUBSTRING(Cod_Cuenta_Mask,7,2)='00' AND SUBSTRING(Cod_Cuenta_Mask,10,1)='0' " +
                             "AND SUBSTRING(Cod_Cuenta_Mask,12,2)='00' AND SUBSTRING(Cod_Cuenta_Mask,15,2)='00'",

                        3 => "SUBSTRING(Cod_Cuenta_Mask,5,1)<>'0' AND SUBSTRING(Cod_Cuenta_Mask,7,2)='00' " +
                             "AND SUBSTRING(Cod_Cuenta_Mask,10,1)='0' AND SUBSTRING(Cod_Cuenta_Mask,12,2)='00' " +
                             "AND SUBSTRING(Cod_Cuenta_Mask,15,2)='00'",

                        4 => "SUBSTRING(Cod_Cuenta_Mask,7,2)<>'00' AND SUBSTRING(Cod_Cuenta_Mask,10,1)='0' " +
                             "AND SUBSTRING(Cod_Cuenta_Mask,12,2)='00' AND SUBSTRING(Cod_Cuenta_Mask,15,2)='00'",

                        5 => "SUBSTRING(Cod_Cuenta_Mask,10,1)<>'0' AND SUBSTRING(Cod_Cuenta_Mask,12,2)='00' " +
                             "AND SUBSTRING(Cod_Cuenta_Mask,15,2)='00'",

                        6 => "SUBSTRING(Cod_Cuenta_Mask,12,2)<>'00' AND SUBSTRING(Cod_Cuenta_Mask,15,2)='00'",

                        7 => "SUBSTRING(Cod_Cuenta_Mask,15,2)<>'00'",

                        _ => "Acepta_Movimientos = 1"
                    };
                    where += " AND (" + wNivel + ")";
                }

                // Total
                var sqlCount = $"SELECT COUNT(*) FROM vCNTX_CUENTAS_LOCAL {where}";
                result.Result.total = connection.Query<int>(sqlCount, p).FirstOrDefault();

                // Orden / Paginación
                string sortField = (filtros?.sortField ?? "").Trim().ToLowerInvariant();
                string sortFieldSql = sortField switch
                {
                    "codigo" => "Cod_Cuenta_Mask",
                    "codigomask" => "Cod_Cuenta_Mask",
                    "nombre" => "COALESCE(NULLIF(Descripcion_Alterna,''), Descripcion)",
                    _ => "Cod_Cuenta_Mask"
                };
                string sortDir = (filtros?.sortOrder ?? 1) == 0 ? "DESC" : "ASC";
                int pagina = Math.Max(0, filtros?.pagina ?? 0);
                int paginacion = Math.Max(1, filtros?.paginacion ?? 30);

                // Lista
                var sql = $@"
            SELECT
               CONVERT(varchar(50), COD_CUENTA)                         AS codigo,
               Cod_Cuenta_Mask                                          AS codigoMask,
               Cod_Cuenta_Alterna                                       AS codigoAlterna,
               COALESCE(NULLIF(Descripcion_Alterna,''), Descripcion)    AS nombre,
               CASE WHEN Acepta_Movimientos=1 THEN 'S' ELSE 'N' END     AS movimientos,
               COD_DIVISA                                               AS divisa,
               NIVEL                                                    AS nivel
            FROM vCNTX_CUENTAS_LOCAL
            {where}
            ORDER BY {sortFieldSql} {sortDir}
            OFFSET {pagina} ROWS FETCH NEXT {paginacion} ROWS ONLY;";

                result.Result.lista = connection.Query<SysIvaCuentasResumenData>(sql, p).ToList();
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result.total = 0;
                result.Result.lista = null;
            }

            return result;
        }

        /// <summary>
        /// Obtiene una cuenta exacta (SIN máscara) desde vCNTX_CUENTAS_LOCAL, devolviendo máscara y descripción.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="codContabilidad"></param>
        /// <param name="codigoSinMask"></param>
        /// <returns></returns>
        public ErrorDto<SysIvaCuentasResumenData> Sys_Iva_CuentaPorCodigo_Obtener(int CodEmpresa, int codContabilidad, string codigoSinMask)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDto<SysIvaCuentasResumenData> { Code = 0 };

            try
            {
                using var connection = new SqlConnection(stringConn);
                const string sql = @"
                SELECT TOP 1
                    COD_CUENTA      AS codigo,
                    Cod_Cuenta_Mask AS codigoMask,
                    Descripcion     AS nombre,
                    ''              AS movimientos,
                    NULL            AS divisa,
                    NULL            AS nivel
                FROM vCNTX_CUENTAS_LOCAL
                WHERE COD_CONTABILIDAD=@conta AND COD_CUENTA=@cod;";
                result.Result = connection.QueryFirstOrDefault<SysIvaCuentasResumenData>(sql, new { conta = codContabilidad, cod = codigoSinMask });
                result.Description = result.Result == null ? "No existe la cuenta" : "OK";
                result.Code = result.Result == null ? 1 : 0;
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
            }

            return result;
        }
        /// <summary>
        /// Obtiene todas las cuentas de contabilidad
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="codContabilidad"></param>
        /// <returns></returns
        public ErrorDto<SysIvaCuentasResumenLista> Sys_Iva_Cuentas_Todas_Obtener(int CodEmpresa, int codContabilidad)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDto<SysIvaCuentasResumenLista>
            {
                Code = 0,
                Description = "Ok",
                Result = new SysIvaCuentasResumenLista { total = 0, lista = new List<SysIvaCuentasResumenData>() }
            };

            try
            {
                using var connection = new SqlConnection(stringConn);
                connection.Open();

                const string sql = @"
                SELECT
                   CONVERT(varchar(50), COD_CUENTA)               AS codigo,
                   Cod_Cuenta_Mask                                 AS codigoMask,
                   Cod_Cuenta_Alterna                              AS codigoAlterna,
                   COALESCE(NULLIF(Descripcion_Alterna,''), Descripcion) AS nombre,
                   CASE WHEN Acepta_Movimientos=1 THEN 'S' ELSE 'N' END AS movimientos,
                   COD_DIVISA                                      AS divisa,
                   NIVEL                                           AS nivel
                FROM vCNTX_CUENTAS_LOCAL
                WHERE COD_CONTABILIDAD = @conta
                ORDER BY Cod_Cuenta_Alterna, Cod_Cuenta_Mask;";

                var lista = connection.Query<SysIvaCuentasResumenData>(sql, new { conta = codContabilidad }).ToList();
                result.Result.lista = lista;
                result.Result.total = lista.Count;
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result.total = 0;
                result.Result.lista = null;
            }

            return result;
        }
    }
}
