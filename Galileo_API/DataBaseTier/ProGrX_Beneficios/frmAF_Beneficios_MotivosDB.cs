using Dapper;
using Galileo.Models.AF;
using Galileo.Models.ERROR;

namespace Galileo.DataBaseTier.ProGrX_Beneficios
{
    /// <summary>
    /// Acceso a datos del catálogo de Motivos de Beneficios (frmAF_Beneficios_Motivos).
    /// </summary>
    public partial class FrmAfBeneficiosMotivosDB
    {
        private readonly IConfiguration _config;

        /// <summary>
        /// Inicializa el acceso a datos con la configuración inyectada.
        /// </summary>
        /// <param name="config">Configuración de la aplicación.</param>
        public FrmAfBeneficiosMotivosDB(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        /// <summary>
        /// Crea una instancia de acceso al portal usando la configuración inyectada.
        /// </summary>
        private PortalDB CreatePortalDb() => new(_config);

        /// <summary>
        /// Obtiene la lista de motivos de beneficios con paginación y filtro.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="pagina">Offset de paginación.</param>
        /// <param name="paginacion">Cantidad de registros por página.</param>
        /// <param name="filtro">Filtro de búsqueda por descripción o código.</param>
        /// <returns>Lista de motivos y total.</returns>
        public ErrorDto<BeneMotivosDataLista> BeneficiosMotivos_Obtener(int CodEmpresa, int? pagina, int? paginacion, string? filtro)
        {
            return DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
            {
                var response = new BeneMotivosDataLista();

                const string sqlCount = "SELECT COUNT(*) FROM AFI_BENE_MOTIVOS";
                response.Total = connection.QueryFirstOrDefault<int>(sqlCount);

                var like = string.IsNullOrWhiteSpace(filtro) ? null : $"%{filtro}%";
                var offset = pagina ?? 0;
                var fetch = paginacion ?? 10;

                const string sql = @"SELECT cod_motivo, descripcion, activo, registro_fecha,
                                            registro_usuario, modifica_fecha, modifica_usuario
                                     FROM AFI_BENE_MOTIVOS
                                     WHERE (@like IS NULL OR DESCRIPCION LIKE @like OR COD_MOTIVO LIKE @like)
                                     ORDER BY COD_MOTIVO
                                     OFFSET @offset ROWS FETCH NEXT @fetch ROWS ONLY";

                response.Lista = connection.Query<BeneMotivos>(sql, new { like, offset, fetch }).ToList();
                return response;
            });
        }

        /// <summary>
        /// Inserta un motivo de beneficio, validando que el código no exista.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="request">Datos del motivo.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto BeneficiosMotivos_Agregar(int CodEmpresa, BeneMotivos request)
        {
            using var connection = DbHelper.OpenConnection(CreatePortalDb(), CodEmpresa);
            try
            {
                const string sqlExiste = "SELECT COUNT(*) FROM AFI_BENE_MOTIVOS WHERE COD_MOTIVO = @cod_motivo";
                var existe = connection.QueryFirstOrDefault<int>(sqlExiste, new { request.cod_motivo });

                if (existe > 0)
                {
                    return DbHelper.ErrorResponse("Ya existe un motivo con el codigo: " + request.cod_motivo + ", por favor verifique");
                }

                const string sql = @"INSERT INTO AFI_BENE_MOTIVOS (cod_motivo, descripcion, activo, registro_fecha, registro_usuario)
                                     VALUES (@cod_motivo, @descripcion, @activo, GETDATE(), @registro_usuario)";

                connection.Execute(sql, new
                {
                    request.cod_motivo,
                    request.descripcion,
                    activo = request.activo ? 1 : 0,
                    request.registro_usuario
                });

                return DbHelper.OkResponse("Ok");
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        /// <summary>
        /// Actualiza el detalle de un motivo de beneficio.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="request">Datos del motivo.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto BeneficiosMotivos_Actualizar(int CodEmpresa, BeneMotivos request)
        {
            const string sql = @"UPDATE AFI_BENE_MOTIVOS
                                 SET descripcion = @descripcion, activo = @activo,
                                     modifica_fecha = GETDATE(), modifica_usuario = @modifica_usuario
                                 WHERE cod_motivo = @cod_motivo";

            return DbHelper.ExecuteNonQuery(CreatePortalDb(), CodEmpresa, sql, new
            {
                request.descripcion,
                activo = request.activo ? 1 : 0,
                request.modifica_usuario,
                request.cod_motivo
            });
        }

        /// <summary>
        /// Elimina un motivo de beneficio.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="id">Código del motivo a eliminar.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto BeneficiosMotivos_Eliminar(int CodEmpresa, string id)
        {
            const string sql = "DELETE FROM AFI_BENE_MOTIVOS WHERE COD_MOTIVO = @id";

            return DbHelper.ExecuteNonQuery(CreatePortalDb(), CodEmpresa, sql, new { id });
        }
    }
}
