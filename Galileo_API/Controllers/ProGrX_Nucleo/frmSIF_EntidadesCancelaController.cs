using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic.ProGrX_Nucleo;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Nucleo;

namespace Galileo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmSifEntidadesCancelaController : ControllerBase
    {
        private readonly FrmSifEntidadesCancelaBL _bl;
        public FrmSifEntidadesCancelaController(IConfiguration config)
        {
            _bl = new FrmSifEntidadesCancelaBL(config);
        }
        
        [Authorize]
        [HttpGet("Sif_EntidadesCancelaLista_Obtener")]
        public ErrorDto<SifEntidadesCancelaLista> Sif_EntidadesCancelaLista_Obtener(int CodEmpresa, string filtros)
        {
            return _bl.SIF_EntidadesCancelaLista_Obtener(CodEmpresa, filtros);
        }

        [Authorize]
        [HttpGet("Sif_EntidadesCancela_Obtener")]
        public ErrorDto<List<SifEntidadesCancelaData>> Sif_EntidadesCancela_Obtener(int CodEmpresa, string filtros)
        {
            return _bl.SIF_EntidadesCancela_Obtener(CodEmpresa, filtros);
        }

        [Authorize]
        [HttpPost("Sif_EntidadesCancela_Guardar")]
        public ErrorDto Sif_EntidadesCancela_Guardar(int CodEmpresa, string usuario, SifEntidadesCancelaData entidad)
        {
            return _bl.SIF_EntidadesCancela_Guardar(CodEmpresa, usuario, entidad);
        }

        [Authorize]
        [HttpDelete("Sif_EntidadesCancela_Eliminar")]
        public ErrorDto Sif_EntidadesCancela_Eliminar(int CodEmpresa, string entidad, string usuario)
        {
            return _bl.SIF_EntidadesCancela_Eliminar(CodEmpresa, entidad, usuario);
        }

        [Authorize]
        [HttpGet("Sif_EntidadesCancela_Valida")]
        public ErrorDto Sif_EntidadesCancela_Valida(int CodEmpresa, string cod_entidad_pago)
        {
            return _bl.SIF_EntidadesCancela_Valida(CodEmpresa, cod_entidad_pago);
        }
    }
}