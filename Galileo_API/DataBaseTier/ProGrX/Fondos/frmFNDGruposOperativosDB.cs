using Dapper;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Fondos;
using Galileo.Models.Security;

namespace Galileo.DataBaseTier.ProGrX.Fondos
{
    public class FrmFndGruposOperativosDb
    {
        private readonly IConfiguration _config;
        private readonly int vModulo = 18; 
        private readonly MSecurityMainDb _Security_MainDB;

        private const string SqlGruposOperativosLazyLoad = @"
                    SELECT COUNT(1)
                    FROM dbo.FND_CONFIGURACION_GRUPOS
                    WHERE @hasFilter = 0 OR
                    (
                        GRUPO_CODIGO LIKE @filtro OR
                        DESCRIPCION LIKE @filtro
                    );

                    SELECT
                        GRUPO_CODIGO,
                        DESCRIPCION,
                        TIPO_GRUPO,
                        ESTADO,
                        FECHA_REGISTRA,
                        USUARIO_REGISTRA
                    FROM dbo.FND_CONFIGURACION_GRUPOS
                    WHERE @hasFilter = 0 OR
                    (
                        GRUPO_CODIGO LIKE @filtro OR
                        DESCRIPCION LIKE @filtro
                    )
                    ORDER BY
                        CASE WHEN @sortCode = 1 AND @isAsc = 1 THEN GRUPO_CODIGO END ASC,
                        CASE WHEN @sortCode = 1 AND @isAsc = 0 THEN GRUPO_CODIGO END DESC,
                        CASE WHEN @sortCode = 2 AND @isAsc = 1 THEN DESCRIPCION END ASC,
                        CASE WHEN @sortCode = 2 AND @isAsc = 0 THEN DESCRIPCION END DESC,
                        CASE WHEN @sortCode = 3 AND @isAsc = 1 THEN TIPO_GRUPO END ASC,
                        CASE WHEN @sortCode = 3 AND @isAsc = 0 THEN TIPO_GRUPO END DESC,
                        CASE WHEN @sortCode = 4 AND @isAsc = 1 THEN ESTADO END ASC,
                        CASE WHEN @sortCode = 4 AND @isAsc = 0 THEN ESTADO END DESC,
                        GRUPO_CODIGO ASC
                    OFFSET @offset ROWS FETCH NEXT @fetch ROWS ONLY;";

        private const string SqlGruposOperativos = @"
                    SELECT
                        GRUPO_CODIGO,
                        DESCRIPCION,
                        TIPO_GRUPO,
                        ESTADO,
                        FECHA_REGISTRA,
                        USUARIO_REGISTRA
                    FROM dbo.FND_CONFIGURACION_GRUPOS
                    ORDER BY GRUPO_CODIGO;";

        private const string SqlExisteGrupoOperativo = @"
                    SELECT ISNULL(COUNT(1), 0)
                    FROM dbo.FND_CONFIGURACION_GRUPOS
                    WHERE GRUPO_CODIGO = @Grupo_Codigo;";

        private const string SqlInsertGrupoOperativo = @"
                    INSERT INTO dbo.FND_CONFIGURACION_GRUPOS
                    (
                        GRUPO_CODIGO,
                        DESCRIPCION,
                        TIPO_GRUPO,
                        ESTADO,
                        FECHA_REGISTRA,
                        USUARIO_REGISTRA
                    )
                    VALUES
                    (
                        @Grupo_Codigo,
                        @Descripcion,
                        @Tipo_Grupo,
                        @Estado,
                        dbo.MyGetdate(),
                        @Usuario_Registra
                    );";

        private const string SqlUpdateGrupoOperativo = @"
                    UPDATE dbo.FND_CONFIGURACION_GRUPOS
                    SET DESCRIPCION = @Descripcion,
                        TIPO_GRUPO = @Tipo_Grupo,
                        ESTADO = @Estado
                    WHERE GRUPO_CODIGO = @Grupo_Codigo;";

        private const string SqlDeleteGrupoOperativo = @"
                    DELETE FROM dbo.FND_CONFIGURACION_GRUPOS
                    WHERE GRUPO_CODIGO = @Grupo_Codigo;";

        private const string SqlPlanesGrupoOperativo = @"
                    SELECT
                        Pl.cod_Operadora,
                        Pl.cod_Plan,
                        Pl.Descripcion,
                        Asg.FECHA_REGISTRA,
                        Asg.USUARIO_REGISTRA
                    FROM dbo.Fnd_Planes Pl
                    LEFT JOIN dbo.FND_CONFIGURACION_GRUPOS_PLANES Asg
                        ON Pl.cod_operadora = Asg.cod_Operadora
                       AND Pl.cod_Plan = Asg.PLANES_CODIGO
                       AND Asg.GRUPO_CODIGO = @GrupoCodigo
                    WHERE Pl.Estado = 'A'
                      AND (@Filtro IS NULL OR Pl.Cod_Plan LIKE @Filtro OR Pl.Descripcion LIKE @Filtro)
                    ORDER BY
                        ISNULL(Asg.PLANES_CODIGO, 'ZZZZZZZZZZZZ') ASC,
                        Pl.cod_Plan ASC;";

        private const string SqlUsuariosGrupoOperativo = @"
                    SELECT
                        Us.Nombre,
                        Us.Descripcion,
                        Asg.FECHA_REGISTRA,
                        Asg.USUARIO_REGISTRA
                    FROM dbo.Usuarios Us
                    LEFT JOIN dbo.FND_CONFIGURACION_GRUPOS_USUARIOS Asg
                        ON Us.Nombre = Asg.USUARIO_CODIGO
                       AND Asg.GRUPO_CODIGO = @GrupoCodigo
                    WHERE Us.Estado = 'A'
                      AND (@Filtro IS NULL OR Us.Nombre LIKE @Filtro OR Us.Descripcion LIKE @Filtro)
                    ORDER BY
                        ISNULL(Asg.USUARIO_CODIGO, 'ZZZZZZZZZZZ') ASC,
                        Us.Nombre ASC;";

        private const string SqlConceptosGrupoOperativo = @"
                    SELECT
                        Pl.RETENCION_CODIGO,
                        Pl.Descripcion,
                        Asg.FECHA_REGISTRA,
                        Asg.USUARIO_REGISTRA
                    FROM dbo.FND_RETENCION_CONCEPTOS Pl
                    LEFT JOIN dbo.FND_CONFIGURACION_GRUPOS_CONCEPTOS Asg
                        ON Pl.RETENCION_CODIGO = Asg.RETENCION_CODIGO
                       AND Asg.GRUPO_CODIGO = @GrupoCodigo
                    WHERE Pl.Activo = 1
                      AND (@Filtro IS NULL OR Pl.RETENCION_CODIGO LIKE @Filtro OR Pl.Descripcion LIKE @Filtro)
                    ORDER BY
                        ISNULL(Asg.RETENCION_CODIGO, 'ZZZZZZZZZZZZ') ASC,
                        Pl.RETENCION_CODIGO ASC;";

        private const string SqlInsertPlanGrupo = @"
                    INSERT INTO dbo.FND_CONFIGURACION_GRUPOS_PLANES
                    (
                        cod_operadora,
                        PLANES_CODIGO,
                        GRUPO_CODIGO,
                        USUARIO_REGISTRA,
                        FECHA_REGISTRA
                    )
                    VALUES
                    (
                        @Cod_Operadora,
                        @Plan_Codigo,
                        @Grupo_Codigo,
                        @Usuario,
                        dbo.MyGetdate()
                    );";

        private const string SqlDeletePlanGrupo = @"
                    DELETE FROM dbo.FND_CONFIGURACION_GRUPOS_PLANES
                    WHERE cod_operadora = @Cod_Operadora
                      AND PLANES_CODIGO = @Plan_Codigo
                      AND GRUPO_CODIGO = @Grupo_Codigo;";

        private const string SqlInsertUsuarioGrupo = @"
                    INSERT INTO dbo.FND_CONFIGURACION_GRUPOS_USUARIOS
                    (
                        USUARIO_CODIGO,
                        GRUPO_CODIGO,
                        USUARIO_REGISTRA,
                        FECHA_REGISTRA
                    )
                    VALUES
                    (
                        @Usuario_Codigo,
                        @Grupo_Codigo,
                        @Usuario,
                        dbo.MyGetdate()
                    );";

        private const string SqlDeleteUsuarioGrupo = @"
                    DELETE FROM dbo.FND_CONFIGURACION_GRUPOS_USUARIOS
                    WHERE USUARIO_CODIGO = @Usuario_Codigo
                      AND GRUPO_CODIGO = @Grupo_Codigo;";

        private const string SqlInsertConceptoGrupo = @"
                    INSERT INTO dbo.FND_CONFIGURACION_GRUPOS_CONCEPTOS
                    (
                        RETENCION_CODIGO,
                        GRUPO_CODIGO,
                        USUARIO_REGISTRA,
                        FECHA_REGISTRA
                    )
                    VALUES
                    (
                        @Retencion_Codigo,
                        @Grupo_Codigo,
                        @Usuario,
                        dbo.MyGetdate()
                    );";

        private const string SqlDeleteConceptoGrupo = @"
                    DELETE FROM dbo.FND_CONFIGURACION_GRUPOS_CONCEPTOS
                    WHERE RETENCION_CODIGO = @Retencion_Codigo
                      AND GRUPO_CODIGO = @Grupo_Codigo;";

        private static readonly IReadOnlyDictionary<string, int> GruposOperativosSortMap = new Dictionary<string, int>
        {
            ["GRUPO_CODIGO"] = 1,
            ["DESCRIPCION"] = 2,
            ["TIPO_GRUPO"] = 3,
            ["ESTADO"] = 4
        };

        public FrmFndGruposOperativosDb(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _Security_MainDB = new MSecurityMainDb(_config);
        }

        /// <summary>
        /// Obtiene la lista de grupos operativos con lazy load (paginación y filtro).
        /// </summary>
        public ErrorDto<FndGruposOperativosLista> Fnd_GruposOperativos_Lista_Obtener(int CodEmpresa, Models.FiltrosLazyLoadData filtros)
        {
            var result = DbHelper.CreateOkResponse(new FndGruposOperativosLista
            {
                total = 0,
                lista = new List<FndGrupoOperativoModel>()
            });

            try
            {
                var spec = LazyLoadHelper.Build(filtros, GruposOperativosSortMap, "GRUPO_CODIGO");
                var queryResult = DbHelper.WithConn(new PortalDB(_config), CodEmpresa, connection =>
                {
                    using var multi = connection.QueryMultiple(SqlGruposOperativosLazyLoad, spec.Params);
                    return new FndGruposOperativosLista
                    {
                        total = multi.ReadFirstOrDefault<int>(),
                        lista = multi.Read<FndGrupoOperativoModel>().ToList()
                    };
                });

                if (queryResult.Code != 0)
                {
                    return CrearErrorLista(queryResult.Description ?? "Error al consultar grupos operativos.");
                }

                result.Result = queryResult.Result ?? new FndGruposOperativosLista
                {
                    total = 0,
                    lista = new List<FndGrupoOperativoModel>()
                };
            }
            catch (Exception ex)
            {
                result = CrearErrorLista(ex.Message);
            }

            return result;
        }

        /// <summary>
        /// Obtiene la lista de grupos operativos sin paginación.
        /// </summary>
        public ErrorDto<List<FndGrupoOperativoModel>> Fnd_GruposOperativos_Obtener(int CodEmpresa)
        {
            return DbHelper.ExecuteListQuery<FndGrupoOperativoModel>(
                new PortalDB(_config),
                CodEmpresa,
                SqlGruposOperativos);
        }

        /// <summary>
        /// Valida si un grupo operativo ya existe.
        /// </summary>
        public ErrorDto<FndGrupoOperativoValidaResult> Fnd_GruposOperativos_Valida(int CodEmpresa, string grupoCodigo)
        {
            var existe = DbHelper.ExecuteSingleQuery(
                new PortalDB(_config),
                CodEmpresa,
                SqlExisteGrupoOperativo,
                0,
                new { Grupo_Codigo = NormalizarTexto(grupoCodigo) });

            return new ErrorDto<FndGrupoOperativoValidaResult>
            {
                Code = existe.Code,
                Description = existe.Description,
                Result = new FndGrupoOperativoValidaResult
                {
                    Existe = existe.Result
                }
            };
        }

        /// <summary>
        /// Inserta o actualiza un grupo operativo y guarda en bitácora.
        /// </summary>
        public ErrorDto Fnd_GruposOperativos_Guardar(int CodEmpresa, FndGrupoOperativoModel grupo)
        {
            if (grupo is null)
            {
                return DbHelper.ErrorResponse("Los datos del grupo operativo son requeridos.", -2);
            }

            var existe = Fnd_GruposOperativos_Valida(CodEmpresa, grupo.Grupo_Codigo);
            if (existe.Code != 0)
            {
                return DbHelper.ErrorResponse(existe.Description ?? "Error al validar grupo operativo.", existe.Code.GetValueOrDefault(-1));
            }

            if (grupo.IsNew && existe.Result?.Existe > 0)
            {
                return DbHelper.ErrorResponse($"El grupo operativo {grupo.Grupo_Codigo} ya existe.", -2);
            }

            if (!grupo.IsNew && existe.Result?.Existe == 0)
            {
                return DbHelper.ErrorResponse($"El grupo operativo {grupo.Grupo_Codigo} no existe.", -2);
            }

            var result = DbHelper.ExecuteNonQuery(
                new PortalDB(_config),
                CodEmpresa,
                grupo.IsNew ? SqlInsertGrupoOperativo : SqlUpdateGrupoOperativo,
                CrearParametrosGrupo(grupo));

            if (result.Code != 0)
            {
                return result;
            }

            RegistrarBitacora(
                CodEmpresa,
                grupo.Usuario_Registra,
                $"Grupo Operativo de Fondos: {NormalizarTexto(grupo.Grupo_Codigo)}",
                grupo.IsNew ? "Registra - Web" : "Modifica - Web");

            return result;
        }

        /// <summary>
        /// Elimina un grupo operativo y guarda en bitácora.
        /// </summary>
        public ErrorDto Fnd_GruposOperativos_Eliminar(int CodEmpresa, string grupoCodigo, string usuario)
        {
            var result = DbHelper.ExecuteNonQuery(
                new PortalDB(_config),
                CodEmpresa,
                SqlDeleteGrupoOperativo,
                new { Grupo_Codigo = NormalizarTexto(grupoCodigo) });

            if (result.Code != 0)
            {
                return result;
            }

            RegistrarBitacora(
                CodEmpresa,
                usuario,
                $"Grupo Operativo de Fondos: {NormalizarTexto(grupoCodigo)}",
                "Elimina - Web");

            return result;
        }

        /// <summary>
        /// Obtiene los planes asignables a un grupo operativo con filtro.
        /// </summary>
        public ErrorDto<List<FndGrupoOperativoPlanResult>> Fnd_GruposOperativos_Planes_Obtener(int CodEmpresa, FndGrupoOperativoFiltroRequest request)
        {
            if (request is null)
            {
                return DbHelper.CreateErrorResponse(
                    "Los filtros son requeridos.",
                    -2,
                    new List<FndGrupoOperativoPlanResult>());
            }

            return DbHelper.ExecuteListQuery<FndGrupoOperativoPlanResult>(
                new PortalDB(_config),
                CodEmpresa,
                SqlPlanesGrupoOperativo,
                CrearParametrosFiltro(request));
        }

        /// <summary>
        /// Obtiene los usuarios asignables a un grupo operativo con filtro.
        /// </summary>
        public ErrorDto<List<FndGrupoOperativoUsuarioResult>> Fnd_GruposOperativos_Usuarios_Obtener(int CodEmpresa, FndGrupoOperativoFiltroRequest request)
        {
            if (request is null)
            {
                return DbHelper.CreateErrorResponse(
                    "Los filtros son requeridos.",
                    -2,
                    new List<FndGrupoOperativoUsuarioResult>());
            }

            return DbHelper.ExecuteListQuery<FndGrupoOperativoUsuarioResult>(
                new PortalDB(_config),
                CodEmpresa,
                SqlUsuariosGrupoOperativo,
                CrearParametrosFiltro(request));
        }

        /// <summary>
        /// Obtiene los conceptos asignables a un grupo operativo con filtro.
        /// </summary>
        public ErrorDto<List<FndGrupoOperativoConceptoResult>> Fnd_GruposOperativos_Conceptos_Obtener(int CodEmpresa, FndGrupoOperativoFiltroRequest request)
        {
            if (request is null)
            {
                return DbHelper.CreateErrorResponse(
                    "Los filtros son requeridos.",
                    -2,
                    new List<FndGrupoOperativoConceptoResult>());
            }

            return DbHelper.ExecuteListQuery<FndGrupoOperativoConceptoResult>(
                new PortalDB(_config),
                CodEmpresa,
                SqlConceptosGrupoOperativo,
                CrearParametrosFiltro(request));
        }

        /// <summary>
        /// Asigna o desasigna un plan a un grupo operativo.
        /// </summary>
        public ErrorDto Fnd_GruposOperativos_AsignarPlan(int CodEmpresa, FndGrupoOperativoAsignarPlanRequest request)
        {
            if (request is null)
            {
                return DbHelper.ErrorResponse("Los datos de asignación del plan son requeridos.", -2);
            }

            return EjecutarAsignacionPlan(CodEmpresa, request);
        }

        /// <summary>
        /// Asigna o desasigna un usuario a un grupo operativo.
        /// </summary>
        public ErrorDto Fnd_GruposOperativos_AsignarUsuario(int CodEmpresa, FndGrupoOperativoAsignarUsuarioRequest request)
        {
            if (request is null)
            {
                return DbHelper.ErrorResponse("Los datos de asignación del usuario son requeridos.", -2);
            }

            return EjecutarAsignacionUsuario(CodEmpresa, request);
        }

        /// <summary>
        /// Asigna o desasigna un concepto a un grupo operativo.
        /// </summary>
        public ErrorDto Fnd_GruposOperativos_AsignarConcepto(int CodEmpresa, FndGrupoOperativoAsignarConceptoRequest request)
        {
            if (request is null)
            {
                return DbHelper.ErrorResponse("Los datos de asignación del concepto son requeridos.", -2);
            }

            return EjecutarAsignacionConcepto(CodEmpresa, request);
        }
        private static ErrorDto<FndGruposOperativosLista> CrearErrorLista(string mensaje) =>
            DbHelper.CreateErrorResponse(mensaje, -1, new FndGruposOperativosLista
            {
                total = 0,
                lista = new List<FndGrupoOperativoModel>()
            });

        private static object CrearParametrosGrupo(FndGrupoOperativoModel grupo)
        {
            return new
            {
                Grupo_Codigo = NormalizarTexto(grupo.Grupo_Codigo),
                Descripcion = NormalizarTexto(grupo.Descripcion),
                Tipo_Grupo = NormalizarTexto(grupo.Tipo_Grupo),
                grupo.Estado,
                Usuario_Registra = NormalizarTexto(grupo.Usuario_Registra)
            };
        }

        private static object CrearParametrosFiltro(FndGrupoOperativoFiltroRequest request)
        {
            return new
            {
                GrupoCodigo = NormalizarTexto(request.GrupoCodigo),
                Filtro = CrearFiltroLike(request.Filtro)
            };
        }

        private ErrorDto EjecutarAsignacionPlan(int codEmpresa, FndGrupoOperativoAsignarPlanRequest request)
        {
            return DbHelper.ExecuteNonQuery(
                new PortalDB(_config),
                codEmpresa,
                request.Asignar ? SqlInsertPlanGrupo : SqlDeletePlanGrupo,
                new
                {
                    Cod_Operadora = NormalizarValor(request.Cod_Operadora),
                    Plan_Codigo = NormalizarValor(request.Plan_Codigo),
                    Grupo_Codigo = NormalizarValor(request.Grupo_Codigo),
                    Usuario = NormalizarValor(request.Usuario)
                });
        }

        private ErrorDto EjecutarAsignacionUsuario(int codEmpresa, FndGrupoOperativoAsignarUsuarioRequest request)
        {
            return DbHelper.ExecuteNonQuery(
                new PortalDB(_config),
                codEmpresa,
                request.Asignar ? SqlInsertUsuarioGrupo : SqlDeleteUsuarioGrupo,
                new
                {
                    Usuario_Codigo = NormalizarValor(request.Usuario_Codigo),
                    Grupo_Codigo = NormalizarValor(request.Grupo_Codigo),
                    Usuario = NormalizarValor(request.Usuario)
                });
        }

        private ErrorDto EjecutarAsignacionConcepto(int codEmpresa, FndGrupoOperativoAsignarConceptoRequest request)
        {
            return DbHelper.ExecuteNonQuery(
                new PortalDB(_config),
                codEmpresa,
                request.Asignar ? SqlInsertConceptoGrupo : SqlDeleteConceptoGrupo,
                new
                {
                    Retencion_Codigo = NormalizarValor(request.Retencion_Codigo),
                    Grupo_Codigo = NormalizarValor(request.Grupo_Codigo),
                    Usuario = NormalizarValor(request.Usuario)
                });
        }

        private void RegistrarBitacora(int codEmpresa, string usuario, string detalleMovimiento, string movimiento)
        {
            _Security_MainDB.Bitacora(new BitacoraInsertarDto
            {
                EmpresaId = codEmpresa,
                Usuario = NormalizarTexto(usuario),
                DetalleMovimiento = detalleMovimiento,
                Movimiento = movimiento,
                Modulo = vModulo
            });
        }

        private static string? CrearFiltroLike(string? valor)
        {
            var texto = NormalizarTexto(valor);
            return string.IsNullOrWhiteSpace(texto) ? null : $"%{texto}%";
        }

        private static string NormalizarValor(object? valor) => Convert.ToString(valor)?.Trim() ?? string.Empty;
        private static string NormalizarTexto(string? valor) => (valor ?? string.Empty).Trim();
    }
}
