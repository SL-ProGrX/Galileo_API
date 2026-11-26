using System.Globalization;
using Dapper;
using Galileo.Models.AF;
using Microsoft.Data.SqlClient;

namespace Galileo.DataBaseTier.ProGrX_Beneficios
{
    public partial class FrmAfBeneficiosTrasladoDB
    {
        /// <summary>
        /// Encola la notificación de depósito de cada caso trasladado. El traslado ya está aplicado,
        /// por lo que un fallo de correo solo se informa. Devuelve el texto de las advertencias.
        /// Se activa con AFI_Beneficios:NotificaDeposito = "S".
        /// </summary>
        private async Task<string> NotificarDepositosAsync(SqlConnection connection, int codEmpresa, AfiTrasladoAplicar infoCarga)
        {
            var fallidos = new List<string>();

            foreach (var caso in infoCarga.casos)
            {
                try
                {
                    var error = await NotificarDepositoAsync(connection, codEmpresa, caso, infoCarga.usuario);
                    if (error != null)
                    {
                        fallidos.Add($"{caso.cod_beneficio.Trim()} {caso.consec}: {error}");
                    }
                }
                catch (Exception ex)
                {
                    fallidos.Add($"{caso.cod_beneficio.Trim()} {caso.consec}: {ex.Message}");
                }
            }

            return fallidos.Count == 0
                ? string.Empty
                : "No se registró el correo de depósito de: " + string.Join("; ", fallidos);
        }

        private async Task<string?> NotificarDepositoAsync(
            SqlConnection connection, int codEmpresa, AfiBeneTrasladoAplciar caso, string usuario)
        {
            const string sql = @"
                SELECT TOP 1
                    P.CEDULA AS cedula,
                    RTRIM(P.COD_BENEFICIO) + RIGHT('00000' + CAST(P.CONSEC AS VARCHAR(5)), 5) AS expediente,
                    B.DESCRIPCION AS beneficio,
                    P.T_EMAIL AS email,
                    C.COD_DIVISA AS cod_divisa
                FROM AFI_BENE_PAGO P
                INNER JOIN SYS_CUENTAS_BANCARIAS C ON P.CEDULA = C.IDENTIFICACION
                INNER JOIN AFI_BENEFICIOS B ON P.COD_BENEFICIO = B.COD_BENEFICIO
                WHERE P.CEDULA = @cedula AND P.COD_BENEFICIO = @codBeneficio AND P.CONSEC = @consec AND P.ID_PAGO = @idPago";

            var pago = await connection.QueryFirstOrDefaultAsync<AfiBeneDatosCorreoPago>(sql,
                new { caso.cedula, codBeneficio = caso.cod_beneficio, caso.consec, idPago = caso.id_pago });

            if (pago == null || string.IsNullOrWhiteSpace(pago.email))
            {
                return "el pago no tiene correo electrónico";
            }

            var envio = await _beneCorreo.EncolarAsync(new CorreoBeneficio
            {
                CodEmpresa = codEmpresa,
                CodPlantilla = BeneficiosPlantillas.Deposito,
                CodSmtp = _beneCorreo.CodSmtpPorBeneficio(codEmpresa, caso.cod_beneficio),
                Para = pago.email,
                Usuario = usuario,
                Datos = new Dictionary<string, string>
                {
                    ["NOMBRE"] = caso.nombre,
                    ["CEDULA"] = pago.cedula,
                    ["CUENTA"] = caso.cta_bancaria,
                    ["FECHA"] = DateTime.Now.ToString("d/M/yyyy", CultureInfo.InvariantCulture),
                    ["TIPO_MOVIMIENTO"] = DescribirEmision(caso.tipo_emision),
                    ["MONTO"] = caso.monto.ToString("N2", CultureInfo.InvariantCulture),
                    ["MONEDA"] = DescribirDivisa(pago.cod_divisa),
                    ["EXPEDIENTE"] = pago.expediente,
                    ["BENEFICIO"] = pago.beneficio
                }
            });

            return envio.Code == 0 ? null : envio.Description;
        }

        private static string DescribirEmision(string tipo) => tipo switch
        {
            "CK" => "Cheque",
            "TE" => "Transferencia",
            "RE" => "Efectivo",
            "ND" => "Nota Débito",
            "NC" => "Nota Crédito",
            "OT" => "Otro...",
            "CD" => "Ctrl Desembolsos",
            _ => tipo
        };

        private static string DescribirDivisa(string divisa) => divisa?.Trim() switch
        {
            "COL" => "Colones",
            "DOL" => "Dólares",
            _ => divisa ?? string.Empty
        };
    }
}
