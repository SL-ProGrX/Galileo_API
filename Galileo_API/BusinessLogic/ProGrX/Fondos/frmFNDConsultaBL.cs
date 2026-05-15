using Newtonsoft.Json;
using Galileo.DataBaseTier.ProGrX.Fondos;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Fondos;

namespace Galileo.BusinessLogic.ProGrX.Fondos
{
    public class FrmFndConsultaBl
    {
        private readonly FrmFndConsultaDb DbFndConsulta;

        public FrmFndConsultaBl(IConfiguration config)
        {
            DbFndConsulta = new FrmFndConsultaDb(config);
        }

        public ErrorDto<List<FndConsultaDto>> FND_Consulta_Obtener(int CodEmpresa, string Filtros)
        {
            FndConsultaFiltros filtro = JsonConvert.DeserializeObject<FndConsultaFiltros>(Filtros) ?? new FndConsultaFiltros();
            return DbFndConsulta.FND_Consulta_Obtener(CodEmpresa, filtro);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> FND_Consulta_Operadora_Obtener(int CodEmpresa)
        {
            return DbFndConsulta.FND_Consulta_Operadora_Obtener(CodEmpresa);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> FND_Consulta_Planes_Obtener(int CodEmpresa, int? Operadora)
        {
            return DbFndConsulta.FND_Consulta_Planes_Obtener(CodEmpresa, Operadora);
        }
    }
}
