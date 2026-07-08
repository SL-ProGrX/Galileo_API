using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX.Creditos;
using Galileo_API.Models.ProGrX.Credito;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX.Creditos
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FrmCrTasasPtsBonificacionController : ControllerBase
    {
        private readonly FrmCrTasasPtsBonificacionBl _bl;

        public FrmCrTasasPtsBonificacionController(IConfiguration config)
        {
            _bl = new FrmCrTasasPtsBonificacionBl(config);
        }

        [HttpGet("CrTasasPtsBonificacion_Planes_Obtener")]
        public ErrorDto<List<CrTasasPtsBonificacionPlanData>> CrTasasPtsBonificacion_Planes_Obtener(int codEmpresa)
        {
            return _bl.CrTasasPtsBonificacion_Planes_Obtener(codEmpresa);
        }

        [HttpGet("CrTasasPtsBonificacion_Scroll_Obtener")]
        public ErrorDto<CrTasasPtsBonificacionDefinicionData?> CrTasasPtsBonificacion_Scroll_Obtener(
            int codEmpresa,
            int scroll,
            string codTasaBono)
        {
            return _bl.CrTasasPtsBonificacion_Scroll_Obtener(codEmpresa, scroll, codTasaBono);
        }

        [HttpGet("CrTasasPtsBonificacion_Definicion_Obtener")]
        public ErrorDto<CrTasasPtsBonificacionDefinicionData?> CrTasasPtsBonificacion_Definicion_Obtener(
            int codEmpresa,
            string cod_tasa_bono)
        {
            return _bl.CrTasasPtsBonificacion_Definicion_Obtener(codEmpresa, cod_tasa_bono);
        }

        [HttpPost("CrTasasPtsBonificacion_Definicion_Guardar")]
        public ErrorDto CrTasasPtsBonificacion_Definicion_Guardar(
            int codEmpresa,
            CrTasasPtsBonificacionDefinicionGuardarRequest request)
        {
            return _bl.CrTasasPtsBonificacion_Definicion_Guardar(codEmpresa, request);
        }

        [HttpDelete("CrTasasPtsBonificacion_Definicion_Eliminar")]
        public ErrorDto CrTasasPtsBonificacion_Definicion_Eliminar(
            int codEmpresa,
            CrTasasPtsBonificacionDefinicionEliminarRequest request)
        {
            return _bl.CrTasasPtsBonificacion_Definicion_Eliminar(codEmpresa, request);
        }

        [HttpGet("CrTasasPtsBonificacion_Membresias_Obtener")]
        public ErrorDto<List<CrTasasPtsBonificacionMembresiaData>> CrTasasPtsBonificacion_Membresias_Obtener(
            int codEmpresa,
            string cod_tasa_bono)
        {
            return _bl.CrTasasPtsBonificacion_Membresias_Obtener(codEmpresa, cod_tasa_bono);
        }

        [HttpPost("CrTasasPtsBonificacion_Membresias_Guardar")]
        public ErrorDto CrTasasPtsBonificacion_Membresias_Guardar(
            int codEmpresa,
            CrTasasPtsBonificacionMembresiaGuardarRequest request)
        {
            return _bl.CrTasasPtsBonificacion_Membresias_Guardar(codEmpresa, request);
        }

        [HttpDelete("CrTasasPtsBonificacion_Membresias_Eliminar")]
        public ErrorDto CrTasasPtsBonificacion_Membresias_Eliminar(
            int codEmpresa,
            CrTasasPtsBonificacionLineaEliminarRequest request)
        {
            return _bl.CrTasasPtsBonificacion_Membresias_Eliminar(codEmpresa, request);
        }

        [HttpGet("CrTasasPtsBonificacion_Destinos_Obtener")]
        public ErrorDto<List<CrTasasPtsBonificacionDestinoData>> CrTasasPtsBonificacion_Destinos_Obtener(
            int codEmpresa,
            string cod_tasa_bono)
        {
            return _bl.CrTasasPtsBonificacion_Destinos_Obtener(codEmpresa, cod_tasa_bono);
        }

        [HttpPost("CrTasasPtsBonificacion_Destinos_Guardar")]
        public ErrorDto CrTasasPtsBonificacion_Destinos_Guardar(
            int codEmpresa,
            CrTasasPtsBonificacionDestinoGuardarRequest request)
        {
            return _bl.CrTasasPtsBonificacion_Destinos_Guardar(codEmpresa, request);
        }

        [HttpDelete("CrTasasPtsBonificacion_Destinos_Eliminar")]
        public ErrorDto CrTasasPtsBonificacion_Destinos_Eliminar(
            int codEmpresa,
            CrTasasPtsBonificacionLineaEliminarRequest request)
        {
            return _bl.CrTasasPtsBonificacion_Destinos_Eliminar(codEmpresa, request);
        }

        [HttpGet("CrTasasPtsBonificacion_Liquidez_Obtener")]
        public ErrorDto<List<CrTasasPtsBonificacionLiquidezData>> CrTasasPtsBonificacion_Liquidez_Obtener(
            int codEmpresa,
            string cod_tasa_bono)
        {
            return _bl.CrTasasPtsBonificacion_Liquidez_Obtener(codEmpresa, cod_tasa_bono);
        }

        [HttpPost("CrTasasPtsBonificacion_Liquidez_Guardar")]
        public ErrorDto CrTasasPtsBonificacion_Liquidez_Guardar(
            int codEmpresa,
            CrTasasPtsBonificacionLiquidezGuardarRequest request)
        {
            return _bl.CrTasasPtsBonificacion_Liquidez_Guardar(codEmpresa, request);
        }

        [HttpDelete("CrTasasPtsBonificacion_Liquidez_Eliminar")]
        public ErrorDto CrTasasPtsBonificacion_Liquidez_Eliminar(
            int codEmpresa,
            CrTasasPtsBonificacionLineaEliminarRequest request)
        {
            return _bl.CrTasasPtsBonificacion_Liquidez_Eliminar(codEmpresa, request);
        }

        [HttpGet("CrTasasPtsBonificacion_Asignaciones_Obtener")]
        public ErrorDto<List<CrTasasPtsBonificacionAsignacionLineaData>> CrTasasPtsBonificacion_Asignaciones_Obtener(
            int codEmpresa,
            string cod_tasa_bono)
        {
            return _bl.CrTasasPtsBonificacion_Asignaciones_Obtener(codEmpresa, cod_tasa_bono);
        }

        [HttpGet("CrTasasPtsBonificacion_AsignacionPlanes_Obtener")]
        public ErrorDto<List<CrTasasPtsBonificacionAsignacionPlanData>> CrTasasPtsBonificacion_AsignacionPlanes_Obtener(
            int codEmpresa,
            string codigo,
            string garantia)
        {
            return _bl.CrTasasPtsBonificacion_AsignacionPlanes_Obtener(codEmpresa, codigo, garantia);
        }

        [HttpPost("CrTasasPtsBonificacion_Asignaciones_Guardar")]
        public ErrorDto CrTasasPtsBonificacion_Asignaciones_Guardar(
            int codEmpresa,
            CrTasasPtsBonificacionAsignacionGuardarRequest request)
        {
            return _bl.CrTasasPtsBonificacion_Asignaciones_Guardar(codEmpresa, request);
        }

        [HttpGet("CrTasasPtsBonificacion_DestinosCatalogo_Obtener")]
        public ErrorDto<List<CrTasasPtsBonificacionDestinoCatalogoData>> CrTasasPtsBonificacion_DestinosCatalogo_Obtener(
            int codEmpresa)
        {
            return _bl.CrTasasPtsBonificacion_DestinosCatalogo_Obtener(codEmpresa);
        }
    }
}
