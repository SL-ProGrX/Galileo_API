using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.Creditos;
using Galileo_API.Models.ProGrX.Creditos;

namespace Galileo_API.BusinessLogic.ProGrX.Creditos
{
    public class FrmCrSolicitudesFiadoresBL
    {
        private readonly FrmCrSolicitudesFiadoresDB _db;

        public FrmCrSolicitudesFiadoresBL(IConfiguration config)
        {
            _db = new FrmCrSolicitudesFiadoresDB(config);
        }

        public ErrorDto<List<CrSolicitudesFiadoresInstitucionDto>> CR_SolicitudesFiadores_Instituciones_Obtener(int CodEmpresa)
        {
            return _db.CR_SolicitudesFiadores_Instituciones_Obtener(CodEmpresa);
        }

        public ErrorDto<TablasListaGenericaModel> CR_SolicitudesFiadores_Lista_Obtener(int CodEmpresa, string parametros)
        {
            return _db.CR_SolicitudesFiadores_Lista_Obtener(CodEmpresa, parametros);
        }

        public ErrorDto<TablasListaGenericaModel> CR_SolicitudesFiadores_Lista_Export(int CodEmpresa, string parametros)
        {
            return _db.CR_SolicitudesFiadores_Lista_Export(CodEmpresa, parametros);
        }

        public ErrorDto<CrSolicitudesFiadoresDetalleDto> CR_SolicitudesFiadores_Detalle_Obtener(int CodEmpresa, long fiaConsec)
        {
            return _db.CR_SolicitudesFiadores_Detalle_Obtener(CodEmpresa, fiaConsec);
        }

        public ErrorDto<CrSolicitudesFiadoresSocioDto> CR_SolicitudesFiadores_Socio_Obtener(int CodEmpresa, string cedula)
        {
            return _db.CR_SolicitudesFiadores_Socio_Obtener(CodEmpresa, cedula);
        }

        public ErrorDto CR_SolicitudesFiadores_Guardar(int CodEmpresa, CrSolicitudesFiadoresGuardarRequest request)
        {
            return _db.CR_SolicitudesFiadores_Guardar(CodEmpresa, request);
        }

        public ErrorDto CR_SolicitudesFiadores_Eliminar(int CodEmpresa, CrSolicitudesFiadoresEliminarRequest request)
        {
            return _db.CR_SolicitudesFiadores_Eliminar(CodEmpresa, request);
        }
    }
}