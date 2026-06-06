using Galileo.Models;
using Galileo.Models.AH;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.Patrimonio;
using Newtonsoft.Json;

namespace Galileo_API.BusinessLogic.ProGrX.Patrimonio
{
    public class FrmAhAutorizacionesBL
    {
        private readonly FrmAhAutorizacionesDB _db;

        public FrmAhAutorizacionesBL(IConfiguration config)
        {
            _db = new FrmAhAutorizacionesDB(config);
        }

        public ErrorDto<List<PatGestionesPatrimonio>> Ah_Autorizaciones_Obtener(
            int codEmpresa,
            string filtros)
        {
            FiltrosAutorizacionesPatrimonioDto request = JsonConvert.DeserializeObject<FiltrosAutorizacionesPatrimonioDto>(filtros) ?? new FiltrosAutorizacionesPatrimonioDto();
            return _db.Ah_Autorizaciones_Obtener(codEmpresa, request);
        }

        public ErrorDto<FrmAhAutorizacionesProcesarResponse> Ah_Autorizaciones_Procesar(
            int codEmpresa,
            FrmAhAutorizacionesProcesarRequest request)
        {
            return _db.Ah_Autorizaciones_Procesar(codEmpresa, request);
        }
    }
}
