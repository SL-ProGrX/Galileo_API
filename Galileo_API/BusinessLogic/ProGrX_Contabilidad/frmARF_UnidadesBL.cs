using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX_Contabilidad;
using Galileo_API.Models.ProGrX_Contabilidad;

namespace Galileo_API.BusinessLogic.ProGrX_Contabilidad
{
    public class FrmArfUnidadesBl
    {
        private readonly FrmArfUnidadesDb _db;

        public FrmArfUnidadesBl(IConfiguration config) => _db = new FrmArfUnidadesDb(config);

        public ErrorDto<List<DropDownListaGenericaModel>> ArfUnidades_Provincias_Obtener(int codEmpresa)
        {
            return _db.ArfUnidades_Provincias_Obtener(codEmpresa);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> ArfUnidades_Cantones_Obtener(int codEmpresa, string codProvincia)
        {
            return _db.ArfUnidades_Cantones_Obtener(codEmpresa, codProvincia);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> ArfUnidades_Distritos_Obtener(int codEmpresa, string codProvincia, string codCanton)
        {
            return _db.ArfUnidades_Distritos_Obtener(codEmpresa, codProvincia, codCanton);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> ArfUnidades_Unidades_Obtener(int codEmpresa)
        {
            return _db.ArfUnidades_Unidades_Obtener(codEmpresa);
        }

        public ErrorDto<ArfUnidadesData> ArfUnidades_Scroll_Obtener(int codEmpresa, string codUnidad, int scrollCode)
        {
            return _db.ArfUnidades_Scroll_Obtener(codEmpresa, codUnidad, scrollCode);
        }

        public ErrorDto<ArfUnidadesData> ArfUnidades_ConsultaUnidad_Obtener(int codEmpresa, string codUnidad)
        {
            return _db.ArfUnidades_ConsultaUnidad_Obtener(codEmpresa, codUnidad);
        }
    }
}
