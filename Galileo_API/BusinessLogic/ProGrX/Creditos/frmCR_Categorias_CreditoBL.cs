
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.Creditos;
using Galileo_API.Models.ProGrX.Creditos;

namespace Galileo_API.BusinessLogic.ProGrX.Creditos
{
    public class FrmCrCategoriasCreditoBl
    {
        private readonly FrmCrCategoriasCreditoDb _db;

        public FrmCrCategoriasCreditoBl(
            IConfiguration config)
        {
            _db = new FrmCrCategoriasCreditoDb(
                config);
        }

        public ErrorDto<List<
            CrCategoriasCreditoProbabilidadDefaultData>>
            CR_frmCR_Categorias_Credito_ProbabilidadDefault_Obtener(
                int codEmpresa)
        {
            return _db
                .CR_frmCR_Categorias_Credito_ProbabilidadDefault_Obtener(
                    codEmpresa);
        }

        public ErrorDto<List<
            CrCategoriasCreditoProbabilidadMoraData>>
            CR_frmCR_Categorias_Credito_ProbabilidadMora_Obtener(
                int codEmpresa)
        {
            return _db
                .CR_frmCR_Categorias_Credito_ProbabilidadMora_Obtener(
                    codEmpresa);
        }

        public ErrorDto<List<
            CrCategoriasCreditoSegmentoData>>
            CR_frmCR_Categorias_Credito_Segmentos_Obtener(
                int codEmpresa)
        {
            return _db
                .CR_frmCR_Categorias_Credito_Segmentos_Obtener(
                    codEmpresa);
        }
    }
}