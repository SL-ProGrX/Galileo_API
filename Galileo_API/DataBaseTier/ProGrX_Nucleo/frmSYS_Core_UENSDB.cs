using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;
using Newtonsoft.Json;
using Galileo.Models.ERROR;
using Galileo.Models.SYS;

namespace Galileo.DataBaseTier
{
    public class FrmSysCoreUensDB
    {
        private readonly PortalDB _portalDB;
        private const string RegistroActualizadoMsg = "Registro actualizado satisfactoriamente";
        private const string DelRolesSql = @"delete from CORE_UENS_USUARIOS_ROLES where COD_UNIDAD = @cod_unidad";
        private const string DelUensSql = @"delete from CORE_UENS where COD_UNIDAD = @cod_unidad";
        private const string DelUensCascadeSql = @"delete from CORE_UENS where COD_UNIDAD = @cod_unidad OR UNIDAD_PRINCIPAL = @cod_unidad";
        private const string TopUnidadSql = @"select TOP 1 COD_UNIDAD from CORE_UENS where UNIDAD_PRINCIPAL = @cod_unidad order by ACTIVA desc";
        private const string UpSetPrincipalSql = @"update CORE_UENS set UNIDAD_PRINCIPAL = @nuevaUnidadPrincipal where UNIDAD_PRINCIPAL = @cod_unidad";
        private const string UpClearPrincipalSql = @"update CORE_UENS set UNIDAD_PRINCIPAL = NULL where COD_UNIDAD = @nuevaUnidadPrincipal";
        private const string FindSubUnidadSql = @"select COD_UNIDAD from CORE_UENS where UNIDAD_PRINCIPAL = @cod_unidad AND CNTX_UNIDAD = @cntx_unidad";
        private const string UnidadInfoSql = @"select * from CORE_UENS where COD_UNIDAD = @cod_unidad";

        public FrmSysCoreUensDB(IConfiguration config)
        {
            _portalDB = new PortalDB(config);
        }

        private static CoreUeNsDtoList EmptyUensList()
            => new CoreUeNsDtoList { uens = new List<CoreUeNsDto>(), Total = 0 };


        private static ErrorDto<T> MapDb<T>(ErrorDto<T> db, Func<T> defaultFactory)
            => db.Code == 0
                ? db
                : DbHelper.CreateErrorResponse(db.Description ?? "Error inesperado", -1, defaultFactory());

        private static ErrorDto MapDbNonQuery(ErrorDto<int> db, string okDescription)
            => db.Code == 0
                ? new ErrorDto { Code = db.Result, Description = okDescription }
                : new ErrorDto { Code = -1, Description = db.Description ?? "Error inesperado" };

        private static int ToActivaBit(bool? activa)
            => activa == true ? 1 : 0;

        private static string GetNextNumericCodUnidad(SqlConnection connection)
        {
            const string getMaxSql = @"SELECT CAST(MAX(CAST(COD_UNIDAD AS INT)) + 1 AS VARCHAR) AS NuevoCodigo
                FROM CORE_UENS WHERE ISNUMERIC(COD_UNIDAD) = 1";

            int ultimoID = connection.Query<int>(getMaxSql).FirstOrDefault();
            return ultimoID < 10 ? "0" + ultimoID : ultimoID.ToString();
        }

        private static void DeleteRoles(SqlConnection connection, string cod_unidad)
            => connection.Execute(DelRolesSql, new { cod_unidad });

        private static void DeleteUnidad(SqlConnection connection, string cod_unidad)
            => connection.Execute(DelUensSql, new { cod_unidad });

        private static void DeleteUnidadCascade(SqlConnection connection, string cod_unidad)
            => connection.Execute(DelUensCascadeSql, new { cod_unidad });

        private static void DeleteRolesAndUnidad(SqlConnection connection, string cod_unidad)
        {
            DeleteRoles(connection, cod_unidad);
            DeleteUnidad(connection, cod_unidad);
        }

        private static void PromoteAnotherAsPrincipalIfAny(SqlConnection connection, string cod_unidad)
        {
            string? nuevaUnidadPrincipal = connection.Query<string>(TopUnidadSql, new { cod_unidad }).FirstOrDefault();
            if (nuevaUnidadPrincipal == null) return;

            connection.Execute(UpSetPrincipalSql, new { nuevaUnidadPrincipal, cod_unidad });
            connection.Execute(UpClearPrincipalSql, new { nuevaUnidadPrincipal });
        }

        private static int DeletePrincipalAndReassign(SqlConnection connection, string cod_unidad)
        {
            DeleteRolesAndUnidad(connection, cod_unidad);
            PromoteAnotherAsPrincipalIfAny(connection, cod_unidad);
            return 1;
        }

        private static int DeleteSubUnidad(SqlConnection connection, string cod_unidad)
        {
            DeleteRolesAndUnidad(connection, cod_unidad);
            return 1;
        }

        private CoreUeNsDtoList QueryUensPaged(SqlConnection connection, CoreUeNsFiltros? vfiltro, bool onlyPrincipales)
        {
            var search = vfiltro?.filtro?.Trim();
            string? searchLike = string.IsNullOrWhiteSpace(search) ? null : $"%{search}%";

            var offset = vfiltro?.pagina ?? 0;
            var fetch = vfiltro?.paginacion ?? 0;
            if (fetch <= 0) fetch = int.MaxValue;

            const string countSql = @"SELECT COUNT(*)
                                      FROM CORE_UENS
                                      WHERE (@onlyPrincipales = 0 OR UNIDAD_PRINCIPAL IS NULL)
                                        AND (@search IS NULL
                                             OR COD_UNIDAD LIKE @search
                                             OR descripcion LIKE @search);";

            const string pageSql = @"SELECT COD_UNIDAD, descripcion, CntX_Unidad, CntX_Centro_Costo, Activa, 0 as 'btn'
                                     FROM CORE_UENS
                                     WHERE (@onlyPrincipales = 0 OR UNIDAD_PRINCIPAL IS NULL)
                                       AND (@search IS NULL
                                            OR COD_UNIDAD LIKE @search
                                            OR descripcion LIKE @search)
                                     ORDER BY COD_UNIDAD DESC
                                     OFFSET @offset ROWS FETCH NEXT @fetch ROWS ONLY;";

            var dto = EmptyUensList();
            dto.Total = connection.Query<int>(countSql, new { search = searchLike, onlyPrincipales = onlyPrincipales ? 1 : 0 }).FirstOrDefault();
            dto.uens = connection.Query<CoreUeNsDto>(pageSql, new { search = searchLike, offset, fetch, onlyPrincipales = onlyPrincipales ? 1 : 0 }).ToList();
            return dto;
        }

        /// <summary>
        /// Obtener UENs
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDto<CoreUeNsDtoList> Core_UENS_Obtener(int CodCliente, string filtros)
        {
            var vfiltro = JsonConvert.DeserializeObject<CoreUeNsFiltros>(filtros);

            var db = DbHelper.WithConn(_portalDB, CodCliente,
                connection => QueryUensPaged(connection, vfiltro, onlyPrincipales: false));

            return MapDb(db, EmptyUensList);
        }


        /// <summary>
        /// Insertar y Actualizar Core UENs
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="usuario"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto Core_UENS_Upsert(int CodCliente, string usuario, CoreUeNsDto request)
        {
            var esNuevo = string.IsNullOrWhiteSpace(request.cod_unidad);
            var okMsg = esNuevo
                ? "Registro agregado satisfactoriamente"
                : RegistroActualizadoMsg;

            var db = DbHelper.WithConn(_portalDB, CodCliente, connection =>
            {
                var activa = ToActivaBit(request.activa);

                if (esNuevo)
                {
                    const string nextCodeSql = @"
                SELECT RIGHT('00' + CAST(ISNULL(MAX(CAST(COD_UNIDAD AS INT)), 0) + 1 AS VARCHAR(10)), 2)
                FROM CORE_UENS WITH (UPDLOCK, HOLDLOCK)
                WHERE ISNUMERIC(COD_UNIDAD) = 1;";

                    var nuevoCodigo = connection.ExecuteScalar<string>(nextCodeSql);

                    const string insertSql = @"
                INSERT INTO CORE_UENS
                    (COD_UNIDAD, descripcion, Activa, Registro_Fecha, Registro_Usuario)
                VALUES
                    (@cod_unidad, @descripcion, @activa, GETDATE(), @usuario);";

                    return connection.Execute(insertSql, new
                    {
                        cod_unidad = nuevoCodigo,
                        descripcion = request.descripcion,
                        activa,
                        usuario
                    });
                }

                const string existsSql = @"
            SELECT ISNULL(COUNT(*), 0)
            FROM CORE_UENS
            WHERE COD_UNIDAD = @cod_unidad;";

                int existe = connection.QueryFirstOrDefault<int>(existsSql, new
                {
                    cod_unidad = request.cod_unidad
                });

                if (existe == 0)
                    return 0;

                const string updateSql = @"
            UPDATE CORE_UENS
               SET descripcion = @descripcion,
                   Activa = @activa,
                   Modifica_Fecha = GETDATE(),
                   Modifica_Usuario = @usuario
             WHERE COD_UNIDAD = @cod_unidad
                OR UNIDAD_PRINCIPAL = @cod_unidad;";

                return connection.Execute(updateSql, new
                {
                    cod_unidad = request.cod_unidad,
                    descripcion = request.descripcion,
                    activa,
                    usuario
                });
            });

            return MapDbNonQuery(db, okMsg);
        }


        /// <summary>
        /// Insertar y Actualizar una unidad de perteneciente a una UEN
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="usuario"></param>
        /// <param name="unidad_anterior"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto Core_SubUnidad_Upsert(int CodCliente, string usuario, string? unidad_anterior, CoreUeNsDto request)
        {
            var db = DbHelper.WithConn(_portalDB, CodCliente, connection =>
            {
                var activa = ToActivaBit(request?.activa);

                const string qPrincipal = @"select * from CORE_UENS where COD_UNIDAD = @unidad_principal";
                CoreUeNsDto unidadPrincipal = connection.Query<CoreUeNsDto>(qPrincipal, new { unidad_principal = request?.unidad_principal }).First();

                const string qExiste = @"select isnull(count(*),0) as Existe from CORE_UENS
                                        where (COD_UNIDAD = @unidad_principal OR UNIDAD_PRINCIPAL = @unidad_principal)
                                          AND CNTX_UNIDAD = @cntx_unidad";

                int existe = connection.Query<int>(qExiste, new { unidad_principal = request?.unidad_principal, cntx_unidad = request?.cntx_unidad }).FirstOrDefault();

                if (existe == 0 && unidadPrincipal.cntx_unidad == "")
                {
                    const string upSql = @"update CORE_UENS set
                                         CntX_Unidad = @cntx_unidad,
                                         Activa = @activa,
                                         Modifica_Fecha = Getdate(),
                                         Modifica_Usuario = @usuario
                                       where COD_UNIDAD = @cod_unidad";

                    return connection.Execute(upSql, new { cntx_unidad = request?.cntx_unidad, activa, usuario, cod_unidad = unidadPrincipal.cod_unidad });
                }

                if (existe == 0 && request != null && request.cod_unidad == "")
                {
                    string nuevoCodigo = GetNextNumericCodUnidad(connection);

                    const string insSql = @"insert into CORE_UENS(COD_UNIDAD, descripcion, CntX_Unidad, unidad_principal, Activa, Registro_Fecha, Registro_Usuario)
                                       values(@cod_unidad, @descripcion, @cntx_unidad, @unidad_principal, @activa, Getdate(), @usuario)";

                    return connection.Execute(insSql, new
                    {
                        cod_unidad = nuevoCodigo,
                        descripcion = unidadPrincipal.descripcion,
                        cntx_unidad = request.cntx_unidad,
                        unidad_principal = request.unidad_principal,
                        activa,
                        usuario
                    });
                }

                const string upSql2 = @"update CORE_UENS set
                                         CntX_Unidad = @cntx_unidad,
                                         Activa = @activa,
                                         Modifica_Fecha = Getdate(),
                                         Modifica_Usuario = @usuario
                                       where (COD_UNIDAD = @cod_unidad OR UNIDAD_PRINCIPAL = @cod_unidad)
                                         AND CNTX_UNIDAD = @unidad_anterior";

                return connection.Execute(upSql2, new
                {
                    cntx_unidad = request?.cntx_unidad,
                    activa,
                    usuario,
                    cod_unidad = request?.cod_unidad,
                    unidad_anterior
                });
            });

            return MapDbNonQuery(db, RegistroActualizadoMsg);
        }


        /// <summary>
        /// Insertar y Actualizar un centro de costo de perteneciente a una UEN
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="usuario"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto Core_SubCentroCosto_Upsert(int CodCliente, string usuario, CoreUeNsDto request)
        {
            var okMsg = request?.cod_unidad == null || request.cod_unidad == ""
                ? "Registro agregado satisfactoriamente"
                : RegistroActualizadoMsg;

            var db = DbHelper.WithConn(_portalDB, CodCliente, connection =>
            {
                var activa = ToActivaBit(request?.activa);

                const string qExisteCod = @"select isnull(count(*),0) as Existe from CORE_UENS where COD_UNIDAD = @cod_unidad";
                int existe = connection.Query<int>(qExisteCod, new { cod_unidad = request?.cod_unidad }).FirstOrDefault();

                if (existe == 0)
                {
                    const string query2 = @"select * from CORE_UENS
                                           where (COD_UNIDAD = @unidad_principal OR UNIDAD_PRINCIPAL = @unidad_principal)
                                             AND CNTX_UNIDAD = @cntx_unidad";
                    CoreUeNsDto unidadPrincipal = connection.Query<CoreUeNsDto>(query2, new { unidad_principal = request?.unidad_principal, cntx_unidad = request?.cntx_unidad }).First();

                    if (unidadPrincipal.cntx_centro_costo == "")
                    {
                        const string upSql = @"update CORE_UENS set
                                                 CntX_Centro_Costo = @cntx_centro_costo,
                                                 Activa = @activa,
                                                 Modifica_Fecha = Getdate(),
                                                 Modifica_Usuario = @usuario
                                               where COD_UNIDAD = @cod_unidad";

                        return connection.Execute(upSql, new { cntx_centro_costo = request?.cntx_centro_costo, activa, usuario, cod_unidad = unidadPrincipal.cod_unidad });
                    }

                    string nuevoCodigo = GetNextNumericCodUnidad(connection);

                    const string insSql = @"insert into CORE_UENS(COD_UNIDAD, descripcion, CntX_Unidad, CntX_Centro_Costo, unidad_principal, Activa, Registro_Fecha, Registro_Usuario)
                                               values(@cod_unidad, @descripcion, @cntx_unidad, @cntx_centro_costo, @unidad_principal, @activa, Getdate(), @usuario)";

                    return connection.Execute(insSql, new
                    {
                        cod_unidad = nuevoCodigo,
                        descripcion = unidadPrincipal.descripcion,
                        cntx_unidad = request?.cntx_unidad,
                        cntx_centro_costo = request?.cntx_centro_costo,
                        unidad_principal = request?.unidad_principal,
                        activa,
                        usuario
                    });
                }

                const string upSql2 = @"update CORE_UENS set
                                             CntX_Centro_Costo = @cntx_centro_costo,
                                             Activa = @activa,
                                             Modifica_Fecha = Getdate(),
                                             Modifica_Usuario = @usuario
                                           where COD_UNIDAD = @cod_unidad";

                return connection.Execute(upSql2, new { cntx_centro_costo = request?.cntx_centro_costo, activa, usuario, cod_unidad = request?.cod_unidad });
            });

            return MapDbNonQuery(db, okMsg);
        }

        /// <summary>
        /// Borra las UEns
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="cod_unidad"></param>
        /// <returns></returns>
        public ErrorDto Core_UENS_Delete(int CodCliente, string cod_unidad)
        {
            var db = DbHelper.WithConn(_portalDB, CodCliente, connection =>
            {
                DeleteUnidadCascade(connection, cod_unidad);
                return 1;
            });

            return MapDbNonQuery(db, "Registros eliminados satisfactoriamente");
        }


        /// <summary>
        /// Borrar las unidades de una UEN
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="cod_unidad"></param>
        /// <param name="cntx_unidad"></param>
        /// <returns></returns>
        public ErrorDto Core_SubUnidad_Delete(int CodCliente, string cod_unidad, string cntx_unidad)
        {
            var db = DbHelper.WithConn(_portalDB, CodCliente, connection =>
            {
                string? codUnidad = connection.Query<string>(FindSubUnidadSql, new { cod_unidad, cntx_unidad }).FirstOrDefault();

                // Si no hay sub-unidad para esa unidad/cntx_unidad, se asume que es la principal
                if (codUnidad == null)
                    return DeletePrincipalAndReassign(connection, cod_unidad);

                return DeleteSubUnidad(connection, codUnidad);
            });

            return MapDbNonQuery(db, "Registros eliminado satisfactoriamente");
        }


        /// <summary>
        /// Borrar el centro de costo de una UEN
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="cod_unidad"></param>
        /// <returns></returns>
        public ErrorDto Core_SubCentroCosto_Delete(int CodCliente, string cod_unidad)
        {
            var db = DbHelper.WithConn(_portalDB, CodCliente, connection =>
            {
                CoreUeNsDto unidadInfo = connection.Query<CoreUeNsDto>(UnidadInfoSql, new { cod_unidad }).First();

                // Si no tiene unidad_principal, es la principal
                if (unidadInfo.unidad_principal == "")
                    return DeletePrincipalAndReassign(connection, cod_unidad);

                return DeleteSubUnidad(connection, cod_unidad);
            });

            return MapDbNonQuery(db, "Registro eliminado satisfactoriamente");
        }


        /// <summary>
        /// Obtener UENs principales
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="cod_unidad"></param>
        /// <returns></returns>
        public ErrorDto<CoreUeNsDtoList> Core_UENSPrincipales_Obtener(int CodCliente, string filtros)
        {
            var vfiltro = JsonConvert.DeserializeObject<CoreUeNsFiltros>(filtros);

            var db = DbHelper.WithConn(_portalDB, CodCliente,
                connection => QueryUensPaged(connection, vfiltro, onlyPrincipales: true));

            return MapDb(db, EmptyUensList);
        }


        /// <summary>
        /// Obtener las unidades pertenecientes a una UEN
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="cod_unidad"></param>
        /// <returns></returns>
        public ErrorDto<CoreUeNsDtoList> Core_SubUnidades_Obtener(int CodCliente, string cod_unidad)
        {
            var db = DbHelper.WithConn(_portalDB, CodCliente, connection =>
            {
                const string query = @"SELECT
                                    @cod_unidad AS COD_UNIDAD,
                                    C.CNTX_UNIDAD,
                                    @cod_unidad AS UNIDAD_PRINCIPAL,
                                    U.DESCRIPCION
                                FROM CORE_UENS C
                                LEFT JOIN CNTX_UNIDADES U
                                    ON U.COD_UNIDAD = C.CNTX_UNIDAD
                                WHERE C.UNIDAD_PRINCIPAL = @cod_unidad

                                UNION

                                SELECT
                                    @cod_unidad AS COD_UNIDAD,
                                    C.CNTX_UNIDAD,
                                    @cod_unidad AS UNIDAD_PRINCIPAL,
                                    U.DESCRIPCION
                                FROM CORE_UENS C
                                LEFT JOIN CNTX_UNIDADES U
                                    ON U.COD_UNIDAD = C.CNTX_UNIDAD
                                WHERE C.COD_UNIDAD = @cod_unidad

                                ORDER BY CNTX_UNIDAD DESC";

                var dto = EmptyUensList();
                dto.Total = 0;
                dto.uens = connection.Query<CoreUeNsDto>(query, new { cod_unidad }).ToList();

                if (dto.uens == null || dto.uens.Count == 0 || string.IsNullOrWhiteSpace(dto.uens[0].cntx_unidad))
                    return null;

                return dto;
            });

            return MapDb(db, EmptyUensList);
        }


        /// <summary>
        /// Obtener los centros de costo pertenecientes a una UEN
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="cod_unidad"></param>
        /// <returns></returns>
        public ErrorDto<CoreUeNsDtoList> Core_SubCentroCosto_Obtener(int CodCliente, string cod_unidad, string sub_unidad)
        {
            var db = DbHelper.WithConn(_portalDB, CodCliente, connection =>
            {
                const string query = @"select C.COD_UNIDAD, C.CNTX_UNIDAD, C.CNTX_CENTRO_COSTO, C.ACTIVA, @cod_unidad AS UNIDAD_PRINCIPAL,
                    (select TOP 1 DESCRIPCION from CNTX_CENTRO_COSTOS WHERE COD_CENTRO_COSTO = C.CNTX_CENTRO_COSTO) AS DESCRIPCION
                    from CORE_UENS C
                    WHERE (C.UNIDAD_PRINCIPAL = @cod_unidad OR C.COD_UNIDAD = @cod_unidad) AND C.CNTX_UNIDAD = @sub_unidad
                    order by C.CNTX_CENTRO_COSTO desc";

                var dto = EmptyUensList();
                dto.Total = 0;
                dto.uens = connection.Query<CoreUeNsDto>(query, new { cod_unidad, sub_unidad }).ToList();

                if (dto.uens == null || dto.uens.Count == 0 || string.IsNullOrWhiteSpace(dto.uens[0].cntx_centro_costo))
                    return null;

                return dto;
            });

            return MapDb(db, EmptyUensList);
        }


        /// <summary>
        /// Obtiene los miembros
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="cod_unidad"></param>
        /// <param name="filtro"></param>
        /// <returns></returns>
        public ErrorDto<List<CoreUsuariosDto>> Core_Miembros_Obtener(int CodCliente, string cod_unidad, string? filtro)
        {
            var db = DbHelper.WithConn(_portalDB, CodCliente, connection =>
            {
                const string sp = "spSys_UENS_Miembros_Consultas";
                return connection.Query<CoreUsuariosDto>(sp, new
                {
                    UEN = cod_unidad,
                    filtro = (filtro ?? string.Empty)
                }, commandType: CommandType.StoredProcedure).ToList();
            });

            return MapDb(db, () => new List<CoreUsuariosDto>());
        }


        /// <summary>
        /// Registra los miembros
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="cod_unidad"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto Core_Miembros_Registro(int CodCliente, string cod_unidad, CoreUsuariosDto request)
        {
            var db = DbHelper.WithConn(_portalDB, CodCliente, connection =>
            {
                var mov = request.asignado ? 'A' : 'E';
                const string sp = "spSys_UENS_Miembros_Registro";

                connection.Execute(sp, new
                {
                    uen = cod_unidad,
                    CoreUser = request.core_usuario,
                    RegUser = request.registro_usuario,
                    Mov = mov
                }, commandType: CommandType.StoredProcedure);

                return 1;
            });

            return MapDbNonQuery(db, RegistroActualizadoMsg);
        }


        /// <summary>
        /// Obtiene los roles
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="cod_unidad"></param>
        /// <param name="filtro"></param>
        /// <returns></returns>
        public ErrorDto<List<CoreRolesDto>> Core_Roles_Obtener(int CodCliente, string cod_unidad, string? filtro)
        {
            var db = DbHelper.WithConn(_portalDB, CodCliente, connection =>
            {
                const string sp = "spSys_UENS_Roles_Consultas";
                return connection.Query<CoreRolesDto>(sp, new
                {
                    UEN = cod_unidad,
                    filtro = (filtro ?? string.Empty)
                }, commandType: CommandType.StoredProcedure).ToList();
            });

            return MapDb(db, () => new List<CoreRolesDto>());
        }


        /// <summary>
        /// Registra los Roles
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="cod_unidad"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto Core_Roles_Registro(int CodCliente, string cod_unidad, CoreRolesDto request)
        {
            if (request == null)
                return new ErrorDto { Code = -1, Description = "El parámetro 'request' no puede ser nulo." };

            var db = DbHelper.WithConn(_portalDB, CodCliente, connection =>
            {
                const string sp = "spSys_UENS_Roles_Registro";
                connection.Execute(sp, new
                {
                    UEN = cod_unidad,
                    CoreUser = request.core_usuario,
                    R_Solicita = Convert.ToInt32(request.rol_solicita),
                    R_Consulta = Convert.ToInt32(request.rol_consulta),
                    R_Autoriza = Convert.ToInt32(request.rol_autoriza),
                    R_Encargado = Convert.ToInt32(request.rol_encargado),
                    R_Lider = Convert.ToInt32(request.rol_lider),
                    Usuario = request.registro_usuario
                }, commandType: CommandType.StoredProcedure);

                return 1;
            });

            return MapDbNonQuery(db, RegistroActualizadoMsg);
        }


        /// <summary>
        /// Obtiene la lista de UENs
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto<List<UensListaDatos>> Core_UENLista_Obtener(int CodCliente, string? usuario)
        {
            var db = DbHelper.WithConn(_portalDB, CodCliente, connection =>
            {
                if (usuario == null || usuario == "null")
                {
                    const string query = @"select U.COD_UNIDAD AS ITEM, U.DESCRIPCION, U.CntX_Unidad, U.CntX_Centro_Costo from CORE_UENS U";
                    return connection.Query<UensListaDatos>(query).ToList();
                }

                const string query2 = @"SELECT S.COD_UNIDAD AS ITEM, U.DESCRIPCION, U.CntX_Unidad, U.CntX_Centro_Costo
                                           FROM CORE_UENS_USUARIOS_ROLES S
                                           LEFT JOIN CORE_UENS U ON S.COD_UNIDAD = U.COD_UNIDAD
                                           WHERE S.CORE_USUARIO = @usuario";

                return connection.Query<UensListaDatos>(query2, new { usuario }).ToList();
            });

            return MapDb(db, () => new List<UensListaDatos>());
        }
    }
}