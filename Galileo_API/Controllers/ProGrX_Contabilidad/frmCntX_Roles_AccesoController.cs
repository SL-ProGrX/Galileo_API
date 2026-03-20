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

        [HttpGet("CntXAcMiembros_Consulta")]
        public ActionResult<ErrorDto<List<CntXAcMiembroDto>>> CntXAcMiembros_Consulta(
            [FromQuery] int codEmpresa,
            [FromQuery] int codContabilidad,
            [FromQuery] string rol,
            [FromQuery] string usuario,
            [FromQuery] string filtro = "")
            => _bl.CntXAcMiembros_Consulta(codEmpresa, codContabilidad, rol, filtro, usuario);

        [HttpPost("CntXAcCuentas_Asigna")]
        public ActionResult<ErrorDto<bool>> CntXAcCuentas_Asigna([FromQuery] int codEmpresa, [FromBody] CntXAcCuentaAsignaParams param)
            => _bl.CntXAcCuentas_Asigna(codEmpresa, param);

        [HttpPost("CntXAcUnidades_Asigna")]
        public ActionResult<ErrorDto<bool>> CntXAcUnidades_Asigna([FromQuery] int codEmpresa, [FromBody] CntXAcUnidadAsignaParams param)
            => _bl.CntXAcUnidades_Asigna(codEmpresa, param);

        [HttpPost("CntXAcCentroCosto_Asigna")]
        public ActionResult<ErrorDto<bool>> CntXAcCentroCosto_Asigna([FromQuery] int codEmpresa, [FromBody] CntXAcCentroCostoAsignaParams param)
            => _bl.CntXAcCentroCosto_Asigna(codEmpresa, param);

        [HttpPost("CntXAcMiembros_Asigna")]
        public ActionResult<ErrorDto<bool>> CntXAcMiembros_Asigna([FromQuery] int codEmpresa, [FromBody] CntXAcMiembroAsignaParams param)
            => _bl.CntXAcMiembros_Asigna(codEmpresa, param);

        [HttpPost("CntXAcRol_Add")]
        public ActionResult<ErrorDto<bool>> CntXAcRol_Add([FromQuery] int codEmpresa, [FromBody] CntXAcRolAddParams param)
            => _bl.CntXAcRol_Add(codEmpresa, param);

        [HttpPost("CntXAcRol_Delete")]
        public ActionResult<ErrorDto<bool>> CntXAcRol_Delete([FromQuery] int codEmpresa, [FromBody] CntXAcRolDeleteParams param)
            => _bl.CntXAcRol_Delete(codEmpresa, param);
    }
}
