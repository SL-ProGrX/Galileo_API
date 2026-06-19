using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.INV;

namespace Galileo.BusinessLogic
{
    public class FrmInvUnidadesBL
    {
        private readonly FrmInvUnidadesDB _db;

        public FrmInvUnidadesBL(IConfiguration config)
        {
            _db = new FrmInvUnidadesDB(config);
        }

        public ErrorDto<UnidadesDataLista> UnidadMedicion_Obtener(int CodCliente, int? pagina, int? paginacion, string? filtro)
        {
            return _db.UnidadMedicion_Obtener(CodCliente, pagina, paginacion, filtro);
        }

        public ErrorDto<List<UnidadMedicionDto>> UnidadMedicion_ObtenerTodosDetalle(int CodEmpresa)
        {
            return _db.UnidadMedicion_ObtenerTodosDetalle(CodEmpresa);
        }

        public ErrorDto<List<UnidadMedicion>> UnidadMedicion_ObtenerTodos(int CodEmpresa)
        {
            return _db.UnidadMedicion_ObtenerTodos(CodEmpresa);
        }

        public ErrorDto UnidadMedicion_Insertar(int CodEmpresa, UnidadMedicionDto request)
        {
            return _db.UnidadMedicion_Agregar(CodEmpresa, request);
        }

        public ErrorDto UnidadMedicion_Actualizar(int CodEmpresa, UnidadMedicionDto request)
        {
            return _db.UnidadMedicion_Actualizar(CodEmpresa, request);
        }

        public ErrorDto UnidadMedicion_Eliminar(int CodEmpresa, string unidad)
        {
            return _db.UnidadMedicion_Eliminar(CodEmpresa, unidad);
        }
    }
}
