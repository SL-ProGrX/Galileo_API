using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX_Polizas;
using Galileo_API.Models.ProGrX_Polizas;

namespace Galileo_API.BusinessLogic.ProGrX_Polizas
{
    public class FrmCrPolizasCargaLoteBL
    {
        private readonly FrmCrPolizasCargaLoteDB _DB;

        public FrmCrPolizasCargaLoteBL(IConfiguration config)
        {
                _DB = new FrmCrPolizasCargaLoteDB(config);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> CrdPolizasCargaLote_Cliente_Obtener(int CodEmpresa)
        {
            return _DB.CrdPolizasCargaLote_Cliente_Obtener(CodEmpresa);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> CrdPolizasCargaLote_Aseguradora_Obtener(int CodEmpresa)
        {
            return _DB.CrdPolizasCargaLote_Aseguradora_Obtener(CodEmpresa);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> CrdPolizasCargaLote_Banco_Obtener(int CodEmpresa, string usuario)
        {
            return _DB.CrdPolizasCargaLote_Banco_Obtener(CodEmpresa, usuario);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> CrdPolizasCargaLote_Cuenta_Obtener(
           int CodEmpresa,
           CrdPolizasCargaLoteCuentaCatalogoRequest request)
        {
            return _DB.CrdPolizasCargaLote_Cuenta_Obtener(CodEmpresa, request);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> CrdPolizasCargaLote_Prideduc_Obtener(int codEmpresa, string usuario, int codContabilidad)
        {
            return _DB.CrdPolizasCargaLote_Prideduc_Obtener(codEmpresa, usuario, codContabilidad);
        }

        public ErrorDto<CrdPolizasCargaLoteCargaResponse> CrdPolizasCargaLote_Cargar(
                int codEmpresa,
                string usuario,
                CrdPolizasCargaLoteCargaRequest request)
        {
            return _DB.CrdPolizasCargaLote_Cargar(codEmpresa, usuario, request);
        }
    }
}
