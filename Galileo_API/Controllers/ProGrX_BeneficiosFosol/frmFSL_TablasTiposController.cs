using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.Models.ERROR;
using Galileo.Models.FSL;
using Galileo_API.BusinessLogic.ProGrX_BeneficiosFosol;

namespace Galileo_API.Controllers.ProGrX_BeneficiosFosol
{
    /// <summary>
    /// Endpoints de los catálogos de Tipos Fosol (frmFSL_TablasTipos).
    /// </summary>
    [Route("api/frmFSL_TablasTipos")]
    [ApiController]
    public class FrmFslTablasTiposController : ControllerBase
    {
        private readonly FrmFslTablasTiposBL _bl;

        public FrmFslTablasTiposController(IConfiguration config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            _bl = new FrmFslTablasTiposBL(config);
        }

        /// <summary>Lista de tipos (gestiones, apelaciones o enfermedades).</summary>
        [Authorize]
        [HttpGet("FslTablaTipos_Obtener")]
        public ErrorDto<FslTablaTipoLista> FslTablaTipos_Obtener(int CodCliente, string tipo, string? filtro, int? pagina, int? paginacion)
            => _bl.FslTablaTipos_Obtener(CodCliente, tipo, filtro, pagina, paginacion);

        /// <summary>Actualiza un tipo.</summary>
        [Authorize]
        [HttpPut("FslTablaTipos_Actualizar")]
        public ErrorDto FslTablaTipos_Actualizar(int CodCliente, string tipo, [FromBody] FslTablaTipoData tipoData)
            => _bl.FslTablaTipos_Actualizar(CodCliente, tipo, tipoData);

        /// <summary>Inserta un tipo (o actualiza si existe).</summary>
        [Authorize]
        [HttpPost("FslTablaTipo_Insertar")]
        public ErrorDto FslTablaTipo_Insertar(int CodCliente, string tipo, string usuario, [FromBody] FslTablaTipoData tipoData)
            => _bl.FslTablaTipo_Insertar(CodCliente, tipo, usuario, tipoData);

        /// <summary>Elimina un tipo.</summary>
        [Authorize]
        [HttpDelete("FslTablaTipo_Eliminar")]
        public ErrorDto FslTablaTipo_Eliminar(int CodCliente, string tipo, string codigo)
            => _bl.FslTablaTipo_Eliminar(CodCliente, tipo, codigo);
    }
}
