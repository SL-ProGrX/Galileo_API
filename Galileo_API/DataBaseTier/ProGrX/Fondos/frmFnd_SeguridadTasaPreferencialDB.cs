using Dapper;
using Microsoft.Data.SqlClient;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Fondos;
using Galileo.Models.Security;
using System.Data;

namespace Galileo.DataBaseTier.ProGrX.Fondos
{
    public class FrmFndSeguridadTasaPreferencialDB
    {
        private readonly IConfiguration _config;
        private readonly MSecurityMainDb _Security_MainDB;
        private const int vModulo = 18;
        private const string vTpRol = "TP_ROL";

        /// <summary>
        /// Inicializa una nueva instancia de seguridad de tasa preferencial.
        /// </summary>
        /// <param name="config">Configuración de la aplicación.</param>
        public FrmFndSeguridadTasaPreferencialDB(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _Security_MainDB = new MSecurityMainDb(_config);
        }

        /// <summary>
        /// Obtiene la lista paginada de roles de tasa preferencial.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="filtros">Filtros de búsqueda y paginación.</param>
        /// <returns>Listado de roles configurados.</returns>
        public ErrorDto<TablasListaGenericaModel> Fnd_SeguridadTasaPreferencial_Obtener(int CodEmpresa, FiltrosLazyLoadData filtros)
        {
            var result = DbHelper.CreateOkResponse(new TablasListaGenericaModel
            {
                total = 0,
                lista = new List<FndSeguridadTasaPreferencialDto>()
            });

            try
            {
                var spec = LazyLoadHelper.Build(filtros, SeguridadSortMap, vTpRol);
                var queryResult = DbHelper.WithConn(new PortalDB(_config), CodEmpresa, connection =>
                {
                    using var multi = connection.QueryMultiple(SqlRoles, spec.Params);

                    return new TablasListaGenericaModel
                    {
                        total = multi.ReadFirstOrDefault<int>(),
                        lista = multi.Read<FndSeguridadTasaPreferencialDto>().ToList()
                    };
                });

                if (queryResult.Code != 0)
                {
                    return CrearErrorLista(queryResult.Description ?? "Error al consultar roles de tasa preferencial.");
                }

                result.Result = queryResult.Result ?? new TablasListaGenericaModel
                {
                    total = 0,
                    lista = new List<FndSeguridadTasaPreferencialDto>()
                };
            }
            catch (Exception ex)
            {
                result = CrearErrorLista(ex.Message);
            }

            return result;
        }

        /// <summary>
        /// Obtiene los planes asociados o disponibles para un rol.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="rol">Rol de tasa preferencial.</param>
        /// <param name="filtro">Filtro de búsqueda.</param>
        /// <returns>Listado de planes.</returns>
        public ErrorDto<List<FndSeguridadTasaPreferenciaPlanData>> Fnd_SeguridadTasaPreferencial_Planes_Obtener(int CodEmpresa, string rol, string? filtro)
        {
            const string query = @"
                    SELECT 
                        Pl.cod_Operadora AS cod_operadora,
                        Pl.cod_Plan AS cod_plan,
                        Pl.Descripcion AS descripcion,
                        Asg.registro_Fecha AS registro_fecha,
                        Asg.Registro_Usuario AS registro_usuario
                    FROM Fnd_Planes Pl
                    LEFT JOIN FND_TP_ROLES_PLANES Asg 
                        ON Pl.cod_operadora = Asg.cod_Operadora
                        AND Pl.cod_Plan = Asg.Cod_Plan
                        AND Asg.TP_ROL = @rol
                    WHERE Pl.Estado = 'A'
                      AND (@filtro IS NULL
                           OR Pl.Cod_Plan LIKE @filtro
                           OR Pl.Descripcion LIKE @filtro)
                    ORDER BY ISNULL(Asg.Cod_Plan, 'ZZZZZZZZZZZZ') ASC,
                             Pl.cod_Plan ASC;";

            return DbHelper.ExecuteListQuery<FndSeguridadTasaPreferenciaPlanData>(
                new PortalDB(_config),
                CodEmpresa,
                query,
                new
                {
                    rol = NormalizarTexto(rol),
                    filtro = CrearFiltroLike(filtro)
                });
        }

        /// <summary>
        /// Obtiene los usuarios asociados o disponibles para un rol.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="rol">Rol de tasa preferencial.</param>
        /// <param name="filtro">Filtro de búsqueda.</param>
        /// <returns>Listado de usuarios.</returns>
        public ErrorDto<List<FndSeguridadTasaPreferenciaUsuarioData>> Fnd_SeguridadTasaPreferencial_Usuarios_Obtener(int CodEmpresa, string rol, string? filtro)
        {
            const string query = @"
                    SELECT 
                        Us.Nombre AS nombre,
                        Us.Descripcion AS descripcion,
                        Asg.registro_Fecha AS registro_fecha,
                        Asg.Registro_Usuario AS registro_usuario
                    FROM Usuarios Us
                    LEFT JOIN FND_TP_ROLES_AUTORIZADORES Asg 
                        ON Us.Nombre = Asg.Usuario
                        AND Asg.TP_ROL = @rol
                    WHERE Us.Estado = 'A'
                      AND (@filtro IS NULL
                           OR Us.Nombre LIKE @filtro
                           OR Us.Descripcion LIKE @filtro)
                    ORDER BY ISNULL(Asg.Usuario, 'ZZZZZZZZZZZ') ASC,
                             Us.Nombre ASC;";

            return DbHelper.ExecuteListQuery<FndSeguridadTasaPreferenciaUsuarioData>(
                new PortalDB(_config),
                CodEmpresa,
                query,
                new
                {
                    rol = NormalizarTexto(rol),
                    filtro = CrearFiltroLike(filtro)
                });
        }

        /// <summary>
        /// Guarda o actualiza un rol de tasa preferencial.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="row">Datos del rol.</param>
        /// <param name="usuario">Usuario ejecutando la acción.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto Fnd_SeguridadTasaPreferencial_Guardar(int CodEmpresa, FndSeguridadTasaPreferencialDto row, string usuario)
        {
            if (row is null)
            {
                return DbHelper.ErrorResponse("Los datos del rol son requeridos.", -2);
            }

            var result = DbHelper.WithConn(new PortalDB(_config), CodEmpresa, connection =>
            {
                var existe = connection.ExecuteScalar<int>(
                    "SELECT COUNT(1) FROM FND_TP_ROLES WHERE TP_ROL = @tp_rol;",
                    new { row.tp_rol });

                return existe == 0
                    ? InsertarRol(connection, row, usuario)
                    : ActualizarRol(connection, row, usuario);
            });

            if (result.Code != 0 || result.Result?.Code != 0)
            {
                return DbHelper.ErrorResponse(result.Description ?? result.Result?.Description ?? "Error al guardar rol.");
            }

            RegistrarBitacora(
                CodEmpresa,
                usuario,
                $"Grupo de Seguridad Tasa Preferencial {row.tp_rol}",
                string.IsNullOrWhiteSpace(row.tp_rol) ? "Registra - WEB" : "Modifica - WEB");

            return DbHelper.OkResponse("Ok");
        }

        /// <summary>
        /// Inserta un nuevo rol de tasa preferencial.
        /// </summary>
        /// <param name="connection">Conexión activa.</param>
        /// <param name="row">Datos del rol.</param>
        /// <param name="usuario">Usuario ejecutando la acción.</param>
        /// <returns>Resultado de la operación.</returns>
        private ErrorDto InsertarRol(SqlConnection connection, FndSeguridadTasaPreferencialDto row, string usuario)
        {
            var result = new ErrorDto
            {
                Code = 0,
                Description = "Ok"
            };

            try
            {
                const string sqlInsert = @"
                    INSERT INTO FND_TP_ROLES
                        (TP_ROL, Descripcion, Activo, registro_fecha, registro_usuario)
                    VALUES
                        (@tp_rol, @descripcion, @activo, dbo.MyGetDate(), @usuario);
                ";

                connection.Execute(sqlInsert, new
                {
                    row.tp_rol,
                    row.descripcion,
                    row.activo,
                    usuario
                });
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
            }

            return result;

        }

        /// <summary>
        /// Actualiza un rol de tasa preferencial.
        /// </summary>
        /// <param name="connection">Conexión activa.</param>
        /// <param name="row">Datos del rol.</param>
        /// <param name="usuario">Usuario ejecutando la acción.</param>
        /// <returns>Resultado de la operación.</returns>
        private ErrorDto ActualizarRol(SqlConnection connection, FndSeguridadTasaPreferencialDto row, string usuario)
        {
            var result = new ErrorDto
            {
                Code = 0,
                Description = "Ok"
            };

            try
            {
                const string sqlUpdate = @"
                        UPDATE FND_TP_ROLES
                        SET 
                            Descripcion = @descripcion,
                            Activo = @activo,
                            modifica_fecha = dbo.MyGetDate(),
                            modifica_usuario = @usuario
                        WHERE TP_ROL = @tp_rol;
                    ";

                connection.Execute(sqlUpdate, new
                {
                    row.tp_rol,
                    row.descripcion,
                    row.activo,
                    usuario
                });


            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
            }

            return result;

        }

        /// <summary>
        /// Elimina un rol de tasa preferencial.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="tp_rol">Rol a eliminar.</param>
        /// <param name="usuario">Usuario ejecutando la acción.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto Fnd_SeguridadTasaPreferencial_Eliminar(int CodEmpresa, string tp_rol, string usuario)
        {
            var result = DbHelper.WithConn(new PortalDB(_config), CodEmpresa, connection =>
                connection.Execute(
                    "spFndSeguridad_ApAnul_Delete",
                    new
                    {
                        Grupo = NormalizarTexto(tp_rol),
                        usuario = NormalizarTexto(usuario)
                    },
                    commandType: CommandType.StoredProcedure));

            if (result.Code != 0)
            {
                return DbHelper.ErrorResponse(result.Description ?? "Error al eliminar rol.", result.Code ?? -1);
            }

            RegistrarBitacora(CodEmpresa, usuario, $"Grupo de Seguridad Tasa Preferencial {tp_rol}", "Elimina - WEB");
            return DbHelper.OkResponse("Ok");
        }

        /// <summary>
        /// Asigna o elimina un plan asociado a un rol.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="data">Datos de asignación.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto Fnd_SeguridadTasaPreferencial_RolPlan_Actualizar(int CodEmpresa, FndSeguridadTasaPreferencialRolPlanDto data)
        {
            if (data is null)
            {
                return DbHelper.ErrorResponse("Los datos de asignación del plan son requeridos.", -2);
            }

            return data.asignar
                ? InsertarRolPlan(CodEmpresa, data)
                : EliminarRolPlan(CodEmpresa, data);
        }

        /// <summary>
        /// Asigna o elimina un usuario asociado a un rol.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="data">Datos de asignación.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto Fnd_SeguridadTasaPreferencial_RolAutorizador_Actualizar(int CodEmpresa, FndSeguridadTasaPreferencialRolAutorizadorDto data)
        {
            if (data is null)
            {
                return DbHelper.ErrorResponse("Los datos de asignación del autorizador son requeridos.", -2);
            }

            return data.asignar
                ? InsertarRolAutorizador(CodEmpresa, data)
                : EliminarRolAutorizador(CodEmpresa, data);
        }

        private static readonly IReadOnlyDictionary<string, int> SeguridadSortMap = new Dictionary<string, int>
        {
            [vTpRol] = 1,
            ["DESCRIPCION"] = 2,
            ["ACTIVO"] = 3
        };

        private const string SqlRoles = @"
                    SELECT COUNT(1)
                    FROM dbo.FND_TP_ROLES
                    WHERE @hasFilter = 0
                       OR TP_ROL LIKE @filtro
                       OR DESCRIPCION LIKE @filtro;

                    SELECT
                        TP_ROL,
                        DESCRIPCION,
                        ACTIVO,
                        registro_fecha,
                        registro_usuario,
                        modifica_fecha,
                        modifica_usuario
                    FROM dbo.FND_TP_ROLES
                    WHERE @hasFilter = 0
                       OR TP_ROL LIKE @filtro
                       OR DESCRIPCION LIKE @filtro
                    ORDER BY
                        CASE WHEN @sortCode = 1 AND @isAsc = 1 THEN TP_ROL END ASC,
                        CASE WHEN @sortCode = 1 AND @isAsc = 0 THEN TP_ROL END DESC,
                        CASE WHEN @sortCode = 2 AND @isAsc = 1 THEN DESCRIPCION END ASC,
                        CASE WHEN @sortCode = 2 AND @isAsc = 0 THEN DESCRIPCION END DESC,
                        CASE WHEN @sortCode = 3 AND @isAsc = 1 THEN ACTIVO END ASC,
                        CASE WHEN @sortCode = 3 AND @isAsc = 0 THEN ACTIVO END DESC,
                        TP_ROL ASC
                    OFFSET @offset ROWS FETCH NEXT @fetch ROWS ONLY;";


        /// <summary>
        /// Crea un resultado de error para listados.
        /// </summary>
        private static ErrorDto<TablasListaGenericaModel> CrearErrorLista(string mensaje) =>
            DbHelper.CreateErrorResponse(mensaje, -1, new TablasListaGenericaModel
            {
                total = 0,
                lista = new List<FndSeguridadTasaPreferencialDto>()
            });


        private ErrorDto InsertarRolPlan(int codEmpresa, FndSeguridadTasaPreferencialRolPlanDto data)
        {
            const string sql = @"
                    INSERT INTO dbo.FND_TP_ROLES_PLANES
                    (
                        cod_operadora,
                        cod_plan,
                        TP_ROL,
                        registro_usuario,
                        registro_fecha
                    )
                    VALUES
                    (
                        @cod_operadora,
                        @cod_plan,
                        @tp_rol,
                        @usuario,
                        dbo.MyGetDate()
                    );";

            return DbHelper.ExecuteNonQuery(new PortalDB(_config), codEmpresa, sql, CrearParametrosRolPlan(data));
        }

        private ErrorDto EliminarRolPlan(int codEmpresa, FndSeguridadTasaPreferencialRolPlanDto data)
        {
            const string sql = @"
                    DELETE FROM dbo.FND_TP_ROLES_PLANES
                    WHERE cod_operadora = @cod_operadora
                      AND cod_plan = @cod_plan
                      AND TP_ROL = @tp_rol;";

            return DbHelper.ExecuteNonQuery(new PortalDB(_config), codEmpresa, sql, CrearParametrosRolPlan(data));
        }

        private ErrorDto InsertarRolAutorizador(int codEmpresa, FndSeguridadTasaPreferencialRolAutorizadorDto data)
        {
            const string sql = @"
                    INSERT INTO dbo.FND_TP_ROLES_AUTORIZADORES
                    (
                        usuario,
                        TP_ROL,
                        registro_usuario,
                        registro_fecha
                    )
                    VALUES
                    (
                        @usuario,
                        @tp_rol,
                        @registro_usuario,
                        dbo.MyGetDate()
                    );";

            return DbHelper.ExecuteNonQuery(new PortalDB(_config), codEmpresa, sql, CrearParametrosRolAutorizador(data));
        }

        private ErrorDto EliminarRolAutorizador(int codEmpresa, FndSeguridadTasaPreferencialRolAutorizadorDto data)
        {
            const string sql = @"
                    DELETE FROM dbo.FND_TP_ROLES_AUTORIZADORES
                    WHERE usuario = @usuario
                      AND TP_ROL = @tp_rol;";

            return DbHelper.ExecuteNonQuery(new PortalDB(_config), codEmpresa, sql, CrearParametrosRolAutorizador(data));
        }

        private static object CrearParametrosRolPlan(FndSeguridadTasaPreferencialRolPlanDto data)
        {
            return new
            {
                data.cod_operadora,
                cod_plan = NormalizarTexto(data.cod_plan),
                tp_rol = NormalizarTexto(data.tp_rol),
                usuario = NormalizarTexto(data.usuario)
            };
        }

        private static object CrearParametrosRolAutorizador(FndSeguridadTasaPreferencialRolAutorizadorDto data)
        {
            return new
            {
                usuario = NormalizarTexto(data.usuario),
                tp_rol = NormalizarTexto(data.tp_rol),
                registro_usuario = NormalizarTexto(data.registro_usuario)
            };
        }

        /// <summary>
        /// Registra movimientos en bitácora.
        /// </summary>
        private void RegistrarBitacora(int codEmpresa, string usuario, string detalleMovimiento, string movimiento)
        {
            _Security_MainDB.Bitacora(new BitacoraInsertarDto
            {
                EmpresaId = codEmpresa,
                Usuario = usuario,
                DetalleMovimiento = detalleMovimiento,
                Movimiento = movimiento,
                Modulo = vModulo
            });
        }

        /// <summary>
        /// Normaliza texto evitando valores nulos.
        /// </summary>
        private static string NormalizarTexto(string? valor) => (valor ?? string.Empty).Trim();

        /// <summary>
        /// Genera filtros LIKE seguros.
        /// </summary>
        private static string? CrearFiltroLike(string? valor)
        {
            var texto = NormalizarTexto(valor);
            return string.IsNullOrWhiteSpace(texto) ? null : $"%{texto}%";
        }
    }
}