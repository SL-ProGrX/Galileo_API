using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.ProGrX_Personas;
using PgxAPI.Models.Security;

namespace PgxAPI.DataBaseTier.ProGrX_Personas
{
    public class frmAF_CausasRenunciasDB
    {
        private readonly IConfiguration? _config;
        private readonly int vModulo = 1;
        private readonly MSecurityMainDb _Security_MainDB;

        public frmAF_CausasRenunciasDB(IConfiguration? config)
        {
            _config = config;
            _Security_MainDB = new MSecurityMainDb(_config);
        }

        /// <summary>
        /// Obtiene la lista de causas de renuncia con filtros, orden y paginación.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDto<List<AfCausasRenunciasData>> AF_CausasRenuncias_Obtener(int CodEmpresa, FiltrosLazyLoadData filtros)
        {
            var result = new ErrorDto<List<AfCausasRenunciasData>>()
            {
                Code = 0,
                Description = "OK",
                Result = new List<AfCausasRenunciasData>()
            };
            try
            {
                string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var connection = new SqlConnection(stringConn);

                string where = "";
                object param = new { };
                if (!string.IsNullOrWhiteSpace(filtros.filtro))
                {
                    where = " WHERE descripcion LIKE @filtro OR cod_plan LIKE @filtro ";
                    param = new { filtro = $"%{filtros.filtro}%" };
                }

                string orderBy = " ORDER BY id_causa ";
                if (!string.IsNullOrEmpty(filtros.sortField))
                {
                    orderBy = $" ORDER BY {filtros.sortField} {(filtros.sortOrder == 0 ? "DESC" : "ASC")} ";
                }

                string paginacion = "";
                if (filtros.paginacion > 0)
                {
                    paginacion = $" OFFSET {filtros.pagina} ROWS FETCH NEXT {filtros.paginacion} ROWS ONLY ";
                }

                string query = $"SELECT Id_Causa AS id_causa, Descripcion AS descripcion, Tipo_Apl AS tipo_apl, mortalidad, AJUSTE_TASAS AS ajuste_tasas, liq_alterna, tasa_planilla, tasa_ventanilla, institucion, cod_Plan AS cod_plan, activo FROM vAFI_Causas_Renuncias{where}{orderBy}{paginacion}";
                result.Result = connection.Query<AfCausasRenunciasData>(query, param).ToList();
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = null;
            }
            return result;
        }

        /// <summary>
        /// Guarda (inserta o actualiza) una causa de renuncia.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="causa"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto AF_CausasRenuncias_Guardar(int CodEmpresa, AfCausasRenunciasData causa, string usuario)
        {
            try
            {
                string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var connection = new SqlConnection(stringConn);

                var queryExiste = "SELECT COUNT(*) FROM causas_renuncias WHERE id_causa = @id_causa";
                var existe = connection.QueryFirstOrDefault<int>(queryExiste, new { id_causa = causa.id_causa });

                if (existe > 0)
                {
                    return AF_CausasRenuncias_Actualizar(connection, causa, CodEmpresa, usuario);
                }
                else
                {
                    return AF_CausasRenuncias_Insertar(connection, causa, CodEmpresa, usuario);
                }
            }
            catch (Exception ex)
            {
                return new ErrorDto
                {
                    Code = -1,
                    Description = ex.Message
                };
            }
        }

        private ErrorDto AF_CausasRenuncias_Insertar(SqlConnection connection, AfCausasRenunciasData causa, int CodEmpresa, string usuario)
        {
            var result = new ErrorDto()
            {
                Code = 0,
                Description = "Insertado correctamente"
            };
            try
            {
                var queryInsert = @"INSERT INTO causas_renuncias (
                        descripcion, Tipo_Apl, mortalidad, AJUSTE_TASAS, liq_alterna,
                        tasa_planilla, tasa_ventanilla, institucion, Cod_Plan, Activo)
                        VALUES (@descripcion, @tipo_apl, @mortalidad, @ajuste_tasas, @liq_alterna,
                                @tasa_planilla, @tasa_ventanilla, @institucion, @cod_plan, @activo);
                        SELECT CAST(SCOPE_IDENTITY() AS INT) AS new_id;";
                int newId = connection.QuerySingle<int>(queryInsert, new
                {
                    causa.descripcion,
                    tipo_apl = causa.tipo_apl[0],
                    causa.mortalidad,
                    causa.ajuste_tasas,
                    causa.liq_alterna,
                    causa.tasa_planilla,
                    causa.tasa_ventanilla,
                    causa.institucion,
                    causa.cod_plan,
                    causa.activo
                });
                result.Description += $" (Id: {newId})";

                _Security_MainDB.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = usuario,
                    DetalleMovimiento = $"Causa de Renuncia: {newId} - {causa.descripcion}",
                    Movimiento = "Registra - WEB",
                    Modulo = vModulo
                });
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
            }
            return result;
        }

        private ErrorDto AF_CausasRenuncias_Actualizar(SqlConnection connection, AfCausasRenunciasData causa, int CodEmpresa, string usuario)
        {
            var result = new ErrorDto()
            {
                Code = 0,
                Description = "Actualizado correctamente"
            };
            try
            {
                var queryUpdate = @"UPDATE causas_renuncias SET
                        descripcion     = @descripcion,
                        Tipo_Apl        = @tipo_apl,
                        mortalidad      = @mortalidad,
                        AJUSTE_TASAS    = @ajuste_tasas,
                        liq_alterna     = @liq_alterna,
                        tasa_planilla   = @tasa_planilla,
                        tasa_ventanilla = @tasa_ventanilla,
                        institucion     = @institucion,
                        Cod_Plan        = @cod_plan,
                        Activo          = @activo
                        WHERE id_causa  = @id_causa";
                connection.Execute(queryUpdate, new
                {
                    causa.id_causa,
                    causa.descripcion,
                    tipo_apl = causa.tipo_apl[0],
                    causa.mortalidad,
                    causa.ajuste_tasas,
                    causa.liq_alterna,
                    causa.tasa_planilla,
                    causa.tasa_ventanilla,
                    causa.institucion,
                    causa.cod_plan,
                    causa.activo
                });

                _Security_MainDB.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = usuario,
                    DetalleMovimiento = $"Causa de Renuncia: {causa.id_causa} - {causa.descripcion}",
                    Movimiento = "Modifica - WEB",
                    Modulo = vModulo
                });
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
            }
            return result;
        }

        /// <summary>
        /// Elimina una causa de renuncia por su identificador.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="id_causa"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto AF_CausasRenuncias_Eliminar(int CodEmpresa, int id_causa, string usuario)
        {
            var result = new ErrorDto()
            {
                Code = 0,
                Description = "OK"
            };
            try
            {
                string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var connection = new SqlConnection(stringConn);
                var queryDelete = "DELETE FROM causas_renuncias WHERE id_causa = @id_causa";
                int rows = connection.Execute(queryDelete, new { id_causa });
                if (rows > 0)
                {
                    result.Description = "Eliminado correctamente";
                    _Security_MainDB.Bitacora(new BitacoraInsertarDto
                    {
                        EmpresaId = CodEmpresa,
                        Usuario = usuario,
                        DetalleMovimiento = $"Causa de Renuncia: {id_causa}",
                        Movimiento = "Elimina - WEB",
                        Modulo = vModulo
                    });
                }
                else
                {
                    result.Code = 1;
                    result.Description = "No se encontró el registro";
                }
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
            }
            return result;
        }
    }
}
