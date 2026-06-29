using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.Creditos;
using Galileo_API.Models.ProGrX.Creditos;

namespace Galileo_API.BusinessLogic.ProGrX.Creditos
{
    public class FrmCRPlanPagosBL
    {
        private readonly FrmCRPlanPagosDB _db;

        public FrmCRPlanPagosBL(IConfiguration config)
        {
            _db = new FrmCRPlanPagosDB(config);
        }

        public ErrorDto<CrPlanPagosObtenerDto> CR_PlanPagos_Obtener(int CodEmpresa, int operacion,string? usuario)
        {
            return _db.CR_PlanPagos_Obtener(CodEmpresa, operacion, usuario);
        }

        public ErrorDto<CrPlanPagosListaResult<CrPlanPagosCargosData>> CR_PlanPagos_Cargos_Lista_Obtener(
            int CodEmpresa,
            int operacion,
            decimal idSeq,
            FiltrosLazyLoadData parametros)
        {
            return _db.CR_PlanPagos_Cargos_Lista_Obtener(CodEmpresa, operacion, idSeq, parametros);
        }

        public ErrorDto<CrPlanPagosListaResult<CrPlanPagosCargosData>> CR_PlanPagos_Cargos_Lista_Export(
            int CodEmpresa,
            int operacion,
            decimal idSeq,
            FiltrosLazyLoadData parametros)
        {
            return _db.CR_PlanPagos_Cargos_Lista_Export(CodEmpresa, operacion, idSeq, parametros);
        }

        public ErrorDto<CrPlanPagosListaResult<CrPlanPagosPolizasData>> CR_PlanPagos_Polizas_Lista_Obtener(
            int CodEmpresa,
            int operacion,
            decimal idSeq,
            FiltrosLazyLoadData parametros)
        {
            return _db.CR_PlanPagos_Polizas_Lista_Obtener(CodEmpresa, operacion, idSeq, parametros);
        }

        public ErrorDto<CrPlanPagosListaResult<CrPlanPagosPolizasData>> CR_PlanPagos_Polizas_Lista_Export(
            int CodEmpresa,
            int operacion,
            decimal idSeq,
            FiltrosLazyLoadData parametros)
        {
            return _db.CR_PlanPagos_Polizas_Lista_Export(CodEmpresa, operacion, idSeq, parametros);
        }

        public ErrorDto<CrPlanPagosListaResult<CrPlanPagosDocumentosData>> CR_PlanPagos_Documentos_Lista_Obtener(
            int CodEmpresa,
            int operacion,
            decimal idSeq,
            bool todos,
            FiltrosLazyLoadData parametros)
        {
            return _db.CR_PlanPagos_Documentos_Lista_Obtener(CodEmpresa, operacion, idSeq, todos, parametros);
        }

        public ErrorDto<CrPlanPagosListaResult<CrPlanPagosDocumentosData>> CR_PlanPagos_Documentos_Lista_Export(
            int CodEmpresa,
            int operacion,
            decimal idSeq,
            bool todos,
            FiltrosLazyLoadData parametros)
        {
            return _db.CR_PlanPagos_Documentos_Lista_Export(CodEmpresa, operacion, idSeq, todos, parametros);
        }

        public ErrorDto<List<CrPlanPagosValoresData>> CR_PlanPagos_DocumentoValores_Obtener(
            int CodEmpresa,
            string tipoDocumento,
            string transaccion)
        {
            return _db.CR_PlanPagos_DocumentoValores_Obtener(CodEmpresa, tipoDocumento, transaccion);
        }

        public ErrorDto<CrPlanPagosListaResult<CrPlanPagosAjustesData>> CR_PlanPagos_Ajustes_Lista_Obtener(
            int CodEmpresa,
            int operacion,
            FiltrosLazyLoadData parametros)
        {
            return _db.CR_PlanPagos_Ajustes_Lista_Obtener(CodEmpresa, operacion, parametros);
        }

        public ErrorDto<CrPlanPagosListaResult<CrPlanPagosAjustesData>> CR_PlanPagos_Ajustes_Lista_Export(
            int CodEmpresa,
            int operacion,
            FiltrosLazyLoadData parametros)
        {
            return _db.CR_PlanPagos_Ajustes_Lista_Export(CodEmpresa, operacion, parametros);
        }

        public ErrorDto CR_PlanPagos_Activar(int CodEmpresa, CrPlanPagosActivarRequest request)
        {
            return _db.CR_PlanPagos_Activar(CodEmpresa, request);
        }

        public ErrorDto CR_PlanPagos_Revisar(int CodEmpresa, CrPlanPagosRevisarRequest request)
        {
            return _db.CR_PlanPagos_Revisar(CodEmpresa, request);
        }

        public ErrorDto CR_PlanPagos_Email_Enviar(int CodEmpresa, CrPlanPagosEmailRequest request)
        {
            return _db.CR_PlanPagos_Email_Enviar(CodEmpresa, request);
        }
    }
}