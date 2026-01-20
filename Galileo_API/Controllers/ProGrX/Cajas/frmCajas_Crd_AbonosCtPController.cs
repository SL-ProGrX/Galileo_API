using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX.Cajas;
using Galileo_API.Models.ProGrX.Cajas;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX.Cajas
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FrmCajasCrdAbonosCtPController : ControllerBase
    {
        private readonly FrmCajasCrdAbonosCtPBl _bl;

        public FrmCajasCrdAbonosCtPController(IConfiguration config)
        {
            _bl = new FrmCajasCrdAbonosCtPBl(config);
        }

        [HttpGet("CajasCrdAbonosCtP_ConsultaOperacion_Obtener")]
        public ErrorDto<CajasCrdAbonosCtPData> CajasCrdAbonosCtP_ConsultaOperacion_Obtener(int CodEmpresa, string CodCaja, int OperacionId)
        {
            return _bl.CajasCrdAbonosCtP_ConsultaOperacion_Obtener(CodEmpresa, CodCaja, OperacionId);
        }

        [HttpGet("CajasCrdAbonosCtP_Operaciones_Obtener")]
        public ErrorDto<List<CajasCrdAbonosCtPData>> CajasCrdAbonosCtP_Operaciones_Obtener(int CodEmpresa)
        {
            return _bl.CajasCrdAbonosCtP_Operaciones_Obtener(CodEmpresa);
        }

        [HttpGet("CajasCrdAbonosCtP_OperacionTransac_Obtener")]
        public ErrorDto<List<CajasCrdOperacionTransacData>> CajasCrdAbonosCtP_OperacionTransac_Obtener(int CodEmpresa, int IdSolicitud)
        {
            return _bl.CajasCrdAbonosCtP_OperacionTransac_Obtener(CodEmpresa, IdSolicitud);
        }
    }
}
