using Galileo.DataBaseTier.ProGrX_Beneficios;
using Galileo.Models;
using Galileo.Models.AF;
using Galileo.Models.ERROR;
using Newtonsoft.Json;

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

        /// <summary>Lista de grupos de beneficios con paginación, filtro y ordenamiento.</summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <param name="filtros">Filtros de carga perezosa serializados en JSON.</param>
        public ErrorDto<AfiBeneGruposLista> AfiBeneGrupos_Obtener(int CodCliente, string? filtros)
            => _db.AfiBeneGrupos_Obtener(CodCliente, DeserializarFiltros(filtros));

        /// <summary>
        /// Convierte el JSON de filtros recibido desde Angular en el modelo de carga perezosa.
        /// </summary>
        /// <param name="filtros">Filtros serializados en JSON.</param>
        /// <returns>Filtros deserializados; instancia vacía si el JSON viene nulo o inválido.</returns>
        private static FiltrosLazyLoadData DeserializarFiltros(string? filtros)
        {
            if (string.IsNullOrWhiteSpace(filtros))
            {
                return new FiltrosLazyLoadData();
            }

            return JsonConvert.DeserializeObject<FiltrosLazyLoadData>(filtros) ?? new FiltrosLazyLoadData();
        }

        /// <summary>Lista de beneficios y su marca de asignación a un grupo.</summary>
        public ErrorDto<AfiBeneGruposAsigandosLista> BeneficioUsuariosLista_Obtener(int CodCliente, int? pagina, int? paginacion, string? filtro, string cod_grupo)
            => _db.BeneficioUsuariosLista_Obtener(CodCliente, pagina, paginacion, filtro, cod_grupo);

        /// <summary>Lista simple de grupos de beneficios.</summary>
        public ErrorDto<List<AfiBeneGrupos>> AfiBeneGrupos_lista(int CodCliente)
            => _db.AfiBeneGrupos_lista(CodCliente);

        /// <summary>Catálogo de categorías de beneficios activas.</summary>
        public ErrorDto<List<AfiBeneLista>> AfiBeneCategoriaLista_Obtener(int CodCliente)
            => _db.AfiBeneCategoriaLista_Obtener(CodCliente);

        /// <summary>Exporta la lista de grupos aplicando el filtro vigente, sin paginar.</summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <param name="filtros">Filtros de carga perezosa serializados en JSON.</param>
        public ErrorDto<List<AfiBeneGrupos>> AfiBeneGrupoExportar(int CodCliente, string? filtros)
            => _db.AfiBeneGrupoExportar(CodCliente, DeserializarFiltros(filtros));

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

        /// <summary>Registra o retira una asignación de grupo por tipo.</summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <param name="request">Datos de la asignación (tipo, grupo, valor, usuario y movimiento).</param>
        public ErrorDto AfiAsignaciones_Actualizar(int CodCliente, AfiAsignacionRequest request)
            => _db.AfiAsignaciones_Actualizar(CodCliente, request);
    }
}
