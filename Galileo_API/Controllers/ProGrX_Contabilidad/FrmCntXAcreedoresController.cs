using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX_ARF;
using Galileo_API.Models.ProGrX_Contabilidad;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX_ARF
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmArfAcreedoresController : ControllerBase
    {
        private readonly FrmArfAcreedoresBl _bl;

        public FrmArfAcreedoresController(IConfiguration config)
        {
            _bl = new FrmArfAcreedoresBl(config);
        }

        [Authorize]
        [HttpGet("Consultar")]
        public ErrorDto<ArfAcreedorDto?> Consultar(int codEmpresa, int codigo)
        {
            return _bl.Consultar(codEmpresa, codigo);
        }

        [Authorize]
        [HttpPost("Insertar")]
        public ErrorDto<int> Insertar(int codEmpresa, ArfAcreedorDto modelo)
        {
            return _bl.Insertar(codEmpresa, modelo);
        }

        [Authorize]
        [HttpPut("Actualizar")]
        public ErrorDto<int> Actualizar(int codEmpresa, ArfAcreedorDto modelo)
        {
            return _bl.Actualizar(codEmpresa, modelo);
        }

        [Authorize]
        [HttpDelete("Borrar")]
        public ErrorDto<int> Borrar(int codEmpresa, int codigo)
        {
            return _bl.Borrar(codEmpresa, codigo);
        }

        [Authorize]
        [HttpGet("Scroll")]
        public ErrorDto<int?> Scroll(int codEmpresa, int? codigoActual, int direccion)
        {
            return _bl.Scroll(codEmpresa, codigoActual, direccion);
        }

        [Authorize]
        [HttpGet("CuentasBancarias")]
        public ErrorDto<List<CuentaBancariaAcreedorDto>> CuentasBancarias(
            int codEmpresa,
            string identificacion)
        {
            return _bl.CuentasBancarias(codEmpresa, identificacion);
        }

        [Authorize]
        [HttpGet("ObtenerProvincias")]
        public ErrorDto<List<ProvinciaAcreedorDto>> ObtenerProvincias(int codEmpresa)
        {
            return _bl.ObtenerProvincias(codEmpresa);
        }

        [Authorize]
        [HttpGet("ObtenerCantones")]
        public ErrorDto<List<CantonAcreedorDto>> ObtenerCantones(int codEmpresa, string provincia)
        {
            return _bl.ObtenerCantones(codEmpresa, provincia);
        }

        [Authorize]
        [HttpGet("ObtenerDistritos")]
        public ErrorDto<List<DistritoAcreedorDto>> ObtenerDistritos(int codEmpresa, string provincia, string canton)
        {
            return _bl.ObtenerDistritos(codEmpresa, provincia, canton);
        }

        [Authorize]
        [HttpGet("ObtenerTiposIdentificacion")]
        public ErrorDto<List<DropDownListaGenericaModel>> ObtenerTiposIdentificacion(int codEmpresa)
        {
            return _bl.ObtenerTiposIdentificacion(codEmpresa);
        }

        [Authorize]
        [HttpGet("BuscarAcreedores")]
        public ErrorDto<List<ArfAcreedorDto>> BuscarAcreedores(int codEmpresa, string? filtro)
        {
            return _bl.BuscarAcreedores(codEmpresa, filtro);
        }

        [Authorize]
        [HttpGet("BuscarProveedores")]
        public ErrorDto<List<DropDownListaGenericaModel>> BuscarProveedores(int codEmpresa, string? filtro)
        {
            return _bl.BuscarProveedores(codEmpresa, filtro);
        }

        [Authorize]
        [HttpGet("ObtenerBancos")]
        public ErrorDto<List<DropDownListaGenericaModel>> ObtenerBancos(int codEmpresa,string usuario)
        {
            return _bl.ObtenerBancos(codEmpresa, usuario);
        }



    }
}
