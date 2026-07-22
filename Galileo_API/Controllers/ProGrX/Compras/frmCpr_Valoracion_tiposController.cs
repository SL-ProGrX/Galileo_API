using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic;
using Galileo.Models.CPR;
using Galileo.Models.ERROR;
using Microsoft.AspNetCore.Authorization;

namespace Galileo.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class FrmCprValoraciontiposController : ControllerBase
    {
        private readonly FrmCprValoracionTiposBL _bl;
        public FrmCprValoraciontiposController(IConfiguration config)
        {
            _bl = new FrmCprValoracionTiposBL(config);
        }

        [HttpGet("EsquemaValoracion_Obtener")]
        public ErrorDto<CprValoraEsquemaDtoList> CPR_frmCpr_Valoracion_Tipos_EsquemaValoracion_Obtener(
            int CodEmpresa,
            [FromQuery] CprValoraConsultaRequest request)
        {
            return _bl.CPR_frmCpr_Valoracion_Tipos_EsquemaValoracion_Obtener(CodEmpresa, request);
        }

        [HttpGet("ValoracionItems_Obtener")]
        public ErrorDto<CprValoraItemsDtoList> CPR_frmCpr_Valoracion_Tipos_ValoracionItems_Obtener(
            int CodEmpresa,
            string val_id,
            [FromQuery] CprValoraConsultaRequest request)
        {
            return _bl.CPR_frmCpr_Valoracion_Tipos_ValoracionItems_Obtener(CodEmpresa, val_id, request);
        }

        [HttpPost("EsquemaValoracion_Upsert")]
        public ErrorDto EsquemaValoracion_Upsert(int CodEmpresa, string usuario, CprValoraEsquemaDto request)
        {
            return _bl.EsquemaValoracion_Upsert(CodEmpresa, usuario, request);
        }

        [HttpPost("ValoracionItems_Upsert")]
        public ErrorDto ValoracionItems_Upsert(int CodEmpresa, string usuario, string val_id, CprValoraItemsDto request)
        {
            return _bl.ValoracionItems_Upsert(CodEmpresa, usuario, val_id, request);
        }

        [HttpDelete("EsquemaValoracion_Delete")]
        public ErrorDto EsquemaValoracion_Delete(int CodEmpresa, string val_id)
        {
            return _bl.EsquemaValoracion_Delete(CodEmpresa, val_id);
        }

        [HttpDelete("ValoracionItems_Delete")]
        public ErrorDto ValoracionItems_Delete(int CodEmpresa, string val_id, string val_item)
        {
            return _bl.ValoracionItems_Delete(CodEmpresa, val_id, val_item);
        }
    }
}
