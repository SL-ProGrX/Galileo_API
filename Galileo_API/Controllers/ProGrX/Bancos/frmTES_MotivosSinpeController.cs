using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using PgxAPI.BusinessLogic.ProGrX.Bancos;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Bancos;

namespace PgxAPI.Controllers.ProGrX.Bancos
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FrmTesMotivosSinpeController : ControllerBase
    {
        private readonly FrmTesMotivosSinpeBL _bl;

        public FrmTesMotivosSinpeController(IConfiguration config)
        {
            _bl = new FrmTesMotivosSinpeBL(config);
        }

       
        [HttpGet("TES_MotivoSinpe_Obtener")]
        public ErrorDto<TesMotivosSinpeLista> TES_MotivoSinpe_Obtener(int CodEmpresa, string filtros)
        {
            return _bl.TES_MotivoSinpe_Obtener(CodEmpresa, filtros);
        }

        [HttpGet("TES_MotivoSinpeExportar_Obtener")]
        public ErrorDto<List<TesMotivosSinpeDto>> TES_MotivoSinpeExportar_Obtener(int CodEmpresa, string filtros)
        {
            return _bl.TES_MotivoSinpeExportar_Obtener(CodEmpresa, filtros);
        }

        [HttpPost("TES_MotivoSinpe_Guardar")]
        public ErrorDto TES_MotivoSinpe_Guardar(int CodEmpresa, string usuario, TesMotivosSinpeDto motivo)
        {
            return _bl.TES_MotivoSinpe_Guardar(CodEmpresa, usuario, motivo);
        }

        [HttpDelete("TES_MotivoSinpe_Eliminar")]
        public ErrorDto TES_MotivoSinpe_Eliminar(int CodEmpresa, string usuario, int cod_motivo)
        {
            return _bl.TES_MotivoSinpe_Eliminar(CodEmpresa, usuario, cod_motivo);
        }

    }
}
