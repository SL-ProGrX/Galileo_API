using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.Cobros;
using Galileo_API.Models.ProGrX.Cobros;

namespace Galileo_API.BusinessLogic.ProGrX.Cobros
{
    public class FrmCoReportesBL
    {
        private readonly FrmCoReportesDB Db;

        public FrmCoReportesBL(IConfiguration config)
        {
            Db = new FrmCoReportesDB(config);
        }

        public ErrorDto<List<CoReporteItemDto>> CO_Reportes_Catalogo_Obtener(int CodEmpresa)
        {
            return Db.CO_Reportes_Catalogo_Obtener(CodEmpresa);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> CO_Lineas_Obtener(int CodEmpresa, string? texto)
        {
            return Db.CO_Lineas_Obtener(CodEmpresa, texto);
        }

        public ErrorDto<CoReporteCodigoDescripcionDto> CO_Linea_Descripcion_Obtener(int CodEmpresa, string codigo)
        {
            return Db.CO_Linea_Descripcion_Obtener(CodEmpresa, codigo);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> CO_Recursos_Dropdown_Obtener(int CodEmpresa, string? codigo, bool todasLineas)
        {
            return Db.CO_Recursos_Dropdown_Obtener(CodEmpresa, codigo, todasLineas);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> CO_Destinos_Dropdown_Obtener(int CodEmpresa, string? codigo, bool todasLineas)
        {
            return Db.CO_Destinos_Dropdown_Obtener(CodEmpresa, codigo, todasLineas);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> CO_Comites_Dropdown_Obtener(int CodEmpresa)
        {
            return Db.CO_Comites_Dropdown_Obtener(CodEmpresa);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> CO_Instituciones_Dropdown_Obtener(int CodEmpresa)
        {
            return Db.CO_Instituciones_Dropdown_Obtener(CodEmpresa);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> CO_Deductoras_Dropdown_Obtener(int CodEmpresa, int? codInstitucion)
        {
            return Db.CO_Deductoras_Dropdown_Obtener(CodEmpresa, codInstitucion);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> CO_Divisas_Dropdown_Obtener(int CodEmpresa, int gEnlace)
        {
            return Db.CO_Divisas_Dropdown_Obtener(CodEmpresa, gEnlace);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> CO_EstadosLaborales_Dropdown_Obtener(int CodEmpresa)
        {
            return Db.CO_EstadosLaborales_Dropdown_Obtener(CodEmpresa);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> CO_Gestiona_Dropdown_Obtener(int CodEmpresa)
        {
            return Db.CO_Gestiona_Dropdown_Obtener(CodEmpresa);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> CO_Garantias_Dropdown_Obtener(int CodEmpresa)
        {
            return Db.CO_Garantias_Dropdown_Obtener(CodEmpresa);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> CO_Antiguedades_Dropdown_Obtener(int CodEmpresa)
        {
            return Db.CO_Antiguedades_Dropdown_Obtener(CodEmpresa);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> CO_Carteras_Dropdown_Obtener(int CodEmpresa)
        {
            return Db.CO_Carteras_Dropdown_Obtener(CodEmpresa);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> CO_EstadosPersona_Dropdown_Obtener(int CodEmpresa)
        {
            return Db.CO_EstadosPersona_Dropdown_Obtener(CodEmpresa);
        }

        public ErrorDto CO_Reportes_Cubo_Procesar(int CodEmpresa, string usuario)
        {
            return Db.CO_Reportes_Cubo_Procesar(CodEmpresa, usuario);
        }
    }
}