using Dapper;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_EstudioCrd;
using System.Collections.Generic;
using System.Data;

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
                connection,
                new CreditoPolizasCalculoParametros
                {
                    Monto = monto,
                    Plazo = plazo,
                    Tasa = tasa,
                    MontoConstruccion = montoConstruccion,
                    PolizaVida = polizaVida,
                    PolizaIncendio = polizaIncendio,
                    PolizaPrenda = polizaPrenda,
                    PolizaDesempleo = polizaDesempleo
                });

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
                poliza_incendio = polizaIncendio || recalculo.PolizaIncendioAutoMarcada,
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
                        connection,
                        new CreditoCatalogoCalculoParametros
                        {
                            Linea = request.linea,
                            Destino = request.destino,
                            Garantia = request.garantia,
                            Cedula = request.cedula,
                            Estado = request.estado,
                            Monto = request.monto,
                            TasaActual = tasa,
                            PlazoActual = plazo
                        });
                }

                var polizaIncendio = request.poliza_incendio;

                var recalculo = RecalcularCreditoPolizas(
                    connection,
                    new CreditoPolizasCalculoParametros
                    {
                        Monto = request.monto,
                        Plazo = plazo,
                        Tasa = tasa,
                        MontoConstruccion = request.monto_construccion,
                        PolizaVida = request.poliza_vida,
                        PolizaIncendio = polizaIncendio,
                        PolizaPrenda = request.poliza_prenda,
                        PolizaDesempleo = request.poliza_desempleo
                    });

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
                    poliza_incendio = polizaIncendio || recalculo.PolizaIncendioAutoMarcada,
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

        private sealed class CreditoCatalogoCalculoParametros
        {
            public string Linea { get; init; } = string.Empty;
            public string Destino { get; init; } = string.Empty;
            public string Garantia { get; init; } = string.Empty;
            public string Cedula { get; init; } = string.Empty;
            public string Estado { get; init; } = string.Empty;
            public decimal Monto { get; init; }
            public decimal TasaActual { get; init; }
            public int PlazoActual { get; init; }
        }

        private sealed class CreditoPolizasCalculoParametros
        {
            public decimal Monto { get; init; }
            public int Plazo { get; init; }
            public decimal Tasa { get; init; }
            public decimal MontoConstruccion { get; init; }
            public bool PolizaVida { get; init; }
            public bool PolizaIncendio { get; init; }
            public bool PolizaPrenda { get; init; }
            public bool PolizaDesempleo { get; init; }
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
            const string sql = "SELECT dbo.fxCrdCatalogoRango(@Codigo, @Monto, @Tipo, @Destino, @Garantia)";
            return connection.QueryFirstOrDefault<decimal?>(sql, new
            {
                Codigo = codigo ?? string.Empty,
                Monto = monto,
                Tipo = tipo,
                Destino = destino ?? string.Empty,
                Garantia = garantia ?? string.Empty
            }) ?? 0m;
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

            const string sql = "SELECT dbo.fxCrdTasaBonifica(@Cedula, @Linea, @Garantia, @Destino, @Plazo)";
            return connection.QueryFirstOrDefault<decimal?>(sql, new
            {
                Cedula = cedula,
                Linea = linea,
                Garantia = garantia,
                Destino = destino ?? string.Empty,
                Plazo = plazo
            }) ?? 0m;
        }

        /// <summary>VB6: fxBonoPlazoMembresia(cedula, garantia) — mValidacion.bas línea
        /// 386-406: SELECT dbo.fxCrdPlazoBonifica(...).</summary>
        private static int CalcularBonoPlazoMembresia(IDbConnection connection, string cedula, string garantia)
        {
            if (string.IsNullOrWhiteSpace(cedula) || string.IsNullOrWhiteSpace(garantia))
            {
                return 0;
            }

            const string sql = "SELECT dbo.fxCrdPlazoBonifica(@Cedula, @Garantia)";
            return connection.QueryFirstOrDefault<int?>(sql, new
            {
                Cedula = cedula,
                Garantia = garantia
            }) ?? 0;
        }

        /// <summary>
        /// VB6: sbCalcularCuota, Case "txtMonto" (frmPreaEstudiov2.frm ~línea 14879-14911).
        /// Re-deriva Tasa/Plazo desde el catálogo por rango (línea/monto/destino/garantía) y
        /// aplica el bono de membresía cuando el Estado del expediente es 'R' o 'P'. No cubre
        /// la rama de Garantía "Y" (Fondos) con Tag de fondo ya establecido — para Fondos el
        /// Tasa/Plazo ya vienen fijados por Prea_frmPreaEstudiov2_Fondo_Calcular.
        /// </summary>
        private static (decimal tasa, int plazo, decimal tasaPtsBono) RecalcularTasaPlazoCatalogo(
            IDbConnection connection,
            CreditoCatalogoCalculoParametros parametros)
        {
            var tasa = parametros.TasaActual;
            var plazo = parametros.PlazoActual;

            var esFondo = string.Equals(parametros.Garantia.Trim(), "Y", StringComparison.OrdinalIgnoreCase);

            if (!esFondo && !string.IsNullOrWhiteSpace(parametros.Linea) && parametros.Monto > 0)
            {
                plazo = (int)CalcularCatalogoRango(
                    connection, parametros.Linea, parametros.Monto, "P", parametros.Destino, parametros.Garantia);
                tasa = CalcularCatalogoRango(
                    connection, parametros.Linea, parametros.Monto, "I", parametros.Destino, parametros.Garantia);
            }

            var tasaPtsBono = 0m;
            var estadoTrim = parametros.Estado.Trim();
            if (estadoTrim.Equals("R", StringComparison.OrdinalIgnoreCase) || estadoTrim.Equals("P", StringComparison.OrdinalIgnoreCase))
            {
                var bono = CalcularBonoMembresia(
                    connection,
                    parametros.Cedula,
                    parametros.Linea,
                    parametros.Garantia,
                    parametros.Destino,
                    plazo);
                var plazoBono = CalcularBonoPlazoMembresia(
                    connection, parametros.Cedula, parametros.Garantia);

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

            const string sql = "SELECT dbo.fxCrd_Prea_Poliza_Vida(@Monto)";
            return connection.QueryFirstOrDefault<decimal?>(sql, new { Monto = monto }) ?? 0m;
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
            const string sql = "SELECT dbo.fxCrd_Prea_Poliza_Incendio(@MontoBase)";
            return connection.QueryFirstOrDefault<decimal?>(sql, new { MontoBase = montoBase }) ?? 0m;
        }

        /// <summary>VB6: dbo.fxCrd_Prea_Poliza_Vehiculo(Monto) — sbCalculaPolizaDePrenda, línea ~15026.</summary>
        private static decimal CalcularPolizaPrenda(IDbConnection connection, decimal monto)
        {
            if (monto <= 0)
            {
                return 0m;
            }

            const string sql = "SELECT dbo.fxCrd_Prea_Poliza_Vehiculo(@Monto)";
            return connection.QueryFirstOrDefault<decimal?>(sql, new { Monto = monto }) ?? 0m;
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
            const string sql = "SELECT dbo.fxCrd_Prea_Poliza_Desempleo(@Monto)";
            return connection.QueryFirstOrDefault<decimal?>(sql, new { Monto = monto }) ?? 0m;
        }

        /// <summary>
        /// Recalcula Cuota, montos de pólizas y Compromiso en vivo, replicando la cadena
        /// sbCalcularCuota -&gt; sbCalculaPolizaDeVida/Incendio/Prenda/Desempleo -&gt;
        /// sbCalcularCompromiso de VB6. Se aplica siempre (no solo Estado R/P) porque así
        /// se observó en pantalla contra datos reales de LAB04 (expediente 5424).
        /// </summary>
        private static CreditoRecalculoResultado RecalcularCreditoPolizas(
            IDbConnection connection,
            CreditoPolizasCalculoParametros parametros)
        {
            var cuota = CalcularCuota(parametros.Monto, parametros.Plazo, parametros.Tasa);

            // VB6 (sbCalculaPolizaDeIncendio ~línea 14999): si hay Monto Construcción y el
            // checkbox está sin marcar, lo marca automáticamente.
            var autoMarcada = false;
            var polizaIncendio = parametros.PolizaIncendio;
            if (parametros.MontoConstruccion > 0 && !polizaIncendio)
            {
                polizaIncendio = true;
                autoMarcada = true;
            }

            var montoPolizaVida = parametros.PolizaVida ? CalcularPolizaVida(connection, parametros.Monto) : 0m;
            var montoPolizaIncendio = polizaIncendio
                ? CalcularPolizaIncendio(connection, parametros.Monto, parametros.MontoConstruccion)
                : 0m;
            var montoPolizaPrenda = parametros.PolizaPrenda ? CalcularPolizaPrenda(connection, parametros.Monto) : 0m;
            var montoPolizaDesempleo = parametros.PolizaDesempleo
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
