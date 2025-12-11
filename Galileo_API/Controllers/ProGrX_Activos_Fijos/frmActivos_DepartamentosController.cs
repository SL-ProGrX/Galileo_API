using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic.ProGrX_Activos_Fijos;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Activos_Fijos;
using Galileo.Models;

namespace Galileo.Controllers.ProGrX_Activos_Fijos
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmActivosDepartamentosController : ControllerBase
    {
        private readonly FrmActivosDepartamentosBl _bl;

        public FrmActivosDepartamentosController(IConfiguration config)
        {
            _bl = new FrmActivosDepartamentosBl(config);
        }

        [Authorize]
        [HttpGet("Activos_DepartamentosLista_Obtener")]
        public ErrorDto<ActivosDepartamentosLista> Activos_DepartamentosLista_Obtener(int CodEmpresa, string filtros)
        {
            return _bl.Activos_DepartamentosLista_Obtener(CodEmpresa, filtros);
        }

        [Authorize]
        [HttpGet("Activos_Departamentos_Obtener")]
        public ErrorDto<List<ActivosDepartamentosData>> Activos_Departamentos_Obtener(int CodEmpresa, string filtros)
        {
            return _bl.Activos_Departamentos_Obtener(CodEmpresa, filtros);
        }

        [Authorize]
        [HttpPost("Activos_Departamentos_Guardar")]
        public ErrorDto Activos_Departamentos_Guardar(int CodEmpresa, string usuario, ActivosDepartamentosData departamento)
        {
            return _bl.Activos_Departamentos_Guardar(CodEmpresa, usuario, departamento);
        }

        [Authorize]
        [HttpDelete("Activos_Departamentos_Eliminar")]
        public ErrorDto Activos_Departamentos_Eliminar(int CodEmpresa, string usuario, string cod_departamento)
        {
            return _bl.Activos_Departamentos_Eliminar(CodEmpresa, usuario, cod_departamento);
        }

        [Authorize]
        [HttpGet("Activos_Departamentos_Valida")]
        public ErrorDto Activos_Departamentos_Valida(int CodEmpresa, string cod_departamento)
        {
            return _bl.Activos_Departamentos_Valida(CodEmpresa, cod_departamento);
        }

        [Authorize]
        [HttpGet("Activos_Departamentos_Unidades_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> Activos_Departamentos_Unidades_Obtener(int CodEmpresa, int contabilidad)
        {
            return _bl.Activos_Departamentos_Unidades_Obtener(CodEmpresa, contabilidad);
        }

        [Authorize]
        [HttpGet("Activos_Departamentos_Dropdown_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> Activos_Departamentos_Dropdown_Obtener(int CodEmpresa)
        {
            return _bl.Activos_Departamentos_Dropdown_Obtener(CodEmpresa);
        }

        [Authorize]
        [HttpGet("Activos_SeccionesLista_Obtener")]
        public ErrorDto<ActivosSeccionesLista> Activos_SeccionesLista_Obtener(int CodEmpresa, string? cod_departamento, string filtros)
        {
            return _bl.Activos_SeccionesLista_Obtener(CodEmpresa, cod_departamento, filtros);
        }

        [Authorize]
        [HttpGet("Activos_Secciones_Obtener")]
        public ErrorDto<List<ActivosSeccionesData>> Activos_Secciones_Obtener(int CodEmpresa, string? cod_departamento, string filtros)
        {
            return _bl.Activos_Secciones_Obtener(CodEmpresa, cod_departamento, filtros);
        }

        [Authorize]
        [HttpPost("Activos_Secciones_Guardar")]
        public ErrorDto Activos_Secciones_Guardar(int CodEmpresa, string usuario, ActivosSeccionesData seccion)
        {
            return _bl.Activos_Secciones_Guardar(CodEmpresa, usuario, seccion);
        }

        [Authorize]
        [HttpDelete("Activos_Secciones_Eliminar")]
        public ErrorDto Activos_Secciones_Eliminar(int CodEmpresa, string usuario, string cod_departamento, string cod_seccion)
        {
            return _bl.Activos_Secciones_Eliminar(CodEmpresa, usuario, cod_departamento, cod_seccion);
        }

        [Authorize]
        [HttpGet("Activos_Secciones_Valida")]
        public ErrorDto Activos_Secciones_Valida(int CodEmpresa, string cod_departamento, string cod_seccion)
        {
            return _bl.Activos_Secciones_Valida(CodEmpresa, cod_departamento, cod_seccion);
        }

        [Authorize]
        [HttpGet("Activos_Secciones_CentrosCostos_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> Activos_Secciones_CentrosCostos_Obtener(int CodEmpresa, int contabilidad)
        {
            return _bl.Activos_Secciones_CentrosCostos_Obtener(CodEmpresa, contabilidad);
        }
    }
}