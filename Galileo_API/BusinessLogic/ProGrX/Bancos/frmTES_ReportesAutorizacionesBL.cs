using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.Bancos;

namespace Galileo_API.BusinessLogic.ProGrX.Bancos
{
    public class FrmTesReportesAutorizacionesBL
    {
        private readonly FrmTesReportesAutorizacionesDB _reportesDb;

        public FrmTesReportesAutorizacionesBL(IConfiguration config)
        {
            _reportesDb = new FrmTesReportesAutorizacionesDB(config);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> sbTesBancoCargaCboAccesoGeneral(int CodEmpresa)
        {
            return _reportesDb.sbTesBancoCargaCboAccesoGeneral(CodEmpresa);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> sbTesTiposDocsCargaCbo(int CodEmpresa, int id_banco)
        {
            return _reportesDb.sbTesTiposDocsCargaCbo(CodEmpresa, id_banco);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> Tes_RepAuthUsuarios_Obtener(int CodEmpresa)
        {
            return _reportesDb.Tes_RepAuthUsuarios_Obtener(CodEmpresa);
        }

    }
}
