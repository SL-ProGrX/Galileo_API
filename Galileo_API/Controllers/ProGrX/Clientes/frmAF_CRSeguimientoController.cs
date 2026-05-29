using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic.ProGrX.Clientes;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Clientes;
using Galileo.Models;

namespace Galileo.Controllers.ProGrX.Clientes
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmAFCrSeguimientoController : ControllerBase
    {
        private readonly FrmAFCrSeguimientoBL _bl;

        public FrmAFCrSeguimientoController(IConfiguration config)
        {
            _bl = new FrmAFCrSeguimientoBL(config);
        }

        [Authorize]
        [HttpPost("AF_CR_Seguimiento_Obtener")]
        public ErrorDto<List<AfCrSeguimientoData>> AF_CR_Seguimiento_Obtener(int CodEmpresa, [FromBody] AfCrSeguimientoFiltros filtros)
        {
            return _bl.AF_CR_Seguimiento_Obtener(CodEmpresa, filtros);
        }

        [Authorize]
        [HttpGet("AF_CR_Seguimiento_Obtener_Gestiones")]
        public ErrorDto<List<DropDownListaGenericaModel>> AF_CR_Seguimiento_Obtener_Gestiones(int CodEmpresa)
        {
            return _bl.AF_CR_Seguimiento_Obtener_Gestiones(CodEmpresa);
        }

        [Authorize]
        [HttpGet("AF_CR_Seguimiento_Obtener_Causas")]
        public ErrorDto<List<DropDownListaGenericaModel>> AF_CR_Seguimiento_Obtener_Causas(int CodEmpresa)
        {
            return _bl.AF_CR_Seguimiento_Obtener_Causas(CodEmpresa);
        }

        [Authorize]
        [HttpGet("AF_CR_Seguimiento_Obtener_Institucion")]
        public ErrorDto<List<DropDownListaGenericaModel>> AF_CR_Seguimiento_Obtener_Institucion(int CodEmpresa)
        {
            return _bl.AF_CR_Seguimiento_Obtener_Institucion(CodEmpresa);
        }

        [Authorize]
        [HttpGet("AF_CR_Seguimiento_Obtener_Provincia")]
        public ErrorDto<List<DropDownListaGenericaModel>> AF_CR_Seguimiento_Obtener_Provincia(int CodEmpresa)
        {
            return _bl.AF_CR_Seguimiento_Obtener_Provincia(CodEmpresa);
        }

        [Authorize]
        [HttpGet("AF_CR_Seguimiento_Obtener_Zona")]
        public ErrorDto<List<DropDownListaGenericaModel>> AF_CR_Seguimiento_Obtener_Zona(int CodEmpresa)
        {
            return _bl.AF_CR_Seguimiento_Obtener_Zona(CodEmpresa);
        }

        [Authorize]
        [HttpGet("AF_CR_Seguimiento_Obtener_Detalle_Renuncia")]
        public ErrorDto<AfCrSeguimientoDetalle?> AF_CR_Seguimiento_Obtener_Detalle_Renuncia(int CodEmpresa, int codRenuncia)
        {
            return _bl.AF_CR_Seguimiento_Obtener_Detalle_Renuncia(CodEmpresa, codRenuncia);
        }

        [Authorize]
        [HttpGet("AF_CR_Seguimiento_Obtener_Motivos")]
        public ErrorDto<List<AfCrSeguimientoMotivo>> AF_CR_Seguimiento_Obtener_Motivos(int CodEmpresa, int renunciaId)
        {
            return _bl.AF_CR_Seguimiento_Obtener_Motivos(CodEmpresa, renunciaId);
        }

        [Authorize]
        [HttpGet("AF_CR_Seguimiento_Obtener_Historial")]
        public ErrorDto<List<AfCrSeguimientoHistorial>> AF_CR_Seguimiento_Obtener_Historial(int CodEmpresa, int codRenuncia)
        {
            return _bl.AF_CR_Seguimiento_Obtener_Historial(CodEmpresa, codRenuncia);
        }

        [Authorize]
        [HttpGet("AF_CR_Seguimiento_Obtener_Gestion")]
        public ErrorDto<List<DropDownListaGenericaModel>> AF_CR_Seguimiento_Obtener_Gestion(int CodEmpresa)
        {
            return _bl.AF_CR_Seguimiento_Obtener_Gestion(CodEmpresa);
        }

        [Authorize]
        [HttpPost("AF_CR_Seguimiento_Motivos_Registrar")]
        public ErrorDto AF_CR_Seguimiento_Motivos_Registrar(int CodEmpresa, [FromBody] AfCrSeguimientoMotivosRegistrar motivos)
        {
            return _bl.AF_CR_Seguimiento_Motivos_Registrar(CodEmpresa, motivos);
        }

        [Authorize]
        [HttpPost("AF_CR_Seguimiento_Renuncia_Estado")]
        public ErrorDto AF_CR_Seguimiento_Renuncia_Estado(int CodEmpresa, [FromBody] AfCrSeguimientoRenunciaEstado estado)
        {
            return _bl.AF_CR_Seguimiento_Renuncia_Estado(CodEmpresa, estado);
        }
    }
}