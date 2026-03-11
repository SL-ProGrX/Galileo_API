using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX_Contabilidad;
using Galileo_API.Models.ProGrX_Contabilidad;

namespace Galileo_API.BusinessLogic.ProGrX_Contabilidad
{
    public class FrmCntXDiferidosCreacionBl
    {
        private readonly FrmCntXDiferidosCreacionDb _db;

        public FrmCntXDiferidosCreacionBl(IConfiguration config)
            => _db = new FrmCntXDiferidosCreacionDb(config);

        public ErrorDto<CntXDiferidoCreacionData?> CntXDiferidosCreacion_Obtener(
            int codEmpresa, int codConta, int codDifPlantilla, int codDiferido)
        {
            return _db.CntXDiferidosCreacion_Obtener(codEmpresa, codConta, codDifPlantilla, codDiferido);   
        }

        public ErrorDto<CntXDiferidoCreacionData?> CntXDiferidosCreacion_Scroll_Obtener(
            int codEmpresa, int codConta, int scroll, int codDiferido, int codDifPlantillaActual)
        {
            return _db.CntXDiferidosCreacion_Scroll_Obtener(codEmpresa, codConta, scroll, codDiferido, codDifPlantillaActual);
        }

        public ErrorDto<List<CntXDiferidoHistoricoData>> CntXDiferidosCreacion_Historico_Obtener(
            int codEmpresa, int codConta, int codDifPlantilla, int codDiferido)
        {
            return _db.CntXDiferidosCreacion_Historico_Obtener(codEmpresa, codConta, codDifPlantilla, codDiferido);
        }

        public ErrorDto<List<CntXDiferidosPlantillaData>> CntXDiferidosCreacion_PlantillaLista_Obtener(int codEmpresa, int codConta)
        {
            return _db.CntXDiferidosCreacion_PlantillaLista_Obtener(codEmpresa, codConta);
        }

        public ErrorDto<List<CntXDiferidoCreacionData>> CntXDiferidosCreacion_Lista_Obtener(int codEmpresa, int codConta)
        {
            return _db.CntXDiferidosCreacion_Lista_Obtener(codEmpresa, codConta);
        }

        public ErrorDto CntXDiferidosCreacion_Guardar(int codEmpresa, CntXDiferidosCreacionRequest request)
        {
            return _db.CntXDiferidosCreacion_Guardar(codEmpresa, request);
        }

        public ErrorDto CntXDiferidosCreacion_Eliminar(
            int codEmpresa, int codConta, string usuario, int codDifPlantilla, int codDiferido)
        {
            return _db.CntXDiferidosCreacion_Eliminar(codEmpresa, codConta, usuario, codDifPlantilla, codDiferido);
        }
    }
}
