
using Dapper;
using Galileo.DataBaseTier;
using System.Data;

namespace Galileo_API.DataBaseTier.ProGrX_Procesos.frmCC_ProcesoMensualDB
{
    public class CcProcesoMensualGeneralDb
    {

        private readonly MProGrxMain _mProGrx;
        public CcProcesoMensualGeneralDb(IConfiguration config)
        {       
            _mProGrx = new MProGrxMain(config);
        }
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

            var query = string.IsNullOrWhiteSpace(parametrosAdd)
                ? $"EXEC {procedimiento} @CodInstitucion, @Proceso"
                : $"EXEC {procedimiento} @CodInstitucion, @Proceso, {parametrosAdd}";

            connection.Execute(query, new
            {
                CodInstitucion = codInstitucion,
                Proceso = proceso
            });
        }
        private static bool EsNombreProcedimientoValido(string procedimiento)
        {
            return !string.IsNullOrWhiteSpace(procedimiento)
                && procedimiento.Length <= 128
                && procedimiento.All(c =>
                    char.IsLetterOrDigit(c)
                    || c == '_'
                    || c == '.');
        }

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
        private static string CrearParametrosProcedimiento(CcProcesoMensualProcesoGeneralDbModel item, int codInstitucion, decimal proceso)
        {
            if (item.ParametrosPlanillas == 1)
            {
                var parametros = $"{codInstitucion},{proceso}";

                if (!string.IsNullOrWhiteSpace(item.ParametrosAdd))
                {
                    parametros = $"{parametros},{item.ParametrosAdd.Trim()}";
                }

                return parametros;
            }

            return item.ParametrosAdd.Trim();
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


