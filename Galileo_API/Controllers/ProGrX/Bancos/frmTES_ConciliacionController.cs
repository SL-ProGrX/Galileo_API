using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Bancos;
using Galileo_API.BusinessLogic.ProGrX.Bancos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX.Bancos
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FrmTesConciliacionController : ControllerBase
    {
        private readonly FrmTesConciliacionBL _BL;

        public FrmTesConciliacionController(IConfiguration config)
        {
            _BL = new FrmTesConciliacionBL(config);
        }
        [HttpGet("TES_ConciliacionBancosLst_Obtener")]
        public ErrorDto<List<TesConciliacionCuentaData>> TES_ConciliacionBancosLst_Obtener(int CodEmpresa, string usuario)
        {
            return _BL.TES_ConciliacionBancosLst_Obtener(CodEmpresa, usuario);
        }

        #region Historial
        [HttpGet("TES_ConciliacionHistorico_Obtener")]
        public ErrorDto<List<TesConciliacionHistorico>> TES_ConciliacionHistorico_Obtener(int CodEmpresa, int id_banco, string usuario)
        {
            return _BL.TES_ConciliacionHistorico_Obtener(CodEmpresa, id_banco, usuario);
        }

        #endregion

        #region Resumen
        [HttpGet("TES_ConciliacionPeriodo_Consulta")]
        public ErrorDto<TesConciliaPeriodo> TES_ConciliacionPeriodo_Consulta(int CodEmpresa, string usuario, int id_banco, int pAnio, int mes)
        {
            return _BL.TES_ConciliacionPeriodo_Consulta(CodEmpresa, usuario, id_banco, pAnio, mes);
        }

        [HttpPatch("TES_ConciliacionSaldo_Actualiza")]
        public ErrorDto TES_ConciliacionSaldo_Actualiza(int CodEmpresa, TesConciliaFiltros filtro)
        {
            return _BL.TES_ConciliacionSaldo_Actualiza(CodEmpresa, filtro);
        }

        [HttpPost("TES_ConciliacionResumen_Guardar")]
        public ErrorDto TES_ConciliacionResumen_Guardar(int CodEmpresa, TesConciliaFiltros filtro)
        {
            return _BL.TES_ConciliacionResumen_Guardar(CodEmpresa, filtro);
        }

        [HttpPost("TES_ConciliacionResumenArchivo_Cargar")]
        public ErrorDto TES_ConciliacionResumenArchivo_Cargar(int CodEmpresa, string filtro, List<TesConciliacioExcelDto> file)
        {
            return _BL.TES_ConciliacionResumenArchivo_Cargar(CodEmpresa, filtro, file);
        }

        [HttpPost("TES_ConciliacionResumenPeriodo_Cerrar")]
        public ErrorDto TES_ConciliacionResumenPeriodo_Cerrar(int CodEmpresa, TesConciliaFiltros filtro)
        {
            return _BL.TES_ConciliacionResumenPeriodo_Cerrar(CodEmpresa, filtro);
        }

        [HttpPost("TES_ConciliacionResumen_Concilia")]
        public ErrorDto TES_ConciliacionResumen_Concilia(int CodEmpresa, int tipo, TesConciliaFiltros filtro)
        {
            return _BL.TES_ConciliacionResumen_Concilia(CodEmpresa, tipo, filtro);
        }

        [HttpPatch("TES_Conciliacion_Actualizar")]
        public ErrorDto TES_Conciliacion_Actualizar(int CodEmpresa, TesConciliaFiltros filtro)
        {
            return _BL.TES_Conciliacion_Actualizar(CodEmpresa, filtro);
        }

        [HttpPost("TES_Conciliacion_Inicializa")]
        public ErrorDto TES_Conciliacion_Inicializa(int CodEmpresa, TesConciliaFiltros filtro)
        {
            return _BL.TES_Conciliacion_Inicializa(CodEmpresa, filtro);
        }

        #endregion

        #region Resultados

        [HttpGet("TES_ConciliacionResultados_Obtener")]
        public ErrorDto<List<TesConciliaResultados>> TES_ConciliacionResultados_Obtener(int CodEmpresa, string filtros)
        {
            return _BL.TES_ConciliacionResultados_Obtener(CodEmpresa, filtros);
        }

        [HttpPost("TES_ConciliacionResultados_Autoregistro")]
        public ErrorDto TES_ConciliacionResultados_Autoregistro(int CodEmpresa, string filtros, List<TesConciliaResultados> datos)
        {
            return _BL.TES_ConciliacionResultados_Autoregistro(CodEmpresa, filtros, datos);
        }

        [HttpPost("TES_ConciliacionResultados_Pendiente")]
        public ErrorDto TES_ConciliacionResultados_Pendiente(int CodEmpresa, string filtros, List<TesConciliaResultados> datos)
        {
            return _BL.TES_ConciliacionResultados_Pendiente(CodEmpresa, filtros, datos);
        }

        #endregion

        #region Conciliación

        [HttpGet("TES_ConciliacionAsigna_Obtener")]
        public ErrorDto<List<TesConciliaAsigna>> TES_ConciliacionAsigna_Obtener(int CodEmpresa, string filtros)
        {
            return _BL.TES_ConciliacionAsigna_Obtener(CodEmpresa, filtros);
        }

        [HttpPost("TES_Conciliacion_Aplicar")]
        public ErrorDto TES_Conciliacion_Aplicar(int CodEmpresa, string filtros, List<TesConciliaAsigna> datos)
        {
            return _BL.TES_Conciliacion_Aplicar(CodEmpresa, filtros, datos);
        }

        [HttpGet("TES_ConciliacionDetalle_Obtener")]
        public ErrorDto<List<TesConciliacionDetallesData>> TES_ConciliacionDetalle_Obtener(int CodEmpresa, string filtros)
        {
            return _BL.TES_ConciliacionDetalle_Obtener(CodEmpresa, filtros);
        }

        [HttpGet("TES_ConciliacionDetalleLote_Obtener")]
        public ErrorDto<List<TesConciliacionDetallesLoteData>> TES_ConciliacionDetalleLote_Obtener(int CodEmpresa, string filtros)
        {
            return _BL.TES_ConciliacionDetalleLote_Obtener(CodEmpresa, filtros);
        }

        
        [HttpPost("TES_Conciliacion_Reversa")]
        public ErrorDto TES_Conciliacion_Reversa(int CodEmpresa, string filtros, List<TesConciliacionDetallesData> datos)
        {
            return _BL.TES_Conciliacion_Reversa(CodEmpresa, filtros, datos);
        }

        #endregion

    }
}
