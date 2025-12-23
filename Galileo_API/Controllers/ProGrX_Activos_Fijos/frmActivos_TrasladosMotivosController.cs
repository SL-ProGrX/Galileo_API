using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic.ProGrX_Activos_Fijos;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Activos_Fijos;

namespace Galileo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class FrmActivosTrasladosMotivosController : ControllerBase
    {
        private readonly FrmActivosTrasladosMotivosBL _bl;
        public FrmActivosTrasladosMotivosController(IConfiguration config)
        {
            _bl = new FrmActivosTrasladosMotivosBL(config);
        }

        [Authorize]
        [HttpGet("Activos_TrasladosMotivos_Consultar")]
        public ErrorDto<ActivosTrasladosMotivosDataLista> Activos_TrasladosMotivos_Consultar(int CodEmpresa, string filtros)
        {        
            return _bl.Activos_TrasladosMotivos_Consultar(CodEmpresa, filtros);
        }

        [Authorize]
        [HttpGet("Activos_TrasladosMotivos_Obtener")]
        public ErrorDto<List<ActivosTrasladosMotivosData>> Activos_TrasladosMotivos_Obtener(int CodEmpresa, string filtros)
        {            
            return _bl.Activos_TrasladosMotivos_Obtener(CodEmpresa, filtros);
        }

        [Authorize]
        [HttpPost("Activos_TrasladosMotivos_Guardar")]
        public ErrorDto Activos_TrasladosMotivos_Guardar(int CodEmpresa, string usuario, ActivosTrasladosMotivosData datos)
        {
            return _bl.Activos_TrasladosMotivos_Guardar(CodEmpresa, usuario, datos);
        }

        [Authorize]
        [HttpDelete("Activos_TrasladosMotivos_Eliminar")]
        public ErrorDto Activos_TrasladosMotivos_Eliminar(int CodEmpresa, string usuario, string cod_motivo)
        {
            return _bl.Activos_TrasladosMotivos_Eliminar(CodEmpresa, usuario, cod_motivo);
        }
    }
}