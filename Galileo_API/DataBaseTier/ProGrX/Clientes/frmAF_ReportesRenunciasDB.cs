using System.Text;
using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models;
using PgxAPI.Models.ERROR;

namespace PgxAPI.DataBaseTier.ProGrX.Clientes
{
    public class frmAF_ReportesRenunciasDB
    {
        private readonly IConfiguration? _config;
        public frmAF_ReportesRenunciasDB(IConfiguration? config)
        {
            _config = config;
        }

        /// <summary>
        /// Obtiene la lista de gestiones para seguimiento.
        /// </summary>
        public ErrorDTO<List<DropDownListaGenericaModel>> AF_Reportes_Renuncias_Oficinas_Obtener(int CodEmpresa)
        {
            var result = new ErrorDTO<List<DropDownListaGenericaModel>>()
            {
                Code = 0,
                Description = "Ok",
                Result = new List<DropDownListaGenericaModel>()
            };

            try
            {
                string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var connection = new SqlConnection(stringConn);

                string query = @"select cod_Oficina as item , rtrim(descripcion) as descripcion from SIF_Oficinas order by descripcion";
                result.Result = connection.Query<DropDownListaGenericaModel>(query).ToList();
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
