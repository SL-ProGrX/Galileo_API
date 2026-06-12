using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX.Creditos;
using Galileo_API.Models.ProGrX.Credito;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX.Creditos
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FrmCrCambioTasasController : ControllerBase
    {
        private readonly FrmCrCambioTasasBl _bl;

        public FrmCrCambioTasasController(IConfiguration config)
        {
            _bl = new FrmCrCambioTasasBl(config);
        }

        [HttpGet("CR_CambioTasas_Inicializar")]
        public ErrorDto<CrCambioTasasInicialResponse> CR_CambioTasas_Inicializar(int codEmpresa, string usuario)
        {
            return _bl.CR_CambioTasas_Inicializar(codEmpresa, usuario);
        }

        [HttpGet("CR_CambioTasas_Deductoras")]
        public ErrorDto<List<DropDownListaGenericaModel>> CR_CambioTasas_Deductoras(
            int codEmpresa,
            int? codInstitucion)
        {
            return _bl.CR_CambioTasas_Deductoras(codEmpresa, codInstitucion);
        }

        [HttpGet("CR_CambioTasas_Catalogos_Linea")]
        public ErrorDto<CrCambioTasasCatalogosLineaResponse> CR_CambioTasas_Catalogos_Linea(
            int codEmpresa,
            string? codigo,
            bool todas = true)
        {
            return _bl.CR_CambioTasas_Catalogos_Linea(codEmpresa, codigo, todas);
        }

        [HttpGet("CR_CambioTasas_Lineas_F4")]
        public ErrorDto<List<DropDownListaGenericaModel>> CR_CambioTasas_Lineas_F4(int codEmpresa)
        {
            return _bl.CR_CambioTasas_Lineas_F4(codEmpresa);
        }

        [HttpGet("CR_CambioTasas_Linea_Describir")]
        public ErrorDto<string> CR_CambioTasas_Linea_Describir(int codEmpresa, string codigo)
        {
            return _bl.CR_CambioTasas_Linea_Describir(codEmpresa, codigo);
        }

        [HttpPost("CR_CambioTasas_Consultar")]
        public ErrorDto<CrCambioTasasConsultaResponse> CR_CambioTasas_Consultar(
            int codEmpresa,
            CrCambioTasasConsultaRequest request)
        {
            return _bl.CR_CambioTasas_Consultar(codEmpresa, request);
        }

        [HttpPost("CR_CambioTasas_Aplicar")]
        public ErrorDto CR_CambioTasas_Aplicar(
            int codEmpresa,
            CrCambioTasasAplicarRequest request)
        {
            return _bl.CR_CambioTasas_Aplicar(codEmpresa, request);
        }
    }
}
