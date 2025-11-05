using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models;
using PgxAPI.Models.ERROR;

namespace PgxAPI.DataBaseTier
{
    public class frmUS_DerechoXOpcionDB
    {
        private readonly IConfiguration _config;

        public frmUS_DerechoXOpcionDB(IConfiguration config)
        {
            _config = config;
        }

        public List<ModuloResultDto> ModulosObtener()
        {
            List<ModuloResultDto> resp = new List<ModuloResultDto>();
            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString("DefaultConnString")))
                {

                    var strSQL = "SELECT [MODULO],[NOMBRE],[DESCRIPCION],[ACTIVO],[KEYENT]" +
                        " FROM [PGX_Portal].[dbo].[US_MODULOS]" +
                        "ORDER BY modulo";

                    resp = connection.Query<ModuloResultDto>(strSQL).ToList();
                }
            }
            catch (Exception)
            {
                resp = null;
            }
            return resp;
        }

        public List<FormularioResultDto> FormulariosObtener()
        {
            List<FormularioResultDto> resp = new List<FormularioResultDto>();
            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString("DefaultConnString")))
                {

                    var strSQL = "SELECT [FORMULARIO],[MODULO],[DESCRIPCION],[REGISTRO_FECHA],[REGISTRO_USUARIO]" +
                                  "FROM[PGX_Portal].[dbo].[US_FORMULARIOS]" +
                                  "ORDER BY modulo,Descripcion";

                    resp = connection.Query<FormularioResultDto>(strSQL).ToList();
                }
            }
            catch (Exception)
            {
                resp = null;
            }
            return resp;
        }

        public List<OpcionResultDto> OpcionesObtener()
        {
            List<OpcionResultDto> resp = new List<OpcionResultDto>();
            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString("DefaultConnString")))
                {

                    var strSQL = "SELECT [COD_OPCION],[FORMULARIO],[MODULO],[OPCION],[OPCION_DESCRIPCION],[REGISTRO_FECHA],[REGISTRO_USUARIO]" +
                                 "FROM[PGX_Portal].[dbo].[US_OPCIONES]";

                    resp = connection.Query<OpcionResultDto>(strSQL).ToList();
                }
            }
            catch (Exception)
            {
                resp = null;
            }
            return resp;
        }

        public List<DatosResultDto> DatosObtener(int opcion, char estado)
        {
            List<DatosResultDto> resp = new List<DatosResultDto>();
            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString("DefaultConnString")))
                {

                    var strSQL = "Select R.cod_rol,R.descripcion, isnull(P.Estado,'Z') as 'Estado'"
                                + " from US_ROLES R left join US_ROL_PERMISOS P on R.cod_Rol = P.cod_Rol and P.cod_Opcion = " + opcion
                                + " and P.Estado = '" + estado.ToString() + "'"
                                + " where R.Activo = 1"
                                + " order by isnull(P.Estado,'Z'), R.Descripcion";

                    resp = connection.Query<DatosResultDto>(strSQL).ToList();
                }
            }
            catch (Exception)
            {
                resp = null;
            }
            return resp;
        }

        public ErrorDTO RolPermisosActualizar(OpcionRolRequestDto req)
        {
            ErrorDTO resp = new ErrorDTO();
            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString("DefaultConnString")))
                {
                    if (req.check)
                    {
                        var strSQL = "INSERT INTO [dbo].[US_ROL_PERMISOS]([COD_OPCION],[COD_ROL],[ESTADO],[REGISTRO_FECHA],[REGISTRO_USUARIO]) VALUES(@COD_OPCION,@COD_ROL,@ESTADO,@REGISTRO_FECHA,@REGISTRO_USUARIO)";

                        resp.Code = connection.Execute(strSQL, new { COD_OPCION = req.opcion, COD_ROL = req.rol, ESTADO = req.tipo, REGISTRO_FECHA = DateTime.Now, REGISTRO_USUARIO = req.usuario });
                        resp.Description = "Inserted";
                    }
                    else
                    {
                        var strSQL = "delete Us_Rol_Permisos where cod_Opcion = " + req.opcion +
                                     " and estado = '" + req.tipo + "'" +
                                     " and cod_rol = '" + req.rol + "'";

                        resp.Code = connection.Execute(strSQL);
                        resp.Description = "Deleted";
                    }

                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }
            return resp;
        }
    }
}
