using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX.Creditos;
using Galileo_API.Models.ProGrX.Creditos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX.Creditos
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FrmCrMoraCargosAjustesController : ControllerBase
    {
        private readonly FrmCrMoraCargosAjustesBl _bl;

        public FrmCrMoraCargosAjustesController(IConfiguration config)
            => _bl = new FrmCrMoraCargosAjustesBl(config);

        [HttpGet("CrMoraCargosAjustes_ConsultaOperacion_Obtener")]
        public ErrorDto<CrMoraCargosAjustesOperacionData?> CrMoraCargosAjustes_ConsultaOperacion_Obtener(
            int codEmpresa,
            int operacionId)
        {
            return _bl.CrMoraCargosAjustes_ConsultaOperacion_Obtener(codEmpresa, operacionId);
        }

        [HttpGet("CrMoraCargosAjustes_CuotasMora_Obtener")]
        public ErrorDto<List<CrMoraCargosAjustesCuotasData>> CrMoraCargosAjustes_CuotasMora_Obtener(
            int codEmpresa,
            int operacionId)
        {
            return _bl.CrMoraCargosAjustes_CuotasMora_Obtener(codEmpresa, operacionId);
        }

        [HttpGet("CrMoraCargosAjustes_Cargos_Obtener")]
        public ErrorDto<List<CrMoraCargosAjustesCargosData>> CrMoraCargosAjustes_Cargos_Obtener(
            int codEmpresa,
            int operacionId)
        {
            return _bl.CrMoraCargosAjustes_Cargos_Obtener(codEmpresa, operacionId);
        }

        [HttpPost("CrMoraCargosAjustes_Fecha_Aplicar")]
        public ErrorDto CrMoraCargosAjustes_Fecha_Aplicar(
            int codEmpresa,
             CrMoraCargosAjustesFechaRequest request)
        {
            return _bl.CrMoraCargosAjustes_Fecha_Aplicar(codEmpresa, request);
        }

        [HttpDelete("CrMoraCargosAjustes_CuotasMora_Eliminar")]
        public ErrorDto CrMoraCargosAjustes_CuotasMora_Eliminar(
            int codEmpresa,
             string request)
        {
            return _bl.CrMoraCargosAjustes_CuotasMora_Eliminar(codEmpresa, request);
        }

        [HttpDelete("CrMoraCargosAjustes_Cargos_Eliminar")]
        public ErrorDto CrMoraCargosAjustes_Cargos_Eliminar(
            int codEmpresa,
             string request)
        {
            return _bl.CrMoraCargosAjustes_Cargos_Eliminar(codEmpresa, request);
        }
    }
}