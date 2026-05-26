using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX.Credito;

namespace Galileo_API.DataBaseTier.ProGrX.Creditos
{
    public partial class FrmCrGruposTrabajoDB
    {
        /// <summary>
        /// Obtiene la definición de grupos de trabajo.
        /// </summary>
        public ErrorDto<List<CrGrupoTrabajoGrupoData>> CR_GruposTrabajo_Grupos_Obtener(int codEmpresa)
        {
            const string sql = @"
                select
                    rtrim(cod_grupo) as cod_grupo,
                    rtrim(isnull(descripcion, '')) as descripcion
                from crd_grupos
                order by cod_grupo;";

            return DbHelper.ExecuteListQuery<CrGrupoTrabajoGrupoData>(_portalDb, codEmpresa, sql);
        }

        /// <summary>
        /// Inserta o actualiza un grupo de trabajo según exista o no en CRD_GRUPOS.
        /// </summary>
        public ErrorDto CR_GruposTrabajo_Grupos_Guardar(
            int codEmpresa,
            CrGrupoTrabajoGrupoGuardarRequest request)
        {
            request ??= new CrGrupoTrabajoGrupoGuardarRequest();

            var usuario = CR_GruposTrabajo_NormalizarTexto(request.usuario);
            var codGrupo = CR_GruposTrabajo_NormalizarTexto(request.cod_grupo);
            var descripcion = CR_GruposTrabajo_NormalizarTexto(request.descripcion);

            var err = CR_GruposTrabajo_ValidarRequerido(usuario, "Debe indicar el usuario.");
            if (err != null) return err;

            err = CR_GruposTrabajo_ValidarRequerido(codGrupo, "Debe indicar el código del grupo.");
            if (err != null) return err;

            err = CR_GruposTrabajo_ValidarRequerido(descripcion, "Debe indicar la descripción del grupo.");
            if (err != null) return err;

            using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);

            try
            {
                const string sqlExiste = @"
                    select isnull(count(*), 0)
                    from crd_grupos
                    where cod_grupo = @cod_grupo;";

                var existe = conn.ExecuteScalar<int>(sqlExiste, new { cod_grupo = codGrupo });

                if (existe == 0)
                {
                    const string sqlInsert = @"
                        insert into crd_grupos(cod_grupo, descripcion)
                        values(@cod_grupo, @descripcion);";

                    conn.Execute(sqlInsert, new
                    {
                        cod_grupo = codGrupo,
                        descripcion
                    });

                    CR_GruposTrabajo_RegistrarBitacora(
                        codEmpresa,
                        usuario,
                        "Registra - WEB",
                        $"Grupo de Trabajo: {codGrupo}");
                }
                else
                {
                    const string sqlUpdate = @"
                        update crd_grupos
                        set descripcion = @descripcion
                        where cod_grupo = @cod_grupo;";

                    conn.Execute(sqlUpdate, new
                    {
                        cod_grupo = codGrupo,
                        descripcion
                    });

                    CR_GruposTrabajo_RegistrarBitacora(
                        codEmpresa,
                        usuario,
                        "Modifica - WEB",
                        $"Grupo de Trabajo: {codGrupo}");
                }

                return DbHelper.OkResponse(GuardadoExitoso);
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }
    }
}
