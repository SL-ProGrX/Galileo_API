using Galileo.Models.ERROR;
using Galileo.Models;
using Galileo_API.DataBaseTier.ProGrX.Creditos;
using Galileo_API.Models.ProGrX.Creditos;

namespace Galileo_API.BusinessLogic.ProGrX.Creditos
{
    public class FrmCrAprobacionMasivaBL
    {
        private readonly FrmCrAprobacionMasivaDB _db;

        public FrmCrAprobacionMasivaBL(IConfiguration config)
        {
            _db = new FrmCrAprobacionMasivaDB(config);
        }

        public ErrorDto<List<CrAprobacionMasivaOperacionData>> CrAprobacionMasiva_Consulta_Obtener(
            int codEmpresa,
            CrAprobacionMasivaConsultaRequest request)
            => _db.CrAprobacionMasiva_Consulta_Obtener(codEmpresa, request);

        public ErrorDto<List<DropDownListaGenericaModel>> CrAprobacionMasiva_LineasCatalago_Obtener(
            int codEmpresa,
            string? codigo)
            => _db.CrAprobacionMasiva_LineasCatalogo_Obtener(codEmpresa, codigo);

        public ErrorDto CrAprobacionMasiva_Formalizar(
            int codEmpresa,
            CrAprobacionMasivaFormalizarRequest request)
            => _db.CrAprobacionMasiva_Formalizar(codEmpresa, request);
    }
}