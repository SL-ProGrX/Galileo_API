using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.Conciliacion;
using Galileo_API.Models.ProGrX.Conciliacion;

namespace Galileo_API.BusinessLogic.ProGrX.Conciliacion
{
    public class FrmCcReportesEstudioBL
    {
        private readonly FrmCcReportesEstudioDB _db;

        public FrmCcReportesEstudioBL(IConfiguration config)
        {
            _db = new FrmCcReportesEstudioDB(config);
        }

        public ErrorDto<CcReportesEstudioAuxiliaresInicialDto> CC_ReportesEstudio_Auxiliares_Inicial_Obtener(int CodEmpresa, int codContabilidad)
        {
            return _db.CC_ReportesEstudio_Auxiliares_Inicial_Obtener(CodEmpresa, codContabilidad);
        }

        public ErrorDto<List<CcReportesEstudioPeriodoDto>> CC_ReportesEstudio_Periodos_Dropdown_Obtener(int CodEmpresa)
        {
            return _db.CC_ReportesEstudio_Periodos_Dropdown_Obtener(CodEmpresa);
        }

        public ErrorDto<CcReportesEstudioPeriodoData> CC_ReportesEstudio_Periodo_Obtener(int CodEmpresa, int idPerHistorico)
        {
            return _db.CC_ReportesEstudio_Periodo_Obtener(CodEmpresa, idPerHistorico);
        }

        public ErrorDto<DateTime> CC_ReportesEstudio_FechaServidor_Obtener(int CodEmpresa)
        {
            return _db.CC_ReportesEstudio_FechaServidor_Obtener(CodEmpresa);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> CC_ReportesEstudio_Garantias_Dropdown_Obtener(int CodEmpresa)
        {
            return _db.CC_ReportesEstudio_Garantias_Dropdown_Obtener(CodEmpresa);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> CC_ReportesEstudio_Divisas_Dropdown_Obtener(int CodEmpresa, int codContabilidad)
        {
            return _db.CC_ReportesEstudio_Divisas_Dropdown_Obtener(CodEmpresa, codContabilidad);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> CC_ReportesEstudio_Operadoras_Dropdown_Obtener(int CodEmpresa)
        {
            return _db.CC_ReportesEstudio_Operadoras_Dropdown_Obtener(CodEmpresa);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> CC_ReportesEstudio_GruposFondos_Dropdown_Obtener(int CodEmpresa)
        {
            return _db.CC_ReportesEstudio_GruposFondos_Dropdown_Obtener(CodEmpresa);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> CC_ReportesEstudio_Instituciones_Obtener(int CodEmpresa, string? texto)
        {
            return _db.CC_ReportesEstudio_Instituciones_Obtener(CodEmpresa, texto);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> CC_ReportesEstudio_Lineas_Obtener(int CodEmpresa, string? texto)
        {
            return _db.CC_ReportesEstudio_Lineas_Obtener(CodEmpresa, texto);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> CC_ReportesEstudio_Destinos_Obtener(int CodEmpresa, string? texto)
        {
            return _db.CC_ReportesEstudio_Destinos_Obtener(CodEmpresa, texto);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> CC_ReportesEstudio_Recursos_Obtener(int CodEmpresa, string? texto)
        {
            return _db.CC_ReportesEstudio_Recursos_Obtener(CodEmpresa, texto);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> CC_ReportesEstudio_Planes_Obtener(int CodEmpresa, int codOperadora, string? texto)
        {
            return _db.CC_ReportesEstudio_Planes_Obtener(CodEmpresa, codOperadora, texto);
        }

        public ErrorDto<CcReportesEstudioCuentaDto> CC_ReportesEstudio_Cuenta_Descripcion_Obtener(int CodEmpresa, string? cuenta, int codContabilidad)
        {
            return _db.CC_ReportesEstudio_Cuenta_Descripcion_Obtener(CodEmpresa, cuenta, codContabilidad);
        }

        public ErrorDto<CcReportesEstudioAuxiliarGenerarResult> CC_ReportesEstudio_Auxiliar_Generar(int CodEmpresa, CcReportesEstudioAuxiliarGenerarRequest? request)
        {
            return _db.CC_ReportesEstudio_Auxiliar_Generar(CodEmpresa, request);
        }
        public ErrorDto<List<DropDownListaGenericaModel>> CC_ReportesEstudio_Carteras_Lista_Obtener(int CodEmpresa)
        {
            return _db.CC_ReportesEstudio_Carteras_Lista_Obtener(CodEmpresa);
        }
        public ErrorDto<CcReportesEstudioEspecialReporteDto> CC_ReportesEstudio_Especial_Generar(int CodEmpresa, CcReportesEstudioEspecialRequest? request)
        {
            return _db.CC_ReportesEstudio_Especial_Generar(CodEmpresa, request);
        }
    }
}