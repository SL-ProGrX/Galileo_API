
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
            //var tipoNormalizado = tipo?.Trim().ToUpperInvariant() ?? string.Empty;

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
        private static void EjecutarProcesoAdd(IDbConnection connection, CcProcesoMensualProcesoGeneralDbModel item, int codInstitucion, decimal proceso)
        {
            var procedimiento = item.Procedimiento.Trim();

            if (string.IsNullOrWhiteSpace(procedimiento))
            {
                return;
            }

            var parametros = CrearParametrosProcedimiento(
                item,
                codInstitucion,
                proceso);

            var query = $"EXEC {procedimiento} {parametros}";

            connection.Execute(query);
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
            public int ParametrosPlanillas { get; set; }
            public string ParametrosAdd { get; set; } = string.Empty;
        }
    }
}


