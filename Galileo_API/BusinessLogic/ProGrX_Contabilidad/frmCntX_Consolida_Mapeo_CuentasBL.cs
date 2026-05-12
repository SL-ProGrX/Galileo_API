using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX_Contabilidad;
using System.Collections.Generic;

namespace Galileo_API.BusinessLogic.ProGrX_Contabilidad
{
    public class FrmCntXConsolidaMapeoCuentasBL
    {
        private readonly FrmCntXConsolidaMapeoCuentasDB _db;

        public FrmCntXConsolidaMapeoCuentasBL(IConfiguration config)
        {
            _db = new FrmCntXConsolidaMapeoCuentasDB(config);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> ConsolidaMapeoCuentas_ObtenerUnidades(int codEmpresa, int mContabilidad)
            => _db.ConsolidaMapeoCuentas_ObtenerUnidades(codEmpresa, mContabilidad);
    }
}
