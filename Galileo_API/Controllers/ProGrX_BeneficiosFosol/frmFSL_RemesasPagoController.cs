using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.Models.ERROR;
using Galileo.Models.FSL;
using Galileo_API.BusinessLogic.ProGrX_BeneficiosFosol;

namespace Galileo_API.Controllers.ProGrX_BeneficiosFosol
{
    /// <summary>
    /// Endpoints de Remesas de Pago Fosol (frmFSL_RemesasPago).
    /// </summary>
    [Route("api/frmFSL_RemesasPago")]
    [ApiController]
    public class FrmFslRemesasPagoController : ControllerBase
    {
        private readonly FrmFslRemesasPagoBL _bl;

        public FrmFslRemesasPagoController(IConfiguration config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            _bl = new FrmFslRemesasPagoBL(config);
        }

        /// <summary>Fechas de una remesa de tesorería.</summary>
        [Authorize]
        [HttpGet("FslFechas_Obtener")]
        public ErrorDto<List<FslRemesasListaDatos>> FslFechas_Obtener(int CodEmpresa, int cod_remesa)
            => _bl.FslFechas_Obtener(CodEmpresa, cod_remesa);

        /// <summary>Lista de remesas de tesorería.</summary>
        [Authorize]
        [HttpGet("FslRemesas_Obtener")]
        public ErrorDto<FslRemesasLista> FslRemesas_Obtener(int CodEmpresa, string? filtro, int? pagina, int? paginacion)
            => _bl.FslRemesas_Obtener(CodEmpresa, filtro, pagina, paginacion);

        /// <summary>Remesas abiertas para cargas.</summary>
        [Authorize]
        [HttpGet("FslCargas_Obtener")]
        public ErrorDto<List<FslRemesasListaDatos>> FslCargas_Obtener(int CodEmpresa)
            => _bl.FslCargas_Obtener(CodEmpresa);

        /// <summary>Expedientes elegibles para carga.</summary>
        [Authorize]
        [HttpGet("FslCargasLista_Obtener")]
        public ErrorDto<FslCargasLista> FslCargasLista_Obtener(int CodEmpresa, string fecha_inicio, string fecha_corte, string? filtro, int? pagina, int? paginacion)
            => _bl.FslCargasLista_Obtener(CodEmpresa, fecha_inicio, fecha_corte, filtro, pagina, paginacion);

        /// <summary>Remesas cerradas listas para trasladar.</summary>
        [Authorize]
        [HttpGet("FslTraslados_Obtener")]
        public ErrorDto<List<FslRemesasListaDatos>> FslTraslados_Obtener(int CodEmpresa)
            => _bl.FslTraslados_Obtener(CodEmpresa);

        /// <summary>Expedientes de una remesa pendientes de traslado.</summary>
        [Authorize]
        [HttpGet("FslTrasladoLista_Obtener")]
        public ErrorDto<List<FslTrasladoListaData>> FslTrasladoLista_Obtener(int CodEmpresa, string fecha_inicio, string fecha_corte, int cod_remesa)
            => _bl.FslTrasladoLista_Obtener(CodEmpresa, fecha_inicio, fecha_corte, cod_remesa);

        /// <summary>Inserta una remesa de tesorería.</summary>
        [Authorize]
        [HttpPost("FslRemesa_Agregar")]
        public ErrorDto FslRemesa_Agregar(int CodEmpresa, [FromBody] FslRemesaInsertar remesa)
            => _bl.FslRemesa_Agregar(CodEmpresa, remesa);

        /// <summary>Actualiza una remesa de tesorería.</summary>
        [Authorize]
        [HttpPut("FslRemesa_Actualizar")]
        public ErrorDto FslRemesa_Actualizar(int CodEmpresa, [FromBody] FslRemesaInsertar remesa)
            => _bl.FslRemesa_Actualizar(CodEmpresa, remesa);

        /// <summary>Cierra una remesa de tesorería.</summary>
        [Authorize]
        [HttpPost("FslRemesa_Cerrar")]
        public ErrorDto FslRemesa_Cerrar(int CodEmpresa, int cod_remesa, string usuario)
            => _bl.FslRemesa_Cerrar(CodEmpresa, cod_remesa, usuario);

        /// <summary>Aplica una remesa a los expedientes seleccionados.</summary>
        [Authorize]
        [HttpPost("FslCargas_Aplicar")]
        public ErrorDto FslCargas_Aplicar(int CodEmpresa, [FromBody] string cargas)
            => _bl.FslCargas_Aplicar(CodEmpresa, cargas);

        /// <summary>Cierra una remesa de cargas.</summary>
        [Authorize]
        [HttpPost("FslCargas_Cerrar")]
        public ErrorDto FslCargas_Cerrar(int CodEmpresa, int cod_remesa, string usuario)
            => _bl.FslCargas_Cerrar(CodEmpresa, cod_remesa, usuario);

        /// <summary>Aplica el traslado a tesorería de los expedientes de una remesa.</summary>
        [Authorize]
        [HttpPost("FslTraslado_Aplicar")]
        public ErrorDto FslTraslado_Aplicar(int CodEmpresa, [FromBody] string traslados)
            => _bl.FslTraslado_Aplicar(CodEmpresa, traslados);

        /// <summary>Elimina una remesa de tesorería.</summary>
        [Authorize]
        [HttpDelete("FslRemesa_Eliminar")]
        public ErrorDto FslRemesa_Eliminar(int CodEmpresa, int cod_remesa)
            => _bl.FslRemesa_Eliminar(CodEmpresa, cod_remesa);
    }
}
