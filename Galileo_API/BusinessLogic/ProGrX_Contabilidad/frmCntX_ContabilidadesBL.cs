using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX_Contabilidad;
using Galileo_API.Models.ProGrX_Contabilidad;

namespace Galileo_API.BusinessLogic.ProGrX_Contabilidad
{
    public class FrmCntXContabilidadesBl
    {
        private readonly FrmCntXContabilidadesDb _db;

        public FrmCntXContabilidadesBl(IConfiguration config) 
            => _db = new FrmCntXContabilidadesDb(config);

        public ErrorDto<CntXContabilidadData?> CntXContabilidad_Obtener(int codEmpresa, int codConta)
        {
            return _db.CntXContabilidad_Obtener(codEmpresa, codConta);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> CntXContabilidades_Lista_Obtener(int codEmpresa)
        {
            return _db.CntXContabilidades_Lista_Obtener(codEmpresa);
        }

        public ErrorDto<CntXContabilidadData?> CntXContabilidad_Scroll_Obtener(int codEmpresa, int scrollCode, int codConta)
        {
            return _db.CntXContabilidad_Scroll_Obtener(codEmpresa, scrollCode, codConta);
        }

        public ErrorDto<List<DropDownConsolidaListaData>> CntXContabilidad_ConsolidaBaseList_Obtener(int codEmpresa, int codConta)
        {
            return _db.CntXContabilidad_ConsolidaBaseList_Obtener(codEmpresa, codConta);
        }

        public ErrorDto<List<DropDownConsolidaListaData>> CntXContabilidad_ConsolidaUnidadesList_Obtener(int codEmpresa, int codConta)
        {
            return _db.CntXContabilidad_ConsolidaUnidadesList_Obtener(codEmpresa, codConta);
        }

        public ErrorDto CntXContabilidades_Guardar(int codEmpresa, string usuario, bool edita, CntXContabilidadData request)
        {
            return _db.CntXContabilidades_Guardar(codEmpresa, usuario, edita, request);
        }

        public ErrorDto CntXContabilidades_Eliminar(int codEmpresa, int codConta, string usuario)
        {
            return _db.CntXContabilidades_Eliminar(codEmpresa, codConta, usuario);
        }
    }
}
