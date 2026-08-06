using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX.Creditos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static Galileo_API.Models.ProGrX.Creditos.FrmCrComisionesPagoModels;



namespace Galileo_API.Controllers.ProGrX.Creditos
{

    [Route("api/[controller]")]
    [ApiController]
    [Authorize]

    public class FrmCrComisionesPagoController : ControllerBase
    {
        private readonly FrmCrComisionesPagoBL _bl;

        public FrmCrComisionesPagoController(IConfiguration config)
        { 
            _bl = new FrmCrComisionesPagoBL(config);
        }

        [HttpGet("Crd_ComisionesPago_RemesasPorEstado_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CrdComisionesPago_RemesasPorEstado_Obtener(int codEmpresa, string estado)
           => _bl.CrdComisionesPago_RemesasPorEstado_Obtener(codEmpresa, estado);



        [HttpGet("Crd_ComisionesPago_Comisiones_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CrdComisionesPago_Comisiones_Obtener(int CodEmpresa)
           => _bl.CrdComisionesPago_Comisiones_Obtener(CodEmpresa);


        [HttpGet("Crd_ComisionesPago_Bancos_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CrdComisionesPago_Bancos_Obtener(int CodEmpresa)
                => _bl.CrdComisionesPago_Bancos_Obtener(CodEmpresa);

        [HttpGet("Crd_ComisionesPago_OficinasPendientes_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CrdComisionesPago_OficinasPendientes_Obtener(int codEmpresa, int codRemesa)
            => _bl.CrdComisionesPago_OficinasPendientes_Obtener(codEmpresa, codRemesa);

        [HttpGet("Crd_ComisionesPago_Oficinas_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CrdComisionesPago_Oficinas_Obtener(int codEmpresa)
              => _bl.CrdComisionesPago_Oficinas_Obtener(codEmpresa);


        [HttpGet("Crd_ComisionesPago_Remesas_Obtener")]
        public ErrorDto<List<CrdComisionesPagoRemesaModel>> CrdComisionesPago_Remesas_Obtener(int codEmpresa, int cantidad = 50)
               => _bl.CrdComisionesPago_Remesas_Obtener(codEmpresa, cantidad);


        [HttpGet("Crd_ComisionesPago_Remesa_Obtener")]
        public ErrorDto<CrdComisionesPagoRemesaModel?> CrdComisionesPago_Remesa_Obtener(int codEmpresa, int codRemesa)
            => _bl.CrdComisionesPago_Remesa_Obtener(codEmpresa, codRemesa);

        [HttpPost("Crd_ComisionesPago_Remesa_Guardar")]
        public ErrorDto<CrdComisionesPagoRemesaGuardarResponse> CrdComisionesPago_Remesa_Guardar(int codEmpresa, [FromBody] CrdComisionesPagoRemesaGuardarRequest request)
            => _bl.CrdComisionesPago_Remesa_Guardar(codEmpresa, request);


        [HttpPost("Crd_ComisionesPago_Remesa_Eliminar")]
        public ErrorDto<bool> CrdComisionesPago_Remesa_Eliminar(int codEmpresa, [FromBody] CrdComisionesPagoRemesaEliminarRequest request)
            => _bl.CrdComisionesPago_Remesa_Eliminar(codEmpresa, request);

        [HttpPost("Crd_ComisionesPago_Pendientes_Obtener")]
        public ErrorDto<List<CrdComisionesPagoPendienteModel>> CrdComisionesPago_Pendientes_Obtener(int codEmpresa, [FromBody] CrdComisionesPagoPendientesRequest request)
            => _bl.CrdComisionesPago_Pendientes_Obtener(codEmpresa, request);


        [HttpPost("Crd_ComisionesPago_Remesa_Cargar")]
        public ErrorDto<CrdComisionesPagoProcesoResponse> CrdComisionesPago_Remesa_Cargar(int codEmpresa, [FromBody] CrdComisionesPagoCargaRequest request)
            => _bl.CrdComisionesPago_Remesa_Cargar(codEmpresa, request);

        [HttpPost("Crd_ComisionesPago_Remesa_Cerrar")]
        public ErrorDto<bool> CrdComisionesPago_Remesa_Cerrar(int codEmpresa, [FromBody] CrdComisionesPagoCerrarRequest request)
            => _bl.CrdComisionesPago_Remesa_Cerrar(codEmpresa, request);      

        [HttpGet("Crd_ComisionesPago_Traslado_Obtener")]
        public ErrorDto<List<CrdComisionesPagoTrasladoModel>> CrdComisionesPago_Traslado_Obtener(int codEmpresa, int codRemesa)
            => _bl.CrdComisionesPago_Traslado_Obtener(codEmpresa, codRemesa);


        [HttpPost("Crd_ComisionesPago_Remesa_Trasladar")]
        public ErrorDto<CrdComisionesPagoProcesoResponse> CrdComisionesPago_Remesa_Trasladar(int codEmpresa, CrdComisionesPagoTrasladarRequest request)
            => _bl.CrdComisionesPago_Remesa_Trasladar(codEmpresa, request);

        [HttpPost("Crd_ComisionesPago_Reportes_Obtener")]
        public ErrorDto<List<CrdComisionesPagoReporteModel>> CrdComisionesPago_Reportes_Obtener(int codEmpresa, int cantidad)
              => _bl.CrdComisionesPago_Reportes_Obtener(codEmpresa, cantidad);
    }


}
