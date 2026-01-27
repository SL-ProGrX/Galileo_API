using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models;
using Galileo_API.Models.ProGrX.CuentasPorCobrar;
using Galileo.Models.Security;
using System.Data.Common;

namespace Galileo_API.DataBaseTier.ProGrX.CuentasPorCobrar
{
    public class FrmCxCClientesClasificaDB
    {

        private readonly PortalDB _portalDB;
        private readonly MSecurityMainDb _Security_MainDB;
        private readonly int vModulo = 31;
        private const string Tabla = "dbo.CxC_Categoria_Clientes";

        public FrmCxCClientesClasificaDB(IConfiguration config)
        {
            _portalDB = new PortalDB(config);
            _Security_MainDB = new MSecurityMainDb(config!);
        }

        private static ErrorDto Err(string msg) => DbHelper.ErrorResponse(msg);

        /// <summary>
        /// Obtiene la lista de clasificación de clientes.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// <param name="esExportar"></param>
        /// <returns></returns>
        public ErrorDto<CxCClientesClasificaLista> CxCClientesClasificaLista_Obtener(int codEmpresa, FiltrosLazyLoadData filtros, bool esExportar)
        {
            filtros ??= new FiltrosLazyLoadData();

            var result = new ErrorDto<CxCClientesClasificaLista>
            {
                Code = 0,
                Description = "Ok",
                Result = new CxCClientesClasificaLista { total = 0, lista = new List<CxCClientesClasificaData>() }
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
                    "cod_categoria" => "cod_categoria",
                    "descripcion" => "descripcion",
                    "activa" => "activa", 
                    _ => "cod_categoria"
                };
                var direction = filtros.sortOrder == 1 ? "DESC" : "ASC";

                // WHERE compartido para COUNT y SELECT (corrige bug: antes el COUNT no filtraba)
                const string where = @"
                    WHERE
                        (@filtro IS NULL)
                        OR (CAST(cod_categoria AS NVARCHAR(50)) LIKE @like)
                        OR (descripcion LIKE @like) ";

                var sqlCount = $@"SELECT COUNT(1) FROM {Tabla} {where};";

                var sqlList = $@"
                    SELECT
                        cod_categoria,
                        descripcion, 
                        activa as  Activo
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

                result.Result.total = conn.QuerySingle<int>(sqlCount, @params);
                result.Result.lista = conn.Query<CxCClientesClasificaData>(sqlList, @params).ToList();

            }
            catch (DbException)
            {
                result.Code = -1;
                result.Description = "No fue posible consultar los datos.";
                result.Result.total = 0;
                result.Result.lista = new List<CxCClientesClasificaData>();
            }
            catch (Exception)
            {
                result.Code = -1;
                result.Description = "Error inesperado al consultar los datos.";
                result.Result.total = 0;
                result.Result.lista = new List<CxCClientesClasificaData>();
            }

            return result;
        }

        /// <summary>
        /// Guarda o actualiza una clasificación.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="datos"></param>
        /// <returns></returns>
        public ErrorDto CxCClientesClasifica_Guardar(int codEmpresa, string usuario, CxCClientesClasificaData datos)
        {
            if (datos is null) return Err("Datos requeridos.");
            if (string.IsNullOrWhiteSpace(datos.Cod_categoria)) return Err("El campo 'cod_categoria' es requerido.");
            if (string.IsNullOrWhiteSpace(usuario)) return Err("El usuario es requerido.");

            try
            {
                using var conn = DbHelper.OpenConnection(_portalDB, codEmpresa);

                const string sqlExiste = $"SELECT ISNULL(COUNT(*),0) FROM {Tabla} WHERE cod_categoria = @cod_categoria;";
                var existe = conn.QueryFirstOrDefault<int>(sqlExiste, new { datos.Cod_categoria });

                return existe > 0
                    ? CxCClientesClasifica_Actualizar(codEmpresa, usuario, datos)
                    : CxCClientesClasifica_Insertar(codEmpresa, usuario, datos);
            }
            catch (DbException)
            {
                return Err("No fue posible guardar la clasificacion de CxC.");
            }
            catch (Exception)
            {
                return Err("Error inesperado al guardar la clasificacion de CxC.");
            }
        }

        /// <summary>
        /// Inserta un nuevo registro
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="datos"></param>
        /// <returns></returns>
        private ErrorDto CxCClientesClasifica_Insertar(int CodEmpresa, string usuario, CxCClientesClasificaData datos)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);
            try
            {
                string query = @"
                            INSERT INTO CxC_Categoria_Clientes (cod_categoria,descripcion,activa,registro_fecha,registro_usuario)
                            VALUES (
                                @Cod_categoria, @Descripcion,@Activo,dbo.MyGetdate(),@usuario)";

                conn.Execute(query, new
                {
                    datos.Cod_categoria,
                    datos.Descripcion, 
                     datos.Activo,
                    usuario
                });

                _Security_MainDB.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = usuario,
                    DetalleMovimiento = $"Categoria de Cliente : {datos.Cod_categoria}",
                    Movimiento = "Registra - WEB",
                    Modulo = vModulo
                });

                return DbHelper.OkResponse("Categoria de Cliente insertado correctamente.");
            }

            catch (DbException ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }

        }

        /// <summary>
        /// Actualiza un registro existente
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="datos"></param>
        /// <returns></returns>
        private ErrorDto CxCClientesClasifica_Actualizar(int CodEmpresa, string usuario, CxCClientesClasificaData datos)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);
            try
            {
                string query = @"
                            UPDATE CxC_Categoria_Clientes
                            SET 
                                descripcion = @Descripcion,                              
                                activa = @Activo                                 
                            WHERE cod_categoria = @cod_categoria";

                conn.Execute(query, new
                {
                    datos.Descripcion, 
                    datos.Activo,
                    datos.Cod_categoria
                });

                _Security_MainDB.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = usuario,
                    DetalleMovimiento = $"Categoria de Cliente :  {datos.Cod_categoria}",
                    Movimiento = "MODIFICA - WEB",
                    Modulo = vModulo
                });

                return DbHelper.OkResponse("Categoria de Cliente actualizado correctamente.");
            }
            catch (DbException ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }

        }

        /// <summary>
        /// Elimina una clasificación.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="CodCargo"></param>
        /// <returns></returns>
        public ErrorDto CxCClientesClasifica_Eliminar(int CodEmpresa, string usuario, string CodCategoria)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);
            var result = new ErrorDto()
            {
                Code = 0,
                Description = "Ok"
            };
            try
            {
                string query = $@"delete CxC_Categoria_Clientes where cod_categoria = @CodCategoria";
                conn.Execute(query, new { CodCategoria });
                _Security_MainDB.Bitacora(
                    new BitacoraInsertarDto
                    {
                        EmpresaId = CodEmpresa,
                        Usuario = usuario,
                        DetalleMovimiento = $"Categoria de Cliente : {CodCategoria}",
                        Movimiento = "ELIMINAR - WEB",
                        Modulo = vModulo
                    });
            }
            catch (DbException ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
            return result;
        }

    }
}
