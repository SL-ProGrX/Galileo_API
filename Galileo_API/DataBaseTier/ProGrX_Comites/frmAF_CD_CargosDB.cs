using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models;
using Galileo.Models.Security;
using System.Data.Common;
using static Galileo_API.Models.ProGrX_Comites.FrmAfCdCargos;

namespace Galileo_API.DataBaseTier.ProGrX_Comites
{
    public class FrmAfCdCargosDB
    {

        private readonly PortalDB _portalDB;
        private readonly MSecurityMainDb _Security_MainDB;
        private readonly int vModulo = 40;
        private readonly MCntLinkDB mCntLink;
        private const string Tabla = "dbo.AFI_CD_CARGOS";

        public FrmAfCdCargosDB(IConfiguration config)
        {
            _portalDB = new PortalDB(config);
            _Security_MainDB = new MSecurityMainDb(config!);
            mCntLink = new MCntLinkDB(config);
        }

        private static ErrorDto Err(string msg) => DbHelper.ErrorResponse(msg);

        public ErrorDto<CdCargosLista> CdCargosLista_Obtener(int codEmpresa, FiltrosLazyLoadData filtros, bool esExportar)
        {
            filtros ??= new FiltrosLazyLoadData();

            var result = new ErrorDto<CdCargosLista>
            {
                Code = 0,
                Description = "Ok",
                Result = new CdCargosLista {Total = 0, lista = new List<CdCargosData>() }
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
                    "Codigo" => "Codigo",
                    "descripcion" => "descripcion",
                    "cuenta" => "cuenta",
                    "estado" => "estado", 
                    _ => "Codigo"
                };
                var direction = filtros.sortOrder == 1 ? "DESC" : "ASC";

                // WHERE compartido para COUNT y SELECT (corrige bug: antes el COUNT no filtraba)
                const string where = @"
                    WHERE
                        (@filtro IS NULL)
                        OR (CAST(Codigo AS NVARCHAR(50)) LIKE @like)
                        OR (descripcion LIKE @like)
                        OR (cuenta LIKE @like)";

                var sqlCount = $@"SELECT COUNT(1) FROM {Tabla} {where};";

                var sqlList = $@"
                    SELECT
                        Codigo,
                        descripcion,
                        cuenta,
                        estado
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
                result.Result.Total = conn.QuerySingle<int>(sqlCount, @params);

                // Lista filtrada/paginada
                result.Result.lista = conn.Query<CdCargosData>(sqlList, @params).ToList();

                //// Formateo de cuenta (null-safe)
                //foreach (var item in result.Result.lista)
                //{
                //    item.cod_cuenta_mask = string.IsNullOrWhiteSpace(item.Cod_cuenta)
                //        ? null
                //        : mCntLink.fxgCntCuentaFormato(codEmpresa, blnMascara: true, pCuenta: item.Cod_cuenta, optMensaje: 1);
                //}
            }
            catch (DbException)
            {
                result.Code = -1;
                result.Description = "No fue posible consultar los datos.";
                result.Result.Total = 0;
                result.Result.lista = new List<CdCargosData>();
            }
            catch (Exception)
            {
                result.Code = -1;
                result.Description = "Error inesperado al consultar los datos.";
                result.Result.Total = 0;
                result.Result.lista = new List<CdCargosData>();
            }

            return result;
        }

       
        public ErrorDto AfCdCargos_Guardar(int codEmpresa, string usuario, CdCargosData datos)
        {
            if (datos is null) return Err("Datos requeridos.");
            if (string.IsNullOrWhiteSpace(usuario)) return Err("El usuario es requerido.");

            try
            {
                using var conn = DbHelper.OpenConnection(_portalDB, codEmpresa);

                const string sqlExiste = $"SELECT ISNULL(COUNT(*),0) FROM {Tabla} WHERE Codigo = @Codigo;";
                var existe = conn.QueryFirstOrDefault<int>(sqlExiste, new { datos.Codigo });

                return existe > 0
                    ? AfCdCargos_Actualizar(codEmpresa, usuario, datos)
                    : AfCdCargos_Insertar(codEmpresa, usuario, datos);
            }
            catch (DbException)
            {
                return Err("No fue posible guardar el Cargo.");
            }
            catch (Exception)
            {
                return Err("Error inesperado al guardar el Cargo.");
            }
        }

 
        private ErrorDto AfCdCargos_Insertar(int CodEmpresa, string usuario, CdCargosData datos)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);
            try
            {
                const string queryCodigo = @"
                            SELECT COALESCE(MAX(Codigo), 0) + 1
                            FROM AFI_CD_CARGOS";
                int nuevoCodigo = conn.ExecuteScalar<int>(queryCodigo);

                string query = @"
                            INSERT INTO AFI_CD_CARGOS (Codigo,descripcion,cuenta,estado)
                            VALUES (
                                @Codigo, @Descripcion, @cuenta,
                                @estado)";

                conn.Execute(query, new
                {
                    Codigo = nuevoCodigo,
                    datos.Descripcion,
                    estado = datos.Estado,
                    cuenta = datos.Cuenta,
                });

                _Security_MainDB.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = usuario,
                    DetalleMovimiento = $"Cargo: {datos.Codigo}",
                    Movimiento = "Registra - WEB",
                    Modulo = vModulo
                });

                return DbHelper.OkResponse("Cargo insertado correctamente.");
            }

            catch (DbException ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }

        }

    
        private ErrorDto AfCdCargos_Actualizar(int CodEmpresa, string usuario, CdCargosData datos)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);
            try
            {
                string query = @"
                            UPDATE AFI_CD_CARGOS
                            SET 
                                descripcion = @Descripcion,
                                cuenta = @Cuenta,
                                Estado = @Estado                                 
                            WHERE Codigo = @Codigo";

                conn.Execute(query, new
                {
                    datos.Descripcion, 
                    datos.Cuenta,
                    datos.Estado,
                    datos.Codigo
                });

                _Security_MainDB.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = usuario,
                    DetalleMovimiento = $"Cargo :  {datos.Codigo}",
                    Movimiento = "MODIFICA - WEB",
                    Modulo = vModulo
                });

                return DbHelper.OkResponse("Cargo actualizado correctamente.");
            }
            catch (DbException ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }

        }


        public ErrorDto AfCdCargos_Eliminar(int CodEmpresa, string usuario, string CodCargo)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);
            var result = new ErrorDto()
            {
                Code = 0,
                Description = "Ok"
            };
            try
            {
                string query = $@"delete AFI_CD_CARGOS where Codigo = @CodCargo";
                conn.Execute(query, new { CodCargo });
                _Security_MainDB.Bitacora(
                    new BitacoraInsertarDto
                    {
                        EmpresaId = CodEmpresa,
                        Usuario = usuario,
                        DetalleMovimiento = $"Cargo :  {CodCargo}",
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
