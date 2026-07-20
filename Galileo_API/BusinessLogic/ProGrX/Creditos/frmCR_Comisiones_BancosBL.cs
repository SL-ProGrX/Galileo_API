using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.Creditos;
using Galileo_API.Models.ProGrX.Creditos;

namespace Galileo_API.BusinessLogic.ProGrX.Creditos
{
    public class FrmCrComisionesBancosBl
    {
        private readonly FrmCrComisionesBancosDb _db;

        public FrmCrComisionesBancosBl(IConfiguration config)
        {
            _db = new FrmCrComisionesBancosDb(config);
        }

        public ErrorDto<List<CrComisionesBancosItem>>
            CR_frmCR_Comisiones_Bancos_Inicializar(
                int codEmpresa,
                CrComisionesBancosInicializarRequest request)
        {
            return _db.CR_frmCR_Comisiones_Bancos_Inicializar(
                codEmpresa,
                request);
        }

        public ErrorDto<List<CrComisionesBancosItem>>
            CR_frmCR_Comisiones_Bancos_Obtener(int codEmpresa)
        {
            return _db.CR_frmCR_Comisiones_Bancos_Obtener(codEmpresa);
        }

        public ErrorDto CR_frmCR_Comisiones_Bancos_Actualizar(
            int codEmpresa,
            CrComisionesBancosActualizarRequest request)
        {
            return _db.CR_frmCR_Comisiones_Bancos_Actualizar(
                codEmpresa,
                request);
        }
    }
}