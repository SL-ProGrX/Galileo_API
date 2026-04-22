using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX_Hipotecario;
using Galileo_API.Models.ProGrX.Cobros;
using Galileo_API.Models.ProGrX_Hipotecario;

namespace Galileo_API.BusinessLogic.ProGrX_Hipotecario
{
    public class FrmVivDesembolsosBl
    {
        private readonly FrmVivDesembolsosDb _db;

        public FrmVivDesembolsosBl(IConfiguration config)
            => _db = new FrmVivDesembolsosDb(config);


        public ErrorDto<List<OperacionBusquedaDto>> Operaciones_Listar(int codEmpresa)
        {
            return _db.Operaciones_Listar(codEmpresa);
        }




    }
}
