using Galileo.BusinessLogic;
using Galileo.Models.ERROR;
using Galileo.Models.INV;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public sealed class FrmInvTipoEsController : ControllerBase
    {
        private readonly FrmInvTipoEsBl _bl;

        public FrmInvTipoEsController(
            IConfiguration config)
        {
            ArgumentNullException.ThrowIfNull(config);

            _bl = new FrmInvTipoEsBl(config);
        }

        [HttpGet("INV_TipoES_Lista_Obtener")]
        public ErrorDto<TipoESList> INV_TipoES_Lista_Obtener(
            int CodEmpresa,
            int CodContabilidad,
            string? filtros)
        {
            return _bl.INV_TipoES_Lista_Obtener(
                CodEmpresa,
                CodContabilidad,
                filtros);
        }

        [HttpGet("INV_TipoES_Tipo_Buscar")]
        public ErrorDto<List<TipoEsDto>> INV_TipoES_Tipo_Buscar(
            int CodEmpresa,
            int CodContabilidad,
            string? tipo)
        {
            return _bl.INV_TipoES_Tipo_Buscar(
                CodEmpresa,
                CodContabilidad,
                tipo);
        }

        [HttpPost("INV_TipoES_Registrar")]
        public ErrorDto INV_TipoES_Registrar(
            int CodEmpresa,
            TipoEsGuardarRequest? request)
        {
            return _bl.INV_TipoES_Registrar(
                CodEmpresa,
                request);
        }

        [HttpPost("INV_TipoES_Actualizar")]
        public ErrorDto INV_TipoES_Actualizar(
            int CodEmpresa,
            TipoEsGuardarRequest? request)
        {
            return _bl.INV_TipoES_Actualizar(
                CodEmpresa,
                request);
        }

        [HttpDelete("INV_TipoES_Eliminar")]
        public ErrorDto INV_TipoES_Eliminar(
            int CodEmpresa,
            TipoEsEliminarRequest? request)
        {
            return _bl.INV_TipoES_Eliminar(
                CodEmpresa,
                request);
        }
    }
}