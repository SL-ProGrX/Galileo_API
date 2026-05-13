using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.Security;
using Galileo_API.Models.ProGrX_EstudioCrd;
using System.Data;
using System.Globalization;

namespace Galileo_API.DataBaseTier.ProGrX_EstudioCrd
{
    public class FrmPreaBallonPaymentDB
    {
        private readonly PortalDB _portalDb;
        private readonly MSecurityMainDb _bitacora;
        private readonly int vModulo = 3;

        public FrmPreaBallonPaymentDB(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
            _bitacora = new MSecurityMainDb(config);
        }

        /// <summary>
        /// Carga la información inicial de frmPrea_BallonPayment.
        /// Obtiene condiciones, datos base del cálculo, ahorro especial y la lista de periodicidades.
        /// </summary>
        public ErrorDto<FrmPreaBallonPaymentCargarResponse> Prea_frmPrea_BallonPayment_Cargar(
            int codEmpresa,
            string usuario,
            string cod_preanalisis)
        {
            try
            {
                using var connection = _portalDb.CreateConnection(codEmpresa);

                var rawRow = connection.QueryFirstOrDefault(
                    @"EXEC spCrdPreaObtenerBalloonPayment_ProGrX @cod_preanalisis",
                    new { cod_preanalisis = cod_preanalisis.Trim() },
                    commandType: CommandType.Text);

                var row = ToDictionary(rawRow);

                var periodicidades = connection.Query<FrmPreaBallonPaymentPeriodicidadItem>(
                    @"SELECT CAST(ID_PERIODICIDAD AS int) AS idx,
                             RTRIM(DESCRIPCION) AS itmx
                      FROM CRD_PREA_PERIODICIDAD
                      WHERE ACTIVO = 1
                      ORDER BY ID_PERIODICIDAD",
                    commandType: CommandType.Text).ToList();

                var result = new FrmPreaBallonPaymentCargarResponse
                {
                    cod_preanalisis = cod_preanalisis.Trim(),
                    traslada_salario = GetBool(row, "TRASLADA_SALARIO", "traslada_salario"),
                    deduce_planilla = GetBool(row, "DEDUCE_PLANILLA", "deduce_planilla"),
                    monto = GetDecimal(row, "MONTO", "Monto", "monto"),
                    cuota = GetDecimal(row, "CUOTA", "Cuota", "cuota"),
                    cuota_balloon = GetDecimal(row, "CUOTA_BALLOON", "cuota_balloon"),
                    tasa = GetDecimal(row, "TASA", "tasa"),
                    plazo = GetInt(row, "PLAZO", "Plazo", "plazo"),
                    periodicidad = GetInt(row, "PERIODICIDAD", "periodicidad"),
                    periodicidad_desc = GetString(row, "PERIODICIDAD_DESC", "periodicidad_desc"),
                    codigo_plan = GetString(row, "COD_PLAN", "codigo_plan", "PLAN", "plan"),
                    no_contrato = GetString(row, "NO_CONTRATO", "no_contrato", "CONTRATO", "contrato"),
                    plazo_ahorro = GetString(row, "PLAZO_AHORRO", "plazo_ahorro", "PLAZO_FCOB", "plazo_fcob"),
                    monto_ahorro = GetDecimal(row, "MONTO_AHORRO", "monto_ahorro", "MONTO_FCOB", "monto_fcob"),
                    periodicidades = periodicidades
                };

                return DbHelper.CreateOkResponse(result);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<FrmPreaBallonPaymentCargarResponse>(ex.Message);
            }
        }

        /// <summary>
        /// Obtiene la tabla de pagos calculada para el expediente desde CRD_PREA_TABLA_PAGOS_BALLOON.
        /// </summary>
        public ErrorDto<FrmPreaBallonPaymentTablaPagosResponse> Prea_frmPrea_BallonPayment_TablaPagos_Obtener(
            int codEmpresa,
            string usuario,
            string cod_preanalisis)
        {
            try
            {
                using var connection = _portalDb.CreateConnection(codEmpresa);

                var tabla = connection.Query<FrmPreaBallonPaymentTablaPagoItem>(
                    @"SELECT CAST(ID_CUOTA AS int) AS id_cuota,
                             ISNULL(MONTO_CUOTA, 0) AS monto_cuota,
                             ISNULL(AMORTIZA, 0) AS amortiza,
                             ISNULL(INTERESES, 0) AS intereses,
                             ISNULL(MONTO_PRINCIPAL, 0) AS monto_principal
                      FROM dbo.CRD_PREA_TABLA_PAGOS_BALLOON
                      WHERE COD_PREANALISIS = @cod_preanalisis
                      ORDER BY ID_CUOTA ASC",
                    new { cod_preanalisis = cod_preanalisis.Trim() },
                    commandType: CommandType.Text).ToList();

                return DbHelper.CreateOkResponse(new FrmPreaBallonPaymentTablaPagosResponse
                {
                    tabla = tabla
                });
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<FrmPreaBallonPaymentTablaPagosResponse>(ex.Message);
            }
        }

        /// <summary>
        /// Ejecuta el cálculo de Balloon Payment y luego consulta la primera cuota generada.
        /// </summary>
        public ErrorDto<FrmPreaBallonPaymentCalcularResponse> Prea_frmPrea_BallonPayment_Calcular(
            int codEmpresa,
            FrmPreaBallonPaymentCalcularRequest request)
        {
            try
            {
                using var connection = _portalDb.CreateConnection(codEmpresa);
                connection.Open();

                connection.Execute(
                    @"EXEC spCRD_PREA_Calculo_BalloonPayment
                        @monto,
                        @tasa,
                        @plazo,
                        @periodicidad,
                        @cuota_balloon,
                        @cod_preanalisis",
                    new
                    {
                        monto = request.monto,
                        tasa = request.tasa,
                        plazo = request.plazo,
                        periodicidad = request.periodicidad,
                        cuota_balloon = request.cuota_balloon,
                        cod_preanalisis = request.cod_preanalisis.Trim()
                    },
                    commandType: CommandType.Text);

                var cuota = connection.QueryFirstOrDefault<decimal?>(
                    @"SELECT TOP 1 ISNULL(MONTO_CUOTA, 0)
                      FROM CRD_PREA_TABLA_PAGOS_BALLOON
                      WHERE COD_PREANALISIS = @cod_preanalisis
                      ORDER BY ID_CUOTA ASC",
                    new { cod_preanalisis = request.cod_preanalisis.Trim() },
                    commandType: CommandType.Text) ?? 0;

                return DbHelper.CreateOkResponse(new FrmPreaBallonPaymentCalcularResponse
                {
                    cod_preanalisis = request.cod_preanalisis.Trim(),
                    cuota = cuota,
                    mensaje = "Cálculo realizado correctamente."
                });
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<FrmPreaBallonPaymentCalcularResponse>(ex.Message);
            }
        }

        /// <summary>
        /// Guarda las condiciones de recuperación de mora del expediente.
        /// </summary>
        public ErrorDto<FrmPreaBallonPaymentCondicionesGuardarResponse> Prea_frmPrea_BallonPayment_Condiciones_Guardar(
            int codEmpresa,
            FrmPreaBallonPaymentCondicionesGuardarRequest request)
        {
            try
            {
                using var connection = _portalDb.CreateConnection(codEmpresa);
                connection.Open();

                connection.Execute(
                    @"EXEC spCrdPreaCondicionesRecuperaMora
                        @cod_preanalisis,
                        @traslada_salario,
                        @deduce_planilla",
                    new
                    {
                        cod_preanalisis = request.cod_preanalisis.Trim(),
                        traslada_salario = request.traslada_salario ? 1 : 0,
                        deduce_planilla = request.deduce_planilla ? 1 : 0
                    },
                    commandType: CommandType.Text);

                RegistrarBitacora(
                        codEmpresa,
                        request.usuario,
                        "Registra - WEB",
                        $"BallonPayment Condiciones, Expediente {request.cod_preanalisis.Trim()}, Traslado Salario [{(request.traslada_salario ? 1 : 0)}] Deduce de Planilla [{(request.deduce_planilla ? 1 : 0)}]"
                    );

                return DbHelper.CreateOkResponse(new FrmPreaBallonPaymentCondicionesGuardarResponse
                {
                    cod_preanalisis = request.cod_preanalisis.Trim(),
                    traslada_salario = request.traslada_salario,
                    deduce_planilla = request.deduce_planilla,
                    mensaje = "Condiciones actualizadas satisfactoriamente."
                });
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<FrmPreaBallonPaymentCondicionesGuardarResponse>(ex.Message);
            }
        }

        /// <summary>
        /// Guarda la configuración principal de Balloon Payment calculada para el expediente.
        /// </summary>
        public ErrorDto<FrmPreaBallonPaymentGuardarResponse> Prea_frmPrea_BallonPayment_Guardar(
            int codEmpresa,
            FrmPreaBallonPaymentGuardarRequest request)
        {
            try
            {
                using var connection = _portalDb.CreateConnection(codEmpresa);
                connection.Open();

                connection.Execute(
                    @"EXEC spCrdPreaRegistraBalloonRecuperaMora
                        @cod_preanalisis,
                        @periodicidad,
                        @tasa,
                        @plazo,
                        @cuota_balloon,
                        @cuota,
                        @monto,
                        @usuario",
                    new
                    {
                        cod_preanalisis = request.cod_preanalisis.Trim(),
                        periodicidad = request.periodicidad,
                        tasa = request.tasa,
                        plazo = request.plazo,
                        cuota_balloon = request.cuota_balloon,
                        cuota = request.cuota,
                        monto = request.monto,
                        usuario = request.usuario.Trim()
                    },
                    commandType: CommandType.Text);

                return DbHelper.CreateOkResponse(new FrmPreaBallonPaymentGuardarResponse
                {
                    cod_preanalisis = request.cod_preanalisis.Trim(),
                    mensaje = "Balloon Payment guardado correctamente."
                });
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<FrmPreaBallonPaymentGuardarResponse>(ex.Message);
            }
        }

        private static Dictionary<string, object?> ToDictionary(object? row)
        {
            var result = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);

            if (row is null)
            {
                return result;
            }

            if (row is IDictionary<string, object> dict)
            {
                foreach (var item in dict)
                {
                    result[item.Key] = item.Value;
                }

                return result;
            }

            foreach (var property in row.GetType().GetProperties())
            {
                result[property.Name] = property.GetValue(row);
            }

            return result;
        }

        private static string GetString(IDictionary<string, object?> row, params string[] keys)
        {
            foreach (var key in keys)
            {
                if (!row.TryGetValue(key, out var value) || value is null || value == DBNull.Value)
                {
                    continue;
                }

                return Convert.ToString(value)?.Trim() ?? string.Empty;
            }

            return string.Empty;
        }

        private static int GetInt(IDictionary<string, object?> row, params string[] keys)
        {
            foreach (var key in keys)
            {
                if (!row.TryGetValue(key, out var value) || value is null || value == DBNull.Value)
                {
                    continue;
                }

                if (value is int intValue)
                {
                    return intValue;
                }

                if (int.TryParse(Convert.ToString(value), out var parsed))
                {
                    return parsed;
                }
            }

            return 0;
        }

        private static decimal GetDecimal(IDictionary<string, object?> row, params string[] keys)
        {
            foreach (var key in keys)
            {
                if (!row.TryGetValue(key, out var value) || value is null || value == DBNull.Value)
                {
                    continue;
                }

                if (value is decimal decimalValue)
                {
                    return decimalValue;
                }

                if (value is double doubleValue)
                {
                    return Convert.ToDecimal(doubleValue);
                }

                if (value is float floatValue)
                {
                    return Convert.ToDecimal(floatValue);
                }

                var text = Convert.ToString(value) ?? string.Empty;

                if (decimal.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out var parsedInvariant))
                {
                    return parsedInvariant;
                }

                if (decimal.TryParse(text, NumberStyles.Any, CultureInfo.CurrentCulture, out var parsedCurrent))
                {
                    return parsedCurrent;
                }
            }

            return 0;
        }

        private static bool GetBool(IDictionary<string, object?> row, params string[] keys)
        {
            foreach (var key in keys)
            {
                if (!row.TryGetValue(key, out var value) || value is null || value == DBNull.Value)
                {
                    continue;
                }

                if (value is bool boolValue)
                {
                    return boolValue;
                }

                if (int.TryParse(Convert.ToString(value), out var intValue))
                {
                    return intValue != 0;
                }

                if (bool.TryParse(Convert.ToString(value), out var parsedBool))
                {
                    return parsedBool;
                }
            }

            return false;
        }

        private void RegistrarBitacora(int codEmpresa, string usuario, string movimiento, string detalle)
        {
            _bitacora.Bitacora(new BitacoraInsertarDto
            {
                EmpresaId = codEmpresa,
                Usuario = usuario?.Trim() ?? string.Empty,
                DetalleMovimiento = detalle,
                Movimiento = movimiento,
                Modulo = vModulo
            });
        }
    }
}
