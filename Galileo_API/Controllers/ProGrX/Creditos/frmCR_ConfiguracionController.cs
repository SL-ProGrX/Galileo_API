using Galileo.DataBaseTier;
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
    public class FrmCRConfiguracionController : ControllerBase
    {
        private readonly FrmCRConfiguracionBL _bl;
        private const string DATOSREQUERIDOS = "Datos requeridos.";
        public FrmCRConfiguracionController(IConfiguration config)
        {
            _bl = new FrmCRConfiguracionBL(config);
        }

        [Authorize]
        [HttpGet("CR_Configuracion_Generales_Lista_Obtener")]
        public ErrorDto<List<CrConfiguracionGeneralDto>> CR_Configuracion_Generales_Lista_Obtener(int CodEmpresa)
        {
            return _bl.CR_Configuracion_Generales_Lista_Obtener(CodEmpresa);
        }
        [Authorize]
        [HttpGet("CR_Configuracion_Generales_Lista_Export")]
        public ErrorDto<List<CrConfiguracionGeneralDto>> CR_Configuracion_Generales_Lista_Export(int CodEmpresa)
        {
            return _bl.CR_Configuracion_Generales_Lista_Export(CodEmpresa);
        }
        [Authorize]
        [HttpPost("CR_Configuracion_Generales_Guardar")]
        public ErrorDto CR_Configuracion_Generales_Guardar(int CodEmpresa,string usuario,[FromBody] CrConfiguracionGeneralGuardarDto request)
        {
            if (request == null)
            {
                return DbHelper.ErrorResponse(DATOSREQUERIDOS, -2);
            }

            return _bl.CR_Configuracion_Generales_Guardar(CodEmpresa, request, usuario);
        }
        [Authorize]
        [HttpGet("CR_Configuracion_Operativos_Obtener")]
        public ErrorDto<CrConfiguracionOperativosDto> CR_Configuracion_Operativos_Obtener(int CodEmpresa)
        {
            return _bl.CR_Configuracion_Operativos_Obtener(CodEmpresa);
        }
        [Authorize]
        [HttpPost("CR_Configuracion_Operativos_Guardar")]
        public ErrorDto CR_Configuracion_Operativos_Guardar(int CodEmpresa,string usuario,[FromBody] CrConfiguracionOperativosGuardarDto request)
        {
            if (request == null)
            {
                return DbHelper.ErrorResponse(DATOSREQUERIDOS, -2);
            }

            return _bl.CR_Configuracion_Operativos_Guardar(CodEmpresa, request, usuario);
        }
        [Authorize]
        [HttpGet("CR_Configuracion_Bancos_Dropdown_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CR_Configuracion_Bancos_Dropdown_Obtener(int CodEmpresa)
        {
            return _bl.CR_Configuracion_Bancos_Dropdown_Obtener(CodEmpresa);
        }
        [Authorize]
        [HttpGet("CR_Configuracion_TiposDocumento_Dropdown_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CR_Configuracion_TiposDocumento_Dropdown_Obtener()
        {
            return _bl.CR_Configuracion_TiposDocumento_Dropdown_Obtener();
        }
        [Authorize]
        [HttpPost("CR_Configuracion_FechaCorte_Guardar")]
        public ErrorDto CR_Configuracion_FechaCorte_Guardar(int CodEmpresa,string usuario,[FromBody] CrConfiguracionFechaCorteGuardarDto request)
        {
            if (request == null)
            {
                return DbHelper.ErrorResponse(DATOSREQUERIDOS, -2);
            }

            return _bl.CR_Configuracion_FechaCorte_Guardar(CodEmpresa, request, usuario);
        }
        [Authorize]
        [HttpPost("CR_Configuracion_TBP_Guardar")]
        public ErrorDto CR_Configuracion_TBP_Guardar(int CodEmpresa, string usuario,[FromBody] CrConfiguracionTbpGuardarDto request)
        {
            if (request == null)
            {
                return DbHelper.ErrorResponse(DATOSREQUERIDOS, -2);
            }

            return _bl.CR_Configuracion_TBP_Guardar(CodEmpresa, request, usuario);
        }
    }
}