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
    public class FrmFndPagoComisionController : ControllerBase
    {
        private readonly FrmFndPagoComisionBl _bl;

        public FrmFndPagoComisionController(IConfiguration config)
        {
            _bl = new FrmFndPagoComisionBl(config);
        }

        [Authorize]
        [HttpGet("FND_PagoComision_Bancos_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> FND_PagoComision_Bancos_Obtener(int CodEmpresa, string Usuario)
        {
            return _bl.FND_PagoComision_Bancos_Obtener(CodEmpresa, Usuario);
        }

        [Authorize]
        [HttpGet("FND_PagoComision_Obtener")]
        public ErrorDto<List<FndPagoComisionVendedorData>> FND_PagoComision_Obtener(int CodEmpresa, string Filtros)
        {
            return _bl.FND_PagoComision_Obtener(CodEmpresa, Filtros);
        }

        [Authorize]
        [HttpPost("FND_PagoComision_Generar")]
        public ErrorDto FND_PagoComision_Generar(int CodEmpresa, string Filtros, List<FndPagoComisionVendedorData> Vendedores)
        {
            return _bl.FND_PagoComision_Generar(CodEmpresa, Filtros, Vendedores);
        }
    }
}


