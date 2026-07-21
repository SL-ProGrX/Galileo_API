using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.TES;
using Galileo_API.BusinessLogic.ProGrX.Bancos;
using Galileo_API.DataBaseTier.ProGrX.Bancos;
using Galileo_API.Services.ProGrX.Bancos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX.Bancos
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FrmTesEmisionDocumentosController : ControllerBase
    {
        private readonly FrmTesEmisionDocumentosBL _bl;
        private readonly TesEmisionDocumentosProcesoQueue _queue;
        private readonly string _archivosRaiz;

        public FrmTesEmisionDocumentosController(
            IConfiguration config,
            TesEmisionDocumentosProcesoQueue queue)
        {
            _bl = new FrmTesEmisionDocumentosBL(config);
            _queue = queue;
            var rutaBase = config["ArchivosGenerados:RutaBase"] ?? string.Empty;
            var subcarpeta = config["TES_EmisionDocumentos:Subcarpeta"]
                ?? "TES_EmisionDocumentos";
            _archivosRaiz = Path.GetFullPath(Path.Combine(rutaBase, subcarpeta));
        }


        [HttpGet("TES_EmisionDocumentos_Ctas_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> TES_EmisionDocumentos_Ctas_Obtener(int CodEmpresa, string usuario)
        {
            return _bl.TES_EmisionDocumentos_Ctas_Obtener(CodEmpresa, usuario);
        }

        [HttpGet("TES_EmisionDocumentos_TiposDocs_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> TES_EmisionDocumentos_TiposDocs_Obtener(int CodEmpresa, string usuario, int banco)
        {
            return _bl.TES_EmisionDocumentos_TiposDocs_Obtener(CodEmpresa, usuario, banco);
        }

        [HttpGet("TES_EmisionDocumento_Formato_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> TES_EmisionDocumento_Formato_Obtener(int CodEmpresa, int banco)
        {
            return _bl.TES_EmisionDocumento_Formato_Obtener(CodEmpresa, banco);
        }

        [HttpGet("TES_EmisionDocumento_Plan_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> TES_EmisionDocumento_Plan_Obtener(int CodEmpresa, int banco)
        {
            return _bl.TES_EmisionDocumento_Plan_Obtener(CodEmpresa, banco);
        }

        [HttpGet("TES_EmisionDocumento_Buscar")]
        public ErrorDto<TesTransaccionesData> TES_EmisionDocumento_Buscar(int CodEmpresa, string tipoDoc, int banco, string plan)
        {
            return _bl.TES_EmisionDocumento_Buscar(CodEmpresa, tipoDoc, banco, plan);
        }

        [HttpGet("TES_EmisionDocumento_Solicitudes_Obtener")]
        public ErrorDto<List<TesSolicitudesGenData>> TES_EmisionDocumento_Solicitudes_Obtener(int CodEmpresa, string filtros)
        {
            return _bl.TES_EmisionDocumento_Solicitudes_Obtener(CodEmpresa, filtros);
        }

        [HttpGet("TES_EmisionDocumento_TipoDocGestion")]
        public ErrorDto<string> TES_EmisionDocumento_TipoDocGestion(int CodEmpresa, int banco, string tipoDoc)
        {
            return _bl.TES_EmisionDocumento_TipoDocGestion(CodEmpresa, banco, tipoDoc);
        }

        [HttpPost("TES_EmisionDocumento_ValidaNumDocumento")]
        public ErrorDto TES_EmisionDocumento_ValidaNumDocumento(int CodEmpresa, int banco, string tipoDoc, int docInicial, int cantidadList)
        {
            return _bl.TES_EmisionDocumento_ValidaNumDocumento(CodEmpresa, banco, tipoDoc, docInicial, cantidadList);
        }

        [HttpPost("TES_EmisionDocumento_RevisaCuentas_SP")]
        public ErrorDto TES_EmisionDocumento_RevisaCuentas_SP(int CodEmpresa, int banco)
        {
            return _bl.TES_EmisionDocumento_RevisaCuentas_SP(CodEmpresa, banco);
        }

        [HttpGet("TES_EmisionDocumento_SolicitudesCtaPuente_Obtener")]
        public ErrorDto<List<TesTransaccionDto>> TES_EmisionDocumento_SolicitudesCtaPuente_Obtener(int CodEmpresa, int banco, string tipoDoc)
        {
            return _bl.TES_EmisionDocumento_SolicitudesCtaPuente_Obtener(CodEmpresa, banco, tipoDoc);
        }

        [HttpGet("TES_EmisionDocumento_CtasPuente_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> TES_EmisionDocumento_CtasPuente_Obtener(int CodEmpresa, string Usuario)
        {
            return _bl.TES_EmisionDocumento_CtasPuente_Obtener(CodEmpresa, Usuario);
        }

        [HttpPost("TES_EmisionDocumento_CtaPuente_Aplicar")]
        public ErrorDto TES_EmisionDocumento_CtaPuente_Aplicar(int CodEmpresa, int Banco, string Usuario, string Solicitudes)
        {
            return _bl.TES_EmisionDocumento_CtaPuente_Aplicar(CodEmpresa, Banco, Usuario, Solicitudes);
        }

        [HttpGet("TES_EmisionDocumento_Generar")]
        public ErrorDto<object> TES_EmisionDocumento_Generar(int CodEmpresa, string filtros)
        {
            return _bl.TES_EmisionDocumento_Generar(CodEmpresa, filtros);
        }

        [HttpPost("TES_EmisionDocumentos_Proceso_Iniciar")]
        public ErrorDto<TesEmisionDocumentosProcesoResult> TES_EmisionDocumentos_Proceso_Iniciar(
            int codEmpresa,
            [FromBody] TesEmisionDocumentosProcesoIniciarRequest request)
        {
            var propietario = ObtenerPropietario();
            var response = _bl.TES_EmisionDocumentos_Proceso_Iniciar(
                codEmpresa,
                propietario,
                request);

            if (response.Code == 0 && response.Result != null)
            {
                _queue.Encolar(new TesEmisionDocumentosProcesoTrabajo
                {
                    CodEmpresa = codEmpresa,
                    ProcesoId = response.Result.procesoId
                });
            }

            return response;
        }

        [HttpGet("TES_EmisionDocumentos_Proceso_Estado")]
        public ErrorDto<TesEmisionDocumentosProcesoResult> TES_EmisionDocumentos_Proceso_Estado(
            int codEmpresa,
            Guid procesoId)
        {
            var response = _bl.TES_EmisionDocumentos_Proceso_Estado_Obtener(
                codEmpresa,
                procesoId,
                ObtenerPropietario());
            if (response.Code == 0 &&
                response.Result?.estado == TesEmisionDocumentosEstado.Pendiente)
            {
                _queue.Encolar(new TesEmisionDocumentosProcesoTrabajo
                {
                    CodEmpresa = codEmpresa,
                    ProcesoId = procesoId
                });
            }
            return response;
        }

        [HttpGet("TES_EmisionDocumentos_Proceso_Resultado")]
        public ErrorDto<TesEmisionDocumentosProcesoManifiestoResult> TES_EmisionDocumentos_Proceso_Resultado(
            int codEmpresa,
            Guid procesoId)
        {
            return _bl.TES_EmisionDocumentos_Proceso_Resultado_Obtener(
                codEmpresa,
                procesoId,
                ObtenerPropietario());
        }

        [HttpGet("TES_EmisionDocumentos_Proceso_Archivo")]
        public IActionResult TES_EmisionDocumentos_Proceso_Archivo(
            int codEmpresa,
            Guid procesoId,
            Guid archivoId)
        {
            var archivo = _bl.TES_EmisionDocumentos_Proceso_Archivo_Obtener(
                codEmpresa,
                procesoId,
                archivoId,
                ObtenerPropietario());
            if (archivo == null || !RutaInternaEsValida(archivo.ruta_interna))
            {
                return NotFound();
            }

            var stream = new FileStream(
                archivo.ruta_interna,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read);
            return File(stream, archivo.content_type, archivo.nombre, enableRangeProcessing: true);
        }

        [HttpGet("ValidaUsuarioEspecial")]
        public ErrorDto<int> ValidaUsuarioEspecial(int CodEmpresa, string usuario)
        {
            return _bl.ValidaUsuarioEspecial(CodEmpresa, usuario);
        }

        private string ObtenerPropietario()
        {
            var propietario = User.Identity?.Name;
            if (string.IsNullOrWhiteSpace(propietario))
            {
                throw new UnauthorizedAccessException("No se pudo identificar al usuario autenticado.");
            }
            return propietario;
        }

        private bool RutaInternaEsValida(string ruta)
        {
            var raiz = _archivosRaiz.EndsWith(Path.DirectorySeparatorChar)
                ? _archivosRaiz
                : _archivosRaiz + Path.DirectorySeparatorChar;
            var rutaCompleta = Path.GetFullPath(ruta);
            return rutaCompleta.StartsWith(raiz, StringComparison.OrdinalIgnoreCase)
                && System.IO.File.Exists(rutaCompleta);
        }
    }
}
