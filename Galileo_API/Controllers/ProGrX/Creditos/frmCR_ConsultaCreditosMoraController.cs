using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX.Creditos;
using Galileo_API.Models.ProGrX.Creditos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX.Creditos
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmCRConsultaCreditosMoraController : ControllerBase
    {
        private readonly FrmCRConsultaCreditosMoraBL _bl;

        public FrmCRConsultaCreditosMoraController(IConfiguration config)
        {
            _bl = new FrmCRConsultaCreditosMoraBL(config);
        }

        [Authorize]
        [HttpGet("CR_ConsultaCreditosMora_Header_Obtener")]
        public ErrorDto<CrConsultaCreditosMoraHeaderDto> CR_ConsultaCreditosMora_Header_Obtener(int CodEmpresa, string cedula)
        {
            return _bl.CR_ConsultaCreditosMora_Header_Obtener(CodEmpresa, cedula);
        }

        [Authorize]
        [HttpGet("CR_ConsultaCreditosMora_Detalle_Lista_Obtener")]
        public ErrorDto<CrConsultaCreditosMoraListaResult<CrConsultaCreditosMoraDetalleDto>> CR_ConsultaCreditosMora_Detalle_Lista_Obtener(int CodEmpresa, string parametros)
        {
            return _bl.CR_ConsultaCreditosMora_Detalle_Lista_Obtener(CodEmpresa, parametros);
        }

        [Authorize]
        [HttpGet("CR_ConsultaCreditosMora_Detalle_Lista_Export")]
        public ErrorDto<CrConsultaCreditosMoraListaResult<CrConsultaCreditosMoraDetalleDto>> CR_ConsultaCreditosMora_Detalle_Lista_Export(int CodEmpresa, string parametros)
        {
            return _bl.CR_ConsultaCreditosMora_Detalle_Lista_Export(CodEmpresa, parametros);
        }

        [Authorize]
        [HttpGet("CR_ConsultaCreditosMora_Garantia_Lista_Obtener")]
        public ErrorDto<CrConsultaCreditosMoraListaResult<CrConsultaCreditosMoraGarantiaDto>> CR_ConsultaCreditosMora_Garantia_Lista_Obtener(int CodEmpresa, string parametros)
        {
            return _bl.CR_ConsultaCreditosMora_Garantia_Lista_Obtener(CodEmpresa, parametros);
        }

        [Authorize]
        [HttpGet("CR_ConsultaCreditosMora_Garantia_Lista_Export")]
        public ErrorDto<CrConsultaCreditosMoraListaResult<CrConsultaCreditosMoraGarantiaDto>> CR_ConsultaCreditosMora_Garantia_Lista_Export(int CodEmpresa, string parametros)
        {
            return _bl.CR_ConsultaCreditosMora_Garantia_Lista_Export(CodEmpresa, parametros);
        }
    }
}