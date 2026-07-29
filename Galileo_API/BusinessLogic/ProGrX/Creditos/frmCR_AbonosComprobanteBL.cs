using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.Creditos;
using Galileo_API.Models.ProGrX.Creditos;

namespace Galileo_API.BusinessLogic.ProGrX.Creditos
{
    public class FrmCrAbonosComprobanteBl
    {
        private readonly FrmCrAbonosComprobanteDb _Db;

        public FrmCrAbonosComprobanteBl(IConfiguration config)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));
            _Db = new FrmCrAbonosComprobanteDb(config);
        }

        public ErrorDto<CrAbonosComprobanteOperacionData> CrAbonosComprobante_Operacion_Obtener(
            int codEmpresa,
            int operacion)
        {
            return _Db.CrAbonosComprobante_Operacion_Obtener(codEmpresa, operacion);
        }

        public ErrorDto<List<CrAbonosComprobanteOperacionListaItem>>
            CrAbonosComprobante_Operaciones_Lista_Obtener(int codEmpresa)
        {
            return _Db.CrAbonosComprobante_Operaciones_Lista_Obtener(codEmpresa);
        }

        public ErrorDto<List<DropDownListaGenericaModel>>
            CrAbonosComprobante_TiposDocumento_Obtener(int codEmpresa)
        {
            return _Db.CrAbonosComprobante_TiposDocumento_Obtener(codEmpresa);
        }

        public ErrorDto<CrAbonosComprobanteAplicarResultadoData> CrAbonosComprobante_Aplicar(
            int codEmpresa,
            CrAbonosComprobanteAplicarRequest request)
        {
            return _Db.CrAbonosComprobante_Aplicar(codEmpresa, request);
        }
    }
}
