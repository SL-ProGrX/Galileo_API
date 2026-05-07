using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX_Hipotecario;
using Galileo_API.Models.ProGrX_Hipotecario;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX_Hipotecario
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmPreaSeguimientoEtiquetasController : ControllerBase
    {
        private readonly FrmPreaSeguimientoEtiquetasBL BL;

        public FrmPreaSeguimientoEtiquetasController(IConfiguration config)
        {
            BL = new FrmPreaSeguimientoEtiquetasBL(config);
        }
        [Authorize]
        [HttpGet("Prea_SeguimientoEtiquetas_Info_Obtener")]
        public ErrorDto<PreaSeguimientoEtiquetasInfoDto> Prea_SeguimientoEtiquetas_Info_Obtener(int CodEmpresa,int idSolicitud,string? codPreanalisis)
        {
            return BL.Prea_SeguimientoEtiquetas_Info_Obtener(CodEmpresa, idSolicitud, codPreanalisis);
        }
        [Authorize]
        [HttpGet("Prea_SeguimientoEtiquetas_Lista_Obtener")]
        public ErrorDto<PreaSeguimientoEtiquetasLista> Prea_SeguimientoEtiquetas_Lista_Obtener(int CodEmpresa,int idSolicitud,string? codPreanalisis)
        {
            return BL.Prea_SeguimientoEtiquetas_Lista_Obtener(CodEmpresa, idSolicitud, codPreanalisis);
        }

        [Authorize]
        [HttpGet("Prea_SeguimientoEtiquetas_Etiquetas_Dropdown_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> Prea_SeguimientoEtiquetas_Etiquetas_Dropdown_Obtener(int CodEmpresa,string usuario)
        {
            return BL.Prea_SeguimientoEtiquetas_Etiquetas_Dropdown_Obtener(CodEmpresa, usuario);
        }

        [Authorize]
        [HttpPost("Prea_SeguimientoEtiquetas_Aplicar")]
        public ErrorDto Prea_SeguimientoEtiquetas_Aplicar(int CodEmpresa,string usuario,[FromBody] PreaSeguimientoEtiquetasAplicarDto data)
        {
            if (data == null)
            {
                return new ErrorDto
                {
                    Code = -2,
                    Description = "Debe indicar la información a registrar."
                };
            }

            return BL.Prea_SeguimientoEtiquetas_Aplicar(CodEmpresa, data, usuario);
        }
    }
}