using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Galileo.BusinessLogic.ProGrX.Fondos;
using Galileo.Models.ProGrX.Fondos;
using Galileo.Models;
using Galileo.Models.ERROR;

namespace Galileo.Controllers.ProGrX.Fondos
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmFndGarantiasController : ControllerBase
    {
        private readonly FrmFndGarantiasBl _bl;

        public FrmFndGarantiasController(IConfiguration config)
        {
            _bl = new FrmFndGarantiasBl(config);
        }

        [Authorize]
        [HttpGet("Fnd_GarantiasLista_Obtener")]
        public ErrorDto<FndGarantiasLista> Fnd_GarantiasLista_Obtener(int CodEmpresa, string filtros)
        {
            return _bl.Fnd_GarantiasLista_Obtener(CodEmpresa, filtros);
        }

        [Authorize]
        [HttpGet("Fnd_Garantias_Obtener")]
        public ErrorDto<List<FndGarantiaModel>> Fnd_Garantias_Obtener(int CodEmpresa)
        {
            return _bl.Fnd_Garantias_Obtener(CodEmpresa);
        }

        [Authorize]
        [HttpGet("Fnd_Garantias_Valida")]
        public ErrorDto<FndGarantiaValidaResult> Fnd_Garantias_Valida(int CodEmpresa, string garantiaFND)
        {
            return _bl.Fnd_Garantias_Valida(CodEmpresa, garantiaFND);
        }

        [Authorize]
        [HttpPost("Fnd_Garantias_Guardar")]
        public ErrorDto Fnd_Garantias_Guardar(int CodEmpresa, [FromBody] FndGarantiaModel garantia)
        {
            return _bl.Fnd_Garantias_Guardar(CodEmpresa, garantia);
        }

        [Authorize]
        [HttpDelete("Fnd_Garantias_Eliminar")]
        public ErrorDto Fnd_Garantias_Eliminar(int CodEmpresa, string garantiaFND, string usuario)
        {
            return _bl.Fnd_Garantias_Eliminar(CodEmpresa, garantiaFND, usuario);
        }

        [Authorize]
        [HttpPost("Fnd_Garantia_Ahorros_Consulta")]
        public ErrorDto<List<FndGarantiaAhorrosConsultaResult>> Fnd_Garantia_Ahorros_Consulta(
            int CodEmpresa, [FromBody] FndGarantiaAhorrosConsultaRequest request)
        {
            return _bl.Fnd_Garantia_Ahorros_Consulta(CodEmpresa, request);
        }

        [Authorize]
        [HttpPost("Fnd_Garantia_Ahorros_Registro")]
        public ErrorDto Fnd_Garantia_Ahorros_Registro(
            int CodEmpresa, [FromBody] FndGarantiaAhorrosRegistroRequest request)
        {
            return _bl.Fnd_Garantia_Ahorros_Registro(CodEmpresa, request);
        }

        [Authorize]
        [HttpGet("Fnd_Garantias_Lista_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> Fnd_Garantias_Lista_Obtener(int CodEmpresa)
        {
            return _bl.Fnd_Garantias_Lista_Obtener(CodEmpresa);
        }

        [Authorize]
        [HttpGet("Fnd_Operadoras_Lista_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> Fnd_Operadoras_Lista_Obtener(int CodEmpresa)
        {
            return _bl.Fnd_Operadoras_Lista_Obtener(CodEmpresa);
        }

        [Authorize]
        [HttpGet("Fnd_EstadosPersona_Lista_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> Fnd_EstadosPersona_Lista_Obtener(int CodEmpresa)
        {
            return _bl.Fnd_EstadosPersona_Lista_Obtener(CodEmpresa);
        }
    }
}