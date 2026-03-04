using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX_Contabilidad;
using Galileo_API.Models.ProGrX_Contabilidad;

namespace Galileo_API.BusinessLogic.ProGrX_Contabilidad
{
    public class FrmCntXDiferidosPlantillaBl
    {
        private readonly FrmCntXDiferidosPlantillaDb _db;

        public FrmCntXDiferidosPlantillaBl(IConfiguration config) 
            => _db = new FrmCntXDiferidosPlantillaDb(config);

        public ErrorDto<CntXDiferidosData?> CntXDiferidosPlantilla_Obtener(int codEmpresa, int codConta, int codDiferido)
        {
            return _db.CntXDiferidosPlantilla_Obtener(codEmpresa, codConta, codDiferido);
        }

        public ErrorDto<CntXDiferidosData?> CntXDiferidosPlantilla_Scroll_Obtener(int CodEmpresa, int codConta, int scrollCode, int codDiferido)
        {
            return _db.CntXDiferidosPlantilla_Scroll_Obtener(CodEmpresa, codConta, scrollCode, codDiferido);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> CntXDiferidosPlantilla_Lista_Obtener(int codEmpresa, int codConta)
        {
            return _db.CntXDiferidosPlantilla_Lista_Obtener(codEmpresa, codConta);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> CntXDiferidosPlantilla_TiposAsientos_Obtener(int codEmpresa, int codConta)
        {
            return _db.CntXDiferidosPlantilla_TiposAsientos_Obtener(codEmpresa, codConta);
        }

        public ErrorDto<string?> CntXDiferidosPlantilla_TipoAsientoDesc_Obtener(int codEmpresa, int codConta, string tipoAsiento)
        {
            return _db.CntXDiferidosPlantilla_TipoAsientoDesc_Obtener(codEmpresa, codConta, tipoAsiento);
        }

        public ErrorDto<List<CntXDiferidosDetalleData>> CntXDiferidosPlantilla_Detalle_Obtener(int codEmpresa, int codConta, int codDiferido)
        {
            return _db.CntXDiferidosPlantilla_Detalle_Obtener(codEmpresa, codConta, codDiferido);
        }

        public ErrorDto CntXDiferidosPlantilla_Guardar(int codEmpresa, CntXDiferidosPlantillaRequest request)
        {
            return _db.CntXDiferidosPlantilla_Guardar(codEmpresa, request);
        }

        public ErrorDto CntXDiferidosPlantilla_Eliminar(int codEmpresa, int codConta, string usuario, int codDiferido)
        {
            return _db.CntXDiferidosPlantilla_Eliminar(codEmpresa, codConta, usuario, codDiferido);
        }
    }
}
