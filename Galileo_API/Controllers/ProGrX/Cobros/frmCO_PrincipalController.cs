using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX.Cobros;
using Galileo_API.BusinessLogic.ProGrX.Cobros;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.Models;

namespace Galileo_API.Controllers.ProGrX.Cobros
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmCOPrincipalController : ControllerBase
    {
        private readonly FrmCOPrincipalBL _bl;

        public FrmCOPrincipalController(IConfiguration config)
            => _bl = new FrmCOPrincipalBL(config);

        [Authorize]
        [HttpGet("Operaciones_Listar")]
        public ErrorDto<List<OperacionBusquedaDto>> Operaciones_Listar(int codEmpresa)
        {
            return _bl.Operaciones_Listar(codEmpresa);
        }

        [Authorize]
        [HttpGet("Operacion_Consultar")]
        public ErrorDto<OperacionConsultarDto> Operacion_Consultar(int codEmpresa, int operacion)
        {
            return _bl.Operacion_Consultar(codEmpresa, operacion);
        }

        [Authorize]
        [HttpGet("Deductoras_Listar")]
        public ErrorDto<List<DropDownListaGenericaModel>> Deductoras_Listar(int CodEmpresa, int codInstitucion)
        {
            return _bl.Deductoras_Listar(CodEmpresa, codInstitucion);
        }
    }
}