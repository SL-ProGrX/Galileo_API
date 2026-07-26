using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.Models.ERROR;
using Galileo.Models.FSL;
using Galileo_API.BusinessLogic.ProGrX_BeneficiosFosol;

namespace Galileo_API.Controllers.ProGrX_BeneficiosFosol
{
    /// <summary>
    /// Endpoints de los Comités Fosol (frmFSL_Comite).
    /// </summary>
    [Route("api/frmFSL_Comite")]
    [ApiController]
    public class FrmFslComiteController : ControllerBase
    {
        private readonly FrmFslComiteBL _bl;

        public FrmFslComiteController(IConfiguration config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            _bl = new FrmFslComiteBL(config);
        }

        /// <summary>Lista de comités.</summary>
        [Authorize]
        [HttpGet("FslComites_Obtener")]
        public ErrorDto<FslComitesDataLista> FslComites_Obtener(int CodCliente, string filtros)
            => _bl.FslComites_Obtener(CodCliente, filtros);

        /// <summary>Comités activos.</summary>
        [Authorize]
        [HttpGet("FslComitesActivos_Obtener")]
        public ErrorDto<List<FslComitesActivosData>> FslComitesActivos_Obtener(int CodCliente)
            => _bl.FslComitesActivos_Obtener(CodCliente);

        /// <summary>Miembros de un comité.</summary>
        [Authorize]
        [HttpGet("FslMiembrosComite_Obtener")]
        public ErrorDto<FslMiembrosComitesDataLista> FslMiembrosComite_Obtener(int CodCliente, string filtros)
            => _bl.FslMiembrosComite_Obtener(CodCliente, filtros);

        /// <summary>Guarda un comité (inserta o actualiza).</summary>
        [Authorize]
        [HttpPost("Comite_Guardar")]
        public ErrorDto Comite_Guardar(int CodCliente, [FromBody] FslComitesDto comite)
            => _bl.Comite_Guardar(CodCliente, comite);

        /// <summary>Guarda un miembro de comité (inserta o actualiza).</summary>
        [Authorize]
        [HttpPost("ComiteMiembro_Guardar")]
        public ErrorDto ComiteMiembro_Guardar(int CodCliente, [FromBody] FslMiembrosComitesDto miembro)
            => _bl.ComiteMiembro_Guardar(CodCliente, miembro);

        /// <summary>Elimina un comité.</summary>
        [Authorize]
        [HttpDelete("FslComites_Eliminar")]
        public ErrorDto FslComites_Eliminar(int CodCliente, string comite)
            => _bl.FslComites_Eliminar(CodCliente, comite);

        /// <summary>Elimina un miembro de un comité.</summary>
        [Authorize]
        [HttpDelete("FslMiembrosComite_Eliminar")]
        public ErrorDto FslMiembrosComite_Eliminar(int CodCliente, string cedula, string comite)
            => _bl.FslMiembrosComite_Eliminar(CodCliente, cedula, comite);
    }
}
