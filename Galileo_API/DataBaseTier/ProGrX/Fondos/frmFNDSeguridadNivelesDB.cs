using Dapper;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Fondos;
using Galileo.Models.Security;

namespace Galileo.DataBaseTier.ProGrX.Fondos
{
    public class FrmFndSeguridadNivelesDb
    {
        private readonly MSecurityMainDb _securityMainDB;
        private readonly PortalDB _portalDB;
        private readonly int vModulo = 18;

        public FrmFndSeguridadNivelesDb(IConfiguration config)
        {
            _securityMainDB = new MSecurityMainDb(config);
            _portalDB = new PortalDB(config);
        }

        /// <summary>
        /// Obtener Grupos de Seguridad
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Exporta"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDto<TablasListaGenericaModel> Fnd_SegNiveles_Grupos_Obtener(int CodEmpresa, bool Exporta, FiltrosLazyLoadData filtros)
        {
            var response = new ErrorDto<TablasListaGenericaModel>
            {
                Code = 0,
                Description = "Ok",
                Result = new TablasListaGenericaModel
                {
                    total = 0,
                    lista = new List<FndSegNivelesGrupoDto>()
                }
            };

            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);

                var parameters = new DynamicParameters();

                var whereClause = string.Empty;
                if (!string.IsNullOrWhiteSpace(filtros?.filtro))
                {
                    whereClause = " WHERE (COD_GRUPO LIKE @Filter OR DESCRIPCION LIKE @Filter) ";
                    parameters.Add("@Filter", $"%{filtros.filtro}%");
                }

                var countQuery = $"SELECT COUNT(COD_GRUPO) FROM FND_SEGURIDAD_GRUPOS {whereClause};";
                response.Result.total = connection.QueryFirstOrDefault<int>(countQuery, parameters);

                var allowedSortFields = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                {
                    "COD_GRUPO",
                    "DESCRIPCION"
                };

                var sortField = filtros?.sortField;
                if (string.IsNullOrWhiteSpace(sortField) || !allowedSortFields.Contains(sortField))
                    sortField = "COD_GRUPO";

                string sortDirection = (filtros?.sortOrder == 0) ? "DESC" : "ASC";

                var query = $@"select * from FND_SEGURIDAD_GRUPOS
                    {whereClause}
                    order by {sortField} {sortDirection}";

                if (!Exporta)
                {
                    var pageIndex = filtros?.pagina ?? 0;
                    var pageSize = filtros?.paginacion ?? 30;
                    query += " OFFSET @Offset ROWS FETCH NEXT @FetchNext ROWS ONLY";
                    parameters.Add("@Offset", pageSize);
                    parameters.Add("@FetchNext", pageSize);
                }

                response.Result.lista = connection.Query<FndSegNivelesGrupoDto>(query, parameters).ToList();
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = null;
            }

            return response;
        }

        /// <summary>
        /// Obtener planes de seguridad por grupo
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="CodGrupo"></param>
        /// <param name="Filtro"></param>
        /// <returns></returns>
        public ErrorDto<List<FndSegNivelesPlanesData>> Fnd_SegNiveles_Planes_Obtener(int CodEmpresa, string CodGrupo, string? Filtro)
        {
            Filtro ??= string.Empty;

            var query = @"select Pl.cod_Operadora, Pl.cod_Plan,Pl.Descripcion,Asg.registro_Fecha,ASg.Registro_Usuario 
                    from Fnd_Planes Pl left join FND_SEGURIDAD_PLANES Asg on Pl.cod_operadora = Asg.cod_Operadora 
                    and Pl.cod_Plan = Asg.Cod_Plan and Asg.COD_GRUPO = @CodGrupo 
                    where Estado = 'A' and (Pl.Cod_Plan like @Filtro or Pl.Descripcion like @Filtro) 
                    order by isnull(Asg.Cod_Plan,'ZZZZZZZZZZZZ') asc,Pl.cod_Plan asc";

            return DbHelper.ExecuteListQuery<FndSegNivelesPlanesData>(
                _portalDB,
                CodEmpresa,
                query,
                new
                {
                    CodGrupo,
                    Filtro = $"%{Filtro}%"
                });
        }

        /// <summary>
        /// Obtener usuarios de seguridad por grupo
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="CodGrupo"></param>
        /// <param name="Filtro"></param>
        /// <returns></returns>
        public ErrorDto<List<FndSegNivelesUsuariosData>> Fnd_SegNiveles_Usuarios_Obtener(int CodEmpresa, string CodGrupo, string? Filtro)
        {
            Filtro ??= string.Empty;

            var query = @"select Us.Nombre,Us.Descripcion,Asg.registro_Fecha,ASg.Registro_Usuario 
                from Usuarios Us left join FND_SEGURIDAD_USUARIOS Asg on Us.Nombre = Asg.Usuario 
                   and Asg.COD_GRUPO = @CodGrupo 
                Where Us.Estado = 'A' and (Us.Nombre like @Filtro or Us.Descripcion like @Filtro)
                Order by isnull(Asg.Usuario,'ZZZZZZZZZZZ') asc, Us.Nombre asc";

            return DbHelper.ExecuteListQuery<FndSegNivelesUsuariosData>(
                _portalDB,
                CodEmpresa,
                query,
                new
                {
                    CodGrupo,
                    Filtro = $"%{Filtro}%"
                });
        }

        /// <summary>
        /// Guardar grupo de seguridad
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Data"></param>
        /// <returns></returns>
        public ErrorDto Fnd_SegNiveles_Grupos_Guardar(int CodEmpresa, FndSegNivelesGrupoDto Data)
        {
            var response = new ErrorDto
            {
                Code = 0,
                Description = "Ok"
            };

            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);

                const string qryExiste = @"select isnull(count(*),0) from FND_SEGURIDAD_GRUPOS where COD_GRUPO = @CodGrupo";

                int existe = connection.QueryFirstOrDefault<int>(qryExiste, new { CodGrupo = Data.cod_grupo });

                if (existe == 0)
                {
                    const string qryInsert = @"
                        insert into FND_SEGURIDAD_GRUPOS 
                        (
                            COD_GRUPO,
                            descripcion,
                            monto_inicio,
                            Monto_Corte,
                            activo,
                            registro_fecha,
                            registro_usuario
                        )
                        values 
                        (
                            @CodGrupo,
                            @Descripcion,
                            @MontoInicio,
                            @MontoCorte,
                            @Activo,
                            GETDATE(),
                            @Usuario
                        )";

                    connection.Execute(qryInsert, new
                    {
                        CodGrupo = Data.cod_grupo?.Trim(),
                        Descripcion = Data.descripcion?.Trim(),
                        MontoInicio = Data.monto_inicio,
                        MontoCorte = Data.monto_corte,
                        Activo = Data.activo ? 1 : 0,
                        Usuario = Data.registro_usuario?.Trim()
                    });

                    _securityMainDB.Bitacora(new BitacoraInsertarDto
                    {
                        EmpresaId = CodEmpresa,
                        Usuario = Data.registro_usuario?.Trim().ToUpper(),
                        DetalleMovimiento = $"Grupo de Seguridad: {Data.cod_grupo} - {Data.descripcion}",
                        Movimiento = "Registra - WEB",
                        Modulo = vModulo
                    });
                }
                else
                {
                    const string qryUpdate = @"update FND_SEGURIDAD_GRUPOS 
                           set descripcion  = @Descripcion,
                               Monto_Inicio = @MontoInicio,
                               Monto_Corte  = @MontoCorte,
                               activo       = @Activo
                         where COD_GRUPO    = @CodGrupo";

                    connection.Execute(qryUpdate, new
                    {
                        CodGrupo = Data.cod_grupo?.Trim(),
                        Descripcion = Data.descripcion?.Trim(),
                        MontoInicio = Data.monto_inicio,
                        MontoCorte = Data.monto_corte,
                        Activo = Data.activo ? 1 : 0
                    });

                    _securityMainDB.Bitacora(new BitacoraInsertarDto
                    {
                        EmpresaId = CodEmpresa,
                        Usuario = Data.registro_usuario?.Trim().ToUpper(),
                        DetalleMovimiento = $"Grupo de Seguridad: {Data.cod_grupo} - {Data.descripcion}",
                        Movimiento = "Modifica - WEB",
                        Modulo = vModulo
                    });
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }

        /// <summary>
        /// Eliminar grupo de seguridad
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="CodGrupo"></param>
        /// <param name="Usuario"></param>
        /// <returns></returns>
        public ErrorDto Fnd_SegNiveles_Grupos_Eliminar(int CodEmpresa, string CodGrupo, string Usuario)
        {
            var result = new ErrorDto
            {
                Code = 0,
                Description = "Ok"
            };
            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);

                const string sqlPlanes = "DELETE FROM FND_SEGURIDAD_PLANES WHERE COD_GRUPO = @CodGrupo";
                const string sqlUsuarios = "DELETE FROM FND_SEGURIDAD_USUARIOS WHERE COD_GRUPO = @CodGrupo";
                connection.Execute(sqlPlanes, new { CodGrupo });
                connection.Execute(sqlUsuarios, new { CodGrupo });

                const string sql = "delete FND_SEGURIDAD_GRUPOS where COD_GRUPO = @CodGrupo;";
                connection.Execute(sql, new { CodGrupo });

                _securityMainDB.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = Usuario.ToUpper(),
                    DetalleMovimiento = $"Grupo de Seguridad: {CodGrupo}",
                    Movimiento = "Elimina - WEB",
                    Modulo = vModulo
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
        /// Actualizar planes de seguridad
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public ErrorDto Fnd_SegNiveles_Planes_Actualizar(int CodEmpresa, FndSegNivelesPlanesDto data)
        {
            if (data.asignar)
            {
                const string sqlInsert = @"
                        INSERT INTO FND_SEGURIDAD_PLANES
                            (cod_operadora,cod_plan,COD_GRUPO,registro_usuario,registro_fecha)
                        VALUES
                            (@cod_operadora, @cod_plan, @cod_grupo, @registro_usuario, GETDATE());";

                return DbHelper.ExecuteNonQuery(
                    _portalDB,
                    CodEmpresa,
                    sqlInsert,
                    new
                    {
                        data.cod_operadora,
                        data.cod_plan,
                        data.cod_grupo,
                        data.registro_usuario
                    });
            }
            else
            {
                const string sqlDelete = @"
                        DELETE FROM FND_SEGURIDAD_PLANES
                        WHERE cod_operadora = @cod_operadora
                          AND cod_plan = @cod_plan
                          AND COD_GRUPO = @cod_grupo;";

                return DbHelper.ExecuteNonQuery(
                    _portalDB,
                    CodEmpresa,
                    sqlDelete,
                    new
                    {
                        data.cod_operadora,
                        data.cod_plan,
                        data.cod_grupo
                    });
            }
        }

        /// <summary>
        /// Actualizar usuarios de seguridad
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public ErrorDto Fnd_SegNiveles_Usuarios_Actualizar(int CodEmpresa, FndSegNivelesUsuariosDto data)
        {
            if (data.asignar)
            {
                const string sqlInsert = @"
                        INSERT INTO FND_SEGURIDAD_USUARIOS
                            (usuario,COD_GRUPO,registro_usuario,registro_fecha)
                        VALUES
                            (@usuario, @cod_grupo, @registro_usuario, GETDATE());";

                return DbHelper.ExecuteNonQuery(
                    _portalDB,
                    CodEmpresa,
                    sqlInsert,
                    new
                    {
                        data.usuario,
                        data.cod_grupo,
                        data.registro_usuario
                    });
            }
            else
            {
                const string sqlDelete = @"
                        DELETE FROM FND_SEGURIDAD_USUARIOS
                        WHERE usuario = @usuario
                          AND COD_GRUPO = @cod_grupo;";

                return DbHelper.ExecuteNonQuery(
                    _portalDB,
                    CodEmpresa,
                    sqlDelete,
                    new
                    {
                        data.usuario,
                        data.cod_grupo
                    });
            }
        }
    }
}
