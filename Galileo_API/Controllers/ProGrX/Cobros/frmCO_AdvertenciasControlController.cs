using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Cobros; 
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PgxAPI.BusinessLogic.ProGrX.Cobros;

namespace PgxAPI.Controllers.ProGrX.Cobros
{
    [Route("api/[controller]")]
    [ApiController]

    public class FrmCoAdvertenciasControlController : ControllerBase
    {
        
        private readonly FrmCoAdvertenciasControlBL _bl;

        public FrmCoAdvertenciasControlController(IConfiguration config)
            => _bl = new FrmCoAdvertenciasControlBL(config);


       
        [Authorize]
        [HttpGet("TiposAdvertiencia_Consultar")]
        public ErrorDto<List<DropDownListaGenericaModel>> TiposAdvertiencia_Consultar(int CodEmpresa)
        {
            return _bl.TiposAdvertiencia_Consultar(CodEmpresa);
        }

        [Authorize]
        [HttpGet("EstadosPersonas_Consultar")]
        public ErrorDto<List<DropDownListaGenericaModel>> EstadosPersonas_Consultar(int CodEmpresa)
        {
            return _bl.EstadosPersonas_Consultar(CodEmpresa);
        }

        [Authorize]
        [HttpGet("CoAdvertenciasControl_Buscar")]
        public ErrorDto<CoAdvertenciasControlLista> CoAdvertenciasControlBuscar(int CodEmpresa, bool esExportar, [FromQuery] CoAdvertenciasControlFiltros filtros)
        {
            return _bl.CoAdvertenciasControlBuscar(CodEmpresa, filtros, esExportar);
        }
    }
}
