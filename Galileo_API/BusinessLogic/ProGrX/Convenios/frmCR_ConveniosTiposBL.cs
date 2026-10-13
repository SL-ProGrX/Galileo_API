using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.Convenios;
using Galileo_API.Models.ProGrX.Convenios;

namespace Galileo_API.BusinessLogic.ProGrX.Convenios
{
    public class FrmCrConveniosTiposBl
    {
        private readonly FrmCrConveniosTiposDb _db;

        public FrmCrConveniosTiposBl(IConfiguration config)
        {
            _db = new FrmCrConveniosTiposDb(config);
        }

        public ErrorDto<List<CrConvenioTipoData>> CrConveniosTipos_Obtener(int codEmpresa)
            => _db.CrConveniosTipos_Obtener(codEmpresa);

        public ErrorDto CrConveniosTipos_Guardar(int codEmpresa, CrConvenioTipoGuardarRequest request)
            => _db.CrConveniosTipos_Guardar(codEmpresa, request);

        public ErrorDto CrConveniosTipos_Eliminar(int codEmpresa, CrConvenioTipoEliminarRequest request)
            => _db.CrConveniosTipos_Eliminar(codEmpresa, request);
    }
}
