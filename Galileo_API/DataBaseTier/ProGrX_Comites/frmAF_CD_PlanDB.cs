using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_Comites;

namespace Galileo_API.DataBaseTier.ProGrX_Comites
{
    public class FrmAfCdPlanDb
    {
        private readonly PortalDB _portalDb;

        public FrmAfCdPlanDb(IConfiguration config)
           : this(
                 new PortalDB(config))
        {
        }

        public FrmAfCdPlanDb(PortalDB portalDB)
        {
            _portalDb = portalDB;
        }

        /// <summary>
        /// Obtiene la lista de comites.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> AfCdComites_Lista_Obtener(int codEmpresa)
        {
            const string query = @"
                select cod_comite as item, descripcion
                from afi_cd_comites order by cod_comite;";

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                _portalDb,
                codEmpresa,
                query);
        }

        /// <summary>
        /// Obtiene los mensajes vigentes de un comite.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codComite"></param>
        /// <returns></returns>
        public ErrorDto<List<AfCdPlanMensajeData>> AfCdPlanMensajes_Lista_Obtener(int codEmpresa, string codComite)
        {
            const string query = @"
                select * from afi_cd_comites_mensajes
                where cod_comite = @codComite
                  and datediff(day, getdate(), vencimiento) >= 0
                order by vencimiento, num_mensaje;";

            return DbHelper.ExecuteListQuery<AfCdPlanMensajeData>(
                _portalDb,
                codEmpresa,
                query,
                new { codComite = codComite.Trim() });
        }

        /// <summary>
        /// Registra un nuevo mensaje para un comite.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto AfCdPlanMensaje_Guardar(int codEmpresa, AfCdPlanMensajeData request)
        {
            using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);

            try
            {
                var codComite = request.cod_comite?.Trim() ?? string.Empty;
                var mensaje = request.mensaje?.Trim() ?? string.Empty;

                const string queryComite = @"
                    select count(1)
                    from afi_cd_comites
                    where cod_comite = @codComite;";

                var existe = conn.ExecuteScalar<int>(queryComite, new { codComite });

                if (existe <= 0)
                {
                    return new ErrorDto
                    {
                        Code = -2,
                        Description = "Este comit&eacute; no est&aacute; registrado, consulte uno v&aacute;lido."
                    };
                }

                const string queryConsecutivo = @"
                    select isnull(max(num_mensaje), 0) + 1
                    from afi_cd_comites_mensajes;";

                var numMensaje = conn.ExecuteScalar<int>(queryConsecutivo);

                const string queryInsert = @"
                    insert into afi_cd_comites_mensajes
                    (
                        fecha,
                        usuario,
                        cod_comite,
                        vencimiento,
                        mensaje,
                        num_mensaje
                    )
                    values
                    (
                        getdate(),
                        @usuario,
                        @codComite,
                        @vencimiento,
                        @mensaje,
                        @numMensaje
                    );";

                conn.Execute(
                    queryInsert,
                    new
                    {
                        usuario = request.usuario.Trim(),
                        codComite,
                        vencimiento = request.vencimiento?.Date,
                        mensaje,
                        numMensaje
                    });

                return new ErrorDto
                {
                    Code = 0,
                    Description = "Mensaje registrado satisfactoriamente."
                };
            }
            catch (Exception ex)
            {
                return new ErrorDto
                {
                    Code = -1,
                    Description = ex.Message
                };
            }
        }

        /// <summary>
        /// Elimina mensaje de un comite.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codComite"></param>
        /// <returns></returns>
        public ErrorDto AfCdPlanMensajes_Eliminar(int codEmpresa, string codComite, int numMensaje)
        {
            const string queryDelete = @"
                delete from afi_cd_comites_mensajes
                where cod_comite = @codComite
                and num_mensaje = @numMensaje;";

            return DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                queryDelete,
                new
                {
                    codComite = codComite.Trim(),
                    numMensaje
                });
        }
    }
}
