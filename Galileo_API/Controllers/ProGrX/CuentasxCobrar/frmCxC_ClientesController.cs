using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX.CuentasxCobrar;
using Galileo_API.Models.ProGrX.CuentasxCobrar;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace Galileo_API.Controllers.ProGrX.CuentasxCobrar
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmCxCClientesController : ControllerBase
    {
        private readonly FrmCxCClientesBL _bl;

        public FrmCxCClientesController(IConfiguration config)
        {
            _bl = new FrmCxCClientesBL(config);
        }

        [Authorize]
        [HttpGet("CxcPersonas_Lista")]
        public ErrorDto<List<CxcPersonaDto>> CxcPersonas_Lista(int codEmpresa, string orden)
        {
            return _bl.CxcPersonas_Lista(codEmpresa, orden);
        }

        [Authorize]
        [HttpGet("EstadoCivil_Lista")]
        public ErrorDto<List<DropDownListaGenericaModel>> EstadoCivil_Lista(int codEmpresa)
        {
            return _bl.EstadoCivil_Lista(codEmpresa);
        }

        [Authorize]
        [HttpGet("Clasificacion_Lista")]
        public ErrorDto<List<DropDownListaGenericaModel>> Clasificacion_Lista(int codEmpresa)
        {
            return _bl.Clasificacion_Lista(codEmpresa);
        }

        [Authorize]
        [HttpGet("TiposId_Lista")]
        public ErrorDto<List<DropDownListaGenericaModel>> TiposId_Lista(int codEmpresa)
        {
            return _bl.TiposId_Lista(codEmpresa);
        }
    }
}
