using Dapper;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Cobros;
using Galileo.Models.Security;

namespace Galileo.DataBaseTier.ProGrX.Cobros
{
    public class FrmCOControlGestionesDB
    {
        private readonly IConfiguration _config;
        private readonly int vModulo = 4;
        private readonly MSecurityMainDb _Security_MainDB;

        public FrmCOControlGestionesDB(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _Security_MainDB = new MSecurityMainDb(_config);
        }

        /// <summary>
        /// Obtiene la lista de tipos de gestiones de cobro.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDto<CoControlGestionesLista> Co_GestionesLista_Obtener(int CodEmpresa, FiltrosLazyLoadData filtros)
        {
            var portalDb = new PortalDB(_config);
            var result = CrearResultadoListaGestiones();

            try
            {
                var consulta = CrearParametrosConsultaGestiones(filtros);
                var queryResult = DbHelper.WithConn(portalDb, CodEmpresa, connection =>
                {
                    using var multi = connection.QueryMultiple(CrearSqlListaGestiones(consulta), consulta.Parametros);

                    return new CoControlGestionesLista
                    {
                        total = multi.ReadFirstOrDefault<int>(),
                        lista = multi.Read<CoControlGestionesData>().ToList()
                    };
                });

                if (queryResult.Code != 0)
                {
                    return CrearErrorListaGestiones(queryResult.Description ?? "Error al consultar gestiones de cobro.");
                }

                result.Result = queryResult.Result ?? new CoControlGestionesLista
                {
                    total = 0,
                    lista = new List<CoControlGestionesData>()
                };
            }
            catch (Exception ex)
            {
                result = CrearErrorListaGestiones(ex.Message);
            }

            return result;
        }

        /// <summary>
        /// Inserta o actualiza un tipo de gestión de cobro según isNew y existencia del código.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="gestion"></param>
        /// <returns></returns>
        public ErrorDto Co_Gestiones_Guardar(int CodEmpresa, string usuario, CoControlGestionesData gestion)
        {
            if (gestion is null)
            {
                return DbHelper.ErrorResponse("La gestión es requerida.", -2);
            }

            var cod = NormalizarCodigo(gestion.cod_gestion);
            var existeResult = ExisteGestion(CodEmpresa, cod);

            if (existeResult.Code != 0)
            {
                return DbHelper.ErrorResponse(existeResult.Description ?? "Error al validar la gestión.");
            }

            return ResolverGuardadoGestion(CodEmpresa, usuario, gestion, cod, existeResult.Result);
        }

        /// <summary>
        /// Elimina un tipo de gestión de cobro por su código.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="cod_gestion"></param>
        /// <returns></returns>
        public ErrorDto Co_Gestiones_Eliminar(int CodEmpresa, string usuario, string cod_gestion)
        {
            const string query = @"DELETE FROM dbo.CBR_GESTIONES WHERE UPPER(COD_GESTION) = @cod;";

            var cod = NormalizarCodigo(cod_gestion);
            var result = DbHelper.ExecuteNonQuery(new PortalDB(_config), CodEmpresa, query, new { cod });

            if (result.Code != 0)
            {
                return result;
            }

            RegistrarBitacora(
                CodEmpresa,
                usuario,
                $"Tipo Gestión Cobros: {cod_gestion}",
                "Elimina - WEB");

            return result;
        }

        /// <summary>
        /// Retorna el catálogo para el dropdown de nivel de gestión (U=Usuario, S=Sistema).
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Co_NivelGestion_Obtener(int CodEmpresa)
        {
            return DbHelper.CreateOkResponse(new List<DropDownListaGenericaModel>
            {
                new() { item = "U", descripcion = "Usuario" },
                new() { item = "S", descripcion = "Sistema" }
            });
        }

        /// <summary>
        /// Obtiene las gestiones restringidas a nivel usuario para la pestaña de seguridad.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<CoControlGestionesSeguridadGestionData>> Co_Seguridad_Gestiones_Obtener(int CodEmpresa)
        {
            const string query = @"
                    SELECT
                        COD_GESTION AS cod_gestion,
                        DESCRIPCION AS descripcion
                    FROM dbo.CBR_GESTIONES
                    WHERE ISNULL(ACCESO_RESTRINGIDO,0) = 1
                      AND ISNULL(NIVEL_GESTION,'U') = 'U'
                    ORDER BY DESCRIPCION;";

            return DbHelper.ExecuteListQuery<CoControlGestionesSeguridadGestionData>(new PortalDB(_config), CodEmpresa, query);
        }

        /// <summary>
        /// Obtiene el listado de usuarios activos y si están asignados a una gestión.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="cod_gestion"></param>
        /// <returns></returns>
        public ErrorDto<List<CoControlGestionesSeguridadUsuarioData>> Co_Seguridad_Usuarios_Obtener(int CodEmpresa, string cod_gestion)
        {
            const string query = @"
                    SELECT
                        U.USUARIO AS usuario,
                        U.NOMBRE  AS nombre,
                        CASE WHEN GU.USUARIO IS NULL THEN CAST(0 AS bit) ELSE CAST(1 AS bit) END AS asignado
                    FROM dbo.CBR_USUARIOS U
                    LEFT JOIN dbo.CBR_GESTIONES_USUARIOS GU
                        ON GU.USUARIO = U.USUARIO
                       AND GU.COD_GESTION = @cod
                    WHERE ISNULL(U.ESTADO,0) = 1
                    ORDER BY U.NOMBRE;";

            var cod = NormalizarCodigo(cod_gestion);
            return DbHelper.ExecuteListQuery<CoControlGestionesSeguridadUsuarioData>(new PortalDB(_config), CodEmpresa, query, new { cod });
        }

        /// <summary>
        /// Guarda la asignación o eliminación de un usuario a una gestión.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="dto"></param>
        /// <returns></returns>
        public ErrorDto Co_Seguridad_Asignacion_Guardar(int CodEmpresa, string usuario, CoControlGestionesSeguridadAsignacionDto dto)
        {
            var cod = NormalizarCodigo(dto?.cod_gestion);
            var usr = NormalizarCodigo(dto?.usuario_asignado);

            if (string.IsNullOrWhiteSpace(cod) || string.IsNullOrWhiteSpace(usr))
            {
                return DbHelper.ErrorResponse("Datos incompletos para asignación de seguridad.", -2);
            }

            var result = dto!.asignar
                ? GuardarAsignacionUsuario(CodEmpresa, cod, usr)
                : EliminarAsignacionUsuario(CodEmpresa, cod, usr);

            if (result.Code != 0)
            {
                return result;
            }

            RegistrarBitacoraAsignacion(CodEmpresa, usuario, cod, usr, dto.asignar);
            return result;
        }

        /// <summary>
        /// Inserta un tipo de gestión de cobro.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="gestion"></param>
        /// <returns></returns>
        private ErrorDto Co_Gestiones_Insertar(int CodEmpresa, string usuario, CoControlGestionesData gestion)
        {
            const string query = @"
                    INSERT INTO dbo.CBR_GESTIONES
                    (
                        COD_GESTION, DESCRIPCION, CODIGO_REFERENCIA, MONTO,
                        MODIFICA_USUARIO, MODIFICA_DESVIACION,
                        COD_CUENTA, NIVEL_GESTION, ACCESO_RESTRINGIDO,
                        MRECUPERACION, IVA_PORCENTAJE, ESTADO,
                        REGISTRO_USUARIO, REGISTRO_FECHA
                    )
                    VALUES
                    (
                        @cod_gestion, @descripcion, @codigo_referencia, @monto,
                        @modifica_usuario, @modifica_desviacion,
                        @cod_cuenta, @nivel_gestion, @acceso_restringido,
                        @mrecuperacion, @iva_porcentaje, @estado,
                        @registro_usuario, GETDATE()
                    );";

            var result = DbHelper.ExecuteNonQuery(new PortalDB(_config), CodEmpresa, query, CrearParametrosGestion(gestion, usuario, false));

            if (result.Code != 0)
            {
                return result;
            }

            RegistrarBitacora(
                CodEmpresa,
                usuario,
                $"Tipo Gestión Cobros: {gestion.cod_gestion} - {gestion.descripcion}",
                "Registra - WEB");

            return result;
        }

        /// <summary>
        /// Actualiza un tipo de gestión de cobro.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="gestion"></param>
        /// <returns></returns>
        private ErrorDto Co_Gestiones_Actualizar(int CodEmpresa, string usuario, CoControlGestionesData gestion)
        {
            const string query = @"
                    UPDATE dbo.CBR_GESTIONES
                    SET
                        DESCRIPCION = @descripcion,
                        CODIGO_REFERENCIA = @codigo_referencia,
                        MONTO = @monto,
                        MODIFICA_USUARIO = @modifica_usuario,
                        MODIFICA_DESVIACION = @modifica_desviacion,
                        COD_CUENTA = @cod_cuenta,
                        NIVEL_GESTION = @nivel_gestion,
                        ACCESO_RESTRINGIDO = @acceso_restringido,
                        MRECUPERACION = @mrecuperacion,
                        IVA_PORCENTAJE = @iva_porcentaje,
                        ESTADO = @estado,
                        ACTUALIZA_USUARIO = @actualiza_usuario,
                        ACTUALIZA_FECHA = GETDATE()
                    WHERE UPPER(COD_GESTION) = @cod_gestion;";

            var result = DbHelper.ExecuteNonQuery(new PortalDB(_config), CodEmpresa, query, CrearParametrosGestion(gestion, usuario, true));

            if (result.Code != 0)
            {
                return result;
            }

            RegistrarBitacora(
                CodEmpresa,
                usuario,
                $"Tipo Gestión Cobros: {gestion.cod_gestion} - {gestion.descripcion}",
                "Modifica - WEB");

            return result;
        }

        private ErrorDto<int> ExisteGestion(int codEmpresa, string cod)
        {
            const string query = @"SELECT ISNULL(COUNT(1),0) FROM dbo.CBR_GESTIONES WHERE UPPER(COD_GESTION) = @cod;";
            return DbHelper.ExecuteSingleQuery(new PortalDB(_config), codEmpresa, query, 0, new { cod });
        }

        private ErrorDto ResolverGuardadoGestion(
            int codEmpresa,
            string usuario,
            CoControlGestionesData gestion,
            string cod,
            int existe)
        {
            if (gestion.isNew)
            {
                return ResolverInsercionGestion(codEmpresa, usuario, gestion, cod, existe);
            }

            return ResolverActualizacionGestion(codEmpresa, usuario, gestion, cod, existe);
        }

        private ErrorDto ResolverInsercionGestion(
            int codEmpresa,
            string usuario,
            CoControlGestionesData gestion,
            string cod,
            int existe)
        {
            if (existe > 0)
            {
                return DbHelper.ErrorResponse($"La Gestión con el código {cod} ya existe.", -2);
            }

            return Co_Gestiones_Insertar(codEmpresa, usuario, gestion);
        }

        private ErrorDto ResolverActualizacionGestion(
            int codEmpresa,
            string usuario,
            CoControlGestionesData gestion,
            string cod,
            int existe)
        {
            if (existe == 0)
            {
                return DbHelper.ErrorResponse($"La Gestión con el código {cod} no existe.", -2);
            }

            return Co_Gestiones_Actualizar(codEmpresa, usuario, gestion);
        }

        private static ErrorDto<CoControlGestionesLista> CrearResultadoListaGestiones()
        {
            return DbHelper.CreateOkResponse(new CoControlGestionesLista
            {
                total = 0,
                lista = new List<CoControlGestionesData>()
            });
        }

        private static ErrorDto<CoControlGestionesLista> CrearErrorListaGestiones(string mensaje)
        {
            return DbHelper.CreateErrorResponse(
                mensaje,
                -1,
                new CoControlGestionesLista
                {
                    total = 0,
                    lista = new List<CoControlGestionesData>()
                });
        }

        private static CoControlGestionesConsultaParams CrearParametrosConsultaGestiones(FiltrosLazyLoadData filtros)
        {
            var filtro = (filtros?.filtro ?? string.Empty).Trim();
            var pagina = filtros?.pagina ?? 0;
            var paginacion = filtros?.paginacion ?? 0;
            var exportAll = pagina == 0 || paginacion == 0;

            var parametros = new DynamicParameters();
            AgregarFiltroGestiones(parametros, filtro);
            AgregarPaginacion(parametros, pagina, paginacion, exportAll);

            return new CoControlGestionesConsultaParams
            {
                Parametros = parametros,
                TieneFiltro = !string.IsNullOrWhiteSpace(filtro),
                ExportAll = exportAll,
                SortField = ObtenerSortField(filtros?.sortField),
                SortOrder = ObtenerSortOrder(filtros?.sortOrder)
            };
        }

        private static void AgregarFiltroGestiones(DynamicParameters parametros, string filtro)
        {
            if (!string.IsNullOrWhiteSpace(filtro))
            {
                parametros.Add("@q", $"%{filtro}%");
            }
        }

        private static void AgregarPaginacion(DynamicParameters parametros, int pagina, int paginacion, bool exportAll)
        {
            if (exportAll)
            {
                return;
            }

            parametros.Add("@offset", pagina);
            parametros.Add("@fetch", paginacion);
        }

        private static string CrearSqlListaGestiones(CoControlGestionesConsultaParams consulta)
        {
            var whereSql = CrearWhereGestiones(consulta.TieneFiltro);
            var paginacionSql = consulta.ExportAll ? string.Empty : "OFFSET @offset ROWS FETCH NEXT @fetch ROWS ONLY";

            return $@"
                    SELECT COUNT(1)
                    FROM dbo.CBR_GESTIONES
                    {whereSql};

                    SELECT
                        COD_GESTION         AS cod_gestion,
                        DESCRIPCION         AS descripcion,
                        CODIGO_REFERENCIA   AS codigo_referencia,
                        ISNULL(MONTO,0)     AS monto,
                        CASE WHEN ISNULL(MODIFICA_USUARIO,0) = 1 THEN CAST(1 AS bit) ELSE CAST(0 AS bit) END AS modifica_usuario,
                        ISNULL(MODIFICA_DESVIACION,0) AS modifica_desviacion,
                        ISNULL(COD_CUENTA,'') AS cod_cuenta,
                        ISNULL(NIVEL_GESTION,'U') AS nivel_gestion,
                        CASE WHEN ISNULL(ACCESO_RESTRINGIDO,0) = 1 THEN CAST(1 AS bit) ELSE CAST(0 AS bit) END AS acceso_restringido,
                        CASE WHEN ISNULL(MRECUPERACION,0) = 1 THEN CAST(1 AS bit) ELSE CAST(0 AS bit) END AS mrecuperacion,
                        ISNULL(IVA_PORCENTAJE,0) AS iva_porcentaje,
                        CASE WHEN ISNULL(ESTADO,1) = 1 THEN CAST(1 AS bit) ELSE CAST(0 AS bit) END AS activo
                    FROM dbo.CBR_GESTIONES
                    {whereSql}
                    ORDER BY {consulta.SortField} {consulta.SortOrder}
                    {paginacionSql};";
        }

        private static string CrearWhereGestiones(bool tieneFiltro)
        {
            if (!tieneFiltro)
            {
                return string.Empty;
            }

            return @"WHERE (
                        UPPER(COD_GESTION) LIKE UPPER(@q) OR
                        UPPER(DESCRIPCION) LIKE UPPER(@q) OR
                        UPPER(CODIGO_REFERENCIA) LIKE UPPER(@q) OR
                        UPPER(ISNULL(COD_CUENTA,'')) LIKE UPPER(@q) OR
                        UPPER(ISNULL(NIVEL_GESTION,'')) LIKE UPPER(@q)
                    )";
        }

        private static string ObtenerSortField(string? sortField)
        {
            return (sortField ?? string.Empty).Trim() switch
            {
                "cod_gestion" => "COD_GESTION",
                "descripcion" => "DESCRIPCION",
                "codigo_referencia" => "CODIGO_REFERENCIA",
                "monto" => "MONTO",
                "modifica_usuario" => "MODIFICA_USUARIO",
                "modifica_desviacion" => "MODIFICA_DESVIACION",
                "cod_cuenta" => "COD_CUENTA",
                "nivel_gestion" => "NIVEL_GESTION",
                "acceso_restringido" => "ACCESO_RESTRINGIDO",
                "mrecuperacion" => "MRECUPERACION",
                "iva_porcentaje" => "IVA_PORCENTAJE",
                "activo" => "ESTADO",
                _ => "COD_GESTION"
            };
        }

        private static string ObtenerSortOrder(int? sortOrder)
        {
            return (sortOrder ?? 1) == 0 ? "DESC" : "ASC";
        }

        private static string NormalizarCodigo(string? valor)
        {
            return (valor ?? string.Empty).Trim().ToUpper();
        }

        private static string NormalizarNivelGestion(string? nivelGestion)
        {
            return NormalizarCodigo(nivelGestion) == "S" ? "S" : "U";
        }

        private static object CrearParametrosGestion(CoControlGestionesData gestion, string usuario, bool esActualizacion)
        {
            var parametros = new DynamicParameters();
            parametros.Add("@cod_gestion", NormalizarCodigo(gestion.cod_gestion));
            parametros.Add("@descripcion", NormalizarCodigo(gestion.descripcion));
            parametros.Add("@codigo_referencia", NormalizarCodigo(gestion.codigo_referencia));
            parametros.Add("@monto", gestion.monto);
            parametros.Add("@modifica_usuario", gestion.modifica_usuario ? 1 : 0);
            parametros.Add("@modifica_desviacion", gestion.modifica_desviacion);
            parametros.Add("@cod_cuenta", (gestion.cod_cuenta ?? string.Empty).Trim());
            parametros.Add("@nivel_gestion", NormalizarNivelGestion(gestion.nivel_gestion));
            parametros.Add("@acceso_restringido", gestion.acceso_restringido ? 1 : 0);
            parametros.Add("@mrecuperacion", gestion.mrecuperacion ? 1 : 0);
            parametros.Add("@iva_porcentaje", gestion.iva_porcentaje);
            parametros.Add("@estado", gestion.activo ? 1 : 0);
            parametros.Add(esActualizacion ? "@actualiza_usuario" : "@registro_usuario", usuario);

            return parametros;
        }

        private ErrorDto GuardarAsignacionUsuario(int codEmpresa, string cod, string usr)
        {
            const string query = @"
                        IF NOT EXISTS (SELECT 1 FROM dbo.CBR_GESTIONES_USUARIOS WHERE COD_GESTION = @cod AND USUARIO = @usr)
                        BEGIN
                            INSERT INTO dbo.CBR_GESTIONES_USUARIOS (COD_GESTION, USUARIO)
                            VALUES (@cod, @usr);
                        END;";

            return DbHelper.ExecuteNonQuery(new PortalDB(_config), codEmpresa, query, new { cod, usr });
        }

        private ErrorDto EliminarAsignacionUsuario(int codEmpresa, string cod, string usr)
        {
            const string query = @"DELETE FROM dbo.CBR_GESTIONES_USUARIOS WHERE COD_GESTION = @cod AND USUARIO = @usr;";
            return DbHelper.ExecuteNonQuery(new PortalDB(_config), codEmpresa, query, new { cod, usr });
        }

        private void RegistrarBitacoraAsignacion(int codEmpresa, string usuario, string cod, string usr, bool asignar)
        {
            var movimiento = asignar ? "Registra - WEB" : "Elimina - WEB";
            var accion = asignar ? "Asigna" : "Elimina";
            RegistrarBitacora(codEmpresa, usuario, $"Gestión {cod} - {accion} usuario {usr}", movimiento);
        }

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
    }

    internal sealed class CoControlGestionesConsultaParams
    {
        public DynamicParameters Parametros { get; init; } = new();
        public bool TieneFiltro { get; init; }
        public bool ExportAll { get; init; }
        public string SortField { get; init; } = "COD_GESTION";
        public string SortOrder { get; init; } = "ASC";
    }
}
