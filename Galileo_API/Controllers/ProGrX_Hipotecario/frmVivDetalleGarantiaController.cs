using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX_Hipotecario;
using Galileo_API.Models.ProGrX_Hipotecario;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX_Hipotecario
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmVivDetalleGarantiaController : ControllerBase
    {
        private readonly FrmVivDetalleGarantiaBL BL;

        public FrmVivDetalleGarantiaController(IConfiguration config)
        {
            BL = new FrmVivDetalleGarantiaBL(config);
        }

        [Authorize]
        [HttpGet("Viv_DetalleGarantia_Lista_Obtener")]
        public ErrorDto<VivDetalleGarantiaLista> Viv_DetalleGarantia_Lista_Obtener(int CodEmpresa, int idGarantia, short linea = -1)
        {
            return BL.Viv_DetalleGarantia_Lista_Obtener(CodEmpresa, idGarantia, linea);
        }

        [Authorize]
        [HttpGet("Viv_DetalleGarantia_Grados_Dropdown_Obtener")]
        public ErrorDto<List<VivDetalleGarantiaGradoItem>> Viv_DetalleGarantia_Grados_Dropdown_Obtener(int CodEmpresa, string descGradoHipoteca)
        {
            return BL.Viv_DetalleGarantia_Grados_Dropdown_Obtener(CodEmpresa, descGradoHipoteca);
        }

        [Authorize]
        [HttpPost("Viv_DetalleGarantia_Guardar")]
        public ErrorDto Viv_DetalleGarantia_Guardar(int CodEmpresa, string usuario, [FromBody] VivDetalleGarantiaGuardarDto data)
        {
            if (data == null)
            {
                return new ErrorDto
                {
                    Code = -2,
                    Description = "Debe indicar la información a guardar."
                };
            }

            return BL.Viv_DetalleGarantia_Guardar(CodEmpresa, data, usuario);
        }

        [Authorize]
        [HttpPost("Viv_DetalleGarantia_Eliminar")]
        public ErrorDto Viv_DetalleGarantia_Eliminar(int CodEmpresa, [FromBody] VivDetalleGarantiaEliminarDto data)
        {
            if (data == null)
            {
                return new ErrorDto
                {
                    Code = -2,
                    Description = "Debe indicar la información a eliminar."
                };
            }

            return BL.Viv_DetalleGarantia_Eliminar(CodEmpresa, data);
        }
    }
}