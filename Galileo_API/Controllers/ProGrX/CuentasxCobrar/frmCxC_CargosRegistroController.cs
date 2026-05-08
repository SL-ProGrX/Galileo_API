using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX.CuentasxCobrar;
using Galileo_API.Models.ProGrX.CuentasxCobrar;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX.CuentasxCobrar
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FrmCxCCargosRegistroController : ControllerBase
    {
        private readonly FrmCxCCargosRegistroBl _bl;

        public FrmCxCCargosRegistroController(IConfiguration config) =>
            _bl = new FrmCxCCargosRegistroBl(config);

        [HttpGet("CxCCargosRegistro_CargosAdicionales_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CxCCargosRegistro_CargosAdicionales_Obtener(int codEmpresa)
        {
            return _bl.CxCCargosRegistro_CargosAdicionales_Obtener(codEmpresa);
        }

        [HttpGet("CxCCargosRegistro_Operacion_Obtener")]
        public ErrorDto<CxCCargosRegistroOperacionData?> CxCCargosRegistro_Operacion_Obtener(int codEmpresa, int operacion)
        {
            return _bl.CxCCargosRegistro_Operacion_Obtener(codEmpresa, operacion);
        }

        [HttpGet("CxCCargosRegistro_Cargo_Obtener")]
        public ErrorDto<CxCCargosRegistroCargoData?> CxCCargosRegistro_Cargo_Obtener(int codEmpresa, string codCargo)
        {
            return _bl.CxCCargosRegistro_Cargo_Obtener(codEmpresa, codCargo);
        }

        [HttpGet("CxCCargosRegistro_CargoReposicion_Obtener")]
        public ErrorDto<CxCCargosRegistroCargoReposicionData?> CxCCargosRegistro_CargoReposicion_Obtener(int codEmpresa, int operacion)
        {
            return _bl.CxCCargosRegistro_CargoReposicion_Obtener(codEmpresa, operacion);
        }

        [HttpPost("CxCCargosRegistro_Aplicar")]
        public ErrorDto CxCCargosRegistro_Aplicar(
            int codEmpresa, string usuario, CxCCargosRegistroAplicarRequest request)
        {
            return _bl.CxCCargosRegistro_Aplicar(codEmpresa, usuario, request);
        }
    }
}