using Dapper;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.PRES;
using System.Data;

namespace PgxAPI.DataBaseTier
{
    public class frmPres_AnaliticoDB
    {
        private readonly IConfiguration _config;
        private readonly mProGrX_AuxiliarDB _AuxiliarDB;

        public frmPres_AnaliticoDB(IConfiguration config)
        {
            _config = config;
            _AuxiliarDB = new mProGrX_AuxiliarDB(_config);

        }

        /// <summary>
        /// Metodo para obtener las descripciones del analitico
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="datos"></param>
        /// <returns></returns>
        public ErrorDto<List<presAnaliticoDescData>> PresAnaliticoDesc_Obtener(int CodCliente, string datos)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            presAnaliticoBuscar filtros = JsonConvert.DeserializeObject<presAnaliticoBuscar>(datos);
            var info = new ErrorDto<List<presAnaliticoDescData>> 
            { 
                Code = 0,
                Description = "OK",
                Result = new List<presAnaliticoDescData>()
            };
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

                    info.Result = connection.Query<presAnaliticoDescData>(procedure, values, commandType: CommandType.StoredProcedure).ToList();
                }
            }
            catch (Exception ex)
            {
                info.Code = -1;
                info.Description = ex.Message;
                info.Result = new List<presAnaliticoDescData>();
            }
            return info;
        }

        /// <summary>
        /// Metodo para obtener el analitico
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="datos"></param>
        /// <returns></returns>
        public ErrorDto<List<presAnaliticoData>> PresAnalitico_Obtener(int CodCliente, string datos)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            presAnaliticoBuscar filtros = JsonConvert.DeserializeObject<presAnaliticoBuscar>(datos);
            var info = new ErrorDto<List<presAnaliticoData>>
            {
                Code = 0,
                Description = "OK",
                Result = new List<presAnaliticoData>()
            };
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    DateTime vFecha = Convert.ToDateTime(filtros.Periodo);
                    string vStringFecha = _AuxiliarDB.validaFechaGlobal(vFecha);

                    var procedure = "[spPres_Analitico]";
                    var values = new
                    {
                        Contabilidad = filtros.Contabilidad,
                        Periodo = filtros.Periodo,
                        Cuenta = filtros.Cuenta,
                        Unidad = (filtros.Unidad == "") ? null : filtros.Unidad,
                        CentroCosto = (filtros.CentroCosto == "")? null: filtros.CentroCosto
                    };

                    info.Result = connection.Query<presAnaliticoData>(procedure, values, commandType: CommandType.StoredProcedure).ToList();
                }
            }
            catch (Exception ex)
            {
                info.Code = -1;
                info.Description = ex.Message;
                info.Result = new List<presAnaliticoData>();
            }
            return info;
        }
    }
}