using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Fondos;

namespace Galileo.DataBaseTier.ProGrX.Fondos
{
    public class FrmFndGruposDb
    {
        private readonly IConfiguration _config;

        private const string SqlGrupos = @"
                    SELECT
                        cod_grupo AS Cod_Grupo,
                        descripcion AS Descripcion,
                        categoria AS Categoria,
                        interno AS Interno,
                        prioridad AS Prioridad
                    FROM dbo.fnd_grupos
                    ORDER BY cod_grupo;";

        private const string SqlExisteGrupo = @"
                    SELECT ISNULL(COUNT(1), 0)
                    FROM dbo.fnd_grupos
                    WHERE cod_grupo = @Cod_Grupo;";

        private const string SqlInsertGrupo = @"
                    INSERT INTO dbo.fnd_grupos
                    (
                        cod_grupo,
                        descripcion,
                        categoria,
                        interno,
                        prioridad
                    )
                    VALUES
                    (
                        @Cod_Grupo,
                        @Descripcion,
                        @Categoria,
                        @Interno,
                        @Prioridad
                    );";

        private const string SqlUpdateGrupo = @"
                    UPDATE dbo.fnd_grupos
                    SET descripcion = @Descripcion,
                        categoria = @Categoria,
                        interno = @Interno,
                        prioridad = @Prioridad
                    WHERE cod_grupo = @Cod_Grupo;";

        private const string SqlDeleteGrupo = @"
                    DELETE FROM dbo.fnd_grupos
                    WHERE cod_grupo = @Cod_Grupo;";

        private const string SqlPlanesGrupo = @"
                    SELECT
                        P.cod_operadora AS Cod_Operadora,
                        P.cod_plan AS Cod_Plan,
                        P.descripcion AS Descripcion,
                        G.cod_grupo AS Cod_Grupo
                    FROM dbo.fnd_Planes P
                    LEFT JOIN dbo.fnd_grupos G
                        ON P.cod_grupo = G.cod_grupo
                       AND P.cod_grupo = @CodGrupo
                    WHERE P.Estado = 'A';";

        private const string SqlAsignarPlanGrupo = @"
                    UPDATE dbo.fnd_Planes
                    SET cod_grupo = @CodGrupo
                    WHERE cod_plan = @CodPlan
                      AND cod_operadora = @CodOperadora;";

        private const string SqlQuitarPlanGrupo = @"
                    UPDATE dbo.fnd_Planes
                    SET cod_grupo = NULL
                    WHERE cod_plan = @CodPlan
                      AND cod_operadora = @CodOperadora
                      AND cod_grupo = @CodGrupo;";

        public FrmFndGruposDb(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        /// <summary>
        /// Obtiene los grupos
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<FndGrupoDto>> Fnd_Grupos_Obtener(int CodEmpresa)
        {
            return DbHelper.ExecuteListQuery<FndGrupoDto>(
                new PortalDB(_config),
                CodEmpresa,
                SqlGrupos);
        }

        /// <summary>
        /// Guarda o actualiza los grupos
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="grupo"></param>
        /// <returns></returns>
        public ErrorDto<FndGrupoDto> Fnd_Grupos_Guardar(int CodEmpresa, FndGrupoDto grupo)
        {
            if (grupo is null)
            {
                return DbHelper.CreateErrorResponse(
                    "Los datos del grupo son requeridos.",
                    -2,
                    new FndGrupoDto { interno = false, prioridad = 0 });
            }

            var existe = DbHelper.ExecuteSingleQuery(
                new PortalDB(_config),
                CodEmpresa,
                SqlExisteGrupo,
                0,
                new { Cod_Grupo = NormalizarTexto(grupo.cod_grupo) });

            if (existe.Code != 0)
            {
                return DbHelper.CreateErrorResponse(
                    existe.Description ?? "Error al validar grupo.",
                    existe.Code.GetValueOrDefault(-1),
                    grupo);
            }

            var sql = existe.Result == 0 ? SqlInsertGrupo : SqlUpdateGrupo;
            var result = DbHelper.ExecuteNonQuery(
                new PortalDB(_config),
                CodEmpresa,
                sql,
                CrearParametrosGrupo(grupo));

            return new ErrorDto<FndGrupoDto>
            {
                Code = result.Code,
                Description = result.Description,
                Result = grupo
            };
        }

        /// <summary>
        /// Elimina los grupos
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="CodGrupo"></param>
        /// <returns></returns>
        public ErrorDto Fnd_Grupos_Eliminar(int CodEmpresa, string CodGrupo)
        {
            return DbHelper.ExecuteNonQuery(
                new PortalDB(_config),
                CodEmpresa,
                SqlDeleteGrupo,
                new { Cod_Grupo = NormalizarTexto(CodGrupo) });
        }

        /// <summary>
        /// Obtiene los planes del grupo
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="CodGrupo"></param>
        /// <returns></returns>
        public ErrorDto<List<FndPlanGrupoDto>> Fnd_Grupos_ObtenerPlanes(int CodEmpresa, string CodGrupo)
        {
            return DbHelper.ExecuteListQuery<FndPlanGrupoDto>(
                new PortalDB(_config),
                CodEmpresa,
                SqlPlanesGrupo,
                new { CodGrupo = NormalizarTexto(CodGrupo) });
        }

        /// <summary>
        /// Asigna los planes a un grupo
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="CodGrupo"></param>
        /// <param name="CodPlan"></param>
        /// <param name="CodOperadora"></param>
        /// <param name="Checked"></param>
        /// <returns></returns>
        public ErrorDto Fnd_Grupos_ActualizarAsignacionPlan(int CodEmpresa, string CodGrupo, string CodPlan, int CodOperadora, bool Checked)
        {
            var query = Checked ? SqlAsignarPlanGrupo : SqlQuitarPlanGrupo;

            return DbHelper.ExecuteNonQuery(
                new PortalDB(_config),
                CodEmpresa,
                query,
                new
                {
                    CodGrupo = NormalizarTexto(CodGrupo),
                    CodPlan = NormalizarTexto(CodPlan),
                    CodOperadora
                });
        }

        private static object CrearParametrosGrupo(FndGrupoDto grupo)
        {
            return new
            {
                Cod_Grupo = NormalizarTexto(grupo.cod_grupo),
                Descripcion = NormalizarTexto(grupo.descripcion),
                Categoria = NormalizarTexto(grupo.categoria),
                grupo.interno,
                Prioridad = grupo.prioridad
            };
        }

        private static string NormalizarTexto(string? valor) => (valor ?? string.Empty).Trim();
    }
}