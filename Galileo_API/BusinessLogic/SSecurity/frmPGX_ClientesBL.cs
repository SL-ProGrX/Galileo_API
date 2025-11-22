using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.Security;

namespace Galileo.BusinessLogic
{
    public class FrmPgxClientesBl
    {
        readonly FrmPgxClientesDb _clientesDB;

        public FrmPgxClientesBl(IConfiguration config)
        {
            _clientesDB = new FrmPgxClientesDb(config);
        }

        public ClientesDataLista Clientes_Obtener(int? pagina, int? paginacion, string? filtro)
        {
            return _clientesDB.Clientes_Obtener(pagina, paginacion, filtro);
        }

        public ClienteDto Cliente_Obtener(int CodEmpresa)
        {
            return _clientesDB.Cliente_Obtener(CodEmpresa);
        }

        public ClienteDto ConsultaAscDesc(int CodEmpresa, string tipo)
        {
            return _clientesDB.ConsultaAscDesc(CodEmpresa, tipo);
        }

        public ErrorDto Cliente_Modificar(ClienteDto info)
        {
            return _clientesDB.Cliente_Modificar(info);
        }

        public RespuestaDto Cliente_Crear(ClienteDto info)
        {
            return _clientesDB.Cliente_Crear(info);
        }

        public ErrorDto Cliente_Eliminar(int CodEmpresa, string usuario)
        {
            return _clientesDB.Cliente_Eliminar(CodEmpresa, usuario);
        }

        public List<ListaDD> Cliente_TiposId_Obtener()
        {
            return _clientesDB.Cliente_TiposId_Obtener();
        }

        public List<ListaDD> Cliente_Clasificaciones_Obtener()
        {
            return _clientesDB.Cliente_Clasificaciones_Obtener();
        }

        public List<ListaDD> Cliente_Vendedores_Obtener()
        {
            return _clientesDB.Cliente_Vendedores_Obtener();
        }

        public List<ContactoDto> ContactosCliente_Obtener(int CodEmpresa)
        {
            return _clientesDB.ContactosCliente_Obtener(CodEmpresa);
        }

        public ErrorDto ContactoCliente_Actualizar(ContactoDto info)
        {
            return _clientesDB.ContactoCliente_Actualizar(info);
        }

        public ErrorDto ContactoCliente_Insertar(ContactoDto info)
        {
            return _clientesDB.ContactoCliente_Insertar(info);
        }

        public ErrorDto ContactoCliente_Eliminar(int cod_contacto, int cod_empresa)
        {
            return _clientesDB.ContactoCliente_Eliminar(cod_contacto, cod_empresa);
        }

        public List<ServicioDto> ServiciosCliente_Obtener(int CodEmpresa)
        {
            return _clientesDB.ServiciosCliente_Obtener(CodEmpresa);
        }

        public List<SmtpDto> ListaSMTP(int CodEmpresa)
        {
            return _clientesDB.ListaSMTP(CodEmpresa);
        }

        public ErrorDto Smtp_Autorizar(SmtpDto info)
        {
            return _clientesDB.SMTP_Autorizar(info);
        }

        public static ErrorDto TestConnection(string connectionName, ConnectionModel connection)
        {
            return FrmPgxClientesDb.TestConnection(connectionName, connection);
        }

        public ErrorDto Clientes_Sincronizar(int CodEmpresa, bool logos)
        {
            return _clientesDB.Clientes_Sincronizar(CodEmpresa, logos);
        }

        public List<PaisesDto> ObtenerPaises()
        {
            return _clientesDB.ObtenerPaises();
        }

        public List<ProvinciaDto> ObtenerProvincia(string CodPais)
        {
            return _clientesDB.ObtenerProvincia(CodPais);
        }

        public List<CantonDto> ObtenerCanton(string CodPais, string CodProvincia)
        {
            return _clientesDB.ObtenerCanton(CodPais, CodProvincia);
        }

        public List<DistritoDto> ObtenerDistrito(string CodPais, string CodProvincia, string CodCanton)
        {
            return _clientesDB.ObtenerDistrito(CodPais, CodProvincia, CodCanton);
        }

    }
}