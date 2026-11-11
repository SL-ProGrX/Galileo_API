using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX_Polizas;
using Galileo_API.Models.ProGrX_Polizas;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX_Polizas
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FrmCrPolizaMacHogarController : ControllerBase
    {
        private readonly FrmCrPolizaMacHogarBL _bl;

        public FrmCrPolizaMacHogarController(IConfiguration config)
        {
            _bl = new FrmCrPolizaMacHogarBL(config);
        }

        [HttpGet("Cr_PolizaMacHogar_Polizas_Lista")]
        public ErrorDto<List<DropDownListaGenericaModel>> Cr_PolizaMacHogar_Polizas_Lista(int codEmpresa)
        {
            return _bl.Cr_PolizaMacHogar_Polizas_Lista(codEmpresa);
        }

        [HttpGet("fxFechaServidor")]
        public DateTime fxFechaServidor(int codEmpresa)
        {
            return _bl.fxFechaServidor(codEmpresa);
        }

        #region Envio

        [ValidateAntiForgeryToken]
        [HttpPost("Cr_PolizaMacHogar_Envio_Consulta")]
        public ErrorDto<List<CrPolizaMacHogarEnvioRow>> Cr_PolizaMacHogar_Envio_Consulta(
           int codEmpresa,
           string usuario,
           CrPolizaMacHogarEnvioConsultaRequest request)
        {
            return _bl.Cr_PolizaMacHogar_Envio_Consulta(codEmpresa, usuario, request);
        }

        [ValidateAntiForgeryToken]
        [HttpPost("Cr_PolizaMacHogar_Envio_Pagina")]
        public ErrorDto<CrPolizaMacHogarEnvioPagina> Cr_PolizaMacHogar_Envio_Pagina(
            int codEmpresa,
            string usuario,
            CrPolizaMacHogarEnvioPaginaRequest request)
        {
            return _bl.Cr_PolizaMacHogar_Envio_Pagina(codEmpresa, usuario, request);
        }

        #endregion

        #region Consulta
        [ValidateAntiForgeryToken]
        [HttpPost("Cr_PolizaMacHogar_Consulta_Obtener")]
        public ErrorDto<List<CrPolizaMacHogarEnvioRow>> Cr_PolizaMacHogar_Consulta_Obtener(
            int codEmpresa,
            string usuario,
            CrPolizaMacHogarEnvioConsultaRequest request)
        {
            return _bl.Cr_PolizaMacHogar_Consulta_Obtener(codEmpresa, usuario, request);
        }
        #endregion

        #region Beneficiarios
        [HttpGet("Cr_PolizaMacHogar_Beneficiarios_Lista")]
        public ErrorDto<List<CrPolizaMacHogarBeneficiariosRowDto>> Cr_PolizaMacHogar_Beneficiarios_Lista(
            int codEmpresa,
            string usuario)
        {
            return _bl.Cr_PolizaMacHogar_Beneficiarios_Lista(codEmpresa, usuario);
        }
        #endregion
    }
}
