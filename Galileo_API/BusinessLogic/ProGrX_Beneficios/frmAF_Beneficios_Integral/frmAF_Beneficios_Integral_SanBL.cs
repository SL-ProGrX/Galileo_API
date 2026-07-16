using Galileo.DataBaseTier.ProGrX_Beneficios;
using Galileo.Models.AF;
using Galileo.Models.ERROR;

namespace Galileo.BusinessLogic.ProGrX_Beneficios
{
    /// <summary>
    /// Lógica de negocio del proceso Sanciones de Beneficios Integrales (frmAF_Beneficios_Integral_San).
    /// </summary>
    public class frmAF_Beneficios_Integral_SanBL
    {
        private readonly frmAF_Beneficios_Integral_SanDB _db;

        public frmAF_Beneficios_Integral_SanBL(IConfiguration config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            _db = new frmAF_Beneficios_Integral_SanDB(config);
        }

        /// <summary>Lista de tipos de sanción activos.</summary>
        public List<BeneficiosSancionesLista> BeneSancionMotivoLista_Obtener(int CodCliente)
            => _db.BeneSancionMotivoLista_Obtener(CodCliente);

        /// <summary>Sanciones registradas del socio.</summary>
        public ErrorDto<List<AfiBeneSancionesDto>> BeneSacionesSocio_Obtener(int CodCliente, string cedula)
            => _db.BeneSacionesSocio_Obtener(CodCliente, cedula);

        /// <summary>Guarda (inserta o actualiza) la sanción del socio.</summary>
        public ErrorDto BeneSancionesSocio_Guardar(int CodCliente, AfiBeneSancionesDto sancion)
            => _db.BeneSancionesSocio_Guardar(CodCliente, sancion);
    }
}
