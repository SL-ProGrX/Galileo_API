using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic.ProGrX.Clientes;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Clientes;

namespace Galileo.Controllers.ProGrX.Clientes
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmAFBitacoraEspecialController : ControllerBase
    {
        private readonly FrmAFBitacoraEspecialBL _bl;

        public FrmAFBitacoraEspecialController(IConfiguration config)
        {
            _bl = new FrmAFBitacoraEspecialBL(config);
        }

        [Authorize]
        [HttpGet("AF_BitacoraEspecialMov_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> AF_BitacoraEspecialMov_Obtener(int CodEmpresa)
        {
            return _bl.AF_BitacoraEspecialMov_Obtener(CodEmpresa);
        }

        [Authorize]
        [HttpPost("AF_BitacoraEspecial_Revisar")]
        public ErrorDto AF_BitacoraEspecial_Revisar(int CodEmpresa, string usuario, List<AFBitacoraEspecialData> bitacora)
        {
            return _bl.AF_BitacoraEspecial_Revisar(CodEmpresa, usuario, bitacora);
        }

        [Authorize]
        [HttpGet("AF_BitacoraEspecialBusquedas_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> AF_BitacoraEspecialBusquedas_Obtener(int CodEmpresa, string campo)
        {
            return _bl.AF_BitacoraEspecialBusquedas_Obtener(CodEmpresa, campo);
        }

        [Authorize]
        [HttpGet("AF_BitacoraEspecial_Obtener")]
        public ErrorDto<List<AFBitacoraEspecialData>> AF_BitacoraEspecial_Obtener(int CodEmpresa, string filtros)
        {
            return _bl.AF_BitacoraEspecial_Obtener(CodEmpresa, filtros);
        }
    }
}