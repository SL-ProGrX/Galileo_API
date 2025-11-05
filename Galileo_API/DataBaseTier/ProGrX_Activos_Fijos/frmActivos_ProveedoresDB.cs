using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.ProGrX_Activos_Fijos;

namespace PgxAPI.DataBaseTier.ProGrX_Activos_Fijos
{
    public class frmActivos_ProveedoresDB
    {
        private readonly IConfiguration? _config;
        private readonly int vModulo = 36;
        private readonly mSecurityMainDb _Security_MainDB;

        public frmActivos_ProveedoresDB(IConfiguration? config)
        {
            _config = config;
            _Security_MainDB = new mSecurityMainDb(_config);
        }

        /// <summary>
        /// Obtiene una lista de proveedores con paginación y filtros.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDTO<ActivosProveedoresLista> Activos_ProveedoresLista_Obtener(int CodEmpresa, FiltrosLazyLoadData filtros)
        {
            string connStr = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var resp = new ErrorDTO<ActivosProveedoresLista>
            {
                Code = 0,
                Description = "Ok",
                Result = new ActivosProveedoresLista()
            };

            try
            {
                using var cn = new SqlConnection(connStr);

                // Total (con o sin filtro)
                string where = "";
                if (!string.IsNullOrWhiteSpace(filtros?.filtro))
                {
                    var f = filtros.filtro.Replace("'", "''");
                    where = $" WHERE COD_PROVEEDOR LIKE '%{f}%' OR DESCRIPCION LIKE '%{f}%'";
                }

                var queryTotal = $"SELECT COUNT(*) FROM dbo.ACTIVOS_PROVEEDORES {where}";
                resp.Result.total = cn.QueryFirstOrDefault<int>(queryTotal);

                // Order by
                string sortField = string.IsNullOrWhiteSpace(filtros?.sortField) ? "COD_PROVEEDOR" : filtros.sortField;
                string sortOrder = filtros?.sortOrder == 0 ? "DESC" : "ASC";

                // Paginación
                int pagina = filtros?.pagina ?? 0;
                int paginacion = filtros?.paginacion ?? 50;

                var query = $@"
                    SELECT
                        COD_PROVEEDOR                                     AS cod_proveedor,
                        ISNULL(DESCRIPCION,'')                            AS descripcion,
                        CAST(CASE WHEN ISNULL(ACTIVO,1)=1 THEN 1 ELSE 0 END AS bit) AS activo,
                        ISNULL(REGISTRO_USUARIO,'')                       AS usuario
                    FROM dbo.ACTIVOS_PROVEEDORES
                    {where}
                    ORDER BY {sortField} {sortOrder}
                    OFFSET {pagina} ROWS
                    FETCH NEXT {paginacion} ROWS ONLY;";

                resp.Result.lista = cn.Query<ActivosProveedoresData>(query).ToList();
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
        /// Obtener lista completa de proveedores (sin paginación), con filtro opcional.
        /// <param name="CodEmpresa"></param>"
        /// <param name="filtros"></param>
        /// </summary>
        /// <returns></returns>
        public ErrorDTO<List<ActivosProveedoresData>> Activos_Proveedores_Obtener(int CodEmpresa, FiltrosLazyLoadData filtros)
        {
            string connStr = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var resp = new ErrorDTO<List<ActivosProveedoresData>>
            {
                Code = 0,
                Description = "Ok",
                Result = new List<ActivosProveedoresData>()
            };

            try
            {
                using var cn = new SqlConnection(connStr);

                string where = "";
                if (!string.IsNullOrWhiteSpace(filtros?.filtro))
                {
                    var f = filtros.filtro.Replace("'", "''");
                    where = $" WHERE COD_PROVEEDOR LIKE '%{f}%' OR DESCRIPCION LIKE '%{f}%'";
                }

                var query = $@"
                    SELECT
                        COD_PROVEEDOR                                     AS cod_proveedor,
                        ISNULL(DESCRIPCION,'')                            AS descripcion,
                        CAST(CASE WHEN ISNULL(ACTIVO,1)=1 THEN 1 ELSE 0 END AS bit) AS activo,
                        ISNULL(REGISTRO_USUARIO,'')                       AS usuario
                    FROM dbo.ACTIVOS_PROVEEDORES
                    {where}
                    ORDER BY COD_PROVEEDOR;";

                resp.Result = cn.Query<ActivosProveedoresData>(query).ToList();
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
        /// <param name="CodEmpresa"></param>"
        /// <param name="usuario"></param>"
        /// <param name="proveedor"></param>"
        /// </summary>
        /// <returns></returns>
        public ErrorDTO Activos_Proveedores_Guardar(int CodEmpresa, string usuario, ActivosProveedoresData proveedor)
        {
            string connStr = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var resp = new ErrorDTO { Code = 0, Description = "Ok" };

            try
            {
                // Validaciones básicas
                if (proveedor == null)
                    return new ErrorDTO { Code = -1, Description = "Datos no proporcionados." };

                if (string.IsNullOrWhiteSpace(proveedor.cod_proveedor))
                    return new ErrorDTO { Code = -1, Description = "Debe indicar el código de proveedor." };

                if (string.IsNullOrWhiteSpace(proveedor.descripcion))
                    return new ErrorDTO { Code = -1, Description = "Debe indicar la descripción." };

                using var cn = new SqlConnection(connStr);

                const string query = @"SELECT COUNT(1) FROM dbo.ACTIVOS_PROVEEDORES WHERE COD_PROVEEDOR = @cod";
                int existe = cn.QueryFirstOrDefault<int>(query, new { cod = proveedor.cod_proveedor.ToUpper() });

                if (proveedor.isNew)
                {
                    if (existe > 0)
                        return new ErrorDTO { Code = -2, Description = $"El proveedor {proveedor.cod_proveedor.ToUpper()} ya existe." };

                    // Insertar
                    return Activos_Proveedores_Insertar(CodEmpresa, usuario, proveedor);
                }
                else
                {
                    if (existe == 0)
                        return new ErrorDTO { Code = -2, Description = $"El proveedor {proveedor.cod_proveedor.ToUpper()} no existe." };

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
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="proveedor"></param>
        /// </summary>
        /// <returns></returns>
        private ErrorDTO Activos_Proveedores_Insertar(int CodEmpresa, string usuario, ActivosProveedoresData proveedor)
        {
            string connStr = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var resp = new ErrorDTO { Code = 0, Description = "Ok" };

            try
            {
                using var cn = new SqlConnection(connStr);
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

                _Security_MainDB.Bitacora(new BitacoraInsertarDTO
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
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="proveedor"></param>
        /// </summary>
        /// <returns></returns>
        private ErrorDTO Activos_Proveedores_Actualizar(int CodEmpresa, string usuario, ActivosProveedoresData proveedor)
        {
            string connStr = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var resp = new ErrorDTO { Code = 0, Description = "Ok" };

            try
            {
                using var cn = new SqlConnection(connStr);
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

                _Security_MainDB.Bitacora(new BitacoraInsertarDTO
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
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="cod_proveedor"></param>
        /// </summary>
        /// <returns></returns>
        public ErrorDTO Activos_Proveedores_Eliminar(int CodEmpresa, string usuario, string cod_proveedor)
        {
            string connStr = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var resp = new ErrorDTO { Code = 0, Description = "Ok" };

            try
            {
                if (string.IsNullOrWhiteSpace(cod_proveedor))
                    return new ErrorDTO { Code = -1, Description = "Debe indicar el código de proveedor." };

                using var cn = new SqlConnection(connStr);

                var query = @"DELETE FROM dbo.ACTIVOS_PROVEEDORES WHERE COD_PROVEEDOR = @cod";
                int rows = cn.Execute(query, new { cod = cod_proveedor.ToUpper() });

                if (rows == 0)
                    return new ErrorDTO { Code = -2, Description = $"El proveedor {cod_proveedor.ToUpper()} no existe." };

                _Security_MainDB.Bitacora(new BitacoraInsertarDTO
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
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>"
        /// </summary>
        /// <returns></returns>
        public ErrorDTO Activos_Proveedores_Importar(int CodEmpresa, string usuario)
        {
            string connStr = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var resp = new ErrorDTO { Code = 0, Description = "Ok" };

            try
            {
                using var cn = new SqlConnection(connStr);

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

                _Security_MainDB.Bitacora(new BitacoraInsertarDTO
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
        /// <param name="CodEmpresa">.</param>
        /// <param name="cod_proveedor"></param>
        /// <returns></returns>
        public ErrorDTO Activos_Proveedores_Valida(int CodEmpresa, string cod_proveedor)
        {
            string connStr = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var resp = new ErrorDTO
            {
                Code = 0,
                Description = "Ok"
            };

            try
            {
                using var cn = new SqlConnection(connStr);
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
