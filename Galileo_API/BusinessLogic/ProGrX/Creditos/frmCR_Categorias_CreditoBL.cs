
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

        public ErrorDto<List<
            CrCategoriasCreditoCargaProbabilidadData>>
            CR_frmCR_Categorias_Credito_CargaProbabilidades_Obtener(
                int codEmpresa)
        {
            return _db
                .CR_frmCR_Categorias_Credito_CargaProbabilidades_Obtener(
                    codEmpresa);
        }

        public ErrorDto
            CR_frmCR_Categorias_Credito_CargaProbabilidades_Procesar(
                int codEmpresa,
                string usuario,
                CrCategoriasCreditoCargaProbabilidadRequest request)
        {
            return _db
                .CR_frmCR_Categorias_Credito_CargaProbabilidades_Procesar(
                    codEmpresa,
                    usuario,
                    request);
        }

        public ErrorDto CR_frmCR_Categorias_Credito_Guardar(
            int codEmpresa,
            string usuario,
            string tipo,
            CrCategoriasCreditoCrudRequest request)
        {
            return _db.CR_frmCR_Categorias_Credito_Guardar(
                codEmpresa,
                usuario,
                tipo,
                request);
        }

        public ErrorDto CR_frmCR_Categorias_Credito_Eliminar(
            int codEmpresa,
            string usuario,
            string tipo,
            int id)
        {
            return _db.CR_frmCR_Categorias_Credito_Eliminar(
                codEmpresa,
                usuario,
                tipo,
                id);
        }
    }
}
