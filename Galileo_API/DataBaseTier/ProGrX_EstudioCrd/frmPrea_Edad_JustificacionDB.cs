using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_EstudioCrd;
using System.Data;

namespace Galileo_API.DataBaseTier.ProGrX_EstudioCrd
{
    public class FrmPreaEdadJustificacionDB
    {
        private readonly PortalDB _portalDb;

        public FrmPreaEdadJustificacionDB(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
        }

        /// <summary>
        /// Carga la justificación y la cantidad de cuotas asociadas al incumplimiento
        /// de edad de pensión del expediente.
        /// </summary>
        public ErrorDto<FrmPreaEdadJustificacionCargarResponse> Prea_frmPreaEdadJustificacion_Cargar(
            int codEmpresa,
            FrmPreaEdadJustificacionCargarRequest request)
        {
            try
            {
                using var connection = _portalDb.CreateConnection(codEmpresa);

                const string sql = @"
SELECT
    ISNULL(APL_JUSTIFICACION_EDAD, 0) AS edad_aplica,
    ISNULL(JUSTIFICACION_EDAD, '') AS edad_justificacion,
    ISNULL(CANTIDAD_CUOTAS_JUSTI_EDAD, PLAZO) AS edad_cuotas
FROM CRD_PREA_PREANALISIS
WHERE COD_PREANALISIS = @cod_preanalisis;";

                var data = connection.QueryFirstOrDefault<FrmPreaEdadJustificacionData>(
                    sql,
                    new
                    {
                        cod_preanalisis = request.cod_preanalisis.Trim()
                    },
                    commandType: CommandType.Text
                ) ?? new FrmPreaEdadJustificacionData();

                var result = new FrmPreaEdadJustificacionCargarResponse
                {
                    cod_preanalisis = request.cod_preanalisis.Trim(),
                    edad_aplica = data.edad_aplica,
                    edad_justificacion = data.edad_justificacion,
                    edad_cuotas = data.edad_cuotas
                };

                return DbHelper.CreateOkResponse<FrmPreaEdadJustificacionCargarResponse>(result);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<FrmPreaEdadJustificacionCargarResponse>(ex.Message);
            }
        }

        /// <summary>
        /// Guarda la justificación de incumplimiento de edad de pensión y la cantidad
        /// de cuotas definida para el expediente.
        /// </summary>
        public ErrorDto<FrmPreaEdadJustificacionGuardarResponse> Prea_frmPreaEdadJustificacion_Guardar(
            int codEmpresa,
            FrmPreaEdadJustificacionGuardarRequest request)
        {
            try
            {
                using var connection = _portalDb.CreateConnection(codEmpresa);
                connection.Open();

                const string sql = @"
EXEC spCrdPreaGuardaJustificacionEdadPension
    @cod_preanalisis,
    @edad_aplica,
    @edad_justificacion,
    @edad_cuotas,
    @usuario;";

                connection.Execute(
                    sql,
                    new
                    {
                        cod_preanalisis = request.cod_preanalisis.Trim(),
                        edad_aplica = request.edad_aplica,
                        edad_justificacion = request.edad_justificacion.Trim(),
                        edad_cuotas = request.edad_cuotas,
                        usuario = request.usuario.Trim()
                    },
                    commandType: CommandType.Text
                );

                var result = new FrmPreaEdadJustificacionGuardarResponse
                {
                    cod_preanalisis = request.cod_preanalisis.Trim(),
                    edad_aplica = request.edad_aplica,
                    edad_justificacion = request.edad_justificacion.Trim(),
                    edad_cuotas = request.edad_cuotas,
                    mensaje = "Se ha realizado la acción correctamente."
                };

                return DbHelper.CreateOkResponse<FrmPreaEdadJustificacionGuardarResponse>(result);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<FrmPreaEdadJustificacionGuardarResponse>(ex.Message);
            }
        }
    }
}
