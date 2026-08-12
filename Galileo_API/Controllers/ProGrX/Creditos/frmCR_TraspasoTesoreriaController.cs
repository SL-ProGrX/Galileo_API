using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic.ProGrX.Credito;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Clientes;
using Galileo.Models.ProGrX.Credito;

namespace Galileo.Controllers.ProGrX.Credito
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmCRTraspasoTesoreriaController : ControllerBase
    {
        private readonly FrmCRTraspasoTesoreriaBL _BL;
        public FrmCRTraspasoTesoreriaController(IConfiguration config)
        {
            _BL = new FrmCRTraspasoTesoreriaBL(config);
        }

        #region remesas

        [Authorize]
        [HttpGet("Cr_TraspasoTes_Remesas_Listar")]
        public ErrorDto<List<RemesaModel>> Cr_TraspasoTes_Remesas_Listar(int CodEmpresa)
        {
            return _BL.Cr_TraspasoTes_Remesas_Listar(CodEmpresa);
        }

        [Authorize]
        [HttpGet("Cr_TraspasoTes_Remesa_Obtener")]
        public ErrorDto<RemesaModel> Cr_TraspasoTes_Remesa_Obtener(int CodEmpresa, int cod_remesa)
        {
            return _BL.Cr_TraspasoTes_Remesa_Obtener(CodEmpresa, cod_remesa);
        }

        [Authorize]
        [HttpPost("Cr_TraspasoTes_Remesa_Crear")]
        public ErrorDto<RemesaModel> Cr_TraspasoTes_Remesa_Crear(int CodEmpresa, [FromBody] RemesaRequest request, string usuario)
        {
            return _BL.Cr_TraspasoTes_Remesa_Crear(CodEmpresa, request, usuario);
        }

        [Authorize]
        [HttpPut("Cr_TraspasoTes_Remesa_Modificar")]
        public ErrorDto Cr_TraspasoTes_Remesa_Modificar(int CodEmpresa, [FromBody] RemesaRequest request, string usuario)
        {
            return _BL.Cr_TraspasoTes_Remesa_Modificar(CodEmpresa, request, usuario);
        }

        [Authorize]
        [HttpDelete("Cr_TraspasoTes_Remesa_Eliminar")]
        public ErrorDto Cr_TraspasoTes_Remesa_Eliminar(int CodEmpresa, int cod_remesa, string usuario)
        {
            return _BL.Cr_TraspasoTes_Remesa_Eliminar(CodEmpresa, cod_remesa, usuario);
        }

        #endregion

        #region cargar

        [Authorize]
        [HttpGet("Cr_TraspasoTes_RemesasAbiertas_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> Cr_TraspasoTes_RemesasAbiertas_Obtener(int CodEmpresa)
        {
            return _BL.Cr_TraspasoTes_RemesasAbiertas_Obtener(CodEmpresa);
        }

        [Authorize]
        [HttpGet("Cr_TraspasoTes_Carga_Buscar")]
        public ErrorDto<List<CargaOperacionModel>> Cr_TraspasoTes_Carga_Buscar(int CodEmpresa, int cod_remesa)
        {
            return _BL.Cr_TraspasoTes_Carga_Buscar(CodEmpresa, cod_remesa);
        }

        [Authorize]
        [HttpPost("Cr_TraspasoTes_Carga_Ejecutar")]
        public ErrorDto Cr_TraspasoTes_Carga_Ejecutar(int CodEmpresa, [FromBody] CargaRequest request, string usuario)
        {
            return _BL.Cr_TraspasoTes_Carga_Ejecutar(CodEmpresa, request, usuario);
        }

        [Authorize]
        [HttpPost("Cr_TraspasoTes_Remesa_Cerrar")]
        public ErrorDto Cr_TraspasoTes_Remesa_Cerrar(int CodEmpresa, int cod_remesa, string usuario)
        {
            return _BL.Cr_TraspasoTes_Remesa_Cerrar(CodEmpresa, cod_remesa, usuario);
        }

        #endregion

        #region trasladar

        [Authorize]
        [HttpGet("Cr_TraspasoTes_Remesas_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> Cr_TraspasoTes_Remesas_Obtener(int CodEmpresa)
        {
            return _BL.Cr_TraspasoTes_Remesas_Obtener(CodEmpresa);
        }

        [Authorize]
        [HttpGet("Cr_TraspasoTesToken_Obtener")]
        public ErrorDto<List<TokenConsultaModel>> Cr_TraspasoTesToken_Obtener(int CodEmpresa, string usuario)
        {
            return _BL.Cr_TraspasoTesToken_Obtener(CodEmpresa, usuario);
        }

        [Authorize]
        [HttpPost("Cr_TraspasoTesToken_Nuevo")]
        public ErrorDto Cr_TraspasoTesToken_Nuevo(int CodEmpresa, string usuario)
        {
            return _BL.Cr_TraspasoTesToken_Nuevo(CodEmpresa, usuario);
        }

        [Authorize]
        [HttpGet("Cr_TraspasoTesTraslado_Buscar")]
        public ErrorDto<List<TraspasoModel>> Cr_TraspasoTesTraslado_Buscar(int CodEmpresa, int cod_remesa)
        {
            return _BL.Cr_TraspasoTesTraslado_Buscar(CodEmpresa, cod_remesa);
        }

        [Authorize]
        [HttpPost("CrTraspasoTes_Traslado_Generar")]
        public ErrorDto CrTraspasoTes_Traslado_Generar(int CodEmpresa, int cod_remesa, string usuario, string? token)
        {
            return _BL.CrTraspasoTes_Traslado_Generar(CodEmpresa, cod_remesa, usuario, token);
        }

        #endregion

        #region informes
        #endregion

        #region reactivaciones

        [Authorize]
        [HttpGet("Cr_TraspasoTes_Reactivacion_Buscar")]
        public ErrorDto<ReactivacionModel> Cr_TraspasoTes_Reactivacion_Buscar(int CodEmpresa, int id_solicitud)
        {
            return _BL.Cr_TraspasoTes_Reactivacion_Buscar(CodEmpresa, id_solicitud);
        }

        [Authorize]
        [HttpPost("Cr_TraspasoTes_Reactivacion_Ejecutar")]
        public ErrorDto Cr_TraspasoTes_Reactivacion_Ejecutar(int CodEmpresa, int id_solicitud, string usuario)
        {
            return _BL.Cr_TraspasoTes_Reactivacion_Ejecutar(CodEmpresa, id_solicitud, usuario);
        }

        #endregion

        #region cambio

        [Authorize]
        [HttpGet("Cr_TraspasoTes_Cambio_Buscar")]
        public ErrorDto<List<CambioConceptoModel>> Cr_TraspasoTes_Cambio_Buscar(int CodEmpresa, int id_solicitud)
        {
            return _BL.Cr_TraspasoTes_Cambio_Buscar(CodEmpresa, id_solicitud);
        }

        [Authorize]
        [HttpPost("Cr_TraspasoTes_Cambio_Ejecutar")]
        public ErrorDto Cr_TraspasoTes_Cambio_Ejecutar(int CodEmpresa, [FromBody] CambioConceptoRequest request, string usuario)
        {
            return _BL.Cr_TraspasoTes_Cambio_Ejecutar(CodEmpresa, request, usuario);
        }

        #endregion

        #region consultas

        [Authorize]
        [HttpGet("Cr_TraspasoTes_Consulta_Operacion")]
        public ErrorDto<ConsultaModel> Cr_TraspasoTes_Consulta_Operacion(int CodEmpresa, int id_solicitud)
        {
            return _BL.Cr_TraspasoTes_Consulta_Operacion(CodEmpresa, id_solicitud);
        }

        #endregion

        #region aux.giro
        #endregion
    }
}