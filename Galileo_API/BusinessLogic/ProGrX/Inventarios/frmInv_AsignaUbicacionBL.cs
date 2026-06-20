using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.INV;

namespace Galileo.BusinessLogic
{
    public class FrmInvAsignaUbicacionBL
    {
        private readonly FrmInvAsignaUbicacionDB _db;

        public FrmInvAsignaUbicacionBL(IConfiguration config)
        {
            _db = new FrmInvAsignaUbicacionDB(config);
        }

        public ErrorDto<AsignaUbicacionDto?> InvUbicaciones_Obtener(int CodEmpresa, int CodAsignaUbicacion)
        {
            return _db.InvUbicaciones_Obtener(CodEmpresa, CodAsignaUbicacion);
        }

        public ErrorDto<List<AsignaUbicacionDetalleDto>> InvUbicacionProduc_Obtener(int CodEmpresa, int CodAsignaUbicacion)
        {
            return _db.InvUbicacionProduc_Obtener(CodEmpresa, CodAsignaUbicacion);
        }

        public ErrorDto<AsignaUbicacionDto?> InvUbicacion_scroll(int CodEmpresa, int scrollValue, int? CodAsignaUbicacion)
        {
            return _db.InvUbicacion_scroll(CodEmpresa, scrollValue, CodAsignaUbicacion);
        }

        public ErrorDto InvAsignaUbicacion_Insertar(int CodEmpresa, AsignaUbicacionDto request)
        {
            return _db.InvAsignaUbicacion_Insertar(CodEmpresa, request);
        }

        public ErrorDto InvAsignaUbicacion_Actualizar(int CodEmpresa, AsignaUbicacionDto request)
        {
            return _db.InvAsignaUbicacion_Actualizar(CodEmpresa, request);
        }

        public ErrorDto InvAsignaUbicacion_Eliminar(int CodEmpresa, int CodAsignaUbicacion)
        {
            return _db.InvAsignaUbicacion_Eliminar(CodEmpresa, CodAsignaUbicacion);
        }

        public ErrorDto InvAsignaUbicacionProduc_Insertar(int CodEmpresa, int CodRequisicion, List<AsignaUbicacionDetalleDto> producLineas)
        {
            return _db.InvAsignaUbicacionProduc_Insertar(CodEmpresa, CodRequisicion, producLineas);
        }

        public ErrorDto<List<AsignaUbicacionDto>> InvAsignaUbicacion_Lista(int CodEmpresa)
        {
            return _db.InvAsignaUbicacion_Lista(CodEmpresa);
        }

        public ErrorDto InvAsignacionUbicacion_CerrarOrden_Finalizar(int CodEmpresa, int codigoAsignaUbicacion, string Usuario, string Estado)
        {
            return _db.InvAsignacionUbicacion_CerrarOrden_Finalizar(CodEmpresa, codigoAsignaUbicacion, Usuario, Estado);
        }

        public ErrorDto InvAsignaUbicacionProduc_Eliminar(int CodEmpresa, int CodAsignaUbicacion, int Linea)
        {
            return _db.InvAsignaUbicacionProduc_Eliminar(CodEmpresa, CodAsignaUbicacion, Linea);
        }
    }
}