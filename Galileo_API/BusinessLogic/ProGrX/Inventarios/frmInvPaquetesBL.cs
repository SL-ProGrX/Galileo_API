using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.INV;

namespace Galileo.BusinessLogic
{
    public class FrmInvPaquetesBL
    {
        private readonly FrmInvPaquetesDB _db;

        public FrmInvPaquetesBL(IConfiguration config)
        {
            _db = new FrmInvPaquetesDB(config);
        }

        public ErrorDto<PaqueteDataLista> Paquetes_Obtener(int CodCliente, int? pagina, int? paginacion, string? filtro)
        {
            return _db.Paquetes_Obtener(CodCliente, pagina, paginacion, filtro);
        }

        public ErrorDto<List<PaqueteDto>> Paquetes_ObtenerTodos(int CodEmpresa)
        {
            return _db.Paquetes_ObtenerTodos(CodEmpresa);
        }

        public ErrorDto<PaqueteDto> Paquete_Obtener(int CodEmpresa, int Cod_Paquete)
        {
            return _db.Paquete_Obtener(CodEmpresa, Cod_Paquete);
        }

        public ErrorDto<List<PaqueteDetalleDto>> Paquete_ObtenerDetalles(int CodEmpresa, int Cod_Paquete)
        {
            return _db.Paquete_ObtenerDetalles(CodEmpresa, Cod_Paquete);
        }

        public ErrorDto Paquete_Insertar(int CodEmpresa, PaqueteDto request)
        {
            return _db.Paquete_Insertar(CodEmpresa, request);
        }

        public ErrorDto Paquete_Actualizar(int CodEmpresa, PaqueteDto request)
        {
            return _db.Paquete_Actualizar(CodEmpresa, request);
        }

        public ErrorDto PaqueteDetalle_Insertar(int CodEmpresa, PaqueteDetalleDto request)
        {
            return _db.PaqueteDetalle_Insertar(CodEmpresa, request);
        }

        public ErrorDto PaqueteDetalle_Actualizar(int CodEmpresa, PaqueteDetalleDto request)
        {
            return _db.PaqueteDetalle_Actualizar(CodEmpresa, request);
        }

        public ErrorDto PaqueteDetalle_Eliminar(int CodEmpresa, PaqueteDetalleDto request)
        {
            return _db.PaqueteDetalle_Eliminar(CodEmpresa, request);
        }
    }
}