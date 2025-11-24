using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic;
using Galileo.Models;
using Galileo.Models.Security;

namespace Galileo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmUs_MenusController : ControllerBase
    {
        readonly FrmUsMenusBl SecurityUsBL;
        public FrmUs_MenusController(IConfiguration config)
        {
            SecurityUsBL = new FrmUsMenusBl(config);
        }

        [HttpGet("obtenerUsMenus")]
        public List<UsMenuDto> obtenerUsMenus()
        {
            return SecurityUsBL.ObtenerUsMenus();
        }

        [HttpGet("ObtenerUsModulos")]
        public List<UsModuloDto> ObtenerUsModulos()
        {
            return SecurityUsBL.ObtenerUsModulos();
        }

        [HttpGet("ObtenerUsFormularios")]
        public List<UsFormularioDto> ObtenerUsFormularios()
        {
            return SecurityUsBL.ObtenerUsFormularios();
        }//end ObtenerUsFormularios

        [HttpGet("ObtenerMenuNodoConIsNull")]
        public int? ObtenerMenuNodoConIsNull()
        {
            return SecurityUsBL.ObtenerMenuNodoConIsNull();
        }

        [HttpGet("ObtenerUsMenusPorTipoYNodoPadreEsNull")]
        public List<UsMenuDto> ObtenerUsMenusPorTipoYNodoPadreEsNull(string Tipo)
        {
            return SecurityUsBL.ObtenerUsMenusPorTipoYNodoPadreEsNull(Tipo);
        }

        [HttpGet("ObtenerMenuPrioridadPorMenuNodoPadre")]
        public int? ObtenerMenuPrioridadPorMenuNodoPadre(int NodoPadre)
        {
            return SecurityUsBL.ObtenerMenuPrioridadPorMenuNodoPadre(NodoPadre);
        }

        [HttpGet("ObtenerUsModulosOrdenadosPorTipo")]
        public UsModuloDto ObtenerUsModulosOrdenadosPorTipo(string Tipo)
        {
            var result = SecurityUsBL.ObtenerUsModulosOrdenadosPorTipo(Tipo);
            return result ?? new UsModuloDto();
        }

        [HttpGet("ObtenerMenuNodoPorNodoPadreYPrioridad")]
        public int? ObtenerMenuNodoPorNodoPadreYPrioridad(int NodoPadre, int Prioridad)
        {
            return SecurityUsBL.ObtenerMenuNodoPorNodoPadreYPrioridad(NodoPadre, Prioridad);
        }

        [HttpGet("EliminarUnMenuPorNodoPadre")]
        public int? EliminarUnMenuPorNodoPadre(int NodoPadre)
        {
            return SecurityUsBL.EliminarUnMenuPorNodoPadre(NodoPadre);
        }

        [HttpGet("EliminarUsMenusPorMenuNodo")]
        public int? EliminarUsMenusPorMenuNodo(int MenuNodo)
        {
            return SecurityUsBL.EliminarUsMenusPorMenuNodo(MenuNodo);
        }

        [HttpGet("EliminarTodosLosMenusPorNodoPadre")]
        public int? EliminarTodosLosMenusPorNodoPadre(int NodoPadre)
        {
            return SecurityUsBL.EliminarTodosLosMenusPorNodoPadre(NodoPadre);
        }

        [HttpGet("ObtenerMenuNodoPorMenuFormulario")]
        public int? ObtenerMenuNodoPorMenuFormulario(string Formulario)
        {
            return SecurityUsBL.ObtenerMenuNodoPorMenuFormulario(Formulario);
        }

        [HttpGet("ObtenerUsFormularioPorFormulario")]
        public UsFormularioDto ObtenerUsFormularioPorFormulario(string Formulario)
        {
            return SecurityUsBL.ObtenerUsFormularioPorFormulario(Formulario);
        }

        [HttpGet("ObtenerUsMenuPorMenuNodo")]
        public UsMenuDto ObtenerUsMenuPorMenuNodo(int MenuNodo)
        {
            return SecurityUsBL.ObtenerUsMenuPorMenuNodo(MenuNodo);
        }

        [HttpPost("ActualizarUsMenu")]
        public ResultadoCrearYEditarUsMenuDto? ActualizarUsMenu(UsMenuDto Info)
        {
            return SecurityUsBL.ActualizarUsMenu(Info);
        }

        [HttpPost("CrearUsMenu")]
        public ResultadoCrearYEditarUsMenuDto? CrearUsMenu(UsMenuDto Info)
        {
            return SecurityUsBL.CrearUsMenu(Info);
        }

        [HttpPost("ObtenerUsMenu_IconosWeb")]
        public List<UsIconWeb> ObtenerUsMenu_IconosWeb()
        {
            return SecurityUsBL.ObtenerUsMenu_IconosWeb();
        }

    }
}
