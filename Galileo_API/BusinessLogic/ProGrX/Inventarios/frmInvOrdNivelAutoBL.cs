using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.INV;

namespace Galileo.BusinessLogic
{
    public class FrmInvOrdNivelAutoBL
    {
        private readonly FrmInvOrdNivelAutoDB _db;

        public FrmInvOrdNivelAutoBL(IConfiguration config)
        {
            _db = new FrmInvOrdNivelAutoDB(config);
        }

        public ErrorDto<AutorizadorDataLista> Autorizadores_Obtener(int CodCliente, int? pagina, int? paginacion, string? filtro)
        {
            return _db.Autorizadores_Obtener(CodCliente, pagina, paginacion, filtro);
        }

        public ErrorDto<List<AutorizadorDto>> Autorizador_ObtenerTodos(int CodEmpresa)
        {
            return _db.Autorizador_ObtenerTodos(CodEmpresa);
        }

        public ErrorDto<List<AutorizadorDto>> Autorizador_Obtener(int CodEmpresa)
        {
            return _db.Autorizador_Obtener(CodEmpresa);
        }
        public ErrorDto Autorizador_Insertar(int CodEmpresa, AutorizadorDto request)
        {
            return _db.Autorizador_Insertar(CodEmpresa, request);
        }

        public ErrorDto Autorizador_Eliminar(int CodEmpresa, AutorizadorDto request)
        {
            return _db.Autorizador_Eliminar(CodEmpresa, request.Usuario);
        }

        public ErrorDto<UsuariosACargoDataLista> UsuariosACargoAut_Obtener(int CodCliente, string usuario, int? pagina, int? paginacion, string? filtro)
        {
            return _db.UsuariosACargoAut_Obtener(CodCliente, usuario, pagina, paginacion, filtro);
        }

        public List<UsuarioaCargoDto> UsuariosACargo_Obtener(int CodEmpresa, string usuario)
        {
            return _db.UsuariosACargo_Obtener(CodEmpresa, usuario);
        }
        public ErrorDto UsuarioACargo_Actualizar(int CodEmpresa, UsuarioaCargoDto request)
        {
            return _db.UsuarioACargo_Actualizar(CodEmpresa, request);
        }

        public UsuariosCambioFchDataLista UsuariosCambioFch_Obtener(int CodCliente, string tipo, int? pagina, int? paginacion, string? filtro)
        {
            return _db.UsuariosCambioFch_Obtener(CodCliente, tipo, pagina, paginacion, filtro);
        }

        public List<UsuarioaCambioFechaDto> UsuariosCambioFecha_Obtener(int CodEmpresa, string tipo)
        {
            return _db.UsuariosCambioFecha_Obtener(CodEmpresa, tipo);
        }

        public ErrorDto CambioFechas_Insertar(int CodEmpresa, UsuarioaCambioFechaDto request)
        {
            return _db.CambioFechas_Insertar(CodEmpresa, request);
        }

        public ErrorDto CambioFechas_Eliminar(int CodEmpresa, UsuarioaCambioFechaDto request)
        {
            return _db.CambioFechas_Eliminar(CodEmpresa, request);
        }
    }
}