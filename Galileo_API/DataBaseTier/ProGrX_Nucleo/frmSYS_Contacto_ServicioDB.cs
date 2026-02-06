using Dapper;
using Microsoft.Data.SqlClient;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Nucleo;

namespace Galileo.DataBaseTier.ProGrX_Nucleo
{
    public class FrmSysContactoServicioDB
    {
        private readonly IConfiguration _config;
        private const string BaseConnStringName = "BaseConnString";
        public FrmSysContactoServicioDB(IConfiguration config)
        {
            _config = config;
        }

        private string GetSafeConnectionString()
        {
            var connStr = _config.GetConnectionString(BaseConnStringName);
            if (string.IsNullOrWhiteSpace(connStr))
                throw new InvalidOperationException($"Connection string '{BaseConnStringName}' is not configured.");
            return connStr;
        }


        /// <summary>
        /// Lista: devuelve General con paginación, orden y búsqueda.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="identificacion"></param>
        /// <param name="codPais"></param>
        /// /// <returns></returns>
        public ErrorDto<SysContactoServicioGeneralData>SysContactoServicio_General_Obtener(int CodEmpresa, string identificacion, string codPais = "CRC")
        {
            var dto = new ErrorDto<SysContactoServicioGeneralData>
            {
                Code = 0,
                Description = "Ok",
                Result = null
            };

            try
            {
                string connStr = GetSafeConnectionString();
                using var conn = new SqlConnection(connStr);

                var sql = @"
                SELECT
                    -- Identificación y encabezado
                    P.IDENTIFICACION                                 AS Identificacion,
                    P.COD_PAIS                                       AS CodPais,
                    P.TIPO_ID                                        AS Tipo_Id,
                    P.APELLIDO_1                                     AS Apellido_1,
                    P.APELLIDO_2                                     AS Apellido_2,
                    P.NOMBRE                                         AS Nombre,

                    -- Fechas y edad
                    P.FECHA_NACIMIENTO                               AS Fecha_Nacimiento,
                    P.FECHA_CADUCIDAD                                AS Fecha_Caducidad,
                    CASE
                        WHEN P.FECHA_NACIMIENTO IS NULL THEN NULL
                        ELSE
                            DATEDIFF(YEAR, P.FECHA_NACIMIENTO, GETDATE()) -
                            CASE WHEN DATEADD(YEAR, DATEDIFF(YEAR, P.FECHA_NACIMIENTO, GETDATE()), P.FECHA_NACIMIENTO) > CAST(GETDATE() AS DATE)
                                 THEN 1 ELSE 0 END
                    END                                              AS Edad,

                    -- Datos generales
                    P.SEXO                                           AS Sexo,
                    P.ESTADO_CIVIL                                   AS Estado_Civil,
                    P.PROFESION                                      AS Profesion,

                    -- Económicos / contacto
                    P.SALARIO                                        AS Salario,
                    P.EMAIL_01                                       AS Email_01,
                    P.EMAIL_02                                       AS Email_02,
                    P.EMAIL_03                                       AS Email_03,

                    -- Códigos de geografía y dirección
                    P.COD_PROVINCIA                                  AS Cod_Provincia,
                    P.COD_CANTON                                     AS Cod_Canton,
                    P.COD_DISTRITO                                   AS Cod_Distrito,
                    P.DIRECCION                                      AS Direccion,

                    -- Descripciones (sin JOIN, por subconsulta)
                    (SELECT PR.DESCRIPCION
                       FROM dbo.SYS_PROVINCIAS PR WITH (NOLOCK)
                      WHERE PR.COD_PAIS = P.COD_PAIS AND PR.COD_PROVINCIA = P.COD_PROVINCIA) AS Provincia,

                    (SELECT CA.DESCRIPCION
                       FROM dbo.SYS_CANTONES CA WITH (NOLOCK)
                      WHERE CA.COD_PAIS = P.COD_PAIS AND CA.COD_PROVINCIA = P.COD_PROVINCIA AND CA.COD_CANTON = P.COD_CANTON) AS Canton,

                    (SELECT DI.DESCRIPCION
                       FROM dbo.SYS_DISTRITOS DI WITH (NOLOCK)
                      WHERE DI.COD_PAIS = P.COD_PAIS AND DI.COD_PROVINCIA = P.COD_PROVINCIA AND DI.COD_CANTON = P.COD_CANTON AND DI.COD_DISTRITO = P.COD_DISTRITO) AS Distrito

                FROM dbo.SYS_PADRON P WITH (NOLOCK)
                WHERE UPPER(P.IDENTIFICACION) = @identificacion AND UPPER(P.COD_PAIS) = @codPais;";

                var r = conn.QueryFirstOrDefault<SysContactoServicioGeneralData>(sql, new
                {
                    identificacion = (identificacion ?? "").Trim().ToUpper(),
                    codPais = (codPais ?? "CRC").Trim().ToUpper()
                });

                if (r == null)
                {
                    dto.Code = -2;
                    dto.Description = "No existe la identificación en SYS_PADRON.";
                    return dto;
                }

                dto.Result = r;
            }
            catch (Exception ex)
            {
                dto.Code = -1;
                dto.Description = ex.Message;
                dto.Result = null;
            }

            return dto;
        }


        /// <summary>
        ///Lista: devuelve General sin paginación, orden y búsqueda y sin lazy loading.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="identificacion"></param>
        /// <param name="codPais"></param>
        /// <param name="filtros"></param>
        /// /// <returns></returns>
        public ErrorDto<List<SysContactoServicioGeneralData>>SysContactoServicio_Obtener(int CodEmpresa, string identificacion, string codPais, FiltrosLazyLoadData filtros)
        {
            var result = new ErrorDto<List<SysContactoServicioGeneralData>>
            { Code = 0, Description = "Ok", Result = new() };

            try
            {
                using var connection = new SqlConnection(GetSafeConnectionString());

                var rawQuery = (filtros?.filtro ?? string.Empty).Trim();
                var query = string.IsNullOrWhiteSpace(rawQuery) ? null : rawQuery;

                var sortField = (filtros?.sortField ?? string.Empty).Trim().ToLowerInvariant();
                var sortOrder = filtros?.sortOrder ?? 1; // 0=DESC, 1=ASC

                var identParam = string.IsNullOrWhiteSpace(identificacion) ? null : identificacion.Trim().ToUpper();
                var countryCode = (codPais ?? "CRC").Trim().ToUpper();

                var looksLikeId = query != null && query.All(ch => char.IsDigit(ch) || ch == '-' || ch == ' ');
                var queryPrefix = query == null ? null : query + "%";

                const string sql = @"
                        SELECT
                            P.IDENTIFICACION                               AS Identificacion,
                            P.COD_PAIS                                     AS CodPais,
                            P.TIPO_ID                                      AS Tipo_Id,
                            P.APELLIDO_1                                   AS Apellido_1,
                            P.APELLIDO_2                                   AS Apellido_2,
                            P.NOMBRE                                       AS Nombre,
                            P.FECHA_NACIMIENTO                             AS Fecha_Nacimiento,
                            P.FECHA_CADUCIDAD                              AS Fecha_Caducidad,
                            P.SEXO                                         AS Sexo,
                            P.ESTADO_CIVIL                                 AS Estado_Civil,
                            P.PROFESION                                    AS Profesion,
                            P.SALARIO                                      AS Salario,
                            P.EMAIL_01                                     AS Email_01,
                            P.EMAIL_02                                     AS Email_02,
                            P.EMAIL_03                                     AS Email_03,
                            P.COD_PROVINCIA                                AS Cod_Provincia,
                            P.COD_CANTON                                   AS Cod_Canton,
                            P.COD_DISTRITO                                 AS Cod_Distrito,
                            P.DIRECCION                                    AS Direccion,
                            PR.NOMBRE                                      AS Provincia,
                            CA.NOMBRE                                      AS Canton,
                            DI.NOMBRE                                      AS Distrito,
                            P.REGISTRO_USUARIO                             AS Registro_Usuario,
                            P.REGISTRO_FECHA                               AS Registro_Fecha,
                            P.CODIGO_ELECTORAL                             AS Codigo_Electoral
                        FROM dbo.SYS_PADRON P WITH (NOLOCK)
                        LEFT JOIN dbo.SYS_PROVINCIAS PR ON PR.COD_PAIS = P.COD_PAIS AND PR.COD_PROVINCIA = P.COD_PROVINCIA
                        LEFT JOIN dbo.SYS_CANTONES   CA ON CA.COD_PAIS = P.COD_PAIS AND CA.COD_PROVINCIA = P.COD_PROVINCIA AND CA.COD_CANTON = P.COD_CANTON
                        LEFT JOIN dbo.SYS_DISTRITOS  DI ON DI.COD_PAIS = P.COD_PAIS AND DI.COD_PROVINCIA = P.COD_PROVINCIA AND DI.COD_CANTON = P.COD_CANTON AND DI.COD_DISTRITO = P.COD_DISTRITO
                        WHERE UPPER(P.COD_PAIS) = @countryCode
                          AND (@ident IS NULL OR UPPER(P.IDENTIFICACION) = @ident)
                          AND (
                                @query IS NULL
                                OR (
                                     @looksLikeId = 1 AND P.IDENTIFICACION LIKE @queryPrefix
                                   )
                                OR (
                                     @looksLikeId = 0 AND (
                                            P.IDENTIFICACION LIKE '%' + @query + '%'
                                         OR P.NOMBRE         LIKE '%' + @query + '%'
                                         OR P.APELLIDO_1     LIKE '%' + @query + '%'
                                         OR P.APELLIDO_2     LIKE '%' + @query + '%'
                                     )
                                   )
                              )
                        ORDER BY
                            -- ASC
                            CASE WHEN @sortOrder = 1 AND @sortField = 'identificacion' THEN P.IDENTIFICACION END ASC,
                            CASE WHEN @sortOrder = 1 AND @sortField = 'apellido_1' THEN P.APELLIDO_1 END ASC,
                            CASE WHEN @sortOrder = 1 AND @sortField = 'apellido_2' THEN P.APELLIDO_2 END ASC,
                            CASE WHEN @sortOrder = 1 AND @sortField = 'nombre' THEN P.NOMBRE END ASC,
                            CASE WHEN @sortOrder = 1 AND @sortField = 'registro_fecha' THEN P.REGISTRO_FECHA END ASC,

                            -- DESC
                            CASE WHEN @sortOrder = 0 AND @sortField = 'identificacion' THEN P.IDENTIFICACION END DESC,
                            CASE WHEN @sortOrder = 0 AND @sortField = 'apellido_1' THEN P.APELLIDO_1 END DESC,
                            CASE WHEN @sortOrder = 0 AND @sortField = 'apellido_2' THEN P.APELLIDO_2 END DESC,
                            CASE WHEN @sortOrder = 0 AND @sortField = 'nombre' THEN P.NOMBRE END DESC,
                            CASE WHEN @sortOrder = 0 AND @sortField = 'registro_fecha' THEN P.REGISTRO_FECHA END DESC,

                            -- Fallback (equivalente al orden default anterior)
                            P.APELLIDO_1 ASC, P.APELLIDO_2 ASC, P.NOMBRE ASC;";

                result.Result = connection.Query<SysContactoServicioGeneralData>(sql, new
                {
                    ident = identParam,
                    countryCode,
                    query,
                    queryPrefix,
                    looksLikeId = looksLikeId ? 1 : 0,
                    sortField,
                    sortOrder
                }).ToList();
            }
            catch (Exception ex)
            {
                result.Code = -1; result.Description = ex.Message; result.Result = null;
            }
            return result;
        }


        /// <summary>
        /// Lista de teléfonos con paginación / filtro / sort.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="identificacion"></param>
        /// <param name="codPais"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDto<SysContactoServicioTelefonoLista>SysContactoServicio_Telefonos_Lista_Obtener(int CodEmpresa, string identificacion, string codPais, FiltrosLazyLoadData filtros)
        {
            var dto = new ErrorDto<SysContactoServicioTelefonoLista>
            { Code = 0, Description = "Ok", Result = new() { total = 0, lista = new() } };

            try
            {
                using var connection = new SqlConnection(_config.GetConnectionString(BaseConnStringName));

                var search = (filtros?.filtro ?? string.Empty).Trim();
                string? searchLike = string.IsNullOrWhiteSpace(search) ? null : $"%{search}%";

                int page = Math.Max(0, filtros?.pagina ?? 0);
                int pageSize = Math.Clamp(filtros?.paginacion ?? 30, 1, 200);

                var sortField = (filtros?.sortField ?? string.Empty).Trim().ToLowerInvariant();
                var sortOrder = filtros?.sortOrder ?? 1; // 0=DESC, 1=ASC

                var ident = (identificacion ?? string.Empty).Trim().ToUpper();
                var countryCode = (codPais ?? "CRC").Trim().ToUpper();

                const string totalSql = @"SELECT COUNT(1)
                                         FROM dbo.SYS_PADRON_TELEFONOS T WITH (NOLOCK)
                                         WHERE UPPER(T.IDENTIFICACION) = @ident
                                           AND UPPER(T.COD_PAIS) = @countryCode
                                           AND (@search IS NULL
                                                OR T.TELEFONO_TIPO LIKE @search
                                                OR T.TELEFONO      LIKE @search
                                                OR T.EXTENSION     LIKE @search
                                                OR T.ATIENDE       LIKE @search);";

                dto.Result.total = connection.QueryFirstOrDefault<int>(totalSql, new
                {
                    ident,
                    countryCode,
                    search = searchLike
                });

                const string sql = @"
                    SELECT 
                        T.NUM_LINEA          AS Num_Linea,
                        @ident               AS Identificacion,
                        @countryCode         AS CodPais,
                        T.TELEFONO_TIPO      AS Telefono_Tipo,
                        T.TELEFONO           AS Telefono,
                        T.EXTENSION          AS Extension,
                        T.ATIENDE            AS Atiende,
                        CAST(0 AS BIT)       AS isNew
                    FROM dbo.SYS_PADRON_TELEFONOS T WITH (NOLOCK)
                    WHERE UPPER(T.IDENTIFICACION) = @ident
                      AND UPPER(T.COD_PAIS) = @countryCode
                      AND (@search IS NULL
                           OR T.TELEFONO_TIPO LIKE @search
                           OR T.TELEFONO      LIKE @search
                           OR T.EXTENSION     LIKE @search
                           OR T.ATIENDE       LIKE @search)
                    ORDER BY
                        -- ASC
                        CASE WHEN @sortOrder = 1 AND @sortField = 'num_linea' THEN T.NUM_LINEA END ASC,
                        CASE WHEN @sortOrder = 1 AND @sortField = 'telefono_tipo' THEN T.TELEFONO_TIPO END ASC,
                        CASE WHEN @sortOrder = 1 AND @sortField = 'telefono' THEN T.TELEFONO END ASC,
                        CASE WHEN @sortOrder = 1 AND @sortField = 'extension' THEN T.EXTENSION END ASC,
                        CASE WHEN @sortOrder = 1 AND @sortField = 'atiende' THEN T.ATIENDE END ASC,

                        -- DESC
                        CASE WHEN @sortOrder = 0 AND @sortField = 'num_linea' THEN T.NUM_LINEA END DESC,
                        CASE WHEN @sortOrder = 0 AND @sortField = 'telefono_tipo' THEN T.TELEFONO_TIPO END DESC,
                        CASE WHEN @sortOrder = 0 AND @sortField = 'telefono' THEN T.TELEFONO END DESC,
                        CASE WHEN @sortOrder = 0 AND @sortField = 'extension' THEN T.EXTENSION END DESC,
                        CASE WHEN @sortOrder = 0 AND @sortField = 'atiende' THEN T.ATIENDE END DESC,

                        T.NUM_LINEA ASC
                    OFFSET @page ROWS FETCH NEXT @pageSize ROWS ONLY;";

                dto.Result.lista = connection.Query<SysContactoServicioTelefonoData>(sql, new
                {
                    ident,
                    countryCode,
                    search = searchLike,
                    page,
                    pageSize,
                    sortField,
                    sortOrder
                }).ToList();
            }
            catch (Exception ex)
            {
                dto.Code = -1; dto.Description = ex.Message; dto.Result.lista = new List<SysContactoServicioTelefonoData>(); dto.Result.total = 0;
            }
            return dto;
        }


        /// <summary>
        /// Lista de teléfonos para exportar y obtener informacion.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="identificacion"></param>
        /// <param name="codPais"></param>
        /// <returns></returns>
        public ErrorDto<List<SysContactoServicioTelefonoData>>SysContactoServicio_Telefonos_Obtener(int CodEmpresa, string identificacion, string codPais)
        {
            var dto = new ErrorDto<List<SysContactoServicioTelefonoData>>
            {
                Code = 0,
                Description = "Ok",
                Result = new()
            };

            try
            {
                using var connection = new SqlConnection(_config.GetConnectionString(BaseConnStringName));

                const string sql = @"
                SELECT
                    T.NUM_LINEA          AS Num_Linea,
                    T.IDENTIFICACION     AS Identificacion,
                    T.COD_PAIS           AS CodPais,
                    T.TELEFONO_TIPO      AS Telefono_Tipo,
                    T.TELEFONO           AS Telefono,
                    T.EXTENSION          AS Extension,
                    T.ATIENDE            AS Atiende
                FROM dbo.SYS_PADRON_TELEFONOS AS T WITH (NOLOCK)
                WHERE UPPER(T.IDENTIFICACION) = @ident
                  AND UPPER(T.COD_PAIS) = @countryCode
                ORDER BY T.NUM_LINEA;";

                dto.Result = connection.Query<SysContactoServicioTelefonoData>(sql, new
                {
                    ident = (identificacion ?? "").Trim().ToUpper(),
                    countryCode = (codPais ?? "CRC").Trim().ToUpper()
                }).ToList();
            }
            catch (Exception ex)
            {
                dto.Code = -1;
                dto.Description = ex.Message;
                dto.Result = null;
            }
            return dto;
        }


        /// <summary>
        /// Lista de direcciones con paginación / filtro / sort (para tabla).
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="identificacion"></param>
        /// <param name="codPais"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDto<SysContactoServicioDireccionLista>SysContactoServicio_Direcciones_Lista_Obtener(int CodEmpresa, string identificacion, string codPais, FiltrosLazyLoadData filtros)
        {
            var dto = new ErrorDto<SysContactoServicioDireccionLista>
            { Code = 0, Description = "Ok", Result = new() { total = 0, lista = new() } };

            try
            {
                using var connection = new SqlConnection(_config.GetConnectionString(BaseConnStringName));

                var search = (filtros?.filtro ?? string.Empty).Trim();
                string? searchLike = string.IsNullOrWhiteSpace(search) ? null : $"%{search}%";

                int page = Math.Max(0, filtros?.pagina ?? 0);
                int pageSize = Math.Clamp(filtros?.paginacion ?? 30, 1, 200);

                var sortField = (filtros?.sortField ?? string.Empty).Trim().ToLowerInvariant();
                var sortOrder = filtros?.sortOrder ?? 1; // 0=DESC, 1=ASC

                var ident = (identificacion ?? string.Empty).Trim().ToUpper();
                var countryCode = (codPais ?? "CRC").Trim().ToUpper();

                const string totalSql = @"
                    SELECT COUNT(1)
                    FROM dbo.SYS_PADRON_DIRECCIONES D WITH (NOLOCK)
                    LEFT JOIN dbo.SYS_PROVINCIAS PR ON PR.COD_PAIS = D.COD_PAIS AND PR.COD_PROVINCIA = D.COD_PROVINCIA
                    LEFT JOIN dbo.SYS_CANTONES   CA ON CA.COD_PAIS = D.COD_PAIS AND CA.COD_PROVINCIA = D.COD_PROVINCIA AND CA.COD_CANTON = D.COD_CANTON
                    LEFT JOIN dbo.SYS_DISTRITOS  DI ON DI.COD_PAIS = D.COD_PAIS AND DI.COD_PROVINCIA = D.COD_PROVINCIA AND DI.COD_CANTON = D.COD_CANTON AND DI.COD_DISTRITO = D.COD_DISTRITO
                    WHERE UPPER(D.IDENTIFICACION) = @ident
                      AND UPPER(D.COD_PAIS) = @countryCode
                      AND (@search IS NULL
                           OR D.DIRECCION LIKE @search
                           OR PR.NOMBRE   LIKE @search
                           OR CA.NOMBRE   LIKE @search
                           OR DI.NOMBRE   LIKE @search
                           OR CAST(D.COD_PROVINCIA AS VARCHAR(5)) LIKE @search
                           OR CAST(D.COD_CANTON    AS VARCHAR(5)) LIKE @search
                           OR CAST(D.COD_DISTRITO  AS VARCHAR(5)) LIKE @search);";

                dto.Result.total = connection.QueryFirstOrDefault<int>(totalSql, new
                {
                    ident,
                    countryCode,
                    search = searchLike
                });

                const string sql = @"
                    SELECT 
                        D.NUM_LINEA                                 AS Num_Linea,
                        @ident                                      AS Identificacion,
                        @countryCode                                AS CodPais,
                        D.COD_PROVINCIA                             AS Cod_Provincia,
                        D.COD_CANTON                                AS Cod_Canton,
                        D.COD_DISTRITO                              AS Cod_Distrito,
                        COALESCE(PR.NOMBRE, '')                     AS Provincia,
                        COALESCE(CA.NOMBRE, '')                     AS Canton,
                        COALESCE(DI.NOMBRE, '')                     AS Distrito,
                        D.DIRECCION                                 AS Direccion,
                        CAST(0 AS BIT)                              AS isNew
                    FROM dbo.SYS_PADRON_DIRECCIONES D WITH (NOLOCK)
                    LEFT JOIN dbo.SYS_PROVINCIAS PR ON PR.COD_PAIS = D.COD_PAIS AND PR.COD_PROVINCIA = D.COD_PROVINCIA
                    LEFT JOIN dbo.SYS_CANTONES   CA ON CA.COD_PAIS = D.COD_PAIS AND CA.COD_PROVINCIA = D.COD_PROVINCIA AND CA.COD_CANTON = D.COD_CANTON
                    LEFT JOIN dbo.SYS_DISTRITOS  DI ON DI.COD_PAIS = D.COD_PAIS AND DI.COD_PROVINCIA = D.COD_PROVINCIA AND DI.COD_CANTON = D.COD_CANTON AND DI.COD_DISTRITO = D.COD_DISTRITO
                    WHERE UPPER(D.IDENTIFICACION) = @ident
                      AND UPPER(D.COD_PAIS) = @countryCode
                      AND (@search IS NULL
                           OR D.DIRECCION LIKE @search
                           OR PR.NOMBRE   LIKE @search
                           OR CA.NOMBRE   LIKE @search
                           OR DI.NOMBRE   LIKE @search
                           OR CAST(D.COD_PROVINCIA AS VARCHAR(5)) LIKE @search
                           OR CAST(D.COD_CANTON    AS VARCHAR(5)) LIKE @search
                           OR CAST(D.COD_DISTRITO  AS VARCHAR(5)) LIKE @search)
                    ORDER BY
                        -- ASC
                        CASE WHEN @sortOrder = 1 AND @sortField = 'num_linea' THEN D.NUM_LINEA END ASC,
                        CASE WHEN @sortOrder = 1 AND @sortField = 'cod_provincia' THEN D.COD_PROVINCIA END ASC,
                        CASE WHEN @sortOrder = 1 AND @sortField = 'cod_canton' THEN D.COD_CANTON END ASC,
                        CASE WHEN @sortOrder = 1 AND @sortField = 'cod_distrito' THEN D.COD_DISTRITO END ASC,
                        CASE WHEN @sortOrder = 1 AND @sortField = 'provincia' THEN PR.NOMBRE END ASC,
                        CASE WHEN @sortOrder = 1 AND @sortField = 'canton' THEN CA.NOMBRE END ASC,
                        CASE WHEN @sortOrder = 1 AND @sortField = 'distrito' THEN DI.NOMBRE END ASC,
                        CASE WHEN @sortOrder = 1 AND @sortField = 'direccion' THEN D.DIRECCION END ASC,

                        -- DESC
                        CASE WHEN @sortOrder = 0 AND @sortField = 'num_linea' THEN D.NUM_LINEA END DESC,
                        CASE WHEN @sortOrder = 0 AND @sortField = 'cod_provincia' THEN D.COD_PROVINCIA END DESC,
                        CASE WHEN @sortOrder = 0 AND @sortField = 'cod_canton' THEN D.COD_CANTON END DESC,
                        CASE WHEN @sortOrder = 0 AND @sortField = 'cod_distrito' THEN D.COD_DISTRITO END DESC,
                        CASE WHEN @sortOrder = 0 AND @sortField = 'provincia' THEN PR.NOMBRE END DESC,
                        CASE WHEN @sortOrder = 0 AND @sortField = 'canton' THEN CA.NOMBRE END DESC,
                        CASE WHEN @sortOrder = 0 AND @sortField = 'distrito' THEN DI.NOMBRE END DESC,
                        CASE WHEN @sortOrder = 0 AND @sortField = 'direccion' THEN D.DIRECCION END DESC,

                        D.COD_PROVINCIA ASC, D.COD_CANTON ASC, D.COD_DISTRITO ASC, D.NUM_LINEA ASC
                    OFFSET @page ROWS FETCH NEXT @pageSize ROWS ONLY;";

                dto.Result.lista = connection.Query<SysContactoServicioDireccionData>(sql, new
                {
                    ident,
                    countryCode,
                    search = searchLike,
                    page,
                    pageSize,
                    sortField,
                    sortOrder
                }).ToList();
            }
            catch (Exception ex)
            {
                dto.Code = -1; dto.Description = ex.Message; dto.Result.lista = new List<SysContactoServicioDireccionData>(); dto.Result.total = 0;
            }
            return dto;
        }


        /// <summary>
        /// Lista de direcciones para exportar y obtener informacion.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="identificacion"></param>
        /// <param name="codPais"></param>
        /// <returns></returns>
        public ErrorDto<List<SysContactoServicioDireccionData>>SysContactoServicio_Direcciones_Obtener(int CodEmpresa, string identificacion, string codPais)
        {
            var dto = new ErrorDto<List<SysContactoServicioDireccionData>>
            {
                Code = 0,
                Description = "Ok",
                Result = new()
            };

            try
            {
                using var connection = new SqlConnection(_config.GetConnectionString(BaseConnStringName));

                string sql = @"
                    SELECT 
                        D.NUM_LINEA                     AS Num_Linea,
                        @ident                          AS Identificacion,
                        @codPais                        AS CodPais,
                        D.COD_PROVINCIA                 AS Cod_Provincia,
                        D.COD_CANTON                    AS Cod_Canton,
                        D.COD_DISTRITO                  AS Cod_Distrito,
                        COALESCE(PR.DESCRIPCION, '')    AS Provincia,
                        COALESCE(CA.DESCRIPCION, '')    AS Canton,
                        COALESCE(DI.DESCRIPCION, '')    AS Distrito,
                        D.DIRECCION                     AS Direccion
                    FROM dbo.SYS_PADRON_DIRECCIONES D WITH (NOLOCK)
                    LEFT JOIN dbo.SYS_PROVINCIAS PR
                      ON PR.COD_PAIS = D.COD_PAIS AND PR.COD_PROVINCIA = D.COD_PROVINCIA
                    LEFT JOIN dbo.SYS_CANTONES CA
                      ON CA.COD_PAIS = D.COD_PAIS AND CA.COD_PROVINCIA = D.COD_PROVINCIA AND CA.COD_CANTON = D.COD_CANTON
                    LEFT JOIN dbo.SYS_DISTRITOS DI
                      ON DI.COD_PAIS = D.COD_PAIS AND DI.COD_PROVINCIA = D.COD_PROVINCIA AND DI.COD_CANTON = D.COD_CANTON AND DI.COD_DISTRITO = D.COD_DISTRITO
                    WHERE UPPER(D.IDENTIFICACION) = @ident
                      AND UPPER(D.COD_PAIS)       = @codPais
                    ORDER BY D.COD_PROVINCIA, D.COD_CANTON, D.COD_DISTRITO, D.NUM_LINEA;";

                dto.Result = connection.Query<SysContactoServicioDireccionData>(sql, new
                {
                    ident = (identificacion ?? string.Empty).Trim().ToUpper(),
                    codPais = (codPais ?? "CRC").Trim().ToUpper()
                }).ToList();
            }
            catch (Exception ex)
            {
                dto.Code = -1;
                dto.Description = ex.Message;
                dto.Result = null;
            }
            return dto;
        }


        /// <summary>
        /// Lista de empresas con paginación / filtro / sort (para tabla).
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="identificacion"></param>
        /// <param name="codPais"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDto<SysContactoServicioEmpresaLista>SysContactoServicio_Empresas_Lista_Obtener(int CodEmpresa, string identificacion, string codPais, FiltrosLazyLoadData filtros)
        {
            var dto = new ErrorDto<SysContactoServicioEmpresaLista>
            { Code = 0, Description = "Ok", Result = new() { total = 0, lista = new() } };

            try
            {
                using var connection = new SqlConnection(_config.GetConnectionString(BaseConnStringName));

                var search = (filtros?.filtro ?? string.Empty).Trim();
                string? searchLike = string.IsNullOrWhiteSpace(search) ? null : $"%{search}%";

                int page = Math.Max(0, filtros?.pagina ?? 0);
                int pageSize = Math.Clamp(filtros?.paginacion ?? 30, 1, 200);

                var sortField = (filtros?.sortField ?? string.Empty).Trim().ToLowerInvariant();
                var sortOrder = filtros?.sortOrder ?? 1; // 0=DESC, 1=ASC

                var ident = (identificacion ?? string.Empty).Trim().ToUpper();
                var countryCode = (codPais ?? "CRC").Trim().ToUpper();

                const string totalSql = @"
                    SELECT COUNT(1)
                    FROM dbo.PADRON_PERSONA_EMP E WITH (NOLOCK)
                    LEFT JOIN dbo.SYS_CANTONES CA
                      ON CA.COD_PAIS = E.COD_PAIS AND CA.COD_PROVINCIA = E.COD_PROVINCIA AND CA.COD_CANTON = E.COD_CANTON
                    WHERE UPPER(E.IDENTIFICACION) = @ident
                      AND UPPER(E.COD_PAIS) = @countryCode
                      AND (@search IS NULL
                           OR E.NOMBRE      LIKE @search
                           OR CA.NOMBRE     LIKE @search
                           OR E.TELEFONO_1  LIKE @search
                           OR E.TELEFONO_2  LIKE @search);";

                dto.Result.total = connection.QueryFirstOrDefault<int>(totalSql, new
                {
                    ident,
                    countryCode,
                    search = searchLike
                });

                const string sql = @"
                    SELECT
                        @ident                 AS Identificacion,
                        @countryCode           AS CodPais,
                        E.NOMBRE               AS Nombre,
                        CA.NOMBRE              AS Canton,
                        E.FECHA_INGRESO        AS Fecha_Ingreso,
                        E.TELEFONO_1           AS Telefono_1,
                        E.TELEFONO_2           AS Telefono_2,
                        E.SALARIO              AS Salario,
                        E.ACTIVO               AS Activo
                    FROM dbo.PADRON_PERSONA_EMP E WITH (NOLOCK)
                    LEFT JOIN dbo.SYS_CANTONES CA
                      ON CA.COD_PAIS = E.COD_PAIS AND CA.COD_PROVINCIA = E.COD_PROVINCIA AND CA.COD_CANTON = E.COD_CANTON
                    WHERE UPPER(E.IDENTIFICACION) = @ident
                      AND UPPER(E.COD_PAIS) = @countryCode
                      AND (@search IS NULL
                           OR E.NOMBRE      LIKE @search
                           OR CA.NOMBRE     LIKE @search
                           OR E.TELEFONO_1  LIKE @search
                           OR E.TELEFONO_2  LIKE @search)
                    ORDER BY
                        -- ASC
                        CASE WHEN @sortOrder = 1 AND @sortField = 'nombre' THEN E.NOMBRE END ASC,
                        CASE WHEN @sortOrder = 1 AND @sortField = 'canton' THEN CA.NOMBRE END ASC,
                        CASE WHEN @sortOrder = 1 AND @sortField = 'fecha_ingreso' THEN E.FECHA_INGRESO END ASC,
                        CASE WHEN @sortOrder = 1 AND @sortField = 'telefono_1' THEN E.TELEFONO_1 END ASC,
                        CASE WHEN @sortOrder = 1 AND @sortField = 'telefono_2' THEN E.TELEFONO_2 END ASC,
                        CASE WHEN @sortOrder = 1 AND @sortField = 'salario' THEN E.SALARIO END ASC,
                        CASE WHEN @sortOrder = 1 AND @sortField = 'activo' THEN CONVERT(int, E.ACTIVO) END ASC,

                        -- DESC
                        CASE WHEN @sortOrder = 0 AND @sortField = 'nombre' THEN E.NOMBRE END DESC,
                        CASE WHEN @sortOrder = 0 AND @sortField = 'canton' THEN CA.NOMBRE END DESC,
                        CASE WHEN @sortOrder = 0 AND @sortField = 'fecha_ingreso' THEN E.FECHA_INGRESO END DESC,
                        CASE WHEN @sortOrder = 0 AND @sortField = 'telefono_1' THEN E.TELEFONO_1 END DESC,
                        CASE WHEN @sortOrder = 0 AND @sortField = 'telefono_2' THEN E.TELEFONO_2 END DESC,
                        CASE WHEN @sortOrder = 0 AND @sortField = 'salario' THEN E.SALARIO END DESC,
                        CASE WHEN @sortOrder = 0 AND @sortField = 'activo' THEN CONVERT(int, E.ACTIVO) END DESC,

                        E.FECHA_INGRESO DESC
                    OFFSET @page ROWS FETCH NEXT @pageSize ROWS ONLY;";

                dto.Result.lista = connection.Query<SysContactoServicioEmpresaData>(sql, new
                {
                    ident,
                    countryCode,
                    search = searchLike,
                    page,
                    pageSize,
                    sortField,
                    sortOrder
                }).ToList();
            }
            catch (Exception ex)
            {
                dto.Code = -1; dto.Description = ex.Message; dto.Result.lista = new List<SysContactoServicioEmpresaData>(); dto.Result.total = 0;
            }
            return dto;
        }


        /// <summary>
        /// Lista de empresas para exportar y obtener informacion.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="identificacion"></param>
        /// <param name="codPais"></param>
        /// <returns></returns>
        public ErrorDto<List<SysContactoServicioEmpresaData>>SysContactoServicio_Empresas_Obtener(int CodEmpresa, string identificacion, string codPais)
        {
            var dto = new ErrorDto<List<SysContactoServicioEmpresaData>>
            {
                Code = 0,
                Description = "Ok",
                Result = new()
            };

            try
            {
                using var connection = new SqlConnection(_config.GetConnectionString(BaseConnStringName));

                string sql = @"
                SELECT
                    @ident                          AS Identificacion,
                    @codPais                        AS CodPais,
                    E.COD_EMPRESA                   AS Cod_Empresa,
                    COALESCE(EM.DESCRIPCION, '')    AS Nombre,
                    -- opcionales:
                    EM.COD_PROVINCIA                AS Cod_Provincia,
                    EM.COD_CANTON                   AS Cod_Canton,
                    COALESCE(CA.DESCRIPCION, '')    AS Canton,

                    EP.FECHA_INGRESO                AS Fecha_Ingreso,
                    EP.TELEFONO_1                   AS Telefono_1,
                    EP.TELEFONO_2                   AS Telefono_2,
                    EP.EMAIL                        AS Email,
                    EP.SALARIO                      AS Salario,
                    EP.ACTIVO                       AS Activo
                FROM dbo.SYS_PADRON_EMPRESARIAL EP WITH (NOLOCK)   -- <== pon aquí el nombre real
                LEFT JOIN dbo.SYS_EMPRESAS EM
                  ON EM.COD_PAIS = EP.COD_PAIS AND EM.COD_EMPRESA = EP.COD_EMPRESA
                LEFT JOIN dbo.SYS_CANTONES CA
                  ON CA.COD_PAIS = EM.COD_PAIS
                 AND CA.COD_PROVINCIA = EM.COD_PROVINCIA
                 AND CA.COD_CANTON = EM.COD_CANTON
                -- alias corto para ordenar por nombre:
                OUTER APPLY (SELECT EP.COD_EMPRESA) AS E
                WHERE UPPER(EP.IDENTIFICACION) = @ident
                  AND UPPER(EP.COD_PAIS)       = @codPais
                ORDER BY EM.DESCRIPCION;";

                dto.Result = connection.Query<SysContactoServicioEmpresaData>(sql, new
                {
                    ident = (identificacion ?? "").Trim().ToUpper(),
                    codPais = (codPais ?? "CRC").Trim().ToUpper()
                }).ToList();
            }
            catch (Exception ex)
            {
                dto.Code = -1;
                dto.Description = ex.Message;
                dto.Result = null;
            }
            return dto;
        }


        /// <summary>
        /// Busca personas para el diálogo (TOP 50). Filtra por identificación y nombre.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="codPais"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDto<SysContactoServicioPersonaLookupLista> SysContactoServicio_Personas_Lista_Buscar(int CodEmpresa, string codPais, FiltrosLazyLoadData filtros)
        {
            string connectionString = GetSafeConnectionString();

            var response = new ErrorDto<SysContactoServicioPersonaLookupLista>
            {
                Code = 0,
                Description = "Ok",
                Result = new SysContactoServicioPersonaLookupLista
                {
                    total = 0,
                    lista = new List<SysContactoServicioPersonaLookupDto>()
                }
            };

            try
            {
                using var connection = new SqlConnection(connectionString);

                string rawQuery = (filtros?.filtro ?? string.Empty).Trim();
                string? query = string.IsNullOrWhiteSpace(rawQuery) ? null : rawQuery;

                int page = Math.Max(0, filtros?.pagina ?? 0);
                int pageSize = Math.Clamp(filtros?.paginacion ?? 30, 1, 200);

                var sortField = (filtros?.sortField ?? string.Empty).Trim().ToLowerInvariant();
                var sortOrder = filtros?.sortOrder ?? 1; // 0=DESC, 1=ASC

                bool looksLikeId = query != null && query.All(ch => char.IsDigit(ch) || ch == '-' || ch == ' ');
                var queryPrefix = query == null ? null : query + "%";

                // Total con filtro
                const string sqlCount = @"
                    SELECT COUNT(1)
                    FROM dbo.SYS_PADRON P WITH (NOLOCK)
                    WHERE P.COD_PAIS = @countryCode
                      AND (
                            @query IS NULL
                            OR (
                                 @looksLikeId = 1 AND P.IDENTIFICACION LIKE @queryPrefix
                               )
                            OR (
                                 @looksLikeId = 0 AND (
                                        P.IDENTIFICACION LIKE '%' + @query + '%'
                                     OR P.NOMBRE         LIKE '%' + @query + '%'
                                     OR P.APELLIDO_1     LIKE '%' + @query + '%'
                                     OR P.APELLIDO_2     LIKE '%' + @query + '%'
                                 )
                               )
                          );";

                response.Result.total = connection.Query<int>(sqlCount, new
                {
                    countryCode = codPais,
                    query,
                    queryPrefix,
                    looksLikeId = looksLikeId ? 1 : 0
                }).FirstOrDefault();

                // Página
                const string sqlPage = @"
                    SELECT
                        P.IDENTIFICACION AS Identificacion,
                        LTRIM(RTRIM(CONCAT(
                            ISNULL(P.APELLIDO_1,''),
                            CASE WHEN NULLIF(P.APELLIDO_2,'') IS NOT NULL THEN ' ' + P.APELLIDO_2 ELSE '' END,
                            CASE WHEN NULLIF(P.NOMBRE,'')     IS NOT NULL THEN ', ' + P.NOMBRE     ELSE '' END
                        ))) AS Nombre
                    FROM dbo.SYS_PADRON P WITH (NOLOCK)
                    WHERE P.COD_PAIS = @countryCode
                      AND (
                            @query IS NULL
                            OR (
                                 @looksLikeId = 1 AND P.IDENTIFICACION LIKE @queryPrefix
                               )
                            OR (
                                 @looksLikeId = 0 AND (
                                        P.IDENTIFICACION LIKE '%' + @query + '%'
                                     OR P.NOMBRE         LIKE '%' + @query + '%'
                                     OR P.APELLIDO_1     LIKE '%' + @query + '%'
                                     OR P.APELLIDO_2     LIKE '%' + @query + '%'
                                 )
                               )
                          )
                    ORDER BY
                        -- ASC
                        CASE WHEN @sortOrder = 1 AND @sortField = 'identificacion' THEN P.IDENTIFICACION END ASC,
                        CASE WHEN @sortOrder = 1 AND @sortField = 'apellido_1' THEN P.APELLIDO_1 END ASC,
                        CASE WHEN @sortOrder = 1 AND @sortField = 'apellido_2' THEN P.APELLIDO_2 END ASC,
                        CASE WHEN @sortOrder = 1 AND @sortField = 'nombre' THEN P.NOMBRE END ASC,

                        -- DESC
                        CASE WHEN @sortOrder = 0 AND @sortField = 'identificacion' THEN P.IDENTIFICACION END DESC,
                        CASE WHEN @sortOrder = 0 AND @sortField = 'apellido_1' THEN P.APELLIDO_1 END DESC,
                        CASE WHEN @sortOrder = 0 AND @sortField = 'apellido_2' THEN P.APELLIDO_2 END DESC,
                        CASE WHEN @sortOrder = 0 AND @sortField = 'nombre' THEN P.NOMBRE END DESC,

                        P.APELLIDO_1 ASC, P.APELLIDO_2 ASC, P.NOMBRE ASC
                    OFFSET @page ROWS
                    FETCH NEXT @pageSize ROWS ONLY
                    OPTION (FAST 30, RECOMPILE);";

                response.Result.lista = connection.Query<SysContactoServicioPersonaLookupDto>(sqlPage, new
                {
                    countryCode = codPais,
                    query,
                    queryPrefix,
                    looksLikeId = looksLikeId ? 1 : 0,
                    page,
                    pageSize,
                    sortField,
                    sortOrder
                }).ToList();

                if (response.Result.total == 0)
                {
                    response.Result.total = page + (response.Result.lista?.Count ?? 0) + 1;
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result.total = 0;
                response.Result.lista = new List<SysContactoServicioPersonaLookupDto>();
            }

            return response;
        }
    }
}