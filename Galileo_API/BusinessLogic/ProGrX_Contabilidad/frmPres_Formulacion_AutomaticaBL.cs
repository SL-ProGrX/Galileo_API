using Galileo.DataBaseTier.ProGrX_Contabilidad;
using Galileo.Models.ERROR;
using Galileo.Models.PRES;
using Galileo.Models.ProGrX_Contabilidad;

namespace Galileo.BusinessLogic.ProGrX_Contabilidad
{
    public class FrmPresFormulacionAutomaticaBl
    {
        readonly FrmPresFormulacionAutomaticaDb _db;
        public FrmPresFormulacionAutomaticaBl(IConfiguration config)
        {
            _db = new FrmPresFormulacionAutomaticaDb(config);
        }

        public ErrorDto<List<PresModelisLista>> Pres_Modelos_Obtener(int CodEmpresa, int CodContab, string Usuario)
        {
            return _db.Pres_Modelos_Obtener(CodEmpresa, CodContab, Usuario);
        }

        public ErrorDto<List<PresFormulacionAutoDto>> Pres_Formulacion_Automatica(
           int CodEmpresa, string CodModelo, string vTipo, string Usuario)
        {
            return _db.Pres_Formulacion_Automatica(CodEmpresa, CodModelo, vTipo, Usuario);
        }
    }
}
