using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX_Contabilidad;
using Galileo_API.Models.ProGrX_Contabilidad;

namespace Galileo_API.BusinessLogic.ProGrX_Contabilidad
{
    public class FrmCntXRepGeneralResultadosBL
    {
        private readonly FrmCntXRepGeneralResultadosDB Db;

        public FrmCntXRepGeneralResultadosBL(IConfiguration config)
        {
            Db = new FrmCntXRepGeneralResultadosDB(config);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> CntX_Unidades_Dropdown_Obtener(int CodEmpresa, int codContabilidad)
        {
            return Db.CntX_Unidades_Dropdown_Obtener(CodEmpresa, codContabilidad);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> CntX_CentroCosto_Dropdown_Obtener(int CodEmpresa, int codContabilidad, string? codUnidad)
        {
            return Db.CntX_CentroCosto_Dropdown_Obtener(CodEmpresa, codContabilidad, codUnidad);
        }

        public ErrorDto<CntXRepGeneralResultadosValidarResponseDto> CntX_RepGeneralResultados_ValidarReporte(
            int CodEmpresa,
            CntXRepGeneralResultadosValidarRequestDto data)
        {
            return Db.CntX_RepGeneralResultados_ValidarReporte(CodEmpresa, data);
        }
    }
}