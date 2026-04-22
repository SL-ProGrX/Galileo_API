using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX_EstudioCrd;
using Galileo_API.Models.ProGrX_EstudioCrd;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX_EstudioCrd
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FrmPreaClasificacionesController : ControllerBase
    {
        private readonly FrmPreaClasificacionesBl _bl;

        public FrmPreaClasificacionesController(IConfiguration config) =>
            _bl = new FrmPreaClasificacionesBl(config);

        [HttpGet("PreaClasificacion_Razones_Obtener")]
        public ErrorDto<List<PreaClasificacionRazonData>> PreaClasificacion_Razones_Obtener(int codEmpresa)
        {
            return _bl.PreaClasificacion_Razones_Obtener(codEmpresa);
        }

        [HttpGet("PreaClasificacion_Catalogo_Obtener")]
        public ErrorDto<List<PreaClasificacionData>> PreaClasificacion_Catalogo_Obtener(int codEmpresa, string catalogo)
        {
            return _bl.PreaClasificacion_Catalogo_Obtener(codEmpresa, catalogo);
        }

        [HttpPost("PreaClasificacion_Razon_Guardar")]
        public ErrorDto PreaClasificacion_Razon_Guardar(int codEmpresa, string usuario, PreaClasificacionRazonData request)
        {
            return _bl.PreaClasificacion_Razon_Guardar(codEmpresa, usuario, request);
        }

        [HttpDelete("PreaClasificacion_Razon_Eliminar")]
        public ErrorDto PreaClasificacion_Razon_Eliminar(int codEmpresa, string codRazon, string usuario)
        {
            return _bl.PreaClasificacion_Razon_Eliminar(codEmpresa, codRazon, usuario);
        }

        [HttpPost("PreaClasificacion_Garantia_Guardar")]
        public ErrorDto PreaClasificacion_Garantia_Guardar(int codEmpresa, string usuario, PreaClasificacionData request)
        {
            return _bl.PreaClasificacion_Garantia_Guardar(codEmpresa, usuario, request);
        }

        [HttpDelete("PreaClasificacion_Garantia_Eliminar")]
        public ErrorDto PreaClasificacion_Garantia_Eliminar(int codEmpresa, string codGarantia, string usuario)
        {
            return _bl.PreaClasificacion_Garantia_Eliminar(codEmpresa, codGarantia, usuario);
        }

        [HttpPost("PreaClasificacion_Mora_Guardar")]
        public ErrorDto PreaClasificacion_Mora_Guardar(int codEmpresa, string usuario, PreaClasificacionData request)
        {
            return _bl.PreaClasificacion_Mora_Guardar(codEmpresa, usuario, request);
        }
        
        [HttpDelete("PreaClasificacion_Mora_Eliminar")]
        public ErrorDto PreaClasificacion_Mora_Eliminar(int codEmpresa, string codMora, string usuario)
        {
            return _bl.PreaClasificacion_Mora_Eliminar(codEmpresa, codMora, usuario);
        }

        [HttpPost("PreaClasificacion_Capacidad_Guardar")]
        public ErrorDto PreaClasificacion_Capacidad_Guardar(int codEmpresa, string usuario, PreaClasificacionData request)
        {
            return _bl.PreaClasificacion_Capacidad_Guardar(codEmpresa, usuario, request);
        }

        [HttpDelete("PreaClasificacion_Capacidad_Eliminar")]
        public ErrorDto PreaClasificacion_Capacidad_Eliminar(int codEmpresa, string codCapacidad, string usuario)
        {
            return _bl.PreaClasificacion_Capacidad_Eliminar(codEmpresa, codCapacidad, usuario);
        }

        [HttpPost("PreaClasificacion_Endeudamiento_Guardar")]
        public ErrorDto PreaClasificacion_Endeudamiento_Guardar(int codEmpresa, string usuario, PreaClasificacionData request)
        {
            return _bl.PreaClasificacion_Endeudamiento_Guardar(codEmpresa, usuario, request);
        }

        [HttpDelete("PreaClasificacion_Endeudamiento_Eliminar")]
        public ErrorDto PreaClasificacion_Endeudamiento_Eliminar(int codEmpresa, string codEndeudamiento, string usuario)
        {
            return _bl.PreaClasificacion_Endeudamiento_Eliminar(codEmpresa, codEndeudamiento, usuario);
        }

        [HttpPost("PreaClasificacion_Historial_Guardar")]
        public ErrorDto PreaClasificacion_Historial_Guardar(int codEmpresa, string usuario, PreaClasificacionData request)
        {
            return _bl.PreaClasificacion_Historial_Guardar(codEmpresa, usuario, request);
        }

        [HttpDelete("PreaClasificacion_Historial_Eliminar")]
        public ErrorDto PreaClasificacion_Historial_Eliminar(int codEmpresa, string codHistorial, string usuario)
        {
            return _bl.PreaClasificacion_Historial_Eliminar(codEmpresa, codHistorial, usuario);
        }

        [HttpGet("PreaClasificacion_Garantia_Obtener")]
        public ErrorDto<List<PreaClasificacionGarantiaData>> PreaClasificacion_Garantia_Obtener(int codEmpresa, string codGarantia)
        {
            return _bl.PreaClasificacion_Garantia_Obtener(codEmpresa, codGarantia);
        }

        [HttpPost("PreaClasificacion_Garantia_Asignar")]
        public ErrorDto PreaClasificacion_Garantia_Asignar(int codEmpresa, string codGarantia, string garantia, bool asignado)
        {
            return _bl.PreaClasificacion_Garantia_Asignar(codEmpresa, codGarantia, garantia, asignado);
        }
    }
}