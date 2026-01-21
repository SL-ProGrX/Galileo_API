using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.TES;
using Galileo_API.BusinessLogic.TES;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


namespace PgxAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FrmTesConsultaDesembolsosController : ControllerBase
    {
        private readonly FrmTesConsultaDesembolsosBL _bl;

        public FrmTesConsultaDesembolsosController(IConfiguration config)
        {
            _bl = new FrmTesConsultaDesembolsosBL(config);
        }

        [HttpGet("VerificarAutorizacion")]
        public ErrorDto VerificarAutorizacion(int codEmpresa, string usuario)
        {
            return _bl.VerificarAutorizacion(codEmpresa, usuario);
        }

        [HttpGet("TES_Bancos_Grupos_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> TES_Bancos_Grupos_Obtener(int CodEmpresa)
        {
            return _bl.TES_Bancos_Grupos_Obtener(CodEmpresa);
        }

        [HttpGet("TES_Bancos_Cuentas_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> TES_Bancos_Cuentas_Obtener(int CodEmpresa, string usuario, string? codGrupo = null)
        {
            return _bl.TES_Bancos_Cuentas_Obtener(CodEmpresa, usuario, codGrupo);
        }

        [HttpGet("TES_Conceptos_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> TES_Conceptos_Obtener(int CodEmpresa)
        {
            return _bl.TES_Conceptos_Obtener(CodEmpresa);
        }

        [HttpGet("TES_Tipos_Documentos_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> TES_Tipos_Documentos_Obtener(int CodEmpresa)
        {
            return _bl.TES_Tipos_Documentos_Obtener(CodEmpresa);
        }

        [HttpPost("Desembolsos_Buscar")]
        public ErrorDto<DesembolsosLista> Desembolsos_Buscar(int codEmpresa, int CodConta, [FromBody] FiltrosBusqueda filtros)
        {
            return _bl.Desembolsos_Buscar(codEmpresa, CodConta, filtros);
        }

        [HttpPost("Desembolsos_Exportar")]
        public ErrorDto<List<Desembolsos>> Desembolsos_Exportar(int codEmpresa, int CodConta, FiltrosBusqueda filtros)
        {
            return _bl.Desembolsos_Exportar(codEmpresa, CodConta, filtros);
        }
    }
}