using Galileo.Models.ERROR;
using Galileo.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo_API.BusinessLogic.ProGrX_Procesos;

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
        public ErrorDto Crd_CuotaMantenimiento_Ejecutar(int codEmpresa, string usuario, int codContabilidad, int codInstitucion)
                     => _bl.Crd_CuotaMantenimiento_Ejecutar(codEmpresa, usuario, codContabilidad, codInstitucion);


    }
}
