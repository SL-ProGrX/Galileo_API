using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo_API.BusinessLogic.ProGrX.Cajas;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX.Cajas;

namespace Galileo_API.Controllers.ProGrX.Cajas
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmCajasDatosAsociadosController : ControllerBase
    {
        private readonly FrmCajasDatosAsociadosBl BL_Cajas_DatosAsociados;
        public FrmCajasDatosAsociadosController(IConfiguration config)
        {
            BL_Cajas_DatosAsociados = new FrmCajasDatosAsociadosBl(config);
        }

        [Authorize]
        [HttpGet("Cajas_Consulta_Creditos")]
        public ErrorDto<List<CajasCreditoDto>> Cajas_Consulta_Creditos(
    int codEmpresa, string cedula)
        {
            return BL_Cajas_DatosAsociados.Cajas_Consulta_Creditos(codEmpresa, cedula);
        }

        [Authorize]
        [HttpGet("Cajas_Consulta_Fondos")]
        public ErrorDto<List<CajasFondosDto>> Cajas_Consulta_Fondos(
            int codEmpresa, string cedula, string usuario)
        {
            return BL_Cajas_DatosAsociados.Cajas_Consulta_Fondos(codEmpresa, cedula, usuario);
        }

        [Authorize]
        [HttpGet("Cajas_Consulta_CxC")]
        public ErrorDto<List<CajasCxcDto>> Cajas_Consulta_CxC(
            int codEmpresa, string cedula)
        {
            return BL_Cajas_DatosAsociados.Cajas_Consulta_CxC(codEmpresa, cedula);
        }

        [Authorize]
        [HttpGet("Cajas_Consulta_Servicios")]
        public ErrorDto<List<CajasServiciosDto>> Cajas_Consulta_Servicios(
            int codEmpresa, string cedula)
        {
            return BL_Cajas_DatosAsociados.Cajas_Consulta_Servicios(codEmpresa, cedula);
        }

        [Authorize]
        [HttpGet("Cajas_Consulta_SaldosFavor")]
        public ErrorDto<List<CajasSaldoFavorDto>> Cajas_Consulta_SaldosFavor(
            int codEmpresa, string cedula, bool liquidados)
        {
            return BL_Cajas_DatosAsociados.Cajas_Consulta_SaldosFavor(codEmpresa, cedula, liquidados);
        }

        [Authorize]
        [HttpGet("Cajas_Consulta_RecibosMultiples")]
        public ErrorDto<List<CajasReciboMultipleDto>> Cajas_Consulta_RecibosMultiples(
            int codEmpresa, string cedula)
        {
            return BL_Cajas_DatosAsociados.Cajas_Consulta_RecibosMultiples(codEmpresa, cedula);
        }

    }
}