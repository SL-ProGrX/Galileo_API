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
        [HttpGet("AF_LiquidacionAsiento_Obtener")]
        public ErrorDto<List<LiquidacionAsientoModel>> AF_LiquidacionAsiento_Obtener(int CodEmpresa, string filtros)
        {
            return _BL.AF_LiquidacionAsiento_Obtener(CodEmpresa, filtros);
        }

        [Authorize]
        [HttpPost("Af_LiquidacionAsiento_Generar")]
        public ErrorDto Af_LiquidacionAsiento_Generar(int CodEmpresa, string usuario, string filtros, List<LiquidacionAsientoModel> liquidaciones)
        {
            return _BL.Af_LiquidacionAsiento_Generar(CodEmpresa, usuario, filtros, liquidaciones);
        }
    }
}