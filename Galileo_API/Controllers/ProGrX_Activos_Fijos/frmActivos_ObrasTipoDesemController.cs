using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic.ProGrX_Activos_Fijos;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Activos_Fijos;

namespace Galileo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class FrmActivosObrasTipoDesemController : ControllerBase
    {
        private readonly FrmActivosObrasTipoDesemBL _bl;
        public FrmActivosObrasTipoDesemController(IConfiguration config)
        {
            _bl = new FrmActivosObrasTipoDesemBL(config);
        }

        [Authorize]
        [HttpGet("Activos_ObrasTipoDesem_Consultar")]
        public ErrorDto<ActivosObrasTipoDesemDataLista> Activos_ObrasTipoDesem_Consultar(int CodEmpresa, string filtros)
        {
            return _bl.Activos_ObrasTipoDesem_Consultar(CodEmpresa, filtros);
        }

        [Authorize]
        [HttpGet("Activos_ObrasTipoDesem_Obtener")]
        public ErrorDto<List<ActivosObrasTipoDesemData>> Activos_ObrasTipoDesem_Obtener(int CodEmpresa, string filtros)
        {
            return _bl.Activos_ObrasTipoDesem_Obtener(CodEmpresa, filtros);
        }

        [Authorize]
        [HttpPost("Activos_ObrasTipoDesem_Guardar")]
        public ErrorDto Activos_ObrasTipoDesem_Guardar(int CodEmpresa, string usuario, ActivosObrasTipoDesemData datos)
        {
            return _bl.Activos_ObrasTipoDesem_Guardar(CodEmpresa, usuario, datos);
        }

        [Authorize]
        [HttpDelete("Activos_ObrasTipoDesem_Eliminar")]
        public ErrorDto Activos_ObrasTipoDesem_Eliminar(int CodEmpresa, string usuario, string cod_desembolso)
        {
            return _bl.Activos_ObrasTipoDesem_Eliminar(CodEmpresa, usuario, cod_desembolso);
        }
    }
}