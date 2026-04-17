using Dapper;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Cajas;

namespace Galileo.DataBaseTier.ProGrX.Cajas
{
    public class FrmCajasUsuariosDB
    {
        private readonly PortalDB _portalDb;

        public FrmCajasUsuariosDB(IConfiguration config)
        {
            _portalDb = new PortalDB(config ?? throw new ArgumentNullException(nameof(config)));
        }

        public ErrorDto<List<CajasUsuariosListadoUsuarioData>> Cajas_Usuarios_Lista_Obtener(int CodEmpresa, FiltrosLazyLoadData filtros, bool soloAsignados)
        {
            var result = new ErrorDto<List<CajasUsuariosListadoUsuarioData>>
            {
                Code = 0,
                Description = "Ok",
                Result = new List<CajasUsuariosListadoUsuarioData>()
            };

            try
            {
                filtros ??= new FiltrosLazyLoadData();
                using var cn = DbHelper.OpenConnection(_portalDb, CodEmpresa);

                var parameters = new DynamicParameters();
                parameters.Add("@soloAsignados", soloAsignados ? 1 : 0);

                var where = " WHERE u.Estado = 'A' ";

                if (!string.IsNullOrWhiteSpace(filtros.filtro))
                {
                    where += @" AND (RTRIM(u.Nombre) LIKE @filtro OR RTRIM(u.Descripcion) LIKE @filtro)";
                    parameters.Add("@filtro", "%" + filtros.filtro + "%");
                }

                if (soloAsignados)
                {
                    where += @" AND EXISTS (SELECT 1 FROM CAJAS_USUARIOS cu WHERE cu.USUARIO = u.Nombre)";
                }

                var sortField = "USUARIO";
                var sortDirection = filtros.sortOrder == 0 ? "ASC" : "DESC";

                var q = $@"
                SELECT 
                      RTRIM(u.Nombre) AS usuario
                    , RTRIM(u.Descripcion) AS nombre_usuario
                    , CASE 
                        WHEN EXISTS (SELECT 1 FROM CAJAS_USUARIOS cu WHERE cu.USUARIO = u.Nombre)
                        THEN 1 ELSE 0 END AS tiene_cajas
                FROM Usuarios u
                {where}
                ORDER BY {sortField} {sortDirection};";

                result.Result = cn.Query<CajasUsuariosListadoUsuarioData>(q, parameters).ToList();
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = new List<CajasUsuariosListadoUsuarioData>();
            }

            return result;
        }

        public ErrorDto Cajas_Usuarios_Guardar(int CodEmpresa, string usuario, CajasUsuariosData usuarioCaja)
        {
            using var cn = DbHelper.OpenConnection(_portalDb, CodEmpresa);

            var resp = new ErrorDto { Code = 0, Description = "Ok" };

            try
            {
                if (string.IsNullOrWhiteSpace(usuarioCaja.cod_caja))
                    return DbHelper.ErrorResponse("Debe indicar el código de caja.", -2);

                if (string.IsNullOrWhiteSpace(usuarioCaja.usuario))
                    return DbHelper.ErrorResponse("Debe indicar el usuario.", -2);

                const string qExiste = @"SELECT ISNULL(COUNT(*),0) FROM CAJAS_USUARIOS WHERE COD_CAJA=@cod_caja AND USUARIO=@usuario;";

                var existe = cn.ExecuteScalar<int>(qExiste, usuarioCaja);

                if (usuarioCaja.isNew)
                {
                    if (existe > 0)
                        return DbHelper.ErrorResponse($"El usuario {usuarioCaja.usuario} ya está asignado.", -2);

                    cn.Execute(@"INSERT INTO CAJAS_USUARIOS (COD_CAJA,USUARIO,CONTRASENA,CONTRASENA_RENOVACION,BLOQUEO,REGISTRO_FECHA,REGISTRO_USUARIO)
                                 VALUES (@cod_caja,@usuario,'',dbo.MyGetdate(),NULL,dbo.MyGetdate(),@registro_usuario);",
                        new { usuarioCaja.cod_caja, usuarioCaja.usuario, registro_usuario = usuario });
                }
                else
                {
                    if (existe == 0)
                        return DbHelper.ErrorResponse($"Usuario no existe.", -2);

                    cn.Execute(@"UPDATE CAJAS_USUARIOS SET CONTRASENA=@contrasena WHERE COD_CAJA=@cod_caja AND USUARIO=@usuario;",
                        usuarioCaja);
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }

            return resp;
        }

        public ErrorDto Cajas_Usuarios_Eliminar(int CodEmpresa, string usuario, string cod_caja, string usuarioCaja)
        {
            using var cn = DbHelper.OpenConnection(_portalDb, CodEmpresa);

            var resp = new ErrorDto { Code = 0, Description = "Ok" };

            try
            {
                cn.Execute(@"DELETE FROM CAJAS_USUARIOS WHERE COD_CAJA=@cod_caja AND USUARIO=@usuario;",
                    new { cod_caja, usuario = usuarioCaja });
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }

            return resp;
        }

        public ErrorDto<List<CajasUsuariosHistData>> Cajas_Usuarios_Historico_Obtener(int CodEmpresa, string cod_caja, string usuarioCaja)
        {
            using var cn = DbHelper.OpenConnection(_portalDb, CodEmpresa);

            var result = new ErrorDto<List<CajasUsuariosHistData>>
            {
                Code = 0,
                Description = "Ok",
                Result = new List<CajasUsuariosHistData>()
            };

            try
            {
                result.Result = cn.Query<CajasUsuariosHistData>(
                    @"SELECT * FROM CAJAS_USUARIOS_H WHERE COD_CAJA=@cod_caja AND USUARIO=@usuario",
                    new { cod_caja, usuario = usuarioCaja }).ToList();
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = new List<CajasUsuariosHistData>();
            }

            return result;
        }

        public ErrorDto<List<CajasUsuariosCajaListaData>> Cajas_Usuarios_Cajas_Lista_Obtener(int CodEmpresa, string usuarioCaja)
        {
            using var cn = DbHelper.OpenConnection(_portalDb, CodEmpresa);

            var result = new ErrorDto<List<CajasUsuariosCajaListaData>>
            {
                Code = 0,
                Description = "Ok",
                Result = new List<CajasUsuariosCajaListaData>()
            };

            try
            {
                result.Result = cn.Query<CajasUsuariosCajaListaData>(
                    @"SELECT Def.COD_CAJA, Def.DESCRIPCION,
                      CASE WHEN Cus.USUARIO IS NULL THEN 0 ELSE 1 END AS asignado
                      FROM CAJAS_DEFINICION Def
                      LEFT JOIN CAJAS_USUARIOS Cus ON Def.COD_CAJA = Cus.COD_CAJA AND Cus.USUARIO=@usuario",
                    new { usuario = usuarioCaja }).ToList();
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = new List<CajasUsuariosCajaListaData>();
            }

            return result;
        }
    }
}