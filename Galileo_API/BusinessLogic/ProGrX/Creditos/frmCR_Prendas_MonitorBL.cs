using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.Creditos;
using Galileo_API.Models.ProGrX.Creditos;

namespace Galileo_API.BusinessLogic.ProGrX.Creditos
{
    public class FrmCrPrendasMonitorBl
    {
        private readonly FrmCrPrendasMonitorDb _db;

        public FrmCrPrendasMonitorBl(IConfiguration config)
        {
            _db = new FrmCrPrendasMonitorDb(config);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> CrPrendasMonitor_TiposPrenda_Obtener(int codEmpresa)
            => _db.CrPrendasMonitor_TiposPrenda_Obtener(codEmpresa);

        public ErrorDto<List<DropDownListaGenericaModel>> CrPrendasMonitor_Catalogo_Obtener(int codEmpresa, string tipo)
            => _db.CrPrendasMonitor_Catalogo_Obtener(codEmpresa, tipo);

        public ErrorDto<List<DropDownListaGenericaModel>> CrPrendasMonitor_EstadosPersona_Obtener(int codEmpresa)
            => _db.CrPrendasMonitor_EstadosPersona_Obtener(codEmpresa);

        public ErrorDto<List<DropDownListaGenericaModel>> CrPrendasMonitor_UnidadesCilindraje_Obtener(int codEmpresa, string tipo)
            => _db.CrPrendasMonitor_UnidadesCilindraje_Obtener(codEmpresa, tipo);

        public ErrorDto<List<CrPrendasMonitorConsultaData>> CrPrendasMonitor_Consulta_Obtener(
            int codEmpresa,
            CrPrendasMonitorConsultaRequest request)
            => _db.CrPrendasMonitor_Consulta_Obtener(codEmpresa, request);
    }
}
