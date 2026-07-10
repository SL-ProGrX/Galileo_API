using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX.Creditos;

namespace Galileo_API.DataBaseTier.ProGrX.Creditos
{
    public class FrmCrAutorizacionTranferenciasDB
    {
        private readonly PortalDB _portalDb;

        public FrmCrAutorizacionTranferenciasDB(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
        }

        public ErrorDto<List<CrAutorizacionTranferenciasTag>> CrAutorizacionTranferencias_Tags_Obtener(int CodEmpresa, string Usuario)
        {
            Usuario = (Usuario ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(Usuario))
            {
                return new ErrorDto<List<CrAutorizacionTranferenciasTag>>
                {
                    Code = -1,
                    Description = "Debe indicar el usuario logueado.",
                    Result = new List<CrAutorizacionTranferenciasTag>()
                };
            }

            const string sqlQuery = @"
                SELECT
                    RTRIM(CT.TAG_CODIGO) AS llave,
                    RTRIM(CT.DESCRIPCION) AS describe
                FROM CRD_TAGS CT
                INNER JOIN CRD_TAGS_GRUPOS CTG
                    ON CT.TAG_CODIGO = CTG.TAG_CODIGO
                INNER JOIN CRD_GRPUSERS CGU
                    ON CTG.COD_GRUPO = CGU.COD_GRUPO
                WHERE CT.ACTIVO = 1
                  AND UPPER(RTRIM(LTRIM(CGU.USUARIO))) = UPPER(@Usuario)
                ORDER BY CT.TAG_CODIGO;";

            return DbHelper.ExecuteListQuery<CrAutorizacionTranferenciasTag>(
                _portalDb,
                CodEmpresa,
                sqlQuery,
                new { Usuario });
        }

        public ErrorDto<List<CrAutorizacionTranferenciasSolicitud>> CrAutorizacionTranferencias_Solicitudes_Obtener(int CodEmpresa, DateTime FechaDtpFInicio, string CodigoEtiqueta)
        {
            CodigoEtiqueta = (CodigoEtiqueta ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(CodigoEtiqueta))
            {
                return new ErrorDto<List<CrAutorizacionTranferenciasSolicitud>>
                {
                    Code = -1,
                    Description = "Debe indicar el código de etiqueta.",
                    Result = new List<CrAutorizacionTranferenciasSolicitud>()
                };
            }

            const string sqlQuery = @"
                SELECT
                    RC.ID_SOLICITUD AS IdSolicitud,
                    RC.FECHAFORP AS FechaForp,
                    RTRIM(S.NOMBRE) AS Nombre,
                    RTRIM(RC.CODIGO) AS Codigo,
                    RC.MONTOSOL AS MontoSol,
                    RC.CUOTA AS Cuota,
                    RC.PLAZO AS Plazo,
                    RC.INT AS Interes,
                    CASE RC.ESTADOSOL
                        WHEN 'R' THEN 'Recibido'
                        WHEN 'P' THEN 'Pendiente'
                        ELSE RC.ESTADOSOL
                    END AS EstadoSolDescripcion,
                    RC.FECHASOL AS FechaSol
                FROM REG_CREDITOS RC
                INNER JOIN SOCIOS S
                    ON RC.CEDULA = S.CEDULA
                WHERE RC.ESTADOSOL = 'F'
                  AND ISNULL(RC.AUTORIZA_TRANSFERENCIA, 0) = 0
                  -- AND RC.FECHAFORP = @FechaDtpFInicio
                  AND dbo.fxCRDValidaTag(@CodigoEtiqueta, RC.ID_SOLICITUD) > 0
                ORDER BY RC.ID_SOLICITUD;";

            return DbHelper.ExecuteListQuery<CrAutorizacionTranferenciasSolicitud>(
                _portalDb,
                CodEmpresa,
                sqlQuery,
                new
                {
                    FechaDtpFInicio,
                    CodigoEtiqueta
                });
        }
    }
}
