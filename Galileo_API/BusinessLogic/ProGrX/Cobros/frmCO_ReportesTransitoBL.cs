using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.Cobros;
using Galileo_API.Models.ProGrX.Cobros;

namespace Galileo_API.BusinessLogic.ProGrX.Cobros
{
    public class FrmCoReportesTransitoBL
    {
        private readonly FrmCoReportesTransitoDB _db;

        public FrmCoReportesTransitoBL(IConfiguration config)
        {
            _db = new FrmCoReportesTransitoDB(config);
        }

        public ErrorDto<List<CoReporteItemDto>> CO_ReportesTransito_Catalogo_Obtener(int CodEmpresa) => _db.CO_ReportesTransito_Catalogo_Obtener(CodEmpresa);
        public ErrorDto<List<DropDownListaGenericaModel>> CO_ReportesTransito_Lineas_Obtener(int CodEmpresa, string? texto) => _db.CO_ReportesTransito_Lineas_Obtener(CodEmpresa, texto);
        public ErrorDto<CoReporteCodigoDescripcionDto> CO_ReportesTransito_LineaDescripcion_Obtener(int CodEmpresa, string codigo) => _db.CO_ReportesTransito_LineaDescripcion_Obtener(CodEmpresa, codigo);
        public ErrorDto<List<DropDownListaGenericaModel>> CO_ReportesTransito_Recursos_Obtener(int CodEmpresa, string? codigo, bool todasLineas) => _db.CO_ReportesTransito_Recursos_Obtener(CodEmpresa, codigo, todasLineas);
        public ErrorDto<List<DropDownListaGenericaModel>> CO_ReportesTransito_Destinos_Obtener(int CodEmpresa, string? codigo, bool todasLineas) => _db.CO_ReportesTransito_Destinos_Obtener(CodEmpresa, codigo, todasLineas);
        public ErrorDto<List<DropDownListaGenericaModel>> CO_ReportesTransito_Instituciones_Obtener(int CodEmpresa) => _db.CO_ReportesTransito_Instituciones_Obtener(CodEmpresa);
        public ErrorDto<List<DropDownListaGenericaModel>> CO_ReportesTransito_Deductoras_Obtener(int CodEmpresa, int? codInstitucion) => _db.CO_ReportesTransito_Deductoras_Obtener(CodEmpresa, codInstitucion);
        public ErrorDto<List<DropDownListaGenericaModel>> CO_ReportesTransito_Garantias_Obtener(int CodEmpresa) => _db.CO_ReportesTransito_Garantias_Obtener(CodEmpresa);
        public ErrorDto<List<DropDownListaGenericaModel>> CO_ReportesTransito_Oficinas_Obtener(int CodEmpresa) => _db.CO_ReportesTransito_Oficinas_Obtener(CodEmpresa);
        public ErrorDto<List<DropDownListaGenericaModel>> CO_ReportesTransito_Estados_Obtener(int CodEmpresa) => _db.CO_ReportesTransito_Estados_Obtener(CodEmpresa);
        public ErrorDto<List<DropDownListaGenericaModel>> CO_ReportesTransito_Carteras_Obtener(int CodEmpresa) => _db.CO_ReportesTransito_Carteras_Obtener(CodEmpresa);
    }
}
