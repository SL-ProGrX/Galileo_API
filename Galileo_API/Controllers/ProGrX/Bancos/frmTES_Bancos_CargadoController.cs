using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Bancos;
using Galileo_API.BusinessLogic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


namespace Galileo_API.Controllers.ProGrX.Bancos
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FrmTesBancosCargadoController : ControllerBase
    {

        private readonly FrmTesBancosCargadoBL _bl;

        public FrmTesBancosCargadoController(IConfiguration config)
        {
            _bl = new FrmTesBancosCargadoBL(config);
        }

        [HttpGet("Tes_Bancos_Obtener")]
        public ErrorDto<List<DropDownListaBancosCargados>> Tes_Bancos_Obtener(int CodEmpresa, string usuario)
        {
            return _bl.Tes_Bancos_Obtener(CodEmpresa, usuario);
        }

        [HttpGet("Tes_BancosCargadoConceptos_Obtener")]
        public ErrorDto<List<TesBancoCargadoConceptos>> Tes_BancosCargadoConceptos_Obtener(int CodEmpresa, string? concepto = null)
        {
            return _bl.Tes_BancosCargadoConceptos_Obtener(CodEmpresa, concepto);
        }

        [HttpGet("Tes_BancosCargadoCentroUnidades_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> Tes_BancosCargadoCentroUnidades_Obtener(int CodEmpresa)
        {
            return _bl.Tes_BancosCargadoCentroUnidades_Obtener(CodEmpresa);
        }

        [HttpGet("Tes_BancosCargadoCentroCostos_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> Tes_BancosCargadoCentroCostos_Obtener(int CodEmpresa)
        {
            return _bl.Tes_BancosCargadoCentroCostos_Obtener(CodEmpresa);
        }

        [HttpGet("Tes_AutoRegistroLista_Obtener")]
        public ErrorDto<TesAutoRegistroLista> Tes_AutoRegistroLista_Obtener(int CodEmpresa, string filtros)
        {
            return _bl.Tes_AutoRegistroLista_Obtener(CodEmpresa, filtros);
        }

        [HttpPost("TES_BancosCargados_Aplicar")]
        public ErrorDto TES_ConciliacionResumenArchivo_Cargar(int CodEmpresa, string cod_banco, string usuario, List<TesCargadoExcelDto> file)
        {
            return _bl.TES_BancosCargados_Aplicar(CodEmpresa, cod_banco, usuario, file);
        }

        [HttpGet("TES_ListaRegistroBancos_Obtener")]
        public ErrorDto<List<TeslistaRegistroBancosDto>> TES_ListaRegistroBancos_Obtener(int CodEmpresa, string filtros)
        {
            return _bl.TES_ListaRegistroBancos_Obtener(CodEmpresa, filtros);
        }

        [HttpPost("TES_RegistrosBancosCargados_Aplicar")]
        public ErrorDto TES_RegistrosBancosCargados_Aplicar(int CodEmpresa, string registroLista)
        {
            return _bl.TES_RegistrosBancosCargados_Aplicar(CodEmpresa, registroLista);
        }

        [HttpPost("TES_RegistrosBancosCargados_Elimina")]
        public ErrorDto TES_RegistrosBancosCargados_Elimina(int CodEmpresa, string registroLista)
        {
            return _bl.TES_RegistrosBancosCargados_Elimina(CodEmpresa, registroLista);
        }
    }
}
