using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX.Creditos;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Galileo_API.DataBaseTier.ProGrX.Creditos
{
    public partial class FrmCrComitesAprobacionesDB
    {
        /// <summary>
        /// Valida un usuario autorizador del comite: que sea miembro activo y que la clave sea correcta.
        /// La clave llega cifrada igual que en el login (spSEG_Logon).
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto CR_ComitesAprobaciones_Usuario_Validar(int CodEmpresa, CrComitesAprobacionesUsuarioValidarRequest request)
        {
            var usuario = (request.usuario ?? string.Empty).Trim();
            if (usuario.Length == 0 || string.IsNullOrEmpty(request.clave))
            {
                return DbHelper.ErrorResponse("Debe indicar el usuario y la clave.", -2);
            }

            try
            {
                using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

                const string sqlMiembro = @"
                    select count(*) as Existe
                    from CRD_COMITES_MIEMBROS Cp
                        inner join CRD_COMITES_AUTORIZADORES Ca on Cp.CEDULA = Ca.CEDULA
                    where Cp.ESTADO = 'A'
                      and Cp.USUARIO = @usuario
                      and Ca.ID_COMITE = @id_comite;";

                var esMiembro = conn.QueryFirstOrDefault<int>(sqlMiembro, new { usuario, request.id_comite });
                if (esMiembro == 0)
                {
                    return DbHelper.ErrorResponse($"El Usuario: {usuario}, no es miembro de este comité!", -2);
                }

                using var seguridad = new SqlConnection(_securityConnectionString);
                var existe = seguridad.QueryFirstOrDefault<int>(
                    "spSEG_Logon",
                    new { Usuario = usuario, Clave = request.clave },
                    commandType: CommandType.StoredProcedure);

                return existe == 0
                    ? DbHelper.ErrorResponse("Clave de Usuario incorrecta, intente de nuevo", -2)
                    : DbHelper.OkResponse("Usuario validado.");
            }
            catch (SqlException ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }
    }
}
