using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX_Contabilidad;
using Galileo_API.Models.ProGrX_Contabilidad;

namespace Galileo_API.BusinessLogic.ProGrX_Contabilidad
{
    public class FrmCntXPlantillaRateGenBl
    {
        private readonly FrmCntXPlantillaRateGenDb _db;

        public FrmCntXPlantillaRateGenBl(IConfiguration config) =>
            _db = new FrmCntXPlantillaRateGenDb(config);

        public ErrorDto<List<DropDownListaGenericaModel>> CntXPlantillaRate_Lista_Obtener(int codEmpresa, int codConta)
        {
            return _db.CntXPlantillaRate_Lista_Obtener(codEmpresa, codConta);
        }

        public ErrorDto<List<CntXPlantillaRateDetalleData>> CntXPlantillaRate_Detalle_Obtener(int codEmpresa, int codConta, int codPlantilla)
        {
            return _db.CntXPlantillaRate_Detalle_Obtener(codEmpresa, codConta, codPlantilla);
        }

        public ErrorDto CntXPlantillaRate_Generar(int codEmpresa, CntXPlantillaRateGenerarRequest request)
        {
            return _db.CntXPlantillaRate_Generar(codEmpresa, request);
        }
    }
}
