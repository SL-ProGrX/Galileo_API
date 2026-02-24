using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX_Polizas;
using Galileo_API.Models.ProGrX_Polizas;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX_Polizas
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FrmCrPolizasCargaLoteController : ControllerBase
    {
        private readonly FrmCrPolizasCargaLoteBL _BL;

        public FrmCrPolizasCargaLoteController(IConfiguration config)
        {
            _BL = new FrmCrPolizasCargaLoteBL(config);
        }

        [HttpGet("CrdPolizasCargaLote_Cliente_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CrdPolizasCargaLote_Cliente_Obtener(int CodEmpresa)
        {
            return _BL.CrdPolizasCargaLote_Cliente_Obtener(CodEmpresa);
        }

        [HttpGet("CrdPolizasCargaLote_Aseguradora_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CrdPolizasCargaLote_Aseguradora_Obtener(int CodEmpresa)
        {
            return _BL.CrdPolizasCargaLote_Aseguradora_Obtener(CodEmpresa);
        }

        [HttpGet("CrdPolizasCargaLote_Banco_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CrdPolizasCargaLote_Banco_Obtener(int CodEmpresa, string usuario)
        {
            return _BL.CrdPolizasCargaLote_Banco_Obtener(CodEmpresa, usuario);
        }

        [HttpPost("CrdPolizasCargaLote_Cuenta_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CrdPolizasCargaLote_Cuenta_Obtener(
          int CodEmpresa,
          CrdPolizasCargaLoteCuentaCatalogoRequest request)
        {
            return _BL.CrdPolizasCargaLote_Cuenta_Obtener(CodEmpresa, request);
        }

        [HttpGet("CrdPolizasCargaLote_Prideduc_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CrdPolizasCargaLote_Prideduc_Obtener(int codEmpresa, string usuario, int codContabilidad)
        {
            return _BL.CrdPolizasCargaLote_Prideduc_Obtener(codEmpresa, usuario, codContabilidad);
        }

        [HttpPost("CrdPolizasCargaLote_Cargar")]
        public ErrorDto<CrdPolizasCargaLoteCargaResponse> CrdPolizasCargaLote_Cargar(
              int codEmpresa,
              string usuario,
              CrdPolizasCargaLoteCargaRequest request)
        {
            return _BL.CrdPolizasCargaLote_Cargar(codEmpresa, usuario, request);
        }

    }
}
