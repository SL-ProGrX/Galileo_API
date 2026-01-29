using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX_Contabilidad;
using Galileo_API.Models.ProGrX_Contabilidad;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX_Contabilidad
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FrmCntXCentrosCostosController : ControllerBase
    {
        private readonly FrmCntXCentrosCostosBl _bl;

        public FrmCntXCentrosCostosController(IConfiguration config) => _bl = new FrmCntXCentrosCostosBl(config);

        [HttpGet("CntXCentrosCostos_Obtener")]
        public ErrorDto<List<CntXCentroCostosData>> CntXCentrosCostos_Obtener(int codEmpresa, int codConta, bool activo)
        {
            return _bl.CntXCentrosCostos_Obtener(codEmpresa, codConta, activo);
        }

        [HttpPost("CntXCentrosCostos_Guardar")]
        public ErrorDto CntXCentrosCostos_Guardar(int codEmpresa, int codConta, string usuario, CntXCentroCostosData request)
        {
            return _bl.CntXCentrosCostos_Guardar(codEmpresa, codConta, usuario, request);
        }

        [HttpDelete("CntXCentrosCostos_Eliminar")]
        public ErrorDto CntXCentrosCostos_Eliminar(int codEmpresa, int codConta, string usuario, string codCentroCosto)
        {
            return _bl.CntXCentrosCostos_Eliminar(codEmpresa, codConta, usuario, codCentroCosto);
        }

        [HttpGet("CntXCentrosCostos_Unidades_Obtener")]
        public ErrorDto<List<CntXCentroCostosUnidadesDto>> CntXCentrosCostos_Unidades_Obtener(int codEmpresa, int codConta, string codCentroCosto)
        {
            return _bl.CntXCentrosCostos_Unidades_Obtener(codEmpresa, codConta, codCentroCosto);
        }

        [HttpPost("CntXCentrosCostos_Unidades_Asignar")]
        public ErrorDto CntXCentrosCostos_Unidades_Asignar(int codEmpresa, int codConta, string codCentroCosto, string codUnidad, bool itemChecked)
        {
            return _bl.CntXCentrosCostos_Unidades_Asignar(codEmpresa, codConta, codCentroCosto, codUnidad, itemChecked);
        }
    }
}