using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic.ProGrX.Clientes;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Clientes;

namespace Galileo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmAFDepartamentosController : ControllerBase
    {
        private readonly FrmAFDepartamentosBL _bl;
        public FrmAFDepartamentosController(IConfiguration config)
        {
            _bl = new FrmAFDepartamentosBL(config);
        }

        [Authorize]
        [HttpGet("AF_DepartamentosInstituciones_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> AF_DepartamentosInstituciones_Obtener(int CodEmpresa)
        {
            return _bl.AF_DepartamentosInstituciones_Obtener(CodEmpresa);
        }

        [Authorize]
        [HttpGet("AF_DepartamentosLista_Obtener")]
        public ErrorDto<AfDepartamentosLista> AF_DepartamentosLista_Obtener(int CodEmpresa, int Institucion, string Filtros)
        {
            return _bl.AF_DepartamentosLista_Obtener(CodEmpresa, Institucion, Filtros);
        }

        [Authorize]
        [HttpGet("AF_DepartamentosSecciones_Obtener")]
        public ErrorDto<AfSeccionesLista> AF_DepartamentosSecciones_Obtener(int CodEmpresa, int Institucion, string Departamento, string Filtros)
        {
            return _bl.AF_DepartamentosSecciones_Obtener(CodEmpresa, Institucion, Departamento, Filtros);
        }


        [Authorize]
        [HttpPost("AF_Departamentos_Guardar")]
        public ErrorDto AF_Departamentos_Guardar(int CodEmpresa, AfDepartamentosDto Info)
        {
            return _bl.AF_Departamentos_Guardar(CodEmpresa, Info);
        }

        [Authorize]
        [HttpPost("AF_DepartamentosSecciones_Guardar")]
        public ErrorDto AF_DepartamentosSecciones_Guardar(int CodEmpresa, AfSeccionesDto Info)
        {
            return _bl.AF_DepartamentosSecciones_Guardar(CodEmpresa, Info);
        }

        [Authorize]
        [HttpDelete("AF_Departamentos_Borrar")]
        public ErrorDto AF_Departamentos_Borrar(int CodEmpresa, int Institucion, string Departamento)
        {
            return _bl.AF_Departamentos_Borrar(CodEmpresa, Institucion, Departamento);
        }

        [Authorize]
        [HttpDelete("AF_DepartamentosSecciones_Borrar")]
        public ErrorDto AF_DepartamentosSecciones_Borrar(int CodEmpresa, int Institucion, string Departamento, string Seccion)
        {
            return _bl.AF_DepartamentosSecciones_Borrar(CodEmpresa, Institucion, Departamento, Seccion);
        }
    }
}