using Dapper;
using Microsoft.Data.SqlClient;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Personas;
using Galileo.Models.Security;

namespace Galileo.DataBaseTier.ProGrX_Personas
{
    public class FrmAFCrMotivosDB
    {
        private readonly IConfiguration _config;
        private readonly int vModulo = 1;
        private readonly MSecurityMainDb _Security_MainDB;

        public FrmAFCrMotivosDB(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _Security_MainDB = new MSecurityMainDb(_config);
        }

        /// <summary>
        /// Obtiene la lista de motivos de renuncia con filtros, orden y paginación.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDto<List<AfCrMotivosData>> AF_CRMotivos_Obtener(int CodEmpresa, FiltrosLazyLoadData filtros)
        {
            if (filtros is null)
            {
                return DbHelper.CreateErrorResponse("Los filtros de motivos son requeridos.", -2, new List<AfCrMotivosData>());
            }

            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
            {
                var filtroTexto = filtros.filtro?.Trim();
                var sortField = ObtenerSortFieldMotivos(filtros.sortField);
                var sortDirection = ObtenerSortDirectionMotivos(filtros.sortOrder);
                var offsetRows = filtros.pagina;
                var fetchRows = filtros.paginacion;

                var sql = @"
                    SELECT COD_MOTIVO, descripcion, ACTIVO, registro_fecha, registro_usuario
                    FROM dbo.AFI_CR_MOTIVOS_RENUNCIA
                    WHERE (
                        @Filtro IS NULL
                        OR descripcion LIKE @Filtro
                        OR COD_MOTIVO LIKE @Filtro
                    )
                    ORDER BY
                        CASE WHEN @SortField = 'COD_MOTIVO' AND @SortDirection = 'ASC' THEN COD_MOTIVO END ASC,
                        CASE WHEN @SortField = 'COD_MOTIVO' AND @SortDirection = 'DESC' THEN COD_MOTIVO END DESC,
                        CASE WHEN @SortField = 'descripcion' AND @SortDirection = 'ASC' THEN descripcion END ASC,
                        CASE WHEN @SortField = 'descripcion' AND @SortDirection = 'DESC' THEN descripcion END DESC,
                        CASE WHEN @SortField = 'ACTIVO' AND @SortDirection = 'ASC' THEN CAST(ACTIVO AS INT) END ASC,
                        CASE WHEN @SortField = 'ACTIVO' AND @SortDirection = 'DESC' THEN CAST(ACTIVO AS INT) END DESC,
                        CASE WHEN @SortField = 'registro_fecha' AND @SortDirection = 'ASC' THEN registro_fecha END ASC,
                        CASE WHEN @SortField = 'registro_fecha' AND @SortDirection = 'DESC' THEN registro_fecha END DESC,
                        CASE WHEN @SortField = 'registro_usuario' AND @SortDirection = 'ASC' THEN registro_usuario END ASC,
                        CASE WHEN @SortField = 'registro_usuario' AND @SortDirection = 'DESC' THEN registro_usuario END DESC,
                        COD_MOTIVO ASC";

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

                return connection.Query<AfCrMotivosData>(sql, parametros).ToList();
            });

            return result.Code == 0
                ? DbHelper.CreateOkResponse(result.Result ?? new List<AfCrMotivosData>())
                : DbHelper.CreateErrorResponse(result.Description ?? "Error al obtener motivos.", result.Code.GetValueOrDefault(-1), new List<AfCrMotivosData>());
        }

        /// <summary>
        /// Guarda (inserta o actualiza) un motivo de renuncia.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="motivo"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto AF_CRMotivos_Guardar(int CodEmpresa, AfCrMotivosData motivo, string usuario)
        {
            if (motivo is null)
            {
                return DbHelper.ErrorResponse("Los datos del motivo son requeridos.", -2);
            }

            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
            {
                var existe = MotivoExiste(connection, motivo.cod_motivo);

                return existe
                    ? ActualizarMotivo(connection, CodEmpresa, motivo, usuario)
                    : InsertarMotivo(connection, CodEmpresa, motivo, usuario);
            });

            return result.Code == 0 && result.Result is not null
                ? result.Result
                : DbHelper.ErrorResponse(result.Description ?? "Error al guardar motivo de renuncia.", result.Code.GetValueOrDefault(-1));
        }

        /// <summary>
        /// Elimina un motivo de renuncia por su código.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="cod_motivo"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto AF_CRMotivos_Eliminar(int CodEmpresa, string cod_motivo, string usuario)
        {
            var result = DbHelper.ExecuteNonQueryWithResult(
                CreatePortalDb(),
                CodEmpresa,
                "DELETE FROM dbo.AFI_CR_MOTIVOS_RENUNCIA WHERE COD_MOTIVO = @COD_MOTIVO",
                new { COD_MOTIVO = cod_motivo.ToUpper() });

            if (result.Code != 0)
            {
                return DbHelper.ErrorResponse(result.Description ?? "Error al eliminar motivo de renuncia.", result.Code.GetValueOrDefault(-1));
            }

            if (result.Result > 0)
            {
                RegistrarBitacora(
                    CodEmpresa,
                    usuario,
                    $"Motivo Renuncia: {cod_motivo}",
                    "Elimina - WEB");

                return DbHelper.OkResponse("Eliminado correctamente");
            }

            return new ErrorDto
            {
                Code = 1,
                Description = "No se encontró el registro"
            };
        }

        // Métodos privados
        private bool MotivoExiste(SqlConnection connection, string cod_motivo)
        {
            var queryExiste = "SELECT ISNULL(COUNT(*),0) FROM dbo.AFI_CR_MOTIVOS_RENUNCIA WHERE COD_MOTIVO = @COD_MOTIVO";
            var existe = connection.QueryFirstOrDefault<int>(queryExiste, new { COD_MOTIVO = cod_motivo.ToUpper() });
            return existe > 0;
        }

        private ErrorDto InsertarMotivo(SqlConnection connection, int codEmpresa, AfCrMotivosData motivo, string usuario)
        {
            var queryInsert = @"INSERT INTO dbo.AFI_CR_MOTIVOS_RENUNCIA
                                (COD_MOTIVO, descripcion, ACTIVO, registro_fecha, registro_usuario)
                                VALUES (UPPER(@COD_MOTIVO), @descripcion, @ACTIVO, dbo.myGetdate(), @registro_usuario)";
            connection.Execute(queryInsert, new
            {
                COD_MOTIVO = motivo.cod_motivo.ToUpper(),
                descripcion = motivo.descripcion,
                ACTIVO = motivo.activo,
                registro_usuario = usuario
            });

            RegistrarBitacora(
                codEmpresa,
                usuario,
                $"Motivo Renuncia: {motivo.cod_motivo} - {motivo.descripcion}",
                "Registra - WEB");

            return DbHelper.OkResponse("Insertado correctamente");
        }

        private ErrorDto ActualizarMotivo(SqlConnection connection, int codEmpresa, AfCrMotivosData motivo, string usuario)
        {
            var queryUpdate = @"UPDATE dbo.AFI_CR_MOTIVOS_RENUNCIA
                                SET descripcion = @descripcion,
                                    ACTIVO = @ACTIVO
                                WHERE COD_MOTIVO = @COD_MOTIVO";
            connection.Execute(queryUpdate, new
            {
                COD_MOTIVO = motivo.cod_motivo.ToUpper(),
                descripcion = motivo.descripcion,
                ACTIVO = motivo.activo
            });

            RegistrarBitacora(
                codEmpresa,
                usuario,
                $"Motivo Renuncia: {motivo.cod_motivo} - {motivo.descripcion}",
                "Modifica - WEB");

            return DbHelper.OkResponse("Actualizado correctamente");
        }

        private static string ObtenerSortFieldMotivos(string? sortField)
        {
            return sortField switch
            {
                "COD_MOTIVO" => "COD_MOTIVO",
                "descripcion" => "descripcion",
                "ACTIVO" => "ACTIVO",
                "registro_fecha" => "registro_fecha",
                "registro_usuario" => "registro_usuario",
                _ => "COD_MOTIVO"
            };
        }

        private static string ObtenerSortDirectionMotivos(int sortOrder)
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
