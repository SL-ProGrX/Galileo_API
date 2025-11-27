using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.PRES;

namespace Galileo.BusinessLogic
{
    public class FrmPresAnaliticoBl
    {
        readonly FrmPresAnaliticoDb _db;

        public FrmPresAnaliticoBl(IConfiguration config)
        {
            _db = new FrmPresAnaliticoDb(config);
        }

        public ErrorDto<List<PresAnaliticoDescData>> PresAnaliticoDesc_Obtener(int CodCliente, string datos)
        {
            return _db.PresAnaliticoDesc_Obtener(CodCliente, datos);
        }

        public ErrorDto<List<PresAnaliticoData>> PresAnalitico_Obtener(int CodCliente, string datos)
        {
            return _db.PresAnalitico_Obtener(CodCliente, datos);
        }
    }
}