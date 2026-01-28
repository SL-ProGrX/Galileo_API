using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX_Contabilidad;
using Galileo_API.Models.ProGrX_Contabilidad;

namespace Galileo_API.BusinessLogic.ProGrX_Contabilidad
{
    public class FrmCntXCentrosCostosBl
    {
        private readonly FrmCntXCentrosCostosDb _db;

        public FrmCntXCentrosCostosBl(IConfiguration config) => _db = new FrmCntXCentrosCostosDb(config);

        public ErrorDto<List<CntXCentroCostosData>> CntXCentrosCostos_Obtener(int codEmpresa, int codConta, bool activo)
        {
            return _db.CntXCentrosCostos_Obtener(codEmpresa, codConta, activo);
        }

        public ErrorDto CntXCentrosCostos_Guardar(int codEmpresa, int codConta, string usuario, CntXCentroCostosData request)
        {
            return _db.CntXCentrosCostos_Guardar(codEmpresa, codConta, usuario, request);
        }

        public ErrorDto CntXCentrosCostos_Eliminar(int codEmpresa, int codConta, string usuario, string codCentroCosto)
        {
            return _db.CntXCentrosCostos_Eliminar(codEmpresa, codConta, usuario, codCentroCosto);
        }

        public ErrorDto<List<CntXCentroCostosUnidadesDto>> CntXCentrosCostos_Unidades_Obtener(int codEmpresa, int codConta, string codCentroCosto)
        {
            return _db.CntXCentrosCostos_Unidades_Obtener(codEmpresa, codConta, codCentroCosto);
        }

        public ErrorDto CntXCentrosCostos_Unidades_Asignar(int codEmpresa, int codConta, string codCentroCosto, string codUnidad, bool itemChecked)
        {
            return _db.CntXCentrosCostos_Unidades_Asignar(codEmpresa, codConta, codCentroCosto, codUnidad, itemChecked);
        }
    }
}
