using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic;
using Galileo.Models.ERROR;
using Galileo.Models.INV;
using Microsoft.AspNetCore.Authorization;

namespace Galileo.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class FrmInvTipoESController : ControllerBase
    {
        private readonly FrmInvTipoEsBL _bl;
        public FrmInvTipoESController(IConfiguration config)
        {
            _bl = new FrmInvTipoEsBL(config);
        }

        [HttpGet("TiposES_Obtener")]
        public ErrorDto<TipoESList> TipoES_Obtener(int CodEmpresa, string filtros)
        {
            return _bl.TipoES_Obtener(CodEmpresa, filtros);
        }

        [HttpGet("TipoES_Buscar")]
        public ErrorDto<List<TipoEsDto>> TipoES_Buscar(int CodEmpresa, string Tipo)
        {
            return _bl.TipoES_Buscar(CodEmpresa, Tipo);
        }

        [HttpPost("TipoES_Insertar")]
        public ErrorDto TipoES_Insertar(int CodEmpresa, TipoEsDto request)
        {
            return _bl.TipoES_Insertar(CodEmpresa, request);
        }

        [HttpPost("TipoES_Actualizar")]
        public ErrorDto TipoES_Actualizar(int CodEmpresa, TipoEsDto request)
        {
            return _bl.TipoES_Actualizar(CodEmpresa, request);
        }

        [HttpPost("TipoES_Eliminar")]
        public ErrorDto TipoES_Eliminar(int CodEmpresa, string codTipoES)
        {
            return _bl.TipoES_Eliminar(CodEmpresa, codTipoES);
        }
    }
}