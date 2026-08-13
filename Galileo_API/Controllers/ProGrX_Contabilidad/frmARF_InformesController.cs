using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX_ARF;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX_ARF
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmArfInformesController : ControllerBase
    {
        private readonly FrmArfInformesBl _bl;

        public FrmArfInformesController(IConfiguration config)
        {
            _bl = new FrmArfInformesBl(config);
        }

        /// <summary>
        /// Lista las oficinas o unidades disponibles para los informes.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa que se consultará.</param>
        /// <returns>Resultado con las unidades disponibles.</returns>
        [Authorize]
        [HttpGet]
        [Route("ARF_Unidades_Listar")]
        public ErrorDto<List<DropDownListaGenericaModel>> Unidades(int codEmpresa)
        {
            return _bl.ARF_Unidades_Listar(codEmpresa);
        }

        /// <summary>
        /// Lista los arrendadores disponibles para los informes.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa que se consultará.</param>
        /// <returns>Resultado con los arrendadores disponibles.</returns>
        [Authorize]
        [HttpGet]
        [Route("ARF_Arrendadores_Listar")]
        public ErrorDto<List<DropDownListaGenericaModel>> Arrendadores(int codEmpresa)
        {
            return _bl.ARF_Arrendadores_Listar(codEmpresa);
        }
    }
}
