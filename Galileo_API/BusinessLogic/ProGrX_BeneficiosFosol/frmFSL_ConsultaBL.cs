using Galileo.DataBaseTier.ProGrX_BeneficiosFosol;
using Galileo.Models.ERROR;
using Galileo.Models.FSL;

namespace Galileo_API.BusinessLogic.ProGrX_BeneficiosFosol
{
    /// <summary>
    /// Lógica de negocio de la Consulta de Expedientes Fosol (frmFSL_Consulta).
    /// </summary>
    public class FrmFslConsultaBL
    {
        private readonly FrmFslConsultaDB _db;

        public FrmFslConsultaBL(IConfiguration config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            _db = new FrmFslConsultaDB(config);
        }

        /// <summary>Planes Fosol activos.</summary>
        public ErrorDto<List<FslConsultaListas>> FslConsultaPlanes_Obtener(int CodCliente)
            => _db.FslConsultaPlanes_Obtener(CodCliente);

        /// <summary>Causas activas de un plan.</summary>
        public ErrorDto<List<FslConsultaListas>> FslConsultaCausas_Obtener(int CodCliente, string cod_plan)
            => _db.FslConsultaCausas_Obtener(CodCliente, cod_plan);

        /// <summary>Enfermedades activas.</summary>
        public ErrorDto<List<FslConsultaListas>> FslConsultaEnfermedades_Obtener(int CodCliente)
            => _db.FslConsultaEnfermedades_Obtener(CodCliente);

        /// <summary>Comités activos.</summary>
        public ErrorDto<List<FslConsultaListas>> FslConsutaComites_Obtener(int CodCliente)
            => _db.FslConsutaComites_Obtener(CodCliente);

        /// <summary>Estados de persona activos.</summary>
        public ErrorDto<List<FslConsultaListas>> FslConsutaEstadoPersonas_Obtener(int CodCliente)
            => _db.FslConsutaEstadoPersonas_Obtener(CodCliente);

        /// <summary>Tipos de gestión activos.</summary>
        public ErrorDto<List<FslConsultaListas>> FslConsutaTiposGestion_Obtener(int CodCliente)
            => _db.FslConsutaTiposGestion_Obtener(CodCliente);

        /// <summary>Tipos de apelación activos.</summary>
        public ErrorDto<List<FslConsultaListas>> FslConsutaTiposApelaciones_Obtener(int CodCliente)
            => _db.FslConsutaTiposApelaciones_Obtener(CodCliente);

        /// <summary>Miembros activos de un comité.</summary>
        public ErrorDto<List<FslConsultaListas>> FslConsultaComiteMiembros_Obtener(int CodCliente, string cod_comite)
            => _db.FslConsultaComiteMiembros_Obtener(CodCliente, cod_comite);

        /// <summary>Consulta de expedientes con filtros.</summary>
        public ErrorDto<List<FslConsultaExpedienteDatos>> FslConsultaExpedientes_Obtener(int CodCliente, string filtros)
            => _db.FslConsultaExpedientes_Obtener(CodCliente, filtros);
    }
}
