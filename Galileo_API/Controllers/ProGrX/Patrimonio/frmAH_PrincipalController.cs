using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Procesos;
using Galileo_API.BusinessLogic.ProGrX.Patrimonio;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX.Patrimonio
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FrmAHPrincipalController : ControllerBase
    {
        private readonly FrmAHPrincipalBL _bl;

        public FrmAHPrincipalController(IConfiguration config)
        {
            _bl = new FrmAHPrincipalBL(config);
        }

        [HttpGet("Patrimonio_frmAH_Principal_Consulta_Obtener")]
        public ErrorDto<FrmAhPrincipalConsultaResponse?> Patrimonio_frmAH_Principal_Consulta_Obtener(
            [FromQuery] int CodEmpresa,
            [FromQuery] string Cedula,
            [FromQuery] string Usuario)
            => _bl.Patrimonio_frmAH_Principal_Consulta_Obtener(CodEmpresa, Cedula, Usuario);

        [HttpGet("Patrimonio_frmAH_Principal_DetallePatrimonio_Obtener")]
        public ErrorDto<List<FrmAhPrincipalDetallePatrimonioResponse>> Patrimonio_frmAH_Principal_DetallePatrimonio_Obtener(
            [FromQuery] int CodEmpresa,
            [FromQuery] FrmAhPrincipalDetallePatrimonioRequest request)
            => _bl.Patrimonio_frmAH_Principal_DetallePatrimonio_Obtener(CodEmpresa, request);

        [HttpGet("Patrimonio_frmAH_Principal_Excedentes_Obtener")]
        public ErrorDto<List<FrmAhPrincipalExcedentesResponse>> Patrimonio_frmAH_Principal_Excedentes_Obtener(
            [FromQuery] int CodEmpresa,
            [FromQuery] string Cedula)
            => _bl.Patrimonio_frmAH_Principal_Excedentes_Obtener(CodEmpresa, Cedula);

        [HttpGet("Patrimonio_frmAH_Principal_Historico_Obtener")]
        public ErrorDto<List<FrmAhPrincipalHistoricoResponse>> Patrimonio_frmAH_Principal_Historico_Obtener(
            [FromQuery] int CodEmpresa,
            [FromQuery] string Cedula)
            => _bl.Patrimonio_frmAH_Principal_Historico_Obtener(CodEmpresa, Cedula);

        [HttpGet("Patrimonio_frmAH_Principal_Liquidaciones_Obtener")]
        public ErrorDto<List<FrmAhPrincipalLiquidacionesResponse>> Patrimonio_frmAH_Principal_Liquidaciones_Obtener(
            [FromQuery] int CodEmpresa,
            [FromQuery] string Cedula)
            => _bl.Patrimonio_frmAH_Principal_Liquidaciones_Obtener(CodEmpresa, Cedula);
    }
}
