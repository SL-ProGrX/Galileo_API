using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX.Cajas;
using Galileo_API.Models.ProGrX.Cajas;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX.Cajas
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FrmCajasIdentificaSfController : ControllerBase
    {
        private readonly FrmCajasIdentificaSfBL _bl;
        
        public FrmCajasIdentificaSfController(IConfiguration config)
        {
            _bl = new FrmCajasIdentificaSfBL(config);
        }

        [HttpPost("Cajas_CajasIdentificaSf_Identificar")]
        public ErrorDto Cajas_CajasIdentificaSf_Identificar(
          int codEmpresa,
          string cedula,
          string nombre,
          string usuario,
          string pagadorId,
          string origenRecursosId,
          List<TesDepositoIdentificarDto> casos)
        {
            return _bl.Cajas_CajasIdentificaSf_Identificar(
            codEmpresa,
            cedula,
            nombre,
            usuario,
            pagadorId,
            origenRecursosId,
            casos);
        }

        [HttpGet("Cajas_CajasIdentificaSf_Depositos_Obtener")]
        public ErrorDto<List<FrmCajasIdentificaSfDepositoDto>> Cajas_CajasIdentificaSf_Depositos_Obtener(int CodEmpresa)
        {
            return _bl.Cajas_CajasIdentificaSf_Depositos_Obtener(CodEmpresa);
        }

        [HttpGet("Cajas_CajasIdentificaSf_Entidades_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> Cajas_CajasIdentificaSf_Entidades_Obtener(int CodEmpresa)
        {
            return _bl.Cajas_CajasIdentificaSf_Entidades_Obtener(CodEmpresa);
        }

        [HttpGet("Cajas_CajasIdentificaSf_Recursos_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> Cajas_CajasIdentificaSf_Recursos_Obtener(int CodEmpresa)
        {
            return _bl.Cajas_CajasIdentificaSf_Recursos_Obtener(CodEmpresa);
        }

        [HttpGet("Cajas_CajasIdentificaSf_Consultar")]
        public ErrorDto<List<FrmCajasIdentificaSfTramitsRsdto>> Cajas_CajasIdentificaSf_Consultar(
                int codEmpresa,
                DateTime fechaInicio,
                DateTime fechaCorte,
                int bancoId,
                decimal montoInicio,
                decimal montoHasta,
                string? numDocumento)
        {
            return _bl.Cajas_CajasIdentificaSf_Consultar(
                codEmpresa,
                fechaInicio,
                fechaCorte,
                bancoId,
                montoInicio,
                montoHasta,
                numDocumento);
        }
    }
}