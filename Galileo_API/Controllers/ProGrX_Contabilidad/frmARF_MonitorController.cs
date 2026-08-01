using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX_Contabilidad;
using Galileo_API.DataBaseTier.ProGrX_Contabilidad;
using Galileo_API.Models.ProGrX_Activos;
using Galileo_API.Models.ProGrX_Contabilidad;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX_Contabilidad
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmArfMonitorController : ControllerBase
    {
        private readonly FrmArfMonitorBl _bl;

        public FrmArfMonitorController(IConfiguration config)
        {
            _bl = new FrmArfMonitorBl(config);
        }

        [Authorize]
        [HttpPost("Buscar")]
        /// <summary>
        /// Busca las operaciones que cumplen los filtros del monitor.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa que se consultará.</param>
        /// <param name="filtros">Filtros seleccionados en el monitor.</param>
        /// <returns>Operaciones encontradas.</returns>
        public ErrorDto<List<ArfMonitorTablaDto>> Buscar(int codEmpresa,[FromBody] ArfMonitorFiltroDto filtros
        )
        {
            return _bl.Buscar(codEmpresa, filtros);
        }

        [Authorize]
        [HttpGet("Unidades_Buscar")]
        /// <summary>
        /// Obtiene las unidades disponibles.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa que se consultará.</param>
        /// <returns>Lista de unidades.</returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Unidades_Buscar(int codEmpresa
        )
        {
            return _bl.Unidades_Buscar(codEmpresa);
        }

        [Authorize]
        [HttpGet("Arrendadores_Buscar")]
        /// <summary>
        /// Obtiene los arrendadores disponibles.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa que se consultará.</param>
        /// <returns>Lista de arrendadores.</returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Arrendadores_Buscar(int codEmpresa
        )
        {
            return _bl.Arrendadores_Buscar(codEmpresa);
        }

        /// <summary>
        /// Obtiene los cierres disponibles para consultar el auxiliar histórico.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa que se consultará.</param>
        /// <returns>Lista de fechas de cierre.</returns>
        [Authorize]
        [HttpGet("Cierres_Buscar")]
        public ErrorDto<List<DropDownListaGenericaModel>> Cierres_Buscar(
            int codEmpresa
        )
        {
            return _bl.Cierres_Buscar(codEmpresa);
        }
    }
}
