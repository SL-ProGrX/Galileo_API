using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX_Hipotecario;
using Galileo_API.Models.ProGrX_Hipotecario;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX_Hipotecario
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FrmVivControlAsignacionGarantiaController : ControllerBase
    {
        private readonly FrmVivControlAsignacionGarantiaBl _bl;

        public FrmVivControlAsignacionGarantiaController(IConfiguration config)
        {
            _bl = new FrmVivControlAsignacionGarantiaBl(config);
        }

        [HttpGet("VivControlAsignacionGarantia_Asignacion_ObtenerGarantiasPendientes")]
        public ErrorDto<List<VivControlAsignacionGarantiaPendienteData>> VivControlAsignacionGarantia_Asignacion_ObtenerGarantiasPendientes(int codEmpresa, string tipoProfesional)
        {
            return _bl.VivControlAsignacionGarantia_Asignacion_ObtenerGarantiasPendientes(codEmpresa, tipoProfesional);
        }

        [HttpGet("VivControlAsignacionGarantia_Asignacion_ObtenerProfesionales")]
        public ErrorDto<List<VivControlAsignacionProfesionalData>> VivControlAsignacionGarantia_Asignacion_ObtenerProfesionales(int codEmpresa, int idZona, string tipoProfesional, long idGarantia)
        {
            return _bl.VivControlAsignacionGarantia_Asignacion_ObtenerProfesionales(codEmpresa, idZona, tipoProfesional, idGarantia);
        }

        [HttpPost("VivControlAsignacionGarantia_Asignacion_Aplicar")]
        public ErrorDto VivControlAsignacionGarantia_Asignacion_Aplicar(int codEmpresa, string usuario, VivControlAsignacionGarantiaAsignarRequest request)
        {
            return _bl.VivControlAsignacionGarantia_Asignacion_Aplicar(codEmpresa, usuario, request);
        }

        [HttpDelete("VivControlAsignacionGarantia_Asignacion_Borrar")]
        public ErrorDto VivControlAsignacionGarantia_Asignacion_Borrar(int codEmpresa, long idGarantia, int idContacto, string usuario)
        {
            return _bl.VivControlAsignacionGarantia_Asignacion_Borrar(codEmpresa, idGarantia, idContacto, usuario);
        }

        [HttpGet("VivControlAsignacionGarantia_ObtenerProfesionales")]
        public ErrorDto<List<DropDownListaGenericaModel>> VivControlAsignacionGarantia_ObtenerProfesionales(
            int codEmpresa, string tipoLista, string tipoProfesional)
        {
            return _bl.VivControlAsignacionGarantia_ObtenerProfesionales(codEmpresa, tipoLista, tipoProfesional);
        }

        [HttpGet("VivControlAsignacionGarantia_Entrega_ObtenerGarantias")]
        public ErrorDto<List<VivControlEntregaGarantiaData>> VivControlAsignacionGarantia_Entrega_ObtenerGarantias(int codEmpresa, long idContacto, string tipoProfesional)
        {
            return _bl.VivControlAsignacionGarantia_Entrega_ObtenerGarantias(codEmpresa, idContacto, tipoProfesional);
        }

        [HttpPost("VivControlAsignacionGarantia_Entrega_Aplicar")]
        public ErrorDto VivControlAsignacionGarantia_Entrega_Aplicar(int codEmpresa, string usuario, VivControlEntregaGarantiaRequest request)
        {
            return _bl.VivControlAsignacionGarantia_Entrega_Aplicar(codEmpresa, usuario, request);
        }

        [HttpGet("VivControlAsignacionGarantia_ObtenerUltimaNota")]
        public ErrorDto<VivControlAsignacionGarantiaNotaData?> VivControlAsignacionGarantia_ObtenerUltimaNota(int codEmpresa, long idGarantia, string tipoProfesional)
        {
            return _bl.VivControlAsignacionGarantia_ObtenerUltimaNota(codEmpresa, idGarantia, tipoProfesional);
        }

        [HttpGet("VivControlAsignacionGarantia_Recepcion_ObtenerGarantias")]
        public ErrorDto<List<VivControlRecibeGarantiaData>> VivControlAsignacionGarantia_Recepcion_ObtenerGarantias(int codEmpresa, long idContacto, string tipoProfesional)
        {
            return _bl.VivControlAsignacionGarantia_Recepcion_ObtenerGarantias(codEmpresa, idContacto, tipoProfesional);
        }

        [HttpPost("VivControlAsignacionGarantia_Recepcion_Aplicar")]
        public ErrorDto VivControlAsignacionGarantia_Recepcion_Aplicar(int codEmpresa, string usuario, VivControlRecibeGarantiaRequest request)
        {
            return _bl.VivControlAsignacionGarantia_Recepcion_Aplicar(codEmpresa, usuario, request);
        }

        [HttpGet("VivControlAsignacionGarantia_Registro_ObtenerGarantias")]
        public ErrorDto<List<VivControlRegistroGarantiaData>> VivControlAsignacionGarantia_Registro_ObtenerGarantias(int codEmpresa, long idContacto, string tipoProfesional)
        {
            return _bl.VivControlAsignacionGarantia_Registro_ObtenerGarantias(codEmpresa, idContacto, tipoProfesional);
        }

        [HttpPost("VivControlAsignacionGarantia_Registro_Aplicar")]
        public ErrorDto VivControlAsignacionGarantia_Registro_Aplicar(int codEmpresa, string usuario, VivControlRegistroGarantiaRequest request)
        {
            return _bl.VivControlAsignacionGarantia_Registro_Aplicar(codEmpresa, usuario, request);
        }

        [HttpGet("VivControlAsignacionGarantia_ObtenerTiemposSeguimiento")]
        public ErrorDto<VivControlTiemposSeguimientoData> VivControlAsignacionGarantia_ObtenerTiemposSeguimiento(int codEmpresa, string profesional)
        {
            return _bl.VivControlAsignacionGarantia_ObtenerTiemposSeguimiento(codEmpresa, profesional);
        }

        [HttpGet("VivControlAsignacionGarantia_Asignacion_ValidaHonorariosRegistra")]
        public ErrorDto<VivControlHonorariosRegistraData> VivControlAsignacionGarantia_Asignacion_ValidaHonorariosRegistra(int codEmpresa, int idGarantia)
        {
            return _bl.VivControlAsignacionGarantia_Asignacion_ValidaHonorariosRegistra(codEmpresa, idGarantia);
        }
    }
}