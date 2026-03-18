using Galileo_API.BusinessLogic.ProGrX_Contabilidad;
using Galileo_API.Models.ProGrX_Contabilidad;
using Galileo.Models.ERROR;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace Galileo_API.Controllers.ProGrX_Contabilidad
{
    [ApiController]
    [Route("api/[controller]")]
    public class FrmCntxRolesAccesoController : ControllerBase
    {
        private readonly FrmCntxRolesAccesoBL _bl;

        public FrmCntxRolesAccesoController(IConfiguration config)
        {
            _bl = new FrmCntxRolesAccesoBL(config);
        }

        [HttpGet("CntXAcRol_Lista")]
        public ActionResult<ErrorDto<List<CntXAcRolDto>>> CntXAcRol_Lista([FromQuery] int codEmpresa, [FromQuery] int codContabilidad, [FromQuery] string usuario)
            => _bl.CntXAcRol_Lista(codEmpresa, codContabilidad, usuario);

        [HttpGet("CntXAcCuentas_Consulta")]
        public ActionResult<ErrorDto<List<CntXAcCuentaDto>>> CntXAcCuentas_Consulta(
           [FromQuery] int codEmpresa,
           [FromQuery] int codContabilidad,
           [FromQuery] string rol,
           [FromQuery] string usuario,
           [FromQuery] string ctaInicio = "",
           [FromQuery] string ctaCorte = "",
           [FromQuery] string filtro = "")
           => _bl.CntXAcCuentas_Consulta(codEmpresa, codContabilidad, rol, ctaInicio, ctaCorte, filtro, usuario);

        [HttpGet("CntXAcCuentas_Consulta_Asignadas")]
        public ActionResult<ErrorDto<List<CntXAcCuentaDto>>> CntXAcCuentas_Consulta_Asignadas(
            [FromQuery] int codEmpresa,
            [FromQuery] int codContabilidad,
            [FromQuery] string rol,
            [FromQuery] string usuario,
            [FromQuery] string filtro = "")
            => _bl.CntXAcCuentas_Consulta_Asignadas(codEmpresa, codContabilidad, rol, filtro, usuario);

        [HttpGet("CntXAcUnidades_Consulta")]
        public ActionResult<ErrorDto<List<CntXAcUnidadDto>>> CntXAcUnidades_Consulta(
            [FromQuery] int codEmpresa,
            [FromQuery] int codContabilidad,
            [FromQuery] string rol,
            [FromQuery] string usuario,
            [FromQuery] string filtro = "")
            => _bl.CntXAcUnidades_Consulta(codEmpresa, codContabilidad, rol, filtro, usuario);

        [HttpGet("CntXAcCentroCosto_Consulta")]
        public ActionResult<ErrorDto<List<CntXAcCentroCostoDto>>> CntXAcCentroCosto_Consulta(
            [FromQuery] int codEmpresa,
            [FromQuery] int codContabilidad,
            [FromQuery] string rol,
            [FromQuery] string unidad,
            [FromQuery] string usuario,
            [FromQuery] string filtro = "")
            => _bl.CntXAcCentroCosto_Consulta(codEmpresa, codContabilidad, rol, unidad, filtro, usuario);
    }
}
