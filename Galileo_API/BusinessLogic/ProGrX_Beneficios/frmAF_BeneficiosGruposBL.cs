using Galileo.DataBaseTier.ProGrX_Beneficios;
using Galileo.Models.AF;
using Galileo.Models.ERROR;

namespace Galileo_API.BusinessLogic.ProGrX_Beneficios
{
    /// <summary>
    /// Lógica de negocio de Grupos de Beneficios (frmAF_BeneficiosGrupos).
    /// </summary>
    public class FrmAfBeneficiosGruposBL
    {
        private readonly FrmAfBeneficiosGruposDB _db;

        public FrmAfBeneficiosGruposBL(IConfiguration config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            _db = new FrmAfBeneficiosGruposDB(config);
        }

        /// <summary>Lista de grupos de beneficios.</summary>
        public ErrorDto<AfiBeneGruposLista> AfiBeneGrupos_Obtener(int CodCliente, int? pagina, int? paginacion, string? filtro)
            => _db.AfiBeneGrupos_Obtener(CodCliente, pagina, paginacion, filtro);

        /// <summary>Lista de beneficios y su marca de asignación a un grupo.</summary>
        public ErrorDto<AfiBeneGruposAsigandosLista> BeneficioUsuariosLista_Obtener(int CodCliente, int? pagina, int? paginacion, string? filtro, string cod_grupo)
            => _db.BeneficioUsuariosLista_Obtener(CodCliente, pagina, paginacion, filtro, cod_grupo);

        /// <summary>Lista simple de grupos de beneficios.</summary>
        public ErrorDto<List<AfiBeneGrupos>> AfiBeneGrupos_lista(int CodCliente)
            => _db.AfiBeneGrupos_lista(CodCliente);

        /// <summary>Catálogo de categorías de beneficios activas.</summary>
        public ErrorDto<List<AfiBeneLista>> AfiBeneCategoriaLista_Obtener(int CodCliente)
            => _db.AfiBeneCategoriaLista_Obtener(CodCliente);

        /// <summary>Exporta la lista completa de grupos.</summary>
        public ErrorDto<List<AfiBeneGrupos>> AfiBeneGrupoExportar(int CodCliente)
            => _db.AfiBeneGrupoExportar(CodCliente);

        /// <summary>Obtiene las asignaciones de un grupo por tipo.</summary>
        public ErrorDto<List<AfiBeneAsignacionesData>> AfiAsignaciones_Obtener(int CodCliente, int asigna, string grupo)
            => _db.AfiAsignaciones_Obtener(CodCliente, asigna, grupo);

        /// <summary>Guarda un grupo de beneficios (inserta o actualiza).</summary>
        public ErrorDto AfiBeneGrupo_Guardar(int CodCliente, AfiBeneGrupos grupo)
            => _db.AfiBeneGrupo_Guardar(CodCliente, grupo);

        /// <summary>Elimina un grupo de beneficios.</summary>
        public ErrorDto AfiBeneGrupos_Eliminar(int CodCliente, int cod_grupo)
            => _db.AfiBeneGrupos_Eliminar(CodCliente, cod_grupo);

        /// <summary>Asocia un beneficio a un grupo.</summary>
        public ErrorDto AfiGrupoBeneficio_Insertar(int CodCliente, AfiGrupoBeneficioData grupo)
            => _db.AfiGrupoBeneficio_Insertar(CodCliente, grupo);

        /// <summary>Desasocia un beneficio de un grupo.</summary>
        public ErrorDto AfiGrupoBeneficio_Eliminar(int CodCliente, AfiGrupoBeneficioData grupo)
            => _db.AfiGrupoBeneficio_Eliminar(CodCliente, grupo);

        /// <summary>Registra una asignación de grupo por tipo.</summary>
        public ErrorDto AfiAsignaciones_Actualizar(int CodCliente, int asigna, string grupo, string valor, string usuario, string mov)
            => _db.AfiAsignaciones_Actualizar(CodCliente, asigna, grupo, valor, usuario, mov);
    }
}
