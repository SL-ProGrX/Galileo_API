using Galileo.DataBaseTier.ProGrX.Fondos;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Fondos;

namespace Galileo.BusinessLogic.ProGrX.Fondos
{
    public class FrmFndExclusionMultaBl
    {
        private readonly FrmFndExclusionMultaDb _Db;

        public FrmFndExclusionMultaBl(IConfiguration config)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));
            _Db = new FrmFndExclusionMultaDb(config);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> FND_Operadoras_Obtener(int CodEmpresa)
        {
            return _Db.FND_Operadoras_Obtener(CodEmpresa);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> FND_Planes_Obtener(int CodEmpresa, string cod_operadora)
        {
            return _Db.FND_Planes_Obtener(CodEmpresa, cod_operadora);
        }

        public ErrorDto<List<FndContratoDto>> FND_Contratos_Obtener(int CodEmpresa, string cod_operadora, string cod_plan)
        {
            return _Db.FND_Contratos_Obtener(CodEmpresa, cod_operadora, cod_plan);
        }

        public ErrorDto<List<FndExclusionMultaDto>> FND_Exclusion_Multas_List(int CodEmpresa, FiltrosBuscarExclusionDto filtros)
        {
            return _Db.FND_Exclusion_Multas_List(CodEmpresa, filtros);
        }

        public ErrorDto FND_Exclusion_Multas_Add(int CodEmpresa, RegistrarExclusionDto request)
        {
            return _Db.FND_Exclusion_Multas_Add(CodEmpresa, request);
        }
    }
}
