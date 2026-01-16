using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Bancos;
using Newtonsoft.Json;
using PgxAPI.DataBaseTier.ProGrX.Bancos;

namespace PgxAPI.BusinessLogic.ProGrX.Bancos
{

    public class FrmTesConciliacionBL
    {
        private readonly FrmTesConciliacionDB _Db;

        public FrmTesConciliacionBL(IConfiguration config)
        {
            _Db = new FrmTesConciliacionDB(config);
        }

        public ErrorDto<List<TesConciliacionCuentaData>> TES_ConciliacionBancosLst_Obtener(int CodEmpresa, string usuario)
        {
            return _Db.TES_ConciliacionBancosLst_Obtener(CodEmpresa, usuario);
        }

        #region Historial

        public ErrorDto<List<TesConciliacionHistorico>> TES_ConciliacionHistorico_Obtener(int CodEmpresa, int id_banco, string usuario)
        {
            return _Db.TES_ConciliacionHistorico_Obtener(CodEmpresa, id_banco, usuario);
        }

        #endregion

        #region Resumen

        public ErrorDto<TesConciliaPeriodo> TES_ConciliacionPeriodo_Consulta(int CodEmpresa, string usuario, int id_banco, int pAnio, int mes)
        {
            return _Db.TES_ConciliacionPeriodo_Consulta(CodEmpresa, usuario, id_banco, pAnio, mes);
        }

        public ErrorDto TES_ConciliacionSaldo_Actualiza(int CodEmpresa, TesConciliaFiltros filtro)
        {
            return _Db.TES_ConciliacionSaldo_Actualiza(CodEmpresa, filtro);
        }

        public ErrorDto TES_ConciliacionResumen_Guardar(int CodEmpresa, TesConciliaFiltros filtro)
        {
            return _Db.TES_ConciliacionResumen_Guardar(CodEmpresa, filtro);
        }

        public ErrorDto TES_ConciliacionResumenArchivo_Cargar(int CodEmpresa, string filtro, List<TesConciliacioExcelDto> file)
        {
            TesConciliaFiltros vFiltro = JsonConvert.DeserializeObject<TesConciliaFiltros>(filtro) ?? new TesConciliaFiltros();
            return _Db.TES_ConciliacionResumenArchivo_Cargar(CodEmpresa, vFiltro, file);
        }

        public ErrorDto TES_ConciliacionResumenPeriodo_Cerrar(int CodEmpresa, TesConciliaFiltros filtro)
        {
            return _Db.TES_ConciliacionResumenPeriodo_Cerrar(CodEmpresa, filtro);
        }

        public ErrorDto TES_ConciliacionResumen_Concilia(int CodEmpresa, int tipo, TesConciliaFiltros filtro)
        {
            return _Db.TES_ConciliacionResumen_Concilia(CodEmpresa, tipo, filtro);
        }

        public ErrorDto TES_Conciliacion_Actualizar(int CodEmpresa, TesConciliaFiltros filtro)
        {
            return _Db.TES_Conciliacion_Actualizar(CodEmpresa, filtro);
        }

        public ErrorDto TES_Conciliacion_Inicializa(int CodEmpresa, TesConciliaFiltros filtro)
        {
            return _Db.TES_Conciliacion_Inicializa(CodEmpresa, filtro);
        }

        #endregion

        #region Resultados

        public ErrorDto<List<TesConciliaResultados>> TES_ConciliacionResultados_Obtener(int CodEmpresa, string filtros)
        {
            TesConciliaResultadoFiltros filtro = JsonConvert.DeserializeObject<TesConciliaResultadoFiltros>(filtros) ?? new TesConciliaResultadoFiltros();
            return _Db.TES_ConciliacionResultados_Obtener(CodEmpresa, filtro);
        }

        public ErrorDto TES_ConciliacionResultados_Autoregistro(int CodEmpresa, string filtros , List<TesConciliaResultados> datos)
        {
            TesConciliacionResultosFiltro filtro = JsonConvert.DeserializeObject<TesConciliacionResultosFiltro>(filtros) ?? new TesConciliacionResultosFiltro();
            return _Db.TES_ConciliacionResultados_Autoregistro(CodEmpresa, filtro, datos);
        }

        public ErrorDto TES_ConciliacionResultados_Pendiente(int CodEmpresa, string filtros, List<TesConciliaResultados> datos)
        {
            TesConciliacionResultosFiltro filtro = JsonConvert.DeserializeObject<TesConciliacionResultosFiltro>(filtros) ?? new TesConciliacionResultosFiltro();
            return _Db.TES_ConciliacionResultados_Pendiente(CodEmpresa, filtro, datos);
        }

        #endregion

        #region Conciliación

        public ErrorDto<List<TesConciliaAsigna>> TES_ConciliacionAsigna_Obtener(int CodEmpresa, string filtros)
        {
            TesConciliaAsignaFiltros filtro = JsonConvert.DeserializeObject<TesConciliaAsignaFiltros>(filtros) ?? new TesConciliaAsignaFiltros();
            return _Db.TES_ConciliacionAsigna_Obtener(CodEmpresa, filtro);
        }

        public ErrorDto TES_Conciliacion_Aplicar(int CodEmpresa, string filtros, List<TesConciliaAsigna> datos)
        {
            TesConciliacionFiltro filtro = JsonConvert.DeserializeObject<TesConciliacionFiltro>(filtros) ?? new TesConciliacionFiltro();
            return _Db.TES_Conciliacion_Aplicar(CodEmpresa, filtro, datos);
        }

        public ErrorDto<List<TesConciliacionDetallesData>> TES_ConciliacionDetalle_Obtener(int CodEmpresa, string filtros)
        {
            TesConciliacionFiltro filtro = JsonConvert.DeserializeObject<TesConciliacionFiltro>(filtros) ?? new TesConciliacionFiltro();
            return _Db.TES_ConciliacionDetalle_Obtener(CodEmpresa, filtro);
        }

        public ErrorDto<List<TesConciliacionDetallesLoteData>> TES_ConciliacionDetalleLote_Obtener(int CodEmpresa, string filtros)
        {
            TesConciliacionFiltro filtro = JsonConvert.DeserializeObject<TesConciliacionFiltro>(filtros) ?? new TesConciliacionFiltro();
            return _Db.TES_ConciliacionDetalleLote_Obtener(CodEmpresa, filtro);
        }

        public ErrorDto TES_Conciliacion_Reversa(int CodEmpresa, string filtros, List<TesConciliacionDetallesData> datos)
        {
            TesConciliacionFiltro filtro = JsonConvert.DeserializeObject<TesConciliacionFiltro>(filtros) ?? new TesConciliacionFiltro();
            return _Db.TES_Conciliacion_Reversa(CodEmpresa, filtro, datos);
        }

        #endregion


    }
}
