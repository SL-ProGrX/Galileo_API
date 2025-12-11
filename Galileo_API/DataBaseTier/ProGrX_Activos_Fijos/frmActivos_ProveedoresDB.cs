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

        // Bloque común de columnas y WHERE para proveedores
        private const string BaseSelectProveedoresSelect = @"
            SELECT
                COD_PROVEEDOR                                           AS cod_proveedor,
                ISNULL(DESCRIPCION,'')                                  AS descripcion,
                CAST(CASE WHEN ISNULL(ACTIVO,1)=1 THEN 1 ELSE 0 END AS bit) AS activo,
                ISNULL(REGISTRO_USUARIO,'')                             AS usuario";

        private const string BaseSelectProveedoresFromWhere = @"
            FROM dbo.ACTIVOS_PROVEEDORES
            WHERE (@tieneFiltro = 0
                   OR COD_PROVEEDOR LIKE @filtro
                   OR DESCRIPCION  LIKE @filtro)";

        public FrmActivosProveedoresDb(IConfiguration config)
        {
            _portalDB = new PortalDB(config);
            _Security_MainDB = new MSecurityMainDb(config);
        }

        #region Helpers privados

        private static void AddFiltroParametros(DynamicParameters parameters, FiltrosLazyLoadData? filtros)
        {
            string? filtroTexto = filtros?.filtro;
            bool tieneFiltro = !string.IsNullOrWhiteSpace(filtroTexto);
            parameters.Add("@tieneFiltro", tieneFiltro ? 1 : 0);
            parameters.Add("@filtro", tieneFiltro ? $"%{filtroTexto!.Trim()}%" : null);
        }

        private static string NormalizeCodigoProveedor(string? cod)
            => (cod ?? string.Empty).Trim().ToUpperInvariant();

        private object BuildProveedorDbParams(string usuario, ActivosProveedoresData proveedor) => new
        {
            cod = NormalizeCodigoProveedor(proveedor.cod_proveedor),
            desc = proveedor.descripcion?.ToUpper(),
            act = proveedor.activo ? 1 : 0,
            usr = string.IsNullOrWhiteSpace(usuario) ? null : usuario
        };

        private void RegistrarBitacoraProveedor(int CodEmpresa, string usuario, string detalle, string movimiento)
        {
            _Security_MainDB.Bitacora(new BitacoraInsertarDto
            {
                EmpresaId = CodEmpresa,
                Usuario = usuario ?? "",
                DetalleMovimiento = detalle,
                Movimiento = movimiento,
                Modulo = vModulo
            });
        }

        #endregion

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

                // Filtro
                AddFiltroParametros(parameters, filtros);

                // Total de registros
                const string queryTotalFixed = @"
                    SELECT COUNT(*)
                    FROM dbo.ACTIVOS_PROVEEDORES
                    WHERE (@tieneFiltro = 0
                           OR COD_PROVEEDOR LIKE @filtro
                           OR DESCRIPCION  LIKE @filtro);";

                resp.Result.total = cn.QueryFirstOrDefault<int>(queryTotalFixed, parameters);

                // Ordenamiento
                var sortFieldRaw = filtros?.sortField ?? _codproveedor;
                var sortFieldNorm = sortFieldRaw.Trim().ToUpperInvariant();

                int sortIndex = sortFieldNorm switch
                {
                    "COD_PROVEEDOR"                  => 1,
                    "DESCRIPCION"                    => 2,
                    "ACTIVO"                         => 3,
                    "REGISTRO_USUARIO" or "USUARIO"  => 4,
                    _                                => 1
                };
                parameters.Add("@sortIndex", sortIndex);

                int sortDir = (filtros?.sortOrder ?? 0) == 0 ? 0 : 1; // 0 = DESC, 1 = ASC
                parameters.Add("@sortDir", sortDir);

                // Paginación
                int pagina = filtros?.pagina ?? 0;
                int paginacion = filtros?.paginacion ?? 50;
                parameters.Add("@offset", pagina);
                parameters.Add("@fetch", paginacion);

                string query = BaseSelectProveedoresSelect + BaseSelectProveedoresFromWhere + @"
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
                AddFiltroParametros(parameters, filtros);

                string query = BaseSelectProveedoresSelect + BaseSelectProveedoresFromWhere + @"
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

                const string query = @"SELECT COUNT(1) 
                                       FROM dbo.ACTIVOS_PROVEEDORES 
                                       WHERE COD_PROVEEDOR = @cod";

                int existe = cn.QueryFirstOrDefault<int>(
                    query,
                    new { cod = NormalizeCodigoProveedor(proveedor.cod_proveedor) });

                if (proveedor.isNew)
                {
                    if (existe > 0)
                        return new ErrorDto
                        {
                            Code = -2,
                            Description = $"El proveedor {NormalizeCodigoProveedor(proveedor.cod_proveedor)} ya existe."
                        };

                    // Insertar
                    return Activos_Proveedores_Insertar(CodEmpresa, usuario, proveedor);
                }
                else
                {
                    if (existe == 0)
                        return new ErrorDto
                        {
                            Code = -2,
                            Description = $"El proveedor {NormalizeCodigoProveedor(proveedor.cod_proveedor)} no existe."
                        };

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
                const string query = @"
                    INSERT INTO dbo.ACTIVOS_PROVEEDORES
                        (COD_PROVEEDOR, DESCRIPCION, ACTIVO, REGISTRO_FECHA, REGISTRO_USUARIO, MODIFICA_USUARIO, MODIFICA_FECHA)
                    VALUES
                        (@cod, @desc, @act, SYSDATETIME(), @usr, NULL, NULL);";

                cn.Execute(query, BuildProveedorDbParams(usuario, proveedor));

                RegistrarBitacoraProveedor(
                    CodEmpresa,
                    usuario,
                    $"Proveedor: {proveedor.cod_proveedor} - {proveedor.descripcion}",
                    "Registra - WEB");

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
                const string query = @"
                    UPDATE dbo.ACTIVOS_PROVEEDORES
                       SET DESCRIPCION      = @desc,
                           ACTIVO           = @act,
                           MODIFICA_USUARIO = @usr,
                           MODIFICA_FECHA   = SYSDATETIME()
                     WHERE COD_PROVEEDOR    = @cod;";

                cn.Execute(query, BuildProveedorDbParams(usuario, proveedor));

                RegistrarBitacoraProveedor(
                    CodEmpresa,
                    usuario,
                    $"Proveedor: {proveedor.cod_proveedor} - {proveedor.descripcion}",
                    "Modifica - WEB");

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

                string codNorm = NormalizeCodigoProveedor(cod_proveedor);

                using var cn = _portalDB.CreateConnection(CodEmpresa);

                const string query = @"DELETE FROM dbo.ACTIVOS_PROVEEDORES WHERE COD_PROVEEDOR = @cod";
                int rows = cn.Execute(query, new { cod = codNorm });

                if (rows == 0)
                    return new ErrorDto { Code = -2, Description = $"El proveedor {codNorm} no existe." };

                RegistrarBitacoraProveedor(
                    CodEmpresa,
                    usuario,
                    $"Proveedor: {codNorm}",
                    "Elimina - WEB");
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

                const string query = @"
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

                RegistrarBitacoraProveedor(
                    CodEmpresa,
                    usuario,
                    $"Importa Proveedores desde CXP_PROVEEDORES ({rows} nuevos).",
                    "Registra - WEB");

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
                const string query = @"
                    SELECT COUNT(1) 
                    FROM dbo.ACTIVOS_PROVEEDORES 
                    WHERE UPPER(COD_PROVEEDOR) = @cod";

                string codNorm = NormalizeCodigoProveedor(cod_proveedor);
                int existe = cn.QueryFirstOrDefault<int>(query, new { cod = codNorm });

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