using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic;
using Galileo.Models.ERROR;
using Galileo.Models.GEN;


namespace Galileo.Controllers
{
    [Route("api/frmGenParametrosPro")]
    [ApiController]
    public class FrmGenParametrosProController : ControllerBase
    {

        readonly FrmGenParametrosProBL _bl;
        public FrmGenParametrosProController(IConfiguration config)
        {
            _bl = new FrmGenParametrosProBL(config);
        }

        [HttpGet("Obtener_ParamaterosPro")]
        public ErrorDto<PvParametrosModDto> Obtener_ParamaterosPro(int CodEmpresa)
        {
            return _bl.Obtener_ParamaterosPro(CodEmpresa);
        }
        [HttpPost("ParamaterosPro_ActualizaGen")]
        public ErrorDto ParamaterosPro_ActualizaGen(int CodEmpresa, PvParametrosModDto info)
        {
            return _bl.ParamaterosPro_ActualizaGen(CodEmpresa, info);
        }
        [HttpPost("ParamaterosPro_ActualizaCxP")]
        public ErrorDto ParamaterosPro_ActualizaCxP(int CodEmpresa, PvParametrosModDto info)
        {
            return _bl.ParamaterosPro_ActualizaCxP(CodEmpresa, info);
        }

        [HttpPost("ParamaterosPro_ActualizaInv")]
        public ErrorDto ParamaterosPro_ActualizaInv(int CodEmpresa, PvParametrosModDto info)
        {
            return _bl.ParamaterosPro_ActualizaInv(CodEmpresa, info);
        }

        [HttpPost("ParamaterosPro_ActualizaPos")]
        public ErrorDto ParamaterosPro_ActualizaPos(int CodEmpresa, PvParametrosModDto info)
        {
            return _bl.ParamaterosPro_ActualizaPos(CodEmpresa, info);
        }
    }
}
