using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic.ProGrX.Clientes;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Clientes;

namespace Galileo.Controllers.ProGrX.Clientes
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmAfLiquidacionAsientosController : ControllerBase
    {
        private readonly FrmAfLiquidacionAsientosBL _BL;

        public FrmAfLiquidacionAsientosController(IConfiguration config)
        {
            _BL = new FrmAfLiquidacionAsientosBL(config);
        }

        [Authorize]
        [HttpGet("AF_LiqAsientosTipo_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> AF_LiqAsientosTipo_Obtener(int CodEmpresa, string accion)
        {
            return _BL.AF_LiqAsientosTipo_Obtener(CodEmpresa, accion);
        }

        [Authorize]
        [HttpGet("AF_LiqAsientosToken_Obtener")]
        public ErrorDto<List<TokenConsultaModel>> AF_LiqAsientosToken_Obtener(int CodEmpresa, string usuario)
        {
            return _BL.AF_LiqAsientosToken_Obtener(CodEmpresa, usuario);
        }

        [Authorize]
        [HttpPost("AF_LiqAsientoToken_Nuevo")]
        public ErrorDto AF_LiqAsientoToken_Nuevo(int CodEmpresa, string usuario)
        {
            return _BL.AF_LiqAsientoToken_Nuevo(CodEmpresa, usuario);
        }

        [Authorize]
        [HttpPost("Af_LiquidacionAsientos_Generar")]
        public ErrorDto<AfLiquidacionAsientosGenerarResponse> Af_LiquidacionAsientos_Generar(
             int CodEmpresa,
             AfLiquidacionAsientosGenerarRequest request)
        {
            return _BL.Af_LiquidacionAsientos_Generar(CodEmpresa, request);
        }

        [Authorize]
        [HttpPost("Af_LiquidacionAsientos_Buscar")]
        public ErrorDto<List<AfLiquidacionAsientosRowDto>> Af_LiquidacionAsientos_Buscar(
                int CodEmpresa,
                AfLiquidacionAsientosBuscarRequest request)
        {
            return _BL.Af_LiquidacionAsientos_Buscar(CodEmpresa, request);
        }

        [Authorize]
        [HttpPost("Af_LiquidacionAsientos_Bancos")]
        public ErrorDto<List<DropDownListaGenericaModel>> Af_LiquidacionAsientos_Bancos(
              int CodEmpresa,
              AfLiquidacionFiltroRequest request)
        {
            return _BL.Af_LiquidacionAsientos_Bancos(CodEmpresa, request);
        }

        [Authorize]
        [HttpPost("Af_LiquidacionAsientos_Usuarios")]
        public ErrorDto<List<DropDownListaGenericaModel>> Af_LiquidacionAsientos_Usuarios(
               int CodEmpresa,
               AfLiquidacionFiltroRequest request)
        {
            return _BL.Af_LiquidacionAsientos_Usuarios(CodEmpresa, request);
        }

        [Authorize]
        [HttpPost("Af_LiquidacionAsientos_Tokens")]
        public ErrorDto<List<DropDownListaGenericaModel>> Af_LiquidacionAsientos_Tokens(
                int CodEmpresa,
                AfLiquidacionFiltroRequest request)
        {
            return _BL.Af_LiquidacionAsientos_Tokens(CodEmpresa, request);
        }

        [Authorize]
        [HttpPost("Af_LiquidacionAsientos_Oficinas")]
        public ErrorDto<List<DropDownListaGenericaModel>> Af_LiquidacionAsientos_Oficinas(
            int CodEmpresa,
            AfLiquidacionFiltroRequest request)
        {
            return _BL.Af_LiquidacionAsientos_Oficinas(CodEmpresa, request);
        }
    }
}