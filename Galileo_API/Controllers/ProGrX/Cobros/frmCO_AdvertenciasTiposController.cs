using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Galileo.BusinessLogic.ProGrX.Cobros;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Cobros;

namespace Galileo.Controllers.ProGrX.Cobros
{
    [Route("api/[controller]")]
    [ApiController]

    public class FrmCoAdvertenciasTiposController : ControllerBase
    {
        private readonly FrmCoAdvertenciasTiposBL _bl;

        public FrmCoAdvertenciasTiposController(IConfiguration config)
        {
            _bl = new FrmCoAdvertenciasTiposBL(config);
        }

        [Authorize]
        [HttpGet("CoAdvertenciasTipos_Obtener")]
        public ErrorDto<CoAdvertenciasTiposLista> CoAdvertenciasTipos_Obtener(int CodEmpresa, string filtros)
        { 
            return _bl.CoAdvertenciasTipos_Obtener(CodEmpresa, filtros);
        }

        [Authorize]
        [HttpPost("CoAdvertenciasTipos_Guardar")]
        public ErrorDto CoAdvertenciasTipos_Guardar(int CodEmpresa, string usuario, CoAdvertenciasTiposData request)
        {
            return _bl.CoAdvertenciasTipos_Guardar(CodEmpresa, usuario, request);
        }

        [Authorize]
        [HttpDelete("CoAdvertenciasTipos_Delete")]
        public ErrorDto CoAdvertenciasTipos_Delete(int CodEmpresa, string usuario, string cod_advertencia)
        {
            return _bl.CoAdvertenciasTipos_Delete(CodEmpresa, usuario, cod_advertencia);
        }

    }
}
