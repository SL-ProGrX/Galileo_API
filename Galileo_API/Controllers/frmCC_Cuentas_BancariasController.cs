using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic;
using Galileo.Models;
using Galileo.Models.ERROR;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Galileo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class frmCC_Cuentas_BancariasController : Controller
    {

        private readonly IConfiguration _config;

        public frmCC_Cuentas_BancariasController(IConfiguration config)
        {
            _config = config;
        }


        [HttpGet("BancosCC_Obtener")]
        public List<BancosCC> BancosCC_Obtener(int CodEmpresa)
        {
            return new frmCC_Cuentas_BancariasBL(_config).BancosCC_Obtener(CodEmpresa);
        }


        [HttpGet("ValidacionCC_Obtener")]
        public ValidacionCC ValidacionCC_Obtener(int CodEmpresa, string Cod_Grupo)
        {
            return new frmCC_Cuentas_BancariasBL(_config).ValidacionCC_Obtener(CodEmpresa, Cod_Grupo);
        }


        [HttpGet("CuentasBancarias_Obtener")]
        public List<SysCuentasBancariasDto> CuentasBancarias_Obtener(int CodEmpresa, string cedula, string? modulo)
        {
            return new frmCC_Cuentas_BancariasBL(_config).CuentasBancarias_Obtener(CodEmpresa, cedula, modulo);
        }


        [HttpPatch("CuentaBancaria_Actualizar")]
        public ErrorDto CuentaBancaria_Actualizar(int CodEmpresa, SysCuentasBancariasDto data)
        {
            return new frmCC_Cuentas_BancariasBL(_config).CuentaBancaria_Actualizar(CodEmpresa, data);
        }

        [HttpPost("CuentaBancaria_Insertar")]
        public ErrorDto CuentaBancaria_Insertar(int CodEmpresa, SysCuentasBancariasDto data)
        {
            return new frmCC_Cuentas_BancariasBL(_config).CuentaBancaria_Insertar(CodEmpresa, data);
        }


        [HttpDelete("CuentaBancaria_Borrar")]
        public ErrorDto CuentaBancaria_Borrar(int CodEmpresa, string data)
        {
            return new frmCC_Cuentas_BancariasBL(_config).CuentaBancaria_Borrar(CodEmpresa, data);
        }

    }
}
