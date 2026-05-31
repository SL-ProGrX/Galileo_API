using Newtonsoft.Json;
using Galileo.DataBaseTier.ProGrX.Clientes;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Clientes;

namespace Galileo.BusinessLogic.ProGrX.Clientes
{
    public class FrmAFRemesasLiquidacionesBL
    {
        private readonly FrmAFRemesasLiquidacionesDB _db;
        
        public FrmAFRemesasLiquidacionesBL(IConfiguration config)
        {
            _db = new FrmAFRemesasLiquidacionesDB(config);
        }

        #region Remesas
        public ErrorDto<AfRemesasLiquidacionesLista> AF_RemesasLiquidaciones_Remesa_Obtener(int CodEmpresa, string jFiltro)
        {
            FiltrosLazyLoadData filtro = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(jFiltro) ?? new FiltrosLazyLoadData();
            return _db.AF_RemesasLiquidaciones_Remesa_Obtener(CodEmpresa, filtro);
        }

        public ErrorDto<AfRemesaLiquidacionDto?> AF_RemesasLiquidaciones_Remesa_Obtener(int CodEmpresa, int remesa)
        {
            return _db.AF_RemesasLiquidaciones_Remesa_Obtener(CodEmpresa, remesa);
        }

        public ErrorDto AF_RemesasLiquidaciones_Remesa_Guardar(int CodEmpresa, AfRemesaLiquidacionDto remesa)
        {
            return _db.AF_RemesasLiquidaciones_Remesa_Guardar(CodEmpresa, remesa);
        }

        public ErrorDto AF_RemesasLiquidaciones_Remesa_Eliminar(int CodEmpresa, string usuario, int cod_remesa, string estado)
        {
            return _db.AF_RemesasLiquidaciones_Remesa_Eliminar(CodEmpresa, usuario, cod_remesa, estado);
        }
        #endregion

        #region Cargas

        public ErrorDto<List<AfRemesaLiquidacionDto>> AF_RemesasLiquidaciones_Carga_Obtener(int CodEmpresa)
        {
            return _db.AF_RemesasLiquidaciones_Carga_Obtener(CodEmpresa);
        }

        public ErrorDto<AfRemesasLiquiCargaDatos> AF_RemesasLiqui_CargaOficinas_Obtener(int CodEmpresa, int remesa)
        {
            return _db.AF_RemesasLiqui_CargaOficinas_Obtener(CodEmpresa, remesa);
        }

        public ErrorDto<List<AfRemesasLiquiCargaLista>> AF_RemesasLiqui_CargaLista_Obtener(int CodEmpresa, int remesa, string oficina)
        {
            return _db.AF_RemesasLiqui_CargaLista_Obtener(CodEmpresa, remesa, oficina);
        }

        public ErrorDto AF_RemesasLiquidaciones_Carga_Cargar(int CodEmpresa, int remesa, string usuario, List<AfRemesasLiquiCargaLista> datos)
        {
            return _db.AF_RemesasLiquidaciones_Carga_Cargar(CodEmpresa, remesa, usuario, datos);
        }

        public ErrorDto AF_RemesasLiquidaciones_Carga_Cerrar(int CodEmpresa, int remesa, string usuario)
        {
            return _db.AF_RemesasLiquidaciones_Carga_Cerrar(CodEmpresa, remesa, usuario);
        }

        #endregion

        #region Reportes
        public ErrorDto<List<AfRemesaLiquidacionDto>> AF_RemesasLiquidaciones_Reporte_Obtener(int CodEmpresa, DateTime fechaInicio, DateTime fechaCorte, int top)
        {
            return _db.AF_RemesasLiquidaciones_Reporte_Obtener(CodEmpresa, fechaInicio, fechaCorte, top);
        }

        public ErrorDto AF_RemesasLiquidaciones_Reporte_Aplicar(int CodEmpresa, string usuario, int remesa)
        {
            return _db.AF_RemesasLiquidaciones_Reporte_Aplicar(CodEmpresa, usuario, remesa);
        }
        #endregion

        #region Consultas

        public ErrorDto<string> AF_RemesasLiquidaciones_Consultas_Obtener(int CodEmpresa, string consec)
        {
            return _db.AF_RemesasLiquidaciones_Consultas_Obtener(CodEmpresa, consec);
        }

        #endregion
    }
}