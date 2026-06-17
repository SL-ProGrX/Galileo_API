using Galileo.Models.AH;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.Patrimonio;

namespace Galileo_API.BusinessLogic.ProGrX.Patrimonio
{
    public class FrmAhExcedentesAjusteBL
    {
        private readonly FrmAhExcedentesAjusteDB _db;

        public FrmAhExcedentesAjusteBL(IConfiguration config)
        {
            _db = new FrmAhExcedentesAjusteDB(config);
        }

        public ErrorDto<FrmAhExcedentesAjusteCargarResponse> AH_ExcedentesAjuste_Cargar(
            int codEmpresa,
            FrmAhExcedentesAjustePendienteListaRequest? request)
        {
            return _db.AH_ExcedentesAjuste_Cargar(codEmpresa, request);
        }

        public ErrorDto<List<ExcPeriodosDto>> AH_ExcedentesAjuste_Periodos_Lista(int codEmpresa)
        {
            return _db.AH_ExcedentesAjuste_Periodos_Lista(codEmpresa);
        }

        public ErrorDto<List<FrmAhExcedentesAjustePendienteDto>> AH_ExcedentesAjuste_Pendientes_Lista(
            int codEmpresa,
            FrmAhExcedentesAjustePendienteListaRequest? request)
        {
            return _db.AH_ExcedentesAjuste_Pendientes_Lista(codEmpresa, request);
        }

        public ErrorDto<FrmAhExcedentesAjusteCedulaDto> AH_ExcedentesAjuste_Cedula_Consultar(
            int codEmpresa,
            string cedula)
        {
            return _db.AH_ExcedentesAjuste_Cedula_Consultar(codEmpresa, cedula);
        }

        public ErrorDto<FrmAhExcedentesAjusteProcesoResponse> AH_ExcedentesAjuste_Guardar(
            int codEmpresa,
            FrmAhExcedentesAjusteGuardarRequest? request)
        {
            return _db.AH_ExcedentesAjuste_Guardar(codEmpresa, request);
        }

        public ErrorDto<FrmAhExcedentesAjusteProcesoResponse> AH_ExcedentesAjuste_Eliminar(
            int codEmpresa,
            int ajusteId,
            string usuario)
        {
            return _db.AH_ExcedentesAjuste_Eliminar(codEmpresa, ajusteId, usuario);
        }
    }
}
