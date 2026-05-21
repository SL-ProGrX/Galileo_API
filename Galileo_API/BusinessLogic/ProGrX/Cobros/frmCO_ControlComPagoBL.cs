using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Cobros;
using Galileo_API.DataBaseTier.ProGrX.Cobros;

namespace Galileo_API.BusinessLogic.ProGrX.Cobros
{
    public class FrmCoControlComPagoBL
    {
        private readonly FrmCoControlComPagoDB _db;

        public FrmCoControlComPagoBL(IConfiguration config)
        {
            _db = new FrmCoControlComPagoDB(config);
        }

        public ErrorDto<List<CoControlComPagoRemesaData>> CO_ControlComPago_Remesas_Obtener(int CodEmpresa, int top)
        {
            return _db.CO_ControlComPago_Remesas_Obtener(CodEmpresa, top);
        }

        public ErrorDto<CoControlComPagoRemesaData> CO_ControlComPago_Remesa_Obtener(int CodEmpresa, int cod_remesa)
        {
            return _db.CO_ControlComPago_Remesa_Obtener(CodEmpresa, cod_remesa);
        }

        public ErrorDto<int> CO_ControlComPago_Remesa_Guardar(int CodEmpresa, string usuario, CoControlComPagoRemesaGuardarRequest request)
        {
            return _db.CO_ControlComPago_Remesa_Guardar(CodEmpresa, usuario, request);
        }

        public ErrorDto CO_ControlComPago_Remesa_Eliminar(int CodEmpresa, string usuario, int cod_remesa)
        {
            return _db.CO_ControlComPago_Remesa_Eliminar(CodEmpresa, usuario, cod_remesa);
        }

        public ErrorDto<List<CoControlComPagoRemesaComboData>> CO_ControlComPago_RemesasPorEstado_Obtener(int CodEmpresa, string estado)
        {
            return _db.CO_ControlComPago_RemesasPorEstado_Obtener(CodEmpresa, estado);
        }

        public ErrorDto<List<CoControlComPagoBancoData>> CO_ControlComPago_CargaBancos_Obtener(int CodEmpresa, int cod_remesa)
        {
            return _db.CO_ControlComPago_CargaBancos_Obtener(CodEmpresa, cod_remesa);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> CO_ControlComPago_ReportesOficinas_Obtener(int CodEmpresa)
        {
            return _db.CO_ControlComPago_ReportesOficinas_Obtener(CodEmpresa);
        }

        public ErrorDto<List<CoControlComPagoCargaData>> CO_ControlComPago_CargaPendientes_Obtener(int CodEmpresa, int cod_remesa, int? id_banco)
        {
            return _db.CO_ControlComPago_CargaPendientes_Obtener(CodEmpresa, cod_remesa, id_banco);
        }

        public ErrorDto<CoControlComPagoProcesoResult> CO_ControlComPago_Carga_Aplicar(int CodEmpresa, string usuario, CoControlComPagoCargaAplicarRequest request)
        {
            return _db.CO_ControlComPago_Carga_Aplicar(CodEmpresa, usuario, request);
        }

        public ErrorDto CO_ControlComPago_Remesa_Cerrar(int CodEmpresa, string usuario, int cod_remesa)
        {
            return _db.CO_ControlComPago_Remesa_Cerrar(CodEmpresa, usuario, cod_remesa);
        }

        public ErrorDto<List<CoControlComPagoTrasladoData>> CO_ControlComPago_TrasladoPendientes_Obtener(int CodEmpresa, int cod_remesa)
        {
            return _db.CO_ControlComPago_TrasladoPendientes_Obtener(CodEmpresa, cod_remesa);
        }

        public ErrorDto<CoControlComPagoProcesoResult> CO_ControlComPago_Traslado_Aplicar(int CodEmpresa, string usuario, CoControlComPagoTrasladoAplicarRequest request)
        {
            return _db.CO_ControlComPago_Traslado_Aplicar(CodEmpresa, usuario, request);
        }
    }
}
