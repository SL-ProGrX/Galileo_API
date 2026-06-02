using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic.ProGrX.Clientes;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Clientes;

namespace Galileo.Controllers.ProGrX.Clientes
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmAFRemesasLiquidacionesController : ControllerBase
    {
        private readonly FrmAFRemesasLiquidacionesBL _bl;
        
        public FrmAFRemesasLiquidacionesController(IConfiguration config)
        {
            _bl = new FrmAFRemesasLiquidacionesBL(config);
        }

        #region Remesas
        
        [Authorize]
        [HttpGet("AF_RemesasLiquidaciones_Remesa_Obtener")]
        public ErrorDto<AfRemesasLiquidacionesLista> AF_RemesasLiquidaciones_Remesa_Obtener(int CodEmpresa, string filtro)
        {
            return _bl.AF_RemesasLiquidaciones_Remesa_Obtener(CodEmpresa, filtro);
        }

        [Authorize]
        [HttpGet("AF_RemesasLiquidaciones_Remesa_ObtenerId")]
        public ErrorDto<AfRemesaLiquidacionDto?> AF_RemesasLiquidaciones_Remesa_ObtenerId(int CodEmpresa, int remesa)
        {
            return _bl.AF_RemesasLiquidaciones_Remesa_Obtener(CodEmpresa, remesa);
        }

        [Authorize]
        [HttpPost("AF_RemesasLiquidaciones_Remesa_Guardar")]
        public ErrorDto AF_RemesasLiquidaciones_Remesa_Guardar(int CodEmpresa, AfRemesaLiquidacionDto remesa)
        {
            return _bl.AF_RemesasLiquidaciones_Remesa_Guardar(CodEmpresa, remesa);
        }

        [Authorize]
        [HttpDelete("AF_RemesasLiquidaciones_Remesa_Eliminar")]
        public ErrorDto AF_RemesasLiquidaciones_Remesa_Eliminar(int CodEmpresa, string usuario, int cod_remesa, string estado)
        {
            return _bl.AF_RemesasLiquidaciones_Remesa_Eliminar(CodEmpresa, usuario, cod_remesa, estado);
        }

        #endregion

        #region Cargas

        [Authorize]
        [HttpGet("AF_RemesasLiquidaciones_Carga_Obtener")]
        public ErrorDto<List<AfRemesaLiquidacionDto>> AF_RemesasLiquidaciones_Carga_Obtener(int CodEmpresa)
        {
            return _bl.AF_RemesasLiquidaciones_Carga_Obtener(CodEmpresa);
        }

        [Authorize]
        [HttpGet("AF_RemesasLiqui_CargaOficinas_Obtener")]
        public ErrorDto<AfRemesasLiquiCargaDatos> AF_RemesasLiqui_CargaOficinas_Obtener(int CodEmpresa, int remesa)
        {
            return _bl.AF_RemesasLiqui_CargaOficinas_Obtener(CodEmpresa, remesa);
        }

        [Authorize]
        [HttpGet("AF_RemesasLiqui_CargaLista_Obtener")]
        public ErrorDto<List<AfRemesasLiquiCargaLista>> AF_RemesasLiqui_CargaLista_Obtener(int CodEmpresa, int remesa, string oficina)
        {
            return _bl.AF_RemesasLiqui_CargaLista_Obtener(CodEmpresa, remesa, oficina);
        }

        [Authorize]
        [HttpPost("AF_RemesasLiquidaciones_Carga_Cargar")]
        public ErrorDto AF_RemesasLiquidaciones_Carga_Cargar(int CodEmpresa, int remesa, string usuario, List<AfRemesasLiquiCargaLista> datos)
        {
            return _bl.AF_RemesasLiquidaciones_Carga_Cargar(CodEmpresa, remesa, usuario, datos);
        }

        [Authorize]
        [HttpPut("AF_RemesasLiquidaciones_Carga_Cerrar")]
        public ErrorDto AF_RemesasLiquidaciones_Carga_Cerrar(int CodEmpresa, int remesa, string usuario)
        {
            return _bl.AF_RemesasLiquidaciones_Carga_Cerrar(CodEmpresa, remesa, usuario);
        }

        #endregion

        #region Reportes

        [Authorize]
        [HttpGet("AF_RemesasLiquidaciones_Reporte_Obtener")]
        public ErrorDto<List<AfRemesaLiquidacionDto>> AF_RemesasLiquidaciones_Reporte_Obtener(int CodEmpresa, DateTime fechaInicio, DateTime  fechaCorte, int top)
        {
            return _bl.AF_RemesasLiquidaciones_Reporte_Obtener(CodEmpresa, fechaInicio, fechaCorte, top);
        }

        [Authorize]
        [HttpPatch("AF_RemesasLiquidaciones_Reporte_Aplicar")]
        public ErrorDto AF_RemesasLiquidaciones_Reporte_Aplicar(int CodEmpresa, string usuario, int remesa)
        {
            return _bl.AF_RemesasLiquidaciones_Reporte_Aplicar(CodEmpresa, usuario, remesa);
        }

        #endregion

        #region Consultas

        [Authorize]
        [HttpGet("AF_RemesasLiquidaciones_Consultas_Obtener")]
        public ErrorDto<string> AF_RemesasLiquidaciones_Consultas_Obtener(int CodEmpresa, string consec)
        {
            return _bl.AF_RemesasLiquidaciones_Consultas_Obtener(CodEmpresa, consec);
        }

        #endregion
    }
}
