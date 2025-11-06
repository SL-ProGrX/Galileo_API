using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models;
using PgxAPI.Models.AH;
using PgxAPI.Models.ERROR;

namespace PgxAPI.DataBaseTier
{
    public class frmAH_ExcedentesAjusteDB
    {
        private readonly IConfiguration _config;

        public frmAH_ExcedentesAjusteDB(IConfiguration config)
        {
            _config = config;
        }

        public List<AjusteExcedenteDto> AjusteExcedente_Obtener(int CodEmpresa)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            List<AjusteExcedenteDto> info = new List<AjusteExcedenteDto>();
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {

                    var query = "SELECT  Ajuste_ID, A.Cedula, S.Nombre, Ajuste, Detalle FROM Exc_Ajustes A INNER JOIN Socios S ON A.cedula = S.cedula";

                    info = connection.Query<AjusteExcedenteDto>(query).ToList();

                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return info;
        }



        public ErrorDto AjusteExcedente_Insertar(int CodCliente, AjusteExcedenteDto capIndv)
        {
            var stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            ErrorDto info = new ErrorDto();
            info.Code = 0;

            try
            {

                using var connection = new SqlConnection(stringConn);
                {

                    var query = $@"insert into Exc_Ajustes(AJUSTE_ID,CEDULA,AJUSTE,DETALLE)  
                                    values ('{capIndv.Ajuste_id}', '{capIndv.Cedula}', {capIndv.Ajuste}, '{capIndv.Detalle}')
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



        public ErrorDto AjusteExcedente_Actualizar(int CodCliente, AjusteExcedenteDto capIndv)
        {
            var stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            ErrorDto info = new ErrorDto();
            info.Code = 0;

            try
            {
                //int estado = grupo.estado ? 1 : 0;
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"UPDATE Exc_Ajustes SET cedula = '{capIndv.Cedula}',ajuste = {capIndv.Ajuste}, detalle = '{capIndv.Detalle}'
                                WHERE ajuste_id = {capIndv.Ajuste_id}";
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


        public ErrorDto AjusteExcedente_Borrar(int CodCliente, string cod_producto)
        {
            var stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            ErrorDto info = new ErrorDto();
            info.Code = 0;

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"delete from Exc_Ajustes where AJUSTE_ID = '{cod_producto}' ";
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