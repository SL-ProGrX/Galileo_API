using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX_Polizas;
using Galileo.Models;
using Galileo_API.Models.ProGrX_Polizas;
using System.Collections.Generic;

namespace Galileo_API.BusinessLogic.ProGrX_Polizas
{
    public class FrmPolizasCatCcmBL
    {
        private readonly FrmPolizasCatCcmDB _db;

        public FrmPolizasCatCcmBL(IConfiguration config)
        {
            _db = new FrmPolizasCatCcmDB(config);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> PolizasCatalogo_Listar(int codEmpresa)
            => _db.PolizasCatalogo_Listar(codEmpresa);

        public ErrorDto<List<PolizasCoberturasMotivosCausasDto>> PolizasConceptosConfigListas(int codEmpresa, string codPoliza, string tipo)
            => _db.PolizasConceptosConfigListas(codEmpresa, codPoliza, tipo);

        public ErrorDto<PolizasConceptosConfigAddResult> PolizasConceptosConfigAdd(int codEmpresa, PolizasConceptosConfigAddParams param)
            => _db.PolizasConceptosConfigAdd(codEmpresa, param);

        public ErrorDto<PolizasConceptosConfigAddResult> PolizasConceptosConfigDel(int codEmpresa, PolizasConceptosConfigDelParams param)
            => _db.PolizasConceptosConfigDel(codEmpresa, param);
    }
}
