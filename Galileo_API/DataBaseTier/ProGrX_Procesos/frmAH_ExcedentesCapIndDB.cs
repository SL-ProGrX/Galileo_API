using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models;
using PgxAPI.Models.ERROR;

namespace PgxAPI.DataBaseTier
{
    public class frmAH_ExcedentesCapIndDB
    {
        private readonly IConfiguration _config;

        public frmAH_ExcedentesCapIndDB(IConfiguration config)
        {
            _config = config;
        }


        public List<CapIndvDTO> CapInd_Obtener(int CodEmpresa)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            List<CapIndvDTO> info = new List<CapIndvDTO>();
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {

                    var query = "SELECT  exc_cap_ind, s.cedula, s.nombre, PORCENTAJE, VENCIMIENTO FROM EXC_CAP_INDIVIDUAL A INNER JOIN Socios S ON A.cedula = S.cedula";

                    info = connection.Query<CapIndvDTO>(query).ToList();

                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return info;
        }



        public ErrorDto CapIndv_Insertar(int CodCliente, CapIndvDTO capIndv)
        {
            var stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            ErrorDto info = new ErrorDto();
            info.Code = 0;

            try
            {

                using var connection = new SqlConnection(stringConn);
                {

                    var query = $@"insert into EXC_CAP_INDIVIDUAL(exc_cap_ind,cedula,porcentaje,vencimiento)  
                                    values ('{capIndv.Exc_Cap_Ind}', '{capIndv.Cedula}', {capIndv.Porcentaje}, '{capIndv.Vencimiento}')
                                        ";
                    var result = connection.Execute(query);
                }



            }
            catch (Exception ex)
            {
                info.Code = 1;
                info.Description = ex.Message;
            }
            return info;

        }



        public ErrorDto capIndv_Actualizar(int CodCliente, CapIndvDTO capIndv)
        {
            var stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            ErrorDto info = new ErrorDto();
            info.Code = 0;

            try
            {
                //int estado = grupo.estado ? 1 : 0;
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"UPDATE EXC_CAP_INDIVIDUAL SET cedula = '{capIndv.Cedula}',porcentaje = {capIndv.Porcentaje}, vencimiento = '{capIndv.Vencimiento}'
                                WHERE exc_cap_ind = {capIndv.Exc_Cap_Ind}";
                    connection.Execute(query);

                }

                info.Description = "Registro Actualizado";
            }
            catch (Exception ex)
            {
                info.Code = 1;
                info.Description = ex.Message;
            }

            return info;

        }


        public ErrorDto capIndv_Borrar(int CodCliente, string cod_producto)
        {
            var stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            ErrorDto info = new ErrorDto();
            info.Code = 0;

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"delete from EXC_CAP_INDIVIDUAL where EXC_CAP_IND = '{cod_producto}' ";
                    var result = connection.Execute(query);
                }

                info.Description = "Producto Eliminado!";
            }
            catch (Exception ex)
            {
                info.Code = 1;
                info.Description = ex.Message;
            }
            return info;
        }

    }
}