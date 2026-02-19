using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX_Contabilidad;
using Galileo_API.Models.ProGrX_Contabilidad;

namespace Galileo_API.BusinessLogic.ProGrX_Contabilidad
{
    public class FrmCntXPlantillaRateBl
    {
        private readonly FrmCntXPlantillaRateDb _db;

        public FrmCntXPlantillaRateBl(IConfiguration config)
        {
            _db = new FrmCntXPlantillaRateDb(config);
        }

        public ErrorDto<CntxPlantillaRateDto> CntxPlantillaRate_Scroll_Obtener(int codEmpresa, int scrollCode, int? codPlantilla)
        {
            return _db.CntxPlantillaRate_Scroll_Obtener(codEmpresa, scrollCode, codPlantilla);
        }

        public ErrorDto<CntxPlantillaRateDto> CntxPlantillaRate_Consulta_Obtener(int codEmpresa, int codPlantilla)
        {
            return _db.CntxPlantillaRate_Consulta_Obtener(codEmpresa, codPlantilla);
        }

        public ErrorDto CntxPlantillaRate_Guardar(int codEmpresa, bool existe, CntxPlantillaRateDto request)
        {
            return _db.CntxPlantillaRate_Guardar(codEmpresa, existe, request);
        }

        public ErrorDto CntxPlantillaRate_Eliminar(int codEmpresa, string usuario, int codPlantilla)
        {
            return _db.CntxPlantillaRate_Eliminar(codEmpresa, usuario, codPlantilla);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> TiposAsiento_Obtener(int codEmpresa)
        {
            return _db.TiposAsiento_Obtener(codEmpresa);
        }

        public ErrorDto<List<DropDownListaGenericaModel>>Plantillas_Buscar(int codEmpresa)
        {
            return _db.Plantillas_Buscar(codEmpresa);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> Unidades_Obtener(int codEmpresa)
        {
            return _db.Unidades_Obtener(codEmpresa);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> Divisas_Obtener(int codEmpresa)
        {
            return _db.Divisas_Obtener(codEmpresa);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> CentroCosto_Obtener(int codEmpresa,string codUnidad)
        {
            return _db.CentroCosto_Obtener(codEmpresa, codUnidad);
        }

    }
}
