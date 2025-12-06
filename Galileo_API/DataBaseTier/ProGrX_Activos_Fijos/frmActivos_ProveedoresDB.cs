using Dapper;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Activos_Fijos;
using Galileo.Models.Security;

namespace Galileo.DataBaseTier.ProGrX_Activos_Fijos
{
    public class FrmActivosProveedoresDb
    {
        private readonly int vModulo = 36;
        private readonly PortalDB _portalDB;
        private readonly MSecurityMainDb _Security_MainDB;
        private const string _codproveedor = "COD_PROVEEDOR";

        public FrmActivosProveedoresDb(IConfiguration config)
        {
            _portalDB = new PortalDB(config);
            _Security_MainDB = new MSecurityMainDb(config);
        }

        /// <summary>
        /// Obtiene una lista de proveedores con paginación y filtros.
        /// </summary>
        public ErrorDto<ActivosProveedoresLista> Activos_ProveedoresLista_Obtener(int CodEmpresa, FiltrosLazyLoadData filtros)
        {
            var resp = new ErrorDto<ActivosProveedoresLista>
            {
                Code = 0,
                Description = "Ok",
                Result = new ActivosProveedoresLista()
            };

            try
            {
                using var cn = _portalDB.CreateConnection(CodEmpresa);

                var parameters = new DynamicParameters();

                // Filtro seguro (sin WHERE dinámico)
                string? filtroTexto = filtros?.filtro;
                bool tieneFiltro = !string.IsNullOrWhiteSpace(filtroTexto);
                parameters.Add("@tieneFiltro", tieneFiltro ? 1 : 0);
                parameters.Add("@filtro", tieneFiltro ? $"%{filtroTexto!.Trim()}%" : null);

                // Total de registros
                const string queryTotal = @"
                    SELECT COUNT(*)
                    FROM dbo.ACTIVOS_PROVEEDORES
                    WHERE (@tieneFiltro = 0
                           OR COD_PROVEEDOR LIKE @filtro
                           OR DESCRIPCION  LIKE @filtro);";

                resp.Result.total = cn.QueryFirstOrDefault<int>(queryTotal, parameters);

                // Order by seguro: usamos índice + CASE
                var sortFieldRaw = filtros?.sortField ?? _codproveedor;
                var sortFieldNorm = sortFieldRaw.Trim().ToUpperInvariant();

                int sortIndex = sortFieldNorm switch
                {
                    "COD_PROVEEDOR"                => 1,
                    "DESCRIPCION"                  => 2,
                    "ACTIVO"                       => 3,
                    "REGISTRO_USUARIO" or "USUARIO"=> 4,
                    _                              => 1
                };
                parameters.Add("@sortIndex", sortIndex);

                int sortDir = (filtros?.sortOrder ?? 0) == 0 ? 0 : 1; // 0 = DESC, 1 = ASC
                parameters.Add("@sortDir", sortDir);

                // Paginación
                int pagina = filtros?.pagina ?? 0;
                int paginacion = filtros?.paginacion ?? 50;

                parameters.Add("@offset", pagina);
                parameters.Add("@fetch", paginacion);

                const string query = @"
                    SELECT
                        COD_PROVEEDOR                                           AS cod_proveedor,
                        ISNULL(DESCRIPCION,'')                                  AS descripcion,
                        CAST(CASE WHEN ISNULL(ACTIVO,1)=1 THEN 1 ELSE 0 END AS bit) AS activo,
                        ISNULL(REGISTRO_USUARIO,'')                             AS usuario
                    FROM dbo.ACTIVOS_PROVEEDORES
                    WHERE (@tieneFiltro = 0
                           OR COD_PROVEEDOR LIKE @filtro
                           OR DESCRIPCION  LIKE @filtro)
                    ORDER BY
                        -- ASC
                        CASE @sortDir WHEN 1 THEN
                            CASE @sortIndex
                                WHEN 1 THEN COD_PROVEEDOR
                                WHEN 2 THEN DESCRIPCION
                                WHEN 3 THEN ACTIVO
                                WHEN 4 THEN REGISTRO_USUARIO
                            END
                        END ASC,
                        -- DESC
                        CASE @sortDir WHEN 0 THEN
                            CASE @sortIndex
                                WHEN 1 THEN COD_PROVEEDOR
                                WHEN 2 THEN DESCRIPCION
                                WHEN 3 THEN ACTIVO
                                WHEN 4 THEN REGISTRO_USUARIO
                            END
                        END DESC
                    OFFSET @offset ROWS
                    FETCH NEXT @fetch ROWS ONLY;";

                resp.Result.lista = cn.Query<ActivosProveedoresData>(query, parameters).ToList();
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
                resp.Result.total = 0;
                resp.Result.lista = [];
            }
            return resp;
        }

        /// <summary>
        /// Obtener lista completa de proveedores (sin paginación), con filtro opcional.
        /// </summary>
        public ErrorDto<List<ActivosProveedoresData>> Activos_Proveedores_Obtener(int CodEmpresa, FiltrosLazyLoadData filtros)
        {
            var resp = new ErrorDto<List<ActivosProveedoresData>>
            {
                Code = 0,
                Description = "Ok",
                Result = new List<ActivosProveedoresData>()
            };

            try
            {
                using var cn = _portalDB.CreateConnection(CodEmpresa);

                var parameters = new DynamicParameters();

                string? filtroTexto = filtros?.filtro;
                bool tieneFiltro = !string.IsNullOrWhiteSpace(filtroTexto);
                parameters.Add("@tieneFiltro", tieneFiltro ? 1 : 0);
                parameters.Add("@filtro", tieneFiltro ? $"%{filtroTexto!.Trim()}%" : null);

                const string query = @"
                    SELECT
                        COD_PROVEEDOR                                           AS cod_proveedor,
                        ISNULL(DESCRIPCION,'')                                  AS descripcion,
                        CAST(CASE WHEN ISNULL(ACTIVO,1)=1 THEN 1 ELSE 0 END AS bit) AS activo,
                        ISNULL(REGISTRO_USUARIO,'')                             AS usuario
                    FROM dbo.ACTIVOS_PROVEEDORES
                    WHERE (@tieneFiltro = 0
                           OR COD_PROVEEDOR LIKE @filtro
                           OR DESCRIPCION  LIKE @filtro)
                    ORDER BY COD_PROVEEDOR;";

                resp.Result = cn.Query<ActivosProveedoresData>(query, parameters).ToList();
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
        /// Inserta o actualiza un proveedor.
        /// </summary>
        public ErrorDto Activos_Proveedores_Guardar(int CodEmpresa, string usuario, ActivosProveedoresData proveedor)
        {
            var resp = new ErrorDto { Code = 0, Description = "Ok" };

            try
            {
                // Validaciones básicas
                if (proveedor == null)
                    return new ErrorDto { Code = -1, Description = "Datos no proporcionados." };

                if (string.IsNullOrWhiteSpace(proveedor.cod_proveedor))
                    return new ErrorDto { Code = -1, Description = "Debe indicar el código de proveedor." };

                if (string.IsNullOrWhiteSpace(proveedor.descripcion))
                    return new ErrorDto { Code = -1, Description = "Debe indicar la descripción." };

                using var cn = _portalDB.CreateConnection(CodEmpresa);

                const string query = @"SELECT COUNT(1) FROM dbo.ACTIVOS_PROVEEDORES WHERE COD_PROVEEDOR = @cod";
                int existe = cn.QueryFirstOrDefault<int>(query, new { cod = proveedor.cod_proveedor.ToUpper() });

                if (proveedor.isNew)
                {
                    if (existe > 0)
                        return new ErrorDto { Code = -2, Description = $"El proveedor {proveedor.cod_proveedor.ToUpper()} ya existe." };

                    // Insertar
                    return Activos_Proveedores_Insertar(CodEmpresa, usuario, proveedor);
                }
                else
                {
                    if (existe == 0)
                        return new ErrorDto { Code = -2, Description = $"El proveedor {proveedor.cod_proveedor.ToUpper()} no existe." };

                    // Actualizar
                    return Activos_Proveedores_Actualizar(CodEmpresa, usuario, proveedor);
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
        /// Inserta un nuevo proveedor.
        /// </summary>
        private ErrorDto Activos_Proveedores_Insertar(int CodEmpresa, string usuario, ActivosProveedoresData proveedor)
        {
            var resp = new ErrorDto { Code = 0, Description = "Ok" };

            try
            {
                using var cn = _portalDB.CreateConnection(CodEmpresa);
                var query = @"
            INSERT INTO dbo.ACTIVOS_PROVEEDORES
                (COD_PROVEEDOR, DESCRIPCION, ACTIVO, REGISTRO_FECHA, REGISTRO_USUARIO, MODIFICA_USUARIO, MODIFICA_FECHA)
            VALUES
                (@cod, @desc, @act, SYSDATETIME(), @usr, NULL, NULL);";

                cn.Execute(query, new
                {
                    cod = proveedor.cod_proveedor.ToUpper(),
                    desc = proveedor.descripcion?.ToUpper(),
                    act = proveedor.activo ? 1 : 0,
                    usr = string.IsNullOrWhiteSpace(usuario) ? null : usuario
                });

                _Security_MainDB.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = usuario ?? "",
                    DetalleMovimiento = $"Proveedor: {proveedor.cod_proveedor} - {proveedor.descripcion}",
                    Movimiento = "Registra - WEB",
                    Modulo = vModulo
                });

                resp.Description = "Proveedor ingresado satisfactoriamente.";
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }
            return resp;
        }

        /// <summary>
        /// Actualiza un proveedor existente.
        /// </summary>
        private ErrorDto Activos_Proveedores_Actualizar(int CodEmpresa, string usuario, ActivosProveedoresData proveedor)
        {
            var resp = new ErrorDto { Code = 0, Description = "Ok" };

            try
            {
                using var cn = _portalDB.CreateConnection(CodEmpresa);
                var query = @"
            UPDATE dbo.ACTIVOS_PROVEEDORES
               SET DESCRIPCION      = @desc,
                   ACTIVO           = @act,
                   MODIFICA_USUARIO = @usr,
                   MODIFICA_FECHA   = SYSDATETIME()
             WHERE COD_PROVEEDOR    = @cod;";

                cn.Execute(query, new
                {
                    cod = proveedor.cod_proveedor.ToUpper(),
                    desc = proveedor.descripcion?.ToUpper(),
                    act = proveedor.activo ? 1 : 0,
                    usr = string.IsNullOrWhiteSpace(usuario) ? null : usuario
                });

                _Security_MainDB.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = usuario ?? "",
                    DetalleMovimiento = $"Proveedor: {proveedor.cod_proveedor} - {proveedor.descripcion}",
                    Movimiento = "Modifica - WEB",
                    Modulo = vModulo
                });

                resp.Description = "Proveedor actualizado satisfactoriamente.";
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }
            return resp;
        }

        /// <summary>
        /// Eliminar proveedor por su código.
        /// </summary>
        public ErrorDto Activos_Proveedores_Eliminar(int CodEmpresa, string usuario, string cod_proveedor)
        {
            var resp = new ErrorDto { Code = 0, Description = "Ok" };

            try
            {
                if (string.IsNullOrWhiteSpace(cod_proveedor))
                    return new ErrorDto { Code = -1, Description = "Debe indicar el código de proveedor." };

                using var cn = _portalDB.CreateConnection(CodEmpresa);

                var query = @"DELETE FROM dbo.ACTIVOS_PROVEEDORES WHERE COD_PROVEEDOR = @cod";
                int rows = cn.Execute(query, new { cod = cod_proveedor.ToUpper() });

                if (rows == 0)
                    return new ErrorDto { Code = -2, Description = $"El proveedor {cod_proveedor.ToUpper()} no existe." };

                _Security_MainDB.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = usuario ?? "",
                    DetalleMovimiento = $"Proveedor: {cod_proveedor}",
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
        /// Importar proveedores desde CXP_PROVEEDORES a ACTIVOS_PROVEEDORES.
        /// </summary>
        public ErrorDto Activos_Proveedores_Importar(int CodEmpresa, string usuario)
        {
            var resp = new ErrorDto { Code = 0, Description = "Ok" };

            try
            {
                using var cn = _portalDB.CreateConnection(CodEmpresa);

                var query = @"
                    INSERT INTO dbo.ACTIVOS_PROVEEDORES
                        (COD_PROVEEDOR, DESCRIPCION, ACTIVO, REGISTRO_FECHA, REGISTRO_USUARIO)
                    SELECT
                        CONVERT(varchar(20), cxp.COD_PROVEEDOR)      AS COD_PROVEEDOR,
                        cxp.DESCRIPCION                              AS DESCRIPCION,
                        1                                            AS ACTIVO,
                        SYSDATETIME()                                AS REGISTRO_FECHA,
                        @usr                                         AS REGISTRO_USUARIO
                    FROM dbo.CXP_PROVEEDORES cxp
                    WHERE CONVERT(varchar(20), cxp.COD_PROVEEDOR) NOT IN
                          (SELECT COD_PROVEEDOR FROM dbo.ACTIVOS_PROVEEDORES)
                      AND cxp.ESTADO = 'A';";

                int rows = cn.Execute(query, new { usr = usuario });

                _Security_MainDB.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = usuario ?? "",
                    DetalleMovimiento = $"Importa Proveedores desde CXP_PROVEEDORES ({rows} nuevos).",
                    Movimiento = "Registra - WEB",
                    Modulo = vModulo
                });

                resp.Description = rows == 0
                    ? "No había proveedores nuevos para importar."
                    : $"Importación completada. Registros agregados: {rows}.";
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }
            return resp;
        }

        /// <summary>
        /// Valida si un código de proveedor ya existe en la base de datos ACTIVOS_PROVEEDORES.
        /// </summary>
        public ErrorDto Activos_Proveedores_Valida(int CodEmpresa, string cod_proveedor)
        {
            var resp = new ErrorDto
            {
                Code = 0,
                Description = "Ok"
            };

            try
            {
                using var cn = _portalDB.CreateConnection(CodEmpresa);
                const string query = @"SELECT COUNT(1) 
                           FROM dbo.ACTIVOS_PROVEEDORES 
                           WHERE UPPER(COD_PROVEEDOR) = @cod";

                int existe = cn.QueryFirstOrDefault<int>(query, new { cod = (cod_proveedor ?? string.Empty).ToUpper() });

                if (existe > 0)
                {
                    resp.Code = -1;
                    resp.Description = "El código de proveedor ya existe.";
                }
                else
                {
                    resp.Code = 0;
                    resp.Description = "El código de proveedor es válido.";
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }

            return resp;
        }
    }
}