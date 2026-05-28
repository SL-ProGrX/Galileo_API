using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX.Creditos;
using Galileo_API.Models.ProGrX.Creditos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX.Creditos
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmCrGarantiasPatrimonialesController : ControllerBase
    {
        private readonly FrmCrGarantiasPatrimonialesBL BL;

        public FrmCrGarantiasPatrimonialesController(IConfiguration config)
        {
            BL = new FrmCrGarantiasPatrimonialesBL(config);
        }

        [Authorize]
        [HttpGet("CR_GarantiasPatrimoniales_Garantias_Dropdown_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CR_GarantiasPatrimoniales_Garantias_Dropdown_Obtener(int CodEmpresa)
        {
            return BL.CR_GarantiasPatrimoniales_Garantias_Dropdown_Obtener(CodEmpresa);
        }

        [Authorize]
        [HttpGet("CR_GarantiasPatrimoniales_EstadosPersona_Dropdown_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CR_GarantiasPatrimoniales_EstadosPersona_Dropdown_Obtener(int CodEmpresa)
        {
            return BL.CR_GarantiasPatrimoniales_EstadosPersona_Dropdown_Obtener(CodEmpresa);
        }

        [Authorize]
        [HttpGet("CR_GarantiasPatrimoniales_Operadoras_Dropdown_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CR_GarantiasPatrimoniales_Operadoras_Dropdown_Obtener(int CodEmpresa)
        {
            return BL.CR_GarantiasPatrimoniales_Operadoras_Dropdown_Obtener(CodEmpresa);
        }

        [Authorize]
        [HttpGet("CR_GarantiasPatrimoniales_Lista_Obtener")]
        public ErrorDto<CrGarantiasPatrimonialesListaResult> CR_GarantiasPatrimoniales_Lista_Obtener(int CodEmpresa, string parametros)
        {
            return BL.CR_GarantiasPatrimoniales_Lista_Obtener(CodEmpresa, parametros);
        }

        [Authorize]
        [HttpGet("CR_GarantiasPatrimoniales_Lista_Export")]
        public ErrorDto<CrGarantiasPatrimonialesListaResult> CR_GarantiasPatrimoniales_Lista_Export(int CodEmpresa, string parametros)
        {
            return BL.CR_GarantiasPatrimoniales_Lista_Export(CodEmpresa, parametros);
        }

        [Authorize]
        [HttpPost("CR_GarantiasPatrimoniales_Guardar")]
        public ErrorDto CR_GarantiasPatrimoniales_Guardar(int CodEmpresa,string usuario,[FromBody] CrGarantiasPatrimonialesRegistroRequest request)
        {
            return BL.CR_GarantiasPatrimoniales_Guardar(CodEmpresa, request, usuario);
        }

        [Authorize]
        [HttpPost("CR_GarantiasPatrimoniales_Eliminar")]
        public ErrorDto CR_GarantiasPatrimoniales_Eliminar(int CodEmpresa,string usuario,[FromBody] CrGarantiasPatrimonialesRegistroRequest request)
        {
            return BL.CR_GarantiasPatrimoniales_Eliminar(CodEmpresa, request, usuario);
        }
    }
}