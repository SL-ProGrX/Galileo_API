using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.Creditos;
using Galileo_API.Models.ProGrX.Creditos;

namespace Galileo_API.BusinessLogic.ProGrX.Creditos
{
    public class FrmCrAutorizacionTranferenciasBL
    {
        private readonly FrmCrAutorizacionTranferenciasDB _db;

        public FrmCrAutorizacionTranferenciasBL(IConfiguration config)
        {
            _db = new FrmCrAutorizacionTranferenciasDB(config);
        }

        public ErrorDto<List<CrAutorizacionTranferenciasTag>> CrAutorizacionTranferencias_Tags_Obtener(int CodEmpresa, string Usuario)
        {
            return _db.CrAutorizacionTranferencias_Tags_Obtener(CodEmpresa, Usuario);
        }

        public ErrorDto<List<CrAutorizacionTranferenciasSolicitud>> CrAutorizacionTranferencias_Solicitudes_Obtener(int CodEmpresa, DateTime FechaDtpFInicio, string CodigoEtiqueta)
        {
            return _db.CrAutorizacionTranferencias_Solicitudes_Obtener(CodEmpresa, FechaDtpFInicio, CodigoEtiqueta);
        }

        public ErrorDto<string?> CrAutorizacionTranferencias_Parametro_Obtener(int CodEmpresa, string CodParametro)
        {
            return _db.CrAutorizacionTranferencias_Parametro_Obtener(CodEmpresa, CodParametro);
        }

        public ErrorDto CrAutorizacionTranferencias_OperacionTag_Registrar(int CodEmpresa, CrAutorizacionTranferenciasOperacionTagRegistrarRequest request)
        {
            return _db.CrAutorizacionTranferencias_OperacionTag_Registrar(CodEmpresa, request);
        }
    }
}
