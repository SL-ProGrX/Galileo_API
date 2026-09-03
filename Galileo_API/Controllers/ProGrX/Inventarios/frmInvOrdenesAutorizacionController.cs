using Galileo.BusinessLogic;
using Galileo.Models.ERROR;
using Galileo.Models.INV;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public sealed class FrmInvOrdenesAutorizacionController
        : ControllerBase
    {
        private readonly FrmInvOrdenesAutorizacionBl _bl;

        public FrmInvOrdenesAutorizacionController(
            IConfiguration config)
        {
            ArgumentNullException.ThrowIfNull(config);
            _bl = new FrmInvOrdenesAutorizacionBl(config);
        }

        [HttpGet(
            "INV_OrdenesAutorizacion_Ordenes_Obtener")]
        public ErrorDto<List<ResolucionTransaccionDto>>
            INV_OrdenesAutorizacion_Ordenes_Obtener(
                int CodEmpresa,
                string filtros)
        {
            return _bl
                .INV_OrdenesAutorizacion_Ordenes_Obtener(
                    CodEmpresa,
                    filtros);
        }

        [HttpPost(
            "INV_OrdenesAutorizacion_Ordenes_Autorizar")]
        public ErrorDto
            INV_OrdenesAutorizacion_Ordenes_Autorizar(
                int CodEmpresa,
                InvOrdenesAutorizacionProcesarRequest request)
        {
            return _bl
                .INV_OrdenesAutorizacion_Ordenes_Autorizar(
                    CodEmpresa,
                    request);
        }

        [HttpPost(
            "INV_OrdenesAutorizacion_Ordenes_Rechazar")]
        public ErrorDto
            INV_OrdenesAutorizacion_Ordenes_Rechazar(
                int CodEmpresa,
                InvOrdenesAutorizacionProcesarRequest request)
        {
            return _bl
                .INV_OrdenesAutorizacion_Ordenes_Rechazar(
                    CodEmpresa,
                    request);
        }
    }
}