using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX.Credito;
using Galileo_API.Models.ProGrX.Credito.Galileo_API.Models.ProGrX.Credito;
using System.Data.Common;

namespace Galileo_API.DataBaseTier.ProGrX.Creditos
{
    public partial class FrmCrSeguimientoRevisionesTagDB
    {
        /// <summary>
        /// Obtiene el historial de seguimiento por etiquetas aplicado a la operación.
        /// </summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <param name="request">Solicitud con la operación seleccionada.</param>
        /// <returns>Historial de seguimiento registrado.</returns>
        public ErrorDto<CrSeguimientoRevisionesTagSeguimientoResponse> Cr_SeguimientoRevisionesTag_Seguimiento_Obtener(
            int codEmpresa,
            CrSeguimientoRevisionesTagSeguimientoRequest request)
        {
            if (request == null || request.id_solicitud <= 0)
            {
                return DbHelper.CreateErrorResponse<CrSeguimientoRevisionesTagSeguimientoResponse>(
                    validaSolicitud);
            }

            try
            {
                using var conn = DbHelper.OpenConnection(_portalDB, codEmpresa);

                const string sql = """
                    select
                        rtrim(T.DESCRIPCION) as descripcion,
                        isnull(rtrim(OT.NOTAS), '') as notas,
                        OT.REGISTRO_FECHA as registro_fecha,
                        isnull(rtrim(OT.REGISTRO_USUARIO), '') as registro_usuario
                    from CRD_OPERACION_TAGS OT
                    inner join CRD_TAGS T on OT.TAG_CODIGO = T.TAG_CODIGO
                    where OT.ID_SOLICITUD = @id_solicitud
                    order by OT.LINEA
                    """;

                var lista = conn.Query<CrSeguimientoRevisionesTagSeguimientoRow>(
                    sql,
                    new { id_solicitud = request.id_solicitud }).ToList();

                return DbHelper.CreateOkResponse(new CrSeguimientoRevisionesTagSeguimientoResponse
                {
                    total = lista.Count,
                    lista = lista
                });
            }
            catch (DbException ex)
            {
                return DbHelper.CreateErrorResponse<CrSeguimientoRevisionesTagSeguimientoResponse>(ex.Message);
            }
        }
    }
}