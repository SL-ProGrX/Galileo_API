using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Clientes;

namespace Galileo.BusinessLogic.ProGrX.Clientes
{
    public class FrmAFPlanillaEnviaBL
    {
        private readonly FrmAFPlanillaEnviaDB _db;

        public FrmAFPlanillaEnviaBL(IConfiguration config)
        {
            _db = new FrmAFPlanillaEnviaDB(config);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> AF_Instituciones_Obtener(int CodEmpresa)
        {
            return _db.AF_Instituciones_Obtener(CodEmpresa);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> AF_PeriodosProceso_Obtener(int CodEmpresa)
        {
            return _db.AF_PeriodosProceso_Obtener(CodEmpresa);
        }

        public ErrorDto<List<AfArchivoResultadoDto>> AF_Archivo_Obtener(int CodEmpresa, string codinstitucion, string fechaproceso)
        {
            return _db.AF_Archivo_Obtener(CodEmpresa, codinstitucion, fechaproceso);
        }

        public ErrorDto<AfArchivoPlanillaDto> AF_ArchivoPlanilla_Obtener(int CodEmpresa, string codinstitucion, string fechaproceso)
        {
            return _db.AF_ArchivoPlanilla_Obtener(CodEmpresa, codinstitucion, fechaproceso);
        }
    }
}
