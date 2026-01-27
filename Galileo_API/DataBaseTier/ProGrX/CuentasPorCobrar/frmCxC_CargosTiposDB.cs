using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models;
using Galileo_API.Models.ProGrX.CuentasPorCobrar;
using Galileo.Models.Security;
using System.Data.Common;

namespace Galileo_API.DataBaseTier.ProGrX.CuentasPorCobrar
{
    public class FrmCxCCargosTiposDB
    {

        private readonly PortalDB _portalDB;
        private readonly MSecurityMainDb _Security_MainDB;
        private readonly int vModulo = 31;
        private readonly MCntLinkDB mCntLink;
        private const string Tabla = "dbo.cxc_cargos";

        public FrmCxCCargosTiposDB(IConfiguration config)
        {
            _portalDB = new PortalDB(config);
            _Security_MainDB = new MSecurityMainDb(config!);
            mCntLink = new MCntLinkDB(config);
        }

        private static ErrorDto Err(string msg) => DbHelper.ErrorResponse(msg);

        /// <summary>
        /// Obtiene la lista de tipos de cargos .
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// <param name="esExportar"></param>
        /// <returns></returns>
        public ErrorDto<CxCCargosTiposLista> CxCCargosTiposLista_Obtener(int codEmpresa, FiltrosLazyLoadData filtros, bool esExportar)
        {
            filtros ??= new FiltrosLazyLoadData();

            var result = new ErrorDto<CxCCargosTiposLista>
            {
                Code = 0,
                Description = "Ok",
                Result = new CxCCargosTiposLista { total = 0, lista = new List<CxCCargosTiposData>() }
            };

            try
            {
                using var conn = DbHelper.OpenConnection(_portalDB, codEmpresa);

                // Filtros
                var texto = filtros.filtro?.Trim();
                var hasFiltro = !string.IsNullOrWhiteSpace(texto);
                var like = hasFiltro ? $"%{texto}%" : null;

                var offset = filtros.pagina!;
                var fetch = filtros.paginacion!;
                var usarPaginacion = fetch > 0 && !esExportar;

                // Whitelist de columnas ordenables (evita inyección en ORDER BY)
                var sortField = (filtros.sortField ?? string.Empty).Trim();
                var orderByField = sortField switch
                {
                    "cod_cargo" => "cod_cargo",
                    "descripcion" => "descripcion",
                    "activo" => "activo",
                    "cod_cuenta" => "cod_cuenta",
                    _ => "cod_cargo"
                };
                var direction = filtros.sortOrder == 1 ? "DESC" : "ASC";

                // WHERE compartido para COUNT y SELECT (corrige bug: antes el COUNT no filtraba)
                const string where = @"
                    WHERE
                        (@filtro IS NULL)
                        OR (CAST(cod_cargo AS NVARCHAR(50)) LIKE @like)
                        OR (descripcion LIKE @like)
                        OR (cod_cuenta LIKE @like)";

                var sqlCount = $@"SELECT COUNT(1) FROM {Tabla} {where};";

                var sqlList = $@"
                    SELECT
                        cod_cargo,
                        descripcion,
                        Tipo,
                        cod_cuenta,
                        activo
                    FROM {Tabla}
                    {where}
                    ORDER BY {orderByField} {direction}";

                if (usarPaginacion)
                {
                    sqlList += @"
                        OFFSET @offset ROWS
                        FETCH NEXT @fetch ROWS ONLY;";
                }

                var @params = new
                {
                    filtro = hasFiltro ? texto : null,
                    like,
                    offset,
                    fetch
                };

                // Total filtrado
                result.Result.total = conn.QuerySingle<int>(sqlCount, @params);

                // Lista filtrada/paginada
                result.Result.lista = conn.Query<CxCCargosTiposData>(sqlList, @params).ToList();

                // Formateo de cuenta (null-safe)
                foreach (var item in result.Result.lista)
                {
                    item.cod_cuenta_mask = string.IsNullOrWhiteSpace(item.Cod_cuenta)
                        ? null
                        : mCntLink.fxgCntCuentaFormato(codEmpresa, blnMascara: true, pCuenta: item.Cod_cuenta, optMensaje: 1);
                }
            }
            catch (DbException)
            {
                result.Code = -1;
                result.Description = "No fue posible consultar los datos.";
                result.Result.total = 0;
                result.Result.lista = new List<CxCCargosTiposData>();
            }
            catch (Exception)
            {
                result.Code = -1;
                result.Description = "Error inesperado al consultar los datos.";
                result.Result.total = 0;
                result.Result.lista = new List<CxCCargosTiposData>();
            }

            return result;
        }


        /// <summary>
        /// Guarda o actualiza un tipo de cargo.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="datos"></param>
        /// <returns></returns>
        public ErrorDto CxCCargosTipos_Guardar(int codEmpresa, string usuario, CxCCargosTiposData datos)
        {
            if (datos is null) return Err("Datos requeridos.");
            if (string.IsNullOrWhiteSpace(datos.Cod_cargo)) return Err("El campo 'Cod_cargo' es requerido.");
            if (string.IsNullOrWhiteSpace(usuario)) return Err("El usuario es requerido.");

            try
            {
                using var conn = DbHelper.OpenConnection(_portalDB, codEmpresa);

                const string sqlExiste = $"SELECT ISNULL(COUNT(*),0) FROM {Tabla} WHERE cod_cargo = @Cod_cargo;";
                var existe = conn.QueryFirstOrDefault<int>(sqlExiste, new { datos.Cod_cargo });

                return existe > 0
                    ? CxCCargosTipos_Actualizar(codEmpresa, usuario, datos)
                    : CxCCargosTipos_Insertar(codEmpresa, usuario, datos);
            }
            catch (DbException)
            {
                return Err("No fue posible guardar el Tipo de Cargo de CxC.");
            }
            catch (Exception)
            {
                return Err("Error inesperado al guardar el Tipo de Cargo de CxC.");
            }
        }

        /// <summary>
        /// Inserta un nuevo registro
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="datos"></param>
        /// <returns></returns>
        private ErrorDto CxCCargosTipos_Insertar(int CodEmpresa, string usuario, CxCCargosTiposData datos)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);
            try
            {
                string query = @"
                            INSERT INTO cxc_cargos (cod_cargo,descripcion,Tipo,cod_cuenta,activo)
                            VALUES (
                                @Cod_cargo, @Descripcion, @Tipo, @cod_cuenta,
                                @activo)";

                conn.Execute(query, new
                {
                    datos.Cod_cargo,
                    datos.Descripcion,
                    datos.Tipo,
                    activo = datos.Activo,
                    cod_cuenta = datos.Cod_cuenta,
                });

                _Security_MainDB.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = usuario,
                    DetalleMovimiento = $"Tipo de Cargo de CxC: {datos.Cod_cargo}",
                    Movimiento = "Registra - WEB",
                    Modulo = vModulo
                });

                return DbHelper.OkResponse("Tipo de Cargo de CxC insertado correctamente.");
            }
           
             catch (DbException ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
            catch (Exception)
            {
                throw;
            }

        }

        /// <summary>
        /// Actualiza un registro existente
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="datos"></param>
        /// <returns></returns>
        private ErrorDto CxCCargosTipos_Actualizar(int CodEmpresa, string usuario, CxCCargosTiposData datos)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);
            try
            {
                string query = @"
                            UPDATE cxc_cargos
                            SET 
                                descripcion = @Descripcion,
                                Tipo = @Tipo,
                                cod_cuenta = @Cod_cuenta,
                                Activo = @Activo                                 
                            WHERE cod_cargo = @Cod_cargo";

                conn.Execute(query, new
                {
                    datos.Descripcion,
                    datos.Tipo,
                    datos.Cod_cuenta,
                    datos.Activo,
                    datos.Cod_cargo
                });

                _Security_MainDB.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = usuario,
                    DetalleMovimiento = $"Tipo de Cargo de CxC:  {datos.Cod_cargo}",
                    Movimiento = "MODIFICA - WEB",
                    Modulo = vModulo
                });

                return DbHelper.OkResponse("Tipo de Cargo de CxC actualizado correctamente.");
            }
            catch (DbException ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Elimina un tipo de cargo.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="CodCargo"></param>
        /// <returns></returns>
        public ErrorDto CxCCargosTipos_Eliminar(int CodEmpresa, string usuario, string CodCargo)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);
            var result = new ErrorDto()
            {
                Code = 0,
                Description = "Ok"
            };
            try
            {
                string query = $@"delete cxc_cargos where cod_cargo = @CodCargo";
                conn.Execute(query, new { CodCargo });
                _Security_MainDB.Bitacora(
                    new BitacoraInsertarDto
                    {
                        EmpresaId = CodEmpresa,
                        Usuario = usuario,
                        DetalleMovimiento = $"Tipo de Cargo de CxC:  {CodCargo}",
                        Movimiento = "ELIMINAR - WEB",
                        Modulo = vModulo
                    });
            }
            catch (DbException ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
            return result;
        }

    }
}
