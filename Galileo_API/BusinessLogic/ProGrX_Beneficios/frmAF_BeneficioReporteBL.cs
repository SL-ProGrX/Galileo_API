using Galileo.DataBaseTier.ProGrX_Beneficios;
using Galileo.Models.AF;
using Galileo.Models.ERROR;

namespace Galileo_API.BusinessLogic.ProGrX_Beneficios
{
    /// <summary>
    /// Lógica de negocio de Reportes de Beneficios (frmAF_BeneficioReporte).
    /// </summary>
    public class FrmAfBeneficioReporteBL
    {
        private readonly FrmAfBeneficioReporteDB _db;

        public FrmAfBeneficioReporteBL(IConfiguration config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            _db = new FrmAfBeneficioReporteDB(config);
        }

        /// <summary>Lista de beneficios para reportes.</summary>
        public ErrorDto<List<AfiBeneficiosData>> BeneficioLista_Obtener(int CodCliente)
            => _db.BeneficioLista_Obtener(CodCliente);
    }
}
