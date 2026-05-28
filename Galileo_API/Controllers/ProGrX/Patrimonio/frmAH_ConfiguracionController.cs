using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.AH;
using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX.Patrimonio;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX.Patrimonio
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FrmAhConfiguracionController : ControllerBase
    {
        private readonly FrmAhConfiguracionBL _bl;
        private const string DATOSREQUERIDOS = "Datos requeridos.";
        public FrmAhConfiguracionController(IConfiguration config)
        {
            _bl = new FrmAhConfiguracionBL(config);
        }

        [HttpGet("AH_Configuracion_Divisas_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> AH_Configuracion_Divisas_Obtener(int CodEmpresa)
            => _bl.AH_Configuracion_Divisas_Obtener(CodEmpresa);

        [HttpGet("AH_Configuracion_Parametros_Obtener")]
        public ErrorDto<ParametrosPatrimonioDto> AH_Configuracion_Parametros_Obtener(int CodEmpresa, string cod_divisa)
            => _bl.AH_Configuracion_Parametros_Obtener(CodEmpresa, cod_divisa);

        [HttpPost("AH_Configuracion_Parametros_Guardar")]
        public ErrorDto AH_Configuracion_Parametros_Guardar(int CodEmpresa, string usuario, [FromBody] AhConfiguracionGuardarRequest request)
        {
            if (request == null)
            {
                return DbHelper.ErrorResponse(DATOSREQUERIDOS, -2);
            }

            return _bl.AH_Configuracion_Parametros_Guardar(CodEmpresa, request, usuario);
        }

        [HttpGet("AH_Configuracion_Cuenta_Validar")]
        public ErrorDto<AhConfiguracionCuentaValidarResponse> AH_Configuracion_Cuenta_Validar(int CodEmpresa, string cuenta, int contabilidad)
            => _bl.AH_Configuracion_Cuenta_Validar(CodEmpresa, cuenta, contabilidad);
    }
}
