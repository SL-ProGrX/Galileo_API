using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic.ProGrX.Cajas;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Cajas;

namespace Galileo.Controllers.ProGrX.Cajas
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmCajasServiciosController : ControllerBase
    {
        private readonly FrmCajasServiciosBL _bl;

        public FrmCajasServiciosController(IConfiguration config)
        {
            _bl = new FrmCajasServiciosBL(config);
        }
        [HttpGet("Cajas_Servicios_Conceptos_Lista_Obtener")]
        [Authorize]
        public ErrorDto<CajasServiciosConceptosLista>Cajas_Servicios_Conceptos_Lista_Obtener(int CodEmpresa, string cod_recaudador, string jfiltros)
        {
            return _bl.Cajas_Servicios_Conceptos_Lista_Obtener(CodEmpresa, cod_recaudador, jfiltros);
        }

        [HttpGet("Cajas_Servicios_Conceptos_Scroll")]
        [Authorize]
        public ErrorDto<CajasServiciosConceptosData> Cajas_Servicios_Conceptos_Scroll(int CodEmpresa, string cod_recaudador, int scroll, string? cod_servicio)
        {
            // Ensure cod_servicio is not null before passing to BL
            return _bl.Cajas_Servicios_Conceptos_Scroll(CodEmpresa, cod_recaudador, scroll, cod_servicio ?? string.Empty);
        }

        [HttpGet("Cajas_Servicios_Conceptos_DropDown_Obtener")]
        [Authorize]
        public ErrorDto<List<DropDownListaGenericaModel>>Cajas_Servicios_Conceptos_DropDown_Obtener(int CodEmpresa)
        {
            return _bl.Cajas_Servicios_Conceptos_DropDown_Obtener(CodEmpresa);
        }
        
        [HttpGet("Cajas_Servicios_Recaudadores_DropDown_Obtener")]
        [Authorize]
        public ErrorDto<List<DropDownListaGenericaModel>> Cajas_Servicios_Recaudadores_DropDown_Obtener(int CodEmpresa)
        {
            return _bl.Cajas_Servicios_Recaudadores_DropDown_Obtener(CodEmpresa);
        }

        [HttpGet("Cajas_Servicios_Cabys_Lista_Obtener")]
        [Authorize]
        public ActionResult<ErrorDto<CajasServiciosCabysLista>> Cajas_Servicios_Cabys_Lista_Obtener(int CodEmpresa,string? jfiltros)
        {
            return _bl.Cajas_Servicios_Cabys_Lista_Obtener(CodEmpresa, jfiltros);
        }
        
        [HttpGet("Cajas_Servicios_Conceptos_Obtener")]
        [Authorize]
        public ErrorDto<CajasServiciosConceptosData>Cajas_Servicios_Conceptos_Obtener(int CodEmpresa, string cod_recaudador, string cod_servicio)
        {
            return _bl.Cajas_Servicios_Conceptos_Obtener(CodEmpresa, cod_recaudador, cod_servicio);
        }

        [HttpGet("Cajas_Servicios_Conceptos_Existe_Obtener")]
        [Authorize]
        public ErrorDto Cajas_Servicios_Conceptos_Existe_Obtener(int CodEmpresa, string cod_recaudador, string cod_servicio)
        {
            return _bl.Cajas_Servicios_Conceptos_Existe_Obtener(CodEmpresa, cod_recaudador, cod_servicio);
        }

        [HttpPost("Cajas_Servicios_Conceptos_Guardar")]
        [Authorize]
        public ErrorDto Cajas_Servicios_Conceptos_Guardar(int CodEmpresa, string usuario, CajasServiciosConceptosData servicio)
        {
            return _bl.Cajas_Servicios_Conceptos_Guardar(CodEmpresa, usuario, servicio);
        }
        [HttpGet("Cajas_Servicios_Comisiones_Lista_Obtener")]
        [Authorize]
        public ErrorDto<List<CajasServiciosComisionesData>>Cajas_Servicios_Comisiones_Lista_Obtener(int CodEmpresa, string cod_recaudador, string cod_servicio)
        {
            return _bl.Cajas_Servicios_Comisiones_Lista_Obtener(CodEmpresa, cod_recaudador, cod_servicio);
        }
        
        [HttpPost("Cajas_Servicios_Comisiones_Guardar")]
        [Authorize]
        public ErrorDto Cajas_Servicios_Comisiones_Guardar(int CodEmpresa, string usuario, CajasServiciosComisionesData rango)
        {
            return _bl.Cajas_Servicios_Comisiones_Guardar(CodEmpresa, usuario, rango);
        }

        [HttpDelete("Cajas_Servicios_Comisiones_Eliminar")]
        [Authorize]
        public ErrorDto Cajas_Servicios_Comisiones_Eliminar(int CodEmpresa, string usuario, string cod_recaudador, string cod_servicio, int linea)
        {
            return _bl.Cajas_Servicios_Comisiones_Eliminar(CodEmpresa, usuario, cod_recaudador, cod_servicio, linea);
        }
        
        [HttpGet("Cajas_Servicios_CajasVinculadas_Lista_Obtener")]
        [Authorize]
        public ErrorDto<List<CajasServiciosCajasVinculadasData>>Cajas_Servicios_CajasVinculadas_Lista_Obtener(int CodEmpresa, string cod_recaudador, string cod_servicio)
        {
            return _bl.Cajas_Servicios_CajasVinculadas_Lista_Obtener(CodEmpresa, cod_recaudador, cod_servicio);
        }
        
        [HttpPost("Cajas_Servicios_CajasVinculadas_Guardar")]
        [Authorize]
        public ErrorDto Cajas_Servicios_CajasVinculadas_Guardar(int CodEmpresa, string usuario, string cod_recaudador, string cod_servicio, string cod_caja, short asignada)
        {
            return _bl.Cajas_Servicios_CajasVinculadas_Guardar(CodEmpresa, usuario, cod_recaudador, cod_servicio, cod_caja, asignada);
        }
    }
}