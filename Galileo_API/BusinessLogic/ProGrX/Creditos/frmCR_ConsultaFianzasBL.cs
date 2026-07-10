using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.Creditos;
using Galileo_API.Models.ProGrX.Creditos;

namespace Galileo_API.BusinessLogic.ProGrX.Creditos
{
    public class FrmCrConsultaFianzasBl
    {
        private readonly FrmCrConsultaFianzasDb _db;

        public FrmCrConsultaFianzasBl(IConfiguration config)
        {
            _db = new FrmCrConsultaFianzasDb(config);
        }

        public ErrorDto<CrConsultaFianzasConsultaData> CrConsultaFianzas_Consulta_Obtener(
            int codEmpresa,
            CrConsultaFianzasConsultaRequest request)
            => _db.CrConsultaFianzas_Consulta_Obtener(codEmpresa, request);

        public ErrorDto<CrConsultaFianzasDetalleData> CrConsultaFianzas_Detalle_Obtener(
            int codEmpresa,
            CrConsultaFianzasDetalleRequest request)
            => _db.CrConsultaFianzas_Detalle_Obtener(codEmpresa, request);
    }
}