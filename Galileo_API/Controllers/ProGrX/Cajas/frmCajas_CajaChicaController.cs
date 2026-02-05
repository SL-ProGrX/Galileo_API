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
        public ErrorDto<List<CajasCajaChicaServiciosDto>> Cajas_CajaChicaServicios_Buscar(
              int codEmpresa,
              string codCaja,
              string servicioBusqueda)
        {
            return _bl.Cajas_CajaChicaServicios_Buscar(
                codEmpresa,
                codCaja,
                servicioBusqueda);
        }

        [HttpGet("Cajas_CajaChicaDocumentos_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> Cajas_CajaChicaDocumentos_Obtener(
                int codEmpresa,
                string codCaja)
        {
            return _bl.Cajas_CajaChicaDocumentos_Obtener(
                codEmpresa,
                codCaja);
        }

        [HttpGet("Cajas__CajaChicaDivisas_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> Cajas__CajaChicaDivisas_Obtener(
            int codEmpresa,
            int codContabilidad)
        {
            return _bl.Cajas__CajaChicaDivisas_Obtener(
                codEmpresa,
                codContabilidad);
        }

        [HttpGet("Cntx_Divisa_TipoCambio_Obtener")]
        public ErrorDto<CajasCajaChicaTipoCambioRsDto> Cntx_Divisa_TipoCambio_Obtener(
                int codEmpresa,
                int codContabilidad,
                string codDivisa)
        {
            return _bl.Cntx_Divisa_TipoCambio_Obtener(
                codEmpresa,
                codContabilidad,
                codDivisa);
        }

        [HttpGet("Socios_Buscar")]
        public ErrorDto<List<CajasCajaChicaSociosBusquedaRsDto>> Socios_Buscar(
               int codEmpresa,
               string? filtroNombre)
        {
            return _bl.Socios_Buscar(
                codEmpresa,
                filtroNombre);
        }

        [HttpPost("Cajas_Retiro_Aplicar_Db")]
        public ErrorDto<CajasCajaChicaAplicarDbResponseDto> Cajas_Retiro_Aplicar_Db(
                 CajasCajaChicaAplicarDbRequestDto req)
        {
            return _bl.Cajas_Retiro_Aplicar_Db(
                req);
        }
    }
}
