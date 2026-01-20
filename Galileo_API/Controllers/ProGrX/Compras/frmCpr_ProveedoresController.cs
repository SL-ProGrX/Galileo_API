using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic;
using Galileo.Models.CPR;
using Galileo.Models.ERROR;

namespace Galileo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmCprProveedoresController : ControllerBase
    {
        private readonly FrmCprProveedoresBL BL_Cpr_Proveedores;
        public FrmCprProveedoresController(IConfiguration config)
        {
            BL_Cpr_Proveedores = new FrmCprProveedoresBL(config);
        }

        [Authorize]
        [HttpGet("CprProveedores_Importar")]
        public ErrorDto CprProveedores_Importar(int CodEmpresa)
        {
            return BL_Cpr_Proveedores.CprProveedores_Importar(CodEmpresa);
        }

        [HttpGet("CprProveedor_Scroll")]
        public ErrorDto<CprProveedoresDto> CprProveedor_Scroll(int CodEmpresa, int scroll, string? codigo)
        {
            return BL_Cpr_Proveedores.CprProveedor_Scroll(CodEmpresa, scroll, codigo);
        }

        [Authorize]
        [HttpGet("CprProveedoresLista_Obtener")]
        public ErrorDto<CprProveedoresLista> CprProveedoresLista_Obtener(int CodEmpresa, string filtros)
        {
            return BL_Cpr_Proveedores.CprProveedoresLista_Obtener(CodEmpresa, filtros);
        }

        [Authorize]
        [HttpGet("CprProveedores_Obtener")]
        public ErrorDto<CprProveedoresDto> CprProveedores_Obtener(int CodEmpresa, string codigo)
        {
            return BL_Cpr_Proveedores.CprProveedores_Obtener(CodEmpresa, codigo);
        }

        [Authorize]
        [HttpPost("CprProveedores_Guardar")]
        public ErrorDto CprProveedores_Guardar(int CodEmpresa, bool vEdita, CprProveedoresDto proveedor)
        {
            return BL_Cpr_Proveedores.CprProveedores_Guardar(CodEmpresa, vEdita, proveedor);
        }

        [Authorize]
        [HttpDelete("CprProveedores_Eliminar")]
        public ErrorDto CprProveedores_Eliminar(int CodEmpresa, string codigo)
        {
            return BL_Cpr_Proveedores.CprProveedores_Eliminar(CodEmpresa, codigo);
        }

        [Authorize]
        [HttpGet("CprProveedorPuntaje_Obtener")]
        public ErrorDto<float> CprProveedorPuntaje_Obtener(int CodEmpresa, string codigo)
        {
            return BL_Cpr_Proveedores.CprProveedorPuntaje_Obtener(CodEmpresa, codigo);
        }

        [Authorize]
        [HttpGet("CprProveedoreBitacoraPuntaje_Obtener")]
        public ErrorDto<List<CprProveedorBitacoraData>> CprProveedoreBitacoraPuntaje_Obtener(int CodEmpresa, string codigo)
        {
            return BL_Cpr_Proveedores.CprProveedoreBitacoraPuntaje(CodEmpresa, codigo);
        }
    }
}