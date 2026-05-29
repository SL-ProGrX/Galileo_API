using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic.ProGrX.Clientes;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Clientes;
using Microsoft.AspNetCore.Authorization;

namespace Galileo.Controllers.ProGrX.Clientes
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmAFComisionesAutorizaController : ControllerBase
    {
        private readonly FrmAFComisionesAutorizaBL _bl;

        public FrmAFComisionesAutorizaController(IConfiguration config)
        {
            _bl = new FrmAFComisionesAutorizaBL(config);
        }

        [Authorize]
        [HttpPost("AF_ComisionesAutoriza_Obtener")]
        public ErrorDto<List<ComisionAutorizaData>> AF_ComisionesAutoriza_Obtener(int CodEmpresa, [FromBody] ComisionAutorizaFiltroDto filtro)
        {
            return _bl.AF_ComisionesAutoriza_Obtener(CodEmpresa, filtro);
        }

        [Authorize]
        [HttpPost("AF_ComisionesAutoriza_Autorizar")]
        public ErrorDto AF_ComisionesAutoriza_Autorizar(int CodEmpresa, string cedula, int autoriza, string? notas, string usuario)
        {
            return _bl.AF_ComisionesAutoriza_Autorizar(CodEmpresa, cedula, autoriza, notas, usuario);
        }        
    }
}