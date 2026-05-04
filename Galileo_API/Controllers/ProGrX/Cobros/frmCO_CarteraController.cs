using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic.ProGrX.Cobros;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Cobros;

namespace Galileo.Controllers.ProGrX.Cobros
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmCOCarteraController : Controller
    {
        private readonly IConfiguration? _config;
        private readonly FrmCOCarteraBL _bl;

        public FrmCOCarteraController(IConfiguration config)
        {
            _config = config;
            _bl = new FrmCOCarteraBL(_config);
        }

        [Authorize]
        [HttpGet("Co_CarteraLista_Obtener")]
        public ErrorDto<COCarteraListaResult> Co_CarteraLista_Obtener(int CodEmpresa, string filtros)
        {
            return _bl.Co_CarteraLista_Obtener(CodEmpresa, filtros);
        }

        [Authorize]
        [HttpGet("Co_Cartera_Export")]
        public ErrorDto<COCarteraListaResult> Co_Cartera_Export(int CodEmpresa, string filtros)
        {
            return _bl.Co_Cartera_Export(CodEmpresa, filtros);
        }

        [Authorize]
        [HttpPost("Co_Cartera_Guardar")]
        public ErrorDto Co_Cartera_Guardar(int CodEmpresa, string usuario, COCarteraClasificacionData cartera)
        {
            return _bl.Co_Cartera_Guardar(CodEmpresa, usuario, cartera);
        }

        [Authorize]
        [HttpDelete("Co_Cartera_Eliminar")]
        public ErrorDto Co_Cartera_Eliminar(int CodEmpresa, string usuario, string cod_clasificacion)
        {
            return _bl.Co_Cartera_Eliminar(CodEmpresa, usuario, cod_clasificacion);
        }

        [Authorize]
        [HttpGet("Co_Catalogo_Obtener")]
        public ErrorDto<List<COCarteraCatalogoData>> Co_Catalogo_Obtener(int CodEmpresa)
        {
            return _bl.Co_Catalogo_Obtener(CodEmpresa);
        }

        [Authorize]
        [HttpGet("Co_Asignacion_Carteras_PorCodigo_Obtener")]
        public ErrorDto<List<COCarteraAsignacionCatItemData>> Co_Asignacion_Carteras_PorCodigo_Obtener(int CodEmpresa, string codigo)
        {
            return _bl.Co_Asignacion_Carteras_PorCodigo_Obtener(CodEmpresa, codigo);
        }

        [Authorize]
        [HttpGet("Co_Asignacion_Codigos_PorCartera_Obtener")]
        public ErrorDto<List<COCarteraAsignacionCodigoItemData>> Co_Asignacion_Codigos_PorCartera_Obtener(int CodEmpresa, string cod_clasificacion)
        {
            return _bl.Co_Asignacion_Codigos_PorCartera_Obtener(CodEmpresa, cod_clasificacion);
        }

        [Authorize]
        [HttpPost("Co_Asignacion_Guardar")]
        public ErrorDto Co_Asignacion_Guardar(int CodEmpresa, string usuario, COCarteraAsignacionGuardarDto dto)
        {
            return _bl.Co_Asignacion_Guardar(CodEmpresa, usuario, dto);
        }

        [Authorize]
        [HttpPost("Co_Asignacion_Bulk_Guardar")]
        public ErrorDto Co_Asignacion_Bulk_Guardar(int CodEmpresa, string usuario, COCarteraAsignacionBulkDto dto)
        {
            return _bl.Co_Asignacion_Bulk_Guardar(CodEmpresa, usuario, dto);
        }
        [Authorize]
        [HttpGet("Co_Carteras_Dropdown_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> Co_Carteras_Dropdown_Obtener(int CodEmpresa)
        {
            return _bl.Co_Carteras_Dropdown_Obtener(CodEmpresa);
        }
    }
}
