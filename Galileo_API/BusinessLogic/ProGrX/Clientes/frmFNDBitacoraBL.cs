using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Clientes;
using Galileo.DataBaseTier.ProGrX.Clientes;

namespace Galileo.BusinessLogic.ProGrX.Clientes
{
    public class FrmFndBitacoraBl
    {
        private readonly FrmFndBitacoraDb _db;

        public FrmFndBitacoraBl(IConfiguration config)
        {
            _db = new FrmFndBitacoraDb(config);
        }

        public ErrorDto<List<UsMovimiento>> Fnd_Movimientos_Obtener(int CodEmpresa)
        {
            return _db.Fnd_Movimientos_Obtener(CodEmpresa);
        }

        public ErrorDto<List<FndBitacoraCambiosResult>> Fnd_Bitacora_Cambios_Obtener(int CodEmpresa, FndBitacoraCambiosRequest request)
        {
            return _db.Fnd_Bitacora_Cambios_Obtener(CodEmpresa, request);
        }

        public ErrorDto<bool> Fnd_Bitacora_Cambio_Revisar(int CodEmpresa, FndBitacoraCambioRevisarRequest request)
        {
            return _db.Fnd_Bitacora_Cambio_Revisar(CodEmpresa, request);
        }

        public ErrorDto<bool> Sif_RegistraTags(int CodEmpresa, SifRegistraTagsRequest request)
        {
            return _db.Sif_RegistraTags(CodEmpresa, request);
        }
    }
}
