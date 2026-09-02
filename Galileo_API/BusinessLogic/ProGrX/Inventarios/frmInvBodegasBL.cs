using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.INV;

namespace Galileo.BusinessLogic
{
    public class FrmInvBodegasBl
    {
        private readonly FrmInvBodegasDb _db;

        public FrmInvBodegasBl(IConfiguration config)
        {
            _db = new FrmInvBodegasDb(config);
        }

        public ErrorDto<List<BodegasDto>> INV_Bodegas_Lista_Obtener(
            int CodEmpresa)
        {
            return _db.INV_Bodegas_Lista_Obtener(
                CodEmpresa);
        }

        public ErrorDto<BodegasDto> INV_Bodegas_Codigo_Obtener(
            int CodEmpresa,
            string cod_bodega)
        {
            return _db.INV_Bodegas_Codigo_Obtener(
                CodEmpresa,
                cod_bodega);
        }

        public ErrorDto<BodegasDto> INV_Bodegas_Navegacion_Obtener(
            int CodEmpresa,
            string consecutivo,
            string tipo)
        {
            return _db.INV_Bodegas_Navegacion_Obtener(
                CodEmpresa,
                consecutivo,
                tipo);
        }

        public ErrorDto<List<PermisosBodegasDto>> INV_Bodegas_Permisos_Obtener(
            int CodEmpresa,
            string cod_bodega,
            string tipo_transaccion)
        {
            return _db.INV_Bodegas_Permisos_Obtener(
                CodEmpresa,
                cod_bodega,
                tipo_transaccion);
        }

        public ErrorDto INV_Bodegas_Registrar(
            int CodEmpresa,
            BodegasDto request)
        {
            return _db.INV_Bodegas_Registrar(
                CodEmpresa,
                request);
        }

        public ErrorDto INV_Bodegas_Actualizar(
            int CodEmpresa,
            BodegasDto request)
        {
            return _db.INV_Bodegas_Actualizar(
                CodEmpresa,
                request);
        }

        public ErrorDto INV_Bodegas_Eliminar(
            int CodEmpresa,
            string cod_bodega)
        {
            return _db.INV_Bodegas_Eliminar(
                CodEmpresa,
                cod_bodega);
        }

        public ErrorDto INV_Bodegas_Permiso_Actualizar(
            int CodEmpresa,
            InvBodegasPermisoActualizarRequest request)
        {
            return _db.INV_Bodegas_Permiso_Actualizar(
                CodEmpresa,
                request);
        }
    }
}