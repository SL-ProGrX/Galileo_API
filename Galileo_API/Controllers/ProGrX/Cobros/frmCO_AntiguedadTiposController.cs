using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogicTier.ProGrX.Cobros;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Cobros;

namespace Galileo.Controllers.ProGrX.Cobros
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmCOAntiguedadTiposController : Controller
    {
        private readonly IConfiguration? _config;
        private readonly FrmCOAntiguedadTiposBL _bl;

        public FrmCOAntiguedadTiposController(IConfiguration config)
        {
            _config = config;
            _bl = new FrmCOAntiguedadTiposBL(_config);
        }

        [Authorize]
        [HttpGet("Co_AntiguedadTipos_Lista_Obtener")]
        public ErrorDto<FrmCOAntiguedadTiposListaResult> Co_AntiguedadTipos_Lista_Obtener(int CodEmpresa, string filtros)
        {
            return _bl.Co_AntiguedadTipos_Lista_Obtener(CodEmpresa, filtros);
        }

        [Authorize]
        [HttpGet("Co_AntiguedadTipos_Lista_Export")]
        public ErrorDto<FrmCOAntiguedadTiposListaResult> Co_AntiguedadTipos_Lista_Export(int CodEmpresa, string filtros)
        {
            return _bl.Co_AntiguedadTipos_Lista_Export(CodEmpresa, filtros);
        }

        [Authorize]
        [HttpPost("Co_AntiguedadTipos_Guardar")]
        public ErrorDto Co_AntiguedadTipos_Guardar(int CodEmpresa, string usuario, FrmCOAntiguedadTipoData dto)
        {
            return _bl.Co_AntiguedadTipos_Guardar(CodEmpresa, usuario, dto);
        }

        [Authorize]
        [HttpDelete("Co_AntiguedadTipos_Eliminar")]
        public ErrorDto Co_AntiguedadTipos_Eliminar(int CodEmpresa, string usuario, string cod_antiguedad)
        {
            return _bl.Co_AntiguedadTipos_Eliminar(CodEmpresa, usuario, cod_antiguedad);
        }

        [Authorize]
        [HttpGet("Co_AntiguedadTipos_Detalle_Obtener")]
        public ErrorDto<List<FrmCOAntiguedadGarantiaMitigadorData>> Co_AntiguedadTipos_Detalle_Obtener(int CodEmpresa, string cod_antiguedad, string usuario)
        {
            return _bl.Co_AntiguedadTipos_Detalle_Obtener(CodEmpresa, cod_antiguedad, usuario);
        }

        [Authorize]
        [HttpPost("Co_AntiguedadTipos_Detalle_Guardar")]
        public ErrorDto Co_AntiguedadTipos_Detalle_Guardar(int CodEmpresa, string usuario, FrmCOAntiguedadDetalleGuardarDto dto)
        {
            return _bl.Co_AntiguedadTipos_Detalle_Guardar(CodEmpresa, usuario, dto);
        }
    }
}
