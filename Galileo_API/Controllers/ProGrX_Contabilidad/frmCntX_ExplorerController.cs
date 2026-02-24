using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX_Contabilidad;
using Galileo_API.Models.ProGrX_Contabilidad;
using Galileo_API.Models.ProGrX_Contabilidad.Galileo_API.Models.ProGrX_Contabilidad;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX_Contabilidad
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FrmCntXExploradorContableController : ControllerBase
    {
        private readonly FrmCntXExploradorContableBl _bl;

        public FrmCntXExploradorContableController(IConfiguration config) => _bl = new FrmCntXExploradorContableBl(config);

        [HttpGet("Cuentas")]
        public ErrorDto<List<DropDownListaGenericaModel>> Cuentas(int codEmpresa)
        {
            return _bl.Cuentas_Obtener(codEmpresa);
        }

        [HttpGet("TiposAsiento")]
        public ErrorDto<List<DropDownListaGenericaModel>> TiposAsiento(int codEmpresa)
        {
            return _bl.TiposAsiento_Obtener(codEmpresa);
        }

        [HttpGet("Periodos")]
        public ErrorDto<List<CntxPeriodoDto>> Periodos(int codEmpresa, string estado) // "P"|"C"
        {
            return _bl.Periodos_Obtener(codEmpresa, estado);
        }

        [HttpPost("ListarAsientos")]
        public ErrorDto<List<CntxAsientoRsmDto>> ListarAsientos(int codEmpresa, [FromBody] CntxExploradorFiltrosDto filtros)
        {
            return _bl.Asientos_Listar(codEmpresa, filtros);
        }

        [HttpPost("AsientoDetalle")]
        public ErrorDto<List<CntxAsientoDetDto>> AsientoDetalle(int codEmpresa, [FromBody] CntxExploradorFiltrosDto filtros)
        {
            return _bl.AsientoDetalle_Listar(codEmpresa, filtros);
        }

        [HttpGet("FechaServidor_Obtener")]
        public ErrorDto<string> FechaServidor_Obtener(int codEmpresa)
        {
            return _bl.FechaServidor_Obtener(codEmpresa);
        }
    }
}