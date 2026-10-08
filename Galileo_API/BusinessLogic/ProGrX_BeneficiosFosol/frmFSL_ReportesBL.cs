using Galileo.DataBaseTier.ProGrX_BeneficiosFosol;
using Galileo.Models;
using Galileo.Models.ERROR;

namespace Galileo_API.BusinessLogic.ProGrX_BeneficiosFosol
{
    public sealed class FrmFslReportesBL
    {
        private readonly FrmFslReportesDB _db;

        public FrmFslReportesBL(
            IConfiguration config)
        {
            ArgumentNullException.ThrowIfNull(config);

            _db = new FrmFslReportesDB(config);
        }

        public ErrorDto<
            List<DropDownListaGenericaModel<string>>>
            FSL_Reportes_Oficinas_Obtener(
                int CodEmpresa)
        {
            return _db
                .FSL_Reportes_Oficinas_Obtener(
                    CodEmpresa);
        }

        public ErrorDto<
            List<DropDownListaGenericaModel<string>>>
            FSL_Reportes_Planes_Obtener(
                int CodEmpresa)
        {
            return _db
                .FSL_Reportes_Planes_Obtener(
                    CodEmpresa);
        }
    }
}