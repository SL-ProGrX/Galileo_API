using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic.ProGrX_Nucleo;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Nucleo;

namespace Galileo.Controllers.ProGrX_Nucleo
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmSysCorreosBandejaController : ControllerBase
    {
        private readonly FrmSysCorreosBandejaBL _bl;
        public FrmSysCorreosBandejaController(IConfiguration config)
        {
            _bl = new FrmSysCorreosBandejaBL(config);
        }

        [Authorize]
        [HttpGet("Sys_Correos_Bandeja_Lista_Obtener")]
        public ErrorDto<SysCorreosBandejaLista> sys_Correos_bandeja_lista_obtener(int CodEmpresa,string para_Buscar = "",string asunto_Buscar = "",string fecha_Inicio = "",string fecha_Fin = "",string filtros = "")
        {
            return _bl.Sys_Correos_Bandeja_Lista_Obtener( CodEmpresa,para_Buscar,asunto_Buscar,fecha_Inicio,fecha_Fin,filtros);
        }

        [Authorize]
        [HttpGet("Sys_Correos_Bandeja_Obtener")]
        public ErrorDto<List<SysCorreosBandejaData>> Correos_Bandeja_Obtener(int CodEmpresa, string Para_Buscar = "", string Asunto_Buscar = "",string Fecha_Inicio = "", string Fecha_Fin = "", string Filtro_Global = "")
        {
            return _bl.Correos_Bandeja_Obtener(CodEmpresa,Para_Buscar ?? string.Empty,Asunto_Buscar ?? string.Empty,Fecha_Inicio ?? string.Empty,Fecha_Fin ?? string.Empty,Filtro_Global ?? string.Empty);
        }

        [Authorize]
        [HttpGet("Sys_Correos_Bandeja_Resumen_Lista_Obtener")]
        public ErrorDto<SysCorreosBandejaResumenLista> Sys_Correos_Bandeja_Resumen_Lista_Obtener(int CodEmpresa,string Para_Buscar = "",string Asunto_Buscar = "",string Fecha_Inicio = "",string Fecha_Fin = "",string filtros = "")
        {
            return _bl.Sys_Correos_Bandeja_Resumen_Lista_Obtener(CodEmpresa, Para_Buscar, Asunto_Buscar, Fecha_Inicio, Fecha_Fin, filtros);
        }

        [Authorize]
        [HttpGet("Correos_Bandeja_Resumen_Obtener")]
        public ErrorDto<List<SysCorreosBandejaResumenData>> Correos_Bandeja_Resumen_Obtener(int CodEmpresa,string Para_Buscar = "",string Asunto_Buscar = "",string Fecha_Inicio = "",string Fecha_Fin = "",string Filtro_Global = "")
        {
            return _bl.Correos_Bandeja_Resumen_Obtener(CodEmpresa, Para_Buscar, Asunto_Buscar, Fecha_Inicio, Fecha_Fin, Filtro_Global);
        }
    }
}