using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic;
using Galileo.Models.ERROR;
using Galileo.Models.INV;
using Microsoft.AspNetCore.Authorization;

namespace Galileo.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class FrmInvDepartamentosController : ControllerBase
    {
        private readonly FrmInvDepartamentosBL DepartamentosBL;

        public FrmInvDepartamentosController(IConfiguration config)
        {
            DepartamentosBL = new FrmInvDepartamentosBL(config);
        }

        [HttpGet("Departamentos_Obtener")]
        public ErrorDto<DepartamentosDataLista> Departamentos_Obtener(int CodCliente, int? pagina, int? paginacion, string? filtro)
        {
            return DepartamentosBL.Departamentos_Obtener(CodCliente, pagina, paginacion, filtro);
        }

        [HttpPost("Departamentos_Insertar")]
        public ErrorDto Departamentos_Insertar(int CodEmpresa, string usuario, DepartamentosDto request)
        {
            return DepartamentosBL.Departamentos_Insertar(CodEmpresa, usuario, request);
        }

        [HttpPost("Departamentos_Actualizar")]
        public ErrorDto Departamentos_Actualizar(int CodEmpresa, string usuario, DepartamentosDto request)
        {
            return DepartamentosBL.Departamentos_Actualizar(CodEmpresa, usuario, request);
        }

        [HttpDelete("Departamentos_Eliminar")]
        public ErrorDto Departamentos_Eliminar(int CodEmpresa, string departamento, string usuario)
        {
            return DepartamentosBL.Departamentos_Eliminar(CodEmpresa, departamento, usuario);
        }

        [HttpGet("Asignaciones_ObtenerTodos")]
        public ErrorDto<List<AsignacionesDto>> Asignaciones_ObtenerTodos(int CodEmpresa, string departamento)
        {
            return DepartamentosBL.Asignaciones_ObtenerTodos(CodEmpresa, departamento);
        }

        [HttpPost("Asignaciones_Insertar")]
        public ErrorDto Asignaciones_Insertar(int CodEmpresa, AsignacionesDto request)
        {
            return DepartamentosBL.Asignaciones_Insertar(CodEmpresa, request);
        }

        [HttpDelete("Asignaciones_Eliminar")]
        public ErrorDto Asignaciones_Eliminar(int CodEmpresa, string Cod_Departamento, string Cod_Prodclas)
        {
            return DepartamentosBL.Asignaciones_Eliminar(CodEmpresa, Cod_Departamento, Cod_Prodclas);
        }

    }
}
