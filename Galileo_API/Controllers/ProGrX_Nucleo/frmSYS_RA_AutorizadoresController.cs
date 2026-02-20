using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Nucleo;

namespace Galileo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmSysRaAutorizadoresController : ControllerBase
    {
        private readonly FrmSysRaAutorizadoresBL _bl;
        public FrmSysRaAutorizadoresController(IConfiguration config)
        {
            _bl = new FrmSysRaAutorizadoresBL(config);
        }

        [HttpGet("Frm_Sys_Ra_Autorizadores_ConsultaAscDesc")]
        public ErrorDto<int> Frm_Sys_Ra_Autorizadores_ConsultaAscDesc(int CodEmpresa, int consecutivo, string tipo)
        {
            return _bl.Frm_Sys_Ra_Autorizadores_ConsultaAscDesc(CodEmpresa, consecutivo, tipo);
        }

        [HttpGet("Frm_Sys_Ra_Autorizadores_Obtener")]
        public ErrorDto<AutorizadoresExpDto> Frm_Sys_Ra_Autorizadores_Obtener(int CodEmpresa, int Cod_Proveedor)
        {
            return _bl.Frm_Sys_Ra_Autorizadores_Obtener(CodEmpresa, Cod_Proveedor);
        }

        [HttpPost("Frm_Sys_Ra_Autorizadores_Insertar")]
        public ErrorDto Frm_Sys_Ra_Autorizadores_Insertar(int CodCliente, AutorizadoresExpDto autorizador)
        {
            return _bl.Frm_Sys_Ra_Autorizadores_Insertar(CodCliente, autorizador);
        }

        [HttpPost("Frm_Sys_Ra_Autorizadores_Actualizar")]
        public ErrorDto Frm_Sys_Ra_Autorizadores_Actualizar(int CodEmpresa, AutorizadoresExpDto request)
        {
            return _bl.Frm_Sys_Ra_Autorizadores_Actualizar(CodEmpresa, request);
        }

        [HttpGet("Frm_Sys_Ra_AutorizadoresLista_Obtener")]
        public ErrorDto<List<AutorizadoresExpDto>> Frm_Sys_Ra_AutorizadoresLista_Obtener(int CodEmpresa)
        {
            return _bl.Frm_Sys_Ra_AutorizadoresLista_Obtener(CodEmpresa);
        }
    }
}