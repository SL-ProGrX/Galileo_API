using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.GEN;

namespace Galileo.BusinessLogic
{
    public class FrmCcAutorizaSolicitudesBl
    {
        private readonly FrmCcAutorizaSolicitudesDb DbfrmCC_AutorizaSolicitudes;

        public FrmCcAutorizaSolicitudesBl(IConfiguration config)
        {
            DbfrmCC_AutorizaSolicitudes = new FrmCcAutorizaSolicitudesDb(config);
        }

        public List<CCGenericList> CC_Cuentas_Obtener(int CodEmpresa)
        {
            return DbfrmCC_AutorizaSolicitudes.CC_Cuentas_Obtener(CodEmpresa);
        }

        public List<AutorizaSolicitudesCreditoData> CC_ModuloCredito_Obtener(int CodEmpresa, int? CodBanco, string FechaInicio, string FechaCorte)
        {
            return DbfrmCC_AutorizaSolicitudes.CC_ModuloCredito_Obtener(CodEmpresa, CodBanco, FechaInicio, FechaCorte);
        }

        public List<AutorizaSolicitudesFondosData> CC_ModuloFondos_Obtener(int CodEmpresa, int? CodBanco, string FechaInicio, string FechaCorte)
        {
            return DbfrmCC_AutorizaSolicitudes.CC_ModuloFondos_Obtener(CodEmpresa, CodBanco, FechaInicio, FechaCorte);
        }

        public List<AutorizaSolicitudesLiquidacionData> CC_ModuloLiquidacion_Obtener(int CodEmpresa, int? CodBanco, string FechaInicio, string FechaCorte)
        {
            return DbfrmCC_AutorizaSolicitudes.CC_ModuloLiquidacion_Obtener(CodEmpresa, CodBanco, FechaInicio, FechaCorte);
        }

        public List<AutorizaSolicitudesBeneficiosData> CC_ModuloBeneficios_Obtener(int CodEmpresa, int? CodBanco, string FechaInicio, string FechaCorte)
        {
            return DbfrmCC_AutorizaSolicitudes.CC_ModuloBeneficios_Obtener(CodEmpresa, CodBanco, FechaInicio, FechaCorte);
        }

        public List<AutorizaSolicitudesHipotecarioData> CC_ModuloHipotecario_Obtener(int CodEmpresa, int? CodBanco, string FechaInicio, string FechaCorte)
        {
            return DbfrmCC_AutorizaSolicitudes.CC_ModuloHipotecario_Obtener(CodEmpresa, CodBanco, FechaInicio, FechaCorte);
        }

        public ErrorDto CC_ModuloCredito_Autorizar(int CodEmpresa, string Usuario, int Id_Solicitud)
        {
            return DbfrmCC_AutorizaSolicitudes.CC_ModuloCredito_Autorizar(CodEmpresa, Usuario, Id_Solicitud);
        }

        public ErrorDto CC_ModuloFondos_Autorizar(int CodEmpresa, string Usuario, int Consec)
        {
            return DbfrmCC_AutorizaSolicitudes.CC_ModuloFondos_Autorizar(CodEmpresa, Usuario, Consec);
        }

        public ErrorDto CC_ModuloLiquidacion_Autorizar(int CodEmpresa, string Usuario, int Consec)
        {
            return DbfrmCC_AutorizaSolicitudes.CC_ModuloLiquidacion_Autorizar(CodEmpresa, Usuario, Consec);
        }

        public ErrorDto CC_ModuloBeneficios_Autorizar(int CodEmpresa, string Usuario, int Consec, string Cod_Beneficio)
        {
            return DbfrmCC_AutorizaSolicitudes.CC_ModuloBeneficios_Autorizar(CodEmpresa, Usuario, Consec, Cod_Beneficio);
        }

        public ErrorDto CC_ModuloHipotecario_Autorizar(int CodEmpresa, string Usuario, int CodigoDesembolso)
        {
            return DbfrmCC_AutorizaSolicitudes.CC_ModuloHipotecario_Autorizar(CodEmpresa, Usuario, CodigoDesembolso);
        }
    }
}