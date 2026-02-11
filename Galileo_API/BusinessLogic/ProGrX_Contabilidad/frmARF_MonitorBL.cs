using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX_Contabilidad;
using Galileo_API.Models.ProGrX_Activos;
using Galileo_API.Models.ProGrX_Contabilidad;

namespace Galileo_API.BusinessLogic.ProGrX_Contabilidad
{
    public class FrmArfMonitorBl
    {
        private readonly FrmArfMonitorDb _db;

        public FrmArfMonitorBl(IConfiguration config)
        {
            _db = new FrmArfMonitorDb(config);
        }

        public ErrorDto<List<ARFMonitorTablaDto>> Buscar(int codEmpresa,ARFMonitorFiltroDto filtros)
        {
            return _db.Buscar(codEmpresa, filtros);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> Unidades_Buscar(int codEmpresa)
        {
            return _db.Unidades_Buscar(codEmpresa);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> Arrendadores_Buscar(int codEmpresa)
        {
            return _db.Arrendadores_Buscar(codEmpresa);
        }


    }
}
