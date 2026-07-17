using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX.Creditos;
using Galileo_API.Models.ProGrX.Creditos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX.Creditos
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FrmCrSeguimientoEtiquetasController : ControllerBase
    {
        private readonly FrmCrSeguimientoEtiquetasBl _bl;

        public FrmCrSeguimientoEtiquetasController(IConfiguration config)
        {
            _bl = new FrmCrSeguimientoEtiquetasBl(config);
        }

        [Authorize]
        [HttpGet("Cr_SeguimientoEtiquetas_Lista_Obtener")]
        public ErrorDto<List<CrSeguimientoEtiquetasData>> Cr_SeguimientoEtiquetas_Lista_Obtener(int codEmpresa, int idSolicitud)
        {
            return _bl.Cr_SeguimientoEtiquetas_Lista_Obtener(codEmpresa, idSolicitud);
        }

        [Authorize]
        [HttpGet("Cr_SeguimientoEtiquetas_Etiquetas_Dropdown_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> Cr_SeguimientoEtiquetas_Etiquetas_Dropdown_Obtener(int codEmpresa, string usuario)
        {
            return _bl.Cr_SeguimientoEtiquetas_Etiquetas_Dropdown_Obtener(codEmpresa, usuario);
        }

        [Authorize]
        [HttpPost("Cr_SeguimientoEtiquetas_Aplicar")]
        public ErrorDto Cr_SeguimientoEtiquetas_Aplicar(int codEmpresa, CrSeguimientoEtiquetasAplicarRequest request)
        {
            return _bl.Cr_SeguimientoEtiquetas_Aplicar(codEmpresa, request);
        }

        [Authorize]
        [HttpGet("Cr_SeguimientoEtiquetas_NotaLargo_Obtener")]
        public ErrorDto<int> Cr_SeguimientoEtiquetas_NotaLargo_Obtener(int codEmpresa, string tagCodigo)
        {
            return _bl.Cr_SeguimientoEtiquetas_NotaLargo_Obtener(codEmpresa, tagCodigo);
        }
    }
}
