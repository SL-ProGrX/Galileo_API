using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX.Creditos;
using Galileo_API.Models.ProGrX.Creditos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX.Creditos
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FrmCrSeguimientoTramitesController : ControllerBase
    {
        private readonly FrmCrSeguimientoTramitesBl _bl;

        public FrmCrSeguimientoTramitesController(IConfiguration config)
        {
            _bl = new FrmCrSeguimientoTramitesBl(config);
        }

        [HttpGet("Cr_SeguimientoTramites_Inicializar")]
        public ErrorDto<CrSeguimientoTramitesInicializarData> Cr_SeguimientoTramites_Inicializar(
            int codEmpresa,
            string usuario)
            => _bl.Cr_SeguimientoTramites_Inicializar(codEmpresa, usuario);

        [HttpGet("Cr_SeguimientoTramites_Buscar")]
        public ErrorDto<List<CrSeguimientoTramitesBusquedaItem>> Cr_SeguimientoTramites_Buscar(
            int codEmpresa,
            string? cedula,
            string? nombre)
            => _bl.Cr_SeguimientoTramites_Buscar(codEmpresa, cedula, nombre);

        [HttpGet("Cr_SeguimientoTramites_Operacion_Obtener")]
        public ErrorDto<CrSeguimientoTramitesOperacionData> Cr_SeguimientoTramites_Operacion_Obtener(
            int codEmpresa,
            int operacion)
            => _bl.Cr_SeguimientoTramites_Operacion_Obtener(codEmpresa, operacion);

        [HttpPost("Cr_SeguimientoTramites_Recepcion_Guardar")]
        public ErrorDto<CrSeguimientoTramitesRecepcionGuardarResult>
            Cr_SeguimientoTramites_Recepcion_Guardar(
                int codEmpresa,
                CrSeguimientoTramitesRecepcionGuardarRequest request)
            => _bl.Cr_SeguimientoTramites_Recepcion_Guardar(codEmpresa, request);

        [HttpGet("Cr_SeguimientoTramites_Recepcion_Socios_Buscar")]
        public ErrorDto<List<CrSeguimientoTramitesRecepcionSocioItem>>
            Cr_SeguimientoTramites_Recepcion_Socios_Buscar(
                int codEmpresa,
                string? filtro)
            => _bl.Cr_SeguimientoTramites_Recepcion_Socios_Buscar(codEmpresa, filtro);

        [HttpGet("Cr_SeguimientoTramites_Recepcion_Lineas_Buscar")]
        public ErrorDto<List<CrSeguimientoTramitesRecepcionLineaItem>>
            Cr_SeguimientoTramites_Recepcion_Lineas_Buscar(
                int codEmpresa,
                string? filtro)
            => _bl.Cr_SeguimientoTramites_Recepcion_Lineas_Buscar(codEmpresa, filtro);

        [HttpGet("Cr_SeguimientoTramites_Recepcion_Promotores_Buscar")]
        public ErrorDto<List<CrSeguimientoTramitesRecepcionPromotorItem>>
            Cr_SeguimientoTramites_Recepcion_Promotores_Buscar(
                int codEmpresa,
                string? filtro)
            => _bl.Cr_SeguimientoTramites_Recepcion_Promotores_Buscar(codEmpresa, filtro);

        [HttpGet("Cr_SeguimientoTramites_Recepcion_Proveedores_Buscar")]
        public ErrorDto<List<CrSeguimientoTramitesRecepcionProveedorItem>>
            Cr_SeguimientoTramites_Recepcion_Proveedores_Buscar(
                int codEmpresa,
                string? filtro)
            => _bl.Cr_SeguimientoTramites_Recepcion_Proveedores_Buscar(codEmpresa, filtro);

        [HttpGet("Cr_SeguimientoTramites_Recepcion_Linea_Contexto_Obtener")]
        public ErrorDto<CrSeguimientoTramitesRecepcionLineaContextoData>
            Cr_SeguimientoTramites_Recepcion_Linea_Contexto_Obtener(
                int codEmpresa,
                [FromQuery] CrSeguimientoTramitesRecepcionLineaContextoRequest request)
            => _bl.Cr_SeguimientoTramites_Recepcion_Linea_Contexto_Obtener(codEmpresa, request);

        [HttpGet("Cr_SeguimientoTramites_Recepcion_Garantia_Contexto_Obtener")]
        public ErrorDto<CrSeguimientoTramitesRecepcionGarantiaContextoData>
            Cr_SeguimientoTramites_Recepcion_Garantia_Contexto_Obtener(
                int codEmpresa,
                [FromQuery] CrSeguimientoTramitesRecepcionGarantiaContextoRequest request)
            => _bl.Cr_SeguimientoTramites_Recepcion_Garantia_Contexto_Obtener(codEmpresa, request);

        [HttpGet("Cr_SeguimientoTramites_Recepcion_Banco_Cuentas_Obtener")]
        public ErrorDto<List<CrSeguimientoTramitesOpcionItem>>
            Cr_SeguimientoTramites_Recepcion_Banco_Cuentas_Obtener(
                int codEmpresa,
                [FromQuery] CrSeguimientoTramitesRecepcionBancoCuentasRequest request)
            => _bl.Cr_SeguimientoTramites_Recepcion_Banco_Cuentas_Obtener(codEmpresa, request);

        [HttpGet("Cr_SeguimientoTramites_Recepcion_Fondo_Contexto_Obtener")]
        public ErrorDto<CrSeguimientoTramitesRecepcionFondoContextoData>
            Cr_SeguimientoTramites_Recepcion_Fondo_Contexto_Obtener(
                int codEmpresa,
                [FromQuery] CrSeguimientoTramitesRecepcionFondoContextoRequest request)
            => _bl.Cr_SeguimientoTramites_Recepcion_Fondo_Contexto_Obtener(codEmpresa, request);
    }
}
