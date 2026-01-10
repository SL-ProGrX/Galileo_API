using Galileo.DataBaseTier;
using Galileo.Models.CPR;
using Galileo.Models.ERROR;


namespace Galileo.BusinessLogic
{
    public class FrmCprOrdNivelAutoBL
    {
        private readonly FrmCprOrdNivelAutoDB _db;
        public FrmCprOrdNivelAutoBL(IConfiguration config)
        {
            _db = new FrmCprOrdNivelAutoDB(config);
        }

        public ErrorDto<UsuariosAuthorizaLista> UsuariosAutorizadores_Obtener(int CodEmpresa, string jFiltros)
        {
            return _db.UsuariosAutorizadores_Obtener(CodEmpresa, jFiltros);
        }

        public ErrorDto OrdenAutousers_Insertar(int CodEmpresa, string usuario, string usuario_asignado)
        {
            return _db.OrdenAutousers_Insertar(CodEmpresa, usuario, usuario_asignado);
        }

        public ErrorDto OrdenAutousers_Eliminar(int CodEmpresa, string usuario, string usuario_asignado)
        {
            return _db.OrdenAutousers_Eliminar(CodEmpresa, usuario, usuario_asignado);
        }

        public ErrorDto OrdenAutorizadores_Insertar(int CodEmpresa, string usuario)
        {
            return _db.OrdenAutorizadores_Insertar(CodEmpresa, usuario);
        }

        public ErrorDto OrdenAutorizadores_Eliminar(int CodEmpresa, string usuario)
        {
            return _db.OrdenAutorizadores_Eliminar(CodEmpresa, usuario);
        }

        public ErrorDto<UsuariosAuthorizaLista> FechaCamnbioAutorizadores_Obtener(int CodEmpresa, string filtro)
        {
            return _db.FechaCamnbioAutorizadores_Obtener(CodEmpresa, filtro);
        }

        public ErrorDto<List<UsuariosAutorizaData>> ListaAutorizador_Obtener(int CodEmpresa, string filtro)
        {
            return _db.ListaAutorizador_Obtener(CodEmpresa, filtro);
        }

        public ErrorDto<UsuariosAuthorizaLista> ListaAutousers_Obtener(int CodEmpresa, string usuario, string filtro)
        {
            return _db.ListaAutousers_Obtener(CodEmpresa, usuario, filtro);
        }

        public ErrorDto FechaCambioAutorizadores_Insertar(int CodEmpresa, string usuario, string registro_usuario)
        {
            return _db.FechaCambioAutorizadores_Insertar(CodEmpresa, usuario, registro_usuario);
        }

        public ErrorDto FechaCambioAutorizadores_Eliminar(int CodEmpresa, string usuario)
        {
            return _db.FechaCambioAutorizadores_Eliminar(CodEmpresa, usuario);
        }

        public ErrorDto<List<RangosDto>> ObtenerListaRangos(int CodEmpresa)
        {
            return _db.ObtenerListaRangos(CodEmpresa);
        }

        public ErrorDto<List<RangosUsuariosDto>> obtenerRangoUsuarios(int CodCliente, string cod_rango, string cod_uen, string? filtro)
        {
            return _db.obtenerRangoUsuarios(CodCliente, cod_rango, cod_uen, filtro);
        }

        public ErrorDto registroRangosUsuarios(int CodCliente, string cod_rango, RangosUsuariosDto request)
        {
            return _db.registroRangosUsuarios(CodCliente, cod_rango, request);
        }

        public ErrorDto Rangos_Agregar(int CodEmpresa, RangosDto request)
        {
            return _db.Rangos_Agregar(CodEmpresa, request);
        }

        public ErrorDto Rangos_Actualizar(int CodEmpresa, RangosDto request)
        {
            return _db.Rangos_Actualizar(CodEmpresa, request);
        }

        public ErrorDto Rangos_Eliminar(int CodEmpresa, string id)
        {
            return _db.Rangos_Eliminar(CodEmpresa, id);
        }
    }
}