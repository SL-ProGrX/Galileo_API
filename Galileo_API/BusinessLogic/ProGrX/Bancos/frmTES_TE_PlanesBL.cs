using Galileo.Models.ERROR;
using Galileo.Models.TES;
using Galileo_API.DataBaseTier;

namespace Galileo_API.BusinessLogic
{
    public class FrmTesTePlanesBL
    {
        private readonly FrmTesTePlanesDB PlanesDb;

        public FrmTesTePlanesBL(IConfiguration config)
        {
            PlanesDb = new FrmTesTePlanesDB(config);
        }

        public ErrorDto<TesBancoPlanesData> TES_Planes_Scroll(int CodEmpresa, int scrollCode, string codPlan, int banco)
        {
            return PlanesDb.TES_Planes_Scroll(CodEmpresa, scrollCode, codPlan, banco);
        }

        public ErrorDto<TesBancoPlanesData> TES_PlanesConsulta_Obtener(int CodEmpresa, int banco, string codPlan)
        {
            return PlanesDb.TES_PlanesConsulta_Obtener(CodEmpresa, banco, codPlan);
        }

        public ErrorDto<Galileo.Models.ProGrX.Bancos.TesBancosGruposData> TES_Planes_BancosGrupos_Obtener(int CodEmpresa, int banco)
        {
            return PlanesDb.TES_Planes_BancosGrupos_Obtener(CodEmpresa, banco);
        }

        public ErrorDto TES_Planes_Guardar(int CodEmpresa, string infoPlan)
        {
            return PlanesDb.TES_Planes_Guardar(CodEmpresa, infoPlan);
        }

        public ErrorDto TES_Planes_Borrar(int CodEmpresa, string infoPlan)
        {
            return PlanesDb.TES_Planes_Borrar(CodEmpresa, infoPlan);
        }
    }
}