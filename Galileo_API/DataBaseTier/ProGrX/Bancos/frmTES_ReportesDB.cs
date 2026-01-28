using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;

namespace Galileo_API.DataBaseTier.ProGrX.Bancos
{
    public class FrmTesReportesDB
    {
        private readonly PortalDB _portalDB;
        private readonly MTesoreria mTesoreria;

        public FrmTesReportesDB(IConfiguration config)
        {
            _portalDB = new PortalDB(config);
            mTesoreria = new MTesoreria(config);
        }

        /// <summary>
        /// Carga el combo de acceso general para los bancos.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> sbTesBancoCargaCboAccesoGeneral(int CodEmpresa)
        {
            return mTesoreria.sbTesBancoCargaCboAccesoGeneral(CodEmpresa);
        }

        /// <summary>
        /// Carga el combo de tipos de documentos para la carga de archivos en tesorería.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="id_banco"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>>  sbTesTiposDocsCargaCbo(int CodEmpresa, int id_banco)
        {
            return mTesoreria.sbTesTiposDocsCargaCbo(CodEmpresa, id_banco);
        }

        /// <summary>
        /// Carga los combos de tesorería según el tipo especificado.
        /// </summary>
        /// <param name="tipo"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> sbTESCombos(string tipo)
        {
            return mTesoreria.sbTESCombos(tipo);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> sbTesTokens(int CodEmpresa)
        {
            return DbHelper.WithConn(_portalDB, CodEmpresa, conn =>
            {
                const string query = $@" SELECT TOP 200
                                     Tok.ID_TOKEN AS item,
                                     CONCAT(Tok.ID_TOKEN ,' - ' , Tok.ESTADO, ' - ' ,ISNULL(COUNT(*), 0),' - ', ISNULL(SUM(Tra.Monto), 0)) as descripcion
                                 FROM TES_TOKENS Tok
                                 LEFT JOIN TES_TRANSACCIONES Tra ON Tok.ID_TOKEN = Tra.ID_TOKEN AND Tra.ESTADO = 'P'
                                 GROUP BY Tok.ID_TOKEN, Tok.ESTADO, Tok.REGISTRO_FECHA, Tok.REGISTRO_USUARIO
                                 ORDER BY Tok.REGISTRO_FECHA DESC";

                return conn.Query<DropDownListaGenericaModel>(query).ToList();
            });
        }

        /// <summary>
        /// Carga combo de unidades 
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="contabilidad"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> sbTesUnidadesCargaCboGeneral(int CodEmpresa, int contabilidad)
        {
            return mTesoreria.sbTesUnidadesCargaCboGeneral(CodEmpresa, contabilidad);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> sbTesConceptosCargaCboGeneral(int CodEmpresa)
        {
            return mTesoreria.sbTesConceptosCargaCboGeneral(CodEmpresa);
        }

        public ErrorDto<string> Tes_AnalisisCubo_Obtener(int CodEmpresa, string tipo,DateTime FechaInicio,DateTime FechaCorte)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);
            try
            {
                var query = "";
                string? vFechaInicio = MProGrXAuxiliarDB.validaFechaGlobal(FechaInicio, "yyyy-MM-dd HH:mm:ss");
                string? vFechaCorte = MProGrXAuxiliarDB.validaFechaGlobal(FechaCorte, "yyyy-MM-dd HH:mm:ss");

                if (tipo == "T")
                {
                    query = $@"exec spTesAnalisisCubo '{vFechaInicio}', '{vFechaCorte}' ";
                }
                else
                {
                    query = $@"exec spTesAnalisisContableCubo '{vFechaInicio}', '{vFechaCorte}' ";
                }

                var datos = conn.Query(query).ToList();

                //convertir a JSON
                var result = Newtonsoft.Json.JsonConvert.SerializeObject(datos);
                return DbHelper.CreateOkResponse<string>(result);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<string>($"Error al obtener el análisis de cubo: {ex.Message}");
            }
        }

    }
}
