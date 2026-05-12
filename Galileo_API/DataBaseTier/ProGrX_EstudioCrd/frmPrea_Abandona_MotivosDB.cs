using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_EstudioCrd;
using System.Data;

namespace Galileo_API.DataBaseTier.ProGrX_EstudioCrd
{
    public class FrmPreaAbandonaMotivosDB
    {
        private readonly PortalDB _portalDb;

        public FrmPreaAbandonaMotivosDB(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
        }

        /// <summary>
        /// Obtiene los motivos de abandono y marca cuáles están activos para el expediente.
        /// </summary>
        public ErrorDto<FrmPreaAbandonaMotivosListaResponse> Prea_frmPreaAbandonaMotivos_Lista_Obtener(
            int codEmpresa,
             string usuario,
             string cod_preanalisis)
        {
            var Result = new FrmPreaAbandonaMotivosListaResponse();

            try
            {
                using var connection = _portalDb.CreateConnection(codEmpresa);
                connection.Open();

                const string sql = @"EXEC spCrdPreaListaMotivosSeleccion @cod_preanalisis";

                Result.lista = connection.Query<FrmPreaAbandonaMotivoDto>(
                    sql,
                    new
                    {
                        cod_preanalisis = cod_preanalisis.Trim()
                    },
                    commandType: CommandType.Text
                ).ToList();

                return DbHelper.CreateOkResponse<FrmPreaAbandonaMotivosListaResponse>(Result);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<FrmPreaAbandonaMotivosListaResponse>(ex.Message);
            }
        }

        /// <summary>
        /// Registra o desregistra un motivo de abandono para el expediente.
        /// </summary>
        public ErrorDto<FrmPreaAbandonaMotivosRegistrarResponse> Prea_frmPreaAbandonaMotivos_Registrar(
            int codEmpresa,
            FrmPreaAbandonaMotivosRegistrarRequest request)
        {
            
            try
            {
                using var connection = _portalDb.CreateConnection(codEmpresa);
                connection.Open();

                const string sql = @"EXEC spCrdPreaMotivosAbandono_Registro @cod_preanalisis, @id_motivo, @activo, @usuario";

                connection.Execute(
                    sql,
                    new
                    {
                        cod_preanalisis = request.cod_preanalisis.Trim(),
                        id_motivo = request.id_motivo,
                        activo = request.activo ? 1 : 0,
                        usuario = request.usuario.Trim()
                    },
                    commandType: CommandType.Text
                );

                var Result = new FrmPreaAbandonaMotivosRegistrarResponse
                {
                    id_motivo = request.id_motivo,
                    activo = request.activo,
                    mensaje = "Motivo actualizado correctamente."
                };

                return DbHelper.CreateOkResponse<FrmPreaAbandonaMotivosRegistrarResponse>(Result);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<FrmPreaAbandonaMotivosRegistrarResponse>(ex.Message);
            }
        }
    }
}
