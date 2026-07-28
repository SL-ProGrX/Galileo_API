using Galileo.DataBaseTier.ProGrX_Beneficios;
using Galileo.Models.AF;
using Galileo.Models.ERROR;

namespace Galileo_API.BusinessLogic.ProGrX_Beneficios
{
    /// <summary>
    /// Lógica de negocio de Remesas de Archivo de Beneficios (frmAF_BeneRemesasArchivo).
    /// </summary>
    public class FrmAfBeneRemesasArchivoBL
    {
        private readonly FrmAfBeneRemesasArchivoDB _db;

        public FrmAfBeneRemesasArchivoBL(IConfiguration config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            _db = new FrmAfBeneRemesasArchivoDB(config);
        }

        /// <summary>Tipos de documento activos para remesas.</summary>
        public ErrorDto<List<TipoDocumentosLista>> TipoDocumentos_Obtener(int CodCliente)
            => _db.TipoDocumentos_Obtener(CodCliente);

        /// <summary>Remesas de archivo en estado de origen configurado.</summary>
        public ErrorDto<RmsRemesasDataLista> RemesasArchivo_Obtener(int CodCliente)
            => _db.RemesasArchivo_Obtener(CodCliente);

        /// <summary>Documentos de beneficios elegibles para remesa.</summary>
        public ErrorDto<List<RmsRemesaDocuementos>> RemesaDocumentos_Obtener(int CodCliente, string filtros)
            => _db.RemesaDocumentos_Obtener(CodCliente, filtros);

        /// <summary>Detalle de una remesa específica.</summary>
        public ErrorDto<RmsRemesasDetalleDataLista> RemesaDetalle_Obtener(int CodCliente, int IdRemesa)
            => _db.RemesaDetalle_Obtener(CodCliente, IdRemesa);

        /// <summary>Inserta o actualiza una remesa de archivo.</summary>
        public ErrorDto RemesaArchivo_Guardar(int CodCliente, RmsRemesasData remesa)
            => _db.RemesaArchivo_Guardar(CodCliente, remesa);

        /// <summary>Inserta el detalle de documentos de una remesa.</summary>
        public ErrorDto RemesaDetalle_Guardar(int CodCliente, int idRemesa, string usuario, List<RmsRemesaDocuementos> documentos)
            => _db.RemesaDetalle_Guardar(CodCliente, idRemesa, usuario, documentos);
    }
}
