using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic;
using Galileo.Models.ERROR;
using Galileo.Models.SIF;

namespace Galileo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmSifComunicadosController : ControllerBase
    {
        private readonly FrmSifComunicadosBL _bl;

        public FrmSifComunicadosController(IConfiguration config)
        {
            _bl = new FrmSifComunicadosBL(config);
        }

        [Authorize]
        [HttpPost("Comunicados_Insertar")]
        public ErrorDto Comunicados_Insertar(int CodCliente, SifComunicadoDto comunicado)
        {
            return _bl.Comunicados_Insertar(CodCliente, comunicado);
        }

        [Authorize]
        [HttpGet("ConsultaAscDesc")]
        public ErrorDto<int> ConsultaAscDesc(int CodEmpresa, int consecutivo, string tipo)
        {
            return _bl.ConsultaAscDesc(CodEmpresa, consecutivo, tipo);
        }

        [Authorize]
        [HttpGet("Comunicado_Obtener")]
        public ErrorDto<SifComunicadoDto> Comunicado_Obtener(int CodEmpresa, int Cod_Comunicado)
        {
            return _bl.Comunicado_Obtener(CodEmpresa, Cod_Comunicado);
        }

        [Authorize]
        [HttpGet("ComunicadosLista_Obtener")]
        public ErrorDto<List<SifComunicadoDto>> ComunicadosLista_Obtener(int CodEmpresa)
        {
            return _bl.ComunicadosLista_Obtener(CodEmpresa);
        }

        [Authorize]
        [HttpPost("Comunicado_Actualizar")]
        public ErrorDto Comunicado_Actualizar(int CodEmpresa, SifComunicadoDto request)
        {
            return _bl.Comunicado_Actualizar(CodEmpresa, request);
        }
    }
}