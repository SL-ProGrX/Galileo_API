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

        private static ErrorDto<T> Success<T>(T result) => new()
        {
            Result = result,
            Code = 0,
            Description = "Ok"
        };

        private static ErrorDto<T> Failure<T>(Exception exception) => new()
        {
            Result = default,
            Code = -1,
            Description = exception.Message
        };

        [HttpGet("obtenerUsMenus")]
        public ErrorDto<List<UsMenuDto>> obtenerUsMenus()
        {
            try
            {
                return Success(SecurityUsBL.ObtenerUsMenus());
            }
            catch (Exception ex)
            {
                return Failure<List<UsMenuDto>>(ex);
            }
        }

        [HttpGet("ObtenerUsModulos")]
        public ErrorDto<List<UsModuloDto>> ObtenerUsModulos()
        {
            try
            {
                return Success(SecurityUsBL.ObtenerUsModulos());
            }
            catch (Exception ex)
            {
                return Failure<List<UsModuloDto>>(ex);
            }
        }

        [HttpGet("ObtenerUsFormularios")]
        public ErrorDto<List<UsFormularioDto>> ObtenerUsFormularios()
        {
            try
            {
                return Success(SecurityUsBL.ObtenerUsFormularios());
            }
            catch (Exception ex)
            {
                return Failure<List<UsFormularioDto>>(ex);
            }
        }//end ObtenerUsFormularios

        [HttpGet("ObtenerMenuNodoConIsNull")]
        public ErrorDto<int?> ObtenerMenuNodoConIsNull()
        {
            try
            {
                return Success(SecurityUsBL.ObtenerMenuNodoConIsNull());
            }
            catch (Exception ex)
            {
                return Failure<int?>(ex);
            }
        }

        [HttpGet("ObtenerUsMenusPorTipoYNodoPadreEsNull")]
        public ErrorDto<List<UsMenuDto>> ObtenerUsMenusPorTipoYNodoPadreEsNull(string Tipo)
        {
            try
            {
                return Success(SecurityUsBL.ObtenerUsMenusPorTipoYNodoPadreEsNull(Tipo));
            }
            catch (Exception ex)
            {
                return Failure<List<UsMenuDto>>(ex);
            }
        }

        [HttpGet("ObtenerMenuPrioridadPorMenuNodoPadre")]
        public ErrorDto<int?> ObtenerMenuPrioridadPorMenuNodoPadre(int NodoPadre)
        {
            try
            {
                return Success(SecurityUsBL.ObtenerMenuPrioridadPorMenuNodoPadre(NodoPadre));
            }
            catch (Exception ex)
            {
                return Failure<int?>(ex);
            }
        }

        [HttpGet("ObtenerUsModulosOrdenadosPorTipo")]
        public ErrorDto<UsModuloDto?> ObtenerUsModulosOrdenadosPorTipo(string Tipo)
        {
            try
            {
                return Success(SecurityUsBL.ObtenerUsModulosOrdenadosPorTipo(Tipo));
            }
            catch (Exception ex)
            {
                return Failure<UsModuloDto?>(ex);
            }
        }

        [HttpGet("ObtenerMenuNodoPorNodoPadreYPrioridad")]
        public ErrorDto<int?> ObtenerMenuNodoPorNodoPadreYPrioridad(int NodoPadre, int Prioridad)
        {
            try
            {
                return Success(SecurityUsBL.ObtenerMenuNodoPorNodoPadreYPrioridad(NodoPadre, Prioridad));
            }
            catch (Exception ex)
            {
                return Failure<int?>(ex);
            }
        }

        [HttpGet("EliminarUnMenuPorNodoPadre")]
        public ErrorDto<int?> EliminarUnMenuPorNodoPadre(int NodoPadre)
        {
            try
            {
                return Success(SecurityUsBL.EliminarUnMenuPorNodoPadre(NodoPadre));
            }
            catch (Exception ex)
            {
                return Failure<int?>(ex);
            }
        }

        [HttpGet("EliminarUsMenusPorMenuNodo")]
        public ErrorDto<int?> EliminarUsMenusPorMenuNodo(int MenuNodo)
        {
            try
            {
                return Success(SecurityUsBL.EliminarUsMenusPorMenuNodo(MenuNodo));
            }
            catch (Exception ex)
            {
                return Failure<int?>(ex);
            }
        }

        [HttpGet("EliminarTodosLosMenusPorNodoPadre")]
        public ErrorDto<int?> EliminarTodosLosMenusPorNodoPadre(int NodoPadre)
        {
            try
            {
                return Success(SecurityUsBL.EliminarTodosLosMenusPorNodoPadre(NodoPadre));
            }
            catch (Exception ex)
            {
                return Failure<int?>(ex);
            }
        }

        [HttpGet("ObtenerMenuNodoPorMenuFormulario")]
        public ErrorDto<int?> ObtenerMenuNodoPorMenuFormulario(string Formulario)
        {
            try
            {
                return Success(SecurityUsBL.ObtenerMenuNodoPorMenuFormulario(Formulario));
            }
            catch (Exception ex)
            {
                return Failure<int?>(ex);
            }
        }

        [HttpGet("ObtenerUsFormularioPorFormulario")]
        public ErrorDto<UsFormularioDto> ObtenerUsFormularioPorFormulario(string Formulario)
        {
            try
            {
                return Success(SecurityUsBL.ObtenerUsFormularioPorFormulario(Formulario));
            }
            catch (Exception ex)
            {
                return Failure<UsFormularioDto>(ex);
            }
        }

        [HttpGet("ObtenerUsMenuPorMenuNodo")]
        public ErrorDto<UsMenuDto> ObtenerUsMenuPorMenuNodo(int MenuNodo)
        {
            try
            {
                return Success(SecurityUsBL.ObtenerUsMenuPorMenuNodo(MenuNodo));
            }
            catch (Exception ex)
            {
                return Failure<UsMenuDto>(ex);
            }
        }

        [HttpPost("ActualizarUsMenu")]
        public ErrorDto<ResultadoCrearYEditarUsMenuDto?> ActualizarUsMenu(UsMenuDto Info)
        {
            try
            {
                return Success(SecurityUsBL.ActualizarUsMenu(Info));
            }
            catch (Exception ex)
            {
                return Failure<ResultadoCrearYEditarUsMenuDto?>(ex);
            }
        }

        [HttpPost("CrearUsMenu")]
        public ErrorDto<ResultadoCrearYEditarUsMenuDto?> CrearUsMenu(UsMenuDto Info)
        {
            try
            {
                return Success(SecurityUsBL.CrearUsMenu(Info));
            }
            catch (Exception ex)
            {
                return Failure<ResultadoCrearYEditarUsMenuDto?>(ex);
            }
        }

        [HttpPost("ObtenerUsMenu_IconosWeb")]
        public ErrorDto<List<UsIconWeb>> ObtenerUsMenu_IconosWeb()
        {
            try
            {
                return Success(SecurityUsBL.ObtenerUsMenu_IconosWeb());
            }
            catch (Exception ex)
            {
                return Failure<List<UsIconWeb>>(ex);
            }
        }

    }
}
