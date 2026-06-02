using Galileo.DataBaseTier.ProGrX.Clientes;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Clientes;

namespace Galileo.BusinessLogic.ProGrX.Clientes
{
    public class FrmAFComisionesAutorizaBL
    {
        private readonly FrmAFComisionesAutorizaDB _db;
        public FrmAFComisionesAutorizaBL(IConfiguration config)
        {
            _db = new FrmAFComisionesAutorizaDB(config);
        }

        public ErrorDto<List<ComisionAutorizaData>> AF_ComisionesAutoriza_Obtener(int CodEmpresa, ComisionAutorizaFiltroDto filtro)
        {
            return _db.AF_ComisionesAutoriza_Obtener(CodEmpresa, filtro);
        }

        public ErrorDto AF_ComisionesAutoriza_Autorizar(int CodEmpresa, string cedula, int autoriza, string? notas, string usuario)
        {
            return _db.AF_ComisionesAutoriza_Autorizar(CodEmpresa, cedula, autoriza, notas, usuario);
        }
    }
}