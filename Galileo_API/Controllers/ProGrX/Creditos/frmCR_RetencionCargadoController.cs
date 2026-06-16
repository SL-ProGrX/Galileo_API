using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX.Creditos;
using Galileo_API.Models.ProGrX.Creditos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX.Creditos
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class FrmCrRetencionCargadoController : ControllerBase
    {
        private readonly FrmCrRetencionCargadoBl _bl;

        public FrmCrRetencionCargadoController(IConfiguration config)
        {
            _bl = new FrmCrRetencionCargadoBl(config);
        }

        [HttpGet("CrRetencionCargado_Pantalla_Obtener")]
        public ErrorDto<CrRetencionCargadoPantallaData> CrRetencionCargado_Pantalla_Obtener(
            int codEmpresa,
            string usuario)
            => _bl.CrRetencionCargado_Pantalla_Obtener(codEmpresa, usuario);

        [HttpGet("CrRetencionCargado_Deductoras_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CrRetencionCargado_Deductoras_Obtener(
            int codEmpresa,
            int codInstitucion)
            => _bl.CrRetencionCargado_Deductoras_Obtener(codEmpresa, codInstitucion);

        [HttpGet("CrRetencionCargado_DeductoraDetalle_Obtener")]
        public ErrorDto<CrRetencionCargadoDeductoraDetalleData> CrRetencionCargado_DeductoraDetalle_Obtener(
            int codEmpresa,
            int codDeductora)
            => _bl.CrRetencionCargado_DeductoraDetalle_Obtener(codEmpresa, codDeductora);

        [HttpPost("CrRetencionCargado_Cargar")]
        public ErrorDto<CrRetencionCargadoCargaData> CrRetencionCargado_Cargar(
            int codEmpresa,
            string usuario,
            [FromBody] CrRetencionCargadoCargaRequest request)
            => _bl.CrRetencionCargado_Cargar(codEmpresa, usuario, request);

        [HttpPost("CrRetencionCargado_Aplicar")]
        public ErrorDto CrRetencionCargado_Aplicar(
            int codEmpresa,
            string usuario,
            [FromBody] CrRetencionCargadoAplicarRequest request)
            => _bl.CrRetencionCargado_Aplicar(codEmpresa, usuario, request);
    }
}