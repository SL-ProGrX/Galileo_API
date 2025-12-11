
using Microsoft.AspNetCore.Mvc;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Activos_Fijos;
using Galileo.BusinessLogic.ProGrX_Activos_Fijos;

namespace Galileo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class FrmActivosRevaluacionesController : ControllerBase
    {
        private readonly FrmActivosRevaluacionesBL _bl;
        public FrmActivosRevaluacionesController(IConfiguration config)
        {
            _bl = new FrmActivosRevaluacionesBL(config);
        }

        [HttpPost("Activos_Revaluaciones_Guardar")]
        public ErrorDto Activos_Revaluaciones_Guardar(int CodEmpresa, string usuario, ActivosRevaluacionData data)
        {
            return _bl.Activos_Revaluaciones_Guardar(CodEmpresa, usuario, data);
        }

        [HttpGet("Activos_Revaluaciones_Historico_Consultar")]
        public ErrorDto<List<ActivosHistoricoData>> Activos_Revaluaciones_Historico_Consultar(int CodEmpresa, string placa)
        {
            return _bl.Activos_Revaluaciones_Historico_Consultar(CodEmpresa, placa);
        }

        [HttpDelete("Activos_Revaluaciones_Eliminar")]
        public ErrorDto Activos_Revaluaciones_Eliminar(int CodEmpresa, string placa, int Id_AddRet, string usuario)
        {
            return _bl.Activos_Revaluaciones_Eliminar(CodEmpresa, placa, Id_AddRet, usuario);
        }

    }
}