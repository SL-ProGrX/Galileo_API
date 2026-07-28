using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.Models.ERROR;
using Galileo.Models.FSL;
using Galileo_API.BusinessLogic.ProGrX_BeneficiosFosol;

namespace Galileo_API.Controllers.ProGrX_BeneficiosFosol
{
    /// <summary>
    /// Endpoints de la Tabla de Coberturas Fosol (frmFSL_TablaCoberturas).
    /// </summary>
    [Route("api/frmFSL_TablaCoberturas")]
    [ApiController]
    public class FrmFslTablaCoberturasController : ControllerBase
    {
        private readonly FrmFslTablaCoberturasBL _bl;

        public FrmFslTablaCoberturasController(IConfiguration config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            _bl = new FrmFslTablaCoberturasBL(config);
        }

        /// <summary>Tabla de aplicación (coberturas) por tipo.</summary>
        [Authorize]
        [HttpGet("TablaAplicacion_Obtener")]
        public ErrorDto<FslTablaAplicacionDataLista> TablaAplicacion_Obtener(int CodCliente, string filtros)
            => _bl.TablaAplicacion_Obtener(CodCliente, filtros);

        /// <summary>Guarda una cobertura (inserta o actualiza).</summary>
        [Authorize]
        [HttpPost("Cobertura_Guardar")]
        public ErrorDto Cobertura_Guardar(int CodCliente, [FromBody] FslTablaAplicacionData aplicacion)
            => _bl.Cobertura_Guardar(CodCliente, aplicacion);

        /// <summary>Elimina una cobertura.</summary>
        [Authorize]
        [HttpDelete("TablaAplicacion_Eliminar")]
        public ErrorDto TablaAplicacion_Eliminar(int CodCliente, string tipo, int linea)
            => _bl.TablaAplicacion_Eliminar(CodCliente, tipo, linea);
    }
}
