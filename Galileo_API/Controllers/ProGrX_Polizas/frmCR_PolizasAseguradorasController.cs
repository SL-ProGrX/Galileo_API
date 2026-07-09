using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Clientes;
using Galileo.Models.Security;
using Galileo_API.BusinessLogic.ProGrX_Polizas;
using Galileo_API.Models.ProGrX_Polizas;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX_Polizas
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmCRPolizasAseguradorasController : ControllerBase
    {
        private readonly FrmCRPolizasAseguradorasBl BL_CR_PolizasAseguradoras;
        public FrmCRPolizasAseguradorasController(IConfiguration config)
        {
            BL_CR_PolizasAseguradoras = new FrmCRPolizasAseguradorasBl(config);
        }

        [Authorize]
        [HttpGet("Consultar")]
        public ErrorDto<PolizaAseguradoraDto?> Consultar(int codEmpresa,string codigo)
        {
            return BL_CR_PolizasAseguradoras.Consultar(codEmpresa,codigo);
        }

        [HttpPost("Insertar")]
        public ErrorDto<int> Insertar(int codEmpresa, PolizaAseguradoraDto modelo)
        {
            return BL_CR_PolizasAseguradoras.Insertar(codEmpresa, modelo);
        }

        [HttpPut("Actualizar")]
        public ErrorDto<int> Actualizar(int codEmpresa,PolizaAseguradoraDto modelo)
        {
            return BL_CR_PolizasAseguradoras.Actualizar(codEmpresa, modelo);
        }

        [HttpDelete("Borrar")]
        public ErrorDto<int> Borrar(int codEmpresa,string codigo)
        {
            return BL_CR_PolizasAseguradoras.Borrar(codEmpresa, codigo);
        }

        [HttpGet("Scroll")]
        public ErrorDto<string?> Scroll(int codEmpresa,string codigoActual,int direccion)
        {
            return BL_CR_PolizasAseguradoras.Scroll(codEmpresa, codigoActual, direccion);
        }

        [HttpGet("CuentasBancarias")]
        public ErrorDto<List<CuentaBancariaDto>> CuentasBancarias(int codEmpresa,string cedula
)
        {
            return BL_CR_PolizasAseguradoras.CuentasBancarias(codEmpresa, cedula);
        }

        [HttpGet("ObtenerProvincias")]
        public ErrorDto<List<ProvinciaaseguradoraDto>> ObtenerProvincias(int codEmpresa)
        {
            return BL_CR_PolizasAseguradoras.ObtenerProvincias(codEmpresa);
        }

        [Authorize]
        [HttpGet("ObtenerCantones")]
        public ErrorDto<List<CantonaseguradoraDto>> ObtenerCantones(int codEmpresa,string provincia)
        {
            return BL_CR_PolizasAseguradoras.ObtenerCantones(codEmpresa, provincia);
        }

        [Authorize]
        [HttpGet("ObtenerDistritos")]
        public ErrorDto<List<DistritoaseguradoraDto>> ObtenerDistritos(int codEmpresa,string provincia,string canton)
        {
            return BL_CR_PolizasAseguradoras.ObtenerDistritos(codEmpresa, provincia, canton);
        }

        [Authorize]
        [HttpGet("Listar")]
        public ErrorDto<List<DropDownListaGenericaModel>> Listar(int codEmpresa)
        {
            return BL_CR_PolizasAseguradoras.Listar(codEmpresa);
        }

        [Authorize]
        [HttpGet("ObtenerBancos")]
        public ErrorDto<List<DropDownListaGenericaModel>> ObtenerBancos(int codEmpresa,string usuario)
        {
            return BL_CR_PolizasAseguradoras.ObtenerBancos(codEmpresa, usuario);
        }
        [Authorize]
        [HttpGet("BuscarRetenciones")]
        public ErrorDto<List<DropDownListaGenericaModel>> BuscarRetenciones(int codEmpresa, string? codigo = null)
        {
            return BL_CR_PolizasAseguradoras.BuscarRetenciones(codEmpresa, codigo);
        }

    }
}
