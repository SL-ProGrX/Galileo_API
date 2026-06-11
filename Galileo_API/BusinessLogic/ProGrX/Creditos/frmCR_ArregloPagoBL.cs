using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.Creditos;
using Galileo_API.Models.ProGrX.Credito;

namespace Galileo_API.BusinessLogic.ProGrX.Creditos
{
    public class FrmCrArregloPagoBl
    {
        private readonly FrmCrArregloPagoDb _db;

        public FrmCrArregloPagoBl(IConfiguration config)
        {
            _db = new FrmCrArregloPagoDb(config);
        }

        public ErrorDto<CrArregloPagoCajaInicialData> Cr_ArregloPago_CajaInicial_Obtener(
            int codEmpresa,
            string caja,
            string usuario)
            => _db.Cr_ArregloPago_CajaInicial_Obtener(codEmpresa, caja, usuario);

        public ErrorDto<CrArregloPagoOperacionData?> Cr_ArregloPago_Operacion_Obtener(
            int codEmpresa,
            int operacion,
            string usuario,
            bool tipoIntereses = false)
            => _db.Cr_ArregloPago_Operacion_Obtener(codEmpresa, operacion, usuario, tipoIntereses);

        public ErrorDto<CrArregloPagoAplicacionResultadoData> Cr_ArregloPago_Capitaliza_Aplicar(
            int codEmpresa,
            CrArregloPagoCapitalizaRequest request)
            => _db.Cr_ArregloPago_Capitaliza_Aplicar(codEmpresa, request);

        public ErrorDto Cr_ArregloPago_PeriodoGracia_Aplicar(
            int codEmpresa,
            CrArregloPagoPeriodoGraciaRequest request)
            => _db.Cr_ArregloPago_PeriodoGracia_Aplicar(codEmpresa, request);

        public ErrorDto<CrArregloPagoAplicacionResultadoData> Cr_ArregloPago_VencimientoIntereses_Aplicar(
            int codEmpresa,
            CrArregloPagoVencimientoInteresesRequest request)
            => _db.Cr_ArregloPago_VencimientoIntereses_Aplicar(codEmpresa, request);

        public ErrorDto<CrArregloPagoAplicacionResultadoData> Cr_ArregloPago_AbonoEspecial_Aplicar(
            int codEmpresa,
            CrArregloPagoAbonoEspecialRequest request)
            => _db.Cr_ArregloPago_AbonoEspecial_Aplicar(codEmpresa, request);
    }
}