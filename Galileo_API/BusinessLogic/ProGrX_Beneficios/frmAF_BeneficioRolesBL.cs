using Galileo.DataBaseTier.ProGrX_Beneficios;
using Galileo.Models;
using Galileo.Models.AF;
using Galileo.Models.ERROR;
using Newtonsoft.Json;

namespace Galileo_API.BusinessLogic.ProGrX_Beneficios
{
    /// <summary>
    /// Lógica de negocio de Roles/Grupos de Beneficios (frmAF_BeneficioRoles).
    /// </summary>
    public class FrmAfBeneficioRolesBL
    {
        private readonly FrmAfBeneficioRolesDB _db;

        public FrmAfBeneficioRolesBL(IConfiguration config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            _db = new FrmAfBeneficioRolesDB(config);
        }

        /// <summary>Lista de grupos de beneficios con paginación, filtro y ordenamiento.</summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <param name="filtros">Filtros de carga perezosa serializados en JSON.</param>
        public ErrorDto<BeneficioGrupoDataLista> BeneficioGrupoLista_Obtener(int CodCliente, string? filtros)
            => _db.BeneficioGrupoLista_Obtener(CodCliente, DeserializarFiltros(filtros));

        /// <summary>Exporta la lista de grupos aplicando el filtro vigente, sin paginar.</summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <param name="filtros">Filtros de carga perezosa serializados en JSON.</param>
        public ErrorDto<List<BeneficioGrupoData>> BeneficioGrupo_Exportar(int CodCliente, string? filtros)
            => _db.BeneficioGrupo_Exportar(CodCliente, DeserializarFiltros(filtros));

        /// <summary>Lista de usuarios y su pertenencia a un grupo.</summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <param name="cod_grupo">Código del grupo.</param>
        /// <param name="filtros">Filtros de carga perezosa serializados en JSON.</param>
        public ErrorDto<BeneficioUsuariosDataLista> BeneficioUsuariosLista_Obtener(
            int CodCliente, string cod_grupo, string? filtros)
            => _db.BeneficioUsuariosLista_Obtener(CodCliente, cod_grupo, DeserializarFiltros(filtros));

        /// <summary>Lista completa de grupos de beneficios.</summary>
        public ErrorDto<List<BeneficioGrupoData>> BeneficioGrupoData_Obtener(int CodCliente)
            => _db.BeneficioGrupoData_Obtener(CodCliente);

        /// <summary>Asocia un usuario a un grupo de beneficios.</summary>
        public ErrorDto GrupoUsuario_Insertar(int CodCliente, string usuario, string cod_grupo)
            => _db.GrupoUsuario_Insertar(CodCliente, usuario, cod_grupo);

        /// <summary>Desasocia un usuario de un grupo de beneficios.</summary>
        public ErrorDto GrupoUsuario_Eliminar(int CodCliente, string usuario, string cod_grupo)
            => _db.GrupoUsuario_Eliminar(CodCliente, usuario, cod_grupo);

        /// <summary>Guarda un grupo de beneficios (inserta o actualiza).</summary>
        public ErrorDto BeneficioGrupo_Guardar(int CodCliente, BeneficioGrupoData grupo)
            => _db.BeneficioGrupo_Guardar(CodCliente, grupo);

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
    }
}
