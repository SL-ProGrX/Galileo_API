using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.BusinessLogic.ProGrX_Nucleo;
using Galileo.Models.ProGrX_Nucleo;

namespace Galileo.Controllers.ProGrX_Nucleo
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmSysGestionesBitacoraController : ControllerBase
    {
        private readonly FrmSysGestionesBitacoraBL _bl;
        public FrmSysGestionesBitacoraController(IConfiguration config)
        {
            _bl = new FrmSysGestionesBitacoraBL(config);
        }

        [Authorize]
        [HttpGet("Sys_Gestiones_Bitacoras_Lista_Obtener")]
        public ErrorDto<SysGestionesBitacorasLista> Sys_Gestiones_Bitacoras_Lista_Obtener(int CodEmpresa,SysGestionesBitacoraFiltro filtros)
        {
            return _bl.Sys_Gestiones_Bitacoras_Lista_Obtener(
                CodEmpresa, filtros);
        }

        [Authorize]
        [HttpGet("Sys_Gestiones_Bitacoras_Obtener")]
        public ErrorDto<List<SysGestionesBitacorasData>> Sys_Gestiones_Bitacoras_Obtener(int CodEmpresa,SysGestionesBitacoraFiltro filtros)
        {
            return _bl.Sys_Gestiones_Bitacoras_Obtener(
                CodEmpresa, filtros);
        }

        [Authorize]
        [HttpGet("Sys_Gestiones_Tipos_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> Sys_Gestiones_Tipos_Obtener(int CodEmpresa)
        {
            return _bl.Sys_Gestiones_Tipos_Obtener(CodEmpresa);
        }

        [Authorize]
        [HttpGet("Sys_Socios_Buscar_Lista_Obtener")]
        public ErrorDto<SociosLookupLista> Sys_Socios_Buscar_Lista_Obtener(int CodEmpresa, string filtros = "")
        {
            return _bl.Sys_Socios_Buscar_Lista_Obtener(CodEmpresa, filtros);
        }
    }
}