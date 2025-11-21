using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic;
using Galileo.Models.ERROR;
using Galileo.Models.Security;

namespace Galileo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MenuController : ControllerBase
    {
        readonly MenuBL MenuBL;

        public MenuController(IConfiguration config)
        {
            MenuBL = new MenuBL(config);
        }


        [HttpGet("ObtenerMenuV2Usuario")]
        //[Authorize]
        public List<PrimeTreeDtoV2> ObtenerMenuV2Usuario(string nombreUsuario, int codEmpresa)
        {
            return MenuBL.GenerarMenuV2(nombreUsuario, codEmpresa);
        }

        [HttpPost("Agregar_MenuFavoritos")]
        //[Authorize]
        public int Agregar_MenuFavoritos(string nombreUsuario, int codEmpresa, int nodo, string opcion)
        {
            return MenuBL.Agregar_MenuFavoritos(nombreUsuario, codEmpresa, nodo, opcion);
        }

        [HttpGet("Obtener_MenuFavoritos")]
        //[Authorize]
        public List<PrimeTreeDto> Obtener_MenuFavoritos(string nombreUsuario, int codEmpresa)
        {
            return MenuBL.Obtener_MenuFavoritos(nombreUsuario, codEmpresa);
        }

        [HttpGet("ManualMenu_Obtener")]
        //[Authorize]
        public ErrorDto<UsMenuManual> ManualMenu_Obtener(string formulario)
        {
            return MenuBL.ManualMenu_Obtener(formulario);
        }

        [HttpGet("ManualFormulario_Obtener")]
        public ErrorDto<string> ManualFormulario_Obtener(string formulario)
        {
            return MenuBL.ManualFormulario_Obtener(formulario);
        }


    }
}
