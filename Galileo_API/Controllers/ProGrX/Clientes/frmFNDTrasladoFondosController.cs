using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Galileo.BusinessLogic.ProGrX.Clientes;
using Galileo.Models.ProGrX.Clientes;
using Galileo.Models;
using Galileo.Models.ERROR;

namespace Galileo.Controllers.ProGrX.Clientes
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmFndTrasladoFondosController : ControllerBase
    {
        private readonly FrmFndTrasladoFondosBL _bl;

        public FrmFndTrasladoFondosController(IConfiguration config)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));
            _bl = new FrmFndTrasladoFondosBL(config);
        }

        [Authorize]
        [HttpGet("Fnd_Operadoras_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> Fnd_Operadoras_Obtener(int CodEmpresa)
        {
            return _bl.Fnd_Operadoras_Obtener(CodEmpresa);
        }

        [Authorize]
        [HttpGet("Fnd_Traslado_Socios_Obtener")]
        public ErrorDto<List<FndTrasladoSocioSimple>> Fnd_Traslado_Socios_Obtener(int CodEmpresa, string ordenarPor)
        {
            return _bl.Fnd_Traslado_Socios_Obtener(CodEmpresa, ordenarPor);
        }

        [Authorize]
        [HttpGet("Fnd_Traslado_ContratosDisponibles_Obtener")]
        public ErrorDto<List<FndTrasladoContratoDisponible>> Fnd_Traslado_ContratosDisponibles_Obtener(
            int CodEmpresa, string codOperadora, string? cedula)
        {
            return _bl.Fnd_Traslado_ContratosDisponibles_Obtener(CodEmpresa, codOperadora, cedula);
        }

        [Authorize]
        [HttpPost("Fnd_TrasladoFondos_Ejecutar")]
        public ErrorDto<FndTrasladoFondosResult> Fnd_TrasladoFondos_Ejecutar(int CodEmpresa, [FromBody] FndTrasladoFondosRequest request)
        {
            return _bl.Fnd_TrasladoFondos_Ejecutar(CodEmpresa, request);
        }
    }
}