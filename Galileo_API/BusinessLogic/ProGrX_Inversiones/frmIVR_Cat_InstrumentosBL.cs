using Galileo.DataBaseTier.ProGrX_Inversiones;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Inversiones;

namespace Galileo_API.BusinessLogic.ProGrX_Inversiones
{
    public sealed class FrmIvrCatInstrumentosBl
    {
        private readonly FrmIvrCatInstrumentosDb _db;

        public FrmIvrCatInstrumentosBl(IConfiguration config)
        {
            _db = new FrmIvrCatInstrumentosDb(config);
        }

        public ErrorDto<List<IvrCatInstrumentoData>> IvrCatInstrumentos_Obtener(int codEmpresa)
            => _db.IvrCatInstrumentos_Obtener(codEmpresa);

        public ErrorDto IvrCatInstrumentos_Guardar(int codEmpresa, IvrCatInstrumentoGuardarRequest request)
            => _db.IvrCatInstrumentos_Guardar(codEmpresa, request);

        public ErrorDto IvrCatInstrumentos_Eliminar(int codEmpresa, IvrCatInstrumentoEliminarRequest request)
            => _db.IvrCatInstrumentos_Eliminar(codEmpresa, request);
    }
}
