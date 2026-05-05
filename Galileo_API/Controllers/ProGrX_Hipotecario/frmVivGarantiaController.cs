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

        [HttpPost("Viv_Garantia_Principal_Cargar")]
        public ErrorDto<FrmVivGarantiaPrincipalResponse> Viv_GarantiaPrincipal_Cargar(
            int codEmpresa,
            FrmVivGarantiaCargaRequest request)
        {
            return _bl.Viv_GarantiaPrincipal_Cargar(codEmpresa, request);
        }

        [HttpPost("Viv_Garantia_Guardar")]
        public ErrorDto<FrmVivGarantiaGuardarResponse> Viv_GarantiaGuardar(
    int codEmpresa,
    FrmVivGarantiaGuardarRequest request)
        {
            return _bl.Viv_GarantiaGuardar(codEmpresa, request);
        }

        #endregion

        #region General

        [HttpPost("Viv_Garantia_General_Listar")]
        public ErrorDto<List<FrmVivGarantiaGeneralItem>> Viv_GarantiaGeneral_Listar(
            int codEmpresa,
            FrmVivGarantiaCargaRequest request)
        {
            return _bl.Viv_GarantiaGeneral_Listar(codEmpresa, request);
        }

        #endregion

        #region Garantia

        [HttpPost("Viv_GarantiaDetalle_Obtener")]
        public ErrorDto<FrmVivGarantiaDetalleResponse> Viv_GarantiaDetalle_Obtener(
    int codEmpresa,
    FrmVivGarantiaDetalleRequest request)
        {
            return _bl.Viv_GarantiaDetalle_Obtener(codEmpresa, request);
        }

        [HttpGet("Viv_GarantiaCantones_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> Viv_GarantiaCantones_Obtener(
    int codEmpresa,
    string provincia)
        {
            return _bl.Viv_GarantiaCantones_Obtener(codEmpresa, provincia);
        }

        [HttpGet("Viv_GarantiaDistritos_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> Viv_GarantiaDistritos_Obtener(
            int codEmpresa,
            string provincia,
            string canton)
        {
            return _bl.Viv_GarantiaDistritos_Obtener(codEmpresa, provincia, canton);
        }

        [HttpPost("Viv_GarantiaProfesionales_Buscar")]
        public ErrorDto<FrmVivGarantiaProfesionalesBuscarResponse> Viv_GarantiaProfesionales_Buscar(
    int codEmpresa,
    FrmVivGarantiaProfesionalesBuscarRequest request)
        {
            return _bl.Viv_GarantiaProfesionales_Buscar(codEmpresa, request);
        }
        #endregion

        #region Derechos
        [HttpPost("Viv_GarantiaDerechos_Listar")]
        public ErrorDto<List<FrmVivGarantiaDerechoDuenoItem>> Viv_GarantiaDerechos_Listar(
            int codEmpresa,
            FrmVivGarantiaIdGarantiaRequest request)
        {
            return _bl.Viv_GarantiaDerechos_Listar(codEmpresa, request);
        }

        [HttpPost("Viv_GarantiaSocio_Obtener")]
        public ErrorDto<FrmVivGarantiaSocioItem> Viv_GarantiaSocio_Obtener(
    int codEmpresa,
    FrmVivGarantiaSocioRequest request)
        {
            return _bl.Viv_GarantiaSocio_Obtener(codEmpresa, request);
        }

        [HttpPost("Viv_GarantiaSocios_Buscar")]
        public ErrorDto<FrmVivGarantiaSociosBuscarResponse> Viv_GarantiaSocios_Buscar(
    int codEmpresa,
    FrmVivGarantiaSociosBuscarRequest request)
        {
            return _bl.Viv_GarantiaSocios_Buscar(codEmpresa, request);
        }

        [HttpPost("Viv_GarantiaDerecho_Guardar")]
        public ErrorDto Viv_GarantiaDerecho_Guardar(
    int codEmpresa,
    FrmVivGarantiaDerechoGuardarRequest request)
        {
            return _bl.Viv_GarantiaDerecho_Guardar(codEmpresa, request);
        }

        [HttpPost("Viv_GarantiaDerecho_Borrar")]
        public ErrorDto Viv_GarantiaDerecho_Borrar(
            int codEmpresa,
            FrmVivGarantiaDerechoBorrarRequest request)
        {
            return _bl.Viv_GarantiaDerecho_Borrar(codEmpresa, request);
        }

        #endregion

        #region Historial del Tramite

        [HttpPost("Viv_GarantiaHistorial_Obtener")]
        public ErrorDto<FrmVivGarantiaHistorialResponse> Viv_GarantiaHistorial_Obtener(
    int codEmpresa,
    FrmVivGarantiaIdGarantiaRequest request)
        {
            return _bl.Viv_GarantiaHistorial_Obtener(codEmpresa, request);
        }

        #endregion

        #region Fincas

        [HttpPost("Viv_GarantiaFincasAsociadas_Listar")]
        public ErrorDto<List<FrmVivGarantiaFincaAsociadaItem>> Viv_GarantiaFincasAsociadas_Listar(
    int codEmpresa,
    FrmVivGarantiaCargaRequest request)
        {
            return _bl.Viv_GarantiaFincasAsociadas_Listar(codEmpresa, request);
        }

        #endregion

        #region Notas

        [HttpPost("Viv_GarantiaNotas_Listar")]
        public ErrorDto<List<FrmVivGarantiaNotaTramiteItem>> Viv_GarantiaNotas_Listar(
    int codEmpresa,
    FrmVivGarantiaNotasRequest request)
        {
            return _bl.Viv_GarantiaNotas_Listar(codEmpresa, request);
        }

        #endregion
    }
}
