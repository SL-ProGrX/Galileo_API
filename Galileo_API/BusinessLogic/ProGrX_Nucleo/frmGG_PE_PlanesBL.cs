using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.GG_PE;

namespace Galileo.BusinessLogic
{
    public class FrmGgPePlanesBL(IConfiguration config)
    {
        private readonly FrmGgPePlanesDB _db = new(config);

        public ErrorDto<PePlanesDatosLista> PePlanesLista_Obtener(int CodEmpresa, string Jfiltros)
        {
            return _db.PePlanesLista_Obtener(CodEmpresa, Jfiltros);
        }

        public ErrorDto PePlanes_Guardar(int CodEmpresa, PePlanesDto plan)
        {
            return _db.PePlanes_Guardar(CodEmpresa, plan);
        }

        public ErrorDto PePlanes_Eliminar(int CodEmpresa, int pe_id)
        {
            return _db.PePlanes_Eliminar(CodEmpresa, pe_id);
        }

        public ErrorDto<List<PePlanesDto>> PePlanes_Exportar(int CodEmpresa)
        {
            return _db.PePlanes_Exportar(CodEmpresa);
        }

    }
}