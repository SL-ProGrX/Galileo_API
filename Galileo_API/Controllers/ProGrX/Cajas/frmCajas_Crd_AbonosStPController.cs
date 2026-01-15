using Galileo.Models;
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
    public class FrmCajasCrdAbonosStPController : ControllerBase
    {
        private readonly FrmCajasCrdAbonosStpBL _bl;

        public FrmCajasCrdAbonosStPController(IConfiguration config)
        {
            _bl = new FrmCajasCrdAbonosStpBL(config);
        }

        [HttpGet("CajasCrdAbonosSt_fxCrdParametro")]
        public ErrorDto<int> CajasCrdAbonosSt_fxCrdParametro(int CodEmpresa, string parametro)
        {
            return _bl.CajasCrdAbonosSt_fxCrdParametro(CodEmpresa, parametro);
        }

        [HttpGet("CajasCrdAbonosSt_ConsultaOperacion_Obtener")]
        public ErrorDto<CajasCrdAbonosStPDData> CajasCrdAbonosSt_ConsultaOperacion_Obtener(int CodEmpresa, string CodCaja, int OperacionId)
        {
            return _bl.CajasCrdAbonosSt_ConsultaOperacion_Obtener(CodEmpresa, CodCaja, OperacionId);
        }

        [HttpGet("CajasCrdAbonosSt_MoraConsulta")]
        public ErrorDto<List<CajasCrdAbonoMorosidadData>> CajasCrdAbonosSt_MoraConsulta(int CodEmpresa, int Operacion, DateTime FechaPago)
        {
            return _bl.CajasCrdAbonosSt_MoraConsulta(CodEmpresa, Operacion, FechaPago);
        }

        [HttpGet("CajasCrdAbonosSt_Documentos_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CajasCrdAbonosSt_Documentos_Obtener(int CodEmpresa, string codCaja)
        {
            return _bl.CajasCrdAbonosSt_Documentos_Obtener(CodEmpresa, codCaja);
        }

        [HttpGet("CajasCrdAbonosSt_Operaciones_Obtener")]
        public ErrorDto<List<CajasCrdAbonosStPDData>> CajasCrdAbonosSt_Operaciones_Obtener(int CodEmpresa)
        {
            return _bl.CajasCrdAbonosSt_Operaciones_Obtener(CodEmpresa);
        }

    }
}
