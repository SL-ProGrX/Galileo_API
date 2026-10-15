using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.Convenios;
using Galileo_API.Models.ProGrX.Convenios;

namespace Galileo_API.BusinessLogic.ProGrX.Convenios
{
    public class FrmCrConveniosLiquidacionBl
    {
        private readonly FrmCrConveniosLiquidacionDb _db;
        private const string MsgSinConvenio = "No se ha indicado ningun convenio?";

        public FrmCrConveniosLiquidacionBl(IConfiguration config)
        {
            _db = new FrmCrConveniosLiquidacionDb(config);
        }

        public ErrorDto<List<CrConvenioConsultaData>> CrConveniosLiquidacion_Convenios_Obtener(int codEmpresa)
            => _db.CrConveniosLiquidacion_Convenios_Obtener(codEmpresa);

        public ErrorDto<CrConvenioConsultaData?> CrConveniosLiquidacion_Convenio_Navegar(
            int codEmpresa,
            string? codConvenio,
            bool siguiente)
            => _db.CrConveniosLiquidacion_Convenio_Navegar(codEmpresa, codConvenio, siguiente);

        public ErrorDto<CrLiqConvenioData?> CrConveniosLiquidacion_Convenio_Obtener(
            int codEmpresa,
            string? codConvenio,
            string? descripcion)
            => _db.CrConveniosLiquidacion_Convenio_Obtener(codEmpresa, codConvenio, descripcion);

        public ErrorDto<List<CrLiqOrdenListaData>> CrConveniosLiquidacion_Ordenes_Obtener(
            int codEmpresa,
            string codConvenio,
            int lineas)
        {
            if (string.IsNullOrWhiteSpace(codConvenio))
                return DbHelper.CreateErrorResponse(MsgSinConvenio, -2, new List<CrLiqOrdenListaData>());

            return _db.CrConveniosLiquidacion_Ordenes_Obtener(codEmpresa, codConvenio, lineas);
        }

        public ErrorDto<CrLiqOrdenData?> CrConveniosLiquidacion_Orden_Obtener(
            int codEmpresa,
            string codConvenio,
            int codOrden)
        {
            if (string.IsNullOrWhiteSpace(codConvenio))
                return DbHelper.CreateErrorResponse<CrLiqOrdenData?>(MsgSinConvenio, -2);

            return _db.CrConveniosLiquidacion_Orden_Obtener(codEmpresa, codConvenio, codOrden);
        }

        public ErrorDto<CrLiqNuevaOrdenData> CrConveniosLiquidacion_Orden_Nueva(int codEmpresa, string codConvenio)
        {
            if (string.IsNullOrWhiteSpace(codConvenio))
                return DbHelper.CreateErrorResponse<CrLiqNuevaOrdenData>(MsgSinConvenio, -2);

            return _db.CrConveniosLiquidacion_Orden_Nueva(codEmpresa, codConvenio);
        }

        public ErrorDto<CrLiqOrdenData?> CrConveniosLiquidacion_Orden_Guardar(
            int codEmpresa,
            CrLiqOrdenGuardarRequest? request)
        {
            var error = ValidarRequest(request);
            if (error != null)
                return DbHelper.CreateErrorResponse<CrLiqOrdenData?>(error, -2);

            return _db.CrConveniosLiquidacion_Orden_Guardar(codEmpresa, request!);
        }

        public ErrorDto<CrLiqOrdenData?> CrConveniosLiquidacion_Orden_Calcular(
            int codEmpresa,
            string codConvenio,
            int codOrden)
        {
            if (string.IsNullOrWhiteSpace(codConvenio))
                return DbHelper.CreateErrorResponse<CrLiqOrdenData?>(MsgSinConvenio, -2);

            return _db.CrConveniosLiquidacion_Orden_Calcular(codEmpresa, codConvenio, codOrden);
        }

        public ErrorDto<CrLiqOrdenData?> CrConveniosLiquidacion_Orden_Cerrar(
            int codEmpresa,
            CrLiqOrdenGuardarRequest? request)
        {
            var error = ValidarRequest(request);
            if (error != null)
                return DbHelper.CreateErrorResponse<CrLiqOrdenData?>(error, -2);

            return _db.CrConveniosLiquidacion_Orden_Cerrar(codEmpresa, request!);
        }

        public ErrorDto<List<CrLiqFlotanteData>> CrConveniosLiquidacion_Flotante_Obtener(
            int codEmpresa,
            string codConvenio)
        {
            if (string.IsNullOrWhiteSpace(codConvenio))
                return DbHelper.CreateErrorResponse(MsgSinConvenio, -2, new List<CrLiqFlotanteData>());

            return _db.CrConveniosLiquidacion_Flotante_Obtener(codEmpresa, codConvenio);
        }

        private static string? ValidarRequest(CrLiqOrdenGuardarRequest? request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.cod_convenio))
                return "- No se ha consultado ningun convenio?";

            if (string.IsNullOrWhiteSpace(request.usuario))
                return "Debe indicar el usuario.";

            return null;
        }
    }
}
