
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Bancos;
using Galileo_API.DataBaseTier.ProGrX.Bancos;

namespace Galileo_API.BusinessLogic.ProGrX.Bancos
{
    public class FrmTesCambiosFechasBL
    {
        private readonly FrmTesCambiosFechasDB _CambiosFechasDb;

        public FrmTesCambiosFechasBL(IConfiguration config)
        {
            _CambiosFechasDb = new FrmTesCambiosFechasDB(config);
        }

        public ErrorDto<TesCambioFechasData> TES_CambioFechas_Obtener(int CodEmpresa, int solicitud)
        {
            return _CambiosFechasDb.TES_CambioFechas_Obtener(CodEmpresa, solicitud);
        }

        public ErrorDto TES_CambioFecha_Cambiar(int CodEmpresa, TesCambioFechasModel fechas)
        {
            return _CambiosFechasDb.TES_CambioFecha_Cambiar(CodEmpresa, fechas);
        }

    }
}
