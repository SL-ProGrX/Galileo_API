using Galileo_API.DataBaseTier.ProGrX.Cobros;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Cobros;

namespace Galileo_API.BusinessTier.ProGrX.Cobros
{
    public class FrmCOTrasladoDeudaBL
    {
        private readonly FrmCOTrasladoDeudaDB _db;

        public FrmCOTrasladoDeudaBL(IConfiguration config)
        {
            _db = new FrmCOTrasladoDeudaDB(config);
        }
        public ErrorDto<CoTrasladoDeudaObtenerDto> CO_TrasladoDeuda_Obtener(int codEmpresa, long idSolicitud)
        {
            return _db.CO_TrasladoDeuda_Obtener(codEmpresa, idSolicitud);
        }
        public ErrorDto<CoTrasladoDeudaCalcularResponse> CO_TrasladoDeuda_Calcular(int codEmpresa, CoTrasladoDeudaCalcularRequest data)
        {
            return _db.CO_TrasladoDeuda_Calcular(codEmpresa, data);
        }
        public ErrorDto<CoTrasladoDeudaAplicarResponse> CO_TrasladoDeuda_Aplicar(int codEmpresa, CoTrasladoDeudaAplicarRequest data)
        {
            return _db.CO_TrasladoDeuda_Aplicar(codEmpresa, data);
        }
        public ErrorDto<CoTrasladoDeudaExportResponse> CO_TrasladoDeuda_Export(int codEmpresa, CoTrasladoDeudaExportRequest data)
        {
            return _db.CO_TrasladoDeuda_Export(codEmpresa, data);
        }
    }
}