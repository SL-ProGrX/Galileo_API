using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX.Credito;

namespace Galileo_API.DataBaseTier.ProGrX.Creditos
{
    public partial class FrmCrGruposTrabajoDB
    {
        /// <summary>
        /// Obtiene las etiquetas y marca las ya vinculadas al grupo indicado.
        /// </summary>
        public ErrorDto<List<CrGrupoTrabajoEtiquetaData>> CR_GruposTrabajo_Etiquetas_Obtener(
            int codEmpresa,
            string codGrupo)
        {
            var grupo = CR_GruposTrabajo_NormalizarTexto(codGrupo);
            var err = CR_GruposTrabajo_ValidarRequerido(grupo, "Debe indicar el grupo.");
            if (err != null)
                return DbHelper.CreateErrorResponse<List<CrGrupoTrabajoEtiquetaData>>(err.Description!, err.Code ?? 0);

            using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);
            var grupoErr = CR_GruposTrabajo_ValidarGrupoExiste(conn, grupo);

            if (grupoErr != null)
                return DbHelper.CreateErrorResponse<List<CrGrupoTrabajoEtiquetaData>>(grupoErr.Description!, grupoErr.Code ?? 0);

            const string sql = @"
                select
                    rtrim(T.TAG_CODIGO) as tag_codigo,
                    rtrim(isnull(T.DESCRIPCION, '')) as descripcion,
                    case when TG.TAG_CODIGO is not null then cast(1 as bit) else cast(0 as bit) end as asignado
                from CRD_TAGS T
                left join CRD_TAGS_GRUPOS TG
                    on TG.TAG_CODIGO = T.TAG_CODIGO
                   and TG.COD_GRUPO = @cod_grupo
                order by asignado desc, T.DESCRIPCION;";

            return DbHelper.ExecuteListQuery<CrGrupoTrabajoEtiquetaData>(
                _portalDb,
                codEmpresa,
                sql,
                new { cod_grupo = grupo });
        }

        /// <summary>
        /// Asigna o excluye una etiqueta del grupo indicado.
        /// </summary>
        public ErrorDto CR_GruposTrabajo_Etiquetas_Marcar(
            int codEmpresa,
            CrGrupoTrabajoEtiquetaMarcarRequest request)
        {
            request ??= new CrGrupoTrabajoEtiquetaMarcarRequest();

            var usuarioSesion = CR_GruposTrabajo_NormalizarTexto(request.usuario_sesion);
            var codGrupo = CR_GruposTrabajo_NormalizarTexto(request.cod_grupo);
            var tagCodigo = CR_GruposTrabajo_NormalizarTexto(request.tag_codigo);

            var err = CR_GruposTrabajo_ValidarRequerido(usuarioSesion, "Debe indicar el usuario de sesión.");
            if (err != null) return err;

            err = CR_GruposTrabajo_ValidarRequerido(codGrupo, "Debe indicar el grupo.");
            if (err != null) return err;

            err = CR_GruposTrabajo_ValidarRequerido(tagCodigo, "Debe indicar la etiqueta a procesar.");
            if (err != null) return err;

            using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);

            try
            {
                var grupoErr = CR_GruposTrabajo_ValidarGrupoExiste(conn, codGrupo);
                if (grupoErr != null) return grupoErr;

                if (request.marcado)
                {
                    const string sqlInsert = @"
                        if not exists (
                            select 1
                            from crd_tags_grupos
                            where tag_codigo = @tag_codigo
                              and cod_grupo = @cod_grupo
                        )
                        begin
                            insert into crd_tags_grupos(tag_codigo, cod_grupo)
                            values(@tag_codigo, @cod_grupo)
                        end;";

                    conn.Execute(sqlInsert, new
                    {
                        tag_codigo = tagCodigo,
                        cod_grupo = codGrupo
                    });

                    CR_GruposTrabajo_RegistrarBitacora(
                        codEmpresa,
                        usuarioSesion,
                        "Registra - WEB",
                        $"Grupo Trabajo {codGrupo} > Asigna etiqueta: {tagCodigo}");
                }
                else
                {
                    const string sqlDelete = @"
                        delete from crd_tags_grupos
                        where tag_codigo = @tag_codigo
                          and cod_grupo = @cod_grupo;";

                    conn.Execute(sqlDelete, new
                    {
                        tag_codigo = tagCodigo,
                        cod_grupo = codGrupo
                    });

                    CR_GruposTrabajo_RegistrarBitacora(
                        codEmpresa,
                        usuarioSesion,
                        "Elimina - WEB",
                        $"Grupo Trabajo {codGrupo} > Excluye etiqueta: {tagCodigo}");
                }

                return DbHelper.CreateOkResponse();
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }
    }
}
