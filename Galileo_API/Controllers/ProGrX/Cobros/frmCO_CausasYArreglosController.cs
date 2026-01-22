using Galileo_API.BusinessLogic.ProGrX.Cobros;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Cobros;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX.Cobros
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmCOCausasYArreglosController : ControllerBase
    {
        private readonly FrmCOCausasYArreglosBL _bl;

        public FrmCOCausasYArreglosController(IConfiguration config)
        {
            _bl = new FrmCOCausasYArreglosBL(config);
        }
        [Authorize]
        [HttpGet("Co_CausasMorosidad_Lista_Obtener")]
        public ErrorDto<COCausaMorosidadListaResult> Co_CausasMorosidad_Lista_Obtener(int CodEmpresa, string filtros)
        {
            return _bl.Co_CausasMorosidad_Lista_Obtener(CodEmpresa, filtros);
        }
        [Authorize]
        [HttpGet("Co_CausasMorosidad_Lista_Export")]
        public ErrorDto<COCausaMorosidadListaResult> Co_CausasMorosidad_Lista_Export(int CodEmpresa, string filtros)
        {
            return _bl.Co_CausasMorosidad_Lista_Export(CodEmpresa, filtros);
        }
        [Authorize]
        [HttpPost("Co_CausasMorosidad_Guardar")]
        public ErrorDto Co_CausasMorosidad_Guardar(int CodEmpresa, string usuario, COCausaMorosidadData causa)
        {
            return _bl.Co_CausasMorosidad_Guardar(CodEmpresa, usuario, causa);
        }
        [Authorize]
        [HttpDelete("Co_CausasMorosidad_Eliminar")]
        public ErrorDto Co_CausasMorosidad_Eliminar(int CodEmpresa, string usuario, string cod_causa)
        {
            return _bl.Co_CausasMorosidad_Eliminar(CodEmpresa, usuario, cod_causa);
        }
        [Authorize]
        [HttpGet("Co_TiposArreglos_Lista_Obtener")]
        public ErrorDto<COArregloPagoTipoListaResult> Co_TiposArreglos_Lista_Obtener(int CodEmpresa, string filtros)
        {
            return _bl.Co_TiposArreglos_Lista_Obtener(CodEmpresa, filtros);
        }
        [Authorize]
        [HttpGet("Co_TiposArreglos_Lista_Export")]
        public ErrorDto<COArregloPagoTipoListaResult> Co_TiposArreglos_Lista_Export(int CodEmpresa, string filtros)
        {
            return _bl.Co_TiposArreglos_Lista_Export(CodEmpresa, filtros);
        }
        [Authorize]
        [HttpPost("Co_TiposArreglos_Guardar")]
        public ErrorDto Co_TiposArreglos_Guardar(int CodEmpresa, string usuario, COArregloPagoTipoData tipo)
        {
            return _bl.Co_TiposArreglos_Guardar(CodEmpresa, usuario, tipo);
        }
        [Authorize]
        [HttpDelete("Co_TiposArreglos_Eliminar")]
        public ErrorDto Co_TiposArreglos_Eliminar(int CodEmpresa, string usuario, string cod_arreglo)
        {
            return _bl.Co_TiposArreglos_Eliminar(CodEmpresa, usuario, cod_arreglo);
        }
    }
}
