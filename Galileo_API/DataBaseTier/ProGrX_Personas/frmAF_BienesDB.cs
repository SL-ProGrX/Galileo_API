using Dapper;
using Microsoft.Data.SqlClient;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Personas;
using Galileo.Models.Security;

namespace Galileo.DataBaseTier.ProGrX_Personas
{
    public class FrmAFBienesDB
    {
        private readonly IConfiguration _config;
        private readonly int vModulo = 1; // Modulo de Tesorería
        private readonly MSecurityMainDb _Security_MainDB;

        public FrmAFBienesDB(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _Security_MainDB = new MSecurityMainDb(_config);
        }

        /// <summary>
        /// Obtiene la lista de tipos de bienes con filtros y paginación.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa</param>
        /// <param name="filtros">Filtros de búsqueda, orden y paginación</param>
        /// <returns></returns>
        public ErrorDto<BienesTipoLista> AF_BienesTipos_Obtener(int CodEmpresa, FiltrosLazyLoadData filtros)
        {
            if (filtros is null)
            {
                return DbHelper.CreateErrorResponse("Los filtros de tipos de bienes son requeridos.", -2, new BienesTipoLista());
            }

            var resultadoVacio = new BienesTipoLista
            {
                Total = 0,
                Lista = new List<BienesTipoData>()
            };

            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
            {
                var salida = new BienesTipoLista
                {
                    Total = connection.QueryFirstOrDefault<int>("select COUNT(BIEN_TIPO) from AFI_BIENES_TIPOS"),
                    Lista = new List<BienesTipoData>()
                };

                var filtroTexto = filtros.filtro?.Trim();
                var sortField = ObtenerSortFieldBienes(filtros.sortField);
                var sortDirection = ObtenerSortDirectionBienes(filtros.sortOrder);
                var offsetRows = filtros.pagina;
                var fetchRows = filtros.paginacion;

                const string query = @"
                    select BIEN_TIPO, descripcion, ACTIVO, Registro_Fecha, Registro_Usuario
                    from AFI_BIENES_TIPOS
                    where (
                        @Filtro is null
                        or BIEN_TIPO like @Filtro
                        or descripcion like @Filtro
                        or Registro_Usuario like @Filtro
                    )
                    order by
                        CASE WHEN @SortField = 'BIEN_TIPO' AND @SortDirection = 'ASC' THEN BIEN_TIPO END ASC,
                        CASE WHEN @SortField = 'BIEN_TIPO' AND @SortDirection = 'DESC' THEN BIEN_TIPO END DESC,
                        CASE WHEN @SortField = 'descripcion' AND @SortDirection = 'ASC' THEN descripcion END ASC,
                        CASE WHEN @SortField = 'descripcion' AND @SortDirection = 'DESC' THEN descripcion END DESC,
                        CASE WHEN @SortField = 'ACTIVO' AND @SortDirection = 'ASC' THEN CAST(ACTIVO AS INT) END ASC,
                        CASE WHEN @SortField = 'ACTIVO' AND @SortDirection = 'DESC' THEN CAST(ACTIVO AS INT) END DESC,
                        CASE WHEN @SortField = 'Registro_Fecha' AND @SortDirection = 'ASC' THEN Registro_Fecha END ASC,
                        CASE WHEN @SortField = 'Registro_Fecha' AND @SortDirection = 'DESC' THEN Registro_Fecha END DESC,
                        CASE WHEN @SortField = 'Registro_Usuario' AND @SortDirection = 'ASC' THEN Registro_Usuario END ASC,
                        CASE WHEN @SortField = 'Registro_Usuario' AND @SortDirection = 'DESC' THEN Registro_Usuario END DESC,
                        BIEN_TIPO ASC
                    OFFSET @OffsetRows ROWS
                    FETCH NEXT @FetchRows ROWS ONLY";

                var parametros = new DynamicParameters();
                parametros.Add("Filtro", string.IsNullOrWhiteSpace(filtroTexto) ? null : $"%{filtroTexto}%");
                parametros.Add("SortField", sortField);
                parametros.Add("SortDirection", sortDirection);
                parametros.Add("OffsetRows", offsetRows);
                parametros.Add("FetchRows", fetchRows);

                salida.Lista = connection.Query<BienesTipoData>(query, parametros).ToList();
                return salida;
            });

            return result.Code == 0
                ? DbHelper.CreateOkResponse(result.Result ?? resultadoVacio)
                : DbHelper.CreateErrorResponse(result.Description ?? "Error al obtener tipos de bienes.", result.Code.GetValueOrDefault(-1), resultadoVacio);
        }

        /// <summary>
        /// Inserta o actualiza un tipo de bien.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa</param>
        /// <param name="usuario">Usuario que realiza la operación</param>
        /// <param name="bienTipo">Datos del tipo de bien</param>
        /// <returns></returns>
        public ErrorDto AF_BienesTipos_Guardar(int CodEmpresa, string usuario, BienesTipoData bienTipo)
        {
            if (bienTipo is null)
            {
                return DbHelper.ErrorResponse("Los datos del tipo de bien son requeridos.", -2);
            }

            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
            {
                var queryExiste = @"SELECT ISNULL(COUNT(*),0) AS Existe FROM AFI_BIENES_TIPOS WHERE BIEN_TIPO = @BIEN_TIPO";
                var existe = connection.QueryFirstOrDefault<int>(queryExiste, new { BIEN_TIPO = bienTipo.Bien_Tipo.ToUpper() });

                return existe == 0
                    ? AF_BienesTipos_Insertar(connection, CodEmpresa, usuario, bienTipo)
                    : AF_BienesTipos_Actualizar(connection, CodEmpresa, usuario, bienTipo);
            });

            return result.Code == 0 && result.Result is not null
                ? result.Result
                : DbHelper.ErrorResponse(result.Description ?? "Error al guardar tipo de bien.", result.Code.GetValueOrDefault(-1));
        }

        /// <summary>
        /// Inserta un nuevo tipo de bien.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa</param>
        /// <param name="usuario">Usuario que realiza la operación</param>
        /// <param name="bienTipo">Datos del tipo de bien a insertar</param>
        /// <returns></returns>
        private ErrorDto AF_BienesTipos_Insertar(SqlConnection connection, int CodEmpresa, string usuario, BienesTipoData bienTipo)
        {
            connection.Execute(
                @"INSERT INTO AFI_BIENES_TIPOS (BIEN_TIPO, Descripcion, ACTIVO, registro_fecha, registro_usuario)
                  VALUES (@BIEN_TIPO, @Descripcion, @ACTIVO, GETDATE(), @Usuario)",
                new
                {
                    BIEN_TIPO = bienTipo.Bien_Tipo.ToUpper(),
                    Descripcion = bienTipo.Descripcion,
                    ACTIVO = bienTipo.Activo,
                    Usuario = usuario
                });

            RegistrarBitacora(
                CodEmpresa,
                usuario,
                $"Tipo de Bien : {bienTipo.Bien_Tipo} - {bienTipo.Descripcion}",
                "Registra - WEB");

            return DbHelper.OkResponse("Ok");
        }

        /// <summary>
        /// Actualiza un tipo de bien existente.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa</param>
        /// <param name="usuario">Usuario que realiza la operación</param>
        /// <param name="bienTipo">Datos del tipo de bien a actualizar</param>
        /// <returns></returns>
        private ErrorDto AF_BienesTipos_Actualizar(SqlConnection connection, int CodEmpresa, string usuario, BienesTipoData bienTipo)
        {
            connection.Execute(
                @"UPDATE AFI_BIENES_TIPOS
                  SET Descripcion = @Descripcion,
                      ACTIVO = @ACTIVO
                  WHERE BIEN_TIPO = @BIEN_TIPO",
                new
                {
                    BIEN_TIPO = bienTipo.Bien_Tipo.ToUpper(),
                    Descripcion = bienTipo.Descripcion,
                    ACTIVO = bienTipo.Activo
                });

            RegistrarBitacora(
                CodEmpresa,
                usuario,
                $"Tipo de Bien : {bienTipo.Bien_Tipo} - {bienTipo.Descripcion}",
                "Modifica - WEB");

            return DbHelper.OkResponse("Ok");
        }

        /// <summary>
        /// Elimina un tipo de bien por su código.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa</param>
        /// <param name="usuario">Usuario que realiza la operación</param>
        /// <param name="bienTipo">Código del tipo de bien a eliminar</param>
        /// <returns></returns>
        public ErrorDto AF_BienesTipos_Eliminar(int CodEmpresa, string usuario, string bienTipo)
        {
            var result = DbHelper.ExecuteNonQuery(
                CreatePortalDb(),
                CodEmpresa,
                @"DELETE FROM AFI_BIENES_TIPOS WHERE BIEN_TIPO = @BIEN_TIPO",
                new { BIEN_TIPO = bienTipo.ToUpper() });

            if (result.Code != 0)
            {
                return DbHelper.ErrorResponse(result.Description ?? "Error al eliminar tipo de bien.", result.Code.GetValueOrDefault(-1));
            }

            RegistrarBitacora(
                CodEmpresa,
                usuario,
                $"Tipo de Bien : {bienTipo}",
                "Elimina - WEB");

            return DbHelper.OkResponse("Ok");
        }

        /// <summary>
        /// Valida si un tipo de bien ya existe en la base de datos.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa</param>
        /// <param name="bienTipo">Código del tipo de bien a validar</param>
        /// <returns></returns>
        public ErrorDto AF_BienesTipos_Valida(int CodEmpresa, string bienTipo)
        {
            var result = DbHelper.ExecuteSingleQuery<int>(
                CreatePortalDb(),
                CodEmpresa,
                @"SELECT ISNULL(COUNT(*),0) AS Existe FROM AFI_BIENES_TIPOS WHERE BIEN_TIPO = @BIEN_TIPO",
                0,
                new { BIEN_TIPO = bienTipo.ToUpper() });

            if (result.Code != 0)
            {
                return DbHelper.ErrorResponse(result.Description ?? "Error al validar tipo de bien.", result.Code.GetValueOrDefault(-1));
            }

            return result.Result > 0
                ? DbHelper.ErrorResponse("El tipo de bien ya existe.", -1)
                : DbHelper.OkResponse("El tipo de bien es válido.");
        }

        /// <summary>
        /// Obtiene la lista de tipos de bienes sin paginación, con filtros aplicados.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa</param>
        /// <param name="filtros">Filtros de búsqueda</param>
        /// <returns></returns>
        public ErrorDto<List<BienesTipoData>> AF_BienesTipos_Exportar(int CodEmpresa, FiltrosLazyLoadData filtros)
        {
            if (filtros is null)
            {
                return DbHelper.CreateErrorResponse("Los filtros de exportación son requeridos.", -2, new List<BienesTipoData>());
            }

            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
            {
                var filtroTexto = filtros.filtro?.Trim();

                const string query = @"
                    select BIEN_TIPO, descripcion, ACTIVO, Registro_Fecha, Registro_Usuario
                    from AFI_BIENES_TIPOS
                    where (
                        @Filtro is null
                        or BIEN_TIPO like @Filtro
                        or descripcion like @Filtro
                        or Registro_Usuario like @Filtro
                    )
                    order by BIEN_TIPO";

                return connection.Query<BienesTipoData>(
                    query,
                    new
                    {
                        Filtro = string.IsNullOrWhiteSpace(filtroTexto) ? null : $"%{filtroTexto}%"
                    }).ToList();
            });

            return result.Code == 0
                ? DbHelper.CreateOkResponse(result.Result ?? new List<BienesTipoData>())
                : DbHelper.CreateErrorResponse(result.Description ?? "Error al exportar tipos de bienes.", result.Code.GetValueOrDefault(-1), new List<BienesTipoData>());
        }

        private static string ObtenerSortFieldBienes(string? sortField)
        {
            return sortField switch
            {
                "BIEN_TIPO" => "BIEN_TIPO",
                "descripcion" => "descripcion",
                "ACTIVO" => "ACTIVO",
                "Registro_Fecha" => "Registro_Fecha",
                "Registro_Usuario" => "Registro_Usuario",
                _ => "BIEN_TIPO"
            };
        }

        private static string ObtenerSortDirectionBienes(int sortOrder)
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