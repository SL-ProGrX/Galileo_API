using Galileo_API.DataBaseTier.ProGrX.Clientes;
using Galileo.Models.ERROR;
using Galileo.Models;
using System.Collections.Generic;

namespace Galileo_API.BusinessLogic.ProGrX.Clientes
{
    public class FrmAFReportesRenunciasBL
    {
        private readonly FrmAFReportesRenunciasDB _db;

        public FrmAFReportesRenunciasBL(IConfiguration config)
        {
            _db = new FrmAFReportesRenunciasDB(config);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> AfReportesRenunciasOficinas_Obtener(int codEmpresa)
        {
            return _db.AfReportesRenunciasOficinas_Obtener(codEmpresa);
        }
    }
}
