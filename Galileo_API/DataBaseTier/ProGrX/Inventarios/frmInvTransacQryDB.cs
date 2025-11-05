using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.INV;

namespace PgxAPI.DataBaseTier
{
    public class frmInvTransacQryDB
    {
        private readonly IConfiguration _config;

        public frmInvTransacQryDB(IConfiguration config)
        {
            _config = config;
        }

        /// Obtiene la lista de transacciones a inventarios
        public ErrorDTO<TransacQryDataList> TransacInv_Obtener(int CodEmpresa, TransacQryParametros parametros)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            var response = new ErrorDTO<TransacQryDataList>
            {
                Code = 0,
                Result = new TransacQryDataList()
            };
            response.Result.Total = 0;

            string fechaBase = "";
            string usuarioBase = "";
            string where = "", paginaActual = " ", paginacionActual = " ";

            switch (parametros.TipoFecha)
            {
                case "S": //Fecha que Procesada
                    fechaBase = "genera_fecha";
                    break;
                case "A": //Fecha que Autorizada
                    fechaBase = "autoriza_fecha";
                    break;
                case "P": //Fecha que Rechazada
                    fechaBase = "procesa_fecha";
                    break;
                case "I": //Fecha de sistema
                    fechaBase = "fecha";
                    break;
                default:
                    break;
            }

            switch (parametros.TipoUsuario)
            {
                case "S": //Usuario que solicita
                    usuarioBase = "genera_user";
                    break;
                case "A": //Usuario que autoriza
                    usuarioBase = "autoriza_user";
                    break;
                case "P": //Usuario que procesa
                    usuarioBase = "procesa_user";
                    break;
                default:
                    break;
            }

            try
            {
                if (!string.IsNullOrEmpty(parametros.Estado))
                {
                    where += $" and estado = '{parametros.Estado}' ";
                }
                if (!string.IsNullOrEmpty(fechaBase))
                {
                    where += $" and {fechaBase} between '{parametros.FechaInicio} 00:00:00' and '{parametros.FechaCorte} 23:59:59' ";
                }
                if (!string.IsNullOrEmpty(usuarioBase))
                {
                    where += $" and {usuarioBase} like '%{parametros.Usuario}%' ";
                }

                if (parametros.vfiltro != null)
                {
                    where += "AND (boleta LIKE '%" + parametros.vfiltro + "%' OR notas LIKE '%" + parametros.vfiltro + "%' OR documento LIKE '%" + parametros.vfiltro + "%')";
                }

                if (parametros.pagina != null)
                {
                    paginaActual = "OFFSET " + parametros.pagina + " ROWS ";
                    paginacionActual = " FETCH NEXT " + parametros.paginacion + " ROWS ONLY ";
                }

                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"SELECT COUNT(*) FROM pv_invTransac where tipo like '%{parametros.Tipo}%' {where}";
                    response.Result.Total = connection.Query<int>(query).FirstOrDefault();

                    query = $@"SELECT * FROM pv_invTransac where tipo like '%{parametros.Tipo}%' 
                        {where} order by fecha desc 
                        {paginaActual} {paginacionActual}";

                    response.Result.Transacciones = connection.Query<TransacQryData>(query).ToList();
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result.Transacciones = null;
                response.Result.Total = 0;
            }
            return response;
        }
    }
}