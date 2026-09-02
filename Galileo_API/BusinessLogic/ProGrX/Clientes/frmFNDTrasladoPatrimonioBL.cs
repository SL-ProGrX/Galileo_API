using Galileo.DataBaseTier.ProGrX.Clientes;
using Galileo.Models.ProGrX.Clientes;
using Galileo.Models.ERROR;

namespace Galileo.BusinessLogic.ProGrX.Clientes
{
    public class FrmFndTrasladoPatrimonioBl
    {
        private readonly FrmFndTrasladoPatrimonioDB _db;

        public FrmFndTrasladoPatrimonioBl(IConfiguration config)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));
            _db = new FrmFndTrasladoPatrimonioDB(config);
        }

        public ErrorDto<List<FndTrasladoPatrimonioPlan>> Fnd_TrasladoPatrimonio_Planes_Obtener(int CodEmpresa, string IdOperadora)
        {
            return _db.Fnd_TrasladoPatrimonio_Planes_Obtener(CodEmpresa, IdOperadora);
        }

        public ErrorDto<FndTrasladoPatrimonioDetalle?> Fnd_TrasladoPatrimonio_PlanDetalle_Obtener(int CodEmpresa, string IdOperadora, string CodPlan)
        {
            return _db.Fnd_TrasladoPatrimonio_PlanDetalle_Obtener(CodEmpresa, IdOperadora, CodPlan);
        }

        public ErrorDto<List<FndTrasladoPatrimonioContrato>> Fnd_TrasladoPatrimonio_Contratos_Obtener(
            int CodEmpresa, string IdOperadora, string CodPlan, string Destino, bool Marcado)
        {
            return _db.Fnd_TrasladoPatrimonio_Contratos_Obtener(CodEmpresa, IdOperadora, CodPlan, Destino, Marcado);
        }

        public ErrorDto<FndDocumentoConsecutivoResult?> Fnd_TrasladoPatrimonio_DocumentoConsecutivo_Obtener(int CodEmpresa, FndDocumentoConsecutivoRequest request)
        {
            return _db.Fnd_TrasladoPatrimonio_DocumentoConsecutivo_Obtener(CodEmpresa, request);
        }

        public ErrorDto<FndDocumentoConsecutivoAseResult?> Fnd_TrasladoPatrimonio_DocumentoConsecutivoAse_Obtener(
            int CodEmpresa, FndDocumentoConsecutivoAseRequest request)
        {
            return _db.Fnd_TrasladoPatrimonio_DocumentoConsecutivoAse_Obtener(CodEmpresa, request);
        }

        public ErrorDto<SimpleSuccessResult> Fnd_ContratoDetalle_Insertar(int CodEmpresa, FndContratoDetalleInsertRequest request)
        {
            return _db.Fnd_ContratoDetalle_Insertar(CodEmpresa, request);
        }

        public ErrorDto<SimpleSuccessResult> Fnd_Contrato_UpdateAportesRendimiento(int CodEmpresa, FndContratoUpdateRequest request)
        {
            return _db.Fnd_Contrato_UpdateAportesRendimiento(CodEmpresa, request);
        }

        public ErrorDto<SimpleSuccessResult> Fnd_Documento_Insertar(int CodEmpresa, FndDocumentoInsertRequest request)
        {
            return _db.Fnd_Documento_Insertar(CodEmpresa, request);
        }

        public ErrorDto<SimpleSuccessResult> Fnd_Asiento_Insertar(int CodEmpresa, FndAsientoInsertRequest request)
        {
            return _db.Fnd_Asiento_Insertar(CodEmpresa, request);
        }

        public ErrorDto<SimpleSuccessResult> SifTransaccion_Insertar(int CodEmpresa, SifTransaccionInsertRequest request)
        {
            return _db.SifTransaccion_Insertar(CodEmpresa, request);
        }

        public ErrorDto<SimpleSuccessResult> SifDocsAsiento_Ejecutar(int CodEmpresa, SifDocsAsientoRequest request)
        {
            return _db.SifDocsAsiento_Ejecutar(CodEmpresa, request);
        }

        public ErrorDto<List<FndTrasladoPatrimonioSocioDetalle>> Fnd_TrasladoPatrimonio_SocioDetalle_Obtener(
            int CodEmpresa, string Tcon, string Ncon)
        {
            return _db.Fnd_TrasladoPatrimonio_SocioDetalle_Obtener(CodEmpresa, Tcon, Ncon);
        }

        public ErrorDto<SimpleSuccessResult> Fnd_AhorroConsolidado_Procesar(int CodEmpresa, FndAhorroConsolidadoRequest request)
        {
            return _db.Fnd_AhorroConsolidado_Procesar(CodEmpresa, request);
        }

        public ErrorDto<SimpleSuccessResult> SifTransaccionPatrimonio_Insertar(int CodEmpresa, SifTransaccionPatrimonioInsertRequest request)
        {
            return _db.SifTransaccionPatrimonio_Insertar(CodEmpresa, request);
        }

        public ErrorDto<List<FndAhorroDetalladoResumen>> Fnd_AhorroDetallado_Resumen_Obtener(int CodEmpresa, FndAhorroDetalladoResumenRequest request)
        {
            return _db.Fnd_AhorroDetallado_Resumen_Obtener(CodEmpresa, request);
        }

        public ErrorDto<ParAfahCuentasResult?> ParAfah_Cuentas_Obtener(int CodEmpresa)
        {
            return _db.ParAfah_Cuentas_Obtener(CodEmpresa);
        }

        public ErrorDto<FndTrasladoPatrimonioGlobalesResult?> Fnd_TrasladoPatrimonio_Globales_Obtener(
            int CodEmpresa,
            string usuario,
            int codContabilidad)
        {
            return _db.Fnd_TrasladoPatrimonio_Globales_Obtener(
                CodEmpresa,
                usuario,
                codContabilidad);
        }
    }
}