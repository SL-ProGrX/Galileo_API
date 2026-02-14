using Galileo_API.DataBaseTier.ProGrX.CuentasxCobrar;
using Galileo_API.Models.ProGrX.CuentasxCobrar;
using Galileo.Models.ERROR;

namespace Galileo_API.BusinessLogic.ProGrX.CuentasxCobrar
{
    public class FrmCxCContratosPagadoresBL
    {
        private readonly FrmCxCContratosPagadoresDB _db;

        public FrmCxCContratosPagadoresBL(IConfiguration config)
        {
            _db = new FrmCxCContratosPagadoresDB(config);
        }

        public ErrorDto<List<CxcContratoPagadorDto>> CxcContratosPagadores_Lista(int codEmpresa, CxcContratoPagadorListaParams param)
        {
            return _db.CxcContratosPagadores_Lista(codEmpresa, param);
        }

        public ErrorDto<bool> CxcContratoPagador_Insertar(int codEmpresa, CxcContratoPagadorSaveParams param)
        {
            return _db.CxcContratoPagador_Insertar(codEmpresa, param);
        }

        public ErrorDto<bool> CxcContratoPagador_Eliminar(int codEmpresa, CxcContratoPagadorDeleteParams param)
        {
            return _db.CxcContratoPagador_Eliminar(codEmpresa, param);
        }
    }
}
