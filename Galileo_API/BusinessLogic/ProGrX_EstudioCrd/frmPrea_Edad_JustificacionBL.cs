using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX_EstudioCrd;
using Galileo_API.Models.ProGrX_EstudioCrd;

namespace Galileo_API.BusinessLogic.ProGrX_EstudioCrd
{
    public class FrmPreaEdadJustificacionBL
    {
        private readonly FrmPreaEdadJustificacionDB _db;

        public FrmPreaEdadJustificacionBL(IConfiguration config)
        {
            _db = new FrmPreaEdadJustificacionDB(config);
        }

        public ErrorDto<FrmPreaEdadJustificacionCargarResponse> Prea_frmPreaEdadJustificacion_Cargar(
            int codEmpresa,
            FrmPreaEdadJustificacionCargarRequest request)
        {
            if (request is null || string.IsNullOrWhiteSpace(request.cod_preanalisis))
            {
                return new ErrorDto<FrmPreaEdadJustificacionCargarResponse>
                {
                    Code = -1,
                    Description = "Debe indicar el expediente.",
                    Result = new FrmPreaEdadJustificacionCargarResponse()
                };
            }

            return _db.Prea_frmPreaEdadJustificacion_Cargar(codEmpresa, request);
        }

        public ErrorDto<FrmPreaEdadJustificacionGuardarResponse> Prea_frmPreaEdadJustificacion_Guardar(
            int codEmpresa,
            FrmPreaEdadJustificacionGuardarRequest request)
        {
            if (request is null)
            {
                return new ErrorDto<FrmPreaEdadJustificacionGuardarResponse>
                {
                    Code = -1,
                    Description = "Debe indicar la información de la justificación.",
                    Result = new FrmPreaEdadJustificacionGuardarResponse()
                };
            }

            if (string.IsNullOrWhiteSpace(request.cod_preanalisis))
            {
                return new ErrorDto<FrmPreaEdadJustificacionGuardarResponse>
                {
                    Code = -1,
                    Description = "Debe indicar el expediente.",
                    Result = new FrmPreaEdadJustificacionGuardarResponse()
                };
            }

            if (string.IsNullOrWhiteSpace(request.edad_justificacion) || request.edad_justificacion.Trim().Length < 50)
            {
                return new ErrorDto<FrmPreaEdadJustificacionGuardarResponse>
                {
                    Code = -1,
                    Description = "Debe ingresar una justificación de al menos 50 caracteres para continuar.",
                    Result = new FrmPreaEdadJustificacionGuardarResponse()
                };
            }

            if (request.edad_cuotas < 0)
            {
                return new ErrorDto<FrmPreaEdadJustificacionGuardarResponse>
                {
                    Code = -1,
                    Description = "La cantidad de cuotas no es válida.",
                    Result = new FrmPreaEdadJustificacionGuardarResponse()
                };
            }

            return _db.Prea_frmPreaEdadJustificacion_Guardar(codEmpresa, request);
        }
    }
}
