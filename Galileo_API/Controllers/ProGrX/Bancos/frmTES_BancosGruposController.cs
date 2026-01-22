using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Bancos;
using Galileo_API.BusinessLogic.ProGrX.Bancos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX.Bancos
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FrmTesBancosGruposController : ControllerBase
    {
        private readonly FrmTesBancosGruposBL _bancosGruposBL;

        public FrmTesBancosGruposController(IConfiguration config)
        {
            _bancosGruposBL = new FrmTesBancosGruposBL(config);
        }

        
        [HttpGet("Tes_BancosGruposLista_Obtener")]
        public ErrorDto<TesBancosGruposLista> Tes_BancosGruposLista_Obtener(int CodEmpresa, string filtros)
        {
            return _bancosGruposBL.Tes_BancosGruposLista_Obtener(CodEmpresa, filtros);
        }

        [HttpGet("Tes_BancosGruposExportar_Obtener")]
        public ErrorDto<List<TesBancosGruposData>> Tes_BancosGruposExportar_Obtener(int CodEmpresa)
        {
            return _bancosGruposBL.Tes_BancosGruposExportar_Obtener(CodEmpresa);
        }

        [HttpPost("Tes_BancoGrupoFirma_Guardar")]
        public ErrorDto Tes_BancoGrupoFirma_Guardar(TesBancosGruposImgData firma)
        {
            return _bancosGruposBL.Tes_BancoGrupoFirma_Guardar(firma);
        }

        [HttpPost("Tes_BancosGrupo_Guardar")]
        public ErrorDto Tes_BancosGrupo_Guardar(int CodEmpresa, TesBancosGruposData banco)
        {
            return _bancosGruposBL.Tes_BancosGrupo_Guardar(CodEmpresa, banco);
        }

        [HttpDelete("Tes_BancoGrupo_Eliminar")]
        public ErrorDto Tes_BancoGrupo_Eliminar(int CodEmpresa, string cod_grupo)
        {
            return _bancosGruposBL.Tes_BancoGrupo_Eliminar(CodEmpresa, cod_grupo);
        }

        [HttpGet("Tes_BancosGrupo_Valida")]
        public ErrorDto Tes_BancosGrupo_Valida(int CodEmpresa, string cod_grupo)
        {
            return _bancosGruposBL.Tes_BancosGrupo_Valida(CodEmpresa, cod_grupo);
        }
    }
}
