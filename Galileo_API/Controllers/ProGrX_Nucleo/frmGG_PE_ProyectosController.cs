using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.GG_PE;

namespace Galileo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmGgPeProyectosController : ControllerBase
    {
        private readonly FrmGgPeProyectosBL BL_GG_PE_Proyectos;
        public FrmGgPeProyectosController(IConfiguration config)
        {
            BL_GG_PE_Proyectos = new FrmGgPeProyectosBL(config);
        }

        [HttpGet("PeProyectoLista_Obtener")]
        public ErrorDto<PeProyectosLista> PeProyectoLista_Obtener(int CodEmpresa, string filtros)
        {
            return BL_GG_PE_Proyectos.PeProyectoLista_Obtener(CodEmpresa, filtros);
        }

        [HttpPost("PeProyecto_Guardar")]
        public ErrorDto PeProyecto_Guardar(int CodEmpresa, PeProyectosDto proyectos)
        {
            return BL_GG_PE_Proyectos.PeProyecto_Guardar(CodEmpresa, proyectos);
        }

        [HttpDelete("PeProyecto_Eliminar")]
        public ErrorDto PeProyecto_Eliminar(int CodEmpresa, int proyecto_id)
        {
            return BL_GG_PE_Proyectos.PeProyecto_Eliminar(CodEmpresa, proyecto_id);
        }

        [HttpGet("PeObservacionesProyectos_Obtener")]
        public ErrorDto<List<PeProyectoObjetivosLista>> PeObservacionesProyectos_Obtener(int CodEmpresa, int proyecto_id)
        {
            return BL_GG_PE_Proyectos.PeObservacionesProyectos_Obtener(CodEmpresa, proyecto_id);
        }

        [HttpPost("PeObjetivoProyecto_Asociar")]
        public ErrorDto PeObjetivoProyecto_Asociar(int CodEmpresa, int proyecto_id, int objetivo_id, string usuario)
        {
            return BL_GG_PE_Proyectos.PeObjetivoProyecto_Asociar(CodEmpresa, proyecto_id, objetivo_id, usuario);
        }

        [HttpGet("PeProyectoObj_Exportar")]
        public ErrorDto<List<PeProyectoObjetivosExportar>> PeProyectoObj_Exportar(int CodEmpresa)
        {
            return BL_GG_PE_Proyectos.PeProyectoObj_Exportar(CodEmpresa);
        }

    }
}