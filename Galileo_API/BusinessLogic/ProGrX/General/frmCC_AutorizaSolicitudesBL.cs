using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.GEN;

namespace Galileo.BusinessLogic
{
    public class FrmCcAutorizaSolicitudesBl
    {
        private readonly FrmCcAutorizaSolicitudesDb _db;

        public FrmCcAutorizaSolicitudesBl(
            IConfiguration config)
        {
            _db = new FrmCcAutorizaSolicitudesDb(config);
        }

        public ErrorDto<List<CCGenericList>>
            CC_Cuentas_Obtener(
                int CodEmpresa)
        {
            return _db
                .CC_Cuentas_Obtener(
                    CodEmpresa);
        }

        public ErrorDto<List<AutorizaSolicitudesCreditoData>>
            CC_ModuloCredito_Obtener(
                int CodEmpresa,
                int? CodBanco,
                string FechaInicio,
                string FechaCorte)
        {
            return _db
                .CC_ModuloCredito_Obtener(
                    CodEmpresa,
                    CodBanco,
                    FechaInicio,
                    FechaCorte);
        }

        public ErrorDto<List<AutorizaSolicitudesFondosData>>
            CC_ModuloFondos_Obtener(
                int CodEmpresa,
                int? CodBanco,
                string FechaInicio,
                string FechaCorte)
        {
            return _db
                .CC_ModuloFondos_Obtener(
                    CodEmpresa,
                    CodBanco,
                    FechaInicio,
                    FechaCorte);
        }

        public ErrorDto<List<AutorizaSolicitudesLiquidacionData>>
            CC_ModuloLiquidacion_Obtener(
                int CodEmpresa,
                int? CodBanco,
                string FechaInicio,
                string FechaCorte)
        {
            return _db
                .CC_ModuloLiquidacion_Obtener(
                    CodEmpresa,
                    CodBanco,
                    FechaInicio,
                    FechaCorte);
        }

        public ErrorDto<List<AutorizaSolicitudesBeneficiosData>>
            CC_ModuloBeneficios_Obtener(
                int CodEmpresa,
                int? CodBanco,
                string FechaInicio,
                string FechaCorte)
        {
            return _db
                .CC_ModuloBeneficios_Obtener(
                    CodEmpresa,
                    CodBanco,
                    FechaInicio,
                    FechaCorte);
        }

        public ErrorDto<List<AutorizaSolicitudesHipotecarioData>>
            CC_ModuloHipotecario_Obtener(
                int CodEmpresa,
                int? CodBanco,
                string FechaInicio,
                string FechaCorte)
        {
            return _db
                .CC_ModuloHipotecario_Obtener(
                    CodEmpresa,
                    CodBanco,
                    FechaInicio,
                    FechaCorte);
        }

        public ErrorDto
            CC_ModuloCredito_Autorizar(
                int CodEmpresa,
                string Usuario,
                int Id_Solicitud)
        {
            return _db
                .CC_ModuloCredito_Autorizar(
                    CodEmpresa,
                    Usuario,
                    Id_Solicitud);
        }

        public ErrorDto
            CC_ModuloFondos_Autorizar(
                int CodEmpresa,
                string Usuario,
                int Consec)
        {
            return _db
                .CC_ModuloFondos_Autorizar(
                    CodEmpresa,
                    Usuario,
                    Consec);
        }

        public ErrorDto
            CC_ModuloLiquidacion_Autorizar(
                int CodEmpresa,
                string Usuario,
                int Consec)
        {
            return _db
                .CC_ModuloLiquidacion_Autorizar(
                    CodEmpresa,
                    Usuario,
                    Consec);
        }

        public ErrorDto
            CC_ModuloBeneficios_Autorizar(
                int CodEmpresa,
                string Usuario,
                int Consec,
                string Cod_Beneficio)
        {
            return _db
                .CC_ModuloBeneficios_Autorizar(
                    CodEmpresa,
                    Usuario,
                    Consec,
                    Cod_Beneficio);
        }

        public ErrorDto
            CC_ModuloHipotecario_Autorizar(
                int CodEmpresa,
                string Usuario,
                int CodigoDesembolso)
        {
            return _db
                .CC_ModuloHipotecario_Autorizar(
                    CodEmpresa,
                    Usuario,
                    CodigoDesembolso);
        }
    }
}