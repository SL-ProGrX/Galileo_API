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

        /// <summary>
        /// Método para obtener las descripciones del analitico
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="datos"></param>
        /// <returns></returns>
        public ErrorDto<List<PresAnaliticoDescData>> PresAnaliticoDesc_Obtener(int CodCliente, string datos)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            PresAnaliticoBuscar? filtros = JsonConvert.DeserializeObject<PresAnaliticoBuscar>(datos);
            var info = new ErrorDto<List<PresAnaliticoDescData>> 
            { 
                Code = 0,
                Description = "OK",
                Result = new List<PresAnaliticoDescData>()
            };
            if (filtros == null)
            {
                info.Code = -1;
                info.Description = "Error: No se pudo deserializar los filtros del analítico.";
                return info;
            }
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var procedure = "[spPres_Analitico_Descripciones]";
                    var values = new
                    {
                        Modelo = filtros.Modelo,
                        Contabilidad = filtros.Contabilidad,
                        Cuenta = filtros.Cuenta,
                        Unidad = filtros.Unidad,
                        CentroCosto = filtros.CentroCosto
                    };

                    info.Result = connection.Query<PresAnaliticoDescData>(procedure, values, commandType: CommandType.StoredProcedure).ToList();
                }
            }
            catch (Exception ex)
            {
                info.Code = -1;
                info.Description = ex.Message;
                info.Result = new List<PresAnaliticoDescData>();
            }
            return info;
        }

        /// <summary>
        /// Método para obtener el analitico
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="datos"></param>
        /// <returns></returns>
        public ErrorDto<List<PresAnaliticoData>> PresAnalitico_Obtener(int CodCliente, string datos)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            PresAnaliticoBuscar? filtros = JsonConvert.DeserializeObject<PresAnaliticoBuscar>(datos);
            var info = new ErrorDto<List<PresAnaliticoData>>
            {
                Code = 0,
                Description = "OK",
                Result = new List<PresAnaliticoData>()
            };
            if (filtros == null)
            {
                info.Code = -1;
                info.Description = "Error: No se pudo deserializar los filtros del analítico.";
                return info;
            }
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    if (string.IsNullOrEmpty(filtros.Periodo) || !DateTime.TryParse(filtros.Periodo, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out _))
                    {
                        info.Code = -1;
                        info.Description = "Error: El campo 'Periodo' es nulo o tiene un formato inválido.";
                        return info;
                    }
                    var procedure = "[spPres_Analitico]";
                    var values = new
                    {
                        Contabilidad = filtros.Contabilidad,
                        Periodo = filtros.Periodo,
                        Cuenta = filtros.Cuenta,
                        Unidad = (filtros.Unidad == "") ? null : filtros.Unidad,
                        CentroCosto = (filtros.CentroCosto == "")? null: filtros.CentroCosto
                    };

                    info.Result = connection.Query<PresAnaliticoData>(procedure, values, commandType: CommandType.StoredProcedure).ToList();
                }
            }
            catch (Exception ex)
            {
                info.Code = -1;
                info.Description = ex.Message;
                info.Result = new List<PresAnaliticoData>();
            }
            return info;
        }
    }
}