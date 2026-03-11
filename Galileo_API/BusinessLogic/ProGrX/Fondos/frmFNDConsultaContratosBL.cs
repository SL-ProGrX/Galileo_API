using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Credito;
using Galileo.Models.ProGrX.Fondos;
using Galileo_API.DataBaseTier.ProGrX.Fondos;
using Newtonsoft.Json;

namespace Galileo_API.BusinessLogic.ProGrX.Fondos
{
    public class FrmFndConsultaContratosBL
    {
        private readonly FrmFndConsultaContratosDB _Db;

        public FrmFndConsultaContratosBL(IConfiguration? config)
        {
            _Db = new FrmFndConsultaContratosDB(config);
        }

        public ErrorDto<List<CrConsultaCrdSociosData>> FND_ConsultaContratosSocios_Obtener(int CodEmpresa)
        {
           return _Db.FND_ConsultaContratosSocios_Obtener(CodEmpresa);
        }

        public ErrorDto<List<FndConsultaContratosData>> FND_ConsultaContratos_Contratos_Obtener(int CodEmpresa, string vCedula, string vUsuario, string opcion)
        {
           return _Db.FND_ConsultaContratos_Contratos_Obtener(CodEmpresa, vCedula, vUsuario, opcion);
        }

        public ErrorDto<List<FndConsultaSubContratosData>> FND_ConsultaContratos_SubCuentas_Obtener(int CodEmpresa, string vCedula, string cod_plan, string cod_contrato)
        {
            return _Db.FND_ConsultaContratos_SubCuentas_Obtener(CodEmpresa, vCedula, cod_plan, cod_contrato);
        }

        public ErrorDto<List<FndConsultaLiquidacionesData>> FND_ConsultaContratos_Liquidaciones_Obtener(int CodEmpresa, string vCedula)
        {
            return _Db.FND_ConsultaContratos_Liquidaciones_Obtener(CodEmpresa, vCedula);
        }

        public ErrorDto<List<FndConsultaMovimientosData>> FND_ConsultaContratos_Movimiento_Obtener(
            int CodEmpresa,
            string vCedula,
            string jFiltros)
        {
            FndConsultaMovimientosParams filtros = JsonConvert.DeserializeObject<FndConsultaMovimientosParams>(jFiltros)  ?? new FndConsultaMovimientosParams();
            return _Db.FND_ConsultaContratos_Movimiento_Obtener(CodEmpresa, vCedula, filtros);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> FND_ConsultaContratos_Planes_Obtener(int CodEmpresa, string vCedula)
        {
            return _Db.FND_ConsultaContratos_Planes_Obtener(CodEmpresa, vCedula);
        }

        public ErrorDto FND_ConsultaContratos_Reversar(int CodEmpresa, string usuario, string boleta)
        {
            return _Db.FND_ConsultaContratos_Reversar(CodEmpresa, usuario, boleta);
        }

    }
}
