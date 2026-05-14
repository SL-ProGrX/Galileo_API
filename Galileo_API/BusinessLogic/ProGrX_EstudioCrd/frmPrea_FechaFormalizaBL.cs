using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX_EstudioCrd;
using Galileo_API.Models.ProGrX_EstudioCrd.Galileo_API.Models.ProGrX_EstudioCrd;

namespace Galileo_API.BusinessLogic.ProGrX_EstudioCrd
{
    public class FrmPreaFechaFormalizaBL
    {
        private readonly FrmPreaFechaFormalizaDB _db;

        public FrmPreaFechaFormalizaBL(IConfiguration config)
        {
            _db = new FrmPreaFechaFormalizaDB(config);
        }

        public ErrorDto<FrmPreaFechaFormalizaCargarResponse> Prea_frmPreaFechaFormaliza_Cargar(
            int codEmpresa,
            FrmPreaFechaFormalizaCargarRequest request)
        {
            if (request is null || string.IsNullOrWhiteSpace(request.cod_preanalisis))
            {
                return new ErrorDto<FrmPreaFechaFormalizaCargarResponse>
                {
                    Code = -1,
                    Description = "Debe indicar el expediente.",
                    Result = new FrmPreaFechaFormalizaCargarResponse()
                };
            }

            return _db.Prea_frmPreaFechaFormaliza_Cargar(codEmpresa, request);
        }

        public ErrorDto<FrmPreaFechaFormalizaCalcularResponse> Prea_frmPreaFechaFormaliza_Calcular(
            int codEmpresa,
            FrmPreaFechaFormalizaCalcularRequest request)
        {
            if (request is null || string.IsNullOrWhiteSpace(request.cod_preanalisis))
            {
                return new ErrorDto<FrmPreaFechaFormalizaCalcularResponse>
                {
                    Code = -1,
                    Description = "Debe indicar el expediente.",
                    Result = new FrmPreaFechaFormalizaCalcularResponse()
                };
            }

            if (request.fecha_formaliza is null)
            {
                return new ErrorDto<FrmPreaFechaFormalizaCalcularResponse>
                {
                    Code = -1,
                    Description = "Debe indicar la fecha de formalización.",
                    Result = new FrmPreaFechaFormalizaCalcularResponse()
                };
            }

            return _db.Prea_frmPreaFechaFormaliza_Calcular(codEmpresa, request);
        }

        public ErrorDto<FrmPreaFechaFormalizaCambiarResponse> Prea_frmPreaFechaFormaliza_Cambiar(
            int codEmpresa,
            FrmPreaFechaFormalizaCambiarRequest request)
        {
            if (request is null || string.IsNullOrWhiteSpace(request.cod_preanalisis))
            {
                return new ErrorDto<FrmPreaFechaFormalizaCambiarResponse>
                {
                    Code = -1,
                    Description = "Debe indicar el expediente.",
                    Result = new FrmPreaFechaFormalizaCambiarResponse()
                };
            }

            if (request.fecha_formaliza is null)
            {
                return new ErrorDto<FrmPreaFechaFormalizaCambiarResponse>
                {
                    Code = -1,
                    Description = "Debe indicar la fecha de formalización.",
                    Result = new FrmPreaFechaFormalizaCambiarResponse()
                };
            }

            if (request.monto_interes < 0)
            {
                return new ErrorDto<FrmPreaFechaFormalizaCambiarResponse>
                {
                    Code = -1,
                    Description = "El monto de interés no es válido.",
                    Result = new FrmPreaFechaFormalizaCambiarResponse()
                };
            }

            return _db.Prea_frmPreaFechaFormaliza_Cambiar(codEmpresa, request);
        }
    }
}
