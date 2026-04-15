using Galileo_API.BusinessLogic.ProGrX.Cobros;
using Galileo.Models;
using Galileo.Models.ERROR; 
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static Galileo_API.Models.ProGrX.Cobros.FrmCoGestorExternoModels;

namespace Galileo_API.Controllers.ProGrX.Cobros
{
    [Route("api/[controller]")]
    [ApiController]


    public class FrmCoGestorExternoController : ControllerBase
    {
        private readonly FrmCoGestorExternoBL _bl;

        public FrmCoGestorExternoController(IConfiguration config)
            => _bl = new FrmCoGestorExternoBL(config);

        [Authorize]
        [HttpPost("Crd_GestorExterno_Listado_Obtener")]
        public ErrorDto<List<CrdGestorExternoListaItemModel>> Crd_GestorExterno_Listado_Obtener(int codEmpresa, [FromBody] CrdGestorExternoFiltroRequest request)
                => _bl.Crd_GestorExterno_Listado_Obtener(codEmpresa, request);

        [Authorize]
        [HttpPost("Crd_GestorExterno_Registrar")]
        public ErrorDto<string> Crd_GestorExterno_Registrar(int codEmpresa, [FromBody]  CrdGestorExternoRegistrarRequest request)
                        => _bl.Crd_GestorExterno_Registrar(codEmpresa, request);

        [Authorize]
        [HttpPost("Crd_GestorExterno_Reversar")]
        public ErrorDto<string> Crd_GestorExterno_Reversar(int codEmpresa, [FromBody]  CrdGestorExternoReversaRequest request)
                => _bl.Crd_GestorExterno_Reversar(codEmpresa, request);

        [Authorize]
        [HttpPost("Crd_GestorExterno_CargaMasiva_Procesar")]
        public ErrorDto<CrdGestorExternoCargaMasivaResponse> Crd_GestorExterno_CargaMasiva_Procesar(int codEmpresa, [FromBody] CrdGestorExternoCargaMasivaRequest request)
                 => _bl.Crd_GestorExterno_CargaMasiva_Procesar(codEmpresa, request);
        
        [Authorize]
        [HttpGet("Crd_GestorExterno_Operacion_Buscar")]
        public ErrorDto<List<CrdGestorExternoOperacionModel>> Crd_GestorExterno_Operacion_Buscar(int codEmpresa)
                 => _bl.Crd_GestorExterno_Operacion_Buscar(codEmpresa);
        
        [Authorize]
        [HttpGet("Crd_GestorExterno_Gestores_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> Crd_GestorExterno_Gestores_Obtener(int codEmpresa)
                 => _bl.Crd_GestorExterno_Gestores_Obtener(codEmpresa);

    }
}
