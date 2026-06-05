using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Personas;
using Galileo.BusinessLogic.ProGrX_Personas;

namespace Galileo.Controllers.ProGrX_Personas
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmAFCatalogosController : ControllerBase
    {
        private readonly FrmAFCatalogosBL _bl;

        public FrmAFCatalogosController(IConfiguration config)
        {
            _bl = new FrmAFCatalogosBL(config);
        }

        [Authorize]
        [HttpGet("AF_Catalogos_Obtener")]
        public ErrorDto<CatalogoLista> AF_Catalogos_Obtener(int CodEmpresa, int tipoId, string filtros)
        {
            return _bl.AF_Catalogos_Obtener(CodEmpresa, tipoId, filtros);
        }

        [Authorize]
        [HttpGet("AF_Catalogos_Valida")]
        public ErrorDto<CatalogoValidate> AF_Catalogos_Valida(int CodEmpresa, string catalogoId, int tipoId)
        {
            return _bl.AF_Catalogos_Valida(CodEmpresa, catalogoId, tipoId);
        }

        [Authorize]
        [HttpPost("AF_Catalogos_Guardar")]
        public ErrorDto AF_Catalogos_Guardar(int CodEmpresa, string usuario, CatalogoData catalogo)
        {
            return _bl.AF_Catalogos_Guardar(CodEmpresa, usuario, catalogo);
        }

        [Authorize]
        [HttpDelete("AF_Catalogos_Eliminar")]
        public ErrorDto AF_Catalogos_Eliminar(int CodEmpresa, string usuario, int lineaId)
        {
            return _bl.AF_Catalogos_Eliminar(CodEmpresa, usuario, lineaId);
        }

        [Authorize]
        [HttpGet("AF_Catalogos_Tipos_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> AF_Catalogos_Tipos_Obtener(int CodEmpresa)
        {
            return _bl.AF_Catalogos_Tipos_Obtener(CodEmpresa);
        }

        [Authorize]
        [HttpGet("AF_Catalogos_Tipos_ObtenerTodos")]
        public ErrorDto<List<CatalogoTipoData>> AF_Catalogos_Tipos_ObtenerTodos(int CodEmpresa, string filtros)
        {
            return _bl.AF_Catalogos_Tipos_ObtenerTodos(CodEmpresa, filtros);
        }
    }
}
