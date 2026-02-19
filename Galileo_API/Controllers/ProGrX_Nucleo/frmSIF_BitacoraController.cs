using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.SIF;

namespace Galileo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmSifBitacoraController : ControllerBase
    {
        private readonly FrmSifBitacoraBL _bl;
        public FrmSifBitacoraController(IConfiguration config)
        {
            _bl = new FrmSifBitacoraBL(config);
        }

        [HttpGet("Bitacora_Obtener")]
        [Authorize]
        public ErrorDto<SifBitacoraLista> Bitacora_Obtener(int codEmpresa, string filtros)
        {
            return _bl.Bitacora_Obtener(codEmpresa, filtros);
        }

        [HttpGet("BitacoraModulos_Obtener")]
        [Authorize]
        public ErrorDto<List<DropDownListaGenericaModel>> BitacoraModulos_Obtener(int codEmpresa)
        {
            return _bl.BitacoraModulos_Obtener(codEmpresa);
        }

        [HttpGet("BitacoraUsuarios_Obtener")]
        [Authorize]
        public ErrorDto<List<DropDownListaGenericaModel>> BitacoraUsuarios_Obtener(int CodEmpresa)
        {
            return _bl.BitacoraUsuarios_Obtener(CodEmpresa);
        }

    }
}