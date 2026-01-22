using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Cobros;
using Galileo_API.DataBaseTier.ProGrX.Cobros;

namespace Galileo_API.BusinessLogicTier.ProGrX.Cobros
{
    public class FrmCOControlParametrosBL
    {
        private readonly FrmCOControlParametrosDB _db;

        public FrmCOControlParametrosBL(IConfiguration config)
        {
            _db = new FrmCOControlParametrosDB(config);
        }

        public ErrorDto<CoControlParametrosListaResult> Co_ControlParametros_Lista_Obtener(int CodEmpresa, string jfiltros)
        {
            return _db.Co_ControlParametros_Lista_Obtener(CodEmpresa, jfiltros);
        }

        public ErrorDto<CoControlParametrosListaResult> Co_ControlParametros_Lista_Export(int CodEmpresa, string jfiltros)
        {
            return _db.Co_ControlParametros_Lista_Export(CodEmpresa, jfiltros);
        }

        public ErrorDto Co_ControlParametros_Guardar(int CodEmpresa, CoControlParametrosGuardarRequest req)
        {
            return _db.Co_ControlParametros_Guardar(CodEmpresa, req);
        }
    }
}
