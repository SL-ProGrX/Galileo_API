using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.Conciliacion;
using Galileo_API.Models.ProGrX_Conciliacion;

namespace Galileo_API.BusinessLogic.ProGrX.Conciliacion
{
    public class FrmVerificaSaldosAsebl
    {
        private readonly FrmVerificaSaldosAsedb _db;

        public FrmVerificaSaldosAsebl(IConfiguration config)
        {
            _db = new FrmVerificaSaldosAsedb(config);
        }

        public ErrorDto<AseVerificaSaldosInicialData> ASE_VerificaSaldos_Inicial_Obtener(int CodEmpresa)
        {
            return _db.ASE_VerificaSaldos_Inicial_Obtener(CodEmpresa);
        }

        public ErrorDto<List<AseVerificaSaldosPeriodoData>> ASE_VerificaSaldos_Periodos_Dropdown_Obtener(int CodEmpresa)
        {
            return _db.ASE_VerificaSaldos_Periodos_Dropdown_Obtener(CodEmpresa);
        }

        public ErrorDto<AseVerificaSaldosListaResult> ASE_VerificaSaldos_Lista_Obtener(int CodEmpresa, AseVerificaSaldosListaRequest? request)
        {
            return _db.ASE_VerificaSaldos_Lista_Obtener(CodEmpresa, request);
        }

        public ErrorDto<AseVerificaSaldosListaResult> ASE_VerificaSaldos_Lista_Export(int CodEmpresa, AseVerificaSaldosListaRequest? request)
        {
            return _db.ASE_VerificaSaldos_Lista_Export(CodEmpresa, request);
        }
    }
}