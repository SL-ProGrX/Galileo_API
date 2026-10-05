using Galileo.Models.ERROR;
using Galileo.Models.FSL;
using Galileo_API.BusinessLogic.ProGrX_BeneficiosFosol;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX_BeneficiosFosol
{
    [Route("api/frmFSL_TablaCoberturas")]
    [Authorize]
    [ApiController]
    public sealed class FrmFslTablaCoberturasController
        : ControllerBase
    {
        private readonly FrmFslTablaCoberturasBl _bl;

        public FrmFslTablaCoberturasController(
            IConfiguration config)
        {
            ArgumentNullException.ThrowIfNull(config);

            _bl = new FrmFslTablaCoberturasBl(config);
        }

        [HttpGet(
            "FSL_TablaCoberturas_Lista_Obtener")]
        public ErrorDto<
            FslListaPaginadaDto<FslTablaCoberturaDto>>
            FSL_TablaCoberturas_Lista_Obtener(
                int CodEmpresa,
                string filtros = "")
        {
            return _bl
                .FSL_TablaCoberturas_Lista_Obtener(
                    CodEmpresa,
                    filtros);
        }

        [HttpPost(
            "FSL_TablaCoberturas_Cobertura_Registrar")]
        public ErrorDto
            FSL_TablaCoberturas_Cobertura_Registrar(
                int CodEmpresa,
                [FromBody]
                FslTablaCoberturaGuardarRequest request)
        {
            return _bl
                .FSL_TablaCoberturas_Cobertura_Registrar(
                    CodEmpresa,
                    request);
        }

        [HttpPut(
            "FSL_TablaCoberturas_Cobertura_Actualizar")]
        public ErrorDto
            FSL_TablaCoberturas_Cobertura_Actualizar(
                int CodEmpresa,
                [FromBody]
                FslTablaCoberturaGuardarRequest request)
        {
            return _bl
                .FSL_TablaCoberturas_Cobertura_Actualizar(
                    CodEmpresa,
                    request);
        }

        [HttpDelete(
            "FSL_TablaCoberturas_Cobertura_Eliminar")]
        public ErrorDto
            FSL_TablaCoberturas_Cobertura_Eliminar(
                int CodEmpresa,
                string tipo,
                int linea,
                string usuario)
        {
            return _bl
                .FSL_TablaCoberturas_Cobertura_Eliminar(
                    CodEmpresa,
                    tipo,
                    linea,
                    usuario);
        }
    }
}