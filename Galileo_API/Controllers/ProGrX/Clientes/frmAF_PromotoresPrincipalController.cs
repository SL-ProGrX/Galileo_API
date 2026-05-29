using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic.ProGrX.Clientes;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Clientes;

namespace Galileo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmAFPromotoresPrincipalController : ControllerBase
    {
        private readonly FrmAFPromotoresPrincipalBL _bl;

        public FrmAFPromotoresPrincipalController(IConfiguration config)
        {
            _bl = new FrmAFPromotoresPrincipalBL(config);
        }

        [Authorize]
        [HttpGet("AF_Promotores_Lista_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> AF_Promotores_Lista_Obtener(int CodEmpresa)
        {
            return _bl.AF_Promotores_Lista_Obtener(CodEmpresa);
        }

        [Authorize]
        [HttpGet("AF_Promotores_Usuarios_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> AF_Promotores_Usuarios_Obtener(int CodEmpresa)
        {
            return _bl.AF_Promotores_Usuarios_Obtener(CodEmpresa);
        }

        [Authorize]
        [HttpGet("AF_Promotores_Scroll_Obtener")]
        public ErrorDto<AfPromotoresPrincipalDto?> AF_Promotores_Scroll_Obtener(int CodEmpresa, int ScrollCode, int Codigo)
        {
            return _bl.AF_Promotores_Scroll_Obtener(CodEmpresa, ScrollCode, Codigo);
        }

        [Authorize]
        [HttpGet("AF_Promotor_Obtener")]
        public ErrorDto<AfPromotoresPrincipalDto?> AF_Promotor_Obtener(int CodEmpresa, int Codigo)
        {
            return _bl.AF_Promotor_Obtener(CodEmpresa, Codigo);
        }

        [Authorize]
        [HttpGet("AF_Promotores_Cuentas_Obtener")]
        public ErrorDto<List<AfPromotoresCuentasDto>> AF_Promotores_Cuentas_Obtener(int CodEmpresa, string CodComision)
        {
            return _bl.AF_Promotores_Cuentas_Obtener(CodEmpresa, CodComision);
        }

        [Authorize]
        [HttpGet("AF_Promotores_ListadoConsulta_Obtener")]
        public ErrorDto<AfPromotoresPrincipalLista> AF_Promotores_ListadoConsulta_Obtener(int CodEmpresa, string Tipo, int Estado, string filtros)
        {
            return _bl.AF_Promotores_ListadoConsulta_Obtener(CodEmpresa, Tipo, Estado, filtros);
        }

        [Authorize]
        [HttpGet("AF_Promotores_Bancos_Obtener")]
        public ErrorDto<List<AfPromotoresBancoDto>> AF_Promotores_Bancos_Obtener(int CodEmpresa, string Usuario)
        {
            return _bl.AF_Promotores_Bancos_Obtener(CodEmpresa, Usuario);
        }

        [Authorize]
        [HttpPost("AF_Promotores_Guardar")]
        public ErrorDto AF_Promotores_Guardar(int CodEmpresa, string Usuario, AfPromotoresPrincipalDto Info)
        {
            return _bl.AF_Promotores_Guardar(CodEmpresa, Usuario, Info);
        }

        [Authorize]
        [HttpDelete("AF_Promotores_Eliminar")]
        public ErrorDto AF_Promotores_Eliminar(int CodEmpresa, string Usuario, int Codigo)
        {
            return _bl.AF_Promotores_Eliminar(CodEmpresa, Usuario, Codigo);
        }
    }
}