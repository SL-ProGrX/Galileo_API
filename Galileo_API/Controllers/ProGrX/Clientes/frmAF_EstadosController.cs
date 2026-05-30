using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic.ProGrX.Clientes;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Clientes;

namespace Galileo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmAFEstadosController : ControllerBase
    {
        private readonly FrmAFEstadosBL _bl;

        public FrmAFEstadosController(IConfiguration config)
        {
            _bl = new FrmAFEstadosBL(config);
        }

        [Authorize]
        [HttpGet("AF_Estados_Obtener")]
        public ErrorDto<AfEstadosLista> AF_Estados_Obtener(int CodEmpresa, string filtros)
        {
            return _bl.AF_Estados_Obtener(CodEmpresa, filtros);
        }

        [Authorize]
        [HttpPost("AF_Estados_Guardar")]
        public ErrorDto AF_Estados_Guardar(int CodEmpresa, string Usuario, AfEstadosDto Info)
        {
            return _bl.AF_Estados_Guardar(CodEmpresa, Usuario, Info);
        }

        [Authorize]
        [HttpDelete("AF_Estados_Eliminar")]
        public ErrorDto AF_Estados_Eliminar(int CodEmpresa, string Usuario, string CodEstado)
        {
            return _bl.AF_Estados_Eliminar(CodEmpresa, Usuario, CodEstado);
        }

        [Authorize]
        [HttpGet("AF_Estados_Movimientos_Obtener")]
        public ErrorDto<List<AfEstadosMovimientosDto>> AF_Estados_Movimientos_Obtener(int CodEmpresa)
        {
            return _bl.AF_Estados_Movimientos_Obtener(CodEmpresa);
        }

        [Authorize]
        [HttpPost("AF_Estados_Movimientos_Registrar")]
        public ErrorDto AF_Estados_Movimientos_Registrar(int CodEmpresa, AfEstadosMovimientosDto Info)
        {
            return _bl.AF_Estados_Movimientos_Registrar(CodEmpresa, Info);
        }

        [Authorize]
        [HttpDelete("AF_Estados_Movimientos_Eliminar")]
        public ErrorDto AF_Estados_Movimientos_Eliminar(int CodEmpresa, string Lista)
        {
            return _bl.AF_Estados_Movimientos_Eliminar(CodEmpresa, Lista);
        }

        [Authorize]
        [HttpGet("AF_Estados_Entidades_Obtener")]
        public ErrorDto<List<AfEstadosEntidadesDto>> AF_Estados_Entidades_Obtener(int CodEmpresa, string CodEstado)
        {
            return _bl.AF_Estados_Entidades_Obtener(CodEmpresa, CodEstado);
        }

        [Authorize]
        [HttpPost("AF_Estados_Entidad_Guardar")]
        public ErrorDto AF_Estados_Entidad_Guardar(int CodEmpresa, string Usuario, AfEstadosEntidadesDto Info)
        {
            return _bl.AF_Estados_Entidad_Guardar(CodEmpresa, Usuario, Info);
        }

        [Authorize]
        [HttpPost("AF_Estados_EntidadesTodas_Guardar")]
        public ErrorDto AF_Estados_EntidadesTodas_Guardar(int CodEmpresa, string Usuario, string CodEstado, bool Checked)
        {
            return _bl.AF_Estados_EntidadesTodas_Guardar(CodEmpresa, Usuario, CodEstado, Checked);
        }
    }
}