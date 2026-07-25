using Galileo.DataBaseTier.ProGrX_Beneficios;
using Galileo.Models.AF;
using Galileo.Models.ERROR;

namespace Galileo_API.BusinessLogic.ProGrX_Beneficios
{
    /// <summary>
    /// Lógica de negocio del catálogo de Tipos de Sanciones de Beneficios (frmAF_Bene_Sanciones_Tipos).
    /// </summary>
    public class FrmAfBeneSancionesTiposBL
    {
        private readonly FrmAfBeneSancionesTiposDB _db;

        public FrmAfBeneSancionesTiposBL(IConfiguration config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            _db = new FrmAfBeneSancionesTiposDB(config);
        }

        /// <summary>Lista de tipos de sanciones.</summary>
        public ErrorDto<AfTipoSancionesDtoLista> afBeneTipoSancionObtener(int CodCliente, string filtros)
            => _db.afBeneTipoSancionObtener(CodCliente, filtros);

        /// <summary>Catálogo de retenciones disponibles.</summary>
        public ErrorDto<List<BeneListaRetencion>> BeneRetenciones_Obtener(int CodCliente)
            => _db.BeneRetenciones_Obtener(CodCliente);

        /// <summary>Inserta un tipo de sanción (o actualiza si existe).</summary>
        public ErrorDto AfBeneTipoSancion_Insertar(int CodCliente, AfTipoSancionesDto tipo_sancion)
            => _db.AfBeneTipoSancion_Insertar(CodCliente, tipo_sancion);

        /// <summary>Actualiza un tipo de sanción.</summary>
        public ErrorDto AfBeneTipoSancion_Actualizar(int CodCliente, AfTipoSancionesDto tipo_sancion)
            => _db.AfBeneTipoSancion_Actualizar(CodCliente, tipo_sancion);

        /// <summary>Elimina un tipo de sanción.</summary>
        public ErrorDto AfBeneTipoSancion_Eliminar(int CodCliente, int tipo_sancion)
            => _db.AfBeneTipoSancion_Eliminar(CodCliente, tipo_sancion);
    }
}
