using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX_EstudioCrd;
using Galileo_API.Models.ProGrX_EstudioCrd;

namespace Galileo_API.BusinessLogic.ProGrX_EstudioCrd
{
    public class FrmPreaEstudiov2BL
    {
        private readonly FrmPreaEstudiov2DB _db;

        public FrmPreaEstudiov2BL(IConfiguration config)
        {
            _db = new FrmPreaEstudiov2DB(config);
        }

        public ErrorDto<FrmPreaEstudiov2HipotecarioResponse> Prea_frmPreaEstudiov2_Hipotecario_Obtener(
    int codEmpresa,
    FrmPreaEstudiov2HipotecarioRequest request)
        {
            if (request is null)
            {
                return new ErrorDto<FrmPreaEstudiov2HipotecarioResponse>
                {
                    Code = -1,
                    Description = "Debe indicar la información del estudio.",
                    Result = new FrmPreaEstudiov2HipotecarioResponse()
                };
            }

            if (string.IsNullOrWhiteSpace(request.cod_preanalisis) && request.id_solicitud <= 0)
            {
                return new ErrorDto<FrmPreaEstudiov2HipotecarioResponse>
                {
                    Code = -1,
                    Description = "Debe indicar un expediente o una solicitud válida.",
                    Result = new FrmPreaEstudiov2HipotecarioResponse()
                };
            }

            return _db.Prea_frmPreaEstudiov2_Hipotecario_Obtener(codEmpresa, request);
        }


    }
}
