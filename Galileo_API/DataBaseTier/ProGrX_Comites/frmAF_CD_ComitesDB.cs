using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.Security;
using Galileo_API.Models.ProGrX_Comites;
using Microsoft.Data.SqlClient;

namespace Galileo_API.DataBaseTier.ProGrX_Comites
{
    public class FrmAfCdComitesDB
    {
        private readonly PortalDB _portalDb;
        private readonly MSecurityMainDb _securityMainDb;
        private const int VModulo = 40;
        private const string TipoUnidad = "unidad";
        private const string TipoActividad = "actividad";
        private const string TipoEjecutivo = "ejecutivo";
        private const string TipoAsociacionNoValido = "Tipo de asociacion no valido.";

        public FrmAfCdComitesDB(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
            _securityMainDb = new MSecurityMainDb(config);
        }

        /// <summary>
        /// Obtiene el detalle completo de un comite con sus listas relacionadas.
        /// </summary>
        public ErrorDto<AfCdComiteDetalleDto> AfCdComites_Detalle(int codEmpresa, string codComite)
        {
            var result = DbHelper.CreateOkResponse(new AfCdComiteDetalleDto());
            try
            {
                using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);
                var detalle = new AfCdComiteDetalleDto
                {
                    Comite = ObtenerResumen(conn, codComite),
                    Unidades = ObtenerUnidades(conn, codComite),
                    Actividades = ObtenerActividades(conn, codComite),
                    Ejecutivos = ObtenerEjecutivos(conn, codComite),
                    Miembros = ObtenerMiembros(conn, codComite, true),
                    Liquidaciones = ObtenerLiquidaciones(conn, codComite),
                    LiquidacionesHistorico = ObtenerLiquidacionesHistorico(conn, codComite),
                    Mensajes = ObtenerMensajes(conn, codComite)
                };
                result.Result = detalle;
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = null;
            }

            return result;
        }

        /// <summary>
        /// Busca comites por codigo o descripcion.
        /// </summary>
        public ErrorDto<List<AfCdComiteListaDto>> AfCdComites_BuscarComites(int codEmpresa, string? filtro)
        {
            const string sql = @"
                SELECT TOP 100 cod_comite AS Codigo, descripcion AS Descripcion
                FROM afi_cd_comites
                WHERE cod_comite LIKE @Filtro OR descripcion LIKE @Filtro
                ORDER BY cod_comite";
            return DbHelper.ExecuteListQuery<AfCdComiteListaDto>(_portalDb, codEmpresa, sql, new { Filtro = $"%{NormalizarFiltro(filtro)}%" });
        }

        /// <summary>
        /// Lista directores disponibles para el formulario.
        /// </summary>
        public ErrorDto<List<AfCdDirectorDto>> AfCdComites_DirectoresLista(int codEmpresa)
        {
            const string sql = @"SELECT cod_director AS Cod_Director, nombre AS Nombre, puesto AS Puesto, activo AS Activo FROM afi_cd_directores ORDER BY nombre";
            return DbHelper.ExecuteListQuery<AfCdDirectorDto>(_portalDb, codEmpresa, sql);
        }

        /// <summary>
        /// Busca unidades programaticas por codigo o descripcion.
        /// </summary>
        public ErrorDto<List<AfCdComiteListaDto>> AfCdComites_BuscarUnidades(int codEmpresa, string? filtro)
        {
            const string sql = @"
                SELECT TOP 100 codigo AS Codigo, descripcion AS Descripcion
                FROM uprogramatica
                WHERE codigo LIKE @Filtro OR descripcion LIKE @Filtro
                ORDER BY codigo";
            return DbHelper.ExecuteListQuery<AfCdComiteListaDto>(_portalDb, codEmpresa, sql, new { Filtro = $"%{NormalizarFiltro(filtro)}%" });
        }

        /// <summary>
        /// Busca actividades por descripcion o codigo.
        /// </summary>
        public ErrorDto<List<AfCdComiteListaDto>> AfCdComites_BuscarActividades(int codEmpresa, string? filtro)
        {
            const string sql = @"
                SELECT TOP 100 CAST(cod_actividad AS varchar(20)) AS Codigo, descripcion AS Descripcion
                FROM afi_cd_actividades
                WHERE CAST(cod_actividad AS varchar(20)) LIKE @Filtro OR descripcion LIKE @Filtro
                ORDER BY cod_actividad";
            return DbHelper.ExecuteListQuery<AfCdComiteListaDto>(_portalDb, codEmpresa, sql, new { Filtro = $"%{NormalizarFiltro(filtro)}%" });
        }

        /// <summary>
        /// Busca promotores ejecutivos activos por nombre o codigo.
        /// </summary>
        public ErrorDto<List<AfCdComiteListaDto>> AfCdComites_BuscarEjecutivos(int codEmpresa, string? filtro)
        {
            const string sql = @"
                SELECT TOP 100 CAST(id_promotor AS varchar(20)) AS Codigo, nombre AS Descripcion
                FROM promotores
                WHERE tipo = 'P' AND (CAST(id_promotor AS varchar(20)) LIKE @Filtro OR nombre LIKE @Filtro)
                ORDER BY nombre";
            return DbHelper.ExecuteListQuery<AfCdComiteListaDto>(_portalDb, codEmpresa, sql, new { Filtro = $"%{NormalizarFiltro(filtro)}%" });
        }

        /// <summary>
        /// Busca asociados disponibles para miembros del comite.
        /// </summary>
        public ErrorDto<List<AfCdComiteMiembroDto>> AfCdComites_BuscarMiembros(int codEmpresa, string? filtro)
        {
            const string sql = @"
                SELECT TOP 100 cedula AS Cedula, nombre AS Nombre, af_email AS Af_Email
                FROM socios
                WHERE estadoactual = 'S'
                  AND (cedula LIKE @Filtro OR nombre LIKE @Filtro)
                ORDER BY nombre";
            return DbHelper.ExecuteListQuery<AfCdComiteMiembroDto>(_portalDb, codEmpresa, sql, new { Filtro = $"%{NormalizarFiltro(filtro)}%" });
        }

        /// <summary>
        /// Busca comite por unidad programatica asociada.
        /// </summary>
        public ErrorDto<string?> AfCdComites_BuscarPorUnidad(int codEmpresa, string codigoUp)
        {
            const string sql = "SELECT cod_comite FROM afi_cd_comites_unidades WHERE codigo_up = @CodigoUp";
            return DbHelper.ExecuteSingleQuery<string?>(_portalDb, codEmpresa, sql, null, new { CodigoUp = codigoUp });
        }

        /// <summary>
        /// Obtiene el comite anterior o siguiente al comite actual.
        /// </summary>
        public ErrorDto<AfCdComiteDetalleDto?> AfCdComites_Scroll(int codEmpresa, string? codComite, int direccion)
        {
            var result = DbHelper.CreateOkResponse<AfCdComiteDetalleDto?>(null);
            var codComiteSeguro = (codComite ?? string.Empty).Trim();
            try
            {
                using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);
                string sql;
                object? parameters = null;

                if (string.IsNullOrWhiteSpace(codComiteSeguro))
                {
                    sql = direccion > 0
                        ? @"SELECT TOP 1 cod_comite FROM afi_cd_comites ORDER BY cod_comite ASC"
                        : @"SELECT TOP 1 cod_comite FROM afi_cd_comites ORDER BY cod_comite DESC";
                }
                else
                {
                    sql = direccion > 0
                        ? @"SELECT TOP 1 cod_comite FROM afi_cd_comites WHERE cod_comite > @CodComite ORDER BY cod_comite ASC"
                        : @"SELECT TOP 1 cod_comite FROM afi_cd_comites WHERE cod_comite < @CodComite ORDER BY cod_comite DESC";
                    parameters = new { CodComite = codComiteSeguro };
                }

                var codComiteSiguiente = conn.QueryFirstOrDefault<string?>(sql, parameters);
                if (string.IsNullOrWhiteSpace(codComiteSiguiente))
                {
                    result.Code = -2;
                    result.Description = "No se encontraron mas comites.";
                    return result;
                }

                result.Result = new AfCdComiteDetalleDto
                {
                    Comite = ObtenerResumen(conn, codComiteSiguiente),
                    Unidades = ObtenerUnidades(conn, codComiteSiguiente),
                    Actividades = ObtenerActividades(conn, codComiteSiguiente),
                    Ejecutivos = ObtenerEjecutivos(conn, codComiteSiguiente),
                    Miembros = ObtenerMiembros(conn, codComiteSiguiente, true),
                    Liquidaciones = ObtenerLiquidaciones(conn, codComiteSiguiente),
                    LiquidacionesHistorico = ObtenerLiquidacionesHistorico(conn, codComiteSiguiente),
                    Mensajes = ObtenerMensajes(conn, codComiteSiguiente)
                };
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
            }

            return result;
        }

        /// <summary>
        /// Inserta o actualiza la cabecera del comite.
        /// </summary>
        public ErrorDto<bool> AfCdComites_Guardar(int codEmpresa, AfCdComiteGuardarRequest request)
        {
            return DbHelper.WithConn(_portalDb, codEmpresa, conn =>
            {
                var existe = conn.QueryFirstOrDefault<int>("SELECT COUNT(1) FROM afi_cd_comites WHERE cod_comite = @Cod_Comite", request) > 0;
                if (existe)
                {
                    const string updateSql = @"
                        UPDATE afi_cd_comites
                        SET cod_director = @Cod_Director,
                            descripcion = @Descripcion,
                            activo = @Activo,
                            modifica_usuario = @Usuario,
                            modifica_fecha = CONVERT(date, GETDATE())
                        WHERE cod_comite = @Cod_Comite";
                    conn.Execute(updateSql, request);
                    RegistrarBitacora(codEmpresa, request.Usuario, "Modifica - WEB", $"Comite:{request.Cod_Comite}");
                }
                else
                {
                    const string insertSql = @"
                        INSERT INTO afi_cd_comites(cod_comite, cod_director, descripcion, activo, registro_usuario, registro_fecha)
                        VALUES(@Cod_Comite, @Cod_Director, @Descripcion, @Activo, @Usuario, CONVERT(date, GETDATE()))";
                    conn.Execute(insertSql, request);
                    RegistrarBitacora(codEmpresa, request.Usuario, "Ingresa - WEB", $"Comite:{request.Cod_Comite}");
                }

                return true;
            });
        }

        /// <summary>
        /// Asocia una unidad, actividad o ejecutivo al comite.
        /// </summary>
        public ErrorDto<bool> AfCdComites_Asociar(int codEmpresa, string tipo, AfCdComiteAsociacionRequest request)
        {
            return DbHelper.WithConn(_portalDb, codEmpresa, conn =>
            {
                if (tipo.Equals(TipoUnidad, StringComparison.OrdinalIgnoreCase))
                {
                    ValidarUnidadLibre(conn, request.Codigo, request.Cod_Comite);
                    InsertarSiNoExiste(conn, TipoUnidad, request);
                }
                else if (tipo.Equals(TipoActividad, StringComparison.OrdinalIgnoreCase))
                {
                    InsertarSiNoExiste(conn, TipoActividad, request);
                }
                else if (tipo.Equals(TipoEjecutivo, StringComparison.OrdinalIgnoreCase))
                {
                    InsertarSiNoExiste(conn, TipoEjecutivo, request);
                }
                else
                {
                    throw new ArgumentException(TipoAsociacionNoValido);
                }

                RegistrarBitacora(codEmpresa, request.Usuario, "Ingresa - WEB", $"{tipo}:{request.Codigo} Comite:{request.Cod_Comite}");
                return true;
            });
        }

        /// <summary>
        /// Elimina una asociacion de unidad, actividad o ejecutivo.
        /// </summary>
        public ErrorDto<bool> AfCdComites_EliminarAsociacion(int codEmpresa, string tipo, AfCdComiteAsociacionRequest request)
        {
            return DbHelper.WithConn(_portalDb, codEmpresa, conn =>
            {
                var sql = ResolverEliminarAsociacionSql(tipo);
                conn.Execute(sql, request);
                RegistrarBitacora(codEmpresa, request.Usuario, "Borra - WEB", $"{tipo}:{request.Codigo} Comite:{request.Cod_Comite}");
                return true;
            });
        }

        /// <summary>
        /// Obtiene miembros del comite filtrando activos o inactivos.
        /// </summary>
        public ErrorDto<List<AfCdComiteMiembroDto>> AfCdComites_Miembros(int codEmpresa, string codComite, bool activos)
        {
            return DbHelper.WithConn(_portalDb, codEmpresa, conn => ObtenerMiembros(conn, codComite, activos));
        }

        /// <summary>
        /// Obtiene datos de un asociado y su nombramiento actual si existe.
        /// </summary>
        public ErrorDto<AfCdComiteMiembroDto?> AfCdComites_DatosMiembro(int codEmpresa, string cedula, string? codComite)
        {
            return DbHelper.WithConn(_portalDb, codEmpresa, conn => ObtenerDatosMiembro(conn, cedula, codComite));
        }

        /// <summary>
        /// Guarda o actualiza el nombramiento de un miembro y registra historico.
        /// </summary>
        public ErrorDto<bool> AfCdComites_GuardarMiembro(int codEmpresa, AfCdComiteMiembroGuardarRequest request)
        {
            return DbHelper.WithConn(_portalDb, codEmpresa, conn =>
            {
                var activoEnOtro = conn.QueryFirstOrDefault<string>(
                    @"SELECT TOP 1 cod_comite FROM afi_cd_nombramientos WHERE cedula = @Cedula AND activo = 1 AND cod_comite <> @Cod_Comite",
                    request);
                if (!string.IsNullOrWhiteSpace(activoEnOtro))
                {
                    throw new InvalidOperationException($"Este asociado es miembro activo del comite {activoEnOtro}.");
                }

                var existe = conn.QueryFirstOrDefault<int>(
                    "SELECT COUNT(1) FROM afi_cd_nombramientos WHERE cedula = @Cedula AND cod_comite = @Cod_Comite",
                    request) > 0;
                if (existe)
                {
                    const string updateSql = @"
                        UPDATE afi_cd_nombramientos
                        SET cod_puesto = @Cod_Puesto, apl_desembolsos = @Apl_Desembolsos, notas = @Notas,
                            activo = @Activo, registro_usuario = @Usuario, nombre_jefe = @Nombre_Jefe,
                            telefono_jefe = @Telefono_Jefe, celular_jefe = @Celular_Jefe, correo_jefe = @Correo_Jefe,
                            rango_jefe = @Rango_Jefe, fecha_eleccion = @Fecha_Eleccion
                        WHERE cod_comite = @Cod_Comite AND cedula = @Cedula";
                    conn.Execute(updateSql, request);
                }
                else
                {
                    const string insertSql = @"
                        INSERT INTO afi_cd_nombramientos
                        (cedula, cod_puesto, apl_desembolsos, notas, cod_comite, activo, registro_fecha, registro_usuario,
                         nombre_jefe, telefono_jefe, celular_jefe, correo_jefe, rango_jefe, fecha_eleccion)
                        VALUES
                        (@Cedula, @Cod_Puesto, @Apl_Desembolsos, @Notas, @Cod_Comite, @Activo, CONVERT(date, GETDATE()), @Usuario,
                         @Nombre_Jefe, @Telefono_Jefe, @Celular_Jefe, @Correo_Jefe, @Rango_Jefe, @Fecha_Eleccion)";
                    conn.Execute(insertSql, request);
                }

                var linea = conn.QueryFirstOrDefault<int>("SELECT COALESCE(MAX(linea), 0) + 1 FROM afi_cd_nombramientos_h");
                const string histSql = @"
                    INSERT INTO afi_cd_nombramientos_h
                    (cod_comite, cedula, cod_puesto, linea, apl_desembolsos, registro_fecha, registro_usuario, activo, fecha_eleccion)
                    VALUES
                    (@Cod_Comite, @Cedula, @Cod_Puesto, @Linea, @Apl_Desembolsos, CONVERT(date, GETDATE()), @Usuario, @Activo, @Fecha_Eleccion)";
                conn.Execute(histSql, new
                {
                    request.Cod_Comite,
                    request.Cedula,
                    request.Cod_Puesto,
                    Linea = linea,
                    request.Apl_Desembolsos,
                    request.Usuario,
                    request.Activo,
                    request.Fecha_Eleccion
                });
                RegistrarBitacora(codEmpresa, request.Usuario, "Ingresa Historia - WEB", $"Comite:{request.Cod_Comite} Nombramiento:{request.Cedula}");
                return true;
            });
        }

        /// <summary>
        /// Elimina el nombramiento de un miembro del comite.
        /// </summary>
        public ErrorDto<bool> AfCdComites_EliminarMiembro(int codEmpresa, string codComite, string cedula, string usuario)
        {
            return DbHelper.WithConn(_portalDb, codEmpresa, conn =>
            {
                conn.Execute("DELETE FROM afi_cd_nombramientos WHERE cod_comite = @CodComite AND cedula = @Cedula", new { CodComite = codComite, Cedula = cedula });
                RegistrarBitacora(codEmpresa, usuario, "Borra - WEB", $"Miembro:{cedula} Comite:{codComite}");
                return true;
            });
        }

        /// <summary>
        /// Obtiene historial de nombramientos de un comite.
        /// </summary>
        public ErrorDto<List<AfCdComiteMiembroHistorialDto>> AfCdComites_HistorialMiembros(int codEmpresa, string codComite)
        {
            const string sql = @"
                SELECT H.cedula AS Cedula, S.nombre AS Nombre, H.cod_puesto AS Cod_Puesto,
                       H.registro_fecha AS Registro_Fecha, H.registro_usuario AS Registro_Usuario,
                       H.apl_desembolsos AS Apl_Desembolsos, H.activo AS Activo, H.cod_comite AS Cod_Comite
                FROM afi_cd_nombramientos_h H
                INNER JOIN socios S ON H.cedula = S.cedula
                WHERE H.cod_comite = @CodComite
                ORDER BY H.cod_puesto";
            var result = DbHelper.ExecuteListQuery<AfCdComiteMiembroHistorialDto>(_portalDb, codEmpresa, sql, new { CodComite = codComite });
            result.Result = result.Result?.Select(x => { x.Puesto = PuestoNombre(x.Cod_Puesto); return x; }).ToList();
            return result;
        }

        /// <summary>
        /// Obtiene mensajes vigentes del plan de trabajo.
        /// </summary>
        public ErrorDto<List<AfCdComiteMensajeDto>> AfCdComites_Mensajes(int codEmpresa, string codComite)
        {
            return DbHelper.WithConn(_portalDb, codEmpresa, conn => ObtenerMensajes(conn, codComite));
        }

        private static AfCdComiteResumenDto ObtenerResumen(SqlConnection conn, string codComite)
        {
            const string sql = @"
                SELECT C.cod_comite AS Cod_Comite, C.descripcion AS Descripcion, C.cod_director AS Cod_Director,
                       D.nombre AS Director, C.activo AS Activo, U.descripcion AS Unidad_Relacionada, CAST(1 AS bit) AS Existe
                FROM afi_cd_comites C
                LEFT JOIN afi_cd_directores D ON C.cod_director = D.cod_director
                LEFT JOIN uprogramatica U ON C.cod_comite = U.codigo
                WHERE C.cod_comite = @CodComite";
            var resumen = conn.QueryFirstOrDefault<AfCdComiteResumenDto>(sql, new { CodComite = codComite });
            if (resumen != null)
            {
                return resumen;
            }

            const string unidadSql = "SELECT codigo AS Cod_Comite, descripcion AS Descripcion, CAST(0 AS bit) AS Existe FROM uprogramatica WHERE codigo = @CodComite";
            return conn.QueryFirstOrDefault<AfCdComiteResumenDto>(unidadSql, new { CodComite = codComite })
                ?? new AfCdComiteResumenDto { Cod_Comite = codComite, Existe = false };
        }

        private static List<AfCdComiteListaDto> ObtenerUnidades(SqlConnection conn, string codComite)
        {
            const string sql = @"
                SELECT U.codigo_up AS Codigo, P.descripcion AS Descripcion
                FROM afi_cd_comites_unidades U
                LEFT JOIN uprogramatica P ON P.codigo = U.codigo_up
                WHERE U.cod_comite = @CodComite AND U.codigo_up IS NOT NULL
                ORDER BY U.codigo_up";
            return conn.Query<AfCdComiteListaDto>(sql, new { CodComite = codComite }).ToList();
        }

        private static List<AfCdComiteListaDto> ObtenerActividades(SqlConnection conn, string codComite)
        {
            const string sql = @"
                SELECT CAST(U.cod_actividad AS varchar(20)) AS Codigo, A.descripcion AS Descripcion
                FROM afi_cd_comites_actividades U
                LEFT JOIN afi_cd_actividades A ON A.cod_actividad = U.cod_actividad
                WHERE U.cod_comite = @CodComite AND U.cod_actividad IS NOT NULL
                ORDER BY U.cod_actividad";
            return conn.Query<AfCdComiteListaDto>(sql, new { CodComite = codComite }).ToList();
        }

        private static List<AfCdComiteListaDto> ObtenerEjecutivos(SqlConnection conn, string codComite)
        {
            const string sql = @"
                SELECT CAST(E.id_promotor AS varchar(20)) AS Codigo, P.nombre AS Descripcion
                FROM afi_cd_comites_ejecutivo E
                LEFT JOIN promotores P ON E.id_promotor = P.id_promotor
                WHERE E.cod_comite = @CodComite AND E.id_promotor IS NOT NULL
                ORDER BY P.nombre";
            return conn.Query<AfCdComiteListaDto>(sql, new { CodComite = codComite }).ToList();
        }

        private static List<AfCdComiteMiembroDto> ObtenerMiembros(SqlConnection conn, string codComite, bool activos)
        {
            const string sql = @"
                SELECT N.cedula AS Cedula, S.nombre AS Nombre, N.cod_puesto AS Cod_Puesto, N.notas AS Notas,
                       N.apl_desembolsos AS Apl_Desembolsos, N.activo AS Activo
                FROM afi_cd_nombramientos N
                LEFT JOIN socios S ON N.cedula = S.cedula
                WHERE N.cod_comite = @CodComite AND N.activo = @Activos
                ORDER BY N.cod_puesto, S.nombre";
            return conn.Query<AfCdComiteMiembroDto>(sql, new { CodComite = codComite, Activos = activos }).Select(x =>
            {
                x.Puesto = PuestoNombre(x.Cod_Puesto);
                return x;
            }).ToList();
        }

        private static List<AfCdComiteLiquidacionDto> ObtenerLiquidaciones(SqlConnection conn, string codComite)
        {
            const string sql = @"
                SELECT A.noperacion AS Noperacion, C.notas AS Notas, SUM(A.monto) AS Monto,
                       C.tesoreria_nsolicitud AS Tesoreria_Nsolicitud, C.liquida_fecha AS Liquida_Fecha
                FROM afi_cd_cuentas C
                INNER JOIN afi_cd_cuentas_actividades A ON C.noperacion = A.noperacion
                WHERE C.cod_comite = @CodComite AND C.estado = 'T'
                GROUP BY C.notas, A.noperacion, C.estado, C.tesoreria_nsolicitud, C.liquida_fecha
                ORDER BY A.noperacion";
            return conn.Query<AfCdComiteLiquidacionDto>(sql, new { CodComite = codComite }).ToList();
        }

        private static List<AfCdComiteLiquidacionDto> ObtenerLiquidacionesHistorico(SqlConnection conn, string codComite)
        {
            const string sql = @"
                SELECT A.noperacion AS Noperacion, C.notas AS Notas, SUM(A.monto) AS Monto,
                       C.tesoreria_nsolicitud AS Tesoreria_Nsolicitud, C.liquida_fecha AS Liquida_Fecha
                FROM afi_cd_cuentas C
                INNER JOIN afi_cd_cuentas_actividades A ON C.noperacion = A.noperacion
                WHERE C.cod_comite = @CodComite AND C.estado = 'L'
                GROUP BY C.notas, A.noperacion, C.estado, C.tesoreria_nsolicitud, C.liquida_fecha
                ORDER BY C.liquida_fecha DESC, A.noperacion";
            return conn.Query<AfCdComiteLiquidacionDto>(sql, new { CodComite = codComite }).ToList();
        }

        private static List<AfCdComiteMensajeDto> ObtenerMensajes(SqlConnection conn, string codComite)
        {
            const string sql = @"
                SELECT mensaje AS Mensaje, fecha AS Fecha, vencimiento AS Vencimiento, usuario AS Usuario
                FROM afi_cd_comites_mensajes
                WHERE cod_comite = @CodComite AND vencimiento >= GETDATE()
                ORDER BY fecha DESC";
            return conn.Query<AfCdComiteMensajeDto>(sql, new { CodComite = codComite }).ToList();
        }

        private static AfCdComiteMiembroDto? ObtenerDatosMiembro(SqlConnection conn, string cedula, string? codComite)
        {
            const string sql = @"
                SELECT TOP 1 S.cedula AS Cedula, S.nombre AS Nombre, UT.ut_descripcion AS Ut_Descripcion,
                       S.af_email AS Af_Email, N.cod_puesto AS Cod_Puesto, N.notas AS Notas,
                       N.activo AS Activo, N.apl_desembolsos AS Apl_Desembolsos, N.nombre_jefe AS Nombre_Jefe,
                       N.telefono_jefe AS Telefono_Jefe, N.celular_jefe AS Celular_Jefe, N.correo_jefe AS Correo_Jefe,
                       N.rango_jefe AS Rango_Jefe, N.fecha_eleccion AS Fecha_Eleccion
                FROM socios S
                LEFT JOIN afi_cd_nombramientos N ON N.cedula = S.cedula AND N.cod_comite = @CodComite
                LEFT JOIN utrabajo UT ON UT.ut_codigo = S.ut
                WHERE S.cedula = @Cedula";
            var miembro = conn.QueryFirstOrDefault<AfCdComiteMiembroDto>(sql, new { Cedula = cedula, CodComite = codComite });
            if (miembro == null)
            {
                return null;
            }

            miembro.Puesto = PuestoNombre(miembro.Cod_Puesto);
            var telefonos = conn.Query<(int Tipo, string Numero)>("SELECT tipo AS Tipo, numero AS Numero FROM telefonos WHERE cedula = @Cedula", new { Cedula = cedula }).ToList();
            miembro.Telefono = telefonos.FirstOrDefault(x => x.Tipo == 1).Numero;
            miembro.Celular = telefonos.FirstOrDefault(x => x.Tipo == 3).Numero;
            return miembro;
        }

        private static void ValidarUnidadLibre(SqlConnection conn, string? codigo, string? codComite)
        {
            var actual = conn.QueryFirstOrDefault<AfCdComiteResumenDto>(
                @"SELECT U.cod_comite AS Cod_Comite, C.descripcion AS Descripcion
                  FROM afi_cd_comites_unidades U
                  INNER JOIN afi_cd_comites C ON U.cod_comite = C.cod_comite
                  WHERE U.codigo_up = @Codigo",
                new { Codigo = codigo });
            if (actual != null && !string.Equals(actual.Cod_Comite, codComite, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException($"La unidad ya pertenece al comite {actual.Cod_Comite} {actual.Descripcion}.");
            }
        }

        private static void InsertarSiNoExiste(SqlConnection conn, string tipo, AfCdComiteAsociacionRequest request)
        {
            var existeSql = ResolverExisteAsociacionSql(tipo);
            var existe = conn.QueryFirstOrDefault<int>(existeSql, request) > 0;
            if (existe)
            {
                return;
            }

            var insertSql = ResolverInsertarAsociacionSql(tipo);
            conn.Execute(insertSql, request);
        }

        private static string ResolverEliminarAsociacionSql(string tipo)
        {
            if (tipo.Equals(TipoUnidad, StringComparison.OrdinalIgnoreCase))
            {
                return "DELETE FROM afi_cd_comites_unidades WHERE cod_comite = @Cod_Comite AND codigo_up = @Codigo";
            }
            if (tipo.Equals(TipoActividad, StringComparison.OrdinalIgnoreCase))
            {
                return "DELETE FROM afi_cd_comites_actividades WHERE cod_comite = @Cod_Comite AND cod_actividad = @Codigo";
            }
            if (tipo.Equals(TipoEjecutivo, StringComparison.OrdinalIgnoreCase))
            {
                return "DELETE FROM afi_cd_comites_ejecutivo WHERE cod_comite = @Cod_Comite AND id_promotor = @Codigo";
            }

            throw new ArgumentException(TipoAsociacionNoValido);
        }

        private static string ResolverExisteAsociacionSql(string tipo)
        {
            if (tipo.Equals(TipoUnidad, StringComparison.OrdinalIgnoreCase))
            {
                return "SELECT COUNT(1) FROM afi_cd_comites_unidades WHERE cod_comite = @Cod_Comite AND codigo_up = @Codigo";
            }
            if (tipo.Equals(TipoActividad, StringComparison.OrdinalIgnoreCase))
            {
                return "SELECT COUNT(1) FROM afi_cd_comites_actividades WHERE cod_comite = @Cod_Comite AND cod_actividad = @Codigo";
            }
            if (tipo.Equals(TipoEjecutivo, StringComparison.OrdinalIgnoreCase))
            {
                return "SELECT COUNT(1) FROM afi_cd_comites_ejecutivo WHERE cod_comite = @Cod_Comite AND id_promotor = @Codigo";
            }

            throw new ArgumentException(TipoAsociacionNoValido);
        }

        private static string ResolverInsertarAsociacionSql(string tipo)
        {
            if (tipo.Equals(TipoUnidad, StringComparison.OrdinalIgnoreCase))
            {
                return "INSERT INTO afi_cd_comites_unidades(cod_comite, codigo_up) VALUES(@Cod_Comite, @Codigo)";
            }
            if (tipo.Equals(TipoActividad, StringComparison.OrdinalIgnoreCase))
            {
                return "INSERT INTO afi_cd_comites_actividades(cod_comite, cod_actividad) VALUES(@Cod_Comite, @Codigo)";
            }
            if (tipo.Equals(TipoEjecutivo, StringComparison.OrdinalIgnoreCase))
            {
                return "INSERT INTO afi_cd_comites_ejecutivo(cod_comite, id_promotor) VALUES(@Cod_Comite, @Codigo)";
            }

            throw new ArgumentException(TipoAsociacionNoValido);
        }

        private void RegistrarBitacora(int codEmpresa, string? usuario, string movimiento, string detalle)
        {
            _securityMainDb.Bitacora(new BitacoraInsertarDto
            {
                EmpresaId = codEmpresa,
                Usuario = (usuario ?? string.Empty).ToUpperInvariant(),
                Movimiento = movimiento,
                DetalleMovimiento = detalle,
                Modulo = VModulo
            });
        }

        private static string PuestoNombre(int? codPuesto)
        {
            return codPuesto switch
            {
                1 => "PRESIDENTE",
                2 => "VICEPRESIDENTE",
                3 => "SECRETARIO",
                4 => "TESORERO",
                5 => "FISCAL",
                6 => "VOCAL",
                7 => "VOCAL2",
                8 => "DELEGADO",
                _ => string.Empty
            };
        }

        private static string NormalizarFiltro(string? filtro)
        {
            return (filtro ?? string.Empty).Trim();
        }
    }
}
