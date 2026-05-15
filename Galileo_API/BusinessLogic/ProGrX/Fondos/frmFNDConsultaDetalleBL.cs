
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Fondos;
using Galileo_API.DataBaseTier.ProGrX.Fondos;

namespace Galileo_API.BusinessLogic.ProGrX.Fondos
{
    public class FrmFndConsultaDetalleBL
    {

        private readonly FrmFndConsultaDetalleDB _Db;

        public FrmFndConsultaDetalleBL(IConfiguration? config)
        {
            _Db = new FrmFndConsultaDetalleDB(config);
        }

        public ErrorDto<FndConsultaDetalleData> FndConsultaDetalle_Obtener(int CodEmpresa, string vCedula, string cod_plan, int cod_contrato)
        {
            return _Db.FndConsultaDetalle_Obtener(CodEmpresa, vCedula, cod_plan, cod_contrato);
        }

        public ErrorDto<List<FndConsultaContratoDetallesData>> FndConsultaContratos_Obtener(int CodEmpresa, string vCedula, string cod_plan, int cod_contrato)
        {
            return _Db.FndConsultaContratos_Obtener(CodEmpresa, vCedula, cod_plan, cod_contrato);
        }

        public ErrorDto<List<FndConsultaSubCuentasData>> FndConsultaSubCuentas_Obtener(int CodEmpresa, string vCedula, string cod_plan, int cod_contrato, string subCuenta)
        {
            return _Db.FndConsultaSubCuentas_Obtener(CodEmpresa, vCedula, cod_plan, cod_contrato, subCuenta);
        }

        public ErrorDto<List<FndConsultaSubCuentasDetalleData>> FndConsultaSubCuentasDetalle_Obtener(int CodEmpresa, string vCedula, string cod_plan, int cod_contrato, string subCuenta)
        {
            return _Db.FndConsultaSubCuentasDetalle_Obtener(CodEmpresa, vCedula, cod_plan, cod_contrato, subCuenta);
        }

        public ErrorDto<List<FndConsultaBeneficiarioDetalle>> FndConsultaContratosBeneficiario_Obtener(int CodEmpresa, string vCedula, string cod_plan, int cod_contrato)
        {
            return _Db.FndConsultaContratosBeneficiario_Obtener(CodEmpresa, vCedula, cod_plan, cod_contrato);
        }

        public ErrorDto<List<FndConsultaMovTransitoData>> FndConsultaMovTransito_Obtener(int CodEmpresa, string cuenta)
        {
            return _Db.FndConsultaMovTransito_Obtener(CodEmpresa, cuenta);
        }
    }
}