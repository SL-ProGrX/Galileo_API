using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.Creditos;
using Galileo_API.Models.ProGrX.Creditos;

namespace Galileo_API.BusinessLogic.ProGrX.Creditos
{
    public class FrmCrReportesRevisionBL
    {
        private readonly FrmCrReportesRevisionDB _db;

        public FrmCrReportesRevisionBL(IConfiguration config)
        {
            _db = new FrmCrReportesRevisionDB(config);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> CR_ReportesRevision_UsuariosGrupos_Obtener(int codEmpresa)
            => _db.CR_ReportesRevision_UsuariosGrupos_Obtener(codEmpresa);

        public ErrorDto<List<DropDownListaGenericaModel>> CR_ReportesRevision_Garantias_Obtener(int codEmpresa)
            => _db.CR_ReportesRevision_Garantias_Obtener(codEmpresa);

        public ErrorDto<List<DropDownListaGenericaModel>> CR_ReportesRevision_Oficinas_Obtener(int codEmpresa)
            => _db.CR_ReportesRevision_Oficinas_Obtener(codEmpresa);

        public ErrorDto<List<DropDownListaGenericaModel>> CR_ReportesRevision_Comites_Obtener(int codEmpresa)
            => _db.CR_ReportesRevision_Comites_Obtener(codEmpresa);

        public ErrorDto<List<DropDownListaGenericaModel>> CR_ReportesRevision_Instituciones_Obtener(int codEmpresa)
            => _db.CR_ReportesRevision_Instituciones_Obtener(codEmpresa);

        public ErrorDto<List<CrAutorizacionTranferenciasTag>> CR_ReportesRevision_Etiquetas_Obtener(int codEmpresa)
            => _db.CR_ReportesRevision_Etiquetas_Obtener(codEmpresa);

        public ErrorDto<List<DropDownListaGenericaModel>> CR_ReportesRevision_Omisiones_Obtener(int codEmpresa)
            => _db.CR_ReportesRevision_Omisiones_Obtener(codEmpresa);

        public ErrorDto<List<DropDownListaGenericaModel>> CR_ReportesRevision_Catalogo_F4_Obtener(int codEmpresa)
            => _db.CR_ReportesRevision_Catalogo_F4_Obtener(codEmpresa);

        public ErrorDto<string?> CR_ReportesRevision_Catalogo_Descripcion_Obtener(int codEmpresa, string codigo)
            => _db.CR_ReportesRevision_Catalogo_Descripcion_Obtener(codEmpresa, codigo);
    }
}
