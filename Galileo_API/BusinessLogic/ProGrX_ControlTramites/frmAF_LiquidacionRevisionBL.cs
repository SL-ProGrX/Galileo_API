namespace Galileo_API.BusinessLogic.ProGrX_ControlTramites
{
    using Galileo.Models.ERROR;
    using Galileo_API.DataBaseTier.ProGrX_ControlTramites;
    using Galileo_API.Models.ProGrX_ControlTramites;

    public class FrmAfLiquidacionRevisionBL
    {
        private readonly FrmAfLiquidacionRevisionDB _db;

        public FrmAfLiquidacionRevisionBL(IConfiguration config)
        {
            _db = new FrmAfLiquidacionRevisionDB(config);
        }

        public ErrorDto<List<AfLiquidacionRevisionListaModel>> AF_LiquidacionRevision_Obtener(
            int CodEmpresa,
            string? Cedula)
        {
            return _db.AF_LiquidacionRevision_Obtener(CodEmpresa, Cedula);
        }

        public ErrorDto<AfLiquidacionRevisionDetalleModel?> AF_LiquidacionRevision_Detalle_Obtener(
            int CodEmpresa,
            int Consec)
        {
            return _db.AF_LiquidacionRevision_Detalle_Obtener(CodEmpresa, Consec);
        }

        public ErrorDto<List<AfLiquidacionRevisionOperacionModel>> AF_LiquidacionRevision_Operaciones_Obtener(
            int CodEmpresa,
            int Consec)
        {
            return _db.AF_LiquidacionRevision_Operaciones_Obtener(CodEmpresa, Consec);
        }

        public ErrorDto<List<AfLiquidacionRevisionSeguimientoModel>> AF_LiquidacionRevision_Seguimiento_Obtener(
            int CodEmpresa,
            string Cedula,
            string Documento)
        {
            return _db.AF_LiquidacionRevision_Seguimiento_Obtener(CodEmpresa, Cedula, Documento);
        }

        public ErrorDto<List<AfLiquidacionRevisionEtiquetaModel>> AF_LiquidacionRevision_Etiquetas_Obtener(
            int CodEmpresa,
            string Usuario)
        {
            return _db.AF_LiquidacionRevision_Etiquetas_Obtener(CodEmpresa, Usuario);
        }

        public ErrorDto<List<AfLiquidacionRevisionOmisionModel>> AF_LiquidacionRevision_Omisiones_Obtener(
            int CodEmpresa,
            string Cedula,
            string Documento)
        {
            return _db.AF_LiquidacionRevision_Omisiones_Obtener(CodEmpresa, Cedula, Documento);
        }

        public ErrorDto<AfLiquidacionRevisionAvisoModel?> AF_LiquidacionRevision_Aviso_Obtener(
            int CodEmpresa,
            string TagCodigo)
        {
            return _db.AF_LiquidacionRevision_Aviso_Obtener(CodEmpresa, TagCodigo);
        }

        public ErrorDto<AfLiquidacionRevisionOmisionInsertarModel?> AF_LiquidacionRevision_Omision_Insertar(
            int CodEmpresa,
            AfLiquidacionRevisionOmisionInsertarRequest request)
        {
            if (request == null
                || string.IsNullOrWhiteSpace(request.Cedula)
                || string.IsNullOrWhiteSpace(request.Id_Error)
                || string.IsNullOrWhiteSpace(request.Documento)
                || string.IsNullOrWhiteSpace(request.Usuario))
            {
                return new ErrorDto<AfLiquidacionRevisionOmisionInsertarModel?>
                {
                    Code = -1,
                    Description = "Cédula, omisión, documento y usuario son requeridos.",
                    Result = null,
                };
            }

            return _db.AF_LiquidacionRevision_Omision_Insertar(CodEmpresa, request);
        }

        public ErrorDto AF_LiquidacionRevision_Omision_Eliminar(
            int CodEmpresa,
            AfLiquidacionRevisionOmisionEliminarRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Linea_Err))
            {
                return new ErrorDto
                {
                    Code = -1,
                    Description = "LINEA_ERR es requerido.",
                };
            }

            return _db.AF_LiquidacionRevision_Omision_Eliminar(CodEmpresa, request);
        }

        public ErrorDto AF_LiquidacionRevision_Omisiones_Aplicar(
            int CodEmpresa,
            AfLiquidacionRevisionOmisionesAplicarRequest request)
        {
            if (request == null
                || string.IsNullOrWhiteSpace(request.Cedula)
                || string.IsNullOrWhiteSpace(request.Documento))
            {
                return new ErrorDto
                {
                    Code = -1,
                    Description = "Cédula y documento son requeridos.",
                };
            }

            return _db.AF_LiquidacionRevision_Omisiones_Aplicar(CodEmpresa, request);
        }

        public ErrorDto AF_LiquidacionRevision_Aplicar(
            int CodEmpresa,
            AfLiquidacionRevisionAplicarRequest request)
        {
            if (request == null
                || string.IsNullOrWhiteSpace(request.Cedula)
                || string.IsNullOrWhiteSpace(request.Documento)
                || string.IsNullOrWhiteSpace(request.Tag)
                || string.IsNullOrWhiteSpace(request.Usuario))
            {
                return new ErrorDto
                {
                    Code = -1,
                    Description = "Cédula, documento, etiqueta y usuario son requeridos.",
                };
            }

            return _db.AF_LiquidacionRevision_Aplicar(CodEmpresa, request);
        }
    }
}
