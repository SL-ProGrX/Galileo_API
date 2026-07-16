using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX_Pasivos;
using Galileo_API.Models.ProGrX_Pasivos;
using Microsoft.AspNetCore.Mvc;
using Galileo.Models;

namespace Galileo_API.Controllers.ProGrX_Pasivos
{
    [ApiController]
    [Route("api/[controller]")]
    public class FrmCrApaOperacionRenumeraController : ControllerBase
    {
        private readonly FrmCrApaOperacionRenumeraBL _bl;

        public FrmCrApaOperacionRenumeraController(IConfiguration config)
        {
            _bl = new FrmCrApaOperacionRenumeraBL(config);
        }

        [HttpGet("CR_APA_OperacionRenumera_Acreedores_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CR_APA_OperacionRenumera_Acreedores_Obtener(
            int codEmpresa)
            => _bl.CR_APA_OperacionRenumera_Acreedores_Obtener(codEmpresa);

        [HttpGet("CR_APA_OperacionRenumera_Acreedor_Obtener")]
        public ErrorDto<DropDownListaGenericaModel?> CR_APA_OperacionRenumera_Acreedor_Obtener(
            int codEmpresa,
            string cod_acreedor)
            => _bl.CR_APA_OperacionRenumera_Acreedor_Obtener(codEmpresa, cod_acreedor);

        [HttpGet("CR_APA_OperacionRenumera_Operacion_Obtener")]
        public ErrorDto<FrmCrApaOperacionRenumeraOperacionDto?> CR_APA_OperacionRenumera_Operacion_Obtener(
            int codEmpresa,
            string cod_acreedor,
            string operacion)
            => _bl.CR_APA_OperacionRenumera_Operacion_Obtener(codEmpresa, cod_acreedor, operacion);

        [HttpGet("CR_APA_OperacionRenumera_Operaciones_Obtener")]
        public ErrorDto<List<FrmCrApaOperacionRenumeraOperacionDto>> CR_APA_OperacionRenumera_Operaciones_Obtener(
            int codEmpresa,
            string cod_acreedor)
            => _bl.CR_APA_OperacionRenumera_Operaciones_Obtener(codEmpresa, cod_acreedor);

        [HttpPost("CR_APA_OperacionRenumera_Aplicar")]
        public ErrorDto<FrmCrApaOperacionRenumeraResultadoDto> CR_APA_OperacionRenumera_Aplicar(
            int codEmpresa,
            [FromBody] FrmCrApaOperacionRenumeraAplicarRequest request)
            => _bl.CR_APA_OperacionRenumera_Aplicar(codEmpresa, request);
    }
}