using Galileo.DataBaseTier.ProGrX_Nucleo;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Nucleo;

namespace Galileo.BusinessLogic.ProGrX_Nucleo
{
    public class FrmSifFormasDePagoBL(IConfiguration config)
    {
        private readonly FrmSifFormasDePagoDB _db = new FrmSifFormasDePagoDB(config);

        public ErrorDto<SifFormasPago> SIF_Formas_Pago_Obtener(int codEmpresa, string codFormaPago)
        {
            return _db.SIF_Formas_Pago_Obtener(codEmpresa, codFormaPago);
        }

        public ErrorDto<string> SIF_Formas_Pago_Obtener_SigAnt(int codEmpresa, string? codFormaPagoActual, string orden)
        {
            return _db.SIF_Formas_Pago_Obtener_SigAnt(codEmpresa, codFormaPagoActual, orden);
        }

        public ErrorDto SIF_Formas_Pago_Guardar(int codEmpresa, SifFormasPago model)
        {
            return _db.SIF_Formas_Pago_Guardar(codEmpresa, model);
        }

        public ErrorDto<List<SifFormasPagoList>> SIF_Formas_Pago_Obtener_Lista(int codEmpresa, string? filtro)
        {
            return _db.SIF_Formas_Pago_Obtener_Lista(codEmpresa, filtro);
        }

        public List<SysCuentasBancariasList> CuentasBancarias_Obtener_Lista(int CodEmpresa, string codFormaPago)
        {
            return _db.CuentasBancarias_Obtener_Lista(CodEmpresa, codFormaPago);
        }

        public ErrorDto CuentasBancarias_Asignar(int codEmpresa, SifFormasPagoBancoAsgDto data)
        {
            return _db.CuentasBancarias_Asignar(codEmpresa, data);
        }
    }
}