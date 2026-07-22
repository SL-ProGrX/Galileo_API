
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.Creditos;
using static Galileo_API.Models.ProGrX.Creditos.FrmCRCambioInfoEstadisticaModels;

namespace Galileo_API.BusinessLogic.ProGrX.Creditos
{
    public class FrmCRCambioInfoEstadisticaBL
    {

        private readonly FrmCRCambioInfoEstadisticaDB _db;

        public FrmCRCambioInfoEstadisticaBL(IConfiguration config)
        {
            _db = new FrmCRCambioInfoEstadisticaDB(config);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> CR_CambioInfoEstadistica_DatosTipo_Obtener(int codEmpresa, string tipo)
           => _db.CR_CambioInfoEstadistica_DatosTipo_Obtener(codEmpresa, tipo);

        public ErrorDto<bool> CR_CambioInfoEstadistica_Procesar(int codEmpresa, CrCambioInfoEstadisticaProcesarRequest request)
               => _db.CR_CambioInfoEstadistica_Procesar(codEmpresa, request);

        public ErrorDto<CrCambioInfoEstadisticaCargaListadoResponse> CR_CambioInfoEstadistica_CargarListado(int codEmpresa, CrCambioInfoEstadisticaCargaListadoRequest request)
              => _db.CR_CambioInfoEstadistica_CargarListado(codEmpresa, request);
    }
}
