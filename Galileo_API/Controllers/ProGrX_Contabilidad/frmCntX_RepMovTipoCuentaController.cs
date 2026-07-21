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
    public class FrmCntXRepMovTipoCuentaController :
        ControllerBase
    {
        private readonly FrmCntXRepMovTipoCuentaBl _bl;

        public FrmCntXRepMovTipoCuentaController(
            IConfiguration config)
        {
            _bl = new FrmCntXRepMovTipoCuentaBl(config);
        }

        [HttpGet(
            "CntX_frmCntX_RepMovTipoCuenta_Inicializar")]
        public ErrorDto<CntXRepMovTipoCuentaInicializarResponse>
            CntX_frmCntX_RepMovTipoCuenta_Inicializar(
                int codEmpresa,
                int codContabilidad)
        {
            return _bl
                .CntX_frmCntX_RepMovTipoCuenta_Inicializar(
                    codEmpresa,
                    codContabilidad);
        }

        [HttpGet(
            "CntX_frmCntX_RepMovTipoCuenta_CentrosCosto_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>>
            CntX_frmCntX_RepMovTipoCuenta_CentrosCosto_Obtener(
                int codEmpresa,
                int codContabilidad,
                string? unidad)
        {
            return _bl
                .CntX_frmCntX_RepMovTipoCuenta_CentrosCosto_Obtener(
                    codEmpresa,
                    codContabilidad,
                    unidad);
        }

        [HttpGet(
            "CntX_frmCntX_RepMovTipoCuenta_Cuenta_Obtener")]
        public ErrorDto<CntXRepMovTipoCuentaData?>
            CntX_frmCntX_RepMovTipoCuenta_Cuenta_Obtener(
                int codEmpresa,
                int codContabilidad,
                string? cuenta)
        {
            return _bl
                .CntX_frmCntX_RepMovTipoCuenta_Cuenta_Obtener(
                    codEmpresa,
                    codContabilidad,
                    cuenta);
        }

        [HttpPost(
            "CntX_frmCntX_RepMovTipoCuenta_Reporte_Preparar")]
        public ErrorDto
            CntX_frmCntX_RepMovTipoCuenta_Reporte_Preparar(
                int codEmpresa,
                CntXRepMovTipoCuentaPrepararRequest request)
        {
            return _bl
                .CntX_frmCntX_RepMovTipoCuenta_Reporte_Preparar(
                    codEmpresa,
                    request);
        }
    }
}