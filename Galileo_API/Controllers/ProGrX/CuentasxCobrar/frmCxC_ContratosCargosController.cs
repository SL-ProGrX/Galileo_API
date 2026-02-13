using Galileo_API.BusinessLogic.ProGrX.CuentasxCobrar;
using Galileo_API.Models.ProGrX.CuentasxCobrar;
using Galileo.Models.ERROR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace Galileo_API.Controllers.ProGrX.CuentasxCobrar
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmCxCContratosCargosController : ControllerBase
    {
        private readonly FrmCxCContratosCargosBL _bl;

        public FrmCxCContratosCargosController(IConfiguration config)
        {
            _bl = new FrmCxCContratosCargosBL(config);
        }

        [Authorize]
        [HttpPost("CxcCargos_Lista")]
        public ErrorDto<List<CxcCargoDto>> CxcCargos_Lista(int codEmpresa, string orden)
        {
            return _bl.CxcCargos_Lista(codEmpresa, orden);
        }

        [Authorize]
        [HttpPost("CxcContratoCargos_Lista")]
        public ErrorDto<List<CxcContratoCargoDto>> CxcContratoCargos_Lista(int codEmpresa, string codContrato)
        {
            return _bl.CxcContratoCargos_Lista(codEmpresa, codContrato);
        }

        [Authorize]
        [HttpPost("CxcContratoCargo_Guardar")]
        public ErrorDto<bool> CxcContratoCargo_Guardar(int codEmpresa, [FromBody] CxcContratoCargoSaveParams param)
        {
            return _bl.CxcContratoCargo_Guardar(codEmpresa, param);
        }

        [Authorize]
        [HttpPost("CxcContratoCargo_Eliminar")]
        public ErrorDto<bool> CxcContratoCargo_Eliminar(int codEmpresa, [FromBody] CxcContratoCargoDeleteParams param)
        {
            return _bl.CxcContratoCargo_Eliminar(codEmpresa, param);
        }
    }
}
