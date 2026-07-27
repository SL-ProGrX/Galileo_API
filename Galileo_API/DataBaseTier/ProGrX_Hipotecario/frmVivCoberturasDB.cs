using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_Hipotecario;
using Microsoft.Data.SqlClient;

namespace Galileo_API.DataBaseTier.ProGrX_Hipotecario
{
    public class FrmVivCoberturasDb
    {
        private readonly PortalDB _portalDb;

        public FrmVivCoberturasDb(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
        }

        /// <summary>
        /// Obtiene la carga inicial del formulario frmVivCoberturas.
        /// Replica la carga inicial de VB6: operación, socio, fincas asociadas y cobertura general.
        /// </summary>
        public ErrorDto<FrmVivCoberturasCargaResponse> Viv_Coberturas_Cargar(
            int codEmpresa,
            long numero_operacion)
        {
            var response = new ErrorDto<FrmVivCoberturasCargaResponse>
            {
                Code = 0,
                Description = "Ok",
                Result = new FrmVivCoberturasCargaResponse()
            };

            const string sqlOperacion = @"
SELECT TOP 1
    ISNULL(R.id_solicitud, 0) AS id_solicitud,
    RTRIM(ISNULL(R.cedula, '')) AS cedula,
    RTRIM(ISNULL(S.nombre, '')) AS nombre
FROM reg_creditos R
INNER JOIN Socios S
    ON R.cedula = S.cedula
WHERE R.id_solicitud = @numero_operacion;";

            const string sqlFincas = @"
SELECT
    RTRIM(ISNULL(NumeroFinca, '')) AS numero_finca,
    ISNULL(valorTerreno, 0) + ISNULL(ValorConstruccion, 0) AS avaluo
FROM viviendaGarantia
WHERE NumeroOperacion = @numero_operacion
ORDER BY NumeroFinca;";

            try
            {
                using var connection = DbHelper.OpenConnection(_portalDb, codEmpresa);
                connection.Open();

                response.Result.operacion =
                    connection.QueryFirstOrDefault<FrmVivCoberturasOperacionResponse>(
                        sqlOperacion,
                        new
                        {
                            numero_operacion = numero_operacion
                        })
                    ?? new FrmVivCoberturasOperacionResponse();

                response.Result.fincas =
                    connection.Query<FrmVivCoberturasFincaItem>(
                        sqlFincas,
                        new
                        {
                            numero_operacion = numero_operacion
                        }).ToList();

                var resumenRaw = ObtenerResumenRaw(
                    connection,
                    numero_operacion,
                    "general",
                    string.Empty);

                response.Result.resumen = MapearResumen(resumenRaw);
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = new FrmVivCoberturasCargaResponse();
            }

            return response;
        }

        /// <summary>
        /// Obtiene el resumen de cobertura general o individual.
        /// Replica spCRDViviendaCoberturaTotal y spCRDViviendaCoberturaIndividual de VB6.
        /// </summary>
        public ErrorDto<FrmVivCoberturasResumenResponse> Viv_CoberturasResumen_Obtener(
            int codEmpresa,
            FrmVivCoberturasResumenRequest request)
        {
            var response = new ErrorDto<FrmVivCoberturasResumenResponse>
            {
                Code = 0,
                Description = "Ok",
                Result = new FrmVivCoberturasResumenResponse()
            };

            try
            {
                using var connection = DbHelper.OpenConnection(_portalDb, codEmpresa);
                connection.Open();

                var resumenRaw = ObtenerResumenRaw(
                    connection,
                    request.numero_operacion,
                    request.modo_cobertura,
                    request.numero_finca);

                response.Result = MapearResumen(resumenRaw);
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = new FrmVivCoberturasResumenResponse();
            }

            return response;
        }

        private static FrmVivCoberturasResumenRawResponse ObtenerResumenRaw(
            SqlConnection connection,
            long numeroOperacion,
            string modoCobertura,
            string numeroFinca)
        {
            string modoNormalizado = NormalizarTexto(modoCobertura).ToUpperInvariant();

            if (modoNormalizado == "INDIVIDUAL")
            {
                const string sqlIndividual = @"
EXEC dbo.spCRDViviendaCoberturaIndividual
    @Operacion,
    @NumeroFinca;";

                return connection.QueryFirstOrDefault<FrmVivCoberturasResumenRawResponse>(
                    sqlIndividual,
                    new
                    {
                        Operacion = numeroOperacion,
                        NumeroFinca = NormalizarTexto(numeroFinca)
                    })
                    ?? new FrmVivCoberturasResumenRawResponse();
            }

            const string sqlGeneral = @"
EXEC dbo.spCRDViviendaCoberturaTotal
    @Operacion;";

            return connection.QueryFirstOrDefault<FrmVivCoberturasResumenRawResponse>(
                sqlGeneral,
                new
                {
                    Operacion = numeroOperacion
                })
                ?? new FrmVivCoberturasResumenRawResponse();
        }

        private static FrmVivCoberturasResumenResponse MapearResumen(
            FrmVivCoberturasResumenRawResponse raw)
        {
            return new FrmVivCoberturasResumenResponse
            {
                avaluo = raw.Avaluo,
                disponible = raw.disponible,
                hip_externa = raw.HipExterna,
                hip_interna = raw.HipInterna,
                hip_libera = raw.HipLibera,
                cobertura = raw.Cobertura
            };
        }

        private static string NormalizarTexto(string? valor)
        {
            return valor?.Trim() ?? string.Empty;
        }

    }
}
