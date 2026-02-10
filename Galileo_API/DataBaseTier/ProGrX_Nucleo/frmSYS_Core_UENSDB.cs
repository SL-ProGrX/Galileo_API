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

        public FrmSysCoreUensDB(IConfiguration config)
        {
            _portalDB = new PortalDB(config);
        }

        private static CoreUeNsDtoList EmptyUensList()
            => new CoreUeNsDtoList { uens = new List<CoreUeNsDto>(), Total = 0 };

        private ErrorDto<T> WithClienteConn<T>(int codCliente, Func<SqlConnection, T> action, Func<T> defaultFactory)
        {
            try
            {
                using var conn = DbHelper.OpenConnection(_portalDB, codCliente);
                var result = action(conn);
                return DbHelper.CreateOkResponse(result);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse(ex.Message ?? "Error inesperado", -1, defaultFactory());
            }
        }

        private ErrorDto WithClienteConnNonQuery(int codCliente, Func<SqlConnection, int> action, string okDescription)
        {
            try
            {
                using var conn = DbHelper.OpenConnection(_portalDB, codCliente);
                var rows = action(conn);
                return new ErrorDto { Code = rows, Description = okDescription };
            }
            catch (Exception ex)
            {
                return new ErrorDto { Code = -1, Description = ex.Message ?? "Error inesperado" };
            }
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

            return WithClienteConn(CodCliente, connection =>
            {
                var search = vfiltro?.filtro?.Trim();
                string? searchLike = string.IsNullOrWhiteSpace(search) ? null : $"%{search}%";

                var offset = vfiltro?.pagina ?? 0;
                var fetch = vfiltro?.paginacion ?? 0;
                if (fetch <= 0) fetch = int.MaxValue;

                const string countSql = @"SELECT COUNT(*)
                                          FROM CORE_UENS
                                          WHERE (@search IS NULL
                                                 OR COD_UNIDAD LIKE @search
                                                 OR descripcion LIKE @search);";

                const string pageSql = @"SELECT COD_UNIDAD, descripcion, CntX_Unidad, CntX_Centro_Costo, Activa, 0 as 'btn'
                                         FROM CORE_UENS
                                         WHERE (@search IS NULL
                                                OR COD_UNIDAD LIKE @search
                                                OR descripcion LIKE @search)
                                         ORDER BY COD_UNIDAD DESC
                                         OFFSET @offset ROWS FETCH NEXT @fetch ROWS ONLY;";

                var dto = EmptyUensList();
                dto.Total = connection.Query<int>(countSql, new { search = searchLike }).FirstOrDefault();
                dto.uens = connection.Query<CoreUeNsDto>(pageSql, new { search = searchLike, offset, fetch }).ToList();
                return dto;
            }, EmptyUensList);
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
            return WithClienteConnNonQuery(CodCliente, connection =>
            {
                var activa = request?.activa == true ? 1 : 0;

                const string existsSql = @"select isnull(count(*),0) as Existe from CORE_UENS where COD_UNIDAD = @cod_unidad";
                int existe = connection.Query<int>(existsSql, new { cod_unidad = request?.cod_unidad }).FirstOrDefault();

                if (existe == 0)
                {
                    const string getMaxSql = @"SELECT CAST(MAX(CAST(COD_UNIDAD AS INT)) + 1 AS VARCHAR) AS NuevoCodigo
                        FROM CORE_UENS WHERE ISNUMERIC(COD_UNIDAD) = 1";
                    int ultimoID = connection.Query<int>(getMaxSql).FirstOrDefault();
                    string nuevoCodigo = ultimoID < 10 ? "0" + ultimoID : ultimoID.ToString();

                    const string insertSql = @"insert into CORE_UENS(COD_UNIDAD, descripcion, Activa, Registro_Fecha, Registro_Usuario)
                                              values(@cod_unidad, @descripcion, @activa, Getdate(), @usuario);";

                    return connection.Execute(insertSql, new { cod_unidad = nuevoCodigo, descripcion = request?.descripcion, activa, usuario });
                }

                const string updateSql = @"update CORE_UENS
                                             set descripcion = @descripcion,
                                                 Activa = @activa,
                                                 Modifica_Fecha = Getdate(),
                                                 Modifica_Usuario = @usuario
                                             where COD_UNIDAD = @cod_unidad OR UNIDAD_PRINCIPAL = @cod_unidad;";

                return connection.Execute(updateSql, new { cod_unidad = request?.cod_unidad, descripcion = request?.descripcion, activa, usuario });
            }, request?.cod_unidad == null || request.cod_unidad == "" ? "Registro agregado satisfactoriamente" : RegistroActualizadoMsg);
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
            return WithClienteConnNonQuery(CodCliente, connection =>
            {
                var activa = request?.activa == true ? 1 : 0;

                //Se obtiene información de la unidad principal
                const string qPrincipal = @"select * from CORE_UENS where COD_UNIDAD = @unidad_principal";
                CoreUeNsDto unidadPrincipal = connection.Query<CoreUeNsDto>(qPrincipal, new { unidad_principal = request?.unidad_principal }).First();

                const string qExiste = @"select isnull(count(*),0) as Existe from CORE_UENS
                                        where (COD_UNIDAD = @unidad_principal OR UNIDAD_PRINCIPAL = @unidad_principal)
                                          AND CNTX_UNIDAD = @cntx_unidad";

                int existe = connection.Query<int>(qExiste, new { unidad_principal = request?.unidad_principal, cntx_unidad = request?.cntx_unidad }).FirstOrDefault();

                if (existe == 0 && unidadPrincipal.cntx_unidad == "")
                {
                    //Asigna la unidad a la UEN principal, porque no tiene unidad
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
                    //Agrega una nueva unidad
                    const string getMaxSql = @"SELECT CAST(MAX(CAST(COD_UNIDAD AS INT)) + 1 AS VARCHAR) AS NuevoCodigo
                    FROM CORE_UENS WHERE ISNUMERIC(COD_UNIDAD) = 1";
                    int ultimoID = connection.Query<int>(getMaxSql).FirstOrDefault();
                    string nuevoCodigo = ultimoID < 10 ? "0" + ultimoID : ultimoID.ToString();

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

                //Actualiza la unidad
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
            }, RegistroActualizadoMsg);
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
            return WithClienteConnNonQuery(CodCliente, connection =>
            {
                var activa = request?.activa == true ? 1 : 0;

                const string qExisteCod = @"select isnull(count(*),0) as Existe from CORE_UENS where COD_UNIDAD = @cod_unidad";
                int existe = connection.Query<int>(qExisteCod, new { cod_unidad = request?.cod_unidad }).FirstOrDefault();

                if (existe == 0)
                {
                    //Se obtiene información de la unidad principal
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

                    const string getMaxSql = @"SELECT CAST(MAX(CAST(COD_UNIDAD AS INT)) + 1 AS VARCHAR) AS NuevoCodigo
                            FROM CORE_UENS WHERE ISNUMERIC(COD_UNIDAD) = 1";
                    int ultimoID = connection.Query<int>(getMaxSql).FirstOrDefault();
                    string nuevoCodigo = ultimoID < 10 ? "0" + ultimoID : ultimoID.ToString();

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
            }, request?.cod_unidad == null || request.cod_unidad == "" ? "Registro agregado satisfactoriamente" : RegistroActualizadoMsg);
        }

        /// <summary>
        /// Borra las UEns
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="cod_unidad"></param>
        /// <returns></returns>
        public ErrorDto Core_UENS_Delete(int CodCliente, string cod_unidad)
        {
            return WithClienteConnNonQuery(CodCliente, connection =>
            {
                const string query = @"delete from CORE_UENS where COD_UNIDAD = @cod_unidad OR UNIDAD_PRINCIPAL = @cod_unidad";
                return connection.Execute(query, new { cod_unidad });
            }, "Registros eliminados satisfactoriamente");
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
            return WithClienteConnNonQuery(CodCliente, connection =>
            {
                const string qFind = @"select COD_UNIDAD from CORE_UENS where UNIDAD_PRINCIPAL = @cod_unidad AND CNTX_UNIDAD = @cntx_unidad";
                string? codUnidad = connection.Query<string>(qFind, new { cod_unidad, cntx_unidad }).FirstOrDefault();

                //Valida si es la unidad principal
                if (codUnidad == null)
                {
                    //Es la UEN Principal
                    const string delRoles = @"delete from CORE_UENS_USUARIOS_ROLES where COD_UNIDAD = @cod_unidad";
                    connection.Execute(delRoles, new { cod_unidad });

                    const string delUens = @"delete from CORE_UENS where COD_UNIDAD = @cod_unidad";
                    connection.Execute(delUens, new { cod_unidad });

                    //Valida si existen otras unidades asociadas a la UEN Principal
                    const string qTopUnidad = @"select TOP 1 COD_UNIDAD from CORE_UENS where UNIDAD_PRINCIPAL = @cod_unidad order by ACTIVA desc";
                    string? nuevaUnidadPrincipal = connection.Query<string>(qTopUnidad, new { cod_unidad }).FirstOrDefault();

                    if (nuevaUnidadPrincipal != null)
                    {
                        const string upUensPrincipal = @"update CORE_UENS set 
                            UNIDAD_PRINCIPAL = @nuevaUnidadPrincipal 
                            where UNIDAD_PRINCIPAL = @cod_unidad";
                        connection.Execute(upUensPrincipal, new { nuevaUnidadPrincipal, cod_unidad });

                        const string upNullPrincipal = @"update CORE_UENS set 
                            UNIDAD_PRINCIPAL = NULL 
                            where COD_UNIDAD = @nuevaUnidadPrincipal";
                        connection.Execute(upNullPrincipal, new { nuevaUnidadPrincipal });
                    }

                    return 1;
                }

                //No es la UEN Principal
                const string delRoles2 = @"delete from CORE_UENS_USUARIOS_ROLES where COD_UNIDAD = @codUnidad";
                connection.Execute(delRoles2, new { codUnidad });

                const string delUens2 = @"delete from CORE_UENS where COD_UNIDAD = @codUnidad";
                connection.Execute(delUens2, new { codUnidad });

                return 1;
            }, "Registros eliminado satisfactoriamente");
        }


        /// <summary>
        /// Borrar el centro de costo de una UEN
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="cod_unidad"></param>
        /// <returns></returns>
        public ErrorDto Core_SubCentroCosto_Delete(int CodCliente, string cod_unidad)
        {
            return WithClienteConnNonQuery(CodCliente, connection =>
            {
                const string qInfo = @"select * from CORE_UENS where COD_UNIDAD = @cod_unidad";
                CoreUeNsDto unidadInfo = connection.Query<CoreUeNsDto>(qInfo, new { cod_unidad }).First();

                //Valida si es la unidad principal
                if (unidadInfo.unidad_principal == "")
                {
                    //Es la UEN Principal
                    const string delRoles = @"delete from CORE_UENS_USUARIOS_ROLES where COD_UNIDAD = @cod_unidad";
                    connection.Execute(delRoles, new { cod_unidad });

                    const string delUens = @"delete from CORE_UENS where COD_UNIDAD = @cod_unidad";
                    connection.Execute(delUens, new { cod_unidad });

                    //Valida si existen otras unidades asociadas a la UEN Principal
                    const string qTopUnidad = @"select TOP 1 COD_UNIDAD from CORE_UENS where UNIDAD_PRINCIPAL = @cod_unidad order by ACTIVA desc";
                    string? nuevaUnidadPrincipal = connection.Query<string>(qTopUnidad, new { cod_unidad }).FirstOrDefault();
                    if (nuevaUnidadPrincipal != null)
                    {
                        const string upUensPrincipal = @"update CORE_UENS set 
                            UNIDAD_PRINCIPAL = @nuevaUnidadPrincipal 
                            where UNIDAD_PRINCIPAL = @cod_unidad";
                        connection.Execute(upUensPrincipal, new { nuevaUnidadPrincipal, cod_unidad });

                        const string upNullPrincipal = @"update CORE_UENS set 
                            UNIDAD_PRINCIPAL = NULL 
                            where COD_UNIDAD = @nuevaUnidadPrincipal";
                        connection.Execute(upNullPrincipal, new { nuevaUnidadPrincipal });
                    }

                    return 1;
                }

                //No es la UEN Principal
                const string delRoles2 = @"delete from CORE_UENS_USUARIOS_ROLES where COD_UNIDAD = @cod_unidad";
                connection.Execute(delRoles2, new { cod_unidad });

                const string delUens2 = @"delete from CORE_UENS where COD_UNIDAD = @cod_unidad";
                connection.Execute(delUens2, new { cod_unidad });

                return 1;
            }, "Registro eliminado satisfactoriamente");
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

            return WithClienteConn(CodCliente, connection =>
            {
                var search = vfiltro?.filtro?.Trim();
                string? searchLike = string.IsNullOrWhiteSpace(search) ? null : $"%{search}%";

                var offset = vfiltro?.pagina ?? 0;
                var fetch = vfiltro?.paginacion ?? 0;
                if (fetch <= 0) fetch = int.MaxValue;

                const string countSql = @"SELECT COUNT(*)
                                          FROM CORE_UENS
                                          WHERE UNIDAD_PRINCIPAL IS NULL
                                            AND (@search IS NULL
                                                 OR COD_UNIDAD LIKE @search
                                                 OR descripcion LIKE @search);";

                const string pageSql = @"SELECT COD_UNIDAD, descripcion, CntX_Unidad, CntX_Centro_Costo, Activa, 0 as 'btn'
                                         FROM CORE_UENS
                                         WHERE UNIDAD_PRINCIPAL IS NULL
                                           AND (@search IS NULL
                                                OR COD_UNIDAD LIKE @search
                                                OR descripcion LIKE @search)
                                         ORDER BY COD_UNIDAD DESC
                                         OFFSET @offset ROWS FETCH NEXT @fetch ROWS ONLY;";

                var dto = EmptyUensList();
                dto.Total = connection.Query<int>(countSql, new { search = searchLike }).FirstOrDefault();
                dto.uens = connection.Query<CoreUeNsDto>(pageSql, new { search = searchLike, offset, fetch }).ToList();
                return dto;
            }, EmptyUensList);
        }


        /// <summary>
        /// Obtener las unidades pertenecientes a una UEN
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="cod_unidad"></param>
        /// <returns></returns>
        public ErrorDto<CoreUeNsDtoList> Core_SubUnidades_Obtener(int CodCliente, string cod_unidad)
        {
            return WithClienteConn(CodCliente, connection =>
            {
                const string query = @"select DISTINCT @cod_unidad AS COD_UNIDAD, C.CNTX_UNIDAD, @cod_unidad as UNIDAD_PRINCIPAL,
                    (select TOP 1 DESCRIPCION from CNTX_UNIDADES WHERE COD_UNIDAD = C.CNTX_UNIDAD) AS DESCRIPCION
                    from CORE_UENS C
                    WHERE C.UNIDAD_PRINCIPAL = @cod_unidad OR C.COD_UNIDAD = @cod_unidad
                    order by C.CNTX_UNIDAD desc";

                var dto = EmptyUensList();
                dto.Total = 0;
                dto.uens = connection.Query<CoreUeNsDto>(query, new { cod_unidad }).ToList();

                // Mantiene el comportamiento anterior: si no hay cntx_unidad, Result = null
                if (dto.uens == null || dto.uens.Count == 0 || string.IsNullOrWhiteSpace(dto.uens[0].cntx_unidad))
                    return null!;

                return dto;
            }, EmptyUensList);
        }


        /// <summary>
        /// Obtener los centros de costo pertenecientes a una UEN
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="cod_unidad"></param>
        /// <returns></returns>
        public ErrorDto<CoreUeNsDtoList> Core_SubCentroCosto_Obtener(int CodCliente, string cod_unidad, string sub_unidad)
        {
            return WithClienteConn(CodCliente, connection =>
            {
                const string query = @"select C.COD_UNIDAD, C.CNTX_UNIDAD, C.CNTX_CENTRO_COSTO, C.ACTIVA, @cod_unidad AS UNIDAD_PRINCIPAL,
                    (select TOP 1 DESCRIPCION from CNTX_CENTRO_COSTOS WHERE COD_CENTRO_COSTO = C.CNTX_CENTRO_COSTO) AS DESCRIPCION
                    from CORE_UENS C
                    WHERE (C.UNIDAD_PRINCIPAL = @cod_unidad OR C.COD_UNIDAD = @cod_unidad) AND C.CNTX_UNIDAD = @sub_unidad
                    order by C.CNTX_CENTRO_COSTO desc";

                var dto = EmptyUensList();
                dto.Total = 0;
                dto.uens = connection.Query<CoreUeNsDto>(query, new { cod_unidad, sub_unidad }).ToList();

                // Mantiene el comportamiento anterior: si no hay cntx_centro_costo, Result = null
                if (dto.uens == null || dto.uens.Count == 0 || string.IsNullOrWhiteSpace(dto.uens[0].cntx_centro_costo))
                    return null!;

                return dto;
            }, EmptyUensList);
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
            return WithClienteConn(CodCliente, connection =>
            {
                const string sp = "spSys_UENS_Miembros_Consultas";
                return connection.Query<CoreUsuariosDto>(sp, new
                {
                    cod_unidad,
                    filtro = (filtro ?? string.Empty)
                }, commandType: CommandType.StoredProcedure).ToList();
            }, () => new List<CoreUsuariosDto>());
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
            return WithClienteConnNonQuery(CodCliente, connection =>
            {
                var mov = request.asignado ? 'A' : 'E';
                const string sp = "spSys_UENS_Miembros_Registro";

                connection.Execute(sp, new
                {
                    cod_unidad,
                    core_usuario = request.core_usuario,
                    registro_usuario = request.registro_usuario,
                    mov
                }, commandType: CommandType.StoredProcedure);

                return 1;
            }, RegistroActualizadoMsg);
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
            return WithClienteConn(CodCliente, connection =>
            {
                const string sp = "spSys_UENS_Roles_Consultas";
                return connection.Query<CoreRolesDto>(sp, new
                {
                    cod_unidad,
                    filtro = (filtro ?? string.Empty)
                }, commandType: CommandType.StoredProcedure).ToList();
            }, () => new List<CoreRolesDto>());
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

            return WithClienteConnNonQuery(CodCliente, connection =>
            {
                const string sp = "spSys_UENS_Roles_Registro";
                connection.Execute(sp, new
                {
                    cod_unidad,
                    core_usuario = request.core_usuario,
                    rol_solicita = Convert.ToInt32(request.rol_solicita),
                    rol_consulta = Convert.ToInt32(request.rol_consulta),
                    rol_autoriza = Convert.ToInt32(request.rol_autoriza),
                    rol_encargado = Convert.ToInt32(request.rol_encargado),
                    rol_lider = Convert.ToInt32(request.rol_lider),
                    registro_usuario = request.registro_usuario
                }, commandType: CommandType.StoredProcedure);

                return 1;
            }, RegistroActualizadoMsg);
        }


        /// <summary>
        /// Obtiene la lista de UENs
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto<List<UensListaDatos>> Core_UENLista_Obtener(int CodCliente, string? usuario)
        {
            return WithClienteConn(CodCliente, connection =>
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
            }, () => new List<UensListaDatos>());
        }
    }
}