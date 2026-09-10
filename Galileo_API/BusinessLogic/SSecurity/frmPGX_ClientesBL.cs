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

        public ErrorDto<ClientesDataLista> Clientes_Obtener(int? pagina, int? paginacion, string? filtro)
        {
            return _clientesDB.Clientes_Obtener(pagina, paginacion, filtro);
        }

        public ErrorDto<ClienteDto?> Cliente_Obtener(int CodEmpresa)
        {
            return _clientesDB.Cliente_Obtener(CodEmpresa);
        }

        public ErrorDto<List<ClienteSincronizacionDto>> Clientes_Sincronizacion_Obtener()
        {
            return _clientesDB.Clientes_Sincronizacion_Obtener();
        }

        public ErrorDto<ClienteDto?> ConsultaAscDesc(int CodEmpresa, string tipo)
        {
            return _clientesDB.ConsultaAscDesc(CodEmpresa, tipo);
        }

        public ErrorDto TestConnection(int CodEmpresa, string connectionName)
        {
            return _clientesDB.TestConnection(CodEmpresa, connectionName);
        }

        public ErrorDto Cliente_Modificar(ClienteDto info)
        {
            return _clientesDB.Cliente_Modificar(info);
        }

        public ErrorDto<RespuestaDto> Cliente_Crear(ClienteDto info)
        {
            return _clientesDB.Cliente_Crear(info);
        }

        public ErrorDto Cliente_Eliminar(int CodEmpresa, string usuario)
        {
            return _clientesDB.Cliente_Eliminar(CodEmpresa, usuario);
        }

        public ErrorDto<List<ListaDD>> Cliente_TiposId_Obtener()
        {
            return _clientesDB.Cliente_TiposId_Obtener();
        }

        public ErrorDto<List<ListaDD>> Cliente_Clasificaciones_Obtener()
        {
            return _clientesDB.Cliente_Clasificaciones_Obtener();
        }

        public ErrorDto<List<ListaDD>> Cliente_Vendedores_Obtener()
        {
            return _clientesDB.Cliente_Vendedores_Obtener();
        }

        public ErrorDto<List<ContactoDto>> ContactosCliente_Obtener(int CodEmpresa)
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

        public ErrorDto ContactoCliente_Eliminar(int cod_contacto, int cod_empresa, string usuario)
        {
            return _clientesDB.ContactoCliente_Eliminar(cod_contacto, cod_empresa, usuario);
        }

        public ErrorDto<List<ServicioDto>> ServiciosCliente_Obtener(int CodEmpresa)
        {
            return _clientesDB.ServiciosCliente_Obtener(CodEmpresa);
        }

        public ErrorDto ServicioAsignar(ServicioAsignarDto request, string modo)
        {
            return _clientesDB.ServicioAsignar(request, modo);
        }

        public ErrorDto<List<SmtpDto>> ListaSMTP(int CodEmpresa)
        {
            return _clientesDB.ListaSMTP(CodEmpresa);
        }

        public ErrorDto Smtp_Autorizar(SmtpDto info)
        {
            return _clientesDB.SMTP_Autorizar(info);
        }

        public ErrorDto Clientes_Sincronizar(int CodEmpresa, bool logos)
        {
            return _clientesDB.Clientes_Sincronizar(CodEmpresa, logos);
        }

        public ErrorDto<List<PaisesDto>> ObtenerPaises()
        {
            return _clientesDB.ObtenerPaises();
        }

        public ErrorDto<List<ProvinciaDto>> ObtenerProvincia(string CodPais)
        {
            return _clientesDB.ObtenerProvincia(CodPais);
        }

        public ErrorDto<List<CantonDto>> ObtenerCanton(string CodPais, string CodProvincia)
        {
            return _clientesDB.ObtenerCanton(CodPais, CodProvincia);
        }

        public ErrorDto<List<DistritoDto>> ObtenerDistrito(string CodPais, string CodProvincia, string CodCanton)
        {
            return _clientesDB.ObtenerDistrito(CodPais, CodProvincia, CodCanton);
        }

    }
}
