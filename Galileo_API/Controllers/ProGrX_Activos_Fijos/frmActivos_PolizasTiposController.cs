using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Activos_Fijos;
using Galileo.BusinessLogic.ProGrX_Activos_Fijos;

namespace Galileo.Controllers.ProGrX_Activos_Fijos
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmActivosPolizasTiposController : ControllerBase
    {
        private readonly FrmActivosPolizasTiposBL _bl;

        public FrmActivosPolizasTiposController(IConfiguration config)
        {
            _bl = new FrmActivosPolizasTiposBL(config);
        }


        [Authorize]
        [HttpGet("Activos_PolizasTiposLista_Obtener")]
        public ErrorDto<ActivosPolizasTiposLista> Activos_PolizasTiposLista_Obtener(int CodEmpresa, string filtros)
        {
            return _bl.Activos_PolizasTiposLista_Obtener(CodEmpresa, filtros);
        }

        [Authorize]
        [HttpGet("Activos_PolizasTipos_Obtener")]
        public ErrorDto<List<ActivosPolizasTiposData>> Activos_PolizasTipos_Obtener(int CodEmpresa, string filtros)
        {
            return _bl.Activos_PolizasTipos_Obtener(CodEmpresa, filtros);
        }

        [Authorize]
        [HttpPost("Activos_PolizasTipos_Guardar")]
        public ErrorDto Activos_PolizasTipos_Guardar(int CodEmpresa, string usuario, ActivosPolizasTiposData tipoPoliza)
        {
            return _bl.Activos_PolizasTipos_Guardar(CodEmpresa, usuario, tipoPoliza);
        }

        [Authorize]
        [HttpDelete("Activos_PolizasTipos_Eliminar")]
        public ErrorDto Activos_PolizasTipos_Eliminar(int CodEmpresa, string usuario, string tipo_poliza)
        {
            return _bl.Activos_PolizasTipos_Eliminar(CodEmpresa, usuario, tipo_poliza);
        }

        [Authorize]
        [HttpGet("Activos_PolizasTipos_Valida")]
        public ErrorDto Activos_PolizasTipos_Valida(int CodEmpresa, string tipo_poliza)
        {
            return _bl.Activos_PolizasTipos_Valida(CodEmpresa, tipo_poliza);
        }

    }
}