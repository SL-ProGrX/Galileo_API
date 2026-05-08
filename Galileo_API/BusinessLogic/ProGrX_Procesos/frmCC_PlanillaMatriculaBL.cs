using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.Procesos;
using Galileo_API.Models.ProGrX_Procesos;

namespace Galileo_API.BusinessLogic.ProGrX.Procesos
{
    public class FrmCCPlanillaMatriculaBL
    {
        private readonly FrmCCPlanillaMatriculaDB DB;

        public FrmCCPlanillaMatriculaBL(IConfiguration config)
        {
            DB = new FrmCCPlanillaMatriculaDB(config);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> CC_PlanillaMatricula_Instituciones_Dropdown_Obtener(int CodEmpresa)
        {
            return DB.CC_PlanillaMatricula_Instituciones_Dropdown_Obtener(CodEmpresa);
        }

        public ErrorDto<CcPlanillaMatriculaListaResultDto> CC_PlanillaMatricula_Lista_Obtener(int CodEmpresa, string parametros)
        {
            return DB.CC_PlanillaMatricula_Lista_Obtener(CodEmpresa, parametros);
        }

        public ErrorDto<CcPlanillaMatriculaListaResultDto> CC_PlanillaMatricula_Lista_Export(int CodEmpresa, string parametros)
        {
            return DB.CC_PlanillaMatricula_Lista_Export(CodEmpresa, parametros);
        }

        public ErrorDto CC_PlanillaMatricula_Bloquear(int CodEmpresa, string usuario, CcPlanillaMatriculaBloquearRequest request)
        {
            return DB.CC_PlanillaMatricula_Bloquear(CodEmpresa, usuario, request);
        }

        public ErrorDto<CcPlanillaMatriculaBloqueoMasivoResultDto> CC_PlanillaMatricula_BloqueoMasivo(int CodEmpresa,string usuario,CcPlanillaMatriculaBloqueoMasivoRequest request)
        {
            return DB.CC_PlanillaMatricula_BloqueoMasivo(CodEmpresa, usuario, request);
        }
        public ErrorDto<CcPlanillaMatriculaArchivoTotalDto> CC_PlanillaMatricula_ArchivoTotal_Generar(int CodEmpresa,CcPlanillaMatriculaArchivoTotalRequest request)
        {
            return DB.CC_PlanillaMatricula_ArchivoTotal_Generar(CodEmpresa, request);
        }
    }
}