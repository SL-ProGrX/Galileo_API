using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic;
using Galileo.Models.ERROR;
using Galileo.Models.SIF;

namespace Galileo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmSifJuzgadosController : ControllerBase
    {
        private readonly FrmSifJuzgadosBL _bl;
        public FrmSifJuzgadosController(IConfiguration config)
        {
            _bl = new FrmSifJuzgadosBL(config);
        }

        [Authorize]
        [HttpPost("SIF_Juzgados_Insertar")]
        public ErrorDto SIF_Juzgados_Insertar(int CodCliente, JuzgadosDto juzgado)
        {
            return _bl.SIF_Juzgados_Insertar(CodCliente, juzgado);
        }

        [Authorize]
        [HttpGet("SIF_Juzgados_ConsultaAscDesc")]
        public ErrorDto<string> SIF_Juzgados_ConsultaAscDesc(int CodEmpresa, string consecutivo, string tipo)
        {
            return _bl.SIF_Juzgados_ConsultaAscDesc(CodEmpresa, consecutivo, tipo);
        }

        [Authorize]
        [HttpGet("SIF_Juzgados_Obtener")]
        public ErrorDto<JuzgadosDto>  SIF_Juzgados_Obtener(int CodEmpresa, string consecutivo)
        {
            return _bl.SIF_Juzgados_Obtener(CodEmpresa, consecutivo);
        }

        [Authorize]
        [HttpPost("SIF_Juzgados_Actualizar")]
        public ErrorDto SIF_Juzgados_Actualizar(int CodEmpresa, JuzgadosDto request)
        {
            return _bl.SIF_Juzgados_Actualizar(CodEmpresa, request);
        }

        [Authorize]
        [HttpDelete("SIF_Juzgados_Eliminar")]
        public ErrorDto SIF_Juzgados_Eliminar(int CodEmpresa, string consecutivo)
        {
            return _bl.SIF_Juzgados_Eliminar(CodEmpresa, consecutivo);
        }

        [Authorize]
        [HttpGet("SIF_JuzgadosLista_Obtener")]
        public ErrorDto<List<JuzgadosDto>> SIF_JuzgadosLista_Obtener(int CodEmpresa)
        {
            return _bl.SIF_JuzgadosLista_Obtener(CodEmpresa);
        }

    }
}