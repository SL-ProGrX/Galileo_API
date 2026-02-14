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
    public class FrmCxCContratosPagadoresController : ControllerBase
    {
        private readonly FrmCxCContratosPagadoresBL _bl;

        public FrmCxCContratosPagadoresController(IConfiguration config)
        {
            _bl = new FrmCxCContratosPagadoresBL(config);
        }

        [Authorize]
        [HttpPost("CxcContratosPagadores_Lista")]
        public ErrorDto<List<CxcContratoPagadorDto>> CxcContratosPagadores_Lista(int codEmpresa, [FromBody] CxcContratoPagadorListaParams param)
        {
            return _bl.CxcContratosPagadores_Lista(codEmpresa, param);
        }

        [Authorize]
        [HttpPost("CxcContratoPagador_Insertar")]
        public ErrorDto<bool> CxcContratoPagador_Insertar(int codEmpresa, [FromBody] CxcContratoPagadorSaveParams param)
        {
            return _bl.CxcContratoPagador_Insertar(codEmpresa, param);
        }

        [Authorize]
        [HttpPost("CxcContratoPagador_Eliminar")]
        public ErrorDto<bool> CxcContratoPagador_Eliminar(int codEmpresa, [FromBody] CxcContratoPagadorDeleteParams param)
        {
            return _bl.CxcContratoPagador_Eliminar(codEmpresa, param);
        }
    }
}
