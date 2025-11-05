using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.ProGrX_Personas;

namespace PgxAPI.DataBaseTier.ProGrX_Personas
{
    public class frmAF_Ingresos_ConsultaDB
    {
        private readonly IConfiguration? _config;

        public frmAF_Ingresos_ConsultaDB(IConfiguration? config)
        {
            _config = config;
        }

        /// <summary>
        /// Consulta principal de ingresos de personas usando filtros.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa</param>
        /// <param name="filtro">Filtros de consulta</param>
        /// <returns></returns>
        public ErrorDto<IngresosConsultaLista> AF_Ingresos_Consulta(int CodEmpresa, IngresosConsultaFiltro filtro)
        {
            var result = new ErrorDto<IngresosConsultaLista>
            {
                Code = 0,
                Description = "Ok",
                Result = new IngresosConsultaLista()
            };

            try
            {
                string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var connection = new SqlConnection(stringConn);

                var query = "spAFI_Afiliaciones_Consulta";
                var parametros = new
                {
                    Cedula = filtro.Cedula,
                    Nombre = filtro.Nombre,
                    Inicio = filtro.Inicio,
                    Corte = filtro.Corte,
                    Usuario = filtro.Usuario,
                    Promotor = filtro.Promotor
                };

                result.Result.Lista = connection.Query<IngresosConsultaData>(
                    query,
                    parametros,
                    commandType: System.Data.CommandType.StoredProcedure
                ).ToList();
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result.Lista = null;
            }
            return result;
        }
    }
}
