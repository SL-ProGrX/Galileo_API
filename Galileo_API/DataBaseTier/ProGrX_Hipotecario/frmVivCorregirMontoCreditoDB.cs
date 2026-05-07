using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_Hipotecario;

namespace Galileo_API.DataBaseTier.ProGrX_Hipotecario
{
    public class FrmVivCorregirMontoCreditoDB
    {
        private readonly PortalDB _portalDb;

        public FrmVivCorregirMontoCreditoDB(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
        }

        /// <summary>
        /// Obtiene la carga inicial del formulario frmVivCorregirMontoCredito.
        /// Replica la lectura de REG_CREDITOS y el máximo MontoNoGravable de ViviendaGarantia.
        /// </summary>
        public ErrorDto<FrmVivCorregirMontoCreditoResponse> Viv_CorregirMontoCredito_Obtener(
            int codEmpresa,
            long numero_operacion)
        {
            const string query = @"
SELECT TOP 1
    ISNULL(R.ID_SOLICITUD, 0) AS numero_operacion,
    RTRIM(ISNULL(R.CEDULA, '')) AS cedula,
    RTRIM(ISNULL(S.NOMBRE, '')) AS nombre,
    ISNULL(R.MONTOSOL, 0) AS monto_credito,
    ISNULL(R.PLAZO, 0) AS plazo,
    ISNULL(R.INT, 0) AS tasa,
    ISNULL(R.CUOTA, 0) AS cuota,
    RTRIM(ISNULL(R.ESTADOSOL, '')) AS estado_operacion,
    ISNULL(VG.monto_no_gravable, 0) AS monto_no_gravable
FROM REG_CREDITOS AS R
INNER JOIN SOCIOS AS S
    ON R.CEDULA = S.CEDULA
OUTER APPLY (
    SELECT ISNULL(MAX(V.MontoNoGravable), 0) AS monto_no_gravable
    FROM ViviendaGarantia AS V
    WHERE V.NumeroOperacion = R.ID_SOLICITUD
) AS VG
WHERE R.ID_SOLICITUD = @numero_operacion;";

            return DbHelper.ExecuteSingleQuery(
                _portalDb,
                codEmpresa,
                query,
                new FrmVivCorregirMontoCreditoResponse(),
                new
                {
                    numero_operacion = numero_operacion
                })!;
        }

        /// <summary>
        /// Guarda la corrección del monto del crédito y el monto no gravable.
        /// Replica la actualización de REG_CREDITOS y ViviendaGarantia del VB6.
        /// </summary>
        public ErrorDto<FrmVivCorregirMontoCreditoGuardarResponse> Viv_CorregirMontoCredito_Guardar(
            int codEmpresa,
            FrmVivCorregirMontoCreditoGuardarRequest request)
        {
            var response = new ErrorDto<FrmVivCorregirMontoCreditoGuardarResponse>
            {
                Code = 0,
                Description = string.Empty,
                Result = new FrmVivCorregirMontoCreditoGuardarResponse()
            };

            const string sqlOperacion = @"
SELECT TOP 1
    ISNULL(R.PLAZO, 0) AS plazo,
    ISNULL(R.INT, 0) AS tasa,
    ISNULL(R.CUOTA, 0) AS cuota_actual
FROM REG_CREDITOS AS R
WHERE R.ID_SOLICITUD = @numero_operacion;";

            const string sqlUpdateCredito = @"
UPDATE REG_CREDITOS
SET
    montoapr = @monto_credito,
    montosol = @monto_credito,
    saldo = @monto_credito,
    cuota = @cuota
WHERE ID_SOLICITUD = @numero_operacion;";

            const string sqlUpdateGarantia = @"
UPDATE ViviendaGarantia
SET MontoNoGravable = @monto_no_gravable
WHERE NumeroOperacion = @numero_operacion;";

            try
            {
                using var connection = DbHelper.OpenConnection(_portalDb, codEmpresa);
                connection.Open();

                using var tx = connection.BeginTransaction();

                try
                {
                    var contexto = connection.QueryFirstOrDefault<FrmVivCorregirMontoCreditoGuardarContexto>(
                        sqlOperacion,
                        new
                        {
                            numero_operacion = request.numero_operacion
                        },
                        transaction: tx);

                    if (contexto is null)
                    {
                        tx.Rollback();
                        response.Code = -1;
                        response.Description = "No se encontró la operación indicada.";
                        response.Result = new FrmVivCorregirMontoCreditoGuardarResponse();
                        return response;
                    }

                    decimal cuota = CalcularCuota(
                        request.monto_credito,
                        contexto.plazo,
                        contexto.tasa,
                        contexto.cuota_actual);

                    connection.Execute(
                        sqlUpdateCredito,
                        new
                        {
                            numero_operacion = request.numero_operacion,
                            monto_credito = request.monto_credito,
                            cuota
                        },
                        transaction: tx);

                    connection.Execute(
                        sqlUpdateGarantia,
                        new
                        {
                            numero_operacion = request.numero_operacion,
                            monto_no_gravable = request.monto_no_gravable
                        },
                        transaction: tx);

                    tx.Commit();

                    response.Description = "Información fue actualizada correctamente.";
                    response.Result = new FrmVivCorregirMontoCreditoGuardarResponse
                    {
                        cuota = cuota,
                        mensaje = response.Description
                    };
                }
                catch
                {
                    tx.Rollback();
                    throw;
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = new FrmVivCorregirMontoCreditoGuardarResponse();
            }

            return response;
        }

        private static decimal CalcularCuota(
            decimal montoCredito,
            int plazo,
            decimal tasa,
            decimal cuotaActual)
        {
            if (montoCredito <= 0)
            {
                return 0;
            }

            if (plazo <= 0 || tasa <= 0)
            {
                return cuotaActual > 0
                    ? decimal.Round(cuotaActual, 2, MidpointRounding.AwayFromZero)
                    : 0;
            }

            double monto = Convert.ToDouble(montoCredito);
            double tasaMensual = Convert.ToDouble(tasa) / 100d / 12d;
            double cuota = monto * (tasaMensual / (1d - Math.Pow(1d + tasaMensual, -plazo)));

            return decimal.Round(Convert.ToDecimal(cuota), 2, MidpointRounding.AwayFromZero);
        }


    }
}
