using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX_Contabilidad;
using Galileo_API.Models.ProGrX_Contabilidad;
using System.Collections.Generic;

namespace Galileo_API.BusinessLogic.ProGrX_Contabilidad
{
    public class FrmCntXErCuentasBL
    {
        private readonly FrmCntXErCuentasDB _db;

        public FrmCntXErCuentasBL(IConfiguration config)
        {
            _db = new FrmCntXErCuentasDB(config);
        }

        public ErrorDto<List<CntXInvPeriodicoDto>> CntXInvPeriodico_Lista(int codEmpresa, int codContabilidad)
            => _db.CntXInvPeriodico_Lista(codEmpresa, codContabilidad);

        public ErrorDto<List<DropDownListaGenericaModel>> CntXCuentasClasificacion(int codEmpresa, int codContabilidad)
            => _db.CntXCuentasClasificacion(codEmpresa, codContabilidad);

        public ErrorDto<bool> CntXInvPeriodico_Guardar(int codEmpresa, CntXInvPeriodicoSaveParams param)
            => _db.CntXInvPeriodico_Guardar(codEmpresa, param);

        public ErrorDto<bool> CntXInvPeriodico_Eliminar(int codEmpresa, CntXInvPeriodicoDeleteParams param)
            => _db.CntXInvPeriodico_Eliminar(codEmpresa, param);

        public ErrorDto<int> CntXCuentasClasificacionA_Validar(int codEmpresa, int codContabilidad, string codCuenta)
            => _db.CntXCuentasClasificacionA_Validar(codEmpresa, codContabilidad, codCuenta);
    }
}
