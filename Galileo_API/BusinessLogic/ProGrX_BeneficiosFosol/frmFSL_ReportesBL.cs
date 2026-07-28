using Galileo.DataBaseTier.ProGrX_BeneficiosFosol;
using Galileo.Models.ERROR;
using Galileo.Models.FSL;

namespace Galileo_API.BusinessLogic.ProGrX_BeneficiosFosol
{
    /// <summary>
    /// Lógica de negocio de los Reportes de Beneficios Fosol (frmFSL_Reportes).
    /// </summary>
    public class FrmFslReportesBL
    {
        private readonly FrmFslReportesDB _db;

        public FrmFslReportesBL(IConfiguration config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            _db = new FrmFslReportesDB(config);
        }

        /// <summary>Catálogo de oficinas.</summary>
        public ErrorDto<List<Oficina>> FSL_Oficinas_Obtener(int CodEmpresa)
            => _db.FSL_Oficinas_Obtener(CodEmpresa);

        /// <summary>Catálogo de planes Fosol activos.</summary>
        public ErrorDto<List<Plan>> FSL_Planes_Obtener(int CodEmpresa)
            => _db.FSL_Planes_Obtener(CodEmpresa);
    }
}
