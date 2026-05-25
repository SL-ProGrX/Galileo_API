using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX.Creditos;
using Galileo_API.Models.ProGrX.Credito;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX.Creditos
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FrmCrEtiquetasController : ControllerBase
    {
        private readonly FrmCrEtiquetasBl _bl;

        public FrmCrEtiquetasController(IConfiguration config)
        {
            _bl = new FrmCrEtiquetasBl(config);
        }

        [HttpGet("CrEtiquetas_Obtener")]
        public ErrorDto<List<CrEtiquetaData>> CrEtiquetas_Obtener(int codEmpresa)
            => _bl.CrEtiquetas_Obtener(codEmpresa);

        [HttpGet("CrEtiquetas_Requisitos_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CrEtiquetas_Requisitos_Obtener(int codEmpresa)
            => _bl.CrEtiquetas_Requisitos_Obtener(codEmpresa);

        [HttpGet("CrEtiquetas_TagsCombo_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CrEtiquetas_TagsCombo_Obtener(int codEmpresa)
            => _bl.CrEtiquetas_TagsCombo_Obtener(codEmpresa);

        [HttpGet("CrEtiquetas_Notificacion_Obtener")]
        public ErrorDto<CrEtiquetaNotificacionData> CrEtiquetas_Notificacion_Obtener(int codEmpresa, string tag_codigo)
            => _bl.CrEtiquetas_Notificacion_Obtener(codEmpresa, tag_codigo);

        [HttpPost("CrEtiquetas_Guardar")]
        public ErrorDto CrEtiquetas_Guardar(int codEmpresa, CrEtiquetaGuardarRequest request)
            => _bl.CrEtiquetas_Guardar(codEmpresa, request);

        [HttpDelete("CrEtiquetas_Eliminar")]
        public ErrorDto CrEtiquetas_Eliminar(int codEmpresa, CrEtiquetaEliminarRequest request)
            => _bl.CrEtiquetas_Eliminar(codEmpresa, request);

        [HttpPost("CrEtiquetas_Notificacion_Guardar")]
        public ErrorDto CrEtiquetas_Notificacion_Guardar(int codEmpresa, CrEtiquetaNotificacionGuardarRequest request)
            => _bl.CrEtiquetas_Notificacion_Guardar(codEmpresa, request);

        [HttpDelete("CrEtiquetas_Notificacion_Eliminar")]
        public ErrorDto CrEtiquetas_Notificacion_Eliminar(int codEmpresa, CrEtiquetaNotificacionEliminarRequest request)
            => _bl.CrEtiquetas_Notificacion_Eliminar(codEmpresa, request);
    }
}