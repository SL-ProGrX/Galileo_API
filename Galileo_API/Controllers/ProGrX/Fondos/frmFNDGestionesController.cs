using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic.ProGrX.Fondos;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Fondos;

namespace Galileo.Controllers.ProGrX.Fondos
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmFndGestionesController : ControllerBase
    {
        private readonly FrmFndGestionesBl _bl;

        public FrmFndGestionesController(IConfiguration config)
        {
            _bl = new FrmFndGestionesBl(config);
        }

        [Authorize]
        [HttpPost("Gestiones_BuscarContratos")]
        public ErrorDto<List<FndGestionesBuscarContratosResult>> Gestiones_BuscarContratos([FromBody] FndGestionesBuscarContratosParams param)
        {
            return _bl.Gestiones_BuscarContratos(param);
        }

        [Authorize]
        [HttpPost("Gestiones_Contratos_Busqueda_Obtener")]
        public ErrorDto<List<FndGestionesContratosBusquedaResult>> Gestiones_Contratos_Busqueda_Obtener(
            [FromBody] FndGestionesContratosBusquedaParams param)
        {
            return _bl.Gestiones_Contratos_Busqueda_Obtener(param);
        }

        [Authorize]
        [HttpPost("Gestiones_Contrato_Obtener")]
        public ErrorDto<FndGestionesContratoResult> Gestiones_Contrato_Obtener([FromBody] FndGestionesContratoParams param)
        {
            return _bl.Gestiones_Contrato_Obtener(param);
        }

        [Authorize]
        [HttpPost("Gestiones_ContratosRenovacion")]
        public ErrorDto<List<FndGestionesContratosRenovacionResult>> Gestiones_ContratosRenovacion([FromBody] FndGestionesContratosRenovacionParams param)
        {
            return _bl.Gestiones_ContratosRenovacion(param);
        }

        [Authorize]
        [HttpPost("Gestiones_Contrato_Actualizar")]
        public ErrorDto<FndGestionesContratoActualizarResult> Gestiones_Contrato_Actualizar([FromBody] FndGestionesContratoActualizarParams param)
        {
            return _bl.Gestiones_Contrato_Actualizar(param);
        }
    }
}