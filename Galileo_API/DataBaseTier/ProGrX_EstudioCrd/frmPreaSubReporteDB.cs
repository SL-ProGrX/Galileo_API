using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_EstudioCrd;
using System.Data;

namespace Galileo_API.DataBaseTier.ProGrX_EstudioCrd
{
    public class FrmPreaSubReporteDB
    {
        private readonly PortalDB _portalDb;

        public FrmPreaSubReporteDB(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
        }

        /// <summary>
        /// Carga la configuración inicial de frmPreaSubReporte respetando
        /// el comportamiento base del formulario VB6.
        /// </summary>
        public ErrorDto<FrmPreaSubReporteCargarResponse> Prea_frmPreaSubReporte_Cargar(
            int codEmpresa,
            FrmPreaSubReporteCargarRequest request)
        {
            try
            {
                using var connection = _portalDb.CreateConnection(codEmpresa);
                connection.Open();

                var codPreanalisis = request.cod_preanalisis?.Trim() ?? string.Empty;
                var esSubExpediente = codPreanalisis.Contains('-');

                var subExpedientes = connection.Query<FrmPreaSubReporteBaseData>(
                    @"EXEC spCrdPrea_Reportes_Informacion_Base @Expediente, @Tipo",
                    new
                    {
                        Expediente = codPreanalisis,
                        Tipo = "S"
                    },
                    commandType: CommandType.Text
                ).ToList();

                var tieneSubExpedientes = subExpedientes.Count > 0 && !esSubExpediente;

                var result = new FrmPreaSubReporteCargarResponse
                {
                    cod_preanalisis = codPreanalisis,
                    titulo = $"Expediente : {codPreanalisis}",

                    chk_resumen = !esSubExpediente,
                    chk_detalle = !esSubExpediente,
                    chk_ficha_convenio = false,
                    chk_estado_cuenta = false,
                    chk_deducciones = false,
                    chk_impresora = true,

                    chk_sub_expediente = tieneSubExpedientes,
                    chk_sub_expediente_resumen = tieneSubExpedientes,
                    chk_sub_expediente_detalle = false,
                    chk_sub_expediente_estado = false,

                    habilita_resumen = !esSubExpediente,
                    habilita_detalle = !esSubExpediente,
                    habilita_ficha_convenio = !esSubExpediente,
                    habilita_estado_cuenta = false,
                    habilita_sub_expediente = tieneSubExpedientes,
                    habilita_sub_expediente_resumen = tieneSubExpedientes,
                    habilita_sub_expediente_detalle = false,
                    habilita_sub_expediente_estado = false
                };

                return DbHelper.CreateOkResponse(result);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<FrmPreaSubReporteCargarResponse>(ex.Message);
            }
        }

        /// <summary>
        /// Construye la lista de reportes a imprimir para frmPreaSubReporte
        /// respetando la secuencia funcional del VB6.
        /// </summary>
        public ErrorDto<FrmPreaSubReporteImprimirResponse> Prea_frmPreaSubReporte_Imprimir_Obtener(
            int codEmpresa,
            FrmPreaSubReporteImprimirRequest request)
        {
            try
            {
                using var connection = _portalDb.CreateConnection(codEmpresa);
                connection.Open();

                var codPreanalisis = request.cod_preanalisis?.Trim() ?? string.Empty;
                var reportes = new List<FrmReporteGlobal>();

                var expedienteMaestro = connection.QueryFirstOrDefault<FrmPreaSubReporteInformacionBaseData>(
                    @"EXEC spCrdPrea_Reportes_Informacion_Base @Expediente, @Tipo",
                    new
                    {
                        Expediente = codPreanalisis,
                        Tipo = "E"
                    }
                );

                if (expedienteMaestro is not null)
                {
                    AgregarReportesExpediente(
                        reportes,
                        codEmpresa,
                        request.usuario,
                        expedienteMaestro.cod_preanalisis?.Trim() ?? codPreanalisis,
                        request.chk_resumen,
                        request.chk_detalle,
                        request.chk_ficha_convenio,
                        request.chk_estado_cuenta,
                        request.chk_deducciones
                    );
                }

                if (request.chk_sub_expediente &&
                    (request.chk_sub_expediente_resumen || request.chk_sub_expediente_detalle || request.chk_sub_expediente_estado))
                {
                    var subExpedientes = connection.Query<FrmPreaSubReporteInformacionBaseData>(
                        @"EXEC spCrdPrea_Reportes_Informacion_Base @Expediente, @Tipo",
                        new
                        {
                            Expediente = codPreanalisis,
                            Tipo = "S"
                        }
                    ).ToList();

                    foreach (var item in subExpedientes)
                    {
                        var estadoActual = (item.estado_actual ?? string.Empty).Trim();

                        AgregarReportesExpediente(
                            reportes,
                            codEmpresa,
                            request.usuario,
                            item.cod_preanalisis?.Trim() ?? string.Empty,
                            request.chk_sub_expediente_resumen,
                            request.chk_sub_expediente_detalle,
                            false,
                            request.chk_sub_expediente_estado && estadoActual == "S",
                            false
                        );
                    }
                }

                return DbHelper.CreateOkResponse(new FrmPreaSubReporteImprimirResponse
                {
                    reportes = reportes
                });
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<FrmPreaSubReporteImprimirResponse>(ex.Message);
            }
        }

        private static void AgregarReportesExpediente(
            List<FrmReporteGlobal> reportes,
            int codEmpresa,
            string? usuario,
            string codPreanalisis,
            bool chkResumen,
            bool chkDetalle,
            bool chkFichaConvenio,
            bool chkEstadoCuenta,
            bool chkDeducciones)
        {
            if (string.IsNullOrWhiteSpace(codPreanalisis))
            {
                return;
            }

            if (chkResumen)
            {
                reportes.Add(CrearReporte(
                    codEmpresa,
                    usuario,
                    chkDeducciones ? "Credito_Analisis_FichaResumenWsec" : "Credito_Analisis_FichaResumen",
                    codPreanalisis
                ));
            }

            if (chkFichaConvenio)
            {
                reportes.Add(CrearReporte(
                    codEmpresa,
                    usuario,
                    "Credito_Analisis_FichaConvenio",
                    codPreanalisis
                ));
            }

            if (chkDetalle)
            {
                reportes.Add(CrearReporte(
                    codEmpresa,
                    usuario,
                    "Credito_Analisis_FichaDetalle",
                    codPreanalisis
                ));
            }

            if (chkEstadoCuenta)
            {
                reportes.Add(CrearReporte(
                    codEmpresa,
                    usuario,
                    "CrdPreaEstadoCuenta",
                    codPreanalisis
                ));
            }
        }

        private static FrmReporteGlobal CrearReporte(
            int codEmpresa,
            string? usuario,
            string nombreReporte,
            string codPreanalisis)
        {
            return new FrmReporteGlobal
            {
                codEmpresa = codEmpresa,
                usuario = usuario?.Trim(),
                nombreReporte = nombreReporte,
                cod_reporte = "P",
                folder = "EstudioCrd",
                parametros = $$"""
                {
                  "CODPREANALISIS": "{{codPreanalisis.Trim()}}"
                }
                """
            };
        }
    }
}
