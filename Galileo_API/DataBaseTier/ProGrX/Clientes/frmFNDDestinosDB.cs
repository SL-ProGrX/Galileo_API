using Dapper;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Clientes;
using Galileo.Models.Security;

namespace Galileo.DataBaseTier.ProGrX.Clientes
{
    public class FrmFndDestinosDB
    {
        private readonly IConfiguration _config;
        private const int VModulo = 18;
        private readonly MSecurityMainDb _securityMainDb;

        private const string SqlDestinos = @"
                    SELECT cod_destino AS Cod_Destino,
                           descripcion,
                           activo
                    FROM dbo.fnd_destinos
                    ORDER BY cod_destino;";

        private const string SqlDestinosTotal = @"
                    SELECT COUNT(cod_destino)
                    FROM dbo.fnd_destinos
                    WHERE @hasFilter = 0 OR
                          cod_destino LIKE @filtro OR
                          descripcion LIKE @filtro;";

        private const string SqlDestinosLista = @"
                    SELECT cod_destino AS Cod_Destino,
                           descripcion,
                           activo
                    FROM dbo.fnd_destinos
                    WHERE @hasFilter = 0 OR
                          cod_destino LIKE @filtro OR
                          descripcion LIKE @filtro
                    ORDER BY
                        CASE WHEN @sortCode = 1 AND @isAsc = 1 THEN cod_destino END ASC,
                        CASE WHEN @sortCode = 1 AND @isAsc = 0 THEN cod_destino END DESC,
                        CASE WHEN @sortCode = 2 AND @isAsc = 1 THEN descripcion END ASC,
                        CASE WHEN @sortCode = 2 AND @isAsc = 0 THEN descripcion END DESC,
                        cod_destino ASC
                    OFFSET @offset ROWS FETCH NEXT @fetch ROWS ONLY;";

        private const string SqlDestinoExiste = @"
                    SELECT ISNULL(COUNT(*), 0) AS Existe
                    FROM dbo.FND_DESTINOS
                    WHERE cod_destino = @CodDestino;";

        private const string SqlDestinoInsert = @"
                    INSERT INTO dbo.FND_DESTINOS
                    (
                        cod_destino,
                        descripcion,
                        Activo,
                        registro_usuario,
                        registro_fecha
                    )
                    VALUES
                    (
                        @CodDestino,
                        @Descripcion,
                        @Activo,
                        @Usuario,
                        dbo.MyGetdate()
                    );";

        private const string SqlDestinoUpdate = @"
                    UPDATE dbo.FND_DESTINOS
                    SET descripcion = @Descripcion,
                        Activo = @Activo,
                        actualiza_usuario = @Usuario,
                        actualiza_fecha = dbo.MyGetdate()
                    WHERE cod_destino = @CodDestino;";

        private const string SqlDestinoDelete = @"
                    DELETE FROM dbo.FND_DESTINOS
                    WHERE cod_destino = @CodDestino;";

        private const string SqlPlanesDestino = @"
                    SELECT D.cod_operadora,
                           D.COD_PLAN,
                           D.descripcion,
                           A.cod_destino
                    FROM dbo.fnd_Planes D
                    LEFT JOIN dbo.fnd_planes_destinos A
                        ON D.COD_OPERADORA = A.cod_operadora
                       AND D.COD_PLAN = A.cod_plan
                       AND A.cod_destino = @CodDestino
                    WHERE D.Estado = 'A';";

        private const string SqlPlanesDestinoTotal = @"
                    SELECT COUNT(1)
                    FROM dbo.fnd_Planes D
                    LEFT JOIN dbo.fnd_planes_destinos A
                        ON D.COD_OPERADORA = A.cod_operadora
                       AND D.COD_PLAN = A.cod_plan
                       AND A.cod_destino = @CodDestino
                    WHERE D.Estado = 'A'
                      AND (@hasFilter = 0 OR
                           D.cod_plan LIKE @filtro OR
                           D.descripcion LIKE @filtro OR
                           D.cod_operadora LIKE @filtro);";

        private const string SqlPlanesDestinoLista = @"
                    SELECT D.cod_operadora,
                           D.COD_PLAN,
                           D.descripcion,
                           A.cod_destino
                    FROM dbo.fnd_Planes D
                    LEFT JOIN dbo.fnd_planes_destinos A
                        ON D.COD_OPERADORA = A.cod_operadora
                       AND D.COD_PLAN = A.cod_plan
                       AND A.cod_destino = @CodDestino
                    WHERE D.Estado = 'A'
                      AND (@hasFilter = 0 OR
                           D.cod_plan LIKE @filtro OR
                           D.descripcion LIKE @filtro OR
                           D.cod_operadora LIKE @filtro)
                    ORDER BY
                        CASE WHEN @sortCode = 1 AND @isAsc = 1 THEN D.cod_plan END ASC,
                        CASE WHEN @sortCode = 1 AND @isAsc = 0 THEN D.cod_plan END DESC,
                        CASE WHEN @sortCode = 2 AND @isAsc = 1 THEN D.descripcion END ASC,
                        CASE WHEN @sortCode = 2 AND @isAsc = 0 THEN D.descripcion END DESC,
                        CASE WHEN @sortCode = 3 AND @isAsc = 1 THEN D.cod_operadora END ASC,
                        CASE WHEN @sortCode = 3 AND @isAsc = 0 THEN D.cod_operadora END DESC,
                        D.cod_plan ASC
                    OFFSET @offset ROWS FETCH NEXT @fetch ROWS ONLY;";

        private const string SqlPlanDestinoInsert = @"
                    INSERT INTO dbo.fnd_planes_destinos
                    (
                        cod_plan,
                        cod_operadora,
                        cod_destino,
                        registro_usuario,
                        registro_fecha
                    )
                    VALUES
                    (
                        @CodPlan,
                        @CodOperadora,
                        @CodDestino,
                        @Usuario,
                        dbo.MyGetdate()
                    );";

        private const string SqlPlanDestinoDelete = @"
                    DELETE FROM dbo.fnd_planes_destinos
                    WHERE cod_destino = @CodDestino
                      AND cod_operadora = @CodOperadora
                      AND cod_plan = @CodPlan;";

        private static readonly IReadOnlyDictionary<string, int> DestinosSortMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            ["cod_destino"] = 1,
            ["Cod_Destino"] = 1,
            ["descripcion"] = 2
        };

        private static readonly IReadOnlyDictionary<string, int> PlanesSortMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            ["D.cod_plan"] = 1,
            ["cod_plan"] = 1,
            ["COD_PLAN"] = 1,
            ["D.descripcion"] = 2,
            ["descripcion"] = 2,
            ["D.cod_operadora"] = 3,
            ["cod_operadora"] = 3
        };

        public FrmFndDestinosDB(IConfiguration? config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _securityMainDb = new MSecurityMainDb(_config);
        }

        /// <summary>
        /// Obtiene la lista de destinos.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <returns>Listado de destinos.</returns>
        public ErrorDto<List<FndDestinosData>> Fnd_Destinos_Obtener(int CodEmpresa)
        {
            return DbHelper.ExecuteListQuery<FndDestinosData>(
                CreatePortalDb(),
                CodEmpresa,
                SqlDestinos);
        }

        /// <summary>
        /// Obtiene la lista de destinos con filtros y paginación.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="filtros">Parámetros de filtro y paginación.</param>
        /// <returns>Lista paginada de destinos.</returns>
        public ErrorDto<FndDestinosLista> Fnd_DestinosLista_Obtener(int CodEmpresa, FiltrosLazyLoadData filtros)
        {
            var spec = LazyLoadHelper.Build(filtros, DestinosSortMap, "cod_destino");

            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection => new FndDestinosLista
            {
                total = connection.QueryFirstOrDefault<int>(SqlDestinosTotal, spec.Params),
                lista = connection.Query<FndDestinosData>(SqlDestinosLista, spec.Params).ToList()
            });

            return result.Code == 0
                ? DbHelper.CreateOkResponse(result.Result ?? CrearDestinosListaVacia())
                : DbHelper.CreateErrorResponse(
                    result.Description ?? "Error al obtener destinos.",
                    result.Code.GetValueOrDefault(-1),
                    CrearDestinosListaVacia());
        }

        /// <summary>
        /// Valida si un código de destino ya existe en la base de datos.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="Cod_Destino">Código del destino a validar.</param>
        /// <returns>Resultado de la validación.</returns>
        public ErrorDto Fnd_Destinos_Valida(int CodEmpresa, string Cod_Destino)
        {
            var result = DbHelper.ExecuteSingleQuery<int>(
                CreatePortalDb(),
                CodEmpresa,
                SqlDestinoExiste,
                0,
                new { CodDestino = NormalizarTexto(Cod_Destino) });

            if (result.Code != 0)
            {
                return DbHelper.ErrorResponse(result.Description ?? "Error al validar destino.", result.Code.GetValueOrDefault(-1));
            }

            return result.Result > 0
                ? DbHelper.ErrorResponse("El código de destino ya existe.", -1)
                : DbHelper.OkResponse("El código de destino es válido.");
        }

        /// <summary>
        /// Inserta o actualiza un destino.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="usuario">Usuario que realiza la acción.</param>
        /// <param name="destino">Datos del destino a guardar.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto Fnd_Destinos_Guardar(int CodEmpresa, string usuario, FndDestinosData destino)
        {
            if (destino is null)
            {
                return DbHelper.ErrorResponse("Los datos del destino son requeridos.", -2);
            }

            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
            {
                var parametros = CrearParametrosDestino(destino, usuario);
                var existe = connection.QueryFirstOrDefault<int>(SqlDestinoExiste, parametros);

                if (destino.IsNew && existe > 0)
                {
                    return DbHelper.ErrorResponse($"El destino con el código {destino.Cod_Destino} ya existe.", -2);
                }

                if (!destino.IsNew && existe == 0)
                {
                    return DbHelper.ErrorResponse($"El destino con el código {destino.Cod_Destino} no existe.", -2);
                }

                connection.Execute(destino.IsNew ? SqlDestinoInsert : SqlDestinoUpdate, parametros);
                return DbHelper.OkResponse(destino.IsNew ? "Registra - WEB" : "Modifica - WEB");
            });

            if (result.Code != 0 || result.Result is null)
            {
                return DbHelper.ErrorResponse(result.Description ?? "Error al guardar destino.", result.Code.GetValueOrDefault(-1));
            }

            RegistrarBitacora(CodEmpresa, usuario, "Destinos de Planes.", result.Result.Description ?? string.Empty);
            return DbHelper.OkResponse("Ok");
        }

        /// <summary>
        /// Elimina un destino por su código y registra en bitácora.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="usuario">Usuario que realiza la acción.</param>
        /// <param name="Cod_Destino">Código del destino a eliminar.</param>
        /// <returns>Resultado de la eliminación.</returns>
        public ErrorDto Fnd_Destinos_Eliminar(int CodEmpresa, string usuario, string Cod_Destino)
        {
            var codDestinoSeguro = NormalizarTexto(Cod_Destino);
            var result = DbHelper.ExecuteNonQuery(
                CreatePortalDb(),
                CodEmpresa,
                SqlDestinoDelete,
                new { CodDestino = codDestinoSeguro });

            if (result.Code != 0)
            {
                return DbHelper.ErrorResponse(result.Description ?? "Error al eliminar destino.", result.Code.GetValueOrDefault(-1));
            }

            RegistrarBitacora(CodEmpresa, usuario, "Destinos de Planes.", "Elimina - WEB");
            return DbHelper.OkResponse("Ok");
        }

        /// <summary>
        /// Obtiene la lista de planes y su asignación a un destino sin paginación.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="Cod_Destino">Código del destino.</param>
        /// <returns>Lista de planes del destino.</returns>
        public ErrorDto<List<FndPlanesDestinoData>> Fnd_Planes_Obtener(int CodEmpresa, string Cod_Destino)
        {
            return DbHelper.ExecuteListQuery<FndPlanesDestinoData>(
                CreatePortalDb(),
                CodEmpresa,
                SqlPlanesDestino,
                new { CodDestino = NormalizarTexto(Cod_Destino) });
        }

        /// <summary>
        /// Obtiene la lista de planes y su asignación a un destino con filtros y paginación.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="Cod_Destino">Código del destino.</param>
        /// <param name="filtros">Parámetros de filtro y paginación.</param>
        /// <returns>Lista paginada de planes por destino.</returns>
        public ErrorDto<FndPlanesDestinoLista> Fnd_PlanesLista_Obtener(int CodEmpresa, string Cod_Destino, FiltrosLazyLoadData filtros)
        {
            var spec = LazyLoadHelper.Build(filtros, PlanesSortMap, "D.cod_plan");
            spec.Params.Add("@CodDestino", NormalizarTexto(Cod_Destino));

            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection => new FndPlanesDestinoLista
            {
                total = connection.QueryFirstOrDefault<int>(SqlPlanesDestinoTotal, spec.Params),
                lista = connection.Query<FndPlanesDestinoData>(SqlPlanesDestinoLista, spec.Params).ToList()
            });

            return result.Code == 0
                ? DbHelper.CreateOkResponse(result.Result ?? CrearPlanesListaVacia())
                : DbHelper.CreateErrorResponse(
                    result.Description ?? "Error al obtener planes por destino.",
                    result.Code.GetValueOrDefault(-1),
                    CrearPlanesListaVacia());
        }

        /// <summary>
        /// Asigna o desasigna un plan a un destino y registra en bitácora.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="request">Datos de la asignación o desasignación.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto Fnd_Planes_AsignarDesasignar(int CodEmpresa, FndAsignarPlanRequest request)
        {
            if (request is null)
            {
                return DbHelper.ErrorResponse("Los datos de asignación son requeridos.", -2);
            }

            var result = DbHelper.ExecuteNonQuery(
                CreatePortalDb(),
                CodEmpresa,
                request.Asignar ? SqlPlanDestinoInsert : SqlPlanDestinoDelete,
                CrearParametrosPlanDestino(request));

            if (result.Code != 0)
            {
                return DbHelper.ErrorResponse(result.Description ?? "Error al asignar o desasignar plan.", result.Code.GetValueOrDefault(-1));
            }

            RegistrarBitacora(
                CodEmpresa,
                request.Usuario,
                $"Asignación Plan {request.Cod_Plan} -> Destino : {request.Cod_Destino}",
                request.Asignar ? "Aplica - WEB" : "Elimina - WEB");

            return DbHelper.OkResponse("Ok");
        }

        /// <summary>
        /// Crea parámetros seguros para guardar destinos.
        /// </summary>
        private static object CrearParametrosDestino(FndDestinosData destino, string usuario)
        {
            return new
            {
                CodDestino = NormalizarTexto(destino.Cod_Destino),
                Descripcion = NormalizarTexto(destino.Descripcion),
                Activo = destino.Activo ? 1 : 0,
                Usuario = NormalizarTexto(usuario)
            };
        }

        /// <summary>
        /// Crea parámetros seguros para asignar o desasignar planes.
        /// </summary>
        private static object CrearParametrosPlanDestino(FndAsignarPlanRequest request)
        {
            return new
            {
                CodPlan = NormalizarTexto(request.Cod_Plan),
                CodOperadora = request.Cod_Operadora,
                CodDestino = NormalizarTexto(request.Cod_Destino),
                Usuario = NormalizarTexto(request.Usuario)
            };
        }

        /// <summary>
        /// Registra movimientos en bitácora de seguridad.
        /// </summary>
        private void RegistrarBitacora(int codEmpresa, string usuario, string detalle, string movimiento)
        {
            _securityMainDb.Bitacora(new BitacoraInsertarDto
            {
                EmpresaId = codEmpresa,
                Usuario = NormalizarTexto(usuario),
                DetalleMovimiento = detalle,
                Movimiento = movimiento,
                Modulo = VModulo
            });
        }

        /// <summary>
        /// Crea una lista vacía de destinos.
        /// </summary>
        private static FndDestinosLista CrearDestinosListaVacia()
        {
            return new FndDestinosLista
            {
                total = 0,
                lista = new List<FndDestinosData>()
            };
        }

        /// <summary>
        /// Crea una lista vacía de planes por destino.
        /// </summary>
        private static FndPlanesDestinoLista CrearPlanesListaVacia()
        {
            return new FndPlanesDestinoLista
            {
                total = 0,
                lista = new List<FndPlanesDestinoData>()
            };
        }

        /// <summary>
        /// Crea una instancia de acceso al portal usando la configuración inyectada.
        /// </summary>
        private PortalDB CreatePortalDb() => new(_config);

        /// <summary>
        /// Normaliza valores de texto recibidos desde filtros o formularios.
        /// </summary>
        private static string NormalizarTexto(string? valor) => (valor ?? string.Empty).Trim();
    }
}