using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.Models.AF;
using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX_Beneficios;

namespace Galileo_API.Controllers.ProGrX_Beneficios
{
    /// <summary>
    /// Endpoints de Remesas de Archivo de Beneficios (frmAF_BeneRemesasArchivo).
    /// </summary>
    [Route("api/frmAF_BeneRemesasArchivo")]
    [ApiController]
    public class FrmAfBeneRemesasArchivoController : ControllerBase
    {
        private readonly FrmAfBeneRemesasArchivoBL _bl;

        public FrmAfBeneRemesasArchivoController(IConfiguration config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            _bl = new FrmAfBeneRemesasArchivoBL(config);
        }

        /// <summary>Tipos de documento activos para remesas.</summary>
        [Authorize]
        [HttpGet("TipoDocumentos_Obtener")]
        public ErrorDto<List<TipoDocumentosLista>> TipoDocumentos_Obtener(int CodCliente)
            => _bl.TipoDocumentos_Obtener(CodCliente);

        /// <summary>Remesas de archivo en estado de origen configurado.</summary>
        [Authorize]
        [HttpGet("RemesasArchivo_Obtener")]
        public ErrorDto<RmsRemesasDataLista> RemesasArchivo_Obtener(int CodCliente)
            => _bl.RemesasArchivo_Obtener(CodCliente);

        /// <summary>Documentos de beneficios elegibles para remesa.</summary>
        [Authorize]
        [HttpGet("RemesaDocumentos_Obtener")]
        public ErrorDto<List<RmsRemesaDocuementos>> RemesaDocumentos_Obtener(int CodCliente, string filtros)
            => _bl.RemesaDocumentos_Obtener(CodCliente, filtros);

        /// <summary>Detalle de una remesa específica.</summary>
        [Authorize]
        [HttpGet("RemesaDetalle_Obtener")]
        public ErrorDto<RmsRemesasDetalleDataLista> RemesaDetalle_Obtener(int CodCliente, int IdRemesa)
            => _bl.RemesaDetalle_Obtener(CodCliente, IdRemesa);

        /// <summary>Inserta o actualiza una remesa de archivo.</summary>
        [Authorize]
        [HttpPost("RemesaArchivo_Guardar")]
        public ErrorDto RemesaArchivo_Guardar(int CodCliente, [FromBody] RmsRemesasData remesa)
            => _bl.RemesaArchivo_Guardar(CodCliente, remesa);

        /// <summary>Inserta el detalle de documentos de una remesa.</summary>
        [Authorize]
        [HttpPost("RemesaDetalle_Guardar")]
        public ErrorDto RemesaDetalle_Guardar(int CodCliente, int idRemesa, string usuario, [FromBody] List<RmsRemesaDocuementos> documentos)
            => _bl.RemesaDetalle_Guardar(CodCliente, idRemesa, usuario, documentos);
    }
}
