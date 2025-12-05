using System.Data;
using Dapper;
using Galileo.DataBaseTier;
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
            _portalDB = new PortalDB(config);
            _Security_MainDB = new MSecurityMainDb(config);
        }

        private string GetConn(int CodEmpresa) =>
           _portalDB.ObtenerDbConnStringEmpresa(CodEmpresa);

        /// <summary>
        /// Obtiene el historial de declaraciones por lazy loading.
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// </summary>
        /// <returns></returns>
        public ErrorDto<ActivosDeclaracionLista> Activos_Declaraciones_Lista_Obtener(int CodEmpresa, FiltrosLazyLoadData filtros)
        {
            var result = new ErrorDto<ActivosDeclaracionLista>
            {
                Code = 0,
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

                string where = " WHERE 1 = 1 ";

                if (!string.IsNullOrWhiteSpace(filtros?.filtro))
                {
                    where +=
                        " AND ( CONVERT(varchar(10), id_declara) LIKE '%" + filtros.filtro + "%' " +
                        "   OR notas LIKE '%" + filtros.filtro + "%' ) ";
                }

                var qTotal = $"SELECT COUNT(1) FROM vActivos_Declara {where}";
                result.Result.total = connection.Query<int>(qTotal).FirstOrDefault();

                if (string.IsNullOrWhiteSpace(filtros?.sortField))
                    filtros!.sortField = "id_declara";

                var orden = filtros.sortOrder == 0 ? "DESC" : "ASC";

                bool sinPaginacion = filtros.paginacion <= 0;

                var orderBy = $" ORDER BY {filtros.sortField} {orden} ";
                var paging = sinPaginacion
                    ? ""
                    : $" OFFSET {filtros.pagina} ROWS FETCH NEXT {filtros.paginacion} ROWS ONLY ";

                var qDatos = $@"
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
                    {where}
                    {orderBy}
                    {paging};";

                result.Result.lista = connection
                    .Query<ActivosDeclaracionResumen>(qDatos)
                    .ToList();
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result.total = 0;
                result.Result.lista = [];
            }

            return result;
        }

        /// <summary>
        /// Obtiene una declaración específica por Id.
        /// <param name="CodEmpresa"></param>
        /// <param name="id_declara"></param>
        /// </summary>
        /// <returns></returns>
        public ErrorDto<ActivosDeclaracion> Activos_Declaraciones_Registro_Obtener(int CodEmpresa, int id_declara)
        {
            var connStr = GetConn(CodEmpresa);
            var resp = new ErrorDto<ActivosDeclaracion>
            {
                Code = 0,
                Description = "",
                Result = null
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
                using var connection = new SqlConnection(connStr);
                resp.Result = connection.QueryFirstOrDefault<ActivosDeclaracion>(sql, new { id = id_declara });

                if (resp.Result is null)
                {
                    resp.Code = -2;
                    resp.Description = "Declaración no encontrada.";
                }
                else
                {
                    resp.Description = "Ok";
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
                resp.Result = null;
            }

            return resp;
        }

        /// <summary>
        /// Verifica si existe una declaración (para validar números manuales).
        /// <param name="CodEmpresa"></param>
        /// <param name="id_declara"></param>
        /// </summary>
        /// <returns></returns>
        public ErrorDto Activos_Declaraciones_Registro_Existe_Obtener(int CodEmpresa, int id_declara)
        {
            var resp = new ErrorDto { Code = 0, Description = "Ok" };

            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);

                var existe = connection.QueryFirstOrDefault<int>(
                    "SELECT TOP (1) 1 FROM vActivos_Declara WHERE ID_DECLARA = @id",
                    new { id = id_declara });

                (resp.Code, resp.Description) = (existe == 0)
                    ? (0, "DECLARACIÓN: Libre!")
                    : (-2, "DECLARACIÓN: Ocupada!");
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }

            return resp;
        }

        /// <summary>
        /// Guarda una declaración de activos (insertar o actualizar).
        /// <param name="CodEmpresa"></param>
        /// <param name="data"></param>
        /// </summary>
        /// <returns></returns>
        public ErrorDto<ActivosDeclaracionResult> Activos_Declaraciones_Registro_Guardar(int CodEmpresa, ActivosDeclaracionGuardarRequest data)
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
                var fi = DateTime.Parse(data.fecha_inicio.Split(' ')[0], System.Globalization.CultureInfo.InvariantCulture);
                var fc = DateTime.Parse(data.fecha_corte.Split(' ')[0], System.Globalization.CultureInfo.InvariantCulture);
                if (fi >= fc)
                    errores.Add("Verifique el rango de fechas (Inicio debe ser menor que Corte).");
            }

            return errores;
        }

        private ErrorDto<ActivosDeclaracionResult> GuardarNuevaDeclaracion(int CodEmpresa, ActivosDeclaracionGuardarRequest data)
        {
            if (data.id_declara > 0)
            {
                var exi = Activos_Declaraciones_Registro_Existe_Obtener(CodEmpresa, data.id_declara);
                if (exi.Code == -2)
                    return ErrorResult(-2, $"La declaración {data.id_declara} ya existe.");
            }

            return Activos_Declaraciones_Registro_Insertar(CodEmpresa, data);
        }

        private ErrorDto<ActivosDeclaracionResult> ActualizarDeclaracionExistente(int CodEmpresa, ActivosDeclaracionGuardarRequest data)
        {
            if (data.id_declara <= 0)
                return ErrorResult(-2, "No se indicó una declaración válida para modificar.");

            var exi = Activos_Declaraciones_Registro_Existe_Obtener(CodEmpresa, data.id_declara);
            if (exi.Code == 0)
                return ErrorResult(-2, $"La declaración {data.id_declara} no existe.");

            var upd = Activos_Declaraciones_Registro_Actualizar(CodEmpresa, data);
            return new ErrorDto<ActivosDeclaracionResult>
            {
                Code = upd.Code,
                Description = upd.Description,
                Result = new ActivosDeclaracionResult { id_declara = data.id_declara }
            };
        }

        private ErrorDto<ActivosDeclaracionResult> ErrorResult(int code, string description)
        {
            return new ErrorDto<ActivosDeclaracionResult>
            {
                Code = code,
                Description = description,
                Result = null
            };
        }

        /// <summary>
        /// Inserta una nueva declaración de activos.
        /// <param name="CodEmpresa"></param>
        /// <param name="data"></param>
        /// </summary>
        /// <returns></returns>
        private ErrorDto<ActivosDeclaracionResult> Activos_Declaraciones_Registro_Insertar(int CodEmpresa, ActivosDeclaracionGuardarRequest data)
        {
            var resp = new ErrorDto<ActivosDeclaracionResult>
            {
                Code = 0,
                Description = "",
                Result = null
            };

            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);
                var p = new DynamicParameters();
                p.Add(_declaraId, 0);
                p.Add("@Notas", data.notas);
                p.Add("@Tipo", data.tipo); 
                p.Add("@Inicio", data.fecha_inicio);
                p.Add("@Corte", data.fecha_corte);    
                p.Add(_usuario, data.usuario);

                var rs = connection.QueryFirstOrDefault<dynamic>(
                    "spActivos_Declara_Main_Add",
                    p,
                    commandType: CommandType.StoredProcedure);

                int pass = (int)(rs?.Pass ?? 0);
                string mensaje = (string)(rs?.Mensaje ?? "Error al insertar");
                int idDeclara = (int)(rs?.ID_DECLARA ?? 0);

                if (pass != 1)
                    return new ErrorDto<ActivosDeclaracionResult>
                    {
                        Code = -2,
                        Description = mensaje,
                        Result = null
                    };

                if (idDeclara > 0)
                    data.id_declara = idDeclara;

                _Security_MainDB.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = data.usuario ?? "",
                    DetalleMovimiento = $"Declaración de Activo Id: {data.id_declara}, Inicio: {data.fecha_inicio}, Corte: {data.fecha_corte}, Tipo: {data.tipo}",
                    Movimiento = "Registra - WEB",
                    Modulo = vModulo
                });

                resp.Code = 0;
                resp.Description = string.IsNullOrWhiteSpace(mensaje)
                    ? "Declaración registrada satisfactoriamente!"
                    : mensaje;
                resp.Result = new ActivosDeclaracionResult { id_declara = data.id_declara };
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
                resp.Result = null;
            }

            return resp;
        }

        /// <summary>
        /// Actualiza una declaración de activos existente.
        /// <param name="CodEmpresa"></param>
        /// <param name="data"></param>
        /// </summary>
        /// <returns></returns>
        private ErrorDto Activos_Declaraciones_Registro_Actualizar(int CodEmpresa, ActivosDeclaracionGuardarRequest data)
        {
            var resp = new ErrorDto { Code = 0, Description = "" };

            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);
                var p = new DynamicParameters();
                p.Add(_declaraId, data.id_declara);
                p.Add("@Notas", data.notas);
                p.Add("@Tipo", data.tipo);
                p.Add("@Inicio", data.fecha_inicio);
                p.Add("@Corte", data.fecha_corte);
                p.Add(_usuario, data.usuario);

                var rs = connection.QueryFirstOrDefault<dynamic>(
                    "spActivos_Declara_Main_Add",
                    p,
                    commandType: CommandType.StoredProcedure);

                int pass = (int)(rs?.Pass ?? 0);
                string mensaje = (string)(rs?.Mensaje ?? "Error");

                if (pass != 1)
                    return new ErrorDto { Code = -2, Description = mensaje };

                _Security_MainDB.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = data.usuario ?? "",
                    DetalleMovimiento = $"Declaración de Activo Id: {data.id_declara}",
                    Movimiento = "Modifica - WEB",
                    Modulo = vModulo
                });

                resp.Description = string.IsNullOrWhiteSpace(mensaje)
                    ? "Declaración actualizada satisfactoriamente!"
                    : mensaje;
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }

            return resp;
        }

        /// <summary>
        /// Elimina una declaración de activos (si no tiene registros).
        /// <param name="CodEmpresa"></param>
        /// <param name="id_declara"></param>
        /// <param name="usuario"></param>
        /// </summary>
        /// <returns></returns>
        public ErrorDto Activos_Declaraciones_Registro_Eliminar(int CodEmpresa, int id_declara, string usuario)
        {
            var connStr = GetConn(CodEmpresa);
            var resp = new ErrorDto { Code = 0, Description = "Ok" };

            try
            {
                using var connection = new SqlConnection(connStr);
                var p = new DynamicParameters();
                p.Add(_declaraId, id_declara);
                p.Add(_usuario, usuario);

                var rs = connection.QueryFirstOrDefault<dynamic>(
                    "spActivos_Declara_Main_Delete",
                    p,
                    commandType: CommandType.StoredProcedure);

                int pass = (int)(rs?.Pass ?? 0);
                string mensaje = (string)(rs?.Mensaje ?? "Error");

                if (pass != 1)
                    return new ErrorDto { Code = -2, Description = mensaje };

                _Security_MainDB.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = usuario ?? "",
                    DetalleMovimiento = $"Declaración de Activo Id: {id_declara}",
                    Movimiento = "Elimina - WEB",
                    Modulo = vModulo
                });
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }

            return resp;
        }

        /// <summary>
        /// Cierra una declaración de activos (cambia a estado C).
        /// <param name="CodEmpresa"></param>
        /// <param name="id_declara"></param>
        /// <param name="usuario"></param>
        /// </summary>
        /// <returns></returns>
        public ErrorDto Activos_Declaraciones_Registro_Cerrar(int CodEmpresa, int id_declara, string usuario)
        {
            var connStr = GetConn(CodEmpresa);
            var resp = new ErrorDto { Code = 0, Description = "Ok" };

            try
            {
                using var connection = new SqlConnection(connStr);
                var p = new DynamicParameters();
                p.Add(_declaraId, id_declara);
                p.Add(_usuario, usuario);

                var rs = connection.QueryFirstOrDefault<dynamic>(
                    "spActivos_Declara_Main_Cierra",
                    p,
                    commandType: CommandType.StoredProcedure);

                int pass = (int)(rs?.Pass ?? 0);
                string mensaje = (string)(rs?.Mensaje ?? "Error");

                if (pass != 1)
                    return new ErrorDto { Code = -2, Description = mensaje };

                _Security_MainDB.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = usuario ?? "",
                    DetalleMovimiento = $"Declaración de Activo Id: {id_declara}",
                    Movimiento = "Cierra - WEB",
                    Modulo = vModulo
                });

                resp.Description = string.IsNullOrWhiteSpace(mensaje)
                    ? "Declaración cerrada satisfactoriamente!"
                    : mensaje;
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }

            return resp;
        }

        /// <summary>
        /// Procesa una declaración de activos (cambia a estado P).
        /// <param name="CodEmpresa"></param>
        /// <param name="id_declara"></param>
        /// <param name="usuario"></param>
        /// </summary>
        /// <returns></returns>
        public ErrorDto Activos_Declaraciones_Registro_Procesar(int CodEmpresa, int id_declara, string usuario)
        {
            var connStr = GetConn(CodEmpresa);
            var resp = new ErrorDto { Code = 0, Description = "Ok" };

            try
            {
                using var connection = new SqlConnection(connStr);
                var p = new DynamicParameters();
                p.Add(_declaraId, id_declara);
                p.Add(_usuario, usuario);

                var rs = connection.QueryFirstOrDefault<dynamic>(
                    "spActivos_Declara_Main_Procesa",
                    p,
                    commandType: CommandType.StoredProcedure);

                int pass = (int)(rs?.Pass ?? 0);
                string mensaje = (string)(rs?.Mensaje ?? "Error al procesar");

                if (pass != 1)
                    return new ErrorDto { Code = -2, Description = mensaje };

                _Security_MainDB.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = usuario ?? "",
                    DetalleMovimiento = $"Declaración de Activo Id: {id_declara}",
                    Movimiento = "Procesa - WEB",
                    Modulo = vModulo
                });

                resp.Description = string.IsNullOrWhiteSpace(mensaje)
                    ? "Declaración procesada satisfactoriamente!"
                    : mensaje;
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }

            return resp;
        }

        /// <summary>
        /// Navega entre declaraciones (anterior / siguiente) usando el Id.
        /// <param name="CodEmpresa"></param>
        /// <param name="scroll"></param>
        /// <param name="id_declara"></param>
        /// <param name="usuario"></param>
        /// </summary>
        /// <returns></returns>
        public ErrorDto<ActivosDeclaracion> Activos_Declaraciones_Registro_Scroll(int CodEmpresa, int scroll, int? id_declara, string usuario)
        {
            var connStr = GetConn(CodEmpresa);
            var resp = new ErrorDto<ActivosDeclaracion>
            {
                Code = 0,
                Description = "",
                Result = null
            };

            try
            {
                string whereOrder = (scroll == 1)
                    ? " WHERE ID_DECLARA > @id ORDER BY ID_DECLARA ASC "
                    : " WHERE ID_DECLARA < @id ORDER BY ID_DECLARA DESC ";

                using var connection = new SqlConnection(connStr);
                var nextId = connection.QueryFirstOrDefault<int?>(
                    $"SELECT TOP 1 ID_DECLARA FROM vActivos_Declara {whereOrder}",
                    new { id = id_declara ?? 0 });

                if (nextId == null)
                    return new ErrorDto<ActivosDeclaracion>
                    {
                        Code = -2,
                        Description = "No se encontraron más resultados.",
                        Result = null
                    };

                resp = Activos_Declaraciones_Registro_Obtener(CodEmpresa, nextId.Value);
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
                resp.Result = null;
            }

            return resp;
        }
    }
}
