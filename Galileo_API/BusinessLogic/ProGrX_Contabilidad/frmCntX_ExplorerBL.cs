using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX_Contabilidad;
using Galileo_API.Models.ProGrX_Contabilidad;
using Galileo_API.Models.ProGrX_Contabilidad.Galileo_API.Models.ProGrX_Contabilidad;

namespace Galileo_API.BusinessLogic.ProGrX_Contabilidad
{
    public class FrmCntXExploradorContableBl
    {
        private readonly FrmCntXExploradorContableDb _db;

        public FrmCntXExploradorContableBl(IConfiguration config)
        {
            _db = new FrmCntXExploradorContableDb(config);
        }

        #region TREE

        public ErrorDto<List<DropDownListaGenericaModel>> Cuentas_Obtener(int codEmpresa)
        {
            return _db.Cntx_Cuentas_Obtener(codEmpresa);
        }

        public ErrorDto<List<CntxTipoAsientoDto>> TiposAsiento_Obtener(int codEmpresa, int cod_contabilidad)
        {
            return _db.Cntx_TiposAsiento_Obtener(codEmpresa, cod_contabilidad);
        }

        public ErrorDto<List<CntxPeriodoDto>> Periodos_Obtener(int codEmpresa, int cod_contabilidad, string estado)
        {
            return _db.Cntx_Periodos_Obtener(codEmpresa, cod_contabilidad, estado);
        }

        #endregion

        #region LISTADOS

        public ErrorDto<List<CntxAsientoRsmDto>> Asientos_Listar(int codEmpresa, int cod_contabilidad, CntxExploradorFiltrosDto filtros)
        {
            return _db.Cntx_Asientos_Listar(codEmpresa, cod_contabilidad, filtros);
        }

        public ErrorDto<List<CntxAsientoDetDto>> AsientoDetalle_Listar(int codEmpresa, CntxExploradorFiltrosDto filtros)
        {
            return _db.AsientoDetalle_Listar(codEmpresa, filtros);
        }

        public ErrorDto<List<CntxConsultaAnaliticaDto>> ConsultaAnalitica(
            int codEmpresa,
            int codContabilidad,
            CntxExploradorFiltrosDto filtros)
        {
            return _db.ConsultaAnalitica(codEmpresa, codContabilidad, filtros);
        }

        public ErrorDto<CntxMovimientoNodoDto> MovimientosNodo(CntxMovimientoNodoRequest request)
        {
            return _db.MovimientosNodo(request);
        }

        #endregion

        #region AUX

        public ErrorDto<string> FechaServidor_Obtener(int codEmpresa)
        {
            return _db.FechaServidor_Obtener(codEmpresa);
        }

        #endregion

        public ErrorDto<List<CntxCuentaDto>> CuentasPorPadre(int codEmpresa, int cod_contabilidad, string? codCuentaPadre)
        {
            return _db.CuentasPorPadre(codEmpresa, cod_contabilidad, codCuentaPadre);
        }

        public ErrorDto<List<CntxAsientoTreeDto>> AsientosTreePorTipo(int codEmpresa, int cod_contabilidad, string tipo, int anio, int mes)
        {
            return _db.Cntx_Asientos_TreePorTipo(codEmpresa, cod_contabilidad, tipo, anio, mes);
        }

        public ErrorDto<List<CntxTipoCuentaDto>> Cntx_TiposCuenta_Obtener(int codEmpresa, int codContabilidad)
        {
            return _db.Cntx_TiposCuenta_Obtener(codEmpresa, codContabilidad);
        }

        public ErrorDto<List<CntxCuentaDto>> Cntx_CuentasRaizPorTipo_Obtener(int codEmpresa, int codContabilidad, string tipoCuenta)
        {
            return _db.Cntx_CuentasRaizPorTipo_Obtener(codEmpresa, codContabilidad, tipoCuenta);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> Diferidos_Obtener(int codEmpresa, int codContabilidad)
        {
            return _db.Diferidos_Obtener(codEmpresa, codContabilidad);
        }

        public ErrorDto<List<CntxDiferidoPlantillaDto>> DiferidoPlantillas_Obtener(int codEmpresa, int codContabilidad, int codDiferido)
        {
            return _db.DiferidoPlantillas_Obtener(codEmpresa, codContabilidad, codDiferido);
        }

        public ErrorDto<List<CntxDiferidoHistoricoDto>> DiferidoHistorico_Obtener(int codEmpresa, int codContabilidad, int codDiferido, int codPlantilla)
        {
            return _db.DiferidoHistorico_Obtener(codEmpresa, codContabilidad, codDiferido, codPlantilla);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> Cntx_TiposAsientos_Buscar(int codEmpresa, int cod_contabilidad)
        {
            return _db.Cntx_TiposAsientos_Buscar(codEmpresa, cod_contabilidad);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> Cntx_Unidades_Buscar(int codEmpresa, int cod_contabilidad)
        {
            return _db.Cntx_Unidades_Buscar(codEmpresa, cod_contabilidad);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> Cntx_CentroCosto_Buscar(int codEmpresa, int cod_contabilidad)
        {
            return _db.Cntx_CentroCosto_Buscar(codEmpresa, cod_contabilidad);
        }

        public ErrorDto<List<CntxDivisaDto>> Cntx_Divisas_Buscar(int codEmpresa, int cod_contabilidad)
        {
            return _db.Cntx_Divisas_Buscar(codEmpresa, cod_contabilidad);
        }

        public ErrorDto<List<CntxAsientoResumenDto>> Asientos_Resumen(int codEmpresa, int cod_contabilidad, int anio, int mes)
        {
            return _db.Asientos_Resumen(codEmpresa, cod_contabilidad, anio, mes);
        }

        public ErrorDto<List<CntxCatalogoResumenDto>> Catalogo_Resumen(CatalogoResumenRequest request)
        {
            return _db.Catalogo_Resumen(request);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> PlantillaRate_Obtener(int codEmpresa, int codContabilidad)
        {
            return _db.PlantillaRate_Obtener(codEmpresa, codContabilidad);
        }

        public ErrorDto<List<CntxPlantillaRateDetalleDto>> PlantillaRate_Detalle(int codEmpresa, int codContabilidad, int codPlantilla)
        {
            return _db.PlantillaRate_Detalle(codEmpresa, codContabilidad, codPlantilla);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> PlantillasFijas_Obtener(
            int codEmpresa,
            int codContabilidad)
        {
            return _db.PlantillasFijas_Obtener(codEmpresa, codContabilidad);
        }

        public ErrorDto<List<CntxPlantillaFijaDetalleDto>> PlantillaFija_Detalle(
            int codEmpresa,
            int codContabilidad,
            int codPlantilla)
        {
            return _db.PlantillaFija_Detalle(
                codEmpresa,
                codContabilidad,
                codPlantilla);
        }

        public ErrorDto<List<AreaTrabajoDto>> AreasTrabajo_ObtenerPorPadre(int codEmpresa, int codContabilidad)
        {
            return _db.AreasTrabajo_ObtenerPorPadre(codEmpresa, codContabilidad);
        }

        public ErrorDto<List<AreaCuentaDto>> AreasTrabajo_Cuentas(int codEmpresa, int codContabilidad, int codArea)
        {
            return _db.AreasTrabajo_Cuentas(codEmpresa, codContabilidad, codArea);
        }

        public ErrorDto<List<AreaResumenDto>> AreasTrabajo_Resumen(int codEmpresa, int codContabilidad, int codArea, DateTime fechaDesde, DateTime fechaHasta)
        {
            return _db.AreasTrabajo_Resumen(codEmpresa, codContabilidad, codArea, fechaDesde, fechaHasta);
        }

        public ErrorDto<List<CntxContabilidadDto>> ObtenerContabilidades(int codEmpresa)
        {
            return _db.ObtenerContabilidades(codEmpresa);
        }

        public ErrorDto<CntxConfiguracionArbolDto> ConfiguracionArbol_Obtener(
            int codEmpresa,
            int codContabilidad)
        {
            return _db.ConfiguracionArbol_Obtener(codEmpresa, codContabilidad);
        }

        public ErrorDto<List<CntxCierreDto>> ObtenerCierres(int codEmpresa, int cod_contabilidad)
        {
            return _db.ObtenerCierres(codEmpresa, cod_contabilidad);
        }

        public ErrorDto<bool> Asiento_Mayorizar(CntxMayorizarRequest dto)
        {
            return _db.Asientos_Mayorizar(dto);
        }

        public ErrorDto<bool> Asiento_Borrar(CntxBorrarAsientoRequest dto)
        {
            return _db.Asiento_Borrar(dto);
        }

        public ErrorDto<string?> NotasAsiento(int codEmpresa, int cod_contabilidad, string tipo_asiento, string num_asiento)
        {
            return _db.NotasAsiento(codEmpresa, cod_contabilidad, tipo_asiento, num_asiento);
        }
    }
}
