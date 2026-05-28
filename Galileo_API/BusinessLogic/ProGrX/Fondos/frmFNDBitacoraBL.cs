using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.Fondos;
using Galileo_API.Models.ProGrX.Fondos;

namespace Galileo_API.BusinessLogic.ProGrX.Fondos
{
    public class FrmFndBitacoraBl
    {
        private readonly FrmFndBitacoraDb _db;

        public FrmFndBitacoraBl(IConfiguration config)
        {
            _db = new FrmFndBitacoraDb(config);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> Fnd_Operadoras_Obtener(int codEmpresa)
        {
            return _db.Fnd_Operadoras_Obtener(codEmpresa);
        }

        public ErrorDto<List<FrmFndBitacoraMovimientoDto>> Fnd_Movimientos_Obtener(int codEmpresa)
        {
            return _db.Fnd_Movimientos_Obtener(codEmpresa);
        }

        public ErrorDto<List<FrmFndBitacoraCambiosDto>> Fnd_Bitacora_Cambios_Obtener(
            int codEmpresa,
            FrmFndBitacoraCambiosRequest request)
        {
            return _db.Fnd_Bitacora_Cambios_Obtener(codEmpresa, request);
        }

        public ErrorDto<bool> Fnd_Bitacora_Cambio_Revisar(
            int codEmpresa,
            FrmFndBitacoraCambioRevisarRequest request)
        {
            return _db.Fnd_Bitacora_Cambio_Revisar(codEmpresa, request);
        }

        public ErrorDto<bool> Sif_RegistraTags(
            int codEmpresa,
            FrmFndBitacoraSifRegistraTagsRequest request)
        {
            return _db.Sif_RegistraTags(codEmpresa, request);
        }
    }
}
