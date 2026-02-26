using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX_Polizas;
using Galileo_API.Models.ProGrX_Polizas;

namespace Galileo_API.BusinessLogic.ProGrX_Polizas
{
    public class FrmCrPolizaProcPrevistaBL
    {
        private readonly FrmCrPolizaProcPrevistaDB _db;

        public FrmCrPolizaProcPrevistaBL(IConfiguration config)
        {
            _db = new FrmCrPolizaProcPrevistaDB(config);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> Cr_PolProcPrevista_PolizaFacturables_Lista(int CodEmpresa)
        {
            return _db.Cr_PolProcPrevista_PolizaFacturables_Lista(CodEmpresa);
        }

        public ErrorDto Cr_PolProcPrevista_Corte_Detalle_Cargar(int CodEmpresa, string usuario, CrPolProcPrevistaDetalleAddRequest request)
        {
            return _db.Cr_PolProcPrevista_Corte_Detalle_Cargar(CodEmpresa, usuario, request);
        }

        public ErrorDto Cr_PolProcPrevista_Corte_Detalle_Eliminar(int CodEmpresa, string usuario, CrPolProcPrevistaDetalleEliminarRequest request)
        {
            return _db.Cr_PolProcPrevista_Corte_Detalle_Eliminar(CodEmpresa, usuario, request);
        }

        public ErrorDto<List<CrPolProcprevistaDetalleDto>> Cr_PolProcPrevista_Corte_Detalle_Consulta(
            int CodEmpresa,
            string codPoliza,
            DateTime corte)
        {
            return _db.Cr_PolProcPrevista_Corte_Detalle_Consulta(CodEmpresa, codPoliza, corte);
        }

        public ErrorDto<List<CrPolProcPrevistaConciliaDto>> Cr_PolProcPrevista_Corte_Concilia_Consulta(
           int CodEmpresa,
           CrPolProcPrevistaConciliaConsultaRequest request)
        {
            return _db.Cr_PolProcPrevista_Corte_Concilia_Consulta(CodEmpresa, request);
        }
    }
}
