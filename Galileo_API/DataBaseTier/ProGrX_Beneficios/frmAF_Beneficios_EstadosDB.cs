using Dapper;
using Galileo.Models.AF;
using Galileo.Models.ERROR;

namespace Galileo.DataBaseTier.ProGrX_Beneficios
{
    /// <summary>
    /// Acceso a datos del catálogo de Estados de Beneficios (frmAF_Beneficios_Estados).
    /// </summary>
    public partial class FrmAfBeneficiosEstadosDB
    {
        private readonly IConfiguration _config;

        /// <summary>
        /// Inicializa el acceso a datos con la configuración inyectada.
        /// </summary>
        /// <param name="config">Configuración de la aplicación.</param>
        public FrmAfBeneficiosEstadosDB(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        /// <summary>
        /// Crea una instancia de acceso al portal usando la configuración inyectada.
        /// </summary>
        private PortalDB CreatePortalDb() => new(_config);

        /// <summary>
        /// Obtiene la lista de estados de beneficios con paginación y filtro.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="pagina">Offset de paginación.</param>
        /// <param name="paginacion">Cantidad de registros por página.</param>
        /// <param name="filtro">Filtro de búsqueda por código o descripción.</param>
        /// <returns>Lista de estados y total.</returns>
        public ErrorDto<BeneEstadoDataLista> BeneficiosEstados_Obtener(int CodEmpresa, int? pagina, int? paginacion, string? filtro)
        {
            return DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
            {
                var response = new BeneEstadoDataLista();

                const string sqlCount = "SELECT COUNT(*) FROM AFI_BENE_ESTADOS";
                response.Total = connection.QueryFirstOrDefault<int>(sqlCount);

                var like = string.IsNullOrWhiteSpace(filtro) ? null : $"%{filtro}%";
                var offset = pagina ?? 0;
                var fetch = paginacion ?? 10;

                const string sql = @"SELECT cod_estado, descripcion, activo, orden, p_inicia, p_finaliza,
                                            registro_fecha, registro_usuario, modifica_fecha, modifica_usuario, proceso
                                     FROM AFI_BENE_ESTADOS
                                     WHERE (@like IS NULL OR COD_ESTADO LIKE @like OR DESCRIPCION LIKE @like)
                                     ORDER BY COD_ESTADO
                                     OFFSET @offset ROWS FETCH NEXT @fetch ROWS ONLY";

                response.Lista = connection.Query<BeneEstado>(sql, new { like, offset, fetch }).ToList();
                return response;
            });
        }

        /// <summary>
        /// Inserta un estado de beneficio, validando que el código no exista.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="request">Datos del estado.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto BeneficiosEstados_Agregar(int CodEmpresa, BeneEstado request)
        {
            using var connection = DbHelper.OpenConnection(CreatePortalDb(), CodEmpresa);
            try
            {
                const string sqlExiste = "SELECT COUNT(*) FROM AFI_BENE_ESTADOS WHERE COD_ESTADO = @cod_estado";
                var existe = connection.QueryFirstOrDefault<int>(sqlExiste, new { request.cod_estado });

                if (existe > 0)
                {
                    return DbHelper.ErrorResponse("Ya existe un estado con el codigo: " + request.cod_estado + ", por favor verifique");
                }

                const string sql = @"INSERT INTO AFI_BENE_ESTADOS
                                        (cod_estado, descripcion, activo, orden, p_inicia, p_finaliza,
                                         registro_fecha, registro_usuario, proceso)
                                     VALUES
                                        (@cod_estado, @descripcion, @activo, @orden, @p_inicia, @p_finaliza,
                                         GETDATE(), @registro_usuario, @proceso)";

                connection.Execute(sql, new
                {
                    request.cod_estado,
                    request.descripcion,
                    activo = request.activo ? 1 : 0,
                    request.orden,
                    p_inicia = request.p_inicia ? 1 : 0,
                    p_finaliza = request.p_finaliza ? 1 : 0,
                    request.registro_usuario,
                    request.proceso
                });

                return DbHelper.OkResponse("Estado agregado correctamente");
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        /// <summary>
        /// Actualiza un estado de beneficio.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="request">Datos del estado.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto BeneficiosEstados_Actualizar(int CodEmpresa, BeneEstado request)
        {
            const string sql = @"UPDATE AFI_BENE_ESTADOS
                                 SET descripcion = @descripcion, activo = @activo, orden = @orden,
                                     p_inicia = @p_inicia, p_finaliza = @p_finaliza,
                                     modifica_fecha = GETDATE(), modifica_usuario = @modifica_usuario, proceso = @proceso
                                 WHERE cod_estado = @cod_estado";

            var result = DbHelper.ExecuteNonQuery(CreatePortalDb(), CodEmpresa, sql, new
            {
                request.descripcion,
                activo = request.activo ? 1 : 0,
                request.orden,
                p_inicia = request.p_inicia ? 1 : 0,
                p_finaliza = request.p_finaliza ? 1 : 0,
                request.modifica_usuario,
                request.proceso,
                request.cod_estado
            });

            if (result.Code == 0)
            {
                result.Description = "Estado actualizado correctamente";
            }

            return result;
        }

        /// <summary>
        /// Elimina un estado de beneficio.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="id">Código del estado a eliminar.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto BeneficiosEstados_Eliminar(int CodEmpresa, string id)
        {
            const string sql = "DELETE FROM AFI_BENE_ESTADOS WHERE COD_ESTADO = @id";

            var result = DbHelper.ExecuteNonQuery(CreatePortalDb(), CodEmpresa, sql, new { id });

            if (result.Code == 0)
            {
                result.Description = "Estado eliminado correctamente";
            }

            return result;
        }
    }
}
