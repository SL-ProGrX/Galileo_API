using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Procesos;
using Galileo_API.BusinessLogic.ProGrX.Patrimonio;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX.Patrimonio
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmAHExcedentesEncargadosController : ControllerBase
    {
        private readonly FrmAHExcedentesEncargadosBL _bl;

        public FrmAHExcedentesEncargadosController(
            IConfiguration config)
        {
            _bl = new FrmAHExcedentesEncargadosBL(config);
        }

        /// <summary>
        /// Obtiene la lista completa de encargados de excedentes.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        [Authorize]
        [HttpGet("AH_Excedentes_Encargados_Lista_Obtener")]
        public ErrorDto<List<EncargadoExcedenteDto>>AH_Excedentes_Encargados_Lista_Obtener(int CodEmpresa)
        {
            return _bl.AH_Excedentes_Encargados_Lista_Obtener(CodEmpresa);
        }

        /// <summary>
        /// Registra o actualiza un encargado de excedentes.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="encargado"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost("AH_Excedentes_Encargados_Guardar")]
        public ErrorDto AH_Excedentes_Encargados_Guardar( int CodEmpresa,string usuario,[FromBody] EncargadoExcedenteDto encargado)
        {
            return _bl.AH_Excedentes_Encargados_Guardar(CodEmpresa,usuario,encargado);
        }

        /// <summary>
        /// Elimina un encargado de excedentes.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuarioEncargado"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        [Authorize]
        [HttpDelete("AH_Excedentes_Encargados_Eliminar")]
        public ErrorDto AH_Excedentes_Encargados_Eliminar(int CodEmpresa,string usuarioEncargado,string usuario)
        {
            return _bl.AH_Excedentes_Encargados_Eliminar(CodEmpresa,usuarioEncargado,usuario);
        }

        /// <summary>
        /// Obtiene los usuarios activos disponibles para seleccionar.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        [Authorize]
        [HttpGet("AH_Excedentes_Encargados_Usuarios_Dropdown_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>>AH_Excedentes_Encargados_Usuarios_Dropdown_Obtener(int CodEmpresa)
        {
            return _bl.AH_Excedentes_Encargados_Usuarios_Dropdown_Obtener(CodEmpresa);
        }
    }
}