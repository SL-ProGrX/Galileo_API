using Galileo.DataBaseTier;
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
    public class FrmCRComitesController : ControllerBase
    {
        private readonly FrmCrComitesBL _bl;
        private const string DATOSREQUERIDOS = "Datos requeridos.";

        public FrmCRComitesController(IConfiguration config)
        {
            _bl = new FrmCrComitesBL(config);
        }

        [Authorize]
        [HttpGet("CR_Comites_Lista_Obtener")]
        public ErrorDto<CrComitesLista> CR_Comites_Lista_Obtener(int CodEmpresa, string parametros)
        {
            return _bl.CR_Comites_Lista_Obtener(CodEmpresa, parametros);
        }

        [Authorize]
        [HttpGet("CR_Comites_Lista_Export")]
        public ErrorDto<CrComitesLista> CR_Comites_Lista_Export(int CodEmpresa, string parametros)
        {
            return _bl.CR_Comites_Lista_Export(CodEmpresa, parametros);
        }

        [Authorize]
        [HttpPost("CR_Comites_Guardar")]
        public ErrorDto<CrComitesGuardarResult> CR_Comites_Guardar(int CodEmpresa, string usuario, [FromBody] CrComitesGuardarRequest request)
        {
            if (request == null)
            {
                return DbHelper.CreateErrorResponse<CrComitesGuardarResult>(
                    DATOSREQUERIDOS,
                    -2,
                    new CrComitesGuardarResult());
            }

            return _bl.CR_Comites_Guardar(CodEmpresa, request, usuario);
        }

        [Authorize]
        [HttpDelete("CR_Comites_Eliminar")]
        public ErrorDto CR_Comites_Eliminar(int CodEmpresa, int id_comite, string usuario)
        {
            return _bl.CR_Comites_Eliminar(CodEmpresa, id_comite, usuario);
        }

        [Authorize]
        [HttpGet("CR_Comites_NivelAprobacion_Dropdown_Obtener")]
        public ErrorDto<List<CrComitesNivelAprobacionDto>> CR_Comites_NivelAprobacion_Dropdown_Obtener(int CodEmpresa)
        {
            return _bl.CR_Comites_NivelAprobacion_Dropdown_Obtener(CodEmpresa);
        }

        [Authorize]
        [HttpGet("CR_Comites_Garantias_Lista_Obtener")]
        public ErrorDto<CrComitesGarantiasLista> CR_Comites_Garantias_Lista_Obtener(int CodEmpresa, int id_comite, string usuario)
        {
            return _bl.CR_Comites_Garantias_Lista_Obtener(CodEmpresa, id_comite, usuario);
        }

        [Authorize]
        [HttpGet("CR_Comites_Garantias_Lista_Export")]
        public ErrorDto<CrComitesGarantiasLista> CR_Comites_Garantias_Lista_Export(int CodEmpresa, int id_comite, string usuario)
        {
            return _bl.CR_Comites_Garantias_Lista_Export(CodEmpresa, id_comite, usuario);
        }

        [Authorize]
        [HttpPost("CR_Comites_Garantias_Guardar")]
        public ErrorDto CR_Comites_Garantias_Guardar(int CodEmpresa, string usuario, [FromBody] CrComitesGarantiasGuardarRequest request)
        {
            if (request == null)
            {
                return DbHelper.ErrorResponse(DATOSREQUERIDOS, -2);
            }

            return _bl.CR_Comites_Garantias_Guardar(CodEmpresa, request, usuario);
        }

        [Authorize]
        [HttpGet("CR_Comites_Lineas_Lista_Obtener")]
        public ErrorDto<CrComitesLineasLista> CR_Comites_Lineas_Lista_Obtener(int CodEmpresa, int id_comite)
        {
            return _bl.CR_Comites_Lineas_Lista_Obtener(CodEmpresa, id_comite);
        }

        [Authorize]
        [HttpPost("CR_Comites_Lineas_Asignar")]
        public ErrorDto CR_Comites_Lineas_Asignar(int CodEmpresa, string usuario, [FromBody] CrComitesLineasAsignarRequest request)
        {
            if (request == null)
            {
                return DbHelper.ErrorResponse(DATOSREQUERIDOS, -2);
            }

            return _bl.CR_Comites_Lineas_Asignar(CodEmpresa, request, usuario);
        }
    }
}