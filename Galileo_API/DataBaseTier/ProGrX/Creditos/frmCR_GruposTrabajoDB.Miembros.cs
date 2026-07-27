using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX.Credito;

namespace Galileo_API.DataBaseTier.ProGrX.Creditos
{
    public partial class FrmCrGruposTrabajoDB
    {
        /// <summary>
        /// Obtiene los usuarios activos y marca los ya asignados al grupo indicado.
        /// </summary>
        public ErrorDto<List<CrGrupoTrabajoMiembroData>> CR_GruposTrabajo_Miembros_Obtener(
            int codEmpresa,
            string codGrupo)
        {
            var grupo = CR_GruposTrabajo_NormalizarTexto(codGrupo);
            var err = CR_GruposTrabajo_ValidarRequerido(grupo, "Debe indicar el grupo.");
            if (err != null)
                return DbHelper.CreateErrorResponse<List<CrGrupoTrabajoMiembroData>>(err.Description, err.Code ?? 0);

            using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);
            var grupoErr = CR_GruposTrabajo_ValidarGrupoExiste(conn, grupo);

            if (grupoErr != null)
                return DbHelper.CreateErrorResponse<List<CrGrupoTrabajoMiembroData>>(grupoErr.Description, grupoErr.Code ?? 0);

            const string sql = @"
                select
                    rtrim(U.nombre) as nombre,
                    rtrim(isnull(U.descripcion, '')) as descripcion,
                    case when A.usuario is not null then cast(1 as bit) else cast(0 as bit) end as asignado
                from Usuarios U
                left join crd_grpusers A
                    on U.nombre = A.usuario
                   and A.cod_grupo = @cod_grupo
                where U.estado = 'A'
                order by asignado desc, U.nombre asc;";

            return DbHelper.ExecuteListQuery<CrGrupoTrabajoMiembroData>(
                _portalDb,
                codEmpresa,
                sql,
                new { cod_grupo = grupo });
        }

        /// <summary>
        /// Asigna o excluye un usuario del grupo indicado.
        /// </summary>
        public ErrorDto CR_GruposTrabajo_Miembros_Marcar(
            int codEmpresa,
            CrGrupoTrabajoMiembroMarcarRequest request)
        {
            request ??= new CrGrupoTrabajoMiembroMarcarRequest();

            var usuarioSesion = CR_GruposTrabajo_NormalizarTexto(request.usuario_sesion);
            var codGrupo = CR_GruposTrabajo_NormalizarTexto(request.cod_grupo);
            var usuario = CR_GruposTrabajo_NormalizarTexto(request.usuario);

            var err = CR_GruposTrabajo_ValidarRequerido(usuarioSesion, "Debe indicar el usuario de sesión.");
            if (err != null) return err;

            err = CR_GruposTrabajo_ValidarRequerido(codGrupo, "Debe indicar el grupo.");
            if (err != null) return err;

            err = CR_GruposTrabajo_ValidarRequerido(usuario, "Debe indicar el usuario a procesar.");
            if (err != null) return err;

            using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);

            try
            {
                var grupoErr = CR_GruposTrabajo_ValidarGrupoExiste(conn, codGrupo);
                if (grupoErr != null) return grupoErr;

                if (request.marcado)
                {
                    const string sqlOtroGrupo = @"
                        select isnull(count(*), 0)
                        from crd_grpusers
                        where cod_grupo <> @cod_grupo
                          and usuario = @usuario;";

                    var existeEnOtroGrupo = conn.ExecuteScalar<int>(sqlOtroGrupo, new
                    {
                        cod_grupo = codGrupo,
                        usuario
                    });

                    if (existeEnOtroGrupo > 0)
                    {
                        return DbHelper.ErrorResponse(
                            "El usuario ya ha sido asignado a otro grupo, proceda a excluirlo primero del otro grupo antes de agregarlo a este.");
                    }

                    const string sqlInsert = @"
                        if not exists (
                            select 1
                            from crd_grpusers
                            where cod_grupo = @cod_grupo
                              and usuario = @usuario
                        )
                        begin
                            insert into crd_grpusers(cod_grupo, usuario)
                            values(@cod_grupo, @usuario)
                        end;";

                    conn.Execute(sqlInsert, new
                    {
                        cod_grupo = codGrupo,
                        usuario
                    });

                    CR_GruposTrabajo_RegistrarBitacora(
                        codEmpresa,
                        usuarioSesion,
                        "Registra - WEB",
                        $"Grupo Trabajo {codGrupo} > Asigna miembro: {usuario}");
                }
                else
                {
                    const string sqlDelete = @"
                        delete from crd_grpusers
                        where cod_grupo = @cod_grupo
                          and usuario = @usuario;";

                    conn.Execute(sqlDelete, new
                    {
                        cod_grupo = codGrupo,
                        usuario
                    });

                    CR_GruposTrabajo_RegistrarBitacora(
                        codEmpresa,
                        usuarioSesion,
                        "Elimina - WEB",
                        $"Grupo Trabajo {codGrupo} > Excluye miembro: {usuario}");
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
