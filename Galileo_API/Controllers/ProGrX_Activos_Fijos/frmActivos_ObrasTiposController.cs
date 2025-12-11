using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic.ProGrX_Activos_Fijos;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Activos_Fijos;

namespace Galileo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class FrmActivosObrasTiposController : ControllerBase
    {
        private readonly FrmActivosObrasTiposBL _bl;

        public FrmActivosObrasTiposController(IConfiguration config)
        {
            _bl = new FrmActivosObrasTiposBL(config);
        }

        [Authorize]
        [HttpGet("Activos_ObrasTipos_Consultar")]
        public ErrorDto<ActivosObrasTipoDataLista> Activos_ObrasTipos_Consultar(int CodEmpresa, string filtros)
        {
            return _bl.Activos_ObrasTipos_Consultar(CodEmpresa, filtros);
        }

        [Authorize]
        [HttpGet("Activos_ObrasTipos_Obtener")]
        public ErrorDto<List<ActivosObrasTipoData>> Activos_ObrasTipos_Obtener(int CodEmpresa, string filtros)
        {
            return _bl.Activos_ObrasTipos_Obtener(CodEmpresa, filtros);
        }

        [Authorize]
        [HttpPost("Activos_ObrasTipos_Guardar")]
        public ErrorDto Activos_ObrasTipos_Guardar(int CodEmpresa, string usuario, ActivosObrasTipoData datos)
        {
            return _bl.Activos_ObrasTipos_Guardar(CodEmpresa, usuario, datos);
        }

        [Authorize]
        [HttpDelete("Activos_ObrasTipos_Eliminar")]
        public ErrorDto Activos_ObrasTipos_Eliminar(int CodEmpresa, string usuario, string cod_tipo)
        {
            return _bl.Activos_ObrasTipos_Eliminar(CodEmpresa, usuario, cod_tipo);
        }
    }
}