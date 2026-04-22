using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic.ProGrX.Cajas;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Cajas;

namespace Galileo.Controllers.ProGrX.Cajas
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmCajasRecaudadoresController : ControllerBase
    {
        private readonly FrmCajasRecaudadoresBL _bl;

        public FrmCajasRecaudadoresController(IConfiguration config)
        {
            _bl = new FrmCajasRecaudadoresBL(config);
        }

        [HttpGet("Cajas_Recaudadores_Lista_Obtener")]
        [Authorize]
        public ErrorDto<CajasRecaudadoresLista> Cajas_Recaudadores_Lista_Obtener(int CodEmpresa,string jfiltros)
        {
            return _bl.Cajas_Recaudadores_Lista_Obtener(CodEmpresa, jfiltros);
        }

        [HttpGet("Cajas_Recaudadores_Scroll")]
        [Authorize]
        public ErrorDto<CajasRecaudadorData> Cajas_Recaudadores_Scroll(int CodEmpresa,int cod_contabilidad,int scroll,string? cod_recaudador)
        {
            return _bl.Cajas_Recaudadores_Scroll(CodEmpresa, cod_contabilidad, scroll, cod_recaudador);
        }

        [HttpGet("Cajas_Recaudadores_Obtener")]
        [Authorize]
        public ErrorDto<CajasRecaudadorData> Cajas_Recaudadores_Obtener(int CodEmpresa,int cod_contabilidad,string cod_recaudador)
        {
            return _bl.Cajas_Recaudadores_Obtener(CodEmpresa, cod_contabilidad, cod_recaudador);
        }

        [HttpGet("Cajas_Recaudadores_Existe_Obtener")]
        [Authorize]
        public ErrorDto Cajas_Recaudadores_Existe_Obtener(int CodEmpresa,string cod_recaudador)
        {
            return _bl.Cajas_Recaudadores_Existe_Obtener(CodEmpresa, cod_recaudador);
        }

        [HttpPost("Cajas_Recaudadores_Guardar")]
        [Authorize]
        public ErrorDto Cajas_Recaudadores_Guardar(int CodEmpresa, string usuario,CajasRecaudadorData recaudador)
        {
            return _bl.Cajas_Recaudadores_Guardar(CodEmpresa, usuario, recaudador);
        }

        [HttpDelete("Cajas_Recaudadores_Eliminar")]
        [Authorize]
        public ErrorDto Cajas_Recaudadores_Eliminar(int CodEmpresa,string usuario,string cod_recaudador)
        {
            return _bl.Cajas_Recaudadores_Eliminar(CodEmpresa, usuario, cod_recaudador);
        }

        [HttpGet("Cajas_Recaudadores_Contactos_Lista_Obtener")]
        [Authorize]
        public ErrorDto<List<CajasRecaudadorContactoData>> Cajas_Recaudadores_Contactos_Lista_Obtener(int CodEmpresa,string cod_recaudador)
        {
            return _bl.Cajas_Recaudadores_Contactos_Lista_Obtener(CodEmpresa, cod_recaudador);
        }

        [HttpPost("Cajas_Recaudadores_Contactos_Guardar")]
        [Authorize]
        public ErrorDto Cajas_Recaudadores_Contactos_Guardar(int CodEmpresa,string usuario,CajasRecaudadorContactoData contacto)
        {
            return _bl.Cajas_Recaudadores_Contactos_Guardar(CodEmpresa, usuario, contacto);
        }

        [HttpDelete("Cajas_Recaudadores_Contactos_Eliminar")]
        [Authorize]
        public ErrorDto Cajas_Recaudadores_Contactos_Eliminar(int CodEmpresa,string usuario,string cod_recaudador,int linea)
        {
            return _bl.Cajas_Recaudadores_Contactos_Eliminar(CodEmpresa, usuario, cod_recaudador, linea);
        }

        [HttpGet("Cajas_Recaudadores_Servicios_Lista_Obtener")]
        [Authorize]
        public ErrorDto<List<CajasRecaudadorServicioItem>> Cajas_Recaudadores_Servicios_Lista_Obtener(int CodEmpresa,string cod_recaudador)
        {
            return _bl.Cajas_Recaudadores_Servicios_Lista_Obtener(CodEmpresa, cod_recaudador);
        }

        [HttpGet("Cajas_Recaudadores_Servicios_CajasVinculadas_Lista_Obtener")]
        [Authorize]
        public ErrorDto<List<CajasServiciosCajasVinculadasData>> Cajas_Recaudadores_Servicios_CajasVinculadas_Lista_Obtener(int CodEmpresa,string cod_recaudador,string cod_servicio)
        {
            return _bl.Cajas_Recaudadores_Servicios_CajasVinculadas_Lista_Obtener(CodEmpresa,cod_recaudador,cod_servicio);
        }
        
        [HttpPost("Cajas_Recaudadores_Servicios_CajasVinculadas_Guardar")]
        [Authorize]
        public ErrorDto Cajas_Recaudadores_Servicios_CajasVinculadas_Guardar(int CodEmpresa,string usuario,string cod_recaudador,string cod_servicio,string cod_caja,short asignada)
        {
            return _bl.Cajas_Recaudadores_Servicios_CajasVinculadas_Guardar(CodEmpresa,usuario,cod_recaudador,cod_servicio,cod_caja, asignada);
        }
    }
}