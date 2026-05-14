using System.Data;
using Dapper;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Fondos;

namespace Galileo.DataBaseTier.ProGrX.Fondos
{
    public partial class FrmFndPlanesDb
    {

        private const string SpReglasTasasActiva = "spFnd_Reglas_Tasas_Activa";

        #region Reglas

        /// <summary>
        /// Activa una regla de tasas para planes de fondos.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="dto">Información de la regla a activar.</param>
        /// <returns>Resultado de la activación.</returns>
        public ErrorDto Fnd_Reglas_Activar(int CodEmpresa, FndReglaActivarDto dto)
        {
            if (dto is null)
            {
                return DbHelper.ErrorResponse("La información de la regla es requerida.", -2);
            }

            try
            {
                var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
                    connection.QueryFirstOrDefault<dynamic>(
                        SpReglasTasasActiva,
                        new
                        {
                            Id = dto.id_regla,
                            Usuario = NormalizarTexto(dto.usuario)
                        },
                        commandType: CommandType.StoredProcedure));

                if (result.Code != 0)
                {
                    return DbHelper.ErrorResponse(
                        result.Description ?? "Error al activar regla.",
                        result.Code.GetValueOrDefault(-1));
                }

                if ((object?)result.Result is null || (result.Result?.Pass ?? 0) != 1)
                {
                    return DbHelper.ErrorResponse("No se pudo activar la regla.", -1);
                }

                return DbHelper.OkResponse("Regla activada satisfactoriamente");
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message, -1);
            }
        }


        #endregion

    }
}