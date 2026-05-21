using Galileo.Models;
using Galileo.Models.ERROR;
using Dapper;
using Microsoft.Data.SqlClient;

namespace Galileo.DataBaseTier.ProGrX.Fondos
{
    public class FrmFndCalcePLazosDB
    {
        private readonly IConfiguration _config;

        public FrmFndCalcePLazosDB(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        /// <summary>
        /// Obtiene los periodos historicos disponibles para el reporte de calce de plazos.
        /// </summary>
        /// <param name="CodEmpresa">Codigo de empresa usado para resolver la base de datos cliente.</param>
        public ErrorDto<List<DropDownListaGenericaModel>> Periodos_Lista(int CodEmpresa)
        {
            const string query = @"
                    SELECT
                        id_per_historico AS item,
                        CONCAT(anio, '-', mes) AS descripcion
                    FROM dbo.fnd_per_historico
                    ORDER BY anio DESC, mes DESC;";

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(new PortalDB(_config), CodEmpresa, query);
        }

        /// <summary>
        /// Ejecuta la proyeccion anual o la actualizacion de real obtenido.
        /// </summary>
        /// <param name="CodEmpresa">Codigo de empresa usado para resolver la base de datos cliente.</param>
        /// <param name="Anio">Anio de la proyeccion.</param>
        /// <param name="Usuario">Usuario que ejecuta el proceso.</param>
        /// <param name="Tipo">Tipo de proceso: 0 = Proyectar, 1 = Actualiza Real.</param>
        public ErrorDto Proyeccion_Presupuesto(int CodEmpresa, int Anio, string Usuario, int Tipo)
        {
            const string query = @"
                EXEC dbo.spFnd_Proyeccion_Presupuesto
                    @Anio,
                    @Usuario,
                    @Tipo;";

            return DbHelper.ExecuteNonQuery(
                new PortalDB(_config),
                CodEmpresa,
                query,
                new { Anio, Usuario, Tipo });
        }

        /// <summary>
        /// Consulta los datos generados por la proyeccion anual para exportarlos a Excel.
        /// </summary>
        /// <param name="CodEmpresa">Codigo de empresa usado para resolver la base de datos cliente.</param>
        /// <param name="Anio">Anio de la proyeccion a exportar.</param>
        public ErrorDto<List<Dictionary<string, object?>>> Proyeccion_Presupuesto_Export(int CodEmpresa, int Anio)
        {
            var response = DbHelper.CreateOkResponse(new List<Dictionary<string, object?>>());

            try
            {
                const string query = "EXEC dbo.spFnd_Proyeccion_Presupuesto_Export @Anio;";

                using var connection = new PortalDB(_config).CreateConnection(CodEmpresa);
                var rows = connection.Query(query, new { Anio });

                response.Result = rows
                    .Select(row => ((IDictionary<string, object?>)row)
                        .ToDictionary(col => col.Key, col => col.Value))
                    .ToList();
            }
            catch (SqlException ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }
            catch (InvalidOperationException ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }
    }
}
