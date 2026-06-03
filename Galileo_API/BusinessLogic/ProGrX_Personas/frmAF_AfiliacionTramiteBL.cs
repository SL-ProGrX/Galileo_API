using Newtonsoft.Json;
using Galileo.DataBaseTier.ProGrX_Personas;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Personas;

namespace Galileo.BusinessLogic.ProGrx_Personas
{
    public class FrmAfAfiliacionTramiteBl
    {
        private readonly FrmAfAfiliacionTramiteDb DbAfAfiliacionTramite;

        public FrmAfAfiliacionTramiteBl(IConfiguration config)
        {
            DbAfAfiliacionTramite = new FrmAfAfiliacionTramiteDb(config);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> AF_AfiliacionTramite_Instituciones_Obtener(int CodEmpresa)
        {
            return DbAfAfiliacionTramite.AF_AfiliacionTramite_Instituciones_Obtener(CodEmpresa);
        }

        public ErrorDto<List<AfAfiliacionTramiteDto>> AF_AfiliacionTramite_Obtener(int CodEmpresa, string Filtro)
        {
            AfAfiliacionTramiteFiltros filtros = JsonConvert.DeserializeObject<AfAfiliacionTramiteFiltros>(Filtro) ?? new AfAfiliacionTramiteFiltros();
            return DbAfAfiliacionTramite.AF_AfiliacionTramite_Obtener(CodEmpresa, filtros);
        }

        public ErrorDto AF_AfiliacionTramite_Aprobar(int CodEmpresa, List<AfAfiliacionTramiteDto> Lista, string Usuario)
        {
            return DbAfAfiliacionTramite.AF_AfiliacionTramite_Aprobar(CodEmpresa, Lista, Usuario);
        }
    }
}