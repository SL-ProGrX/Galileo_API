using System.Data;
using Dapper;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.Security;
using Microsoft.Data.SqlClient;
using Galileo.Models.ProGrX_Activos_Fijos;

namespace Galileo.DataBaseTier.ProGrX_Activos_Fijos
{
    public class FrmActivosDeclaracionesDB
    {
        private readonly PortalDB _portalDB;
        private readonly int vModulo = 36;
        private readonly MSecurityMainDb _Security_MainDB;

        private const string _declaraId = "DeclaraId";
        private const string _usuario = "Usuario";

        public FrmActivosDeclaracionesDB(IConfiguration config)
        {
            _portalDB      = new PortalDB(config);
            _Security_MainDB = new MSecurityMainDb(config);
        }

        private string GetConn(int CodEmpresa) =>
           _portalDB.ObtenerDbConnStringEmpresa(CodEmpresa);

        #region Helpers privados

        private static string? BuildFiltroLike(FiltrosLazyLoadData? filtros)
        {
            if (string.IsNullOrWhiteSpace(filtros?.filtro))
                return null;

            return $"%{filtros.filtro.Trim()}%";
        }

        /// <summary>
        /// Arma DynamicParameters para el listado paginado de declaraciones.
        /// </summary>
        private static (DynamicParameters parametros, bool sinPaginacion) BuildListaParametros(FiltrosLazyLoadData? filtros)
        {
            var p = new DynamicParameters();

            // Filtro
            p.Add("@filtro", BuildFiltroLike(filtros), DbType.String);

            // Ordenamiento
            var sortFieldNorm = (filtros?.sortField ?? "id_declara")
                .Trim()
                .ToLowerInvariant();
            int sortOrder = filtros?.sortOrder ?? 0; // 0 = DESC, 1 = ASC

            p.Add("@sortField", sortFieldNorm, DbType.String);
            p.Add("@sortOrder", sortOrder, DbType.Int32);

            // Paginación
            bool sinPaginacion = filtros == null || filtros.paginacion <= 0;
            if (!sinPaginacion)
            {
                p.Add("@offset", filtros!.pagina, DbType.Int32);
                p.Add("@rows", filtros.paginacion, DbType.Int32);
            }

            return (p, sinPaginacion);
        }

        /// <summary>
        /// Ejecuta el SP spActivos_Declara_Main_Add para insertar/actualizar.
        /// </summary>
        private (int pass, string mensaje, int idDeclara) EjecutarSpDeclaraMainAdd(
            int CodEmpresa,
            int declaraId,
            ActivosDeclaracionGuardarRequest data)
        {
            using var connection = _portalDB.CreateConnection(CodEmpresa);

            var p = new DynamicParameters();
            p.Add(_declaraId, declaraId);
            p.Add("@Notas",   data.notas);
            p.Add("@Tipo",    data.tipo);
            p.Add("@Inicio",  data.fecha_inicio);
            p.Add("@Corte",   data.fecha_corte);
            p.Add(_usuario,   data.usuario);

            var rs = connection.QueryFirstOrDefault<dynamic>(
                "spActivos_Declara_Main_Add",
                p,
                commandType: CommandType.StoredProcedure);

            int pass       = (int)(rs?.Pass ?? 0);
            string mensaje = (string)(rs?.Mensaje ?? "Error al procesar declaración.");
            int idDeclara  = (int)(rs?.ID_DECLARA ?? declaraId);

            return (pass, mensaje, idDeclara);
        }

        /// <summary>
        /// Ejecuta un SP simple (Delete/Cierra/Procesa) que recibe DeclaraId y Usuario y devuelve Pass/Mensaje.
        /// </summary>
        private (int pass, string mensaje) EjecutarSpDeclaraAccion(
            int CodEmpresa,
            string storedProcedure,
            int id_declara,
            string usuario,
            string defaultErrorMessage)
        {
            using var connection = _portalDB.CreateConnection(CodEmpresa);

            var p = new DynamicParameters();
            p.Add(_declaraId, id_declara);
            p.Add(_usuario,   usuario);

            var rs = connection.QueryFirstOrDefault<dynamic>(
                storedProcedure,
                p,
                commandType: CommandType.StoredProcedure);

            int pass       = (int)(rs?.Pass ?? 0);
            string mensaje = (string)(rs?.Mensaje ?? defaultErrorMessage);

            return (pass, mensaje);
        }

        private void RegistrarBitacoraDeclaracion(
            int CodEmpresa,
            string usuario,
            int idDeclara,
            string movimiento,
            string? detalleExtra = null)
        {
            var detalle = detalleExtra ??
                          $"Declaración de Activo Id: {idDeclara}";

            _Security_MainDB.Bitacora(new BitacoraInsertarDto
            {
                EmpresaId        = CodEmpresa,
                Usuario          = usuario ?? "",
                DetalleMovimiento = detalle,
                Movimiento       = movimiento,
                Modulo           = vModulo
            });
        }

        private ErrorDto<ActivosDeclaracionResult> ErrorResult(int code, string description)
        {
            return new ErrorDto<ActivosDeclaracionResult>
            {
                Code        = code,
                Description = description,
                Result      = null
            };
        }

        #endregion

        /// <summary>
        /// Obtiene el historial de declaraciones por lazy loading.
        /// </summary>
        public ErrorDto<ActivosDeclaracionLista> Activos_Declaraciones_Lista_Obtener(
            int CodEmpresa,
            FiltrosLazyLoadData filtros)
        {
            var result = new ErrorDto<ActivosDeclaracionLista>
            {
                Code        = 0,
                Description = "Ok",
                Result = new ActivosDeclaracionLista
                {
                    total = 0,
                    lista = new List<ActivosDeclaracionResumen>()
                }
            };

            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);

                var (p, sinPaginacion) = BuildListaParametros(filtros);

                const string countSql = @"
                    SELECT COUNT(1)
                    FROM vActivos_Declara
                    WHERE 1 = 1
                      AND (
                            @filtro IS NULL
                            OR CONVERT(varchar(10), id_declara) LIKE @filtro
                            OR notas LIKE @filtro
                          );";

                result.Result.total = connection.QueryFirstOrDefault<int>(countSql, p);

                const string selectBase = @"
                    SELECT
                        id_declara,
                        tipo_desc,
                        estado_desc,
                        CONVERT(varchar(10), fecha_inicio, 120) AS fecha_inicio,
                        CONVERT(varchar(10), fecha_corte, 120)  AS fecha_corte,
                        notas,
                        registro_fecha,
                        registro_usuario,
                        cerrado_fecha,
                        cerrado_usuario,
                        procesado_fecha,
                        procesado_usuario
                    FROM vActivos_Declara
                    WHERE 1 = 1
                      AND (
                            @filtro IS NULL
                            OR CONVERT(varchar(10), id_declara) LIKE @filtro
                            OR notas LIKE @filtro
                          )
                    ORDER BY
                        -- sortOrder = 0 => DESC
                        CASE WHEN @sortOrder = 0 AND @sortField = 'id_declara'        THEN id_declara        END DESC,
                        CASE WHEN @sortOrder = 0 AND @sortField = 'tipo_desc'         THEN tipo_desc         END DESC,
                        CASE WHEN @sortOrder = 0 AND @sortField = 'estado_desc'       THEN estado_desc       END DESC,
                        CASE WHEN @sortOrder = 0 AND @sortField = 'fecha_inicio'      THEN fecha_inicio      END DESC,
                        CASE WHEN @sortOrder = 0 AND @sortField = 'fecha_corte'       THEN fecha_corte       END DESC,
                        CASE WHEN @sortOrder = 0 AND @sortField = 'notas'             THEN notas             END DESC,
                        CASE WHEN @sortOrder = 0 AND @sortField = 'registro_fecha'    THEN registro_fecha    END DESC,
                        CASE WHEN @sortOrder = 0 AND @sortField = 'registro_usuario'  THEN registro_usuario  END DESC,
                        CASE WHEN @sortOrder = 0 AND @sortField = 'cerrado_fecha'     THEN cerrado_fecha     END DESC,
                        CASE WHEN @sortOrder = 0 AND @sortField = 'cerrado_usuario'   THEN cerrado_usuario   END DESC,
                        CASE WHEN @sortOrder = 0 AND @sortField = 'procesado_fecha'   THEN procesado_fecha   END DESC,
                        CASE WHEN @sortOrder = 0 AND @sortField = 'procesado_usuario' THEN procesado_usuario END DESC,

                        -- sortOrder = 1 => ASC
                        CASE WHEN @sortOrder = 1 AND @sortField = 'id_declara'        THEN id_declara        END ASC,
                        CASE WHEN @sortOrder = 1 AND @sortField = 'tipo_desc'         THEN tipo_desc         END ASC,
                        CASE WHEN @sortOrder = 1 AND @sortField = 'estado_desc'       THEN estado_desc       END ASC,
                        CASE WHEN @sortOrder = 1 AND @sortField = 'fecha_inicio'      THEN fecha_inicio      END ASC,
                        CASE WHEN @sortOrder = 1 AND @sortField = 'fecha_corte'       THEN fecha_corte       END ASC,
                        CASE WHEN @sortOrder = 1 AND @sortField = 'notas'             THEN notas             END ASC,
                        CASE WHEN @sortOrder = 1 AND @sortField = 'registro_fecha'    THEN registro_fecha    END ASC,
                        CASE WHEN @sortOrder = 1 AND @sortField = 'registro_usuario'  THEN registro_usuario  END ASC,
                        CASE WHEN @sortOrder = 1 AND @sortField = 'cerrado_fecha'     THEN cerrado_fecha     END ASC,
                        CASE WHEN @sortOrder = 1 AND @sortField = 'cerrado_usuario'   THEN cerrado_usuario   END ASC,
                        CASE WHEN @sortOrder = 1 AND @sortField = 'procesado_fecha'   THEN procesado_fecha   END ASC,
                        CASE WHEN @sortOrder = 1 AND @sortField = 'procesado_usuario' THEN procesado_usuario END ASC";

                const string pagingSql = @"
                    OFFSET @offset ROWS FETCH NEXT @rows ROWS ONLY";

                var dataSql = sinPaginacion ? selectBase : selectBase + pagingSql;

                result.Result.lista = connection
                    .Query<ActivosDeclaracionResumen>(dataSql, p)
                    .ToList();
            }
            catch (Exception ex)
            {
                result.Code           = -1;
                result.Description    = ex.Message;
                result.Result.total   = 0;
                result.Result.lista   = new List<ActivosDeclaracionResumen>();
            }

            return result;
        }

        /// <summary>
        /// Obtiene una declaración específica por Id.
        /// </summary>
        public ErrorDto<ActivosDeclaracion> Activos_Declaraciones_Registro_Obtener(
            int CodEmpresa,
            int id_declara)
        {
            var resp = new ErrorDto<ActivosDeclaracion>
            {
                Code        = 0,
                Description = "",
                Result      = null
            };

            const string sql = @"
                SET NOCOUNT ON;

                SELECT TOP (1)
                    ID_DECLARA                                AS id_declara,
                    TIPO                                      AS tipo,
                    Tipo_Desc                                 AS tipo_desc,
                    ESTADO                                    AS estado,
                    Estado_Desc                               AS estado_desc,
                    Notas                                     AS notas,
                    CONVERT(varchar(10), Fecha_Inicio, 120)   AS fecha_inicio,
                    CONVERT(varchar(10), Fecha_Corte, 120)    AS fecha_corte,
                    CONVERT(varchar(19), Registro_fecha,120 ) AS registro_fecha,
                    Registro_Usuario                          AS registro_usuario,
                    CONVERT(varchar(19), Cerrado_fecha,120 )  AS cerrado_fecha,
                    Cerrado_Usuario                           AS cerrado_usuario,
                    CONVERT(varchar(19), Procesado_fecha,120) AS procesado_fecha,
                    Procesado_Usuario                         AS procesado_usuario
                FROM vActivos_Declara
                WHERE ID_DECLARA = @id;";

            try
            {
                var connStr = GetConn(CodEmpresa);
                using var connection = new SqlConnection(connStr);

                resp.Result = connection.QueryFirstOrDefault<ActivosDeclaracion>(
                    sql,
                    new { id = id_declara });

                if (resp.Result is null)
                {
                    resp.Code        = -2;
                    resp.Description = "Declaración no encontrada.";
                }
                else
                {
                    resp.Description = "Ok";
                }
            }
            catch (Exception ex)
            {
                resp.Code        = -1;
                resp.Description = ex.Message;
                resp.Result      = null;
            }

            return resp;
        }

        public ErrorDto Activos_Declaraciones_Registro_Existe_Obtener(
            int CodEmpresa,
            int id_declara)
        {
            var resp = new ErrorDto { Code = 0, Description = "Ok" };

            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);

                var existe = connection.QueryFirstOrDefault<int>(
                    "SELECT TOP (1) 1 FROM vActivos_Declara WHERE ID_DECLARA = @id",
                    new { id = id_declara });

                (resp.Code, resp.Description) = (existe == 0)
                    ? (0,  "DECLARACIÓN: Libre!")
                    : (-2, "DECLARACIÓN: Ocupada!");
            }
            catch (Exception ex)
            {
                resp.Code        = -1;
                resp.Description = ex.Message;
            }

            return resp;
        }

        public ErrorDto<ActivosDeclaracionResult> Activos_Declaraciones_Registro_Guardar(
            int CodEmpresa,
            ActivosDeclaracionGuardarRequest data)
        {
            if (data == null)
                return ErrorResult(-1, "Datos no proporcionados.");

            var errores = ValidarDeclaracion(data);
            if (errores.Count > 0)
                return ErrorResult(-1, string.Join(" | ", errores));

            if (data.isNew)
                return GuardarNuevaDeclaracion(CodEmpresa, data);

            return ActualizarDeclaracionExistente(CodEmpresa, data);
        }

        private static List<string> ValidarDeclaracion(ActivosDeclaracionGuardarRequest data)
        {
            var errores = new List<string>();

            if (string.IsNullOrWhiteSpace(data.tipo))
                errores.Add("No ha indicado el tipo de registro.");

            if (string.IsNullOrWhiteSpace(data.notas) || data.notas.Trim().Length < 3)
                errores.Add("No ha indicado una nota válida para esta declaración.");

            if (string.IsNullOrWhiteSpace(data.fecha_inicio))
                errores.Add("No ha indicado la fecha de inicio.");

            if (string.IsNullOrWhiteSpace(data.fecha_corte))
                errores.Add("No ha indicado la fecha de corte.");

            if (!string.IsNullOrWhiteSpace(data.fecha_inicio) &&
                !string.IsNullOrWhiteSpace(data.fecha_corte))
            {
                var fi = DateTime.Parse(
                    data.fecha_inicio.Split(' ')[0],
                    System.Globalization.CultureInfo.InvariantCulture);
                var fc = DateTime.Parse(
                    data.fecha_corte.Split(' ')[0],
                    System.Globalization.CultureInfo.InvariantCulture);

                if (fi >= fc)
                    errores.Add("Verifique el rango de fechas (Inicio debe ser menor que Corte).");
            }

            return errores;
        }

        private ErrorDto<ActivosDeclaracionResult> GuardarNuevaDeclaracion(
            int CodEmpresa,
            ActivosDeclaracionGuardarRequest data)
        {
            if (data.id_declara > 0)
            {
                var exi = Activos_Declaraciones_Registro_Existe_Obtener(CodEmpresa, data.id_declara);
                if (exi.Code == -2)
                    return ErrorResult(-2, $"La declaración {data.id_declara} ya existe.");
            }

            return Activos_Declaraciones_Registro_Insertar(CodEmpresa, data);
        }

        private ErrorDto<ActivosDeclaracionResult> ActualizarDeclaracionExistente(
            int CodEmpresa,
            ActivosDeclaracionGuardarRequest data)
        {
            if (data.id_declara <= 0)
                return ErrorResult(-2, "No se indicó una declaración válida para modificar.");

            var exi = Activos_Declaraciones_Registro_Existe_Obtener(CodEmpresa, data.id_declara);
            if (exi.Code == 0)
                return ErrorResult(-2, $"La declaración {data.id_declara} no existe.");

            var upd = Activos_Declaraciones_Registro_Actualizar(CodEmpresa, data);
            return new ErrorDto<ActivosDeclaracionResult>
            {
                Code        = upd.Code,
                Description = upd.Description,
                Result      = new ActivosDeclaracionResult { id_declara = data.id_declara }
            };
        }

        private ErrorDto<ActivosDeclaracionResult> Activos_Declaraciones_Registro_Insertar(
            int CodEmpresa,
            ActivosDeclaracionGuardarRequest data)
        {
            var resp = new ErrorDto<ActivosDeclaracionResult>
            {
                Code        = 0,
                Description = "",
                Result      = null
            };

            try
            {
                var (pass, mensaje, idDeclara) =
                    EjecutarSpDeclaraMainAdd(CodEmpresa, declaraId: 0, data);

                if (pass != 1)
                    return new ErrorDto<ActivosDeclaracionResult>
                    {
                        Code        = -2,
                        Description = mensaje,
                        Result      = null
                    };

                if (idDeclara > 0)
                    data.id_declara = idDeclara;

                var detalle =
                    $"Declaración de Activo Id: {data.id_declara}, Inicio: {data.fecha_inicio}, Corte: {data.fecha_corte}, Tipo: {data.tipo}";

                RegistrarBitacoraDeclaracion(
                    CodEmpresa,
                    data.usuario ?? "",
                    data.id_declara,
                    movimiento: "Registra - WEB",
                    detalleExtra: detalle);

                resp.Code        = 0;
                resp.Description = string.IsNullOrWhiteSpace(mensaje)
                    ? "Declaración registrada satisfactoriamente!"
                    : mensaje;
                resp.Result = new ActivosDeclaracionResult { id_declara = data.id_declara };
            }
            catch (Exception ex)
            {
                resp.Code        = -1;
                resp.Description = ex.Message;
                resp.Result      = null;
            }

            return resp;
        }

        private ErrorDto Activos_Declaraciones_Registro_Actualizar(
            int CodEmpresa,
            ActivosDeclaracionGuardarRequest data)
        {
            var resp = new ErrorDto { Code = 0, Description = "" };

            try
            {
                var (pass, mensaje, _) =
                    EjecutarSpDeclaraMainAdd(CodEmpresa, declaraId: data.id_declara, data);

                if (pass != 1)
                    return new ErrorDto { Code = -2, Description = mensaje };

                RegistrarBitacoraDeclaracion(
                    CodEmpresa,
                    data.usuario ?? "",
                    data.id_declara,
                    movimiento: "Modifica - WEB");

                resp.Description = string.IsNullOrWhiteSpace(mensaje)
                    ? "Declaración actualizada satisfactoriamente!"
                    : mensaje;
            }
            catch (Exception ex)
            {
                resp.Code        = -1;
                resp.Description = ex.Message;
            }

            return resp;
        }

        public ErrorDto Activos_Declaraciones_Registro_Eliminar(
            int CodEmpresa,
            int id_declara,
            string usuario)
        {
            var resp = new ErrorDto { Code = 0, Description = "Ok" };

            try
            {
                var (pass, mensaje) = EjecutarSpDeclaraAccion(
                    CodEmpresa,
                    storedProcedure: "spActivos_Declara_Main_Delete",
                    id_declara,
                    usuario,
                    defaultErrorMessage: "Error");

                if (pass != 1)
                    return new ErrorDto { Code = -2, Description = mensaje };

                RegistrarBitacoraDeclaracion(
                    CodEmpresa,
                    usuario,
                    id_declara,
                    movimiento: "Elimina - WEB");
            }
            catch (Exception ex)
            {
                resp.Code        = -1;
                resp.Description = ex.Message;
            }

            return resp;
        }

        public ErrorDto Activos_Declaraciones_Registro_Cerrar(
            int CodEmpresa,
            int id_declara,
            string usuario)
        {
            var resp = new ErrorDto { Code = 0, Description = "Ok" };

            try
            {
                var (pass, mensaje) = EjecutarSpDeclaraAccion(
                    CodEmpresa,
                    storedProcedure: "spActivos_Declara_Main_Cierra",
                    id_declara,
                    usuario,
                    defaultErrorMessage: "Error");

                if (pass != 1)
                    return new ErrorDto { Code = -2, Description = mensaje };

                RegistrarBitacoraDeclaracion(
                    CodEmpresa,
                    usuario,
                    id_declara,
                    movimiento: "Cierra - WEB");

                resp.Description = string.IsNullOrWhiteSpace(mensaje)
                    ? "Declaración cerrada satisfactoriamente!"
                    : mensaje;
            }
            catch (Exception ex)
            {
                resp.Code        = -1;
                resp.Description = ex.Message;
            }

            return resp;
        }

        public ErrorDto Activos_Declaraciones_Registro_Procesar(
            int CodEmpresa,
            int id_declara,
            string usuario)
        {
            var resp = new ErrorDto { Code = 0, Description = "Ok" };

            try
            {
                var (pass, mensaje) = EjecutarSpDeclaraAccion(
                    CodEmpresa,
                    storedProcedure: "spActivos_Declara_Main_Procesa",
                    id_declara,
                    usuario,
                    defaultErrorMessage: "Error al procesar");

                if (pass != 1)
                    return new ErrorDto { Code = -2, Description = mensaje };

                RegistrarBitacoraDeclaracion(
                    CodEmpresa,
                    usuario,
                    id_declara,
                    movimiento: "Procesa - WEB");

                resp.Description = string.IsNullOrWhiteSpace(mensaje)
                    ? "Declaración procesada satisfactoriamente!"
                    : mensaje;
            }
            catch (Exception ex)
            {
                resp.Code        = -1;
                resp.Description = ex.Message;
            }

            return resp;
        }

        /// <summary>
        /// Navega entre declaraciones (anterior / siguiente) usando el Id.
        /// </summary>
        public ErrorDto<ActivosDeclaracion> Activos_Declaraciones_Registro_Scroll(
            int CodEmpresa,
            int scroll,
            int? id_declara,
            string usuario)
        {
            var resp = new ErrorDto<ActivosDeclaracion>
            {
                Code        = 0,
                Description = "",
                Result      = null
            };

            try
            {
                var connStr = GetConn(CodEmpresa);
                using var connection = new SqlConnection(connStr);

                string sqlNext = scroll == 1
                    ? @"SELECT TOP 1 ID_DECLARA 
                        FROM vActivos_Declara 
                        WHERE ID_DECLARA > @id 
                        ORDER BY ID_DECLARA ASC;"
                    : @"SELECT TOP 1 ID_DECLARA 
                        FROM vActivos_Declara 
                        WHERE ID_DECLARA < @id 
                        ORDER BY ID_DECLARA DESC;";

                var nextId = connection.QueryFirstOrDefault<int?>(
                    sqlNext,
                    new { id = id_declara ?? 0 });

                if (nextId == null)
                    return new ErrorDto<ActivosDeclaracion>
                    {
                        Code        = -2,
                        Description = "No se encontraron más resultados.",
                        Result      = null
                    };

                resp = Activos_Declaraciones_Registro_Obtener(CodEmpresa, nextId.Value);
            }
            catch (Exception ex)
            {
                resp.Code        = -1;
                resp.Description = ex.Message;
                resp.Result      = null;
            }

            return resp;
        }
    }
}