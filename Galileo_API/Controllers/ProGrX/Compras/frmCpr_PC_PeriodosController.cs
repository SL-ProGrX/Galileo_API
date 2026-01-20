using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic;
using Galileo.Models;
using Galileo.Models.CPR;
using Galileo.Models.ERROR;
using Microsoft.AspNetCore.Authorization;

namespace Galileo.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class FrmCprPCPeriodosController : ControllerBase
    {
        private readonly FrmCprPCPeriodosBL _bl;
        public FrmCprPCPeriodosController(IConfiguration config)
        {
            _bl = new FrmCprPCPeriodosBL(config);
        }

        [HttpGet("CprPeriodosContabilidades_Obtener")]
        public ErrorDto<List<CatalogosLista>> CprPeriodosContabilidades_Obtener(int CodEmpresa)
        {
            return _bl.CprPeriodosContabilidades_Obtener(CodEmpresa);
        }

        [HttpGet("CprPeriodosModelos_Obtener")]
        public ErrorDto<List<CatalogosLista>> CprPeriodosModelos_Obtener(int CodEmpresa, string usuario, int cod_contabilidad)
        {
            return _bl.CprPeriodosModelos_Obtener(CodEmpresa, usuario, cod_contabilidad);
        }

        [HttpGet("CprPeriodosPlan_Obtener")]
        public ErrorDto<CprPlanPeriodosDto> CprPeriodosPlan_Obtener(int CodEmpresa, int id_periodo)
        {
            return _bl.CprPeriodosPlan_Obtener(CodEmpresa, id_periodo);
        }

        [HttpGet("CprPeriodosPlanLista_Obtener")]
        public ErrorDto<CprPeriodosPlanLista> CprPeriodosPlanLista_Obtener(int CodEmpresa, string filtros)
        {
            return _bl.CprPeriodosPlanLista_Obtener(CodEmpresa, filtros);
        }

        [HttpGet("CprPeriodoPlan_Scroll")]
        public ErrorDto<CprPlanPeriodosDto> CprPeriodoPlan_Scroll(int CodEmpresa, int scroll, int? id_periodo)
        {
            return _bl.CprPeriodoPlan_Scroll(CodEmpresa, scroll, id_periodo);
        }

        [HttpPost("CprPeriodoPlan_Guardar")]
        public ErrorDto CprPeriodoPlan_Guardar(int CodEmpresa, CprPlanPeriodosDto periodo)
        {
            return _bl.CprPeriodoPlan_Guardar(CodEmpresa, periodo);
        }

        [HttpDelete("CprPeriodoPlan_Eliminar")]
        public ErrorDto CprPeriodoPlan_Eliminar(int CodEmpresa, int id_periodo)
        {
            return _bl.CprPeriodoPlan_Eliminar(CodEmpresa, id_periodo);
        }

        [HttpPut("CprPeriodoPlan_Aprobar")]
        public ErrorDto CprPeriodoPlan_Aprobar(int CodEmpresa, int id_periodo, string usuario)
        {
            return _bl.CprPeriodoPlan_Aprobar(CodEmpresa, id_periodo, usuario);
        }

        [HttpGet("CprPeriodoPlanMeses_Obtener")]
        public ErrorDto<CprModeloDateDatos> CprPeriodoPlanMeses_Obtener(string modelo)
        {
            return _bl.CprPeriodoPlanMeses_Obtener(modelo);
        }
    }
}