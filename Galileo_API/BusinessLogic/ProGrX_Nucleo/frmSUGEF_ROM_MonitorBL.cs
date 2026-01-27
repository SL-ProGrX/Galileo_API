using Galileo.Models.ERROR;
using Galileo.Models;
using Galileo_API.DataBaseTier.ProGrX_Nucleo;
using Galileo_API.Models.ProGrX_Nucleo;

namespace Galileo_API.BusinessLogic.ProGrX_Nucleo
{
    public class FrmSugefRomMonitorBL
    {
        private readonly FrmSugefRomMonitorDB _db;

        public FrmSugefRomMonitorBL(IConfiguration config)
        {
            _db = new FrmSugefRomMonitorDB(config);
        }

        public ErrorDto<SugefTipoCambioResult?> SUGEF_TipoCambio_Obtener(int codEmpresa, DateTime fecha)
        {
            return _db.SUGEF_TipoCambio_Obtener(codEmpresa, fecha);
        }

        public ErrorDto<List<SugefRomMonitorConsultaResult>> SUGEF_ROM_Monitor_Consulta(int codEmpresa, DateTime corte)
        {
            return _db.SUGEF_ROM_Monitor_Consulta(codEmpresa, corte);
        }

        public ErrorDto<List<SugefRomMonitorDetalleResult>> SUGEF_ROM_Monitor_Detalle(int codEmpresa, DateTime corte, int rom)
        {
            return _db.SUGEF_ROM_Monitor_Detalle(codEmpresa, corte, rom);
        }

        public ErrorDto<List<SugefRomMonitorFormaPagoResult>> SUGEF_ROM_Monitor_Forma_Pago(int codEmpresa, DateTime corte, string tipoDoc, string numDoc)
        {
            return _db.SUGEF_ROM_Monitor_Forma_Pago(codEmpresa, corte, tipoDoc, numDoc);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> SUGEF_EntidadesPago_Lista(int codEmpresa)
        {
            return _db.SUGEF_EntidadesPago_Lista(codEmpresa);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> SUGEF_OrigenRecursos_Lista(int codEmpresa)
        {
            return _db.SUGEF_OrigenRecursos_Lista(codEmpresa);
        }

        public ErrorDto<bool> SUGEF_ROM_Monitor(int codEmpresa, SugefRomMonitorParams param)
        {
            return _db.SUGEF_ROM_Monitor(codEmpresa, param);
        }

        public ErrorDto<bool> SUGEF_ROM_Monitor_Forma_Pago_Actualiza(int codEmpresa, SugefRomMonitorFormaPagoActualizaParams param)
        {
            return _db.SUGEF_ROM_Monitor_Forma_Pago_Actualiza(codEmpresa, param);
        }
    }
}
