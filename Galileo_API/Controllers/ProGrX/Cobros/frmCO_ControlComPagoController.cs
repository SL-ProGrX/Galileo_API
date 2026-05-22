using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Cobros;
using Galileo_API.BusinessLogic.ProGrX.Cobros;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX.Cobros
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FrmCoControlComPagoController : ControllerBase
    {
        private readonly FrmCoControlComPagoBL _bl;

        public FrmCoControlComPagoController(IConfiguration config)
        {
            _bl = new FrmCoControlComPagoBL(config);
        }

        [HttpGet("CO_ControlComPago_Remesas_Obtener")]
        public ErrorDto<List<CoControlComPagoRemesaData>> CO_ControlComPago_Remesas_Obtener(int CodEmpresa, int top = 50)
        {
            return _bl.CO_ControlComPago_Remesas_Obtener(CodEmpresa, top);
        }

        [HttpGet("CO_ControlComPago_Remesa_Obtener")]
        public ErrorDto<CoControlComPagoRemesaData> CO_ControlComPago_Remesa_Obtener(int CodEmpresa, int cod_remesa)
        {
            return _bl.CO_ControlComPago_Remesa_Obtener(CodEmpresa, cod_remesa);
        }

        [HttpPost("CO_ControlComPago_Remesa_Guardar")]
        public ErrorDto<int> CO_ControlComPago_Remesa_Guardar(int CodEmpresa, string usuario, CoControlComPagoRemesaGuardarRequest request)
        {
            return _bl.CO_ControlComPago_Remesa_Guardar(CodEmpresa, usuario, request);
        }

        [HttpDelete("CO_ControlComPago_Remesa_Eliminar")]
        public ErrorDto CO_ControlComPago_Remesa_Eliminar(int CodEmpresa, string usuario, int cod_remesa)
        {
            return _bl.CO_ControlComPago_Remesa_Eliminar(CodEmpresa, usuario, cod_remesa);
        }

        [HttpGet("CO_ControlComPago_RemesasPorEstado_Obtener")]
        public ErrorDto<List<CoControlComPagoRemesaComboData>> CO_ControlComPago_RemesasPorEstado_Obtener(int CodEmpresa, string estado)
        {
            return _bl.CO_ControlComPago_RemesasPorEstado_Obtener(CodEmpresa, estado);
        }

        [HttpGet("CO_ControlComPago_CargaBancos_Obtener")]
        public ErrorDto<List<CoControlComPagoBancoData>> CO_ControlComPago_CargaBancos_Obtener(int CodEmpresa, int cod_remesa)
        {
            return _bl.CO_ControlComPago_CargaBancos_Obtener(CodEmpresa, cod_remesa);
        }

        [HttpGet("CO_ControlComPago_ReportesOficinas_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CO_ControlComPago_ReportesOficinas_Obtener(int CodEmpresa)
        {
            return _bl.CO_ControlComPago_ReportesOficinas_Obtener(CodEmpresa);
        }

        [HttpGet("CO_ControlComPago_CargaPendientes_Obtener")]
        public ErrorDto<List<CoControlComPagoCargaData>> CO_ControlComPago_CargaPendientes_Obtener(int CodEmpresa, int cod_remesa, int? id_banco)
        {
            return _bl.CO_ControlComPago_CargaPendientes_Obtener(CodEmpresa, cod_remesa, id_banco);
        }

        [HttpPost("CO_ControlComPago_Carga_Aplicar")]
        public ErrorDto<CoControlComPagoProcesoResult> CO_ControlComPago_Carga_Aplicar(int CodEmpresa, string usuario, CoControlComPagoCargaAplicarRequest request)
        {
            return _bl.CO_ControlComPago_Carga_Aplicar(CodEmpresa, usuario, request);
        }

        [HttpPost("CO_ControlComPago_Remesa_Cerrar")]
        public ErrorDto CO_ControlComPago_Remesa_Cerrar(int CodEmpresa, string usuario, int cod_remesa)
        {
            return _bl.CO_ControlComPago_Remesa_Cerrar(CodEmpresa, usuario, cod_remesa);
        }

        [HttpGet("CO_ControlComPago_TrasladoPendientes_Obtener")]
        public ErrorDto<List<CoControlComPagoTrasladoData>> CO_ControlComPago_TrasladoPendientes_Obtener(int CodEmpresa, int cod_remesa)
        {
            return _bl.CO_ControlComPago_TrasladoPendientes_Obtener(CodEmpresa, cod_remesa);
        }

        [HttpPost("CO_ControlComPago_Traslado_Aplicar")]
        public ErrorDto<CoControlComPagoProcesoResult> CO_ControlComPago_Traslado_Aplicar(int CodEmpresa, string usuario, CoControlComPagoTrasladoAplicarRequest request)
        {
            return _bl.CO_ControlComPago_Traslado_Aplicar(CodEmpresa, usuario, request);
        }
    }
}
