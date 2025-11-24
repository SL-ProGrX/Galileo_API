using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.Security;

namespace Galileo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmPgx_ClientesController : ControllerBase
    {
        readonly FrmPgxClientesBl ClientesBL;

        public FrmPgx_ClientesController(IConfiguration config)
        {
            ClientesBL = new FrmPgxClientesBl(config);
        }

        [HttpGet("Clientes_Obtener")]
        public ClientesDataLista Clientes_Obtener(int? pagina, int? paginacion, string? filtro)
        {
            return ClientesBL.Clientes_Obtener(pagina, paginacion, filtro);
        }

        [HttpGet("Cliente_Obtener")]
        public ClienteDto Cliente_Obtener(int CodEmpresa)
        {
            return ClientesBL.Cliente_Obtener(CodEmpresa);
        }

        [HttpGet("ConsultaAscDesc")]
        public ClienteDto ConsultaAscDesc(int CodEmpresa, string tipo)
        {
            return ClientesBL.ConsultaAscDesc(CodEmpresa, tipo);
        }

        [HttpPost("Cliente_Modificar")]
        public ErrorDto Cliente_Modificar(ClienteDto info)
        {
            return ClientesBL.Cliente_Modificar(info);
        }

        [HttpPost("Cliente_Crear")]
        public RespuestaDto Cliente_Crear(ClienteDto info)
        {
            return ClientesBL.Cliente_Crear(info);
        }

        [HttpDelete("Cliente_Eliminar")]
        public ErrorDto Cliente_Eliminar(int CodEmpresa, string usuario)
        {
            return ClientesBL.Cliente_Eliminar(CodEmpresa, usuario);
        }

        [HttpGet("Cliente_TiposId_Obtener")]
        public List<ListaDD> Cliente_TiposId_Obtener()
        {
            return ClientesBL.Cliente_TiposId_Obtener();
        }

        [HttpGet("Cliente_Clasificaciones_Obtener")]
        public List<ListaDD> Cliente_Clasificaciones_Obtener()
        {
            return ClientesBL.Cliente_Clasificaciones_Obtener();
        }

        [HttpGet("Cliente_Vendedores_Obtener")]
        public List<ListaDD> Cliente_Vendedores_Obtener()
        {
            return ClientesBL.Cliente_Vendedores_Obtener();
        }




        [HttpGet("ContactosCliente_Obtener")]
        public List<ContactoDto> ContactosCliente_Obtener(int CodEmpresa)
        {
            return ClientesBL.ContactosCliente_Obtener(CodEmpresa);
        }

        [HttpPost("ContactoCliente_Actualizar")]
        public ErrorDto ContactoCliente_Actualizar(ContactoDto info)
        {
            return ClientesBL.ContactoCliente_Actualizar(info);
        }

        [HttpPost("ContactoCliente_Insertar")]
        public ErrorDto ContactoCliente_Insertar(ContactoDto info)
        {
            return ClientesBL.ContactoCliente_Insertar(info);
        }

        [HttpPost("ContactoCliente_Eliminar")]
        public ErrorDto ContactoCliente_Eliminar(int cod_contacto, int cod_empresa)
        {
            return ClientesBL.ContactoCliente_Eliminar(cod_contacto, cod_empresa);
        }

        [HttpGet("ServiciosCliente_Obtener")]
        public List<ServicioDto> ServiciosCliente_Obtener(int CodEmpresa)
        {
            return ClientesBL.ServiciosCliente_Obtener(CodEmpresa);
        }


        [HttpGet("ListaSMTP")]
        public List<SmtpDto> ListaSMTP(int CodEmpresa)
        {
            return ClientesBL.ListaSMTP(CodEmpresa);
        }

        [HttpPost("SMTP_Autorizar")]
        public ErrorDto SMTP_Autorizar(SmtpDto info)
        {
            return ClientesBL.Smtp_Autorizar(info);
        }


        [HttpPost("TestConnection")]
        public ErrorDto TestConnection(string connectionName, ConnectionModel connection)
        {
            return FrmPgxClientesBl.TestConnection(connectionName, connection);
        }

        [HttpPost("Clientes_Sincronizar")]
        public ErrorDto Clientes_Sincronizar(int CodEmpresa, bool logos)
        {
            return ClientesBL.Clientes_Sincronizar(CodEmpresa, logos);
        }


        [HttpGet("ObtenerPaises")]
        public List<PaisesDto> ObtenerPaises()
        {
            return ClientesBL.ObtenerPaises();
        }

        [HttpGet("ObtenerProvincia")]
        public List<ProvinciaDto> ObtenerProvincia(string CodPais)
        {
            return ClientesBL.ObtenerProvincia(CodPais);
        }

        [HttpGet("ObtenerCanton")]
        public List<CantonDto> ObtenerCanton(string CodPais, string CodProvincia)
        {
            return ClientesBL.ObtenerCanton(CodPais, CodProvincia);
        }

        [HttpGet("ObtenerDistrito")]
        public List<DistritoDto> ObtenerDistrito(string CodPais, string CodProvincia, string CodCanton)
        {
            return ClientesBL.ObtenerDistrito(CodPais, CodProvincia, CodCanton);
        }



    }//end class
}//end controller
