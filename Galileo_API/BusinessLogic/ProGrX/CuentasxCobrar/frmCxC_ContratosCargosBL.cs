using Galileo_API.DataBaseTier.ProGrX.CuentasxCobrar;
using Galileo_API.Models.ProGrX.CuentasxCobrar;
using Galileo.Models.ERROR;
using System.Collections.Generic;

namespace Galileo_API.BusinessLogic.ProGrX.CuentasxCobrar
{
    public class FrmCxCContratosCargosBL
    {
        private readonly FrmCxCContratosCargosDB _db;

        public FrmCxCContratosCargosBL(IConfiguration config)
        {
            _db = new FrmCxCContratosCargosDB(config);
        }

        public ErrorDto<List<CxcCargoDto>> CxcCargos_Lista(int codEmpresa, string orden)
        {
            return _db.CxcCargos_Lista(codEmpresa, orden);
        }

        public ErrorDto<List<CxcContratoCargoDto>> CxcContratoCargos_Lista(int codEmpresa, string codContrato)
        {
            return _db.CxcContratoCargos_Lista(codEmpresa, codContrato);
        }

        public ErrorDto<bool> CxcContratoCargo_Guardar(int codEmpresa, CxcContratoCargoSaveParams param)
        {
            return _db.CxcContratoCargo_Guardar(codEmpresa, param);
        }

        public ErrorDto<bool> CxcContratoCargo_Eliminar(int codEmpresa, CxcContratoCargoDeleteParams param)
        {
            return _db.CxcContratoCargo_Eliminar(codEmpresa, param);
        }
    }
}
