using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX_Hipotecario;
using Galileo_API.Models.ProGrX_Hipotecario;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX_Hipotecario
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FrmVivGarantiaController : ControllerBase
    {
        private readonly FrmVivGarantiaBL _bl;

        public FrmVivGarantiaController(IConfiguration config)
        {
            _bl = new FrmVivGarantiaBL(config);
        }

        #region Principal

        [Authorize]
        [HttpPost("Viv_Garantia_Principal_Cargar")]
        public ErrorDto<FrmVivGarantiaPrincipalResponse> FrmVivGarantiaPrincipal_Cargar(
            int codEmpresa,
            FrmVivGarantiaCargaRequest request)
        {
            return _bl.FrmVivGarantiaPrincipal_Cargar(codEmpresa, request);
        }

        #endregion

        #region General

        [Authorize]
        [HttpPost("Viv_Garantia_General_Listar")]
        public ErrorDto<List<FrmVivGarantiaGeneralItem>> FrmVivGarantiaGeneral_Listar(
            int codEmpresa,
            FrmVivGarantiaCargaRequest request)
        {
            return _bl.FrmVivGarantiaGeneral_Listar(codEmpresa, request);
        }

        #endregion

        #region Garantia

        [HttpPost("Viv_GarantiaDetalle_Obtener")]
        public ErrorDto<FrmVivGarantiaDetalleResponse> FrmVivGarantiaDetalle_Obtener(
    int codEmpresa,
    FrmVivGarantiaDetalleRequest request)
        {
            return _bl.FrmVivGarantiaDetalle_Obtener(codEmpresa, request);
        }

        [HttpPost("Viv_GarantiaCantones_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> FrmVivGarantiaCantones_Obtener(
    int codEmpresa,
    FrmVivGarantiaProvinciaRequest request)
        {
            return _bl.FrmVivGarantiaCantones_Obtener(codEmpresa, request);
        }

        [HttpPost("Viv_GarantiaDistritos_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> FrmVivGarantiaDistritos_Obtener(
    int codEmpresa,
    FrmVivGarantiaCantonRequest request)
        {
            return _bl.FrmVivGarantiaDistritos_Obtener(codEmpresa, request);
        }

        #endregion

        #region Derechos
        #endregion

        #region Historial del Tramite
        #endregion

        #region Fincas
        #endregion

        #region Notas
        #endregion
    }
}
