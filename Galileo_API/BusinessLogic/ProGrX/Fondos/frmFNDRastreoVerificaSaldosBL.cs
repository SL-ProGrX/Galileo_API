using Galileo.DataBaseTier.ProGrX.Fondos;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Fondos;

namespace Galileo.BusinessLogic.ProGrX.Fondos
{
    public class FrmFndRastreoVerificaSaldosBL
    {
        private readonly FrmFndRastreoVerificaSaldosDB _Db;

        public FrmFndRastreoVerificaSaldosBL(IConfiguration? config)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));
            _Db = new FrmFndRastreoVerificaSaldosDB(config);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> Planes_Lista(int CodEmpresa)
        {
            return _Db.Planes_Lista(CodEmpresa);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> Periodos_Lista(int CodEmpresa)
        {
            return _Db.Periodos_Lista(CodEmpresa);
        }

        public ErrorDto<List<FndVerificacionSaldoDto>> VerificacionSaldos_Buscar(
        int CodEmpresa, string Plan, string PeriodoId, int Lineas, bool SoloDiferencias)
        {
            return _Db.VerificacionSaldos_Buscar(CodEmpresa, Plan, PeriodoId, Lineas, SoloDiferencias);
        }
    }
}