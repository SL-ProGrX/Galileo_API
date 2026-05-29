using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.BusinessLogic.ProGrX.Clientes;
using Galileo.Models.ProGrX.Clientes;

namespace Galileo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmAFRemesasIngresosController : ControllerBase
    {
        private readonly FrmAFRemesasIngresosBL _bl;

        public FrmAFRemesasIngresosController(IConfiguration config)
        {
            _bl = new FrmAFRemesasIngresosBL(config);
        }

        [Authorize]
        [HttpGet("AFI_Remesas_Obtener")]
        public ErrorDto<List<AdiRemesaIngDto>> AFI_Remesas_Obtener(int CodEmpresa)
        {
            return _bl.AFI_Remesas_Obtener(CodEmpresa);
        }

        [Authorize]
        [HttpDelete("AFI_Remesa_Eliminar")]
        public ErrorDto AFI_Remesa_Eliminar(int CodEmpresa, string CodRemesa)
        {
            return _bl.AFI_Remesa_Eliminar(CodEmpresa, CodRemesa);
        }

        [Authorize]
        [HttpPost("AFI_Remesa_Registrar")]
        public ErrorDto AFI_Remesa_Registrar(int CodEmpresa, AdiRemesaIngRequestDto request)
        {
            return _bl.AFI_Remesa_Registrar(CodEmpresa, request);
        }

        [Authorize]
        [HttpGet("AF_RemesaAbiertas_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> AF_RemesaAbiertas_Obtener(int CodEmpresa)
        {
            return _bl.AF_RemesaAbiertas_Obtener(CodEmpresa);
        }

        [Authorize]
        [HttpGet("AFI_IngresosPendientes_Obtener")]
        public ErrorDto<List<IngresosPendientesDto>> AFI_IngresosPendientes_Obtener(int CodEmpresa, string CodRemesa, string Oficina)
        {
            return _bl.AFI_IngresosPendientes_Obtener(CodEmpresa, CodRemesa, Oficina);
        }

        [Authorize]
        [HttpGet("AFI_Remesa_Cerrar")]
        public ErrorDto AFI_Remesa_Cerrar(int codEmpresa, int codRemesa)
        {
            return _bl.AFI_Remesa_Cerrar(codEmpresa, codRemesa);
        }

        [Authorize]
        [HttpGet("AFI_Remesa_Cargar")]
        public ErrorDto AFI_Remesa_Cargar(int codEmpresa, int codRemesa, List<int> ingresosSeleccionados)
        {
            return _bl.AFI_Remesa_Cargar(codEmpresa, codRemesa, ingresosSeleccionados);
        }


        [Authorize]
        [HttpGet("AFI_RemesaPorCedula_Obtener")]
        public ErrorDto<List<RemesaConsultaDto>> AFI_RemesaPorCedula_Obtener(int CodEmpresa, string Cedula)
        {
            return _bl.AFI_RemesaPorCedula_Obtener(CodEmpresa, Cedula);
        }
    }
}
