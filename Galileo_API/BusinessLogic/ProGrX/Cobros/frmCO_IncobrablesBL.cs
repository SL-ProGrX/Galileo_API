using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.Cobros;
using static Galileo_API.Models.ProGrX.Cobros.FrmCOIncobrablesModels; 

namespace Galileo_API.BusinessLogic.ProGrX.Cobros
{
    public class FrmCOIncobrablesBL
    {
        private readonly FrmCOIncobrablesDB _db;

        public FrmCOIncobrablesBL(IConfiguration config)
        {
            _db = new FrmCOIncobrablesDB(config);
        }

        public ErrorDto<CrdIncobrableDetalleResponse> Crd_Incobrables_Operacion_Consultar(int codEmpresa, int idSolicitud)
                 => _db.Crd_Incobrables_Operacion_Consultar(codEmpresa, idSolicitud);

        public ErrorDto<List<DropDownListaGenericaModel>> Crd_Incobrables_Codigos_Obtener(int codEmpresa, int idSolicitud)
                        => _db.Crd_Incobrables_Codigos_Obtener(codEmpresa, idSolicitud);
        public ErrorDto<CrdIncobrableDetalleResponse> Crd_Incobrables_Detalle_Obtener(
             int codEmpresa,
             string usuario,
             int codContabilidad,
             int idSolicitud,
             int codIncobrable)
                 => _db.Crd_Incobrables_Detalle_Obtener(codEmpresa, usuario, codContabilidad, idSolicitud, codIncobrable);
        public ErrorDto<object> Crd_Incobrables_Aplicar(int codEmpresa, CrdIncobrableAplicarRequest request)
                   => _db.Crd_Incobrables_Aplicar(codEmpresa, request);
        public ErrorDto<object> Crd_Incobrables_Reversar(int codEmpresa, CrdIncobrableReversaRequest request)
                 => _db.Crd_Incobrables_Reversar(codEmpresa, request);

    }
}
