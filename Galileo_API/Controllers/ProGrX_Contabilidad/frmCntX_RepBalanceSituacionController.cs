using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX_Contabilidad;
using Galileo_API.Models.ProGrX_Contabilidad;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX_Contabilidad
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class FrmCntXRepBalanceSituacionController :
        ControllerBase
    {
        private readonly FrmCntXRepBalanceSituacionBl _bl;

        public FrmCntXRepBalanceSituacionController(
            IConfiguration config)
        {
            _bl = new FrmCntXRepBalanceSituacionBl(config);
        }

        [HttpGet(
            "CntX_frmCntX_RepBalanceSituacion_Inicializar")]
        public ErrorDto<CntXRepBalanceSituacionInicializarResponse>
            CntX_frmCntX_RepBalanceSituacion_Inicializar(
                int codEmpresa,
                int codContabilidad)
        {
            return _bl
                .CntX_frmCntX_RepBalanceSituacion_Inicializar(
                    codEmpresa,
                    codContabilidad);
        }

        [HttpGet(
            "CntX_frmCntX_RepBalanceSituacion_CentrosCosto_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>>
            CntX_frmCntX_RepBalanceSituacion_CentrosCosto_Obtener(
                int codEmpresa,
                int codContabilidad,
                string? unidad)
        {
            return _bl
                .CntX_frmCntX_RepBalanceSituacion_CentrosCosto_Obtener(
                    codEmpresa,
                    codContabilidad,
                    unidad);
        }

        [HttpGet(
            "CntX_frmCntX_RepBalanceSituacion_Cuenta_Obtener")]
        public ErrorDto<CntXRepBalanceSituacionCuentaData?>
            CntX_frmCntX_RepBalanceSituacion_Cuenta_Obtener(
                int codEmpresa,
                int codContabilidad,
                string? cuenta)
        {
            return _bl
                .CntX_frmCntX_RepBalanceSituacion_Cuenta_Obtener(
                    codEmpresa,
                    codContabilidad,
                    cuenta);
        }

        [HttpPost(
            "CntX_frmCntX_RepBalanceSituacion_Reporte_Preparar")]
        public ErrorDto<CntXRepBalanceSituacionPrepararResponse>
            CntX_frmCntX_RepBalanceSituacion_Reporte_Preparar(
                int codEmpresa,
                CntXRepBalanceSituacionPrepararRequest request)
        {
            return _bl
                .CntX_frmCntX_RepBalanceSituacion_Reporte_Preparar(
                    codEmpresa,
                    request);
        }
    }
}