using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.Security;
using Galileo_API.Models.ProGrX.Cobros;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Linq;
namespace Galileo_API.DataBaseTier.ProGrX.Cobros
{
    public class FrmCOControlUsuariosDB
    {
        private readonly PortalDB _portalDB;
        private readonly MSecurityMainDb _securityMainDb;
        private const int vModulo = 4;
        private const string USUARIO_INVALIDO = "Usuario inválido.";
        private const string USUARIO_SESION_INVALIDO = "Usuario de sesión inválido.";
        private const string USUARIO = "@Usuario";
        private const string DESCRIPCION = "Descripcion";
        private const string DESCRIPCION_MINUS = "descripcion";
        private const string DESCRIPCION_MAYUS = "DESCRIPCION";
        public FrmCOControlUsuariosDB(IConfiguration config)
        {
            _portalDB = new PortalDB(config);
            _securityMainDb = new MSecurityMainDb(config);
        }
        /// <summary>
        /// Obtener usuario de cobro.
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// </summary>
        /// <returns></returns>
        public ErrorDto<CoControlUsuariosData> CO_Usuarios_Obtener(int CodEmpresa, string usuario)
        {
            if (string.IsNullOrWhiteSpace(usuario))
                return DbHelper.CreateErrorResponse<CoControlUsuariosData>(USUARIO_INVALIDO, -2);

            var pUsuario = usuario.Trim();

            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);
            try
            {
                const string sql = @"
                SELECT 
                    RTRIM(U.usuario) AS usuario,
                    ISNULL(RTRIM(U.cedula),'') AS cedula,
                    ISNULL(RTRIM(U.nombre),'') AS nombre,
                    ISNULL(U.estado,0) AS estado,
                    ISNULL(U.aplica_comision,0) AS aplica_comision,
                    ISNULL(U.Operador_Externo,0) AS operador_externo,
                    ISNULL(U.porc_comision,0) AS porc_comision,
                    ISNULL(U.tiempo_resolucion_com,0) AS tiempo_resolucion_com,
                    ISNULL(U.cod_banco,0) AS cod_banco,
                    ISNULL(U.tipo_documento,'') AS tipo_documento,
                    ISNULL(CONVERT(varchar(19),U.registro_fecha,120),'') AS registro_fecha,
                    ISNULL(U.registro_usuario,'') AS registro_usuario,
                    ISNULL(CONVERT(varchar(19),U.modifica_fecha,120),'') AS modifica_fecha,
                    ISNULL(U.modifica_usuario,'') AS modifica_usuario,
                    ISNULL(RTRIM(B.Descripcion),'') AS banco_desc
                FROM cbr_usuarios U
                LEFT JOIN tes_Bancos B ON U.Cod_Banco = B.id_Banco
                WHERE U.usuario = @usuario;";

                var row = conn.QueryFirstOrDefault<CoControlUsuariosData>(sql, new { usuario = pUsuario })
                          ?? new CoControlUsuariosData();

                return DbHelper.CreateOkResponse(row);
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<CoControlUsuariosData>(ex.Message);
            }
        }
        /// <summary>
        /// Scroll usuarios: 1=siguiente (mayor), 2=anterior (menor)
        /// <param name="CodEmpresa"></param>
        /// <param name="scrollCode"></param>
        /// <param name="usuarioActual"></param> 
        /// </summary>
        /// <returns></returns>
        public ErrorDto<CoControlUsuariosData> CO_Usuarios_Scroll_Obtener(int CodEmpresa, int scrollCode, string? usuarioActual)
        {
            var pActual = (usuarioActual ?? string.Empty).Trim();

            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);
            try
            {
                string next = string.Empty;

                if (string.IsNullOrWhiteSpace(pActual))
                {
                    const string sqlFirst = @"
                    SELECT TOP 1 usuario
                    FROM cbr_usuarios
                    ORDER BY
                        CASE WHEN @scroll = 1 THEN usuario END ASC,
                        CASE WHEN @scroll <> 1 THEN usuario END DESC;";

                    next = (conn.QueryFirstOrDefault<string>(sqlFirst, new
                    {
                        scroll = scrollCode
                    }) ?? "").Trim();
                }

                else
                {
                    const string sql = @"
            SELECT TOP 1 usuario
            FROM cbr_usuarios
            WHERE
                  (@scroll = 1 AND usuario > @actual)
               OR (@scroll <> 1 AND usuario < @actual)
            ORDER BY
                CASE WHEN @scroll = 1 THEN usuario END ASC,
                CASE WHEN @scroll <> 1 THEN usuario END DESC;";

                    next = (conn.QueryFirstOrDefault<string>(sql, new
                    {
                        scroll = scrollCode,
                        actual = pActual
                    }) ?? "").Trim();

                    if (string.IsNullOrWhiteSpace(next))
                        next = pActual;
                }

                if (string.IsNullOrWhiteSpace(next))
                    return DbHelper.CreateErrorResponse<CoControlUsuariosData>(
                        "No hay usuarios para navegar.", -2);

                return CO_Usuarios_Obtener(CodEmpresa, next);
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<CoControlUsuariosData>(ex.Message);
            }
        }
        /// <summary>
        /// Verifica que el usuario exista.
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// </summary>
        /// <returns></returns>
        public ErrorDto CO_Usuarios_Existe_Obtener(int CodEmpresa, string usuario)
        {
            var resp = DbHelper.CreateOkResponse();

            if (string.IsNullOrWhiteSpace(usuario))
            {
                resp.Code = -2;
                resp.Description = USUARIO_INVALIDO;
                return resp;
            }

            var pUsuario = usuario.Trim().ToUpperInvariant();

            using var cn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                const string sql = @"
                SELECT COUNT(1)
                FROM dbo.cbr_usuarios
                WHERE UPPER(RTRIM(usuario)) = @usr;";

                var count = cn.QueryFirstOrDefault<int>(sql, new { usr = pUsuario });
                (resp.Code, resp.Description) =
                    count == 0 ? (0, "USUARIO: Libre!") : (-2, "USUARIO: Ocupado!");

                return resp;
            }
            catch (SqlException ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
                return resp;
            }
        }
        /// <summary>
        /// F4 Usuarios (usuario/cedula/nombre)
        /// <param name="CodEmpresa"></param>
        /// <param name="filtro"></param>
        /// </summary>
        /// <returns></returns>
        public ErrorDto<CoControlUsuariosListaResult<CoControlUsuariosF4Item>> CO_Usuarios_F4_Obtener(int CodEmpresa, string? filtro)
        {
            var texto = (filtro ?? string.Empty).Trim();
            var hasFiltro = !string.IsNullOrWhiteSpace(texto);
            var like = hasFiltro ? $"%{texto}%" : null;

            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            var response = new ErrorDto<CoControlUsuariosListaResult<CoControlUsuariosF4Item>>
            {
                Code = 0,
                Description = "Ok",
                Result = new CoControlUsuariosListaResult<CoControlUsuariosF4Item>()
            };
            response.Result ??= new CoControlUsuariosListaResult<CoControlUsuariosF4Item>();

            try
            {
                const string sql = @"
                SELECT
                    RTRIM(usuario) AS usuario,
                    ISNULL(RTRIM(cedula),'') AS cedula,
                    ISNULL(RTRIM(nombre),'') AS nombre
                FROM cbr_usuarios
                WHERE
                    (@filtro IS NULL)
                 OR (usuario LIKE @like)
                 OR (cedula LIKE @like)
                 OR (nombre LIKE @like)
                ORDER BY nombre;";

                var lista = conn.Query<CoControlUsuariosF4Item>(sql, new
                {
                    filtro = hasFiltro ? texto : null,
                    like
                }).ToList();

                response.Result.total = lista.Count;
                response.Result.lista = lista;
                return response;
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<CoControlUsuariosListaResult<CoControlUsuariosF4Item>>(ex.Message);
            }
        }
        /// <summary>
        /// Dropdown de bancos para cboBancos (spCrd_SGT_Bancos)
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario_sesion"></param> 
        /// </summary>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CO_Bancos_Dropdown_Obtener(int CodEmpresa, string usuario_sesion)
        {
            if (string.IsNullOrWhiteSpace(usuario_sesion))
                return DbHelper.CreateErrorResponse<List<DropDownListaGenericaModel>>(USUARIO_SESION_INVALIDO, -2);

            var pUsuario = usuario_sesion.Trim();

            return DbHelper.WithConn(_portalDB, CodEmpresa, conn =>
            {
                var p = new DynamicParameters();
                p.Add(USUARIO, pUsuario, DbType.String);

                var rows = conn.Query("dbo.spCrd_SGT_Bancos", p, commandType: CommandType.StoredProcedure);
                var itemKeys = new[] { "item", "ID_BANCO", "COD_BANCO", "id_banco", "cod_banco" };
                var descKeys = new[] { DESCRIPCION_MINUS, DESCRIPCION_MAYUS, DESCRIPCION };

                var lista = new List<DropDownListaGenericaModel>();

                foreach (var d in rows.Cast<IDictionary<string, object?>>())
                {
                    var item = GetFirstNonEmpty(d, itemKeys);
                    if (string.IsNullOrWhiteSpace(item)) continue;

                    var desc = GetFirstNonEmpty(d, descKeys);

                    lista.Add(new DropDownListaGenericaModel
                    {
                        item = item,
                        descripcion = desc
                    });
                }

                return lista;
            });
        }
        /// <summary>
        /// Lista cuentas bancarias por identificación.
        /// <param name="CodEmpresa"></param>
        /// <param name="cedula"></param>
        /// <param name="filtro"></param>
        /// </summary>
        /// <returns></returns>
        public ErrorDto<CoControlUsuariosListaResult<CoControlUsuariosCuentasData>> CO_Usuarios_Cuentas_Lista_Obtener(int CodEmpresa, string cedula, string? filtro)
        {
            return CO_Usuarios_Cuentas_Listar_Core(CodEmpresa, cedula);
        }
        /// <summary>
        /// Export cuentas bancarias por identificación.
        /// <param name="CodEmpresa"></param>
        /// <param name="cedula"></param>
        /// <param name="filtro"></param>
        /// </summary>
        /// <returns></returns>
        public ErrorDto<CoControlUsuariosListaResult<CoControlUsuariosCuentasData>> CO_Usuarios_Cuentas_Lista_Export(int CodEmpresa, string cedula, string? filtro)
        {
            return CO_Usuarios_Cuentas_Listar_Core(CodEmpresa, cedula);
        }
        /// <summary>
        /// // Core para listar cuentas bancarias por identificación.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="cedula"></param>
        /// <returns></returns>
        private ErrorDto<CoControlUsuariosListaResult<CoControlUsuariosCuentasData>> CO_Usuarios_Cuentas_Listar_Core(int CodEmpresa, string cedula)
        {
            if (string.IsNullOrWhiteSpace(cedula))
                return DbHelper.CreateErrorResponse<CoControlUsuariosListaResult<CoControlUsuariosCuentasData>>(
                    "Identificación inválida.", -2);

            var pCedula = cedula.Trim();

            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            var response = BuildListaResponse<CoControlUsuariosCuentasData>();

            try
            {
                const string sql = @"
                select 
                    RTRIM(B.Descripcion) as Banco,
                    case when C.tipo = 'A' then 'Ahorros' else 'Corriente' end as TipoDesc,
                    C.cod_Divisa,
                    C.CUENTA_INTERNA,
                    C.CUENTA_INTERBANCA,
                    C.ACTIVA,
                    ISNULL(C.DESTINO,'') as DESTINO,
                    ISNULL(CONVERT(varchar(19),C.REGISTRO_FECHA,120),'') as REGISTRO_FECHA,
                    ISNULL(C.REGISTRO_USUARIO,'') as REGISTRO_USUARIO
                from SYS_CUENTAS_BANCARIAS C
                inner join TES_BANCOS_GRUPOS B on C.cod_banco = B.cod_grupo
                where C.Identificacion = @cedula;";

                var rows = conn.Query(sql, new { cedula = pCedula });

                var lista = rows
                    .Cast<IDictionary<string, object?>>()
                    .Select(MapCuenta)
                    .ToList();

                response.Result!.total = lista.Count;
                response.Result!.lista = lista;
                return response;
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<CoControlUsuariosListaResult<CoControlUsuariosCuentasData>>(ex.Message);
            }
        }

        /// <summary>
        /// Lista de grupos (spCbr_Usuarios_Grupos_List)
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// </summary>
        /// <returns></returns>
        public ErrorDto<CoControlUsuariosListaResult<CoControlUsuariosGrupoItem>> CO_Usuarios_Grupos_Lista_Obtener(int CodEmpresa, string usuario)
        {
            if (string.IsNullOrWhiteSpace(usuario))
                return DbHelper.CreateErrorResponse<CoControlUsuariosListaResult<CoControlUsuariosGrupoItem>>(USUARIO_INVALIDO, -2);

            var pUsuario = usuario.Trim();

            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            return EjecutarListaSP(conn, "dbo.spCbr_Usuarios_Grupos_List", pUsuario, d =>
                new CoControlUsuariosGrupoItem
                {
                    id_grupo = I(V(d, "ID_GRUPO", "id_grupo", "Id_Grupo")),
                    descripcion = S(V(d, DESCRIPCION, DESCRIPCION_MAYUS, DESCRIPCION_MINUS)),
                    asignado = B01(V(d, "asignado", "ASIGNADO", "Asignado")),
                }
            );
        }
        /// <summary>
        /// Lista de carteras.
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// </summary>
        /// <returns></returns>
        public ErrorDto<CoControlUsuariosListaResult<CoControlUsuariosCarteraItem>> CO_Usuarios_Carteras_Lista_Obtener(int CodEmpresa, string usuario)
        {
            if (string.IsNullOrWhiteSpace(usuario))
                return DbHelper.CreateErrorResponse<CoControlUsuariosListaResult<CoControlUsuariosCarteraItem>>(USUARIO_INVALIDO, -2);

            var pUsuario = usuario.Trim();

            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            return EjecutarListaSP(conn, "dbo.spCbr_Usuarios_Carteras_List", pUsuario, d =>
                new CoControlUsuariosCarteraItem
                {
                    cod_clasificacion = S(V(d, "COD_CLASIFICACION", "cod_clasificacion", "Cod_Clasificacion")),
                    descripcion = S(V(d, DESCRIPCION, DESCRIPCION_MAYUS, DESCRIPCION_MINUS)),
                    asignado = B01(V(d, "asignado", "ASIGNADO", "Asignado")),
                }
            );
        }
        /// <summary>
        /// Guardar usuario (insert/update cbr_usuarios)
        /// <param name="CodEmpresa"></param>
        /// <param name="req"></param>
        /// </summary>
        /// <returns></returns>
        public ErrorDto CO_Usuarios_Guardar(int CodEmpresa, CoControlUsuariosGuardarRequest req)
        {
            var err = ValidarGuardar(req);
            if (err != null) return err;

            var user = req.usuario!.Trim();
            var userSesion = req.usuario_sesion!.Trim();

            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);
            try
            {
                if (req.edita.GetValueOrDefault(false))
                {
                    EjecutarUpdate(conn, user, userSesion, req);
                    RegistrarBitacora(CodEmpresa, userSesion,
                        $"Cobros > Control Usuarios > Modifica usuario: {user}", "Modifica - WEB");
                    return DbHelper.CreateOkResponse();
                }

                EjecutarInsert(conn, user, userSesion, req);
                RegistrarBitacora(CodEmpresa, userSesion,
                    $"Cobros > Control Usuarios > Registra usuario: {user}", "Registra - WEB");
                return DbHelper.CreateOkResponse();
            }
            catch (BusinessException bex)
            {
                return DbHelper.ErrorResponse(bex.Message, bex.Code);
            }
            catch (SqlException ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }
        /// <summary>
        /// Asignar/Desasignar grupo.
        /// <param name="CodEmpresa"></param>
        /// <param name="req"></param>
        /// </summary>
        /// <returns></returns>
        public ErrorDto CO_Usuarios_Grupos_Asignar(int CodEmpresa, CoControlUsuariosAsignacionRequest req)
        {
            if (req == null)
                return DbHelper.ErrorResponse("Solicitud inválida.", -2);

            if (string.IsNullOrWhiteSpace(req.usuario))
                return DbHelper.ErrorResponse(USUARIO_INVALIDO, -2);

            if (req.id_grupo == null || req.id_grupo <= 0)
                return DbHelper.ErrorResponse("Grupo inválido.", -2);

            if (string.IsNullOrWhiteSpace(req.usuario_sesion))
                return DbHelper.ErrorResponse(USUARIO_SESION_INVALIDO, -2);

            var user = req.usuario.Trim();
            var userSesion = req.usuario_sesion.Trim();
            var accion = req.asignar == true ? "A" : "E";
            accion = accion == "A" ? "A" : "E";

            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);
            try
            {
                var p = new DynamicParameters();
                p.Add("@UsuarioList", user, DbType.String);
                p.Add("@GrupoId", req.id_grupo.Value, DbType.Int32);
                p.Add(USUARIO, userSesion, DbType.String);
                p.Add("@Mov", accion, DbType.AnsiStringFixedLength);

                conn.Execute("dbo.spCbr_Usuarios_Grupos_Add", p, commandType: CommandType.StoredProcedure);

                _securityMainDb.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = userSesion,
                    DetalleMovimiento = $"Cobros > Control Usuarios > Grupo {req.id_grupo} => {(req.asignar == true ? "Asignar" : "Eliminar")} (Usuario {user})",
                    Movimiento = "Modifica - WEB",
                    Modulo = vModulo
                });

                return DbHelper.CreateOkResponse();
            }
            catch (SqlException ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }
        /// <summary>
        /// Asignar/Desasignar cartera.
        /// <param name="CodEmpresa"></param>
        /// <param name="req"></param>
        /// </summary>
        /// <returns></returns>
        public ErrorDto CO_Usuarios_Carteras_Asignar(int CodEmpresa, CoControlUsuariosAsignacionRequest req)
        {
            if (req == null)
                return DbHelper.ErrorResponse("Solicitud inválida.", -2);

            if (string.IsNullOrWhiteSpace(req.usuario))
                return DbHelper.ErrorResponse(USUARIO_INVALIDO, -2);

            if (string.IsNullOrWhiteSpace(req.cod_clasificacion))
                return DbHelper.ErrorResponse("Cartera inválida.", -2);

            if (string.IsNullOrWhiteSpace(req.usuario_sesion))
                return DbHelper.ErrorResponse(USUARIO_SESION_INVALIDO, -2);

            var user = req.usuario.Trim(); 
            var userSesion = req.usuario_sesion.Trim();    
            var cartera = req.cod_clasificacion.Trim();
            var accion = req.asignar == true ? "A" : "E";
            accion = accion == "A" ? "A" : "E";

            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);
            try
            {
                var p = new DynamicParameters();

                p.Add("@UsuarioList", user, DbType.String);
                p.Add("@Codigo", cartera, DbType.String);
                p.Add(USUARIO, userSesion, DbType.String);
                p.Add("@Mov", accion, DbType.AnsiStringFixedLength);

                conn.Execute("dbo.spCbr_Usuarios_Carteras_Add", p, commandType: CommandType.StoredProcedure);

                _securityMainDb.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = userSesion,
                    DetalleMovimiento = $"Cobros > Control Usuarios > Cartera {cartera} => {(req.asignar == true ? "Asignar" : "Eliminar")} (Usuario {user})",
                    Movimiento = "Modifica - WEB",
                    Modulo = vModulo
                });

                return DbHelper.CreateOkResponse();
            }
            catch (SqlException ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }
        /// <summary>
        /// Borrar usuario.
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="usuario_sesion"></param>
        /// </summary>
        /// <returns></returns>
        public ErrorDto CO_Usuarios_Eliminar(int CodEmpresa, string usuario, string usuario_sesion)
        {
            if (string.IsNullOrWhiteSpace(usuario))
                return DbHelper.ErrorResponse(USUARIO_INVALIDO, -2);

            if (string.IsNullOrWhiteSpace(usuario_sesion))
                return DbHelper.ErrorResponse(USUARIO_SESION_INVALIDO, -2);

            var user = usuario.Trim();
            var userSesion = usuario_sesion.Trim();

            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);
            try
            {
                const string sql = @"delete cbr_usuarios where usuario = @usuario;";
                var rows = conn.Execute(sql, new { usuario = user });

                if (rows <= 0)
                    return DbHelper.ErrorResponse("No se encontró registro verifique...", -2);

                _securityMainDb.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = userSesion,
                    DetalleMovimiento = $"Cobros > Control Usuarios > Elimina usuario: {user}",
                    Movimiento = "Elimina - WEB",
                    Modulo = vModulo
                });

                return DbHelper.CreateOkResponse();
            }
            catch (SqlException ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }
        private static object? V(IDictionary<string, object?> d, params string[] keys)
        {
            return keys
                .Select(k => d.TryGetValue(k, out var v) ? v : null)
                .FirstOrDefault(v => v != null);
        }
        private static string S(object? v) => (Convert.ToString(v) ?? string.Empty).Trim();
        private static string GetStr(IDictionary<string, object?> d, params string[] keys)
        {
            var value = keys
                .Select(k => d.TryGetValue(k, out var v) ? v : null)
                .FirstOrDefault(v => v != null);

            return value != null ? S(value) : string.Empty;
        }
        private static int GetInt(IDictionary<string, object?> d, params string[] keys)
        {
            var txt = GetStr(d, keys);
            if (string.IsNullOrWhiteSpace(txt)) return 0;
            return int.TryParse(txt, out var n) ? n : 0;
        }
        private static int I(object? v)
        {
            if (v == null) return 0;
            if (v is int i) return i;
            if (v is short s) return s;
            if (v is long l) return (int)l;
            if (v is byte b) return b;
            if (v is decimal m) return (int)m;

            var txt = (Convert.ToString(v) ?? "").Trim();
            if (string.IsNullOrWhiteSpace(txt)) return 0;
            return int.TryParse(txt, out var n) ? n : 0;
        }
        private static bool B01(object? v)
        {
            if (v == null) return false;
            if (v is bool bb) return bb;
            if (v is int i) return i == 1;
            if (v is short s) return s == 1;
            if (v is long l) return l == 1;
            if (v is byte b) return b == 1;
            if (v is decimal m) return m == 1;

            var txt = (Convert.ToString(v) ?? "").Trim();
            if (string.IsNullOrWhiteSpace(txt)) return false;

            if (int.TryParse(txt, out var n)) return n == 1;

            txt = txt.ToUpperInvariant();
            return (txt == "TRUE" || txt == "T" || txt == "S" || txt == "SI" || txt == "YES" || txt == "Y");
        }
        private static ErrorDto<CoControlUsuariosListaResult<T>> BuildListaResponse<T>()
        {
            var r = new ErrorDto<CoControlUsuariosListaResult<T>>
            {
                Code = 0,
                Description = "Ok",
                Result = new CoControlUsuariosListaResult<T>()
            };
            r.Result ??= new CoControlUsuariosListaResult<T>();
            return r;
        }
        private static ErrorDto<CoControlUsuariosListaResult<T>> EjecutarListaSP<T>(SqlConnection conn,string spName,string pUsuario,Func<IDictionary<string, object?>, T> map)
        {
            var response = BuildListaResponse<T>();
            response.Result ??= new CoControlUsuariosListaResult<T>();

            try
            {
                var p = new DynamicParameters();
                p.Add(USUARIO, pUsuario, DbType.String);

                var rows = conn.Query(spName, p, commandType: CommandType.StoredProcedure);

                var lista = new List<T>();

                foreach (var d in rows.Cast<IDictionary<string, object?>>())
                {
                    lista.Add(map(d));
                }

                response.Result.total = lista.Count;
                response.Result.lista = lista;

                return response;
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<CoControlUsuariosListaResult<T>>(ex.Message);
            }
        }
        private static string GetFirstNonEmpty(IDictionary<string, object?> d, params string[] keys)
        {
            return keys
                .Where(d.ContainsKey)
                .Select(k => S(d[k]))
                .FirstOrDefault(s => !string.IsNullOrWhiteSpace(s))
                ?? string.Empty;
        }
        private static ErrorDto? ValidarGuardar(CoControlUsuariosGuardarRequest? req)
        {
            if (req == null)
                return DbHelper.ErrorResponse("Solicitud inválida.", -2);

            if (string.IsNullOrWhiteSpace(req.usuario))
                return DbHelper.ErrorResponse("Usuario no es válido.", -2);

            if (string.IsNullOrWhiteSpace(req.nombre))
                return DbHelper.ErrorResponse("Nombre no es válido.", -2);

            if (string.IsNullOrWhiteSpace(req.cedula))
                return DbHelper.ErrorResponse("Número de Identificación no es válida.", -2);

            if (req.porc_comision < 0 || req.porc_comision > 100)
                return DbHelper.ErrorResponse("Porcentaje de Comisión no es válida.", -2);

            if (req.tiempo_resolucion_com < 0)
                return DbHelper.ErrorResponse("Tiempo de Resolución no es válida.", -2);

            if (string.IsNullOrWhiteSpace(req.usuario_sesion))
                return DbHelper.ErrorResponse(USUARIO_SESION_INVALIDO, -2);

            return null;
        }
        private static object BuildParamsGuardar(string user, string userSesion, CoControlUsuariosGuardarRequest req, bool isUpdate)
        {
            return new
            {
                usuario = user,
                nombre = (req.nombre ?? string.Empty).Trim().ToUpperInvariant(),
                cedula = (req.cedula ?? string.Empty).Trim(),
                estado = req.estado == 1 ? 1 : 0,
                aplica = req.aplica_comision == 1 ? 1 : 0,
                externo = req.operador_externo == 1 ? 1 : 0,
                cod_banco = req.cod_banco,
                tipo_documento = (req.tipo_documento ?? string.Empty).Trim(),
                porc = req.porc_comision,
                tiempo = req.tiempo_resolucion_com,
                usuario_mod = isUpdate ? userSesion : null,
                usuario_reg = isUpdate ? null : userSesion
            };
        }
        private static void EjecutarUpdate(SqlConnection conn, string user, string userSesion, CoControlUsuariosGuardarRequest req)
        {
            const string sqlUpd = @"
            update cbr_usuarios
            set nombre = @nombre,
                cedula = @cedula,
                estado = @estado,
                aplica_comision = @aplica,
                Operador_Externo = @externo,
                cod_banco = @cod_banco,
                tipo_documento = @tipo_documento,
                porc_comision = @porc,
                tiempo_resolucion_com = @tiempo,
                Modifica_Fecha = getdate(),
                Modifica_Usuario = @usuario_mod
            where usuario = @usuario;";

            var p = BuildParamsGuardar(user, userSesion, req, isUpdate: true);
            conn.Execute(sqlUpd, p);
        }
        public sealed class BusinessException : Exception
        {
            public int Code { get; }

            public BusinessException(string message, int code)
                : base(message)
            {
                Code = code;
            }
        }
        private static void EjecutarInsert(SqlConnection conn, string user, string userSesion, CoControlUsuariosGuardarRequest req)
        {
            const string sqlExiste = @"select count(1) from cbr_usuarios where usuario = @usuario;";
            var existe = conn.QuerySingle<int>(sqlExiste, new { usuario = user }) > 0;
            if (existe)
                throw new BusinessException("Usuario ya existe, verifique...", -2);

            const string sqlIns = @"
            insert into cbr_usuarios
            (usuario, nombre, cedula, estado, aplica_comision, Operador_Externo, cod_banco, tipo_documento, tiempo_resolucion_com, porc_comision, registro_fecha, registro_usuario)
            values
            (@usuario, @nombre, @cedula, @estado, @aplica, @externo, @cod_banco, @tipo_documento, @tiempo, @porc, getdate(), @usuario_reg);";

            var p = BuildParamsGuardar(user, userSesion, req, isUpdate: false);
            conn.Execute(sqlIns, p);
        }
        private void RegistrarBitacora(int CodEmpresa, string userSesion, string detalle, string movimiento)
        {
            _securityMainDb.Bitacora(new BitacoraInsertarDto
            {
                EmpresaId = CodEmpresa,
                Usuario = userSesion,
                DetalleMovimiento = detalle,
                Movimiento = movimiento,
                Modulo = vModulo
            });
        }
        private static CoControlUsuariosCuentasData MapCuenta(IDictionary<string, object?> d)
        {
            var cuenta = GetStr(d, "CUENTA_INTERNA");
            var banco = GetStr(d, "Banco", "BANCO");
            var tipo = GetStr(d, "TipoDesc", "TIPODESC");
            var divisa = GetStr(d, "cod_Divisa", "COD_DIVISA");

            var interbanca01 = GetInt(d, "CUENTA_INTERBANCA");
            var activa01 = GetInt(d, "ACTIVA");

            return new CoControlUsuariosCuentasData
            {
                cuenta = cuenta,
                banco = banco,
                tipo = tipo,
                cod_divisa = divisa,
                interbanca = interbanca01 == 1 ? "Sí" : "No",
                destino = GetStr(d, "DESTINO"),
                activa = activa01 == 1 ? "Activa" : "Cerrada",
                registro_fecha = GetStr(d, "REGISTRO_FECHA"),
                registro_usuario = GetStr(d, "REGISTRO_USUARIO")
            };
        }


    }
}
