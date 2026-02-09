using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX_Polizas;
using Galileo_API.DataBaseTier.ProGrX_Polizas;
using Galileo_API.Models.ProGrX_Polizas;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace Galileo_API.Controllers.ProGrX_Polizas
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmPolizasEstadosReclamosController : ControllerBase
    {
        private readonly FrmPolizasEstadosReclamosBL _bl;

        public FrmPolizasEstadosReclamosController(IConfiguration config)
        {
            _bl = new FrmPolizasEstadosReclamosBL(config);
        }

        [Authorize]
        [HttpGet("EstadosReclamos_Listar")]
        public ErrorDto<List<PolizasEstadosReclamosDto>> EstadosReclamos_Listar(int codEmpresa)
        {
            return _bl.EstadosReclamos_Listar(codEmpresa);
        }

        [Authorize]
        [HttpGet("EstadosReclamos_Existe")]
        public ErrorDto<PolizasEstadosReclamosExisteResult?> EstadosReclamos_Existe(int codEmpresa, [FromQuery] int idEstado)
        {
            return _bl.EstadosReclamos_Existe(codEmpresa, idEstado);
        }

        [Authorize]
        [HttpPost("EstadosReclamos_Guardar")]
        public ErrorDto<bool> EstadosReclamos_Guardar(int codEmpresa, [FromBody] PolizasEstadosReclamosSaveParams param)
        {
            return _bl.EstadosReclamos_Guardar(codEmpresa, param);
        }

        [Authorize]
        [HttpPost("EstadosReclamos_Eliminar")]
        public ErrorDto<bool> EstadosReclamos_Eliminar(int codEmpresa, [FromBody] PolizasEstadosReclamosDeleteParams param)
        {
            return _bl.EstadosReclamos_Eliminar(codEmpresa, param);
        }
    }
}
