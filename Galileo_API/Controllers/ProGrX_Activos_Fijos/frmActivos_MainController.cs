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

    public class FrmActivosMainController : ControllerBase
    {

        private readonly FrmActivosMainBl _bl;
        public FrmActivosMainController(IConfiguration config)
        {
            _bl = new FrmActivosMainBl(config);
        }

        [Authorize]
        [HttpGet("Activos_Main_Departamentos_Obtener")]      
        public ErrorDto<List<DropDownListaGenericaModel>> Activos_Main_Departamentos_Obtener(int CodEmpresa)
        {
            return _bl.Activos_Main_Departamentos_Obtener(CodEmpresa);
        }

        [Authorize]
        [HttpGet("Activos_Main_Secciones_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> Activos_Main_Secciones_Obtener(int CodEmpresa, string departamento)
        {
            return _bl.Activos_Main_Secciones_Obtener(CodEmpresa, departamento);
        }

        [Authorize]
        [HttpGet("Activos_Main_Responsable_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> Activos_Main_Responsable_Obtener(int CodEmpresa, string departamento, string seccion)
        {
            return _bl.Activos_Main_Responsable_Obtener(CodEmpresa, departamento, seccion);
        }

        [Authorize]
        [HttpGet("Activos_Main_Localizacion_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> Activos_Main_Localizacion_Obtener(int CodEmpresa)
        {
            return _bl.Activos_Main_Localizacion_Obtener(CodEmpresa);
        }

        [Authorize]
        [HttpGet("Activos_Main_TipoActivo_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> Activos_Main_TipoActivo_Obtener(int CodEmpresa)
        {
            return _bl.Activos_Main_TipoActivo_Obtener(CodEmpresa);
        }

        [Authorize]
        [HttpGet("Activos_Main_PermiteRegistros_Validar")]
        public ErrorDto<int> Activos_Main_PermiteRegistros_Validar(int CodEmpresa)
        {
            return _bl.Activos_Main_PermiteRegistros_Validar(CodEmpresa);
        }

        [Authorize]
        [HttpGet("Activos_Main_ForzarTipoActivo_Validar")]
        public ErrorDto<int> Activos_Main_ForzarTipoActivo_Validar(int CodEmpresa)
        {
            return _bl.Activos_Main_ForzarTipoActivo_Validar(CodEmpresa);
        }

        [Authorize]
        [HttpGet("Activos_Main_NumeroPlaca_Consultar")]
        public ErrorDto<string> Activos_Main_NumeroPlaca_Consultar(int CodEmpresa, int orden, string placa = "")
        {
            return _bl.Activos_Main_NumeroPlaca_Consultar(CodEmpresa, orden, placa);
        }

        [Authorize]
        [HttpGet("Activos_Main_Historico_Consultar")]
        public ErrorDto<List<MainHistoricoData>> Activos_Main_Historico_Consultar(int CodEmpresa, string codigo, string estadoHistorico)
        {
            return _bl.Activos_Main_Historico_Consultar(CodEmpresa, codigo, estadoHistorico);
        }

        [Authorize]
        [HttpGet("Activos_Main_DetalleResponsables_Consultar")]
        public ErrorDto<List<MainDetalleResponsablesData>> Activos_Main_DetalleResponsables_Consultar(int CodEmpresa, string placa)
        {
            return _bl.Activos_Main_DetalleResponsables_Consultar(CodEmpresa, placa);
        }

        [Authorize]
        [HttpGet("Activos_Main_Modificaciones_Consultar")]
        public ErrorDto<List<MainModificacionesData>> Activos_Main_Modificaciones_Consultar(int CodEmpresa, string placa)
        {
            return _bl.Activos_Main_Modificaciones_Consultar(CodEmpresa, placa);
        }

        [Authorize]
        [HttpGet("Activos_Main_Composicion_Consultar")]
        public ErrorDto<List<MainComposicionData>> Activos_Main_Composicion_Consultar(int CodEmpresa, string placa)
        {
            return _bl.Activos_Main_Composicion_Consultar(CodEmpresa, placa);
        }

        [Authorize]
        [HttpGet("Activos_Main_Polizas_Consultar")]
        public ErrorDto<List<MainPolizasData>> Activos_Main_Polizas_Consultar(int CodEmpresa, string placa)
        {
            return _bl.Activos_Main_Polizas_Consultar(CodEmpresa, placa);
        }

        [Authorize]
        [HttpGet("Activos_Main_DatosGenerales_Consultar")]
        public ErrorDto<MainGeneralData> Activos_Main_DatosGenerales_Consultar(int CodEmpresa, string placa)
        {
            return _bl.Activos_Main_DatosGenerales_Consultar(CodEmpresa, placa);
        }

        [Authorize]
        [HttpGet("Activos_Main_Validaciones_Consultar")]
        public ErrorDto<string> Activos_Main_Validaciones_Consultar(int CodEmpresa, string placa, string placaAlternativa = "")
        {
            return _bl.Activos_Main_Validaciones_Consultar(CodEmpresa, placa, placaAlternativa);
        }

        [Authorize]
        [HttpPost("Activos_Main_Modificar")]
        public ErrorDto Activos_Main_Modificar(int CodEmpresa, MainGeneralData data, int aplicacionTotal, string usuario)
        {
            return _bl.Activos_Main_Modificar(CodEmpresa, data, aplicacionTotal, usuario);
        }

        [Authorize]
        [HttpPost("Activos_Main_Guardar")]
        public ErrorDto Activos_Main_Guardar(int CodEmpresa, MainGeneralData data, string usuario)
        {
            return _bl.Activos_Main_Guardar(CodEmpresa, data, usuario);
        }

        [Authorize]
        [HttpDelete("Activos_Main_Eliminar")]
        public ErrorDto Activos_Main_Eliminar(int CodEmpresa, string codigo, string usuario)
        {
            return _bl.Activos_Main_Eliminar(CodEmpresa, codigo, usuario);
        }

        [Authorize]
        [HttpGet("Activos_Main_Obtener")]
        public ErrorDto<List<ActivosData>> Activos_Main_Obtener(int CodEmpresa)
        {
            return _bl.Activos_Main_Obtener(CodEmpresa);
        }

        [Authorize]
        [HttpGet("Activos_Main_TipoActivo_Consultar")]
        public ErrorDto<MainActivosTiposData> Activos_Main_TipoActivo_Consultar(int CodEmpresa, string tipo_activo)
        {
            return _bl.Activos_Main_TipoActivo_Consultar(CodEmpresa, tipo_activo);
        }

        [Authorize]
        [HttpGet("Activos_Main_FechaUltimoCierre")]
        public ErrorDto<DateTime> Activos_Main_FechaUltimoCierre(int CodEmpresa)
        {
            return _bl.Activos_Main_FechaUltimoCierre(CodEmpresa);
        }

        [Authorize]
        [HttpGet("Activos_Main_Proveedores_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> Activos_Main_Proveedores_Obtener(int CodEmpresa)
        {
            return _bl.Activos_Main_Proveedores_Obtener(CodEmpresa);
        }

        [Authorize]
        [HttpGet("Activos_Main_PlacaId_Consultar")]
        public ErrorDto<string> Activos_Main_PlacaId_Consultar(int CodEmpresa)
        {
            return _bl.Activos_Main_PlacaId_Consultar(CodEmpresa);
        }

        [Authorize]
        [HttpGet("Activos_Main_DocCompas_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> Activos_Main_DocCompas_Obtener(int CodEmpresa, string proveedor, DateTime adquisicion)
        {
            return _bl.Activos_Main_DocCompas_Obtener(CodEmpresa, proveedor, adquisicion);
        }
    }
}