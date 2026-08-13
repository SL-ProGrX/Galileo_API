using Dapper;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_EstudioCrd;
using System.Collections.Generic;
using System.Data;
using System.Globalization;

namespace Galileo_API.DataBaseTier.ProGrX_EstudioCrd
{
    public partial class FrmPreaEstudiov2DB
    {
        /// <summary>
        /// Arma el DTO de crédito a partir del recordset y recalcula Cuota/Pólizas/
        /// Compromiso en vivo (ver RecalcularCreditoPolizas) — VB6 no confía en las
        /// columnas crudas de CRD_PREA_PREANALISIS para estos campos, siempre las
        /// recalcula al mostrar el expediente.
        /// </summary>
        private static FrmPreaEstudiov2CreditoDto ConstruirCredito(IDbConnection connection, IDictionary<string, object> row)
        {
            var monto = GetDecimal(row, "Monto");
            var plazo = GetInt(row, "Plazo");
            var tasa = GetDecimal(row, "TASA");
            var montoConstruccion = GetDecimal(row, "MONTO_CONSTRUCCION");
            var polizaVida = GetBool(row, "APL_POLIZA_VIDA");
            var polizaIncendio = GetBool(row, "apl_poliza_incendio");
            var polizaPrenda = GetBool(row, "APL_POLIZA_VEHICULO");
            var polizaDesempleo = GetBool(row, "APL_POLIZA_DESEMPLEO");

            var recalculo = RecalcularCreditoPolizas(
                connection, monto, plazo, tasa, montoConstruccion,
                polizaVida, ref polizaIncendio, polizaPrenda, polizaDesempleo);

            return new FrmPreaEstudiov2CreditoDto
            {
                linea = GetString(row, "Cod_Linea"),
                destino = GetString(row, "cod_destino"),
                garantia = GetString(row, "GARANTIA"),
                fiadores = GetInt(row, "NSUB_EXP"),
                no_op_crm = GetString(row, "NUM_OPORT_CRM"),
                monto = monto,
                tasa = tasa,
                plazo = plazo,
                cuota = recalculo.Cuota,
                monto_construccion = montoConstruccion,
                poliza_vida = polizaVida,
                poliza_incendio = polizaIncendio,
                poliza_prenda = polizaPrenda,
                poliza_desempleo = polizaDesempleo,
                monto_poliza_vida = recalculo.MontoPolizaVida,
                monto_poliza_incendio = recalculo.MontoPolizaIncendio,
                monto_poliza_prenda = recalculo.MontoPolizaPrenda,
                monto_poliza_desempleo = recalculo.MontoPolizaDesempleo,
                compromiso = recalculo.Compromiso,
                asignado_operacion = GetString(row, "ID_SOLICITUD"),
                cph = GetString(row, "COD_FORMULARIO_CPH"),
                valor_prenda = GetDecimal(row, "MONTO_VALOR_VEHICULO"),
                id_promotor = GetString(row, "Id_Promotor"),
                promotor_desc = GetString(row, "PromotorDesc"),
                monto_avaluo_cfia = GetDecimal(row, "MONTO_AVALUO_CFIA"),
                dias_interes_gastos_op = GetInt(row, "DIAS_INTERES_GASTOS_OP"),
                cod_capacidad = GetString(row, "COD_CAPACIDAD"),
                cod_endeudamiento = GetString(row, "COD_ENDEUDAMIENTO"),
                cod_garantia_clasificacion = GetString(row, "COD_GARANTIA"),
                cod_historial = GetString(row, "COD_HISTORIAL"),
                cod_mora = GetString(row, "COD_MORA"),
                cic_puntaje = GetString(row, "PUNTOS_CIC_DEUDOR"),
                cic_nivel_historico = GetString(row, "NIVEL_COMPORTAMIENTO_HIST"),
            };
        }

        /// <summary>
        /// Recalcula Cuota/Pólizas/Compromiso a pedido de Angular cuando el usuario
        /// cambia Monto/Plazo/Tasa/Monto Construcción o una póliza. VB6: sbCalcularCuota
        /// (dispara con cambios en txtMonto/txtPlazo/txtTasa) y chkPolizaX_Click.
        /// </summary>
        public ErrorDto<FrmPreaEstudiov2CreditoRecalculoResponse> Prea_frmPreaEstudiov2_Credito_Recalcular(
            int codEmpresa,
            FrmPreaEstudiov2CreditoRecalcularRequest request)
        {
            var result = new ErrorDto<FrmPreaEstudiov2CreditoRecalculoResponse>
            {
                Code = 0,
                Description = "Ok",
                Result = new FrmPreaEstudiov2CreditoRecalculoResponse()
            };

            try
            {
                using var connection = _portalDb.CreateConnection(codEmpresa);

                var tasa = request.tasa;
                var plazo = request.plazo;
                var tasaPtsBono = 0m;

                if (string.Equals(request.origen, "monto", StringComparison.OrdinalIgnoreCase))
                {
                    (tasa, plazo, tasaPtsBono) = RecalcularTasaPlazoCatalogo(
                        connection, request.linea, request.destino, request.garantia,
                        request.cedula, request.estado, request.monto, tasa, plazo);
                }

                var polizaIncendio = request.poliza_incendio;

                var recalculo = RecalcularCreditoPolizas(
                    connection,
                    request.monto,
                    plazo,
                    tasa,
                    request.monto_construccion,
                    request.poliza_vida,
                    ref polizaIncendio,
                    request.poliza_prenda,
                    request.poliza_desempleo);

                result.Result = new FrmPreaEstudiov2CreditoRecalculoResponse
                {
                    tasa = tasa,
                    plazo = plazo,
                    cuota = recalculo.Cuota,
                    compromiso = recalculo.Compromiso,
                    monto_poliza_vida = recalculo.MontoPolizaVida,
                    monto_poliza_incendio = recalculo.MontoPolizaIncendio,
                    monto_poliza_prenda = recalculo.MontoPolizaPrenda,
                    monto_poliza_desempleo = recalculo.MontoPolizaDesempleo,
                    poliza_incendio = polizaIncendio,
                    tasa_pts_bono = tasaPtsBono,
                };
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = new FrmPreaEstudiov2CreditoRecalculoResponse();
            }

            return result;
        }

        /// <summary>
        /// Resultado del recálculo en vivo de Cuota/Pólizas/Compromiso.
        /// VB6: sbCalcularCuota, sbCalculaPolizaDeVida/Incendio/Prenda/Desempleo,
        /// sbCalcularCompromiso (frmPreaEstudiov2.frm ~línea 14835-15092).
        /// </summary>
        private readonly struct CreditoRecalculoResultado
        {
            public CreditoRecalculoResultado(
                decimal cuota,
                decimal montoPolizaVida,
                decimal montoPolizaIncendio,
                decimal montoPolizaPrenda,
                decimal montoPolizaDesempleo,
                decimal compromiso,
                bool polizaIncendioAutoMarcada)
            {
                Cuota = cuota;
                MontoPolizaVida = montoPolizaVida;
                MontoPolizaIncendio = montoPolizaIncendio;
                MontoPolizaPrenda = montoPolizaPrenda;
                MontoPolizaDesempleo = montoPolizaDesempleo;
                Compromiso = compromiso;
                PolizaIncendioAutoMarcada = polizaIncendioAutoMarcada;
            }

            public decimal Cuota { get; }
            public decimal MontoPolizaVida { get; }
            public decimal MontoPolizaIncendio { get; }
            public decimal MontoPolizaPrenda { get; }
            public decimal MontoPolizaDesempleo { get; }
            public decimal Compromiso { get; }
            public bool PolizaIncendioAutoMarcada { get; }
        }

        /// <summary>
        /// VB6: fxCalcula_Cuota(Monto, Plazo, Interes, Frecuencia) — mValidacion.bas
        /// (PGX DLL Estudio Credito), línea ~656. Fórmula de amortización estándar.
        /// </summary>
        private static decimal CalcularCuota(decimal monto, int plazo, decimal tasa, string frecuencia = "M")
        {
            if (plazo <= 0 || monto <= 0)
            {
                return 0m;
            }

            double interesPeriodo = string.Equals(frecuencia, "Q", StringComparison.OrdinalIgnoreCase)
                ? (double)tasa / (24 * 100)
                : (double)tasa / (12 * 100);

            if (tasa == 0)
            {
                return Math.Round(monto / plazo, 2, MidpointRounding.AwayFromZero);
            }

            double factor = Math.Pow(1 + interesPeriodo, plazo);
            if (factor - 1 == 0)
            {
                return 0m;
            }

            double cuota = (double)monto * interesPeriodo * factor / (factor - 1);
            return Math.Round((decimal)cuota, 2, MidpointRounding.AwayFromZero);
        }

        /// <summary>
        /// VB6: fxCatalogoRango(codigo, monto, tipo, destino, garantia) — mValidacion.bas
        /// línea 414-426. Wrapper delgado sobre el UDF de SQL Server, sin lógica de rango
        /// en VB6: SELECT dbo.fxCrdCatalogoRango('&lt;codigo&gt;',&lt;monto&gt;,'&lt;tipo&gt;','&lt;destino&gt;','&lt;garantia&gt;').
        /// tipo: 'I' = Tasa (Interés), 'P' = Plazo.
        /// </summary>
        private static decimal CalcularCatalogoRango(
            IDbConnection connection, string codigo, decimal monto, string tipo, string destino, string garantia)
        {
            var sql = "SELECT dbo.fxCrdCatalogoRango('"
                + (codigo ?? string.Empty).Replace("'", "''") + "', "
                + monto.ToString(CultureInfo.InvariantCulture) + ", '"
                + tipo + "', '"
                + (destino ?? string.Empty).Replace("'", "''") + "', '"
                + (garantia ?? string.Empty).Replace("'", "''") + "')";
            return connection.QueryFirstOrDefault<decimal?>(sql) ?? 0m;
        }

        /// <summary>VB6: fxBonoMembresia(cedula, linea, garantia, destino, plazo) —
        /// mValidacion.bas línea 355-383: SELECT dbo.fxCrdTasaBonifica(...).</summary>
        private static decimal CalcularBonoMembresia(
            IDbConnection connection, string cedula, string linea, string garantia, string destino, int plazo)
        {
            if (string.IsNullOrWhiteSpace(cedula) || string.IsNullOrWhiteSpace(linea) || string.IsNullOrWhiteSpace(garantia))
            {
                return 0m;
            }

            var sql = "SELECT dbo.fxCrdTasaBonifica('"
                + cedula.Replace("'", "''") + "', '"
                + linea.Replace("'", "''") + "', '"
                + garantia.Replace("'", "''") + "', '"
                + (destino ?? string.Empty).Replace("'", "''") + "', "
                + plazo + ")";
            return connection.QueryFirstOrDefault<decimal?>(sql) ?? 0m;
        }

        /// <summary>VB6: fxBonoPlazoMembresia(cedula, garantia) — mValidacion.bas línea
        /// 386-406: SELECT dbo.fxCrdPlazoBonifica(...).</summary>
        private static int CalcularBonoPlazoMembresia(IDbConnection connection, string cedula, string garantia)
        {
            if (string.IsNullOrWhiteSpace(cedula) || string.IsNullOrWhiteSpace(garantia))
            {
                return 0;
            }

            var sql = "SELECT dbo.fxCrdPlazoBonifica('"
                + cedula.Replace("'", "''") + "', '"
                + garantia.Replace("'", "''") + "')";
            return connection.QueryFirstOrDefault<int?>(sql) ?? 0;
        }

        /// <summary>
        /// VB6: sbCalcularCuota, Case "txtMonto" (frmPreaEstudiov2.frm ~línea 14879-14911).
        /// Re-deriva Tasa/Plazo desde el catálogo por rango (línea/monto/destino/garantía) y
        /// aplica el bono de membresía cuando el Estado del expediente es 'R' o 'P'. No cubre
        /// la rama de Garantía "Y" (Fondos) con Tag de fondo ya establecido — para Fondos el
        /// Tasa/Plazo ya vienen fijados por Prea_frmPreaEstudiov2_Fondo_Calcular.
        /// </summary>
        private static (decimal tasa, int plazo, decimal tasaPtsBono) RecalcularTasaPlazoCatalogo(
            IDbConnection connection, string linea, string destino, string garantia,
            string cedula, string estado, decimal monto, decimal tasaActual, int plazoActual)
        {
            var tasa = tasaActual;
            var plazo = plazoActual;

            var esFondo = string.Equals((garantia ?? string.Empty).Trim(), "Y", StringComparison.OrdinalIgnoreCase);

            if (!esFondo && !string.IsNullOrWhiteSpace(linea) && monto > 0)
            {
                plazo = (int)CalcularCatalogoRango(connection, linea, monto, "P", destino, garantia);
                tasa = CalcularCatalogoRango(connection, linea, monto, "I", destino, garantia);
            }

            var tasaPtsBono = 0m;
            var estadoTrim = (estado ?? string.Empty).Trim();
            if (estadoTrim.Equals("R", StringComparison.OrdinalIgnoreCase) || estadoTrim.Equals("P", StringComparison.OrdinalIgnoreCase))
            {
                var bono = CalcularBonoMembresia(connection, cedula, linea, garantia, destino, plazo);
                var plazoBono = CalcularBonoPlazoMembresia(connection, cedula, garantia);

                if (bono > 0)
                {
                    tasa -= bono;
                    tasaPtsBono = bono;
                }

                if (plazoBono > 0)
                {
                    plazo = plazoBono;
                }
            }

            return (tasa, plazo, tasaPtsBono);
        }

        /// <summary>VB6: dbo.fxCrd_Prea_Poliza_Vida(Monto) — sbCalculaPolizaDeVida, línea ~15080.</summary>
        private static decimal CalcularPolizaVida(IDbConnection connection, decimal monto)
        {
            if (monto <= 0)
            {
                return 0m;
            }

            var sql = "SELECT dbo.fxCrd_Prea_Poliza_Vida(" + monto.ToString(CultureInfo.InvariantCulture) + ")";
            return connection.QueryFirstOrDefault<decimal?>(sql) ?? 0m;
        }

        /// <summary>
        /// VB6: dbo.fxCrd_Prea_Poliza_Incendio(Monto) — sbCalculaPolizaDeIncendio, línea ~14992.
        /// Usa MontoConstruccion si es &gt; 1000, si no usa Monto del crédito.
        /// </summary>
        private static decimal CalcularPolizaIncendio(IDbConnection connection, decimal monto, decimal montoConstruccion)
        {
            if (monto <= 0)
            {
                return 0m;
            }

            var montoBase = montoConstruccion > 1000 ? montoConstruccion : monto;
            var sql = "SELECT dbo.fxCrd_Prea_Poliza_Incendio(" + montoBase.ToString(CultureInfo.InvariantCulture) + ")";
            return connection.QueryFirstOrDefault<decimal?>(sql) ?? 0m;
        }

        /// <summary>VB6: dbo.fxCrd_Prea_Poliza_Vehiculo(Monto) — sbCalculaPolizaDePrenda, línea ~15026.</summary>
        private static decimal CalcularPolizaPrenda(IDbConnection connection, decimal monto)
        {
            if (monto <= 0)
            {
                return 0m;
            }

            var sql = "SELECT dbo.fxCrd_Prea_Poliza_Vehiculo(" + monto.ToString(CultureInfo.InvariantCulture) + ")";
            return connection.QueryFirstOrDefault<decimal?>(sql) ?? 0m;
        }

        /// <summary>
        /// VB6: dbo.fxCrd_Prea_Poliza_Desempleo(Cuota + PolizaVida + PolizaIncendio) —
        /// sbCalculaPolizaDesempleo, línea ~15053.
        /// </summary>
        private static decimal CalcularPolizaDesempleo(IDbConnection connection, decimal cuota, decimal montoPolizaVida, decimal montoPolizaIncendio)
        {
            if (cuota <= 0)
            {
                return 0m;
            }

            var monto = cuota + montoPolizaVida + montoPolizaIncendio;
            var sql = "SELECT dbo.fxCrd_Prea_Poliza_Desempleo(" + monto.ToString(CultureInfo.InvariantCulture) + ")";
            return connection.QueryFirstOrDefault<decimal?>(sql) ?? 0m;
        }

        /// <summary>
        /// Recalcula Cuota, montos de pólizas y Compromiso en vivo, replicando la cadena
        /// sbCalcularCuota -&gt; sbCalculaPolizaDeVida/Incendio/Prenda/Desempleo -&gt;
        /// sbCalcularCompromiso de VB6. Se aplica siempre (no solo Estado R/P) porque así
        /// se observó en pantalla contra datos reales de LAB04 (expediente 5424).
        /// </summary>
        private static CreditoRecalculoResultado RecalcularCreditoPolizas(
            IDbConnection connection,
            decimal monto,
            int plazo,
            decimal tasa,
            decimal montoConstruccion,
            bool polizaVida,
            ref bool polizaIncendio,
            bool polizaPrenda,
            bool polizaDesempleo)
        {
            var cuota = CalcularCuota(monto, plazo, tasa);

            // VB6 (sbCalculaPolizaDeIncendio ~línea 14999): si hay Monto Construcción y el
            // checkbox está sin marcar, lo marca automáticamente.
            var autoMarcada = false;
            if (montoConstruccion > 0 && !polizaIncendio)
            {
                polizaIncendio = true;
                autoMarcada = true;
            }

            var montoPolizaVida = polizaVida ? CalcularPolizaVida(connection, monto) : 0m;
            var montoPolizaIncendio = polizaIncendio ? CalcularPolizaIncendio(connection, monto, montoConstruccion) : 0m;
            var montoPolizaPrenda = polizaPrenda ? CalcularPolizaPrenda(connection, monto) : 0m;
            var montoPolizaDesempleo = polizaDesempleo
                ? CalcularPolizaDesempleo(connection, cuota, montoPolizaVida, montoPolizaIncendio)
                : 0m;

            var compromiso = cuota + montoPolizaVida + montoPolizaIncendio + montoPolizaDesempleo + montoPolizaPrenda;

            return new CreditoRecalculoResultado(
                cuota,
                montoPolizaVida,
                montoPolizaIncendio,
                montoPolizaPrenda,
                montoPolizaDesempleo,
                compromiso,
                autoMarcada);
        }
    }
}
