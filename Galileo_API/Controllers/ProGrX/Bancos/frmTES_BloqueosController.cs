using Galileo_API.BusinessLogic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Bancos;
using Galileo.Models;

namespace Galileo_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FrmTesBloqueosController : ControllerBase
    {
        private readonly  FrmTesBloqueosBL BloqueosBL;
        public FrmTesBloqueosController(IConfiguration config)
        {
            BloqueosBL = new FrmTesBloqueosBL(config);
        }

        
        [HttpGet("TES_Bloqueos_Solicitud_Obtener")]
        public ErrorDto<TesBloqueoTransaccionDto> TES_Bloqueos_Solicitud_Obtener(int CodEmpresa, int Contabilidad, int Solicitud)
        {
            return BloqueosBL.TES_Bloqueos_Solicitud_Obtener(CodEmpresa, Contabilidad, Solicitud);
        }

        [HttpGet("TES_Bloqueos_SolicitudesBloquedas_Obtener")]
        public ErrorDto<TablasListaGenericaModel> TES_Bloqueos_SolicitudesBloquedas_Obtener(int CodEmpresa, string filtros)
        {
            return BloqueosBL.TES_Bloqueos_SolicitudesBloquedas_Obtener(CodEmpresa, filtros);
        }


        [HttpPost("TES_Bloqueos_Solicitud_Bloquear")]
        public ErrorDto TES_Bloqueos_Solicitud_Bloquear(int CodEmpresa, int Solicitud, string Razon, string Usuario)
        {
            return BloqueosBL.TES_Bloqueos_Solicitud_Bloquear(CodEmpresa, Solicitud, Razon, Usuario);
        }

        [HttpPost("TES_Bloqueos_Solicitud_Desbloquear")]
        public ErrorDto TES_Bloqueos_Solicitud_Desbloquear(int CodEmpresa, int Solicitud, string Usuario)
        {
            return BloqueosBL.TES_Bloqueos_Solicitud_Desbloquear(CodEmpresa, Solicitud, Usuario);
        }
    }
}