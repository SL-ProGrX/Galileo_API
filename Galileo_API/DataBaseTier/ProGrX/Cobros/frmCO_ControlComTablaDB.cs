using Dapper;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Cobros;
using Galileo.Models.Security;


namespace Galileo.DataBaseTier.ProGrX.Cobros
{
    public class FrmCoControlComTablaDB
    {

        private readonly IConfiguration _config;
        private readonly int vModulo = 4;
        private readonly MSecurityMainDb _Security_MainDB;

        public FrmCoControlComTablaDB(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _Security_MainDB = new MSecurityMainDb(_config);
        }

        /// <summary>
        /// Consulta de listado de comisiones
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDto<CoControlComTablaLista> CO_ControlComTabla_Obtener(int CodEmpresa, FiltrosLazyLoadData filtros)
        {
            var portalDb = new PortalDB(_config);
            var result = CrearResultadoListaComisiones();

            try
            {
                var consulta = CrearParametrosConsultaComisiones(filtros);
                var queryResult = DbHelper.WithConn(portalDb, CodEmpresa, connection =>
                {
                    using var multi = connection.QueryMultiple(CrearSqlListaComisiones(consulta), consulta.Parametros);

                    return new CoControlComTablaLista
                    {
                        total = multi.ReadFirstOrDefault<int>(),
                        lista = multi.Read<CoControlComTablaData>().ToList()
                    };
                });

                if (queryResult.Code != 0)
                {
                    return CrearErrorListaComisiones(queryResult.Description ?? "Error al consultar tabla de comisiones.");
                }

                result.Result = queryResult.Result ?? new CoControlComTablaLista
                {
                    total = 0,
                    lista = new List<CoControlComTablaData>()
                };
            }
            catch (Exception ex)
            {
                result = CrearErrorListaComisiones(ex.Message);
            }

            return result;
        }

        /// <summary>
        /// Inserta o actualiza un registro
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto CO_ControlComTabla_Guardar(int CodEmpresa, string usuario, CoControlComTablaData request)
        {
            if (request is null)
            {
                return DbHelper.ErrorResponse("Los datos de la comisión son requeridos.", -2);
            }

            var existeResult = ExisteComision(CodEmpresa, request.id_linea);
            if (existeResult.Code != 0)
            {
                return DbHelper.ErrorResponse(existeResult.Description ?? "Error al validar la línea de comisión.");
            }

            return ResolverGuardadoComision(CodEmpresa, usuario, request, existeResult.Result);
        }

        /// <summary>
        /// Actualiza un registro
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="datos"></param>
        /// <returns></returns>
        private ErrorDto CO_ControlComTabla_Actualizar(int CodEmpresa, string usuario, CoControlComTablaData datos)
        {
            const string query = @"
                    UPDATE dbo.Cbr_Comisiones_Tabla
                    SET
                        Inicio = @inicio,
                        Corte = @corte,
                        Porcentaje = @porcentaje,
                        Registro_Fecha = dbo.MyGetdate(),
                        Registro_Usuario = @usuario
                    WHERE Id_Linea = @id_linea;";

            var result = DbHelper.ExecuteNonQuery(new PortalDB(_config), CodEmpresa, query, CrearParametrosComision(datos, usuario));

            if (result.Code != 0)
            {
                return result;
            }

            RegistrarBitacora(
                CodEmpresa,
                usuario,
                $"Tabla Comisión..Línea: {datos.id_linea}",
                "Modifica - WEB");

            return result;
        }

        /// <summary>
        /// Inserta un nuevo registro
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="datos"></param>
        /// <returns></returns>
        private ErrorDto CO_ControlComTabla_Insertar(int CodEmpresa, string usuario, CoControlComTablaData datos)
        {
            const string query = @"
                    INSERT INTO dbo.Cbr_Comisiones_Tabla
                    (
                        Inicio,
                        Corte,
                        Porcentaje,
                        Registro_Fecha,
                        Registro_Usuario
                    )
                    VALUES
                    (
                        @inicio,
                        @corte,
                        @porcentaje,
                        GETDATE(),
                        @usuario
                    );

                    SELECT ISNULL(MAX(Id_Linea),0)
                    FROM dbo.Cbr_Comisiones_Tabla;";

            var insertResult = DbHelper.ExecuteSingleQuery(new PortalDB(_config), CodEmpresa, query, 0, CrearParametrosComision(datos, usuario));

            if (insertResult.Code != 0)
            {
                return DbHelper.ErrorResponse(insertResult.Description ?? "Error al insertar la línea de comisión.");
            }

            RegistrarBitacora(
                CodEmpresa,
                usuario,
                $"Tabla Comisión..Línea:  {insertResult.Result}",
                "Registra - WEB");

            return DbHelper.OkResponse("Ok");
        }

        /// <summary>
        /// Elimina un registro existente
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="id_Linea"></param>
        /// <returns></returns>
        public ErrorDto CO_ControlComTabla_Delete(int CodEmpresa, string usuario, int id_linea)
        {
            const string query = @"DELETE FROM dbo.Cbr_Comisiones_Tabla WHERE Id_Linea = @id_linea;";

            var result = DbHelper.ExecuteNonQuery(new PortalDB(_config), CodEmpresa, query, new { id_linea });

            if (result.Code != 0)
            {
                return result;
            }

            RegistrarBitacora(
                CodEmpresa,
                usuario,
                $"Tabla Comisión..Línea:  {id_linea}",
                "Elimina - WEB");

            return result;
        }

        private ErrorDto<int> ExisteComision(int codEmpresa, int idLinea)
        {
            const string query = @"SELECT ISNULL(COUNT(1),0) FROM dbo.Cbr_Comisiones_Tabla WHERE Id_Linea = @idLinea;";
            return DbHelper.ExecuteSingleQuery(new PortalDB(_config), codEmpresa, query, 0, new { idLinea });
        }

        private ErrorDto ResolverGuardadoComision(
            int codEmpresa,
            string usuario,
            CoControlComTablaData request,
            int existe)
        {
            if (request.isNew)
            {
                return ResolverInsercionComision(codEmpresa, usuario, request, existe);
            }

            return ResolverActualizacionComision(codEmpresa, usuario, request, existe);
        }

        private ErrorDto ResolverInsercionComision(
            int codEmpresa,
            string usuario,
            CoControlComTablaData request,
            int existe)
        {
            if (existe > 0)
            {
                return DbHelper.ErrorResponse($"La linea con el código {request.id_linea} ya existe.", -2);
            }

            return CO_ControlComTabla_Insertar(codEmpresa, usuario, request);
        }

        private ErrorDto ResolverActualizacionComision(
            int codEmpresa,
            string usuario,
            CoControlComTablaData request,
            int existe)
        {
            if (existe == 0)
            {
                return DbHelper.ErrorResponse($"La linea con el código {request.id_linea} no existe.", -2);
            }

            return CO_ControlComTabla_Actualizar(codEmpresa, usuario, request);
        }

        private static ErrorDto<CoControlComTablaLista> CrearResultadoListaComisiones()
        {
            return DbHelper.CreateOkResponse(new CoControlComTablaLista
            {
                total = 0,
                lista = new List<CoControlComTablaData>()
            });
        }

        private static ErrorDto<CoControlComTablaLista> CrearErrorListaComisiones(string mensaje)
        {
            return DbHelper.CreateErrorResponse(
                mensaje,
                -1,
                new CoControlComTablaLista
                {
                    total = 0,
                    lista = new List<CoControlComTablaData>()
                });
        }

        private static CoControlComTablaConsultaParams CrearParametrosConsultaComisiones(FiltrosLazyLoadData? filtros)
        {
            filtros ??= new FiltrosLazyLoadData();

            var filtro = (filtros.filtro ?? string.Empty).Trim();
            var pagina = filtros.pagina;
            var paginacion = filtros.paginacion;
            var exportAll = pagina == 0 || paginacion == 0;

            var parametros = new DynamicParameters();
            AgregarFiltroComisiones(parametros, filtro);
            AgregarPaginacion(parametros, pagina, paginacion, exportAll);

            return new CoControlComTablaConsultaParams
            {
                Parametros = parametros,
                TieneFiltro = !string.IsNullOrWhiteSpace(filtro),
                ExportAll = exportAll,
                SortField = ObtenerSortField(filtros.sortField),
                SortOrder = ObtenerSortOrder(filtros.sortOrder)
            };
        }

        private static void AgregarFiltroComisiones(DynamicParameters parametros, string filtro)
        {
            if (!string.IsNullOrWhiteSpace(filtro))
            {
                parametros.Add("@filtro", $"%{filtro}%");
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

        private static string CrearSqlListaComisiones(CoControlComTablaConsultaParams consulta)
        {
            var whereSql = CrearWhereComisiones(consulta.TieneFiltro);
            var paginacionSql = consulta.ExportAll ? string.Empty : "OFFSET @offset ROWS FETCH NEXT @fetch ROWS ONLY";

            return $@"
                    SELECT COUNT(1)
                    FROM dbo.Cbr_Comisiones_Tabla
                    {whereSql};

                    SELECT
                        Id_Linea,
                        Inicio,
                        Corte,
                        Porcentaje,
                        Registro_Fecha,
                        Registro_Usuario
                    FROM dbo.Cbr_Comisiones_Tabla
                    {whereSql}
                    ORDER BY {consulta.SortField} {consulta.SortOrder}
                    {paginacionSql};";
        }

        private static string CrearWhereComisiones(bool tieneFiltro)
        {
            if (!tieneFiltro)
            {
                return string.Empty;
            }

            return @"WHERE (
                        CONVERT(varchar(30), Id_Linea) LIKE @filtro OR
                        CONVERT(varchar(30), Inicio) LIKE @filtro OR
                        CONVERT(varchar(30), Corte) LIKE @filtro
                    )";
        }

        private static string ObtenerSortField(string? sortField)
        {
            return (sortField ?? string.Empty).Trim() switch
            {
                "Id_Linea" => "Id_Linea",
                "id_linea" => "Id_Linea",
                "Inicio" => "Inicio",
                "inicio" => "Inicio",
                "Corte" => "Corte",
                "corte" => "Corte",
                "Porcentaje" => "Porcentaje",
                "porcentaje" => "Porcentaje",
                "Registro_Fecha" => "Registro_Fecha",
                "registro_fecha" => "Registro_Fecha",
                "Registro_Usuario" => "Registro_Usuario",
                "registro_usuario" => "Registro_Usuario",
                _ => "Id_Linea"
            };
        }

        private static string ObtenerSortOrder(int sortOrder)
        {
            return sortOrder == 0 ? "DESC" : "ASC";
        }

        private static object CrearParametrosComision(CoControlComTablaData datos, string usuario)
        {
            return new
            {
                datos.inicio,
                datos.corte,
                datos.porcentaje,
                datos.id_linea,
                usuario
            };
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

    internal sealed class CoControlComTablaConsultaParams
    {
        public DynamicParameters Parametros { get; init; } = new();
        public bool TieneFiltro { get; init; }
        public bool ExportAll { get; init; }
        public string SortField { get; init; } = "Id_Linea";
        public string SortOrder { get; init; } = "ASC";
    }
}
