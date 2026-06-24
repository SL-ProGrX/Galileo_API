using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.Creditos;

namespace Galileo_API.BusinessLogic.ProGrX.Creditos
{
    public class FrmCrReportesMovBl
    {
        private readonly FrmCrReportesMovDb _db;

        public FrmCrReportesMovBl(IConfiguration config)
        {
            _db = new FrmCrReportesMovDb(config);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> CrReportesMov_Documentos_Obtener(int codEmpresa)
            => _db.CrReportesMov_Documentos_Obtener(codEmpresa);

        public ErrorDto<List<DropDownListaGenericaModel>> CrReportesMov_Conceptos_Obtener(int codEmpresa)
            => _db.CrReportesMov_Conceptos_Obtener(codEmpresa);

        public ErrorDto<List<DropDownListaGenericaModel>> CrReportesMov_Instituciones_Obtener(int codEmpresa)
            => _db.CrReportesMov_Instituciones_Obtener(codEmpresa);

        public ErrorDto<List<DropDownListaGenericaModel>> CrReportesMov_Grupos_Obtener(
            int codEmpresa,
            bool lineaActiva,
            string? codigo)
            => _db.CrReportesMov_Grupos_Obtener(codEmpresa, lineaActiva, codigo);

        public ErrorDto<List<DropDownListaGenericaModel>> CrReportesMov_Destinos_Obtener(
            int codEmpresa,
            bool lineaActiva,
            string? codigo)
            => _db.CrReportesMov_Destinos_Obtener(codEmpresa, lineaActiva, codigo);

        public ErrorDto<List<DropDownListaGenericaModel>> CrReportesMov_Lineas_Obtener(int codEmpresa)
            => _db.CrReportesMov_Lineas_Obtener(codEmpresa);

        public ErrorDto<List<DropDownListaGenericaModel>> CrReportesMov_Oficinas_Obtener(int codEmpresa)
            => _db.CrReportesMov_Oficinas_Obtener(codEmpresa);

        public ErrorDto<List<DropDownListaGenericaModel>> CrReportesMov_Garantias_Obtener(int codEmpresa)
            => _db.CrReportesMov_Garantias_Obtener(codEmpresa);

        public ErrorDto<List<DropDownListaGenericaModel>> CrReportesMov_Divisas_Obtener(int codEmpresa)
            => _db.CrReportesMov_Divisas_Obtener(codEmpresa);

        public ErrorDto<List<DropDownListaGenericaModel>> CrReportesMov_Cargos_Obtener(int codEmpresa)
            => _db.CrReportesMov_Cargos_Obtener(codEmpresa);

        public ErrorDto<List<DropDownListaGenericaModel>> CrReportesMov_Aseguradoras_Obtener(int codEmpresa)
            => _db.CrReportesMov_Aseguradoras_Obtener(codEmpresa);

        public ErrorDto<List<DropDownListaGenericaModel>> CrReportesMov_Polizas_Obtener(int codEmpresa)
            => _db.CrReportesMov_Polizas_Obtener(codEmpresa);

        public ErrorDto<List<DropDownListaGenericaModel>> CrReportesMov_Gestores_Obtener(int codEmpresa)
            => _db.CrReportesMov_Gestores_Obtener(codEmpresa);
    }
}
