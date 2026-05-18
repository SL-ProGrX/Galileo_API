using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX_Comites;
using Galileo_API.Models.ProGrX_Comites;

namespace Galileo_API.BusinessLogic.ProGrX_Comites
{
    public class FrmAfCdInformeEspecialBl
    {
        private readonly FrmAfCdInformeEspecialDb _db;

        public FrmAfCdInformeEspecialBl(IConfiguration config) =>
            _db = new FrmAfCdInformeEspecialDb(config);

        public ErrorDto<AfCdInformeEspecialPantallaData> AfCdInformeEspecial_Pantalla_Obtener(int codEmpresa)
        {
            return _db.AfCdInformeEspecial_Pantalla_Obtener(codEmpresa);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> AfCdInformeEspecial_Comites_Obtener(int codEmpresa, string codZona)
        {
            return _db.AfCdInformeEspecial_Comites_Obtener(codEmpresa, codZona);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> AfCdInformeEspecial_Unidades_Obtener(int codEmpresa, string codComite)
        {
            return _db.AfCdInformeEspecial_Unidades_Obtener(codEmpresa, codComite);
        }
    }
}
