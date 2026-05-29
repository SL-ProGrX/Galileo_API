using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic.ProGrX.Clientes;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Clientes;

namespace Galileo.Controllers.ProGrX.Clientes
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmAFCrRenunciasTagsController : ControllerBase
    {
        private readonly FrmAfCrRenunciasTagsBL _bl;

        public FrmAFCrRenunciasTagsController(IConfiguration config)
        {
            _bl = new FrmAfCrRenunciasTagsBL(config);
        }

        [Authorize]
        [HttpGet("AF_CR_Renuncias_Tags_Obtener")]
        public ErrorDto<List<AfCrRenunciasTagsData>> AF_CR_Renuncias_Tags_Obtener(int CodEmpresa, string Estado, string? Filtro)
        {
            return _bl.AF_CR_Renuncias_Tags_Obtener(CodEmpresa, Estado, Filtro ?? string.Empty);
        }

        [Authorize]
        [HttpPost("AF_CR_Renuncia_Recepcion_Aplica")]
        public ErrorDto AF_CR_Renuncia_Recepcion_Aplica(int CodEmpresa, [FromBody] AfCrRenunciaRecepcionAplica recepcionDatos)
        {
            return _bl.AF_CR_Renuncia_Recepcion_Aplica(CodEmpresa, recepcionDatos);
        }

        [Authorize]
        [HttpPost("AF_CR_Renuncia_Revision_Aplica")]
        public ErrorDto AF_CR_Renuncia_Revision_Aplica(int CodEmpresa, [FromBody] AfCrRenunciaRevisionAplica revisionDatos)
        {
            return _bl.AF_CR_Renuncia_Revision_Aplica(CodEmpresa, revisionDatos);
        }

        [Authorize]
        [HttpGet("AF_CR_Renuncia_Etiquetas_Consulta")]
        public ErrorDto<List<AfCrRenunciaEtiquetas>> AF_CR_Renuncia_Etiquetas_Consulta(int CodEmpresa, int RenunciaId)
        {
            return _bl.AF_CR_Renuncia_Etiquetas_Consulta(CodEmpresa, RenunciaId);
        }

        [Authorize]
        [HttpGet("AF_CR_Renuncia_Revision_Reversar_Valida")]
        public ErrorDto<int> AF_CR_Renuncia_Revision_Reversar_Valida(int CodEmpresa, int RenunciaId)
        {
            return _bl.AF_CR_Renuncia_Revision_Reversar_Valida(CodEmpresa, RenunciaId);
        }

        [Authorize]
        [HttpPost("AF_CR_Renuncia_Revision_Reversar")]
        public ErrorDto AF_CR_Renuncia_Revision_Reversar(int CodEmpresa, [FromBody] AfCrRenunciaReversa dto)
        {
            return _bl.AF_CR_Renuncia_Revision_Reversar(CodEmpresa, dto);
        }

        [Authorize]
        [HttpGet("AF_CR_Renuncias_Pendientes_Obtener")]
        public ErrorDto<List<AfCrRenunciasTagsData>> AF_CR_Renuncias_Pendientes_Obtener(int CodEmpresa)
        {
            return _bl.AF_CR_Renuncias_Pendientes_Obtener(CodEmpresa);
        }

        [Authorize]
        [HttpGet("AF_CR_Renuncias_Obtener")]
        public ErrorDto<List<AfCrRenunciasTagsData>> AF_CR_Renuncias_Obtener(int CodEmpresa)
        {
            return _bl.AF_CR_Renuncias_Obtener(CodEmpresa);
        }
    }
}