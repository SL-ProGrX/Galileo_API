using Dapper;
using Galileo.Models;
using Galileo.Models.ERROR; 
using Galileo.Models.ProGrX.Cobros;
using Galileo.Models.Security;

namespace Galileo.DataBaseTier.ProGrX.Cobros
{
    public class FrmCoAdvertenciasTiposDB
    {

        private readonly IConfiguration _config;
        private readonly int vModulo = 4;
        private readonly MSecurityMainDb _Security_MainDB;

        public FrmCoAdvertenciasTiposDB(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _Security_MainDB = new MSecurityMainDb(_config);
        }

        /// <summary>
        /// Consulta de listado de tipos de advertencia
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDto<CoAdvertenciasTiposLista> CoAdvertenciasTipos_Obtener(int CodEmpresa, FiltrosLazyLoadData filtros)
        {
            var portalDb = new PortalDB(_config);
            var result = CrearResultadoListaAdvertencias();

            try
            {
                var consulta = CrearParametrosConsultaAdvertencias(filtros);
                var queryResult = DbHelper.WithConn(portalDb, CodEmpresa, connection =>
                {
                    using var multi = connection.QueryMultiple(CrearSqlListaAdvertencias(consulta), consulta.Parametros);

                    return new CoAdvertenciasTiposLista
                    {
                        total = multi.ReadFirstOrDefault<int>(),
                        lista = multi.Read<CoAdvertenciasTiposData>().ToList()
                    };
                });

                if (queryResult.Code != 0)
                {
                    return CrearErrorListaAdvertencias(queryResult.Description ?? "Error al consultar tipos de advertencia.");
                }

                result.Result = queryResult.Result ?? new CoAdvertenciasTiposLista
                {
                    total = 0,
                    lista = new List<CoAdvertenciasTiposData>()
                };
            }
            catch (Exception ex)
            {
                result = CrearErrorListaAdvertencias(ex.Message);
            }

            return result;
        }


        /// <summary>
        /// Inserta o actualiza un tipo de advertencia
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto CoAdvertenciasTipos_Guardar(int CodEmpresa, string usuario, CoAdvertenciasTiposData request)
        {
            if (request is null)
            {
                return DbHelper.ErrorResponse("Los datos de la advertencia son requeridos.", -2);
            }

            var cod = NormalizarCodigo(request.cod_advertencia);
            if (string.IsNullOrWhiteSpace(cod))
            {
                return DbHelper.ErrorResponse("El código de advertencia es requerido.", -2);
            }

            var existeResult = ExisteAdvertencia(CodEmpresa, cod);
            if (existeResult.Code != 0)
            {
                return DbHelper.ErrorResponse(existeResult.Description ?? "Error al validar la advertencia.");
            }

            return ResolverGuardadoAdvertencia(CodEmpresa, usuario, request, cod, existeResult.Result);
        }


        /// <summary>
        /// Actualiza un tipo de advertencia
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="datos"></param>
        /// <returns></returns>
        private ErrorDto CoAdvertenciasTipos_Actualizar(int CodEmpresa, string usuario, CoAdvertenciasTiposData datos)
        {
            const string query = @"
                    UPDATE dbo.CBR_ADVERTENCIAS_TIPO
                    SET
                        descripcion = @descripcion,
                        AFECTA_CLASIFICACION = @afectaClasificacion,
                        Activa = @activa
                    WHERE UPPER(RTRIM(COD_ADVERTENCIA)) = @codAdvertencia;";

            var result = DbHelper.ExecuteNonQuery(new PortalDB(_config), CodEmpresa, query, CrearParametrosAdvertencia(datos));

            if (result.Code != 0)
            {
                return result;
            }

            RegistrarBitacora(CodEmpresa, usuario, $"Tipo de Advertencia: {NormalizarCodigo(datos.cod_advertencia)}", "Modifica - WEB");
            return result;
        }


        /// <summary>
        /// Inserta  un tipo de advertencia
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="datos"></param>
        /// <returns></returns>
        private ErrorDto CoAdvertenciasTipos_Insertar(int CodEmpresa, string usuario, CoAdvertenciasTiposData datos)
        {
            const string query = @"
                    INSERT INTO dbo.CBR_ADVERTENCIAS_TIPO
                    (
                        COD_ADVERTENCIA,
                        descripcion,
                        AFECTA_CLASIFICACION,
                        Activa,
                        Registro_Usuario,
                        Registro_Fecha
                    )
                    VALUES
                    (
                        @codAdvertencia,
                        @descripcion,
                        @afectaClasificacion,
                        @activa,
                        @usuario,
                        dbo.MyGetdate()
                    );";

            var parametros = CrearParametrosAdvertencia(datos, usuario);
            var result = DbHelper.ExecuteNonQuery(new PortalDB(_config), CodEmpresa, query, parametros);

            if (result.Code != 0)
            {
                return result;
            }

            RegistrarBitacora(CodEmpresa, usuario, $"Tipo de Advertencia: {NormalizarCodigo(datos.cod_advertencia)}", "Registra - WEB");
            return result;
        }


        /// <summary>
        /// Elimina un tipo de advertencia
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="cod_advertencia"></param>
        /// <returns></returns>
        public ErrorDto CoAdvertenciasTipos_Delete(int CodEmpresa, string usuario, string cod_advertencia)
        {
            var cod = NormalizarCodigo(cod_advertencia);
            if (string.IsNullOrWhiteSpace(cod))
            {
                return DbHelper.ErrorResponse("El código de advertencia es requerido.", -2);
            }

            const string query = @"DELETE FROM dbo.CBR_ADVERTENCIAS_TIPO WHERE UPPER(RTRIM(COD_ADVERTENCIA)) = @cod;";
            var result = DbHelper.ExecuteNonQuery(new PortalDB(_config), CodEmpresa, query, new { cod });

            if (result.Code != 0)
            {
                return result;
            }

            RegistrarBitacora(CodEmpresa, usuario, $"Tipo de Advertencia: {cod}", "Elimina - WEB");
            return result;
        }

        private ErrorDto<int> ExisteAdvertencia(int codEmpresa, string cod)
        {
            const string query = @"SELECT ISNULL(COUNT(1),0) FROM dbo.CBR_ADVERTENCIAS_TIPO WHERE UPPER(RTRIM(COD_ADVERTENCIA)) = @cod;";
            return DbHelper.ExecuteSingleQuery(new PortalDB(_config), codEmpresa, query, 0, new { cod });
        }

        private ErrorDto ResolverGuardadoAdvertencia(
            int codEmpresa,
            string usuario,
            CoAdvertenciasTiposData request,
            string cod,
            int existe)
        {
            if (request.isNew)
            {
                return ResolverInsercionAdvertencia(codEmpresa, usuario, request, cod, existe);
            }

            return ResolverActualizacionAdvertencia(codEmpresa, usuario, request, cod, existe);
        }

        private ErrorDto ResolverInsercionAdvertencia(
            int codEmpresa,
            string usuario,
            CoAdvertenciasTiposData request,
            string cod,
            int existe)
        {
            if (existe > 0)
            {
                return DbHelper.ErrorResponse($"La advertencia con el código {cod} ya existe.", -2);
            }

            return CoAdvertenciasTipos_Insertar(codEmpresa, usuario, request);
        }

        private ErrorDto ResolverActualizacionAdvertencia(
            int codEmpresa,
            string usuario,
            CoAdvertenciasTiposData request,
            string cod,
            int existe)
        {
            if (existe == 0)
            {
                return DbHelper.ErrorResponse($"La advertencia con el código {cod} no existe.", -2);
            }

            return CoAdvertenciasTipos_Actualizar(codEmpresa, usuario, request);
        }

        private static ErrorDto<CoAdvertenciasTiposLista> CrearResultadoListaAdvertencias()
        {
            return DbHelper.CreateOkResponse(new CoAdvertenciasTiposLista
            {
                total = 0,
                lista = new List<CoAdvertenciasTiposData>()
            });
        }

        private static ErrorDto<CoAdvertenciasTiposLista> CrearErrorListaAdvertencias(string mensaje)
        {
            return DbHelper.CreateErrorResponse(
                mensaje,
                -1,
                new CoAdvertenciasTiposLista
                {
                    total = 0,
                    lista = new List<CoAdvertenciasTiposData>()
                });
        }

        private static CoAdvertenciasTiposConsultaParams CrearParametrosConsultaAdvertencias(FiltrosLazyLoadData? filtros)
        {
            filtros ??= new FiltrosLazyLoadData();

            var filtro = (filtros.filtro ?? string.Empty).Trim();
            var pagina = filtros.pagina;
            var paginacion = filtros.paginacion;
            var exportAll = pagina == 0 || paginacion == 0;

            var parametros = new DynamicParameters();
            AgregarFiltroAdvertencias(parametros, filtro);
            AgregarPaginacion(parametros, pagina, paginacion, exportAll);

            return new CoAdvertenciasTiposConsultaParams
            {
                Parametros = parametros,
                TieneFiltro = !string.IsNullOrWhiteSpace(filtro),
                ExportAll = exportAll,
                SortField = ObtenerSortField(filtros.sortField),
                SortOrder = ObtenerSortOrder(filtros.sortOrder)
            };
        }

        private static void AgregarFiltroAdvertencias(DynamicParameters parametros, string filtro)
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

        private static string CrearSqlListaAdvertencias(CoAdvertenciasTiposConsultaParams consulta)
        {
            var whereSql = CrearWhereAdvertencias(consulta.TieneFiltro);
            var paginacionSql = consulta.ExportAll ? string.Empty : "OFFSET @offset ROWS FETCH NEXT @fetch ROWS ONLY";

            return $@"
                    SELECT COUNT(1)
                    FROM dbo.CBR_ADVERTENCIAS_TIPO
                    {whereSql};

                    SELECT
                        RTRIM(COD_ADVERTENCIA) AS cod_advertencia,
                        RTRIM(descripcion) AS descripcion,
                        CASE WHEN ISNULL(AFECTA_CLASIFICACION,0) = 1 THEN CAST(1 AS bit) ELSE CAST(0 AS bit) END AS afecta_clasificacion,
                        CASE WHEN ISNULL(Activa,1) = 1 THEN CAST(1 AS bit) ELSE CAST(0 AS bit) END AS activo,
                        CAST(0 AS bit) AS isNew
                    FROM dbo.CBR_ADVERTENCIAS_TIPO
                    {whereSql}
                    ORDER BY {consulta.SortField} {consulta.SortOrder}
                    {paginacionSql};";
        }

        private static string CrearWhereAdvertencias(bool tieneFiltro)
        {
            if (!tieneFiltro)
            {
                return string.Empty;
            }

            return @"WHERE (
                        UPPER(RTRIM(COD_ADVERTENCIA)) LIKE UPPER(@q) OR
                        UPPER(RTRIM(descripcion)) LIKE UPPER(@q)
                    )";
        }

        private static string ObtenerSortField(string? sortField)
        {
            return (sortField ?? string.Empty).Trim() switch
            {
                "cod_advertencia" => "COD_ADVERTENCIA",
                "COD_ADVERTENCIA" => "COD_ADVERTENCIA",
                "descripcion" => "descripcion",
                "afecta_clasificacion" => "AFECTA_CLASIFICACION",
                "AFECTA_CLASIFICACION" => "AFECTA_CLASIFICACION",
                "activo" => "Activa",
                "Activa" => "Activa",
                _ => "COD_ADVERTENCIA"
            };
        }

        private static string ObtenerSortOrder(int sortOrder)
        {
            return sortOrder == 0 ? "DESC" : "ASC";
        }

        private static string NormalizarCodigo(string? valor)
        {
            return (valor ?? string.Empty).Trim().ToUpper();
        }

        private static object CrearParametrosAdvertencia(CoAdvertenciasTiposData datos, string? usuario = null)
        {
            var parametros = new DynamicParameters();
            parametros.Add("@codAdvertencia", NormalizarCodigo(datos.cod_advertencia));
            parametros.Add("@descripcion", (datos.descripcion ?? string.Empty).Trim());
            parametros.Add("@afectaClasificacion", datos.afecta_clasificacion ? 1 : 0);
            parametros.Add("@activa", datos.activo ? 1 : 0);

            if (usuario is not null)
            {
                parametros.Add("@usuario", usuario);
            }

            return parametros;
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

    internal sealed class CoAdvertenciasTiposConsultaParams
    {
        public DynamicParameters Parametros { get; init; } = new();
        public bool TieneFiltro { get; init; }
        public bool ExportAll { get; init; }
        public string SortField { get; init; } = "COD_ADVERTENCIA";
        public string SortOrder { get; init; } = "ASC";
    }
}