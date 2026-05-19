using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.Creditos;
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

        public ErrorDto<CrSeguimientoRevisionesTagDetalleCreditoResponse> Cr_SeguimientoRevisionesTag_DetalleCredito_Obtener(
            int codEmpresa,
            CrSeguimientoRevisionesTagDetalleRequest request)
        {
            return _db.Cr_SeguimientoRevisionesTag_DetalleCredito_Obtener(codEmpresa, request);
        }

        public ErrorDto<CrSeguimientoRevisionesTagSeguimientoResponse> Cr_SeguimientoRevisionesTag_Seguimiento_Obtener(
            int codEmpresa,
            CrSeguimientoRevisionesTagSeguimientoRequest request)
        {
            return _db.Cr_SeguimientoRevisionesTag_Seguimiento_Obtener(codEmpresa, request);
        }

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
    }
}
