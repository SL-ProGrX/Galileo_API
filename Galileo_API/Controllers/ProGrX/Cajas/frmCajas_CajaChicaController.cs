using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX.Cajas;
using Galileo_API.Models.ProGrX.Cajas;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX.Cajas
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FrmCajasCajaChicaController : ControllerBase
    {
        private readonly FrmCajasCajaChicaBL _bl;
        public FrmCajasCajaChicaController(IConfiguration config)
        {
            _bl = new FrmCajasCajaChicaBL(config);
        }

        [HttpGet("Cajas_CajaChicaServicios_Buscar")]
        public ErrorDto<List<CajasCajaChicaServiciosDto>> Cajas_CajaChicaServicios_Buscar(int codEmpresa,string codCaja,string servicioBusqueda)
        {
            return _bl.Cajas_CajaChicaServicios_Buscar(
                codEmpresa,
                codCaja,
                servicioBusqueda);
        }

        [HttpGet("Cajas_CajaChicaDocumentos_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> Cajas_CajaChicaDocumentos_Obtener(int codEmpresa,string codCaja)
        {
            return _bl.Cajas_CajaChicaDocumentos_Obtener(
                codEmpresa,
                codCaja);
        }

        [HttpGet("Cajas_CajaChicaDivisas_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> Cajas_CajaChicaDivisas_Obtener(int codEmpresa,int codContabilidad)
        {
            return _bl.Cajas_CajaChicaDivisas_Obtener(
                codEmpresa,
                codContabilidad);
        }

        [HttpGet("Cajas_CajaChicaTipoCambio_Obtener")]
        public ErrorDto<CajasCajaChicaTipoCambioRsDto> Cajas_CajaChicaTipoCambio_Obtener(int codEmpresa,int codContabilidad,string codDivisa)
        {
            return _bl.Cajas_CajaChicaTipoCambio_Obtener(
                codEmpresa,
                codContabilidad,
                codDivisa);
        }

        [HttpGet("Cajas_CajaChicaSocios_Buscar")]
        public ErrorDto<List<CajasCajaChicaSociosBusquedaRsDto>> Cajas_CajaChicaSocios_Buscar(int codEmpresa,string? filtroNombre)
        {
            return _bl.Cajas_CajaChicaSocios_Buscar(
                codEmpresa,
                filtroNombre);
        }

        [HttpPost("Cajas_CajaChicaRetiro_Aplicar")]
        public ErrorDto<CajasCajaChicaAplicarDbResponseDto> Cajas_CajaChicaRetiro_Aplicar(CajasCajaChicaAplicarDbRequestDto req)
        {
            return _bl.Cajas_CajaChicaRetiro_Aplicar(
                req);
        }
    }
}