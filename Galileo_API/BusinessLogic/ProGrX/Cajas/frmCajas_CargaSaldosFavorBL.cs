using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.Cajas;
using Galileo_API.Models.ProGrX.Cajas;

namespace Galileo_API.BusinessLogic.ProGrX.Cajas
{
    public class FrmCajasCargaSaldosFavorBL
    {
        private readonly FrmCajasCargaSaldosFavorDB _db;

        public FrmCajasCargaSaldosFavorBL(IConfiguration config)
        {
            _db = new FrmCajasCargaSaldosFavorDB(config);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> CargaSaldosFavor_Tipos_Obtener(int codEmpresa)
        {
            return _db.CargaSaldosFavor_Tipos_Obtener(codEmpresa);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> CargaSaldosFavor_EntidadesPagadoras_Obtener(int codEmpresa, bool ordenPorDescripcion)
        {
            return _db.CargaSaldosFavor_EntidadesPagadoras_Obtener(codEmpresa, ordenPorDescripcion);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> CargaSaldosFavor_OrigenRecursos_Obtener(int codEmpresa, bool ordenPorDescripcion)
        {
            return _db.CargaSaldosFavor_OrigenRecursos_Obtener(codEmpresa, ordenPorDescripcion);
        }

        public ErrorDto<List<CajasSaldoFavorTipoLiquidacionResult>> CargaSaldosFavor_TipoLiquidacion_Obtener(int codEmpresa, CajasSaldoFavorTipoLiquidacionParams param)
        {
            return _db.CargaSaldosFavor_TipoLiquidacion_Obtener(codEmpresa, param);
        }

        public ErrorDto<List<CajasSaldosFavorConsultaResult>> CargaSaldosFavor_Consulta(CajasSaldosFavorConsultaParams param)
        {
            return _db.CargaSaldosFavor_Consulta(param);
        }

        public ErrorDto<List<CajasDepositosCuentaBancariaAutResult>> CargaSaldosFavor_CuentasBancariasAut_Obtener(int codEmpresa, CajasDepositosCuentaBancariaAutParams param)
        {
            return _db.CargaSaldosFavor_CuentasBancariasAut_Obtener(codEmpresa, param);
        }

        public ErrorDto<List<CajasDepositosTramiteIdentificaResult>> Cajas_DepositosTramiteIdentifica_Consulta(CajasDepositosTramiteIdentificaParams param)
        {
            return _db.Cajas_DepositosTramiteIdentifica_Consulta(param);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> CargaSaldosFavor_FormasPago_Obtener(int codEmpresa)
        {
            return _db.CargaSaldosFavor_FormasPago_Obtener(codEmpresa);
        }

        public ErrorDto<CajasFormasPagoTipoResult?> CargaSaldosFavor_FormaPagoTipo_Obtener(int codEmpresa, string codFormaPago)
        {
            return _db.CargaSaldosFavor_FormaPagoTipo_Obtener(codEmpresa, codFormaPago);
        }

        public ErrorDto<CajasDepositosCargadoResult?> Cajas_DepositosCargado_Existe(CajasDepositosCargadoParams param)
        {
            return _db.Cajas_DepositosCargado_Existe(param);
        }

        public ErrorDto<bool> Cajas_DepositosTramite_Insertar(CajasDepositosTramiteInsertParams param)
        {
            return _db.Cajas_DepositosTramite_Insertar(param);
        }

        public ErrorDto<bool> Cajas_IdentificaTesDepositos(CajasIdentificaTesDepositosParams param)
        {
            return _db.Cajas_IdentificaTesDepositos(param);
        }

        public ErrorDto<bool> Cajas_DepositosTramiteInconsistencia_Insertar(CajasDepositosTramiteInconsistenciaInsertParams param)
        {
            return _db.Cajas_DepositosTramiteInconsistencia_Insertar(param);
        }

        public ErrorDto<bool> Cajas_SaldoFavorCarga(CajasSaldoFavorCargaParams param)
        {
            return _db.Cajas_SaldoFavorCarga(param);
        }

        public ErrorDto<bool> Cajas_IdentificaTesDepositos_Full(CajasIdentificaTesDepositosFullParams param)
        {
            return _db.Cajas_IdentificaTesDepositos_Full(param);
        }

        public ErrorDto<bool> Cajas_NotificaDepositos(CajasNotificaDepositosParams param)
        {
            return _db.Cajas_NotificaDepositos(param);
        }

        public ErrorDto<bool> Cajas_SaldoFavorLiquidacion(CajasSaldoFavorLiquidacionParams param)
        {
            return _db.Cajas_SaldoFavorLiquidacion(param);
        }
    }

}
