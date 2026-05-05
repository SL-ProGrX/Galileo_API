using Dapper;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Cobros;
using Galileo.Models.Security;

namespace Galileo.DataBaseTier.ProGrX.Cobros
{
    public class FrmCOCarteraDB
    {
        private readonly IConfiguration _config;
        private readonly int vModulo = 4;
        private readonly MSecurityMainDb _Security_MainDB;

        public FrmCOCarteraDB(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _Security_MainDB = new MSecurityMainDb(_config);
        }

        /// <summary>
        /// Obtiene la lista de carteras de crédito (CBR_CLASIFICACION_CARTERA) con lazyload.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDto<COCarteraListaResult> Co_CarteraLista_Obtener(int CodEmpresa, FiltrosLazyLoadData filtros)
        {
            var portalDb = new PortalDB(_config);
            var result = CrearResultadoListaCartera();

            try
            {
                var consulta = CrearParametrosConsultaCartera(filtros);
                var queryResult = DbHelper.WithConn(portalDb, CodEmpresa, connection =>
                {
                    using var multi = connection.QueryMultiple(CrearSqlListaCartera(consulta), consulta.Parametros);

                    return new COCarteraListaResult
                    {
                        total = multi.ReadFirstOrDefault<int>(),
                        lista = multi.Read<COCarteraClasificacionData>().ToList()
                    };
                });

                if (queryResult.Code != 0)
                {
                    return CrearErrorListaCartera(queryResult.Description ?? "Error al consultar carteras.");
                }

                result.Result = queryResult.Result ?? new COCarteraListaResult
                {
                    total = 0,
                    lista = new List<COCarteraClasificacionData>()
                };
            }
            catch (Exception ex)
            {
                result = CrearErrorListaCartera(ex.Message);
            }

            return result;
        }

        /// <summary>
        /// Inserta o actualiza una cartera de crédito según isNew y existencia del código.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="cartera"></param>
        /// <returns></returns>
        public ErrorDto Co_Cartera_Guardar(int CodEmpresa, string usuario, COCarteraClasificacionData cartera)
        {
            if (cartera is null)
            {
                return DbHelper.ErrorResponse("Datos de cartera no proporcionados.", -2);
            }

            var cod = NormalizarCodigo(cartera.cod_clasificacion);
            if (string.IsNullOrWhiteSpace(cod))
            {
                return DbHelper.ErrorResponse("Código de cartera inválido.", -2);
            }

            var existeResult = ExisteCartera(CodEmpresa, cod);
            if (existeResult.Code != 0)
            {
                return DbHelper.ErrorResponse(existeResult.Description ?? "Error al validar la cartera.");
            }

            return ResolverGuardadoCartera(CodEmpresa, usuario, cartera, cod, existeResult.Result);
        }

        /// <summary>
        /// Elimina una cartera de crédito por su código.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="cod_clasificacion"></param>
        /// <returns></returns>
        public ErrorDto Co_Cartera_Eliminar(int CodEmpresa, string usuario, string cod_clasificacion)
        {
            const string query = @"DELETE FROM dbo.CBR_CLASIFICACION_CARTERA WHERE UPPER(RTRIM(COD_CLASIFICACION)) = @cod;";

            var cod = NormalizarCodigo(cod_clasificacion);
            var result = DbHelper.ExecuteNonQuery(new PortalDB(_config), CodEmpresa, query, new { cod });

            if (result.Code != 0)
            {
                return result;
            }

            RegistrarBitacora(CodEmpresa, usuario, $"Cartera Crédito: {cod_clasificacion}", "Elimina - WEB");
            return result;
        }

        /// <summary>
        /// Obtiene el catálogo base de códigos para asignación (tabla Catalogo).
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<COCarteraCatalogoData>> Co_Catalogo_Obtener(int CodEmpresa)
        {
            const string query = @"
                    SELECT
                        RTRIM(codigo) AS codigo,
                        RTRIM(descripcion) AS descripcion
                    FROM dbo.Catalogo
                    ORDER BY codigo;";

            return DbHelper.ExecuteListQuery<COCarteraCatalogoData>(new PortalDB(_config), CodEmpresa, query);
        }

        /// <summary>
        /// Obtiene las carteras con bandera asignado para un código (Tab 2: seleccionar código y asignar carteras).
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="codigo"></param>
        /// <returns></returns>
        public ErrorDto<List<COCarteraAsignacionCatItemData>> Co_Asignacion_Carteras_PorCodigo_Obtener(int CodEmpresa, string codigo)
        {
            const string query = @"
                    SELECT
                        RTRIM(R.COD_CLASIFICACION) AS cod_clasificacion,
                        RTRIM(R.DESCRIPCION)       AS descripcion,
                        CASE WHEN A.codigo IS NULL THEN CAST(0 AS bit) ELSE CAST(1 AS bit) END AS asignado
                    FROM dbo.CBR_CLASIFICACION_CARTERA R
                    LEFT JOIN dbo.CBR_CLASIFICACION_DETALLE A
                        ON A.COD_CLASIFICACION = R.COD_CLASIFICACION
                       AND UPPER(RTRIM(A.codigo)) = @cod
                    ORDER BY asignado DESC, R.COD_CLASIFICACION;";

            var cod = NormalizarCodigo(codigo);
            return DbHelper.ExecuteListQuery<COCarteraAsignacionCatItemData>(new PortalDB(_config), CodEmpresa, query, new { cod });
        }

        /// <summary>
        /// Obtiene los códigos (Catalogo) con bandera asignado para una cartera (Tab 3).
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="cod_clasificacion"></param>
        /// <returns></returns>
        public ErrorDto<List<COCarteraAsignacionCodigoItemData>> Co_Asignacion_Codigos_PorCartera_Obtener(int CodEmpresa, string cod_clasificacion)
        {
            const string query = @"
                    SELECT
                        RTRIM(R.codigo) AS codigo,
                        RTRIM(R.descripcion) AS descripcion,
                        CASE WHEN A.codigo IS NULL THEN CAST(0 AS bit) ELSE CAST(1 AS bit) END AS asignado
                    FROM dbo.Catalogo R
                    LEFT JOIN dbo.CBR_CLASIFICACION_DETALLE A
                        ON UPPER(RTRIM(A.COD_CLASIFICACION)) = @clasif
                       AND A.codigo = R.codigo
                    ORDER BY asignado DESC, R.codigo;";

            var clasif = NormalizarCodigo(cod_clasificacion);
            return DbHelper.ExecuteListQuery<COCarteraAsignacionCodigoItemData>(new PortalDB(_config), CodEmpresa, query, new { clasif });
        }

        /// <summary>
        /// Guarda la asignación o desasignación individual en CBR_CLASIFICACION_DETALLE.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="dto"></param>
        /// <returns></returns>
        public ErrorDto Co_Asignacion_Guardar(int CodEmpresa, string usuario, COCarteraAsignacionGuardarDto dto)
        {
            if (dto == null)
            {
                return DbHelper.ErrorResponse("Datos incompletos para asignación.", -2);
            }

            var clasif = NormalizarCodigo(dto.cod_clasificacion);
            var codigo = NormalizarCodigo(dto.codigo);

            if (string.IsNullOrWhiteSpace(clasif) || string.IsNullOrWhiteSpace(codigo))
            {
                return DbHelper.ErrorResponse("Datos incompletos para asignación.", -2);
            }

            var asignar = dto.asignar;
            var result = asignar
                ? GuardarAsignacionCodigo(CodEmpresa, clasif, codigo)
                : EliminarAsignacionCodigo(CodEmpresa, clasif, codigo);

            if (result.Code != 0)
            {
                return result;
            }

            RegistrarBitacoraAsignacion(CodEmpresa, usuario, clasif, codigo, asignar);
            return result;
        }

        /// <summary>
        /// Aplica asignación o desasignación masiva de todos los códigos para una cartera (Tab 3 - bulk).
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="dto"></param>
        /// <returns></returns>
        public ErrorDto Co_Asignacion_Bulk_Guardar(int CodEmpresa, string usuario, COCarteraAsignacionBulkDto dto)
        {
            if (dto is null)
            {
                return DbHelper.ErrorResponse("Parámetros inválidos para asignación masiva.", -2);
            }

            var clasif = NormalizarCodigo(dto.cod_clasificacion);

            if (string.IsNullOrWhiteSpace(clasif))
            {
                return DbHelper.ErrorResponse("Cartera inválida para asignación masiva.", -2);
            }

            var result = dto.asignar_todos
                ? GuardarAsignacionMasiva(CodEmpresa, clasif)
                : EliminarAsignacionMasiva(CodEmpresa, clasif);

            if (result.Code != 0)
            {
                return result;
            }

            RegistrarBitacoraBulk(CodEmpresa, usuario, clasif, dto.asignar_todos);
            return result;
        }

        /// <summary>
        /// Inserta una cartera de crédito.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="cartera"></param>
        /// <returns></returns>
        private ErrorDto Co_Cartera_Insertar(int CodEmpresa, string usuario, COCarteraClasificacionData cartera)
        {
            const string query = @"
                    INSERT INTO dbo.CBR_CLASIFICACION_CARTERA
                    (
                        COD_CLASIFICACION,
                        DESCRIPCION,
                        ESTADO
                    )
                    VALUES
                    (
                        @cod,
                        @desc,
                        @estado
                    );";

            var result = DbHelper.ExecuteNonQuery(new PortalDB(_config), CodEmpresa, query, CrearParametrosCartera(cartera));

            if (result.Code != 0)
            {
                return result;
            }

            RegistrarBitacora(
                CodEmpresa,
                usuario,
                $"Cartera Crédito: {cartera.cod_clasificacion} - {cartera.descripcion}",
                "Registra - WEB");

            return result;
        }

        /// <summary>
        /// Actualiza una cartera de crédito.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="cartera"></param>
        /// <returns></returns>
        private ErrorDto Co_Cartera_Actualizar(int CodEmpresa, string usuario, COCarteraClasificacionData cartera)
        {
            const string query = @"
                    UPDATE dbo.CBR_CLASIFICACION_CARTERA
                    SET
                        DESCRIPCION = @desc,
                        ESTADO      = @estado
                    WHERE UPPER(RTRIM(COD_CLASIFICACION)) = @cod;";

            var result = DbHelper.ExecuteNonQuery(new PortalDB(_config), CodEmpresa, query, CrearParametrosCartera(cartera));

            if (result.Code != 0)
            {
                return result;
            }

            RegistrarBitacora(
                CodEmpresa,
                usuario,
                $"Cartera Crédito: {cartera.cod_clasificacion} - {cartera.descripcion}",
                "Modifica - WEB");

            return result;
        }

        /// <summary>
        /// Obtiene el catálogo de Carteras de Crédito para el dropdown.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Co_Carteras_Dropdown_Obtener(int CodEmpresa)
        {
            const string query = @"
                    SELECT
                        RTRIM(COD_CLASIFICACION) AS item,
                        RTRIM(DESCRIPCION)       AS descripcion
                    FROM dbo.CBR_CLASIFICACION_CARTERA
                    WHERE ISNULL(ESTADO,1) = 1
                    ORDER BY COD_CLASIFICACION;";

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(new PortalDB(_config), CodEmpresa, query);
        }

        private ErrorDto<int> ExisteCartera(int codEmpresa, string cod)
        {
            const string query = @"SELECT ISNULL(COUNT(1),0) FROM dbo.CBR_CLASIFICACION_CARTERA WHERE UPPER(RTRIM(COD_CLASIFICACION)) = @cod;";
            return DbHelper.ExecuteSingleQuery(new PortalDB(_config), codEmpresa, query, 0, new { cod });
        }

        private ErrorDto ResolverGuardadoCartera(
            int codEmpresa,
            string usuario,
            COCarteraClasificacionData cartera,
            string cod,
            int existe)
        {
            if (cartera.isNew)
            {
                return ResolverInsercionCartera(codEmpresa, usuario, cartera, cod, existe);
            }

            return ResolverActualizacionCartera(codEmpresa, usuario, cartera, cod, existe);
        }

        private ErrorDto ResolverInsercionCartera(
            int codEmpresa,
            string usuario,
            COCarteraClasificacionData cartera,
            string cod,
            int existe)
        {
            if (existe > 0)
            {
                return DbHelper.ErrorResponse($"La Cartera con el código {cod} ya existe.", -2);
            }

            return Co_Cartera_Insertar(codEmpresa, usuario, cartera);
        }

        private ErrorDto ResolverActualizacionCartera(
            int codEmpresa,
            string usuario,
            COCarteraClasificacionData cartera,
            string cod,
            int existe)
        {
            if (existe == 0)
            {
                return DbHelper.ErrorResponse($"La Cartera con el código {cod} no existe.", -2);
            }

            return Co_Cartera_Actualizar(codEmpresa, usuario, cartera);
        }

        private static ErrorDto<COCarteraListaResult> CrearResultadoListaCartera()
        {
            return DbHelper.CreateOkResponse(new COCarteraListaResult
            {
                total = 0,
                lista = new List<COCarteraClasificacionData>()
            });
        }

        private static ErrorDto<COCarteraListaResult> CrearErrorListaCartera(string mensaje)
        {
            return DbHelper.CreateErrorResponse(
                mensaje,
                -1,
                new COCarteraListaResult
                {
                    total = 0,
                    lista = new List<COCarteraClasificacionData>()
                });
        }

        private static COCarteraConsultaParams CrearParametrosConsultaCartera(FiltrosLazyLoadData? filtros)
        {
            filtros ??= new FiltrosLazyLoadData();

            var filtro = (filtros.filtro ?? string.Empty).Trim();
            var pagina = filtros.pagina;
            var paginacion = filtros.paginacion;
            var exportAll = pagina == 0 || paginacion == 0;

            var parametros = new DynamicParameters();
            AgregarFiltroCartera(parametros, filtro);
            AgregarPaginacion(parametros, pagina, paginacion, exportAll);

            return new COCarteraConsultaParams
            {
                Parametros = parametros,
                TieneFiltro = !string.IsNullOrWhiteSpace(filtro),
                ExportAll = exportAll,
                SortField = ObtenerSortField(filtros.sortField),
                SortOrder = ObtenerSortOrder(filtros.sortOrder)
            };
        }

        private static void AgregarFiltroCartera(DynamicParameters parametros, string filtro)
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

        private static string CrearSqlListaCartera(COCarteraConsultaParams consulta)
        {
            var whereSql = CrearWhereCartera(consulta.TieneFiltro);
            var paginacionSql = consulta.ExportAll ? string.Empty : "OFFSET @offset ROWS FETCH NEXT @fetch ROWS ONLY";

            return $@"
                    SELECT COUNT(1)
                    FROM dbo.CBR_CLASIFICACION_CARTERA
                    {whereSql};

                    SELECT
                        RTRIM(COD_CLASIFICACION) AS cod_clasificacion,
                        RTRIM(DESCRIPCION)       AS descripcion,
                        CASE WHEN ISNULL(ESTADO,1) = 1 THEN CAST(1 AS bit) ELSE CAST(0 AS bit) END AS estado,
                        CAST(0 AS bit) AS isNew
                    FROM dbo.CBR_CLASIFICACION_CARTERA
                    {whereSql}
                    ORDER BY {consulta.SortField} {consulta.SortOrder}
                    {paginacionSql};";
        }

        private static string CrearWhereCartera(bool tieneFiltro)
        {
            if (!tieneFiltro)
            {
                return string.Empty;
            }

            return @"WHERE (
                        UPPER(RTRIM(COD_CLASIFICACION)) LIKE UPPER(@q) OR
                        UPPER(RTRIM(DESCRIPCION)) LIKE UPPER(@q)
                    )";
        }

        private static string ObtenerSortField(string? sortField)
        {
            return (sortField ?? string.Empty).Trim() switch
            {
                "cod_clasificacion" => "COD_CLASIFICACION",
                "descripcion" => "DESCRIPCION",
                "estado" => "ESTADO",
                _ => "COD_CLASIFICACION"
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

        private static object CrearParametrosCartera(COCarteraClasificacionData cartera)
        {
            return new
            {
                cod = NormalizarCodigo(cartera.cod_clasificacion),
                desc = NormalizarCodigo(cartera.descripcion),
                estado = cartera.estado ? 1 : 0
            };
        }

        private ErrorDto GuardarAsignacionCodigo(int codEmpresa, string clasif, string codigo)
        {
            const string query = @"
                        IF NOT EXISTS (
                            SELECT 1 FROM dbo.CBR_CLASIFICACION_DETALLE
                            WHERE UPPER(RTRIM(COD_CLASIFICACION)) = @clasif AND UPPER(RTRIM(codigo)) = @codigo
                        )
                        BEGIN
                            INSERT INTO dbo.CBR_CLASIFICACION_DETALLE (COD_CLASIFICACION, codigo)
                            VALUES (@clasif, @codigo);
                        END;";

            return DbHelper.ExecuteNonQuery(new PortalDB(_config), codEmpresa, query, new { clasif, codigo });
        }

        private ErrorDto EliminarAsignacionCodigo(int codEmpresa, string clasif, string codigo)
        {
            const string query = @"
                    DELETE FROM dbo.CBR_CLASIFICACION_DETALLE
                    WHERE UPPER(RTRIM(COD_CLASIFICACION)) = @clasif
                      AND UPPER(RTRIM(codigo)) = @codigo;";

            return DbHelper.ExecuteNonQuery(new PortalDB(_config), codEmpresa, query, new { clasif, codigo });
        }

        private ErrorDto GuardarAsignacionMasiva(int codEmpresa, string clasif)
        {
            const string query = @"
                        INSERT INTO dbo.CBR_CLASIFICACION_DETALLE (COD_CLASIFICACION, codigo)
                        SELECT @clasif, C.codigo
                        FROM dbo.Catalogo C
                        WHERE NOT EXISTS (
                            SELECT 1 FROM dbo.CBR_CLASIFICACION_DETALLE D
                            WHERE UPPER(RTRIM(D.COD_CLASIFICACION)) = @clasif
                              AND D.codigo = C.codigo
                        );";

            return DbHelper.ExecuteNonQuery(new PortalDB(_config), codEmpresa, query, new { clasif });
        }

        private ErrorDto EliminarAsignacionMasiva(int codEmpresa, string clasif)
        {
            const string query = @"DELETE FROM dbo.CBR_CLASIFICACION_DETALLE WHERE UPPER(RTRIM(COD_CLASIFICACION)) = @clasif;";
            return DbHelper.ExecuteNonQuery(new PortalDB(_config), codEmpresa, query, new { clasif });
        }

        private void RegistrarBitacoraAsignacion(int codEmpresa, string usuario, string clasif, string codigo, bool asignar)
        {
            RegistrarBitacora(
                codEmpresa,
                usuario,
                $"Cartera Asignación: {clasif} - Código {codigo}",
                asignar ? "Registra - WEB" : "Elimina - WEB");
        }

        private void RegistrarBitacoraBulk(int codEmpresa, string usuario, string clasif, bool asignarTodos)
        {
            RegistrarBitacora(
                codEmpresa,
                usuario,
                $"Cartera Bulk: {clasif} - {(asignarTodos ? "Asignar TODOS" : "Desasignar TODOS")}",
                "Modifica - WEB");
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

    internal sealed class COCarteraConsultaParams
    {
        public DynamicParameters Parametros { get; init; } = new();
        public bool TieneFiltro { get; init; }
        public bool ExportAll { get; init; }
        public string SortField { get; init; } = "COD_CLASIFICACION";
        public string SortOrder { get; init; } = "ASC";
    }
}