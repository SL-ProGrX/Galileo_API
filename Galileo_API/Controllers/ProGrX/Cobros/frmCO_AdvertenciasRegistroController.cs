using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Galileo.BusinessLogic.ProGrX.Cobros;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Cobros;

namespace Galileo.Controllers.ProGrX.Cobros
{
    [Route("api/[controller]")]
    [ApiController]

    public class FrmCoAdvertenciasRegistroController : Controller
    {
        private readonly IConfiguration? _config;
        private readonly FrmCoAdvertenciasRegistroBL _bl;

        public FrmCoAdvertenciasRegistroController(IConfiguration config)
        {
            _config = config;
            _bl = new FrmCoAdvertenciasRegistroBL(_config);
        }

        [Authorize]
        [HttpGet("CoAdvertenciasRegistro_Consultar")]
        public ErrorDto<List<CoAdvertenciasRegistroData>> CoAdvertenciasRegistro_Consultar(int CodEmpresa, string cedula, string cod_advertencia ="", int linea = 0)
        {
            return _bl.CoAdvertenciasRegistro_Consultar(CodEmpresa, cedula, cod_advertencia, linea);
        }

        [Authorize]
        [HttpPost("CoAdvertenciasRegistro_Guardar")]
        public ErrorDto<int> CoAdvertenciasRegistro_Guardar(int CodEmpresa, string usuario, CoAdvertenciasRegistroData datos)
        {
            return _bl.CoAdvertenciasRegistro_Guardar(CodEmpresa, usuario, datos);
        }

        [Authorize]
        [HttpDelete("CoAdvertenciasRegistro_Delete")]
        public ErrorDto CoAdvertenciasRegistro_Delete(int CodEmpresa, string usuario, string cedula, string cod_advertencia, int linea)
        {
            return _bl.CoAdvertenciasRegistro_Delete(CodEmpresa, usuario, cedula, cod_advertencia, linea);
        }

        [Authorize]
        [HttpGet("CoAdvertenciasRegistro_TipoAdvertencia")]
        public ErrorDto<DropDownListaGenericaModel> CoAdvertenciasRegistro_TipoAdvertencia(int CodEmpresa,  int orden, string cod_advertencia = "")
        {
            return _bl.CoAdvertenciasRegistro_TipoAdvertencia(CodEmpresa, cod_advertencia, orden);
        }

        [Authorize]
        [HttpGet("TiposAdvertiencia_Consultar")]
        public ErrorDto<List<DropDownListaGenericaModel>> TiposAdvertiencia_Consultar(int CodEmpresa)
        {
            return _bl.TiposAdvertiencia_Consultar(CodEmpresa);
        }

        [Authorize]
        [HttpGet("CoAdvertenciasRegistroSocios_Obtener")]
        public ErrorDto<List<CoAdvertenciasRegistroSociosData>> CoAdvertenciasRegistroSocios_Obtener(int CodEmpresa)
        {
            return _bl.CoAdvertenciasRegistroSocios_Obtener(CodEmpresa);
        }

        [Authorize]
        [HttpGet("CoAdvertenciasRegistroNombreSocios_Consultar")]
        public ErrorDto<string> CoAdvertenciasRegistroNombreSocios_Consultar(int CodEmpresa, string cedula)
        {
            return _bl.CoAdvertenciasRegistroNombreSocios_Consultar(CodEmpresa, cedula);
        }

        [Authorize]
        [HttpPost("CoAdvertenciasRegistroResolucion_Guardar")]
        public ErrorDto CoAdvertenciasRegistroResolucion_Guardar(int CodEmpresa, string usuario, CoAdvertenciasRegistroData datos)
        {
            return _bl.CoAdvertenciasRegistroResolucion_Guardar(CodEmpresa, usuario, datos);
        }
    }
}
