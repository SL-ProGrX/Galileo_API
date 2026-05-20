using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Procesos;
using Galileo_API.DataBaseTier.ProGrX.Patrimonio;

namespace Galileo_API.BusinessLogic.ProGrX.Patrimonio
{
    public class FrmAHPrincipalBL
    {
        private readonly FrmAHPrincipalDB _db;

        public FrmAHPrincipalBL(IConfiguration config)
        {
            _db = new FrmAHPrincipalDB(config);
        }

        public ErrorDto<FrmAhPrincipalConsultaResponse?> Patrimonio_frmAH_Principal_Consulta_Obtener(
            int codEmpresa,
            string cedula,
            string usuario)
            => _db.Patrimonio_frmAH_Principal_Consulta_Obtener(codEmpresa, cedula, usuario);

        public ErrorDto<List<FrmAhPrincipalDetallePatrimonioResponse>> Patrimonio_frmAH_Principal_DetallePatrimonio_Obtener(
            int codEmpresa,
            FrmAhPrincipalDetallePatrimonioRequest request)
            => _db.Patrimonio_frmAH_Principal_DetallePatrimonio_Obtener(codEmpresa, request);

        public ErrorDto<List<FrmAhPrincipalExcedentesResponse>> Patrimonio_frmAH_Principal_Excedentes_Obtener(
            int codEmpresa,
            string cedula)
            => _db.Patrimonio_frmAH_Principal_Excedentes_Obtener(codEmpresa, cedula);

        public ErrorDto<List<FrmAhPrincipalHistoricoResponse>> Patrimonio_frmAH_Principal_Historico_Obtener(
            int codEmpresa,
            string cedula)
            => _db.Patrimonio_frmAH_Principal_Historico_Obtener(codEmpresa, cedula);

        public ErrorDto<List<FrmAhPrincipalLiquidacionesResponse>> Patrimonio_frmAH_Principal_Liquidaciones_Obtener(
            int codEmpresa,
            string cedula)
            => _db.Patrimonio_frmAH_Principal_Liquidaciones_Obtener(codEmpresa, cedula);
    }
}
