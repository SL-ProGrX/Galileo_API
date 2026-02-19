using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic;
using Galileo.Models.ERROR;
using Galileo.Models.GG_PE;
using Microsoft.AspNetCore.Authorization;

namespace Galileo.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class FrmGgPeObjetivosController : ControllerBase
    {
        private readonly FrmGgPeObjetivosBL _bl;
        public FrmGgPeObjetivosController(IConfiguration config)
        {
            _bl = new FrmGgPeObjetivosBL(config);
        }

        [HttpGet("PeObjetivosEstrategicosLista_Obtener")]
        public ErrorDto<PeObjetivosEstrategicosDatosLista> PeObjetivosEstrategicosLista_Obtener(int CodEmpresa, string filtros)
        {
            return _bl.PeObjetivosEstrategicosLista_Obtener(CodEmpresa, filtros);
        }

        [HttpPost("ObjetivosEstrategicos_Guardar")]
        public ErrorDto ObjetivosEstrategicos_Guardar(int CodEmpresa, PeObjetivosEstrategicosDto objetivo)
        {
            return _bl.ObjetivosEstrategicos_Guardar(CodEmpresa, objetivo);
        }

        [HttpDelete("ObjetivosEstrategicos_Eliminar")]
        public ErrorDto ObjetivosEstrategicos_Eliminar(int CodEmpresa, int objetivo_id)
        {
            return _bl.ObjetivosEstrategicos_Eliminar(CodEmpresa, objetivo_id);
        }

        [HttpGet("PePerspectivaLista_Obtener")]
        public ErrorDto<List<PeObjetivosEstrategicosDto>> PePerspectivaLista_Obtener(int CodEmpresa)
        {
            return _bl.PePerspectivaLista_Obtener(CodEmpresa);
        }

        [HttpGet("PeObservacionesExportar_Obtener")]
        public ErrorDto<List<PeObjetivosEstrategicosDto>> PeObservacionesExportar_Obtener(int CodEmpresa)
        {
            return _bl.PeObservacionesExportar_Obtener(CodEmpresa);
        }

    }
}