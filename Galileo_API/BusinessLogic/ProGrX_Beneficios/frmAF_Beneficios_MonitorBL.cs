using Galileo.DataBaseTier.ProGrX_Beneficios;
using Galileo.Models.AF;
using Galileo.Models.ERROR;

namespace Galileo_API.BusinessLogic.ProGrX_Beneficios
{
    /// <summary>
    /// Lógica de negocio del Monitor de Beneficios (frmAF_Beneficios_Monitor).
    /// </summary>
    public class FrmAfBeneficiosMonitorBL
    {
        private readonly FrmAfBeneficiosMonitorDB _db;

        public FrmAfBeneficiosMonitorBL(IConfiguration config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            _db = new FrmAfBeneficiosMonitorDB(config);
        }

        /// <summary>Lista de beneficios del monitor con filtros.</summary>
        public ErrorDto<VBeneficiosIntegralDtoLista> BeneficiosMonitor_Obtener(int CodCliente, string filtroString)
            => _db.BeneficiosMonitor_Obtener(CodCliente, filtroString);

        /// <summary>Lista de instituciones.</summary>
        public ErrorDto<List<OpcionesLista>> InstitucionesLista_Obtener(int CodCliente)
            => _db.InstitucionesLista_Obtener(CodCliente);

        /// <summary>Lista de estados de persona.</summary>
        public ErrorDto<List<OpcionesLista>> EstadosLista_Obtener(int CodCliente)
            => _db.EstadosLista_Obtener(CodCliente);

        /// <summary>Lista de oficinas.</summary>
        public ErrorDto<List<OpcionesLista>> OficinasLista_Obtener(int CodCliente)
            => _db.OficinasLista_Obtener(CodCliente);

        /// <summary>Lista de beneficios activos.</summary>
        public ErrorDto<List<OpcionesLista>> BeneficiosLista_Obtener(int CodCliente)
            => _db.BeneficiosLista_Obtener(CodCliente);
    }
}
