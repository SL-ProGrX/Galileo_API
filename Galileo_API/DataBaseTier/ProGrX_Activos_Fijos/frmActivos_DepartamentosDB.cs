using System.Data;
using Dapper;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Activos_Fijos;
using Galileo.Models.Security;

namespace Galileo.DataBaseTier.ProGrX_Activos_Fijos
{
    public class FrmActivosSeccionesDb
    {
        private readonly int vModulo = 36; // Activos Fijos
        private readonly MSecurityMainDb _Security_MainDB;
        private readonly PortalDB _portalDB;
        private const string _filtro = "@filtro";

        // SQL base para DEPARTAMENTOS (reutilizado en lista y exportación)
        private const string DeptSelectBaseSql = @"
            FROM dbo.ACTIVOS_DEPARTAMENTOS d
            LEFT JOIN dbo.CNTX_UNIDADES u
              ON u.COD_UNIDAD = d.COD_UNIDAD";

        private const string DeptWhereFilterSql = @"
            WHERE 1 = 1
              AND (
                    @filtro IS NULL
                    OR d.COD_DEPARTAMENTO         LIKE @filtro
                    OR d.DESCRIPCION              LIKE @filtro
                    OR d.COD_UNIDAD               LIKE @filtro
                    OR ISNULL(u.DESCRIPCION,'')   LIKE @filtro
                    OR ISNULL(d.REGISTRO_USUARIO,'') LIKE @filtro
                  )";

        // SQL base para SECCIONES (reutilizado en lista y exportación)
        private const string SeccSelectBaseSql = @"
            FROM dbo.ACTIVOS_SECCIONES s
            LEFT JOIN dbo.CNTX_CENTRO_COSTOS cc
                ON cc.COD_CENTRO_COSTO = s.COD_CENTRO_COSTO
            LEFT JOIN dbo.ACTIVOS_DEPARTAMENTOS d
                ON d.COD_DEPARTAMENTO = s.COD_DEPARTAMENT";

        private const string SeccWhereFilterSql = @"
            WHERE 1 = 1
              AND (@dept IS NULL OR s.COD_DEPARTAMENTO = @dept)
              AND (
                    @filtro IS NULL
                    OR s.COD_SECCION             LIKE @filtro
                    OR s.DESCRIPCION             LIKE @filtro
                    OR s.COD_CENTRO_COSTO        LIKE @filtro
                    OR ISNULL(cc.DESCRIPCION,'') LIKE @filtro
                    OR ISNULL(d.DESCRIPCION,'')  LIKE @filtro
                  )";

        public FrmActivosSeccionesDb(IConfiguration config)
        {
            _Security_MainDB = new MSecurityMainDb(config);
            _portalDB        = new PortalDB(config);
        }

        #region Helpers privados

        private static string? BuildFiltroLike(FiltrosLazyLoadData? filtros)
        {
            var f = filtros?.filtro;
            return string.IsNullOrWhiteSpace(f) ? null : $"%{f.Trim()}%";
        }

        private static string Normalize(string? value) =>
            (value ?? string.Empty).ToUpper();

        /// <summary>
        /// Construye parámetros comunes para listas paginadas (departamentos y secciones)
        /// </summary>
        private  static DynamicParameters BuildPagedListParameters(
            FiltrosLazyLoadData? filtros,
            string defaultSortField,
            string? codDepartamento = null)
        {
            var p = new DynamicParameters();

            // Filtro
            var filtroLike = BuildFiltroLike(filtros);
            p.Add(_filtro, filtroLike, DbType.String);

            // Sorting
            var sortFieldNorm = (filtros?.sortField ?? defaultSortField)
                .Trim()
                .ToLowerInvariant();
            int sortOrder = filtros?.sortOrder == 0 ? 0 : 1; // 0 = DESC, 1 = ASC
            p.Add("@sortField", sortFieldNorm, DbType.String);
            p.Add("@sortOrder", sortOrder, DbType.Int32);

            // Paginación
            int pagina     = filtros?.pagina ?? 0;
            int paginacion = filtros?.paginacion ?? 50;
            p.Add("@offset", pagina, DbType.Int32);
            p.Add("@rows",   paginacion, DbType.Int32);

            // Departamento (solo se usa en secciones; en departamentos no afecta)
            string? dept = string.IsNullOrWhiteSpace(codDepartamento)
                ? null
                : Normalize(codDepartamento);
            p.Add("@dept", dept, DbType.String);

            return p;
        }

        /// <summary>
        /// Wrapper genérico para ExecuteNonQuery + Bitácora + mensaje OK.
        /// Centraliza el patrón repetido de insertar/actualizar/eliminar.
        /// </summary>
        private ErrorDto ExecuteNonQueryWithBitacora(
            int CodEmpresa,
            string? usuario,
            string sql,
            object? parameters,
            string detalleMovimiento,
            string movimiento,
            string? mensajeOkOverride = null)
        {
            var resp = DbHelper.CreateOkResponse();

            try
            {
                var dbResp = DbHelper.ExecuteNonQuery(
                    _portalDB,
                    CodEmpresa,
                    sql,
                    parameters);

                resp.Code        = dbResp.Code;
                resp.Description = dbResp.Description;

                if (resp.Code == 0)
                {
                    _Security_MainDB.Bitacora(new BitacoraInsertarDto
                    {
                        EmpresaId         = CodEmpresa,
                        Usuario           = usuario ?? "",
                        DetalleMovimiento = detalleMovimiento,
                        Movimiento        = movimiento,
                        Modulo            = vModulo
                    });

                    if (!string.IsNullOrWhiteSpace(mensajeOkOverride))
                        resp.Description = mensajeOkOverride;
                }
            }
            catch (Exception ex)
            {
                resp.Code        = -1;
                resp.Description = ex.Message;
            }

            return resp;
        }

        /// <summary>
        /// Helper genérico para SELECT COUNT(1) con manejo de errores.
        /// </summary>
        private ErrorDto<int> ExecuteCount(
            int CodEmpresa,
            string sql,
            object parameters)
        {
            return DbHelper.ExecuteSingleQuery(
                _portalDB,
                CodEmpresa,
                sql,
                defaultValue: 0,
                parameters: parameters);
        }

        #endregion

        /// <summary>
        /// Lista paginada (lazy) de departamentos.
        /// </summary>
        public ErrorDto<ActivosDepartamentosLista> Activos_DepartamentosLista_Obtener(
            int CodEmpresa,
            FiltrosLazyLoadData filtros)
        {
            var resp = DbHelper.CreateOkResponse(
                new ActivosDepartamentosLista
                {
                    total = 0,
                    lista = new List<ActivosDepartamentosData>()
                });

            try
            {
                var p = BuildPagedListParameters(filtros, defaultSortField: "cod_departamento");

                const string sqlCount = @"
                    SELECT COUNT(DISTINCT d.COD_DEPARTAMENTO)
                    " + DeptSelectBaseSql + @"
                    " + DeptWhereFilterSql + ";";

                const string sqlData = @"
                    SELECT DISTINCT
                        d.COD_DEPARTAMENTO            AS cod_departamento,
                        ISNULL(d.DESCRIPCION,'')      AS descripcion,
                        ISNULL(d.COD_UNIDAD,'')       AS cod_unidad,
                        ISNULL(u.DESCRIPCION,'')      AS unidad_desc,
                        ISNULL(d.REGISTRO_USUARIO,'') AS usuario
                    " + DeptSelectBaseSql + @"
                    " + DeptWhereFilterSql + @"
                    ORDER BY
                        -- sortOrder = 0 => DESC
                        CASE WHEN @sortOrder = 0 AND @sortField = 'cod_departamento' THEN d.COD_DEPARTAMENTO END DESC,
                        CASE WHEN @sortOrder = 0 AND @sortField = 'descripcion'      THEN d.DESCRIPCION      END DESC,
                        CASE WHEN @sortOrder = 0 AND @sortField = 'cod_unidad'       THEN d.COD_UNIDAD       END DESC,
                        CASE WHEN @sortOrder = 0 AND @sortField = 'unidad_desc'      THEN u.DESCRIPCION      END DESC,
                        CASE WHEN @sortOrder = 0 AND @sortField = 'usuario'          THEN d.REGISTRO_USUARIO END DESC,

                        -- sortOrder = 1 => ASC
                        CASE WHEN @sortOrder = 1 AND @sortField = 'cod_departamento' THEN d.COD_DEPARTAMENTO END ASC,
                        CASE WHEN @sortOrder = 1 AND @sortField = 'descripcion'      THEN d.DESCRIPCION      END ASC,
                        CASE WHEN @sortOrder = 1 AND @sortField = 'cod_unidad'       THEN d.COD_UNIDAD       END ASC,
                        CASE WHEN @sortOrder = 1 AND @sortField = 'unidad_desc'      THEN u.DESCRIPCION      END ASC,
                        CASE WHEN @sortOrder = 1 AND @sortField = 'usuario'          THEN d.REGISTRO_USUARIO END ASC
                    OFFSET @offset ROWS FETCH NEXT @rows ROWS ONLY;";

                using var cn = _portalDB.CreateConnection(CodEmpresa);

                if (resp.Result != null)
                {
                    resp.Result.total = cn.QueryFirstOrDefault<int?>(sqlCount, p) ?? 0;
                    resp.Result.lista = cn.Query<ActivosDepartamentosData>(sqlData, p).ToList();
                }
            }
            catch (Exception ex)
            {
                resp.Code        = -1;
                resp.Description = ex.Message;
                if (resp.Result != null)
                {
                    resp.Result.total = 0;
                    resp.Result.lista = [];
                }
            }
            return resp;
        }

        /// <summary>
        /// Lista completa de departamentos (sin paginar) para exportar.
        /// </summary>
        public ErrorDto<List<ActivosDepartamentosData>> Activos_Departamentos_Obtener(
            int CodEmpresa,
            FiltrosLazyLoadData filtros)
        {
            var p = new DynamicParameters();
            p.Add(_filtro, BuildFiltroLike(filtros), DbType.String);

            string query = @"
                SELECT
                    d.COD_DEPARTAMENTO            AS cod_departamento,
                    ISNULL(d.DESCRIPCION,'')      AS descripcion,
                    ISNULL(d.COD_UNIDAD,'')       AS cod_unidad,
                    ISNULL(u.DESCRIPCION,'')      AS unidad_desc,
                    ISNULL(d.REGISTRO_USUARIO,'') AS usuario
                " + DeptSelectBaseSql + @"
                " + DeptWhereFilterSql + @"
                ORDER BY d.COD_DEPARTAMENTO;";

            return DbHelper.ExecuteListQuery<ActivosDepartamentosData>(
                _portalDB,
                CodEmpresa,
                query,
                p);
        }

        /// <summary>
        /// Guardar un departamento.
        /// </summary>
        public ErrorDto Activos_Departamentos_Guardar(
            int CodEmpresa,
            string usuario,
            ActivosDepartamentosData departamento)
        {
            var resp = DbHelper.CreateOkResponse();

            try
            {
                if (departamento == null)
                    return new ErrorDto { Code = -1, Description = "Datos no proporcionados." };

                if (string.IsNullOrWhiteSpace(departamento.cod_departamento))
                    return new ErrorDto { Code = -1, Description = "Debe indicar el código de departamento." };

                if (string.IsNullOrWhiteSpace(departamento.descripcion))
                    return new ErrorDto { Code = -1, Description = "Debe indicar la descripción." };

                if (string.IsNullOrWhiteSpace(departamento.cod_unidad))
                    return new ErrorDto { Code = -1, Description = "Debe indicar la unidad contable." };

                const string queryExiste = @"
                    SELECT COUNT(1)
                    FROM dbo.ACTIVOS_DEPARTAMENTOS
                    WHERE COD_DEPARTAMENTO = @cod;";

                var existeResult = ExecuteCount(
                    CodEmpresa,
                    queryExiste,
                    new { cod = Normalize(departamento.cod_departamento) });

                if (existeResult.Code != 0)
                    return new ErrorDto
                    {
                        Code        = existeResult.Code,
                        Description = existeResult.Description
                    };

                int existe = existeResult.Result;

                if (departamento.isNew)
                {
                    if (existe > 0)
                        return new ErrorDto
                        {
                            Code        = -2,
                            Description = $"El departamento {Normalize(departamento.cod_departamento)} ya existe."
                        };

                    return Activos_Departamentos_Insertar(CodEmpresa, usuario, departamento);
                }

                if (existe == 0)
                    return new ErrorDto
                    {
                        Code        = -2,
                        Description = $"El departamento {Normalize(departamento.cod_departamento)} no existe."
                    };

                return Activos_Departamentos_Actualizar(CodEmpresa, usuario, departamento);
            }
            catch (Exception ex)
            {
                resp.Code        = -1;
                resp.Description = ex.Message;
            }
            return resp;
        }

        private ErrorDto Activos_Departamentos_Insertar(
            int CodEmpresa,
            string usuario,
            ActivosDepartamentosData departamento)
        {
            const string query = @"
                INSERT INTO dbo.ACTIVOS_DEPARTAMENTOS
                   (COD_DEPARTAMENTO, DESCRIPCION, COD_UNIDAD,
                    REGISTRO_FECHA, REGISTRO_USUARIO, MODIFICA_USUARIO, MODIFICA_FECHA)
                VALUES
                   (@cod, @desc, @unidad,
                    SYSDATETIME(), @usr, NULL, NULL);";

            var parameters = new
            {
                cod    = Normalize(departamento.cod_departamento),
                desc   = departamento.descripcion?.ToUpper(),
                unidad = Normalize(departamento.cod_unidad),
                usr    = string.IsNullOrWhiteSpace(usuario) ? null : usuario
            };

            string detalle = $"Departamento: {departamento.cod_departamento} - {departamento.descripcion} / Unidad: {departamento.cod_unidad}";

            return ExecuteNonQueryWithBitacora(
                CodEmpresa,
                usuario,
                query,
                parameters,
                detalle,
                "Registra - WEB",
                "Departamento ingresado satisfactoriamente.");
        }

        private ErrorDto Activos_Departamentos_Actualizar(
            int CodEmpresa,
            string usuario,
            ActivosDepartamentosData departamento)
        {
            const string query = @"
                UPDATE dbo.ACTIVOS_DEPARTAMENTOS
                   SET DESCRIPCION      = @desc,
                       COD_UNIDAD       = @unidad,
                       MODIFICA_USUARIO = @usr,
                       MODIFICA_FECHA   = SYSDATETIME()
                 WHERE COD_DEPARTAMENTO = @cod;";

            var parameters = new
            {
                cod    = Normalize(departamento.cod_departamento),
                desc   = departamento.descripcion?.ToUpper(),
                unidad = Normalize(departamento.cod_unidad),
                usr    = string.IsNullOrWhiteSpace(usuario) ? null : usuario
            };

            string detalle = $"Departamento: {departamento.cod_departamento} - {departamento.descripcion} / Unidad: {departamento.cod_unidad}";

            return ExecuteNonQueryWithBitacora(
                CodEmpresa,
                usuario,
                query,
                parameters,
                detalle,
                "Modifica - WEB",
                "Departamento actualizado satisfactoriamente.");
        }

        public ErrorDto Activos_Departamentos_Eliminar(
            int CodEmpresa,
            string usuario,
            string cod_departamento)
        {
            if (string.IsNullOrWhiteSpace(cod_departamento))
                return new ErrorDto { Code = -1, Description = "Debe indicar el código de departamento." };

            const string query = @"DELETE FROM dbo.ACTIVOS_DEPARTAMENTOS WHERE COD_DEPARTAMENTO = @cod;";

            var parameters = new { cod = Normalize(cod_departamento) };

            string detalle = $"Departamento: {cod_departamento}";

            // No sobreescribo descripción OK: dejo la que venga del DbHelper
            return ExecuteNonQueryWithBitacora(
                CodEmpresa,
                usuario,
                query,
                parameters,
                detalle,
                "Elimina - WEB");
        }

        public ErrorDto Activos_Departamentos_Valida(int CodEmpresa, string cod_departamento)
        {
            var resp = DbHelper.CreateOkResponse();

            try
            {
                const string query = @"
                    SELECT COUNT(1)
                    FROM dbo.ACTIVOS_DEPARTAMENTOS
                    WHERE UPPER(COD_DEPARTAMENTO) = @cod;";

                var dbResp = ExecuteCount(
                    CodEmpresa,
                    query,
                    new { cod = Normalize(cod_departamento) });

                if (dbResp.Code != 0)
                {
                    resp.Code        = dbResp.Code;
                    resp.Description = dbResp.Description;
                    return resp;
                }

                if (dbResp.Result > 0)
                {
                    resp.Code        = -1;
                    resp.Description = "El código de departamento ya existe.";
                }
                else
                {
                    resp.Code        = 0;
                    resp.Description = "El código de departamento es válido.";
                }
            }
            catch (Exception ex)
            {
                resp.Code        = -1;
                resp.Description = ex.Message;
            }
            return resp;
        }

        /// <summary>
        /// Lista paginada (lazy) de secciones. Puede filtrar por cod_departamento.
        /// </summary>
        public ErrorDto<ActivosSeccionesLista> Activos_SeccionesLista_Obtener(
            int CodEmpresa,
            string? cod_departamento,
            FiltrosLazyLoadData filtros)
        {
            var resp = DbHelper.CreateOkResponse(
                new ActivosSeccionesLista
                {
                    total = 0,
                    lista = new List<ActivosSeccionesData>()
                });

            try
            {
                var p = BuildPagedListParameters(
                    filtros,
                    defaultSortField: "cod_seccion",
                    codDepartamento: cod_departamento);

                const string qTotal = @"
                    SELECT COUNT(1)
                    " + SeccSelectBaseSql + @"
                    " + SeccWhereFilterSql + ";";

                const string query = @"
                    SELECT
                        s.COD_DEPARTAMENTO             AS cod_departamento,
                        ISNULL(d.DESCRIPCION,'')       AS departamento_desc,
                        s.COD_SECCION                  AS cod_seccion,
                        ISNULL(s.DESCRIPCION,'')       AS descripcion,
                        ISNULL(s.COD_CENTRO_COSTO,'')  AS cod_centro_costo,
                        ISNULL(cc.DESCRIPCION,'')      AS centro_costo_desc,
                        ISNULL(s.REGISTRO_USUARIO,'')  AS usuario
                    " + SeccSelectBaseSql + @"
                    " + SeccWhereFilterSql + @"
                    ORDER BY
                        -- DESC
                        CASE WHEN @sortOrder = 0 AND @sortField = 'cod_departamento'   THEN s.COD_DEPARTAMENTO   END DESC,
                        CASE WHEN @sortOrder = 0 AND @sortField = 'departamento_desc'  THEN d.DESCRIPCION        END DESC,
                        CASE WHEN @sortOrder = 0 AND @sortField = 'cod_seccion'        THEN s.COD_SECCION        END DESC,
                        CASE WHEN @sortOrder = 0 AND @sortField = 'descripcion'        THEN s.DESCRIPCION        END DESC,
                        CASE WHEN @sortOrder = 0 AND @sortField = 'cod_centro_costo'   THEN s.COD_CENTRO_COSTO   END DESC,
                        CASE WHEN @sortOrder = 0 AND @sortField = 'centro_costo_desc'  THEN cc.DESCRIPCION       END DESC,
                        CASE WHEN @sortOrder = 0 AND @sortField = 'usuario'            THEN s.REGISTRO_USUARIO   END DESC,

                        -- ASC
                        CASE WHEN @sortOrder = 1 AND @sortField = 'cod_departamento'   THEN s.COD_DEPARTAMENTO   END ASC,
                        CASE WHEN @sortOrder = 1 AND @sortField = 'departamento_desc'  THEN d.DESCRIPCION        END ASC,
                        CASE WHEN @sortOrder = 1 AND @sortField = 'cod_seccion'        THEN s.COD_SECCION        END ASC,
                        CASE WHEN @sortOrder = 1 AND @sortField = 'descripcion'        THEN s.DESCRIPCION        END ASC,
                        CASE WHEN @sortOrder = 1 AND @sortField = 'cod_centro_costo'   THEN s.COD_CENTRO_COSTO   END ASC,
                        CASE WHEN @sortOrder = 1 AND @sortField = 'centro_costo_desc'  THEN cc.DESCRIPCION       END ASC,
                        CASE WHEN @sortOrder = 1 AND @sortField = 'usuario'            THEN s.REGISTRO_USUARIO   END ASC
                    OFFSET @offset ROWS
                    FETCH NEXT @rows ROWS ONLY;";

                using var cn = _portalDB.CreateConnection(CodEmpresa);

                if (resp.Result != null)
                {
                    resp.Result.total = cn.QueryFirstOrDefault<int>(qTotal, p);
                    resp.Result.lista = cn.Query<ActivosSeccionesData>(query, p).ToList();
                }
            }
            catch (Exception ex)
            {
                resp.Code        = -1;
                resp.Description = ex.Message;
                if (resp.Result != null)
                {
                    resp.Result.total = 0;
                    resp.Result.lista = [];
                }
            }
            return resp;
        }

        /// <summary>
        /// Lista completa de secciones (sin paginar) para exportar. Puede filtrar por cod_departamento.
        /// </summary>
        public ErrorDto<List<ActivosSeccionesData>> Activos_Secciones_Obtener(
            int CodEmpresa,
            string? cod_departamento,
            FiltrosLazyLoadData filtros)
        {
            var p = new DynamicParameters();

            string? dept = string.IsNullOrWhiteSpace(cod_departamento)
                ? null
                : Normalize(cod_departamento);
            p.Add("@dept", dept, DbType.String);

            p.Add(_filtro, BuildFiltroLike(filtros), DbType.String);

            string query = @"
                SELECT
                    s.COD_DEPARTAMENTO             AS cod_departamento,
                    ISNULL(d.DESCRIPCION,'')       AS departamento_desc,
                    s.COD_SECCION                  AS cod_seccion,
                    ISNULL(s.DESCRIPCION,'')       AS descripcion,
                    ISNULL(s.COD_CENTRO_COSTO,'')  AS cod_centro_costo,
                    ISNULL(cc.DESCRIPCION,'')      AS centro_costo_desc,
                    ISNULL(s.REGISTRO_USUARIO,'')  AS usuario
                " + SeccSelectBaseSql + @"
                " + SeccWhereFilterSql + @"
                ORDER BY s.COD_SECCION;";

            return DbHelper.ExecuteListQuery<ActivosSeccionesData>(
                _portalDB,
                CodEmpresa,
                query,
                p);
        }

        public ErrorDto Activos_Secciones_Guardar(
            int CodEmpresa,
            string usuario,
            ActivosSeccionesData seccion)
        {
            var resp = DbHelper.CreateOkResponse();

            try
            {
                if (seccion == null)
                    return new ErrorDto { Code = -1, Description = "Datos no proporcionados." };
                if (string.IsNullOrWhiteSpace(seccion.cod_departamento))
                    return new ErrorDto { Code = -1, Description = "Debe indicar el código de departamento." };
                if (string.IsNullOrWhiteSpace(seccion.cod_seccion))
                    return new ErrorDto { Code = -1, Description = "Debe indicar el código de sección." };
                if (string.IsNullOrWhiteSpace(seccion.descripcion))
                    return new ErrorDto { Code = -1, Description = "Debe indicar la descripción." };

                const string qExiste = @"
                    SELECT COUNT(1)
                    FROM dbo.ACTIVOS_SECCIONES
                    WHERE COD_DEPARTAMENTO = @dept AND COD_SECCION = @sec;";

                var existeResult = ExecuteCount(
                    CodEmpresa,
                    qExiste,
                    new
                    {
                        dept = Normalize(seccion.cod_departamento),
                        sec  = Normalize(seccion.cod_seccion)
                    });

                if (existeResult.Code != 0)
                {
                    return new ErrorDto
                    {
                        Code        = existeResult.Code,
                        Description = existeResult.Description
                    };
                }

                int existe = existeResult.Result;

                if (seccion.isNew)
                {
                    if (existe > 0)
                        return new ErrorDto
                        {
                            Code        = -2,
                            Description = $"La sección {Normalize(seccion.cod_seccion)} ya existe para el departamento {Normalize(seccion.cod_departamento)}."
                        };

                    resp = Activos_Secciones_Insertar(CodEmpresa, usuario, seccion);
                }
                else
                {
                    if (existe == 0)
                        return new ErrorDto
                        {
                            Code        = -2,
                            Description = $"La sección {Normalize(seccion.cod_seccion)} no existe en el departamento {Normalize(seccion.cod_departamento)}."
                        };

                    resp = Activos_Secciones_Actualizar(CodEmpresa, usuario, seccion);
                }
            }
            catch (Exception ex)
            {
                resp.Code        = -1;
                resp.Description = ex.Message;
            }
            return resp;
        }

        private ErrorDto Activos_Secciones_Insertar(
            int CodEmpresa,
            string usuario,
            ActivosSeccionesData seccion)
        {
            const string query = @"
                INSERT INTO dbo.ACTIVOS_SECCIONES
                    (COD_DEPARTAMENTO, COD_SECCION, DESCRIPCION, COD_CENTRO_COSTO,
                     REGISTRO_USUARIO, REGISTRO_FECHA, MODIFICA_USUARIO, MODIFICA_FECHA)
                VALUES
                    (@dept, @sec, @desc, @cc, @usr, SYSDATETIME(), NULL, NULL);";

            var parameters = new
            {
                dept = Normalize(seccion.cod_departamento),
                sec  = Normalize(seccion.cod_seccion),
                desc = seccion.descripcion?.ToUpper(),
                cc   = string.IsNullOrWhiteSpace(seccion.cod_centro_costo)
                        ? null
                        : Normalize(seccion.cod_centro_costo),
                usr  = string.IsNullOrWhiteSpace(usuario) ? null : usuario
            };

            string detalle = $"Sección: {seccion.cod_seccion} - Departamento: {seccion.cod_departamento} - {seccion.descripcion}";

            return ExecuteNonQueryWithBitacora(
                CodEmpresa,
                usuario,
                query,
                parameters,
                detalle,
                "Registra - WEB",
                "Sección ingresada satisfactoriamente.");
        }

        private ErrorDto Activos_Secciones_Actualizar(
            int CodEmpresa,
            string usuario,
            ActivosSeccionesData seccion)
        {
            const string query = @"
                UPDATE dbo.ACTIVOS_SECCIONES
                   SET DESCRIPCION      = @desc,
                       COD_CENTRO_COSTO = @cc,
                       MODIFICA_USUARIO = @usr,
                       MODIFICA_FECHA   = SYSDATETIME()
                 WHERE COD_DEPARTAMENTO = @dept
                   AND COD_SECCION      = @sec;";

            var parameters = new
            {
                dept = Normalize(seccion.cod_departamento),
                sec  = Normalize(seccion.cod_seccion),
                desc = seccion.descripcion?.ToUpper(),
                cc   = string.IsNullOrWhiteSpace(seccion.cod_centro_costo)
                        ? null
                        : Normalize(seccion.cod_centro_costo),
                usr  = string.IsNullOrWhiteSpace(usuario) ? null : usuario
            };

            string detalle = $"Sección: {seccion.cod_seccion} - Departamento: {seccion.cod_departamento} - {seccion.descripcion}";

            return ExecuteNonQueryWithBitacora(
                CodEmpresa,
                usuario,
                query,
                parameters,
                detalle,
                "Modifica - WEB",
                "Sección actualizada satisfactoriamente.");
        }

        public ErrorDto Activos_Secciones_Eliminar(
            int CodEmpresa,
            string usuario,
            string cod_departamento,
            string cod_seccion)
        {
            if (string.IsNullOrWhiteSpace(cod_departamento) ||
                string.IsNullOrWhiteSpace(cod_seccion))
            {
                return new ErrorDto
                {
                    Code        = -1,
                    Description = "Debe indicar departamento y sección."
                };
            }

            const string query = @"
                DELETE FROM dbo.ACTIVOS_SECCIONES
                WHERE COD_DEPARTAMENTO = @dept AND COD_SECCION = @sec;";

            var parameters = new
            {
                dept = Normalize(cod_departamento),
                sec  = Normalize(cod_seccion)
            };

            string detalle = $"Sección: {cod_seccion} - Dept: {cod_departamento}";

            return ExecuteNonQueryWithBitacora(
                CodEmpresa,
                usuario,
                query,
                parameters,
                detalle,
                "Elimina - WEB");
        }

        public ErrorDto Activos_Secciones_Valida(
            int CodEmpresa,
            string cod_departamento,
            string cod_seccion)
        {
            var resp = DbHelper.CreateOkResponse();

            try
            {
                const string query = @"
                    SELECT COUNT(1)
                    FROM dbo.ACTIVOS_SECCIONES
                    WHERE UPPER(COD_DEPARTAMENTO) = @dept
                      AND UPPER(COD_SECCION)      = @sec;";

                var dbResp = ExecuteCount(
                    CodEmpresa,
                    query,
                    new
                    {
                        dept = Normalize(cod_departamento),
                        sec  = Normalize(cod_seccion)
                    });

                if (dbResp.Code != 0)
                {
                    resp.Code        = dbResp.Code;
                    resp.Description = dbResp.Description;
                    return resp;
                }

                if (dbResp.Result > 0)
                {
                    resp.Code        = -1;
                    resp.Description = "La sección ya existe para este departamento.";
                }
                else
                {
                    resp.Code        = 0;
                    resp.Description = "La sección es válida.";
                }
            }
            catch (Exception ex)
            {
                resp.Code        = -1;
                resp.Description = ex.Message;
            }
            return resp;
        }

        public ErrorDto<List<DropDownListaGenericaModel>> Activos_Secciones_CentrosCostos_Obtener(
            int CodEmpresa,
            int contabilidad)
        {
            const string query = @"
                SELECT COD_CENTRO_COSTO AS item, DESCRIPCION
                FROM dbo.CNTX_CENTRO_COSTOS
                WHERE COD_CONTABILIDAD = @contabilidad AND ACTIVO = 1
                ORDER BY DESCRIPCION ASC;";

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                _portalDB,
                CodEmpresa,
                query,
                new { contabilidad });
        }

        public ErrorDto<List<DropDownListaGenericaModel>> Activos_Departamentos_Unidades_Obtener(
            int CodEmpresa,
            int contabilidad)
        {
            const string query = @"
                SELECT COD_UNIDAD AS item, DESCRIPCION
                FROM dbo.CNTX_UNIDADES
                WHERE COD_CONTABILIDAD = @contabilidad AND ACTIVA = 1
                ORDER BY DESCRIPCION ASC;";

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                _portalDB,
                CodEmpresa,
                query,
                new { contabilidad });
        }

        public ErrorDto<List<DropDownListaGenericaModel>> Activos_Departamentos_Dropdown_Obtener(
            int CodEmpresa)
        {
            const string query = @"
                SELECT d.COD_DEPARTAMENTO AS item, d.DESCRIPCION
                FROM dbo.ACTIVOS_DEPARTAMENTOS d
                ORDER BY d.DESCRIPCION ASC;";

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                _portalDB,
                CodEmpresa,
                query);
        }
    }
}