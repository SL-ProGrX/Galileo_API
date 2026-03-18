using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX_Contabilidad;
using Galileo_API.Models.ProGrX_Contabilidad;

namespace Galileo_API.BusinessLogic.ProGrX_Contabilidad
{
    public class FrmCntxRepEspecialesBl
    {
        private readonly FrmCntXRepEspecialesDb _db;

        public FrmCntxRepEspecialesBl(IConfiguration config)
        {
            _db = new FrmCntXRepEspecialesDb(config);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> CntX_Periodos_Listar(int codEmpresa, int codContabilidad)
        {
            return _db.CntX_Periodos_Listar(codEmpresa, codContabilidad);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> CntX_Unidades_Listar(int codEmpresa, int codContabilidad)
        {
            return _db.CntX_Unidades_Listar(codEmpresa, codContabilidad);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> CntX_CentroCostos_Listar(int codEmpresa, int codContabilidad, string unidad)
        {
            return _db.CntX_CentroCostos_Listar(codEmpresa, codContabilidad, unidad);
        }
        public ErrorDto<bool> GenerarReporte( int codEmpresa,int codContabilidad,CntxRepEspecialFiltroDto filtros)
        {
            return _db.GenerarReporte(codEmpresa,codContabilidad,filtros);
        }
    }
}