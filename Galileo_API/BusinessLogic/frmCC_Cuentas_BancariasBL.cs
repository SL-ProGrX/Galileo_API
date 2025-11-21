using Newtonsoft.Json;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;

namespace Galileo.BusinessLogic
{
    public class FrmCcCuentasBancariasBl
    {
        private readonly IConfiguration _config;
        FrmCcCuentasBancariasDb DB_Cuentas;

        public FrmCcCuentasBancariasBl(IConfiguration config)
        {
            _config = config;
            DB_Cuentas = new FrmCcCuentasBancariasDb(_config);
        }

        public List<BancosCC> BancosCC_Obtener(int CodEmpresa)
        {
            return DB_Cuentas.BancosCC_Obtener(CodEmpresa);
        }

        public ValidacionCC ValidacionCC_Obtener(int CodEmpresa, string Cod_Grupo)
        {
            return DB_Cuentas.ValidacionCC_Obtener(CodEmpresa, Cod_Grupo);
        }

        public List<SysCuentasBancariasDto> CuentasBancarias_Obtener(int CodEmpresa, string cedula, string? modulo)
        {
            return DB_Cuentas.CuentasBancarias_Obtener(CodEmpresa, cedula, modulo);
        }

        public ErrorDto CuentaBancaria_Actualizar(int CodEmpresa, SysCuentasBancariasDto data)
        {
            return DB_Cuentas.CuentaBancaria_Actualizar(CodEmpresa, data);
        }

        public ErrorDto CuentaBancaria_Insertar(int CodEmpresa, SysCuentasBancariasDto data)
        {
            return DB_Cuentas.CuentaBancaria_Insertar(CodEmpresa, data);
        }

        public ErrorDto CuentaBancaria_Borrar(int CodEmpresa, string jData)
        {
            SysCuentasBancariasDto data = JsonConvert.DeserializeObject<SysCuentasBancariasDto>(jData);
            return DB_Cuentas.CuentaBancaria_Borrar(CodEmpresa, data);
        }

    }//end class
}//end namespace

