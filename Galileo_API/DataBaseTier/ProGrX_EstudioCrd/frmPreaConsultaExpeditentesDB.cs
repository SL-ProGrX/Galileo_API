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
        /// Obtiene la lista de expedientes y subexpedientes de PREA.
        /// Si buscar viene vacío, retorna toda la lista; si trae valor, aplica filtro global.
        /// </summary>
        public ErrorDto<FrmPreaConsultaExpeditentesListaResponse> Prea_frmPreaConsultaExpeditentes_Lista_Obtener(
            int codEmpresa,
            string? buscar)
        {
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
WHERE
    (
        @buscar IS NULL
        OR @buscar = ''
        OR COD_PREANALISIS LIKE @term
        OR COD_PREANALISIS_REF LIKE @term
        OR NOMBRE LIKE @term
        OR CEDULA LIKE @term
    )
ORDER BY COD_PREANALISIS DESC;";

            var filtro = buscar?.Trim() ?? string.Empty;

            var queryResult = DbHelper.ExecuteListQuery<FrmPreaConsultaExpeditentesItemData>(
                _portalDb,
                codEmpresa,
                sql,
                new
                {
                    buscar = filtro,
                    term = string.IsNullOrWhiteSpace(filtro) ? null : $"%{filtro}%"
                });

            if (queryResult.Code != 0)
            {
                return DbHelper.CreateErrorResponse<FrmPreaConsultaExpeditentesListaResponse>(queryResult.Description);
            }

            var result = new FrmPreaConsultaExpeditentesListaResponse
            {
                lista = (queryResult.Result ?? new List<FrmPreaConsultaExpeditentesItemData>())
                    .Select(item => new FrmPreaConsultaExpeditentesItem
                    {
                        cod_preanalisis = item.cod_preanalisis,
                        cod_preanalisis_ref = item.cod_preanalisis_ref,
                        nombre = item.nombre,
                        cedula = item.cedula,
                        fecha_creacion = item.fecha_creacion?.ToString("dd/MM/yyyy") ?? string.Empty,
                        estado = item.estado,
                        usuario = item.usuario
                    })
                    .ToList()
            };

            return DbHelper.CreateOkResponse(result);
        }
    }
}
