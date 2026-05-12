using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX_EstudioCrd;
using Galileo_API.Models.ProGrX_EstudioCrd;

namespace Galileo_API.BusinessLogic.ProGrX_EstudioCrd
{
    public class FrmPreaNotificacionBL
    {
        private readonly FrmPreaNotificacionDB _db;

        public FrmPreaNotificacionBL(IConfiguration config)
        {
            _db = new FrmPreaNotificacionDB(config);
        }

        public ErrorDto<FrmPreaNotificacionCargarResponse> Prea_frmPreaNotificacion_Cargar(
            int codEmpresa,
            FrmPreaNotificacionCargarRequest request)
        {
            if (request is null)
            {
                return new ErrorDto<FrmPreaNotificacionCargarResponse>
                {
                    Code = -1,
                    Description = "Debe indicar la información de la notificación.",
                    Result = new FrmPreaNotificacionCargarResponse()
                };
            }

            if (string.IsNullOrWhiteSpace(request.cod_preanalisis) && request.id_solicitud <= 0)
            {
                return new ErrorDto<FrmPreaNotificacionCargarResponse>
                {
                    Code = -1,
                    Description = "Debe indicar un expediente o una solicitud válida.",
                    Result = new FrmPreaNotificacionCargarResponse()
                };
            }

            return _db.Prea_frmPreaNotificacion_Cargar(codEmpresa, request);
        }

        public ErrorDto<FrmPreaNotificacionEnviarResponse> Prea_frmPreaNotificacion_Notificar(
            int codEmpresa,
            FrmPreaNotificacionEnviarRequest request)
        {
            if (request is null)
            {
                return new ErrorDto<FrmPreaNotificacionEnviarResponse>
                {
                    Code = -1,
                    Description = "Debe indicar la información de envío.",
                    Result = new FrmPreaNotificacionEnviarResponse()
                };
            }

            if (string.IsNullOrWhiteSpace(request.cod_preanalisis) && request.id_solicitud <= 0)
            {
                return new ErrorDto<FrmPreaNotificacionEnviarResponse>
                {
                    Code = -1,
                    Description = "Debe indicar un expediente o una solicitud válida.",
                    Result = new FrmPreaNotificacionEnviarResponse()
                };
            }

            if (string.IsNullOrWhiteSpace(request.tiquete))
            {
                return new ErrorDto<FrmPreaNotificacionEnviarResponse>
                {
                    Code = -1,
                    Description = "Debe indicar el tiquete.",
                    Result = new FrmPreaNotificacionEnviarResponse()
                };
            }

            return _db.Prea_frmPreaNotificacion_Notificar(codEmpresa, request);
        }
    }
}
