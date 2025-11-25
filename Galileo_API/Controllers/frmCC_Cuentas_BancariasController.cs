using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic;
using Galileo.Models;
using Galileo.Models.ERROR;

namespace Galileo.Controllers
{
    [Route("api/FrmCcCuentasBancarias")]
    [Route("api/frmCC_Cuentas_Bancarias")]
    [ApiController]
    public class FrmCcCuentasBancariasController : ControllerBase
    {

        private readonly IConfiguration _config;

        public FrmCcCuentasBancariasController(IConfiguration config)
        {
            _config = config;
        }


        [HttpGet("BancosCC_Obtener")]
        public List<BancosCC> BancosCC_Obtener(int CodEmpresa)
        {
            return new FrmCcCuentasBancariasBl(_config).BancosCC_Obtener(CodEmpresa);
        }


        [HttpGet("ValidacionCC_Obtener")]
        public ValidacionCC ValidacionCC_Obtener(int CodEmpresa, string Cod_Grupo)
        {
            return new FrmCcCuentasBancariasBl(_config).ValidacionCC_Obtener(CodEmpresa, Cod_Grupo);
        }


        [HttpGet("CuentasBancarias_Obtener")]
        public List<SysCuentasBancariasDto> CuentasBancarias_Obtener(int CodEmpresa, string cedula, string? modulo)
        {
            return new FrmCcCuentasBancariasBl(_config).CuentasBancarias_Obtener(CodEmpresa, cedula, modulo);
        }


        [HttpPatch("CuentaBancaria_Actualizar")]
        public ErrorDto CuentaBancaria_Actualizar(int CodEmpresa, SysCuentasBancariasDto data)
        {
            return new FrmCcCuentasBancariasBl(_config).CuentaBancaria_Actualizar(CodEmpresa, data);
        }

        [HttpPost("CuentaBancaria_Insertar")]
        public ErrorDto CuentaBancaria_Insertar(int CodEmpresa, SysCuentasBancariasDto data)
        {
            return new FrmCcCuentasBancariasBl(_config).CuentaBancaria_Insertar(CodEmpresa, data);
        }


        [HttpDelete("CuentaBancaria_Borrar")]
        public ErrorDto CuentaBancaria_Borrar(int CodEmpresa, string data)
        {
            return new FrmCcCuentasBancariasBl(_config).CuentaBancaria_Borrar(CodEmpresa, data);
        }

    }
}
