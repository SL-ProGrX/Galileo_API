using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Galileo.BusinessLogic.ProGrx_Personas;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Personas;

namespace Galileo.Controllers.ProGrx_Personas
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmAfUnidadesController : ControllerBase
    {
        private readonly FrmAfUnidadesBl BlAfUnidades;
        
        public FrmAfUnidadesController(IConfiguration config)
        {
            BlAfUnidades = new FrmAfUnidadesBl(config);
        }

        [Authorize]
        [HttpGet("AF_Unidades_Provincias_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> AF_Unidades_Provincias_Obtener(int CodEmpresa)
        {
            return BlAfUnidades.AF_Unidades_Provincias_Obtener(CodEmpresa);
        }

        [Authorize]
        [HttpGet("AF_Unidades_Lista_Obtener")]
        public ErrorDto<TablasListaGenericaModel> AF_Unidades_Lista_Obtener(int CodEmpresa, int rbTipo, string filtros)
        {
            return BlAfUnidades.AF_Unidades_Lista_Obtener(CodEmpresa, rbTipo, filtros);
        }

        [Authorize]
        [HttpGet("AF_Unidades_BuscarPorCodigo_Obtener")]
        public ErrorDto<AfUnidadesDto> AF_Unidades_BuscarPorCodigo_Obtener(int CodEmpresa, int rbTipo, string Codigo)
        {
            return BlAfUnidades.AF_Unidades_BuscarPorCodigo_Obtener(CodEmpresa, rbTipo, Codigo);
        }

        [Authorize]
        [HttpPost("AF_Unidades_Guardar")]
        public ErrorDto AF_Unidades_Guardar(int CodEmpresa, int rbTipo, bool Editar, AfUnidadesDto Info, string Usuario)
        {
            return BlAfUnidades.AF_Unidades_Guardar(CodEmpresa, rbTipo, Editar, Info, Usuario);
        }

        [Authorize]
        [HttpDelete("AF_Unidades_Eliminar")]
        public ErrorDto AF_Unidades_Eliminar(int CodEmpresa, int rbTipo, string Codigo, string Usuario)
        {
            return BlAfUnidades.AF_Unidades_Eliminar(CodEmpresa, rbTipo, Codigo, Usuario);
        }
    }
}
