using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX_EstudioCrd;
using Galileo_API.Models.ProGrX_EstudioCrd;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Data;

namespace Galileo_API.Controllers.ProGrX_EstudioCrd
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FrmPreaEstudiov2Controller : ControllerBase
    {
        private readonly FrmPreaEstudiov2BL _bl;
        public FrmPreaEstudiov2Controller(IConfiguration config)
        {
            _bl = new FrmPreaEstudiov2BL(config);
        }

        [HttpPost("Prea_frmPreaEstudiov2_Hipotecario_Obtener")]
        public ErrorDto<FrmPreaEstudiov2HipotecarioResponse> Prea_frmPreaEstudiov2_Hipotecario_Obtener(
    int codEmpresa,
    FrmPreaEstudiov2HipotecarioRequest request)
        {
            return _bl.Prea_frmPreaEstudiov2_Hipotecario_Obtener(codEmpresa, request);
        }

    }
}
