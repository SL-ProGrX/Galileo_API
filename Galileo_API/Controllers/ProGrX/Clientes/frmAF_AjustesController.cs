using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Clientes;

namespace Galileo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmAFAjustesController : ControllerBase
    {
        private readonly FrmAFAjustesBL _bl;

        public FrmAFAjustesController(IConfiguration config)
        {
            _bl = new FrmAFAjustesBL(config);
        }

        [Authorize]
        [HttpGet("AF_Instituciones_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> AF_Instituciones_Obtener(int CodEmpresa)
        {
            return _bl.AF_Instituciones_Obtener(CodEmpresa);
        }

        [Authorize]
        [HttpGet("AF_TiposId_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> AF_TiposId_Obtener(int CodEmpresa)
        {
            return _bl.AF_TiposId_Obtener(CodEmpresa);
        }

        [Authorize]
        [HttpGet("AF_EstadosPersona_ObtenerActivos")]
        public ErrorDto<List<DropDownListaGenericaModel>> AF_EstadosPersona_ObtenerActivos(int CodEmpresa)
        {
            return _bl.AF_EstadosPersona_ObtenerActivos(CodEmpresa);
        }

        [Authorize]
        [HttpPost("AF_Ajustes_CambiarIdentificacion")]
        public ErrorDto AF_Ajustes_CambiarIdentificacion(int CodEmpresa, string cedula, int nuevoTipoId)
        {
            return _bl.AF_Ajustes_CambiarIdentificacion(CodEmpresa, cedula, nuevoTipoId);
        }

        [Authorize]
        [HttpPost("AF_Ajustes_CambiarEstado")]
        public ErrorDto AF_Ajustes_CambiarEstado(int CodEmpresa, string cedula, string nuevoEstado)
        {
            return _bl.AF_Ajustes_CambiarEstado(CodEmpresa, cedula, nuevoEstado);
        }

        [Authorize]
        [HttpPost("AF_Ajustes_CambiarInstitucion_ASECCSS")]
        public ErrorDto AF_Ajustes_CambiarInstitucion_ASECCSS(int CodEmpresa, string cedula, string cambiosJson)
        {
            return _bl.AF_Ajustes_CambiarInstitucion_ASECCSS(CodEmpresa, cedula, cambiosJson);
        }

        [Authorize]
        [HttpPost("AF_Ajustes_CambiarInstitucion")]
        public ErrorDto AF_Ajustes_CambiarInstitucion(int CodEmpresa, string cedula, string cambiosJson)
        {
            return _bl.AF_Ajustes_CambiarInstitucion(CodEmpresa, cedula, cambiosJson);
        }

        [Authorize]
        [HttpGet("AF_Ajustes_CargarDatos")]
        public ErrorDto<AfAjustePersonaDetalle> AF_Ajustes_CargarDatos(int CodEmpresa, string cedula)
        {
            return _bl.AF_Ajustes_CargarDatos(CodEmpresa, cedula);
        }

        [Authorize]
        [HttpPost("AF_Ajustes_Cambiar")]
        public ErrorDto AF_Ajustes_Cambiar(int CodEmpresa, string ajuste, int codigo)
        {
            return _bl.AF_Ajustes_Cambiar(CodEmpresa, ajuste, codigo);
        }

        [Authorize]
        [HttpGet("AF_Catalogos_Obtener")]
        public ErrorDto<AfCatalogosGeneralesDto> AF_Catalogos_Obtener(int CodEmpresa, string cod_institucion)
        {
            return _bl.AF_Catalogos_Obtener(CodEmpresa, cod_institucion);
        }
    }
}