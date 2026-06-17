using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX.Creditos;
using Galileo_API.Models.ProGrX.Creditos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX.Creditos
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmCrConsultaCrdExcController : ControllerBase
    {
        private readonly FrmCrConsultaCrdExcBL BL;

        public FrmCrConsultaCrdExcController(IConfiguration config)
        {
            BL = new FrmCrConsultaCrdExcBL(config);
        }

        [Authorize]
        [HttpGet("CR_ConsultaCrdExc_Inicial_Obtener")]
        public ErrorDto<CrConsultaCrdExcInicialDto> CR_ConsultaCrdExc_Inicial_Obtener(int CodEmpresa,string cedula,string usuario)
        {
            return BL.CR_ConsultaCrdExc_Inicial_Obtener(CodEmpresa, cedula, usuario);
        }

        [Authorize]
        [HttpGet("CR_ConsultaCrdExc_CuentasBanco_Obtener")]
        public ErrorDto<List<CrConsultaCrdExcCuentaBancoDto>> CR_ConsultaCrdExc_CuentasBanco_Obtener(int CodEmpresa,string cedula,int banco)
        {
            return BL.CR_ConsultaCrdExc_CuentasBanco_Obtener(CodEmpresa, cedula, banco);
        }

        [Authorize]
        [HttpGet("CR_ConsultaCrdExc_DisponibleRecurso_Obtener")]
        public ErrorDto<CrConsultaCrdExcDisponibleRecursoDto> CR_ConsultaCrdExc_DisponibleRecurso_Obtener(int CodEmpresa,string recurso)
        {
            return BL.CR_ConsultaCrdExc_DisponibleRecurso_Obtener(CodEmpresa, recurso);
        }

        [Authorize]
        [HttpPost("CR_ConsultaCrdExc_Formalizar")]
        public ErrorDto<CrConsultaCrdExcFormalizarDto> CR_ConsultaCrdExc_Formalizar(int CodEmpresa,[FromBody] CrConsultaCrdExcFormalizarRequest request)
        {
            if (request == null)
            {
                return new ErrorDto<CrConsultaCrdExcFormalizarDto>
                {
                    Code = -1,
                    Description = "La solicitud es requerida."
                };
            }

            return BL.CR_ConsultaCrdExc_Formalizar(CodEmpresa, request);
        }
        [Authorize]
        [HttpGet("CR_ConsultaCrdExc_OficinaUsuario_Obtener")]
        public ErrorDto<CrConsultaCrdExcOficinaUsuarioDto> CR_ConsultaCrdExc_OficinaUsuario_Obtener(int CodEmpresa, string usuario)
        {
            return BL.CR_ConsultaCrdExc_OficinaUsuario_Obtener(CodEmpresa, usuario);
        }
    }
}