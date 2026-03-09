using Galileo.Models.ERROR;
using Galileo.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static Galileo_API.Models.ProGrX_Polizas.FrmPolizaFacturaVerModels;
using Galileo_API.DataBaseTier.ProGrX_Polizas;

namespace Galileo_API.Controllers.ProGrX_Polizas
{
    [Route("api/[controller]")]
    [ApiController]

    public class FrmPolizaFacturaVerController : ControllerBase
    {
        private readonly FrmPolizaFacturaVerDB _db;

        public FrmPolizaFacturaVerController(IConfiguration config)
        {
            _db = new FrmPolizaFacturaVerDB(config);
        }


        [Authorize]
        [HttpGet("CrdPolizaFacturaVer_Divisas_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CrdPolizaFacturaVer_Divisas_Obtener(int codEmpresa, int codContabilidad)
                 => _db.CrdPolizaFacturaVer_Divisas_Obtener(codEmpresa, codContabilidad);
        
        [Authorize]
        [HttpGet("CrdPolizaFacturaVer_DivisaLocal_Obtener")]
        public ErrorDto<CrdPolizaFacturaVerDivisaLocalModel> CrdPolizaFacturaVer_DivisaLocal_Obtener(int codEmpresa, int codContabilidad)
                => _db.CrdPolizaFacturaVer_DivisaLocal_Obtener(codEmpresa,codContabilidad);
        
        [Authorize]
        [HttpGet("CrdPolizaFacturaVer_Factura_Obtener")]
        public ErrorDto<CrdPolizaFacturaVerFacturaResponse> CrdPolizaFacturaVer_Factura_Obtener(
            int codEmpresa, int proveedor, string factura)
                => _db.CrdPolizaFacturaVer_Factura_Obtener(codEmpresa, proveedor, factura);
        
        [Authorize]
        [HttpGet("CrdPolizaFacturaVer_Asientos_Obtener")]
        public ErrorDto<CrdPolizaFacturaVerAsientosResponse> CrdPolizaFacturaVer_Asientos_Obtener(
          int codEmpresa, int proveedor, string factura)
                 => _db.CrdPolizaFacturaVer_Asientos_Obtener(codEmpresa, proveedor, factura);
    }
}
