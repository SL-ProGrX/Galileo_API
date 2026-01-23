using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Bancos;
using Galileo_API.BusinessLogic.ProGrX.Bancos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmTesAutoRegistroController : ControllerBase
    {
        private readonly FrmTesAutoRegistroBL _AutoRegistroBL;

        public FrmTesAutoRegistroController(IConfiguration config)
        {
            _AutoRegistroBL = new FrmTesAutoRegistroBL(config);
        }

        [Authorize]
        [HttpGet("Tes_AutoRegistro_Consultar")]
        public ErrorDto<TesAutoRegistroDto> Tes_AutoRegistro_Consultar(int CodEmpresa, int autoReg)
        {
            return _AutoRegistroBL.Tes_AutoRegistro_Consultar(CodEmpresa, autoReg);
        }

        [Authorize]
        [HttpPost("Tes_AutoRegistro_Guardar")]
        public ErrorDto Tes_AutoRegistro_Guardar(int CodEmpresa, TesAutoRegistroDto registro)
        {
            return _AutoRegistroBL.Tes_AutoRegistro_Guardar(CodEmpresa, registro);
        }

        [Authorize]
        [HttpDelete("Tes_AutoRegistro_Eliminar")]
        public ErrorDto Tes_AutoRegistro_Eliminar(int CodEmpresa, string registro)
        {
            return _AutoRegistroBL.Tes_AutoRegistro_Eliminar(CodEmpresa, registro);
        }

        [Authorize]
        [HttpGet("Tes_AutoRegistroCtaBancos_Obtener")]
        public ErrorDto<List<TesAutoRegCtaBancariasData>> Tes_AutoRegistroCtaBancos_Obtener(int CodEmpresa, int? codigo, string? FiltraCtas)
        {
            return _AutoRegistroBL.Tes_AutoRegistroCtaBancos_Obtener(CodEmpresa, codigo, FiltraCtas);
        }

        //[Authorize]
        [HttpPatch("Tes_AutoRegistroCtaBancos_Asignar")]
        public ErrorDto Tes_AutoRegistroCtaBancos_Asignar(int CodEmpresa, int codigo, int cta, bool asignado, string usuario)
        {
            return _AutoRegistroBL.Tes_AutoRegistroCtaBancos_Asignar(CodEmpresa, codigo, cta, asignado, usuario);
        }

        [Authorize]
        [HttpGet("Tes_AutoRegistroTipos_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> Tes_AutoRegistroTipos_Obtener(int CodEmpresa, int? tipo, string? filtro)
        {
            return _AutoRegistroBL.Tes_AutoRegistroTipos_Obtener(CodEmpresa, tipo, filtro);
        }

        [Authorize]
        [HttpGet("Tes_AutoRegistroCentroCostos_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> Tes_AutoRegistroCentroCostos_Obtener(int CodEmpresa)
        {
            return _AutoRegistroBL.Tes_AutoRegistroCentroCostos_Obtener(CodEmpresa);
        }

        [Authorize]
        [HttpGet("Tes_AutoRegistroCodigoDesc_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> Tes_AutoRegistroCodigoDesc_Obtener(int CodEmpresa, string tipo, string codigo)
        {
            return _AutoRegistroBL.Tes_AutoRegistroCodigoDesc_Obtener(CodEmpresa, tipo, codigo);
        }

        [Authorize]
        [HttpGet("Tes_AutoRegistroConceptos_Obtener")]
        public ErrorDto<List<TesAutoregistroConceptos>> Tes_AutoRegistroConceptos_Obtener(int CodEmpresa, string? concepto = null)
        {
            return _AutoRegistroBL.Tes_AutoRegistroConceptos_Obtener(CodEmpresa, concepto);
        }

        [Authorize]
        [HttpGet("Tes_AutoRegistroCentroUnidades_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> Tes_AutoRegistroCentroUnidades_Obtener(int CodEmpresa)
        {
            return _AutoRegistroBL.Tes_AutoRegistroCentroUnidades_Obtener(CodEmpresa);
        }

        [Authorize]
        [HttpGet("Tes_AutoRegistroTiposDoc_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> Tes_AutoRegistroTiposDoc_Obtener(int CodEmpresa, string TipoMov)
        {
           return _AutoRegistroBL.Tes_AutoRegistroTiposDoc_Obtener(CodEmpresa, TipoMov);
        }

        [Authorize]
        [HttpGet("Tes_AutoRegistroLista_Obtener")]
        public ErrorDto<TesAutoRegistroLista> Tes_AutoRegistroLista_Obtener(int CodEmpresa, string filtros)
        {
            return _AutoRegistroBL.Tes_AutoRegistroLista_Obtener(CodEmpresa, filtros);
        }

        [Authorize]
        [HttpGet("Tes_AutoRegistro_scroll")]
        public ErrorDto<TesAutoRegistroDto> Tes_AutoRegistro_scroll(int CodEmpresa, int autoReg, int? scroll)
        {
            return _AutoRegistroBL.Tes_AutoRegistro_scroll(CodEmpresa, autoReg, scroll);
        }
    }
}
