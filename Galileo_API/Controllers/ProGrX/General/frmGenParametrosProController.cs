using Galileo.BusinessLogic;
using Galileo.Models.ERROR;
using Galileo.Models.GEN;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo.Controllers
{
    [Route("api/frmGenParametrosPro")]
    [ApiController]
    [Authorize]
    public class FrmGenParametrosProController : ControllerBase
    {
        private readonly FrmGenParametrosProBL _bl;

        public FrmGenParametrosProController(IConfiguration config)
        {
            _bl = new FrmGenParametrosProBL(config);
        }

        [HttpPost("Gen_ParametrosPro_Inicializar")]
        public ErrorDto Gen_ParametrosPro_Inicializar(int CodEmpresa)
        {
            return _bl.Gen_ParametrosPro_Inicializar(CodEmpresa);
        }

        [HttpGet("Gen_ParametrosPro_Obtener")]
        public ErrorDto<GenParametrosProData?> Gen_ParametrosPro_Obtener(int CodEmpresa)
        {
            return _bl.Gen_ParametrosPro_Obtener(CodEmpresa);
        }

        [HttpPut("Gen_ParametrosProGeneral_Actualizar")]
        public ErrorDto Gen_ParametrosProGeneral_Actualizar(int CodEmpresa, string usuario, GenParametrosProData parametros)
        {
            return _bl.Gen_ParametrosProGeneral_Actualizar(CodEmpresa, usuario, parametros);
        }

        [HttpPut("Gen_ParametrosProCxP_Actualizar")]
        public ErrorDto Gen_ParametrosProCxP_Actualizar(int CodEmpresa, string usuario, GenParametrosProData parametros)
        {
            return _bl.Gen_ParametrosProCxP_Actualizar(CodEmpresa, usuario, parametros);
        }

        [HttpPut("Gen_ParametrosProInv_Actualizar")]
        public ErrorDto Gen_ParametrosProInv_Actualizar(int CodEmpresa, string usuario, GenParametrosProData parametros)
        {
            return _bl.Gen_ParametrosProInv_Actualizar(CodEmpresa, usuario, parametros);
        }

        [HttpPut("Gen_ParametrosProPos_Actualizar")]
        public ErrorDto Gen_ParametrosProPos_Actualizar(int CodEmpresa, string usuario, GenParametrosProData parametros)
        {
            return _bl.Gen_ParametrosProPos_Actualizar(CodEmpresa, usuario, parametros);
        }
    }
}
