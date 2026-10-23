using System.Data;
using System.Security;
using Dapper;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Inversiones;
using Microsoft.Data.SqlClient;

namespace Galileo.DataBaseTier.ProGrX_Inversiones
{
    public sealed class FrmIvrCatCuentasContablesDb
    {
        private const int CodigoMensajeControlado = -2;

        private const string ProcedimientoGuardar =
            "dbo.spIVR_CUENTAS_REGISTRA";

        private const string MensajeActualizado =
            "Información actualizada satisfactoriamente.";

        private const string SqlCuentas = """
            SELECT TOP (1)
                RTRIM(ISNULL(TIPO_BASE, ''))
                    AS tipo_base,
                RTRIM(ISNULL(CODIGO_01, ''))
                    AS codigo_01,
                RTRIM(ISNULL(CODIGO_02, ''))
                    AS codigo_02,
                RTRIM(ISNULL(COD_UNIDAD, ''))
                    AS cod_unidad,
                RTRIM(ISNULL(COD_CENTRO_COSTO, ''))
                    AS cod_centro_costo,
                RTRIM(ISNULL(UNIDAD_DESC, ''))
                    AS unidad_desc,
                RTRIM(ISNULL(CENTRO_DESC, ''))
                    AS centro_desc,
                RTRIM(ISNULL(CTA_INVERSION_MASK, ''))
                    AS cta_inversion_mask,
                RTRIM(ISNULL(CTA_INVERSION_DESC, ''))
                    AS cta_inversion_desc,
                RTRIM(ISNULL(
                    CTA_INTERESES_ACUM_COBRAR_MASK,
                    ''
                ))
                    AS cta_intereses_acum_cobrar_mask,
                RTRIM(ISNULL(
                    CTA_INTERESES_ACUM_COBRAR_DESC,
                    ''
                ))
                    AS cta_intereses_acum_cobrar_desc,
                RTRIM(ISNULL(
                    CTA_INGRESOS_INTERESES_MASK,
                    ''
                ))
                    AS cta_ingresos_intereses_mask,
                RTRIM(ISNULL(
                    CTA_INGRESOS_INTERESES_DESC,
                    ''
                ))
                    AS cta_ingresos_intereses_desc,
                RTRIM(ISNULL(
                    CTA_INGRESOS_FLUCTUACIONES_MASK,
                    ''
                ))
                    AS cta_ingresos_fluctuaciones_mask,
                RTRIM(ISNULL(
                    CTA_INGRESOS_FLUCTUACIONES_DESC,
                    ''
                ))
                    AS cta_ingresos_fluctuaciones_desc,
                RTRIM(ISNULL(
                    CTA_GASTO_COMISIONES_MASK,
                    ''
                ))
                    AS cta_gasto_comisiones_mask,
                RTRIM(ISNULL(
                    CTA_GASTO_COMISIONES_DESC,
                    ''
                ))
                    AS cta_gasto_comisiones_desc,
                RTRIM(ISNULL(CTA_PRIMA_MASK, ''))
                    AS cta_prima_mask,
                RTRIM(ISNULL(CTA_PRIMA_DESC, ''))
                    AS cta_prima_desc,
                RTRIM(ISNULL(
                    CTA_PRIMA_GASTO_AMORT_MASK,
                    ''
                ))
                    AS cta_prima_gasto_amort_mask,
                RTRIM(ISNULL(
                    CTA_PRIMA_GASTO_AMORT_DESC,
                    ''
                ))
                    AS cta_prima_gasto_amort_desc,
                RTRIM(ISNULL(CTA_DESCUENTOS_MASK, ''))
                    AS cta_descuentos_mask,
                RTRIM(ISNULL(CTA_DESCUENTOS_DESC, ''))
                    AS cta_descuentos_desc,
                RTRIM(ISNULL(
                    CTA_DESCUENTO_INGRESO_MASK,
                    ''
                ))
                    AS cta_descuento_ingreso_mask,
                RTRIM(ISNULL(
                    CTA_DESCUENTO_INGRESO_DESC,
                    ''
                ))
                    AS cta_descuento_ingreso_desc,
                RTRIM(ISNULL(CTA_VAL_ACTIVO_MASK, ''))
                    AS cta_val_activo_mask,
                RTRIM(ISNULL(CTA_VAL_ACTIVO_DESC, ''))
                    AS cta_val_activo_desc,
                RTRIM(ISNULL(
                    CTA_VAL_PATRIMONIO_MASK,
                    ''
                ))
                    AS cta_val_patrimonio_mask,
                RTRIM(ISNULL(
                    CTA_VAL_PATRIMONIO_DESC,
                    ''
                ))
                    AS cta_val_patrimonio_desc,
                RTRIM(ISNULL(
                    CTA_VAL_RESULTADOS_GASTOS_MASK,
                    ''
                ))
                    AS cta_val_resultados_gastos_mask,
                RTRIM(ISNULL(
                    CTA_VAL_RESULTADOS_GASTOS_DESC,
                    ''
                ))
                    AS cta_val_resultados_gastos_desc,
                RTRIM(ISNULL(
                    CTA_VAL_RESULTADOS_INGRESOS_MASK,
                    ''
                ))
                    AS cta_val_resultados_ingresos_mask,
                RTRIM(ISNULL(
                    CTA_VAL_RESULTADOS_INGRESOS_DESC,
                    ''
                ))
                    AS cta_val_resultados_ingresos_desc,
                RTRIM(ISNULL(
                    CTA_RECOMPRA_PRINCIPAL_MASK,
                    ''
                ))
                    AS cta_recompra_principal_mask,
                RTRIM(ISNULL(
                    CTA_RECOMPRA_PRINCIPAL_DESC,
                    ''
                ))
                    AS cta_recompra_principal_desc,
                RTRIM(ISNULL(
                    CTA_RECOMPRA_INTERESES_MASK,
                    ''
                ))
                    AS cta_recompra_intereses_mask,
                RTRIM(ISNULL(
                    CTA_RECOMPRA_INTERESES_DESC,
                    ''
                ))
                    AS cta_recompra_intereses_desc,
                RTRIM(ISNULL(
                    CTA_INTERESES_PAGAR_MASK,
                    ''
                ))
                    AS cta_intereses_pagar_mask,
                RTRIM(ISNULL(
                    CTA_INTERESES_PAGAR_DESC,
                    ''
                ))
                    AS cta_intereses_pagar_desc,
                RTRIM(ISNULL(
                    CTA_GASTO_INTERESES_MASK,
                    ''
                ))
                    AS cta_gasto_intereses_mask,
                RTRIM(ISNULL(
                    CTA_GASTO_INTERESES_DESC,
                    ''
                ))
                    AS cta_gasto_intereses_desc,
                RTRIM(ISNULL(REGISTRO_USUARIO, ''))
                    AS registro_usuario,
                REGISTRO_FECHA
                    AS registro_fecha,
                RTRIM(ISNULL(ACTUALIZA_USUARIO, ''))
                    AS actualiza_usuario,
                ACTUALIZA_FECHA
                    AS actualiza_fecha
            FROM vIVR_CUENTAS
            WHERE TIPO_BASE = @tipo
              AND CODIGO_01 = @codigo_01
              AND CODIGO_02 = @codigo_02;
            """;

        private const string SqlUnidades = """
            SELECT
                RTRIM(ISNULL(COD_UNIDAD, ''))
                    AS item,
                RTRIM(ISNULL(DESCRIPCION, ''))
                    AS descripcion
            FROM CntX_Unidades
            WHERE COD_CONTABILIDAD = @cod_contabilidad
            ORDER BY COD_UNIDAD;
            """;

        private const string SqlCentrosCostos = """
            SELECT
                RTRIM(ISNULL(COD_CENTRO_COSTO, ''))
                    AS item,
                RTRIM(ISNULL(DESCRIPCION, ''))
                    AS descripcion
            FROM CntX_Centro_Costos
            WHERE COD_CONTABILIDAD = @cod_contabilidad
              AND ACTIVO = 1
            ORDER BY COD_CENTRO_COSTO;
            """;

        private static readonly HashSet<string> TiposValidos =
            new(StringComparer.OrdinalIgnoreCase)
            {
                "C",
                "P",
                "R",
                "X"
            };

        private readonly PortalDB _portalDb;

        public FrmIvrCatCuentasContablesDb(
            IConfiguration config)
        {
            ArgumentNullException.ThrowIfNull(config);

            _portalDb = new PortalDB(config);
        }

        /// <summary>
        /// Inicializa la configuración contable, las unidades y
        /// los centros de costo requeridos por el diálogo.
        /// </summary>
        /// <param name="codEmpresa">
        /// Código de la empresa activa.
        /// </param>
        /// <param name="request">
        /// Contexto del catálogo y contabilidad enlazada.
        /// </param>
        /// <returns>
        /// Información completa requerida por el diálogo.
        /// </returns>
        public ErrorDto<IvrCatCuentasContablesInicialData?>
            IVR_Cat_Cuentas_Contables_Inicializar(
                int codEmpresa,
                IvrCatCuentasContablesInicializarRequest? request)
        {
            var validacion =
                IVR_Cat_Cuentas_Contables_Inicializar_Validar(
                    request);

            if (!string.IsNullOrEmpty(validacion))
            {
                return DbHelper.CreateErrorResponse
                    <IvrCatCuentasContablesInicialData?>(
                        validacion,
                        CodigoMensajeControlado);
            }

            var contexto =
                IVR_Cat_Cuentas_Contables_Contexto_Normalizar(
                    request!.tipo,
                    request.codigo_01,
                    request.codigo_02);

            try
            {
                using var connection =
                    DbHelper.OpenConnection(
                        _portalDb,
                        codEmpresa);

                var cuentas =
                    IVR_Cat_Cuentas_Contables_Consultar(
                        connection,
                        contexto.tipo,
                        contexto.codigo_01,
                        contexto.codigo_02)
                    ?? IVR_Cat_Cuentas_Contables_Vacio_Crear(
                        contexto.tipo,
                        contexto.codigo_01,
                        contexto.codigo_02);

                var unidades =
                    IVR_Cat_Cuentas_Contables_Unidades_Consultar(
                        connection,
                        request.cod_contabilidad);

                var centrosCostos =
                    IVR_Cat_Cuentas_Contables_Centros_Consultar(
                        connection,
                        request.cod_contabilidad);

                return DbHelper.CreateOkResponse
                    <IvrCatCuentasContablesInicialData?>(
                        new IvrCatCuentasContablesInicialData
                        {
                            cuentas = cuentas,
                            unidades = unidades,
                            centros_costos = centrosCostos
                        });
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse
                    <IvrCatCuentasContablesInicialData?>(
                        ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return DbHelper.CreateErrorResponse
                    <IvrCatCuentasContablesInicialData?>(
                        ex.Message);
            }
            catch (SecurityException ex)
            {
                return DbHelper.CreateErrorResponse
                    <IvrCatCuentasContablesInicialData?>(
                        ex.Message);
            }
        }

        /// <summary>
        /// Registra o actualiza la configuración contable asociada
        /// al catálogo de Inversiones.
        /// </summary>
        /// <param name="codEmpresa">
        /// Código de la empresa activa.
        /// </param>
        /// <param name="request">
        /// Configuración contable que debe guardarse.
        /// </param>
        /// <returns>
        /// Configuración guardada y nuevamente consultada.
        /// </returns>
        public ErrorDto<IvrCatCuentasContablesData?>
            IVR_Cat_Cuentas_Contables_Guardar(
                int codEmpresa,
                IvrCatCuentasContablesGuardarRequest? request)
        {
            var validacion =
                IVR_Cat_Cuentas_Contables_Guardar_Validar(
                    request);

            if (!string.IsNullOrEmpty(validacion))
            {
                return DbHelper.CreateErrorResponse
                    <IvrCatCuentasContablesData?>(
                        validacion,
                        CodigoMensajeControlado);
            }

            var parametros =
                IVR_Cat_Cuentas_Contables_Parametros_Crear(
                    request!);

            try
            {
                using var connection =
                    DbHelper.OpenConnection(
                        _portalDb,
                        codEmpresa);

                _ = connection.Execute(
                    ProcedimientoGuardar,
                    parametros,
                    commandType: CommandType.StoredProcedure);

                var resultado =
                    IVR_Cat_Cuentas_Contables_Consultar(
                        connection,
                        parametros.Tipo,
                        parametros.Codigo_01,
                        parametros.Codigo_02);

                if (resultado is null)
                {
                    return DbHelper.CreateErrorResponse
                        <IvrCatCuentasContablesData?>(
                            "No fue posible recuperar la configuración "
                            + "contable después de actualizarla.");
                }

                return DbHelper.CreateOkResponse
                    <IvrCatCuentasContablesData?>(
                        resultado,
                        MensajeActualizado);
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse
                    <IvrCatCuentasContablesData?>(
                        ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return DbHelper.CreateErrorResponse
                    <IvrCatCuentasContablesData?>(
                        ex.Message);
            }
            catch (SecurityException ex)
            {
                return DbHelper.CreateErrorResponse
                    <IvrCatCuentasContablesData?>(
                        ex.Message);
            }
        }

        /// <summary>
        /// Consulta la configuración contable para una combinación
        /// de tipo y códigos.
        /// </summary>
        /// <param name="connection">
        /// Conexión abierta de la empresa.
        /// </param>
        /// <param name="tipo">
        /// Tipo de catálogo de Inversiones.
        /// </param>
        /// <param name="codigo01">
        /// Código principal del catálogo.
        /// </param>
        /// <param name="codigo02">
        /// Código secundario del catálogo.
        /// </param>
        /// <returns>
        /// Configuración encontrada o null.
        /// </returns>
        private static IvrCatCuentasContablesData?
            IVR_Cat_Cuentas_Contables_Consultar(
                SqlConnection connection,
                string tipo,
                string codigo01,
                string codigo02)
        {
            return connection.QueryFirstOrDefault
                <IvrCatCuentasContablesData>(
                    SqlCuentas,
                    new
                    {
                        tipo,
                        codigo_01 = codigo01,
                        codigo_02 = codigo02
                    });
        }

        /// <summary>
        /// Obtiene las unidades contables de la contabilidad enlazada.
        /// </summary>
        /// <param name="connection">
        /// Conexión abierta de la empresa.
        /// </param>
        /// <param name="codContabilidad">
        /// Código de la contabilidad enlazada.
        /// </param>
        /// <returns>
        /// Lista de unidades contables.
        /// </returns>
        private static List<DropDownListaGenericaModel<string>>
            IVR_Cat_Cuentas_Contables_Unidades_Consultar(
                SqlConnection connection,
                int codContabilidad)
        {
            return connection.Query
                <DropDownListaGenericaModel<string>>(
                    SqlUnidades,
                    new
                    {
                        cod_contabilidad = codContabilidad
                    })
                .ToList();
        }

        /// <summary>
        /// Obtiene los centros de costo activos de la contabilidad
        /// enlazada.
        /// </summary>
        /// <param name="connection">
        /// Conexión abierta de la empresa.
        /// </param>
        /// <param name="codContabilidad">
        /// Código de la contabilidad enlazada.
        /// </param>
        /// <returns>
        /// Lista de centros de costo.
        /// </returns>
        private static List<DropDownListaGenericaModel<string>>
            IVR_Cat_Cuentas_Contables_Centros_Consultar(
                SqlConnection connection,
                int codContabilidad)
        {
            return connection.Query
                <DropDownListaGenericaModel<string>>(
                    SqlCentrosCostos,
                    new
                    {
                        cod_contabilidad = codContabilidad
                    })
                .ToList();
        }

        /// <summary>
        /// Crea una configuración vacía cuando todavía no existe
        /// un registro para el catálogo indicado.
        /// </summary>
        /// <param name="tipo">
        /// Tipo de catálogo.
        /// </param>
        /// <param name="codigo01">
        /// Código principal.
        /// </param>
        /// <param name="codigo02">
        /// Código secundario.
        /// </param>
        /// <returns>
        /// Configuración vacía con las claves inicializadas.
        /// </returns>
        private static IvrCatCuentasContablesData
            IVR_Cat_Cuentas_Contables_Vacio_Crear(
                string tipo,
                string codigo01,
                string codigo02)
        {
            return new IvrCatCuentasContablesData
            {
                tipo_base = tipo,
                codigo_01 = codigo01,
                codigo_02 = codigo02
            };
        }

        /// <summary>
        /// Valida la solicitud utilizada para inicializar el diálogo.
        /// </summary>
        /// <param name="request">
        /// Contexto recibido.
        /// </param>
        /// <returns>
        /// Mensaje de validación o texto vacío.
        /// </returns>
        private static string
            IVR_Cat_Cuentas_Contables_Inicializar_Validar(
                IvrCatCuentasContablesInicializarRequest? request)
        {
            if (request is null)
            {
                return "La información de inicialización es requerida.";
            }

            var validacion =
                IVR_Cat_Cuentas_Contables_Contexto_Validar(
                    request.tipo,
                    request.codigo_01,
                    request.codigo_02);

            if (!string.IsNullOrEmpty(validacion))
            {
                return validacion;
            }

            return request.cod_contabilidad <= 0
                ? "La contabilidad enlazada es requerida."
                : string.Empty;
        }

        /// <summary>
        /// Valida la información requerida para guardar.
        /// </summary>
        /// <param name="request">
        /// Información recibida.
        /// </param>
        /// <returns>
        /// Mensaje de validación o texto vacío.
        /// </returns>
        private static string
            IVR_Cat_Cuentas_Contables_Guardar_Validar(
                IvrCatCuentasContablesGuardarRequest? request)
        {
            if (request is null)
            {
                return "La información por guardar es requerida.";
            }

            var validacion =
                IVR_Cat_Cuentas_Contables_Contexto_Validar(
                    request.tipo,
                    request.codigo_01,
                    request.codigo_02);

            if (!string.IsNullOrEmpty(validacion))
            {
                return validacion;
            }

            if (string.IsNullOrWhiteSpace(request.usuario))
            {
                return "El usuario es requerido.";
            }

            return IVR_Cat_Cuentas_Contables_Longitudes_Validar(
                request);
        }

        /// <summary>
        /// Valida las claves que identifican la configuración.
        /// </summary>
        /// <param name="tipo">
        /// Tipo de catálogo.
        /// </param>
        /// <param name="codigo01">
        /// Código principal.
        /// </param>
        /// <param name="codigo02">
        /// Código secundario.
        /// </param>
        /// <returns>
        /// Mensaje de validación o texto vacío.
        /// </returns>
        private static string
            IVR_Cat_Cuentas_Contables_Contexto_Validar(
                string? tipo,
                string? codigo01,
                string? codigo02)
        {
            var tipoNormalizado =
                tipo?.Trim().ToUpperInvariant() ??
                string.Empty;

            if (!TiposValidos.Contains(tipoNormalizado))
            {
                return "El tipo de configuración contable "
                    + "no es válido.";
            }

            if (string.IsNullOrWhiteSpace(codigo01))
            {
                return "El código principal es requerido.";
            }

            if (codigo01.Trim().Length > 10)
            {
                return "El código principal no puede exceder "
                    + "10 caracteres.";
            }

            if ((codigo02?.Trim().Length ?? 0) > 10)
            {
                return "El código secundario no puede exceder "
                    + "10 caracteres.";
            }

            if (tipoNormalizado == "X" &&
                string.IsNullOrWhiteSpace(codigo02))
            {
                return "El código del administrador es requerido.";
            }

            return string.Empty;
        }

        /// <summary>
        /// Valida que los valores no excedan las longitudes
        /// aceptadas por el procedimiento almacenado.
        /// </summary>
        /// <param name="request">
        /// Información por guardar.
        /// </param>
        /// <returns>
        /// Mensaje de validación o texto vacío.
        /// </returns>
        private static string
            IVR_Cat_Cuentas_Contables_Longitudes_Validar(
                IvrCatCuentasContablesGuardarRequest request)
        {
            var campos = new[]
            {
                new CampoLongitud(
                    "usuario",
                    request.usuario,
                    30),
                new CampoLongitud(
                    "unidad",
                    request.cod_unidad,
                    10),
                new CampoLongitud(
                    "centro de costo",
                    request.cod_centro_costo,
                    10),
                new CampoLongitud(
                    "cuenta de inversión",
                    request.cta_inversion_mask,
                    60),
                new CampoLongitud(
                    "intereses acumulados por cobrar",
                    request.cta_intereses_acum_cobrar_mask,
                    60),
                new CampoLongitud(
                    "ingresos por intereses",
                    request.cta_ingresos_intereses_mask,
                    60),
                new CampoLongitud(
                    "ingresos por fluctuaciones",
                    request.cta_ingresos_fluctuaciones_mask,
                    60),
                new CampoLongitud(
                    "gasto por comisiones",
                    request.cta_gasto_comisiones_mask,
                    60),
                new CampoLongitud(
                    "prima",
                    request.cta_prima_mask,
                    60),
                new CampoLongitud(
                    "amortización de prima",
                    request.cta_prima_gasto_amort_mask,
                    60),
                new CampoLongitud(
                    "descuento",
                    request.cta_descuentos_mask,
                    60),
                new CampoLongitud(
                    "amortización de descuento",
                    request.cta_descuento_ingreso_mask,
                    60),
                new CampoLongitud(
                    "valorización de activo",
                    request.cta_val_activo_mask,
                    60),
                new CampoLongitud(
                    "valorización de patrimonio",
                    request.cta_val_patrimonio_mask,
                    60),
                new CampoLongitud(
                    "resultado de valorización gasto",
                    request.cta_val_resultados_gastos_mask,
                    60),
                new CampoLongitud(
                    "resultado de valorización ingreso",
                    request.cta_val_resultados_ingresos_mask,
                    60),
                new CampoLongitud(
                    "recompra de principal",
                    request.cta_recompra_principal_mask,
                    60),
                new CampoLongitud(
                    "recompra de intereses",
                    request.cta_recompra_intereses_mask,
                    60),
                new CampoLongitud(
                    "intereses por pagar",
                    request.cta_intereses_pagar_mask,
                    60),
                new CampoLongitud(
                    "gasto de intereses",
                    request.cta_gasto_intereses_mask,
                    60)
            };

            foreach (var campo in campos)
            {
                if ((campo.Valor?.Trim().Length ?? 0) >
                    campo.LongitudMaxima)
                {
                    return $"El campo {campo.Nombre} no puede "
                        + $"exceder {campo.LongitudMaxima} caracteres.";
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// Normaliza las claves utilizadas por consultas y guardados.
        /// </summary>
        /// <param name="tipo">
        /// Tipo recibido.
        /// </param>
        /// <param name="codigo01">
        /// Código principal recibido.
        /// </param>
        /// <param name="codigo02">
        /// Código secundario recibido.
        /// </param>
        /// <returns>
        /// Claves normalizadas.
        /// </returns>
        private static ContextoNormalizado
            IVR_Cat_Cuentas_Contables_Contexto_Normalizar(
                string tipo,
                string codigo01,
                string? codigo02)
        {
            return new ContextoNormalizado(
                tipo.Trim().ToUpperInvariant(),
                codigo01.Trim(),
                codigo02?.Trim() ?? string.Empty);
        }

        /// <summary>
        /// Construye los parámetros del procedimiento almacenado.
        /// </summary>
        /// <param name="request">
        /// Solicitud validada.
        /// </param>
        /// <returns>
        /// Parámetros normalizados.
        /// </returns>
        private static ParametrosGuardar
            IVR_Cat_Cuentas_Contables_Parametros_Crear(
                IvrCatCuentasContablesGuardarRequest request)
        {
            var contexto =
                IVR_Cat_Cuentas_Contables_Contexto_Normalizar(
                    request.tipo,
                    request.codigo_01,
                    request.codigo_02);

            return new ParametrosGuardar
            {
                Tipo = contexto.tipo,
                Codigo_01 = contexto.codigo_01,
                Codigo_02 = contexto.codigo_02,
                Usuario = request.usuario
                    .Trim()
                    .ToUpperInvariant(),
                Unidad = request.cod_unidad?.Trim() ??
                    string.Empty,
                CentroCosto =
                    request.cod_centro_costo?.Trim() ??
                    string.Empty,
                CtaInversion =
                    request.cta_inversion_mask?.Trim() ??
                    string.Empty,
                CtaIntAcum =
                    request.cta_intereses_acum_cobrar_mask
                        ?.Trim() ??
                    string.Empty,
                CtaIntereses =
                    request.cta_ingresos_intereses_mask
                        ?.Trim() ??
                    string.Empty,
                CtaIntFluctuacion =
                    request.cta_ingresos_fluctuaciones_mask
                        ?.Trim() ??
                    string.Empty,
                CtaComisionGst =
                    request.cta_gasto_comisiones_mask
                        ?.Trim() ??
                    string.Empty,
                CtaPrima =
                    request.cta_prima_mask?.Trim() ??
                    string.Empty,
                CtaPrimaAmort =
                    request.cta_prima_gasto_amort_mask
                        ?.Trim() ??
                    string.Empty,
                CtaDescuento =
                    request.cta_descuentos_mask?.Trim() ??
                    string.Empty,
                CtaDescuentoAmort =
                    request.cta_descuento_ingreso_mask
                        ?.Trim() ??
                    string.Empty,
                CtaValActivo =
                    request.cta_val_activo_mask?.Trim() ??
                    string.Empty,
                CtaValPatrimonio =
                    request.cta_val_patrimonio_mask
                        ?.Trim() ??
                    string.Empty,
                CtaValResGasto =
                    request.cta_val_resultados_gastos_mask
                        ?.Trim() ??
                    string.Empty,
                CtaValResIngreso =
                    request.cta_val_resultados_ingresos_mask
                        ?.Trim() ??
                    string.Empty,
                CtaRePrincipal =
                    request.cta_recompra_principal_mask
                        ?.Trim() ??
                    string.Empty,
                CtaReIntereses =
                    request.cta_recompra_intereses_mask
                        ?.Trim() ??
                    string.Empty,
                CtaIntXPagar =
                    request.cta_intereses_pagar_mask
                        ?.Trim() ??
                    string.Empty,
                CtaGstIIntereses =
                    request.cta_gasto_intereses_mask
                        ?.Trim() ??
                    string.Empty
            };
        }

        /// <summary>
        /// Longitud máxima permitida para un campo recibido.
        /// </summary>
        private sealed record CampoLongitud(
            string Nombre,
            string? Valor,
            int LongitudMaxima);

        /// <summary>
        /// Claves normalizadas del contexto del diálogo.
        /// </summary>
        private sealed record ContextoNormalizado(
            string tipo,
            string codigo_01,
            string codigo_02);

        /// <summary>
        /// Parámetros internos enviados al procedimiento almacenado.
        /// </summary>
        private sealed class ParametrosGuardar
        {
            public string Tipo { get; init; } = string.Empty;

            public string Codigo_01 { get; init; } = string.Empty;

            public string Codigo_02 { get; init; } = string.Empty;

            public string Usuario { get; init; } = string.Empty;

            public string Unidad { get; init; } = string.Empty;

            public string CentroCosto { get; init; } =
                string.Empty;

            public string CtaInversion { get; init; } =
                string.Empty;

            public string CtaIntAcum { get; init; } =
                string.Empty;

            public string CtaIntereses { get; init; } =
                string.Empty;

            public string CtaIntFluctuacion { get; init; } =
                string.Empty;

            public string CtaComisionGst { get; init; } =
                string.Empty;

            public string CtaPrima { get; init; } =
                string.Empty;

            public string CtaPrimaAmort { get; init; } =
                string.Empty;

            public string CtaDescuento { get; init; } =
                string.Empty;

            public string CtaDescuentoAmort { get; init; } =
                string.Empty;

            public string CtaValActivo { get; init; } =
                string.Empty;

            public string CtaValPatrimonio { get; init; } =
                string.Empty;

            public string CtaValResGasto { get; init; } =
                string.Empty;

            public string CtaValResIngreso { get; init; } =
                string.Empty;

            public string CtaRePrincipal { get; init; } =
                string.Empty;

            public string CtaReIntereses { get; init; } =
                string.Empty;

            public string CtaIntXPagar { get; init; } =
                string.Empty;

            public string CtaGstIIntereses { get; init; } =
                string.Empty;
        }
    }
}