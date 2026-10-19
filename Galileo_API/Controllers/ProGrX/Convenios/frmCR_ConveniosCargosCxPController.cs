using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX.Convenios;
using Galileo_API.Models.ProGrX.Convenios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX.Convenios
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FrmCrConveniosCargosCxPController : ControllerBase
    {
        private readonly FrmCrConveniosCargosCxPBl _bl;

        public FrmCrConveniosCargosCxPController(IConfiguration config)
        {
            _bl = new FrmCrConveniosCargosCxPBl(config);
        }

        [HttpGet("CrConveniosCargosCxP_Cabecera_Obtener")]
        public ErrorDto<CrCargosCxPCabecera?> CrConveniosCargosCxP_Cabecera_Obtener(
            [FromQuery] int codEmpresa,
            [FromQuery] string codConvenio,
            [FromQuery] int codOrden)
            => _bl.CrConveniosCargosCxP_Cabecera_Obtener(codEmpresa, codConvenio, codOrden);

        [HttpGet("CrConveniosCargosCxP_Consulta_Obtener")]
        public ErrorDto<CrCargosCxPConsulta> CrConveniosCargosCxP_Consulta_Obtener(
            [FromQuery] int codEmpresa,
            [FromQuery] string codConvenio,
            [FromQuery] int codOrden)
            => _bl.CrConveniosCargosCxP_Consulta_Obtener(codEmpresa, codConvenio, codOrden);

        [HttpGet("CrConveniosCargosCxP_Pendientes_Obtener")]
        public ErrorDto<List<CrCargosCxPPendiente>> CrConveniosCargosCxP_Pendientes_Obtener(
            [FromQuery] int codEmpresa,
            [FromQuery] string codConvenio,
            [FromQuery] int codOrden)
            => _bl.CrConveniosCargosCxP_Pendientes_Obtener(codEmpresa, codConvenio, codOrden);

        [HttpGet("CrConveniosCargosCxP_Cargos_Obtener")]
        public ErrorDto<List<CrCargosCxPCargo>> CrConveniosCargosCxP_Cargos_Obtener(
            [FromQuery] int codEmpresa,
            [FromQuery] string codConvenio)
            => _bl.CrConveniosCargosCxP_Cargos_Obtener(codEmpresa, codConvenio);

        [HttpGet("CrConveniosCargosCxP_CargoDescripcion_Obtener")]
        public ErrorDto<string> CrConveniosCargosCxP_CargoDescripcion_Obtener(
            [FromQuery] int codEmpresa,
            [FromQuery] string codCargo)
            => _bl.CrConveniosCargosCxP_CargoDescripcion_Obtener(codEmpresa, codCargo);

        [ValidateAntiForgeryToken]
        [HttpPost("CrConveniosCargosCxP_Registrar")]
        public ErrorDto CrConveniosCargosCxP_Registrar(
            [FromQuery] int codEmpresa,
            [FromBody] CrCargosCxPRegistroRequest request)
            => _bl.CrConveniosCargosCxP_Registrar(codEmpresa, request);

        [ValidateAntiForgeryToken]
        [HttpPost("CrConveniosCargosCxP_Editar")]
        public ErrorDto<decimal?> CrConveniosCargosCxP_Editar(
            [FromQuery] int codEmpresa,
            [FromBody] CrCargosCxPEdicionRequest request)
            => _bl.CrConveniosCargosCxP_Editar(codEmpresa, request);

        [ValidateAntiForgeryToken]
        [HttpPost("CrConveniosCargosCxP_Borrar")]
        public ErrorDto CrConveniosCargosCxP_Borrar(
            [FromQuery] int codEmpresa,
            [FromBody] CrCargosCxPBorradoRequest request)
            => _bl.CrConveniosCargosCxP_Borrar(codEmpresa, request);
    }
}
