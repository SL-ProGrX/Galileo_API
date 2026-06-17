using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX.Creditos;
using Galileo_API.Models.ProGrX.Creditos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX.Creditos
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmCrConstanciasController : ControllerBase
    {
        private readonly FrmCrConstanciasBL BL;

        public FrmCrConstanciasController(IConfiguration config)
        {
            BL = new FrmCrConstanciasBL(config);
        }

        [Authorize]
        [HttpGet("CR_Constancias_Inicial_Obtener")]
        public ErrorDto<CrConstanciasInicialDto> CR_Constancias_Inicial_Obtener(
            int CodEmpresa,
            string cedula,
            string nombre,
            DateTime? corte,
            string usuario)
        {
            return BL.CR_Constancias_Inicial_Obtener(
                CodEmpresa,
                cedula,
                nombre,
                corte,
                usuario);
        }

        [Authorize]
        [HttpGet("CR_Constancias_Educacion_List_Obtener")]
        public ErrorDto<List<CrConstanciasEducacionDto>> CR_Constancias_Educacion_List_Obtener(
            int CodEmpresa,
            string tipo,
            string? codigo)
        {
            return BL.CR_Constancias_Educacion_List_Obtener(
                CodEmpresa,
                tipo,
                codigo);
        }

        [Authorize]
        [HttpGet("CR_Constancias_Padron_Nombre_Obtener")]
        public ErrorDto<CrConstanciasPadronDto> CR_Constancias_Padron_Nombre_Obtener(
            int CodEmpresa,
            string identificacion)
        {
            return BL.CR_Constancias_Padron_Nombre_Obtener(
                CodEmpresa,
                identificacion);
        }

        [Authorize]
        [HttpPost("CR_Constancias_Bitacora_Registra")]
        public ErrorDto CR_Constancias_Bitacora_Registra(
            int CodEmpresa,
            [FromBody] CrConstanciasBitacoraRequest request)
        {
            if (request == null)
            {
                return new ErrorDto
                {
                    Code = -1,
                    Description = "La solicitud es requerida."
                };
            }

            return BL.CR_Constancias_Bitacora_Registra(
                CodEmpresa,
                request);
        }
        [Authorize]
        [HttpGet("CR_Constancias_Padron_Buscar")]
        public ErrorDto<List<CrConstanciasPadronBusquedaDto>> CR_Constancias_Padron_Buscar(int CodEmpresa)
        {
            return BL.CR_Constancias_Padron_Buscar(CodEmpresa);
        }

    }
}