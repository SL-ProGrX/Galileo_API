using Dapper;
using Microsoft.Data.SqlClient;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Personas;
using Galileo.Models.Security;

namespace Galileo.DataBaseTier.ProGrX_Personas
{
    public class FrmAFCanalesTiposDB
    {
        private readonly IConfiguration _config;
        private readonly int vModulo = 1; // Modulo de Tesorería
        private readonly MSecurityMainDb _Security_MainDB;

        public FrmAFCanalesTiposDB(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _Security_MainDB = new MSecurityMainDb(_config);
        }

        /// <summary>
        /// Obtiene la lista de tipos de canales con filtros y paginación.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa</param>
        /// <param name="filtros">Filtros de búsqueda, orden y paginación</param>
        /// <returns></returns>
        public ErrorDto<CanalTipoLista> AF_CanalesTipos_Obtener(int CodEmpresa, FiltrosLazyLoadData filtros)
        {
            if (filtros is null)
            {
                return DbHelper.CreateErrorResponse("Los filtros de tipos de canales son requeridos.", -2, new CanalTipoLista());
            }

            var resultadoVacio = new CanalTipoLista
            {
                Total = 0,
                Lista = new List<CanalTipoData>()
            };

            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
            {
                var salida = new CanalTipoLista
                {
                    Total = connection.QueryFirstOrDefault<int>("select COUNT(CANAL_TIPO) from AFI_CANALES_TIPOS"),
                    Lista = new List<CanalTipoData>()
                };

                var filtroTexto = filtros.filtro?.Trim();
                var sortField = ObtenerSortFieldCanales(filtros.sortField);
                var sortDirection = ObtenerSortDirectionCanales(filtros.sortOrder);
                var offsetRows = filtros.pagina;
                var fetchRows = filtros.paginacion;

                const string query = @"
                    select CANAL_TIPO, descripcion, activo, Registro_Fecha, Registro_Usuario
                    from AFI_CANALES_TIPOS
                    where (
                        @Filtro is null
                        or CANAL_TIPO like @Filtro
                        or descripcion like @Filtro
                        or Registro_Usuario like @Filtro
                    )
                    order by
                        CASE WHEN @SortField = 'CANAL_TIPO' AND @SortDirection = 'ASC' THEN CANAL_TIPO END ASC,
                        CASE WHEN @SortField = 'CANAL_TIPO' AND @SortDirection = 'DESC' THEN CANAL_TIPO END DESC,
                        CASE WHEN @SortField = 'descripcion' AND @SortDirection = 'ASC' THEN descripcion END ASC,
                        CASE WHEN @SortField = 'descripcion' AND @SortDirection = 'DESC' THEN descripcion END DESC,
                        CASE WHEN @SortField = 'activo' AND @SortDirection = 'ASC' THEN CAST(activo AS INT) END ASC,
                        CASE WHEN @SortField = 'activo' AND @SortDirection = 'DESC' THEN CAST(activo AS INT) END DESC,
                        CASE WHEN @SortField = 'Registro_Fecha' AND @SortDirection = 'ASC' THEN Registro_Fecha END ASC,
                        CASE WHEN @SortField = 'Registro_Fecha' AND @SortDirection = 'DESC' THEN Registro_Fecha END DESC,
                        CASE WHEN @SortField = 'Registro_Usuario' AND @SortDirection = 'ASC' THEN Registro_Usuario END ASC,
                        CASE WHEN @SortField = 'Registro_Usuario' AND @SortDirection = 'DESC' THEN Registro_Usuario END DESC,
                        CANAL_TIPO ASC
                    OFFSET @OffsetRows ROWS
                    FETCH NEXT @FetchRows ROWS ONLY";

                var parametros = new DynamicParameters();
                parametros.Add("Filtro", string.IsNullOrWhiteSpace(filtroTexto) ? null : $"%{filtroTexto}%");
                parametros.Add("SortField", sortField);
                parametros.Add("SortDirection", sortDirection);
                parametros.Add("OffsetRows", offsetRows);
                parametros.Add("FetchRows", fetchRows);

                salida.Lista = connection.Query<CanalTipoData>(query, parametros).ToList();
                return salida;
            });

            return result.Code == 0
                ? DbHelper.CreateOkResponse(result.Result ?? resultadoVacio)
                : DbHelper.CreateErrorResponse(result.Description ?? "Error al obtener tipos de canales.", result.Code.GetValueOrDefault(-1), resultadoVacio);
        }

        /// <summary>
        /// Inserta o actualiza un tipo de canal.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa</param>
        /// <param name="usuario">Usuario que realiza la operación</param>
        /// <param name="canalTipo">Datos del tipo de canal</param>
        /// <returns></returns>
        public ErrorDto AF_CanalesTipos_Guardar(int CodEmpresa, string usuario, CanalTipoData canalTipo)
        {
            if (canalTipo is null)
            {
                return DbHelper.ErrorResponse("Los datos del tipo de canal son requeridos.", -2);
            }

            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
            {
                var queryExiste = @"SELECT ISNULL(COUNT(*),0) AS Existe FROM AFI_CANALES_TIPOS WHERE CANAL_TIPO = @CANAL_TIPO";
                var existe = connection.QueryFirstOrDefault<int>(queryExiste, new { CANAL_TIPO = canalTipo.Canal_Tipo.ToUpper() });

                return existe == 0
                    ? AF_CanalesTipos_Insertar(connection, CodEmpresa, usuario, canalTipo)
                    : AF_CanalesTipos_Actualizar(connection, CodEmpresa, usuario, canalTipo);
            });

            return result.Code == 0 && result.Result is not null
                ? result.Result
                : DbHelper.ErrorResponse(result.Description ?? "Error al guardar tipo de canal.", result.Code.GetValueOrDefault(-1));
        }

        /// <summary>
        /// Inserta un nuevo tipo de canal.
        /// </summary>
        /// <param name="connection">Conexión SQL abierta</param>
        /// <param name="CodEmpresa">Código de la empresa</param>
        /// <param name="usuario">Usuario que realiza la operación</param>
        /// <param name="canalTipo">Datos del tipo de canal a insertar</param>
        /// <returns></returns>
        private ErrorDto AF_CanalesTipos_Insertar(SqlConnection connection, int CodEmpresa, string usuario, CanalTipoData canalTipo)
        {
            connection.Execute(
                @"INSERT INTO AFI_CANALES_TIPOS (CANAL_TIPO, Descripcion, activo, registro_fecha, registro_usuario)
                  VALUES (@CANAL_TIPO, @Descripcion, @activo, GETDATE(), @Usuario)",
                new
                {
                    CANAL_TIPO = canalTipo.Canal_Tipo.ToUpper(),
                    Descripcion = canalTipo.Descripcion,
                    activo = canalTipo.Activo,
                    Usuario = usuario
                });

            RegistrarBitacora(
                CodEmpresa,
                usuario,
                $"Tipo de Canal : {canalTipo.Canal_Tipo} - {canalTipo.Descripcion}",
                "Registra - WEB");

            return DbHelper.OkResponse("Ok");
        }

        /// <summary>
        /// Actualiza un tipo de canal existente.
        /// </summary>
        /// <param name="connection">Conexión SQL abierta</param>
        /// <param name="CodEmpresa">Código de la empresa</param>
        /// <param name="usuario">Usuario que realiza la operación</param>
        /// <param name="canalTipo">Datos del tipo de canal a actualizar</param>
        /// <returns></returns>
        private ErrorDto AF_CanalesTipos_Actualizar(SqlConnection connection, int CodEmpresa, string usuario, CanalTipoData canalTipo)
        {
            connection.Execute(
                @"UPDATE AFI_CANALES_TIPOS
                  SET Descripcion = @Descripcion,
                      activo = @activo
                  WHERE CANAL_TIPO = @CANAL_TIPO",
                new
                {
                    CANAL_TIPO = canalTipo.Canal_Tipo.ToUpper(),
                    Descripcion = canalTipo.Descripcion,
                    activo = canalTipo.Activo
                });

            RegistrarBitacora(
                CodEmpresa,
                usuario,
                $"Tipo de Canal : {canalTipo.Canal_Tipo} - {canalTipo.Descripcion}",
                "Modifica - WEB");

            return DbHelper.OkResponse("Ok");
        }

        /// <summary>
        /// Elimina un tipo de canal por su código.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa</param>
        /// <param name="usuario">Usuario que realiza la operación</param>
        /// <param name="canalTipo">Código del tipo de canal a eliminar</param>
        /// <returns></returns>
        public ErrorDto AF_CanalesTipos_Eliminar(int CodEmpresa, string usuario, string canalTipo)
        {
            var result = DbHelper.ExecuteNonQuery(
                CreatePortalDb(),
                CodEmpresa,
                @"DELETE FROM AFI_CANALES_TIPOS WHERE CANAL_TIPO = @CANAL_TIPO",
                new { CANAL_TIPO = canalTipo.ToUpper() });

            if (result.Code != 0)
            {
                return DbHelper.ErrorResponse(result.Description ?? "Error al eliminar tipo de canal.", result.Code.GetValueOrDefault(-1));
            }

            RegistrarBitacora(
                CodEmpresa,
                usuario,
                $"Tipo de Canal : {canalTipo}",
                "Elimina - WEB");

            return DbHelper.OkResponse("Ok");
        }

        /// <summary>
        /// Valida si un tipo de canal ya existe en la base de datos.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa</param>
        /// <param name="canalTipo">Código del tipo de canal a validar</param>
        /// <returns></returns>
        public ErrorDto AF_CanalesTipos_Valida(int CodEmpresa, string canalTipo)
        {
            var result = DbHelper.ExecuteSingleQuery<int>(
                CreatePortalDb(),
                CodEmpresa,
                @"SELECT ISNULL(COUNT(*),0) AS Existe FROM AFI_CANALES_TIPOS WHERE CANAL_TIPO = @CANAL_TIPO",
                0,
                new { CANAL_TIPO = canalTipo.ToUpper() });

            if (result.Code != 0)
            {
                return DbHelper.ErrorResponse(result.Description ?? "Error al validar tipo de canal.", result.Code.GetValueOrDefault(-1));
            }

            return result.Result > 0
                ? DbHelper.ErrorResponse("El tipo de canal ya existe.", -1)
                : DbHelper.OkResponse("El tipo de canal es válido.");
        }

        /// <summary>
        /// Obtiene la lista de tipos de canales sin paginación, con filtros aplicados.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa</param>
        /// <param name="filtros">Filtros de búsqueda</param>
        /// <returns></returns>
        public ErrorDto<List<CanalTipoData>> AF_CanalesTipos_Exportar(int CodEmpresa, FiltrosLazyLoadData filtros)
        {
            if (filtros is null)
            {
                return DbHelper.CreateErrorResponse("Los filtros de exportación son requeridos.", -2, new List<CanalTipoData>());
            }

            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
            {
                var filtroTexto = filtros.filtro?.Trim();

                const string query = @"
                    select CANAL_TIPO, descripcion, activo, Registro_Fecha, Registro_Usuario
                    from AFI_CANALES_TIPOS
                    where (
                        @Filtro is null
                        or CANAL_TIPO like @Filtro
                        or descripcion like @Filtro
                        or Registro_Usuario like @Filtro
                    )
                    order by CANAL_TIPO";

                return connection.Query<CanalTipoData>(
                    query,
                    new
                    {
                        Filtro = string.IsNullOrWhiteSpace(filtroTexto) ? null : $"%{filtroTexto}%"
                    }).ToList();
            });

            return result.Code == 0
                ? DbHelper.CreateOkResponse(result.Result ?? new List<CanalTipoData>())
                : DbHelper.CreateErrorResponse(result.Description ?? "Error al exportar tipos de canales.", result.Code.GetValueOrDefault(-1), new List<CanalTipoData>());
        }

        private static string ObtenerSortFieldCanales(string? sortField)
        {
            return sortField switch
            {
                "CANAL_TIPO" => "CANAL_TIPO",
                "descripcion" => "descripcion",
                "activo" => "activo",
                "Registro_Fecha" => "Registro_Fecha",
                "Registro_Usuario" => "Registro_Usuario",
                _ => "CANAL_TIPO"
            };
        }

        private static string ObtenerSortDirectionCanales(int sortOrder)
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
