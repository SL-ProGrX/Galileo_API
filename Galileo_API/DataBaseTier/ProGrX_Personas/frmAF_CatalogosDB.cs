using Dapper;
using Microsoft.Data.SqlClient;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Personas;
using Galileo.Models.Security;

namespace Galileo.DataBaseTier.ProGrX_Personas
{
    public class FrmAFCatalogosDB
    {
        private readonly IConfiguration _config;
        private readonly int vModulo = 1;
        private readonly MSecurityMainDb _Security_MainDB;

        private const string CampoTipoId = "Tipo_Id";
        private const string CampoDescripcion = "Descripcion";
        private const string CampoActivo = "Activo";
        private const string CampoRegistroFecha = "Registro_Fecha";
        private const string CampoRegistroUsuario = "Registro_Usuario";

        public FrmAFCatalogosDB(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _Security_MainDB = new MSecurityMainDb(_config);
        }

        /// <summary>
        /// Obtiene la lista de catálogos por tipo, con filtros aplicados a todos los campos principales.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa</param>
        /// <param name="tipoId">Tipo de catálogo</param>
        /// <param name="filtros">Filtros de búsqueda, orden y paginación</param>
        /// <returns></returns>
        public ErrorDto<CatalogoLista> AF_Catalogos_Obtener(int CodEmpresa, int tipoId, FiltrosLazyLoadData filtros)
        {
            if (filtros is null)
            {
                return DbHelper.CreateErrorResponse("Los filtros de catálogos son requeridos.", -2, new CatalogoLista());
            }

            var resultadoVacio = new CatalogoLista
            {
                Total = 0,
                Lista = new List<CatalogoData>()
            };

            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
            {
                var salida = new CatalogoLista
                {
                    Total = connection.QueryFirstOrDefault<int>(
                        "SELECT COUNT(*) FROM AFI_CATALOGOS WHERE Tipo_Id = @Tipo_Id",
                        new { Tipo_Id = tipoId }),
                    Lista = new List<CatalogoData>()
                };

                var filtroTexto = filtros.filtro?.Trim();
                var sortField = ObtenerSortFieldCatalogos(filtros.sortField);
                var sortDirection = ObtenerSortDirectionCatalogos(filtros.sortOrder);
                var offsetRows = filtros.pagina;
                var fetchRows = filtros.paginacion;

                const string query = @"
                    SELECT Linea_Id, Catalogo_Id, Descripcion, Activo, Tipo_Id, Registro_Fecha, Registro_Usuario
                    FROM AFI_CATALOGOS
                    WHERE Tipo_Id = @Tipo_Id
                      AND (
                        @Filtro IS NULL
                        OR CAST(Linea_Id AS VARCHAR(50)) LIKE @Filtro
                        OR Catalogo_Id LIKE @Filtro
                        OR Descripcion LIKE @Filtro
                        OR CAST(Activo AS VARCHAR(10)) LIKE @Filtro
                        OR CAST(Tipo_Id AS VARCHAR(50)) LIKE @Filtro
                        OR CONVERT(VARCHAR(25), Registro_Fecha, 120) LIKE @Filtro
                        OR Registro_Usuario LIKE @Filtro
                      )
                    ORDER BY
                        CASE WHEN @SortField = 'Linea_Id' AND @SortDirection = 'ASC' THEN Linea_Id END ASC,
                        CASE WHEN @SortField = 'Linea_Id' AND @SortDirection = 'DESC' THEN Linea_Id END DESC,
                        CASE WHEN @SortField = 'Catalogo_Id' AND @SortDirection = 'ASC' THEN Catalogo_Id END ASC,
                        CASE WHEN @SortField = 'Catalogo_Id' AND @SortDirection = 'DESC' THEN Catalogo_Id END DESC,
                        CASE WHEN @SortField = '" + CampoDescripcion + @"' AND @SortDirection = 'ASC' THEN Descripcion END ASC,
                        CASE WHEN @SortField = '" + CampoDescripcion + @"' AND @SortDirection = 'DESC' THEN Descripcion END DESC,
                        CASE WHEN @SortField = '" + CampoActivo + @"' AND @SortDirection = 'ASC' THEN CAST(Activo AS INT) END ASC,
                        CASE WHEN @SortField = '" + CampoActivo + @"' AND @SortDirection = 'DESC' THEN CAST(Activo AS INT) END DESC,
                        CASE WHEN @SortField = '" + CampoTipoId + @"' AND @SortDirection = 'ASC' THEN Tipo_Id END ASC,
                        CASE WHEN @SortField = '" + CampoTipoId + @"' AND @SortDirection = 'DESC' THEN Tipo_Id END DESC,
                        CASE WHEN @SortField = '" + CampoRegistroFecha + @"' AND @SortDirection = 'ASC' THEN Registro_Fecha END ASC,
                        CASE WHEN @SortField = '" + CampoRegistroFecha + @"' AND @SortDirection = 'DESC' THEN Registro_Fecha END DESC,
                        CASE WHEN @SortField = '" + CampoRegistroUsuario + @"' AND @SortDirection = 'ASC' THEN Registro_Usuario END ASC,
                        CASE WHEN @SortField = '" + CampoRegistroUsuario + @"' AND @SortDirection = 'DESC' THEN Registro_Usuario END DESC,
                        Catalogo_Id ASC
                    OFFSET @OffsetRows ROWS
                    FETCH NEXT @FetchRows ROWS ONLY";

                var parametros = new DynamicParameters();
                parametros.Add("Tipo_Id", tipoId);
                parametros.Add("Filtro", string.IsNullOrWhiteSpace(filtroTexto) ? null : $"%{filtroTexto}%");
                parametros.Add("SortField", sortField);
                parametros.Add("SortDirection", sortDirection);
                parametros.Add("OffsetRows", offsetRows);
                parametros.Add("FetchRows", fetchRows);

                salida.Lista = connection.Query<CatalogoData>(query, parametros).ToList();
                return salida;
            });

            return result.Code == 0
                ? DbHelper.CreateOkResponse(result.Result ?? resultadoVacio)
                : DbHelper.CreateErrorResponse(result.Description ?? "Error al obtener catálogos.", result.Code.GetValueOrDefault(-1), resultadoVacio);
        }

        /// <summary>
        /// Valida si existe un catálogo por su id y tipo.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa</param>
        /// <param name="catalogoId">Id del catálogo</param>
        /// <param name="tipoId">Tipo de catálogo</param>
        /// <returns></returns>
        public ErrorDto<CatalogoValidate> AF_Catalogos_Valida(int CodEmpresa, string catalogoId, int tipoId)
        {
            var resultadoVacio = new CatalogoValidate();

            var result = DbHelper.ExecuteSingleQuery<CatalogoValidate>(
                CreatePortalDb(),
                CodEmpresa,
                @"SELECT COUNT(*) AS Existe, MAX(Linea_Id) AS Linea_Id
                  FROM AFI_CATALOGOS
                  WHERE Catalogo_Id = @Catalogo_Id AND Tipo_Id = @Tipo_Id",
                resultadoVacio,
                new { Catalogo_Id = catalogoId, Tipo_Id = tipoId });

            if (result.Code != 0)
            {
                return DbHelper.CreateErrorResponse(result.Description ?? "Error al validar catálogo.", result.Code.GetValueOrDefault(-1), resultadoVacio);
            }

            return result.Result is not null && result.Result.Existe > 0
                ? DbHelper.CreateErrorResponse("El catálogo ya existe.", -1, result.Result)
                : DbHelper.CreateOkResponse(result.Result ?? resultadoVacio);
        }

        /// <summary>
        /// Inserta o actualiza un catálogo.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa</param>
        /// <param name="usuario">Usuario que realiza la operación</param>
        /// <param name="catalogo">Datos del catálogo</param>
        /// <returns></returns>
        public ErrorDto AF_Catalogos_Guardar(int CodEmpresa, string usuario, CatalogoData catalogo)
        {
            if (catalogo is null)
            {
                return DbHelper.ErrorResponse("Los datos del catálogo son requeridos.", -2);
            }

            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
            {
                var existe = connection.QueryFirstOrDefault<int>(
                    @"SELECT COUNT(*) FROM AFI_CATALOGOS WHERE Linea_Id = @Linea_Id",
                    new { Linea_Id = catalogo.Linea_Id });

                return catalogo.Linea_Id == 0 || existe == 0
                    ? AF_Catalogos_Insertar(connection, CodEmpresa, usuario, catalogo)
                    : AF_Catalogos_Actualizar(connection, CodEmpresa, usuario, catalogo);
            });

            return result.Code == 0 && result.Result is not null
                ? result.Result
                : DbHelper.ErrorResponse(result.Description ?? "Error al guardar catálogo.", result.Code.GetValueOrDefault(-1));
        }

        /// <summary>
        /// Inserta un nuevo catálogo.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa</param>
        /// <param name="usuario">Usuario que realiza la operación</param>
        /// <param name="catalogo">Datos del catálogo a insertar</param>
        /// <returns></returns>
        private ErrorDto AF_Catalogos_Insertar(SqlConnection connection, int CodEmpresa, string usuario, CatalogoData catalogo)
        {
            connection.Execute(
                @"INSERT INTO AFI_CATALOGOS
                  (Catalogo_Id, Descripcion, Activo, Tipo_Id, Registro_Fecha, Registro_Usuario)
                  VALUES (@Catalogo_Id, @Descripcion, @Activo, @Tipo_Id, GETDATE(), @Usuario)",
                new
                {
                    Catalogo_Id = catalogo.Catalogo_Id,
                    Descripcion = catalogo.Descripcion,
                    Activo = catalogo.Activo,
                    Tipo_Id = catalogo.Tipo_Id,
                    Usuario = usuario
                });

            RegistrarBitacora(
                CodEmpresa,
                usuario,
                $"Catálogo : {catalogo.Catalogo_Id} - {catalogo.Descripcion}",
                "Registra - WEB");

            return DbHelper.OkResponse("Ok");
        }

        /// <summary>
        /// Actualiza un catálogo existente.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa</param>
        /// <param name="usuario">Usuario que realiza la operación</param>
        /// <param name="catalogo">Datos del catálogo a actualizar</param>
        /// <returns></returns>
        private ErrorDto AF_Catalogos_Actualizar(SqlConnection connection, int CodEmpresa, string usuario, CatalogoData catalogo)
        {
            connection.Execute(
                @"UPDATE AFI_CATALOGOS
                  SET Descripcion = @Descripcion,
                      Activo = @Activo,
                      Modifica_Fecha = GETDATE(),
                      Modifica_Usuario = @Usuario
                  WHERE Linea_Id = @Linea_Id",
                new
                {
                    Linea_Id = catalogo.Linea_Id,
                    Descripcion = catalogo.Descripcion,
                    Activo = catalogo.Activo,
                    Usuario = usuario
                });

            RegistrarBitacora(
                CodEmpresa,
                usuario,
                $"Catálogo : {catalogo.Catalogo_Id} - {catalogo.Descripcion}",
                "Modifica - WEB");

            return DbHelper.OkResponse("Ok");
        }

        /// <summary>
        /// Elimina un catálogo por su id.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa</param>
        /// <param name="usuario">Usuario que realiza la operación</param>
        /// <param name="lineaId">Id del catálogo a eliminar</param>
        /// <returns></returns>
        public ErrorDto AF_Catalogos_Eliminar(int CodEmpresa, string usuario, int lineaId)
        {
            var result = DbHelper.ExecuteNonQuery(
                CreatePortalDb(),
                CodEmpresa,
                @"DELETE FROM AFI_CATALOGOS WHERE Linea_Id = @Linea_Id",
                new { Linea_Id = lineaId });

            if (result.Code != 0)
            {
                return DbHelper.ErrorResponse(result.Description ?? "Error al eliminar catálogo.", result.Code.GetValueOrDefault(-1));
            }

            RegistrarBitacora(
                CodEmpresa,
                usuario,
                $"Catálogo : {lineaId}",
                "Elimina - WEB");

            return DbHelper.OkResponse("Ok");
        }

        /// <summary>
        /// Obtiene la lista de tipos de catálogo activos para dropdown.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa</param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> AF_Catalogos_Tipos_Obtener(int CodEmpresa)
        {
            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                CreatePortalDb(),
                CodEmpresa,
                @"SELECT CAST(Tipo_Id AS VARCHAR) AS item, Descripcion AS descripcion
                  FROM AFI_CATALOGOS_TIPOS
                  WHERE Activo = 1
                  ORDER BY Descripcion");
        }

        /// <summary>
        /// Obtiene la lista completa de tipos de catálogo, con filtros aplicados a todos los campos principales.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa</param>
        /// <param name="filtros">Filtros de búsqueda, orden y paginación</param>
        /// <returns></returns>
        public ErrorDto<List<CatalogoTipoData>> AF_Catalogos_Tipos_ObtenerTodos(int CodEmpresa, FiltrosLazyLoadData filtros)
        {
            if (filtros is null)
            {
                return DbHelper.CreateErrorResponse("Los filtros de tipos de catálogo son requeridos.", -2, new List<CatalogoTipoData>());
            }

            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
            {
                var filtroTexto = filtros.filtro?.Trim();
                var sortField = ObtenerSortFieldCatalogosTipos(filtros.sortField);
                var sortDirection = ObtenerSortDirectionCatalogosTipos(filtros.sortOrder);
                var offsetRows = filtros.pagina;
                var fetchRows = filtros.paginacion;

                const string query = @"
                    SELECT Tipo_Id, Descripcion, Activo, Registro_Fecha, Registro_Usuario
                    FROM AFI_CATALOGOS_TIPOS
                    WHERE (
                        @Filtro IS NULL
                        OR CAST(Tipo_Id AS VARCHAR(50)) LIKE @Filtro
                        OR Descripcion LIKE @Filtro
                        OR CAST(Activo AS VARCHAR(10)) LIKE @Filtro
                        OR CONVERT(VARCHAR(25), Registro_Fecha, 120) LIKE @Filtro
                        OR Registro_Usuario LIKE @Filtro
                    )
                    ORDER BY
                        CASE WHEN @SortField = '" + CampoTipoId + @"' AND @SortDirection = 'ASC' THEN Tipo_Id END ASC,
                        CASE WHEN @SortField = '" + CampoTipoId + @"' AND @SortDirection = 'DESC' THEN Tipo_Id END DESC,
                        CASE WHEN @SortField = '" + CampoDescripcion + @"' AND @SortDirection = 'ASC' THEN Descripcion END ASC,
                        CASE WHEN @SortField = '" + CampoDescripcion + @"' AND @SortDirection = 'DESC' THEN Descripcion END DESC,
                        CASE WHEN @SortField = '" + CampoActivo + @"' AND @SortDirection = 'ASC' THEN CAST(Activo AS INT) END ASC,
                        CASE WHEN @SortField = '" + CampoActivo + @"' AND @SortDirection = 'DESC' THEN CAST(Activo AS INT) END DESC,
                        CASE WHEN @SortField = '" + CampoRegistroFecha + @"' AND @SortDirection = 'ASC' THEN Registro_Fecha END ASC,
                        CASE WHEN @SortField = '" + CampoRegistroFecha + @"' AND @SortDirection = 'DESC' THEN Registro_Fecha END DESC,
                        CASE WHEN @SortField = '" + CampoRegistroUsuario + @"' AND @SortDirection = 'ASC' THEN Registro_Usuario END ASC,
                        CASE WHEN @SortField = '" + CampoRegistroUsuario + @"' AND @SortDirection = 'DESC' THEN Registro_Usuario END DESC,
                        Descripcion ASC
                    OFFSET @OffsetRows ROWS
                    FETCH NEXT @FetchRows ROWS ONLY";

                var parametros = new DynamicParameters();
                parametros.Add("Filtro", string.IsNullOrWhiteSpace(filtroTexto) ? null : $"%{filtroTexto}%");
                parametros.Add("SortField", sortField);
                parametros.Add("SortDirection", sortDirection);
                parametros.Add("OffsetRows", offsetRows);
                parametros.Add("FetchRows", fetchRows);

                return connection.Query<CatalogoTipoData>(query, parametros).ToList();
            });

            return result.Code == 0
                ? DbHelper.CreateOkResponse(result.Result ?? new List<CatalogoTipoData>())
                : DbHelper.CreateErrorResponse(result.Description ?? "Error al obtener tipos de catálogo.", result.Code.GetValueOrDefault(-1), new List<CatalogoTipoData>());
        }

        private static string ObtenerSortFieldCatalogos(string? sortField)
        {
            return sortField switch
            {
                "Linea_Id" => "Linea_Id",
                "Catalogo_Id" => "Catalogo_Id",
                CampoDescripcion => CampoDescripcion,
                CampoActivo => CampoActivo,
                CampoTipoId => CampoTipoId,
                CampoRegistroFecha => CampoRegistroFecha,
                CampoRegistroUsuario => CampoRegistroUsuario,
                _ => "Catalogo_Id"
            };
        }

        private static string ObtenerSortDirectionCatalogos(int sortOrder)
        {
            return sortOrder == 0 ? "DESC" : "ASC";
        }

        private static string ObtenerSortFieldCatalogosTipos(string? sortField)
        {
            return sortField switch
            {
                CampoTipoId => CampoTipoId,
                CampoDescripcion => CampoDescripcion,
                CampoActivo => CampoActivo,
                CampoRegistroFecha => CampoRegistroFecha,
                CampoRegistroUsuario => CampoRegistroUsuario,
                _ => CampoDescripcion
            };
        }

        private static string ObtenerSortDirectionCatalogosTipos(int sortOrder)
        {
            return sortOrder == 0 ? "DESC" : "ASC";
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

        private PortalDB CreatePortalDb() => new(_config);
    }
}