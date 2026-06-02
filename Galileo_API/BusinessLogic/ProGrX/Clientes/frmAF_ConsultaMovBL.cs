using Galileo.DataBaseTier;
using Galileo.Models.AF;
using Galileo.Models.ERROR;

namespace Galileo.BusinessLogic.ProGrX.Clientes
{
    public class FrmAfConsultaMovBl
    {
        private readonly FrmAfConsultaMovDb DbfrmAF_ConsultaMov;

        public FrmAfConsultaMovBl(IConfiguration config)
        {
            DbfrmAF_ConsultaMov = new FrmAfConsultaMovDb(config);
        }

        public ErrorDto<List<AfiConsultaMovIngresos>> ConsultaMovIngresos_Obtener(int CodCliente, string cedula)
        {
            return DbfrmAF_ConsultaMov.ConsultaMovIngresos_Obtener(CodCliente, cedula);
        }

        public ErrorDto<List<AfiConsultaMovRenuncias>> ConsultaMovRenuncias_Obtener(int CodCliente, string cedula)
        {
            return DbfrmAF_ConsultaMov.ConsultaMovRenuncias_Obtener(CodCliente, cedula);
        }

        public ErrorDto<List<AfiConsultaMovLiquidaciones>> ConsultaMovLiquidaciones_Obtener(int CodCliente, string cedula)
        {
            return DbfrmAF_ConsultaMov.ConsultaMovLiquidaciones_Obtener(CodCliente, cedula);
        }

        public ErrorDto AF_MovLiquidaciones_Reversion(int CodEmpresa, string usuario, string idLiquidacion, string cedula)
        {
            return DbfrmAF_ConsultaMov.AF_MovLiquidaciones_Reversion(CodEmpresa, usuario, idLiquidacion, cedula);
        }

        public ErrorDto FechaServidor_Obtener(int CodEmpresa)
        {
            return DbfrmAF_ConsultaMov.FechaServidor_Obtener(CodEmpresa);
        }
    }
}
