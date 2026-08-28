using Galileo.Models.ERROR;
using Galileo.Models.GEN;
using System.Globalization;

namespace Galileo.DataBaseTier
{
    public class FrmCcAppLogDb
    {
        private const int CodigoErrorValidacion = -2;
        private const string FormatoFecha = "yyyy-MM-dd";
        private const string ProcedimientoEstadistica =
            "spAPP_Estadistica";
        private const string ProcedimientoDetalle =
            "spAPP_Estadistica_Detalle";
        private const string ProcedimientoAnalisis =
            "spAPP_Estadistica_Analisis";

        private readonly string _baseConnectionString;

        public FrmCcAppLogDb(
            IConfiguration config)
        {
            _baseConnectionString =
                config.GetConnectionString(
                    "BaseConnString")
                ?? string.Empty;
        }

        /// <summary>
        /// Obtiene la estadistica de consumo de la aplicacion.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="FechaInicio"></param>
        /// <param name="FechaCorte"></param>
        /// <returns></returns>
        public ErrorDto<List<EstadisticaData>>
            CC_App_Log_Estadistica_Obtener(
                int CodEmpresa,
                string FechaInicio,
                string FechaCorte)
        {
            var errorEmpresa =
                CC_App_Log_Empresa_Validar(
                    CodEmpresa);

            if (errorEmpresa is not null)
            {
                return DbHelper
                    .CreateErrorResponse<List<EstadisticaData>>(
                        errorEmpresa,
                        CodigoErrorValidacion,
                        []);
            }

            if (!CC_App_Log_Fechas_Obtener(
                FechaInicio,
                FechaCorte,
                out var fechaInicio,
                out var fechaCorte,
                out var errorFechas))
            {
                return DbHelper
                    .CreateErrorResponse<List<EstadisticaData>>(
                        errorFechas,
                        CodigoErrorValidacion,
                        []);
            }

            var parametros = new
            {
                EmpresaId = CodEmpresa,
                Inicio = fechaInicio,
                Corte = fechaCorte
            };

            return CC_App_Log_Procedimiento_Ejecutar<
                EstadisticaData>(
                    ProcedimientoEstadistica,
                    parametros,
                    "Ocurri&oacute; un error al consultar la estad&iacute;stica de consumo.");
        }

        /// <summary>
        /// Obtiene el detalle de consumo para el codigo seleccionado.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Codigo"></param>
        /// <param name="FechaInicio"></param>
        /// <param name="FechaCorte"></param>
        /// <returns></returns>
        public ErrorDto<List<EstadisticaDetalleData>>
            CC_App_Log_Estadistica_Detalle_Obtener(
                int CodEmpresa,
                string Codigo,
                string FechaInicio,
                string FechaCorte)
        {
            var errorEmpresa =
                CC_App_Log_Empresa_Validar(
                    CodEmpresa);

            if (errorEmpresa is not null)
            {
                return DbHelper
                    .CreateErrorResponse<List<EstadisticaDetalleData>>(
                        errorEmpresa,
                        CodigoErrorValidacion,
                        []);
            }

            if (string.IsNullOrWhiteSpace(Codigo))
            {
                return DbHelper
                    .CreateErrorResponse<List<EstadisticaDetalleData>>(
                        "El c&oacute;digo de la estad&iacute;stica es requerido.",
                        CodigoErrorValidacion,
                        []);
            }

            if (!CC_App_Log_Fechas_Obtener(
                FechaInicio,
                FechaCorte,
                out var fechaInicio,
                out var fechaCorte,
                out var errorFechas))
            {
                return DbHelper
                    .CreateErrorResponse<List<EstadisticaDetalleData>>(
                        errorFechas,
                        CodigoErrorValidacion,
                        []);
            }

            var parametros = new
            {
                EmpresaId = CodEmpresa,
                Codigo = Codigo.Trim(),
                Inicio = fechaInicio,
                Corte = fechaCorte
            };

            return CC_App_Log_Procedimiento_Ejecutar<
                EstadisticaDetalleData>(
                    ProcedimientoDetalle,
                    parametros,
                    "Ocurri&oacute; un error al consultar el detalle de consumo.");
        }

        /// <summary>
        /// Obtiene el analisis de ingresos a la aplicacion.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="FechaInicio"></param>
        /// <param name="FechaCorte"></param>
        /// <param name="Ingreso"></param>
        /// <returns></returns>
        public ErrorDto<List<EstadisticaAnalisisData>>
            CC_App_Log_Estadistica_Analisis_Obtener(
                int CodEmpresa,
                string FechaInicio,
                string FechaCorte,
                int Ingreso)
        {
            var errorEmpresa =
                CC_App_Log_Empresa_Validar(
                    CodEmpresa);

            if (errorEmpresa is not null)
            {
                return DbHelper
                    .CreateErrorResponse<List<EstadisticaAnalisisData>>(
                        errorEmpresa,
                        CodigoErrorValidacion,
                        []);
            }

            if (Ingreso is not 0 and not 1)
            {
                return DbHelper
                    .CreateErrorResponse<List<EstadisticaAnalisisData>>(
                        "El tipo de ingreso no es v&aacute;lido.",
                        CodigoErrorValidacion,
                        []);
            }

            if (!CC_App_Log_Fechas_Obtener(
                FechaInicio,
                FechaCorte,
                out var fechaInicio,
                out var fechaCorte,
                out var errorFechas))
            {
                return DbHelper
                    .CreateErrorResponse<List<EstadisticaAnalisisData>>(
                        errorFechas,
                        CodigoErrorValidacion,
                        []);
            }

            var parametros = new
            {
                EmpresaId = CodEmpresa,
                Inicio = fechaInicio,
                Corte = fechaCorte,
                Ingreso
            };

            return CC_App_Log_Procedimiento_Ejecutar<
                EstadisticaAnalisisData>(
                    ProcedimientoAnalisis,
                    parametros,
                    "Ocurri&oacute; un error al consultar el an&aacute;lisis de ingresos.");
        }

        /// <summary>
        /// Ejecuta un procedimiento almacenado que retorna una lista.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="procedimiento"></param>
        /// <param name="parametros"></param>
        /// <param name="mensajeError"></param>
        /// <returns></returns>
        private ErrorDto<List<T>>
            CC_App_Log_Procedimiento_Ejecutar<T>(
                string procedimiento,
                object parametros,
                string mensajeError)
        {
            if (string.IsNullOrWhiteSpace(
                _baseConnectionString))
            {
                return DbHelper
                    .CreateErrorResponse<List<T>>(
                        "No fue posible obtener la configuraci&oacute;n de la base de datos.",
                        -1,
                        []);
            }

            var respuesta =
                DbHelper.ExecuteStoredProcedureList<T>(
                    _baseConnectionString,
                    procedimiento,
                    parametros);

            if (respuesta.Code != 0)
            {
                var detalleError =
                    string.IsNullOrWhiteSpace(
                        respuesta.Description)
                        ? mensajeError
                        : $"{mensajeError} {respuesta.Description}";

                return DbHelper
                    .CreateErrorResponse<List<T>>(
                        detalleError,
                        respuesta.Code ?? -1,
                        []);
            }

            respuesta.Result ??= [];

            return respuesta;
        }

        /// <summary>
        /// Valida el codigo de empresa requerido por el proceso.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        private static string?
            CC_App_Log_Empresa_Validar(
                int codEmpresa)
        {
            return codEmpresa <= 0
                ? "El c&oacute;digo de empresa es requerido."
                : null;
        }

        /// <summary>
        /// Valida y convierte las fechas al rango requerido por los procedimientos.
        /// </summary>
        /// <param name="fechaInicioTexto"></param>
        /// <param name="fechaCorteTexto"></param>
        /// <param name="fechaInicio"></param>
        /// <param name="fechaCorte"></param>
        /// <param name="mensajeError"></param>
        /// <returns></returns>
        private static bool
            CC_App_Log_Fechas_Obtener(
                string fechaInicioTexto,
                string fechaCorteTexto,
                out DateTime fechaInicio,
                out DateTime fechaCorte,
                out string mensajeError)
        {
            fechaInicio = default;
            fechaCorte = default;
            mensajeError = string.Empty;

            var inicioValido =
                DateTime.TryParseExact(
                    fechaInicioTexto,
                    FormatoFecha,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out var inicio);

            var corteValido =
                DateTime.TryParseExact(
                    fechaCorteTexto,
                    FormatoFecha,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out var corte);

            if (!inicioValido || !corteValido)
            {
                mensajeError =
                    $"Las fechas deben utilizar el formato {FormatoFecha}.";

                return false;
            }

            inicio = inicio.Date;
            corte = corte.Date;

            if (inicio > corte)
            {
                mensajeError =
                    "La fecha inicial no puede ser mayor que la fecha final.";

                return false;
            }

            if (corte >= DateTime.MaxValue.Date)
            {
                mensajeError =
                    "La fecha final no es v&aacute;lida.";

                return false;
            }

            fechaInicio = inicio;
            fechaCorte =
                corte.AddDays(1).AddTicks(-1);

            return true;
        }
    }
}