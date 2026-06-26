using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX.Creditos;

namespace Galileo_API.DataBaseTier.ProGrX.Creditos
{
    public partial class FrmCrPolizasRegistroDb
    {
        private ErrorDto<CrPolizasRegistroFxVerificaData> CrPolizasRegistro_FxVerifica(
            int codEmpresa,
            CrPolizasRegistroPolizaRetencionGuardarRequest request)
        {
            try
            {
                List<string> errores = new();

                var operacionBase = CrPolizasRegistro_OperacionBaseValidar(
                    codEmpresa,
                    request.operacion,
                    errores);

                if (operacionBase is null)
                {
                    return CrPolizasRegistro_FxVerificaRespuesta(errores);
                }

                var lineaData = CrPolizasRegistro_LineaPolizaValidar(
                    codEmpresa,
                    request.poliza_linea,
                    requiereIntegrada: false,
                    errores);

                if (lineaData is null)
                {
                    return CrPolizasRegistro_FxVerificaRespuesta(errores);
                }

                CrPolizasRegistro_ValidarCamposRetencion(
                    codEmpresa,
                    request,
                    operacionBase,
                    lineaData,
                    errores);

                return CrPolizasRegistro_FxVerificaRespuesta(errores);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse(
                    $"No fue posible verificar la p&oacute;liza de retenci&oacute;n. {ex.Message}",
                    -1,
                    new CrPolizasRegistroFxVerificaData
                    {
                        valido = false,
                        mensaje = ex.Message
                    });
            }
        }

        private ErrorDto<CrPolizasRegistroFxVerificaData> CrPolizasRegistro_FxVerifica(
            int codEmpresa,
            CrPolizasRegistroPolizaIntegradaGuardarRequest request)
        {
            try
            {
                List<string> errores = new();

                var operacionBase = CrPolizasRegistro_OperacionBaseValidar(
                    codEmpresa,
                    request.operacion,
                    errores);

                if (operacionBase is null)
                {
                    return CrPolizasRegistro_FxVerificaRespuesta(errores);
                }

                var lineaData = CrPolizasRegistro_LineaPolizaValidar(
                    codEmpresa,
                    request.poliza_linea,
                    requiereIntegrada: true,
                    errores);

                if (lineaData is null)
                {
                    return CrPolizasRegistro_FxVerificaRespuesta(errores);
                }

                CrPolizasRegistro_ValidarCamposIntegrada(
                    codEmpresa,
                    request,
                    errores);

                return CrPolizasRegistro_FxVerificaRespuesta(errores);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse(
                    ex.Message,
                    -1,
                    new CrPolizasRegistroFxVerificaData
                    {
                        valido = false,
                        mensaje = ex.Message
                    });
            }
        }

        private CrPolizasRegistroOperacionBaseData? CrPolizasRegistro_OperacionBaseValidar(
            int codEmpresa,
            int operacion,
            List<string> errores)
        {
            var operacionBaseResp = CrPolizasRegistro_OperacionBase_Obtener(codEmpresa, operacion);
            if (operacionBaseResp.Code != 0 || operacionBaseResp.Result is null)
            {
                errores.Add(MensajeOperacionBaseNoEncontrada);
                return null;
            }

            return operacionBaseResp.Result;
        }

        private CrPolizasRegistroPolizaRetencionData? CrPolizasRegistro_LineaPolizaValidar(
            int codEmpresa,
            string polizaLinea,
            bool requiereIntegrada,
            List<string> errores)
        {
            var lineaResp = CrPolizasRegistro_PolizaRetencionData_Obtener(codEmpresa, polizaLinea);
            if (lineaResp.Code != 0 || lineaResp.Result is null)
            {
                errores.Add(MensajeNoExistenPolizasConfiguradas);
                return null;
            }

            var lineaData = lineaResp.Result;

            if (requiereIntegrada && lineaData.integra_plan_pagos != 1)
            {
                errores.Add("- La l&iacute;nea seleccionada no corresponde a una p&oacute;liza integrada al plan de pagos.");
                return null;
            }

            if (!requiereIntegrada && lineaData.integra_plan_pagos == 1)
            {
                errores.Add("- La l&iacute;nea seleccionada corresponde a una p&oacute;liza integrada al plan de pagos.");
                return null;
            }

            return lineaData;
        }

        private void CrPolizasRegistro_ValidarCamposRetencion(
            int codEmpresa,
            CrPolizasRegistroPolizaRetencionGuardarRequest request,
            CrPolizasRegistroOperacionBaseData operacionBase,
            CrPolizasRegistroPolizaRetencionData lineaData,
            List<string> errores)
        {
            if (!CrPolizasRegistro_ClienteExiste(codEmpresa, operacionBase.cedula))
            {
                errores.Add("- No Existe el cliente definido (debe de Ingresarlo)...");
            }

            if (string.IsNullOrWhiteSpace(request.documento))
            {
                errores.Add("- No se especific&oacute; el # Documento ?");
            }

            if (request.plazo < 1)
            {
                errores.Add("- El Plazo definido no es v&aacute;lido");
            }

            if (string.IsNullOrWhiteSpace(request.garantia))
            {
                errores.Add("- No se especific&oacute; el tipo de garant&iacute;a");
            }

            if (request.monto < 1)
            {
                errores.Add("- El Monto de la P&oacute;liza no es v&aacute;lido");
            }

            if (!CrPolizasRegistro_OperacionMadreValida(codEmpresa, operacionBase.codigo))
            {
                errores.Add("- La L&iacute;nea de la Operacion madre no es un cr&eacute;dito o no es v&aacute;lido...");
            }

            if (CrPolizasRegistro_PolizaOperacionExiste(codEmpresa, request.operacion, lineaData.codigo_retencion))
            {
                errores.Add("- Ya existe una P&oacute;liza activa para esta operaci&oacute;n de cr&eacute;dito...");
            }

            CrPolizasRegistro_ValidarCodigoPolizaContable(
                codEmpresa,
                lineaData.codigo_retencion,
                errores);

            CrPolizasRegistro_ValidarPrimeraDeduccion(
                codEmpresa,
                request.anio,
                request.mes,
                request.usuario,
                errores);
        }

        private void CrPolizasRegistro_ValidarCamposIntegrada(
            int codEmpresa,
            CrPolizasRegistroPolizaIntegradaGuardarRequest request,
            List<string> errores)
        {
            if (request.poliza_monto < 1)
            {
                errores.Add("- La mensualidad de la P&oacute;liza no es v&aacute;lida");
            }

            if (request.poliza_plazo_credito && request.poliza_cuota_resto_plazo < 1)
            {
                errores.Add("- La mensualidad de la P&oacute;liza para el resto del Plan no es v&aacute;lida");
            }

            if (!CrPolizasRegistro_PlanPagos_Disponible(codEmpresa, request.operacion))
            {
                errores.Add("- No existen cuotas disponibles dentro del Plan de Pagos en donde registrar la Poliza");
            }
            else if (request.poliza_plan <= 0)
            {
                errores.Add("- Debe seleccionar una cuota v&aacute;lida dentro del Plan de Pagos.");
            }

            bool coberturaInvalida =
                !request.poliza_cobertura_inicio.HasValue
                || !request.poliza_cobertura_corte.HasValue
                || request.poliza_cobertura_corte.Value <= request.poliza_cobertura_inicio.Value;

            if (coberturaInvalida)
            {
                errores.Add(MensajeCoberturaPolizaInvalida);
            }
        }

        private void CrPolizasRegistro_ValidarCodigoPolizaContable(
            int codEmpresa,
            string codigoPoliza,
            List<string> errores)
        {
            var polizaContableResp = CrPolizasRegistro_CodigoPolizaContable_Obtener(codEmpresa, codigoPoliza);
            if (polizaContableResp.Code != 0 || polizaContableResp.Result is null)
            {
                errores.Add("- El c&oacute;digo de la Poliza no existe");
                return;
            }

            if (string.IsNullOrWhiteSpace(polizaContableResp.Result))
            {
                errores.Add("- El c&oacute;digo no se encuentra codificado contablemente");
            }
        }

        private void CrPolizasRegistro_ValidarPrimeraDeduccion(
            int codEmpresa,
            int anio,
            string mes,
            string usuario,
            List<string> errores)
        {
            if (CrPolizasRegistro_MesNumero_Obtener(mes) <= 0)
            {
                errores.Add("- El Mes para la primer deduccion no es v&aacute;lido");
                return;
            }

            decimal priDeduc = CrPolizasRegistro_PriDeduc_Crear(anio, mes);
            decimal fechaProcesoActual = CrPolizasRegistro_FechaProcesoActual_Obtener(codEmpresa, usuario);

            if (fechaProcesoActual > 0 && priDeduc <= fechaProcesoActual)
            {
                errores.Add("- La primer deducci&oacute;n no es v&aacute;lida porque es igual o menor a la fecha de proceso actual");
            }
        }

        private static ErrorDto<CrPolizasRegistroFxVerificaData> CrPolizasRegistro_FxVerificaRespuesta(
            List<string> errores)
        {
            string mensaje = string.Join("<br>", errores);

            return DbHelper.CreateOkResponse(new CrPolizasRegistroFxVerificaData
            {
                valido = errores.Count == 0,
                mensaje = mensaje
            });
        }

        private bool CrPolizasRegistro_ClienteExiste(int codEmpresa, string cedula)
        {
            const string sql = @"
                select isnull(count(*), 0)
                from socios
                where cedula = @Cedula;";

            return DbHelper.ExecuteSingleQuery<int>(
                _portalDb,
                codEmpresa,
                sql,
                0,
                new { Cedula = (cedula ?? string.Empty).Trim() }).Result > 0;
        }

        private bool CrPolizasRegistro_OperacionMadreValida(int codEmpresa, string codigo)
        {
            const string sql = @"
                select isnull(count(*), 0)
                from catalogo
                where retencion = 'N'
                  and poliza = 'N'
                  and codigo = @Codigo;";

            return DbHelper.ExecuteSingleQuery<int>(
                _portalDb,
                codEmpresa,
                sql,
                0,
                new { Codigo = (codigo ?? string.Empty).Trim() }).Result > 0;
        }

        private bool CrPolizasRegistro_PolizaOperacionExiste(int codEmpresa, int operacion, string codigoPoliza)
        {
            const string sql = @"
                select isnull(count(*), 0)
                from CRD_OPERACION_POLIZAS
                where cod_poliza = @CodigoPoliza
                  and id_solicitud = @Operacion;";

            return DbHelper.ExecuteSingleQuery<int>(
                _portalDb,
                codEmpresa,
                sql,
                0,
                new
                {
                    CodigoPoliza = (codigoPoliza ?? string.Empty).Trim(),
                    Operacion = operacion
                }).Result > 0;
        }

        private ErrorDto<string?> CrPolizasRegistro_CodigoPolizaContable_Obtener(int codEmpresa, string codigoPoliza)
        {
            const string sql = @"
                select top 1
                    convert(varchar(100), ctaNintC) as ctaNintC
                from catalogo
                where codigo = @CodigoPoliza;";

            return DbHelper.ExecuteSingleQuery<string>(
                _portalDb,
                codEmpresa,
                sql,
                null,
                new { CodigoPoliza = (codigoPoliza ?? string.Empty).Trim() });
        }

        private bool CrPolizasRegistro_PlanPagos_Disponible(
            int codEmpresa,
            int operacion)
        {
            const string sql = @"
                select isnull(count(*), 0)
                from CRD_OPERACION_TRANSAC
                where id_solicitud = @Operacion
                  and estado = 'A';";

            return DbHelper.ExecuteSingleQuery<int>(
                _portalDb,
                codEmpresa,
                sql,
                0,
                new { Operacion = operacion }).Result > 0;
        }
    }
}