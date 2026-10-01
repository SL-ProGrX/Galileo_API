using Galileo.Models.ERROR;
using Galileo.Models.FSL;
using Galileo_API.BusinessLogic.ProGrX_BeneficiosFosol;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX_BeneficiosFosol
{
    [Route("api/frmFSL_RemesasPago")]
    [Authorize]
    [ApiController]
    public sealed class FrmFslRemesasPagoController
        : ControllerBase
    {
        private readonly FrmFslRemesasPagoBL _bl;

        public FrmFslRemesasPagoController(
            IConfiguration config)
        {
            ArgumentNullException.ThrowIfNull(config);
            _bl = new FrmFslRemesasPagoBL(config);
        }

        [HttpGet(
            "FSL_RemesasPago_Remesa_Obtener")]
        public ErrorDto<FslRemesaDto?>
            FSL_RemesasPago_Remesa_Obtener(
                int CodEmpresa,
                long codRemesa)
        {
            return _bl
                .FSL_RemesasPago_Remesa_Obtener(
                    CodEmpresa,
                    codRemesa);
        }

        [HttpGet(
            "FSL_RemesasPago_Remesas_Obtener")]
        public ErrorDto<
            FslListaPaginadaDto<FslRemesaDto>>
            FSL_RemesasPago_Remesas_Obtener(
                int CodEmpresa,
                string filtros = "")
        {
            return _bl
                .FSL_RemesasPago_Remesas_Obtener(
                    CodEmpresa,
                    filtros);
        }

        [HttpGet(
            "FSL_RemesasPago_Cargas_Obtener")]
        public ErrorDto<List<FslRemesaDto>>
            FSL_RemesasPago_Cargas_Obtener(
                int CodEmpresa)
        {
            return _bl
                .FSL_RemesasPago_Cargas_Obtener(
                    CodEmpresa);
        }

        [HttpGet(
            "FSL_RemesasPago_CargasLista_Obtener")]
        public ErrorDto<
            FslListaPaginadaDto<FslExpedienteRemesaDto>>
            FSL_RemesasPago_CargasLista_Obtener(
                int CodEmpresa,
                string filtros = "")
        {
            return _bl
                .FSL_RemesasPago_CargasLista_Obtener(
                    CodEmpresa,
                    filtros);
        }

        [HttpGet(
            "FSL_RemesasPago_Traslados_Obtener")]
        public ErrorDto<List<FslRemesaDto>>
            FSL_RemesasPago_Traslados_Obtener(
                int CodEmpresa)
        {
            return _bl
                .FSL_RemesasPago_Traslados_Obtener(
                    CodEmpresa);
        }

        [HttpGet(
            "FSL_RemesasPago_TrasladoLista_Obtener")]
        public ErrorDto<List<FslExpedienteRemesaDto>>
            FSL_RemesasPago_TrasladoLista_Obtener(
                int CodEmpresa,
                long codRemesa)
        {
            return _bl
                .FSL_RemesasPago_TrasladoLista_Obtener(
                    CodEmpresa,
                    codRemesa);
        }

        [HttpPost(
            "FSL_RemesasPago_Remesa_Registrar")]
        public ErrorDto
            FSL_RemesasPago_Remesa_Registrar(
                int CodEmpresa,
                FslRemesaGuardarRequest request)
        {
            return _bl
                .FSL_RemesasPago_Remesa_Registrar(
                    CodEmpresa,
                    request);
        }

        [HttpPut(
            "FSL_RemesasPago_Remesa_Actualizar")]
        public ErrorDto
            FSL_RemesasPago_Remesa_Actualizar(
                int CodEmpresa,
                FslRemesaGuardarRequest request)
        {
            return _bl
                .FSL_RemesasPago_Remesa_Actualizar(
                    CodEmpresa,
                    request);
        }

        [HttpDelete(
            "FSL_RemesasPago_Remesa_Eliminar")]
        public ErrorDto
            FSL_RemesasPago_Remesa_Eliminar(
                int CodEmpresa,
                long codRemesa,
                string usuario)
        {
            return _bl
                .FSL_RemesasPago_Remesa_Eliminar(
                    CodEmpresa,
                    codRemesa,
                    usuario);
        }

        [HttpPost(
            "FSL_RemesasPago_Remesa_Cerrar")]
        public ErrorDto
            FSL_RemesasPago_Remesa_Cerrar(
                int CodEmpresa,
                FslRemesaCerrarRequest request)
        {
            return _bl
                .FSL_RemesasPago_Remesa_Cerrar(
                    CodEmpresa,
                    request);
        }

        [HttpPost(
            "FSL_RemesasPago_Cargas_Aplicar")]
        public ErrorDto
            FSL_RemesasPago_Cargas_Aplicar(
                int CodEmpresa,
                FslRemesaAplicarRequest request)
        {
            return _bl
                .FSL_RemesasPago_Cargas_Aplicar(
                    CodEmpresa,
                    request);
        }

        [HttpPost(
            "FSL_RemesasPago_Traslado_Aplicar")]
        public ErrorDto
            FSL_RemesasPago_Traslado_Aplicar(
                int CodEmpresa,
                FslRemesaAplicarRequest request)
        {
            return _bl
                .FSL_RemesasPago_Traslado_Aplicar(
                    CodEmpresa,
                    request);
        }
    }
}