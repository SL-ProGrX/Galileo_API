using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic.ProGrX.Fondos;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Fondos;

namespace Galileo.Controllers.ProGrX.Fondos
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmFndAnulacionesController : ControllerBase
    {
        private readonly FrmFndAnulacionesBl BlFndAnulaciones;

        public FrmFndAnulacionesController(IConfiguration config)
        {
            BlFndAnulaciones = new FrmFndAnulacionesBl(config);
        }

        [Authorize]
        [HttpGet("FND_Anulaciones_Obtener")]
        public ErrorDto<FndAnulacionesDto> FND_Anulaciones_Obtener(int CodEmpresa, string Params)
        {
            return BlFndAnulaciones.FND_Anulaciones_Obtener(CodEmpresa, Params);
        }

        [Authorize]
        [HttpGet("FND_Anulaciones_SubCuentas_Obtener")]
        public ErrorDto<List<FndAnulacionesSubCuentasDto>> FND_Anulaciones_SubCuentas_Obtener(int CodEmpresa, string Params)
        {
            return BlFndAnulaciones.FND_Anulaciones_SubCuentas_Obtener(CodEmpresa, Params);
        }

        [Authorize]
        [HttpGet("FND_Anulaciones_Autoriza_Obtener")]
        public ErrorDto<FndAutorizaDto> FND_Anulaciones_Autoriza_Obtener(int CodEmpresa, string Plan, string Usuario)
        {
            return BlFndAnulaciones.FND_Anulaciones_Autoriza_Obtener(CodEmpresa, Plan, Usuario);
        }

        [Authorize]
        [HttpGet("FND_Anulaciones_SolicitaAutorizacion_Obtener")]
        public ErrorDto<FndAnulacionesEstadoGestionDto> FND_Anulaciones_SolicitaAutorizacion_Obtener(int CodEmpresa, string Params)
        {
            return BlFndAnulaciones.FND_Anulaciones_SolicitaAutorizacion_Obtener(CodEmpresa, Params);
        }

        [Authorize]
        [HttpGet("FND_Anulaciones_AutorizacionRefresh_Obtener")]
        public ErrorDto<FndAnulacionesEstadoGestionDto> FND_Anulaciones_AutorizacionRefresh_Obtener(int CodEmpresa, int GestionId)
        {
            return BlFndAnulaciones.FND_Anulaciones_AutorizacionRefresh_Obtener(CodEmpresa, GestionId);
        }

        [Authorize]
        [HttpPost("FND_Anulaciones_Anular")]
        public ErrorDto<object> FND_Anulaciones_Anular(int CodEmpresa, string Params, string Accion, string Notas)
        {
            return BlFndAnulaciones.FND_Anulaciones_Anular(CodEmpresa, Params, Accion, Notas);
        }
    }
}