using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic;
using Galileo.Models.ERROR;
using Galileo.Models.INV;
using Microsoft.AspNetCore.Authorization;

namespace Galileo.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class FrmInvTranESController : ControllerBase
    {
        private readonly FrmInvTranEsBL _bl;
        public FrmInvTranESController(IConfiguration config)
        {
            _bl = new FrmInvTranEsBL(config);
        }

        [HttpGet("InvTranES_Obtener")]
        public ErrorDto<TranESData> InvTranES_Obtener(int CodEmpresa, string CodBoleta, string TipoTran)
        {
            return _bl.InvTranES_Obtener(CodEmpresa, CodBoleta, TipoTran);
        }

        [HttpGet("InvProducLineas_Obtener")]
        public ErrorDto<List<InvProducLineas>> InvProducLineas_Obtener(int CodEmpresa, string CodBoleta, string TipoTran)
        {
            return _bl.InvProducLineas_Obtener(CodEmpresa, CodBoleta, TipoTran);
        }

        [HttpGet("InvTranES_scroll")]
        public ErrorDto<TranESData> InvTranES_scroll(int CodEmpresa, int scrollValue, string? CodBoleta, string TipoTran)
        {
            return _bl.InvTranES_scroll(CodEmpresa, scrollValue, CodBoleta, TipoTran);
        }

        [HttpPost("InvTranES_Insertar")]
        public ErrorDto InvTranES_Insertar(int CodEmpresa, string TipoTran, TranESData request)
        {
            return _bl.InvTranES_Insertar(CodEmpresa, TipoTran, request);
        }

        [HttpPost("InvTranES_Actualizar")]
        public ErrorDto InvTranES_Actualizar(int CodEmpresa, TranESUpdate request)
        {
            return _bl.InvTranES_Actualizar(CodEmpresa, request);
        }

        [HttpPost("InvTranES_Eliminar")]
        public ErrorDto InvTranES_Eliminar(int CodEmpresa, string CodBoleta, string TipoTran)
        {
            return _bl.InvTranES_Eliminar(CodEmpresa, CodBoleta, TipoTran);
        }

        [HttpPost("InvProducLineas_Insertar")]
        public ErrorDto InvProducLineas_Insertar(int CodEmpresa, string CodBoleta, string TipoTran, List<InvProducLineasInsert> request)
        {
            return _bl.InvProducLineas_Insertar(CodEmpresa, CodBoleta, TipoTran, request);
        }

        [HttpGet("InvTranPlantilla_Obtener")]
        public ErrorDto<List<InvTranPlantilla>> InvTranPlantilla_Obtener(int CodEmpresa, string TipoTran, string? CodBoleta, string? GeneraUser, string? GeneraFecha)
        {
            return _bl.InvTranPlantilla_Obtener(CodEmpresa, TipoTran, CodBoleta, GeneraUser, GeneraFecha);
        }

        [HttpPost("InvProducLineas_Eliminar")]
        public ErrorDto InvProducLineas_Eliminar(int CodEmpresa, string CodBoleta, string TipoTran, int Linea)
        {
            return _bl.InvProducLineas_Eliminar(CodEmpresa, CodBoleta, TipoTran, Linea);
        }
    }
}