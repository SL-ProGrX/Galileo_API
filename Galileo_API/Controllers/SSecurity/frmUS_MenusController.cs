using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic;
using Galileo.Models;
using Galileo.Models.Security;
using Galileo.Models.ERROR;
using Microsoft.AspNetCore.Authorization;

namespace Galileo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FrmUsMenusController : ControllerBase
    {
        readonly FrmUsMenusBl SecurityUsBL;
        public FrmUsMenusController(IConfiguration config)
        {
            SecurityUsBL = new FrmUsMenusBl(config);
        }

        [HttpGet("obtenerUsMenus")]
        public ErrorDto<List<UsMenuDto>> obtenerUsMenus()
            => SecurityUsBL.ObtenerUsMenus();

        [HttpGet("ObtenerUsModulos")]
        public ErrorDto<List<UsModuloDto>> ObtenerUsModulos()
            => SecurityUsBL.ObtenerUsModulos();

        [HttpGet("ObtenerUsFormularios")]
        public ErrorDto<List<UsFormularioDto>> ObtenerUsFormularios()
            => SecurityUsBL.ObtenerUsFormularios();//end ObtenerUsFormularios

        [HttpGet("ObtenerMenuNodoConIsNull")]
        public ErrorDto<int?> ObtenerMenuNodoConIsNull()
            => SecurityUsBL.ObtenerMenuNodoConIsNull();

        [HttpGet("ObtenerUsMenusPorTipoYNodoPadreEsNull")]
        public ErrorDto<List<UsMenuDto>> ObtenerUsMenusPorTipoYNodoPadreEsNull(string Tipo)
            => SecurityUsBL.ObtenerUsMenusPorTipoYNodoPadreEsNull(Tipo);

        [HttpGet("ObtenerMenuPrioridadPorMenuNodoPadre")]
        public ErrorDto<int?> ObtenerMenuPrioridadPorMenuNodoPadre(int NodoPadre)
            => SecurityUsBL.ObtenerMenuPrioridadPorMenuNodoPadre(NodoPadre);

        [HttpGet("ObtenerUsModulosOrdenadosPorTipo")]
        public ErrorDto<UsModuloDto?> ObtenerUsModulosOrdenadosPorTipo(string Tipo)
            => SecurityUsBL.ObtenerUsModulosOrdenadosPorTipo(Tipo);

        [HttpGet("ObtenerMenuNodoPorNodoPadreYPrioridad")]
        public ErrorDto<int?> ObtenerMenuNodoPorNodoPadreYPrioridad(int NodoPadre, int Prioridad)
            => SecurityUsBL.ObtenerMenuNodoPorNodoPadreYPrioridad(NodoPadre, Prioridad);

        [HttpGet("EliminarUnMenuPorNodoPadre")]
        public ErrorDto<int?> EliminarUnMenuPorNodoPadre(int NodoPadre)
            => SecurityUsBL.EliminarUnMenuPorNodoPadre(NodoPadre);

        [HttpGet("EliminarUsMenusPorMenuNodo")]
        public ErrorDto<int?> EliminarUsMenusPorMenuNodo(int MenuNodo)
            => SecurityUsBL.EliminarUsMenusPorMenuNodo(MenuNodo);

        [HttpGet("EliminarTodosLosMenusPorNodoPadre")]
        public ErrorDto<int?> EliminarTodosLosMenusPorNodoPadre(int NodoPadre)
            => SecurityUsBL.EliminarTodosLosMenusPorNodoPadre(NodoPadre);

        [HttpGet("ObtenerMenuNodoPorMenuFormulario")]
        public ErrorDto<int?> ObtenerMenuNodoPorMenuFormulario(string Formulario)
            => SecurityUsBL.ObtenerMenuNodoPorMenuFormulario(Formulario);

        [HttpGet("ObtenerUsFormularioPorFormulario")]
        public ErrorDto<UsFormularioDto> ObtenerUsFormularioPorFormulario(string Formulario)
            => SecurityUsBL.ObtenerUsFormularioPorFormulario(Formulario);

        [HttpGet("ObtenerUsMenuPorMenuNodo")]
        public ErrorDto<UsMenuDto> ObtenerUsMenuPorMenuNodo(int MenuNodo)
            => SecurityUsBL.ObtenerUsMenuPorMenuNodo(MenuNodo);

        [HttpPost("ActualizarUsMenu")]
        public ErrorDto<ResultadoCrearYEditarUsMenuDto?> ActualizarUsMenu(UsMenuDto Info)
            => SecurityUsBL.ActualizarUsMenu(Info);

        [HttpPost("CrearUsMenu")]
        public ErrorDto<ResultadoCrearYEditarUsMenuDto?> CrearUsMenu(UsMenuDto Info)
            => SecurityUsBL.CrearUsMenu(Info);

        [HttpPost("ObtenerUsMenu_IconosWeb")]
        public ErrorDto<List<UsIconWeb>> ObtenerUsMenu_IconosWeb()
            => SecurityUsBL.ObtenerUsMenu_IconosWeb();

    }
}
