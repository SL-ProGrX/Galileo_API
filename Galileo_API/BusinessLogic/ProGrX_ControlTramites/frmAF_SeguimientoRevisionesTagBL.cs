using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.ControlTramites;
using Galileo_API.DataBaseTier.ProGrX.ControlTramites;

namespace Galileo_API.BusinessLogic.ProGrX.ControlTramites
{
    public class FrmAfSeguimientoRevisionesTagBL
    {
        private readonly FrmAfSeguimientoRevisionesTagDB _db;

        public FrmAfSeguimientoRevisionesTagBL(IConfiguration config)
        {
            _db = new FrmAfSeguimientoRevisionesTagDB(config);
        }

        public ErrorDto<AfSeguimientoRevisionesTagAfiliacionesListaResult> AF_SeguimientoRevisionesTag_Afiliaciones_Lista_Obtener(int CodEmpresa, string parametros)
        {
            return _db.AF_SeguimientoRevisionesTag_Afiliaciones_Lista_Obtener(CodEmpresa, parametros);
        }

        public ErrorDto<AfSeguimientoRevisionesTagAfiliacionesListaResult> AF_SeguimientoRevisionesTag_Afiliaciones_Lista_Export(int CodEmpresa, string parametros)
        {
            return _db.AF_SeguimientoRevisionesTag_Afiliaciones_Lista_Export(CodEmpresa, parametros);
        }

        public ErrorDto<AfSeguimientoRevisionesTagDetalleData> AF_SeguimientoRevisionesTag_Detalle_Obtener(int CodEmpresa, string? cedula, long? consecutivo)
        {
            return _db.AF_SeguimientoRevisionesTag_Detalle_Obtener(CodEmpresa, cedula, consecutivo);
        }

        public ErrorDto<List<AfSeguimientoRevisionesTagSeguimientoData>> AF_SeguimientoRevisionesTag_Seguimiento_Lista_Obtener(int CodEmpresa, string? cedula, long consecutivo)
        {
            return _db.AF_SeguimientoRevisionesTag_Seguimiento_Lista_Obtener(CodEmpresa, cedula, consecutivo);
        }

        public ErrorDto<List<AfSeguimientoRevisionesTagSeguimientoData>> AF_SeguimientoRevisionesTag_Seguimiento_Lista_Export(int CodEmpresa, string? cedula, long consecutivo)
        {
            return _db.AF_SeguimientoRevisionesTag_Seguimiento_Lista_Export(CodEmpresa, cedula, consecutivo);
        }

        public ErrorDto<List<AfSeguimientoRevisionesTagEtiquetaData>> AF_SeguimientoRevisionesTag_Etiquetas_Dropdown_Obtener(int CodEmpresa, string? usuario)
        {
            return _db.AF_SeguimientoRevisionesTag_Etiquetas_Dropdown_Obtener(CodEmpresa, usuario);
        }

        public ErrorDto<List<AfSeguimientoRevisionesTagRevisionData>> AF_SeguimientoRevisionesTag_Revisiones_Lista_Obtener(int CodEmpresa, string? cedula, long consecutivo)
        {
            return _db.AF_SeguimientoRevisionesTag_Revisiones_Lista_Obtener(CodEmpresa, cedula, consecutivo);
        }

        public ErrorDto<List<AfSeguimientoRevisionesTagRevisionData>> AF_SeguimientoRevisionesTag_Revisiones_Lista_Export(int CodEmpresa, string? cedula, long consecutivo)
        {
            return _db.AF_SeguimientoRevisionesTag_Revisiones_Lista_Export(CodEmpresa, cedula, consecutivo);
        }

        public ErrorDto AF_SeguimientoRevisionesTag_Aplicar(int CodEmpresa, string? usuario, AfSeguimientoRevisionesTagAplicarRequest? request)
        {
            return _db.AF_SeguimientoRevisionesTag_Aplicar(CodEmpresa, usuario, request);
        }
        public ErrorDto<long?> AF_SeguimientoRevisionesTag_Seleccion_Actualizar(int CodEmpresa,string? usuario, AfSeguimientoRevisionesTagSeleccionRequest? request)
        {
            return _db.AF_SeguimientoRevisionesTag_Seleccion_Actualizar(CodEmpresa,usuario,request);
        }
    }
}