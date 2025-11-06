using PgxAPI.Models.ERROR;
using PgxAPI.Models;
using PgxAPI.Models.TES;
using Microsoft.Data.SqlClient;
using Dapper;

namespace PgxAPI.DataBaseTier.ProGrX.Bancos
{
    public class frmTES_ReportesDB
    {
        private readonly IConfiguration? _config;
        private readonly MTesoreria MTesoreria;
        private readonly mProGrX_AuxiliarDB mProGrX_Auxiliar;

        public frmTES_ReportesDB(IConfiguration config)
        {
            _config = config;
            MTesoreria = new MTesoreria(_config);
            mProGrX_Auxiliar = new mProGrX_AuxiliarDB(_config);
        }

        /// <summary>
        /// Carga el combo de acceso general para los bancos.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> sbTesBancoCargaCboAccesoGeneral(int CodEmpresa)
        {
            return MTesoreria.sbTesBancoCargaCboAccesoGeneral(CodEmpresa);
        }

        /// <summary>
        /// Carga el combo de tipos de documentos para la carga de archivos en tesorería.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="id_banco"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>>  sbTesTiposDocsCargaCbo(int CodEmpresa, int id_banco)
        {
            return MTesoreria.sbTesTiposDocsCargaCbo(CodEmpresa, id_banco);
        }

        /// <summary>
        /// Carga los combos de tesorería según el tipo especificado.
        /// </summary>
        /// <param name="tipo"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> sbTESCombos(string tipo)
        {
            return MTesoreria.sbTESCombos(tipo);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> sbTesTokens(int CodEmpresa)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<List<DropDownListaGenericaModel>>();
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@" SELECT TOP 200
                                     Tok.ID_TOKEN AS item,
                                     CONCAT(Tok.ID_TOKEN ,' - ' , Tok.ESTADO, ' - ' ,ISNULL(COUNT(*), 0),' - ', ISNULL(SUM(Tra.Monto), 0)) as descripcion
                                 FROM TES_TOKENS Tok
                                 LEFT JOIN TES_TRANSACCIONES Tra ON Tok.ID_TOKEN = Tra.ID_TOKEN AND Tra.ESTADO = 'P'
                                 GROUP BY Tok.ID_TOKEN, Tok.ESTADO, Tok.REGISTRO_FECHA, Tok.REGISTRO_USUARIO
                                 ORDER BY Tok.REGISTRO_FECHA DESC";


                    response.Result = connection.Query<DropDownListaGenericaModel>(query).ToList();
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = null;
            }
            return response;
        }

        /// <summary>
        /// Carga combo de unidades 
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="contabilidad"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> sbTesUnidadesCargaCboGeneral(int CodEmpresa, int contabilidad)
        {
            return MTesoreria.sbTesUnidadesCargaCboGeneral(CodEmpresa, contabilidad);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> sbTesConceptosCargaCboGeneral(int CodEmpresa)
        {
            return MTesoreria.sbTesConceptosCargaCboGeneral(CodEmpresa);
        }

        public ErrorDto<string> Tes_AnalisisCubo_Obtener(int CodEmpresa, string tipo,DateTime FechaInicio,DateTime FechaCorte)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<string>();
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = "";
                    string vFechaInicio = mProGrX_Auxiliar.validaFechaGlobal(FechaInicio);
                    string vFechaCorte = mProGrX_Auxiliar.validaFechaGlobal(FechaCorte);

                    if (tipo == "T")
                    {
                        query = $@"exec spTesAnalisisCubo '{vFechaInicio}', '{vFechaCorte}' "; 
                    }
                    else
                    {
                        query = $@"exec spTesAnalisisContableCubo '{vFechaInicio}', '{vFechaCorte}' ";
                    }

                    var datos = connection.Query(query).ToList();

                    //convertir a JSON
                    response.Result = Newtonsoft.Json.JsonConvert.SerializeObject(datos);
                    response.Description = "Ok";
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = null;
            }
            return response;
        }

    }
}
