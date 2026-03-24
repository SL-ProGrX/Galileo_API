using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX.Cobros;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static Galileo_API.Models.ProGrX.Cobros.FrmCOIncobrablesModels; 

namespace Galileo_API.Controllers.ProGrX.Cobros
{
    [Route("api/[controller]")]
    [ApiController]

    public class FrmCOIncobrablesController : ControllerBase
    {
        private readonly FrmCOIncobrablesBL _bl;

        public FrmCOIncobrablesController(IConfiguration config)
            => _bl = new FrmCOIncobrablesBL(config);

        [Authorize]
        [HttpGet("Crd_Incobrables_Operacion_Consultar")]
        public ErrorDto<CrdIncobrableDetalleResponse> Crd_Incobrables_Operacion_Consultar(int codEmpresa, int idSolicitud)
                 => _bl.Crd_Incobrables_Operacion_Consultar(codEmpresa, idSolicitud);

        [Authorize]
        [HttpGet("Crd_Incobrables_Codigos_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> Crd_Incobrables_Codigos_Obtener(int codEmpresa, int idSolicitud)
                        => _bl.Crd_Incobrables_Codigos_Obtener(codEmpresa, idSolicitud);
        [Authorize]
        [HttpGet("Crd_Incobrables_Detalle_Obtener")]
        public ErrorDto<CrdIncobrableDetalleResponse> Crd_Incobrables_Detalle_Obtener(
             int codEmpresa,
             string usuario,
             int codContabilidad,
             int idSolicitud,
             int codIncobrable)
                 => _bl.Crd_Incobrables_Detalle_Obtener(codEmpresa, usuario, codContabilidad, idSolicitud, codIncobrable);

        [Authorize]
        [HttpPost("Crd_Incobrables_Aplicar")]
        public ErrorDto<object> Crd_Incobrables_Aplicar(int codEmpresa, [FromBody] CrdIncobrableAplicarRequest request)
                   => _bl.Crd_Incobrables_Aplicar(codEmpresa, request);

        [Authorize]
        [HttpPost("Crd_Incobrables_Reversar")]
        public ErrorDto<object> Crd_Incobrables_Reversar(int codEmpresa, [FromBody] CrdIncobrableReversaRequest request)
                 => _bl.Crd_Incobrables_Reversar(codEmpresa, request);

    }
}
