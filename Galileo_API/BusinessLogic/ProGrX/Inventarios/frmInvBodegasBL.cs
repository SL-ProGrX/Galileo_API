using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.INV;

namespace Galileo.BusinessLogic
{
    public class FrmInvBodegasBL
    {
        private readonly FrmInvBodegasDB _db;

        public FrmInvBodegasBL(IConfiguration config)
        {
            _db = new FrmInvBodegasDB(config);
        }

        public ErrorDto<List<PermisosBodegasDto>> Autorizador_ObtenerTodos(int CodEmpresa, string CodBodega)
        {
            return _db.Autorizador_ObtenerTodos(CodEmpresa, CodBodega);
        }

        public ErrorDto<List<BodegasDto>> Bodegas_Obtener(int CodEmpresa)
        {
            return _db.Bodegas_Obtener(CodEmpresa);
        }

        public ErrorDto<BodegasDto> ConsultaAscDesc(int CodEmpresa, int consecutivo, string tipo)
        {
            return _db.ConsultaAscDesc(CodEmpresa, consecutivo, tipo);
        }

        public ErrorDto<BodegasDto> bodegaConsecutivo_Obtener(int CodEmpresa, string consecutivo)
        {
            return _db.bodegaConsecutivo_Obtener(CodEmpresa, consecutivo);
        }

        public ErrorDto bodega_Insertar(int CodEmpresa, BodegasDto data)
        {
            return _db.bodega_Insertar(CodEmpresa, data);
        }

        public ErrorDto bodega_Actualizar(int CodEmpresa, BodegasDto data)
        {
            return _db.bodega_Actualizar(CodEmpresa, data);
        }

        public ErrorDto bodega_Eliminar(int CodEmpresa, string cod_bodega)
        {
            return _db.bodega_Eliminar(CodEmpresa, cod_bodega);
        }

        public ErrorDto permisosBodega_Actualizar(int CodEmpresa, PermisosBodegasDto request, string cod_bodega)
        {
            return _db.permisosBodega_Actualizar(CodEmpresa, request, cod_bodega);
        }
    }
}