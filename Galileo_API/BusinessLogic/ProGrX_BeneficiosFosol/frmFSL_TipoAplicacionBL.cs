using Galileo.DataBaseTier.ProGrX_BeneficiosFosol;
using Galileo.Models.ERROR;
using Galileo.Models.FSL;

namespace Galileo_API.BusinessLogic.ProGrX_BeneficiosFosol
{
    /// <summary>
    /// Lógica de negocio de los Tipos de Aplicación Fosol (frmFSL_TipoAplicacion): planes y causas.
    /// </summary>
    public class FrmFslTipoAplicacionBL
    {
        private readonly FrmFslTipoAplicacionDB _db;

        public FrmFslTipoAplicacionBL(IConfiguration config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            _db = new FrmFslTipoAplicacionDB(config);
        }

        /// <summary>Causas de un plan.</summary>
        public ErrorDto<CausasDataLista> Causas_Obtener(int CodCliente, string TipoCausa, string Jfiltro)
            => _db.Causas_Obtener(CodCliente, TipoCausa, Jfiltro);

        /// <summary>Exporta la lista completa de causas de un plan.</summary>
        public ErrorDto<List<TiposCausaData>> CausasListas_Exportar(int CodCliente, string TipoCausa)
            => _db.CausasListas_Exportar(CodCliente, TipoCausa);

        /// <summary>Planes Fosol.</summary>
        public ErrorDto<PlanesDataLista> Planes_Obtener(int CodCliente, string Jfiltro)
            => _db.Planes_Obtener(CodCliente, Jfiltro);

        /// <summary>Exporta la lista completa de planes.</summary>
        public ErrorDto<List<ListaPlanesData>> PlanesLista_Exportar(int CodCliente)
            => _db.PlanesLista_Exportar(CodCliente);

        /// <summary>Lista simple de planes activos.</summary>
        public ErrorDto<List<ListaPlanesData>> ListaPlanes_Obtener(int CodCliente)
            => _db.ListaPlanes_Obtener(CodCliente);

        /// <summary>Inserta un plan (o actualiza si existe).</summary>
        public ErrorDto Planes_Insertar(int CodCliente, PlanDataInsert planData)
            => _db.Planes_Insertar(CodCliente, planData);

        /// <summary>Elimina un plan.</summary>
        public ErrorDto Planes_Eliminar(int CodCliente, string cod_plan)
            => _db.Planes_Eliminar(CodCliente, cod_plan);

        /// <summary>Inserta una causa (o actualiza si existe).</summary>
        public ErrorDto Causas_Insertar(int CodCliente, CausaDataInsert causaData)
            => _db.Causas_Insertar(CodCliente, causaData);

        /// <summary>Actualiza una causa.</summary>
        public ErrorDto Causas_Actualizar(int CodCliente, CausaDataInsert causaData)
            => _db.Causas_Actualizar(CodCliente, causaData);

        /// <summary>Elimina una causa.</summary>
        public ErrorDto Causas_Eliminar(int CodCliente, string cod_causa, string cod_plan)
            => _db.Causas_Eliminar(CodCliente, cod_causa, cod_plan);
    }
}
