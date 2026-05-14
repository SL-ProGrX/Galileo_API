using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic.ProGrX.Fondos;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Fondos;

namespace Galileo.Controllers.ProGrX.Fondos
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmFndVendedoresController : ControllerBase
    {
        private readonly FrmFndVendedoresBl _BL;

        public FrmFndVendedoresController(IConfiguration config)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));
            _BL = new FrmFndVendedoresBl(config);
        }

        [Authorize]
        [HttpGet("SYS_CuentasBancarias_Obtener")]
        public ErrorDto<List<CuentaBancariaVendedorDto>> SYS_CuentasBancarias_Obtener(int CodEmpresa, string Cedula)
        {
            return _BL.SYS_CuentasBancarias_Obtener(CodEmpresa, Cedula);
        }

        [Authorize]
        [HttpGet("Fnd_Vendedores_Obtener")]
        public ErrorDto<FndVendedorDto> Fnd_Vendedores_Obtener(int CodEmpresa, int cod_vendedor)
        {
            return _BL.Fnd_Vendedores_Obtener(CodEmpresa, cod_vendedor);
        }

        [Authorize]
        [HttpGet("Fnd_Bancos_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> Fnd_Bancos_Obtener(int CodEmpresa, string Usuario)
        {
            return _BL.Fnd_Bancos_Obtener(CodEmpresa, Usuario);
        }

        [Authorize]
        [HttpGet("Fnd_Vendedores_Listas_Obtener")]
        public ErrorDto<List<FndVendedorListaDto>> Fnd_Vendedores_Listas_Obtener(int CodEmpresa)
        {
            return _BL.Fnd_Vendedores_Listas_Obtener(CodEmpresa);
        }

        [Authorize]
        [HttpPost("Fnd_Vendedores_Insertar")]
        public ErrorDto Fnd_Vendedores_Insertar(int CodEmpresa, FndVendedorDto request)
        {
            return _BL.Fnd_Vendedores_Insertar(CodEmpresa, request);
        }

        [Authorize]
        [HttpPut("Fnd_Vendedores_Actualizar")]
        public ErrorDto Fnd_Vendedores_Actualizar(int CodEmpresa, FndVendedorDto request)
        {
            return _BL.Fnd_Vendedores_Actualizar(CodEmpresa, request);
        }

        [Authorize]
        [HttpDelete("Fnd_Vendedores_Eliminar")]
        public ErrorDto Fnd_Vendedores_Eliminar(int CodEmpresa, int cod_vendedor)
        {
            return _BL.Fnd_Vendedores_Eliminar(CodEmpresa, cod_vendedor);
        }

        [Authorize]
        [HttpGet("FND_Vendedor_Scroll_Obtener")]
        public ErrorDto<FndVendedorDto> FND_Vendedor_Scroll_Obtener(int CodEmpresa, int cod_vendedor, int scrollCode)
        {
            return _BL.FND_Vendedor_Scroll_Obtener(CodEmpresa, cod_vendedor, scrollCode);
        }
    }
}