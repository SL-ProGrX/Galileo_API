using PgxAPI.Models.ERROR;
using PgxAPI.Models;
using PgxAPI.Models.TES;
using Microsoft.Data.SqlClient;
using Dapper;

namespace PgxAPI.DataBaseTier.ProGrX.Bancos
{
    public class frmTES_ReportesAutorizacionesDB
    {
        private readonly IConfiguration? _config;
        private readonly MTesoreria MTesoreria;
        private readonly mProGrX_AuxiliarDB mProGrX_Auxiliar;

        public frmTES_ReportesAutorizacionesDB(IConfiguration config)
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
        /// Obtiene una lista de usuarios activos para ubicaciones de tesorería.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Tes_RepAuthUsuarios_Obtener(int CodEmpresa)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDto<List<DropDownListaGenericaModel>>()
            {
                Code = 0,
                Description = "Ok",
                Result = new List<DropDownListaGenericaModel>()
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"select Nombre as 'item',descripcion from usuarios where estado = 'A' ";
                    result.Result = connection.Query<DropDownListaGenericaModel>(query).ToList();
                }
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = null;
            }
            return result;
        }


    }
}
