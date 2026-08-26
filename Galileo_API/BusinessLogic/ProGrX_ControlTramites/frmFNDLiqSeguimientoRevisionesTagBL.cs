using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX_ControlTramites;

namespace Galileo_API.BusinessLogic.ProGrX_ControlTramites
{
    public class FrmFndLiqSeguimientoRevisionesTagBl
    {
        private readonly FrmFndLiqSeguimientoRevisionesTagDb _db;

        public FrmFndLiqSeguimientoRevisionesTagBl(IConfiguration config)
        {
            _db = new FrmFndLiqSeguimientoRevisionesTagDb(config);
        }

        public ErrorDto<FndLiqSeguimientoRevisionesTagLiquidacionesListaResult> FND_LiqSeguimientoRevisionesTag_Liquidaciones_Lista_Obtener(int CodEmpresa, string parametros, bool soloSinRetencion)
        {
            return _db.FND_LiqSeguimientoRevisionesTag_Liquidaciones_Lista_Obtener(CodEmpresa, parametros, soloSinRetencion);
        }

        public ErrorDto<FndLiqSeguimientoRevisionesTagLiquidacionesListaResult> FND_LiqSeguimientoRevisionesTag_Liquidaciones_Lista_Export(int CodEmpresa, string parametros, bool soloSinRetencion)
        {
            return _db.FND_LiqSeguimientoRevisionesTag_Liquidaciones_Lista_Export(CodEmpresa, parametros, soloSinRetencion);
        }

        public ErrorDto<FndLiqSeguimientoRevisionesTagLiquidacionData> FND_LiqSeguimientoRevisionesTag_Nombre_Obtener(int CodEmpresa, string? cedula)
        {
            return _db.FND_LiqSeguimientoRevisionesTag_Nombre_Obtener(CodEmpresa, cedula);
        }

        public ErrorDto<List<FndLiqSeguimientoRevisionesTagSeguimientoData>> FND_LiqSeguimientoRevisionesTag_Seguimiento_Lista_Obtener(int CodEmpresa, long consecutivo)
        {
            return _db.FND_LiqSeguimientoRevisionesTag_Seguimiento_Lista_Obtener(CodEmpresa, consecutivo);
        }

        public ErrorDto<List<FndLiqSeguimientoRevisionesTagSeguimientoData>> FND_LiqSeguimientoRevisionesTag_Seguimiento_Lista_Export(int CodEmpresa, long consecutivo)
        {
            return _db.FND_LiqSeguimientoRevisionesTag_Seguimiento_Lista_Export(CodEmpresa, consecutivo);
        }

        public ErrorDto<List<FndLiqSeguimientoRevisionesTagEtiquetaData>> FND_LiqSeguimientoRevisionesTag_Etiquetas_Dropdown_Obtener(int CodEmpresa, string? usuario)
        {
            return _db.FND_LiqSeguimientoRevisionesTag_Etiquetas_Dropdown_Obtener(CodEmpresa, usuario);
        }

        public ErrorDto<List<FndLiqSeguimientoRevisionesTagRevisionData>> FND_LiqSeguimientoRevisionesTag_Revisiones_Lista_Obtener(int CodEmpresa, string? cedula, long consecutivo)
        {
            return _db.FND_LiqSeguimientoRevisionesTag_Revisiones_Lista_Obtener(CodEmpresa, cedula, consecutivo);
        }

        public ErrorDto<List<FndLiqSeguimientoRevisionesTagRevisionData>> FND_LiqSeguimientoRevisionesTag_Revisiones_Lista_Export(int CodEmpresa, string? cedula, long consecutivo)
        {
            return _db.FND_LiqSeguimientoRevisionesTag_Revisiones_Lista_Export(CodEmpresa, cedula, consecutivo);
        }

        public ErrorDto<long?> FND_LiqSeguimientoRevisionesTag_Seleccion_Actualizar(int CodEmpresa, string? usuario, FndLiqSeguimientoRevisionesTagSeleccionRequest? request)
        {
            return _db.FND_LiqSeguimientoRevisionesTag_Seleccion_Actualizar(CodEmpresa, usuario, request);
        }

        public ErrorDto FND_LiqSeguimientoRevisionesTag_Aplicar(int CodEmpresa, string? usuario, FndLiqSeguimientoRevisionesTagAplicarRequest? request)
        {
            return _db.FND_LiqSeguimientoRevisionesTag_Aplicar(CodEmpresa, usuario, request);
        }
    }
}