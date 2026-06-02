using Dapper;
using Microsoft.Data.SqlClient;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Clientes;
using System.Data;

namespace Galileo.DataBaseTier
{
    public class FrmAfReportesDb
    {
        private const string ConsultaRealizadaCorrectamente = "Consulta realizada correctamente";
        private readonly IConfiguration _config;

        public FrmAfReportesDb(IConfiguration config)
        {
            _config = config;
        }

        /// <summary>
        /// Obtiene los cat�logos de filtros para el m�dulo de reportes (AFI_Reportes)
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<AfReportesCombosDto> AF_Reportes_Combos_Obtener(int CodEmpresa)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            var response = new ErrorDto<AfReportesCombosDto>
            {
                Code = 0,
                Description = ConsultaRealizadaCorrectamente,
                Result = new AfReportesCombosDto()
            };

            try
            {
                using var connection = new SqlConnection(stringConn);

                using var multi = connection.QueryMultiple(
                    "spAFI_ReportesCombos_Cargar",
                    commandType: CommandType.StoredProcedure);

                Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;

                response.Result.Provincias = multi.Read<DropDownListaGenericaModel>().ToList();
                response.Result.GruposUsuarios = multi.Read<DropDownListaGenericaModel>().ToList();
                response.Result.Instituciones = multi.Read<DropDownListaGenericaModel>().ToList();
                response.Result.Promotores = multi.Read<DropDownListaGenericaModel>().ToList();
                response.Result.EstadosPersona = multi.Read<DropDownListaGenericaModel>().ToList();
                response.Result.EstadoCivil = multi.Read<DropDownListaGenericaModel>().ToList();
                response.Result.Profesiones = multi.Read<DropDownListaGenericaModel>().ToList();
                response.Result.Sectores = multi.Read<DropDownListaGenericaModel>().ToList();
                response.Result.Zonas = multi.Read<DropDownListaGenericaModel>().ToList();
                response.Result.TiposIdentificacion = multi.Read<DropDownListaGenericaModel>().ToList();
                response.Result.EstadoLaboral = multi.Read<DropDownListaGenericaModel>().ToList();
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }

        // Nuevo metodo extra�do
        private static List<DropDownListaGenericaModel> ObtenerProvincias(SqlConnection connection)
        {
            var query = @"select Provincia as item, rtrim(Descripcion) as descripcion from Provincias";
            return connection.Query<DropDownListaGenericaModel>(query).ToList();
        }

        /// <summary>
        /// Obtener Provincia
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> AF_Provincias_Obtener(int CodEmpresa)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<List<DropDownListaGenericaModel>>
            {
                Code = 0,
                Description = ConsultaRealizadaCorrectamente,
                Result = new List<DropDownListaGenericaModel>()
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                response.Result = ObtenerProvincias(connection);
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }
            return response;
        }

        // Nuevo metodo extra�do para la consulta de Cantones
        private static List<DropDownListaGenericaModel> ObtenerCantones(SqlConnection connection, string Provincia)
        {
            var query = @"select Canton as item, rtrim(Descripcion) as descripcion from Cantones
                  where provincia = @Provincia order by descripcion";
            return connection.Query<DropDownListaGenericaModel>(query, new { Provincia }).ToList();
        }

        /// <summary>
        /// Obtiene Cantones
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Provincia"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> AF_Cantones_Obtener(int CodEmpresa, string Provincia)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<List<DropDownListaGenericaModel>>
            {
                Code = 0,
                Description = ConsultaRealizadaCorrectamente,
                Result = new List<DropDownListaGenericaModel>()
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                response.Result = ObtenerCantones(connection, Provincia);
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }
            return response;
        }

        // Nuevo metodo extra�do para obtener unidades de trabajo
        private static List<DropDownListaGenericaModel> ObtenerUnidadesTrabajo(SqlConnection connection)
        {
            var query = @"
        SELECT ut_codigo AS item, 
               ut_descripcion AS descripcion
        FROM UTRABAJO
        ORDER BY ut_descripcion;
    ";
            return connection.Query<DropDownListaGenericaModel>(query).ToList();
        }

        public ErrorDto<List<DropDownListaGenericaModel>> AF_UTrabajo_Obtener(int CodEmpresa)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<List<DropDownListaGenericaModel>>
            {
                Code = 0,
                Result = new List<DropDownListaGenericaModel>()
            };

            try
            {
                using var connection = new SqlConnection(stringConn);
                response.Result = ObtenerUnidadesTrabajo(connection);
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = null;
            }

            return response;
        }

        // Nuevo metodo extra�do para obtener unidades program�ticas
        private static List<DropDownListaGenericaModel> ObtenerUnidadesProgramaticas(SqlConnection connection)
        {
            var query = @"
        SELECT codigo AS item,
               descripcion AS descripcion
        FROM uprogramatica
        ORDER BY descripcion;
    ";
            return connection.Query<DropDownListaGenericaModel>(query).ToList();
        }

        /// <summary>
        /// Obtiene las unidades programaticas
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> AF_UProgramatica_Obtener(int CodEmpresa)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<List<DropDownListaGenericaModel>>
            {
                Code = 0,
                Result = new List<DropDownListaGenericaModel>()
            };

            try
            {
                using var connection = new SqlConnection(stringConn);
                response.Result = ObtenerUnidadesProgramaticas(connection);
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
        /// Obtiene la fecha del servidor
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto FechaServidor_Obtener(int CodEmpresa)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            var response = new ErrorDto
            {
                Code = 0,
                Description = "Operaci�n realizada correctamente"
            };

            try
            {
                using var connection = new SqlConnection(stringConn);
                string sql = "SELECT dbo.MyGetdate() AS Fecha";

                var fechaServidor = connection.QuerySingle<DateTime>(sql);

                response.Description = fechaServidor.ToString("yyyy-MM-dd HH:mm:ss");
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }

        // Nuevo metodo extra�do para obtener los grupos de configuraci�n
        private static List<AfGrupoConfiguracionDto> ObtenerGruposConfiguracion(SqlConnection connection)
        {
            var query = @"
        SELECT 
            cod_grupo,
            RTRIM(descripcion) AS descripcion
        FROM afi_grupos
        ORDER BY cod_grupo;
    ";
            return connection.Query<AfGrupoConfiguracionDto>(query).ToList();
        }

        /// <summary>
        /// Obtiene los grupos de configuracion
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<AfGrupoConfiguracionDto>> AF_Configuracion_Grupos_Obtener(int CodEmpresa)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<List<AfGrupoConfiguracionDto>>
            {
                Code = 0,
                Result = new List<AfGrupoConfiguracionDto>()
            };

            try
            {
                using var connection = new SqlConnection(stringConn);
                response.Result = ObtenerGruposConfiguracion(connection);
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = null;
            }

            return response;
        }

        // Nuevo metodo extra�do para obtener los miembros de configuraci�n
        private static List<AfGrupoMiembroDto> ObtenerMiembrosConfiguracion(SqlConnection connection, int CodGrupo)
        {
            var query = @"
        SELECT 
            U.nombre AS nombre,
            U.descripcion AS descripcion,
            A.usuario AS usuario
        FROM Usuarios U
        LEFT JOIN afi_grpusers A 
            ON U.nombre = A.usuario
           AND A.cod_grupo = @CodGrupo
        WHERE U.estado = 'A'
        ORDER BY A.usuario DESC, U.nombre ASC;
    ";

            return connection.Query<AfGrupoMiembroDto>(query, new { CodGrupo }).ToList();
        }

        /// <summary>
        /// Obtiene los miembros de configuracion
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="CodGrupo"></param>
        /// <returns></returns>
        public ErrorDto<List<AfGrupoMiembroDto>> AF_Configuracion_Miembros_Obtener(int CodEmpresa, int CodGrupo)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<List<AfGrupoMiembroDto>>
            {
                Code = 0,
                Result = new List<AfGrupoMiembroDto>()
            };

            try
            {
                using var connection = new SqlConnection(stringConn);
                response.Result = ObtenerMiembrosConfiguracion(connection, CodGrupo);
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = null;
            }

            return response;
        }

        // Nuevo metodo extra�do para obtener los informes de configuraci�n
        private static List<AfReporteDto> ObtenerInformesConfiguracion(SqlConnection connection)
        {
            var query = @"
        SELECT 
            ID_Rep,
            Tipo,
            Reporte,
            Prefijo,
            ISNULL(Seguridad, 0) AS Seguridad
        FROM afi_reportes
        ORDER BY Tipo, Reporte;
    ";
            return connection.Query<AfReporteDto>(query).ToList();
        }

        /// <summary>
        /// Obtiene los informes de configuracion
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<AfReporteDto>> AF_Configuracion_Informes_Obtener(int CodEmpresa)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<List<AfReporteDto>>
            {
                Code = 0,
                Result = new List<AfReporteDto>()
            };

            try
            {
                using var connection = new SqlConnection(stringConn);
                response.Result = ObtenerInformesConfiguracion(connection);
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = null;
            }

            return response;
        }

        // Nuevo metodo extra�do para obtener los grupos de seguridad
        private static List<AfSeguridadGrupoDto> ObtenerGruposSeguridad(SqlConnection connection)
        {
            var query = @"
        SELECT 
            cod_grupo AS CodGrupo,
            descripcion AS Descripcion,
            activo AS Activo
        FROM afi_reportes_grp
        ORDER BY cod_grupo;
    ";
            return connection.Query<AfSeguridadGrupoDto>(query).ToList();
        }

        /// <summary>
        /// Obtiene los grupos de seguridad
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<AfSeguridadGrupoDto>> AF_Seguridad_Grupos_Obtener(int CodEmpresa)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<List<AfSeguridadGrupoDto>>
            {
                Code = 0,
                Result = new List<AfSeguridadGrupoDto>()
            };

            try
            {
                using var connection = new SqlConnection(stringConn);
                response.Result = ObtenerGruposSeguridad(connection);
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }

        // Nuevo metodo extra�do para obtener miembros de seguridad
        private static List<AfSeguridadMiembroDto> ObtenerMiembrosSeguridad(SqlConnection connection, int CodGrupo)
        {
            var query = @"
        SELECT 
            U.nombre AS Nombre,
            U.descripcion AS Descripcion,
            A.usuario AS Usuario
        FROM Usuarios U
        LEFT JOIN afi_reportes_GRP_USR A 
            ON U.nombre = A.usuario
           AND A.cod_grupo = @CodGrupo
        WHERE U.estado = 'A'
        ORDER BY A.usuario DESC, U.nombre ASC;
    ";

            return connection.Query<AfSeguridadMiembroDto>(query, new { CodGrupo }).ToList();
        }

        /// <summary>
        /// Obtiene los miembros de seguridad
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="CodGrupo"></param>
        /// <returns></returns>
        public ErrorDto<List<AfSeguridadMiembroDto>> AF_Seguridad_Miembros_Obtener(int CodEmpresa, int CodGrupo)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<List<AfSeguridadMiembroDto>>
            {
                Code = 0,
                Result = new List<AfSeguridadMiembroDto>()
            };

            try
            {
                using var connection = new SqlConnection(stringConn);
                response.Result = ObtenerMiembrosSeguridad(connection, CodGrupo);
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }

        // Nuevo metodo extra�do para obtener los reportes de seguridad por grupo
        private static List<AfSeguridadReporteDto> ObtenerReportesSeguridadPorGrupo(SqlConnection connection, string CodGrupo)
        {
            var query = @"
        SELECT 
            R.tipo AS Tipo,
            R.id_REP AS IdRep,
            R.reporte AS Reporte,
            A.cod_grupo AS CodGrupo
        FROM afi_reportes R
        LEFT JOIN afi_reportes_GRP_AUT A 
            ON R.id_REP = A.id_REP
           AND A.cod_grupo = @CodGrupo
        ORDER BY A.cod_grupo DESC, R.tipo ASC, R.id_REP ASC;
    ";

            return connection.Query<AfSeguridadReporteDto>(query, new { CodGrupo }).ToList();
        }

        /// <summary>
        /// Obtiene los reportes de seguridad
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="CodGrupo"></param>
        /// <returns></returns>
        public ErrorDto<List<AfSeguridadReporteDto>> AF_Seguridad_Reportes_Obtener(int CodEmpresa, string CodGrupo)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<List<AfSeguridadReporteDto>>
            {
                Code = 0,
                Result = new List<AfSeguridadReporteDto>()
            };

            try
            {
                using var connection = new SqlConnection(stringConn);
                response.Result = ObtenerReportesSeguridadPorGrupo(connection, CodGrupo);
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }

        // Nuevo metodo extraido para obtener los grupos
        private static List<DropDownListaGenericaModel> ObtenerGrupos(SqlConnection connection)
        {
            var query = @"
        SELECT 
            RTRIM(cod_grupo) AS item,
            RTRIM(descripcion) AS descripcion
        FROM AFI_Grupos
        ORDER BY descripcion;
    ";
            return connection.Query<DropDownListaGenericaModel>(query).ToList();
        }

        /// <summary>
        /// Obtiene los grupos
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> AF_Grupos_Obtener(int CodEmpresa)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<List<DropDownListaGenericaModel>>
            {
                Code = 0,
                Result = new List<DropDownListaGenericaModel>()
            };

            try
            {
                using var connection = new SqlConnection(stringConn);
                response.Result = ObtenerGrupos(connection);
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
        /// Obtiene los miembros de grupos
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> AF_Miembros_Grupos_Obtener(int CodEmpresa)
        {
            string conn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<List<DropDownListaGenericaModel>>
            {
                Code = 0,
                Result = new List<DropDownListaGenericaModel>()
            };

            try
            {
                using var connection = new SqlConnection(conn);
                var query = @"
                        SELECT 
                            cod_grupo AS item,
                            descripcion AS descripcion
                        FROM afi_reportes_grp
                        ORDER BY cod_grupo;
                    ";

                response.Result = connection.Query<DropDownListaGenericaModel>(query).ToList();
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }

        /// <summary>
        /// Guarda los grupos
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="grupo"></param>
        /// <returns></returns>
        public ErrorDto AF_Grupos_Guardar(int CodEmpresa, AfGrupoConfiguracionDto grupo)
        {
            string conn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto { Code = 0 };

            try
            {
                using var connection = new SqlConnection(conn);

                var existe = connection.ExecuteScalar<int>(
                    "SELECT COUNT(*) FROM AFI_Grupos WHERE cod_grupo = @cod_grupo",
                    new { grupo.cod_grupo });

                if (existe == 0)
                {
                    // Inserta nuevo grupo
                    connection.Execute(@"
                INSERT INTO AFI_Grupos (cod_grupo, descripcion)
                VALUES (@cod_grupo, @Descripcion);", grupo);

                    response.Description = "Grupo registrado correctamente.";
                }
                else
                {
                    // Actualiza grupo existente
                    connection.Execute(@"
                UPDATE AFI_Grupos
                SET descripcion = @Descripcion
                WHERE cod_grupo = @cod_grupo;", grupo);

                    response.Description = "Grupo actualizado correctamente.";
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
        /// Guarda los miembros
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="cod_grupo"></param>
        /// <param name="miembro"></param>
        /// <returns></returns>
        public ErrorDto AF_Miembros_Guardar(int CodEmpresa, string cod_grupo, AfGrupoMiembroDto miembro)
        {
            string conn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto { Code = 0 };

            try
            {
                using var connection = new SqlConnection(conn);

                var existe = connection.ExecuteScalar<int>(
                    @"SELECT COUNT(*) FROM afi_grpusers WHERE cod_grupo = @cod_grupo AND usuario = @usuario",
                    new { cod_grupo, miembro.usuario });

                if (existe == 0)
                {
                    connection.Execute(@"INSERT INTO afi_grpusers (cod_grupo, usuario)
                                    VALUES (@cod_grupo, @usuario);",
                    new { cod_grupo, usuario = miembro.nombre });

                }
                if (existe > 0)
                {
                    connection.Execute(@"
                        DELETE FROM afi_grpusers WHERE cod_grupo = @cod_grupo AND usuario = @usuario;",
                        new { cod_grupo, usuario = miembro.nombre });
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
        /// Obtiene los reportes a guardar
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="reporte"></param>
        /// <returns></returns>
        public ErrorDto AF_Reportes_Guardar(int CodEmpresa, AfReporteDto reporte)
        {
            string conn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto { Code = 0 };

            try
            {
                using var connection = new SqlConnection(conn);

                var existe = connection.ExecuteScalar<int>(
                    @"SELECT COUNT(*) FROM afi_reportes WHERE id_rep = @Id_Rep",
                    new { reporte.id_rep });

                if (existe == 0)
                {
                    // Inserta nuevo reporte
                    connection.Execute(@"
                INSERT INTO afi_reportes (tipo, reporte, prefijo, seguridad)
                VALUES (@Tipo, @Reporte, @Prefijo, @Seguridad);", reporte);

                    response.Description = "Reporte registrado correctamente.";
                }
                else
                {
                    // Actualiza reporte existente
                    connection.Execute(@"
                UPDATE afi_reportes
                SET tipo = @Tipo,
                    reporte = @Reporte,
                    prefijo = @Prefijo,
                    seguridad = @Seguridad
                WHERE id_rep = @Id_Rep;", reporte);

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
        /// Guarda los grupos
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="grupo"></param>
        /// <returns></returns>
        public ErrorDto AF_Reportes_Grupo_Guardar(int CodEmpresa, AfSeguridadGrupoDto grupo)
        {
            string connStr = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto { Code = 0 };

            try
            {
                using var connection = new SqlConnection(connStr);

                // Verifica si el grupo existe
                var existe = connection.ExecuteScalar<int>(@"
                SELECT COUNT(*) 
                FROM afi_reportes_grp 
                WHERE cod_grupo = @codgrupo",
                    new { grupo.codgrupo });

                if (existe == 0)
                {
                    // INSERT (igual al VB6: descripcion, activo)
                    connection.Execute(@"
                    INSERT INTO afi_reportes_grp (descripcion, activo)
                    VALUES (@Descripcion, @Activo);",
                        grupo);

                    response.Description = "Grupo registrado correctamente.";
                }
                else
                {
                    // UPDATE
                    connection.Execute(@"
                    UPDATE afi_reportes_grp
                    SET descripcion = @Descripcion,
                        activo = @Activo
                    WHERE cod_grupo = @codgrupo;",
                        grupo);

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
        /// Guarda los miembros de grupo de reporte
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="cod_grupo"></param>
        /// <param name="miembroseguridad"></param>
        /// <returns></returns>
        public ErrorDto AF_Reportes_Grupo_Miembros_Guardar(int CodEmpresa, string cod_grupo, AfSeguridadMiembroDto miembroseguridad)
        {
            var response = new ErrorDto { Code = 0 };

            try
            {
                string conn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

                using var connection = new SqlConnection(conn);

                var existe = connection.ExecuteScalar<int>(
                    @"SELECT COUNT(*) FROM afi_reportes_grp_usr  WHERE cod_grupo = @cod_grupo AND usuario = @usuario",
                    new { cod_grupo, miembroseguridad.usuario });

                if (existe == 0)
                {
                    connection.Execute(@"INSERT INTO afi_reportes_grp_usr  (cod_grupo, usuario)
                                    VALUES (@cod_grupo, @usuario);",
                    new { cod_grupo, usuario = miembroseguridad.nombre });

                }
                if (existe > 0)
                {
                    connection.Execute(@"
                        DELETE FROM afi_reportes_grp_usr  WHERE cod_grupo = @cod_grupo AND usuario = @usuario;",
                        new { cod_grupo, usuario = miembroseguridad.nombre });
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
        /// Guarda los reportes de seguridad
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="id_rep"></param>
        /// <param name="cod_grupo"></param>
        /// <returns></returns>
        public ErrorDto AF_Reportes_Seguridad_Guardar(int CodEmpresa, string id_rep, string cod_grupo)
        {
            string conn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            var response = new ErrorDto
            {
                Code = 0,
                Description = "OK"
            };

            try
            {
                using var connection = new SqlConnection(conn);

                // Verificar existencia del registro
                var existe = connection.ExecuteScalar<int>(
                    @"SELECT COUNT(*)
              FROM afi_reportes_GRP_AUT
              WHERE id_rep = @Id_Rep
                AND cod_grupo = @CodGrupo",
                    new { Id_Rep = id_rep, CodGrupo = cod_grupo });

                if (existe == 0)
                {
                    // NO EXISTE ? INSERT (igual que TRASMEX)
                    string insert = @"
                INSERT INTO afi_reportes_GRP_AUT
                (id_rep, cod_grupo)
                VALUES
                (@Id_Rep, @CodGrupo)";

                    connection.Execute(insert, new
                    {
                        Id_Rep = id_rep,
                        CodGrupo = string.IsNullOrEmpty(cod_grupo) ? null : cod_grupo
                    });

                }
                else
                {
                    string delete = @"
                        DELETE FROM afi_reportes_GRP_AUT
                        WHERE id_rep = @Id_Rep
                          AND cod_grupo = @CodGrupo;";

                    connection.Execute(delete, new
                    {
                        Id_Rep = id_rep,
                        CodGrupo = cod_grupo
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

        // Nuevo metodo extraido para la consulta de Distritos
        private static List<DropDownListaGenericaModel> ObtenerDistritos(SqlConnection connection, string Provincia, string Canton)
        {
            var query = @"select Distrito as item, rtrim(Descripcion) as descripcion from Distritos
                        where provincia = @Provincia and canton = @Canton order by descripcion";
            return connection.Query<DropDownListaGenericaModel>(query, new { Provincia, Canton }).ToList();
        }

        /// <summary>
        /// Obtiene Distritos
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Provincia"></param>
        /// <param name="Canton"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> AF_Distritos_Obtener(int CodEmpresa, string Provincia, string Canton)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<List<DropDownListaGenericaModel>>
            {
                Code = 0,
                Description = ConsultaRealizadaCorrectamente,
                Result = new List<DropDownListaGenericaModel>()
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                response.Result = ObtenerDistritos(connection, Provincia, Canton);
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }
            return response;
        }
    }
}
