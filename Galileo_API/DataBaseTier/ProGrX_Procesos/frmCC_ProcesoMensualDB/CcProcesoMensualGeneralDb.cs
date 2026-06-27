
using Dapper;
using Galileo.DataBaseTier;
using System.Data;

namespace Galileo_API.DataBaseTier.ProGrX_Procesos.frmCC_ProcesoMensualDB
{
    public class CcProcesoMensualGeneralDb
    {

        private readonly MProGrxMain _mProGrx;

        /// <summary>
        /// Inicializa una nueva instancia para ejecutar procesos generales del proceso mensual.
        /// </summary>
        /// <param name="config">Configuración general de la aplicación.</param>
        public CcProcesoMensualGeneralDb(IConfiguration config)
        {       
            _mProGrx = new MProGrxMain(config);
        }

        /// <summary>
        /// Ejecuta los procesos adicionales configurados para una transacción y tipo de ejecución.
        /// </summary>
        /// <param name="connection">Conexión activa a base de datos.</param>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="transaccion">Código de transacción.</param>
        /// <param name="tipo">Tipo de ejecución (por ejemplo PRE o POS).</param>
        /// <param name="usuario">Usuario que ejecuta el proceso.</param>
        /// <param name="gInstitucion">Código de la institución.</param>
        /// <param name="proceso">Fecha de proceso; si es 0 se obtiene automáticamente.</param>
        public void CcProcesoMensual_ProcesosAdd_Ejecutar(IDbConnection connection, int codEmpresa, string transaccion, string tipo, string usuario, int gInstitucion, decimal proceso = 0)
        {
           
            if (proceso == 0)
            {
                var globalesResp = _mProGrx.sbSifParametrosInicializa(codEmpresa, usuario);
                proceso = globalesResp?.Result?.GlngFechaCR ?? 0;
            }
            const string query = @"
                SELECT
                    DESCRIPCION AS Descripcion,
                    Procedimiento,
                    ISNULL(PARAMETROS_PLANILLAS, 0) AS ParametrosPlanillas,
                    ISNULL(PARAMETROS_ADD, '') AS ParametrosAdd
                FROM PRM_PROCESOS_ADD
                WHERE Transaccion = @Transaccion
                  AND EJECUCION_TIPO = @Tipo
                  AND PARAMETROS_PLANILLAS = 1
                ORDER BY EJECUCION_ORDEN, PROC_NUM";

            var procesos = connection.Query<CcProcesoMensualProcesoGeneralDbModel>(
                query,
                new
                {
                    Transaccion = transaccion?.Trim() ?? string.Empty,
                    Tipo = tipo?.Trim().ToUpperInvariant() ?? string.Empty
                }).ToList();

            foreach (var item in procesos)
            {
                EjecutarProcesoAdd(
                    connection,
                    item,
                    gInstitucion,
                    proceso);
            }

        }

        /// <summary>
        /// Ejecuta un procedimiento adicional configurado para el proceso mensual.
        /// </summary>
        /// <param name="connection">Conexión activa a base de datos.</param>
        /// <param name="item">Configuración del proceso a ejecutar.</param>
        /// <param name="codInstitucion">Código de la institución.</param>
        /// <param name="proceso">Fecha de proceso.</param>
        private static void EjecutarProcesoAdd(IDbConnection connection,CcProcesoMensualProcesoGeneralDbModel item,int codInstitucion, decimal proceso)
        {
            var procedimiento = item.Procedimiento.Trim();

            if (!EsNombreProcedimientoValido(procedimiento))
            {
                throw new InvalidOperationException(
                    $"El procedimiento configurado no es válido: {procedimiento}");
            }

            if (!SonParametrosAdicionalesValidos(item.ParametrosAdd))
            {
                throw new InvalidOperationException(
                    $"Los parámetros adicionales del procedimiento {procedimiento} no son válidos.");
            }

            var parametrosAdd = item.ParametrosAdd?.Trim();

            var query = CrearQueryProcesoAdd( procedimiento, item.ParametrosPlanillas,parametrosAdd);

            connection.Execute(
                query,
                new
                {
                    CodInstitucion = codInstitucion,
                    Proceso = proceso
                });
        }

        /// <summary>
        /// Construye el comando EXEC para ejecutar un procedimiento con o sin parámetros.
        /// </summary>
        /// <param name="procedimiento">Nombre del procedimiento almacenado.</param>
        /// <param name="parametrosPlanillas">Indicador de uso de parámetros de planilla.</param>
        /// <param name="parametrosAdd">Parámetros adicionales configurados.</param>
        /// <returns>Consulta SQL lista para ejecutar.</returns>
        private static string CrearQueryProcesoAdd( string procedimiento, int parametrosPlanillas, string? parametrosAdd)
        {
            if (parametrosPlanillas == 1)
            {
                return string.IsNullOrWhiteSpace(parametrosAdd)
                    ? $"EXEC {procedimiento} @CodInstitucion, @Proceso"
                    : $"EXEC {procedimiento} @CodInstitucion, @Proceso, {parametrosAdd}";
            }

            return string.IsNullOrWhiteSpace(parametrosAdd)
                ? $"EXEC {procedimiento}"
                : $"EXEC {procedimiento} {parametrosAdd}";
        }

        /// <summary>
        /// Valida que el nombre del procedimiento tenga un formato permitido.
        /// </summary>
        /// <param name="procedimiento">Nombre del procedimiento a validar.</param>
        /// <returns><c>true</c> si el nombre es válido; en caso contrario, <c>false</c>.</returns>
        private static bool EsNombreProcedimientoValido(string procedimiento)
        {
            return !string.IsNullOrWhiteSpace(procedimiento)
                && procedimiento.Length <= 128
                && procedimiento.All(c =>
                    char.IsLetterOrDigit(c)
                    || c == '_'
                    || c == '.');
        }

        /// <summary>
        /// Valida que la cadena de parámetros adicionales contenga solo caracteres permitidos.
        /// </summary>
        /// <param name="parametros">Parámetros adicionales a validar.</param>
        /// <returns><c>true</c> si los parámetros son válidos; en caso contrario, <c>false</c>.</returns>
        private static bool SonParametrosAdicionalesValidos(string parametros)
        {
            if (string.IsNullOrWhiteSpace(parametros))
            {
                return true;
            }

            return parametros.All(c =>
                char.IsLetterOrDigit(c)
                || char.IsWhiteSpace(c)
                || c == '_'
                || c == '.'
                || c == ','
                || c == '-'
                || c == '\''
                || c == '/');
        }
     
        private sealed class CcProcesoMensualProcesoGeneralDbModel
        {
            public string Descripcion { get; set; } = string.Empty;
            public string Procedimiento { get; set; } = string.Empty;
            public int ParametrosPlanillas { get; set; } = 0;
            public string ParametrosAdd { get; set; } = string.Empty;
        }
    }
}


