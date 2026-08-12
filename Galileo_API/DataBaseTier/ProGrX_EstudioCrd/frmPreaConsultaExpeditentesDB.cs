using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_EstudioCrd;

namespace Galileo_API.DataBaseTier.ProGrX_EstudioCrd
{
    public class FrmPreaConsultaExpeditentesDB
    {
        private readonly PortalDB _portalDb;

        public FrmPreaConsultaExpeditentesDB(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
        }

        /// <summary>
        /// Obtiene la lista de expedientes y subexpedientes de PREA con paginación server-side.
        /// </summary>
        public ErrorDto<FrmPreaConsultaExpeditentesListaResponse> Prea_frmPreaConsultaExpeditentes_Lista_Obtener(
            int codEmpresa,
            string? buscar,
            int pagina,
            int paginacion)
        {
            var result = new ErrorDto<FrmPreaConsultaExpeditentesListaResponse>
            {
                Code = 0,
                Description = "Ok",
                Result = new FrmPreaConsultaExpeditentesListaResponse()
            };

            try
            {
                using var connection = _portalDb.CreateConnection(codEmpresa);

                var filtro = buscar?.Trim() ?? string.Empty;
                var like = string.IsNullOrWhiteSpace(filtro) ? null : $"%{filtro}%";
                var offset = pagina;
                var fetch = paginacion;

                const string sqlCount = @"SELECT COUNT(*) FROM CRD_PREA_PREANALISIS
                                          WHERE (@like IS NULL
                                             OR COD_PREANALISIS LIKE @like
                                             OR COD_PREANALISIS_REF LIKE @like
                                             OR NOMBRE LIKE @like
                                             OR CEDULA LIKE @like)";

                var total = connection.QueryFirstOrDefault<int>(sqlCount, new { like });

                const string sql = @"
                    SELECT
                        RTRIM(ISNULL(COD_PREANALISIS, '')) AS cod_preanalisis,
                        RTRIM(ISNULL(COD_PREANALISIS_REF, '')) AS cod_preanalisis_ref,
                        RTRIM(ISNULL(NOMBRE, '')) AS nombre,
                        RTRIM(ISNULL(CEDULA, '')) AS cedula,
                        FECHA_CREACION AS fecha_creacion,
                        RTRIM(ISNULL(ESTADO, '')) AS estado,
                        RTRIM(ISNULL(USUARIO, '')) AS usuario
                    FROM CRD_PREA_PREANALISIS
                    WHERE (@like IS NULL
                       OR COD_PREANALISIS LIKE @like
                       OR COD_PREANALISIS_REF LIKE @like
                       OR NOMBRE LIKE @like
                       OR CEDULA LIKE @like)
                    ORDER BY COD_PREANALISIS DESC
                    OFFSET @offset ROWS FETCH NEXT @fetch ROWS ONLY;";

                var items = connection.Query<FrmPreaConsultaExpeditentesItemData>(
                    sql,
                    new { like, offset, fetch }
                ).ToList();

                result.Result = new FrmPreaConsultaExpeditentesListaResponse
                {
                    lista = items.Select(item => new FrmPreaConsultaExpeditentesItem
                    {
                        cod_preanalisis = item.cod_preanalisis,
                        cod_preanalisis_ref = item.cod_preanalisis_ref,
                        nombre = item.nombre,
                        cedula = item.cedula,
                        fecha_creacion = item.fecha_creacion?.ToString("dd/MM/yyyy") ?? string.Empty,
                        estado = item.estado,
                        usuario = item.usuario
                    }).ToList(),
                    total = total
                };
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = new FrmPreaConsultaExpeditentesListaResponse();
            }

            return result;
        }
    }
}
