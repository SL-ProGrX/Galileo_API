using Galileo.DataBaseTier.ProGrX.Fondos;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Fondos;

namespace Galileo.BusinessLogic.ProGrX.Fondos
{
    public class FrmFndRetirosyLiquidacionesBL
    {
        private readonly FrmFndRetirosyLiquidacionesDB _Db;

        public FrmFndRetirosyLiquidacionesBL(IConfiguration config)
        {
            _Db = new FrmFndRetirosyLiquidacionesDB(config);
        }

        public ErrorDto<FndSeguridadRango> FND_RetLiq_SeguridadRango_Obtener(int CodEmpresa, int Operadora, string Plan, string Usuario)
        {
            return _Db.FND_RetLiq_SeguridadRango_Obtener(CodEmpresa, Operadora, Plan, Usuario);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> FND_RetLiq_Bancos_Obtener(int CodEmpresa, string Usuario)
        {
            return _Db.FND_RetLiq_Bancos_Obtener(CodEmpresa, Usuario);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> FND_RetLiq_CuentasBancarias_Obtener(int CodEmpresa, string Cedula, int Banco)
        {
            return _Db.FND_RetLiq_CuentasBancarias_Obtener(CodEmpresa, Cedula, Banco);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> FND_RetLiq_RetencionConceptos_Obtener(int CodEmpresa, string Usuario)
        {
            return _Db.FND_RetLiq_RetencionConceptos_Obtener(CodEmpresa, Usuario);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> FND_RetLiq_PlanesDestino_Obtener(int CodEmpresa, int Operadora, string Plan, int Contrato)
        {
            return _Db.FND_RetLiq_PlanesDestino_Obtener(CodEmpresa, Operadora, Plan, Contrato);
        }

        public ErrorDto<List<FndRetLiqRebajosData>> FND_RetLiq_Rebajos_Obtener(int CodEmpresa, string Usuario)
        {
            return _Db.FND_RetLiq_Rebajos_Obtener(CodEmpresa, Usuario);
        }

        public ErrorDto<FndRetLiqConsultaData> FND_RetLiq_Consulta_Obtener(int CodEmpresa, int Operadora, string Plan, int Contrato)
        {
            return _Db.FND_RetLiq_Consulta_Obtener(CodEmpresa, Operadora, Plan, Contrato);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> FND_RetLiq_PagoTerceros_Obtener(int CodEmpresa, int Operadora, string Plan, int Contrato, string Cedula)
        {
            return _Db.FND_RetLiq_PagoTerceros_Obtener(CodEmpresa, Operadora, Plan, Contrato, Cedula);
        }

        public ErrorDto<decimal> FND_RetLiq_Multa_Obtener(int CodEmpresa, int Operadora, string Plan, int Contrato, decimal Monto)
        {
            return _Db.FND_RetLiq_Multa_Obtener(CodEmpresa, Operadora, Plan, Contrato, Monto);
        }

        public ErrorDto<FndRetLiqRentaGlobalData> FND_RetLiq_RentaGlobal_Obtener(int CodEmpresa, string Cedula, decimal RndRetiro, string Plan)
        {
            return _Db.FND_RetLiq_RentaGlobal_Obtener(CodEmpresa, Cedula, RndRetiro, Plan);
        }

        public ErrorDto<FndRetLiqProcesoData> FND_RetLiq_Aplicar(int CodEmpresa, string Filtro)
        {
            return _Db.FND_RetLiq_Aplicar(CodEmpresa, Filtro);
        }
    }
}