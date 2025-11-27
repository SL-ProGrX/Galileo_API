using Dapper;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using Galileo.Models.ERROR;
using System.Data;
using Galileo.Models.PRES;

namespace Galileo.DataBaseTier
{
    public class FrmPresAnaliticoDb
    {
        private readonly IConfiguration _config;

        public FrmPresAnaliticoDb(IConfiguration config)
        {
            _config = config;
        }

        #region Helpers

        private SqlConnection CreateConnection(int codCliente)
        {
            var connString = new PortalDB(_config).ObtenerDbConnStringEmpresa(codCliente);
            return new SqlConnection(connString);
        }

        private static PresAnaliticoBuscar? TryGetFiltros(string datos, out string? error)
        {
            error = null;

            if (string.IsNullOrWhiteSpace(datos))
            {
                error = "Error: El parámetro 'datos' está vacío.";
                return null;
            }

            PresAnaliticoBuscar? filtros;
            try
            {
                filtros = JsonConvert.DeserializeObject<PresAnaliticoBuscar>(datos);
            }
            catch (JsonException ex)
            {
                error = "Error al deserializar los filtros del analítico: " + ex.Message;
                return null;
            }

            if (filtros == null)
            {
                error = "Error: No se pudo deserializar los filtros del analítico.";
                return null;
            }

            return filtros;
        }

        #endregion

        /// <summary>
        /// Método para obtener las descripciones del analitico
        /// </summary>
        public ErrorDto<List<PresAnaliticoDescData>> PresAnaliticoDesc_Obtener(int codCliente, string datos)
        {
            var info = new ErrorDto<List<PresAnaliticoDescData>>
            {
                Code = 0,
                Description = "OK",
                Result = new List<PresAnaliticoDescData>()
            };

            var filtros = TryGetFiltros(datos, out var error);
            if (filtros == null)
            {
                info.Code = -1;
                info.Description = error ?? "Error desconocido al procesar los filtros.";
                return info;
            }

            const string procedure = "[spPres_Analitico_Descripciones]";

            try
            {
                using var connection = CreateConnection(codCliente);

                var values = new
                {
                    Modelo = filtros.Modelo,
                    Contabilidad = filtros.Contabilidad,
                    Cuenta = filtros.Cuenta,
                    Unidad = filtros.Unidad,
                    CentroCosto = filtros.CentroCosto
                };

                info.Result = connection
                    .Query<PresAnaliticoDescData>(procedure, values, commandType: CommandType.StoredProcedure)
                    .ToList();
            }
            catch (Exception ex)
            {
                info.Code = -1;
                info.Description = "PresAnaliticoDesc_Obtener: " + ex.Message;
                info.Result = new List<PresAnaliticoDescData>();
            }

            return info;
        }

        /// <summary>
        /// Método para obtener el analitico
        /// </summary>
        public ErrorDto<List<PresAnaliticoData>> PresAnalitico_Obtener(int codCliente, string datos)
        {
            var info = new ErrorDto<List<PresAnaliticoData>>
            {
                Code = 0,
                Description = "OK",
                Result = new List<PresAnaliticoData>()
            };

            var filtros = TryGetFiltros(datos, out var error);
            if (filtros == null)
            {
                info.Code = -1;
                info.Description = error ?? "Error desconocido al procesar los filtros.";
                return info;
            }

            // Validación del Periodo
            if (string.IsNullOrWhiteSpace(filtros.Periodo) ||
                !DateTime.TryParse(
                    filtros.Periodo,
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.None,
                    out _))
            {
                info.Code = -1;
                info.Description = "Error: El campo 'Periodo' es nulo o tiene un formato inválido.";
                return info;
            }

            const string procedure = "[spPres_Analitico]";

            try
            {
                using var connection = CreateConnection(codCliente);

                var values = new
                {
                    Contabilidad = filtros.Contabilidad,
                    Periodo = filtros.Periodo,
                    Cuenta = filtros.Cuenta,
                    Unidad = string.IsNullOrEmpty(filtros.Unidad) ? null : filtros.Unidad,
                    CentroCosto = string.IsNullOrEmpty(filtros.CentroCosto) ? null : filtros.CentroCosto
                };

                info.Result = connection
                    .Query<PresAnaliticoData>(procedure, values, commandType: CommandType.StoredProcedure)
                    .ToList();
            }
            catch (Exception ex)
            {
                info.Code = -1;
                info.Description = "PresAnalitico_Obtener: " + ex.Message;
                info.Result = new List<PresAnaliticoData>();
            }

            return info;
        }
    }
}