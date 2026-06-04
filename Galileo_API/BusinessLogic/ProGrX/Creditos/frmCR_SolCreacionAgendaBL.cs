using Galileo.DataBaseTier.ProGrX.Credito;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Credito;

namespace Galileo.BusinessLogic.ProGrX.Credito
{
    public class FrmCRSolCreacionAgendaBL
    {
        private readonly FrmCRSolCreacionAgendaDBs _db;

        public FrmCRSolCreacionAgendaBL(IConfiguration config)
        {
            _db = new FrmCRSolCreacionAgendaDBs(config);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> CR_SolCreacionAgenda_Comites_Obtener(int CodEmpresa)
        {
            return _db.CR_SolCreacionAgenda_Comites_Obtener(CodEmpresa);
        }

        public ErrorDto<CrSolCreacionAgendaReporteData> CR_SolCreacionAgenda_Acta_Generar(int CodEmpresa, CrSolCreacionAgendaActaData acta)
        {
            return _db.CR_SolCreacionAgenda_Acta_Generar(CodEmpresa, acta);
        }

        public ErrorDto<int> CR_SolCreacionAgenda_Acta_Consulta(int CodEmpresa, int id_comite)
        {
            return _db.CR_SolCreacionAgenda_Acta_Consulta(CodEmpresa, id_comite);
        }
    }
}