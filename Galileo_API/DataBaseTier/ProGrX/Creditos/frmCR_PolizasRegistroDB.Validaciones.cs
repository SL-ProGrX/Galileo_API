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

                var operacionBaseResp = CrPolizasRegistro_OperacionBase_Obtener(codEmpresa, request.operacion);
                if (operacionBaseResp.Code != 0 || operacionBaseResp.Result is null)
                {
                    errores.Add("- No se encontr&oacute; la operaci&oacute;n base.");
                    return CrPolizasRegistro_FxVerificaRespuesta(errores);
                }

                var operacionBase = operacionBaseResp.Result;

                if (!CrPolizasRegistro_ClienteExiste(codEmpresa, operacionBase.cedula))
                {
                    errores.Add("- No Existe el cliente definido (debe de Ingresarlo)...");
                }

                var lineaResp = CrPolizasRegistro_PolizaRetencionData_Obtener(codEmpresa, request.poliza_linea);
                if (lineaResp.Code != 0 || lineaResp.Result is null)
                {
                    errores.Add("- No Existen Polizas Configuradas para usar...");
                    return CrPolizasRegistro_FxVerificaRespuesta(errores);
                }

                var lineaData = lineaResp.Result;

                if (lineaData.integra_plan_pagos == 1)
                {
                    errores.Add("- La l&iacute;nea seleccionada corresponde a una p&oacute;liza integrada al plan de pagos.");
                    return CrPolizasRegistro_FxVerificaRespuesta(errores);
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

                var polizaContableResp = CrPolizasRegistro_CodigoPolizaContable_Obtener(codEmpresa, lineaData.codigo_retencion);
                if (polizaContableResp.Code != 0 || polizaContableResp.Result is null)
                {
                    errores.Add("- El c&oacute;digo de la Poliza no existe");
                }
                else if (string.IsNullOrWhiteSpace(polizaContableResp.Result))
                {
                    errores.Add("- El c&oacute;digo no se encuentra codificado contablemente");
                }

                if (CrPolizasRegistro_MesNumero_Obtener(request.mes) <= 0)
                {
                    errores.Add("- El Mes para la primer deduccion no es v&aacute;lido");
                }

                decimal priDeduc = CrPolizasRegistro_PriDeduc_Crear(request.anio, request.mes);
                decimal fechaProcesoActual = CrPolizasRegistro_FechaProcesoActual_Obtener(codEmpresa, request.usuario);

                if (fechaProcesoActual > 0 && priDeduc <= fechaProcesoActual)
                {
                    errores.Add("- La primer deducci&oacute;n no es v&aacute;lida porque es igual o menor a la fecha de proceso actual");
                }

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

                var operacionBaseResp = CrPolizasRegistro_OperacionBase_Obtener(codEmpresa, request.operacion);
                if (operacionBaseResp.Code != 0 || operacionBaseResp.Result is null)
                {
                    errores.Add("- No se encontr&oacute; la operaci&oacute;n base.");
                    return CrPolizasRegistro_FxVerificaRespuesta(errores);
                }

                var operacionBase = operacionBaseResp.Result;

                if (!CrPolizasRegistro_ClienteExiste(codEmpresa, operacionBase.cedula))
                {
                    errores.Add("- No Existe el cliente definido (debe de Ingresarlo)...");
                }

                var lineaResp = CrPolizasRegistro_PolizaRetencionData_Obtener(codEmpresa, request.poliza_linea);
                if (lineaResp.Code != 0 || lineaResp.Result is null)
                {
                    errores.Add("- No Existen Polizas Configuradas para usar...");
                    return CrPolizasRegistro_FxVerificaRespuesta(errores);
                }

                var lineaData = lineaResp.Result;

                if (lineaData.integra_plan_pagos != 1)
                {
                    errores.Add("- La l&iacute;nea seleccionada no corresponde a una p&oacute;liza integrada al plan de pagos.");
                    return CrPolizasRegistro_FxVerificaRespuesta(errores);
                }

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

                if (!request.poliza_cobertura_inicio.HasValue || !request.poliza_cobertura_corte.HasValue)
                {
                    errores.Add("- La cobertura de la poliza no es v&aacute;lida verifique");
                }
                else if (request.poliza_cobertura_corte.Value <= request.poliza_cobertura_inicio.Value)
                {
                    errores.Add("- La cobertura de la poliza no es v&aacute;lida verifique");
                }

                return CrPolizasRegistro_FxVerificaRespuesta(errores);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse(
                    $"No fue posible verificar la p&oacute;liza integrada. {ex.Message}",
                    -1,
                    new CrPolizasRegistroFxVerificaData
                    {
                        valido = false,
                        mensaje = ex.Message
                    });
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