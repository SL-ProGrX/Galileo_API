using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Clientes;

namespace Galileo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmAFCambioCedulaController : ControllerBase
    {
        private readonly FrmAFCambioCedulaBL _bl;
        public FrmAFCambioCedulaController(IConfiguration config)
        {
            _bl = new FrmAFCambioCedulaBL(config);
        }

        [Authorize]
        [HttpGet("AF_TiposCedulas_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> AF_TiposCedulas_Obtener(int CodEmpresa)
        {
            return _bl.AF_TiposCedulas_Obtener(CodEmpresa);
        }

        [Authorize]
        [HttpPost("AF_CambioCedula_Aplicar")]
        public ErrorDto AF_CambioCedula_Aplicar(int CodEmpresa, string usuario, string cambioCedula)
        {
            return _bl.AF_CambioCedula_Aplicar(CodEmpresa, usuario, cambioCedula);
        }

        [Authorize]
        [HttpGet("AF_Cedula_Obtener")]
        public ErrorDto<AFCedulaCambioDto> AF_Cedula_Obtener(int CodEmpresa, string cedula)
        {
            return _bl.AF_Cedula_Obtener(CodEmpresa, cedula);
        }

        [Authorize]
        [HttpGet("AF_Usuario_Validar")]
        public ErrorDto AF_Usuario_Validar(int CodEmpresa, string parametros)
        {
            return _bl.AF_Usuario_Validar(CodEmpresa, parametros);
        }
    }
}