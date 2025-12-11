using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic.ProGrX_Activos_Fijos;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Activos_Fijos;

namespace Galileo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class FrmActivosDeteriorosController : ControllerBase
    {
        private readonly FrmActivosDeteriorosBl _bl;
        public FrmActivosDeteriorosController(IConfiguration config)
        {
            _bl = new FrmActivosDeteriorosBl(config);
        }

        [HttpGet("Activos_Deterioros_Justificaciones_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> Activos_Deterioros_Justificaciones_Obtener(int CodEmpresa)
        {
            return _bl.Activos_Deterioros_Justificaciones_Obtener(CodEmpresa);
        }

        [HttpGet("Activos_Deterioros_Consultar")]
        public ErrorDto<ActivosDeterioroData?> Activos_Deterioros_Consultar(int CodEmpresa, int Id_AddRet, string placa)
        {
            return _bl.Activos_Deterioros_Consultar(CodEmpresa, Id_AddRet, placa);
        }

        [HttpGet("Activos_Deterioros_Validar")]
        public ErrorDto<string> Activos_Deterioros_Validar(int CodEmpresa, string placa, DateTime fecha)
        {
            return _bl.Activos_Deterioros_Validar(CodEmpresa, placa, fecha);
        }

        [HttpGet("Activos_DeteriorosDetalle_Consultar")]
        public ErrorDto<ActivosDeterioroDetallaData> Activos_DeteriorosDetalle_Consultar(int CodEmpresa, string placa)
        {
            return _bl.Activos_DeteriorosDetalle_Consultar(CodEmpresa, placa);
        }

        [HttpGet("Activos_Deterioros_Activos_Obtener")]
        public ErrorDto<List<ActivosData>> Activos_Deterioros_Activos_Obtener(int CodEmpresa)
        {
            return _bl.Activos_Deterioros_Activos_Obtener(CodEmpresa);
        }

        [HttpPost("Activos_Deterioros_Guardar")]
        public ErrorDto Activos_Deterioros_Guardar(int CodEmpresa, string usuario, ActivosDeterioroData data)
        {
            return _bl.Activos_Deterioros_Guardar(CodEmpresa, usuario, data);
        }
        [HttpGet("Activos_Deterioros_Historico_Consultar")]
        public ErrorDto<List<ActivosHistoricoData>> Activos_Deterioros_Historico_Consultar(int CodEmpresa, string placa)
        {
            return _bl.Activos_Deterioros_Historico_Consultar(CodEmpresa, placa);
        }

        [HttpGet("Activos_Deterioros_ActivosNombre_Consultar")]
        public ErrorDto<string?> Activos_Deterioros_ActivosNombre_Consultar(int CodEmpresa, string placa)
        {
            return _bl.Activos_Deterioros_ActivosNombre_Consultar(CodEmpresa, placa);
        }

        [HttpDelete("Activos_Deterioros_Eliminar")]
        public ErrorDto Activos_Deterioros_Eliminar(int CodEmpresa, string usuario, string placa, int Id_AddRet)
        {
            return _bl.Activos_Deterioros_Eliminar(CodEmpresa, usuario, placa, Id_AddRet);
        }

        [HttpGet("Activos_Periodo_Consultar")]
        public ErrorDto<DateTime> Activos_Periodo_Consultar(int CodEmpresa, int contabilidad)
        {
            return _bl.Activos_Periodo_Consultar(CodEmpresa, contabilidad);
        }
    }
}
