using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX.Bancos;
using Galileo_API.Models.ProGrX.Bancos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FrmTesConsultaCuentaSinpeController : ControllerBase
    {
        private readonly FrmTesConsultaCuentaSinpeBL _bl;

        public FrmTesConsultaCuentaSinpeController(IConfiguration config)
        {
            _bl = new FrmTesConsultaCuentaSinpeBL(config);
        }

        [HttpPost("Tes_ConsultaCuentasSinpe_Aplicar")]
        public ErrorDto Tes_ConsultaCuentasSinpe_Aplicar(int CodEmpresa, int aplica, TesConsultaCuentaSinpeModels cuenta)
        {
            return _bl.Tes_ConsultaCuentasSinpe_Aplicar(CodEmpresa, aplica, cuenta);
        }

    }
}
