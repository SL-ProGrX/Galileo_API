using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.Creditos;
using Galileo_API.Models.ProGrX.Credito;

namespace Galileo_API.BusinessLogic.ProGrX.Creditos
{
    public class FrmCrAdjuntosTiposBl
    {
        private readonly FrmCrAdjuntosTiposDb _db;

        public FrmCrAdjuntosTiposBl(IConfiguration config)
        {
            _db = new FrmCrAdjuntosTiposDb(config);
        }

        public ErrorDto<List<CrAdjuntoTipoData>> CrAdjuntosTipos_Obtener(int codEmpresa)
            => _db.CrAdjuntosTipos_Obtener(codEmpresa);

        public ErrorDto CrAdjuntosTipos_Guardar(int codEmpresa, CrAdjuntoTipoGuardarRequest request)
            => _db.CrAdjuntosTipos_Guardar(codEmpresa, request);

        public ErrorDto CrAdjuntosTipos_Eliminar(int codEmpresa, CrAdjuntoTipoEliminarRequest request)
            => _db.CrAdjuntosTipos_Eliminar(codEmpresa, request);
    }
}