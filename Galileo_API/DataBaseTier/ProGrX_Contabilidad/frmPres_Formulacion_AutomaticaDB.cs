using Dapper;
using Microsoft.Data.SqlClient;
using Galileo.Models.ERROR;
using Galileo.Models.PRES;
using Galileo.Models.ProGrX_Contabilidad;
using System.Data;

namespace Galileo.DataBaseTier.ProGrX_Contabilidad
{
    public class FrmPresFormulacionAutomaticaDb
    {
        private readonly IConfiguration _config;

        public FrmPresFormulacionAutomaticaDb(IConfiguration config)
        {
            _config = config;
        }

        #region Helpers

        private SqlConnection CreateConnection(int codEmpresa)
        {
            var connString = new PortalDB(_config).ObtenerDbConnStringEmpresa(codEmpresa);
            return new SqlConnection(connString);
        }

        #endregion

        /// <summary>
        /// Obtiene los modelos de presupuesto según la contabilidad y el usuario
        /// </summary>
        public ErrorDto<List<PresModelisLista>> Pres_Modelos_Obtener(int codEmpresa, int codContab, string usuario)
        {
            var resp = new ErrorDto<List<PresModelisLista>>
            {
                Code = 0,
                Description = "OK",
                Result = new List<PresModelisLista>()
            };

            const string sql = @"
                SELECT 
                    P.cod_modelo AS IdX,
                    P.DESCRIPCION AS ItmX,
                    P.ESTADO,
                    Cc.Inicio_Anio
                FROM PRES_MODELOS P 
                INNER JOIN PRES_MODELOS_USUARIOS Pmu 
                    ON P.cod_Contabilidad = Pmu.cod_contabilidad
                    AND P.cod_Modelo      = Pmu.cod_Modelo
                    AND Pmu.Usuario       = @Usuario
                INNER JOIN CNTX_CIERRES Cc 
                    ON P.cod_Contabilidad = Cc.cod_Contabilidad
                    AND P.ID_CIERRE       = Cc.ID_CIERRE 
                WHERE P.COD_CONTABILIDAD = @Contabilidad
                GROUP BY P.cod_Modelo, P.Descripcion, P.ESTADO, Cc.Inicio_Anio 
                ORDER BY Cc.INICIO_ANIO DESC, P.Cod_Modelo;";

            try
            {
                using var connection = CreateConnection(codEmpresa);

                resp.Result = connection
                    .Query<PresModelisLista>(
                        sql,
                        new
                        {
                            Contabilidad = codContab,
                            Usuario = usuario
                        })
                    .ToList();
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = "Pres_Modelos_Obtener - " + ex.Message;
                resp.Result = null;
            }

            return resp;
        }

        /// <summary>
        /// Obtiene datos de formulación automática
        /// </summary>
        public ErrorDto<List<PresFormulacionAutoDto>> Pres_Formulacion_Automatica(
            int codEmpresa,
            string codModelo,
            string vTipo,
            string usuario)
        {
            var resp = new ErrorDto<List<PresFormulacionAutoDto>>
            {
                Code = 0,
                Description = "OK",
                Result = new List<PresFormulacionAutoDto>()
            };

            const string procInicial   = "[spPres_Formula_Inicia]";
            const string procCredito   = "[spPres_Formula_Auxiliar_Credito]";
            const string procAhorros   = "[spPres_Formula_Auxiliar_Ahorros]";
            const string procActivos   = "[spPres_Formula_Auxiliar_Activos]";
            const string procResultado = "[spPres_Formula_Resultado]";

            try
            {
                using var connection = CreateConnection(codEmpresa);

                // 1) Inicializa tabla de resultados
                var baseParams = new
                {
                    Modelo = codModelo,
                    Usuario = usuario
                };

                connection.Execute(
                    procInicial,
                    baseParams,
                    commandType: CommandType.StoredProcedure);

                // 2) Ejecuta el auxiliar según tipo
                string? auxProcedure = vTipo switch
                {
                    "CA" => procCredito, // Cartera
                    "AG" => procAhorros, // Ahorros y Gasto Financiero
                    "DA" => procActivos, // Depreciaciones de Activos
                    "PE" => procActivos, // Estandar de Partidas Contables
                    _    => null
                };

                if (auxProcedure == null)
                {
                    resp.Code = -1;
                    resp.Description = $"Tipo de formulación desconocido: '{vTipo}'.";
                    resp.Result = null;
                    return resp;
                }

                connection.Execute(
                    auxProcedure,
                    baseParams,
                    commandType: CommandType.StoredProcedure,
                    commandTimeout: 600);

                // 3) Trae el resultado de la formulación
                resp.Result = connection
                    .Query<PresFormulacionAutoDto>(
                        procResultado,
                        baseParams,
                        commandType: CommandType.StoredProcedure)
                    .ToList();

                if (resp.Result == null || resp.Result.Count == 0)
                {
                    resp.Code = -1;
                    resp.Description = "Error al aplicar la formulación (sin resultados).";
                    resp.Result = null;
                    return resp;
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = "Pres_Formulacion_Automatica - " + ex.Message;
                resp.Result = null;
            }

            return resp;
        }
    }
}