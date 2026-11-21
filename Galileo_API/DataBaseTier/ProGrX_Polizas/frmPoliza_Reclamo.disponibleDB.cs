using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.Security;
using Galileo_API.Models.ProGrX_Polizas;

namespace Galileo_API.DataBaseTier.ProGrX_Polizas
{
    public partial class FrmPolizaReclamoDB
    {
        /// <summary>
        /// Obtiene el disponible actual del fondo del reclamo.
        /// </summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <param name="reclamoId">Id del reclamo.</param>
        /// <param name="plan">Código de plan.</param>
        /// <param name="contrato">Código de contrato/fondo.</param>
        /// <returns>Disponible actual del fondo.</returns>
        public ErrorDto<PolizaReclamoFondoDisponibleResponse> Poliza_Reclamo_Fondo_Disponible(
            int codEmpresa,
            int reclamoId,
            string plan,
            int contrato)
        {
            if (reclamoId <= 0)
                return DbHelper.CreateErrorResponse<PolizaReclamoFondoDisponibleResponse>("Debe indicar el reclamo.", -2);

            if (string.IsNullOrWhiteSpace(plan))
                return DbHelper.CreateErrorResponse<PolizaReclamoFondoDisponibleResponse>(PolizaReclamoConstFrm.valPlan, -2);

            return DbHelper.WithConn(_portalDb, codEmpresa, conn =>
            {
                const string sql = @"EXEC spPoliza_Reclamo_Fondo_Disponible @ReclamoId, @Plan, @Contrato";

                return conn.QueryFirstOrDefault<PolizaReclamoFondoDisponibleResponse>(
                    sql,
                    new
                    {
                        ReclamoId = reclamoId,
                        Plan = plan.Trim(),
                        Contrato = contrato
                    }
                ) ?? new PolizaReclamoFondoDisponibleResponse();
            });
        }


    }
}
