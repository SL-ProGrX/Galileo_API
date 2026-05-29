using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Clientes;
using Galileo.BusinessLogic.ProGrX.Clientes;

namespace Galileo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class frmAF_TelefonosController : ControllerBase
    {
        private readonly IConfiguration _config;
        FrmAFTelefonosBL _bl;

        public frmAF_TelefonosController(IConfiguration config)
        {
            _config = config;
            _bl = new FrmAFTelefonosBL(_config);
        }

        [Authorize]
        [HttpGet("AF_TiposTelefonos_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> AF_TiposTelefonos_Obtener(int CodEmpresa)
        {
            return _bl.AF_TiposTelefonos_Obtener(CodEmpresa);
        }

        [Authorize]
        [HttpGet("AF_Telefonos_ObtenerPorCedula")]
        public ErrorDto<List<AfTelefonosDto>> AF_Telefonos_ObtenerPorCedula(int CodEmpresa, string cedula)
        {
            return _bl.AF_Telefonos_ObtenerPorCedula(CodEmpresa, cedula);
        }

        [Authorize]
        [HttpPost("AF_Telefono_Insertar")]
        public ErrorDto AF_Telefono_Insertar(int CodEmpresa, string cedula, int tipoId, string numero, string ext, string contacto, string usuario)
        {
            return _bl.AF_Telefono_Insertar(CodEmpresa, cedula, tipoId, numero, ext, contacto, usuario);
        }

        [Authorize]
        [HttpPost("AF_Telefono_Actualizar")]
        public ErrorDto AF_Telefono_Actualizar(int CodEmpresa, int telefonoId, int tipoId, string numero, string ext, string contacto, string usuario)
        {
            return _bl.AF_Telefono_Actualizar(CodEmpresa, telefonoId, tipoId, numero, ext, contacto, usuario);
        }

        [Authorize]
        [HttpPost("AF_Telefono_Eliminar")]
        public ErrorDto AF_Telefono_Eliminar(int CodEmpresa, int telefonoId)
        {
            return _bl.AF_Telefono_Eliminar(CodEmpresa, telefonoId);
        }
    }
}