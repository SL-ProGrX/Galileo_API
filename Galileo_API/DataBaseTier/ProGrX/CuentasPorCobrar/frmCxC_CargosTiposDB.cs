using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR; 
using Galileo.Models;
using Galileo_API.Models.ProGrX.CuentasPorCobrar;
using Galileo.Models.Security; 

namespace Galileo_API.DataBaseTier.ProGrX.CuentasPorCobrar
{
    public class FrmCxCCargosTiposDB
    {

        private readonly PortalDB _portalDB;
        private readonly MSecurityMainDb _Security_MainDB;
        private readonly int vModulo = 31;
        private readonly MCntLinkDB mCntLink;

        public FrmCxCCargosTiposDB(IConfiguration config)
        {
            _portalDB = new PortalDB(config);
            _Security_MainDB = new MSecurityMainDb(config!);
            mCntLink = new MCntLinkDB(config);
        }

        /// <summary>
        /// Obtiene la lista de tipos de cargos .
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// <param name="esExportar"></param>
        /// <returns></returns>
        public ErrorDto<CxCCargosTiposLista> CxCCargosTiposLista_Obtener(int CodEmpresa, FiltrosLazyLoadData filtros, bool esExportar)
        {
            filtros ??= new FiltrosLazyLoadData();

            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            var result = new ErrorDto<CxCCargosTiposLista>
            {
                Code = 0,
                Description = "Ok",
                Result = new CxCCargosTiposLista
                {
                    total = 0,
                    lista = new List<CxCCargosTiposData>()
                }
            };

            try
            {
                var texto = filtros.filtro?.Trim();
                var hasFiltro = !string.IsNullOrWhiteSpace(texto);
                var like = hasFiltro ? $"%{texto}%" : null;

                var offset = filtros.pagina!;
                var fetch = filtros.paginacion!;
                var usarPaginacion = fetch > 0;

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

                const string sqlCount = @"
                        SELECT COUNT(1)
                        FROM cxc_cargos";

                result.Result.total = conn.QuerySingle<int>(sqlCount);

                var sqlList = $@"
                        SELECT
                            cod_cargo,
                            descripcion,
                            Tipo,
                            cod_cuenta,
                            activo                             
                        FROM cxc_cargos
                        WHERE
                            (@filtro IS NULL)
                         OR (CAST(cod_cargo AS NVARCHAR(50)) LIKE @like)
                         OR (descripcion LIKE @like)
                         OR (cod_cuenta LIKE @like)
                        ORDER BY {orderByField} {direction}";


                if (!esExportar && usarPaginacion)
                {
                    sqlList += @"
                        OFFSET @offset ROWS
                        FETCH NEXT @fetch ROWS ONLY;";
                    
                }
                result.Result.lista = conn.Query<CxCCargosTiposData>(sqlList, new
                {
                    filtro = hasFiltro ? texto : null,
                    like,
                    offset,
                    fetch
                }).ToList();

                foreach (var item in result.Result.lista)
                {
                    item.cod_cuenta_mask = mCntLink.fxgCntCuentaFormato(CodEmpresa, true, pCuenta: item.Cod_cuenta, 1);

                }

            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
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
        public ErrorDto CxCCargosTipos_Guardar(int CodEmpresa, string usuario, CxCCargosTiposData datos)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);
            try
            {
                var query = $@"select isnull(count(*),0) as Existe from cxc_cargos 
                              where cod_cargo = @Cod_cargo";
                var existe = conn.QueryFirstOrDefault<int>(query, new { datos.Cod_cargo });

                //string vCuenta = _mCnt.fxgCntCuentaFormato(CodEmpresa, false, concepto.cod_cuenta_mask, 0);
                //bool cuentaValida = _mCnt.fxgCntCuentaValida(CodEmpresa, vCuenta);
                //if (!cuentaValida)
                //{
                //    return DbHelper.ErrorResponse("La cuenta contable no es válida.");
                //}

                if (existe > 0)
                {
                    return CxCCargosTipos_Actualizar(CodEmpresa, usuario, datos);
                }
                else
                {
                    return CxCCargosTipos_Insertar(CodEmpresa, usuario, datos);
                }
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
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
            catch (Exception ex)
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
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
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
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
            return result;
        }

    }
}
