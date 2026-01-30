using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Cobros;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic.ProGrX.Cobros;


namespace Galileo.Controllers.ProGrX.Cobros
{
    [Route("api/[controller]")]
    [ApiController]

    public class FrmCoControlCartasAvisosController : ControllerBase
    {
    
        private readonly FrmCoControlCartasAvisosBL _bl;
        public FrmCoControlCartasAvisosController(IConfiguration config)
                  => _bl = new FrmCoControlCartasAvisosBL(config);


        [Authorize]
        [HttpGet("CO_ControlCartasAvisos_Buscar")]
        public ErrorDto<CoControlCartasAvisosLista> CO_ControlCartasAvisos_Buscar(int CodEmpresa, bool esExportar, string filtros, string cedula="")
        {
            return _bl.CO_ControlCartasAvisos_Buscar(CodEmpresa, esExportar, filtros, cedula);
        }

        [Authorize]
        [HttpGet("CO_ControlCartasAvisos_Usuarios_Consultar")]
        public ErrorDto<List<DropDownListaGenericaModel>> CO_ControlCartasAvisos_Usuarios_Consultar(int CodEmpresa)
        {
            return _bl.CO_ControlCartasAvisos_Usuarios_Consultar(CodEmpresa);
        }

    }
}
