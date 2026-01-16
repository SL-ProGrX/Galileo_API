using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Galileo_API.BusinessLogic.ProGrX.Bancos;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Bancos.Autorizadores;

namespace Galileo_API.Controllers.ProGrX.Bancos
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FrmTesAutorizadoresController : ControllerBase
    {
        private readonly FrmTesAutorizadoresBL _autorizadoresBL;

        public FrmTesAutorizadoresController(IConfiguration config)
        {
            _autorizadoresBL = new FrmTesAutorizadoresBL(config);
        }


        [HttpGet("Tes_AutorizadoresUsuarioLista_Obtener")]
        public ErrorDto<TesAutorizadoresLista> Tes_AutorizadoresUsuarioLista_Obtener(int CodEmpresa, string filtros)
        {
            return _autorizadoresBL.Tes_AutorizadoresUsuarioLista_Obtener(CodEmpresa, filtros);
        }


        [HttpGet("Tes_AutorizadoresUsuarioBuscar_scroll")]
        public ErrorDto<TesAutorizadoresDto> Tes_AutorizadoresUsuarioBuscar_scroll(int CodEmpresa, string nombre, int? scroll)
        {
            return _autorizadoresBL.Tes_AutorizadoresUsuarioBuscar_scroll(CodEmpresa, nombre, scroll);
        }


        [HttpGet("Tes_AutorizadoresUsuario_Obtener")]
        public ErrorDto<TesAutorizadoresDto> Tes_AutorizadoresUsuario_Obtener(int CodEmpresa, string nombre)
        {
            return _autorizadoresBL.Tes_AutorizadoresUsuario_Obtener(CodEmpresa, nombre);
        }


        [HttpPost("Tes_Autorizadores_Guardar")]
        public ErrorDto Tes_Autorizadores_Guardar(int CodEmpresa, string usuario, TesAutorizadoresDto autorizador)
        {
            return _autorizadoresBL.Tes_Autorizadores_Guardar(CodEmpresa, usuario, autorizador);
        }


        [HttpDelete("Tes_Autorizadores_Eliminar")]
        public ErrorDto Tes_Autorizadores_Eliminar(int CodEmpresa, string nombre, string usuario)
        {
            return _autorizadoresBL.Tes_Autorizadores_Eliminar(CodEmpresa, nombre, usuario);
        }
    }
}
