using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.Bancos;

namespace Galileo_API.BusinessLogic.ProGrX.Bancos
{
    public class FrmTesReportesBL
    {

        private readonly FrmTesReportesDB _reportesDb;

        public FrmTesReportesBL(IConfiguration config)
        {
            _reportesDb = new FrmTesReportesDB(config);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> sbTesBancoCargaCboAccesoGeneral(int CodEmpresa)
        {
            return _reportesDb.sbTesBancoCargaCboAccesoGeneral(CodEmpresa);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> sbTesTiposDocsCargaCbo(int CodEmpresa, int id_banco)
        {
            return _reportesDb.sbTesTiposDocsCargaCbo(CodEmpresa, id_banco);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> sbTESCombos(string tipo)
        {
            return _reportesDb.sbTESCombos(tipo);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> sbTesUnidadesCargaCboGeneral(int CodEmpresa, int contabilidad)
        {
            return _reportesDb.sbTesUnidadesCargaCboGeneral(CodEmpresa, contabilidad);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> sbTesConceptosCargaCboGeneral(int CodEmpresa)
        {
            return _reportesDb.sbTesConceptosCargaCboGeneral(CodEmpresa);
        }

        public ErrorDto<string> Tes_AnalisisCubo_Obtener(int CodEmpresa, string tipo, DateTime FechaInicio, DateTime FechaCorte)
        {
            return _reportesDb.Tes_AnalisisCubo_Obtener(CodEmpresa, tipo, FechaInicio, FechaCorte);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> sbTesTokens(int CodEmpresa)
        {
            return _reportesDb.sbTesTokens(CodEmpresa);
        }

    }
}
