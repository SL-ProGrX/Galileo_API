using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.Models.ERROR;
using Galileo.Models.FSL;
using Galileo_API.BusinessLogic.ProGrX_BeneficiosFosol;

namespace Galileo_API.Controllers.ProGrX_BeneficiosFosol
{
    /// <summary>
    /// Endpoints de los Tipos de Aplicación Fosol (frmFSL_TipoAplicacion): planes y causas.
    /// </summary>
    [Route("api/frmFSL_TipoAplicacion")]
    [ApiController]
    public class FrmFslTipoAplicacionController : ControllerBase
    {
        private readonly FrmFslTipoAplicacionBL _bl;

        public FrmFslTipoAplicacionController(IConfiguration config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            _bl = new FrmFslTipoAplicacionBL(config);
        }

        /// <summary>Causas de un plan.</summary>
        [Authorize]
        [HttpGet("Causas_Obtener")]
        public ErrorDto<CausasDataLista> Causas_Obtener(int CodCliente, string TipoCausa, string Jfiltro)
            => _bl.Causas_Obtener(CodCliente, TipoCausa, Jfiltro);

        /// <summary>Exporta la lista completa de causas de un plan.</summary>
        [Authorize]
        [HttpGet("CausasListas_Exportar")]
        public ErrorDto<List<TiposCausaData>> CausasListas_Exportar(int CodCliente, string TipoCausa)
            => _bl.CausasListas_Exportar(CodCliente, TipoCausa);

        /// <summary>Planes Fosol.</summary>
        [Authorize]
        [HttpGet("Planes_Obtener")]
        public ErrorDto<PlanesDataLista> Planes_Obtener(int CodCliente, string Jfiltro)
            => _bl.Planes_Obtener(CodCliente, Jfiltro);

        /// <summary>Exporta la lista completa de planes.</summary>
        [Authorize]
        [HttpGet("PlanesLista_Exportar")]
        public ErrorDto<List<ListaPlanesData>> PlanesLista_Exportar(int CodCliente)
            => _bl.PlanesLista_Exportar(CodCliente);

        /// <summary>Lista simple de planes activos.</summary>
        [Authorize]
        [HttpGet("ListaPlanes_Obtener")]
        public ErrorDto<List<ListaPlanesData>> ListaPlanes_Obtener(int CodCliente)
            => _bl.ListaPlanes_Obtener(CodCliente);

        /// <summary>Inserta un plan (o actualiza si existe).</summary>
        [Authorize]
        [HttpPost("Planes_Insertar")]
        public ErrorDto Planes_Insertar(int CodCliente, [FromBody] PlanDataInsert planData)
            => _bl.Planes_Insertar(CodCliente, planData);

        /// <summary>Inserta una causa (o actualiza si existe).</summary>
        [Authorize]
        [HttpPost("Causas_Insertar")]
        public ErrorDto Causas_Insertar(int CodCliente, [FromBody] CausaDataInsert causaData)
            => _bl.Causas_Insertar(CodCliente, causaData);

        /// <summary>Actualiza una causa.</summary>
        [Authorize]
        [HttpPut("Causas_Actualizar")]
        public ErrorDto Causas_Actualizar(int CodCliente, [FromBody] CausaDataInsert causaData)
            => _bl.Causas_Actualizar(CodCliente, causaData);

        /// <summary>Elimina un plan.</summary>
        [Authorize]
        [HttpDelete("Planes_Eliminar")]
        public ErrorDto Planes_Eliminar(int CodCliente, string cod_plan)
            => _bl.Planes_Eliminar(CodCliente, cod_plan);

        /// <summary>Elimina una causa.</summary>
        [Authorize]
        [HttpDelete("Causas_Eliminar")]
        public ErrorDto Causas_Eliminar(int CodCliente, string cod_causa, string cod_plan)
            => _bl.Causas_Eliminar(CodCliente, cod_causa, cod_plan);
    }
}
