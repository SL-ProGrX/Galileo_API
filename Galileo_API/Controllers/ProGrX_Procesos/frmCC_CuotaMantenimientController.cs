using Galileo.Models.ERROR;
using Galileo.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo_API.BusinessLogic.ProGrX_Procesos;
using Galileo_API.Models.ProGrX_Procesos;

namespace Galileo_API.Controllers.ProGrX_Procesos
{
    [Route("api/[controller]")]
    [ApiController]

    public class FrmCcCuotaMantenimientoController : ControllerBase
    {
        private readonly FrmCcCuotaMantenimientoBL _bl;

        public FrmCcCuotaMantenimientoController(IConfiguration config)
            => _bl = new FrmCcCuotaMantenimientoBL(config);

        [Authorize]
        [HttpGet("Crd_CuotaMantenimiento_Instituciones_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> Crd_CuotaMantenimiento_Instituciones_Obtener(int codEmpresa)
                => _bl.Crd_CuotaMantenimiento_Instituciones_Obtener(codEmpresa);

        [Authorize]
        [HttpPost("Crd_CuotaMantenimiento_Ejecutar")]
        public ErrorDto Crd_CuotaMantenimiento_Ejecutar([FromBody] CcCuotaMantenimientoEjecutarRequest request)
                     => _bl.Crd_CuotaMantenimiento_Ejecutar(request);

        [Authorize]
        [HttpGet("Crd_CuotaMantenimiento_Derecho_Obtener")]
        public int Crd_CuotaMantenimiento_Derecho_Obtener(int codEmpresa, string usuario)
            => _bl.Crd_CuotaMantenimiento_Derecho_Obtener(codEmpresa, usuario);

    }
}
