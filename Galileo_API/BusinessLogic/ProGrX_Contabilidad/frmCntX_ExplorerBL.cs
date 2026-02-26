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
            => _db.Cntx_Cuentas_Obtener(codEmpresa);

        public ErrorDto<List<DropDownListaGenericaModel>> TiposAsiento_Obtener(int codEmpresa, int cod_contabilidad)
            => _db.Cntx_TiposAsiento_Obtener(codEmpresa, cod_contabilidad);

        public ErrorDto<List<CntxPeriodoDto>> Periodos_Obtener(int codEmpresa, string estado)
            => _db.Cntx_Periodos_Obtener(codEmpresa, estado);

        #endregion

        #region LISTADOS

        public ErrorDto<List<CntxAsientoRsmDto>> Asientos_Listar(int codEmpresa, int cod_contabilidad, CntxExploradorFiltrosDto filtros)
            => _db.Cntx_Asientos_Listar(codEmpresa, cod_contabilidad, filtros);

        public ErrorDto<List<CntxAsientoDetDto>> AsientoDetalle_Listar(
            int codEmpresa,
            CntxExploradorFiltrosDto filtros)
            => _db.AsientoDetalle_Listar(codEmpresa, filtros);

        #endregion

        #region AUX

        public ErrorDto<string> FechaServidor_Obtener(int codEmpresa)
            => _db.FechaServidor_Obtener(codEmpresa);

        #endregion


        public ErrorDto<List<CntxCuentaDto>> CuentasPorPadre(int codEmpresa, string? codCuentaPadre)
        => _db.CuentasPorPadre(codEmpresa, codCuentaPadre);


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

        public ErrorDto<List<DropDownListaGenericaModel>> DiferidoPlantillas_Obtener(int codEmpresa, int codContabilidad, int codDiferido)
        {
            return _db.DiferidoPlantillas_Obtener(codEmpresa, codContabilidad, codDiferido);
        }

        public ErrorDto<List<CntxDiferidoHistoricoDto>> DiferidoHistorico_Obtener(int codEmpresa, int codContabilidad, int codDiferido, int codPlantilla)
        {
            return _db.DiferidoHistorico_Obtener(codEmpresa, codContabilidad, codDiferido, codPlantilla);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> Cntx_TiposAsientos_Buscar(int codEmpresa, int cod_contabilidad)
      => _db.Cntx_TiposAsientos_Buscar(codEmpresa, cod_contabilidad);

        public ErrorDto<List<DropDownListaGenericaModel>> Cntx_Unidades_Buscar(int codEmpresa, int cod_contabilidad)
            => _db.Cntx_Unidades_Buscar(codEmpresa, cod_contabilidad);

        public ErrorDto<List<DropDownListaGenericaModel>> Cntx_CentroCosto_Buscar(int codEmpresa, int cod_contabilidad)
            => _db.Cntx_CentroCosto_Buscar(codEmpresa, cod_contabilidad);

        public ErrorDto<List<DropDownListaGenericaModel>> Cntx_Divisas_Buscar(int codEmpresa, int cod_contabilidad)
            => _db.Cntx_Divisas_Buscar(codEmpresa, cod_contabilidad);


        public ErrorDto<List<CntxAsientoResumenDto>> Asientos_Resumen(int codEmpresa,int cod_contabilidad,int anio,int mes)
        {
            return _db.Asientos_Resumen(codEmpresa,cod_contabilidad,anio,mes
            );
        }

    }
}