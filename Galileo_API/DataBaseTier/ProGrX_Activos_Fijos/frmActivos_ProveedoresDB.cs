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

        // Mensajes
        private const string MsgOk                      = "Ok";
        private const string MsgDatosNoProporcionados   = "Datos no proporcionados.";
        private const string MsgDebeIndicarCodProveedor = "Debe indicar el código de proveedor.";
        private const string MsgDebeIndicarDescripcion  = "Debe indicar la descripción.";
        private const string MsgProveedorIngresado      = "Proveedor ingresado satisfactoriamente.";
        private const string MsgProveedorActualizado    = "Proveedor actualizado satisfactoriamente.";
        private const string MsgCodProveedorExiste      = "El código de proveedor ya existe.";
        private const string MsgCodProveedorValido      = "El código de proveedor es válido.";

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
            _portalDB       = new PortalDB(config);
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
                EmpresaId         = CodEmpresa,
                Usuario           = usuario ?? "",
                DetalleMovimiento = detalle,
                Movimiento        = movimiento,
                Modulo            = vModulo
            });
        }

        /// <summary>
        /// Helper común para INSERT/UPDATE de proveedores.
        /// </summary>
        private ErrorDto ExecuteNonQueryProveedor(
            int CodEmpresa,
            string usuario,
            string sql,
            object parameters,
            string detalleMovimiento,
            string movimiento,
            string mensajeOk)
        {
            var resp = new ErrorDto { Code = 0, Description = MsgOk };

            try
            {
                using var cn = _portalDB.CreateConnection(CodEmpresa);
                cn.Execute(sql, parameters);

                RegistrarBitacoraProveedor(CodEmpresa, usuario, detalleMovimiento, movimiento);
                resp.Description = mensajeOk;
            }
            catch (Exception ex)
            {
                resp.Code        = -1;
                resp.Description = ex.Message;
            }

            return resp;
        }

        #endregion

        /// <summary>
        /// Obtiene una lista de proveedores con paginación y filtros.
        /// </summary>
        public ErrorDto<ActivosProveedoresLista> Activos_ProveedoresLista_Obtener(int CodEmpresa, FiltrosLazyLoadData filtros)
        {
            var resp = new ErrorDto<ActivosProveedoresLista>
            {
                Code        = 0,
                Description = MsgOk,
                Result      = new ActivosProveedoresLista()
            };

            try
            {
                using var cn = _portalDB.CreateConnection(CodEmpresa);

                var parameters = new DynamicParameters();

                // Filtro
                AddFiltroParametros(parameters, filtros);

                // Total de registros (reutiliza el mismo WHERE)
                string queryTotal = "SELECT COUNT(*) " + BaseSelectProveedoresFromWhere + ";";
                resp.Result.total = cn.QueryFirstOrDefault<int>(queryTotal, parameters);

                // Ordenamiento
                var sortFieldRaw  = filtros?.sortField ?? _codproveedor;
                var sortFieldNorm = sortFieldRaw.Trim().ToUpperInvariant();

                int sortIndex = sortFieldNorm switch
                {
                    "COD_PROVEEDOR"                 => 1,
                    "DESCRIPCION"                   => 2,
                    "ACTIVO"                        => 3,
                    "REGISTRO_USUARIO" or "USUARIO" => 4,
                    _                               => 1
                };
                parameters.Add("@sortIndex", sortIndex);

                int sortDir = (filtros?.sortOrder ?? 0) == 0 ? 0 : 1; // 0 = DESC, 1 = ASC
                parameters.Add("@sortDir", sortDir);

                // Paginación
                int pagina     = filtros?.pagina ?? 0;
                int paginacion = filtros?.paginacion ?? 50;
                parameters.Add("@offset", pagina);
                parameters.Add("@fetch",  paginacion);

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
                resp.Code        = -1;
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
                Code        = 0,
                Description = MsgOk,
                Result      = new List<ActivosProveedoresData>()
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
                resp.Code        = -1;
                resp.Description = ex.Message;
                resp.Result      = null;
            }
            return resp;
        }

        /// <summary>
        /// Inserta o actualiza un proveedor.
        /// </summary>
        public ErrorDto Activos_Proveedores_Guardar(int CodEmpresa, string usuario, ActivosProveedoresData proveedor)
        {
            var resp = new ErrorDto { Code = 0, Description = MsgOk };

            try
            {
                if (proveedor == null)
                    return new ErrorDto { Code = -1, Description = MsgDatosNoProporcionados };

                if (string.IsNullOrWhiteSpace(proveedor.cod_proveedor))
                    return new ErrorDto { Code = -1, Description = MsgDebeIndicarCodProveedor };

                if (string.IsNullOrWhiteSpace(proveedor.descripcion))
                    return new ErrorDto { Code = -1, Description = MsgDebeIndicarDescripcion };

                using var cn = _portalDB.CreateConnection(CodEmpresa);

                const string queryExiste = @"
                    SELECT COUNT(1) 
                    FROM dbo.ACTIVOS_PROVEEDORES 
                    WHERE COD_PROVEEDOR = @cod";

                int existe = cn.QueryFirstOrDefault<int>(
                    queryExiste,
                    new { cod = NormalizeCodigoProveedor(proveedor.cod_proveedor) });

                if (proveedor.isNew)
                {
                    if (existe > 0)
                        return new ErrorDto
                        {
                            Code        = -2,
                            Description = $"El proveedor {NormalizeCodigoProveedor(proveedor.cod_proveedor)} ya existe."
                        };

                    return Activos_Proveedores_Insertar(CodEmpresa, usuario, proveedor);
                }

                if (existe == 0)
                    return new ErrorDto
                    {
                        Code        = -2,
                        Description = $"El proveedor {NormalizeCodigoProveedor(proveedor.cod_proveedor)} no existe."
                    };

                return Activos_Proveedores_Actualizar(CodEmpresa, usuario, proveedor);
            }
            catch (Exception ex)
            {
                resp.Code        = -1;
                resp.Description = ex.Message;
            }
            return resp;
        }

        /// <summary>
        /// Inserta un nuevo proveedor.
        /// </summary>
        private ErrorDto Activos_Proveedores_Insertar(int CodEmpresa, string usuario, ActivosProveedoresData proveedor)
        {
            const string query = @"
                INSERT INTO dbo.ACTIVOS_PROVEEDORES
                    (COD_PROVEEDOR, DESCRIPCION, ACTIVO, REGISTRO_FECHA, REGISTRO_USUARIO, MODIFICA_USUARIO, MODIFICA_FECHA)
                VALUES
                    (@cod, @desc, @act, SYSDATETIME(), @usr, NULL, NULL);";

            var parameters = BuildProveedorDbParams(usuario, proveedor);
            string detalle = $"Proveedor: {proveedor.cod_proveedor} - {proveedor.descripcion}";

            return ExecuteNonQueryProveedor(
                CodEmpresa,
                usuario,
                query,
                parameters,
                detalle,
                "Registra - WEB",
                MsgProveedorIngresado);
        }

        /// <summary>
        /// Actualiza un proveedor existente.
        /// </summary>
        private ErrorDto Activos_Proveedores_Actualizar(int CodEmpresa, string usuario, ActivosProveedoresData proveedor)
        {
            const string query = @"
                UPDATE dbo.ACTIVOS_PROVEEDORES
                   SET DESCRIPCION      = @desc,
                       ACTIVO           = @act,
                       MODIFICA_USUARIO = @usr,
                       MODIFICA_FECHA   = SYSDATETIME()
                 WHERE COD_PROVEEDOR    = @cod;";

            var parameters = BuildProveedorDbParams(usuario, proveedor);
            string detalle = $"Proveedor: {proveedor.cod_proveedor} - {proveedor.descripcion}";

            return ExecuteNonQueryProveedor(
                CodEmpresa,
                usuario,
                query,
                parameters,
                detalle,
                "Modifica - WEB",
                MsgProveedorActualizado);
        }

        /// <summary>
        /// Eliminar proveedor por su código.
        /// </summary>
        public ErrorDto Activos_Proveedores_Eliminar(int CodEmpresa, string usuario, string cod_proveedor)
        {
            var resp = new ErrorDto { Code = 0, Description = MsgOk };

            try
            {
                if (string.IsNullOrWhiteSpace(cod_proveedor))
                    return new ErrorDto { Code = -1, Description = MsgDebeIndicarCodProveedor };

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
                resp.Code        = -1;
                resp.Description = ex.Message;
            }
            return resp;
        }

        /// <summary>
        /// Importar proveedores desde CXP_PROVEEDORES a ACTIVOS_PROVEEDORES.
        /// </summary>
        public ErrorDto Activos_Proveedores_Importar(int CodEmpresa, string usuario)
        {
            var resp = new ErrorDto { Code = 0, Description = MsgOk };

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
                resp.Code        = -1;
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
                Code        = 0,
                Description = MsgOk
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
                    resp.Code        = -1;
                    resp.Description = MsgCodProveedorExiste;
                }
                else
                {
                    resp.Code        = 0;
                    resp.Description = MsgCodProveedorValido;
                }
            }
            catch (Exception ex)
            {
                resp.Code        = -1;
                resp.Description = ex.Message;
            }

            return resp;
        }
    }
}