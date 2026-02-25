using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX_Polizas;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static Galileo_API.Models.ProGrX_Polizas.FrmCrPolizaProcRecepcionModels;

namespace Galileo_API.Controllers.ProGrX_Polizas
{
    [Route("api/[controller]")]
    [ApiController]

    public class FrmCrPolizaProcRecepcionController : ControllerBase
    {

        private readonly FrmCrPolizaProcRecepcionBL _bl;

        public FrmCrPolizaProcRecepcionController(IConfiguration config)
        {
            _bl = new FrmCrPolizaProcRecepcionBL(config);
        }


        [Authorize]
        [HttpGet("PolizaProcRecepcion_Listar")]
        public ErrorDto<List<DropDownListaGenericaModel>> PolizaProcRecepcion_Listar(int codEmpresa)
          => _bl.PolizaProcRecepcion_Listar(codEmpresa);

        [Authorize]
        [HttpGet("PolizaUnidades_Listar")]
        public ErrorDto<List<DropDownListaGenericaModel>> PolizaUnidades_Listar(int codEmpresa, int codContabilidad)
        => _bl.PolizaUnidades_Listar(codEmpresa, codContabilidad);

        [Authorize]
        [HttpGet("PolizaFacturables_Listar")]
        public ErrorDto<List<DropDownListaGenericaModel>> PolizaFacturables_Listar(int codEmpresa)
         => _bl.PolizaFacturables_Listar(codEmpresa);

        [Authorize]
        [HttpGet("PolizaCentrosCosto_Listar")]
        public ErrorDto<List<DropDownListaGenericaModel>> PolizaCentrosCosto_Listar(int codEmpresa, int codContabilidad, string codUnidad)
        => _bl.PolizaCentrosCosto_Listar(codEmpresa, codContabilidad, codUnidad);

        [Authorize]
        [HttpGet("PolizaDivisas_Listar")]
        public ErrorDto<List<DropDownListaGenericaModel>> PolizaDivisas_Listar(int codEmpresa, int codContabilidad)
        => _bl.PolizaDivisas_Listar(codEmpresa, codContabilidad);

        [Authorize]
        [HttpGet("PolizaDivisasLocal_Consulta")]
        public ErrorDto<DropDownListaGenericaModel> PolizaDivisasLocal_Consulta(int codEmpresa, int codContabilidad)
        => _bl.PolizaDivisasLocal_Consulta(codEmpresa, codContabilidad);

        [Authorize]
        [HttpGet("PolizaAseguradoraCorte_Valida")]
        public ErrorDto<PolizaAseguradoraCorte> PolizaAseguradoraCorte_Valida(int codEmpresa, DateTime corte, string codPoliza, int idFactura)
        => _bl.PolizaAseguradoraCorte_Valida(codEmpresa, corte, codPoliza, idFactura);

        [Authorize]
        [HttpPost("PolizaAseguradoraCorte_Agregar")]
        public ErrorDto<PolizaAseguradoraCorte> PolizaAseguradoraCorte_Agregar(int codEmpresa, string usuario, [FromBody] PolizaAseguradoraCorteData datos)
        => _bl.PolizaAseguradoraCorte_Agregar(codEmpresa, usuario, datos);

        [Authorize]
        [HttpPost("PolizaAseguradoraCorteDetalle_Agregar")]
        public ErrorDto<int> PolizaAseguradoraCorteDetalle_Agregar(int codEmpresa, string usuario, int scFacturaId, [FromBody] IEnumerable<PolizaAseguradoraCorteDetalleData> datos)
        => _bl.PolizaAseguradoraCorteDetalle_Agregar(codEmpresa, usuario, scFacturaId, datos);

        [Authorize]
        [HttpGet("PolizaAseguradoraCorte_Pago")]
        public ErrorDto<PolizaAseguradoraCorte> PolizaAseguradoraCorte_Pago(int codEmpresa, string usuario, DateTime corte, string codPoliza, int idFactura)
        => _bl.PolizaAseguradoraCorte_Pago(codEmpresa, usuario, corte, codPoliza, idFactura);

        [Authorize]
        [HttpGet("TipoCambio_Consultar")]
        public ErrorDto<decimal> TipoCambio_Consultar(int codEmpresa, int contabilidad, string divisa)
        => _bl.TipoCambio_Consultar(codEmpresa, contabilidad, divisa);

        [Authorize]
        [HttpGet("PolizaPolizaDatos")]
        public ErrorDto<PolizaDatos> PolizaPolizaDatos(int codEmpresa, string codPoliza)
        => _bl.PolizaPolizaDatos(codEmpresa, codPoliza);

        [Authorize]
        [HttpGet("PolizaAseguradoraCorteDetalle_Consulta")]
        public ErrorDto<List<PolizaAseguradoraCorteDetalleConsulta>> PolizaAseguradoraCorteDetalle_Consulta(int codEmpresa, DateTime corte, string codPoliza, int idFactura)
        => _bl.PolizaAseguradoraCorteDetalle_Consulta(codEmpresa, corte, codPoliza, idFactura);

        [Authorize]
        [HttpGet("PolizaAseguradoraCorte_Consulta")]
        public ErrorDto<PolizaDatos> PolizaAseguradoraCorte_Consulta(int codEmpresa, DateTime corte, string codPoliza)
        => _bl.PolizaAseguradoraCorte_Consulta(codEmpresa, corte, codPoliza);

    }
}
