using Dapper;
using Microsoft.Data.SqlClient;
using Galileo.Models;
using Galileo.Models.ERROR;
using System.Data;
using Galileo.Models.ProGrX_Nucleo;
using Galileo.Models.Security;

namespace Galileo.DataBaseTier.ProGrX_Nucleo
{
    public class FrmSysIvaParametrosDB
    {
        private readonly IConfiguration _config;
        private readonly MSecurityMainDb _security_MainDB;
        private readonly int vModulo = 10;

        public FrmSysIvaParametrosDB(IConfiguration config)
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

                var raw = (filtros?.filtro ?? string.Empty).Trim();
                string? query = string.IsNullOrWhiteSpace(raw) ? null : $"%{raw}%";

                string sortField = (filtros?.sortField ?? string.Empty).Trim().ToLowerInvariant();
                int sortOrder = filtros?.sortOrder ?? 1; // 0=DESC, 1=ASC

                int offset = Math.Max(0, filtros?.pagina ?? 0);
                int fetch = Math.Max(1, filtros?.paginacion ?? 30);

                p.Add("@query", query);
                p.Add("@sortField", sortField);
                p.Add("@sortOrder", sortOrder);
                p.Add("@offset", offset);
                p.Add("@fetch", fetch);

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
                    WHERE (@query IS NULL OR (
                           p.cod_parametro     LIKE @query
                        OR p.descripcion       LIKE @query
                        OR p.valor             LIKE @query
                        OR p.tipo              LIKE @query
                        OR cta.Cod_Cuenta_Mask LIKE @query
                        OR m.mask10            LIKE @query
                    ))";
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
                WHERE (@query IS NULL OR (
                       p.cod_parametro    LIKE @query
                    OR p.descripcion      LIKE @query
                    OR p.valor            LIKE @query
                    OR p.tipo             LIKE @query
                    OR cta.Cod_Cuenta_Mask LIKE @query
                    OR m.mask10           LIKE @query
                ))
                ORDER BY
                    -- ASC
                    CASE WHEN @sortOrder = 1 AND @sortField = 'cod_parametro' THEN p.cod_parametro END ASC,
                    CASE WHEN @sortOrder = 1 AND @sortField = 'descripcion' THEN p.descripcion END ASC,
                    CASE WHEN @sortOrder = 1 AND @sortField = 'valor' THEN p.valor END ASC,
                    CASE WHEN @sortOrder = 1 AND @sortField = 'tipo' THEN p.tipo END ASC,
                    CASE WHEN @sortOrder = 1 AND @sortField = 'modifica_fecha' THEN p.modifica_fecha END ASC,

                    -- DESC
                    CASE WHEN @sortOrder = 0 AND @sortField = 'cod_parametro' THEN p.cod_parametro END DESC,
                    CASE WHEN @sortOrder = 0 AND @sortField = 'descripcion' THEN p.descripcion END DESC,
                    CASE WHEN @sortOrder = 0 AND @sortField = 'valor' THEN p.valor END DESC,
                    CASE WHEN @sortOrder = 0 AND @sortField = 'tipo' THEN p.tipo END DESC,
                    CASE WHEN @sortOrder = 0 AND @sortField = 'modifica_fecha' THEN p.modifica_fecha END DESC,

                    p.cod_parametro ASC
                OFFSET @offset ROWS FETCH NEXT @fetch ROWS ONLY";

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
        public ErrorDto<SysIvaParametrosUpdateResponse> Sys_Iva_Parametro_Actualizar(int CodEmpresa, string codParametro, SysIvaParametrosUpdateRequest dto, string usuario)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDto<SysIvaParametrosUpdateResponse> { Code = 0, Description = "Ok" };

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
                var validation = ValidateParametroValor(connection, tx, tipo, valor);
                if (validation.Code != 0)
                {
                    result.Code = validation.Code;
                    result.Description = validation.Description;
                    return result;
                }
                valor = validation.Description ?? valor; // For PSN, the normalized value is returned in Description

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

                result.Result = connection.QueryFirstOrDefault<SysIvaParametrosUpdateResponse>(sqlRow, new { cod = codParametro }, tx);

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
        /// Valida el valor de un parámetro según su tipo.
        /// Devuelve ErrorDto con Code=0 si es válido. Para PSN, Description contiene el valor normalizado.
        /// </summary>
        private ErrorDto<string> ValidateParametroValor(SqlConnection connection, SqlTransaction tx, string tipo, string valor)
        {
            switch (tipo.ToUpperInvariant())
            {
                case "DEC":
                case "POR":
                    return ValidateDecimal(valor);
                case "NUM":
                    return ValidateNumero(valor);
                case "CTA":
                    return ValidateCuenta(connection, tx, valor);
                case "CHR":
                    return ValidateCaracter(valor);
                case "PSN":
                    return ValidateSiNo(valor);
                case "DTS":
                    return ValidateFecha(valor);
                default:
                    return new ErrorDto<string> { Code = 0, Description = null };
            }
        }

        private ErrorDto<string> ValidateDecimal(string valor)
        {
            if (!decimal.TryParse(valor, out _))
                return new ErrorDto<string> { Code = 2, Description = "Valor inválido (decimal)." };
            return new ErrorDto<string> { Code = 0, Description = null };
        }

        private ErrorDto<string> ValidateNumero(string valor)
        {
            if (!long.TryParse(valor, out _))
                return new ErrorDto<string> { Code = 2, Description = "Valor inválido (entero)." };
            return new ErrorDto<string> { Code = 0, Description = null };
        }

        private ErrorDto<string> ValidateCuenta(SqlConnection connection, SqlTransaction tx, string valor)
        {
            var n = connection.ExecuteScalar<int>(@"
                SELECT COUNT(1)
                FROM vCNTX_CUENTAS_LOCAL
                WHERE COD_CUENTA = @cod AND ACEPTA_MOVIMIENTOS = 1;",
                new { cod = valor }, tx);

            if (n <= 0)
                return new ErrorDto<string> { Code = 2, Description = "La cuenta indicada no es válida o no acepta movimientos." };
            return new ErrorDto<string> { Code = 0, Description = null };
        }

        private ErrorDto<string> ValidateCaracter(string valor)
        {
            if (valor.Contains('\''))
                return new ErrorDto<string> { Code = 2, Description = "El valor contiene caracteres no válidos." };
            return new ErrorDto<string> { Code = 0, Description = null };
        }

        private ErrorDto<string> ValidateSiNo(string valor)
        {
            var c = valor.Length > 0 ? char.ToUpperInvariant(valor[0]) : '\0';
            if (c != 'S' && c != 'N')
                return new ErrorDto<string> { Code = 2, Description = "Indique [S] o [N]." };
            return new ErrorDto<string> { Code = 0, Description = c.ToString() };
        }

        private ErrorDto<string> ValidateFecha(string valor)
        {
            if (!DateTime.TryParse(valor, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out _))
                return new ErrorDto<string> { Code = 2, Description = "Fecha inválida." };
            return new ErrorDto<string> { Code = 0, Description = null };
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

                string? q = string.IsNullOrWhiteSpace(filtros?.filtro) ? null : $"%{filtros.filtro.Trim()}%";

                string div = (divisaRaw ?? string.Empty).Trim().ToUpperInvariant();
                string? codDivisa;
                if (div == "MN")
                {
                    codDivisa = "COL";
                }
                else if (div == "ME")
                {
                    codDivisa = "DOL";
                }
                else
                {
                    codDivisa = null;
                }

                int nlev = nivelMask ?? 0;

                var p = new DynamicParameters();
                p.Add("@conta", codContabilidad);
                p.Add("@q", q);
                p.Add("@codDivisa", codDivisa);
                p.Add("@nlev", nlev);

                const string whereSql = @"
                    WHERE COD_CONTABILIDAD = @conta
                      AND (@q IS NULL OR (
                            COD_CUENTA LIKE @q
                            OR Cod_Cuenta_Mask LIKE @q
                            OR Descripcion LIKE @q
                            OR Descripcion_Alterna LIKE @q
                          ))
                      AND (@codDivisa IS NULL OR COD_DIVISA = @codDivisa)
                      AND (
                            @nlev <= 0
                            OR (
                                (@nlev = 1 AND SUBSTRING(Cod_Cuenta_Mask,3,1)='0'  AND SUBSTRING(Cod_Cuenta_Mask,5,1)='0'  AND SUBSTRING(Cod_Cuenta_Mask,7,2)='00' AND SUBSTRING(Cod_Cuenta_Mask,10,1)='0' AND SUBSTRING(Cod_Cuenta_Mask,12,2)='00' AND SUBSTRING(Cod_Cuenta_Mask,15,2)='00')
                             OR (@nlev = 2 AND SUBSTRING(Cod_Cuenta_Mask,3,1)<>'0' AND SUBSTRING(Cod_Cuenta_Mask,5,1)='0'  AND SUBSTRING(Cod_Cuenta_Mask,7,2)='00' AND SUBSTRING(Cod_Cuenta_Mask,10,1)='0' AND SUBSTRING(Cod_Cuenta_Mask,12,2)='00' AND SUBSTRING(Cod_Cuenta_Mask,15,2)='00')
                             OR (@nlev = 3 AND SUBSTRING(Cod_Cuenta_Mask,5,1)<>'0' AND SUBSTRING(Cod_Cuenta_Mask,7,2)='00' AND SUBSTRING(Cod_Cuenta_Mask,10,1)='0' AND SUBSTRING(Cod_Cuenta_Mask,12,2)='00' AND SUBSTRING(Cod_Cuenta_Mask,15,2)='00')
                             OR (@nlev = 4 AND SUBSTRING(Cod_Cuenta_Mask,7,2)<>'00' AND SUBSTRING(Cod_Cuenta_Mask,10,1)='0' AND SUBSTRING(Cod_Cuenta_Mask,12,2)='00' AND SUBSTRING(Cod_Cuenta_Mask,15,2)='00')
                             OR (@nlev = 5 AND SUBSTRING(Cod_Cuenta_Mask,10,1)<>'0' AND SUBSTRING(Cod_Cuenta_Mask,12,2)='00' AND SUBSTRING(Cod_Cuenta_Mask,15,2)='00')
                             OR (@nlev = 6 AND SUBSTRING(Cod_Cuenta_Mask,12,2)<>'00' AND SUBSTRING(Cod_Cuenta_Mask,15,2)='00')
                             OR (@nlev = 7 AND SUBSTRING(Cod_Cuenta_Mask,15,2)<>'00')
                            )
                          )";

                // Total
                var sqlCount = $"SELECT COUNT(*) FROM vCNTX_CUENTAS_LOCAL {whereSql}";
                result.Result.total = connection.Query<int>(sqlCount, p).FirstOrDefault();

                string sortField = (filtros?.sortField ?? string.Empty).Trim().ToLowerInvariant();
                int sortOrder = filtros?.sortOrder ?? 1; // 0=DESC, 1=ASC
                int offset = Math.Max(0, filtros?.pagina ?? 0);
                int fetch = Math.Max(1, filtros?.paginacion ?? 30);

                p.Add("@sortField", sortField);
                p.Add("@sortOrder", sortOrder);
                p.Add("@offset", offset);
                p.Add("@fetch", fetch);

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
            {whereSql}
            ORDER BY
                -- ASC
                CASE WHEN @sortOrder = 1 AND (@sortField = 'codigo' OR @sortField = 'codigomask') THEN Cod_Cuenta_Mask END ASC,
                CASE WHEN @sortOrder = 1 AND @sortField = 'nombre' THEN COALESCE(NULLIF(Descripcion_Alterna,''), Descripcion) END ASC,

                -- DESC
                CASE WHEN @sortOrder = 0 AND (@sortField = 'codigo' OR @sortField = 'codigomask') THEN Cod_Cuenta_Mask END DESC,
                CASE WHEN @sortOrder = 0 AND @sortField = 'nombre' THEN COALESCE(NULLIF(Descripcion_Alterna,''), Descripcion) END DESC,

                Cod_Cuenta_Mask ASC
            OFFSET @offset ROWS FETCH NEXT @fetch ROWS ONLY;";

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