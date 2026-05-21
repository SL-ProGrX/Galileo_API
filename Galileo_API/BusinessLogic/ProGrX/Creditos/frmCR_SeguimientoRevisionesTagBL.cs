using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.Creditos;
using Galileo_API.Models.ProGrX.Credito;
using Galileo_API.Models.ProGrX.Credito.Galileo_API.Models.ProGrX.Credito;

namespace Galileo_API.BusinessLogic.ProGrX.Creditos
{
    public class FrmCrSeguimientoRevisionesTagBL
    {
        private readonly FrmCrSeguimientoRevisionesTagDB _db;

        public FrmCrSeguimientoRevisionesTagBL(IConfiguration config)
        {
            _db = new FrmCrSeguimientoRevisionesTagDB(config);
        }

        #region Operaciones

        public ErrorDto<List<CrSeguimientoRevisionesTagBancoRow>> Cr_SeguimientoRevisionesTag_Bancos_Obtener(
            int codEmpresa)
        {
            return _db.Cr_SeguimientoRevisionesTag_Bancos_Obtener(codEmpresa);
        }

        public ErrorDto<List<CrSeguimientoRevisionesTagEtiquetaRow>> Cr_SeguimientoRevisionesTag_Etiquetas_Obtener(
            int codEmpresa,
            string usuario)
        {
            return _db.Cr_SeguimientoRevisionesTag_Etiquetas_Obtener(codEmpresa, usuario);
        }

        public ErrorDto<CrSeguimientoRevisionesTagOperacionesResponse> Cr_SeguimientoRevisionesTag_Operaciones_Obtener(
            int codEmpresa,
            CrSeguimientoRevisionesTagOperacionesFiltrosRequest request)
        {
            return _db.Cr_SeguimientoRevisionesTag_Operaciones_Obtener(codEmpresa, request);
        }

        #endregion

        #region Detalle

        public ErrorDto<CrSeguimientoRevisionesTagDetalleCreditoResponse> Cr_SeguimientoRevisionesTag_DetalleCredito_Obtener(
            int codEmpresa,
            CrSeguimientoRevisionesTagDetalleRequest request)
        {
            return _db.Cr_SeguimientoRevisionesTag_DetalleCredito_Obtener(codEmpresa, request);
        }

        public ErrorDto<CrSeguimientoRevisionesTagPatrimonioResponse> Cr_SeguimientoRevisionesTag_Patrimonio_Obtener(
            int codEmpresa,
            CrSeguimientoRevisionesTagDetalleRequest request)
        {
            return _db.Cr_SeguimientoRevisionesTag_Patrimonio_Obtener(codEmpresa, request);
        }

        public ErrorDto<CrSeguimientoRevisionesTagDeudasResponse> Cr_SeguimientoRevisionesTag_Deudas_Obtener(
            int codEmpresa,
            CrSeguimientoRevisionesTagDetalleRequest request)
        {
            return _db.Cr_SeguimientoRevisionesTag_Deudas_Obtener(codEmpresa, request);
        }

        public ErrorDto<CrSeguimientoRevisionesTagFianzasResponse> Cr_SeguimientoRevisionesTag_Fianzas_Obtener(
            int codEmpresa,
            CrSeguimientoRevisionesTagDetalleRequest request)
        {
            return _db.Cr_SeguimientoRevisionesTag_Fianzas_Obtener(codEmpresa, request);
        }

        public ErrorDto<List<CrSeguimientoRevisionesTagRefundicionRow>> Cr_SeguimientoRevisionesTag_Refundiciones_Obtener(
            int codEmpresa,
            CrSeguimientoRevisionesTagDetalleRequest request)
        {
            return _db.Cr_SeguimientoRevisionesTag_Refundiciones_Obtener(codEmpresa, request);
        }

        public ErrorDto<List<CrSeguimientoRevisionesTagDesembolsoRow>> Cr_SeguimientoRevisionesTag_Desembolsos_Obtener(
            int codEmpresa,
            CrSeguimientoRevisionesTagDetalleRequest request)
        {
            return _db.Cr_SeguimientoRevisionesTag_Desembolsos_Obtener(codEmpresa, request);
        }

        public ErrorDto<CrSeguimientoRevisionesTagFiadorResponse> Cr_SeguimientoRevisionesTag_FiadorDetalle_Obtener(
            int codEmpresa,
            CrSeguimientoRevisionesTagFiadorRequest request)
        {
            return _db.Cr_SeguimientoRevisionesTag_FiadorDetalle_Obtener(codEmpresa, request);
        }

        public ErrorDto<List<CrSeguimientoRevisionesTagClasificacionRow>> Cr_SeguimientoRevisionesTag_FiadorClasificacion_Obtener(
            int codEmpresa,
            CrSeguimientoRevisionesTagFiadorRequest request)
        {
            return _db.Cr_SeguimientoRevisionesTag_FiadorClasificacion_Obtener(codEmpresa, request);
        }

        public ErrorDto<List<CrSeguimientoRevisionesTagPersonaRow>> Cr_SeguimientoRevisionesTag_Personas_Obtener(
    int codEmpresa,
    CrSeguimientoRevisionesTagDetalleRequest request)
        {
            return _db.Cr_SeguimientoRevisionesTag_Personas_Obtener(codEmpresa, request);
        }

        #endregion

        #region Seguimiento

        public ErrorDto<CrSeguimientoRevisionesTagSeguimientoResponse> Cr_SeguimientoRevisionesTag_Seguimiento_Obtener(
            int codEmpresa,
            CrSeguimientoRevisionesTagSeguimientoRequest request)
        {
            return _db.Cr_SeguimientoRevisionesTag_Seguimiento_Obtener(codEmpresa, request);
        }

        #endregion

        #region Revision

        public ErrorDto<CrSeguimientoRevisionesTagRevisionResponse> Cr_SeguimientoRevisionesTag_Revision_Obtener(
            int codEmpresa,
            CrSeguimientoRevisionesTagDetalleRequest request)
        {
            return _db.Cr_SeguimientoRevisionesTag_Revision_Obtener(codEmpresa, request);
        }

        public ErrorDto<CrSeguimientoRevisionesTagNotaLargoResponse> Cr_SeguimientoRevisionesTag_NotaLargo_Obtener(
            int codEmpresa,
            string tagCodigo)
        {
            return _db.Cr_SeguimientoRevisionesTag_NotaLargo_Obtener(codEmpresa, tagCodigo);
        }

        public ErrorDto<CrSeguimientoRevisionesTagAvisoResponse> Cr_SeguimientoRevisionesTag_Aviso_Obtener(
            int codEmpresa,
            string tagCodigo)
        {
            return _db.Cr_SeguimientoRevisionesTag_Aviso_Obtener(codEmpresa, tagCodigo);
        }

        public ErrorDto<CrSeguimientoRevisionesTagAplicarResponse> Cr_SeguimientoRevisionesTag_Aplicar(
            int codEmpresa,
            string usuario,
            CrSeguimientoRevisionesTagAplicarRequest request)
        {
            return _db.Cr_SeguimientoRevisionesTag_Aplicar(codEmpresa, usuario, request);
        }

        #endregion
    }
}