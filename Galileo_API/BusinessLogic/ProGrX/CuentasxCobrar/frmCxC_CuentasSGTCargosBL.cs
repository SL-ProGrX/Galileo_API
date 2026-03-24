using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.CuentasxCobrar;
using Galileo_API.Models.ProGrX.CuentasxCobrar;

namespace Galileo_API.BusinessLogic.ProGrX.CuentasxCobrar
{
    public class FrmCxCCuentasCargosBL
    {
        private readonly FrmCxCCuentasCargosDB Db;

        public FrmCxCCuentasCargosBL(IConfiguration config)
        {
            Db = new FrmCxCCuentasCargosDB(config);
        }

        public ErrorDto<CxCCuentasCargoOperacionDto> CxC_Cuentas_Cargos_Operacion_Obtener(int CodEmpresa, int operacion)
        {
            return Db.CxC_Cuentas_Cargos_Operacion_Obtener(CodEmpresa, operacion);
        }

        public ErrorDto<CxCCuentasCargosListaResult> CxC_Cuentas_Cargos_Operacion_Export(int CodEmpresa, int operacion)
        {
            return Db.CxC_Cuentas_Cargos_Operacion_Export(CodEmpresa, operacion);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> CxC_Cuentas_Cargos_Disponibles_Obtener(int CodEmpresa, int operacion, string? filtro)
        {
            return Db.CxC_Cuentas_Cargos_Disponibles_Obtener(CodEmpresa, operacion, filtro);
        }

        public ErrorDto<CxCCuentasCargoDisponibleDto> CxC_Cuentas_Cargos_Scroll_Obtener(int CodEmpresa, int operacion, int scrollCode, string? cargoActual)
        {
            return Db.CxC_Cuentas_Cargos_Scroll_Obtener(CodEmpresa, operacion, scrollCode, cargoActual);
        }

        public ErrorDto CxC_Cuentas_Cargos_Guardar(int CodEmpresa, string usuario, CxCCuentasCargoData cargo)
        {
            return Db.CxC_Cuentas_Cargos_Guardar(CodEmpresa, usuario, cargo);
        }

        public ErrorDto CxC_Cuentas_Cargos_Eliminar(int CodEmpresa, string usuario, int operacion, string codCargo)
        {
            return Db.CxC_Cuentas_Cargos_Eliminar(CodEmpresa, usuario, operacion, codCargo);
        }
    }
}