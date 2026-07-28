using Galileo.DataBaseTier.ProGrX_BeneficiosFosol;
using Galileo.Models.ERROR;
using Galileo.Models.FSL;

namespace Galileo_API.BusinessLogic.ProGrX_BeneficiosFosol
{
    /// <summary>
    /// Lógica de negocio de las Gestiones de Expediente Fosol (frmFSL_ExpedienteGestiones).
    /// </summary>
    public class FrmFslExpedienteGestionesBL
    {
        private readonly FrmFslExpedienteGestionesDB _db;

        public FrmFslExpedienteGestionesBL(IConfiguration config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            _db = new FrmFslExpedienteGestionesDB(config);
        }

        /// <summary>Tipos de gestión activos.</summary>
        public ErrorDto<List<FslGestionesListaDatos>> FslGestiones_Obtener(int CodCliente)
            => _db.FslGestiones_Obtener(CodCliente);

        /// <summary>Registra una gestión de expediente.</summary>
        public ErrorDto FslGestion_Agregar(int CodCliente, FslGestionAgregar gestion)
            => _db.FslGestion_Agregar(CodCliente, gestion);
    }
}
