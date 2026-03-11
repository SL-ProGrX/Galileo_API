using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX_Polizas;

namespace Galileo_API.BusinessLogic.ProGrX_Polizas
{
    public class FrmPolizaReclamoBL
    {
        private readonly FrmPolizaReclamoDB _db;
    
        public FrmPolizaReclamoBL(IConfiguration config)
        {
           _db = new FrmPolizaReclamoDB(config);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> Poliza_Reclamo_Motivos_Lista(
            int codEmpresa,
            string codPoliza)
        {
            return _db.Poliza_Reclamo_Motivos_Lista(codEmpresa, codPoliza);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> Poliza_Reclamo_Causas_Lista(
            int codEmpresa,
            string codPoliza)
        {
            return _db.Poliza_Reclamo_Causas_Lista(codEmpresa, codPoliza);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> Poliza_Reclamo_Estados_Lista(int codEmpresa)
        {
            return _db.Poliza_Reclamo_Estados_Lista(codEmpresa);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> Poliza_Reclamo_Bancos_Lista(
            int codEmpresa,
            string usuario)
        {
            return _db.Poliza_Reclamo_Bancos_Lista(codEmpresa, usuario);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> Poliza_Reclamo_Cuentas_Lista(
            int codEmpresa,
            string cedula,
            int bancoId)
        {
            return _db.Poliza_Reclamo_Cuentas_Lista(codEmpresa, cedula, bancoId);
        }
    }
}
