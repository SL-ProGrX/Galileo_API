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

        [HttpPost("Viv_Garantia_Guardar")]
        public ErrorDto<FrmVivGarantiaGuardarResponse> FrmVivGarantiaGuardar(
    int codEmpresa,
    FrmVivGarantiaGuardarRequest request)
        {
            return _bl.FrmVivGarantiaGuardar(codEmpresa, request);
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

        [HttpGet("Viv_GarantiaCantones_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> FrmVivGarantiaCantones_Obtener(
    int codEmpresa,
    string provincia)
        {
            return _bl.FrmVivGarantiaCantones_Obtener(codEmpresa, provincia);
        }

        [HttpGet("Viv_GarantiaDistritos_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> FrmVivGarantiaDistritos_Obtener(
            int codEmpresa,
            string provincia,
            string canton)
        {
            return _bl.FrmVivGarantiaDistritos_Obtener(codEmpresa, provincia, canton);
        }

        [HttpPost("Viv_GarantiaProfesionales_Buscar")]
        public ErrorDto<FrmVivGarantiaProfesionalesBuscarResponse> FrmVivGarantiaProfesionales_Buscar(
    int codEmpresa,
    FrmVivGarantiaProfesionalesBuscarRequest request)
        {
            return _bl.FrmVivGarantiaProfesionales_Buscar(codEmpresa, request);
        }
        #endregion

        #region Derechos
        [HttpPost("Viv_GarantiaDerechos_Listar")]
        public ErrorDto<List<FrmVivGarantiaDerechoDuenoItem>> FrmVivGarantiaDerechos_Listar(
            int codEmpresa,
            FrmVivGarantiaIdGarantiaRequest request)
        {
            return _bl.FrmVivGarantiaDerechos_Listar(codEmpresa, request);
        }

        [HttpPost("Viv_GarantiaSocio_Obtener")]
        public ErrorDto<FrmVivGarantiaSocioItem> FrmVivGarantiaSocio_Obtener(
    int codEmpresa,
    FrmVivGarantiaSocioRequest request)
        {
            return _bl.FrmVivGarantiaSocio_Obtener(codEmpresa, request);
        }

        [HttpPost("Viv_GarantiaSocios_Buscar")]
        public ErrorDto<FrmVivGarantiaSociosBuscarResponse> FrmVivGarantiaSocios_Buscar(
    int codEmpresa,
    FrmVivGarantiaSociosBuscarRequest request)
        {
            return _bl.FrmVivGarantiaSocios_Buscar(codEmpresa, request);
        }

        [HttpPost("Viv_GarantiaDerecho_Guardar")]
        public ErrorDto FrmVivGarantiaDerecho_Guardar(
    int codEmpresa,
    FrmVivGarantiaDerechoGuardarRequest request)
        {
            return _bl.FrmVivGarantiaDerecho_Guardar(codEmpresa, request);
        }

        [HttpPost("Viv_GarantiaDerecho_Borrar")]
        public ErrorDto FrmVivGarantiaDerecho_Borrar(
            int codEmpresa,
            FrmVivGarantiaDerechoBorrarRequest request)
        {
            return _bl.FrmVivGarantiaDerecho_Borrar(codEmpresa, request);
        }

        #endregion

        #region Historial del Tramite

        [HttpPost("Viv_GarantiaHistorial_Obtener")]
        public ErrorDto<FrmVivGarantiaHistorialResponse> FrmVivGarantiaHistorial_Obtener(
    int codEmpresa,
    FrmVivGarantiaIdGarantiaRequest request)
        {
            return _bl.FrmVivGarantiaHistorial_Obtener(codEmpresa, request);
        }

        #endregion

        #region Fincas

        [HttpPost("Viv_GarantiaFincasAsociadas_Listar")]
        public ErrorDto<List<FrmVivGarantiaFincaAsociadaItem>> FrmVivGarantiaFincasAsociadas_Listar(
    int codEmpresa,
    FrmVivGarantiaCargaRequest request)
        {
            return _bl.FrmVivGarantiaFincasAsociadas_Listar(codEmpresa, request);
        }

        #endregion

        #region Notas

        [HttpPost("Viv_GarantiaNotas_Listar")]
        public ErrorDto<List<FrmVivGarantiaNotaTramiteItem>> FrmVivGarantiaNotas_Listar(
    int codEmpresa,
    FrmVivGarantiaNotasRequest request)
        {
            return _bl.FrmVivGarantiaNotas_Listar(codEmpresa, request);
        }

        #endregion
    }
}
