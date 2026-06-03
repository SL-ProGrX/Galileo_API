using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX.Creditos;
using Galileo_API.Models.ProGrX.Creditos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX.Creditos
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmCrConsultaBitacoraController : ControllerBase
    {
        private readonly FrmCrConsultaBitacoraBL BL;

        public FrmCrConsultaBitacoraController(IConfiguration config)
        {
            BL = new FrmCrConsultaBitacoraBL(config);
        }

        [Authorize]
        [HttpGet("CR_ConsultaBitacora_Encabezado_Obtener")]
        public ErrorDto<CrConsultaBitacoraEncabezadoDto> CR_ConsultaBitacora_Encabezado_Obtener(int CodEmpresa, string cedula)
        {
            return BL.CR_ConsultaBitacora_Encabezado_Obtener(CodEmpresa, cedula);
        }

        [Authorize]
        [HttpPost("CR_ConsultaBitacora_Registro_Lista_Obtener")]
        public ErrorDto<CrConsultaBitacoraLista<CrConsultaBitacoraRegistroDto>> CR_ConsultaBitacora_Registro_Lista_Obtener(int CodEmpresa,  [FromBody] CrConsultaBitacoraRequest request)
        {
            return BL.CR_ConsultaBitacora_Registro_Lista_Obtener(CodEmpresa, request);
        }

        [Authorize]
        [HttpPost("CR_ConsultaBitacora_Registro_Lista_Export")]
        public ErrorDto<CrConsultaBitacoraLista<CrConsultaBitacoraRegistroDto>> CR_ConsultaBitacora_Registro_Lista_Export(int CodEmpresa,[FromBody] CrConsultaBitacoraRequest request)
        {
            return BL.CR_ConsultaBitacora_Registro_Lista_Export(CodEmpresa, request);
        }

        [Authorize]
        [HttpPost("CR_ConsultaBitacora_Creditos_Lista_Obtener")]
        public ErrorDto<CrConsultaBitacoraLista<CrConsultaBitacoraCreditosDto>> CR_ConsultaBitacora_Creditos_Lista_Obtener(int CodEmpresa,[FromBody] CrConsultaBitacoraRequest request)
        {
            return BL.CR_ConsultaBitacora_Creditos_Lista_Obtener(CodEmpresa, request);
        }

        [Authorize]
        [HttpPost("CR_ConsultaBitacora_Creditos_Lista_Export")]
        public ErrorDto<CrConsultaBitacoraLista<CrConsultaBitacoraCreditosDto>> CR_ConsultaBitacora_Creditos_Lista_Export(int CodEmpresa,[FromBody] CrConsultaBitacoraRequest request)
        {
            return BL.CR_ConsultaBitacora_Creditos_Lista_Export(CodEmpresa, request);
        }

        [Authorize]
        [HttpPost("CR_ConsultaBitacora_Fondos_Lista_Obtener")]
        public ErrorDto<CrConsultaBitacoraLista<CrConsultaBitacoraFondosDto>> CR_ConsultaBitacora_Fondos_Lista_Obtener(int CodEmpresa,[FromBody] CrConsultaBitacoraRequest request)
        {
            return BL.CR_ConsultaBitacora_Fondos_Lista_Obtener(CodEmpresa, request);
        }

        [Authorize]
        [HttpPost("CR_ConsultaBitacora_Fondos_Lista_Export")]
        public ErrorDto<CrConsultaBitacoraLista<CrConsultaBitacoraFondosDto>> CR_ConsultaBitacora_Fondos_Lista_Export(int CodEmpresa,[FromBody] CrConsultaBitacoraRequest request)
        {
            return BL.CR_ConsultaBitacora_Fondos_Lista_Export(CodEmpresa, request);
        }

        [Authorize]
        [HttpPost("CR_ConsultaBitacora_Patrimonio_Lista_Obtener")]
        public ErrorDto<CrConsultaBitacoraLista<CrConsultaBitacoraPatrimonioDto>> CR_ConsultaBitacora_Patrimonio_Lista_Obtener(int CodEmpresa,[FromBody] CrConsultaBitacoraRequest request)
        {
            return BL.CR_ConsultaBitacora_Patrimonio_Lista_Obtener(CodEmpresa, request);
        }

        [Authorize]
        [HttpPost("CR_ConsultaBitacora_Patrimonio_Lista_Export")]
        public ErrorDto<CrConsultaBitacoraLista<CrConsultaBitacoraPatrimonioDto>> CR_ConsultaBitacora_Patrimonio_Lista_Export(int CodEmpresa,[FromBody] CrConsultaBitacoraRequest request)
        {
            return BL.CR_ConsultaBitacora_Patrimonio_Lista_Export(CodEmpresa, request);
        }

        [Authorize]
        [HttpPost("CR_ConsultaBitacora_Bancos_Lista_Obtener")]
        public ErrorDto<CrConsultaBitacoraLista<CrConsultaBitacoraBancosDto>> CR_ConsultaBitacora_Bancos_Lista_Obtener(int CodEmpresa,[FromBody] CrConsultaBitacoraRequest request)
        {
            return BL.CR_ConsultaBitacora_Bancos_Lista_Obtener(CodEmpresa, request);
        }

        [Authorize]
        [HttpPost("CR_ConsultaBitacora_Bancos_Lista_Export")]
        public ErrorDto<CrConsultaBitacoraLista<CrConsultaBitacoraBancosDto>> CR_ConsultaBitacora_Bancos_Lista_Export(int CodEmpresa,[FromBody] CrConsultaBitacoraRequest request)
        {
            return BL.CR_ConsultaBitacora_Bancos_Lista_Export(CodEmpresa, request);
        }
    }
}