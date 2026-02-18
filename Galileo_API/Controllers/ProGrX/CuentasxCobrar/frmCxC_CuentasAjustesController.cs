using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX.CuentasxCobrar;
using Galileo_API.Models.ProGrX.CuentasxCobrar;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX.CuentasxCobrar
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FrmCxCCuentasAjustesController : ControllerBase
    {
        private readonly FrmCxCCuentasAjustesBl _bl;

        public FrmCxCCuentasAjustesController(IConfiguration config) => _bl = new FrmCxCCuentasAjustesBl(config);

        [HttpGet("CxCCuentasAjustes_ConsultaOperacion_Obtener")]
        public ErrorDto<CxCCuentasAjustesOperacionData> CxCCuentasAjustes_ConsultaOperacion_Obtener(int codEmpresa, int operacionId)
        {
            return _bl.CxCCuentasAjustes_ConsultaOperacion_Obtener(codEmpresa, operacionId);
        }

        [HttpGet("CxCCuentasAjustes_CuotasMora_Obtener")]
        public ErrorDto<List<CxCCuentasAjustesCuotasData>> CxCCuentasAjustes_CuotasMora_Obtener(int codEmpresa, int operacionId)
        {
            return _bl.CxCCuentasAjustes_CuotasMora_Obtener(codEmpresa, operacionId);
        }

        [HttpGet("CxCCuentasAjustes_Cargos_Obtener")]
        public ErrorDto<List<CxCCuentasAjustesCargosData>> CxCCuentasAjustes_Cargos_Obtener(int codEmpresa, int operacionId)
        {
            return _bl.CxCCuentasAjustes_Cargos_Obtener(codEmpresa, operacionId);
        }
        
        [HttpPost("CxCCuentasAjustes_Fecha_Aplicar")]
        public ErrorDto CxCCuentasAjustes_Fecha_Aplicar(int codEmpresa, CxCCuentasAjustesFechaRequest request)
        {
            return _bl.CxCCuentasAjustes_Fecha_Aplicar(codEmpresa, request);
        }

        [HttpPost("CxCCuentasAjustes_CuotasMora_Eliminar")]
        public ErrorDto CxCCuentasAjustes_CuotasMora_Eliminar(int codEmpresa, int operacionId, string usuario, List<CxCCuentasAjustesCuotasData> lista)
        {
            return _bl.CxCCuentasAjustes_CuotasMora_Eliminar(codEmpresa, operacionId, usuario, lista);
        }
        
        [HttpPost("CxCCuentasAjustes_Cargos_Eliminar")]
        public ErrorDto CxCCuentasAjustes_Cargos_Eliminar(int codEmpresa, int operacionId, string usuario, List<CxCCuentasAjustesCargosData> lista)
        {
            return _bl.CxCCuentasAjustes_Cargos_Eliminar(codEmpresa, operacionId, usuario, lista);
        }
    }
}