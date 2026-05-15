using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Fondos;

namespace Galileo.DataBaseTier.ProGrX.Fondos
{
    public class FrmFndSeguridadMovimientosDB
    {
        private readonly IConfiguration _config;

        private const string SqlPlanes = @"
                    SELECT 
                        Pl.cod_operadora,
                        Pl.cod_plan,
                        Pl.descripcion,
                        Asg.registro_usuario,
                        Asg.registro_fecha,
                        CASE WHEN Asg.registro_fecha IS NULL THEN 0 ELSE 1 END AS seleccionado
                    FROM dbo.Fnd_Planes Pl
                    LEFT JOIN dbo.FND_SEG_PLANESXGRUPO Asg 
                        ON Pl.cod_operadora = Asg.cod_operadora
                       AND Pl.cod_plan = Asg.cod_plan
                       AND Asg.cod_grupo_aprtanul = @Grupo
                    WHERE Pl.estado = 'A'
                    ORDER BY ISNULL(Asg.cod_plan,'ZZZ'), Pl.cod_plan;";

        private const string SqlInsertPlanGrupo = @"
                    INSERT INTO dbo.FND_SEG_PLANESXGRUPO
                    (
                        cod_operadora,
                        cod_plan,
                        cod_grupo_aprtanul,
                        registro_usuario,
                        registro_fecha
                    )
                    VALUES
                    (
                        @Operadora,
                        @Plan,
                        @Grupo,
                        @Usuario,
                        GETDATE()
                    );";

        private const string SqlDeletePlanGrupo = @"
                    DELETE FROM dbo.FND_SEG_PLANESXGRUPO
                    WHERE cod_operadora = @Operadora
                      AND cod_plan = @Plan
                      AND cod_grupo_aprtanul = @Grupo;";

        private const string SqlUsuarios = @"
                    SELECT 
                        Us.Nombre AS usuario,
                        Us.Descripcion AS descripcion,
                        Asg.registro_usuario,
                        Asg.registro_fecha,
                        CASE WHEN Asg.registro_fecha IS NULL THEN 0 ELSE 1 END AS seleccionado
                    FROM dbo.Usuarios Us
                    LEFT JOIN dbo.FND_SEG_GRUPOSXUSUARIO Asg 
                        ON Us.Nombre = Asg.Usuario 
                       AND Asg.COD_GRUPO_APRTANUL = @Grupo
                    WHERE Us.Estado = 'A'
                    ORDER BY ISNULL(Asg.Usuario,'ZZZ'), Us.Nombre;";

        private const string SqlInsertUsuarioGrupo = @"
                    INSERT INTO dbo.FND_SEG_GRUPOSXUSUARIO
                    (
                        usuario,
                        COD_GRUPO_APRTANUL,
                        registro_usuario,
                        registro_fecha
                    )
                    VALUES
                    (
                        @UsuarioMarcado,
                        @Grupo,
                        @Usuario,
                        GETDATE()
                    );";

        private const string SqlDeleteUsuarioGrupo = @"
                    DELETE FROM dbo.FND_SEG_GRUPOSXUSUARIO
                    WHERE usuario = @UsuarioMarcado
                      AND COD_GRUPO_APRTANUL = @Grupo;";

        private const string SqlAutorizadores = @"
                    SELECT 
                        Us.Nombre AS usuario,
                        Us.Descripcion AS descripcion,
                        Asg.registro_usuario,
                        Asg.registro_fecha,
                        CASE WHEN Asg.registro_fecha IS NULL THEN 0 ELSE 1 END AS seleccionado
                    FROM dbo.Usuarios Us
                    LEFT JOIN dbo.FND_SEG_GRUPOSXAUTORIZADOR Asg 
                        ON Us.Nombre = Asg.Usuario 
                       AND Asg.COD_GRUPO_APRTANUL = @Grupo
                    WHERE Us.Estado = 'A'
                    ORDER BY ISNULL(Asg.Usuario,'ZZZ'), Us.Nombre;";

        private const string SqlInsertAutorizadorGrupo = @"
                    INSERT INTO dbo.FND_SEG_GRUPOSXAUTORIZADOR
                    (
                        usuario,
                        COD_GRUPO_APRTANUL,
                        registro_usuario,
                        registro_fecha
                    )
                    VALUES
                    (
                        @UsuarioMarcado,
                        @Grupo,
                        @Usuario,
                        GETDATE()
                    );";

        private const string SqlDeleteAutorizadorGrupo = @"
                    DELETE FROM dbo.FND_SEG_GRUPOSXAUTORIZADOR
                    WHERE usuario = @UsuarioMarcado
                      AND COD_GRUPO_APRTANUL = @Grupo;";

        private const string SqlNiveles = @"
                    SELECT 
                        cod_grupo_aprtanul,
                        descripcion,
                        aporte_autorizado,
                        anulacion_autorizado,
                        activo
                    FROM dbo.FND_SEG_GRUPOS_APRTANUL
                    ORDER BY cod_grupo_aprtanul;";

        private const string SqlExisteNivel = @"
                    SELECT COUNT(1)
                    FROM dbo.FND_SEG_GRUPOS_APRTANUL
                    WHERE cod_grupo_aprtanul = @Codigo;";

        private const string SqlInsertNivel = @"
                    INSERT INTO dbo.FND_SEG_GRUPOS_APRTANUL
                    (
                        cod_grupo_aprtanul,
                        descripcion,
                        aporte_autorizado,
                        anulacion_autorizado,
                        activo,
                        registro_fecha,
                        registro_usuario
                    )
                    VALUES
                    (
                        @Codigo,
                        @Descripcion,
                        @Aporte,
                        @Anulacion,
                        @Activo,
                        GETDATE(),
                        @Usuario
                    );";

        private const string SqlUpdateNivel = @"
                    UPDATE dbo.FND_SEG_GRUPOS_APRTANUL
                    SET descripcion = @Descripcion,
                        aporte_autorizado = @Aporte,
                        anulacion_autorizado = @Anulacion,
                        activo = @Activo
                    WHERE cod_grupo_aprtanul = @Codigo;";

        private const string SqlDeleteNivel = @"
                    DELETE FROM dbo.FND_SEG_GRUPOS_APRTANUL
                    WHERE cod_grupo_aprtanul = @Codigo;";

        public FrmFndSeguridadMovimientosDB(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config), "La configuración no puede ser nula.");
        }

        public ErrorDto<List<SeguridadMovimientoPlanDto>> Seguridad_Planes_Obtener(int CodEmpresa, string cod_grupo)
        {
            return DbHelper.ExecuteListQuery<SeguridadMovimientoPlanDto>(
                new PortalDB(_config),
                CodEmpresa,
                SqlPlanes,
                new { Grupo = NormalizarTexto(cod_grupo) });
        }

        public ErrorDto<bool> Seguridad_Planes_Marcar(
            int CodEmpresa,
            string cod_grupo,
            string cod_plan,
            int cod_operadora,
            bool marcado,
            string usuario)
        {
            var result = DbHelper.ExecuteNonQuery(
                new PortalDB(_config),
                CodEmpresa,
                marcado ? SqlInsertPlanGrupo : SqlDeletePlanGrupo,
                new
                {
                    Operadora = cod_operadora,
                    Plan = NormalizarTexto(cod_plan),
                    Grupo = NormalizarTexto(cod_grupo),
                    Usuario = NormalizarTexto(usuario)
                });

            return CrearResultadoBooleano(result);
        }

        public ErrorDto<List<SeguridadMovimientoUsuarioDto>> Seguridad_Usuarios_Obtener(
            int CodEmpresa,
            string cod_grupo)
        {
            return DbHelper.ExecuteListQuery<SeguridadMovimientoUsuarioDto>(
                new PortalDB(_config),
                CodEmpresa,
                SqlUsuarios,
                new { Grupo = NormalizarTexto(cod_grupo) });
        }

        public ErrorDto<bool> Seguridad_Usuarios_Marcar(
            int CodEmpresa,
            string cod_grupo,
            string usuarioMarcado,
            bool marcado,
            string usuario)
        {
            return EjecutarMarcadoUsuario(
                CodEmpresa,
                marcado ? SqlInsertUsuarioGrupo : SqlDeleteUsuarioGrupo,
                cod_grupo,
                usuarioMarcado,
                usuario);
        }

        public ErrorDto<List<SeguridadMovimientoUsuarioDto>> Seguridad_Autorizadores_Obtener(
            int CodEmpresa,
            string cod_grupo)
        {
            return DbHelper.ExecuteListQuery<SeguridadMovimientoUsuarioDto>(
                new PortalDB(_config),
                CodEmpresa,
                SqlAutorizadores,
                new { Grupo = NormalizarTexto(cod_grupo) });
        }

        public ErrorDto<bool> Seguridad_Autorizadores_Marcar(
            int CodEmpresa,
            string cod_grupo,
            string usuarioMarcado,
            bool marcado,
            string usuario)
        {
            return EjecutarMarcadoUsuario(
                CodEmpresa,
                marcado ? SqlInsertAutorizadorGrupo : SqlDeleteAutorizadorGrupo,
                cod_grupo,
                usuarioMarcado,
                usuario);
        }

        public ErrorDto<List<SeguridadMovimientoNivelDto>> Seguridad_Niveles_Obtener(int CodEmpresa)
        {
            return DbHelper.ExecuteListQuery<SeguridadMovimientoNivelDto>(
                new PortalDB(_config),
                CodEmpresa,
                SqlNiveles);
        }

        public ErrorDto<bool> Seguridad_Niveles_Guardar(
            int CodEmpresa,
            SeguridadMovimientoNivelDto dto,
            string usuario)
        {
            if (dto is null)
            {
                return DbHelper.CreateErrorResponse(
                    "Los datos del nivel son requeridos.",
                    -2,
                    false);
            }

            var existe = DbHelper.ExecuteSingleQuery(
                new PortalDB(_config),
                CodEmpresa,
                SqlExisteNivel,
                0,
                new { Codigo = NormalizarTexto(dto.cod_grupo_aprtanul) });

            if (existe.Code != 0)
            {
                return DbHelper.CreateErrorResponse(
                    existe.Description ?? "Error al validar nivel.",
                    existe.Code.GetValueOrDefault(-1),
                    false);
            }

            var result = DbHelper.ExecuteNonQuery(
                new PortalDB(_config),
                CodEmpresa,
                existe.Result > 0 ? SqlUpdateNivel : SqlInsertNivel,
                CrearParametrosNivel(dto, usuario));

            return CrearResultadoBooleano(result);
        }

        public ErrorDto<bool> Seguridad_Niveles_Eliminar(
            int CodEmpresa,
            string cod_grupo,
            string usuario)
        {
            var result = DbHelper.ExecuteNonQuery(
                new PortalDB(_config),
                CodEmpresa,
                SqlDeleteNivel,
                new { Codigo = NormalizarTexto(cod_grupo) });

            return CrearResultadoBooleano(result);
        }

        private ErrorDto<bool> EjecutarMarcadoUsuario(
            int codEmpresa,
            string sql,
            string codGrupo,
            string usuarioMarcado,
            string usuario)
        {
            var result = DbHelper.ExecuteNonQuery(
                new PortalDB(_config),
                codEmpresa,
                sql,
                new
                {
                    UsuarioMarcado = NormalizarTexto(usuarioMarcado),
                    Grupo = NormalizarTexto(codGrupo),
                    Usuario = NormalizarTexto(usuario)
                });

            return CrearResultadoBooleano(result);
        }

        private static object CrearParametrosNivel(
            SeguridadMovimientoNivelDto dto,
            string usuario)
        {
            return new
            {
                Codigo = NormalizarTexto(dto.cod_grupo_aprtanul),
                Descripcion = NormalizarTexto(dto.descripcion),
                Aporte = dto.aporte_autorizado,
                Anulacion = dto.anulacion_autorizado,
                Activo = dto.activo,
                Usuario = NormalizarTexto(usuario)
            };
        }

        private static ErrorDto<bool> CrearResultadoBooleano(ErrorDto result)
        {
            return new ErrorDto<bool>
            {
                Code = result.Code,
                Description = result.Description,
                Result = result.Code == 0
            };
        }

        private static string NormalizarTexto(string? valor)
            => (valor ?? string.Empty).Trim();
    }
}