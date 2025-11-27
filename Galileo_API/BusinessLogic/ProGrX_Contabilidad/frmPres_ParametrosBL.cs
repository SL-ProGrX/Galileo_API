using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.PRE;

namespace Galileo.BusinessLogic
{
    public class FrmPresParametrosBl
    {
        readonly FrmPresParametrosDb _db;

        public FrmPresParametrosBl(IConfiguration config)
        {
            _db = new FrmPresParametrosDb(config);
        }

        public ErrorDto PresParametros_Guardar(int CodEmpresa, PresParametrosDto parametros)
        {
            return _db.PresParametros_Guardar(CodEmpresa, parametros);
        }

        public ErrorDto<List<PresParametrosDto>> PresParametrosLista_Obtener(int CodEmpresa)
        {
            return _db.PresParametrosLista_Obtener(CodEmpresa);
        }

    }
}