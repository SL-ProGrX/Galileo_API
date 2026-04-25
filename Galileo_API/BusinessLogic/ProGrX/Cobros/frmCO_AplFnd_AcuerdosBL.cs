using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.Cobros;
using static Galileo_API.Models.ProGrX.Cobros.FrmCoAplFndAcuerdosModels;

namespace Galileo_API.BusinessLogic.ProGrX.Cobros
{
    public class FrmCoAplFndAcuerdosBL
    {
        private readonly FrmCoAplFndAcuerdosDB _db;

        public FrmCoAplFndAcuerdosBL(IConfiguration config)
        {
            _db = new FrmCoAplFndAcuerdosDB(config);
        }

        public ErrorDto<CoAplFndAcuerdosDetalleResponse> Co_AplFnd_Acuerdos_Consultar(int codEmpresa, int idAcuerdo)
                => _db.Co_AplFnd_Acuerdos_Consultar(codEmpresa, idAcuerdo);
        public ErrorDto<CoAplFndAcuerdosGuardarResponse> Co_AplFnd_Acuerdos_Guardar(int codEmpresa, CoAplFndAcuerdosDetalleResponse request)
                        => _db.Co_AplFnd_Acuerdos_Guardar(codEmpresa, request);
        public ErrorDto<List<CoAplFndAcuerdosGridResponse>> Co_AplFnd_Acuerdos_Listar(int codEmpresa, CoAplFndAcuerdosFiltroRequest request)
                => _db.Co_AplFnd_Acuerdos_Listar(codEmpresa, request);
        public ErrorDto<CoAplFndAcuerdosCargaMasivaResponse> Co_AplFnd_Acuerdos_CargaMasiva(
            int codEmpresa, CoAplFndAcuerdosCargaMasivaRequest request)
              => _db.Co_AplFnd_Acuerdos_CargaMasiva(codEmpresa, request);
        public ErrorDto<List<CoAplFndAcuerdosSocioResult>> Co_AplFnd_Socios_Obtener(int codEmpresa)
               => _db.Co_AplFnd_Socios_Obtener(codEmpresa);
    }
}
