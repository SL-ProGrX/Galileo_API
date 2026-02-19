using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.Security;
using Galileo_API.Models.ProGrX.Cobros;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Galileo_API.DataBaseTier.ProGrX.Cobros
{
    public class FrmCOControlUsuariosRolDB
    {
        private readonly PortalDB _portalDB;
        private readonly MSecurityMainDb _securityMainDb;
        private const int vModulo = 4;
        private const string USUARIO_INVALIDO = "Usuario inválido.";
        private const string USUARIO_SESION_INVALIDO = "Usuario de sesión inválido.";
        private const string SOLICITUD_INVALIDA = "Solicitud inválida.";
        private const string ASIGNAR = "Asignar";
        private const string ELIMINAR = "Eliminar";
        private const string MODIFICA = "Modifica - WEB";
        public FrmCOControlUsuariosRolDB(IConfiguration config)
        {
            _portalDB = new PortalDB(config);
            _securityMainDb = new MSecurityMainDb(config);
        }

        /// <summary>
        /// Dropdown de ejecutivos/usuarios activos.
        /// <param name="CodEmpresa"></param>
        /// <param name="filtro"></param>
        /// </summary>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CO_UsuariosRol_Usuarios_Dropdown_Obtener(int CodEmpresa, string? filtro)
        {
            var texto = (filtro ?? string.Empty).Trim();
            var hasFiltro = !string.IsNullOrWhiteSpace(texto);
            var like = hasFiltro ? $"%{texto}%" : null;

            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);
            try
            {
                const string sql = @"
                SELECT
                    RTRIM(usuario) AS item,
                    RTRIM(nombre) AS descripcion
                FROM CBR_USUARIOS
                WHERE Estado = 1
                  AND (@filtro IS NULL OR Nombre LIKE @like)
                ORDER BY Nombre;";

                var lista = conn.Query<DropDownListaGenericaModel>(sql, new
                {
                    filtro = hasFiltro ? texto : null,
                    like
                }).ToList();

                return DbHelper.CreateOkResponse(lista);
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<List<DropDownListaGenericaModel>>(ex.Message);
            }
        }
        /// <summary>
        /// Lista Antigüedad con asignación por usuario.
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// </summary>
        /// <returns></returns>
        public ErrorDto<CoControlUsuariosListaResult<CoControlUsuariosRolAntiguedadItem>> CO_UsuariosRol_Antiguedad_Lista_Obtener(int CodEmpresa, string usuario)
        {
            if (string.IsNullOrWhiteSpace(usuario))
                return DbHelper.CreateErrorResponse<CoControlUsuariosListaResult<CoControlUsuariosRolAntiguedadItem>>(USUARIO_INVALIDO, -2);

            var pUsuario = usuario.Trim();

            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);
            try
            {
                const string sql = @"
                SELECT
                    Ant.COD_ANTIGUEDAD AS cod_antiguedad,
                    RTRIM(Ant.DESCRIPCION) AS descripcion,
                    CASE WHEN Asg.Usuario IS NULL THEN 0 ELSE 1 END AS asignado
                FROM CBR_ANTIGUEDAD_TIPOS Ant
                LEFT JOIN CBR_USUARIOS_ANTIGUEDADES Asg
                    ON Ant.COD_ANTIGUEDAD = Asg.COD_ANTIGUEDAD
                   AND Asg.USUARIO = @usuario
                ORDER BY Ant.COD_ANTIGUEDAD;";

                var lista = conn.Query<CoControlUsuariosRolAntiguedadItem>(sql, new { usuario = pUsuario }).ToList();

                var resp = BuildListaResponse<CoControlUsuariosRolAntiguedadItem>();
                resp.Result!.total = lista.Count;
                resp.Result!.lista = lista;
                return resp;
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<CoControlUsuariosListaResult<CoControlUsuariosRolAntiguedadItem>>(ex.Message);
            }
        }
        /// <summary>
        /// Lista Garantías con asignación por usuario.
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// </summary>
        /// <returns></returns>
        public ErrorDto<CoControlUsuariosListaResult<CoControlUsuariosRolGarantiaItem>> CO_UsuariosRol_Garantias_Lista_Obtener(int CodEmpresa, string usuario)
        {
            if (string.IsNullOrWhiteSpace(usuario))
                return DbHelper.CreateErrorResponse<CoControlUsuariosListaResult<CoControlUsuariosRolGarantiaItem>>(USUARIO_INVALIDO, -2);

            var pUsuario = usuario.Trim();

            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);
            try
            {
                const string sql = @"
                SELECT
                    RTRIM(Ant.GARANTIA) AS garantia,
                    RTRIM(Ant.DESCRIPCION) AS descripcion,
                    CASE WHEN Asg.Usuario IS NULL THEN 0 ELSE 1 END AS asignado
                FROM CRD_GARANTIA_TIPOS Ant
                LEFT JOIN CBR_USUARIOS_GARANTIAS Asg
                    ON Ant.GARANTIA = Asg.GARANTIA
                   AND Asg.USUARIO = @usuario
                ORDER BY Ant.GARANTIA;";

                var lista = conn.Query<CoControlUsuariosRolGarantiaItem>(sql, new { usuario = pUsuario }).ToList();

                var resp = BuildListaResponse<CoControlUsuariosRolGarantiaItem>();
                resp.Result!.total = lista.Count;
                resp.Result!.lista = lista;
                return resp;
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<CoControlUsuariosListaResult<CoControlUsuariosRolGarantiaItem>>(ex.Message);
            }
        }
        /// <summary>
        /// Lista Oficinas con asignación por usuario.
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// </summary>
        /// <returns></returns>
        public ErrorDto<CoControlUsuariosListaResult<CoControlUsuariosRolOficinaItem>> CO_UsuariosRol_Oficinas_Lista_Obtener(int CodEmpresa, string usuario)
        {
            if (string.IsNullOrWhiteSpace(usuario))
                return DbHelper.CreateErrorResponse<CoControlUsuariosListaResult<CoControlUsuariosRolOficinaItem>>(USUARIO_INVALIDO, -2);

            var pUsuario = usuario.Trim();

            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);
            try
            {
                const string sql = @"
                SELECT
                    RTRIM(Ant.COD_OFICINA) AS cod_oficina,
                    RTRIM(Ant.DESCRIPCION) AS descripcion,
                    CASE WHEN Asg.Usuario IS NULL THEN 0 ELSE 1 END AS asignado
                FROM SIF_OFICINAS Ant
                LEFT JOIN CBR_USUARIOS_OFICINAS Asg
                    ON Ant.COD_OFICINA = Asg.COD_OFICINA
                   AND Asg.USUARIO = @usuario
                ORDER BY Ant.COD_OFICINA;";

                var lista = conn.Query<CoControlUsuariosRolOficinaItem>(sql, new { usuario = pUsuario }).ToList();

                var resp = BuildListaResponse<CoControlUsuariosRolOficinaItem>();
                resp.Result!.total = lista.Count;
                resp.Result!.lista = lista;
                return resp;
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<CoControlUsuariosListaResult<CoControlUsuariosRolOficinaItem>>(ex.Message);
            }
        }
        /// <summary>
        /// Lista Instituciones con asignación por usuario.
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// </summary>
        /// <returns></returns>
        public ErrorDto<CoControlUsuariosListaResult<CoControlUsuariosRolInstitucionItem>> CO_UsuariosRol_Instituciones_Lista_Obtener(int CodEmpresa, string usuario)
        {
            if (string.IsNullOrWhiteSpace(usuario))
                return DbHelper.CreateErrorResponse<CoControlUsuariosListaResult<CoControlUsuariosRolInstitucionItem>>(USUARIO_INVALIDO, -2);

            var pUsuario = usuario.Trim();

            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);
            try
            {
                const string sql = @"
                SELECT
                    RTRIM(Ant.COD_INSTITUCION) AS cod_institucion,
                    RTRIM(Ant.DESCRIPCION) AS descripcion,
                    CASE WHEN Asg.Usuario IS NULL THEN 0 ELSE 1 END AS asignado
                FROM Instituciones Ant
                LEFT JOIN CBR_USUARIOS_INSTITUCION Asg
                    ON Ant.COD_INSTITUCION = Asg.COD_INSTITUCION
                   AND Asg.USUARIO = @usuario
                ORDER BY Ant.COD_INSTITUCION;";

                var lista = conn.Query<CoControlUsuariosRolInstitucionItem>(sql, new { usuario = pUsuario }).ToList();

                var resp = BuildListaResponse<CoControlUsuariosRolInstitucionItem>();
                resp.Result!.total = lista.Count;
                resp.Result!.lista = lista;
                return resp;
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<CoControlUsuariosListaResult<CoControlUsuariosRolInstitucionItem>>(ex.Message);
            }
        }
        /// <summary>
        /// Asignar/Desasignar Antigüedad.
        /// <param name="CodEmpresa"></param>
        /// <param name="req"></param>
        /// </summary>
        /// <returns></returns>
        public ErrorDto CO_UsuariosRol_Antiguedad_Asignar(int CodEmpresa, CoControlUsuariosRolAsignarAntiguedadRequest req)
        {
            if (req == null)
                return DbHelper.ErrorResponse(SOLICITUD_INVALIDA, -2);
            var err = ValidarAsignacionBase(req.usuario, req.usuario_sesion);
            if (err != null) return err;

            var codAnt = (req.cod_antiguedad ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(codAnt))
                return DbHelper.ErrorResponse("Antigüedad inválida.", -2);

            var usuario = req.usuario!.Trim();
            var usuarioSesion = req.usuario_sesion!.Trim();
            var asignar = req.asignar ?? false;

            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);
            try
            {
                const string sqlIns = @"
                INSERT INTO CBR_USUARIOS_ANTIGUEDADES(usuario, cod_antiguedad, registro_fecha, registro_usuario)
                VALUES(@usuario, @cod, dbo.MyGetdate(), @usr);";

                const string sqlDel = @"
                DELETE CBR_USUARIOS_ANTIGUEDADES
                WHERE usuario = @usuario AND cod_antiguedad = @cod;";

                EjecutarAsignacion(conn, asignar, sqlIns, sqlDel, new
                {
                    usuario,
                    cod = codAnt,
                    usr = usuarioSesion
                });

                RegistrarBitacora(
                    CodEmpresa,
                    usuarioSesion,
                    $"Cobros > Control Usuarios Rol > Antigüedad {codAnt} => {(asignar ? ASIGNAR : ELIMINAR)} (Usuario {usuario})",
                    MODIFICA
                );

                return DbHelper.CreateOkResponse();
            }
            catch (SqlException ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }
        /// <summary>
        /// Asignar/Desasignar Garantía.
        /// <param name="CodEmpresa"></param>
        /// <param name="req"></param>
        /// </summary>
        /// <returns></returns>
        public ErrorDto CO_UsuariosRol_Garantia_Asignar(int CodEmpresa, CoControlUsuariosRolAsignarGarantiaRequest req)
        {
            if (req == null)
                return DbHelper.ErrorResponse(SOLICITUD_INVALIDA, -2);
            var err = ValidarAsignacionBase(req.usuario, req.usuario_sesion);
            if (err != null) return err;
            var garantia = (req.garantia ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(garantia))
                return DbHelper.ErrorResponse("Garantía inválida.", -2);

            var usuario = req.usuario!.Trim();
            var usuarioSesion = req.usuario_sesion!.Trim();
            var asignar = req.asignar ?? false;

            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);
            try
            {
                const string sqlIns = @"
                INSERT INTO CBR_USUARIOS_GARANTIAS(usuario, garantia, registro_fecha, registro_usuario)
                VALUES(@usuario, @gar, dbo.MyGetdate(), @usr);";

                                const string sqlDel = @"
                DELETE CBR_USUARIOS_GARANTIAS
                WHERE usuario = @usuario AND garantia = @gar;";

                EjecutarAsignacion(conn, asignar, sqlIns, sqlDel, new
                {
                    usuario,
                    gar = garantia,
                    usr = usuarioSesion
                });

                RegistrarBitacora(
                    CodEmpresa,
                    usuarioSesion,
                    $"Cobros > Control Usuarios Rol > Garantía {garantia} => {(asignar ? ASIGNAR : ELIMINAR)} (Usuario {usuario})",
                    MODIFICA
                );

                return DbHelper.CreateOkResponse();
            }
            catch (SqlException ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }
        /// <summary>
        /// Asignar/Desasignar Oficina.
        /// <param name="CodEmpresa"></param>
        /// <param name="req"></param>
        /// </summary>
        /// <returns></returns>
        public ErrorDto CO_UsuariosRol_Oficina_Asignar(int CodEmpresa, CoControlUsuariosRolAsignarOficinaRequest req)
        {
            if (req == null)
                return DbHelper.ErrorResponse(SOLICITUD_INVALIDA, -2);
            var err = ValidarAsignacionBase(req.usuario, req.usuario_sesion);
            if (err != null) return err;
            var oficina = (req.cod_oficina ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(oficina))
                return DbHelper.ErrorResponse("Oficina inválida.", -2);

            var usuario = req.usuario!.Trim();
            var usuarioSesion = req.usuario_sesion!.Trim();
            var asignar = req.asignar ?? false;

            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);
            try
            {
                const string sqlIns = @"
                    INSERT INTO CBR_USUARIOS_OFICINAS(usuario, cod_oficina, registro_fecha, registro_usuario)
                    VALUES(@usuario, @cod, dbo.MyGetdate(), @usr);";

                const string sqlDel = @"
                    DELETE CBR_USUARIOS_OFICINAS
                    WHERE usuario = @usuario AND cod_oficina = @cod;";

                EjecutarAsignacion(conn, asignar, sqlIns, sqlDel, new
                {
                    usuario,
                    cod = oficina,
                    usr = usuarioSesion
                });

                RegistrarBitacora(
                    CodEmpresa,
                    usuarioSesion,
                    $"Cobros > Control Usuarios Rol > Oficina {oficina} => {(asignar ? ASIGNAR : ELIMINAR)} (Usuario {usuario})",
                    MODIFICA
                );

                return DbHelper.CreateOkResponse();
            }
            catch (SqlException ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }
        /// <summary>
        /// Asignar/Desasignar Institución.
        /// <param name="CodEmpresa"></param>
        /// <param name="req"></param>
        /// </summary>
        /// <returns></returns>
        public ErrorDto CO_UsuariosRol_Institucion_Asignar(int CodEmpresa, CoControlUsuariosRolAsignarInstitucionRequest req)
        {
            if (req == null)
                return DbHelper.ErrorResponse(SOLICITUD_INVALIDA, -2);
            var err = ValidarAsignacionBase(req.usuario, req.usuario_sesion);
            if (err != null) return err;
            var institucion = (req.cod_institucion ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(institucion))
                return DbHelper.ErrorResponse("Institución inválida.", -2);

            var usuario = req.usuario!.Trim();
            var usuarioSesion = req.usuario_sesion!.Trim();
            var asignar = req.asignar ?? false;

            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);
            try
            {
                const string sqlIns = @"
                INSERT INTO CBR_USUARIOS_INSTITUCION(usuario, cod_institucion, registro_fecha, registro_usuario)
                VALUES(@usuario, @cod, dbo.MyGetdate(), @usr);";

                const string sqlDel = @"
                DELETE CBR_USUARIOS_INSTITUCION
                WHERE usuario = @usuario AND cod_institucion = @cod;";

                EjecutarAsignacion(conn, asignar, sqlIns, sqlDel, new
                {
                    usuario,
                    cod = institucion,
                    usr = usuarioSesion
                });

                RegistrarBitacora(
                    CodEmpresa,
                    usuarioSesion,
                    $"Cobros > Control Usuarios Rol > Institución {institucion} => {(asignar ? ASIGNAR : ELIMINAR)} (Usuario {usuario})",
                    MODIFICA
                );

                return DbHelper.CreateOkResponse();
            }
            catch (SqlException ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }
        /// <summary>
        /// Copia roles (spCBR_UsuarioRol_Copia).
        /// <param name="CodEmpresa"></param>
        /// <param name="req"></param>
        /// </summary>
        /// <returns></returns>
        public ErrorDto CO_UsuariosRol_Copia(int CodEmpresa, CoControlUsuariosRolCopiaRequest req)
        {
            if (req == null)
                return DbHelper.ErrorResponse(SOLICITUD_INVALIDA, -2);

            if (string.IsNullOrWhiteSpace(req.us_origen))
                return DbHelper.ErrorResponse("Usuario origen inválido.", -2);

            if (string.IsNullOrWhiteSpace(req.us_destino))
                return DbHelper.ErrorResponse("Usuario destino inválido.", -2);

            if (string.IsNullOrWhiteSpace(req.usuario_sesion))
                return DbHelper.ErrorResponse(USUARIO_SESION_INVALIDO, -2);

            var origen = req.us_origen.Trim();
            var destino = req.us_destino.Trim();
            var usuarioSesion = req.usuario_sesion.Trim();

            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);
            try
            {
                var p = new DynamicParameters();
                p.Add("@UsOrigen", origen, DbType.String);
                p.Add("@UsDestino", destino, DbType.String);
                p.Add("@Usuario", usuarioSesion, DbType.String);
                p.Add("@RETURN_VALUE", dbType: DbType.Int32, direction: ParameterDirection.ReturnValue);

                conn.Execute("dbo.spCBR_UsuarioRol_Copia", p, commandType: CommandType.StoredProcedure);

                var ret = p.Get<int?>("@RETURN_VALUE") ?? 0;
                if (ret == 1)
                    return DbHelper.ErrorResponse("Usuario origen y destino no pueden ser iguales.", -2);

                RegistrarBitacora(CodEmpresa, usuarioSesion,
                    $"Cobros > Control Usuarios Rol > Copia roles: {origen} => {destino}",
                    MODIFICA);
                    
                return DbHelper.CreateOkResponse();
            }
            catch (SqlException ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }
        /// <summary>
        /// Limpia roles de usuarios inactivos (spCBR_UsuarioRol_Limpia).
        /// <param name="CodEmpresa"></param>
        /// <param name="req"></param>
        /// </summary>
        /// <returns></returns>
        public ErrorDto CO_UsuariosRol_Limpia(int CodEmpresa, CoControlUsuariosRolLimpiaRequest req)
        {
            if (req == null)
                return DbHelper.ErrorResponse(SOLICITUD_INVALIDA, -2);

            if (string.IsNullOrWhiteSpace(req.usuario_sesion))
                return DbHelper.ErrorResponse(USUARIO_SESION_INVALIDO, -2);

            var usuarioSesion = req.usuario_sesion.Trim();

            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);
            try
            {
                conn.Execute("dbo.spCBR_UsuarioRol_Limpia", commandType: CommandType.StoredProcedure);

                RegistrarBitacora(CodEmpresa, usuarioSesion,
                    "Cobros > Control Usuarios Rol > Limpia roles usuarios inactivos",
                    MODIFICA);

                return DbHelper.CreateOkResponse();
            }
            catch (SqlException ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }
        private static ErrorDto? ValidarAsignacionBase(string? usuario, string? usuarioSesion)
        {
            if (string.IsNullOrWhiteSpace(usuario))
                return DbHelper.ErrorResponse(USUARIO_INVALIDO, -2);

            if (string.IsNullOrWhiteSpace(usuarioSesion))
                return DbHelper.ErrorResponse(USUARIO_SESION_INVALIDO, -2);

            return null;
        }
        private static void EjecutarAsignacion(SqlConnection conn, bool asignar, string sqlIns, string sqlDel, object param)
        {
            if (asignar)
            {
                conn.Execute(sqlIns, param);
                return;
            }

            conn.Execute(sqlDel, param);
        }
        private void RegistrarBitacora(int CodEmpresa, string usuarioSesion, string detalle, string movimiento)
        {
            _securityMainDb.Bitacora(new BitacoraInsertarDto
            {
                EmpresaId = CodEmpresa,
                Usuario = usuarioSesion,
                DetalleMovimiento = detalle,
                Movimiento = movimiento,
                Modulo = vModulo
            });
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
    }
}
