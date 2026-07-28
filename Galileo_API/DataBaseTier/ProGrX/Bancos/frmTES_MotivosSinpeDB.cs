using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Bancos;

namespace Galileo_API.DataBaseTier.ProGrX.Bancos
{
    public class FrmTesMotivosSinpeDB
    {
        private readonly PortalDB _portalDB;

        public FrmTesMotivosSinpeDB(IConfiguration? config)
        {
            _portalDB = new PortalDB(config);
        }

        /// <summary>
        /// Obtiene los motivos de SINPE para una empresa específica.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDto<TesMotivosSinpeLista> TES_MotivoSinpe_Obtener(int CodEmpresa, FiltrosLazyLoadData filtros)
        {
            filtros ??= new FiltrosLazyLoadData();

            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            var result = new ErrorDto<TesMotivosSinpeLista>
            {
                Code = 0,
                Description = "Ok",
                Result = new TesMotivosSinpeLista
                {
                    total = 0,
                    lista = new List<TesMotivosSinpeDto>()
                }
            };

            try
            {
                var texto = filtros.filtro?.Trim();
                var hasFiltro = !string.IsNullOrWhiteSpace(texto);
                var like = hasFiltro ? $"%{texto}%" : null;

                var offset = filtros.pagina;
                var fetch = filtros.paginacion;
                var usarPaginacion = fetch > 0;

                // Si viene vacío, default
                var sortField = (filtros.sortField ?? "cod_motivo").Trim();
                if (string.IsNullOrWhiteSpace(sortField))
                    sortField = "cod_motivo";

                // Mantengo tu lógica original: sortOrder == 1 => DESC, else ASC
                var sortDesc = filtros.sortOrder == 1;

                const string sqlCount = @"
            SELECT COUNT(1)
            FROM SINPE_MOTIVOS
            WHERE
                (@filtro IS NULL)
             OR (CAST(cod_motivo AS NVARCHAR(50)) LIKE @like)
             OR (descripcion LIKE @like)
             OR (usuario_registro LIKE @like);";

                result.Result.total = conn.QuerySingle<int>(sqlCount, new
                {
                    filtro = hasFiltro ? texto : null,
                    like
                });

                // SQL fijo: nada de interpolación para ORDER BY.
                // Orden controlado por parámetros con CASE.
                var sqlList = @"
            SELECT
                cod_motivo,
                descripcion,
                usuario_registro,
                ACTIVO
            FROM SINPE_MOTIVOS
            WHERE
                (@filtro IS NULL)
             OR (CAST(cod_motivo AS NVARCHAR(50)) LIKE @like)
             OR (descripcion LIKE @like)
             OR (usuario_registro LIKE @like)
            ORDER BY
                -- ASC
                CASE WHEN @sortDesc = 0 AND @sortField = 'cod_motivo' THEN CAST(cod_motivo AS NVARCHAR(50)) END ASC,
                CASE WHEN @sortDesc = 0 AND @sortField = 'descripcion' THEN descripcion END ASC,
                CASE WHEN @sortDesc = 0 AND @sortField = 'usuario_registro' THEN usuario_registro END ASC,
                CASE WHEN @sortDesc = 0 AND (@sortField = 'ACTIVO' OR @sortField = 'activo') THEN CAST(ACTIVO AS INT) END ASC,

                -- DESC
                CASE WHEN @sortDesc = 1 AND @sortField = 'cod_motivo' THEN CAST(cod_motivo AS NVARCHAR(50)) END DESC,
                CASE WHEN @sortDesc = 1 AND @sortField = 'descripcion' THEN descripcion END DESC,
                CASE WHEN @sortDesc = 1 AND @sortField = 'usuario_registro' THEN usuario_registro END DESC,
                CASE WHEN @sortDesc = 1 AND (@sortField = 'ACTIVO' OR @sortField = 'activo') THEN CAST(ACTIVO AS INT) END DESC";

                if (usarPaginacion)
                {
                    sqlList += @"
            OFFSET @offset ROWS
            FETCH NEXT @fetch ROWS ONLY;";
                }

                result.Result.lista = conn.Query<TesMotivosSinpeDto>(sqlList, new
                {
                    filtro = hasFiltro ? texto : null,
                    like,
                    sortField,
                    sortDesc = sortDesc ? 1 : 0,
                    offset,
                    fetch
                }).ToList();
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result.total = 0;
                result.Result.lista = new List<TesMotivosSinpeDto>();
            }

            return result;
        }

        /// <summary>
        /// Obtiene los motivos de SINPE para exportar a Excel o CSV.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDto<List<TesMotivosSinpeDto>> TES_MotivoSinpeExportar_Obtener(int CodEmpresa, FiltrosLazyLoadData filtros)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                var texto = filtros?.filtro?.Trim();
                var hasFiltro = !string.IsNullOrWhiteSpace(texto);
                var like = hasFiltro ? $"%{texto}%" : null;

                // Valores “crudos” que vienen del UI (pero NO los inyectamos en SQL)
                var sortField = (filtros?.sortField ?? "").Trim();
                if (string.IsNullOrWhiteSpace(sortField))
                    sortField = "cod_motivo";

                // Mantengo tu lógica: 0 => DESC, otro => ASC
                var sortDesc = (filtros?.sortOrder ?? 0) == 0;

                const string sql = @"
            SELECT
                cod_motivo,
                descripcion,
                usuario_registro,
                ACTIVO
            FROM SINPE_MOTIVOS
            WHERE
                (@filtro IS NULL)
             OR (CAST(cod_motivo AS NVARCHAR(50)) LIKE @like)
             OR (descripcion LIKE @like)
             OR (usuario_registro LIKE @like)
            ORDER BY
                -- ASC
                CASE WHEN @sortDesc = 0 AND @sortField = 'cod_motivo' THEN CAST(cod_motivo AS NVARCHAR(50)) END ASC,
                CASE WHEN @sortDesc = 0 AND @sortField = 'descripcion' THEN descripcion END ASC,
                CASE WHEN @sortDesc = 0 AND @sortField = 'usuario_registro' THEN usuario_registro END ASC,
                CASE WHEN @sortDesc = 0 AND (@sortField = 'ACTIVO' OR @sortField = 'activo') THEN CAST(ACTIVO AS INT) END ASC,

                -- DESC
                CASE WHEN @sortDesc = 1 AND @sortField = 'cod_motivo' THEN CAST(cod_motivo AS NVARCHAR(50)) END DESC,
                CASE WHEN @sortDesc = 1 AND @sortField = 'descripcion' THEN descripcion END DESC,
                CASE WHEN @sortDesc = 1 AND @sortField = 'usuario_registro' THEN usuario_registro END DESC,
                CASE WHEN @sortDesc = 1 AND (@sortField = 'ACTIVO' OR @sortField = 'activo') THEN CAST(ACTIVO AS INT) END DESC;";

                var lista = conn.Query<TesMotivosSinpeDto>(sql, new
                {
                    filtro = hasFiltro ? texto : null,
                    like,
                    sortField,
                    sortDesc = sortDesc ? 1 : 0
                }).ToList();

                return DbHelper.CreateOkResponse(lista);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<List<TesMotivosSinpeDto>>(ex.Message);
            }
        }

        /// <summary>
        /// Método para guardar un motivo de SINPE.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="motivo"></param>
        /// <returns></returns>
        public ErrorDto TES_MotivoSinpe_Guardar(int CodEmpresa, string usuario, TesMotivosSinpeDto motivo)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);
            var result = new ErrorDto()
            {
                Code = 0,
                Description = "Ok"
            };
            try
            {
                //Verifico si existe usuario
                var qUsuario = $@"select count(cod_motivo) from SINPE_MOTIVOS where cod_motivo = @cod_motivo ";
                int existe = conn.QueryFirstOrDefault<int>(qUsuario, new { cod_motivo = motivo.cod_motivo });

                if (motivo.isNew)
                {
                    if (existe > 0)
                    {
                        result.Code = -2;
                        result.Description = $"El Motivo con el código {motivo.cod_motivo} ya existe.";
                    }
                    else
                    {
                        result = TES_MotivoSinpe_Insertar(CodEmpresa, usuario, motivo);
                    }
                }
                else if (existe == 0 && !motivo.isNew)
                {
                    result.Code = -2;
                    result.Description = $"El Motivo con el código {motivo.cod_motivo} no existe.";
                }
                else
                {
                    result = TES_MotivoSinpe_Actualizar(CodEmpresa, usuario, motivo);
                }
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
            }
            return result;
        }

        /// <summary>
        /// Inserta un nuevo motivo de SINPE en la base de datos.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="motivo"></param>
        /// <returns></returns>
        private ErrorDto TES_MotivoSinpe_Insertar(int CodEmpresa, string usuario, TesMotivosSinpeDto motivo)
        {
            const string query = $@"INSERT INTO SINPE_MOTIVOS (cod_motivo, descripcion, usuario_registro, fecha_registro,ACTIVO) 
                                   VALUES (@cod_motivo, @descripcion, @usuario_registro, getDate(), 1)";

            var parameters = new
            {
                cod_motivo = motivo.cod_motivo,
                descripcion = motivo.descripcion,
                usuario_registro = usuario
            };

            return DbHelper.ExecuteNonQuery(_portalDB, CodEmpresa, query, parameters);
        }

        /// <summary>
        /// Actualiza un motivo de SINPE existente en la base de datos.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="motivo"></param>
        /// <returns></returns>
        private ErrorDto TES_MotivoSinpe_Actualizar(int CodEmpresa, string usuario, TesMotivosSinpeDto motivo)
        {
            var query = $@"UPDATE SINPE_MOTIVOS 
                                   SET descripcion = @descripcion, usuario_actualiza = @usuario_actualiza, fecha_actualiza = getDate() , ACTIVO = @activo
                                   WHERE cod_motivo = @cod_motivo";

            var parameters = new
            {
                cod_motivo = motivo.cod_motivo,
                descripcion = motivo.descripcion,
                usuario_actualiza = usuario,
                activo = (motivo.activo ? 1 : 0)
            };  
            return DbHelper.ExecuteNonQuery(_portalDB, CodEmpresa, query, parameters);
        }

        /// <summary>
        /// Elimina un motivo de SINPE de la base de datos.<!---->
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="cod_motivo"></param>
        /// <returns></returns>
        public ErrorDto TES_MotivoSinpe_Eliminar(int CodEmpresa, string usuario, int cod_motivo)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);
            try
            {
                var existe = "select count(id_rechazo) from TES_TRANSACCIONES where ID_RECHAZO = @cod_motivo";
                int count = conn.QueryFirstOrDefault<int>(existe, new { cod_motivo });
                if (count > 0)
                {
                    return DbHelper.ErrorResponse("No se puede eliminar el motivo porque está siendo utilizado en transacciones.");
                }

                var query = $@"DELETE FROM SINPE_MOTIVOS WHERE cod_motivo = @cod_motivo";
                conn.Execute(query, new { cod_motivo });

                return DbHelper.CreateOkResponse();
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }
    }
}
