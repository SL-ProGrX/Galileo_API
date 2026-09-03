using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic;
using Galileo.Models.ERROR;
using Galileo.Models.INV;

namespace Galileo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmInvParametrosController : ControllerBase
    {
        private readonly FrmInvParametrosBl _bl;
        public FrmInvParametrosController(IConfiguration config)
        {
            _bl = new FrmInvParametrosBl(config);
        }

        [HttpGet("Parametros_Obtener")]
        public ErrorDto<ParametrosGenDto?> Parametros_Obtener(int CodEmpresa)
        {
            return _bl.Parametros_Obtener(CodEmpresa);
        }

        [HttpGet("obtenerContabilidades")]
        public ErrorDto<List<CntXContaDto>> obtenerContabilidades(int CodEmpresa)
        {
            return _bl.obtenerContabilidades(CodEmpresa);
        }

        [HttpPost("actualizar_Parametros")]
        public ErrorDto actualizar_Parametros(int CodEmpresa, ParametrosGenDto data)
        {
            return _bl.actualizar_Parametros(CodEmpresa, data);
        }

        [HttpGet("Obtener_DescripcionesCuenta")]
        public ErrorDto<List<DescripcionCuentasDto>> Obtener_DescripcionesCuenta(int CodEmpresa)
        {
            return _bl.Obtener_DescripcionesCuenta(CodEmpresa);
        }

        [HttpGet("Obtener_DescripcionesAsiento")]
        public ErrorDto<List<DescripcionTipoAsientoDto>> Obtener_DescripcionesAsiento(int CodEmpresa)
        {
            return _bl.Obtener_DescripcionesAsiento(CodEmpresa);
        }

        [HttpGet("Asientos_Obtener")]
        public ErrorDto<List<DescripcionTipoAsientoDto>> Asientos_Obtener(int CodEmpresa)
        {
            return _bl.Asientos_Obtener(CodEmpresa);
        }
    }
}