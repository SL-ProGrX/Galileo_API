
using Galileo.Models;
using Galileo.Models.ERROR; 
using Galileo_API.BusinessLogic.ProGrX.CuentasxCobrar; 
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static Galileo_API.Models.ProGrX.CuentasxCobrar.FrmCxCBitacoraEspecialModels;

namespace Galileo_API.Controllers.ProGrX.CuentasxCobrar
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmCxCBitacoraEspecialController : ControllerBase
    {
        private readonly FrmCxCBitacoraEspecialBL _bl;

        public FrmCxCBitacoraEspecialController(IConfiguration config)
            => _bl = new FrmCxCBitacoraEspecialBL(config);

        [Authorize]
        [HttpGet("CxCBitacoraEspecialMovimientos_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CxCBitacoraEspecialMovimientos_Obtener(int codEmpresa)
        {

            return _bl.CxCBitacoraEspecialMovimientos_Obtener(codEmpresa);
        }

        [Authorize]
        [HttpGet("CxCBitacoraEspecialPersonas_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CxCBitacoraEspecialPersonas_Obtener(int codEmpresa)
        {

            return _bl.CxCBitacoraEspecialPersonas_Obtener(codEmpresa);
        }

        [Authorize]
        [HttpGet("CxCBitacoraEspecialUsuarios_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CxCBitacoraEspecialUsuarios_Obtener(int codEmpresa)
        {

            return _bl.CxCBitacoraEspecialUsuarios_Obtener(codEmpresa);
        }

        [Authorize]
        [HttpPost("CxCBitacoraEspecial_Actualizar")]
        public ErrorDto CxCBitacoraEspecial_Actualizar(int codEmpresa, string usuario, int idBitacora)
        {
            return _bl.CxCBitacoraEspecial_Actualizar(codEmpresa, usuario, idBitacora);
        }

        [Authorize]
        [HttpGet("CxCBitacoraEspecialBuscar")]
        public ErrorDto<BitacoraEspeciaLista> CxCBitacoraEspecialBuscar(int codEmpresa, bool esExportar, [FromQuery] BitacoraEspeciaFiltros filtros)
        {
            return _bl.CxCBitacoraEspecialBuscar(codEmpresa, filtros, esExportar);
        }
        
    }

}