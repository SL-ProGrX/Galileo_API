using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.PRES;

namespace Galileo.BusinessLogic
{
    public class FrmPresAjustesTiposBl
    {
        readonly FrmPresAjustesTiposDb _db;

        public FrmPresAjustesTiposBl(IConfiguration config)
        {
            _db = new FrmPresAjustesTiposDb(config);
        }

        public ErrorDto<PresAjustestTiposLista> PresAjustestTipos_Obtener(int CodEmpresa)
        {
            return _db.PresAjustestTipos_Obtener(CodEmpresa);
        }

        public ErrorDto PresAjustesTipo_Insertar(int CodEmpresa, PresAjustestTiposDto Info)
        {
            return _db.PresAjustesTipo_Insertar(CodEmpresa, Info);
        }

        public ErrorDto PresAjustesTipo_Actualizar(int CodEmpresa, PresAjustestTiposDto Info)
        {
            return _db.PresAjustesTipo_Actualizar(CodEmpresa, Info);
        }

        public ErrorDto PresAjustesTipo_Eliminar(int CodEmpresa, string CodAjuste)
        {
            return _db.PresAjustesTipo_Eliminar(CodEmpresa, CodAjuste);
        }

    }//end class
}//end namespace
