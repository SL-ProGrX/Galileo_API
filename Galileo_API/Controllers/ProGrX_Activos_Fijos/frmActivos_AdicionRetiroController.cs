using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic.ProGrX_Activos_Fijos;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Activos_Fijos;

namespace Galileo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class FrmActivosAdicionRetiroController : ControllerBase
    {
        private readonly FrmActivosAdicionRetiroBL _bl;
        public FrmActivosAdicionRetiroController(IConfiguration config)
        {
            _bl = new FrmActivosAdicionRetiroBL(config);
        }

        [Authorize]
        [HttpPost("Activos_AdicionRetiro_Guardar")]
        public ErrorDto Activos_AdicionRetiro_Guardar(int CodEmpresa, string usuario, ActivosRetiroAdicionData data)
        {
            return _bl.Activos_AdicionRetiro_Guardar(CodEmpresa, usuario, data);
        }

        [Authorize]
        [HttpGet("Activos_AdicionRetiro_Justificaciones_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> Activos_AdicionRetiro_Justificaciones_Obtener(int CodEmpresa, string tipo)
        {
            return _bl.Activos_AdicionRetiro_Justificaciones_Obtener(CodEmpresa, tipo);
        }

        [Authorize]
        [HttpGet("Activos_AdicionRetiro_Consultar")]
        public ErrorDto<ActivosRetiroAdicionData> Activos_AdicionRetiro_Consultar(int CodEmpresa, int Id_AddRet, string placa)
        {
            return _bl.Activos_AdicionRetiro_Consultar(CodEmpresa, Id_AddRet, placa);
        }

        [Authorize]
        [HttpGet("Activos_AdicionRetiro_Validar")]
        public ErrorDto<string> Activos_AdicionRetiro_Validar(int CodEmpresa, string placa, DateTime fecha)
        {
            return _bl.Activos_AdicionRetiro_Validar(CodEmpresa, placa, fecha);
        }

        [Authorize]
        [HttpGet("Activos_AdicionRetiro_Meses_Consulta")]
        public ErrorDto<int> Activos_AdicionRetiro_Meses_Consulta(int CodEmpresa, string placa,  DateTime fecha,string tipo = "")
        {
            
           return _bl.Activos_AdicionRetiro_Meses_Consulta(CodEmpresa, placa, tipo, fecha);
        }

        [Authorize]
        [HttpGet("Activos_AdicionRetiro_DatosActivo_Consultar")]
        public ErrorDto<ActivosPrincipalData> Activos_AdicionRetiro_DatosActivo_Consultar(int CodEmpresa, string placa)
        {
            return _bl.Activos_AdicionRetiro_DatosActivo_Consultar(CodEmpresa, placa);
        }

        [Authorize]
        [HttpGet("Activos_AdicionRetiro_Proveedores_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> Activos_AdicionRetiro_Proveedores_Obtener(int CodEmpresa)
        {
            return _bl.Activos_AdicionRetiro_Proveedores_Obtener(CodEmpresa);
        }

        [Authorize]
        [HttpGet("Activos_AdicionRetiro_Activos_Obtener")]
        public ErrorDto<List<ActivosData>> Activos_AdicionRetiro_Activos_Obtener(int CodEmpresa)
        {
            return _bl.Activos_AdicionRetiro_Activos_Obtener(CodEmpresa);
        }

        [Authorize]
        [HttpGet("Activos_AdicionRetiro_Historico_Consultar")]
        public ErrorDto<List<ActivosHistoricoData>> Activos_AdicionRetiro_Historico_Consultar(int CodEmpresa, string placa)
        {
            return _bl.Activos_AdicionRetiro_Historico_Consultar(CodEmpresa, placa);
        }

        [Authorize]
        [HttpGet("Activos_AdicionRetiro_Cierres_Consultar")]
        public ErrorDto<List<ActivosRetiroAdicionCierreData>> Activos_AdicionRetiro_Cierres_Consultar(int CodEmpresa, string placa, int Id_AddRet)
        {
            return _bl.Activos_AdicionRetiro_Cierres_Consultar(CodEmpresa, placa, Id_AddRet);
        }

        [Authorize]
        [HttpGet("Activos_AdicionRetiro_ActivosNombre_Consultar")]
        public ErrorDto<string?> Activos_AdicionRetiro_ActivosNombre_Consultar(int CodEmpresa, string placa)
        {
            return _bl.Activos_AdicionRetiro_ActivosNombre_Consultar(CodEmpresa, placa);
        }

        [Authorize]
        [HttpDelete("Activos_AdicionRetiro_Eliminar")]
        public ErrorDto Activos_AdicionRetiro_Eliminar(int CodEmpresa, string placa, int Id_AddRet)
        {
            return _bl.Activos_AdicionRetiro_Eliminar(CodEmpresa, placa, Id_AddRet);
        }

        [Authorize]
        [HttpGet("Activos_Periodo_Consultar")]
        public ErrorDto<DateTime> Activos_Periodo_Consultar(int CodEmpresa, int contabilidad)
        {
            return _bl.Activos_Periodo_Consultar(CodEmpresa, contabilidad);
        }
    }
}
