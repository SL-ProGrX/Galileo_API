using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX.Creditos;
using Galileo_API.Models.ProGrX.Creditos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX.Creditos
{
    [ApiController]
    [Route("api/[controller]")]
    public class FrmCrRetencionesController : ControllerBase
    {
        private readonly FrmCrRetencionesBL _bl;

        public FrmCrRetencionesController(IConfiguration config)
        {
            _bl = new FrmCrRetencionesBL(config);
        }

        [HttpGet("AF_CR_Retenciones_Obtener")]
        [Authorize]
        public List<RetencionCreditoData> AF_CR_Retenciones_Obtener(int codEmpresa, int idSolicitud)
        {
            return _bl.AF_CR_Retenciones_Obtener(codEmpresa, idSolicitud);
        }

        [HttpGet("AF_CR_Retenciones_ObtenerSocios")]
        [Authorize]
        public List<SocioData> AF_CR_Retenciones_ObtenerSocios(int codEmpresa)
        {
            return _bl.AF_CR_Retenciones_ObtenerSocios(codEmpresa);
        }

        [HttpGet("AF_CR_Retenciones_ObtenerCatalogoRetencion")]
        [Authorize]
        public List<CatalogoRetencionData> AF_CR_Retenciones_ObtenerCatalogoRetencion(int codEmpresa)
        {
            return _bl.AF_CR_Retenciones_ObtenerCatalogoRetencion(codEmpresa);
        }

        [HttpGet("AF_CR_Retenciones_ObtenerDeductorasCombo")]
        [Authorize]
        public List<DropDownListaGenericaModel> AF_CR_Retenciones_ObtenerDeductorasCombo(int codEmpresa, string codInstitucion)
        {
            return _bl.AF_CR_Retenciones_ObtenerDeductorasCombo(codEmpresa, codInstitucion);
        }

        [HttpGet("AF_CR_Retenciones_ObtenerInstitucionFrecuencia")]
        [Authorize]
        public List<InstitucionFrecuenciaData> AF_CR_Retenciones_ObtenerInstitucionFrecuencia(int codEmpresa, string codDeductora)
        {
            return _bl.AF_CR_Retenciones_ObtenerInstitucionFrecuencia(codEmpresa, codDeductora);
        }

        [HttpGet("AF_CR_Retenciones_ObtenerSocioDeduccion")]
        [Authorize]
        public List<SocioDeduccionData> AF_CR_Retenciones_ObtenerSocioDeduccion(int codEmpresa, string cedula)
        {
            return _bl.AF_CR_Retenciones_ObtenerSocioDeduccion(codEmpresa, cedula);
        }

        [HttpGet("AF_CR_Retenciones_ObtenerPrimerDeduccion")]
        [Authorize]
        public List<PrimerDeduccionData> AF_CR_Retenciones_ObtenerPrimerDeduccion(int codEmpresa, string codDeductora)
        {
            return _bl.AF_CR_Retenciones_ObtenerPrimerDeduccion(codEmpresa, codDeductora);
        }

        [HttpGet("AF_CR_Retenciones_ObtenerDestinosPorCodigo")]
        [Authorize]
        public List<DropDownListaGenericaModel> AF_CR_Retenciones_ObtenerDestinosPorCodigo(int codEmpresa, string codigo)
        {
            return _bl.AF_CR_Retenciones_ObtenerDestinosPorCodigo(codEmpresa, codigo);
        }

        [HttpGet("AF_CR_Retenciones_ObtenerGarantiasPorLinea")]
        [Authorize]
        public List<DropDownListaGenericaModel> AF_CR_Retenciones_ObtenerGarantiasPorLinea(int codEmpresa, string linea)
        {
            return _bl.AF_CR_Retenciones_ObtenerGarantiasPorLinea(codEmpresa, linea);
        }

        [HttpGet("AF_CR_Retenciones_ObtenerCatalogoDetalle")]
        [Authorize]
        public List<CatalogoDetalleData> AF_CR_Retenciones_ObtenerCatalogoDetalle(int codEmpresa, string codigo)
        {
            return _bl.AF_CR_Retenciones_ObtenerCatalogoDetalle(codEmpresa, codigo);
        }

        [HttpGet("AF_CR_Retenciones_ObtenerSiguienteSolicitud")]
        [Authorize]
        public List<SiguienteSolicitudData> AF_CR_Retenciones_ObtenerSiguienteSolicitud(int codEmpresa, int idSolicitudActual, bool siguiente)
        {
            return _bl.AF_CR_Retenciones_ObtenerSiguienteSolicitud(codEmpresa, idSolicitudActual, siguiente);
        }
    }
}
