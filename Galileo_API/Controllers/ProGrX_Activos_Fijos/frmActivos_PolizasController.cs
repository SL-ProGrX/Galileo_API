using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.BusinessLogic.ProGrX_Activos_Fijos;
using Galileo.Models.ProGrX_Activos_Fijos;

namespace Galileo.Controllers.ProGrX_Activos_Fijos
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmActivosPolizasController : ControllerBase
    {
        private readonly FrmActivosPolizasBL _bl;

        public FrmActivosPolizasController(IConfiguration config)
        {
            _bl = new FrmActivosPolizasBL(config);
        }

        [HttpGet("Activos_PolizasLista_Obtener")]
        [Authorize]
        public ErrorDto<ActivosPolizasLista> Activos_PolizasLista_Obtener(int CodEmpresa, string filtros)
        {
            return _bl.Activos_PolizasLista_Obtener(CodEmpresa, filtros);
        }

        [HttpGet("Activos_PolizasExiste_Obtener")]
        [Authorize]
        public ErrorDto Activos_PolizasExiste_Obtener(int CodEmpresa, string cod_poliza)
        {
            return _bl.Activos_Polizas_Valida(CodEmpresa, cod_poliza);
        }

        [HttpGet("Activos_Polizas_Obtener")]
        [Authorize]
        public ErrorDto<ActivosPolizasData?> Activos_Polizas_Obtener(int CodEmpresa, string cod_poliza)
        {
            return _bl.Activos_Polizas_Obtener(CodEmpresa, cod_poliza);
        }

        [HttpPost("Activos_Polizas_Guardar")]
        [Authorize]
        public ErrorDto Activos_Polizas_Guardar(int CodEmpresa, ActivosPolizasData poliza)
        {
            return _bl.Activos_Polizas_Guardar(CodEmpresa, poliza);
        }

        [HttpDelete("Activos_Polizas_Eliminar")]
        [Authorize]
        public ErrorDto Activos_Polizas_Eliminar(int CodEmpresa, string usuario, string cod_poliza)
        {
            return _bl.Activos_Polizas_Eliminar(CodEmpresa, usuario, cod_poliza);
        }

        [HttpGet("Activos_Polizas_Tipos_Listar")]
        [Authorize]
        public ErrorDto<List<DropDownListaGenericaModel>> Activos_Polizas_Tipos_Listar(int CodEmpresa)
        {
            return _bl.Activos_Polizas_Tipos_Listar(CodEmpresa);
        }
        [HttpGet("Activos_Tipo_Activo_Listar")]
        [Authorize]
        public ErrorDto<List<DropDownListaGenericaModel>> Activos_Tipo_Activo_Listar(int CodEmpresa)
        {
            return _bl.Activos_Tipo_Activo_Listar(CodEmpresa);
        }
        [HttpGet("Activos_Polizas_Asignacion_Listar")]
        [Authorize]
        public ErrorDto<ActivosPolizasLista> Activos_Polizas_Asignacion_Listar(int CodEmpresa, string cod_poliza, string? tipo_activo, string filtros)
        {
            return _bl.Activos_Polizas_Asignacion_Listar(CodEmpresa, cod_poliza, tipo_activo, filtros);
        }
        [HttpGet("Activos_Polizas_Asignacion_Listar_Export")]
        [Authorize]
        public ErrorDto<List<ActivosPolizasAsignacionItem>> Activos_Polizas_Asignacion_Listar_Export(int CodEmpresa, string cod_poliza, string? tipo_activo, string filtros)
        {
            return _bl.Activos_Polizas_Asignacion_Listar_Export(CodEmpresa,cod_poliza, tipo_activo, filtros);
        }
        [HttpPost("Activos_Polizas_Asignar")]
        [Authorize]
        public ErrorDto Activos_Polizas_Asignar(int CodEmpresa, string usuario, string cod_poliza, List<string> placas)
        {
            return _bl.Activos_Polizas_Asignar(CodEmpresa, usuario, cod_poliza, placas);
        }

        [HttpPost("Activos_Polizas_Desasignar")]
        [Authorize]
        public ErrorDto Activos_Polizas_Desasignar(int CodEmpresa, string usuario, string cod_poliza, List<string> placas)
        {
            return _bl.Activos_Polizas_Desasignar(CodEmpresa, usuario, cod_poliza, placas);
        }
    }
}