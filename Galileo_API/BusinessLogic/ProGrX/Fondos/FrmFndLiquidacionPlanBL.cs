using Galileo.Models;
using Galileo.Models.ERROR;
using PgxAPI.DataBaseTier.ProGrX.Fondos;
using PgxAPI.Models.ProGrX.Fondos;
using static Org.BouncyCastle.Math.EC.ECCurve;

namespace PgxAPI.BusinessLogic.ProGrX.Fondos
{
    public class FrmFndLiquidacionPlanBL
    {
        private readonly FrmFndLiquidacionPlanDB _db;

        public FrmFndLiquidacionPlanBL(IConfiguration? config)
        {
            _db = new FrmFndLiquidacionPlanDB(config);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> FND_LiquidacionPlan_Listas(int CodEmpresa, string usuario, string catalogo)
        {
            return _db.FND_LiquidacionPlan_Listas(CodEmpresa, usuario, catalogo);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> FND_LiquidacionPlan_buscar(int CodEmpresa, int codOperadora)
        {
            return _db.FND_LiquidacionPlan_buscar(CodEmpresa, codOperadora);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> FND_LiquidacionPlan_Operadora_Obtener(int CodEmpresa)
        {
            return _db.FND_LiquidacionPlan_Operadora_Obtener(CodEmpresa);
        }

        public ErrorDto<List<FndConsultaPlanRowDto>> FND_LiquidacionPlanContartos_Buscar(
           int CodEmpresa,
           FndLiquidacionPlanFiltrosData filtro)
        {
            return _db.FND_LiquidacionPlanContartos_Buscar(CodEmpresa, filtro);
        }

        //public ErrorDto<List<FndPlanDetalleData>> FND_LiquidacionPlan_ObtenerDetalle(int CodEmpresa, int codOperadora, string codPlan)
        //{
        //    return _db.FND_LiquidacionPlan_ObtenerDetalle(CodEmpresa, codOperadora, codPlan);
        //}

        //public ErrorDto<DropDownListaGenericaModel?> FND_LiquidacionPlan_ObtenerUno(int CodEmpresa, int codOperadora)
        //{
        //    return _db.FND_LiquidacionPlan_ObtenerUno(CodEmpresa, codOperadora);
        //}

        //public ErrorDto<List<DropDownListaGenericaModel>> FND_LiquidacionPlanCatalogo_Obtener(int CodEmpresa)
        //{
        //    return _db.FND_LiquidacionPlanCatalogo_Obtener(CodEmpresa);
        //}

        //public ErrorDto SbDocumentoMaestro_Guardar(int CodEmpresa, string usuario, FndLiquidacionPlanData data)
        //{
        //    return _db.SbDocumentoMaestro_Guardar(CodEmpresa, usuario, data);
        //}

        //public ErrorDto<List<FndConsultaPlanRow>> FND_ConsultaPlan(int CodEmpresa, FndConsultaPlanFiltro f)
        //{
        //    return _db.FND_ConsultaPlan(CodEmpresa, f);
        //}

        //public ErrorDto<string> FND_GrupoBancario_Obtener(int CodEmpresa, int bancoId)
        //{
        //    return _db.FND_GrupoBancario_Obtener(CodEmpresa, bancoId);
        //}

        //public ErrorDto<string> FND_CuentaPlan_Obtener(int CodEmpresa, string tipo, int codOperadora, string codPlan)
        //{
        //    return _db.FND_CuentaPlan_Obtener(CodEmpresa, tipo, codOperadora, codPlan);
        //}

        //public ErrorDto<string> FND_CuentaRetencion_Obtener(int CodEmpresa, string retencionCodigo)
        //{
        //    return _db.FND_CuentaRetencion_Obtener(CodEmpresa, retencionCodigo);
        //}

        //public ErrorDto<string> fxCuentaRetiros(int CodEmpresa, int codOperadora)
        //{
        //    return _db.fxCuentaRetiros(CodEmpresa, codOperadora);
        //}

        //public ErrorDto<bool> sbDocumento(int CodEmpresa, string vTipoDoc, string vDocRef, DateTime vFecha, string vConcepto, int codOperadora, string codPlan, string descripcionPlan)
        //{
        //    return _db.sbDocumento(CodEmpresa, vTipoDoc, vDocRef, vFecha, vConcepto, codOperadora, codPlan, descripcionPlan);
        //}

    }
}
