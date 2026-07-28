using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.Models.ERROR;
using Galileo.Models.FSL;
using Galileo_API.BusinessLogic.ProGrX_BeneficiosFosol;

namespace Galileo_API.Controllers.ProGrX_BeneficiosFosol
{
    /// <summary>
    /// Endpoints de la Consulta de Expedientes Fosol (frmFSL_Consulta).
    /// </summary>
    [Route("api/frmFSL_Consulta")]
    [ApiController]
    public class FrmFslConsultaController : ControllerBase
    {
        private readonly FrmFslConsultaBL _bl;

        public FrmFslConsultaController(IConfiguration config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            _bl = new FrmFslConsultaBL(config);
        }

        /// <summary>Planes Fosol activos.</summary>
        [Authorize]
        [HttpGet("FslConsultaPlanes_Obtener")]
        public ErrorDto<List<FslConsultaListas>> FslConsultaPlanes_Obtener(int CodCliente)
            => _bl.FslConsultaPlanes_Obtener(CodCliente);

        /// <summary>Causas activas de un plan.</summary>
        [Authorize]
        [HttpGet("FslConsultaCausas_Obtener")]
        public ErrorDto<List<FslConsultaListas>> FslConsultaCausas_Obtener(int CodCliente, string cod_plan)
            => _bl.FslConsultaCausas_Obtener(CodCliente, cod_plan);

        /// <summary>Enfermedades activas.</summary>
        [Authorize]
        [HttpGet("FslConsultaEnfermedades_Obtener")]
        public ErrorDto<List<FslConsultaListas>> FslConsultaEnfermedades_Obtener(int CodCliente)
            => _bl.FslConsultaEnfermedades_Obtener(CodCliente);

        /// <summary>Comités activos.</summary>
        [Authorize]
        [HttpGet("FslConsutaComites_Obtener")]
        public ErrorDto<List<FslConsultaListas>> FslConsutaComites_Obtener(int CodCliente)
            => _bl.FslConsutaComites_Obtener(CodCliente);

        /// <summary>Estados de persona activos.</summary>
        [Authorize]
        [HttpGet("FslConsutaEstadoPersonas_Obtener")]
        public ErrorDto<List<FslConsultaListas>> FslConsutaEstadoPersonas_Obtener(int CodCliente)
            => _bl.FslConsutaEstadoPersonas_Obtener(CodCliente);

        /// <summary>Tipos de gestión activos.</summary>
        [Authorize]
        [HttpGet("FslConsutaTiposGestion_Obtener")]
        public ErrorDto<List<FslConsultaListas>> FslConsutaTiposGestion_Obtener(int CodCliente)
            => _bl.FslConsutaTiposGestion_Obtener(CodCliente);

        /// <summary>Tipos de apelación activos.</summary>
        [Authorize]
        [HttpGet("FslConsutaTiposApelaciones_Obtener")]
        public ErrorDto<List<FslConsultaListas>> FslConsutaTiposApelaciones_Obtener(int CodCliente)
            => _bl.FslConsutaTiposApelaciones_Obtener(CodCliente);

        /// <summary>Miembros activos de un comité.</summary>
        [Authorize]
        [HttpGet("FslConsultaComiteMiembros_Obtener")]
        public ErrorDto<List<FslConsultaListas>> FslConsultaComiteMiembros_Obtener(int CodCliente, string cod_comite)
            => _bl.FslConsultaComiteMiembros_Obtener(CodCliente, cod_comite);

        /// <summary>Consulta de expedientes con filtros.</summary>
        [Authorize]
        [HttpGet("FslConsultaExpedientes_Obtener")]
        public ErrorDto<List<FslConsultaExpedienteDatos>> FslConsultaExpedientes_Obtener(int CodCliente, string filtros)
            => _bl.FslConsultaExpedientes_Obtener(CodCliente, filtros);
    }
}
