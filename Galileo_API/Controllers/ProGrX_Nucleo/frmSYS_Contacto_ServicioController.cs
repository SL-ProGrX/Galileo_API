using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic.ProGrX_Nucleo;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Nucleo;

namespace Galileo.Controllers.ProGrX_Nucleo
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmSysContactoServicioController : ControllerBase
    {
        private readonly FrmSysContactoServicioBL _bl;

        public FrmSysContactoServicioController(IConfiguration config)
        {
            _bl = new FrmSysContactoServicioBL(config);
        }
        // ======================== GENERAL ========================

        [Authorize]
        [HttpGet("SysContactoServicio_General_Obtener")]
        public ErrorDto<SysContactoServicioGeneralData?> SysContactoServicio_General_Obtener(int CodEmpresa, string identificacion, string codPais = "CRC")
        {
            return _bl.SysContactoServicio_General_Obtener(CodEmpresa, identificacion, codPais);
        }

        [Authorize]
        [HttpGet("SysContactoServicio_Obtener")]
        public ErrorDto<List<SysContactoServicioGeneralData>> SysContactoServicio_Obtener(int CodEmpresa, string identificacion, string codPais, string filtros)
        {
            return _bl.SysContactoServicio_Obtener(CodEmpresa, identificacion, codPais, filtros);
        }

        // ======================== TELÉFONOS ========================

        [Authorize]
        [HttpGet("SysContactoServicio_Telefonos_Lista_Obtener")]
        public ErrorDto<SysContactoServicioTelefonoLista> SysContactoServicio_Telefonos_Lista_Obtener(int CodEmpresa, string identificacion, string codPais, string filtros)
        {
            return _bl.SysContactoServicio_Telefonos_Lista_Obtener(CodEmpresa, identificacion, codPais, filtros);
        }

        [Authorize]
        [HttpGet("SysContactoServicio_Telefonos_Obtener")]
        public ErrorDto<List<SysContactoServicioTelefonoData>> SysContactoServicio_Telefonos_Obtener(int CodEmpresa, string identificacion, string codPais)
        {
            return _bl.SysContactoServicio_Telefonos_Obtener(CodEmpresa, identificacion, codPais);
        }

        // ======================== DIRECCIONES ========================

        [Authorize]
        [HttpGet("SysContactoServicio_Direcciones_Lista_Obtener")]
        public ErrorDto<SysContactoServicioDireccionLista> SysContactoServicio_Direcciones_Lista_Obtener(int CodEmpresa, string identificacion, string codPais, string filtros)
        {
            return _bl.SysContactoServicio_Direcciones_Lista_Obtener(CodEmpresa, identificacion, codPais, filtros);
        }

        [Authorize]
        [HttpGet("SysContactoServicio_Direcciones_Obtener")]
        public ErrorDto<List<SysContactoServicioDireccionData>> SysContactoServicio_Direcciones_Obtener(int CodEmpresa, string identificacion, string codPais)
        {
            return _bl.SysContactoServicio_Direcciones_Obtener(CodEmpresa, identificacion, codPais);
        }

        // ======================== EMPRESAS ========================

        [Authorize]
        [HttpGet("SysContactoServicio_Empresas_Lista_Obtener")]
        public ErrorDto<SysContactoServicioEmpresaLista> SysContactoServicio_Empresas_Lista_Obtener(int CodEmpresa, string identificacion, string codPais, string filtros)
        {
            return _bl.SysContactoServicio_Empresas_Lista_Obtener(CodEmpresa, identificacion, codPais, filtros);
        }

        [Authorize]
        [HttpGet("SysContactoServicio_Empresas_Obtener")]
        public ErrorDto<List<SysContactoServicioEmpresaData>> SysContactoServicio_Empresas_Obtener(int CodEmpresa, string identificacion, string codPais)
        {
            return _bl.SysContactoServicio_Empresas_Obtener(CodEmpresa, identificacion, codPais);
        }

        [Authorize]
        [HttpGet("SysContactoServicio_Personas_Lista_Buscar")]
        public ErrorDto<SysContactoServicioPersonaLookupLista> SysContactoServicio_Personas_Lista_Buscar(
            int CodEmpresa, string codPais, string filtros)
        {
            return _bl.SysContactoServicio_Personas_Lista_Buscar(CodEmpresa, codPais, filtros);
        }
    }
}