using Dapper;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using PgxAPI.Models;
using PgxAPI.Models.AH;
using PgxAPI.Models.ERROR;

namespace PgxAPI.DataBaseTier
{
    public class frmAH_AutorizacionesDB
    {
        private readonly IConfiguration _config;

        public frmAH_AutorizacionesDB(IConfiguration config)
        {
            _config = config;
        }

        public List<Pat_GestionesPatrimonio> Obtener_Autorizaciones(int CodEmpresa, string filtros)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            Filtros_Autorizaciones_PatrimonioDTO filtrosAutorizaciones = JsonConvert.DeserializeObject<Filtros_Autorizaciones_PatrimonioDTO>(filtros);

            List<Pat_GestionesPatrimonio> info = new List<Pat_GestionesPatrimonio>();
            try
            {

                string where = "";

                where = $"where Registro_Fecha BETWEEN '{filtrosAutorizaciones.fecha_inicio}' AND '{filtrosAutorizaciones.fecha_corte}' AND Estado = '{filtrosAutorizaciones.estado}'";



                if (filtrosAutorizaciones.cedula != "")
                {


                    where = $"where cedula = {filtrosAutorizaciones.cedula} and Registro_Fecha BETWEEN '{filtrosAutorizaciones.fecha_inicio} 00:00:00' AND '{filtrosAutorizaciones.fecha_corte} 23:59:59' AND Estado = '{filtrosAutorizaciones.estado}' ";

                }

                if (filtrosAutorizaciones.usuario != "")
                {


                    where = $"where usuario = {filtrosAutorizaciones.usuario} and Registro_Fecha BETWEEN '{filtrosAutorizaciones.fecha_inicio} 00:00:00' AND '{filtrosAutorizaciones.fecha_corte} 23:59:59' AND Estado = '{filtrosAutorizaciones.estado}' ";

                }

                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"SELECT * FROM vPAT_Gestiones_List 
                                    {where} ORDER BY registro_fecha; ";

                    info = connection.Query<Pat_GestionesPatrimonio>(query).ToList();
                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return info;
        }


        public ErrorDTO autorizaciones_Autorizar(int CodEmpresa, string Usuario, int Id_Solicitud)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            ErrorDTO resp = new ErrorDTO();
            resp.Code = 0;
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"update PAT_AUTORIZACIONES SET ESTADO = 'A', APLICA_FECHA  = Getdate(), APLICA_USUARIO = 'Pedro'
                        where id_autorizacion = {Id_Solicitud}";
                    resp.Code = connection.Execute(query);
                    resp.Description = "Autorización de operación " + Id_Solicitud + " procesada exitosamente";
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }
            return resp;
        }

        public ErrorDTO autorizaciones_Denegar(int CodEmpresa, string Usuario, int Id_Solicitud)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            ErrorDTO resp = new ErrorDTO();
            resp.Code = 0;
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"update PAT_AUTORIZACIONES SET ESTADO = 'D', APLICA_FECHA  = Getdate(), APLICA_USUARIO = 'Pedro'
                        where id_autorizacion = {Id_Solicitud}";
                    resp.Code = connection.Execute(query);
                    resp.Description = "La operación se ha denegado " + Id_Solicitud + " procesada exitosamente";
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }
            return resp;
        }

    }
}