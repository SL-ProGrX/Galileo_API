using Galileo.DataBaseTier.ProGrX_Beneficios;
using Galileo.Models.AF;
using Galileo.Models.ERROR;

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

        /// <summary>Lista de grupos de beneficios.</summary>
        public ErrorDto<BeneficioGrupoDataLista> BeneficioGrupoLista_Obtener(int CodCliente, int? pagina, int? paginacion, string? filtro)
            => _db.BeneficioGrupoLista_Obtener(CodCliente, pagina, paginacion, filtro);

        /// <summary>Lista de usuarios y su pertenencia a un grupo.</summary>
        public ErrorDto<BeneficioUsuariosDataLista> BeneficioUsuariosLista_Obtener(int CodCliente, int? pagina, int? paginacion, string? filtro, string cod_grupo)
            => _db.BeneficioUsuariosLista_Obtener(CodCliente, pagina, paginacion, filtro, cod_grupo);

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
    }
}
