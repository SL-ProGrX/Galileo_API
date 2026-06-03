using Dapper;
using Microsoft.Data.SqlClient;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Personas;
using Galileo.Models.Security;

namespace Galileo.DataBaseTier.ProGrX_Personas
{
    public class FrmAFCrGestionesDB
    {
        private readonly IConfiguration _config;
        private readonly int vModulo = 1;
        private readonly MSecurityMainDb _Security_MainDB;

        public FrmAFCrGestionesDB(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _Security_MainDB = new MSecurityMainDb(_config);
        }

        /// <summary>
        /// Obtiene la lista de gestiones con filtros, orden y paginación.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDto<List<AfCrGestionesData>> AF_CRGestiones_Obtener(int CodEmpresa, FiltrosLazyLoadData filtros)
        {
            if (filtros is null)
            {
                return DbHelper.CreateErrorResponse("Los filtros de gestiones son requeridos.", -2, new List<AfCrGestionesData>());
            }

            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
            {
                var filtroTexto = filtros.filtro?.Trim();
                var sortField = ObtenerSortFieldGestiones(filtros.sortField);
                var sortDirection = ObtenerSortDirectionGestiones(filtros.sortOrder);
                var offsetRows = filtros.pagina;
                var fetchRows = filtros.paginacion;

                var sql = @"
                    SELECT cod_gestion, descripcion
                    FROM afi_cr_gestiones
                    WHERE (
                        @Filtro IS NULL
                        OR cod_gestion LIKE @Filtro
                        OR descripcion LIKE @Filtro
                    )
                    ORDER BY
                        CASE WHEN @SortField = 'cod_gestion' AND @SortDirection = 'ASC' THEN cod_gestion END ASC,
                        CASE WHEN @SortField = 'cod_gestion' AND @SortDirection = 'DESC' THEN cod_gestion END DESC,
                        CASE WHEN @SortField = 'descripcion' AND @SortDirection = 'ASC' THEN descripcion END ASC,
                        CASE WHEN @SortField = 'descripcion' AND @SortDirection = 'DESC' THEN descripcion END DESC,
                        cod_gestion ASC";

                if (fetchRows > 0)
                {
                    sql += " OFFSET @OffsetRows ROWS FETCH NEXT @FetchRows ROWS ONLY";
                }

                var parametros = new DynamicParameters();
                parametros.Add("Filtro", string.IsNullOrWhiteSpace(filtroTexto) ? null : $"%{filtroTexto}%");
                parametros.Add("SortField", sortField);
                parametros.Add("SortDirection", sortDirection);
                parametros.Add("OffsetRows", offsetRows);
                parametros.Add("FetchRows", fetchRows);

                return connection.Query<AfCrGestionesData>(sql, parametros).ToList();
            });

            return result.Code == 0
                ? DbHelper.CreateOkResponse(result.Result ?? new List<AfCrGestionesData>())
                : DbHelper.CreateErrorResponse(result.Description ?? "Error al obtener gestiones.", result.Code.GetValueOrDefault(-1), new List<AfCrGestionesData>());
        }

        /// <summary>
        /// Guarda (inserta o actualiza) una gestión.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="gestion"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto AF_CRGestiones_Guardar(int CodEmpresa, AfCrGestionesData gestion, string usuario)
        {
            if (gestion is null)
            {
                return DbHelper.ErrorResponse("Los datos de la gestión son requeridos.", -2);
            }

            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
            {
                var queryExiste = "select count(*) from afi_cr_gestiones where cod_gestion = @cod_gestion";
                var existe = connection.QueryFirstOrDefault<int>(queryExiste, new { cod_gestion = gestion.cod_gestion.ToUpper() });

                return existe > 0
                    ? AF_CRGestiones_Actualizar(connection, CodEmpresa, gestion, usuario)
                    : AF_CRGestiones_Insertar(connection, CodEmpresa, gestion, usuario);
            });

            return result.Code == 0 && result.Result is not null
                ? result.Result
                : DbHelper.ErrorResponse(result.Description ?? "Error al guardar gestión.", result.Code.GetValueOrDefault(-1));
        }

        private ErrorDto AF_CRGestiones_Insertar(SqlConnection connection, int codEmpresa, AfCrGestionesData gestion, string usuario)
        {
            connection.Execute(
                "insert into afi_cr_gestiones(cod_gestion, descripcion) values(@cod_gestion, @descripcion)",
                new
                {
                    cod_gestion = gestion.cod_gestion.ToUpper(),
                    descripcion = gestion.descripcion.ToUpper()
                });

            RegistrarBitacora(
                codEmpresa,
                usuario,
                $"Control Renuncia/Gestion: {gestion.cod_gestion} - {gestion.descripcion}",
                "Registra - WEB");

            return DbHelper.OkResponse("Insertado correctamente");
        }

        private ErrorDto AF_CRGestiones_Actualizar(SqlConnection connection, int codEmpresa, AfCrGestionesData gestion, string usuario)
        {
            connection.Execute(
                "update afi_cr_gestiones set descripcion = @descripcion where cod_gestion = @cod_gestion",
                new
                {
                    cod_gestion = gestion.cod_gestion.ToUpper(),
                    descripcion = gestion.descripcion.ToUpper()
                });

            RegistrarBitacora(
                codEmpresa,
                usuario,
                $"Control Renuncia/Gestion: {gestion.cod_gestion} - {gestion.descripcion}",
                "Modifica - WEB");

            return DbHelper.OkResponse("Actualizado correctamente");
        }

        /// <summary>
        /// Elimina una gestión por su código.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="cod_gestion"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto AF_CRGestiones_Eliminar(int CodEmpresa, string cod_gestion, string usuario)
        {
            var result = DbHelper.ExecuteNonQueryWithResult(
                CreatePortalDb(),
                CodEmpresa,
                "delete afi_cr_gestiones where cod_gestion = @cod_gestion",
                new { cod_gestion = cod_gestion.ToUpper() });

            if (result.Code != 0)
            {
                return DbHelper.ErrorResponse(result.Description ?? "Error al eliminar gestión.", result.Code.GetValueOrDefault(-1));
            }

            if (result.Result > 0)
            {
                RegistrarBitacora(
                    CodEmpresa,
                    usuario,
                    $"Control Renuncia/Gestion: {cod_gestion}",
                    "Elimina - WEB");

                return DbHelper.OkResponse("Eliminado correctamente");
            }

            return new ErrorDto
            {
                Code = 1,
                Description = "No se encontró el registro"
            };
        }

        private static string ObtenerSortFieldGestiones(string? sortField)
        {
            return sortField switch
            {
                "cod_gestion" => "cod_gestion",
                "descripcion" => "descripcion",
                _ => "cod_gestion"
            };
        }

        private static string ObtenerSortDirectionGestiones(int sortOrder)
        {
            return sortOrder == 1 ? "ASC" : "DESC";
        }

        private void RegistrarBitacora(int codEmpresa, string usuario, string detalleMovimiento, string movimiento)
        {
            _Security_MainDB.Bitacora(new BitacoraInsertarDto
            {
                EmpresaId = codEmpresa,
                Usuario = usuario,
                DetalleMovimiento = detalleMovimiento,
                Movimiento = movimiento,
                Modulo = vModulo
            });
        }

        private PortalDB CreatePortalDb() => new(_config);
    }
}
