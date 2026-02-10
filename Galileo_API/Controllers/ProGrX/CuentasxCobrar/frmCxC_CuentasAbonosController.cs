
using Galileo.Models;
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
    public class FrmCxCCuentasAbonosController : ControllerBase
    {
        private readonly FrmCxCCuentasAbonosBl _bl;

        public FrmCxCCuentasAbonosController(IConfiguration config) => _bl = new FrmCxCCuentasAbonosBl(config);

        [HttpGet("CxCCuentas_ConsultaOperacion_Obtener")]
        public ErrorDto<CxCCuentasAbonosData> CxCCuentas_ConsultaOperacion_Obtener(int codEmpresa, string codCaja, int operacionId)
        {
            return _bl.CxCCuentas_ConsultaOperacion_Obtener(codEmpresa, codCaja, operacionId);
        }

        [HttpGet("CxCCuentas_CuotasActivas_Obtener")]
        public ErrorDto<List<CxCCuotasActivasData>> CxCCuentas_CuotasActivas_Obtener(int codEmpresa, int operacionId)
        {
            return _bl.CxCCuentas_CuotasActivas_Obtener(codEmpresa, operacionId);
        }

        [HttpGet("CxCCuentas_OperacionesActivas_Obtener")]
        public ErrorDto<List<CxCOperacionesActivasData>> CxCCuentas_OperacionesActivas_Obtener(int codEmpresa)
        {
            return _bl.CxCCuentas_OperacionesActivas_Obtener(codEmpresa);
        }

        [HttpGet("CxCCuentas_TipoDoc_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CxCCuentas_TipoDoc_Obtener(int codEmpresa, string caja)
        {
            return _bl.CxCCuentas_TipoDoc_Obtener(codEmpresa, caja);
        }
    }
}