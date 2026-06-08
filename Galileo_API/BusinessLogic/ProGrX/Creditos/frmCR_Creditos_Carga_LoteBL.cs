using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.Creditos;
using Galileo_API.Models.ProGrX.Creditos;

namespace Galileo_API.BusinessLogic.ProGrX.Creditos
{
    public class FrmCrCreditosCargaLoteBL
    {
        private readonly FrmCrCreditosCargaLoteDB _db;

        public FrmCrCreditosCargaLoteBL(IConfiguration config)
        {
            _db = new FrmCrCreditosCargaLoteDB(config);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> CrCreditosCargaLote_Cliente_Obtener(int CodEmpresa)
        {
            return _db.CrCreditosCargaLote_Cliente_Obtener(CodEmpresa);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> CrCreditosCargaLote_Destinos_Obtener(int CodEmpresa, string codigo)
        {
            return _db.CrCreditosCargaLote_Destinos_Obtener(CodEmpresa, codigo);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> CrCreditosCargaLote_ConceptosDesembolso_Obtener(int CodEmpresa)
        {
            return _db.CrCreditosCargaLote_ConceptosDesembolso_Obtener(CodEmpresa);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> CrCreditosCargaLote_ObtenerDeductoras(int CodEmpresa)
        {
            return _db.CrCreditosCargaLote_ObtenerDeductoras(CodEmpresa);
        }

        public ErrorDto<List<FrecuenciaReductora>> CrCreditosCargaLote_ObtenerFrecuenciaDeductora(int CodEmpresa, string CodInstitucion)
        {
            return _db.CrCreditosCargaLote_ObtenerFrecuenciaDeductora(CodEmpresa, CodInstitucion);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> CrCreditosCargaLote_Banco_Obtener(int CodEmpresa, string usuario)
        {
            return _db.CrCreditosCargaLote_Banco_Obtener(CodEmpresa, usuario);
        }
    }
}
