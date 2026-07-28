using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.Security;
using Microsoft.Data.SqlClient;
using System.Data.Common;
using static Galileo_API.Models.ProGrX_Comites.FrmAfCdCargos;

namespace Galileo_API.DataBaseTier.ProGrX_Comites
{
    public class FrmAfCdCargosDB
    {
        private readonly PortalDB _portalDB;
        private readonly MSecurityMainDb _securityMainDb;   

        private const int Modulo = 40;
        private const string Tabla = "dbo.AFI_CD_CARGOS";

        public FrmAfCdCargosDB(IConfiguration config)
        {
            _portalDB = new PortalDB(config);
            _securityMainDb = new MSecurityMainDb(config);
           
        }

        private static ErrorDto Err(string msg) => DbHelper.ErrorResponse(msg);

        /// <summary>
        /// Abre una conexión a la base de datos de la empresa usando el PortalDB para obtener una conexión segura y validada.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        private SqlConnection OpenConnection(int codEmpresa)
            => DbHelper.OpenConnection(_portalDB, codEmpresa);

        /// <summary>
        /// Crea un ErrorDto genérico con código 0 y descripción "Ok", incluyendo el resultado proporcionado, para estandarizar las respuestas exitosas de las consultas a la base de datos.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <returns></returns>
        private static ErrorDto<T> CreateOkResult<T>(T data)
        {
            return new ErrorDto<T>
            {
                Code = 0,
                Description = "Ok",
                Result = data
            };
        }

        /// <summary>
        /// Crea una instancia vacía de CdCargosLista con Total en 0 y una lista vacía, para usar como resultado en casos de error o cuando no se encuentren datos, asegurando que el formato de respuesta sea consistente.
        /// </summary>
        /// <returns></returns>
        private static CdCargosLista CreateEmptyLista()
        {
            return new CdCargosLista
            {
                Total = 0,
                lista = []
            };
        }

        /// <summary>
        /// Resuelve el campo de ordenamiento para la consulta SQL basándose en el valor de sortField recibido, aplicando una lógica de mapeo controlada que asigna campos específicos permitidos y devuelve un valor predeterminado seguro ("Codigo") si el valor no coincide con los casos esperados, evitando así la posibilidad de inyección SQL a través del campo de ordenamiento.
        /// </summary>
        /// <param name="sortField"></param>
        /// <returns></returns>
        private static string ResolveOrderBy(string? sortField)
        {
            return (sortField ?? string.Empty).Trim() switch
            {
                "Codigo" => "Codigo",
                "descripcion" => "descripcion",
                "cuenta" => "cuenta",
                "estado" => "estado",
                _ => "Codigo"
            };
        }

        /// <summary>
        /// Resuelve la dirección de ordenamiento para la consulta SQL basándose en el valor de sortOrder recibido, donde 1 representa orden descendente ("DESC") y cualquier otro valor representa orden ascendente ("ASC"), proporcionando un mecanismo simple y controlado para determinar la dirección del ordenamiento sin exponer la consulta a valores no esperados o maliciosos.
        /// </summary>
        /// <param name="sortOrder"></param>
        /// <returns></returns>
        private static string ResolveDirection(int? sortOrder)
            => sortOrder == 1 ? "DESC" : "ASC";

        /// <summary>
        /// Registra un movimiento en la bitácora de seguridad utilizando el MSecurityMainDb
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="detalle"></param>
        /// <param name="movimiento"></param>
        private void RegistrarBitacora(int codEmpresa, string usuario, string detalle, string movimiento)
        {
            _securityMainDb.Bitacora(new BitacoraInsertarDto
            {
                EmpresaId = codEmpresa,
                Usuario = usuario,
                DetalleMovimiento = detalle,
                Movimiento = movimiento,
                Modulo = Modulo
            });
        }

        /// <summary>
        /// Ejecuta una acción que interactúa con la base de datos
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="action"></param>
        /// <param name="dbErrorMessage"></param>
        /// <param name="genericErrorMessage"></param>
        /// <returns></returns>
        private ErrorDto ExecuteDbAction(
            int codEmpresa,
            Func<DbConnection, ErrorDto> action,
            string dbErrorMessage,
            string genericErrorMessage)
        {
            try
            {
                using var conn = OpenConnection(codEmpresa);
                return action(conn);
            }
            catch (DbException)
            {
                return Err(dbErrorMessage);
            }
            catch (Exception)
            {
                return Err(genericErrorMessage);
            }
        }

        /// <summary>
        /// Ejecuta una consulta a la base de datos que devuelve un resultado tipado, manejando errores específicos de base de datos y errores genéricos
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="codEmpresa"></param>
        /// <param name="action"></param>
        /// <param name="dbErrorMessage"></param>
        /// <param name="genericErrorMessage"></param>
        /// <param name="emptyResultFactory"></param>
        /// <returns></returns>
        private ErrorDto<T> ExecuteDbQuery<T>(
            int codEmpresa,
            Func<DbConnection, ErrorDto<T>> action,
            string dbErrorMessage,
            string genericErrorMessage,
            Func<T> emptyResultFactory)
        {
            try
            {
                using var conn = OpenConnection(codEmpresa);
                return action(conn);
            }
            catch (DbException)
            {
                return new ErrorDto<T>
                {
                    Code = -1,
                    Description = dbErrorMessage,
                    Result = emptyResultFactory()
                };
            }
            catch (Exception)
            {
                return new ErrorDto<T>
                {
                    Code = -1,
                    Description = genericErrorMessage,
                    Result = emptyResultFactory()
                };
            }
        }

        /// <summary>
        /// Obtiene una lista paginada y filtrada de cargos desde la base de datos, aplicando filtros de búsqueda, ordenamiento y paginación según los parámetros proporcionados en el objeto FiltrosLazyLoadData. Si esExportar es verdadero, se omite la paginación para obtener todos los registros que coincidan con el filtro
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="filtros"></param>
        /// <param name="esExportar"></param>
        /// <returns></returns>
        public ErrorDto<CdCargosLista> CdCargosLista_Obtener(
            int codEmpresa,
            FiltrosLazyLoadData filtros,
            bool esExportar)
        {
            filtros ??= new FiltrosLazyLoadData();

            return ExecuteDbQuery(
                codEmpresa,
                conn =>
                {
                    var texto = filtros.filtro?.Trim();
                    var hasFiltro = !string.IsNullOrWhiteSpace(texto);
                    var like = hasFiltro ? $"%{texto}%" : null;

                    var offset = filtros.pagina ;
                    var fetch = filtros.paginacion;
                    var usarPaginacion = fetch > 0 && !esExportar;

                    var orderByField = ResolveOrderBy(filtros.sortField);
                    var direction = ResolveDirection(filtros.sortOrder);

                    const string where = @"
                        WHERE
                            (@filtro IS NULL)
                            OR (CAST(Codigo AS NVARCHAR(50)) LIKE @like)
                            OR (descripcion LIKE @like)
                            OR (cuenta LIKE @like)";

                    var sqlCount = $@"SELECT COUNT(1) FROM {Tabla} {where};";

                    var sqlList = $@"
                        SELECT
                            Codigo,
                            descripcion,
                            cuenta,
                            estado
                        FROM {Tabla}
                        {where}
                        ORDER BY {orderByField} {direction}";

                    if (usarPaginacion)
                    {
                        sqlList += @"
                            OFFSET @offset ROWS
                            FETCH NEXT @fetch ROWS ONLY;";
                    }

                    var parameters = new
                    {
                        filtro = hasFiltro ? texto : null,
                        like,
                        offset,
                        fetch
                    };

                    var lista = CreateEmptyLista();
                    lista.Total = conn.QuerySingle<int>(sqlCount, parameters);
                    lista.lista = conn.Query<CdCargosData>(sqlList, parameters).ToList();

                    return CreateOkResult(lista);
                },
                "No fue posible consultar los datos.",
                "Error inesperado al consultar los datos.",
                CreateEmptyLista);
        }

        /// <summary>
        /// Guarda un cargo en la base de datos, determinando si se debe insertar un nuevo registro o actualizar uno existente según la presencia del código del cargo
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="datos"></param>
        /// <returns></returns>
        public ErrorDto AfCdCargos_Guardar(int codEmpresa, string usuario, CdCargosData datos)
        {
            if (datos is null)
            {
                return Err("Datos requeridos.");
            }

            if (string.IsNullOrWhiteSpace(usuario))
            {
                return Err("El usuario es requerido.");
            }

            return ExecuteDbAction(
                codEmpresa,
                conn =>
                {
                    const string sqlExiste = $"SELECT ISNULL(COUNT(*), 0) FROM {Tabla} WHERE Codigo = @Codigo;";
                    var existe = conn.QueryFirstOrDefault<int>(sqlExiste, new { datos.Codigo });

                    return existe > 0
                        ? AfCdCargos_Actualizar(codEmpresa, usuario, datos)
                        : AfCdCargos_Insertar(codEmpresa, usuario, datos);
                },
                "No fue posible guardar el Cargo.",
                "Error inesperado al guardar el Cargo.");
        }

        /// <summary>
        /// Inserta un nuevo cargo en la base de datos, generando un nuevo código automáticamente, y registra el movimiento en la bitácora de seguridad.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="datos"></param>
        /// <returns></returns>
        private ErrorDto AfCdCargos_Insertar(int codEmpresa, string usuario, CdCargosData datos)
        {
            return ExecuteDbAction(
                codEmpresa,
                conn =>
                {
                    const string sqlNuevoCodigo = @"
                        SELECT COALESCE(MAX(Codigo), 0) + 1
                        FROM AFI_CD_CARGOS";

                    var nuevoCodigo = conn.ExecuteScalar<int>(sqlNuevoCodigo);

                    const string sqlInsert = @"
                        INSERT INTO AFI_CD_CARGOS (Codigo, descripcion, cuenta, estado)
                        VALUES (@Codigo, @Descripcion, @Cuenta, @Estado);";

                    conn.Execute(sqlInsert, new
                    {
                        Codigo = nuevoCodigo,
                         datos.Descripcion,
                         datos.Cuenta,
                         datos.Estado
                    });

                    RegistrarBitacora(
                        codEmpresa,
                        usuario,
                        $"Cargo: {nuevoCodigo}",
                        "REGISTRA - WEB");

                    return DbHelper.OkResponse("Cargo insertado correctamente.");
                },
                "No fue posible insertar el Cargo.",
                "Error inesperado al insertar el Cargo.");
        }

        /// <summary>
        /// Actualiza un cargo existente en la base de datos según el código proporcionado en los datos, y registra el movimiento en la bitácora de seguridad. Si el código no existe, no se realiza ninguna actualización.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="datos"></param>
        /// <returns></returns>
        private ErrorDto AfCdCargos_Actualizar(int codEmpresa, string usuario, CdCargosData datos)
        {
            return ExecuteDbAction(
                codEmpresa,
                conn =>
                {
                    const string sqlUpdate = @"
                        UPDATE AFI_CD_CARGOS
                        SET
                            descripcion = @Descripcion,
                            cuenta = @Cuenta,
                            estado = @Estado
                        WHERE Codigo = @Codigo;";

                    conn.Execute(sqlUpdate, new
                    {
                        datos.Descripcion,
                        datos.Cuenta,
                        datos.Estado,
                        datos.Codigo
                    });

                    RegistrarBitacora(
                        codEmpresa,
                        usuario,
                        $"Cargo: {datos.Codigo}",
                        "MODIFICA - WEB");

                    return DbHelper.OkResponse("Cargo actualizado correctamente.");
                },
                "No fue posible actualizar el Cargo.",
                "Error inesperado al actualizar el Cargo.");
        }

        /// <summary>
        /// Elimina un cargo de la base de datos según el código proporcionado, y registra el movimiento en la bitácora de seguridad. Si el código no existe, no se realiza ninguna eliminación.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="codCargo"></param>
        /// <returns></returns>
        public ErrorDto AfCdCargos_Eliminar(int codEmpresa, string usuario, string codCargo)
        {
            if (string.IsNullOrWhiteSpace(usuario))
            {
                return Err("El usuario es requerido.");
            }

            if (string.IsNullOrWhiteSpace(codCargo))
            {
                return Err("El código del cargo es requerido.");
            }

            return ExecuteDbAction(
                codEmpresa,
                conn =>
                {
                    const string sqlDelete = @"DELETE AFI_CD_CARGOS WHERE Codigo = @CodCargo;";

                    conn.Execute(sqlDelete, new { CodCargo = codCargo });

                    RegistrarBitacora(
                        codEmpresa,
                        usuario,
                        $"Cargo: {codCargo}",
                        "ELIMINAR - WEB");

                    return DbHelper.OkResponse("Cargo eliminado correctamente.");
                },
                "No fue posible eliminar el Cargo.",
                "Error inesperado al eliminar el Cargo.");
        }
    }
}