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
    public class FrmCntXCuentaForaneaInicializaController : ControllerBase
    {
        private readonly FrmCntXCuentaForaneaInicializaBl _bl;

        public FrmCntXCuentaForaneaInicializaController(IConfiguration config) => 
            _bl = new FrmCntXCuentaForaneaInicializaBl(config);

        [HttpGet("CntXDivisaLocal_Obtener")]
        public ErrorDto<string?> CntXDivisaLocal_Obtener(int codEmpresa, int codConta)
        {
            return _bl.CntXDivisaLocal_Obtener(codEmpresa, codConta);
        }
        
        [HttpGet("CntXCuentaForaneas_Obtener")]
        public ErrorDto<List<CntXCuentaForaneaData>> CntXCuentaForaneas_Obtener(int codEmpresa, int codConta, string codDivisa)
        {
            return _bl.CntXCuentaForaneas_Obtener(codEmpresa, codConta, codDivisa);
        }

        [HttpGet("CntXCuentaMovSaldo_Obtener")]
        public ErrorDto<CntXCuentaMovSaldoData?> CntXCuentaMovSaldo_Obtener(int codEmpresa, int codConta, string codCuenta, int anio, int mes)
        {
            return _bl.CntXCuentaMovSaldo_Obtener(codEmpresa, codConta, codCuenta, anio, mes);
        }

        [HttpPost("CntXCuentaForanea_Inicializar")]
        public ErrorDto CntXCuentaForanea_Inicializar(int codEmpresa, CntXCuentaForaneaInicializaRequest request)
        {
            return _bl.CntXCuentaForanea_Inicializar(codEmpresa, request);
        }
    }
}