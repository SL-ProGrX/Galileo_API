using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX.Credito;

namespace Galileo_API.DataBaseTier.ProGrX.Creditos
{
    public partial class FrmCrGruposTrabajoDB
    {
        /// <summary>
        /// Obtiene los comités y marca los ya vinculados al grupo indicado.
        /// </summary>
        public ErrorDto<List<CrGrupoTrabajoComiteData>> CR_GruposTrabajo_Comites_Obtener(
            int codEmpresa,
            string codGrupo)
        {
            var grupo = CR_GruposTrabajo_NormalizarTexto(codGrupo);
            var err = CR_GruposTrabajo_ValidarRequerido(grupo, "Debe indicar el grupo.");
            if (err != null)
                return DbHelper.CreateErrorResponse<List<CrGrupoTrabajoComiteData>>(err.Description!, err.Code ?? 0);

            using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);
            var grupoErr = CR_GruposTrabajo_ValidarGrupoExiste(conn, grupo);

            if (grupoErr != null)
                return DbHelper.CreateErrorResponse<List<CrGrupoTrabajoComiteData>>(grupoErr.Description!, grupoErr.Code ?? 0);

            const string sql = @"
                select
                    C.ID_COMITE as id_comite,
                    rtrim(isnull(C.DESCRIPCION, '')) as descripcion,
                    case when CG.ID_COMITE is not null then cast(1 as bit) else cast(0 as bit) end as asignado
                from COMITES C
                left join CRD_COMITES_GRUPOS CG
                    on CG.ID_COMITE = C.ID_COMITE
                   and CG.COD_GRUPO = @cod_grupo
                order by asignado desc, C.DESCRIPCION;";

            return DbHelper.ExecuteListQuery<CrGrupoTrabajoComiteData>(
                _portalDb,
                codEmpresa,
                sql,
                new { cod_grupo = grupo });
        }

        /// <summary>
        /// Asigna o excluye un comité del grupo indicado.
        /// </summary>
        public ErrorDto CR_GruposTrabajo_Comites_Marcar(
            int codEmpresa,
            CrGrupoTrabajoComiteMarcarRequest request)
        {
            request ??= new CrGrupoTrabajoComiteMarcarRequest();

            var usuarioSesion = CR_GruposTrabajo_NormalizarTexto(request.usuario_sesion);
            var codGrupo = CR_GruposTrabajo_NormalizarTexto(request.cod_grupo);

            var err = CR_GruposTrabajo_ValidarRequerido(usuarioSesion, "Debe indicar el usuario de sesión.");
            if (err != null) return err;

            err = CR_GruposTrabajo_ValidarRequerido(codGrupo, "Debe indicar el grupo.");
            if (err != null) return err;

            if (request.id_comite <= 0)
            {
                return DbHelper.ErrorResponse("Debe indicar el comité a procesar.");
            }

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
                            from crd_comites_grupos
                            where id_comite = @id_comite
                              and cod_grupo = @cod_grupo
                        )
                        begin
                            insert into crd_comites_grupos(id_comite, cod_grupo)
                            values(@id_comite, @cod_grupo)
                        end;";

                    conn.Execute(sqlInsert, new
                    {
                        id_comite = request.id_comite,
                        cod_grupo = codGrupo
                    });

                    CR_GruposTrabajo_RegistrarBitacora(
                        codEmpresa,
                        usuarioSesion,
                        "Registra - WEB",
                        $"Grupo Trabajo {codGrupo} > Asigna comité: {request.id_comite}");
                }
                else
                {
                    const string sqlDelete = @"
                        delete from crd_comites_grupos
                        where id_comite = @id_comite
                          and cod_grupo = @cod_grupo;";

                    conn.Execute(sqlDelete, new
                    {
                        id_comite = request.id_comite,
                        cod_grupo = codGrupo
                    });

                    CR_GruposTrabajo_RegistrarBitacora(
                        codEmpresa,
                        usuarioSesion,
                        "Elimina - WEB",
                        $"Grupo Trabajo {codGrupo} > Excluye comité: {request.id_comite}");
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
